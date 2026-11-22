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
    public static partial class Phase04Generator
    {
        // ------------------------------------------------------------------
        // Espinhos, mercurio e transmutacao
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

        private static void AddSurface(GameObject pool, Phase04Layout.Cells area, float current)
        {
            // A faixa logo acima da superficie: e ela que avisa o Scrap de que
            // ele esta pisando em mercurio.
            var surfaceObject = new GameObject("Surface");
            surfaceObject.transform.SetParent(pool.transform, false);
            surfaceObject.transform.localPosition = new Vector3(area.Width * 0.5f, area.Height + 0.3f, 0f);

            var trigger = surfaceObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(area.Width, 0.6f);

            surfaceObject.AddComponent<MercurySurface>().Configurar(current);
        }

        private static void BuildPools(Transform parent)
        {
            foreach (Phase04Layout.Pool pool in Phase04Layout.Pools)
            {
                GameObject poolObject = SolidBlock(parent, pool.Name, pool.Area, Mercury, 11);

                // O corrompido e esverdeado: da pra saber antes de pisar.
                if (!Mathf.Approximately(pool.Current, 0f)) poolObject.GetComponent<SpriteRenderer>().color = new Color(0.70f, 1f, 0.78f);

                AddSurface(poolObject, pool.Area, pool.Current);
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
                dripObject.AddComponent<MercuryDrip>().Configurar(
                    2.2f + LevelMap.Pick(x, y, 5) * 0.25f, LevelMap.Pick(x, y + 1, 20) * 0.1f, fall + 0.9f, drop);
            }
        }

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
                    SteamVent.Mode.Erguer2, 1, 0f, 0f, _steam);

                Transmutables[name] = vent.gameObject;
            }
        }

        private static GameObject[] Lookup(string[] names)
        {
            return names.Where(Transmutables.ContainsKey).Select(name => Transmutables[name]).ToArray();
        }

        private static SpriteRenderer Rune(GameObject target, float scale = 1f)
        {
            // A gravura fica rente ao chao e tem luz propria: e pra ser achada.
            SpriteRenderer rune = Sprite(target, Circle, Phase01Assets.SortingTerrain, 21);
            rune.color = RuneDim;
            target.transform.localScale = Vector3.one * scale;
            if (_unlit != null) rune.sharedMaterial = _unlit;
            return rune;
        }

        private static void BuildCircles(Transform parent)
        {
            foreach (Phase04Layout.Circle circle in Phase04Layout.Circles)
            {
                var circleObject = new GameObject(circle.Name);
                circleObject.transform.SetParent(parent, false);
                circleObject.transform.position = new Vector3(circle.X + 0.5f, circle.Y, 0f);

                SpriteRenderer rune = Rune(circleObject);

                var area = circleObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.6f, 2.2f);
                area.offset = new Vector2(0f, 1.1f);

                circleObject.AddComponent<TransmutationCircle>()
                    .Configurar(circle.Effect, Lookup(circle.Vanish), Lookup(circle.Appear), rune);
            }
        }

        // ------------------------------------------------------------------
        // Os raios da Optica
        // ------------------------------------------------------------------


        /// <summary>
        /// Os raios. Os de perigo ligam e desligam em ritmo e queimam o Scrap;
        /// a luz-guia dos enigmas e azulada, mais fina, sempre acesa, e nao
        /// queima: so dobra nos espelhos e acende as fechaduras.
        /// </summary>
        private static void BuildBeams(Transform parent)
        {
            foreach (Phase04Layout.Beam beam in Phase04Layout.Beams)
            {
                var emitterObject = new GameObject(beam.Name);
                emitterObject.transform.SetParent(parent, false);
                emitterObject.transform.position = new Vector3(beam.X + 0.5f, beam.Y + 0.5f, 0f);

                var lanternObject = new GameObject("Lantern");
                lanternObject.transform.SetParent(emitterObject.transform, false);
                SpriteRenderer lantern = Sprite(lanternObject, Emitter, Phase01Assets.SortingTerrain, 22);
                if (beam.Guide) lantern.color = new Color(0.80f, 0.92f, 1f);

                // O desenho olha pra direita: gira pro lado do raio.
                float angle = Mathf.Atan2(beam.DirY, beam.DirX) * Mathf.Rad2Deg;
                if (beam.DirX < 0) lantern.flipX = true;
                else lanternObject.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                Color color = beam.Guide ? GuideLight : HazardLight;
                float width = beam.Guide ? 0.11f : 0.16f;

                var line = emitterObject.AddComponent<LineRenderer>();
                line.sharedMaterial = _unlit;
                line.startColor = color;
                line.endColor = color;
                line.startWidth = width;
                line.endWidth = width;
                line.numCapVertices = 2;
                line.numCornerVertices = 2;
                line.useWorldSpace = true;
                line.sortingLayerName = Phase01Assets.SortingActors;
                line.sortingOrder = 4;
                line.positionCount = 0;

                var light = emitterObject.AddComponent<LightBeam>();
                light.Configurar(new Vector2(beam.DirX, beam.DirY));
                if (beam.Guide) light.ConfigurarGuia();
                else light.ConfigurarPulso(beam.On, beam.Off, beam.Offset, false);

                Phase01Generator.CreatePointLight(emitterObject.transform, "Glow",
                    (beam.X + 0.5f, beam.Y + 0.5f), beam.Guide ? new Color(0.70f, 0.88f, 1f) : new Color(1f, 0.95f, 0.65f), 0.8f, 4f);
            }
        }

        /// <summary>
        /// Os enigmas de espelho: os espelhos de prata (girar com E) e as
        /// fechaduras de cristal, cada uma ligada ao portao que abre. A
        /// fechadura escondida do cofre nao brilha ate ser acesa.
        /// </summary>
        private static void BuildMirrorPuzzles(Transform parent)
        {
            foreach (Phase04Layout.MirrorSpec spec in Phase04Layout.Mirrors)
            {
                var mirrorObject = new GameObject(spec.Name);
                mirrorObject.transform.SetParent(parent, false);
                mirrorObject.transform.position = new Vector3(spec.X + 0.5f, spec.Y + 0.5f, 0f);

                SpriteRenderer sprite = Sprite(mirrorObject, MirrorSprite, Phase01Assets.SortingActors, -2);

                // A area onde o Scrap alcanca o espelho: do chao onde ele fica ate o espelho.
                float low = Mathf.Min(spec.Y, spec.StandY);
                float high = Mathf.Max(spec.Y, spec.StandY) + 1f;
                float left = Mathf.Min(spec.X, spec.StandX) - 0.6f;
                float right = Mathf.Max(spec.X, spec.StandX) + 1.6f;

                var reach = mirrorObject.AddComponent<BoxCollider2D>();
                reach.isTrigger = true;
                reach.size = new Vector2(right - left, high - low);
                reach.offset = new Vector2((left + right) * 0.5f - (spec.X + 0.5f), (low + high) * 0.5f - (spec.Y + 0.5f));

                // A face, onde a luz bate: pequena, no meio.
                var faceObject = new GameObject("Face");
                faceObject.transform.SetParent(mirrorObject.transform, false);
                var face = faceObject.AddComponent<BoxCollider2D>();
                face.isTrigger = true;
                face.size = new Vector2(0.45f, 0.45f);

                mirrorObject.AddComponent<SilverMirror>().Configurar(spec.Slash, face, sprite);

                Light2D shine = Phase01Generator.CreatePointLight(mirrorObject.transform, "Shine",
                    (spec.X + 0.5f, spec.Y + 0.5f), new Color(0.85f, 0.92f, 1f), 0.35f, 1.8f);

                // A setinha: o espelho gira no golpe.
                Phase01Generator.AddCue(mirrorObject, CueArt.Use, spec.Y + 1.2f, shine);
            }

            foreach (Phase04Layout.LockSpec spec in Phase04Layout.Locks)
            {
                var lockObject = new GameObject(spec.Name);
                lockObject.transform.SetParent(parent, false);
                lockObject.transform.position = new Vector3(spec.X + 0.5f, spec.Y + 0.5f, 0f);

                SpriteRenderer sprite = Sprite(lockObject, LockSprite, Phase01Assets.SortingTerrain, 23);
                if (spec.Hidden) lockObject.transform.localScale = Vector3.one * 0.75f;

                var trigger = lockObject.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(0.9f, 0.9f);

                // A escondida nao tem brilho nenhum: so a pedra clareando quando a luz chega.
                Light2D glow = spec.Hidden ? null : Phase01Generator.CreatePointLight(lockObject.transform, "Glow",
                    (spec.X + 0.5f, spec.Y + 0.5f), new Color(0.75f, 0.85f, 1f), 0.15f, 3f);

                BuiltGates.TryGetValue(spec.Gate, out Gate gate);
                lockObject.AddComponent<CrystalLock>().Configurar(gate, sprite, glow);
            }
        }

        // ------------------------------------------------------------------
        // Portoes, reagentes e o Grande Alambique
        // ------------------------------------------------------------------

        private static void BuildGates(Transform parent)
        {
            foreach (Phase04Layout.GateSpec spec in Phase04Layout.Gates)
            {
                var gateObject = new GameObject(spec.Name) { layer = _groundLayer };
                gateObject.transform.SetParent(parent, false);
                gateObject.transform.position = new Vector3(spec.X, spec.Y, 0f);

                // O disfarcado e um bloco de marmore igual a parede: so a fechadura escondida o denuncia.
                SpriteRenderer bars = Sprite(gateObject, spec.Disguised ? MarbleBlock : Phase02Assets.GateBars, Phase01Assets.SortingTerrain, 15);
                bars.drawMode = SpriteDrawMode.Tiled;
                bars.size = new Vector2(spec.Width, spec.Height);
                bars.color = spec.Disguised ? new Color(0.78f, 0.80f, 0.88f) : new Color(0.85f, 0.90f, 1f);

                var wall = gateObject.AddComponent<BoxCollider2D>();
                wall.size = new Vector2(spec.Width, spec.Height);
                wall.offset = new Vector2(spec.Width * 0.5f, spec.Height * 0.5f);

                var gate = gateObject.AddComponent<Gate>();
                float travel = spec.MoveX != 0 ? spec.Width + 1f : spec.Height + 1f;
                gate.Configurar(travel, new Vector2(spec.MoveX, spec.MoveY));
                BuiltGates[spec.Name] = gate;
            }
        }

        private static void BuildReagents(Transform parent)
        {
            foreach (Phase04Layout.Reagent reagent in Phase04Layout.Reagents)
            {
                var pickup = new GameObject(reagent.Id);
                pickup.transform.SetParent(parent, false);
                pickup.transform.position = new Vector3(reagent.X + 0.5f, reagent.Y + 1f, 0f);

                Sprite(pickup, LabArt.ReagentName(reagent.Index), Phase01Assets.SortingActors, -1);

                var trigger = pickup.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = 0.7f;

                pickup.AddComponent<KeyItemPickup>().Configurar(reagent.Id, reagent.Name + " obtido",
                    Phase04Layout.ReagentPrefix, Phase04Layout.Reagents.Count);
                pickup.AddComponent<Sway>();

                Light2D reagentGlow = Phase01Generator.CreatePointLight(pickup.transform, "Glow",
                    (reagent.X + 0.5f, reagent.Y + 1f), new Color(1f, 0.96f, 0.85f), 0.9f, 4f);
                Phase01Generator.AddCue(pickup, CueArt.Item, TopOf(pickup, reagent.Y + 1.6f), reagentGlow);
            }
        }

        /// <summary>O Grande Alambique: o desenho que acende encaixe por encaixe e a area onde se entrega.</summary>
        private static void BuildAlembic(Transform parent)
        {
            Phase04Layout.Prop prop = Phase04Layout.Props.Find(p => p.Sprite == "lab_great_alembic");
            (int ax, int ay) = Phase04Layout.AlembicTile;

            var alembicObject = new GameObject("GreatAlembic");
            alembicObject.transform.SetParent(parent, false);
            alembicObject.transform.position = new Vector3(prop.Sprite != null ? prop.X : ax + 1f, prop.Sprite != null ? prop.Y : ay, 0f);

            SpriteRenderer body = Sprite(alembicObject, LabArt.GreatAlembicName(0), Phase01Assets.SortingTerrain, 3);

            var areaObject = new GameObject("Sockets");
            areaObject.transform.SetParent(alembicObject.transform, false);
            areaObject.transform.position = new Vector3(ax + 0.5f, ay, 0f);

            var area = areaObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(5f, 3f);
            area.offset = new Vector2(0f, 1.5f);

            var stages = new Sprite[4];
            for (int lit = 0; lit <= 3; lit++) stages[lit] = Phase02Assets.LoadSprite(LabArt.GreatAlembicName(lit));

            BuiltGates.TryGetValue(Phase04Layout.AlembicGate, out Gate gate);
            areaObject.AddComponent<AlembicDoor>().Configurar(
                Phase04Layout.Reagents.Select(r => r.Id).ToArray(),
                Phase04Layout.Reagents.Select(r => r.Name).ToArray(),
                gate, body, stages);

            Light2D alembicGlow = Phase01Generator.CreatePointLight(alembicObject.transform, "Glow",
                (ax + 0.5f, ay + 3f), new Color(0.90f, 0.94f, 1f), 0.6f, 5f);

            // A setinha em cima do Grande Alambique: e nele que os reagentes vao.
            Phase01Generator.AddCue(alembicObject, CueArt.Use, TopOf(alembicObject, ay + 5f), alembicGlow);
        }

        private static void BuildSecrets(Transform parent)
        {
            // O que o vapor da Destilaria alcanca: um reforco de chassi.
            (int X, int Y) secret = Phase04Layout.SecretChassisTile;

            var pickup = new GameObject("Secret_Chassis");
            pickup.transform.SetParent(parent, false);
            pickup.transform.position = new Vector3(secret.X + 0.5f, secret.Y + 1f, 0f);

            Sprite(pickup, Phase02Assets.ChassisPlate, Phase01Assets.SortingActors, -1);

            var trigger = pickup.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            pickup.AddComponent<RewardPickup>().Configurar(RewardPickup.Kind.Chassi, "D4_Secret_Chassis");
            Phase01Generator.AddCue(pickup, CueArt.Item, TopOf(pickup, pickup.transform.position.y + 0.6f), null);
        }

        /// <summary>O alto do desenho de algo (em mundo), pra pendurar o marcador em cima.</summary>
        private static float TopOf(GameObject target, float fallback)
        {
            float top = float.MinValue;
            foreach (SpriteRenderer renderer in target.GetComponentsInChildren<SpriteRenderer>())
            {
                if (renderer.sprite != null && renderer.gameObject.name != "Cue") top = Mathf.Max(top, renderer.bounds.max.y);
            }

            return top > float.MinValue ? top : fallback;
        }

        /// <summary>
        /// A Grande Escadaria ate o palacio: os meios-degraus (caixas de meio
        /// tile, na camada do chao, que o Scrap sobe andando), o desenho dos
        /// lances e dos patamares e, no alto, a porta do palacio, que e a
        /// saida pro District 5.
        /// </summary>
        private static void BuildGrandStair(Transform parent)
        {
            var stair = new GameObject("GrandStair") { layer = _groundLayer };
            stair.transform.SetParent(parent, false);

            foreach ((int x, int y) in Phase04Layout.HalfSteps)
            {
                var step = stair.AddComponent<BoxCollider2D>();
                step.size = new Vector2(1f, 0.5f);
                step.offset = new Vector2(x + 0.5f, y + 0.25f);
            }

            foreach ((int x0, int steps, int floor) in Phase04Layout.Flights)
            {
                var flight = new GameObject($"Flight_{x0}");
                flight.transform.SetParent(stair.transform, false);
                flight.transform.position = new Vector3(x0, floor, 0f);
                Sprite(flight, StairFlightName(steps), Phase01Assets.SortingTerrain, 12);
            }

            foreach ((int x0, int x1, int y) in Phase04Layout.Landings)
            {
                var landing = new GameObject($"Landing_{x0}");
                landing.transform.SetParent(stair.transform, false);
                landing.transform.position = new Vector3(x0, y, 0f);
                Sprite(landing, StairLandingName(x1 - x0 + 1), Phase01Assets.SortingTerrain, 12);
            }

            // A porta do palacio, no alto, com a luz fria vazando.
            (int dx, int dy, int dw, int dh) = Phase04Layout.PalaceDoor;
            var door = new GameObject("PalaceDoor");
            door.transform.SetParent(stair.transform, false);
            door.transform.position = new Vector3(dx + dw * 0.5f, dy, 0f);
            Sprite(door, StairDoor, Phase01Assets.SortingTerrain, 3);
            Phase01Generator.CreatePointLight(door.transform, "PalaceLight", (dx + dw * 0.5f, dy + 2f), new Color(0.88f, 0.92f, 1f), 0.9f, 6f);

            SceneKit.CreateSceneExit(parent, "Exit_ToDistrict5", new RectInt(dx, dy, dw, dh),
                Phase04Layout.NextSceneName, Phase04Layout.ToDistrict5SpawnId);
        }
    }
}
