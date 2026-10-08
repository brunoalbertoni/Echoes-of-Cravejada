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
    /// Monta a cena da Phase 2 do zero, a partir do desenho em Phase02Layout.
    ///
    /// Rodar de novo refaz a cena inteira e descarta o que voce tiver ajustado
    /// nela na mao — entao ajuste o layout no codigo enquanto estiver iterando.
    ///
    /// Menu:      Echoes > Gerar Phase 2 (Brass Pipelines)
    ///            Echoes > Validar layout da Phase 2
    /// Batchmode: -executeMethod EchoesOfCravejada.EditorTools.Phase02Generator.GenerateFromCommandLine
    /// </summary>
    public static class Phase02Generator
    {
        public const string ScenePath = "Assets/Scenes/" + Phase02Layout.SceneName + ".unity";
        private const string GroundLayerName = "Ground";

        /// <summary>
        /// O que se ve nos saloes abertos, atras do horizonte de caldeiras: o
        /// clarao quente das fornalhas ao longe. E tambem o fundo da camera.
        /// </summary>
        private static readonly Color FurnaceHaze = new Color(0.16f, 0.11f, 0.06f);

        /// <summary>O mesmo tom que a Phase 1 usa pra parede de caverna do fundo.</summary>
        private static readonly Color CaveBackTint = new Color(0.45f, 0.55f, 0.52f);

        private static Color GlowColor(Phase02Layout.Glow glow)
        {
            switch (glow)
            {
                case Phase02Layout.Glow.Furnace: return new Color(1f, 0.55f, 0.20f);
                case Phase02Layout.Glow.Acid: return new Color(0.62f, 1f, 0.30f);
                case Phase02Layout.Glow.Steam: return new Color(0.95f, 0.95f, 0.88f);
                case Phase02Layout.Glow.Lamp: return new Color(1f, 0.82f, 0.55f);
                case Phase02Layout.Glow.Crystal: return new Color(0.50f, 0.85f, 1f);
                default: return new Color(0.36f, 0.95f, 0.60f);
            }
        }

        [MenuItem("Echoes/Gerar Phase 2 (Brass Pipelines)", false, 30)]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[Phase 2] Geracao cancelada.");
                return;
            }

            Build();
        }

        /// <summary>Confere que o mapa e navegavel sem precisar entrar no jogo.</summary>
        [MenuItem("Echoes/Validar layout da Phase 2", false, 31)]
        public static void ValidateLayout()
        {
            List<string> problems = Phase02Layout.Validate();

            if (problems.Count == 0)
            {
                Debug.Log("[Phase 2] Layout OK: da pra ir da Galeria de Transicao ate a saida pros " +
                          "Silent Geodes, e voltar de la ate o poco do District 1.");
            }
            else
            {
                foreach (string problem in problems) Debug.LogWarning($"[Phase 2] {problem}");
                Debug.LogError($"[Phase 2] {problems.Count} problema(s) no layout. Veja os avisos acima.");
            }

            // Nao e erro, mas um trecho grande aqui costuma ser sala isolada.
            foreach (string floor in Phase02Layout.UnreachableFloors())
            {
                Debug.Log($"[Phase 2] Chao sem acesso (pode ser so enfeite): {floor}");
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
            int groundLayer = Phase01Assets.EnsureLayer(GroundLayerName);

            Phase01Assets.EnsureSortingLayers();
            Phase01Assets.VerifySortingLayers();

            foreach (string problem in Phase02Layout.Validate())
            {
                Debug.LogWarning($"[Phase 2] {problem}");
            }

            // A Phase 2 reaproveita rocha, sucata e cristal da Phase 1.
            if (!Phase01Assets.SliceTileset() || !Phase01Assets.SliceBackgroundTileset() || !Phase01Assets.SliceObjects())
            {
                Debug.LogError("[Phase 2] Abortado: nao consegui fatiar as folhas do pack de caverna.");
                return;
            }

            IEnumerable<int> nozzleWidths = Phase02Layout.Fixtures
                .Where(fixture => fixture.Kind == Phase02Layout.FixtureKind.NozzleUp
                    || fixture.Kind == Phase02Layout.FixtureKind.NozzleSide)
                .Select(fixture => fixture.Size);

            if (!Phase02Assets.EnsureArt(nozzleWidths) || !Phase02Assets.EnsureMechanicsArt())
            {
                Debug.LogError("[Phase 2] Abortado: nao consegui preparar a arte de latao.");
                return;
            }

            Dictionary<string, TileBase> tiles = Phase02Assets.BuildTiles();
            if (tiles.Count == 0)
            {
                Debug.LogError("[Phase 2] Abortado: nenhum tile foi criado.");
                return;
            }

            Sprite scrapSprite = Phase01Assets.CreateScrapPlaceholder();
            Sprite polishedSprite = Phase02Assets.LoadSprite(Phase02Assets.ScrapPolished);
            Material fogMaterial = Phase01Assets.GetOrCreateFogMaterial();
            Material unlitMaterial = Phase01Assets.GetOrCreateUnlitMaterial();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildTilemaps(tiles, groundLayer, unlitMaterial);
            BuildSkylines(unlitMaterial);
            BuildFixtures(unlitMaterial);
            BuildLighting();
            BuildAtmosphere(fogMaterial);
            BuildMarkers();
            BuildTravel(fogMaterial);

            Transform scrap = Phase01Generator.BuildScrap(scrapSprite, polishedSprite, groundLayer, Phase02Layout.SpawnTile);
            Phase01Generator.BuildCamera(scrap, Phase02Layout.Width, Phase02Layout.Height, FurnaceHaze);

            // O mapa desta fase conta com o Steam Dash, entao aqui o Scrap ja
            // chega com ele — mesmo dando Play direto na cena.
            scrap.GetComponent<ScrapDash>().SetUnlockedFromStart(true);

            // Por ultimo: a purificacao procura pelo nome as luzes, a nevoa e a camera.
            Phase02Mechanics.Build(groundLayer, fogMaterial, unlitMaterial);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneKit.AddSceneToBuildSettings(ScenePath);

            Debug.Log($"[Phase 2] Cena gerada: {ScenePath}. " +
                      $"Mapa {Phase02Layout.Width}x{Phase02Layout.Height}, {Phase02Layout.Rooms.Count} salas, " +
                      $"{Phase02Layout.Vents.Count} saidas de vapor, {Phase02Layout.AcidPools.Count} pocas de acido, " +
                      $"{Phase02Layout.Valves.Count} valvulas, {Phase02Layout.Enemies.Count} inimigos.");
        }

        // ------------------------------------------------------------------
        // Terreno
        // ------------------------------------------------------------------

        private static void BuildTilemaps(Dictionary<string, TileBase> tiles, int groundLayer, Material unlitMaterial)
        {
            var gridObject = new GameObject("Grid");
            var grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Transform root = gridObject.transform;
            string terrain = Phase01Assets.SortingTerrain;

            // A parede do fundo vem em dois tilemaps porque a de caverna (na
            // entrada e na saida) precisa do mesmo escurecimento da Phase 1, e
            // a de latao ja foi desenhada escura.
            Tilemap backCave = Phase01Generator.CreateTilemap(root, "TM_BackgroundCave", terrain, 0);
            backCave.color = CaveBackTint;

            Tilemap back = Phase01Generator.CreateTilemap(root, "TM_Background", terrain, 0);

            // A tubulacao colossal la do fundo usa os mesmos tiles de cano, com
            // o tilemap inteiro em dobro: cada celula dele vale 2x2 tiles.
            Tilemap farPipes = Phase01Generator.CreateTilemap(root, "TM_PipesFar", terrain, 2);
            farPipes.transform.localScale = new Vector3(2f, 2f, 1f);
            farPipes.color = new Color(0.30f, 0.28f, 0.30f);

            Tilemap backPipes = Phase01Generator.CreateTilemap(root, "TM_PipesBack", terrain, 3);
            backPipes.color = new Color(0.50f, 0.48f, 0.50f);

            Tilemap backProps = Phase01Generator.CreateTilemap(root, "TM_BackProps", terrain, 5);
            backProps.color = new Color(0.50f, 0.62f, 0.60f);

            Tilemap ground = Phase01Generator.CreateTilemap(root, "TM_Ground", terrain, 10);
            Tilemap midPipes = Phase01Generator.CreateTilemap(root, "TM_PipesMid", terrain, 19);
            Tilemap decoration = Phase01Generator.CreateTilemap(root, "TM_Decoration", terrain, 20);

            Tilemap glow = Phase01Generator.CreateTilemap(root, "TM_Glow", terrain, 7);
            SetUnlit(glow, unlitMaterial);

            Tilemap foreground = Phase01Generator.CreateTilemap(root, "TM_Foreground",
                Phase01Assets.SortingForeground, 0);
            foreground.color = new Color(0.30f, 0.36f, 0.36f);

            GameObject groundObject = ground.gameObject;
            groundObject.layer = groundLayer;

            var body = groundObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;

            var tilemapCollider = groundObject.AddComponent<TilemapCollider2D>();
            var composite = groundObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            // Refaz o colisor quando a cena comeca: carregada no meio do jogo
            // ela chegava sem chao.
            groundObject.AddComponent<GroundColliderRefresh>();

            int width = Phase02Layout.Width;
            int height = Phase02Layout.Height;

            var backCaveBuffer = new TileBase[width * height];
            var backBuffer = new TileBase[width * height];
            var groundBuffer = new TileBase[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + y * width;

                    string backName = Phase02Layout.BackgroundTileName(x, y);
                    if (backName != null)
                    {
                        TileBase tile = Phase01Generator.Resolve(tiles, backName);
                        if (backName.StartsWith("bg")) backCaveBuffer[index] = tile;
                        else backBuffer[index] = tile;
                    }

                    groundBuffer[index] = Phase01Generator.Resolve(tiles, Phase02Layout.GroundTileName(x, y));
                }
            }

            var bounds = new BoundsInt(0, 0, 0, width, height, 1);
            backCave.SetTilesBlock(bounds, backCaveBuffer);
            back.SetTilesBlock(bounds, backBuffer);
            ground.SetTilesBlock(bounds, groundBuffer);

            PaintPipes(farPipes, Phase02Layout.PipeLayer.Far, tiles);
            PaintPipes(backPipes, Phase02Layout.PipeLayer.Back, tiles);
            PaintPipes(midPipes, Phase02Layout.PipeLayer.Mid, tiles);

            foreach (Phase02Layout.Prop prop in Phase02Layout.Props)
            {
                Tilemap target;
                switch (prop.Layer)
                {
                    case Phase02Layout.PropLayer.Back: target = backProps; break;
                    case Phase02Layout.PropLayer.Front: target = foreground; break;
                    case Phase02Layout.PropLayer.Glow: target = glow; break;
                    default: target = decoration; break;
                }

                target.SetTile(new Vector3Int(prop.X, prop.Y, 0), Phase01Generator.Resolve(tiles, prop.Tile));
            }

            foreach (Tilemap tilemap in new[]
                     {
                         backCave, back, farPipes, backPipes, backProps,
                         ground, midPipes, decoration, glow, foreground,
                     })
            {
                Phase01Generator.FinishTilemap(tilemap);
            }

            composite.GenerateGeometry();
        }

        private static void PaintPipes(Tilemap tilemap, Phase02Layout.PipeLayer layer,
            Dictionary<string, TileBase> tiles)
        {
            foreach ((int x, int y, string tile) in Phase02Layout.PipeTiles(layer))
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), Phase01Generator.Resolve(tiles, tile));
            }
        }

        private static void SetUnlit(Tilemap tilemap, Material unlitMaterial)
        {
            if (unlitMaterial == null) return;
            tilemap.GetComponent<TilemapRenderer>().sharedMaterial = unlitMaterial;
        }

        // ------------------------------------------------------------------
        // O horizonte de caldeiras
        // ------------------------------------------------------------------

        private static void BuildSkylines(Material unlitMaterial)
        {
            var root = new GameObject("Backdrop");

            foreach (Phase02Layout.Skyline skyline in Phase02Layout.Skylines)
            {
                Sprite sprite = Phase02Assets.LoadSprite(
                    skyline.Near ? Phase02Assets.SkylineNear : Phase02Assets.SkylineFar);
                if (sprite == null) continue;

                var layerObject = new GameObject(skyline.Name);
                layerObject.transform.SetParent(root.transform, false);

                // A faixa repete na horizontal por bem mais que a largura do
                // mapa: assim o parallax pode desloca-la a vontade sem ela acabar.
                float worldWidth = Phase02Layout.Width * 4f;
                layerObject.transform.position = new Vector3(-Phase02Layout.Width * 1.5f, skyline.Y, 0f);
                layerObject.transform.localScale = new Vector3(skyline.Scale, skyline.Scale, 1f);

                var renderer = layerObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = new Vector2(worldWidth / skyline.Scale, sprite.bounds.size.y);
                renderer.sortingLayerName = Phase01Assets.SortingParallax;
                renderer.sortingOrder = skyline.Near ? 1 : 0;
                if (unlitMaterial != null) renderer.sharedMaterial = unlitMaterial;

                var parallax = layerObject.AddComponent<ParallaxLayer>();

                // So anda na horizontal: cada faixa foi posta na altura do
                // salao por onde ela aparece.
                var serialized = new SerializedObject(parallax);
                serialized.FindProperty("parallaxFactor").floatValue = skyline.Parallax;
                serialized.FindProperty("followVertically").boolValue = false;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // ------------------------------------------------------------------
        // Caldeiras, manometros, valvulas e o resto que nao e tile
        // ------------------------------------------------------------------

        private static void BuildFixtures(Material unlitMaterial)
        {
            var root = new GameObject("Fixtures");

            foreach (Phase02Layout.Fixture fixture in Phase02Layout.Fixtures)
            {
                var fixtureObject = new GameObject(fixture.Name);
                fixtureObject.transform.SetParent(root.transform, false);
                fixtureObject.transform.position = new Vector3(fixture.X, fixture.Y, 0f);

                switch (fixture.Kind)
                {
                    case Phase02Layout.FixtureKind.Boiler:
                    {
                        AddSprite(fixtureObject, Phase02Assets.Boiler, 6);

                        // O fogo vai por cima, sem luz, pulsando no compasso da caldeira.
                        var glowObject = new GameObject("Glow");
                        glowObject.transform.SetParent(fixtureObject.transform, false);
                        SpriteRenderer glow = AddSprite(glowObject, Phase02Assets.BoilerGlow, 7);
                        if (unlitMaterial != null) glow.sharedMaterial = unlitMaterial;

                        glowObject.AddComponent<GlowPulse>().Configure(null, glow, PulsePeriod(fixture.X), PulsePhase(fixture.X));
                        break;
                    }

                    case Phase02Layout.FixtureKind.GaugeBig:
                    case Phase02Layout.FixtureKind.GaugeSmall:
                    {
                        bool big = fixture.Kind == Phase02Layout.FixtureKind.GaugeBig;
                        float half = big ? 1f : 0.5f;

                        // O mostrador tem o pivo no centro.
                        fixtureObject.transform.position = new Vector3(fixture.X + half, fixture.Y + half, 0f);
                        AddSprite(fixtureObject, big ? Phase02Assets.GaugeBig : Phase02Assets.GaugeSmall, 6);

                        var needleObject = new GameObject("Needle");
                        needleObject.transform.SetParent(fixtureObject.transform, false);
                        AddSprite(needleObject, big ? Phase02Assets.NeedleBig : Phase02Assets.NeedleSmall, 7);

                        // Todos no vermelho: a pressao do distrito esta fora de controle.
                        needleObject.AddComponent<NeedleJitter>().Configure(72f + LevelMap.Pick(fixture.X, fixture.Y, 30), big ? 9f : 6f);
                        break;
                    }

                    case Phase02Layout.FixtureKind.Valve:
                        // Pivo no centro, pra girar no lugar quando a mecanica existir.
                        fixtureObject.transform.position = new Vector3(fixture.X + 1f, fixture.Y + 1f, 0f);
                        AddSprite(fixtureObject, Phase02Assets.ValveRusty, 6);
                        break;

                    case Phase02Layout.FixtureKind.Furnace:
                        // Na frente do fogo (TM_Glow, ordem 7), pra as barras o cobrirem.
                        AddSprite(fixtureObject, Phase02Assets.Furnace, 9);
                        break;

                    case Phase02Layout.FixtureKind.Hatch:
                        AddSprite(fixtureObject, Phase02Assets.Hatch, 6);
                        break;

                    case Phase02Layout.FixtureKind.Workbench:
                        AddSprite(fixtureObject, Phase02Assets.Workbench, 21);
                        break;

                    case Phase02Layout.FixtureKind.NozzleUp:
                        AddSprite(fixtureObject, Phase02Assets.NozzleName(fixture.Size), 21);
                        break;

                    case Phase02Layout.FixtureKind.NozzleSide:
                    {
                        // O sprite e o bocal em pe; deitado ele gira em volta do
                        // canto de baixo, entao precisa ser recolocado.
                        SpriteRenderer nozzle = AddSprite(fixtureObject, Phase02Assets.NozzleName(fixture.Size), 21);
                        nozzle.transform.rotation = Quaternion.Euler(0f, 0f, fixture.Flipped ? 90f : -90f);
                        nozzle.transform.position = fixture.Flipped
                            ? new Vector3(fixture.X + 1f, fixture.Y, 0f)
                            : new Vector3(fixture.X, fixture.Y + fixture.Size, 0f);
                        break;
                    }
                }
            }
        }

        private static SpriteRenderer AddSprite(GameObject target, string spriteName, int sortingOrder)
        {
            var renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = Phase02Assets.LoadSprite(spriteName);
            renderer.sortingLayerName = Phase01Assets.SortingTerrain;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        // Cada caldeira bate num compasso um pouco diferente, senao parecem uma luz so.
        private static float PulsePeriod(float x) => 1.9f + LevelMap.Pick((int)x, 3, 9) * 0.1f;
        private static float PulsePhase(float x) => LevelMap.Pick((int)x, 5, 10) * 0.1f;

        // ------------------------------------------------------------------
        // Luz
        // ------------------------------------------------------------------

        private static void BuildLighting()
        {
            var root = new GameObject("Lighting");

            var globalObject = new GameObject("GlobalLight2D");
            globalObject.transform.SetParent(root.transform, false);

            // Fraca e quente. O distrito e escuro, e o que da pra ver vem das
            // fornalhas, do acido e dos lampioes. Subir a intensidade daqui e o
            // jeito mais rapido de clarear tudo se ficar dificil de enxergar.
            var global = globalObject.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.color = new Color(0.78f, 0.66f, 0.50f);
            global.intensity = 0.42f;

            for (int i = 0; i < Phase02Layout.Lights.Count; i++)
            {
                Phase02Layout.Light light = Phase02Layout.Lights[i];

                Light2D point = Phase01Generator.CreatePointLight(root.transform,
                    $"{light.Glow}Light_{i + 1}", (light.X, light.Y),
                    GlowColor(light.Glow), light.Intensity, light.Radius);

                if (light.Pulses)
                {
                    point.gameObject.AddComponent<GlowPulse>()
                        .Configure(point, null, PulsePeriod(light.X - 2f), PulsePhase(light.X - 2f));
                }
            }
        }

        // ------------------------------------------------------------------
        // Nevoa, goteiras e vazamentos
        // ------------------------------------------------------------------

        private static void BuildAtmosphere(Material material)
        {
            var root = new GameObject("Atmosphere");

            foreach (Phase02Layout.Haze haze in Phase02Layout.Hazes)
            {
                SceneKit.CreateHaze(root.transform, haze.Name, haze.X, haze.Y, haze.Width, haze.Thickness,
                    GlowColor(haze.Glow), haze.Alpha, haze.Rate, haze.InFront, material);
            }

            var drips = new GameObject("Drips");
            drips.transform.SetParent(root.transform, false);

            for (int i = 0; i < Phase02Layout.Drips.Count; i++)
            {
                CreateDrip(drips.transform, i, Phase02Layout.Drips[i], material);
            }

            var leaks = new GameObject("SteamLeaks");
            leaks.transform.SetParent(root.transform, false);

            for (int i = 0; i < Phase02Layout.Leaks.Count; i++)
            {
                CreateLeak(leaks.transform, i, Phase02Layout.Leaks[i], material);
            }
        }

        /// <summary>Uma goteira: uma gota de cada vez, caindo ate o chao.</summary>
        private static void CreateDrip(Transform parent, int index, Phase02Layout.Drip drip, Material material)
        {
            var dripObject = new GameObject($"Drip_{index + 1}");
            dripObject.transform.SetParent(parent, false);
            dripObject.transform.position = new Vector3(drip.X + 0.5f, drip.Y + 0.95f, 0f);

            var particles = dripObject.AddComponent<ParticleSystem>();
            float gravity = Mathf.Abs(Physics.gravity.y);

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            // A gota some na altura do chao: e o tempo de queda livre ate la.
            main.startLifetime = Mathf.Sqrt(2f * (drip.Fall + 0.9f) / gravity);
            main.startSpeed = 0f;
            main.startSize = 0.16f;
            main.startColor = drip.Acid ? new Color(0.75f, 1f, 0.35f, 0.9f) : new Color(0.70f, 0.85f, 0.90f, 0.7f);
            main.gravityModifier = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 8;

            // Cada goteira no seu ritmo, e nenhuma comecando junto.
            main.startDelay = LevelMap.Pick(drip.X, drip.Y, 30) * 0.1f;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0.35f + LevelMap.Pick(drip.X, drip.Y + 1, 6) * 0.1f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = false;

            var renderer = dripObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = Phase01Assets.SortingTerrain;
            renderer.sortingOrder = 24;
            if (material != null) renderer.sharedMaterial = material;

            particles.Stop();
        }

        /// <summary>Um fio de vapor escapando de uma junta. Nao empurra nada.</summary>
        private static void CreateLeak(Transform parent, int index, Phase02Layout.Leak leak, Material material)
        {
            var leakObject = new GameObject($"SteamLeak_{index + 1}");
            leakObject.transform.SetParent(parent, false);
            leakObject.transform.position = new Vector3(leak.X, leak.Y, 0f);

            var particles = leakObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.6f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.95f, 0.95f, 0.88f, 0.20f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 40;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 7f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = false;

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(leak.DirectionX * 1.6f - 0.3f, leak.DirectionX * 2.6f + 0.3f);
            velocity.y = new ParticleSystem.MinMaxCurve(leak.DirectionY * 1.6f - 0.3f, leak.DirectionY * 2.6f + 0.3f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
            fade.enabled = true;
            fade.color = new ParticleSystem.MinMaxGradient(SceneKit.FadeInOut(0.15f, 0.5f));

            var renderer = leakObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = Phase01Assets.SortingTerrain;
            renderer.sortingOrder = 23;
            if (material != null) renderer.sharedMaterial = material;

            particles.Stop();
        }

        // ------------------------------------------------------------------
        // Marcadores, chegada e saidas
        // ------------------------------------------------------------------

        /// <summary>
        /// Objetos vazios marcando onde entra o que ainda nao existe: valvulas,
        /// inimigos, Crisol, bancadas, o boss. Agrupados por sala.
        /// </summary>
        private static void BuildMarkers()
        {
            var root = new GameObject("Markers");
            var groups = new Dictionary<string, Transform>();

            foreach (Phase02Layout.Marker marker in Phase02Layout.Markers)
            {
                if (!groups.TryGetValue(marker.Group, out Transform group))
                {
                    group = new GameObject(marker.Group).transform;
                    group.SetParent(root.transform, false);
                    groups[marker.Group] = group;
                }

                var markerObject = new GameObject(marker.Name);
                markerObject.transform.SetParent(group, false);
                markerObject.transform.position = new Vector3(marker.X + 0.5f, marker.Y, 0f);
            }
        }

        private static void BuildTravel(Material sparkleMaterial)
        {
            var root = new GameObject("Travel");

            SceneKit.CreateSpawnPoint(root.transform, Phase02Layout.FromDistrict1SpawnId, Phase02Layout.SpawnTile);

            foreach (Phase02Layout.Exit exit in Phase02Layout.Exits)
            {
                SceneKit.CreateSceneExit(root.transform, exit.Name,
                    new RectInt(exit.Area.X, exit.Area.Y, exit.Area.Width, exit.Area.Height),
                    exit.Scene, exit.SpawnId);
            }

            // O tablado do polimento.
            Phase02Layout.Area station = Phase02Layout.PolishStation;

            var stationObject = new GameObject("PolishStation");
            stationObject.transform.SetParent(root.transform, false);
            stationObject.transform.position = new Vector3(station.CenterX, station.CenterY, 0f);

            var trigger = stationObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(station.Width - 1f, station.Height);

            var sparkleObject = new GameObject("Sparkles");
            sparkleObject.transform.SetParent(stationObject.transform, false);
            sparkleObject.transform.position = new Vector3(station.CenterX, station.Y + 1f, 0f);

            var particles = sparkleObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            main.startColor = new Color(1f, 0.95f, 0.75f, 1f);
            main.gravityModifier = 0.15f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 80;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 45) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.6f;

            ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
            fade.enabled = true;
            fade.color = new ParticleSystem.MinMaxGradient(SceneKit.FadeInOut(0.05f, 0.5f));

            var renderer = sparkleObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = Phase01Assets.SortingForeground;
            renderer.sortingOrder = 20;
            if (sparkleMaterial != null) renderer.sharedMaterial = sparkleMaterial;

            particles.Stop();

            stationObject.AddComponent<PolishStation>().Configure(particles);
        }
    }
}
