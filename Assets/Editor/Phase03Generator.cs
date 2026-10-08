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
        public const string ScenePath = "Assets/Scenes/" + Phase03Layout.SceneName + ".unity";
        private const string GroundLayerName = "Ground";

        // Os nomes dos sprites desta fase, na mesma pasta dos de latao.
        private const string Resonant = "crystal_resonant";
        private const string Vein = "crystal_vein";
        private const string FragileTile = "crystal_fragile";
        private const string Glyph = "crystal_glyph";
        private const string EchoHunter = "echo_hunter";
        private const string Mite = "crystal_mite";
        private const string Sentinel = "shard_sentinel";
        private const string ShardShot = "shard_shot";
        private const string TheResonant = "the_resonant";
        private const string GeodeDrill = "geode_drill";
        private const string Sapphire = "sapphire_raw";
        private const string Barrier = "crystal_barrier";

        private static string ClusterName(int size, int hue) => $"crystal_cluster_{size}_{hue}";
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
        private static readonly List<ResonantCrystal> DoorCrystals = new List<ResonantCrystal>();
        private static readonly List<ResonantCrystal> Pillars = new List<ResonantCrystal>();
        private static readonly List<Light2D> OxideLights = new List<Light2D>();

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

            BuildTilemaps(tiles);
            BuildLighting(fogMaterial);
            BuildMarkers();

            var travel = new GameObject("Travel");
            SceneKit.CreateSpawnPoint(travel.transform, Phase03Layout.FromDistrict2SpawnId, Phase03Layout.SpawnTile);

            // No fundo da saida, depois da barreira de cristal: o District 4. Nao ha volta.
            (int X, int Y, int Width, int Height) exit = Phase03Layout.District4Exit;
            SceneKit.CreateSceneExit(travel.transform, "Exit_District4",
                new RectInt(exit.X, exit.Y, exit.Width, exit.Height),
                Phase04Layout.SceneName, Phase04Layout.FromDistrict3SpawnId);

            Transform scrap = Phase01Generator.BuildScrap(scrapSprite, polishedSprite, _groundLayer, Phase03Layout.SpawnTile);
            Phase01Generator.BuildCamera(scrap, Phase03Layout.Width, Phase03Layout.Height, Void);

            // O mapa desta fase conta com o Steam Dash e o Pressure Jump, entao
            // aqui o Scrap ja chega com os dois — mesmo dando Play direto na cena.
            scrap.GetComponent<ScrapDash>().SetUnlockedFromStart(true);
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

            Transform enemies = Child(root, "Enemies");
            BuildFoes(enemies);
            BuildElite(enemies);

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
                      $"{Phase03Layout.Foes.Count} inimigos, The Resonant e The Geode Drill.");
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

            Phase02Assets.WriteSprite(EchoHunter, CrystalArt.EchoHunter(), feet);
            Phase02Assets.WriteSprite(Mite, CrystalArt.Mite(), feet);
            Phase02Assets.WriteSprite(Sentinel, CrystalArt.Sentinel(), feet);
            Phase02Assets.WriteSprite(ShardShot, CrystalArt.ShardShot(), middle);

            Phase02Assets.WriteSprite(TheResonant, CrystalArt.TheResonant(), feet);
            Phase02Assets.WriteSprite(GeodeDrill, CrystalArt.GeodeDrill(), feet);
            Phase02Assets.WriteSprite(Sapphire, CrystalArt.Sapphire(), middle);
            Phase02Assets.WriteSprite(Barrier, CrystalArt.Barrier(), Vector2.zero, true);
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

            // O brilho no fundo do Poco da Broca: verde, lento, e nao depende de som.
            (float X, float Y) heart = Phase03Layout.HeartGlow;

            Light2D deep = Phase01Generator.CreatePointLight(root.transform, "HeartGlow_Deep",
                (heart.X, heart.Y), OxideGreen, 1.6f, 22f);
            deep.gameObject.AddComponent<GlowPulse>().Configure(deep, null, 4.5f, 0f);

            // Ele sobe pelo poco ate a altura da beirada, cada vez mais fraco.
            Light2D rising = Phase01Generator.CreatePointLight(root.transform, "HeartGlow_Rising",
                (heart.X, 42f), OxideGreen, 0.55f, 20f);
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
            SceneKit.CreateHaze(atmosphere.transform, "Oxide_DrillShaft", heart.X, 46f, 22f, 6f,
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

            // Quem cai no Poco da Broca nao precisa cair os cinquenta tiles:
            // um pouco abaixo da beirada ja conta como queda.
            var abyss = new GameObject("Abyss_DrillShaft");
            abyss.transform.SetParent(parent, false);
            abyss.transform.position = new Vector3(258f, 44f, 0f);

            var fall = abyss.AddComponent<BoxCollider2D>();
            fall.isTrigger = true;
            fall.size = new Vector2(22f, 2f);

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

                SpriteRenderer sprite = Sprite(crystalObject, Resonant, Phase01Assets.SortingTerrain, 13);

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

                if (spot.Note > 0) DoorCrystals.Add(resonant);
                else if (spot.X >= arena.X0 && spot.X <= arena.X1 && spot.Y <= Phase03Layout.BossFloors[0]) Pillars.Add(resonant);
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

            // O que a barreira da Fronteira esconde: um reforco de chassi.
            (int X, int Y) secret = Phase03Layout.SecretChassisTile;

            var pickup = new GameObject("Secret_Chassis");
            pickup.transform.SetParent(parent, false);
            pickup.transform.position = new Vector3(secret.X + 0.5f, secret.Y + 1f, 0f);

            SpriteRenderer module = Sprite(pickup, Phase02Assets.DashModule, Phase01Assets.SortingActors, -1);
            module.color = new Color(1f, 0.85f, 0.45f);

            var trigger = pickup.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            pickup.AddComponent<RewardPickup>().Configure(RewardPickup.Kind.Chassis, "D3_Secret_Chassis");

            Phase01Generator.CreatePointLight(pickup.transform, "Glow",
                (secret.X + 0.5f, secret.Y + 1f), LampGlow, 0.5f, 3f);
        }

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

        private static void BuildDoors(Transform parent)
        {
            Gate door = BuildGate(parent, "Gate_HarmonicDoor", Phase03Layout.HarmonicDoor, new Color(0.80f, 0.70f, 1f));

            (int X, int Y, int Width, int Height) preview = Phase03Layout.DoorPreview;

            var doorObject = new GameObject("HarmonicDoor");
            doorObject.transform.SetParent(parent, false);
            doorObject.transform.position = new Vector3(preview.X + preview.Width * 0.5f, preview.Y + preview.Height * 0.5f, 0f);

            var trigger = doorObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(preview.Width, preview.Height);

            // As gemas da sequencia, gravadas na parede ao lado da porta, na
            // ordem em que as notas precisam soar. Tem luz propria: sao a dica.
            int[] sequence = Phase03Layout.DoorSequence;
            var glyphs = new SpriteRenderer[sequence.Length];

            for (int i = 0; i < sequence.Length; i++)
            {
                var glyphObject = new GameObject($"Glyph_{i + 1}");
                glyphObject.transform.SetParent(doorObject.transform, false);
                glyphObject.transform.position = new Vector3(
                    Phase03Layout.HarmonicDoor.X + 2f + i * 1.25f,
                    Phase03Layout.HarmonicDoor.Y + Phase03Layout.HarmonicDoor.Height + 1.5f, 0f);

                glyphs[i] = Sprite(glyphObject, Glyph, Phase01Assets.SortingTerrain, 6);
                glyphs[i].color = NoteColors[Mathf.Clamp(sequence[i], 0, NoteColors.Length - 1)];
                if (_unlit != null) glyphs[i].sharedMaterial = _unlit;
            }

            doorObject.AddComponent<HarmonicDoor>().Configure(DoorCrystals.ToArray(), sequence, door, glyphs);
        }

        // ------------------------------------------------------------------
        // Inimigos
        // ------------------------------------------------------------------

        private static void BuildFoes(Transform parent)
        {
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);

            foreach (Phase03Layout.FoeSpot spot in Phase03Layout.Foes)
            {
                var foe = new GameObject(spot.Name);
                foe.transform.SetParent(parent, false);
                foe.transform.position = new Vector3(spot.HomeX + 0.5f, spot.Y, 0f);

                var body = foe.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;

                // Gatilho, e nao parede: o Scrap atravessa e apanha.
                var trigger = foe.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;

                switch (spot.Kind)
                {
                    case Phase03Layout.FoeKind.EchoHunter:
                        Sprite(foe, EchoHunter, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(0.9f, 1.3f);
                        trigger.offset = new Vector2(0f, 0.7f);

                        foe.AddComponent<Damageable>().Configure(4, 5, shard);
                        foe.AddComponent<EchoHunter>().Configure(spot.X0 + 0.5f, spot.X1 + 0.5f);
                        break;

                    case Phase03Layout.FoeKind.CrystalMite:
                        Sprite(foe, Mite, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(0.7f, 0.5f);
                        trigger.offset = new Vector2(0f, 0.3f);

                        foe.AddComponent<Damageable>().Configure(1, 1, shard);
                        foe.AddComponent<CrystalMite>().Configure(spot.X0 + 0.5f, spot.X1 + 0.5f);
                        break;

                    case Phase03Layout.FoeKind.ShardSentinel:
                        Sprite(foe, Sentinel, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(0.9f, 1.8f);
                        trigger.offset = new Vector2(0f, 0.9f);

                        foe.AddComponent<Damageable>().Configure(3, 4, shard);
                        foe.AddComponent<ShardSentinel>().Configure(Phase02Assets.LoadSprite(ShardShot));
                        break;
                }
            }
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

            Sprite(crisolObject, Phase02Assets.Crisol, Phase01Assets.SortingActors, -4);

            var area = crisolObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3.2f, 2.6f);
            area.offset = new Vector2(0f, 1.3f);

            crisolObject.AddComponent<Crisol>().Configure("Crisol", lines);
            return crisolObject;
        }

        // ------------------------------------------------------------------
        // The Resonant, o Geode Drill e a purificacao
        // ------------------------------------------------------------------

        private static void BuildElite(Transform parent)
        {
            (int X, int Y) tile = Phase03Layout.EliteTile;
            (int X0, int X1) range = Phase03Layout.EliteRange;
            (int X, int Y, int Width, int Height) wake = Phase03Layout.EliteWake;

            var eliteObject = new GameObject("TheResonant");
            eliteObject.transform.SetParent(parent, false);
            eliteObject.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);

            Sprite(eliteObject, TheResonant, Phase01Assets.SortingActors, -2);

            var body = eliteObject.AddComponent<BoxCollider2D>();
            body.isTrigger = true;
            body.size = new Vector2(1.4f, 2.2f);
            body.offset = new Vector2(0f, 1.1f);

            var rigidbody = eliteObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;

            eliteObject.AddComponent<TheResonant>().Configure(
                range.X0 + 0.5f, range.X1 + 0.5f,
                new Rect(wake.X, wake.Y, wake.Width, wake.Height),
                Phase02Assets.LoadSprite(Phase02Assets.ShockWave),
                Phase02Assets.LoadSprite(Phase02Assets.Shard),
                Phase02Assets.LoadSprite(Sapphire));
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

            Sprite(bossObject, GeodeDrill, Phase01Assets.SortingActors, -2);

            var body = bossObject.AddComponent<BoxCollider2D>();
            body.isTrigger = true;
            body.size = new Vector2(3.4f, 2.1f);
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

            plaqueObject.AddComponent<Npc>().ConfigureReadable("a placa do chassi", Phase03Layout.DrillPlaqueLines);

            Phase01Generator.CreatePointLight(plaqueObject.transform, "Glow",
                (tile.X + 0.5f, tile.Y + 1.2f), new Color(0.90f, 0.92f, 1f), 0.6f, 4f);

            bossObject.AddComponent<GeodeDrill>().Configure(
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

                loreObject.AddComponent<Npc>().ConfigureReadable(spot.Title, spot.Lines);

                if (lit)
                {
                    Phase01Generator.CreatePointLight(loreObject.transform, "Glow",
                        (spot.X + 0.5f, spot.Y + 1.2f), new Color(0.95f, 0.90f, 0.75f), 0.4f, 3f);
                }
            }
        }
    }
}
