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
    /// Monta a cena da Phase 3 do zero, a partir do desenho em Phase03Layout.
    ///
    /// Rodar de novo refaz a cena inteira e descarta o que voce tiver ajustado
    /// nela na mao — entao ajuste o layout no codigo enquanto estiver iterando.
    ///
    /// Menu:      Echoes > Gerar Phase 3 (Silent Geodes)
    ///            Echoes > Validar layout da Phase 3
    /// Batchmode: -executeMethod EchoesOfCravejada.EditorTools.Phase03Generator.GenerateFromCommandLine
    /// </summary>
    public static class Phase03Generator
    {
        public const string ScenePath = "Assets/Phases/" + Phase03Layout.SceneName + "/Scene/" + Phase03Layout.SceneName + ".unity";
        private const string GroundLayerName = "Ground";

        // Os nomes dos sprites desta fase, na mesma pasta dos de latao.
        private const string Resonant = "crystal_resonant";
        private const string Vein = "crystal_vein";
        private const string FragileTile = "crystal_fragile";
        private const string Glyph = "crystal_glyph";
        private const string ShardShot = "shard_shot";
        private const string GeodeDrill = "geode_drill";
        private const string Sapphire = "sapphire_raw";
        private const string Barrier = "crystal_barrier";
        private const string Rubble = "elevator_rubble";
        private const string ButtonUp = "statue_button_up";
        private const string ButtonDown = "statue_button_down";
        private const string SilverPlaque = "silver_plaque";
        private const string AncientKey = "ancient_key";
        private const string ResonantPrism = "resonant_prism";
        private const string ResonantBase = "resonant_base";
        private const string CaveGate = "cave_gate";
        private const string ForemanSheet = "foreman_sheet";
        private const string MiteSheet = "d3_mite_sheet";
        private const string HunterSheet = "d3_hunter_sheet";
        private const string SentinelSheet = "d3_sentinel_sheet";
        private const string KeyLock = "key_lock";

        /// <summary>
        /// A vida dos mites: um golpe so, com ou sem rubi. E eles nao soltam
        /// fragmento nenhum.
        /// </summary>
        private const int MiteHealth = 1;

        /// <summary>
        /// Quanto tempo um pilar da arena fica aceso depois do golpe. Curto de
        /// proposito: o primeiro toque chama a broca, e e preciso tocar de novo
        /// com ela chegando pra ela cravar.
        /// </summary>
        private const float DrillPillarLitTime = 1.2f;

        private static string FrameName(string prefix, int frame) => $"{prefix}_{frame}";

        private static Sprite[] LoadFrames(string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++) frames[i] = Phase02Assets.LoadSprite(FrameName(prefix, i));
            return frames;
        }

        private static string ClusterName(int size, int hue) => $"crystal_cluster_{size}_{hue}";
        private static string StatueName(int kind) => "statue_" + kind;
        private static string StatueGlowName(int kind) => "statue_glow_" + kind;
        private static string CrackName(int frame) => "statue_crack_" + frame;
        private static string PebblesName(int seed) => "rubble_pebbles_" + seed;

        /// <summary>A luz dos olhos das estatuas: branca e fria, a mesma pras quatro.</summary>
        private static readonly Color StatueGlowColor = new Color(0.85f, 0.92f, 1f);
        private static string EngravingName(int scene) => "engraving_" + scene;

        /// <summary>O fundo da camera: quase preto, puxado pro violeta.</summary>
        private static readonly Color Void = new Color(0.020f, 0.015f, 0.035f);

        private static readonly Color RockTint = new Color(0.78f, 0.72f, 1f);
        private static readonly Color BackTint = new Color(0.30f, 0.28f, 0.45f);
        private static readonly Color SpikeTint = new Color(0.80f, 0.62f, 1f);
        private static readonly Color OxideGreen = new Color(0.36f, 0.95f, 0.60f);
        private static readonly Color LampGlow = new Color(1f, 0.82f, 0.55f);

        /// <summary>A luz de cada familia de cristal: ametista, quartzo leitoso e cristal fume.</summary>
        private static readonly Color[] HueGlow =
        {
            new Color(0.74f, 0.52f, 1f),
            new Color(0.80f, 0.88f, 1f),
            new Color(0.70f, 0.66f, 0.85f),
        };

        /// <summary>A cor de cada nota. A 0 e a dos cristais que sao so isca.</summary>
        private static readonly Color[] NoteColors =
        {
            new Color(0.90f, 0.88f, 1f),
            new Color(1f, 0.40f, 0.50f),
            new Color(1f, 0.80f, 0.30f),
            new Color(0.40f, 0.90f, 1f),
            new Color(0.62f, 0.50f, 1f),
        };

        private static Material _unlit;
        private static int _groundLayer;

        // O que uma parte da montagem deixa pra outra.
        private static readonly List<Transform> SlabRoots = new List<Transform>();
        private static readonly Dictionary<string, List<ResonantCrystal>> DoorCrystals =
            new Dictionary<string, List<ResonantCrystal>>();
        private static readonly List<ResonantCrystal> Pillars = new List<ResonantCrystal>();
        private static readonly List<Light2D> OxideLights = new List<Light2D>();
        private static readonly Dictionary<string, Gate> BuiltGates = new Dictionary<string, Gate>();

        [MenuItem("Echoes/Gerar Phase 3 (Silent Geodes)", false, 40)]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[Phase 3] Geracao cancelada.");
                return;
            }

            Build();
        }

        /// <summary>Confere que o mapa e navegavel sem precisar entrar no jogo.</summary>
        [MenuItem("Echoes/Validar layout da Phase 3", false, 41)]
        public static void ValidateLayout()
        {
            List<string> problems = Phase03Layout.Validate();

            if (problems.Count == 0)
            {
                Debug.Log("[Phase 3] Layout OK: da pra tocar a Porta Harmonica e ir da Fronteira ate a Camara de Perfuracao.");
            }
            else
            {
                foreach (string problem in problems) Debug.LogWarning($"[Phase 3] {problem}");
                Debug.LogError($"[Phase 3] {problems.Count} problema(s) no layout. Veja os avisos acima.");
            }

            // A saida gradeada aparece aqui, e esta certo: ela so abre com o boss.
            foreach (string floor in Phase03Layout.UnreachableFloors())
            {
                Debug.Log($"[Phase 3] Chao sem acesso (pode ser so enfeite): {floor}");
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

            foreach (string problem in Phase03Layout.Validate())
            {
                Debug.LogWarning($"[Phase 3] {problem}");
            }

            if (!Phase01Assets.SliceTileset() || !Phase01Assets.SliceBackgroundTileset() || !Phase01Assets.SliceObjects())
            {
                Debug.LogError("[Phase 3] Abortado: nao consegui fatiar as folhas do pack de caverna.");
                return;
            }

            // A fronteira ainda e de latao, e a bancada, Crisol e as paginas vem de la.
            if (!Phase02Assets.EnsureArt(new[] { 4 }) || !Phase02Assets.EnsureMechanicsArt())
            {
                Debug.LogError("[Phase 3] Abortado: nao consegui preparar a arte de latao.");
                return;
            }

            EnsureArt();

            Dictionary<string, TileBase> tiles = Phase02Assets.BuildTiles();
            if (tiles.Count == 0)
            {
                Debug.LogError("[Phase 3] Abortado: nenhum tile foi criado.");
                return;
            }

            Sprite scrapSprite = Phase01Assets.CreateScrapPlaceholder();
            Sprite polishedSprite = Phase02Assets.LoadSprite(Phase02Assets.ScrapPolished);
            Material fogMaterial = Phase01Assets.GetOrCreateFogMaterial();
            _unlit = Phase01Assets.GetOrCreateUnlitMaterial();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SlabRoots.Clear();
            DoorCrystals.Clear();
            Pillars.Clear();
            OxideLights.Clear();
            BuiltGates.Clear();

            BuildTilemaps(tiles);
            BuildLighting(fogMaterial);
            BuildMarkers();

            var travel = new GameObject("Travel");
            SceneKit.CreateSpawnPoint(travel.transform, Phase03Layout.FromDistrict2SpawnId, Phase03Layout.SpawnTile);

            // A saida pro District 4 e o proprio elevador da chegada (BuildElevator),
            // depois que os escombros do poco caem.

            Transform scrap = Phase01Generator.BuildScrap(scrapSprite, polishedSprite, _groundLayer, Phase03Layout.SpawnTile);
            Phase01Generator.BuildCamera(scrap, Phase03Layout.Width, Phase03Layout.Height, Void);

            // O mapa desta fase conta com o Steam Dash e o Pressure Jump, entao
            // aqui o Scrap ja chega com os dois — mesmo dando Play direto na cena.
            scrap.GetComponent<ScrapDash>().SetUnlockedFromStart(true);

            // A ferrugem saiu na purificacao do District 1: aqui ele ja chega polido.
            scrap.GetComponent<ScrapAppearance>().SetPolishedFromStart(true);
            scrap.gameObject.AddComponent<AbilityGrant>().Configure(true, true);
            scrap.gameObject.AddComponent<ScrapNoise>();

            // Os olhos dele iluminam bem menos aqui: quem mostra o caminho e o som.
            Transform eyes = scrap.Find("EyeGlow");
            if (eyes != null)
            {
                var eyeLight = eyes.GetComponent<Light2D>();
                eyeLight.intensity = 0.5f;

                var serialized = new SerializedObject(eyeLight);
                serialized.FindProperty("m_PointLightOuterRadius").floatValue = 3f;
                serialized.FindProperty("m_PointLightInnerRadius").floatValue = 0.6f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            var root = new GameObject("Mechanics").transform;

            BuildSpikes(Child(root, "Spikes"));

            // Os chaos da arena vem antes: cristais e pilares em cima deles
            // sao filhos deles, pra sumirem junto quando desabam.
            BuildSlabs(Child(root, "CollapsingFloors"));
            BuildEchoCrystals(Child(root, "EchoCrystals"));
            BuildFragiles(Child(root, "FragilePlatforms"));
            BuildResonants(Child(root, "ResonantCrystals"));
            BuildDoors(Child(root, "Gates"));
            BuildBarriers(Child(root, "CrystalBarriers"));
            BuildElevator(Child(root, "Elevator"));
            BuildRewards(Child(root, "Rewards"));
            BuildDecor(Child(root, "Decor"));

            Transform enemies = Child(root, "Enemies");
            BuildFoes(enemies);
            BuildElite(enemies);
            BuildStatues(Child(root, "StatueHall"), enemies);

            BuildWorkbenches(Child(root, "Workbenches"));
            GameObject crisol = BuildCrisol(root);
            BuildLore(Child(root, "Lore"));

            // Por ultimo: a purificacao precisa de tudo o que ela mexe, e o
            // boss precisa dela.
            AreaPurifier purifier = BuildPurification(root, enemies.gameObject, crisol);
            BuildBoss(root, purifier);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneKit.AddSceneToBuildSettings(ScenePath);

            Debug.Log($"[Phase 3] Cena gerada: {ScenePath}. " +
                      $"Mapa {Phase03Layout.Width}x{Phase03Layout.Height}, {Phase03Layout.Rooms.Count} salas, " +
                      $"{Phase03Layout.Echoes.Count} cristais de eco, {Phase03Layout.Resonants.Count} ressonantes, " +
                      $"{Phase03Layout.Foes.Count} inimigos, o Capataz e The Geode Drill.");
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

        // ------------------------------------------------------------------
        // Arte
        // ------------------------------------------------------------------

        private static void EnsureArt()
        {
            var feet = new Vector2(0.5f, 0f);
            var middle = new Vector2(0.5f, 0.5f);

            for (int size = 0; size < CrystalArt.SizeCount; size++)
            {
                for (int hue = 0; hue < CrystalArt.HueCount; hue++)
                {
                    Phase02Assets.WriteSprite(ClusterName(size, hue), CrystalArt.Cluster(size, hue), feet);
                }
            }

            Phase02Assets.WriteSprite(Resonant, CrystalArt.Resonant(), feet);
            Phase02Assets.WriteSprite(Vein, CrystalArt.Vein(), feet);
            Phase02Assets.WriteSprite(FragileTile, CrystalArt.FragileTile(), Vector2.zero, true);
            Phase02Assets.WriteSprite(Glyph, CrystalArt.Glyph(), middle);

            for (int scene = 0; scene < CrystalArt.EngravingCount; scene++)
            {
                Phase02Assets.WriteSprite(EngravingName(scene), CrystalArt.Engraving(scene), middle);
            }

            Phase02Assets.WriteSprite(ShardShot, CrystalArt.ShardShot(), middle);
            Phase02Assets.WriteSprite(GeodeDrill, CrystalArt.GeodeDrill(), feet);
            Phase02Assets.WriteSprite(Sapphire, CrystalArt.Sapphire(), middle);
            Phase02Assets.WriteSprite(Barrier, CrystalArt.Barrier(), Vector2.zero, true);

            (int _, int _, int rubbleWidth, int rubbleHeight) = Phase03Layout.ElevatorRubble;
            Phase02Assets.WriteSprite(Rubble, RubbleArt.ElevatorRubble(rubbleWidth, rubbleHeight), Vector2.zero);
            for (int seed = 0; seed < 2; seed++) Phase02Assets.WriteSprite(PebblesName(seed), RubbleArt.Pebbles(seed), feet);

            for (int kind = 0; kind < StatueArt.KindCount; kind++)
            {
                Phase02Assets.WriteSprite(StatueName(kind), StatueArt.Statue(kind), feet);
                Phase02Assets.WriteSprite(StatueGlowName(kind), StatueArt.StatueGlow(kind), feet);
            }

            Phase02Assets.WriteSprite(ButtonUp, StatueArt.Button(false), Vector2.zero);
            Phase02Assets.WriteSprite(ButtonDown, StatueArt.Button(true), Vector2.zero);
            for (int frame = 0; frame < 3; frame++) Phase02Assets.WriteSprite(CrackName(frame), StatueArt.Crack(frame), feet);
            Phase02Assets.WriteSprite(SilverPlaque, StatueArt.SilverPlaque(), middle);

            // Os bichos e o Capataz, no detalhe das outras fases: uma folha
            // cada, 32 px por tile, com todas as poses e a morte.
            Phase02Assets.WriteSheet(MiteSheet, MiteArt.Frames(), new Vector2(0.5f, MiteArt.PivotY), MiteArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(HunterSheet, HunterArt.Frames(), new Vector2(HunterArt.PivotX, HunterArt.PivotY), HunterArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(SentinelSheet, SentinelArt.Frames(), new Vector2(0.5f, SentinelArt.PivotY), SentinelArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(ForemanSheet, CaveMinerArt.Frames(),
                new Vector2(CaveMinerArt.PivotX, CaveMinerArt.PivotY), CaveMinerArt.PixelsPerUnit);
            Phase02Assets.WriteSprite(AncientKey, CaveArt.AncientKey(), middle);
            Phase02Assets.WriteSprite(KeyLock, CaveArt.Lock(), middle);

            // A broca animada, o ressonante com base, o portao de caverna, as
            // placas, o bilhete, o capacete e a mina.
            for (int frame = 0; frame < GeodeArt.DrillFrames; frame++)
            {
                Phase02Assets.WriteSprite(FrameName("drill", frame), GeodeArt.Drill(frame), feet);
            }

            Phase02Assets.WriteSprite(ResonantPrism, GeodeArt.Resonant(), feet);
            Phase02Assets.WriteSprite(ResonantBase, GeodeArt.ResonantBase(), feet);
            Phase02Assets.WriteSprite(CaveGate, GeodeArt.CaveGate(), Vector2.zero, true);
            Phase02Assets.WriteSprite("roster_board", GeodeArt.RosterBoard(), feet);
            Phase02Assets.WriteSprite("crown_sign", GeodeArt.CrownSign(), feet);
            Phase02Assets.WriteSprite("miner_note", GeodeArt.MinerNote(), middle);
            Phase02Assets.WriteSprite("miner_helmet", GeodeArt.Helmet(), feet);
            Phase02Assets.WriteSprite("mine_post", GeodeArt.MinePost(), Vector2.zero, true);
            Phase02Assets.WriteSprite("mine_beam", GeodeArt.MineBeam(), Vector2.zero, true);
            Phase02Assets.WriteSprite("mine_rail", GeodeArt.MineRail(), Vector2.zero, true);
            Phase02Assets.WriteSprite("mine_cart", GeodeArt.MineCart(), Vector2.zero);

            // O que se le: o desenho do mapa e o grande, da leitura (LoreArt).
            LoreArtAssets.Write(LoreArt.Phase03.Values);

            // O templo do Salao das Estatuas: todos emendaveis (modo Tiled).
            Phase02Assets.WriteSprite("temple_bricks", StatueArt.BrickWall(), Vector2.zero, true);
            Phase02Assets.WriteSprite("temple_floor", StatueArt.TempleFloor(), Vector2.zero, true);
            Phase02Assets.WriteSprite("temple_lintel", StatueArt.TempleLintel(), Vector2.zero, true);
            Phase02Assets.WriteSprite("temple_pillar", StatueArt.Pillar(), Vector2.zero, true);
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
            ground.color = RockTint;

            Tilemap decoration = Phase01Generator.CreateTilemap(root, "TM_Decoration", terrain, 20);
            decoration.color = SpikeTint;

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

            int width = Phase03Layout.Width;
            int height = Phase03Layout.Height;

            var backBuffer = new TileBase[width * height];
            var groundBuffer = new TileBase[width * height];
            var decorationBuffer = new TileBase[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + y * width;

                    backBuffer[index] = Phase01Generator.Resolve(tiles, Phase03Layout.BackgroundTileName(x, y));
                    groundBuffer[index] = Phase01Generator.Resolve(tiles, Phase03Layout.GroundTileName(x, y));

                    if (Phase03Layout.IsSpike(x, y))
                    {
                        decorationBuffer[index] = Phase01Generator.Resolve(tiles, Phase03Layout.SpikeTileName(x, y));
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
        // Luz
        // ------------------------------------------------------------------

        private static void BuildLighting(Material fogMaterial)
        {
            var root = new GameObject("Lighting");

            var globalObject = new GameObject("GlobalLight2D");
            globalObject.transform.SetParent(root.transform, false);

            // Quase nada. O distrito e escuro de proposito: o que se ve e o que
            // os cristais acendem. Subir a intensidade daqui e o jeito mais
            // rapido de clarear tudo se ficar dificil demais de enxergar.
            var global = globalObject.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.color = new Color(0.50f, 0.45f, 0.80f);
            global.intensity = 0.08f;

            // Os lampioes das salas seguras: as unicas luzes paradas.
            for (int i = 0; i < Phase03Layout.Lamps.Count; i++)
            {
                (float x, float y, float radius) = Phase03Layout.Lamps[i];
                Phase01Generator.CreatePointLight(root.transform, $"Lamp_{i + 1}", (x, y), LampGlow, 0.85f, radius);
            }

            // O brilho no fundo da Fenda: verde, lento, enorme, e nao depende de som.
            (float X, float Y) heart = Phase03Layout.HeartGlow;

            Light2D deep = Phase01Generator.CreatePointLight(root.transform, "HeartGlow_Deep",
                (heart.X, heart.Y), OxideGreen, 1.6f, 40f);
            deep.gameObject.AddComponent<GlowPulse>().Configure(deep, null, 4.5f, 0f);

            // Ele sobe pela Fenda ate a beirada do mirante, cada vez mais fraco.
            Light2D rising = Phase01Generator.CreatePointLight(root.transform, "HeartGlow_Rising",
                (heart.X, 24f), OxideGreen, 0.55f, 34f);
            rising.gameObject.AddComponent<GlowPulse>().Configure(rising, null, 4.5f, 0.15f);

            // E o mesmo verde, bem mais perto, no fundo da arena da broca.
            for (int i = 0; i < Phase03Layout.OxideGlows.Count; i++)
            {
                (float x, float y, float radius) = Phase03Layout.OxideGlows[i];

                Light2D oxide = Phase01Generator.CreatePointLight(root.transform, $"OxideGlow_{i + 1}",
                    (x, y), OxideGreen, 0.55f, radius);
                oxide.gameObject.AddComponent<GlowPulse>().Configure(oxide, null, 4.5f, 0.1f * i);

                OxideLights.Add(oxide);
            }

            var atmosphere = new GameObject("Atmosphere");
            SceneKit.CreateHaze(atmosphere.transform, "Oxide_Rift", heart.X, 14f, 76f, 20f,
                OxideGreen, 0.10f, 5f, false, fogMaterial);
        }

        // ------------------------------------------------------------------
        // Marcadores
        // ------------------------------------------------------------------

        /// <summary>Objetos vazios marcando os lugares do layout, agrupados por sala.</summary>
        private static void BuildMarkers()
        {
            var root = new GameObject("Markers");
            var groups = new Dictionary<string, Transform>();

            foreach (Phase03Layout.Marker marker in Phase03Layout.Markers)
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
        // Espinhos e o vao do poco
        // ------------------------------------------------------------------

        private static void BuildSpikes(Transform parent)
        {
            for (int y = 0; y < Phase03Layout.Height; y++)
            {
                int start = -1;

                for (int x = 0; x <= Phase03Layout.Width; x++)
                {
                    bool spike = x < Phase03Layout.Width && Phase03Layout.IsSpike(x, y);

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

            // Quem cai na Fenda nao precisa cair ate os espinhos do fundo: uns
            // tiles abaixo da beirada do mirante ja contam como queda.
            const int fallY = 26;
            int left = Phase03Layout.RiftLeft(fallY);
            int right = Phase03Layout.RiftRight(fallY);

            var abyss = new GameObject("Abyss_Rift");
            abyss.transform.SetParent(parent, false);
            abyss.transform.position = new Vector3((left + right + 1) * 0.5f, fallY, 0f);

            var fall = abyss.AddComponent<BoxCollider2D>();
            fall.isTrigger = true;
            fall.size = new Vector2(right - left + 1, 2f);

            abyss.AddComponent<Hazard>();
        }

        // ------------------------------------------------------------------
        // Cristais de eco
        // ------------------------------------------------------------------

        private static void BuildEchoCrystals(Transform parent)
        {
            for (int i = 0; i < Phase03Layout.Echoes.Count; i++)
            {
                Phase03Layout.EchoSpot spot = Phase03Layout.Echoes[i];

                int slab = spot.Hanging ? -1 : Phase03Layout.SlabAt(spot.X, spot.Y - 1);

                var crystalObject = new GameObject($"EchoCrystal_{i + 1}");
                crystalObject.transform.SetParent(slab >= 0 ? SlabRoots[slab] : parent, false);

                // O pendurado nasce do teto: o sprite vira de ponta-cabeca em
                // volta da base, que fica na borda de cima da celula.
                crystalObject.transform.position = new Vector3(spot.X + 0.5f, spot.Hanging ? spot.Y + 1f : spot.Y, 0f);

                SpriteRenderer sprite = Sprite(crystalObject, ClusterName(spot.Size, spot.Hue),
                    Phase01Assets.SortingTerrain, 12);
                sprite.flipY = spot.Hanging;
                sprite.flipX = LevelMap.Pick(spot.X, spot.Y, 2) == 0;

                float lift = spot.Hanging ? -0.6f : 0.6f;

                Light2D glow = Phase01Generator.CreatePointLight(crystalObject.transform, "Glow",
                    (spot.X + 0.5f, crystalObject.transform.position.y + lift),
                    HueGlow[spot.Hue], 0.04f, 4.5f + spot.Size);

                crystalObject.AddComponent<EchoCrystal>().Configure(glow, sprite, 1.0f + spot.Size * 0.15f);
            }
        }

        // ------------------------------------------------------------------
        // Plataformas frageis
        // ------------------------------------------------------------------

        private static void BuildFragiles(Transform parent)
        {
            foreach ((int x0, int x1, int y) in Phase03Layout.Fragiles)
            {
                int length = x1 - x0 + 1;

                // A lamina ocupa a metade de cima da celula.
                var plateObject = new GameObject($"Fragile_{x0}_{y}") { layer = _groundLayer };
                plateObject.transform.SetParent(parent, false);
                plateObject.transform.position = new Vector3(x0, y + 0.5f, 0f);

                SpriteRenderer plate = Sprite(plateObject, FragileTile, Phase01Assets.SortingTerrain, 11);
                plate.drawMode = SpriteDrawMode.Tiled;
                plate.size = new Vector2(length, 0.5f);

                var collider = plateObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(length, 0.5f);
                collider.offset = new Vector2(length * 0.5f, 0.25f);

                plateObject.AddComponent<FragileCrystal>().Configure(plate);

                // Um brilho bem fraco e fixo: sem ele a ponte nao existe no escuro.
                Phase01Generator.CreatePointLight(plateObject.transform, "Glow",
                    (x0 + length * 0.5f, y + 1f), HueGlow[1], 0.22f, 2.2f);
            }
        }

        // ------------------------------------------------------------------
        // Cristais ressonantes e a Porta Harmonica
        // ------------------------------------------------------------------

        /// <summary>
        /// Monta todos os ressonantes. Os que tem nota ficam guardados pra a
        /// Porta Harmonica; os da arena, pro Geode Drill.
        /// </summary>
        private static void BuildResonants(Transform parent)
        {
            foreach (Phase03Layout.ResonantSpot spot in Phase03Layout.Resonants)
            {
                Color color = NoteColors[Mathf.Clamp(spot.Note, 0, NoteColors.Length - 1)];

                var crystalObject = new GameObject(spot.Name);
                crystalObject.transform.SetParent(spot.Slab >= 0 ? SlabRoots[spot.Slab] : parent, false);
                crystalObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

                SpriteRenderer sprite = Sprite(crystalObject, ResonantPrism, Phase01Assets.SortingTerrain, 13);

                // A base de pedra: fica fora da cor da nota.
                var baseObject = new GameObject("Base");
                baseObject.transform.SetParent(crystalObject.transform, false);
                Sprite(baseObject, ResonantBase, Phase01Assets.SortingTerrain, 14);

                var veinObject = new GameObject("OxideVein");
                veinObject.transform.SetParent(crystalObject.transform, false);

                // O veio aparece no escuro: e Oxide, tem luz propria.
                SpriteRenderer vein = Sprite(veinObject, Vein, Phase01Assets.SortingTerrain, 14);
                if (_unlit != null) vein.sharedMaterial = _unlit;

                // A area serve pros dois: e onde o cinzel acerta e onde se raspa.
                var area = crystalObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(3.2f, 2.4f);
                area.offset = new Vector2(0f, 1.2f);

                Light2D glow = Phase01Generator.CreatePointLight(crystalObject.transform, "Glow",
                    (spot.X + 0.5f, spot.Y + 1.2f), color, 0.04f, 8f);

                // O sprite fica com o ressonante, que o pinta da cor da nota.
                var echo = crystalObject.AddComponent<EchoCrystal>();
                echo.Configure(glow, null, 1.6f);

                // Um resto de brilho na cor dele, pra dar pra achar no escuro.
                Light2D ember = Phase01Generator.CreatePointLight(crystalObject.transform, "Ember",
                    (spot.X + 0.5f, spot.Y + 1.2f), spot.Corrupted ? OxideGreen : color, 0.30f, 2.4f);

                // O verde dos corrompidos apaga com a purificacao.
                if (spot.Corrupted) OxideLights.Add(ember);

                var resonant = crystalObject.AddComponent<ResonantCrystal>();
                resonant.Configure(spot.Note, color, spot.Corrupted, sprite, vein, echo);

                (int X0, int X1) arena = Phase03Layout.BossRange;

                if (spot.Note > 0 && !string.IsNullOrEmpty(spot.Door))
                {
                    if (!DoorCrystals.TryGetValue(spot.Door, out List<ResonantCrystal> group))
                    {
                        group = new List<ResonantCrystal>();
                        DoorCrystals[spot.Door] = group;
                    }

                    group.Add(resonant);
                }
                else if (spot.X >= arena.X0 && spot.X <= arena.X1 && spot.Y <= Phase03Layout.BossFloors[0])
                {
                    // Os pilares da broca ficam acesos um tempo depois do golpe:
                    // ela so crava se ainda estiver aceso quando chegar.
                    resonant.ConfigureLitTime(DrillPillarLitTime);
                    Pillars.Add(resonant);
                }
            }
        }

        // ------------------------------------------------------------------
        // Os chaos que desabam e as barreiras de cristal
        // ------------------------------------------------------------------

        private static void BuildSlabs(Transform parent)
        {
            foreach ((string name, int x0, int x1, int y0, int y1) in Phase03Layout.Slabs)
            {
                int width = x1 - x0 + 1;
                int height = y1 - y0 + 1;

                var slabObject = new GameObject(name) { layer = _groundLayer };
                slabObject.transform.SetParent(parent, false);
                slabObject.transform.position = new Vector3(x0, y0, 0f);

                SpriteRenderer plate = Sprite(slabObject, Phase02Assets.CrackedWall, Phase01Assets.SortingTerrain, 11);
                plate.drawMode = SpriteDrawMode.Tiled;
                plate.size = new Vector2(width, height);
                plate.color = new Color(0.62f, 0.56f, 0.86f);

                var collider = slabObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(width, height);
                collider.offset = new Vector2(width * 0.5f, height * 0.5f);

                SlabRoots.Add(slabObject.transform);
            }
        }

        private static void BuildBarriers(Transform parent)
        {
            foreach ((string name, int x, int y, int height) in Phase03Layout.Barriers)
            {
                var barrierObject = new GameObject(name) { layer = _groundLayer };
                barrierObject.transform.SetParent(parent, false);
                barrierObject.transform.position = new Vector3(x, y, 0f);

                SpriteRenderer wall = Sprite(barrierObject, Barrier, Phase01Assets.SortingTerrain, 15);
                wall.drawMode = SpriteDrawMode.Tiled;
                wall.size = new Vector2(1f, height);

                var collider = barrierObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1f, height);
                collider.offset = new Vector2(0.5f, height * 0.5f);

                barrierObject.AddComponent<CrystalBarrier>().Configure("D3_" + name);

                // Um brilho frio e fixo: e pra ser vista de longe, e lembrada.
                Phase01Generator.CreatePointLight(barrierObject.transform, "Glow",
                    (x + 0.5f, y + height * 0.5f), HueGlow[1], 0.35f, 3.5f);
            }

        }

        /// <summary>
        /// O fio da fase: a cabine que veio do District 2 e os escombros que
        /// entopem o poco logo acima dela. Os escombros sao uma barreira como
        /// as de cristal (so o Resonance Strike os quebra); enquanto existem,
        /// a cabine so mostra o aviso. Sem eles, ela sobe ate o District 4.
        /// </summary>
        private static void BuildElevator(Transform parent)
        {
            (int rx, int ry, int rw, int rh) = Phase03Layout.ElevatorRubble;

            var rubbleObject = new GameObject("ElevatorRubble") { layer = _groundLayer };
            rubbleObject.transform.SetParent(parent, false);
            rubbleObject.transform.position = new Vector3(rx, ry, 0f);
            Sprite(rubbleObject, Rubble, Phase01Assets.SortingTerrain, 15);

            // Embaixo da viga so ha os cabos pendurados: a parede comeca nela.
            const float underBeam = 0.6f;
            var rubbleBody = rubbleObject.AddComponent<BoxCollider2D>();
            rubbleBody.size = new Vector2(rw, rh - underBeam);
            rubbleBody.offset = new Vector2(rw * 0.5f, underBeam + (rh - underBeam) * 0.5f);
            rubbleObject.AddComponent<CrystalBarrier>().Configure("D3_ElevatorRubble");

            // O que escorregou da pilha, no chao da estacao.
            for (int i = 0; i < Phase03Layout.Pebbles.Count; i++)
            {
                (float px, float py) = Phase03Layout.Pebbles[i];

                var pebbles = new GameObject($"Pebbles_{i + 1}");
                pebbles.transform.SetParent(parent, false);
                pebbles.transform.position = new Vector3(px, py, 0f);
                Sprite(pebbles, PebblesName(i % 2), Phase01Assets.SortingTerrain, 16).flipX = i % 3 == 1;
            }

            // Uma luz fraca e fria em cima dos escombros: e a primeira coisa que se ve.
            Phase01Generator.CreatePointLight(rubbleObject.transform, "Glow",
                (rx + rw * 0.5f, ry + 1.5f), HueGlow[1], 0.3f, 4f);

            (int carX, int carY, int carWidth) = Phase03Layout.ElevatorCar;
            float plate = 4f / Phase01Assets.PixelsPerUnit;

            var car = new GameObject("ElevatorCar");
            car.transform.SetParent(parent, false);
            car.transform.position = new Vector3(carX, carY - plate, 0f);
            Sprite(car, Phase02Assets.ElevatorCage, Phase01Assets.SortingActors, -4);

            var trigger = car.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(carWidth, 2f);
            trigger.offset = new Vector2(carWidth * 0.5f, plate + 1f);

            var ride = car.AddComponent<ElevatorRide>();
            ride.Configure(Phase03Layout.ElevatorTopY, Phase04Layout.SceneName, Phase04Layout.FromDistrict3SpawnId);
            ride.ConfigureBlocker(rubbleObject, Phase03Layout.ElevatorBlockedNotice);

            // O poco atravessa o mapa inteiro: sobe depressa.
            ride.ConfigureSpeed(14f);
        }

        private static Gate BuildGate(Transform parent, Phase03Layout.GateSpec spec, Color tint)
        {
            var gateObject = new GameObject(spec.Name) { layer = _groundLayer };
            gateObject.transform.SetParent(parent, false);
            gateObject.transform.position = new Vector3(spec.X, spec.Y, 0f);

            SpriteRenderer bars = Sprite(gateObject, CaveGate, Phase01Assets.SortingTerrain, 15);
            bars.drawMode = SpriteDrawMode.Tiled;
            bars.size = new Vector2(spec.Width, spec.Height);
            bars.color = Color.Lerp(Color.white, tint, 0.25f);

            var wall = gateObject.AddComponent<BoxCollider2D>();
            wall.size = new Vector2(spec.Width, spec.Height);
            wall.offset = new Vector2(spec.Width * 0.5f, spec.Height * 0.5f);

            // Corre o proprio tamanho e mais um tile, pra sumir dentro da rocha.
            float travel = (spec.MoveX != 0 ? spec.Width : spec.Height) + 1f;

            var gate = gateObject.AddComponent<Gate>();
            gate.Configure(travel, new Vector2(spec.MoveX, spec.MoveY));
            return gate;
        }

        /// <summary>
        /// Os quebra-cabecas de notas: cada porta escuta os cristais dela, toca a
        /// sequencia quando o Scrap entra na area de previa e abre o seu portao.
        /// As gemas da sequencia ficam gravadas na parede, com luz propria: sao
        /// a dica pra quem perdeu a previa.
        /// </summary>
        private static void BuildDoors(Transform parent)
        {
            Dictionary<string, Gate> gates = BuiltGates;
            foreach (Phase03Layout.GateSpec spec in Phase03Layout.Gates)
            {
                gates[spec.Name] = BuildGate(parent, spec, new Color(0.80f, 0.70f, 1f));
            }

            foreach (Phase03Layout.DoorSpec spec in Phase03Layout.Doors)
            {
                (int X, int Y, int Width, int Height) preview = spec.Preview;

                var doorObject = new GameObject(spec.Name);
                doorObject.transform.SetParent(parent, false);
                doorObject.transform.position = new Vector3(preview.X + preview.Width * 0.5f, preview.Y + preview.Height * 0.5f, 0f);

                var trigger = doorObject.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(preview.Width, preview.Height);

                int[] sequence = spec.Sequence;

                // A porta oculta nao tem gemas: o jogador descobre sozinho.
                var glyphs = new SpriteRenderer[spec.Hidden ? 0 : sequence.Length];

                for (int i = 0; i < glyphs.Length; i++)
                {
                    var glyphObject = new GameObject($"Glyph_{i + 1}");
                    glyphObject.transform.SetParent(doorObject.transform, false);
                    glyphObject.transform.position = new Vector3(spec.Glyphs.X + i * 1.25f, spec.Glyphs.Y, 0f);

                    glyphs[i] = Sprite(glyphObject, Glyph, Phase01Assets.SortingTerrain, 6);
                    glyphs[i].color = NoteColors[Mathf.Clamp(sequence[i], 0, NoteColors.Length - 1)];
                    if (_unlit != null) glyphs[i].sharedMaterial = _unlit;
                }

                DoorCrystals.TryGetValue(spec.Name, out List<ResonantCrystal> crystals);
                gates.TryGetValue(spec.Gate, out Gate gate);

                doorObject.AddComponent<HarmonicDoor>().Configure(
                    crystals != null ? crystals.ToArray() : new ResonantCrystal[0], sequence, gate, glyphs, spec.Hidden);
            }
        }

        // ------------------------------------------------------------------
        // Bolsas e enfeites
        // ------------------------------------------------------------------

        /// <summary>Os sacos de fragmentos escondidos.</summary>
        private static void BuildRewards(Transform parent)
        {
            foreach (Phase03Layout.RewardSpot spot in Phase03Layout.Rewards)
            {
                var pickup = new GameObject(spot.Id);
                pickup.transform.SetParent(parent, false);
                pickup.transform.position = new Vector3(spot.X + 0.5f, spot.Y + 0.8f, 0f);

                var trigger = pickup.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = 0.7f;

                Sprite(pickup, Phase02Assets.ShardCache, Phase01Assets.SortingActors, -1);
                pickup.AddComponent<RewardPickup>().Configure(RewardPickup.Kind.ShardCache, spot.Id, spot.Amount);

                Phase01Generator.CreatePointLight(pickup.transform, "Glow",
                    (spot.X + 0.5f, spot.Y + 0.8f), new Color(0.95f, 0.85f, 0.6f), 0.45f, 3f);
            }
        }

        /// <summary>Os enfeites: a cabine do elevador da chegada, tambores, placas, correntes.</summary>
        private static void BuildDecor(Transform parent)
        {
            int index = 0;

            foreach (Phase03Layout.DecorSpot spot in Phase03Layout.Decor)
            {
                var decorObject = new GameObject($"{spot.Sprite}_{index++}");
                decorObject.transform.SetParent(parent, false);
                decorObject.transform.position = new Vector3(spot.X, spot.Y, 0f);

                SpriteRenderer renderer = Sprite(decorObject, spot.Sprite, Phase01Assets.SortingTerrain, spot.Order);

                if (spot.Tiled)
                {
                    renderer.drawMode = SpriteDrawMode.Tiled;
                    renderer.size = new Vector2(spot.Width, spot.Height);
                }
            }
        }

        // ------------------------------------------------------------------
        // Inimigos
        // ------------------------------------------------------------------

        private static void BuildFoes(Transform parent)
        {
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);
            Sprite[] mite = Phase02Assets.LoadSheet(MiteSheet, MiteArt.FrameCount);
            Sprite[] hunter = Phase02Assets.LoadSheet(HunterSheet, HunterArt.FrameCount);
            Sprite[] sentinel = Phase02Assets.LoadSheet(SentinelSheet, SentinelArt.FrameCount);

            foreach (Phase03Layout.FoeSpot spot in Phase03Layout.Foes)
            {
                switch (spot.Kind)
                {
                    case Phase03Layout.FoeKind.EchoHunter:
                    {
                        GameObject foe = Foe(parent, spot.Name, new Vector3(spot.HomeX + 0.5f, spot.Y, 0f), hunter, EchoHunter.Idle,
                            new Vector2(0.9f, 1.3f), new Vector2(0f, 0.7f));
                        foe.AddComponent<Damageable>().Configure(4, 5, shard);
                        foe.AddComponent<EchoHunter>().Configure(spot.X0 + 0.5f, spot.X1 + 0.5f, hunter);
                        break;
                    }

                    case Phase03Layout.FoeKind.CrystalMite:
                        BuildMite(parent, spot.Name, new Vector3(spot.HomeX + 0.5f, spot.Y, 0f), spot.X0 + 0.5f, spot.X1 + 0.5f, mite, shard);
                        break;

                    case Phase03Layout.FoeKind.ShardSentinel:
                    {
                        GameObject foe = Foe(parent, spot.Name, new Vector3(spot.HomeX + 0.5f, spot.Y, 0f), sentinel, ShardSentinel.Idle,
                            new Vector2(1.1f, 1.3f), new Vector2(0f, 0.7f));
                        foe.AddComponent<Damageable>().Configure(3, 4, shard);
                        foe.AddComponent<ShardSentinel>().Configure(Phase02Assets.LoadSprite(ShardShot), sentinel, SentinelArt.CoreY);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// O corpo de um bicho: o primeiro quadro da folha, o gatilho (o Scrap
        /// atravessa e apanha) e o Rigidbody parado que o gatilho precisa.
        /// </summary>
        private static GameObject Foe(Transform parent, string name, Vector3 position, Sprite[] frames, int firstFrame,
            Vector2 size, Vector2 offset)
        {
            var foe = new GameObject(name);
            foe.transform.SetParent(parent, false);
            foe.transform.position = position;

            var body = foe.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var trigger = foe.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = size;
            trigger.offset = offset;

            SpriteRenderer renderer = foe.AddComponent<SpriteRenderer>();
            renderer.sprite = frames.Length > firstFrame ? frames[firstFrame] : null;
            renderer.sortingLayerName = Phase01Assets.SortingActors;
            renderer.sortingOrder = -3;
            return foe;
        }

        /// <summary>Um Crystal Mite: um golpe so, e nao solta fragmento nenhum.</summary>
        private static GameObject BuildMite(Transform parent, string name, Vector3 position, float minX, float maxX,
            Sprite[] frames, Sprite shard)
        {
            GameObject mite = Foe(parent, name, position, frames, CrystalMite.Curl, new Vector2(0.9f, 0.55f), new Vector2(0f, 0.3f));
            mite.AddComponent<Damageable>().Configure(MiteHealth, 0, shard);
            mite.AddComponent<CrystalMite>().Configure(minX, maxX, frames);
            return mite;
        }

        // ------------------------------------------------------------------
        // O Salao das Estatuas
        // ------------------------------------------------------------------

        /// <summary>
        /// O enigma principal: as quatro estatuas com seus botoes, as
        /// rachaduras do pe da parede e os mites que saem delas (ja na cena,
        /// desligados, dentro de Enemies pra sumirem com a purificacao).
        /// </summary>
        private static void BuildStatues(Transform parent, Transform enemies)
        {
            if (Phase03Layout.Statues.Count == 0) return;

            Sprite up = Phase02Assets.LoadSprite(ButtonUp);
            Sprite down = Phase02Assets.LoadSprite(ButtonDown);
            float ppu = Phase01Assets.PixelsPerUnit;

            var statues = new List<StatueButton>();

            foreach (Phase03Layout.StatueSpot spot in Phase03Layout.Statues)
            {
                var statueObject = new GameObject(spot.Name);
                statueObject.transform.SetParent(parent, false);
                statueObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

                Sprite(statueObject, StatueName(spot.Kind), Phase01Assets.SortingTerrain, 5);

                var glowObject = new GameObject("Glow");
                glowObject.transform.SetParent(statueObject.transform, false);
                SpriteRenderer glow = Sprite(glowObject, StatueGlowName(spot.Kind), Phase01Assets.SortingTerrain, 7);
                if (_unlit != null) glow.sharedMaterial = _unlit;

                Light2D light = Phase01Generator.CreatePointLight(glowObject.transform, "Light",
                    (spot.X + 0.5f, spot.Y + 2f), StatueGlowColor, 0.7f, 3.5f);

                // O botao, no nicho da frente do pedestal.
                var buttonObject = new GameObject("Button");
                buttonObject.transform.SetParent(statueObject.transform, false);
                buttonObject.transform.localPosition = new Vector3(
                    -StatueArt.Width * 0.5f / ppu + StatueArt.ButtonX / ppu, StatueArt.ButtonY / ppu, 0f);
                SpriteRenderer button = Sprite(buttonObject, ButtonUp, Phase01Assets.SortingTerrain, 6);

                var area = statueObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.4f, 2.5f);
                area.offset = new Vector2(0f, 1.25f);

                var statue = statueObject.AddComponent<StatueButton>();
                statue.Configure(spot.Order, button, up, down, glow, light);
                statues.Add(statue);
            }

            // No Salao das Estatuas nao nasce bicho nenhum: o clangor dos botoes
            // chama quem ja esta por perto, e so.
            var cracks = new List<Transform>();
            var crackArt = new List<SpriteRenderer>();
            var pool = new List<GameObject>();

            // A grade da entrada, trancada: so a CHAVE ANTIGA, do Capataz, abre.
            if (BuiltGates.TryGetValue(Phase03Layout.StatueEntranceGate, out Gate entrance))
            {
                var lockObject = new GameObject("Lock");
                lockObject.transform.SetParent(entrance.transform, false);
                lockObject.transform.localPosition = new Vector3(0.5f, 1.6f, 0f);
                SpriteRenderer lockSprite = Sprite(lockObject, KeyLock, Phase01Assets.SortingTerrain, 16);

                Vector3 at = entrance.transform.position;
                var keyhole = new GameObject("KeyGate_Salao");
                keyhole.transform.SetParent(parent, false);
                keyhole.transform.position = new Vector3(at.x + 2f, at.y + 1.5f, 0f);

                var area = keyhole.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.5f, 3f);

                keyhole.AddComponent<KeyGate>().Configure(Phase03Layout.AncientKeyId, "A CHAVE ANTIGA", entrance, lockSprite);
            }
            else
            {
                Debug.LogWarning($"[Phase 3] O portao '{Phase03Layout.StatueEntranceGate}' do salao nao foi montado.");
            }

            BuiltGates.TryGetValue(Phase03Layout.StatueGate, out Gate gate);
            if (gate == null) Debug.LogWarning($"[Phase 3] O portao '{Phase03Layout.StatueGate}' das estatuas nao foi montado.");

            var frames = new Sprite[3];
            for (int frame = 0; frame < 3; frame++) frames[frame] = Phase02Assets.LoadSprite(CrackName(frame));

            parent.gameObject.AddComponent<StatueHall>().Configure(
                statues.ToArray(), gate, cracks.ToArray(), crackArt.ToArray(), frames, pool.ToArray());
        }

        // ------------------------------------------------------------------
        // Bancadas, Crisol e a historia
        // ------------------------------------------------------------------

        private static void BuildWorkbenches(Transform parent)
        {
            foreach ((string name, int x, int y) in Phase03Layout.Workbenches)
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

        private static GameObject BuildCrisol(Transform parent)
        {
            return BuildCrisolAt(parent, "Npc_Crisol", Phase03Layout.CrisolTile, Phase03Layout.CrisolLines);
        }

        private static GameObject BuildCrisolAt(Transform parent, string name, (int X, int Y) tile, string[] lines)
        {
            var crisolObject = new GameObject(name);
            crisolObject.transform.SetParent(parent, false);
            crisolObject.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);

            Phase02Assets.AddCrisol(crisolObject, -4);

            var area = crisolObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3.2f, 2.6f);
            area.offset = new Vector2(0f, 1.3f);

            var crisol = crisolObject.AddComponent<Crisol>();
            crisol.Configure("Crisol", lines);

            // O rubi foi no District 2. Aqui ele so conversa e vende.
            crisol.ConfigureGivesRuby(false);
            return crisolObject;
        }

        // ------------------------------------------------------------------
        // O Capataz, o Geode Drill e a purificacao
        // ------------------------------------------------------------------

        /// <summary>
        /// O Capataz: ajoelhado no fundo leste da toca, com a folha de quadros,
        /// o corpo (e nele que se bate), as rachaduras do chao de onde o turno
        /// brota quando ele apita e os bichos do turno, ja na cena, desligados.
        /// </summary>
        private static void BuildElite(Transform parent)
        {
            (int X0, int X1) range = Phase03Layout.EliteRange;
            (int X, int Y, int Width, int Height) wake = Phase03Layout.EliteWake;
            (float hx, float hy) = Phase03Layout.ForemanHome;
            (float minX, float maxX) = Phase03Layout.ForemanRange;

            var eliteObject = new GameObject("Foreman");
            eliteObject.transform.SetParent(parent, false);
            eliteObject.transform.position = new Vector3(hx, hy, 0f);

            Sprite[] frames = Phase02Assets.LoadSheet(ForemanSheet, CaveMinerArt.FrameCount);
            SpriteRenderer body = eliteObject.AddComponent<SpriteRenderer>();
            body.sprite = frames.Length > 0 ? frames[CaveMinerArt.Dormant] : null;
            body.sortingLayerName = Phase01Assets.SortingActors;
            body.sortingOrder = -2;
            body.flipX = true;

            var rigidbody = eliteObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;

            // Do tamanho do homem desenhado: do chao ate o capacete.
            var area = eliteObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(1.4f, 2.8f);
            area.offset = new Vector2(0f, 1.4f);

            // A safira no capacete acende a cara dele no escuro, e vai junto.
            Phase01Generator.CreatePointLight(eliteObject.transform, "SapphireGlow",
                (hx, hy + 2.6f), new Color(0.35f, 0.6f, 1f), 0.8f, 4.5f);

            // O turno: rachaduras no chao da toca e quatro mites desligados.
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);
            Sprite[] mite = Phase02Assets.LoadSheet(MiteSheet, MiteArt.FrameCount);
            var spots = new List<Transform>();
            var cracks = new List<SpriteRenderer>();

            foreach (int x in Phase03Layout.MonsterBroods)
            {
                var spot = new GameObject($"Brood_{x}");
                spot.transform.SetParent(parent, false);
                spot.transform.position = new Vector3(x + 0.5f, hy, 0f);

                var art = new GameObject("Art");
                art.transform.SetParent(spot.transform, false);
                cracks.Add(Sprite(art, CrackName(0), Phase01Assets.SortingTerrain, 6));
                spots.Add(spot.transform);
            }

            var brood = new List<GameObject>();
            for (int i = 0; i < 4; i++)
            {
                GameObject one = BuildMite(parent, $"Mite_Brood_{i + 1}", spots[i % spots.Count].position,
                    range.X0 + 0.5f, range.X1 + 0.5f, mite, shard);
                one.SetActive(false);
                brood.Add(one);
            }

            var crackFrames = new Sprite[3];
            for (int frame = 0; frame < 3; frame++) crackFrames[frame] = Phase02Assets.LoadSprite(CrackName(frame));

            var elite = eliteObject.AddComponent<TheResonant>();
            elite.Configure(
                new Rect(wake.X, wake.Y, wake.Width, wake.Height),
                minX, maxX, hy, Phase03Layout.MonsterCeiling,
                Phase02Assets.LoadSprite(ShardShot),
                shard,
                Phase02Assets.LoadSprite(Sapphire));
            elite.ConfigureFrames(frames, CaveMinerArt.SlamReach);
            elite.ConfigureKey(Phase03Layout.AncientKeyId, "CHAVE ANTIGA obtida", Phase02Assets.LoadSprite(AncientKey));
            elite.ConfigureBrood(brood.ToArray(), spots.ToArray(), cracks.ToArray(), crackFrames);

            // A boca da chamine fecha enquanto ele luta: nao se desce no meio.
            (int X, int Y, int Width, int Height) seal = Phase03Layout.EliteSeal;
            Phase01Generator.CreateArenaSeal(parent, _groundLayer, new RectInt(seal.X, seal.Y, seal.Width, seal.Height)).Watch(elite);
        }

        /// <summary>
        /// A purificacao do District 3: quando a broca para, a luz deixa de
        /// depender de som, o verde apaga, os que cacavam somem e os mineiros
        /// montam o salao de concertos na Camara do Ressonante. Os cristais e
        /// os veios de Oxide se resolvem sozinhos: escutam o mesmo aviso.
        /// </summary>
        private static AreaPurifier BuildPurification(Transform parent, GameObject enemies, GameObject crisol)
        {
            GameObject lighting = GameObject.Find("Lighting");
            GameObject globalObject = GameObject.Find("GlobalLight2D");
            GameObject cameraObject = GameObject.Find("Main Camera");
            GameObject groundObject = GameObject.Find("TM_Ground");

            // Quem volta: os mineiros e Crisol, com lampioes.
            Transform hall = Child(parent, "ConcertHall");
            var lamps = new List<Light2D>();

            foreach (Phase03Layout.LoreSpot spot in Phase03Layout.Purified)
            {
                if (spot.Kind == "crisol")
                {
                    BuildCrisolAt(hall, spot.Name, (spot.X, spot.Y), spot.Lines);
                }
                else
                {
                    var minerObject = new GameObject(spot.Name);
                    minerObject.transform.SetParent(hall, false);
                    minerObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

                    // O mesmo corpo do Brass Welder, limpo: e um Tarnished, nao um corrompido.
                    SpriteRenderer body = Sprite(minerObject, Phase02Assets.BrassWelder, Phase01Assets.SortingActors, -4);
                    body.color = new Color(0.85f, 0.92f, 1f);
                    body.flipX = LevelMap.Pick(spot.X, spot.Y, 2) == 0;

                    var area = minerObject.AddComponent<BoxCollider2D>();
                    area.isTrigger = true;
                    area.size = new Vector2(2.6f, 2.5f);
                    area.offset = new Vector2(0f, 1.25f);

                    minerObject.AddComponent<Npc>().Configure(spot.Title, spot.Lines);
                }

                if (lighting != null)
                {
                    lamps.Add(Phase01Generator.CreatePointLight(lighting.transform, "PurifiedLamp_" + lamps.Count,
                        (spot.X + 0.5f, spot.Y + 3f), LampGlow, 0.8f, 8f));
                }
            }

            var purifierObject = new GameObject("AreaPurifier");
            purifierObject.transform.SetParent(parent, false);

            var purifier = purifierObject.AddComponent<AreaPurifier>();
            purifier.Configure(
                globalObject != null ? globalObject.GetComponent<Light2D>() : null,
                OxideLights.ToArray(), lamps.ToArray(), new ParticleSystem[0],
                groundObject != null ? new[] { groundObject.GetComponent<Tilemap>() } : new Tilemap[0],
                cameraObject != null ? cameraObject.GetComponent<Camera>() : null,
                new[] { hall.gameObject }, new[] { enemies, crisol }, new Gate[0]);

            // Nada de dourado aqui: a luz fica fria, branca e estavel.
            purifier.ConfigureLook(new Color(0.84f, 0.82f, 1f), 0.6f,
                new Color(0.94f, 0.90f, 1f), new Color(0.07f, 0.06f, 0.12f));

            return purifier;
        }

        private static void BuildBoss(Transform parent, AreaPurifier purifier)
        {
            (int X, int Y) tile = Phase03Layout.BossTile;
            (int X0, int X1) range = Phase03Layout.BossRange;
            (int X, int Y, int Width, int Height) wake = Phase03Layout.BossWake;

            var bossObject = new GameObject("GeodeDrill");
            bossObject.transform.SetParent(parent, false);
            bossObject.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);

            Sprite(bossObject, FrameName("drill", 0), Phase01Assets.SortingActors, -2);

            // Parada, o motor ainda trepida; correndo, as esteiras e a espiral giram.
            Sprite[] drillFrames = LoadFrames("drill", GeodeArt.DrillFrames);
            bossObject.AddComponent<MotionFrames>().Configure(drillFrames, drillFrames, 3f, 2.5f);

            var body = bossObject.AddComponent<BoxCollider2D>();
            body.isTrigger = true;
            body.size = new Vector2(4.2f, 2.4f);
            body.offset = new Vector2(0f, 1.1f);

            var rigidbody = bossObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;

            // A placa do chassi: so aparece quando ela para.
            var plaqueObject = new GameObject("ChassisPlaque");
            plaqueObject.transform.SetParent(bossObject.transform, false);

            var area = plaqueObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3f, 2.5f);
            area.offset = new Vector2(0f, 1.25f);

            var plaque = plaqueObject.AddComponent<Npc>();
            plaque.ConfigureReadable("a placa do chassi", Phase03Layout.DrillPlaqueLines);
            if (LoreArt.Phase03.TryGetValue(LoreArt.DrillPlaque, out LoreArt.Piece plaqueArt))
            {
                plaque.ConfigureDocument(Phase02Assets.LoadSprite(LoreArt.ViewSprite(plaqueArt.Lore)));
            }

            Phase01Generator.CreatePointLight(plaqueObject.transform, "Glow",
                (tile.X + 0.5f, tile.Y + 1.2f), new Color(0.90f, 0.92f, 1f), 0.6f, 4f);

            var drill = bossObject.AddComponent<GeodeDrill>();
            drill.Configure(
                range.X0 + 0.5f, range.X1 + 0.5f,
                new Rect(wake.X, wake.Y, wake.Width, wake.Height),
                Phase03Layout.BossFloors.Select(floor => (float)floor).ToArray(),
                SlabRoots.Select(slab => slab.gameObject).ToArray(),
                Pillars.ToArray(),
                Phase02Assets.LoadSprite(ShardShot),
                Phase02Assets.LoadSprite(Phase02Assets.Shard),
                Phase02Assets.LoadSprite(Phase02Assets.DashModule),
                purifier,
                plaqueObject);

            // As traças que a pancada no pilar solta: seis, desligadas, prontas pra brotar.
            Sprite debrisShard = Phase02Assets.LoadSprite(Phase02Assets.Shard);
            Sprite[] miteFrames = Phase02Assets.LoadSheet(MiteSheet, MiteArt.FrameCount);
            var brood = new List<GameObject>();
            for (int i = 0; i < 6; i++)
            {
                GameObject mite = BuildMite(parent, $"Mite_Drill_{i + 1}", new Vector3(tile.X + 0.5f, tile.Y, 0f),
                    range.X0 + 0.5f, range.X1 + 0.5f, miteFrames, debrisShard);
                mite.SetActive(false);
                brood.Add(mite);
            }

            drill.ConfigureBrood(brood.ToArray());

            // A entrada da camara fecha enquanto ela luta.
            (int X, int Y, int Width, int Height) seal = Phase03Layout.BossSeal;
            Phase01Generator.CreateArenaSeal(parent, _groundLayer, new RectInt(seal.X, seal.Y, seal.Width, seal.Height)).Watch(drill);
        }

        /// <summary>
        /// O que da pra ler com E: o aviso, o bilhete, a beirada do poco e as
        /// quatro gravuras. As gravuras nao tem luz propria — pra ve-las e
        /// preciso fazer barulho.
        /// </summary>
        private static void BuildLore(Transform parent)
        {
            foreach (Phase03Layout.LoreSpot spot in Phase03Layout.Lore)
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

                bool lit = true;

                if (spot.Kind == "sign")
                {
                    artObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                    Sprite(artObject, Phase02Assets.Plaque, Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "silverplaque")
                {
                    artObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                    Sprite(artObject, SilverPlaque, Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "board")
                {
                    // A tabua do turno, fincada no chao.
                    Sprite(artObject, "roster_board", Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "crownsign")
                {
                    // A placa da Coroa, num poste na beira da Fenda.
                    Sprite(artObject, "crown_sign", Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "note")
                {
                    // Um bilhete preso na rocha por uma lasca de cristal, na altura do peito.
                    artObject.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                    Sprite(artObject, "miner_note", Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "helmet")
                {
                    Sprite(artObject, "miner_helmet", Phase01Assets.SortingTerrain, 22);
                }
                else if (spot.Kind == "page")
                {
                    Sprite(artObject, Phase02Assets.LoosePage, Phase01Assets.SortingTerrain, 22);
                }
                else if (spot.Kind.StartsWith("engraving"))
                {
                    // Na parede do fundo, do tamanho de quatro tiles.
                    artObject.transform.localPosition = new Vector3(0f, 3f, 0f);
                    Sprite(artObject, EngravingName(int.Parse(spot.Kind.Substring("engraving".Length))),
                        Phase01Assets.SortingTerrain, 4);
                    lit = false;
                }
                else
                {
                    Object.DestroyImmediate(artObject);
                    lit = false;
                }

                var readable = loreObject.AddComponent<Npc>();
                readable.ConfigureReadable(spot.Title, spot.Lines);

                // A arte propria (LoreArt): o desenho do mapa e o grande, mostrado na leitura.
                if (LoreArt.Phase03.TryGetValue(spot.Name, out LoreArt.Piece own))
                {
                    if (artObject == null)
                    {
                        artObject = new GameObject("Art");
                        artObject.transform.SetParent(loreObject.transform, false);
                    }

                    SpriteRenderer old = artObject.GetComponent<SpriteRenderer>();
                    if (old != null) Object.DestroyImmediate(old);
                    LoreArtAssets.Apply(own, artObject, readable, Phase01Assets.SortingTerrain);
                }

                if (lit)
                {
                    Phase01Generator.CreatePointLight(loreObject.transform, "Glow",
                        (spot.X + 0.5f, spot.Y + 1.2f), new Color(0.95f, 0.90f, 0.75f), 0.4f, 3f);
                }
            }
        }
    }
}
