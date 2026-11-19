using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// Builds the Start screen, the Game Over screen, and the Level 0 test level.
/// In Unity: menu bar → "S.P.Dee" → "Build Start, Game Over and Level 0".
/// This OVERWRITES those three scene files, so only run it when you want fresh copies.
/// </summary>
public static class SceneBuilder
{
    const string PackPath = "Assets/2D Pixel Art Platformer Biome - American Forest";
    const string SheetPath = PackPath + "/Sprites.png";
    const string TilePath = PackPath + "/Tilemap/";
    const string ScenesPath = "Assets/Scenes/";
    const string PlaceholderPath = "Assets/Art/Placeholder/Square.png";
    const string NoFrictionPath = "Assets/Art/Placeholder/NoFriction.physicsMaterial2D";

    const string StartScene = "_Start_Scene";
    const string GameOverScene = "_GameOver_Scene";
    const string Level0Scene = "_Scene_Lvl_0";

    static readonly Color SkyColor = new Color(0.18f, 0.29f, 0.31f);

    // ---------------------------------------------------------------- menu

    [MenuItem("S.P.Dee/Build Start, Game Over and Level 0")]
    public static void BuildAll()
    {
        if (!EditorUtility.DisplayDialog("Build scenes",
                "This replaces _Start_Scene, _GameOver_Scene and _Scene_Lvl_0 with freshly built versions.\n\n" +
                "Any changes you made to those three scenes will be lost. Continue?",
                "Build", "Cancel"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        int groundLayer = EnsureLayer("Ground");
        int enemyLayer = EnsureLayer("Enemies");
        if (groundLayer < 0 || enemyLayer < 0)
        {
            EditorUtility.DisplayDialog("Build scenes", "No free layer slots for Ground/Enemies.", "OK");
            return;
        }

        BuildStartScene();
        BuildGameOverScene();
        BuildLevel0(groundLayer, enemyLayer);
        SetBuildSettings();
        AssetDatabase.SaveAssets();

        EditorSceneManager.OpenScene(ScenesPath + StartScene + ".unity");
        EditorUtility.DisplayDialog("Build scenes",
            "Done! Built _Start_Scene, _GameOver_Scene and _Scene_Lvl_0, and set the Build Settings order.\n\n" +
            "Press Play in _Start_Scene to try it.", "OK");
    }

    // ---------------------------------------------------------------- start screen

    static void BuildStartScene()
    {
        Scene scene = NewScene();
        CreateMenuCamera();
        CreateBackground(null, 0f, 3);

        Canvas canvas = CreateCanvas();
        MenuButtons menu = new GameObject("MenuButtons").AddComponent<MenuButtons>();

        CreateText(canvas.transform, "Title", "S. P. D. Is Late", 120, FontStyle.Bold, new Vector2(0, 250), new Vector2(1600, 200));
        CreateText(canvas.transform, "Subtitle", "S. P. Dee overslept. Get through the forest and make it to work on time!",
            40, FontStyle.Normal, new Vector2(0, 110), new Vector2(1600, 120));
        CreateText(canvas.transform, "Controls", "A / D or Left / Right arrows to move     Space to jump",
            32, FontStyle.Italic, new Vector2(0, 30), new Vector2(1600, 80));

        CreateButton(canvas.transform, "PlayButton", "Play", new Vector2(0, -110), menu.PlayGame);
        CreateButton(canvas.transform, "QuitButton", "Quit", new Vector2(0, -230), menu.QuitGame);

        CreateEventSystem();
        EditorSceneManager.SaveScene(scene, ScenesPath + StartScene + ".unity");
    }

    // ---------------------------------------------------------------- game over screen

    static void BuildGameOverScene()
    {
        Scene scene = NewScene();
        CreateMenuCamera();
        CreateBackground(null, 0f, 3);

        Canvas canvas = CreateCanvas();
        MenuButtons menu = new GameObject("MenuButtons").AddComponent<MenuButtons>();

        CreateText(canvas.transform, "Title", "You made it to work!", 110, FontStyle.Bold, new Vector2(0, 250), new Vector2(1600, 200));
        Text stats = CreateText(canvas.transform, "Stats", "Time: 0:00.00\nFalls: 0", 56, FontStyle.Normal,
            new Vector2(0, 60), new Vector2(1200, 180));

        GameOverScreen screen = new GameObject("GameOverScreen").AddComponent<GameOverScreen>();
        SetField(screen, "statsText", stats);

        CreateButton(canvas.transform, "PlayAgainButton", "Play Again", new Vector2(0, -110), menu.PlayGame);
        CreateButton(canvas.transform, "MainMenuButton", "Main Menu", new Vector2(0, -230), menu.OpenMainMenu);

        CreateEventSystem();
        EditorSceneManager.SaveScene(scene, ScenesPath + GameOverScene + ".unity");
    }

    // ---------------------------------------------------------------- level 0

    static void BuildLevel0(int groundLayer, int enemyLayer)
    {
        Scene scene = NewScene();
        int groundMask = 1 << groundLayer;

        // ---- terrain: column heights (top row of ground per x), then floating platforms
        const int bottomRow = -10;
        var heights = new Dictionary<int, int>();
        void Ground(int x0, int x1, int top) { for (int x = x0; x <= x1; x++) heights[x] = top; }

        Ground(-14, -13, 10);   // wall at the start
        Ground(-12, 20, 0);     // start area
        // 21-23: first gap (teaches jumping)
        Ground(24, 45, 0);      // beetle field
        Ground(46, 60, 2);      // raised section with the hedgehog
        Ground(61, 65, -2);     // spike pit floor
        Ground(66, 76, 0);
        // 77-80: gap guarded by a bee
        Ground(81, 110, 0);     // long sprint stretch to the goal
        Ground(111, 112, 10);   // wall at the end

        var cells = new HashSet<Vector2Int>();
        foreach (var kv in heights)
            for (int y = bottomRow; y <= kv.Value; y++) cells.Add(new Vector2Int(kv.Key, y));

        void Platform(int x0, int x1, int top) { for (int x = x0; x <= x1; x++) { cells.Add(new Vector2Int(x, top)); cells.Add(new Vector2Int(x, top - 1)); } }
        Platform(35, 39, 4);    // optional high route over the beetle
        Platform(68, 71, 4);

        // ---- grid + tilemaps
        GameObject grid = new GameObject("Grid", typeof(Grid));

        Tilemap groundMap = CreateTilemap(grid.transform, "Ground", 0);
        groundMap.gameObject.layer = groundLayer;
        PaintGround(groundMap, cells, bottomRow);
        var tmCol = groundMap.gameObject.AddComponent<TilemapCollider2D>();
        tmCol.usedByComposite = true;
        var groundRb = groundMap.gameObject.AddComponent<Rigidbody2D>();
        groundRb.bodyType = RigidbodyType2D.Static;
        var composite = groundMap.gameObject.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

        Tilemap spikeMap = CreateTilemap(grid.transform, "Hazards", 1);
        TileBase spikes = Tile("TileSpikes");
        for (int x = 61; x <= 65; x++) spikeMap.SetTile(new Vector3Int(x, -1, 0), spikes);
        var spikeCol = spikeMap.gameObject.AddComponent<TilemapCollider2D>();
        spikeCol.isTrigger = true;
        spikeMap.gameObject.AddComponent<KillZone>();

        Tilemap decoMap = CreateTilemap(grid.transform, "Decoration", 1);
        int[] plantXs = { -9, -4, 3, 7, 12, 17, 27, 41, 44, 49, 57, 67, 74, 84, 88, 93, 99, 103, 108 };
        for (int i = 0; i < plantXs.Length; i++)
        {
            int x = plantXs[i];
            if (!heights.TryGetValue(x, out int top)) continue;
            decoMap.SetTile(new Vector3Int(x, top + 1, 0), Tile("TilePlant" + (i % 12 + 1)));
        }

        // ---- trees in the background
        Transform decor = new GameObject("Trees").transform;
        float[] treeXs = { -8f, 9f, 29f, 52f, 72f, 89f, 101f };
        for (int i = 0; i < treeXs.Length; i++)
        {
            Sprite tree = PackSprite("Tree" + (i % 8 + 1));
            if (tree == null) continue;
            int x = Mathf.RoundToInt(treeXs[i]);
            float groundTop = heights.TryGetValue(x, out int t) ? t + 1 : 1;
            var sr = CreateSprite(decor, "Tree" + (i + 1), tree, new Vector3(treeXs[i], groundTop + tree.bounds.extents.y - tree.bounds.center.y, 0), -20);
            sr.color = new Color(0.85f, 0.9f, 0.85f);
        }

        // ---- player (placeholder until Jacob's is ready)
        Sprite square = PlaceholderSprite();
        Vector3 start = new Vector3(-8f, 2f, 0f);
        GameObject startPoint = new GameObject("StartPoint");
        startPoint.transform.position = start;

        GameObject player = new GameObject("Player (placeholder)");
        player.tag = "Player";
        player.transform.position = start;
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = square;
        psr.color = new Color(0.82f, 0.68f, 0.5f);
        psr.sortingOrder = 10;
        player.transform.localScale = new Vector3(0.9f, 1.2f, 1f);
        var prb = player.AddComponent<Rigidbody2D>();
        prb.gravityScale = 3f;
        prb.freezeRotation = true;
        prb.interpolation = RigidbodyInterpolation2D.Interpolate;
        prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        prb.sharedMaterial = NoFrictionMaterial(); // so the player doesn't stick to walls mid-jump
        player.AddComponent<BoxCollider2D>();
        var controller = player.AddComponent<PlaceholderPlayer>();
        controller.groundLayer = groundMask;

        // ---- camera + background
        Camera cam = CreateCamera(new Vector3(start.x + 3f, 3f, -10f));
        var follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player.transform;
        CreateBackground(cam, 1f, 9);

        // ---- enemies
        CreatePatrolEnemy("Beetle", new Vector2(31f, 1.4f), new Color(0.35f, 0.22f, 0.12f), true, square, enemyLayer, groundMask);
        CreatePatrolEnemy("Beetle (platform)", new Vector2(37f, 5.4f), new Color(0.35f, 0.22f, 0.12f), true, square, enemyLayer, groundMask);
        CreatePatrolEnemy("Hedgehog", new Vector2(54f, 3.4f), new Color(0.55f, 0.55f, 0.6f), false, square, enemyLayer, groundMask);
        CreateFlyingEnemy("Bee", new Vector2(76f, 4f), new Vector2(5f, 0f), square, enemyLayer);

        // ---- checkpoint, goal, kill zone
        GameObject checkpoint = CreateSign("Checkpoint", PackSprite("Table1"), new Vector2(46.5f, 3f));
        var cp = checkpoint.AddComponent<Checkpoint>();
        SetField(cp, "spriteRenderer", checkpoint.GetComponent<SpriteRenderer>());
        SetField(cp, "activeSprite", PackSprite("Table2"));

        GameObject goal = CreateSign("Goal", PackSprite("Table2"), new Vector2(106f, 1f));
        goal.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        goal.AddComponent<LevelGoal>();

        GameObject killZone = new GameObject("KillZone (bottom)");
        killZone.transform.position = new Vector3(50f, -15f, 0f);
        var kz = killZone.AddComponent<BoxCollider2D>();
        kz.isTrigger = true;
        kz.size = new Vector2(300f, 4f);
        killZone.AddComponent<KillZone>();

        // ---- level manager
        var lm = new GameObject("LevelManager").AddComponent<LevelManager>();
        SetField(lm, "player", player.transform);
        SetField(lm, "startPoint", startPoint.transform);
        // Levels 1-4 are still empty, so for now Level 0 goes straight to the Game Over screen.
        // Clear this field once Level 1 is built so it loads the next level in Build Settings.
        SetField(lm, "nextSceneName", GameOverScene);

        EditorSceneManager.SaveScene(scene, ScenesPath + Level0Scene + ".unity");
    }

    // Picks the right grass/dirt tile for each cell based on its neighbours.
    static void PaintGround(Tilemap map, HashSet<Vector2Int> cells, int bottomRow)
    {
        TileBase[] t = new TileBase[10];
        for (int i = 1; i <= 9; i++) t[i] = Tile("TileGround" + i);

        foreach (Vector2Int c in cells)
        {
            bool top = !cells.Contains(c + Vector2Int.up);
            bool left = !cells.Contains(c + Vector2Int.left);
            bool right = !cells.Contains(c + Vector2Int.right);
            bool bottom = c.y > bottomRow && !cells.Contains(c + Vector2Int.down);
            if (left && right) { left = false; right = false; }

            int col = left ? 0 : right ? 2 : 1;
            int row = top ? 0 : bottom ? 2 : 1;
            map.SetTile(new Vector3Int(c.x, c.y, 0), t[row * 3 + col + 1]);
        }
    }

    // ---------------------------------------------------------------- level helpers

    static Tilemap CreateTilemap(Transform grid, string name, int order)
    {
        GameObject go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        go.transform.SetParent(grid, false);
        go.GetComponent<TilemapRenderer>().sortingOrder = order;
        return go.GetComponent<Tilemap>();
    }

    static void CreatePatrolEnemy(string name, Vector2 pos, Color color, bool stompable, Sprite sprite, int layer, int groundMask)
    {
        GameObject go = new GameObject(name);
        go.layer = layer;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1f, 0.7f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = 5;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.mass = 5f;
        go.AddComponent<BoxCollider2D>();

        GameObject check = new GameObject("GroundCheck");
        check.transform.SetParent(go.transform, false);
        check.transform.localPosition = new Vector3(0.55f, -0.4f, 0f);

        var enemy = go.AddComponent<PatrolEnemy>();
        SetField(enemy, "groundCheck", check.transform);
        SetMask(enemy, "groundLayer", groundMask);
        SetBool(enemy, "canBeStomped", stompable);
    }

    static void CreateFlyingEnemy(string name, Vector2 pos, Vector2 travel, Sprite sprite, int layer)
    {
        GameObject go = new GameObject(name);
        go.layer = layer;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.8f, 0.6f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 0.85f, 0.2f);
        sr.sortingOrder = 5;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        go.AddComponent<BoxCollider2D>();

        var enemy = go.AddComponent<FlyingEnemy>();
        var so = new SerializedObject(enemy);
        so.FindProperty("travelOffset").vector2Value = travel;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject CreateSign(string name, Sprite sprite, Vector2 groundPos)
    {
        GameObject go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;
        float lift = sprite != null ? sprite.bounds.extents.y - sprite.bounds.center.y : 0.5f;
        go.transform.position = new Vector3(groundPos.x, groundPos.y + lift, 0f);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 3f);
        return go;
    }

    // ---------------------------------------------------------------- shared helpers

    static Scene NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

    static Camera CreateCamera(Vector3 position)
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.position = position;
        Camera cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = SkyColor;
        go.AddComponent<AudioListener>();
        return cam;
    }

