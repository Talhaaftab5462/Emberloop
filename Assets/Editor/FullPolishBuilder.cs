#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class FullPolishBuilder
{
    private const string GeneratedName = "Generated_Polish";
    private const string PolishDir = "Assets/Materials/EmberloopPolish";

    [MenuItem("Emberloop/Apply Full Polish")]
    public static void ApplyFullPolish()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("Emberloop", "Open MainLevel first.", "OK");
            return;
        }

        TimeLoopManager loop = UnityEngine.Object.FindFirstObjectByType<TimeLoopManager>();
        PlayerController playerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (loop == null || playerController == null)
        {
            EditorUtility.DisplayDialog("Emberloop", "Could not find TimeLoopManager or PlayerController in the open scene.", "OK");
            return;
        }

        GameObject old = GameObject.Find(GeneratedName);
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        GameObject root = new GameObject(GeneratedName);

        EnsureFolder(PolishDir);

        AudioClip ambience = LoadClip("Assets/Audio/Ambience/dungeon_ambience.wav");
        AudioClip fire = LoadClip("Assets/Audio/Fire/fire_loop.wav");
        AudioClip door = LoadClip("Assets/Audio/Doors/door_heavy.wav");
        AudioClip reset = LoadClip("Assets/Audio/Time/time_reset.wav");
        AudioClip activate = LoadClip("Assets/Audio/Puzzle/anchor_activate.wav");
        AudioClip impact = LoadClip("Assets/Audio/Finale/beacon_impact.mp3");
        AudioClip hum = LoadClip("Assets/Audio/Finale/ritual_hum.wav");
        AudioClip footsteps = LoadClip("Assets/Audio/Player/footsteps_stone.wav");

        ConfigureAtmosphere(root);
        ConfigureGlobalAudio(root, ambience);
        ConfigureHUD(root, loop);
        ConfigureResetFX(loop, reset);
        ConfigurePlayer(playerController, footsteps);
        ConfigureDoors(door);
        ConfigurePressurePlates(activate);
        ConfigureFireSources(fire);
        ConfigureFinalChamber(activate, impact, hum);
        ConfigureShadowPrefab();
        ConfigureCamera();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = root;
        EditorUtility.DisplayDialog("Emberloop", "Full polish applied. Press Play and test audio, loop reset, doors, fire, HUD and finale. You can safely rerun this command; Generated_Polish is rebuilt each time.", "Done");
    }

    private static void ConfigureAtmosphere(GameObject root)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.018f;
        RenderSettings.fogColor = new Color(0.018f, 0.024f, 0.04f, 1f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.035f, 0.045f, 0.07f, 1f);

        Light sun = GameObject.Find("Directional Light")?.GetComponent<Light>();
        if (sun != null)
        {
            sun.intensity = 0.18f;
            sun.color = new Color(0.42f, 0.5f, 0.72f, 1f);
            sun.shadows = LightShadows.Soft;
        }

        string profilePath = PolishDir + "/EmberloopGlobalVolume.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        profile.components.Clear();
        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.85f);
        bloom.threshold.Override(0.75f);
        bloom.scatter.Override(0.65f);

        ColorAdjustments color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(-0.15f);
        color.contrast.Override(22f);
        color.saturation.Override(-12f);
        color.colorFilter.Override(new Color(0.92f, 0.96f, 1f));

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.28f);
        vignette.smoothness.Override(0.62f);

        Tonemapping tonemap = profile.Add<Tonemapping>(true);
        tonemap.mode.Override(TonemappingMode.ACES);

        GameObject volumeGO = new GameObject("Global_PostProcessing");
        volumeGO.transform.SetParent(root.transform);
        Volume volume = volumeGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        volume.profile = profile;

        CreateFloatingDust(root.transform);
    }

    private static void ConfigureGlobalAudio(GameObject root, AudioClip ambience)
    {
        GameObject audio = new GameObject("Global_Ambience");
        audio.transform.SetParent(root.transform);
        AudioSource src = audio.AddComponent<AudioSource>();
        src.clip = ambience;
        src.loop = true;
        src.playOnAwake = true;
        src.volume = 0.24f;
        src.spatialBlend = 0f;
    }

    private static void ConfigureHUD(GameObject root, TimeLoopManager loop)
    {
        EmberloopHUD hud = root.AddComponent<EmberloopHUD>();
        hud.loopManager = loop;
    }

    private static void ConfigureResetFX(TimeLoopManager loop, AudioClip reset)
    {
        AudioSource src = loop.GetComponent<AudioSource>();
        if (src == null) src = loop.gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;

        TimeResetFX fx = loop.GetComponent<TimeResetFX>();
        if (fx == null) fx = loop.gameObject.AddComponent<TimeResetFX>();
        fx.resetClip = reset;
        loop.resetFX = fx;
        EditorUtility.SetDirty(loop);
    }

    private static void ConfigurePlayer(PlayerController playerController, AudioClip footsteps)
    {
        GameObject player = playerController.gameObject;
        AudioSource src = player.GetComponent<AudioSource>();
        if (src == null) src = player.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        src.volume = 0.5f;

        PlayerFootsteps steps = player.GetComponent<PlayerFootsteps>();
        if (steps == null) steps = player.AddComponent<PlayerFootsteps>();
        steps.footstepClip = footsteps;
        steps.stepInterval = 0.43f;
        steps.volume = 0.36f;
    }

    private static void ConfigureDoors(AudioClip clip)
    {
        foreach (DoorController dc in UnityEngine.Object.FindObjectsByType<DoorController>(FindObjectsSortMode.None))
        {
            AudioSource src = dc.GetComponent<AudioSource>();
            if (src == null) src = dc.gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.minDistance = 1.2f;
            src.maxDistance = 14f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            dc.audioSource = src;
            dc.moveClip = clip;

            Transform oldVisual = dc.transform.Find("Generated_DoorVisual");
            if (oldVisual != null) UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
            GameObject doorAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/QuaterniusRuins/Models/Doors_GothicArch_Covered.fbx");
            if (doorAsset != null)
            {
                GameObject wrapper = new GameObject("Generated_DoorVisual");
                wrapper.transform.SetParent(dc.transform);
                wrapper.transform.localPosition = Vector3.zero;
                wrapper.transform.localRotation = Quaternion.identity;
                Vector3 s = dc.transform.lossyScale;
                wrapper.transform.localScale = new Vector3(SafeInv(s.x), SafeInv(s.y), SafeInv(s.z));
                GameObject model = PrefabUtility.InstantiatePrefab(doorAsset, wrapper.transform) as GameObject;
                if (model != null) model.name = "GothicDoor_Model";
                Renderer primitive = dc.GetComponent<Renderer>();
                if (primitive != null) primitive.enabled = false;
            }
            EditorUtility.SetDirty(dc);
        }
    }

    private static void ConfigurePressurePlates(AudioClip clip)
    {
        foreach (PressurePlate plate in UnityEngine.Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None))
        {
            AudioSource src = plate.GetComponent<AudioSource>();
            if (src == null) src = plate.gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.minDistance = 1f;
            src.maxDistance = 9f;
            plate.audioSource = src;
            plate.activationClip = clip;
            plate.indicatorRenderer = plate.GetComponent<Renderer>();
            Transform oldLight = plate.transform.Find("Plate_ActivationLight");
            if (oldLight != null) UnityEngine.Object.DestroyImmediate(oldLight.gameObject);
            GameObject lightGO = new GameObject("Plate_ActivationLight");
            lightGO.transform.SetParent(plate.transform);
            lightGO.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.28f, 0.035f);
            light.range = 3f;
            light.intensity = 0f;
            light.enabled = false;
            plate.indicatorLight = light;
            EditorUtility.SetDirty(plate);
        }
    }

    private static void ConfigureFireSources(AudioClip clip)
    {
        GameObject[] all = SceneManager.GetActiveScene().GetRootGameObjects()
            .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
            .Where(t => t.name.StartsWith("FireSource_", StringComparison.Ordinal) || t.name.StartsWith("FinalAnchorC_FireSource", StringComparison.Ordinal))
            .Select(t => t.gameObject).Distinct().ToArray();

        foreach (GameObject fire in all)
        {
            AudioSource src = fire.GetComponent<AudioSource>();
            if (src == null) src = fire.AddComponent<AudioSource>();
            src.clip = clip;
            src.loop = true;
            src.playOnAwake = true;
            src.volume = 0.35f;
            src.spatialBlend = 1f;
            src.minDistance = 1.5f;
            src.maxDistance = 12f;

            Light light = fire.GetComponentInChildren<Light>(true);
            if (light != null)
            {
                light.color = new Color(1f, 0.26f, 0.055f);
                light.shadows = LightShadows.Soft;
                FireFlicker flicker = light.GetComponent<FireFlicker>();
                if (flicker == null) flicker = light.gameObject.AddComponent<FireFlicker>();
                flicker.targetLight = light;
                flicker.baseIntensity = Mathf.Max(light.intensity, 2.5f);
                flicker.amplitude = 0.48f;
            }

            Transform existing = fire.transform.Find("Generated_FirePolish");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
            GameObject fx = new GameObject("Generated_FirePolish");
            fx.transform.SetParent(fire.transform);
            fx.transform.localPosition = new Vector3(0, 0.55f, 0);
            AddFireParticles(fx);
        }
    }

    private static void ConfigureFinalChamber(AudioClip activation, AudioClip impact, AudioClip hum)
    {
        foreach (FinalAnchor anchor in UnityEngine.Object.FindObjectsByType<FinalAnchor>(FindObjectsSortMode.None))
        {
            AudioSource src = anchor.GetComponent<AudioSource>();
            if (src == null) src = anchor.gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.minDistance = 1f;
            src.maxDistance = 12f;
            anchor.audioSource = src;
            anchor.activationClip = activation;
            EditorUtility.SetDirty(anchor);
        }

        FinalChamberController controller = UnityEngine.Object.FindFirstObjectByType<FinalChamberController>();
        if (controller == null) return;

        AudioSource impactSource = controller.GetComponent<AudioSource>();
        if (impactSource == null) impactSource = controller.gameObject.AddComponent<AudioSource>();
        impactSource.playOnAwake = false;
        impactSource.spatialBlend = 0f;
        controller.impactSource = impactSource;
        controller.beaconImpactClip = impact;

        GameObject beacon = GameObject.Find("Beacon_Final_Generated");
        if (beacon != null)
        {
            AudioSource humSource = beacon.GetComponent<AudioSource>();
            if (humSource == null) humSource = beacon.AddComponent<AudioSource>();
            humSource.clip = hum;
            humSource.loop = true;
            humSource.playOnAwake = false;
            humSource.volume = 0.3f;
            humSource.spatialBlend = 1f;
            humSource.minDistance = 2f;
            humSource.maxDistance = 28f;
            controller.ritualHumSource = humSource;
        }
        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureShadowPrefab()
    {
        const string path = "Assets/Prefabs/Shadow.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            if (root.GetComponent<ShadowEchoFX>() == null)
                root.AddComponent<ShadowEchoFX>();

            TrailRenderer trail = root.GetComponent<TrailRenderer>();
            if (trail == null) trail = root.AddComponent<TrailRenderer>();
            trail.time = 0.38f;
            trail.startWidth = 0.22f;
            trail.endWidth = 0.015f;
            trail.minVertexDistance = 0.08f;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            trail.material = GetOrCreateTrailMaterial();
            trail.startColor = new Color(0.08f, 0.17f, 0.34f, 0.34f);
            trail.endColor = new Color(0.02f, 0.04f, 0.1f, 0f);

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        Material shadowMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ShadowMaterial.mat");
        if (shadowMat != null)
        {
            if (shadowMat.HasProperty("_BaseColor")) shadowMat.SetColor("_BaseColor", new Color(0.015f, 0.02f, 0.045f, 1f));
            if (shadowMat.HasProperty("_EmissionColor"))
            {
                shadowMat.EnableKeyword("_EMISSION");
                shadowMat.SetColor("_EmissionColor", new Color(0.07f, 0.14f, 0.3f));
            }
            EditorUtility.SetDirty(shadowMat);
        }
    }

    private static void ConfigureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.backgroundColor = new Color(0.008f, 0.012f, 0.022f, 1f);
        cam.nearClipPlane = 0.04f;
        cam.farClipPlane = 120f;
    }

    private static void CreateFloatingDust(Transform parent)
    {
        GameObject go = new GameObject("Atmosphere_Dust");
        go.transform.SetParent(parent);
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            go.transform.SetParent(player.transform);
            go.transform.localPosition = new Vector3(0, 1.5f, 0);
        }

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 6f;
        main.startSpeed = 0.08f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.045f);
        main.startColor = new Color(0.75f, 0.55f, 0.34f, 0.22f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 80;
        var emission = ps.emission;
        emission.rateOverTime = 8f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(10f, 5f, 10f);
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.01f, 0.06f);
        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = GetOrCreateDustMaterial();
    }

    private static void AddFireParticles(GameObject parent)
    {
        ParticleSystem ps = parent.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.35f, 0.9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.23f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.22f, 0.015f), new Color(1f, 0.72f, 0.08f));
        main.maxParticles = 80;
        var emission = ps.emission;
        emission.rateOverTime = 22f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 13f;
        shape.radius = 0.18f;
        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, .7f, .12f), 0f), new GradientColorKey(new Color(.45f, .025f, 0f), 1f) },
            new[] { new GradientAlphaKey(.9f, 0f), new GradientAlphaKey(0f, 1f) });
        color.color = g;
        ParticleSystemRenderer r = parent.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = GetOrCreateFireMaterial();
    }

    private static Material GetOrCreateFireMaterial()
    {
        return GetOrCreateParticleMaterial("PolishFire.mat", new Color(1f, 0.25f, 0.025f, 0.9f));
    }

    private static Material GetOrCreateDustMaterial()
    {
        return GetOrCreateParticleMaterial("Dust.mat", new Color(0.75f, 0.52f, 0.28f, 0.25f));
    }

    private static Material GetOrCreateTrailMaterial()
    {
        return GetOrCreateParticleMaterial("ShadowTrail.mat", new Color(0.06f, 0.13f, 0.28f, 0.32f));
    }

    private static Material GetOrCreateParticleMaterial(string file, Color color)
    {
        EnsureFolder(PolishDir);
        string path = PolishDir + "/" + file;
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static AudioClip LoadClip(string path)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogWarning("Emberloop polish: missing audio clip at " + path);
        return clip;
    }

    private static float SafeInv(float value)
    {
        return Mathf.Abs(value) < 0.0001f ? 1f : 1f / value;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
