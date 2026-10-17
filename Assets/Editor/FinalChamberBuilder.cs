#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FinalChamberBuilder
{
    private const string ScenePath = "Assets/Scenes/MainLevel.unity";
    private const string GeneratedRootName = "Generated_FinalChamber";
    private const string MaterialDir = "Assets/Materials/FinalChamberGenerated";

    [MenuItem("Emberloop/Build or Refresh Final Chamber")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject oldRoot = GameObject.Find(GeneratedRootName);
        if (oldRoot != null)
            UnityEngine.Object.DestroyImmediate(oldRoot);

        GameObject finalChamber = GameObject.Find("FinalChamber");
        GameObject finalBeacon = GameObject.Find("FinalBeacon");
        GameObject checkpoint = GameObject.Find("Checkpoint_Final");
        TimeLoopManager loopManager = UnityEngine.Object.FindFirstObjectByType<TimeLoopManager>();
        PlayerController playerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        PlayerRecorder playerRecorder = UnityEngine.Object.FindFirstObjectByType<PlayerRecorder>();

        if (finalBeacon == null || checkpoint == null || loopManager == null)
        {
            EditorUtility.DisplayDialog("Emberloop", "FinalBeacon, Checkpoint_Final, or TimeLoopManager could not be found in MainLevel.", "OK");
            return;
        }

        Vector3 center = CalculateChamberCenter(finalBeacon.transform.position);
        Vector3 floorCenter = center;
        floorCenter.y = EstimateFloorHeight();

        Vector3 forward = center - checkpoint.transform.position;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.forward;
        forward.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        GameObject root = new GameObject(GeneratedRootName);
        if (finalChamber != null)
            root.transform.SetParent(finalChamber.transform, true);

        Material stone = GetOrCreateMaterial("FinalStone.mat", new Color(0.12f, 0.105f, 0.095f), Color.black, 0f);
        Material darkStone = GetOrCreateMaterial("FinalDarkStone.mat", new Color(0.045f, 0.04f, 0.045f), Color.black, 0f);
        Material ember = GetOrCreateMaterial("FinalEmber.mat", new Color(0.17f, 0.045f, 0.01f), new Color(5.0f, 0.7f, 0.08f), 1f);
        Material dormant = GetOrCreateMaterial("FinalAnchorDormant.mat", new Color(0.035f, 0.03f, 0.035f), new Color(0.06f, 0.012f, 0.004f), 1f);
        Material particleMat = GetOrCreateParticleMaterial();

        // Improve the existing placeholder beacon without deleting the user's work.
        GameObject beaconGenerated = new GameObject("Beacon_Final_Generated");
        beaconGenerated.transform.SetParent(root.transform);
        beaconGenerated.transform.position = new Vector3(center.x, floorCenter.y + 0.08f, center.z);

        CreateCylinder("Beacon_Base", beaconGenerated.transform, Vector3.zero, new Vector3(2.35f, 0.22f, 2.35f), stone, true);
        CreateCylinder("Beacon_Middle", beaconGenerated.transform, Vector3.up * 0.33f, new Vector3(1.75f, 0.20f, 1.75f), darkStone, true);
        CreateCylinder("Beacon_Top", beaconGenerated.transform, Vector3.up * 0.63f, new Vector3(1.15f, 0.18f, 1.15f), ember, true);

        GameObject fireOrigin = new GameObject("Final_Flame");
        fireOrigin.transform.SetParent(beaconGenerated.transform);
        fireOrigin.transform.localPosition = Vector3.up * 1.35f;

        Light beaconLight = fireOrigin.AddComponent<Light>();
        beaconLight.type = LightType.Point;
        beaconLight.color = new Color(1f, 0.32f, 0.07f);
        beaconLight.intensity = 1.2f;
        beaconLight.range = 7f;
        beaconLight.shadows = LightShadows.Soft;

        ParticleSystem beaconParticles = AddFireParticles(fireOrigin, particleMat, 10f, 0.35f, 0.85f, 1.4f);

        // Three ritual anchors: two pure shadow anchors, one explicitly requiring firelight.
        Vector3 aPos = floorCenter - right * 4.1f + forward * 2.9f;
        Vector3 bPos = floorCenter + right * 4.1f + forward * 2.9f;
        Vector3 cPos = floorCenter - right * 3.4f - forward * 2.8f;

        FinalAnchor anchorA = CreateAnchor("FinalAnchor_A", root.transform, aPos, false, stone, dormant);
        FinalAnchor anchorB = CreateAnchor("FinalAnchor_B", root.transform, bPos, false, stone, dormant);
        FinalAnchor anchorC = CreateAnchor("FinalAnchor_C_Fire", root.transform, cPos, true, stone, dormant);

        // Firelight ritual brazier beside Anchor C.
        GameObject anchorFire = new GameObject("FinalAnchorC_FireSource");
        anchorFire.transform.SetParent(root.transform);
        anchorFire.transform.position = cPos + right * 1.65f + Vector3.up * 0.45f;
        SphereCollider fireZoneCollider = anchorFire.AddComponent<SphereCollider>();
        fireZoneCollider.isTrigger = true;
        fireZoneCollider.radius = 2.35f;
        Rigidbody fireRb = anchorFire.AddComponent<Rigidbody>();
        fireRb.isKinematic = true;
        fireRb.useGravity = false;
        anchorFire.AddComponent<FireLightZone>();
        Light anchorFireLight = anchorFire.AddComponent<Light>();
        anchorFireLight.type = LightType.Point;
        anchorFireLight.color = new Color(1f, 0.30f, 0.05f);
        anchorFireLight.range = 5.5f;
        anchorFireLight.intensity = 3.5f;
        anchorFireLight.shadows = LightShadows.Soft;
        AddFireParticles(anchorFire, particleMat, 20f, 0.22f, 0.7f, 0.8f);
        InstantiateModel("Assets/Art/QuaterniusRuins/Models/Torch.fbx", anchorFire.transform, Vector3.down * 0.45f, Quaternion.identity, "Ritual_Torch");

        // Controller + completion trigger.
        FinalChamberController controller = root.AddComponent<FinalChamberController>();
        controller.anchors = new[] { anchorA, anchorB, anchorC };
        controller.loopManager = loopManager;
        controller.playerController = playerController;
        controller.playerRecorder = playerRecorder;
        controller.beaconLight = beaconLight;
        controller.beaconParticles = beaconParticles;
        anchorA.controller = controller;
        anchorB.controller = controller;
        anchorC.controller = controller;

        GameObject beaconTrigger = new GameObject("FinalBeacon_EndingTrigger");
        beaconTrigger.transform.SetParent(root.transform);
        beaconTrigger.transform.position = floorCenter + Vector3.up * 1.1f;
        SphereCollider endCollider = beaconTrigger.AddComponent<SphereCollider>();
        endCollider.isTrigger = true;
        endCollider.radius = 1.6f;
        Rigidbody endRb = beaconTrigger.AddComponent<Rigidbody>();
        endRb.isKinematic = true;
        endRb.useGravity = false;
        FinalBeaconTrigger trigger = beaconTrigger.AddComponent<FinalBeaconTrigger>();
        trigger.controller = controller;

        // Loop resets must clear ritual anchors because shadows teleport on rewind.
        loopManager.finalAnchorsToReset = new[] { anchorA, anchorB, anchorC };
        EditorUtility.SetDirty(loopManager);

        // Environmental polish: torches, candles, flags, rubble, skulls around the existing architecture.
        List<Vector3> torchPositions = new List<Vector3>
        {
            floorCenter - right * 5.0f + forward * 4.8f,
            floorCenter + right * 5.0f + forward * 4.8f,
            floorCenter - right * 5.2f - forward * 0.4f,
            floorCenter + right * 5.2f - forward * 0.4f,
            floorCenter - right * 2.3f - forward * 4.6f,
            floorCenter + right * 2.3f - forward * 4.6f,
        };
        for (int i = 0; i < torchPositions.Count; i++)
            CreateTorchCluster(root.transform, torchPositions[i], center, i, particleMat);

        CreatePropCluster(root.transform, floorCenter - right * 4.4f + forward * 4.0f, center, "LeftRear");
        CreatePropCluster(root.transform, floorCenter + right * 4.4f + forward * 4.0f, center, "RightRear");
        CreatePropCluster(root.transform, floorCenter - right * 4.8f - forward * 2.3f, center, "LeftFront");

        // Two vertical flags behind the beacon make the back wall read as a ceremonial focal point.
        InstantiateModel("Assets/Art/QuaterniusRuins/Models/Flag_Wall.fbx", root.transform,
            floorCenter + forward * 5.45f - right * 2.2f + Vector3.up * 1.3f,
            Quaternion.LookRotation(-forward, Vector3.up), "Final_Flag_Left");
        InstantiateModel("Assets/Art/QuaterniusRuins/Models/Flag_Wall2.fbx", root.transform,
            floorCenter + forward * 5.45f + right * 2.2f + Vector3.up * 1.3f,
            Quaternion.LookRotation(-forward, Vector3.up), "Final_Flag_Right");

        // A few broken stone masses make the floor feel less clean/square without changing the user's collision layout.
        InstantiateModel("Assets/Art/QuaterniusRuins/Models/Bricks.fbx", root.transform,
            floorCenter - right * 5.1f + forward * 1.2f, Quaternion.Euler(0, 24, 0), "Rubble_Left");
        InstantiateModel("Assets/Art/QuaterniusRuins/Models/Bricks.fbx", root.transform,
            floorCenter + right * 4.7f - forward * 2.0f, Quaternion.Euler(0, -18, 0), "Rubble_Right");

        // Preserve user's original placeholder light but keep it from competing with the generated beacon.
        GameObject oldFire = GameObject.Find("FireSource_Final");
        if (oldFire != null)
        {
            foreach (Light l in oldFire.GetComponentsInChildren<Light>(true))
                l.enabled = false;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = root;
        SceneView.lastActiveSceneView?.FrameSelected();

        EditorUtility.DisplayDialog("Emberloop", "Final chamber generated and wired. Test it from Checkpoint_Final: record three shadow routes to A, B and the fire anchor C, then enter the central beacon on the fourth loop.", "Done");
    }

    private static Vector3 CalculateChamberCenter(Vector3 fallback)
    {
        Transform[] columns = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
            .Where(t => t.name.StartsWith("Column_Round", StringComparison.Ordinal)).ToArray();
        if (columns.Length >= 4)
        {
            Vector3 sum = Vector3.zero;
            foreach (Transform t in columns.Take(4)) sum += t.position;
            Vector3 c = sum / 4f;
            c.y = fallback.y;
            return c;
        }
        return fallback;
    }

    private static float EstimateFloorHeight()
    {
        GameObject finalBeacon = GameObject.Find("FinalBeacon");
        if (finalBeacon != null)
            return Mathf.Max(0.25f, finalBeacon.transform.position.y - 0.04f);
        return 0.25f;
    }

    private static FinalAnchor CreateAnchor(string name, Transform parent, Vector3 position, bool requireFire,
        Material stone, Material indicatorMaterial)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = position + Vector3.up * 0.08f;

        SphereCollider col = root.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1.05f;
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        CreateCylinder("Stone_Ring", root.transform, Vector3.zero, new Vector3(1.18f, 0.10f, 1.18f), stone, false);
        GameObject indicator = CreateCylinder("Rune_Core", root.transform, Vector3.up * 0.13f,
            new Vector3(0.76f, 0.035f, 0.76f), indicatorMaterial, false);

        Light light = new GameObject("Anchor_Light").AddComponent<Light>();
        light.transform.SetParent(root.transform);
        light.transform.localPosition = Vector3.up * 0.65f;
        light.type = LightType.Point;
        light.color = new Color(1f, 0.24f, 0.03f);
        light.range = 3.4f;
        light.intensity = 0f;
        light.enabled = false;

        FinalAnchor anchor = root.AddComponent<FinalAnchor>();
        anchor.requireFirelight = requireFire;
        anchor.indicatorRenderer = indicator.GetComponent<Renderer>();
        anchor.indicatorLight = light;
        return anchor;
    }

    private static GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, Vector3 scale,
        Material material, bool collider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = scale;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = material;
        Collider c = go.GetComponent<Collider>();
        if (!collider && c != null) UnityEngine.Object.DestroyImmediate(c);
        return go;
    }

    private static ParticleSystem AddFireParticles(GameObject parent, Material material, float emissionRate,
        float size, float lifetime, float speed)
    {
        GameObject pgo = new GameObject("Fire_Particles");
        pgo.transform.SetParent(parent.transform);
        pgo.transform.localPosition = Vector3.zero;
        ParticleSystem ps = pgo.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = lifetime;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.14f, 0.01f), new Color(1f, 0.72f, 0.08f));
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        var emission = ps.emission;
        emission.rateOverTime = emissionRate;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 13f;
        shape.radius = 0.28f;
        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.65f, 0.1f), 0f), new GradientColorKey(new Color(0.5f, 0.02f, 0f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        color.color = gradient;
        ParticleSystemRenderer renderer = pgo.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        ps.Play();
        return ps;
    }

    private static void CreateTorchCluster(Transform parent, Vector3 position, Vector3 faceTarget, int index, Material particleMat)
    {
        GameObject wrapper = new GameObject($"Final_Torch_{index + 1:00}");
        wrapper.transform.SetParent(parent);
        wrapper.transform.position = position;
        Vector3 dir = faceTarget - position; dir.y = 0f;
        wrapper.transform.rotation = dir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(dir) : Quaternion.identity;
        InstantiateModel("Assets/Art/QuaterniusRuins/Models/Torch.fbx", wrapper.transform, Vector3.zero, Quaternion.identity, "Torch_Model");

        GameObject flame = new GameObject("Torch_Flame");
        flame.transform.SetParent(wrapper.transform);
        flame.transform.localPosition = new Vector3(0, 1.35f, 0.08f);
        Light l = flame.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.27f, 0.045f);
        l.intensity = 2.2f;
        l.range = 4.5f;
        l.shadows = LightShadows.Soft;
        AddFireParticles(flame, particleMat, 12f, 0.11f, 0.55f, 0.55f);
    }

    private static void CreatePropCluster(Transform parent, Vector3 position, Vector3 faceTarget, string suffix)
    {
        GameObject cluster = new GameObject("PropCluster_" + suffix);
        cluster.transform.SetParent(parent);
        cluster.transform.position = position;
        Vector3 dir = faceTarget - position; dir.y = 0f;
        cluster.transform.rotation = dir.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir) : Quaternion.identity;

        InstantiateModel("Assets/Art/QuaterniusRuins/Models/Candles_1.fbx", cluster.transform, new Vector3(-0.3f, 0, 0.1f), Quaternion.identity, "Candles_A");
        InstantiateModel("Assets/Art/QuaterniusRuins/Models/Candles_2.fbx", cluster.transform, new Vector3(0.35f, 0, -0.15f), Quaternion.Euler(0, 35, 0), "Candles_B");
        InstantiateModel("Assets/Art/QuaterniusRuins/Models/Skull.fbx", cluster.transform, new Vector3(0.05f, 0, 0.45f), Quaternion.Euler(0, -25, 0), "Skull");
    }

    private static GameObject InstantiateModel(string path, Transform parent, Vector3 localPosition, Quaternion localRotation, string wrapperName)
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        GameObject wrapper = new GameObject(wrapperName);
        wrapper.transform.SetParent(parent);
        wrapper.transform.localPosition = localPosition;
        wrapper.transform.localRotation = localRotation;
        if (asset == null)
            return wrapper;

        GameObject model = PrefabUtility.InstantiatePrefab(asset, wrapper.transform) as GameObject;
        if (model != null)
        {
            // Model-import transforms in this asset pack are intentionally left untouched.
            model.name = Path.GetFileNameWithoutExtension(path);
        }
        return wrapper;
    }

    private static Material GetOrCreateMaterial(string fileName, Color baseColor, Color emission, float emissionStrength)
    {
        EnsureFolder(MaterialDir);
        string path = MaterialDir + "/" + fileName;
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
        else if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission * Mathf.Max(1f, emissionStrength));
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material GetOrCreateParticleMaterial()
    {
        EnsureFolder(MaterialDir);
        string path = MaterialDir + "/FinalFireParticle.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(1f, 0.25f, 0.02f, 0.85f));
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