    static Camera CreateMenuCamera() => CreateCamera(new Vector3(0f, 3f, -10f));

    // The pack's five Background sprites, far (5) to near (1), repeated side by side.
    static void CreateBackground(Camera parallaxCamera, float maxFactor, int copies)
    {
        Transform root = new GameObject("Background").transform;
        float[] factors = { 0.3f, 0.5f, 0.7f, 0.85f, 0.95f };

        for (int layer = 5; layer >= 1; layer--)
        {
            Sprite s = PackSprite("Background" + layer);
            if (s == null) continue;

            Transform layerRoot = new GameObject("Background" + layer).transform;
            layerRoot.SetParent(root, false);
            if (parallaxCamera != null)
                layerRoot.gameObject.AddComponent<ParallaxLayer>().factor = factors[layer - 1] * maxFactor;

            float width = s.bounds.size.x;
            float startX = parallaxCamera != null ? parallaxCamera.transform.position.x - 2 * width : -width * (copies / 2);
            for (int i = 0; i < copies; i++)
                CreateSprite(layerRoot, "Copy" + i, s, new Vector3(startX + i * width, 4.5f, 0f), -100 + (5 - layer) * 10);
        }
    }

    static SpriteRenderer CreateSprite(Transform parent, string name, Sprite sprite, Vector3 pos, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return sr;
    }

