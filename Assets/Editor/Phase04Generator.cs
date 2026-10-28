using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Monta a cena da Phase 4 do zero, a partir do desenho em Phase04Layout.
    ///
    /// Rodar de novo refaz a cena inteira e descarta o que voce tiver ajustado
    /// nela na mao — entao ajuste o layout no codigo enquanto estiver iterando.
    ///
    /// Menu:      Echoes > Gerar Phase 4 (Alchemical Ateliers)
    ///            Echoes > Validar layout da Phase 4
    /// Batchmode: -executeMethod EchoesOfCravejada.EditorTools.Phase04Generator.GenerateFromCommandLine
    /// </summary>
    public static class Phase04Generator
    {
        public const string ScenePath = "Assets/Scenes/" + Phase04Layout.SceneName + ".unity";
        private const string GroundLayerName = "Ground";

        private const string MarblePath = Phase02Assets.BrassFolder + "/marble_tiles.png";
        private const string MarblePrefix = "marble";

        // Os nomes dos sprites desta fase, na mesma pasta dos de latao.
        private const string Mercury = "mercury_body";
        private const string Glass = "glass_pane";
        private const string SilverPlate = "silver_plate";
        private const string MarbleBlock = "marble_block";
        private const string Circle = "transmutation_circle";
        private const string Mirror = "silver_mirror";
        private const string Emitter = "light_emitter";
        private const string Lock = "crystal_lock";
        private const string Drop = "mercury_drop";
        private const string Flask = "oxide_flask";
        private const string Tank = "glass_tank";
        private const string Shelf = "lab_shelf";
        private const string Cabinet = "reagent_cabinet";
        private const string Sheet = "prototype_sheet";
        private const string Slime = "quicksilver_slime";
        private const string Thrower = "flask_thrower";
        private const string Failed = "failed_experiment";
        private const string Sentinel = "transmuted_sentinel";
        private const string Assistant = "lab_assistant";
        private const string Barrier = "crystal_barrier";

        private static string PortraitName(int variant) => "portrait_" + variant;
        private static string WindowName(int variant) => "stained_window_" + variant;

        /// <summary>O fundo da camera: azul-acinzentado escuro, frio.</summary>
        private static readonly Color Gloom = new Color(0.06f, 0.07f, 0.11f);

        private static readonly Color BackTint = new Color(0.42f, 0.45f, 0.56f);
        private static readonly Color OxideGreen = new Color(0.36f, 0.95f, 0.60f);
        private static readonly Color LampGlow = new Color(0.86f, 0.90f, 1f);
        private static readonly Color RuneDim = new Color(0.50f, 0.55f, 0.68f);

        private static Material _unlit;
        private static Material _steam;
        private static int _groundLayer;

        /// <summary>O que os circulos de transmutacao mexem, pelo nome.</summary>
        private static readonly Dictionary<string, GameObject> Transmutables = new Dictionary<string, GameObject>();

        [MenuItem("Echoes/Gerar Phase 4 (Alchemical Ateliers)", false, 50)]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[Phase 4] Geracao cancelada.");
                return;
            }

            Build();
        }

        /// <summary>Confere que o mapa e navegavel sem precisar entrar no jogo.</summary>
        [MenuItem("Echoes/Validar layout da Phase 4", false, 51)]
        public static void ValidateLayout()
        {
            List<string> problems = Phase04Layout.Validate();

            if (problems.Count == 0)
            {
                Debug.Log("[Phase 4] Layout OK: cada circulo, espelho e fechadura esta ao alcance na ordem certa, " +
                          "do Portal de Marmore ate o Grande Laboratorio.");
            }
            else
            {
                foreach (string problem in problems) Debug.LogWarning($"[Phase 4] {problem}");
                Debug.LogError($"[Phase 4] {problems.Count} problema(s) no layout. Veja os avisos acima.");
            }

            // O que fica depois do boss aparece aqui, e esta certo: e da etapa B.
            foreach (string floor in Phase04Layout.UnreachableFloors())
            {
                Debug.Log($"[Phase 4] Chao sem acesso (pode ser so enfeite): {floor}");
            }
        }

        /// <summary>Ponto de entrada pro modo batch, sem nenhum dialogo.</summary>
        public static void GenerateFromCommandLine()
        {
            Build();
        }

        private static void Build()
        {
            Phase01Assets.EnsureFolders();
            _groundLayer = Phase01Assets.EnsureLayer(GroundLayerName);

            Phase01Assets.EnsureSortingLayers();
            Phase01Assets.VerifySortingLayers();

            foreach (string problem in Phase04Layout.Validate())
            {
                Debug.LogWarning($"[Phase 4] {problem}");
            }

            if (!Phase01Assets.SliceTileset() || !Phase01Assets.SliceBackgroundTileset() || !Phase01Assets.SliceObjects())
            {
                Debug.LogError("[Phase 4] Abortado: nao consegui fatiar as folhas do pack de caverna.");
                return;
            }

            // A bancada, Crisol, as paginas e o portao vem da arte de latao.
            if (!Phase02Assets.EnsureArt(new[] { 4 }) || !Phase02Assets.EnsureMechanicsArt())
            {
                Debug.LogError("[Phase 4] Abortado: nao consegui preparar a arte de latao.");
                return;
            }

            if (!EnsureArt())
            {
                Debug.LogError("[Phase 4] Abortado: nao consegui fatiar o tileset de marmore.");
                return;
            }

            Dictionary<string, TileBase> tiles = Phase02Assets.BuildTiles();
            Phase01Assets.AddTilesFrom(MarblePath, tiles);

            if (tiles.Count == 0)
            {
                Debug.LogError("[Phase 4] Abortado: nenhum tile foi criado.");
                return;
            }

            Sprite scrapSprite = Phase01Assets.CreateScrapPlaceholder();
            Sprite polishedSprite = Phase02Assets.LoadSprite(Phase02Assets.ScrapPolished);
            _steam = Phase01Assets.GetOrCreateFogMaterial();
            _unlit = Phase01Assets.GetOrCreateUnlitMaterial();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Transmutables.Clear();

            BuildTilemaps(tiles);
            BuildLighting();
            BuildDecor();
            BuildMarkers();

            var travel = new GameObject("Travel");
            SceneKit.CreateSpawnPoint(travel.transform, Phase04Layout.FromDistrict3SpawnId, Phase04Layout.SpawnTile);

            Transform scrap = Phase01Generator.BuildScrap(scrapSprite, polishedSprite, _groundLayer, Phase04Layout.SpawnTile);
            Phase01Generator.BuildCamera(scrap, Phase04Layout.Width, Phase04Layout.Height, Gloom);

            // O mapa desta fase conta com tudo o que ele ganhou ate aqui, entao
            // o Scrap ja chega com as tres — mesmo dando Play direto na cena.
            scrap.GetComponent<ScrapDash>().SetUnlockedFromStart(true);

            // A ferrugem saiu na purificacao do District 1: aqui ele ja chega polido.
            scrap.GetComponent<ScrapAppearance>().SetPolishedFromStart(true);
            scrap.gameObject.AddComponent<AbilityGrant>().Configure(true, true, true);

            var root = new GameObject("Mechanics").transform;

            BuildSpikes(Child(root, "Spikes"));
            BuildPools(Child(root, "Mercury"));
            BuildBlocks(Child(root, "Transmutables"));
            BuildVapor(Child(root, "Vapor"));
            BuildCircles(Child(root, "TransmutationCircles"));
            BuildLight(Child(root, "LightPuzzle"));
            BuildSecrets(Child(root, "Secrets"));
            BuildDrips(Child(root, "MercuryDrips"));
            BuildFoes(Child(root, "Enemies"));
            BuildWorkbenches(Child(root, "Workbenches"));
            BuildCrisol(root);
            BuildLore(Child(root, "Lore"));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneKit.AddSceneToBuildSettings(ScenePath);

            Debug.Log($"[Phase 4] Cena gerada: {ScenePath}. " +
                      $"Mapa {Phase04Layout.Width}x{Phase04Layout.Height}, {Phase04Layout.Rooms.Count} salas, " +
                      $"{Phase04Layout.Pools.Count} pocas de mercurio, {Phase04Layout.Circles.Count} circulos, " +
                      $"{Phase04Layout.Mirrors.Count} espelhos, {Phase04Layout.Foes.Count} inimigos.");
        }

        private static Transform Child(Transform parent, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static SpriteRenderer Sprite(GameObject target, string spriteName, string layer, int order)
        {
            var renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = Phase02Assets.LoadSprite(spriteName);
            renderer.sortingLayerName = layer;
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>Um bloco solido de celulas, com um sprite repetido por cima.</summary>
        private static GameObject SolidBlock(Transform parent, string name, Phase04Layout.Cells area, string spriteName, int order)
        {
            var blockObject = new GameObject(name) { layer = _groundLayer };
            blockObject.transform.SetParent(parent, false);
            blockObject.transform.position = new Vector3(area.X0, area.Y0, 0f);

            SpriteRenderer plate = Sprite(blockObject, spriteName, Phase01Assets.SortingTerrain, order);
            plate.drawMode = SpriteDrawMode.Tiled;
            plate.size = new Vector2(area.Width, area.Height);

            var collider = blockObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(area.Width, area.Height);
            collider.offset = new Vector2(area.Width * 0.5f, area.Height * 0.5f);

            return blockObject;
        }

        // ------------------------------------------------------------------
        // Arte
        // ------------------------------------------------------------------

        private static bool EnsureArt()
        {
            var feet = new Vector2(0.5f, 0f);
            var middle = new Vector2(0.5f, 0.5f);
            var corner = Vector2.zero;

            Phase02Assets.WriteCanvas(MarblePath, MarbleArt.Tiles());

            Phase02Assets.WriteSprite(Mercury, MarbleArt.Mercury(), corner, true);
            Phase02Assets.WriteSprite(Glass, MarbleArt.Glass(), corner, true);
            Phase02Assets.WriteSprite(SilverPlate, MarbleArt.SilverPlate(), corner, true);
            Phase02Assets.WriteSprite(MarbleBlock, MarbleArt.MarbleBlock(), corner, true);
            Phase02Assets.WriteSprite(Barrier, CrystalArt.Barrier(), corner, true);

            Phase02Assets.WriteSprite(Circle, MarbleArt.Circle(), feet);
            Phase02Assets.WriteSprite(Mirror, MarbleArt.Mirror(), middle);
            Phase02Assets.WriteSprite(Emitter, MarbleArt.Emitter(), middle);
            Phase02Assets.WriteSprite(Lock, MarbleArt.Lock(), middle);
            Phase02Assets.WriteSprite(Drop, MarbleArt.Drop(), middle);
            Phase02Assets.WriteSprite(Flask, MarbleArt.Flask(), middle);

            Phase02Assets.WriteSprite(Tank, MarbleArt.Tank(), feet);
            Phase02Assets.WriteSprite(Shelf, MarbleArt.Shelf(), feet);
            Phase02Assets.WriteSprite(Cabinet, MarbleArt.Cabinet(), feet);
            Phase02Assets.WriteSprite(Sheet, MarbleArt.PrototypeSheet(), middle);

            for (int variant = 0; variant < 3; variant++)
            {
                Phase02Assets.WriteSprite(PortraitName(variant), MarbleArt.Portrait(variant), middle);
            }

            for (int variant = 0; variant < 2; variant++)
            {
                Phase02Assets.WriteSprite(WindowName(variant), MarbleArt.Window(variant), feet);
            }

            Phase02Assets.WriteSprite(Slime, MarbleArt.Slime(), feet);
            Phase02Assets.WriteSprite(Thrower, MarbleArt.FlaskThrower(), feet);
            Phase02Assets.WriteSprite(Failed, MarbleArt.FailedExperiment(), feet);
            Phase02Assets.WriteSprite(Sentinel, MarbleArt.Sentinel(), feet);
            Phase02Assets.WriteSprite(Assistant, MarbleArt.Assistant(), feet);

            return Phase01Assets.SliceGrid(MarblePath, MarblePrefix);
        }

        // ------------------------------------------------------------------
        // Terreno
        // ------------------------------------------------------------------

        private static void BuildTilemaps(Dictionary<string, TileBase> tiles)
        {
            var gridObject = new GameObject("Grid");
            var grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Transform root = gridObject.transform;
            string terrain = Phase01Assets.SortingTerrain;

            Tilemap back = Phase01Generator.CreateTilemap(root, "TM_Background", terrain, 0);
            back.color = BackTint;

            Tilemap ground = Phase01Generator.CreateTilemap(root, "TM_Ground", terrain, 10);

            Tilemap decoration = Phase01Generator.CreateTilemap(root, "TM_Decoration", terrain, 20);
            decoration.color = new Color(0.85f, 0.90f, 1f);

            GameObject groundObject = ground.gameObject;
            groundObject.layer = _groundLayer;

            var body = groundObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;

            var tilemapCollider = groundObject.AddComponent<TilemapCollider2D>();
            var composite = groundObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            // Refaz o colisor quando a cena comeca: carregada no meio do jogo
            // ela chegava sem chao.
            groundObject.AddComponent<GroundColliderRefresh>();

            int width = Phase04Layout.Width;
            int height = Phase04Layout.Height;

            var backBuffer = new TileBase[width * height];
            var groundBuffer = new TileBase[width * height];
            var decorationBuffer = new TileBase[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + y * width;

                    backBuffer[index] = Phase01Generator.Resolve(tiles, Phase04Layout.BackgroundTileName(x, y));
                    groundBuffer[index] = Phase01Generator.Resolve(tiles, Phase04Layout.GroundTileName(x, y));

                    if (Phase04Layout.IsSpike(x, y))
                    {
                        decorationBuffer[index] = Phase01Generator.Resolve(tiles, Phase04Layout.SpikeTileName(x, y));
                    }
                }
            }

            var bounds = new BoundsInt(0, 0, 0, width, height, 1);
            back.SetTilesBlock(bounds, backBuffer);
            ground.SetTilesBlock(bounds, groundBuffer);
            decoration.SetTilesBlock(bounds, decorationBuffer);

            foreach (Tilemap tilemap in new[] { back, ground, decoration })
            {
                Phase01Generator.FinishTilemap(tilemap);
            }

            composite.GenerateGeometry();
        }

        // ------------------------------------------------------------------
        // Luz e cenario
        // ------------------------------------------------------------------

        private static void BuildLighting()
        {
            var root = new GameObject("Lighting");

            var globalObject = new GameObject("GlobalLight2D");
            globalObject.transform.SetParent(root.transform, false);

            // Fria e baixa: marmore no escuro, lustres apagados. O que acende a
            // sala sao os lampioes de laboratorio e o verde dos frascos.
            var global = globalObject.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.color = new Color(0.72f, 0.80f, 1f);
            global.intensity = 0.40f;

            int lamps = 0;
            int oxide = 0;

            foreach ((float x, float y, float radius, bool green) in Phase04Layout.Lamps)
            {
                if (green)
                {
                    Light2D glow = Phase01Generator.CreatePointLight(root.transform, $"OxideGlow_{++oxide}",
                        (x, y), OxideGreen, 0.75f, radius);
                    glow.gameObject.AddComponent<GlowPulse>().Configure(glow, null, 2.6f + oxide % 3 * 0.4f, oxide * 0.17f);
                }
                else
                {
                    Phase01Generator.CreatePointLight(root.transform, $"Lamp_{++lamps}", (x, y), LampGlow, 0.75f, radius);
                }
            }
        }

        /// <summary>Tanques, vitrais e estantes: sprites grandes, atras de tudo o que se mexe.</summary>
        private static void BuildDecor()
        {
            var root = new GameObject("Decor");

            foreach ((string kind, int x, int y) in Phase04Layout.Decor)
            {
                var decorObject = new GameObject($"{kind}_{x}_{y}");
                decorObject.transform.SetParent(root.transform, false);
                decorObject.transform.position = new Vector3(x + 1f, y, 0f);

                if (kind == "elevator")
                {
                    // A cabine do elevador do District 3, com os trilhos subindo ate o teto.
                    decorObject.transform.position = new Vector3(x, y - 4f / Phase01Assets.PixelsPerUnit, 0f);
                    Sprite(decorObject, Phase02Assets.ElevatorCage, Phase01Assets.SortingActors, -4);

                    var rails = new GameObject("Rails");
                    rails.transform.SetParent(decorObject.transform, false);
                    rails.transform.localPosition = new Vector3(0f, 3f, 0f);
                    SpriteRenderer rail = Sprite(rails, Phase02Assets.ElevatorRail, Phase01Assets.SortingTerrain, 4);
                    rail.drawMode = SpriteDrawMode.Tiled;
                    rail.size = new Vector2(3f, 8f);
                }
                else if (kind == "tank")
                {
                    Sprite(decorObject, Tank, Phase01Assets.SortingTerrain, 6);
                }
                else if (kind == "shelf")
                {
                    Sprite(decorObject, Shelf, Phase01Assets.SortingTerrain, 5);
                }
                else
                {
                    // Vitral: e janela, fica na parede do fundo.
                    Sprite(decorObject, WindowName(kind == "window1" ? 1 : 0), Phase01Assets.SortingTerrain, 3);
                }
            }
        }

        /// <summary>Objetos vazios marcando os lugares do layout, agrupados por sala.</summary>
        private static void BuildMarkers()
        {
            var root = new GameObject("Markers");
            var groups = new Dictionary<string, Transform>();

            foreach (Phase04Layout.Marker marker in Phase04Layout.Markers)
            {
                if (!groups.TryGetValue(marker.Group, out Transform group))
                {
                    group = Child(root.transform, marker.Group);
                    groups[marker.Group] = group;
                }

                var markerObject = new GameObject(marker.Name);
                markerObject.transform.SetParent(group, false);
                markerObject.transform.position = new Vector3(marker.X + 0.5f, marker.Y, 0f);
            }
        }

        // ------------------------------------------------------------------
        // Espinhos
        // ------------------------------------------------------------------

        private static void BuildSpikes(Transform parent)
        {
            for (int y = 0; y < Phase04Layout.Height; y++)
            {
                int start = -1;

                for (int x = 0; x <= Phase04Layout.Width; x++)
                {
                    bool spike = x < Phase04Layout.Width && Phase04Layout.IsSpike(x, y);

                    if (spike && start < 0) start = x;
                    if (spike || start < 0) continue;

                    int width = x - start;

                    var spikes = new GameObject($"Spikes_{start}_{y}");
                    spikes.transform.SetParent(parent, false);
                    spikes.transform.position = new Vector3(start + width * 0.5f, y + 0.35f, 0f);

                    var trigger = spikes.AddComponent<BoxCollider2D>();
                    trigger.isTrigger = true;
                    trigger.size = new Vector2(width - 0.3f, 0.6f);

                    spikes.AddComponent<Hazard>();
                    start = -1;
                }
            }
        }

        // ------------------------------------------------------------------
        // Mercurio
        // ------------------------------------------------------------------

        private static void BuildPools(Transform parent)
        {
            foreach (Phase04Layout.Pool pool in Phase04Layout.Pools)
            {
                GameObject poolObject = SolidBlock(parent, pool.Name, pool.Area, Mercury, 11);
                bool corrupted = !Mathf.Approximately(pool.Current, 0f);

                // O corrompido e esverdeado: da pra saber antes de pisar.
                if (corrupted) poolObject.GetComponent<SpriteRenderer>().color = new Color(0.70f, 1f, 0.78f);

                // A faixa logo acima da superficie: e ela que avisa o Scrap
                // de que ele esta pisando em mercurio.
                var surfaceObject = new GameObject("Surface");
                surfaceObject.transform.SetParent(poolObject.transform, false);
                surfaceObject.transform.localPosition = new Vector3(pool.Area.Width * 0.5f, pool.Area.Height + 0.3f, 0f);

                var trigger = surfaceObject.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(pool.Area.Width, 0.6f);

                surfaceObject.AddComponent<MercurySurface>().Configure(pool.Current);

                Transmutables[pool.Name] = poolObject;
            }
        }

        private static void BuildDrips(Transform parent)
        {
            Sprite drop = Phase02Assets.LoadSprite(Drop);

            for (int i = 0; i < Phase04Layout.Drips.Count; i++)
            {
                (int x, int y, int fall) = Phase04Layout.Drips[i];

                var dripObject = new GameObject($"MercuryDrip_{i + 1}");
                dripObject.transform.SetParent(parent, false);
                dripObject.transform.position = new Vector3(x + 0.5f, y + 0.9f, 0f);

                // Cada uma no seu ritmo, e nenhuma comecando junto.
                dripObject.AddComponent<MercuryDrip>().Configure(
                    2.2f + LevelMap.Pick(x, y, 5) * 0.25f, LevelMap.Pick(x, y + 1, 20) * 0.1f, fall + 0.9f, drop);
            }
        }

        // ------------------------------------------------------------------
        // Transmutacao
        // ------------------------------------------------------------------

        private static void BuildBlocks(Transform parent)
        {
            foreach (Phase04Layout.Block block in Phase04Layout.Blocks)
            {
                string sprite = block.Kind == Phase04Layout.BlockKind.Glass ? Glass
                    : block.Kind == Phase04Layout.BlockKind.Silver ? SilverPlate
                    : MarbleBlock;

                Transmutables[block.Name] = SolidBlock(parent, block.Name, block.Area, sprite, 12);
            }
        }

        private static void BuildVapor(Transform parent)
        {
            foreach ((string name, Phase04Layout.Cells area) in Phase04Layout.VaporLifts)
            {
                SteamVent vent = SceneKit.CreateSteamVent(parent, name,
                    new RectInt(area.X0, area.Y0, area.Width, area.Height),
                    SteamVent.Mode.Lift, 1, 0f, 0f, _steam);

                Transmutables[name] = vent.gameObject;
            }
        }

        private static GameObject[] Lookup(string[] names)
        {
            return names.Where(Transmutables.ContainsKey).Select(name => Transmutables[name]).ToArray();
        }

        private static void BuildCircles(Transform parent)
        {
            foreach (Phase04Layout.Circle circle in Phase04Layout.Circles)
            {
                var circleObject = new GameObject(circle.Name);
                circleObject.transform.SetParent(parent, false);
                circleObject.transform.position = new Vector3(circle.X + 0.5f, circle.Y, 0f);

                // A gravura fica rente ao chao e tem luz propria: e pra ser achada.
                SpriteRenderer rune = Sprite(circleObject, Circle, Phase01Assets.SortingTerrain, 21);
                rune.color = RuneDim;
                if (_unlit != null) rune.sharedMaterial = _unlit;

                var area = circleObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.6f, 2.2f);
                area.offset = new Vector2(0f, 1.1f);

                circleObject.AddComponent<TransmutationCircle>()
                    .Configure(circle.Effect, Lookup(circle.Vanish), Lookup(circle.Appear), rune);
            }
        }

        // ------------------------------------------------------------------
        // Espelhos, raio e fechadura
        // ------------------------------------------------------------------

        private static Gate BuildGate(Transform parent, string name, (int X, int Y, int Height) cells, Color tint)
        {
            var gateObject = new GameObject(name) { layer = _groundLayer };
            gateObject.transform.SetParent(parent, false);
            gateObject.transform.position = new Vector3(cells.X, cells.Y, 0f);

            SpriteRenderer bars = Sprite(gateObject, Phase02Assets.GateBars, Phase01Assets.SortingTerrain, 15);
            bars.drawMode = SpriteDrawMode.Tiled;
            bars.size = new Vector2(1f, cells.Height);
            bars.color = tint;

            var wall = gateObject.AddComponent<BoxCollider2D>();
            wall.size = new Vector2(1f, cells.Height);
            wall.offset = new Vector2(0.5f, cells.Height * 0.5f);

            var gate = gateObject.AddComponent<Gate>();
            gate.Configure(cells.Height + 1f, Vector2.up);
            return gate;
        }

        private static void BuildLight(Transform parent)
        {
            Gate door = BuildGate(parent, "Gate_Mirror", Phase04Layout.MirrorGate, new Color(0.85f, 0.90f, 1f));

            // A grade depois do laboratorio: fica fechada ate o boss existir e cair.
            BuildGate(parent, "Gate_Boss", Phase04Layout.BossGate, new Color(0.80f, 0.85f, 0.95f));

            // O emissor e o raio.
            (int X, int Y, int DirX, int DirY) emitter = Phase04Layout.Emitter;

            var emitterObject = new GameObject("LightEmitter");
            emitterObject.transform.SetParent(parent, false);
            emitterObject.transform.position = new Vector3(emitter.X + 0.5f, emitter.Y + 0.5f, 0f);

            SpriteRenderer lantern = Sprite(emitterObject, Emitter, Phase01Assets.SortingTerrain, 22);
            lantern.flipX = emitter.DirX < 0;

            var line = emitterObject.AddComponent<LineRenderer>();
            line.sharedMaterial = _unlit;
            line.startColor = new Color(1f, 0.96f, 0.62f, 0.95f);
            line.endColor = new Color(1f, 0.96f, 0.62f, 0.95f);
            line.startWidth = 0.16f;
            line.endWidth = 0.16f;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            line.useWorldSpace = true;
            line.sortingLayerName = Phase01Assets.SortingActors;
            line.sortingOrder = 4;
            line.positionCount = 0;

            emitterObject.AddComponent<LightBeam>().Configure(new Vector2(emitter.DirX, emitter.DirY));

            Phase01Generator.CreatePointLight(emitterObject.transform, "Glow",
                (emitter.X + 0.5f, emitter.Y + 0.5f), new Color(1f, 0.95f, 0.65f), 0.8f, 4f);

            // Os espelhos.
            foreach (Phase04Layout.Mirror mirror in Phase04Layout.Mirrors)
            {
                var center = new Vector2(mirror.X + 0.5f, mirror.Y + 0.5f);
                var stand = new Vector2(mirror.StandX + 0.5f, mirror.StandY + 1f);

                var mirrorObject = new GameObject(mirror.Name);
                mirrorObject.transform.SetParent(parent, false);
                mirrorObject.transform.position = center;

                SpriteRenderer glass = Sprite(mirrorObject, Mirror, Phase01Assets.SortingTerrain, 22);

                // A area de alcance cobre do espelho ate onde o Scrap para.
                var reach = mirrorObject.AddComponent<BoxCollider2D>();
                reach.isTrigger = true;
                reach.size = new Vector2(Mathf.Abs(stand.x - center.x) + 2.4f, Mathf.Abs(stand.y - center.y) + 2.4f);
                reach.offset = (stand - center) * 0.5f;

                // A face onde o raio bate: pequena, no centro.
                var faceObject = new GameObject("Face");
                faceObject.transform.SetParent(mirrorObject.transform, false);

                var face = faceObject.AddComponent<CircleCollider2D>();
                face.isTrigger = true;
                face.radius = 0.3f;

                mirrorObject.AddComponent<SilverMirror>().Configure(mirror.StartsSlash, face, glass);

                Phase01Generator.CreatePointLight(mirrorObject.transform, "Glow",
                    (center.x, center.y), LampGlow, 0.45f, 3f);
            }

            // A fechadura.
            (int X, int Y) lockTile = Phase04Layout.LockTile;

            var lockObject = new GameObject("CrystalLock");
            lockObject.transform.SetParent(parent, false);
            lockObject.transform.position = new Vector3(lockTile.X + 0.5f, lockTile.Y + 0.5f, 0f);

            SpriteRenderer crystal = Sprite(lockObject, Lock, Phase01Assets.SortingTerrain, 22);

            var target = lockObject.AddComponent<BoxCollider2D>();
            target.isTrigger = true;
            target.size = new Vector2(0.9f, 0.9f);

            Light2D lockGlow = Phase01Generator.CreatePointLight(lockObject.transform, "Glow",
                (lockTile.X + 0.5f, lockTile.Y + 0.5f), new Color(0.80f, 0.90f, 1f), 0.15f, 5f);

            lockObject.AddComponent<CrystalLock>().Configure(door, crystal, lockGlow);
        }

        // ------------------------------------------------------------------
        // Segredos
        // ------------------------------------------------------------------

        private static void BuildSecrets(Transform parent)
        {
            (string Name, int X, int Y, int Height) barrier = Phase04Layout.ArchiveBarrier;

            GameObject barrierObject = SolidBlock(parent, barrier.Name,
                new Phase04Layout.Cells(barrier.X, barrier.Y, barrier.X, barrier.Y + barrier.Height - 1), Barrier, 15);

            barrierObject.AddComponent<CrystalBarrier>().Configure("D4_" + barrier.Name);

            Phase01Generator.CreatePointLight(barrierObject.transform, "Glow",
                (barrier.X + 0.5f, barrier.Y + barrier.Height * 0.5f), new Color(0.80f, 0.88f, 1f), 0.4f, 3.5f);

            // O que o vapor do Salao de Mercurio alcanca: um reforco de chassi.
            (int X, int Y) secret = Phase04Layout.SecretChassisTile;

            var pickup = new GameObject("Secret_Chassis");
            pickup.transform.SetParent(parent, false);
            pickup.transform.position = new Vector3(secret.X + 0.5f, secret.Y + 1f, 0f);

            SpriteRenderer module = Sprite(pickup, Phase02Assets.DashModule, Phase01Assets.SortingActors, -1);
            module.color = new Color(1f, 0.85f, 0.45f);

            var trigger = pickup.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            pickup.AddComponent<RewardPickup>().Configure(RewardPickup.Kind.Chassis, "D4_Secret_Chassis");
        }

        // ------------------------------------------------------------------
        // Inimigos
        // ------------------------------------------------------------------

        private static void BuildFoes(Transform parent)
        {
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);

            foreach (Phase04Layout.Foe spot in Phase04Layout.Foes)
            {
                var foe = new GameObject(spot.Name);
                foe.transform.SetParent(parent, false);
                foe.transform.position = new Vector3((spot.X0 + spot.X1 + 1) * 0.5f, spot.Y, 0f);

                var body = foe.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;

                // Gatilho, e nao parede: o Scrap atravessa e apanha.
                var trigger = foe.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;

                float minX = spot.X0 + 0.5f;
                float maxX = spot.X1 + 0.5f;

                switch (spot.Kind)
                {
                    case Phase04Layout.FoeKind.Slime:
                        Sprite(foe, Slime, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(1.0f, 0.7f);
                        trigger.offset = new Vector2(0f, 0.4f);

                        foe.AddComponent<Damageable>().Configure(2, 3, shard);
                        foe.AddComponent<QuicksilverSlime>().Configure(minX, maxX);
                        break;

                    case Phase04Layout.FoeKind.FlaskThrower:
                        Sprite(foe, Thrower, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(0.8f, 1.5f);
                        trigger.offset = new Vector2(0f, 0.8f);

                        foe.AddComponent<Damageable>().Configure(2, 4, shard);
                        foe.AddComponent<FlaskThrower>().Configure(
                            Phase02Assets.LoadSprite(Flask), Phase02Assets.LoadSprite(Phase02Assets.AcidSurface));
                        break;

                    case Phase04Layout.FoeKind.FailedExperiment:
                        Sprite(foe, Failed, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(1.1f, 1.6f);
                        trigger.offset = new Vector2(0f, 0.85f);

                        foe.AddComponent<Damageable>().Configure(5, 6, shard);
                        foe.AddComponent<FailedExperiment>().Configure(minX, maxX);
                        break;

                    case Phase04Layout.FoeKind.Sentinel:
                        Sprite(foe, Sentinel, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(0.8f, 1.7f);
                        trigger.offset = new Vector2(0f, 0.9f);

                        foe.AddComponent<Damageable>().Configure(4, 5, shard);
                        foe.AddComponent<TransmutedSentinel>().Configure(minX, maxX);
                        break;
                }
            }
        }

        // ------------------------------------------------------------------
        // Bancadas, Crisol e a historia
        // ------------------------------------------------------------------

        private static void BuildWorkbenches(Transform parent)
        {
            foreach ((string name, int x, int y) in Phase04Layout.Workbenches)
            {
                var benchObject = new GameObject(name);
                benchObject.transform.SetParent(parent, false);
                benchObject.transform.position = new Vector3(x, y, 0f);

                Sprite(benchObject, Phase02Assets.Workbench, Phase01Assets.SortingTerrain, 21);

                var area = benchObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(3.6f, 2.5f);
                area.offset = new Vector2(1.5f, 1.25f);

                var rest = new GameObject("RestPoint");
                rest.transform.SetParent(benchObject.transform, false);
                rest.transform.position = new Vector3(x + 1.5f, y, 0f);

                benchObject.AddComponent<Workbench>().Configure(rest.transform);
            }
        }

        private static void BuildCrisol(Transform parent)
        {
            (int X, int Y) tile = Phase04Layout.CrisolTile;

            var crisolObject = new GameObject("Npc_Crisol");
            crisolObject.transform.SetParent(parent, false);
            crisolObject.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);

            Sprite(crisolObject, Phase02Assets.Crisol, Phase01Assets.SortingActors, -4);

            var area = crisolObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3.2f, 2.6f);
            area.offset = new Vector2(0f, 1.3f);

            crisolObject.AddComponent<Crisol>().Configure("Crisol", Phase04Layout.CrisolLines);
        }

        /// <summary>
        /// O que da pra ler com E: a placa, os diarios, os retratos e a ficha do
        /// prototipo. A Assistente tambem entra aqui — ela so fala de dentro do
        /// armario.
        /// </summary>
        private static void BuildLore(Transform parent)
        {
            foreach (Phase04Layout.LoreSpot spot in Phase04Layout.Lore)
            {
                var loreObject = new GameObject(spot.Name);
                loreObject.transform.SetParent(parent, false);
                loreObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

                var area = loreObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.6f, 2.5f);
                area.offset = new Vector2(0f, 1.25f);

                var artObject = new GameObject("Art");
                artObject.transform.SetParent(loreObject.transform, false);

                bool talks = false;

                if (spot.Kind == "sign")
                {
                    artObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                    Sprite(artObject, Phase02Assets.Plaque, Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "diary")
                {
                    Sprite(artObject, Phase02Assets.LoosePage, Phase01Assets.SortingTerrain, 22);
                }
                else if (spot.Kind.StartsWith("portrait"))
                {
                    artObject.transform.localPosition = new Vector3(0f, 2.6f, 0f);
                    Sprite(artObject, PortraitName(int.Parse(spot.Kind.Substring("portrait".Length))),
                        Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "sheet")
                {
                    artObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                    Sprite(artObject, Sheet, Phase01Assets.SortingTerrain, 6);
                }
                else
                {
                    // A Assistente: o armario, de porta entreaberta.
                    Sprite(artObject, Cabinet, Phase01Assets.SortingTerrain, 6);
                    talks = true;
                }

                var npc = loreObject.AddComponent<Npc>();

                if (talks) npc.Configure(spot.Title, spot.Lines);
                else npc.ConfigureReadable(spot.Title, spot.Lines);

                // Uma luz fraca, pra dar pra achar.
                Phase01Generator.CreatePointLight(loreObject.transform, "Glow",
                    (spot.X + 0.5f, spot.Y + 1.6f), new Color(0.95f, 0.92f, 0.80f), 0.45f, 3.5f);
            }
        }
    }
}
