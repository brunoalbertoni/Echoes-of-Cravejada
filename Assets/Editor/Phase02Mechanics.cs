using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Monta o que funciona na Phase 2: vapor, acido, portoes, paredes
    /// rachadas, valvulas, inimigos, caldeiras de pressao, bancadas, Crisol, o
    /// Boiler Master, o acido ascendente e a purificacao.
    ///
    /// E chamado pelo Phase02Generator depois de o cenario, as luzes e a nevoa
    /// existirem, porque a purificacao os procura pelo nome.
    ///
    /// A ordem aqui importa: primeiro o que as valvulas mexem (vapor, pocas,
    /// portoes), depois as valvulas, e por ultimo o boss, que escuta as dele.
    /// </summary>
    internal static class Phase02Mechanics
    {
        private static Material _steam;
        private static Material _unlit;
        private static int _groundLayer;

        private static readonly Dictionary<string, SteamVent> Vents = new Dictionary<string, SteamVent>();
        private static readonly Dictionary<string, AcidPool> Pools = new Dictionary<string, AcidPool>();
        private static readonly Dictionary<string, Gate> Gates = new Dictionary<string, Gate>();
        private static readonly Dictionary<string, GameObject> Walls = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, Valve> Valves = new Dictionary<string, Valve>();

        // O que a purificacao acende (quem so aparece depois dela) e apaga (a nevoa densa).
        private static readonly List<GameObject> AppearOnPurify = new List<GameObject>();
        private static readonly List<GameObject> VanishOnPurify = new List<GameObject>();

        public static void Build(int groundLayer, Material steamMaterial, Material unlitMaterial)
        {
            _groundLayer = groundLayer;
            _steam = steamMaterial;
            _unlit = unlitMaterial;

            Vents.Clear();
            Pools.Clear();
            Gates.Clear();
            Walls.Clear();
            Valves.Clear();
            AppearOnPurify.Clear();
            VanishOnPurify.Clear();

            var root = new GameObject("Mechanics").transform;

            BuildVents(Child(root, "SteamVents"));
            BuildClouds(Child(root, "SteamClouds"));
            BuildPools(Child(root, "Acid"));
            BuildGates(Child(root, "Gates"));
            BuildWalls(Child(root, "BreakableWalls"));
            BuildValves(Child(root, "Valves"));

            Transform enemies = Child(root, "Enemies");
            BuildEnemies(enemies);

            BuildPressureBoilers(Child(root, "PressureBoilers"));
            BuildWorkbenches(Child(root, "Workbenches"));
            BuildCrisol(root);
            BuildLore(Child(root, "Lore"));
            BuildRewards(Child(root, "Rewards"));
            BuildVeils(Child(root, "Veils"));
            BuildBoss(root);
            BuildRisingAcid(root);
            BuildPurification(root, enemies.gameObject);
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

        private static T[] Lookup<T>(Dictionary<string, T> all, string[] names)
        {
            return names.Where(all.ContainsKey).Select(name => all[name]).ToArray();
        }

        // ------------------------------------------------------------------
        // Vapor
        // ------------------------------------------------------------------

        private static void BuildVents(Transform parent)
        {
            foreach (Phase02Layout.Vent vent in Phase02Layout.Vents)
            {
                var cells = new RectInt(vent.Zone.X, vent.Zone.Y, vent.Zone.Width, vent.Zone.Height);
                bool lift = vent.Kind == Phase02Layout.VentKind.Lift;

                SteamVent built = SceneKit.CreateSteamVent(parent, vent.Name, cells,
                    lift ? SteamVent.Mode.Lift : SteamVent.Mode.Jet,
                    vent.Direction, vent.OnTime, vent.OffTime, _steam);

                // Os que esperam uma valvula comecam desligados.
                built.SetStartsOff(vent.StartsOff);
                Vents[vent.Name] = built;
            }
        }

        /// <summary>Nuvens paradas: um gatilho que devolve o dash, e um vapor leve pra dar pra ver.</summary>
        private static void BuildClouds(Transform parent)
        {
            foreach ((string name, Phase02Layout.Area area) in Phase02Layout.Clouds)
            {
                var cloudObject = new GameObject(name);
                cloudObject.transform.SetParent(parent, false);
                cloudObject.transform.position = new Vector3(area.CenterX, area.CenterY, 0f);

                var trigger = cloudObject.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(area.Width, area.Height);

                cloudObject.AddComponent<SteamCloud>();

                var particles = cloudObject.AddComponent<ParticleSystem>();

                ParticleSystem.MainModule main = particles.main;
                main.loop = true;
                main.prewarm = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3.5f);
                main.startSpeed = 0f;
                main.startSize = new ParticleSystem.MinMaxCurve(1.6f, 3f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
                main.startColor = new Color(0.95f, 0.97f, 1f, 0.30f);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 80;

                ParticleSystem.EmissionModule emission = particles.emission;
                emission.rateOverTime = area.Width * area.Height * 0.9f;

                ParticleSystem.ShapeModule shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(area.Width, area.Height, 1f);

                ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.World;
                velocity.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
                velocity.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.2f);
                velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

                ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
                fade.enabled = true;
                fade.color = new ParticleSystem.MinMaxGradient(SceneKit.FadeInOut(0.3f, 0.7f));

                var renderer = cloudObject.GetComponent<ParticleSystemRenderer>();
                renderer.sortingLayerName = Phase01Assets.SortingTerrain;
                renderer.sortingOrder = 26;
                if (_steam != null) renderer.sharedMaterial = _steam;

                particles.Stop();
            }
        }

        // ------------------------------------------------------------------
        // Acido
        // ------------------------------------------------------------------

        private static void BuildPools(Transform parent)
        {
            foreach ((string name, Phase02Layout.Area area) in Phase02Layout.AcidPools)
            {
                // O do poco de fuga ocupa a largura toda do poco, pra poder subir por ele.
                bool rising = name == Phase02Layout.RisingAcidPool;
                int x = area.X;
                int width = rising ? 11 : area.Width;

                var poolObject = new GameObject(name);
                poolObject.transform.SetParent(parent, false);
                poolObject.transform.position = new Vector3(x, area.Y, 0f);

                var bodyObject = new GameObject("Body");
                bodyObject.transform.SetParent(poolObject.transform, false);

                // Atras do terreno: as bordas do tanque e a grade passam por cima.
                SpriteRenderer body = Sprite(bodyObject, Phase02Assets.AcidBody, Phase01Assets.SortingTerrain, 8);
                body.drawMode = SpriteDrawMode.Tiled;
                if (_unlit != null) body.sharedMaterial = _unlit;

                var surfaceObject = new GameObject("Surface");
                surfaceObject.transform.SetParent(poolObject.transform, false);

                SpriteRenderer surface = Sprite(surfaceObject, Phase02Assets.AcidSurface, Phase01Assets.SortingTerrain, 9);
                surface.drawMode = SpriteDrawMode.Tiled;
                surface.size = new Vector2(width, 6f / Phase01Assets.PixelsPerUnit);
                if (_unlit != null) surface.sharedMaterial = _unlit;

                Transform bubbles = BuildBubbles(poolObject.transform, width);

                poolObject.AddComponent<BoxCollider2D>();

                var pool = poolObject.AddComponent<AcidPool>();
                pool.Configure(width, area.Height, body, surface, bubbles);
                Pools[name] = pool;
            }
        }

        /// <summary>Bolhas subindo e estourando logo acima da superficie.</summary>
        private static Transform BuildBubbles(Transform parent, float width)
        {
            var bubbleObject = new GameObject("Bubbles");
            bubbleObject.transform.SetParent(parent, false);

            var particles = bubbleObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.22f);
            main.startColor = new Color(0.85f, 1f, 0.45f, 0.9f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = width * 0.9f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(Mathf.Max(0.5f, width - 0.5f), 0.1f, 1f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
            fade.enabled = true;
            fade.color = new ParticleSystem.MinMaxGradient(SceneKit.FadeInOut(0.1f, 0.7f));

            var renderer = bubbleObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = Phase01Assets.SortingTerrain;
            renderer.sortingOrder = 9;
            if (_steam != null) renderer.sharedMaterial = _steam;

            particles.Stop();
            return bubbleObject.transform;
        }

        // ------------------------------------------------------------------
        // Portoes e paredes rachadas
        // ------------------------------------------------------------------

        private static void BuildGates(Transform parent)
        {
            foreach (Phase02Layout.GateSpec spec in Phase02Layout.Gates)
            {
                var gateObject = new GameObject(spec.Name) { layer = _groundLayer };
                gateObject.transform.SetParent(parent, false);
                gateObject.transform.position = new Vector3(spec.Area.X, spec.Area.Y, 0f);

                SpriteRenderer bars = Sprite(gateObject, Phase02Assets.GateBars, Phase01Assets.SortingTerrain, 15);
                bars.drawMode = SpriteDrawMode.Tiled;
                bars.size = new Vector2(spec.Area.Width, spec.Area.Height);

                var wall = gateObject.AddComponent<BoxCollider2D>();
                wall.size = new Vector2(spec.Area.Width, spec.Area.Height);
                wall.offset = new Vector2(spec.Area.Width * 0.5f, spec.Area.Height * 0.5f);

                // Corre o proprio tamanho e mais um tile, pra sumir dentro da parede.
                float travel = (spec.MoveX != 0 ? spec.Area.Width : spec.Area.Height) + 1f;

                var gate = gateObject.AddComponent<Gate>();
                gate.Configure(travel, new Vector2(spec.MoveX, spec.MoveY));
                Gates[spec.Name] = gate;
            }
        }

        private static void BuildWalls(Transform parent)
        {
            foreach ((string name, Phase02Layout.Area area) in Phase02Layout.BreakableWalls)
            {
                var wallObject = new GameObject(name) { layer = _groundLayer };
                wallObject.transform.SetParent(parent, false);
                wallObject.transform.position = new Vector3(area.X, area.Y, 0f);

                SpriteRenderer plate = Sprite(wallObject, Phase02Assets.CrackedWall, Phase01Assets.SortingTerrain, 15);
                plate.drawMode = SpriteDrawMode.Tiled;
                plate.size = new Vector2(area.Width, area.Height);

                var collider = wallObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(area.Width, area.Height);
                collider.offset = new Vector2(area.Width * 0.5f, area.Height * 0.5f);

                Walls[name] = wallObject;
            }
        }

        // ------------------------------------------------------------------
        // Valvulas
        // ------------------------------------------------------------------

        private static void BuildValves(Transform parent)
        {
            foreach (Phase02Layout.ValveSpec spec in Phase02Layout.Valves)
            {
                // O volante tem o pivo no centro, pra girar no lugar.
                var center = new Vector3(spec.X + 1f, spec.Y + 1f, 0f);

                var valveObject = new GameObject(spec.Name);
                valveObject.transform.SetParent(parent, false);
                valveObject.transform.position = center;

                var wheelObject = new GameObject("Wheel");
                wheelObject.transform.SetParent(valveObject.transform, false);
                SpriteRenderer wheel = Sprite(wheelObject, Phase02Assets.ValveRusty, Phase01Assets.SortingTerrain, 6);

                // A area de raspar fica onde o Scrap para, que nem sempre e embaixo do volante.
                var area = valveObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.6f, 2.6f);
                area.offset = new Vector2(spec.StandX + 0.5f - center.x, spec.StandY + 1.3f - center.y);

                var valve = valveObject.AddComponent<Valve>();
                valve.Configure(
                    Lookup(Vents, spec.Vents),
                    Lookup(Pools, spec.Pools),
                    Lookup(Gates, spec.Gates),
                    wheel,
                    Phase02Assets.LoadSprite(Phase02Assets.ValveClean));

                Valves[spec.Name] = valve;
            }
        }

        // ------------------------------------------------------------------
        // Inimigos
        // ------------------------------------------------------------------

        private static void BuildEnemies(Transform parent)
        {
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);

            foreach (Phase02Layout.EnemySpec spec in Phase02Layout.Enemies)
            {
                var enemy = new GameObject(spec.Name);
                enemy.transform.SetParent(parent, false);

                var body = enemy.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;

                // Gatilho, e nao parede: o Scrap atravessa e apanha.
                var trigger = enemy.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;

                float middle = (spec.X0 + spec.X1 + 1) * 0.5f;

                switch (spec.Kind)
                {
                    case Phase02Layout.EnemyKind.PipeLeech:
                        // O x de onde ela pende vem em Direction (ver Phase02Layout.Leech).
                        enemy.transform.position = new Vector3(spec.Direction + 0.5f, spec.CeilingY + 0.5f, 0f);
                        Sprite(enemy, Phase02Assets.PipeLeech, Phase01Assets.SortingActors, -3).flipY = true;
                        trigger.size = new Vector2(0.8f, 0.7f);

                        enemy.AddComponent<Damageable>().Configure(5, 2, shard);
                        enemy.AddComponent<PipeLeech>().Configure(spec.Y + 0.5f, spec.X0 + 0.5f, spec.X1 + 0.5f);
                        break;

                    case Phase02Layout.EnemyKind.ValveSentry:
                        enemy.transform.position = new Vector3(spec.X0 + 0.5f, spec.Y, 0f);
                        Sprite(enemy, Phase02Assets.ValveSentry, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(0.8f, 1.4f);
                        trigger.offset = new Vector2(0f, 0.7f);

                        enemy.AddComponent<Damageable>().Configure(6, 4, shard);
                        enemy.AddComponent<ValveSentry>().Configure(spec.Direction, spec.Range,
                            Phase02Assets.LoadSprite(Phase02Assets.SteamBolt));
                        break;

                    case Phase02Layout.EnemyKind.BrassWelder:
                        enemy.transform.position = new Vector3(middle, spec.Y, 0f);
                        Sprite(enemy, Phase02Assets.BrassWelder, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(0.8f, 1.4f);
                        trigger.offset = new Vector2(0f, 0.7f);

                        enemy.AddComponent<Damageable>().Configure(7, 5, shard);
                        enemy.AddComponent<BrassWelder>().Configure(spec.X0 + 0.5f, spec.X1 + 0.5f);
                        break;

                    case Phase02Layout.EnemyKind.AcidBloater:
                        // Pivo no centro: os pes ficam 0.75 abaixo.
                        enemy.transform.position = new Vector3(middle, spec.Y + 0.75f, 0f);
                        Sprite(enemy, Phase02Assets.AcidBloater, Phase01Assets.SortingActors, -3);
                        trigger.size = new Vector2(1.2f, 1.3f);

                        enemy.AddComponent<Damageable>().Configure(8, 6, shard);
                        enemy.AddComponent<AcidBloater>().Configure(spec.X0 + 0.75f, spec.X1 + 0.25f,
                            Phase02Assets.LoadSprite(Phase02Assets.AcidSurface));
                        break;
                }
            }
        }

        // ------------------------------------------------------------------
        // Caldeiras de pressao, bancadas e Crisol
        // ------------------------------------------------------------------

        private static void BuildPressureBoilers(Transform parent)
        {
            foreach (Phase02Layout.BoilerSpec spec in Phase02Layout.PressureBoilers)
            {
                var boilerObject = new GameObject(spec.Name);
                boilerObject.transform.SetParent(parent, false);
                boilerObject.transform.position = new Vector3(spec.X, spec.Y, 0f);

                // A mesma caldeira do cenario, esverdeada: esta tomada pelo Oxide.
                SpriteRenderer sprite = Sprite(boilerObject, Phase02Assets.Boiler, Phase01Assets.SortingTerrain, 6);
                sprite.color = new Color(0.72f, 1f, 0.78f);

                var body = boilerObject.AddComponent<BoxCollider2D>();
                body.isTrigger = true;
                body.size = new Vector2(3.2f, 5f);
                body.offset = new Vector2(2f, 2.5f);

                var blastObject = new GameObject("Blast");
                blastObject.transform.SetParent(boilerObject.transform, false);
                blastObject.transform.localPosition = new Vector3(2f, 2.5f, 0f);

                var blast = blastObject.AddComponent<ParticleSystem>();

                ParticleSystem.MainModule main = blast.main;
                main.loop = false;
                main.playOnAwake = false;
                main.duration = 1f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 9f);
                main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 4f);
                main.startColor = new Color(1f, 0.75f, 0.4f, 0.6f);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 80;

                ParticleSystem.EmissionModule emission = blast.emission;
                emission.rateOverTime = 0f;
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 50) });

                ParticleSystem.ShapeModule shape = blast.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 1f;

                ParticleSystem.ColorOverLifetimeModule fade = blast.colorOverLifetime;
                fade.enabled = true;
                fade.color = new ParticleSystem.MinMaxGradient(SceneKit.FadeInOut(0.05f, 0.4f));

                var renderer = blastObject.GetComponent<ParticleSystemRenderer>();
                renderer.sortingLayerName = Phase01Assets.SortingForeground;
                renderer.sortingOrder = 9;
                if (_steam != null) renderer.sharedMaterial = _steam;

                blast.Stop();

                boilerObject.AddComponent<PressureBoiler>()
                    .Configure(Lookup(Walls, spec.Walls), sprite, blast);
            }
        }

        private static void BuildWorkbenches(Transform parent)
        {
            foreach ((string name, int x, int y) in Phase02Layout.Workbenches)
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
            (int X, int Y) tile = Phase02Layout.CrisolTile;

            var crisolObject = new GameObject("Npc_Crisol");
            crisolObject.transform.SetParent(parent, false);
            crisolObject.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);

            Sprite(crisolObject, Phase02Assets.Crisol, Phase01Assets.SortingActors, -4);

            var area = crisolObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3.2f, 2.6f);
            area.offset = new Vector2(0f, 1.3f);

            var crisol = crisolObject.AddComponent<Crisol>();
            crisol.Configure("Crisol", Phase02Layout.CrisolLines);
            crisol.ConfigureRepeat(Phase02Layout.CrisolRepeatLines);
            crisol.ConfigurePurified(Phase02Layout.CrisolPurifiedLines);
            crisol.ConfigureMemory(Phase02Layout.CrisolMemoryLines);
        }

        /// <summary>
        /// A historia espalhada pelo mapa: paginas, pichacoes, a placa e o velho
        /// encanador. Tudo usa o Npc — chegar perto e apertar E.
        /// </summary>
        private static void BuildLore(Transform parent)
        {
            foreach (Phase02Layout.LoreSpec spec in Phase02Layout.Lore)
            {
                var loreObject = new GameObject(spec.Name);
                loreObject.transform.SetParent(parent, false);
                loreObject.transform.position = new Vector3(spec.X + 0.5f, spec.Y, 0f);

                var area = loreObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.6f, 2.5f);
                area.offset = new Vector2(0f, 1.25f);

                // O desenho fica num filho, pra poder subir pra parede sem tirar
                // a area de leitura do chao.
                var artObject = new GameObject("Art");
                artObject.transform.SetParent(loreObject.transform, false);

                switch (spec.Kind)
                {
                    case Phase02Layout.LoreKind.Page:
                        Sprite(artObject, Phase02Assets.LoosePage, Phase01Assets.SortingTerrain, 22);
                        break;

                    case Phase02Layout.LoreKind.Graffiti:
                        artObject.transform.localPosition = new Vector3(0f, 2f, 0f);
                        Sprite(artObject, Phase02Assets.ScribbleText, Phase01Assets.SortingTerrain, 4);
                        break;

                    case Phase02Layout.LoreKind.GraffitiMask:
                        artObject.transform.localPosition = new Vector3(0f, 2f, 0f);
                        Sprite(artObject, Phase02Assets.ScribbleMask, Phase01Assets.SortingTerrain, 4);
                        break;

                    case Phase02Layout.LoreKind.Plaque:
                        artObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                        Sprite(artObject, Phase02Assets.Plaque, Phase01Assets.SortingTerrain, 6);
                        break;

                    case Phase02Layout.LoreKind.Tarnished:
                        // O mesmo corpo do Brass Welder, sem a ferrugem de luta:
                        // e um deles, so que nao foi tomado.
                        SpriteRenderer body = Sprite(loreObject, Phase02Assets.BrassWelder, Phase01Assets.SortingActors, -4);
                        body.color = new Color(0.85f, 0.92f, 1f);
                        Object.DestroyImmediate(artObject);
                        break;
                }

                // O componente diz o que ele faz alem de falar: pede coisas (o
                // velho encanador), se leva embora (as paginas do livro de
                // turnos) ou so se le.
                Npc npc;
                if (spec.Quest != null)
                {
                    var quest = loreObject.AddComponent<ItemQuestNpc>();
                    quest.ConfigureQuest(spec.Quest.ItemPrefix, spec.Quest.ItemCount, spec.Quest.GemReward,
                        "Gema lapidada recebida", spec.Quest.PurifiedGreeting,
                        spec.Quest.Progress.Select(lines => new LineSet(lines)).ToArray(),
                        spec.Quest.Complete, spec.Quest.Done);
                    npc = quest;
                }
                else if (spec.CollectId != null)
                {
                    var page = loreObject.AddComponent<CollectibleReadable>();
                    page.ConfigureCollectible(spec.CollectId, spec.CollectMessage,
                        Phase02Layout.LogbookPrefix, Phase02Layout.LogbookCount);
                    npc = page;
                }
                else
                {
                    npc = loreObject.AddComponent<Npc>();
                }

                if (spec.Kind == Phase02Layout.LoreKind.Tarnished) npc.Configure(spec.Title, spec.Lines);
                else npc.ConfigureReadable(spec.Title, spec.Lines);

                if (spec.PurifiedLines != null) npc.ConfigurePurified(spec.PurifiedLines);

                // Uma luz fraca, pra dar pra achar no escuro.
                Phase01Generator.CreatePointLight(loreObject.transform, "Glow",
                    (spec.X + 0.5f, spec.Y + 1.2f), new Color(0.95f, 0.90f, 0.75f), 0.5f, 3.5f);

                // Quem so aparece depois da purificacao fica guardado pra ela acender.
                if (spec.AfterPurification) AppearOnPurify.Add(loreObject);
            }
        }

        /// <summary>
        /// O que se acha nos esconderijos: gemas lapidadas, cofres de fragmentos
        /// e reforcos de chassi. Cada um lembra que ja foi pego.
        /// </summary>
        private static void BuildRewards(Transform parent)
        {
            foreach (Phase02Layout.RewardSpec spec in Phase02Layout.Rewards)
            {
                var pickup = new GameObject(spec.Id);
                pickup.transform.SetParent(parent, false);
                pickup.transform.position = new Vector3(spec.X + 0.5f, spec.Y + 0.8f, 0f);

                var trigger = pickup.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = 0.7f;

                Color glow;
                switch (spec.Kind)
                {
                    case Phase02Layout.RewardKind.Gem:
                        Sprite(pickup, Phase02Assets.CutGem, Phase01Assets.SortingActors, -1);
                        pickup.AddComponent<RewardPickup>().Configure(RewardPickup.Kind.Gem, spec.Id);
                        glow = new Color(0.55f, 0.85f, 1f);
                        break;

                    case Phase02Layout.RewardKind.ShardCache:
                        Sprite(pickup, Phase02Assets.ShardCache, Phase01Assets.SortingActors, -1);
                        pickup.AddComponent<RewardPickup>().Configure(RewardPickup.Kind.ShardCache, spec.Id, spec.Amount);
                        glow = new Color(0.95f, 0.85f, 0.6f);
                        break;

                    default:
                        // O mesmo reforco escondido dos outros distritos: o modulo, dourado.
                        SpriteRenderer module = Sprite(pickup, Phase02Assets.DashModule, Phase01Assets.SortingActors, -1);
                        module.color = new Color(1f, 0.85f, 0.45f);
                        pickup.AddComponent<RewardPickup>().Configure(RewardPickup.Kind.Chassis, spec.Id);
                        glow = new Color(1f, 0.85f, 0.55f);
                        break;
                }

                Phase01Generator.CreatePointLight(pickup.transform, "Glow",
                    (spec.X + 0.5f, spec.Y + 0.8f), glow, 0.5f, 3f);
            }
        }

        /// <summary>
        /// A nevoa densa de Oxide que tampa a Galeria Alta: uma parede, o verde
        /// por cima (um banco de nevoa grosso, no meio da nevoa do distrito,
        /// que a purificacao para) e uma luz. Some com a purificacao.
        /// </summary>
        private static void BuildVeils(Transform parent)
        {
            GameObject atmosphere = GameObject.Find("Atmosphere");

            foreach ((string name, Phase02Layout.Area area) in Phase02Layout.Veils)
            {
                var veil = new GameObject(name) { layer = _groundLayer };
                veil.transform.SetParent(parent, false);
                veil.transform.position = new Vector3(area.X, area.Y, 0f);

                var wall = veil.AddComponent<BoxCollider2D>();
                wall.size = new Vector2(area.Width, area.Height);
                wall.offset = new Vector2(area.Width * 0.5f, area.Height * 0.5f);

                veil.AddComponent<OxideVeil>();

                Phase01Generator.CreatePointLight(veil.transform, "Glow",
                    (area.CenterX, area.CenterY), new Color(0.36f, 0.95f, 0.60f), 0.9f, 4.5f);

                // O nome comeca com "Oxide_": e o que a purificacao procura pra parar.
                var bank = new Phase01Layout.FogBank("Oxide_" + name, area.CenterX, area.CenterY,
                    area.Width + 1.5f, area.Height, 0.5f, 7f, true);
                Phase01Generator.CreateFogBank(atmosphere != null ? atmosphere.transform : parent, bank, _steam);

                VanishOnPurify.Add(veil);
            }
        }

        // ------------------------------------------------------------------
        // O Boiler Master, o acido ascendente e a purificacao
        // ------------------------------------------------------------------

        private static void BuildBoss(Transform parent)
        {
            (int X, int Y) tile = Phase02Layout.BossTile;
            (int X0, int X1) range = Phase02Layout.BossRange;
            Phase02Layout.Area wake = Phase02Layout.BossWakeArea;

            var bossObject = new GameObject("BoilerMaster");
            bossObject.transform.SetParent(parent, false);
            bossObject.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);

            Sprite(bossObject, Phase02Assets.BoilerMaster, Phase01Assets.SortingActors, -2);

            var body = bossObject.AddComponent<BoxCollider2D>();
            body.isTrigger = true;
            body.size = new Vector2(2.8f, 3.4f);
            body.offset = new Vector2(0f, 1.8f);

            var rigidbody = bossObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;

            Valve[] arenaValves = Phase02Layout.Valves
                .Where(spec => spec.Arena && Valves.ContainsKey(spec.Name))
                .Select(spec => Valves[spec.Name])
                .ToArray();

            Gates.TryGetValue(Phase02Layout.BossGate, out Gate exitGate);

            var master = bossObject.AddComponent<BoilerMaster>();
            master.Configure(
                range.X0 + 0.5f, range.X1 + 0.5f, tile.Y,
                new Rect(wake.X, wake.Y, wake.Width, wake.Height),
                Phase02Assets.LoadSprite(Phase02Assets.SteamBolt),
                Phase02Assets.LoadSprite(Phase02Assets.ShockWave),
                Phase02Assets.LoadSprite(Phase02Assets.SteamColumn),
                Phase02Assets.LoadSprite(Phase02Assets.Shard),
                Phase02Assets.LoadSprite(Phase02Assets.DashModule),
                arenaValves, exitGate);

            // A nuvem em que ele explode ao cair.
            master.ConfigureCloud(Phase01Generator.CreateOxideCloud(bossObject.transform, _steam));

            // A grade da passagem pra sala pre-boss: fecha enquanto ele luta.
            Phase02Layout.Area seal = Phase02Layout.ArenaSeal;
            Phase01Generator.CreateArenaSeal(parent, _groundLayer, new RectInt(seal.X, seal.Y, seal.Width, seal.Height));
        }

        private static void BuildRisingAcid(Transform parent)
        {
            if (!Pools.TryGetValue(Phase02Layout.RisingAcidPool, out AcidPool pool)) return;

            Phase02Layout.Area area = Phase02Layout.RisingAcidTrigger;

            var triggerObject = new GameObject("RisingAcidTrigger");
            triggerObject.transform.SetParent(parent, false);
            triggerObject.transform.position = new Vector3(area.CenterX, area.CenterY, 0f);

            var trigger = triggerObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(area.Width, area.Height);

            triggerObject.AddComponent<RisingAcidTrigger>().Configure(pool, Phase02Layout.RisingAcidHeight);
        }

        /// <summary>
        /// A purificacao do District 2: chegar ao topo do poco de fuga depois do
        /// boss seca todo o acido, apaga o verde, acende os lampioes, some com
        /// os corrompidos e com a nevoa da Galeria Alta, e traz a Solda pra fora.
        /// (Nao abre atalho pro poco: a saida do distrito e so pela sala do boss.)
        /// </summary>
        private static void BuildPurification(Transform parent, GameObject enemies)
        {
            GameObject lighting = GameObject.Find("Lighting");
            var corrupted = new List<Light2D>();
            var purified = new List<Light2D>();
            Light2D global = null;

            if (lighting != null)
            {
                foreach (Light2D light in lighting.GetComponentsInChildren<Light2D>())
                {
                    if (light.lightType == Light2D.LightType.Global)
                    {
                        global = light;
                        continue;
                    }

                    // O que e verde (acido e Oxide) apaga; no lugar acende um lampiao.
                    if (!light.name.StartsWith("Acid") && !light.name.StartsWith("Oxide")) continue;

                    corrupted.Add(light);

                    Vector3 position = light.transform.position;
                    purified.Add(Phase01Generator.CreatePointLight(lighting.transform,
                        "PurifiedLamp_" + purified.Count, (position.x, position.y + 2f),
                        new Color(1f, 0.82f, 0.55f), 0.7f, 8f));
                }
            }

            // A nevoa verde para; o vapor branco fica.
            GameObject atmosphere = GameObject.Find("Atmosphere");
            ParticleSystem[] fog = atmosphere != null
                ? atmosphere.GetComponentsInChildren<ParticleSystem>()
                    .Where(particles => particles.name.StartsWith("Fumes_") || particles.name.StartsWith("Oxide_"))
                    .ToArray()
                : new ParticleSystem[0];

            GameObject cameraObject = GameObject.Find("Main Camera");

            Gate[] shortcuts = Phase02Layout.Gates
                .Where(spec => spec.Opener == Phase02Layout.GateOpener.Purification && Gates.ContainsKey(spec.Name))
                .Select(spec => Gates[spec.Name])
                .ToArray();

            var purifierObject = new GameObject("AreaPurifier");
            purifierObject.transform.SetParent(parent, false);

            // Some: os corrompidos e a nevoa densa da Galeria Alta. Aparece: quem
            // so sai do esconderijo depois (a Solda).
            var vanish = new List<GameObject>(VanishOnPurify) { enemies };

            var purifier = purifierObject.AddComponent<AreaPurifier>();
            purifier.Configure(global, corrupted.ToArray(), purified.ToArray(), fog, new Tilemap[0],
                cameraObject != null ? cameraObject.GetComponent<Camera>() : null,
                AppearOnPurify.ToArray(), vanish.ToArray(), shortcuts);
            purifier.ConfigurePools(Pools.Values.ToArray());

            Phase02Layout.Area area = Phase02Layout.PurifyArea;

            var triggerObject = new GameObject("PurifyTrigger");
            triggerObject.transform.SetParent(parent, false);
            triggerObject.transform.position = new Vector3(area.CenterX, area.CenterY, 0f);

            var trigger = triggerObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(area.Width, area.Height);

            triggerObject.AddComponent<PurifyTrigger>().Configure(purifier);
        }
    }
}