    static Canvas CreateCanvas()
    {
        GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    static void CreateEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static Font UIFont() => Resources.GetBuiltinResource<Font>("Arial.ttf");

    static Text CreateText(Transform parent, string name, string text, int size, FontStyle style, Vector2 pos, Vector2 boxSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = boxSize;
        Text t = go.GetComponent<Text>();
        t.text = text;
        t.font = UIFont();
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        var shadow = go.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(4f, -4f);
        return t;
    }

    static void CreateButton(Transform parent, string name, string label, Vector2 pos, UnityAction onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(380, 90);

        Image img = go.GetComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;
        img.color = new Color(0.36f, 0.55f, 0.27f);

        Button button = go.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, onClick);

        Text text = CreateText(go.transform, "Label", label, 44, FontStyle.Bold, Vector2.zero, Vector2.zero);
        var trt = text.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
    }

    static TileBase Tile(string name)
    {
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>(TilePath + name + ".asset");
        if (tile == null) Debug.LogWarning("SceneBuilder: missing tile " + name);
        return tile;
    }

    static Sprite PackSprite(string name)
    {
        var sprite = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>().FirstOrDefault(s => s.name == name);
        if (sprite == null) Debug.LogWarning("SceneBuilder: missing sprite " + name);
        return sprite;
    }

