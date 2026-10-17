#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class OneFinalRebuildBuilder
{
    const string ScenePath = "Assets/Scenes/Emberloop_Rebuilt.unity";
    const string MatDir = "Assets/FinalRebuild/Materials";
    const string TexDir = "Assets/FinalRebuild/Textures";
    const string ShadowPrefabPath = "Assets/FinalRebuild/ShadowEcho.prefab";
    const float Tile = 2f;

    static Material floorMat, wallMat, ceilingMat, woodMat, metalMat, blueRuneMat, orangeRuneMat, flameMat, emberMat, darkMat, echoMat;
    static AudioClip doorClip, fireClip, resetClip, runeClip, footstepsClip, ambienceClip, ritualHum, beaconImpact;
    static Transform worldRoot, gameplayRoot, decorRoot;
    static TimeLoopManager loopManager;
    static PlayerController playerController;
    static PlayerRecorder playerRecorder;
    static SparkCarrier sparkCarrier;
    static GameObject player;
    static readonly List<DoorController> doors = new List<DoorController>();
    static readonly List<PressurePlate> plates = new List<PressurePlate>();
    static readonly List<FinalAnchor> finalAnchors = new List<FinalAnchor>();

    enum Side { North, South, East, West }
    struct Opening
    {
        public Side side; public float offset; public float width;
        public Opening(Side s, float o, float w) { side=s; offset=o; width=w; }
    }
    struct Hole
    {
        public float xmin,xmax,zmin,zmax;
        public Hole(float a,float b,float c,float d){xmin=a;xmax=b;zmin=c;zmax=d;}
        public bool Contains(float x,float z)=>x>xmin&&x<xmax&&z>zmin&&z<zmax;
    }

    [MenuItem("Emberloop/ONE FINAL REBUILD")]
    public static void Build()
    {
        try
        {
            doors.Clear(); plates.Clear(); finalAnchors.Clear();
            PrepareAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.015f,.022f,.035f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(.014f,.02f,.03f);
            RenderSettings.fogStartDistance = 8f;
            RenderSettings.fogEndDistance = 62f;

            worldRoot = new GameObject("WORLD — REBUILT").transform;
            gameplayRoot = new GameObject("GAMEPLAY").transform;
            decorRoot = new GameObject("DECOR & LIGHTING").transform;

            BuildLightingAndManagers();
            BuildPlayer();
            BuildDungeon();
            WireGameplay();
            BuildAmbience();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
            EditorUtility.DisplayDialog("EMBERLOOP — One Final Rebuild", "Rebuild complete. Open Emberloop_Rebuilt and press Play. This scene is independent from every previous layout.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Rebuild failed", ex.Message + "\n\nCheck Console for the full trace.", "OK");
        }
    }

    static void PrepareAssets()
    {
        EnsureFolder("Assets/FinalRebuild"); EnsureFolder(MatDir); EnsureFolder(TexDir);
        EnsureTag("Shadow");
        SetNormalMap(TexDir + "/stone_wall_normal.png");
        SetNormalMap(TexDir + "/stone_floor_normal.png");
        SetNormalMap(TexDir + "/dark_wood_normal.png");

        Texture2D floorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/stone_floor.png");
        Texture2D floorNorm = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/stone_floor_normal.png");
        Texture2D wallTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/stone_wall.png");
        Texture2D wallNorm = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/stone_wall_normal.png");
        Texture2D woodTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/dark_wood.png");
        Texture2D woodNorm = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/dark_wood_normal.png");
        Texture2D flameTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VFX/FinalSubmission/SoftFlame.png");

        floorMat = LitMaterial(MatDir+"/StoneFloor.mat", new Color(.42f,.42f,.43f), floorTex, floorNorm, .18f, 0f);
        wallMat = LitMaterial(MatDir+"/StoneWall.mat", new Color(.30f,.33f,.37f), wallTex, wallNorm, .12f, 0f);
        ceilingMat = LitMaterial(MatDir+"/CeilingStone.mat", new Color(.16f,.18f,.21f), wallTex, wallNorm, .08f, 0f);
        woodMat = LitMaterial(MatDir+"/DarkWood.mat", new Color(.30f,.15f,.08f), woodTex, woodNorm, .22f, 0f);
        metalMat = LitMaterial(MatDir+"/BlackIron.mat", new Color(.065f,.075f,.09f), null, null, .32f, .72f);
        darkMat = LitMaterial(MatDir+"/Abyss.mat", new Color(.003f,.004f,.007f), null, null, .02f, 0f);
        blueRuneMat = EmissiveMaterial(MatDir+"/RuneBlue.mat", new Color(.025f,.12f,.28f), new Color(.04f,.55f,4.2f));
        orangeRuneMat = EmissiveMaterial(MatDir+"/RuneOrange.mat", new Color(.27f,.055f,.01f), new Color(5f,.55f,.025f));
        emberMat = EmissiveMaterial(MatDir+"/Ember.mat", new Color(.55f,.075f,.01f), new Color(7f,.5f,.02f));
        echoMat = EmissiveMaterial(MatDir+"/Echo.mat", new Color(.025f,.045f,.09f), new Color(.02f,.12f,.6f));
        flameMat = TransparentFlameMaterial(MatDir+"/FlameCard.mat", flameTex);

        doorClip = LoadAudio("Assets/Audio/Doors/door_heavy.wav");
        fireClip = LoadAudio("Assets/Audio/Fire/fire_loop.wav");
        resetClip = LoadAudio("Assets/Audio/Time/time_reset.wav");
        runeClip = LoadAudio("Assets/Audio/Puzzle/anchor_activate.wav");
        footstepsClip = LoadAudio("Assets/Audio/Player/footstep_one_shot.wav");
        ambienceClip = LoadAudio("Assets/Audio/Ambience/dungeon_ambience.wav");
        ritualHum = LoadAudio("Assets/Audio/Finale/ritual_hum.wav");
        beaconImpact = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Finale/beacon_impact.mp3");

        BuildShadowPrefab();
    }

    static void BuildLightingAndManagers()
    {
        var dl = new GameObject("Moonless Ambient");
        var light = dl.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = .06f; light.color = new Color(.32f,.39f,.52f); light.shadows = LightShadows.None;
        dl.transform.rotation = Quaternion.Euler(55f,-35f,0f); dl.transform.SetParent(decorRoot);

        var gm = new GameObject("GameManager"); gm.transform.SetParent(gameplayRoot);
        var resetSource = gm.AddComponent<AudioSource>(); resetSource.playOnAwake=false; resetSource.spatialBlend=0f;
        var resetFX = gm.AddComponent<TimeResetFX>(); resetFX.resetClip=resetClip; resetFX.flashColor=new Color(.85f,.19f,.035f);
        loopManager = gm.AddComponent<TimeLoopManager>(); loopManager.loopDuration=24f; loopManager.resetFX=resetFX;
        var hud = gm.AddComponent<EmberloopHUD>(); hud.loopManager=loopManager;
        gm.AddComponent<TutorialOverlay>();

        var intro = gm.AddComponent<IntroSequence>();
    }

    static void BuildPlayer()
    {
        player = new GameObject("Player"); player.tag="Player"; player.transform.SetParent(gameplayRoot); player.transform.position=new Vector3(0,.3f,-3.6f);
        var cc=player.AddComponent<CharacterController>(); cc.center=new Vector3(0,.9f,0); cc.height=1.8f; cc.radius=.3f; cc.stepOffset=.3f;
        var trigger=player.AddComponent<CapsuleCollider>(); trigger.isTrigger=true; trigger.center=new Vector3(0,.9f,0); trigger.height=1.8f; trigger.radius=.3f;
        playerController=player.AddComponent<PlayerController>(); playerController.moveSpeed=5f; playerController.sprintMultiplier=1.32f;
        playerRecorder=player.AddComponent<PlayerRecorder>(); playerRecorder.sampleInterval=.05f;
        var footSource=player.AddComponent<AudioSource>(); footSource.playOnAwake=false; footSource.spatialBlend=0f;
        var footsteps=player.AddComponent<PlayerFootsteps>(); footsteps.footstepClip=footstepsClip; footsteps.stepInterval=.44f; footsteps.volume=.25f;
        sparkCarrier=player.AddComponent<SparkCarrier>(); sparkCarrier.loopManager=loopManager;

        var camGO=new GameObject("Main Camera"); camGO.tag="MainCamera"; camGO.transform.SetParent(player.transform,false); camGO.transform.localPosition=new Vector3(0,1.55f,0);
        var cam=camGO.AddComponent<Camera>(); cam.fieldOfView=69f; cam.nearClipPlane=.035f; cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.006f,.009f,.015f);
        camGO.AddComponent<AudioListener>(); var bob=camGO.AddComponent<CameraBob>(); bob.controller=cc;
        playerController.cameraTransform=camGO.transform;

        var sparkVisual = Sphere("Carried Spark", Vector3.zero, Vector3.one*.11f, emberMat, camGO.transform, false);
        sparkVisual.transform.localPosition=new Vector3(.38f,-.32f,.78f); sparkVisual.SetActive(false);
        var spl=sparkVisual.AddComponent<Light>(); spl.type=LightType.Point; spl.color=new Color(1f,.23f,.02f); spl.intensity=1.8f; spl.range=2.6f; spl.shadows=LightShadows.None;
        sparkCarrier.carriedVisual=sparkVisual;

        loopManager.player=player.transform;
        loopManager.shadowPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(ShadowPrefabPath);
        var intro=UnityEngine.Object.FindFirstObjectByType<IntroSequence>(); if(intro!=null) intro.player=playerController;
    }

    static void BuildDungeon()
    {
        // Zone I: Echo Crypt
        var start = NewZone("I — THE ECHO CRYPT");
        BuildRoom(start, new Vector3(0,0,0), 10,10,3.7f, new[]{new Opening(Side.North,0,2.6f)}, null);
        AddRoomTorches(start, new[]{new Vector3(-4.55f,1.65f,-1.5f),new Vector3(4.55f,1.65f,1.5f)}, new[]{90f,-90f});
        AddColumn(start,new Vector3(-3.3f,0,3.2f),2.8f); AddColumn(start,new Vector3(3.3f,0,3.2f),2.8f);
        ScatterRubble(start,new Vector3(3.7f,0,-3.5f),5,.55f);
        DoorController d1=CreateDoor("Gate I — Echo",new Vector3(0,0,5f),0f,start); doors.Add(d1);
        PressurePlate p1=CreateRune("Rune I — Blue Echo",new Vector3(-3.1f,.12f,1.7f),false,start); p1.door=d1; plates.Add(p1);
        CreateHint(new Vector3(0,.8f,-2.4f),new Vector3(7,2,1),"WHEN TIME BREAKS, YOUR ECHO REPEATS THIS LOOP.  R REWINDS EARLY.",start);
        CreateHint(new Vector3(-2.8f,.8f,.8f),new Vector3(3,2,3),"BLUE RUNES ANSWER TO YOU — AND TO YOUR ECHO.",start);

        var c1=NewZone("CORRIDOR — ASHES");
        BuildRoom(c1,new Vector3(0,0,9.5f),4,9f,3.5f,new[]{new Opening(Side.South,0,2.6f),new Opening(Side.North,0,2.6f)},null);
        AddTorch(c1,new Vector3(-1.72f,1.65f,8.0f),90f,false); AddTorch(c1,new Vector3(1.72f,1.65f,11.0f),-90f,false);
        AddCeilingBeams(c1,new Vector3(0,0,9.5f),4,9,3.5f,3);

        // Zone II: Ember Vault
        var room2=NewZone("II — THE EMBER VAULT");
        BuildRoom(room2,new Vector3(0,0,18f),14,12,4.1f,new[]{new Opening(Side.South,0,2.6f),new Opening(Side.East,0,2.6f)},null);
        CreateCheckpoint("Checkpoint — Ember Vault",new Vector3(0,.3f,12.8f),0f,28f,"FIRE IS DIFFERENT. LIGHT A BRAZIER — ITS FLAME SURVIVES THE LOOP.",room2);
        CreateSparkSource(new Vector3(-3.8f,.15f,14.3f),room2);
        CreateBrazier("Brazier — Ember Vault",new Vector3(3.6f,0,19.4f),3.3f,room2);
        DoorController d2=CreateDoor("Gate II — Ember",new Vector3(7f,0,18f),90f,room2); doors.Add(d2);
        PressurePlate p2=CreateRune("Rune II — Firebound",new Vector3(4.4f,.12f,21.2f),true,room2); p2.door=d2; plates.Add(p2);
        AddColumn(room2,new Vector3(-4.7f,0,16f),3.25f); AddColumn(room2,new Vector3(-4.7f,0,21.3f),3.25f);
        AddColumn(room2,new Vector3(1.0f,0,15.0f),3.25f);
        AddRoomTorches(room2,new[]{new Vector3(-6.55f,1.8f,18f),new Vector3(0f,1.8f,23.55f)},new[]{90f,180f});
        ScatterRubble(room2,new Vector3(-5.2f,0,22.3f),7,.7f);
        CreateHint(new Vector3(2.8f,.8f,20.2f),new Vector3(5,2,5),"ORANGE RUNES ARE DEAD OUTSIDE FIRELIGHT.",room2);

        var c2=NewZone("CORRIDOR — TURN OF EMBERS");
        BuildRoom(c2,new Vector3(11f,0,18f),8,4,3.6f,new[]{new Opening(Side.West,0,2.6f),new Opening(Side.East,0,2.6f)},null);
        AddTorch(c2,new Vector3(10f,1.65f,16.25f),0f,false); AddCeilingBeams(c2,new Vector3(11,0,18),8,4,3.6f,2);

        // Zone III: Split Sanctum with twin pits
        var room3=NewZone("III — THE SPLIT SANCTUM");
        var holes=new List<Hole>{new Hole(19f,21.2f,12.2f,24.2f),new Hole(24.8f,28f,12.2f,24.2f)};
        BuildRoom(room3,new Vector3(23,0,18),16,18,4.4f,new[]{new Opening(Side.West,0,2.6f),new Opening(Side.North,0,2.6f)},holes);
        BuildPit(room3,19f,21.2f,12.2f,24.2f); BuildPit(room3,24.8f,28f,12.2f,24.2f);
        CreateCheckpoint("Checkpoint — Split Sanctum",new Vector3(15.8f,.3f,18f),90f,32f,"TWO ROUTES. TWO ECHOES. BOTH RUNES MUST ANSWER AT ONCE.",room3);
        CreateSparkSource(new Vector3(17.1f,.15f,15.0f),room3);
        CreateBrazier("Brazier — Split Sanctum",new Vector3(29.0f,0,18.3f),3.3f,room3);
        DoorController d3=CreateDoor("Gate III — Convergence",new Vector3(23,0,27f),0f,room3); doors.Add(d3);
        var multiGO=new GameObject("Twin Rune Controller"); multiGO.transform.SetParent(room3); var multi=multiGO.AddComponent<MultiPlateDoor>(); multi.door=d3;
        PressurePlate p3a=CreateRune("Rune III-A — Blue",new Vector3(17.4f,.12f,22.5f),false,room3); p3a.multiPlateDoor=multi; p3a.isPlateA=true; plates.Add(p3a);
        PressurePlate p3b=CreateRune("Rune III-B — Firebound",new Vector3(29.1f,.12f,20.5f),true,room3); p3b.multiPlateDoor=multi; p3b.isPlateA=false; plates.Add(p3b);
        AddColumn(room3,new Vector3(18f,0,11.6f),3.4f); AddColumn(room3,new Vector3(28.6f,0,25f),3.4f);
        AddRoomTorches(room3,new[]{new Vector3(15.15f,1.8f,13f),new Vector3(30.85f,1.8f,23f)},new[]{90f,-90f});
        CreateHint(new Vector3(22.8f,.8f,11.3f),new Vector3(8,2,3),"LIGHT THE BRAZIER FIRST. THEN WRITE TWO DIFFERENT PASTS.",room3);

        var c3=NewZone("CORRIDOR — TO THE BEACON");
        BuildRoom(c3,new Vector3(23,0,32.5f),4,11,4.1f,new[]{new Opening(Side.South,0,2.6f),new Opening(Side.North,0,3.4f)},null);
        AddTorch(c3,new Vector3(21.25f,1.8f,31f),90f,false); AddTorch(c3,new Vector3(24.75f,1.8f,35f),-90f,false); AddCeilingBeams(c3,new Vector3(23,0,32.5f),4,11,4.1f,4);

        // Zone IV: Final Ritual Hall
        var final=NewZone("IV — THE RITUAL OF THREE");
        BuildRoom(final,new Vector3(23,0,50f),24,24,6.8f,new[]{new Opening(Side.South,0,3.4f)},null);
        CreateCheckpoint("Checkpoint — Final Ritual",new Vector3(23,.3f,39.3f),0f,34f,"THREE ECHOES MUST TAKE THE SIGILS. FIRE BINDS THE LAST.",final);
        CreateSparkSource(new Vector3(19.5f,.15f,41.5f),final);
        CreateBrazier("Brazier — Last Flame",new Vector3(23,0,57.9f),3.6f,final);
        BuildFinalRitual(final);
        BuildFinalArchitecture(final);

        // Invisible fail plane under every area.
        var fall=new GameObject("Abyss Reset"); fall.transform.position=new Vector3(12,-3.1f,27); fall.transform.SetParent(gameplayRoot);
        var fallCol=fall.AddComponent<BoxCollider>(); fallCol.isTrigger=true; fallCol.size=new Vector3(85,1,95);
        var fallRB=fall.AddComponent<Rigidbody>(); fallRB.isKinematic=true; fallRB.useGravity=false;
        var fallScript=fall.AddComponent<FallResetZone>(); fallScript.loopManager=loopManager;
    }

    static void BuildFinalRitual(Transform parent)
    {
        // Central stepped dais
        Box("Dais Lower",new Vector3(23,.12f,50),new Vector3(6,.24f,6),wallMat,parent,true);
        Box("Dais Middle",new Vector3(23,.32f,50),new Vector3(4.8f,.18f,4.8f),floorMat,parent,true);
        Box("Dais Upper",new Vector3(23,.51f,50),new Vector3(3.5f,.20f,3.5f),wallMat,parent,true);
        for(int i=0;i<3;i++) Box("Ritual Step "+i,new Vector3(23,.09f+i*.12f,45.9f+i*.55f),new Vector3(3.5f-i*.35f,.18f,1.0f),floorMat,parent,true);

        GameObject beacon = new GameObject("THE BEACON"); beacon.transform.position=new Vector3(23,.7f,50); beacon.transform.SetParent(parent);
        var flame=CreateFlame(beacon.transform,Vector3.zero,1.4f,5.0f,10f,true); var beaconLight=flame.GetComponentInChildren<Light>();
        var controllerGO=new GameObject("Final Ritual Controller"); controllerGO.transform.SetParent(parent);
        var controller=controllerGO.AddComponent<FinalChamberController>(); controller.loopManager=loopManager; controller.playerController=playerController; controller.playerRecorder=playerRecorder; controller.beaconLight=beaconLight; controller.beaconFlame=flame.GetComponent<SafeFlameVisual>();
        var impactSource=controllerGO.AddComponent<AudioSource>(); impactSource.playOnAwake=false; impactSource.spatialBlend=0f; controller.impactSource=impactSource; controller.beaconImpactClip=beaconImpact;
        var hum=controllerGO.AddComponent<AudioSource>(); hum.clip=ritualHum; hum.loop=true; hum.playOnAwake=false; hum.volume=.36f; hum.spatialBlend=.2f; controller.ritualHumSource=hum;

        FinalAnchor a=CreateFinalAnchor("Sigil A",new Vector3(17.3f,.12f,51.5f),false,parent,controller);
        FinalAnchor b=CreateFinalAnchor("Sigil B",new Vector3(28.7f,.12f,51.5f),false,parent,controller);
        FinalAnchor c=CreateFinalAnchor("Sigil C — Firebound",new Vector3(23,.12f,56.3f),true,parent,controller);
        controller.anchors=new[]{a,b,c}; finalAnchors.AddRange(new[]{a,b,c});

        var endTrig=new GameObject("Beacon Ending Trigger"); endTrig.transform.position=new Vector3(23,1.0f,50); endTrig.transform.SetParent(parent);
        var ec=endTrig.AddComponent<SphereCollider>(); ec.isTrigger=true; ec.radius=1.45f;
        var erb=endTrig.AddComponent<Rigidbody>(); erb.isKinematic=true; erb.useGravity=false;
        var ebt=endTrig.AddComponent<FinalBeaconTrigger>(); ebt.controller=controller;
    }

    static void BuildFinalArchitecture(Transform parent)
    {
        // Monumental columns
        foreach(var p in new[]{new Vector3(15.2f,0,43f),new Vector3(30.8f,0,43f),new Vector3(15.2f,0,55.8f),new Vector3(30.8f,0,55.8f),new Vector3(18.5f,0,46f),new Vector3(27.5f,0,46f)}) AddColumn(parent,p,5.8f);
        // Sealed gate backdrop
        Box("Sealed Gate",new Vector3(23,2.6f,61.7f),new Vector3(5.2f,5.2f,.32f),woodMat,parent,true);
        for(int i=-2;i<=2;i++) Box("Gate Iron "+i,new Vector3(23+i*.85f,2.6f,61.5f),new Vector3(.12f,5.0f,.16f),metalMat,parent,false);
        Box("Gate Crossbar",new Vector3(23,2.6f,61.35f),new Vector3(5.0f,.20f,.20f),metalMat,parent,false);
        // Cloaked guardian silhouettes
        AddGuardian(parent,new Vector3(16.3f,0,58.6f),25f); AddGuardian(parent,new Vector3(29.7f,0,58.6f),-25f);
        AddRoomTorches(parent,new[]{new Vector3(12.2f,2.1f,49f),new Vector3(33.8f,2.1f,49f),new Vector3(19f,2.1f,61.55f),new Vector3(27f,2.1f,61.55f)},new[]{90f,-90f,180f,180f});
        AddCeilingBeams(parent,new Vector3(23,0,50),24,24,6.8f,6);
        ScatterRubble(parent,new Vector3(13.4f,0,59.3f),10,.9f); ScatterRubble(parent,new Vector3(32.4f,0,42.0f),8,.75f);
    }

    static void WireGameplay()
    {
        loopManager.doorsToReset=doors.ToArray();
        loopManager.platesToReset=plates.ToArray();
        loopManager.finalAnchorsToReset=finalAnchors.ToArray();
    }

    static void BuildAmbience()
    {
        var amb=new GameObject("Dungeon Ambience"); amb.transform.SetParent(decorRoot);
        var src=amb.AddComponent<AudioSource>(); src.clip=ambienceClip; src.loop=true; src.playOnAwake=true; src.volume=.26f; src.spatialBlend=0f;
    }

    static Transform NewZone(string name){var go=new GameObject(name);go.transform.SetParent(worldRoot);return go.transform;}

    static void BuildRoom(Transform parent, Vector3 center, float width, float depth, float height, Opening[] openings, List<Hole> holes)
    {
        int nx=Mathf.RoundToInt(width/Tile), nz=Mathf.RoundToInt(depth/Tile);
        float x0=center.x-width/2+Tile/2, z0=center.z-depth/2+Tile/2;
        for(int ix=0;ix<nx;ix++) for(int iz=0;iz<nz;iz++)
        {
            float x=x0+ix*Tile, z=z0+iz*Tile;
            bool skip=false; if(holes!=null) foreach(var h in holes) if(h.Contains(x,z)){skip=true;break;}
            if(!skip) Box("Floor",new Vector3(x,-.10f,z),new Vector3(Tile,.20f,Tile),floorMat,parent,true);
            Box("Ceiling",new Vector3(x,height+.09f,z),new Vector3(Tile,.18f,Tile),ceilingMat,parent,true);
        }
        // North/South walls
        for(int ix=0;ix<nx;ix++)
        {
            float x=x0+ix*Tile, off=x-center.x;
            if(!InOpening(Side.North,off,openings)) Box("Wall N",new Vector3(x,height/2,center.z+depth/2),new Vector3(Tile+.03f,height,.26f),wallMat,parent,true);
            if(!InOpening(Side.South,off,openings)) Box("Wall S",new Vector3(x,height/2,center.z-depth/2),new Vector3(Tile+.03f,height,.26f),wallMat,parent,true);
        }
        for(int iz=0;iz<nz;iz++)
        {
            float z=z0+iz*Tile, off=z-center.z;
            if(!InOpening(Side.East,off,openings)) Box("Wall E",new Vector3(center.x+width/2,height/2,z),new Vector3(.26f,height,Tile+.03f),wallMat,parent,true);
            if(!InOpening(Side.West,off,openings)) Box("Wall W",new Vector3(center.x-width/2,height/2,z),new Vector3(.26f,height,Tile+.03f),wallMat,parent,true);
        }
    }

    static bool InOpening(Side side,float offset,Opening[] openings)
    {
        if(openings==null) return false;
        foreach(var o in openings) if(o.side==side && Mathf.Abs(offset-o.offset)<o.width*.5f) return true;
        return false;
    }

    static DoorController CreateDoor(string name, Vector3 pos, float yRot, Transform parent)
    {
        var root=new GameObject(name); root.transform.SetParent(parent); root.transform.position=pos; root.transform.rotation=Quaternion.Euler(0,yRot,0);
        var blocker=root.AddComponent<BoxCollider>(); blocker.center=new Vector3(0,1.4f,0); blocker.size=new Vector3(2.55f,2.8f,.30f);
        var dc=root.AddComponent<DoorController>(); dc.blockingCollider=blocker; dc.openAngle=102f; dc.rotationSpeed=155f; dc.openHeight=0;
        var source=root.AddComponent<AudioSource>(); source.playOnAwake=false; source.spatialBlend=.75f; source.minDistance=2f; source.maxDistance=15f; dc.audioSource=source; dc.moveClip=doorClip;
        Transform lh=new GameObject("Left Hinge").transform; lh.SetParent(root.transform,false); lh.localPosition=new Vector3(-1.22f,0,0);
        Transform rh=new GameObject("Right Hinge").transform; rh.SetParent(root.transform,false); rh.localPosition=new Vector3(1.22f,0,0);
        MakeDoorLeaf(lh,true); MakeDoorLeaf(rh,false); dc.leftHinge=lh; dc.rightHinge=rh;
        // stone frame
        LocalBox(root.transform,"Frame L",new Vector3(-1.55f,1.55f,.05f),new Vector3(.32f,3.2f,.55f),wallMat,false);
        LocalBox(root.transform,"Frame R",new Vector3(1.55f,1.55f,.05f),new Vector3(.32f,3.2f,.55f),wallMat,false);
        LocalBox(root.transform,"Frame Top",new Vector3(0,3.05f,.05f),new Vector3(3.4f,.32f,.55f),wallMat,false);
        return dc;
    }

    static void MakeDoorLeaf(Transform hinge,bool left)
    {
        // Keep the moving leaf intentionally simple. Decorative children on a scaled leaf
        // inherit that non-uniform scale and can stretch into long rods when the hinge opens.
        // A single solid wooden leaf is stable and reads much better in motion.
        float dir=left?1f:-1f;
        var leaf=LocalBox(hinge,"Wooden Leaf",new Vector3(dir*.60f,1.4f,0),new Vector3(1.18f,2.8f,.18f),woodMat,false);

        // Small handle mounted directly under the hinge so it is not multiplied by leaf scale.
        var handle=Sphere("Iron Handle",Vector3.zero,Vector3.one*.07f,metalMat,hinge,false);
        handle.transform.localPosition=new Vector3(dir*1.02f,1.35f,-.16f);
    }

    static PressurePlate CreateRune(string name, Vector3 pos, bool firebound, Transform parent)
    {
        var root=new GameObject(name); root.transform.position=pos; root.transform.SetParent(parent);
        var col=root.AddComponent<BoxCollider>(); col.isTrigger=true; col.center=new Vector3(0,.18f,0); col.size=new Vector3(1.8f,.5f,1.8f);
        var rb=root.AddComponent<Rigidbody>(); rb.isKinematic=true; rb.useGravity=false;
        var baseObj=Cylinder("Rune Base",pos+Vector3.up*.04f,new Vector3(1.0f,.10f,1.0f),wallMat,root.transform,false); baseObj.transform.localPosition=new Vector3(0,.04f,0);
        var disc=Cylinder("Rune Disc",pos+Vector3.up*.13f,new Vector3(.72f,.04f,.72f),firebound?orangeRuneMat:blueRuneMat,root.transform,false); disc.transform.localPosition=new Vector3(0,.13f,0);
        var lightGO=new GameObject("Rune Light"); lightGO.transform.SetParent(root.transform,false); lightGO.transform.localPosition=new Vector3(0,.45f,0); var li=lightGO.AddComponent<Light>(); li.type=LightType.Point; li.color=firebound?new Color(1f,.18f,.02f):new Color(.05f,.32f,1f); li.intensity=.35f; li.range=2.5f; li.shadows=LightShadows.None;
        var au=root.AddComponent<AudioSource>(); au.playOnAwake=false; au.spatialBlend=.7f; au.minDistance=2f; au.maxDistance=12f;
        var pp=root.AddComponent<PressurePlate>(); pp.requireFirelight=firebound; pp.indicatorRenderer=disc.GetComponent<Renderer>(); pp.indicatorLight=li; pp.audioSource=au; pp.activationClip=runeClip;
        return pp;
    }

    static FinalAnchor CreateFinalAnchor(string name,Vector3 pos,bool firebound,Transform parent,FinalChamberController controller)
    {
        var root=new GameObject(name); root.transform.position=pos; root.transform.SetParent(parent);
        var col=root.AddComponent<BoxCollider>(); col.isTrigger=true; col.center=new Vector3(0,.2f,0); col.size=new Vector3(2.0f,.6f,2.0f);
        var rb=root.AddComponent<Rigidbody>(); rb.isKinematic=true; rb.useGravity=false;
        var baseObj=Cylinder("Sigil Base",pos,new Vector3(1.15f,.12f,1.15f),wallMat,root.transform,false); baseObj.transform.localPosition=new Vector3(0,.07f,0);
        var disc=Cylinder("Sigil Disc",pos,new Vector3(.82f,.045f,.82f),firebound?orangeRuneMat:blueRuneMat,root.transform,false); disc.transform.localPosition=new Vector3(0,.18f,0);
        var lg=new GameObject("Sigil Light"); lg.transform.SetParent(root.transform,false); lg.transform.localPosition=new Vector3(0,.55f,0); var li=lg.AddComponent<Light>(); li.type=LightType.Point; li.color=firebound?new Color(1f,.2f,.02f):new Color(.04f,.36f,1f); li.intensity=.5f; li.range=3f; li.shadows=LightShadows.None;
        var au=root.AddComponent<AudioSource>(); au.playOnAwake=false; au.spatialBlend=.8f;
        var fa=root.AddComponent<FinalAnchor>(); fa.requireFirelight=firebound; fa.controller=controller; fa.indicatorRenderer=disc.GetComponent<Renderer>(); fa.indicatorLight=li; fa.audioSource=au; fa.activationClip=runeClip;
        return fa;
    }

    static PersistentBrazier CreateBrazier(string name,Vector3 pos,float radius,Transform parent)
    {
        var root=new GameObject(name); root.transform.position=pos; root.transform.SetParent(parent);
        Cylinder("Brazier Base",pos,new Vector3(.55f,.18f,.55f),wallMat,root.transform,false).transform.localPosition=new Vector3(0,.18f,0);
        Cylinder("Brazier Bowl",pos,new Vector3(.75f,.12f,.75f),metalMat,root.transform,false).transform.localPosition=new Vector3(0,.56f,0);
        var trigger=root.AddComponent<SphereCollider>(); trigger.isTrigger=true; trigger.radius=1.0f; trigger.center=new Vector3(0,.65f,0);
        var rb=root.AddComponent<Rigidbody>(); rb.isKinematic=true; rb.useGravity=false;
        var flame=CreateFlame(root.transform,new Vector3(0,.68f,0),.85f,3.2f,6f,false); flame.name="Persistent Flame"; flame.SetActive(false);
        var fireLight=flame.GetComponentInChildren<Light>();
        var zone=new GameObject("Firelight Zone"); zone.transform.SetParent(root.transform,false); zone.transform.localPosition=new Vector3(0,.7f,0); var sc=zone.AddComponent<SphereCollider>(); sc.isTrigger=true; sc.radius=radius; zone.AddComponent<FireLightZone>(); zone.SetActive(false);
        var audio=root.AddComponent<AudioSource>(); audio.clip=fireClip; audio.loop=true; audio.playOnAwake=false; audio.spatialBlend=1f; audio.volume=.28f; audio.minDistance=2f; audio.maxDistance=14f;
        var pb=root.AddComponent<PersistentBrazier>(); pb.flameRoot=flame; pb.fireZoneRoot=zone; pb.fireLight=fireLight; pb.fireAudio=audio;
        return pb;
    }

    static void CreateSparkSource(Vector3 pos,Transform parent)
    {
        var root=new GameObject("Spark Shrine"); root.transform.position=pos; root.transform.SetParent(parent);
        Cylinder("Spark Pedestal",pos,new Vector3(.55f,.22f,.55f),wallMat,root.transform,false).transform.localPosition=new Vector3(0,.2f,0);
        var orb=Sphere("Spark",pos+Vector3.up*.85f,Vector3.one*.24f,emberMat,root.transform,false); orb.transform.localPosition=new Vector3(0,.9f,0);
        var li=orb.AddComponent<Light>(); li.type=LightType.Point; li.color=new Color(1f,.22f,.02f); li.intensity=2f; li.range=3.5f; li.shadows=LightShadows.None;
        var col=root.AddComponent<SphereCollider>(); col.isTrigger=true; col.radius=1f; col.center=new Vector3(0,.75f,0);
        var rb=root.AddComponent<Rigidbody>(); rb.isKinematic=true; rb.useGravity=false;
        var source=root.AddComponent<SparkSource>(); source.target=sparkCarrier; source.sourceLight=li;
    }

    static void CreateCheckpoint(string name,Vector3 pos,float yRot,float duration,string msg,Transform parent)
    {
        var go=new GameObject(name); go.transform.position=pos; go.transform.rotation=Quaternion.Euler(0,yRot,0); go.transform.SetParent(parent);
        var col=go.AddComponent<BoxCollider>(); col.isTrigger=true; col.size=new Vector3(3.0f,2.2f,.7f); col.center=new Vector3(0,.9f,0);
        var rb=go.AddComponent<Rigidbody>(); rb.isKinematic=true; rb.useGravity=false;
        var cp=go.AddComponent<LoopCheckpoint>(); cp.timeLoopManager=loopManager; cp.loopDurationOverride=duration; cp.checkpointMessage=msg;
    }

    static void CreateHint(Vector3 pos,Vector3 size,string msg,Transform parent)
    {
        var go=new GameObject("Hint"); go.transform.position=pos; go.transform.SetParent(parent); var col=go.AddComponent<BoxCollider>(); col.isTrigger=true; col.size=size;
        var rb=go.AddComponent<Rigidbody>(); rb.isKinematic=true; rb.useGravity=false; var h=go.AddComponent<HintTrigger>(); h.message=msg; h.duration=4.4f;
    }

    static GameObject CreateFlame(Transform parent,Vector3 localPos,float size,float intensity,float range,bool audio)
    {
        var root=new GameObject("Flame"); root.transform.SetParent(parent,false); root.transform.localPosition=localPos;
        for(int i=0;i<3;i++)
        {
            var q=GameObject.CreatePrimitive(PrimitiveType.Quad); q.name="FlameCard_"+i; q.transform.SetParent(root.transform,false); q.transform.localPosition=new Vector3(0,.45f,0); q.transform.localRotation=Quaternion.Euler(0,i*60f,0); q.transform.localScale=new Vector3(size*.72f,size*1.15f,1);
            RemoveCollider(q); q.GetComponent<Renderer>().sharedMaterial=flameMat;
        }
        var lgo=new GameObject("Fire Light"); lgo.transform.SetParent(root.transform,false); lgo.transform.localPosition=new Vector3(0,.65f,0); var l=lgo.AddComponent<Light>(); l.type=LightType.Point; l.color=new Color(1f,.19f,.02f); l.intensity=intensity; l.range=range; l.shadows=LightShadows.None;
        var sf=root.AddComponent<SafeFlameVisual>(); sf.flameLight=l; sf.lightBaseIntensity=intensity; sf.sway=.045f;
        if(audio && fireClip!=null){var a=root.AddComponent<AudioSource>();a.clip=fireClip;a.loop=true;a.playOnAwake=true;a.volume=.18f;a.spatialBlend=1f;a.minDistance=1.5f;a.maxDistance=10f;}
        return root;
    }

    static void AddTorch(Transform parent,Vector3 pos,float yRot,bool withAudio)
    {
        var root=new GameObject("Wall Torch"); root.transform.position=pos; root.transform.rotation=Quaternion.Euler(0,yRot,0); root.transform.SetParent(parent);
        LocalBox(root.transform,"Bracket",new Vector3(0,-.22f,0),new Vector3(.13f,.55f,.13f),metalMat,false);
        CreateFlame(root.transform,new Vector3(0,.10f,0),.42f,1.35f,4.1f,withAudio);
    }

    static void AddRoomTorches(Transform parent,Vector3[] pos,float[] rot){for(int i=0;i<pos.Length;i++)AddTorch(parent,pos[i],rot[i],i==0);}

    static void AddColumn(Transform parent,Vector3 pos,float height)
    {
        Cylinder("Column Base",pos,new Vector3(.52f,.14f,.52f),wallMat,parent,true).transform.position=pos+Vector3.up*.14f;
        Cylinder("Column",pos,new Vector3(.32f,height*.5f,.32f),wallMat,parent,true).transform.position=pos+Vector3.up*(height*.5f+.25f);
        Cylinder("Column Cap",pos,new Vector3(.48f,.12f,.48f),wallMat,parent,true).transform.position=pos+Vector3.up*(height+.35f);
    }

    static void AddGuardian(Transform parent,Vector3 pos,float rot)
    {
        var g=new GameObject("Stone Guardian"); g.transform.position=pos; g.transform.rotation=Quaternion.Euler(0,rot,0); g.transform.SetParent(parent);
        var body=GameObject.CreatePrimitive(PrimitiveType.Capsule); body.name="Cloaked Body"; body.transform.SetParent(g.transform,false); body.transform.localPosition=new Vector3(0,1.25f,0); body.transform.localScale=new Vector3(.75f,1.3f,.75f); body.GetComponent<Renderer>().sharedMaterial=darkMat; RemoveCollider(body);
        var head=Sphere("Head",Vector3.zero,Vector3.one*.45f,darkMat,g.transform,false); head.transform.localPosition=new Vector3(0,2.65f,0);
    }

    static void ScatterRubble(Transform parent,Vector3 center,int count,float radius)
    {
        var rng=new System.Random(Mathf.RoundToInt(center.x*31+center.z*17)+count);
        for(int i=0;i<count;i++)
        {
            float a=(float)rng.NextDouble()*Mathf.PI*2f, r=(float)rng.NextDouble()*radius;
            Vector3 p=center+new Vector3(Mathf.Cos(a)*r,.08f,Mathf.Sin(a)*r);
            var b=Box("Rubble",p,new Vector3(.18f+(float)rng.NextDouble()*.4f,.14f+(float)rng.NextDouble()*.3f,.18f+(float)rng.NextDouble()*.5f),wallMat,parent,false);
            b.transform.rotation=Quaternion.Euler((float)rng.NextDouble()*20f,(float)rng.NextDouble()*180f,(float)rng.NextDouble()*20f);
        }
    }

    static void BuildPit(Transform parent,float xmin,float xmax,float zmin,float zmax)
    {
        float cx=(xmin+xmax)/2, cz=(zmin+zmax)/2, w=xmax-xmin, d=zmax-zmin;
        Box("Pit Darkness",new Vector3(cx,-2.6f,cz),new Vector3(w,d*.02f,d),darkMat,parent,false);
        // inner stone lips / railings
        Box("Pit Rail",new Vector3(xmin,.62f,cz),new Vector3(.12f,1.05f,d),metalMat,parent,true);
        Box("Pit Rail",new Vector3(xmax,.62f,cz),new Vector3(.12f,1.05f,d),metalMat,parent,true);
        Box("Pit Rail",new Vector3(cx,.62f,zmin),new Vector3(w,1.05f,.12f),metalMat,parent,true);
        Box("Pit Rail",new Vector3(cx,.62f,zmax),new Vector3(w,1.05f,.12f),metalMat,parent,true);
    }

    static void AddCeilingBeams(Transform parent,Vector3 center,float width,float depth,float height,int count)
    {
        for(int i=1;i<=count;i++)
        {
            float t=i/(float)(count+1); float z=center.z-depth/2+t*depth;
            Box("Ceiling Beam",new Vector3(center.x,height-.13f,z),new Vector3(width-.3f,.18f,.18f),woodMat,parent,false);
        }
    }

    static GameObject Box(string name,Vector3 pos,Vector3 scale,Material mat,Transform parent,bool collider)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.position=pos; go.transform.localScale=scale; if(parent!=null)go.transform.SetParent(parent,true); go.GetComponent<Renderer>().sharedMaterial=mat; if(!collider)RemoveCollider(go); return go;
    }
    static GameObject LocalBox(Transform parent,string name,Vector3 localPos,Vector3 scale,Material mat,bool collider)
    {var go=Box(name,Vector3.zero,scale,mat,parent,collider);go.transform.localPosition=localPos;go.transform.localRotation=Quaternion.identity;return go;}
    static GameObject Cylinder(string name,Vector3 pos,Vector3 scale,Material mat,Transform parent,bool collider)
    {var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.position=pos;go.transform.localScale=scale;if(parent!=null)go.transform.SetParent(parent,true);go.GetComponent<Renderer>().sharedMaterial=mat;if(!collider)RemoveCollider(go);return go;}
    static GameObject Sphere(string name,Vector3 pos,Vector3 scale,Material mat,Transform parent,bool collider)
    {var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.transform.position=pos;go.transform.localScale=scale;if(parent!=null)go.transform.SetParent(parent,true);go.GetComponent<Renderer>().sharedMaterial=mat;if(!collider)RemoveCollider(go);return go;}
    static void RemoveCollider(GameObject go){var c=go.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);}

    static void BuildShadowPrefab()
    {
        if(AssetDatabase.LoadAssetAtPath<GameObject>(ShadowPrefabPath)!=null)AssetDatabase.DeleteAsset(ShadowPrefabPath);
        var root=new GameObject("Shadow Echo"); root.tag="Shadow";
        var col=root.AddComponent<CapsuleCollider>(); col.isTrigger=true; col.center=new Vector3(0,.9f,0); col.height=1.8f; col.radius=.34f;
        root.AddComponent<ShadowPlayback>(); var state=root.AddComponent<ShadowState>();
        var renderers=new List<Renderer>();
        GameObject torso=GameObject.CreatePrimitive(PrimitiveType.Capsule); torso.name="Torso"; torso.transform.SetParent(root.transform,false); torso.transform.localPosition=new Vector3(0,1.02f,0); torso.transform.localScale=new Vector3(.42f,.58f,.32f); RemoveCollider(torso); torso.GetComponent<Renderer>().sharedMaterial=echoMat; renderers.Add(torso.GetComponent<Renderer>());
        GameObject head=GameObject.CreatePrimitive(PrimitiveType.Sphere); head.name="Head"; head.transform.SetParent(root.transform,false); head.transform.localPosition=new Vector3(0,1.72f,0); head.transform.localScale=Vector3.one*.42f; RemoveCollider(head); head.GetComponent<Renderer>().sharedMaterial=echoMat; renderers.Add(head.GetComponent<Renderer>());
        for(int s=-1;s<=1;s+=2){GameObject arm=GameObject.CreatePrimitive(PrimitiveType.Capsule);arm.name="Arm";arm.transform.SetParent(root.transform,false);arm.transform.localPosition=new Vector3(s*.38f,1.08f,0);arm.transform.localScale=new Vector3(.14f,.42f,.14f);arm.transform.localRotation=Quaternion.Euler(0,0,s*13f);RemoveCollider(arm);arm.GetComponent<Renderer>().sharedMaterial=echoMat;renderers.Add(arm.GetComponent<Renderer>());GameObject leg=GameObject.CreatePrimitive(PrimitiveType.Capsule);leg.name="Leg";leg.transform.SetParent(root.transform,false);leg.transform.localPosition=new Vector3(s*.16f,.35f,0);leg.transform.localScale=new Vector3(.16f,.38f,.16f);RemoveCollider(leg);leg.GetComponent<Renderer>().sharedMaterial=echoMat;renderers.Add(leg.GetComponent<Renderer>());}
        var fx=root.AddComponent<EchoVisualController>(); fx.shadowState=state; fx.renderers=renderers.ToArray();
        PrefabUtility.SaveAsPrefabAsset(root,ShadowPrefabPath); UnityEngine.Object.DestroyImmediate(root);
    }

    static Material LitMaterial(string path,Color color,Texture2D tex,Texture2D normal,float smooth,float metallic)
    {
        var m=LoadOrCreateMaterial(path,"Universal Render Pipeline/Lit");
        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
        if(tex!=null){m.SetTexture("_BaseMap",tex);m.SetTextureScale("_BaseMap",Vector2.one);}
        if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.65f);}
        if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth); if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);
        EditorUtility.SetDirty(m); return m;
    }
    static Material EmissiveMaterial(string path,Color baseC,Color emission)
    {var m=LitMaterial(path,baseC,null,null,.18f,0f);m.EnableKeyword("_EMISSION");if(m.HasProperty("_EmissionColor"))m.SetColor("_EmissionColor",emission);EditorUtility.SetDirty(m);return m;}
    static Material TransparentFlameMaterial(string path,Texture2D tex)
    {
        var m=LoadOrCreateMaterial(path,"Universal Render Pipeline/Unlit"); if(tex!=null)m.SetTexture("_BaseMap",tex); if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",new Color(1f,.34f,.045f,.88f));
        m.SetFloat("_Surface",1f);m.SetFloat("_Blend",0f);m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)BlendMode.One);m.SetFloat("_ZWrite",0f);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;EditorUtility.SetDirty(m);return m;
    }
    static Material LoadOrCreateMaterial(string path,string shaderName){var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find(shaderName));AssetDatabase.CreateAsset(m,path);}else m.shader=Shader.Find(shaderName);return m;}
    static AudioClip LoadAudio(string path)=>AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    static void EnsureFolder(string path){if(AssetDatabase.IsValidFolder(path))return;string parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');string name=System.IO.Path.GetFileName(path);EnsureFolder(parent);AssetDatabase.CreateFolder(parent,name);}
    static void SetNormalMap(string path){var ti=AssetImporter.GetAtPath(path) as TextureImporter;if(ti!=null&&ti.textureType!=TextureImporterType.NormalMap){ti.textureType=TextureImporterType.NormalMap;ti.SaveAndReimport();}}

    static void EnsureTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");
        for(int i=0;i<tagsProp.arraySize;i++) if(tagsProp.GetArrayElementAtIndex(i).stringValue==tag) return;
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize-1).stringValue=tag;
        tagManager.ApplyModifiedProperties();
    }
}
#endif
