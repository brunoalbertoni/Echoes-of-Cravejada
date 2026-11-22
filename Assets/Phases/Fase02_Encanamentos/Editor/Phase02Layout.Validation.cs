using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase02Layout
    {
        // ------------------------------------------------------------------
        // Validacao
        // ------------------------------------------------------------------

        /// <summary>O que ja mudou no mapa, enquanto a validacao joga a fase na cabeca.</summary>
        private sealed class WorldState
        {
            public LevelMap Map;
            public readonly HashSet<string> TurnedValves = new HashSet<string>();
            public readonly HashSet<string> BurstBoilers = new HashSet<string>();
            public readonly HashSet<string> LiveVents = new HashSet<string>();
            public bool BossDown;
            public bool Purified;

            public LevelMap.Lift[] Lifts()
            {
                var lifts = new List<LevelMap.Lift>();

                foreach (Vent vent in Vents)
                {
                    if (vent.Kind != VentKind.Lift || !LiveVents.Contains(vent.Name)) continue;
                    lifts.Add(new LevelMap.Lift(vent.Zone.X, vent.Zone.Right, vent.Zone.Y, vent.Zone.Top));
                }

                return lifts.ToArray();
            }

            public LevelMap.Jet[] Jets()
            {
                var jets = new List<LevelMap.Jet>();

                foreach (Vent vent in Vents)
                {
                    if (vent.Kind != VentKind.Jet || !LiveVents.Contains(vent.Name)) continue;
                    jets.Add(new LevelMap.Jet(vent.Zone.X, vent.Zone.Right, vent.Zone.Y, vent.Zone.Top, vent.Direction));
                }

                return jets.ToArray();
            }
        }

        private static LevelMap.Lift[] CloudAreas()
        {
            var clouds = new List<LevelMap.Lift>();

            foreach ((string _, Area area) in Clouds)
            {
                clouds.Add(new LevelMap.Lift(area.X, area.Right, area.Y, area.Top));
            }

            return clouds.ToArray();
        }

        /// <summary>
        /// As regras de alcance desta fase: as com folga, mais o Steam Dash, que
        /// soma uns 3 tiles a um pulo.
        /// </summary>
        private static LevelMap.JumpRules Rules()
        {
            LevelMap.JumpRules rules = LevelMap.JumpRules.Comfortable();
            rules.MaxRun += 3;
            return rules;
        }

        /// <summary>
        /// Depois do boss: o Salto de Pressao, um segundo pulo no ar. Conta como
        /// no District 3 — sobe 4 tiles e vai uns 9 de lado, com o dash.
        /// </summary>
        private static LevelMap.JumpRules RulesAfterBoss()
        {
            LevelMap.JumpRules rules = LevelMap.JumpRules.Comfortable();
            rules.MaxRise = 4;
            rules.MaxRun += 5;
            return rules;
        }

        private static void Open(LevelMap map, Area area)
        {
            map.Fill(area.X, area.Y, area.Right, area.Top, LevelMap.Air);
        }

        /// <summary>
        /// Joga a fase na cabeca: explora, gira toda valvula e explode toda
        /// caldeira que alcancar, e repete com o mapa mudado, ate nada mais
        /// mudar. Devolve o estado final e o alcance nele.
        /// </summary>
        private static WorldState Play(out LevelMap.Reach reach, List<string> log, bool bossFalls = true,
            Func<string, bool> allowRoom = null, Action<WorldState, LevelMap.Reach, LevelMap.JumpRules> visit = null)
        {
            // Com allowRoom, so se mexe nas valvulas e caldeiras das salas
            // permitidas: e assim que se joga uma ala sem as outras.
            bool Allowed(string room) => allowRoom == null || allowRoom(room);

            var world = new WorldState { Map = Map.Clone() };

            // No comeco, portoes e paredes rachadas estao fechados, e so o vapor
            // que nao depende de valvula esta ligado.
            foreach (GateSpec gate in Gates)
            {
                world.Map.Fill(gate.Area.X, gate.Area.Y, gate.Area.Right, gate.Area.Top, LevelMap.Rock);
            }

            foreach ((string _, Area area) in BreakableWalls)
            {
                world.Map.Fill(area.X, area.Y, area.Right, area.Top, LevelMap.Rock);
            }

            // A nevoa densa e parede ate a purificacao.
            foreach ((string _, Area area) in Veils)
            {
                world.Map.Fill(area.X, area.Y, area.Right, area.Top, LevelMap.Rock);
            }

            foreach (Vent vent in Vents)
            {
                if (!vent.StartsOff) world.LiveVents.Add(vent.Name);
            }

            LevelMap.Lift[] clouds = CloudAreas();

            while (true)
            {
                // Depois do boss o Scrap tem o Salto de Pressao.
                LevelMap.JumpRules rules = world.BossDown ? RulesAfterBoss() : Rules();
                reach = world.Map.Explore(SpawnTile, rules, world.Lifts(), world.Jets(), clouds);
                visit?.Invoke(world, reach, rules);
                bool changed = false;

                foreach (ValveSpec valve in Valves)
                {
                    if (world.TurnedValves.Contains(valve.Name)) continue;
                    if (!Allowed(valve.Group)) continue;
                    if (!reach.CanStand(valve.StandX, valve.StandY)) continue;

                    world.TurnedValves.Add(valve.Name);
                    changed = true;
                    log?.Add($"gira {valve.Name}");

                    foreach (string vent in valve.Vents) world.LiveVents.Add(vent);

                    foreach (string pool in valve.Pools)
                    {
                        foreach ((string name, Area area) in AcidPools)
                        {
                            if (name != pool) continue;

                            // Seca: o que era acido vira ar. O que era degrau continua degrau.
                            for (int y = area.Y; y <= area.Top; y++)
                            {
                                for (int x = area.X; x <= area.Right; x++)
                                {
                                    if (world.Map.IsAcid(x, y)) world.Map.Set(x, y, LevelMap.Air);
                                }
                            }
                        }
                    }

                    foreach (string gateName in valve.Gates)
                    {
                        foreach (GateSpec gate in Gates)
                        {
                            if (gate.Name == gateName) Open(world.Map, gate.Area);
                        }
                    }
                }

                foreach (BoilerSpec boiler in PressureBoilers)
                {
                    if (world.BurstBoilers.Contains(boiler.Name)) continue;
                    if (!Allowed(boiler.Group)) continue;
                    if (!reach.CanStand(boiler.StandX, boiler.StandY)) continue;

                    world.BurstBoilers.Add(boiler.Name);
                    changed = true;
                    log?.Add($"explode {boiler.Name}");

                    foreach (string wallName in boiler.Walls)
                    {
                        foreach ((string name, Area area) in BreakableWalls)
                        {
                            if (name == wallName) Open(world.Map, area);
                        }
                    }
                }

                if (bossFalls && !world.BossDown && reach.CanStand(BossTile.X, BossTile.Y))
                {
                    world.BossDown = true;
                    changed = true;
                    log?.Add("derrota o Boiler Master");

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Opener == GateOpener.Boss) Open(world.Map, gate.Area);
                    }

                    // A queda dele purifica o distrito na hora.
                    world.Purified = true;
                    log?.Add("purifica o distrito");

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Opener == GateOpener.Purification) Open(world.Map, gate.Area);
                    }

                    foreach ((string _, Area area) in Veils) Open(world.Map, area);
                }

                if (!changed) return world;
            }
        }

        /// <summary>
        /// Confere se a fase e jogavel de verdade, sem precisar jogar.
        ///
        /// Simula a fase inteira (ver <see cref="Play"/>): parte de onde o
        /// Scrap chega, com tudo fechado, e so abre o que ele consegue alcancar
        /// pra abrir. No fim, toda valvula tem que ter sido girada, toda
        /// caldeira explodida, o boss alcancado, o distrito purificado e todo
        /// marcador visitado. E, do fim do mapa, tem que dar pra voltar ao comeco.
        ///
        /// Devolve a lista de problemas. Vazia quer dizer que esta tudo certo.
        /// </summary>
        public static List<string> Validate()
        {
            var problems = new List<string>();

            // O mapa e fechado: se a borda tiver ar, a camera mostra o vazio.
            for (int x = 0; x < Width; x++)
            {
                if (!Map.IsRock(x, 0) || !Map.IsRock(x, Height - 1))
                {
                    problems.Add($"A coluna {x} tem buraco no chao ou no teto do mapa.");
                }
            }

            for (int y = 0; y < Height; y++)
            {
                if (!Map.IsRock(0, y) || !Map.IsRock(Width - 1, y))
                {
                    problems.Add($"A linha {y} tem buraco numa das paredes do mapa.");
                }
            }

            if (!Map.IsStandable(SpawnTile.X, SpawnTile.Y))
            {
                problems.Add($"O Scrap chega em ({SpawnTile.X}, {SpawnTile.Y}) e nao ha chao ali.");
                return problems;
            }

            WorldState world = Play(out LevelMap.Reach reach, null);

            foreach (ValveSpec valve in Valves)
            {
                if (!world.Map.IsStandable(valve.StandX, valve.StandY))
                {
                    problems.Add($"{valve.Name}: ({valve.StandX}, {valve.StandY}) nao e um lugar onde da pra ficar em pe pra raspar.");
                }
                else if (!world.TurnedValves.Contains(valve.Name))
                {
                    problems.Add($"{valve.Name} ({valve.StandX}, {valve.StandY}) nunca e alcancada.");
                }

                foreach (string vent in valve.Vents)
                {
                    if (!Vents.Exists(candidate => candidate.Name == vent)) problems.Add($"{valve.Name} liga '{vent}', que nao existe.");
                }

                foreach (string pool in valve.Pools)
                {
                    if (!AcidPools.Exists(candidate => candidate.Name == pool)) problems.Add($"{valve.Name} seca '{pool}', que nao existe.");
                }

                foreach (string gate in valve.Gates)
                {
                    if (!Gates.Exists(candidate => candidate.Name == gate)) problems.Add($"{valve.Name} abre '{gate}', que nao existe.");
                }
            }

            foreach (Vent vent in Vents)
            {
                if (vent.StartsOff && !world.LiveVents.Contains(vent.Name))
                {
                    problems.Add($"{vent.Name} comeca desligado e nenhuma valvula alcancavel o liga.");
                }

                if (!vent.StartsOff) continue;

                // Uma coluna desligada tem que ter a valvula a vista: na mesma
                // sala. Se a valvula fica longe, ela avisa no HUD o que ligou.
                string ventRoom = RoomAt(vent.Zone.X, vent.Zone.Y);

                foreach (ValveSpec valve in Valves)
                {
                    if (Array.IndexOf(valve.Vents, vent.Name) < 0) continue;
                    if (valve.Group == ventRoom || !string.IsNullOrEmpty(valve.Notice)) continue;

                    problems.Add($"{vent.Name} ({ventRoom}) so liga pela {valve.Name}, em {valve.Group}, longe e sem aviso.");
                }
            }

            foreach (GateSpec gate in Gates)
            {
                if (gate.Opener == GateOpener.Valve && !Valves.Exists(valve => Array.IndexOf(valve.Gates, gate.Name) >= 0))
                {
                    problems.Add($"{gate.Name} espera uma valvula, e nenhuma o abre.");
                }
            }

            foreach (BoilerSpec boiler in PressureBoilers)
            {
                if (!world.BurstBoilers.Contains(boiler.Name))
                {
                    problems.Add($"{boiler.Name} ({boiler.StandX}, {boiler.StandY}) nunca e alcancada.");
                }

                foreach (string wall in boiler.Walls)
                {
                    if (!BreakableWalls.Exists(candidate => candidate.Name == wall)) problems.Add($"{boiler.Name} derruba '{wall}', que nao existe.");
                }
            }

            if (!world.BossDown) problems.Add("Nao da pra chegar no Boiler Master.");

            // A saida e so pela sala do boss: sem derrota-lo, nada da fase
            // (nem caminho alternativo) chega no tunel da saida.
            Play(out LevelMap.Reach withoutBoss, null, bossFalls: false);
            if (withoutBoss.CanStand(District3Tile.X, District3Tile.Y))
            {
                problems.Add("Da pra chegar na saida pros Silent Geodes sem passar pelo Boiler Master.");
            }

            CheckArena(world, problems);
            CheckQuietSpots(reach, problems);
            CheckEveryRoomPays(problems);
            CheckPools(problems);
            CheckNoTraps(problems);
            CheckWings(problems);
            CheckReturnAfterBoss(world, problems);
            CheckBrokenStairway(world, problems);

            int gems = Rewards.FindAll(reward => reward.Kind == RewardKind.Gem).Count;
            foreach (LoreSpec spec in Lore)
            {
                if (spec.Quest != null) gems += spec.Quest.GemReward;
            }

            if (gems != 2) problems.Add($"A fase tem {gems} gemas; tem que ter exatamente 2. Gema e rara.");

            // O mirante da Galeria Alta e o desafio de depois do boss: sem o
            // Salto de Pressao, nao se chega nele.
            LevelMap.Reach withoutJump = world.Map.Explore(SpawnTile, Rules(), world.Lifts(), world.Jets(), CloudAreas());
            foreach (RewardSpec reward in Rewards)
            {
                if (reward.Id == "D2_Gallery_Cache" && withoutJump.CanStand(reward.X, reward.Y))
                {
                    problems.Add("Da pra chegar no mirante da Galeria Alta sem o Salto de Pressao: o desafio nao cobra nada.");
                }
            }

            foreach (Marker marker in Markers)
            {
                if (!marker.MustReach) continue;

                if (!world.Map.IsStandable(marker.X, marker.Y))
                {
                    problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) nao e um lugar onde da pra ficar em pe.");
                }
                else if (!reach.CanStand(marker.X, marker.Y))
                {
                    problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) esta fora de alcance.");
                }
            }

            if (!reach.CanStand(CrisolTile.X, CrisolTile.Y)) problems.Add("Nao da pra chegar em Crisol.");

            foreach (EnemySpec enemy in Enemies)
            {
                if (enemy.Kind == EnemyKind.ValveSentry)
                {
                    if (!Map.IsStandable(enemy.X0, enemy.Y)) problems.Add($"{enemy.Name} esta em ({enemy.X0}, {enemy.Y}), que nao e chao.");
                    continue;
                }

                for (int x = enemy.X0; x <= enemy.X1; x++)
                {
                    if (!Map.IsStandable(x, enemy.Y)) problems.Add($"{enemy.Name} anda por ({x}, {enemy.Y}), que nao e chao.");
                }
            }

            // A volta: do fim do mapa ate onde se chega. A fase nao pode prender ninguem.
            if (Map.IsStandable(District3Tile.X, District3Tile.Y))
            {
                LevelMap.Reach backward = world.Map.Explore(District3Tile, Rules(),
                    world.Lifts(), world.Jets(), CloudAreas());

                if (!backward.CanStand(SpawnTile.X, SpawnTile.Y))
                {
                    problems.Add("Saindo do fim do mapa nao da pra voltar ate a Galeria de Transicao.");
                }
            }

            foreach (Vent vent in Vents)
            {
                for (int y = vent.Zone.Y; y <= vent.Zone.Top; y++)
                {
                    for (int x = vent.Zone.X; x <= vent.Zone.Right; x++)
                    {
                        if (Map.IsSolid(x, y)) problems.Add($"{vent.Name} passa por dentro de terreno em ({x}, {y}).");
                    }
                }

                if (vent.Zone.Contains(SpawnTile.X, SpawnTile.Y)) problems.Add($"O Scrap chega dentro de {vent.Name}.");
            }

            foreach (Fixture fixture in Fixtures)
            {
                if (fixture.X < 0 || fixture.X >= Width || fixture.Y < 0 || fixture.Y >= Height)
                {
                    problems.Add($"{fixture.Name} esta fora do mapa, em ({fixture.X}, {fixture.Y}).");
                }

                // Decoracao nao flutua: lampiao pende de algo, e o que fica no
                // chao esta apoiado nele.
                switch (fixture.Kind)
                {
                    case FixtureKind.Lamp:
                        if (!Map.IsSolid(fixture.X, fixture.Y + 2) || Map.IsRock(fixture.X, fixture.Y))
                        {
                            problems.Add($"{fixture.Name} ({fixture.X}, {fixture.Y}) nao esta pendurado em nada.");
                        }
                        break;

                    case FixtureKind.Locker:
                    case FixtureKind.Barrel:
                    case FixtureKind.Boiler:
                    case FixtureKind.Furnace:
                        if (!Map.IsSolid(fixture.X, fixture.Y - 1) || Map.IsRock(fixture.X, fixture.Y))
                        {
                            problems.Add($"{fixture.Name} ({fixture.X}, {fixture.Y}) nao esta apoiado no chao.");
                        }
                        break;
                }
            }

            CheckDecorPhysics(problems);
            return problems;
        }

        /// <summary>O tamanho, em tiles, que cada objeto de cenario ocupa (largura, altura).</summary>
        public static (int W, int H) FixtureSize(Fixture fixture)
        {
            switch (fixture.Kind)
            {
                case FixtureKind.Boiler: return (4, 6);
                case FixtureKind.GaugeBig: return (2, 2);
                case FixtureKind.GaugeSmall: return (1, 1);
                case FixtureKind.Valve: return (2, 2);
                case FixtureKind.Furnace: return (2, 2);
                case FixtureKind.Hatch: return (2, 3);
                case FixtureKind.Workbench: return (3, 2);
                case FixtureKind.NozzleUp: return (fixture.Size, 1);
                case FixtureKind.NozzleSide: return (1, fixture.Size);
                case FixtureKind.Lamp: return (1, 2);
                case FixtureKind.Locker: return (1, 2);
                case FixtureKind.MaskRack: return (2, 2);
                case FixtureKind.Shelf: return (2, 2);
                case FixtureKind.Barrel: return (1, 2);
                case FixtureKind.Railing: return (fixture.Size, 1);
                case FixtureKind.Poster: return (2, 2);
                case FixtureKind.Sign: return (3, 1);
                case FixtureKind.Chain: return (1, fixture.Size);
                case FixtureKind.ElevatorRail: return (3, fixture.Size);
                case FixtureKind.Crate: return (1, 1);
                case FixtureKind.Toolbox: return (1, 1);
                case FixtureKind.Pallet: return (2, 1);
                case FixtureKind.CoalPile: return (2, 1);
                case FixtureKind.Sack: return (1, 1);
                case FixtureKind.Cart: return (2, 2);
                default: return (1, 1);
            }
        }

        /// <summary>
        /// A fisica de quem esta parado. O que fica no chao tem apoio embaixo
        /// de toda a largura e nao entra na rocha; o que pende (lampiao,
        /// corrente) sai do teto e desce por ar; o que fica na parede nao entra
        /// na rocha; e duas pecas da mesma camada nao ocupam o mesmo lugar.
        /// </summary>
        private static void CheckDecorPhysics(List<string> problems)
        {
            var occupied = new Dictionary<(int, int), string>();

            foreach (Fixture fixture in Fixtures)
            {
                (int w, int h) = FixtureSize(fixture);
                bool grounded = fixture.Kind == FixtureKind.Locker || fixture.Kind == FixtureKind.Barrel
                    || fixture.Kind == FixtureKind.Boiler || fixture.Kind == FixtureKind.Shelf
                    || fixture.Kind == FixtureKind.Workbench || fixture.Kind == FixtureKind.Crate
                    || fixture.Kind == FixtureKind.Toolbox || fixture.Kind == FixtureKind.Pallet
                    || fixture.Kind == FixtureKind.CoalPile || fixture.Kind == FixtureKind.Sack
                    || fixture.Kind == FixtureKind.Cart || fixture.Kind == FixtureKind.Railing;

                bool stackable = fixture.Kind == FixtureKind.Crate || fixture.Kind == FixtureKind.Sack
                    || fixture.Kind == FixtureKind.Toolbox;

                if (grounded)
                {
                    for (int x = fixture.X; x < fixture.X + w; x++)
                    {
                        bool onCrate = stackable && occupied.TryGetValue((x, fixture.Y - 1), out string under)
                            && (under.StartsWith("Crate") || under.StartsWith("Pallet"));
                        if (!Map.IsSolid(x, fixture.Y - 1) && !onCrate)
                        {
                            problems.Add($"{fixture.Name} ({fixture.X}, {fixture.Y}) tem a base no ar em x={x}.");
                            break;
                        }
                    }
                }

                // Nada entra na rocha. Os canos-bocal e os trilhos sim: saem dela.
                if (fixture.Kind != FixtureKind.NozzleUp && fixture.Kind != FixtureKind.NozzleSide
                    && fixture.Kind != FixtureKind.ElevatorRail)
                {
                    for (int y = fixture.Y; y < fixture.Y + h; y++)
                    {
                        for (int x = fixture.X; x < fixture.X + w; x++)
                        {
                            if (fixture.Kind == FixtureKind.Lamp && y == fixture.Y + h) continue;
                            if (Map.IsRock(x, y))
                            {
                                problems.Add($"{fixture.Name} ({fixture.X}, {fixture.Y}) entra na rocha em ({x}, {y}).");
                                y = int.MaxValue - 1;
                                break;
                            }
                        }
                    }
                }

                if (fixture.Kind == FixtureKind.Chain)
                {
                    if (!Map.IsSolid(fixture.X, fixture.Y + fixture.Size))
                    {
                        problems.Add($"{fixture.Name} ({fixture.X}, {fixture.Y}) nao sai do teto.");
                    }
                }

                // Pecas do chao e da parede que se sobrepoem. Lampioes,
                // correntes, guarda-corpos e bocais ficam de fora: passam por
                // cima de tudo de proposito.
                if (fixture.Kind == FixtureKind.Lamp || fixture.Kind == FixtureKind.Chain
                    || fixture.Kind == FixtureKind.Railing || fixture.Kind == FixtureKind.NozzleUp
                    || fixture.Kind == FixtureKind.NozzleSide || fixture.Kind == FixtureKind.ElevatorRail)
                {
                    continue;
                }

                for (int y = fixture.Y; y < fixture.Y + h; y++)
                {
                    for (int x = fixture.X; x < fixture.X + w; x++)
                    {
                        if (occupied.TryGetValue((x, y), out string other))
                        {
                            problems.Add($"{fixture.Name} ({fixture.X}, {fixture.Y}) fica em cima de {other}.");
                            y = int.MaxValue - 1;
                            break;
                        }

                        occupied[(x, y)] = fixture.Name;
                    }
                }
            }

            // Os tiles de cenario do pack (caixote, pedra, cerca) tambem
            // assentam: em chao ou em outro caixote.
            var propCells = new HashSet<(int, int)>();
            foreach (Prop prop in Props) propCells.Add((prop.X, prop.Y));

            foreach (Prop prop in Props)
            {
                if (prop.Layer == PropLayer.Glow) continue;
                if (!prop.Tile.StartsWith("misc_") && !prop.Tile.StartsWith("bgobj_")) continue;
                if (Map.IsRock(prop.X, prop.Y))
                {
                    problems.Add($"O tile {prop.Tile} em ({prop.X}, {prop.Y}) esta dentro da rocha.");
                    continue;
                }

                if (!Map.IsSolid(prop.X, prop.Y - 1) && !propCells.Contains((prop.X, prop.Y - 1)))
                {
                    problems.Add($"O tile {prop.Tile} em ({prop.X}, {prop.Y}) esta flutuando.");
                }
            }
        }

        /// <summary>
        /// A arena do Mestre das Caldeiras, como a do Coletor no District 1:
        /// durante a luta (o selo da entrada e a grade da saida fechados) nao
        /// se sai dela; as plataformas se alcancam do chao; e nao ha patamar na
        /// altura da cabeca dele fora de onde o corpo dele passa.
        /// </summary>
        private static void CheckArena(WorldState world, List<string> problems)
        {
            LevelMap fight = world.Map.Clone();

            Area seal = ArenaSeal;
            fight.Fill(seal.X, seal.Y, seal.Right, seal.Top, LevelMap.Rock);

            foreach (GateSpec gate in Gates)
            {
                if (gate.Name == BossGate) fight.Fill(gate.Area.X, gate.Area.Y, gate.Area.Right, gate.Area.Top, LevelMap.Rock);
            }

            LevelMap.Reach inFight = fight.Explore(BossTile, Rules());
            Area arena = ArenaInside;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (!arena.Contains(x, y) && inFight.CanStand(x, y))
                    {
                        problems.Add($"Durante a luta da pra sair da arena: ({x}, {y}).");
                        return;
                    }
                }
            }

            int floor = BossTile.Y;
            float reachLeft = BossRange.X0 + 0.5f - BossHalfWidth;
            float reachRight = BossRange.X1 + 0.5f + BossHalfWidth;

            for (int y = floor + 1; y <= floor + 3; y++)
            {
                for (int x = arena.X; x <= arena.Right; x++)
                {
                    bool underBoss = x + 1f > reachLeft && x < reachRight;
                    if (fight.IsStandable(x, y) && !underBoss)
                    {
                        problems.Add($"Ha um patamar na altura da cabeca do Mestre das Caldeiras, fora do alcance dele, em ({x}, {y}).");
                    }
                }
            }

            foreach (LevelMap.Perch perch in fight.FindPerches())
            {
                if (perch.Y <= floor + 3 || !arena.Contains(perch.X, perch.Y) || !arena.Contains(perch.Right, perch.Y)) continue;

                bool reached = false;
                for (int x = perch.X; x <= perch.Right && !reached; x++) reached = inFight.CanStand(x, perch.Y);

                if (!reached) problems.Add($"A plataforma da arena em {perch} nao se alcanca do chao durante a luta.");
            }
        }

        /// <summary>
        /// Os lugares onde o Scrap para pra fazer alguma coisa: raspar uma
        /// valvula, ler uma lore, falar com alguem, pegar uma recompensa,
        /// sentar na bancada. Nenhum desses pode ficar na linha de tiro de uma
        /// sentinela — ninguem le uma carta levando jato de vapor nas costas.
        /// Quem anda pode passar por eles (todos morrem); so as bancadas e o
        /// Crisol ficam fora do caminho.
        /// </summary>
        private static List<(string Name, int X, int Y)> QuietSpots()
        {
            var spots = new List<(string, int, int)>();

            foreach (ValveSpec valve in Valves)
            {
                // As da arena sao raspadas no meio da luta, de proposito.
                if (!valve.Arena) spots.Add((valve.Name, valve.StandX, valve.StandY));
            }

            // Quem so aparece depois da purificacao nao conta: os corrompidos
            // somem com ela.
            foreach (LoreSpec spec in Lore)
            {
                if (!spec.AfterPurification) spots.Add((spec.Name, spec.X, spec.Y));
            }
            foreach (RewardSpec spec in Rewards) spots.Add((spec.Id, spec.X, spec.Y));
            foreach ((string name, int x, int y) in Workbenches) spots.Add((name, x + 1, y));
            spots.Add(("Crisol", CrisolTile.X, CrisolTile.Y));
            return spots;
        }

        /// <summary>Se o lugar calmo e de descanso (bancada ou Crisol), e nao so de parar.</summary>
        private static bool RestSpot(string name)
        {
            if (name == "Crisol") return true;
            foreach (var (bench, _, _) in Workbenches)
            {
                if (bench == name) return true;
            }

            return false;
        }

        /// <summary>
        /// Confere os lugares calmos (ver <see cref="QuietSpots"/>) e que toda
        /// sentinela pode ser morta: tem chao do lado dela onde o Scrap alcanca.
        /// </summary>
        private static void CheckQuietSpots(LevelMap.Reach reach, List<string> problems)
        {
            const int margin = 2;

            foreach ((string name, int x, int y) in QuietSpots())
            {
                foreach (EnemySpec enemy in Enemies)
                {
                    if (enemy.Kind == EnemyKind.ValveSentry)
                    {
                        // O jato sai na altura do peito: pega quem esta no
                        // mesmo chao, ou um degrau abaixo. Para na rocha.
                        if (y != enemy.Y && y != enemy.Y - 1) continue;

                        int step = enemy.Direction > 0 ? 1 : -1;
                        bool hit = Math.Abs(x - enemy.X0) <= 1;

                        for (int i = 1; i <= enemy.Range && !hit; i++)
                        {
                            int cx = enemy.X0 + step * i;
                            if (Map.IsRock(cx, enemy.Y)) break;
                            if (cx == x) hit = true;
                        }

                        if (hit) problems.Add($"{name} ({x}, {y}) fica na linha de tiro de {enemy.Name}.");
                        continue;
                    }

                    // Quem anda (ou cai do teto e anda) usa o chao inteiro: todos
                    // morrem, entao podem passar por valvula, lore e recompensa.
                    // So as bancadas e o Crisol ficam fora: ali o Scrap descansa e compra.
                    if (!RestSpot(name)) continue;
                    if (y == enemy.Y && x >= enemy.X0 - margin && x <= enemy.X1 + margin)
                    {
                        problems.Add($"{name} ({x}, {y}) fica no caminho de {enemy.Name}.");
                    }
                }
            }

            // E quem anda nao atravessa sentinela, caldeira de pressao, portao
            // nem parede rachada, e comeca dentro do proprio trecho.
            foreach (EnemySpec enemy in Enemies)
            {
                if (enemy.Kind == EnemyKind.ValveSentry) continue;

                if (enemy.StartX >= 0 && (enemy.StartX < enemy.X0 || enemy.StartX > enemy.X1))
                {
                    problems.Add($"{enemy.Name} comeca em {enemy.StartX}, fora do trecho dele ({enemy.X0} a {enemy.X1}).");
                }

                foreach (EnemySpec sentry in Enemies)
                {
                    if (sentry.Kind != EnemyKind.ValveSentry || sentry.Y != enemy.Y) continue;
                    if (sentry.X0 >= enemy.X0 - 1 && sentry.X0 <= enemy.X1 + 1)
                    {
                        problems.Add($"{enemy.Name} anda por cima de {sentry.Name}.");
                    }
                }

                foreach (BoilerSpec boiler in PressureBoilers)
                {
                    bool level = enemy.Y >= boiler.Y && enemy.Y < boiler.Y + 6;
                    if (level && boiler.X <= enemy.X1 && boiler.X + 3 >= enemy.X0)
                    {
                        problems.Add($"{enemy.Name} atravessa a caldeira {boiler.Name}.");
                    }
                }

                var closed = new List<(string Name, Area Area)>(BreakableWalls);
                foreach (GateSpec gate in Gates) closed.Add((gate.Name, gate.Area));
                foreach ((string closedName, Area area) in closed)
                {
                    bool level = enemy.Y >= area.Y - 1 && enemy.Y <= area.Top;
                    if (level && area.X <= enemy.X1 && area.Right >= enemy.X0)
                    {
                        problems.Add($"{enemy.Name} atravessa {closedName}.");
                    }
                }
            }

            foreach (EnemySpec enemy in Enemies)
            {
                if (enemy.Kind != EnemyKind.ValveSentry) continue;

                bool killable = reach.CanStand(enemy.X0 - 1, enemy.Y) || reach.CanStand(enemy.X0 + 1, enemy.Y);

                if (!killable) problems.Add($"{enemy.Name} ({enemy.X0}, {enemy.Y}) nao tem onde o Scrap pare do lado pra bater nela.");
            }
        }

        /// <summary>
        /// Toda poca de acido tem que estar encaixada: so acido por dentro (o
        /// objeto da poca desenha e queima o retangulo inteiro), parede dos
        /// dois lados e chao embaixo. Senao o acido aparece — e machuca — em
        /// cima de onde se pisa, ou fica pendurado no ar.
        /// </summary>
        private static void CheckPools(List<string> problems)
        {
            foreach ((string name, Area area) in AcidPools)
            {
                bool full = true;
                bool sides = true;
                bool floor = true;

                for (int y = area.Y; y <= area.Top; y++)
                {
                    for (int x = area.X; x <= area.Right; x++)
                    {
                        if (!Map.IsAcid(x, y)) full = false;
                    }

                    if (!Map.IsSolid(area.X - 1, y) || !Map.IsSolid(area.Right + 1, y)) sides = false;
                }

                for (int x = area.X; x <= area.Right; x++)
                {
                    if (!Map.IsSolid(x, area.Y - 1)) floor = false;
                }

                if (!full) problems.Add($"{name}: o retangulo da poca tem terreno dentro; o acido vai aparecer por cima dele.");
                if (!sides) problems.Add($"{name}: falta parede de um dos lados da poca.");
                if (!floor) problems.Add($"{name}: falta chao embaixo da poca.");
            }
        }

        /// <summary>
        /// Nenhuma armadilha: em cada momento da fase (a cada valvula girada),
        /// de todo chao alcancavel da pra voltar ate Crisol sem girar mais nada.
        /// Coluna que so sobe, alcapao que so desce e portao que so abre no fim
        /// nao podem prender ninguem. So a arena fica de fora: durante a luta ela
        /// tranca de proposito (ver CheckArena).
        /// </summary>
        private static void CheckNoTraps(List<string> problems)
        {
            Area[] exempt =
            {
                ArenaInside,
            };

            var reported = new HashSet<string>();

            Play(out _, null, true, null, (world, reach, rules) =>
            {
                // Antes de chegar no centro, nao ha pra onde voltar ainda.
                if (!reach.CanStand(CrisolTile.X, CrisolTile.Y)) return;

                LevelMap.Lift[] lifts = world.Lifts();
                LevelMap.Jet[] jets = world.Jets();
                LevelMap.Lift[] clouds = CloudAreas();

                foreach (LevelMap.Perch perch in world.Map.FindPerches())
                {
                    int x = -1;
                    for (int cx = perch.X; cx <= perch.Right; cx++)
                    {
                        if (reach.CanStand(cx, perch.Y)) { x = cx; break; }
                    }

                    if (x < 0) continue;

                    bool skip = false;
                    foreach (Area area in exempt) skip |= area.Contains(x, perch.Y);
                    if (skip) continue;

                    string room = RoomAt(x, perch.Y) ?? "fora das salas";
                    string key = $"{room}:{perch}";
                    if (reported.Contains(key)) continue;

                    LevelMap.Reach back = world.Map.Explore((x, perch.Y), rules, lifts, jets, clouds);
                    if (back.CanStand(CrisolTile.X, CrisolTile.Y)) continue;

                    reported.Add(key);
                    problems.Add($"Armadilha em {room}, {perch}: dali nao da pra voltar pra Cisterna sem girar mais nada.");
                }
            });
        }


        /// <summary>Toda sala que pede esforco paga: lore, alguem pra ouvir, ou recompensa.</summary>
        private static void CheckEveryRoomPays(List<string> problems)
        {
            foreach ((string name, Area area) in Rooms)
            {
                if (Array.IndexOf(PassageRooms, name) >= 0) continue;

                bool pays = Lore.Exists(spec => area.Contains(spec.X, spec.Y))
                    || Rewards.Exists(spec => area.Contains(spec.X, spec.Y))
                    || area.Contains(CrisolTile.X, CrisolTile.Y);

                if (!pays) problems.Add($"A sala {name} nao tem nada pra achar: nem lore, nem NPC, nem recompensa.");
            }
        }

        /// <summary>
        /// A ordem livre das alas: cada uma se completa partindo do centro sem
        /// nada das outras duas, e o boss nao se alcanca se faltar qualquer uma.
        /// </summary>
        private static void CheckWings(List<string> problems)
        {
            foreach ((string wing, string[] _) in Wings)
            {
                List<ValveSpec> locks = Valves.FindAll(valve => valve.Lock && WingOf(valve.Group) == wing);
                if (locks.Count != 1) problems.Add($"A ala {wing} tem {locks.Count} valvulas de trava; tem que ter uma.");

                // Sozinha: so a entrada, o centro e esta ala.
                WorldState alone = Play(out _, null, false, room => WingOf(room) == null || WingOf(room) == wing);

                foreach (ValveSpec valve in Valves)
                {
                    if (WingOf(valve.Group) == wing && !alone.TurnedValves.Contains(valve.Name))
                    {
                        problems.Add($"Ala {wing} feita sozinha: {valve.Name} nao se alcanca sem outra ala.");
                    }
                }

                foreach (BoilerSpec boiler in PressureBoilers)
                {
                    if (WingOf(boiler.Group) == wing && !alone.BurstBoilers.Contains(boiler.Name))
                    {
                        problems.Add($"Ala {wing} feita sozinha: {boiler.Name} nao se alcanca sem outra ala.");
                    }
                }

                // Sem ela: o boss tem que ficar fora de alcance.
                WorldState without = Play(out _, null, true, room => WingOf(room) != wing);
                if (without.BossDown) problems.Add($"Da pra chegar no Boiler Master sem fazer a ala {wing}.");
            }

            int lockCount = Valves.FindAll(valve => valve.Lock).Count;
            if (lockCount != Wings.Length) problems.Add($"Ha {lockCount} valvulas de trava; tem que haver {Wings.Length}.");
        }

        /// <summary>
        /// Depois do boss, da saida (e da Galeria Alta, por cima dela) tem que
        /// dar pra voltar pra Praca e pra Cisterna: a Solda e as falas novas
        /// ficam la.
        /// </summary>
        private static void CheckReturnAfterBoss(WorldState world, List<string> problems)
        {
            LevelMap.Reach back = world.Map.Explore(District3Tile, RulesAfterBoss(), world.Lifts(), world.Jets(), CloudAreas());

            LoreSpec solda = Lore.Find(spec => spec.Name == "Npc_Solda");
            if (solda != null && !back.CanStand(solda.X, solda.Y))
            {
                problems.Add("Da saida, depois do boss, nao da pra voltar ate a Solda na Praca.");
            }

            if (!back.CanStand(CrisolTile.X, CrisolTile.Y))
            {
                problems.Add("Da saida, depois do boss, nao da pra voltar ate Crisol.");
            }
        }

        /// <summary>
        /// A escadaria quebrada nao pode servir de caminho: com a porta da
        /// subida tampada, nem com o Salto de Pressao se chega no elevador.
        /// </summary>
        private static void CheckBrokenStairway(WorldState world, List<string> problems)
        {
            LevelMap blocked = world.Map.Clone();
            blocked.Fill(189, 78, 189, 81, LevelMap.Rock);

            LevelMap.Reach reach = blocked.Explore(SpawnTile, RulesAfterBoss(), world.Lifts(), world.Jets(), CloudAreas());
            if (reach.CanStand(District3Tile.X, District3Tile.Y))
            {
                problems.Add("Da pra chegar no elevador sem a subida da direita: a escadaria quebrada esta servindo de caminho.");
            }
        }

        /// <summary>A ordem em que a validacao abriu a fase. Serve pra conferir o desenho.</summary>
        public static List<string> Walkthrough()
        {
            var log = new List<string>();
            Play(out _, log);
            return log;
        }

        private static bool TouchesAny(LevelMap.Reach reach, Area area)
        {
            for (int y = area.Y; y <= area.Top; y++)
            {
                for (int x = area.X; x <= area.Right; x++)
                {
                    if (reach.CanTouch(x, y)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Os trechos de chao onde nao da pra chegar nem com a fase toda
        /// aberta. Nao e erro — tampo de caldeira e canto de teto entram aqui —
        /// mas vale olhar a lista quando mexer no mapa: um trecho grande nela e
        /// sala isolada.
        /// </summary>
        public static List<string> UnreachableFloors()
        {
            WorldState world = Play(out LevelMap.Reach reach, null);
            var floors = new List<string>();

            foreach (LevelMap.Perch perch in world.Map.FindPerches())
            {
                if (perch.Length < 2) continue;

                bool reached = false;
                for (int x = perch.X; x <= perch.Right && !reached; x++)
                {
                    reached = reach.CanStand(x, perch.Y);
                }

                if (!reached) floors.Add(perch.ToString());
            }

            return floors;
        }

        /// <summary>O mapa em texto, de cima pra baixo, pra conferir no olho.</summary>
        public static string[] ToRows() => Map.ToRows();
    }
}