    // A plain white 16x16 square used for placeholder player/enemy art.
    static Sprite PlaceholderSprite()
    {
        if (!File.Exists(PlaceholderPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PlaceholderPath));
            var tex = new Texture2D(16, 16);
            tex.SetPixels(Enumerable.Repeat(Color.white, 16 * 16).ToArray());
            File.WriteAllBytes(PlaceholderPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
        }

        var importer = (TextureImporter)AssetImporter.GetAtPath(PlaceholderPath);
        if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 16f)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderPath);
    }

    static PhysicsMaterial2D NoFrictionMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(NoFrictionPath);
        if (mat != null) return mat;
        Directory.CreateDirectory(Path.GetDirectoryName(NoFrictionPath));
        mat = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        AssetDatabase.CreateAsset(mat, NoFrictionPath);
        return mat;
    }

    static void SetField(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetField(Object target, string field, string value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).stringValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetBool(Object target, string field, bool value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetMask(Object target, string field, int mask)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).intValue = mask;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Adds a layer in Project Settings → Tags and Layers if it isn't there yet. Returns its index.
    static int EnsureLayer(string name)
    {
        int existing = LayerMask.NameToLayer(name);
        if (existing >= 0) return existing;

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty slot = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(slot.stringValue))
            {
                slot.stringValue = name;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }
        }
        return -1;
    }

    static void SetBuildSettings()
    {
        string[] order =
        {
            StartScene, "_Scene_Lvl_0", "_Scene_Lvl_1", "_Scene_Lvl_2", "_Scene_Lvl_3", "_Scene_Lvl_4", GameOverScene
        };
        EditorBuildSettings.scenes = order
            .Select(n => ScenesPath + n + ".unity")
            .Where(File.Exists)
            .Select(p => new EditorBuildSettingsScene(p, true))
            .ToArray();
    }
}
