using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase03Layout
    {
        // ------------------------------------------------------------------
        // Validacao
        // ------------------------------------------------------------------

        /// <summary>
        /// As regras de alcance desta fase: as com folga, mais o Steam Dash (uns
        /// 3 tiles a mais de lado), o Pressure Jump (uns 2 a mais de altura e 2
        /// de lado) e o pulo de parede.
        /// </summary>
        private static LevelMap.JumpRules Rules()
        {
            LevelMap.JumpRules rules = LevelMap.JumpRules.Comfortable();
            rules.MaxRise = 4;
            rules.MaxRun += 5;
            rules.WallJump = true;
            return rules;
        }

        /// <summary>A sala que contem a celula, ou null.</summary>
        public static string RoomAt(int x, int y)
        {
            foreach ((string name, int x0, int y0, int x1, int y1) in Rooms)
            {
                if (x >= x0 && x <= x1 && y >= y0 && y <= y1) return name;
            }

            return null;
        }

        private sealed class WorldState
        {
            public LevelMap Map;
            public readonly HashSet<string> OpenDoors = new HashSet<string>();
            public bool BossDown;
        }

        private static void Fill(LevelMap world, GateSpec gate, char cell)
        {
            world.Fill(gate.X, gate.Y, gate.X + gate.Width - 1, gate.Y + gate.Height - 1, cell);
        }

        /// <summary>Fecha as barreiras de cristal e os escombros do elevador: o que so a onda abre.</summary>
        private static void CloseBarriers(LevelMap world) => SetBarriers(world, LevelMap.Rock);

        private static void SetBarriers(LevelMap world, char cell)
        {
            foreach ((string _, int x, int y, int height) in Barriers) world.Fill(x, y, x, y + height - 1, cell);

            (int rx, int ry, int rw, int rh) = ElevatorRubble;
            world.Fill(rx, ry, rx + rw - 1, ry + rh - 1, cell);
        }

        /// <summary>O mapa com os primeiros chaos da arena ja desabados.</summary>
        private static LevelMap Collapsed(LevelMap source, int slabsGone)
        {
            LevelMap world = source.Clone();

            for (int i = 0; i < slabsGone && i < Slabs.Count; i++)
            {
                (string _, int x0, int x1, int y0, int y1) = Slabs[i];
                world.Fill(x0, y0, x1, y1, LevelMap.Air);
            }

            return world;
        }

        /// <summary>
        /// Joga a fase na cabeca: portoes e barreiras fechados; abre cada porta
        /// cujos cristais todos se alcancam; derruba a broca quando se chega
        /// nela (a arena desaba inteira e a barreira quebra com o Resonance
        /// Strike). Repete ate nada mais mudar.
        /// </summary>
        private static WorldState Play(out LevelMap.Reach reach, bool bossFalls = true, Func<string, bool> allowDoor = null,
            Action<WorldState, LevelMap.Reach> visit = null)
        {
            var world = new WorldState { Map = Map.Clone() };
            foreach (GateSpec gate in Gates) Fill(world.Map, gate, LevelMap.Rock);
            CloseBarriers(world.Map);

            LevelMap.JumpRules rules = Rules();

            while (true)
            {
                reach = world.Map.Explore(SpawnTile, rules);
                visit?.Invoke(world, reach);
                bool changed = false;

                foreach (DoorSpec door in Doors)
                {
                    if (world.OpenDoors.Contains(door.Name)) continue;
                    if (allowDoor != null && !allowDoor(door.Name)) continue;

                    bool all = true;
                    foreach (ResonantSpot spot in Resonants)
                    {
                        if (spot.Door == door.Name && !reach.CanStand(spot.StandX, spot.Y)) all = false;
                    }

                    if (!all) continue;

                    world.OpenDoors.Add(door.Name);
                    changed = true;

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Name == door.Gate) Fill(world.Map, gate, LevelMap.Air);
                    }
                }

                // A chave: quem alcanca o Capataz sai com ela, e ela
                // abre a grade do salao.
                if (!world.OpenDoors.Contains(AncientKeyId) && (allowDoor == null || allowDoor(AncientKeyId))
                    && reach.CanStand(EliteTile.X, EliteTile.Y))
                {
                    world.OpenDoors.Add(AncientKeyId);
                    changed = true;

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Name == StatueEntranceGate) Fill(world.Map, gate, LevelMap.Air);
                    }
                }

                // As estatuas: quando o Scrap alcanca as quatro, a sequencia e
                // so questao de saber a historia.
                bool allStatues = Statues.Count > 0;
                foreach (StatueSpot statue in Statues)
                {
                    if (!reach.CanStand(statue.X, statue.Y)) allStatues = false;
                }

                if (!world.OpenDoors.Contains(MainDoor) && (allowDoor == null || allowDoor(MainDoor)) && allStatues)
                {
                    world.OpenDoors.Add(MainDoor);
                    changed = true;

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Name == StatueGate) Fill(world.Map, gate, LevelMap.Air);
                    }
                }

                if (bossFalls && !world.BossDown && reach.CanStand(BossTile.X, BossTile.Y))
                {
                    world.BossDown = true;
                    changed = true;

                    world.Map = Collapsed(world.Map, Slabs.Count);
                    SetBarriers(world.Map, LevelMap.Air);
                }

                if (!changed) return world;
            }
        }

        /// <summary>
        /// Confere se a fase e jogavel de verdade, sem precisar jogar. Devolve a
        /// lista de problemas. Vazia quer dizer que esta tudo certo.
        /// </summary>
        public static List<string> Validate()
        {
            var problems = new List<string>();

            for (int x = 0; x < Width; x++)
            {
                if (!Map.IsRock(x, 0) || !Map.IsRock(x, Height - 1)) problems.Add($"A coluna {x} tem buraco no chao ou no teto do mapa.");
            }

            for (int y = 0; y < Height; y++)
            {
                if (!Map.IsRock(0, y) || !Map.IsRock(Width - 1, y)) problems.Add($"A linha {y} tem buraco numa das paredes do mapa.");
            }

            if (!Map.IsStandable(SpawnTile.X, SpawnTile.Y))
            {
                problems.Add($"O Scrap chega em ({SpawnTile.X}, {SpawnTile.Y}) e nao ha chao ali.");
                return problems;
            }

            LevelMap.JumpRules rules = Rules();
            WorldState world = Play(out LevelMap.Reach reach);

            foreach (DoorSpec door in Doors)
            {
                if (!world.OpenDoors.Contains(door.Name)) problems.Add($"{door.Name}: os cristais dela nao se alcancam todos.");
                if (!Gates.Exists(gate => gate.Name == door.Gate)) problems.Add($"{door.Name} abre '{door.Gate}', que nao existe.");

                foreach (int note in door.Sequence)
                {
                    if (!Resonants.Exists(spot => spot.Door == door.Name && spot.Note == note))
                    {
                        problems.Add($"{door.Name} pede a nota {note}, e nenhum cristal dela a toca.");
                    }
                }
            }

            if (!world.OpenDoors.Contains(MainDoor)) problems.Add("As estatuas do Campanario nao se alcancam todas.");
            if (!Gates.Exists(gate => gate.Name == StatueGate)) problems.Add($"As estatuas abrem '{StatueGate}', que nao existe.");

            for (int order = 1; order <= 4; order++)
            {
                if (!Statues.Exists(statue => statue.Order == order)) problems.Add($"Nenhuma estatua e a de numero {order}.");
            }

            // Duas coisas de apertar E no mesmo lugar brigam pelo botao.
            foreach (StatueSpot statue in Statues)
            {
                foreach (LoreSpot spot in Lore)
                {
                    if (spot.Y == statue.Y && Math.Abs(spot.X - statue.X) < 3) problems.Add($"{statue.Name} fica colada em {spot.Name}.");
                }

                foreach (RewardSpot spot in Rewards)
                {
                    if (spot.Y == statue.Y && Math.Abs(spot.X - statue.X) < 2) problems.Add($"{statue.Name} fica em cima de {spot.Id}.");
                }
            }

            foreach ((int x, int y) in SpawnCracks)
            {
                if (!Map.IsStandable(x, y)) problems.Add($"A rachadura em ({x}, {y}) nao tem chao pros mites.");
            }

            if (!world.BossDown) problems.Add("Nao da pra chegar na broca.");

            Play(out LevelMap.Reach withoutBoss, false);

            foreach ((string name, int x, int y, int height) in Barriers)
            {
                if (withoutBoss.CanStand(x - 2, y)) problems.Add($"Da pra passar pela {name} sem quebra-la.");
            }

            CheckElevator(world, withoutBoss, problems);

            CheckArena(problems);

            // O caminho: sem as estatuas, a broca fica fora de alcance.
            WorldState withoutBelfry = Play(out _, true, name => name != MainDoor);
            if (withoutBelfry.BossDown) problems.Add("Da pra chegar na broca sem o enigma das estatuas.");

            // E sem a chave do Capataz, nem as estatuas.
            WorldState withoutKey = Play(out _, true, name => name != AncientKeyId);
            if (withoutKey.OpenDoors.Contains(MainDoor)) problems.Add("Da pra chegar nas estatuas sem a CHAVE ANTIGA.");

            // Uma plataforma por sala, no maximo: o resto se passa no chao.
            foreach ((string name, int x0, int y0, int x1, int y1) in Rooms)
            {
                if (name == "13_DrillChamber") continue;

                int platforms = 0;
                foreach ((int fx0, int _, int fy) in Fragiles)
                {
                    if (fx0 >= x0 && fx0 <= x1 && fy >= y0 && fy <= y1) platforms++;
                }

                platforms += LedgeCount(x0, y0, x1, y1);
                if (platforms > 1) problems.Add($"A sala {name} tem {platforms} plataformas (o maximo e uma).");
            }
            CheckQuietSpots(problems);
            CheckEveryRoomPays(problems);
            CheckNoTraps(problems);

            foreach (Marker marker in Markers)
            {
                if (!marker.MustReach) continue;
                if (marker.Group == "13_DrillChamber") continue;

                bool reached = reach.CanStand(marker.X, marker.Y) || withoutBoss.CanStand(marker.X, marker.Y);

                if (!Map.IsStandable(marker.X, marker.Y)) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) nao e um lugar onde da pra ficar em pe.");
                else if (!reached) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) esta fora de alcance.");
            }

            foreach (LoreSpot spot in Purified)
            {
                if (!reach.CanStand(spot.X, spot.Y)) problems.Add($"{spot.Name} ({spot.X}, {spot.Y}) esta fora de alcance.");
            }

            if (!withoutBoss.CanStand(CrisolTile.X, CrisolTile.Y)) problems.Add("Nao da pra chegar em Crisol.");

            foreach (FoeSpot foe in Foes)
            {
                int to = foe.Kind == FoeKind.ShardSentinel ? foe.X0 : foe.X1;

                for (int x = foe.X0; x <= to; x++)
                {
                    if (!Map.IsSolid(x, foe.Y - 1)) problems.Add($"{foe.Name} anda por ({x}, {foe.Y}), que nao tem chao.");
                }
            }

            foreach ((int x0, int x1, int y) in Fragiles)
            {
                for (int x = x0; x <= x1; x++)
                {
                    if (Map.IsSolid(x, y + 1)) problems.Add($"A plataforma fragil em ({x}, {y}) tem terreno em cima.");
                }
            }

            CheckDecorPhysics(problems);
            return problems;
        }

        /// <summary>
        /// O tamanho, em tiles, de cada peca de cenario que fica no chao, e
        /// se ela fica no chao (as outras ficam na parede ou penduradas).
        /// </summary>
        private static bool GroundedSize(string sprite, out int w, out int h)
        {
            switch (sprite)
            {
                case "decor_barrel": w = 1; h = 2; return true;
                case "decor_shelf": w = 2; h = 2; return true;
                case "decor_crate": w = 1; h = 1; return true;
                case "decor_toolbox": w = 1; h = 1; return true;
                case "decor_sack": w = 1; h = 1; return true;
                case "mine_cart": w = 3; h = 2; return true;
                case MineDressArt.Pickaxe:
                case MineDressArt.OreSack:
                case MineDressArt.Lantern:
                case MineDressArt.CutGeode:
                case MineDressArt.Stalagmite:
                case MineDressArt.Rubble:
                    (w, h) = MineDressArt.Size(sprite);
                    return true;
                default: w = 1; h = 1; return false;
            }
        }

        /// <summary>
        /// A fisica de quem esta parado: o que fica no chao tem apoio embaixo
        /// de toda a largura e nao entra na rocha; a placa da parede nao entra
        /// na rocha; a corrente sai do teto.
        /// </summary>
        private static void CheckDecorPhysics(List<string> problems)
        {
            var occupied = new Dictionary<(int, int), string>();

            foreach (DecorSpot spot in Decor)
            {
                int x0 = (int)Math.Floor(spot.X);
                int y0 = (int)Math.Floor(spot.Y);

                if (GroundedSize(spot.Sprite, out int w, out int h))
                {
                    if (Math.Abs(spot.Y - y0) > 0.01f) problems.Add($"{spot.Sprite} em ({spot.X}, {spot.Y}) nao assenta numa linha de chao.");

                    for (int x = x0; x < x0 + w; x++)
                    {
                        bool onCrate = occupied.TryGetValue((x, y0 - 1), out string under) && under == "decor_crate";
                        if (!Map.IsSolid(x, y0 - 1) && !onCrate)
                        {
                            problems.Add($"{spot.Sprite} em ({spot.X}, {spot.Y}) tem a base no ar em x={x}.");
                            break;
                        }
                    }

                    for (int y = y0; y < y0 + h; y++)
                    {
                        for (int x = x0; x < x0 + w; x++)
                        {
                            if (Map.IsRock(x, y)) problems.Add($"{spot.Sprite} em ({spot.X}, {spot.Y}) entra na rocha em ({x}, {y}).");
                            else if (occupied.TryGetValue((x, y), out string other)) problems.Add($"{spot.Sprite} em ({spot.X}, {spot.Y}) fica em cima de {other}.");
                            occupied[(x, y)] = spot.Sprite;
                        }
                    }
                }
                else if (spot.Sprite == "decor_sign")
                {
                    for (int x = x0; x < x0 + 3; x++)
                    {
                        if (Map.IsRock(x, y0)) problems.Add($"A placa em ({spot.X}, {spot.Y}) entra na rocha em x={x}.");
                    }
                }
                else if (spot.Sprite == "decor_chain")
                {
                    if (!Map.IsSolid(x0, (int)Math.Round(spot.Y + spot.Height))) problems.Add($"A corrente em ({spot.X}, {spot.Y}) nao sai do teto.");
                }
            }
        }

        /// <summary>
        /// O fio da fase: a cabine se alcanca desde a chegada; os escombros
        /// fecham o poco inteiro (nao se passa por eles, nem por cima); o
        /// Scrap, de pe na cabine ou ao lado dela, fica ao alcance da onda; e
        /// do fundo da arena, com a broca no chao, da pra voltar ate a cabine.
        /// </summary>
        private static void CheckElevator(WorldState world, LevelMap.Reach withoutBoss, List<string> problems)
        {
            (int carX, int carY, int carWidth) = ElevatorCar;
            (int rx, int ry, int rw, int rh) = ElevatorRubble;

            if (!withoutBoss.CanStand(carX + 1, carY)) problems.Add("Nao da pra entrar na cabine do elevador.");

            // Os escombros cobrem a largura toda do poco, de parede a parede.
            for (int x = carX; x < carX + carWidth; x++)
            {
                if (x < rx || x > rx + rw - 1) problems.Add($"Os escombros nao tampam a coluna {x} do poco.");
            }

            for (int y = ry + rh; y <= ElevatorTopY + 4; y++)
            {
                for (int x = carX; x < carX + carWidth; x++)
                {
                    if (withoutBoss.CanStand(x, y)) problems.Add($"Da pra entrar no poco do elevador ({x}, {y}) sem tirar os escombros.");
                }
            }

            // A onda do Resonance Strike alcanca uns 3,5 tiles a partir do peito do Scrap.
            const float radius = 3.5f;
            float chestY = carY + 0.75f;
            float nearestX = Math.Max(rx, Math.Min(carX + 1.5f, rx + rw));
            float distance = (float)Math.Sqrt(Math.Pow(nearestX - (carX + 1.5f), 2) + Math.Pow(ry - chestY, 2));
            if (distance > radius - 0.5f) problems.Add($"De dentro da cabine, a onda nao chega nos escombros ({distance:0.0} tiles).");

            // Depois da luta: do fundo da arena desabada ate a cabine.
            LevelMap.Reach afterFight = world.Map.Explore((BossTile.X, BossFloors[BossFloors.Length - 1]), Rules());
            if (!afterFight.CanStand(carX + 1, carY)) problems.Add("Do fundo da arena, depois da broca, nao da pra voltar ate o elevador.");
        }

        /// <summary>
        /// A arena: com o chao inteiro, cada pilar se alcanca na camada dele;
        /// e, com tudo desabado, do fundo se sobe de volta ao corredor.
        /// </summary>
        private static void CheckArena(List<string> problems)
        {
            LevelMap.JumpRules rules = Rules();

            for (int gone = 0; gone < BossFloors.Length; gone++)
            {
                LevelMap world = Collapsed(Map, gone);
                foreach (GateSpec gate in Gates) Fill(world, gate, LevelMap.Air);
                CloseBarriers(world);

                LevelMap.Reach stage = world.Explore((BossTile.X, BossFloors[gone]), rules);

                foreach (ResonantSpot spot in Resonants)
                {
                    if (spot.Note != 0 || spot.Y != BossFloors[gone] || spot.X < BossRange.X0 || spot.X > BossRange.X1) continue;
                    if (!stage.CanStand(spot.StandX, spot.Y)) problems.Add($"{spot.Name}: nao da pra chegar em ({spot.StandX}, {spot.Y}) na camada dele.");
                }
            }

            LevelMap bottom = Collapsed(Map, Slabs.Count);
            CloseBarriers(bottom);
            LevelMap.Reach climb = bottom.Explore((BossTile.X, BossFloors[BossFloors.Length - 1]), rules);
            if (!climb.CanStand(78, 92)) problems.Add("Do fundo da arena nao da pra subir de volta ao corredor de entrada.");
        }

        /// <summary>
        /// A bancada e o Crisol nao ficam no ninho de ninguem: longe da casa de
        /// cacadores, do trecho dos mites e do pedestal das sentinelas. O resto
        /// (o que se le) pode ficar no caminho de quem patrulha — os bichos
        /// andam o chao inteiro, e o distrito e infestado de proposito.
        /// </summary>
        private static void CheckQuietSpots(List<string> problems)
        {
            var spots = new List<(string, int, int)>();
            foreach ((string name, int x, int y) in Workbenches) spots.Add((name, x + 1, y));
            spots.Add(("Crisol", CrisolTile.X, CrisolTile.Y));

            foreach ((string name, int x, int y) in spots)
            {
                foreach (FoeSpot foe in Foes)
                {
                    if (foe.Y != y) continue;

                    bool near;
                    switch (foe.Kind)
                    {
                        case FoeKind.EchoHunter: near = Math.Abs(x - foe.HomeX) <= 3; break;
                        case FoeKind.ShardSentinel: near = Math.Abs(x - foe.X0) <= 2; break;
                        default: near = x >= foe.X0 - 2 && x <= foe.X1 + 2; break;
                    }

                    if (near) problems.Add($"{name} ({x}, {y}) fica no ninho de {foe.Name}.");
                }
            }
        }


        private static void CheckEveryRoomPays(List<string> problems)
        {
            foreach ((string name, int x0, int y0, int x1, int y1) in Rooms)
            {
                if (Array.IndexOf(PassageRooms, name) >= 0) continue;

                bool Inside(int x, int y) => x >= x0 && x <= x1 && y >= y0 && y <= y1;

                bool pays = Lore.Exists(spot => Inside(spot.X, spot.Y))
                    || Rewards.Exists(spot => Inside(spot.X, spot.Y))
                    || Inside(CrisolTile.X, CrisolTile.Y)
                    || Inside(EliteTile.X, EliteTile.Y)
                    || Doors.Exists(door => Inside(door.Preview.X, door.Preview.Y))
                    || Statues.Exists(statue => Inside(statue.X, statue.Y));

                if (!pays) problems.Add($"A sala {name} nao tem nada pra achar.");
            }
        }

        /// <summary>
        /// Nenhuma armadilha: depois de chegar na gruta, de todo chao alcancavel
        /// da pra voltar pra ela. A arena fica de fora (tranca na luta).
        /// </summary>
        private static void CheckNoTraps(List<string> problems)
        {
            LevelMap.JumpRules rules = Rules();
            var reported = new HashSet<string>();

            Play(out _, true, null, (world, reach) =>
            {
                if (!reach.CanStand(CrisolTile.X, CrisolTile.Y)) return;

                foreach (LevelMap.Perch perch in world.Map.FindPerches())
                {
                    int x = -1;
                    for (int cx = perch.X; cx <= perch.Right; cx++)
                    {
                        if (reach.CanStand(cx, perch.Y)) { x = cx; break; }
                    }

                    if (x < 0) continue;

                    string room = RoomAt(x, perch.Y) ?? "fora das salas";
                    if (room == "13_DrillChamber") continue;

                    string key = $"{room}:{perch}";
                    if (reported.Contains(key)) continue;

                    LevelMap.Reach back = world.Map.Explore((x, perch.Y), rules);
                    if (back.CanStand(CrisolTile.X, CrisolTile.Y)) continue;

                    reported.Add(key);
                    problems.Add($"Armadilha em {room}, {perch}: dali nao da pra voltar pra gruta.");
                }
            });
        }

        /// <summary>Os trechos de chao onde nao da pra chegar nem com tudo aberto.</summary>
        public static List<string> UnreachableFloors()
        {
            WorldState world = Play(out LevelMap.Reach reach);
            var floors = new List<string>();

            foreach (LevelMap.Perch perch in world.Map.FindPerches())
            {
                if (perch.Length < 2) continue;

                bool reached = false;
                for (int x = perch.X; x <= perch.Right && !reached; x++) reached = reach.CanStand(x, perch.Y);

                if (!reached) floors.Add(perch.ToString());
            }

            return floors;
        }

        public static string[] ToRows() => Map.ToRows();
    }
}
