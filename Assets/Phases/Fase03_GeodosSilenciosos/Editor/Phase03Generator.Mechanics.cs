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
    public static partial class Phase03Generator
    {
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

                crystalObject.AddComponent<EchoCrystal>().Configurar(glow, sprite, 1.0f + spot.Size * 0.15f);
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

                plateObject.AddComponent<FragileCrystal>().Configurar(plate);

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
                echo.Configurar(glow, null, 1.6f);

                // Um resto de brilho na cor dele, pra dar pra achar no escuro.
                Light2D ember = Phase01Generator.CreatePointLight(crystalObject.transform, "Ember",
                    (spot.X + 0.5f, spot.Y + 1.2f), spot.Corrupted ? OxideGreen : color, 0.30f, 2.4f);

                // O verde dos corrompidos apaga com a purificacao.
                if (spot.Corrupted) OxideLights.Add(ember);

                var resonant = crystalObject.AddComponent<ResonantCrystal>();
                resonant.Configurar(spot.Note, color, spot.Corrupted, sprite, vein, echo);

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
                    resonant.ConfigurarTempoAceso(DrillPillarLitTime);
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

                barrierObject.AddComponent<CrystalBarrier>().Configurar("D3_" + name);

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
            rubbleObject.AddComponent<CrystalBarrier>().Configurar("D3_ElevatorRubble");

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
            ride.Configurar(Phase03Layout.ElevatorTopY, Phase04Layout.SceneName, Phase04Layout.FromDistrict3SpawnId);
            ride.ConfigurarBloqueio(rubbleObject, Phase03Layout.ElevatorBlockedNotice);

            // A chegada: a cabine parte do fundo do poco e sobe com o Scrap.
            var body = car.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            // O chao fica num filho na camada do terreno: o gatilho da cabine nao vira chao.
            var floorObject = new GameObject("Floor") { layer = _groundLayer };
            floorObject.transform.SetParent(car.transform, false);
            var floor = floorObject.AddComponent<BoxCollider2D>();
            floor.size = new Vector2(carWidth, 1f);
            floor.offset = new Vector2(carWidth * 0.5f, plate - 0.5f);

            car.AddComponent<ElevatorArrival>().Configurar(Phase03Layout.FromDistrict2SpawnId,
                Phase03Layout.ElevatorShaftBottom - plate, 6f);

            // Os trilhos do poco de baixo.
            var rails = new GameObject("ElevatorRailsBelow");
            rails.transform.SetParent(parent, false);
            rails.transform.position = new Vector3(carX, Phase03Layout.ElevatorShaftBottom, 0f);
            SpriteRenderer rail = Sprite(rails, Phase02Assets.ElevatorRail, Phase01Assets.SortingTerrain, 4);
            rail.drawMode = SpriteDrawMode.Tiled;
            rail.size = new Vector2(carWidth, carY + 3 - Phase03Layout.ElevatorShaftBottom);

            // O poco atravessa o mapa inteiro: sobe depressa.
            ride.ConfigurarVelocidade(14f);
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
            gate.Configurar(travel, new Vector2(spec.MoveX, spec.MoveY));
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

                doorObject.AddComponent<HarmonicDoor>().Configurar(
                    crystals != null ? crystals.ToArray() : new ResonantCrystal[0], sequence, gate, glyphs, spec.Hidden);
            }
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
                statue.Configurar(spot.Order, button, up, down, glow, light);
                statues.Add(statue);

                // A setinha em cima do botao da estatua: da pra acionar.
                Phase01Generator.AddCue(statueObject, CueArt.Use, TopOf(statueObject, spot.Y + 3f), null);
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

                keyhole.AddComponent<KeyGate>().Configurar(Phase03Layout.AncientKeyId, "A CHAVE ANTIGA", entrance, lockSprite);
                Phase01Generator.AddCue(keyhole, CueArt.Use, at.y + 3f, null);
            }
            else
            {
                Debug.LogWarning($"[Phase 3] O portao '{Phase03Layout.StatueEntranceGate}' do salao nao foi montado.");
            }

            BuiltGates.TryGetValue(Phase03Layout.StatueGate, out Gate gate);
            if (gate == null) Debug.LogWarning($"[Phase 3] O portao '{Phase03Layout.StatueGate}' das estatuas nao foi montado.");

            var frames = new Sprite[3];
            for (int frame = 0; frame < 3; frame++) frames[frame] = Phase02Assets.LoadSprite(CrackName(frame));

            parent.gameObject.AddComponent<StatueHall>().Configurar(
                statues.ToArray(), gate, cracks.ToArray(), crackArt.ToArray(), frames, pool.ToArray());
        }
    }
}
