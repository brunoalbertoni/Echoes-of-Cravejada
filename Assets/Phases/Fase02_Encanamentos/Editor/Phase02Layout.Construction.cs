using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase02Layout
    {
        // ------------------------------------------------------------------
        // Ferramentas de construcao
        // ------------------------------------------------------------------

        private static void Carve(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Air);

        private static void Solid(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Rock);

        /// <summary>Um cano deitado, de pisar.</summary>
        private static void PipeFloor(int x0, int x1, int y) => Map.Fill(x0, y, x1, y, LevelMap.Platform);

        private static void Grate(int x0, int x1, int y) => Map.Fill(x0, y, x1, y, LevelMap.Grate);

        /// <summary>Enche um retangulo de acido e registra a poca. A linha de cima e a superficie.</summary>
        private static void Acid(string name, int x0, int y0, int x1, int y1)
        {
            Map.Fill(x0, y0, x1, y1, LevelMap.Acid);
            AcidPools.Add((name, Area.Between(x0, y0, x1, y1)));
        }

        private static void Room(string name, int x0, int y0, int x1, int y1)
        {
            Rooms.Add((name, Area.Between(x0, y0, x1, y1)));
        }

        private static Vent Lift(string name, int x0, int x1, int bottom, int top, bool nozzle = true)
        {
            var vent = new Vent { Name = name, Kind = VentKind.Lift, Zone = Area.Between(x0, bottom, x1, top) };
            Vents.Add(vent);

            // O bocal fica na celula logo abaixo de onde o vapor comeca a agir.
            if (nozzle) Fixtures.Add(new Fixture(FixtureKind.NozzleUp, name + "_Nozzle", x0, bottom - 1, x1 - x0 + 1));

            Lights.Add(new Light((x0 + x1 + 1) * 0.5f, bottom + 1.5f, 5f, 0.45f, Glow.Steam, false));
            return vent;
        }

        private static Vent Jet(string name, int x0, int x1, int y0, int y1, int direction)
        {
            var vent = new Vent
            {
                Name = name,
                Kind = VentKind.Jet,
                Zone = Area.Between(x0, y0, x1, y1),
                Direction = direction,
            };

            Vents.Add(vent);

            int nozzleX = direction > 0 ? x0 - 1 : x1 + 1;
            Fixtures.Add(new Fixture(FixtureKind.NozzleSide, name + "_Nozzle", nozzleX, y0, y1 - y0 + 1, direction < 0));
            return vent;
        }

        private static void Gate(string name, int x0, int y0, int x1, int y1, GateOpener opener,
            int moveX = 0, int moveY = 1)
        {
            Gates.Add(new GateSpec
            {
                Name = name,
                Area = Area.Between(x0, y0, x1, y1),
                Opener = opener,
                MoveX = moveX,
                MoveY = moveY,
            });
        }

        private static void Enemy(EnemyKind kind, string name, int x0, int x1, int y,
            int direction = 1, int range = 14, int ceilingY = 0, int start = -1)
        {
            Enemies.Add(new EnemySpec
            {
                Kind = kind,
                Name = name,
                X0 = x0,
                X1 = x1,
                Y = y,
                Direction = direction,
                Range = range,
                CeilingY = ceilingY,
                StartX = start,
            });
        }

        /// <summary>Uma sanguessuga pendurada no teto, em cima de um trecho de chao.</summary>
        private static void Leech(string name, int x, int ceilingY, int floorY, int x0, int x1)
        {
            Enemy(EnemyKind.PipeLeech, name, x0, x1, floorY, 1, 0, ceilingY);
            Enemies[Enemies.Count - 1].Direction = x;
        }

        private static void Cloud(string name, int x0, int y0, int x1, int y1)
        {
            Clouds.Add((name, Area.Between(x0, y0, x1, y1)));
        }

        /// <summary>Uma parede rachada. No mapa ela e ar; quem fecha e o objeto, ate a caldeira explodir.</summary>
        private static void Cracked(string name, int x0, int y0, int x1, int y1)
        {
            Carve(x0, y0, x1, y1);
            BreakableWalls.Add((name, Area.Between(x0, y0, x1, y1)));
        }

        private static void PressureBoiler(string group, string name, int x, int y, int standX, int standY,
            params string[] walls)
        {
            PressureBoilers.Add(new BoilerSpec
            {
                Group = group,
                Name = name,
                X = x,
                Y = y,
                StandX = standX,
                StandY = standY,
                Walls = walls,
            });

            Shine(x + 2f, y + 1.6f, 6f, 0.9f, Glow.Acid, true);
        }

        private static void Bench(string group, string name, int x, int y)
        {
            Workbenches.Add((name, x, y));
            Mark(group, name + "_Rest", x + 1, y);
            Shine(x + 1.5f, y + 3f, 7f, 0.9f, Glow.Lamp);
        }

        private static LoreSpec Tell(LoreKind kind, string group, string name, string title, int x, int y,
            params string[] lines)
        {
            var spec = new LoreSpec { Kind = kind, Name = name, Title = title, X = x, Y = y, Lines = lines };
            Lore.Add(spec);

            // Entra como marcador tambem, pra a validacao conferir que da pra chegar.
            Mark(group, name, x, y);
            return spec;
        }

        /// <summary>Uma pagina do livro de turnos: so se le, como qualquer documento.</summary>
        private static void Logbook(string group, int number, string title, int x, int y, params string[] lines)
        {
            Tell(LoreKind.Page, group, "Lore_Logbook_" + number, title, x, y, lines);
        }

        private static void Reward(string group, RewardKind kind, string id, int x, int y, int amount = 0)
        {
            Rewards.Add(new RewardSpec { Kind = kind, Id = id, X = x, Y = y, Amount = amount });
            Mark(group, id, x, y);
        }

        private static void Veil(string name, int x0, int y0, int x1, int y1)
        {
            Carve(x0, y0, x1, y1);
            Veils.Add((name, Area.Between(x0, y0, x1, y1)));
        }

        private static void Mark(string group, string name, int x, int y, bool mustReach = true)
        {
            Markers.Add(new Marker(group, name, x, y, mustReach));
        }

        private static void Fix(FixtureKind kind, string name, int x, int y, int size = 0)
        {
            Fixtures.Add(new Fixture(kind, name, x, y, size));
        }

        private static void Shine(float x, float y, float radius, float intensity, Glow glow, bool pulses = false)
        {
            Lights.Add(new Light(x, y, radius, intensity, glow, pulses));
        }

        private static void Put(string tile, int x, int y, PropLayer layer = PropLayer.Middle)
        {
            Props.Add(new Prop(tile, x, y, layer));
        }

        /// <summary>
        /// Uma caldeira com tudo o que vem junto: o corpo, a luz da fornalha
        /// pulsando e o calor que sobe dela.
        /// </summary>
        private static void Boiler(string name, int x, int y)
        {
            Fix(FixtureKind.Boiler, name, x, y);
            Shine(x + 2f, y + 1.6f, 7f, 0.95f, Glow.Furnace, true);
            Leaks.Add(new Leak(x + 1.5f, y + 6f, 0f, 1f));
        }

        /// <summary>Uma fornalha de parede: a moldura, os dois tiles de fogo e a luz.</summary>
        private static void Furnace(string name, int x, int y)
        {
            Put(FireTile, x, y, PropLayer.Glow);
            Put(FireTile, x + 1, y, PropLayer.Glow);
            Fix(FixtureKind.Furnace, name, x, y);
            Shine(x + 1f, y + 1f, 6f, 0.85f, Glow.Furnace, true);
        }

        /// <summary>
        /// Uma valvula: o volante enferrujado, um manometro do lado e o que
        /// ela muda ao girar. Os efeitos vao pelo nome: "vent:X", "pool:X" e
        /// "gate:X".
        /// </summary>
        private static ValveSpec Valve(string group, string name, int x, int y, int standX, int standY,
            params string[] effects)
        {
            var vents = new List<string>();
            var pools = new List<string>();
            var gates = new List<string>();

            foreach (string effect in effects)
            {
                if (effect.StartsWith("vent:")) vents.Add(effect.Substring(5));
                else if (effect.StartsWith("pool:")) pools.Add(effect.Substring(5));
                else if (effect.StartsWith("gate:")) gates.Add(effect.Substring(5));
            }

            var valve = new ValveSpec
            {
                Group = group,
                Name = name,
                X = x,
                Y = y,
                StandX = standX,
                StandY = standY,
                Vents = vents.ToArray(),
                Pools = pools.ToArray(),
                Gates = gates.ToArray(),
            };

            Valves.Add(valve);
            // O manometro fica do lado do volante; se ali e parede, do outro lado.
            Fix(FixtureKind.GaugeSmall, name + "_Gauge", Map.IsRock(x + 2, y + 1) ? x - 1 : x + 2, y + 1);
            return valve;
        }

        // ------------------------------------------------------------------
        // Canos de enfeite
        // ------------------------------------------------------------------

        private static void Connect(PipeLayer layer, bool aged, int x, int y, int side)
        {
            Dictionary<(int, int), int> masks = PipeMasks[(int)layer];

            masks.TryGetValue((x, y), out int mask);
            masks[(x, y)] = mask | side;

            if (aged) AgedPipes[(int)layer].Add((x, y));
        }

        /// <summary>
        /// Estende um cano por uma sequencia de pontos. Cada trecho tem que ser
        /// reto, na horizontal ou na vertical.
        /// </summary>
        private static void RunPipe(PipeLayer layer, bool aged, params (int X, int Y)[] points)
        {
            for (int i = 0; i + 1 < points.Length; i++)
            {
                (int X, int Y) from = points[i];
                (int X, int Y) to = points[i + 1];

                int stepX = Math.Sign(to.X - from.X);
                int stepY = Math.Sign(to.Y - from.Y);

                // Um trecho na diagonal nao tem como ser desenhado.
                if (stepX != 0 && stepY != 0) continue;

                int x = from.X;
                int y = from.Y;

                while (x != to.X || y != to.Y)
                {
                    int nextX = x + stepX;
                    int nextY = y + stepY;

                    int forward = stepX > 0 ? BrassArt.PipeEast
                        : stepX < 0 ? BrassArt.PipeWest
                        : stepY > 0 ? BrassArt.PipeNorth
                        : BrassArt.PipeSouth;

                    int backward = stepX > 0 ? BrassArt.PipeWest
                        : stepX < 0 ? BrassArt.PipeEast
                        : stepY > 0 ? BrassArt.PipeSouth
                        : BrassArt.PipeNorth;

                    Connect(layer, aged, x, y, forward);
                    Connect(layer, aged, nextX, nextY, backward);

                    x = nextX;
                    y = nextY;
                }
            }
        }

        /// <summary>
        /// Enche uma sala de encanamento no fundo: um tronco correndo rente ao
        /// teto e descidas a intervalos irregulares, algumas dobrando de lado.
        /// O sorteio e estavel — a mesma sala sempre ganha os mesmos canos.
        /// </summary>
        private static void Plumb(Area room, int seed, PipeLayer layer, bool aged, int trunkDrop = 1)
        {
            int trunk = room.Top - trunkDrop;
            RunPipe(layer, aged, (room.X - 2, trunk), (room.Right + 2, trunk));

            int x = room.X + 2 + LevelMap.Pick(seed, 1, 4);
            int index = 0;

            while (x < room.Right - 1)
            {
                int longest = Math.Max(2, room.Height - trunkDrop - 3);
                int length = 2 + LevelMap.Pick(seed + index, 2, longest);
                int bottom = trunk - length;

                if (LevelMap.Pick(seed + index, 3, 3) == 0)
                {
                    int direction = LevelMap.Pick(seed + index, 4, 2) == 0 ? 1 : -1;
                    int span = 2 + LevelMap.Pick(seed + index, 5, 3);
                    RunPipe(layer, aged, (x, trunk), (x, bottom), (x + direction * span, bottom));
                }
                else
                {
                    RunPipe(layer, aged, (x, trunk), (x, bottom));
                }

                x += 5 + LevelMap.Pick(seed + index, 6, 5);
                index++;
            }
        }

        /// <summary>Os tiles de cano de uma camada, ja com o desenho certo pra cada celula.</summary>
        public static IEnumerable<(int X, int Y, string Tile)> PipeTiles(PipeLayer layer)
        {
            foreach (KeyValuePair<(int, int), int> entry in PipeMasks[(int)layer])
            {
                (int x, int y) = entry.Key;
                int mask = entry.Value;

                if (IsBuried(layer, x, y)) continue;

                bool aged = AgedPipes[(int)layer].Contains((x, y));
                int row = aged ? BrassArt.CopperPipeRow : BrassArt.BrassPipeRow;
                int column = mask;

                // Nos trechos retos, de vez em quando entra uma peca especial.
                const int across = BrassArt.PipeEast | BrassArt.PipeWest;
                const int up = BrassArt.PipeNorth | BrassArt.PipeSouth;

                if (mask == across || mask == up)
                {
                    int roll = LevelMap.Pick(x * 7 + (int)layer, y * 13 + 3, 40);
                    int special = -1;

                    if (roll < 7) special = mask == across ? BrassArt.PipeFlangeAcross : BrassArt.PipeFlangeUp;
                    else if (roll < 11) special = mask == across ? BrassArt.PipeStrapAcross : BrassArt.PipeStrapUp;
                    else if (roll < 13) special = mask == across ? BrassArt.PipeLeakAcross : BrassArt.PipeLeakUp;
                    else if (roll == 13 && mask == across) special = BrassArt.PipeValve;
                    else if (roll == 14 && mask == across) special = BrassArt.PipeGauge;

                    if (special >= 0)
                    {
                        row += 1;
                        column = special;
                    }
                }

                yield return (x, y, $"pipe_c{column}_r{row}");
            }
        }

        /// <summary>Se o cano desta celula esta todo dentro da parede, e portanto nao aparece.</summary>
        private static bool IsBuried(PipeLayer layer, int x, int y)
        {
            if (layer != PipeLayer.Far) return Map.IsRock(x, y);

            // As celulas da camada de longe valem 2x2 tiles.
            return Map.IsRock(x * 2, y * 2) && Map.IsRock(x * 2 + 1, y * 2)
                && Map.IsRock(x * 2, y * 2 + 1) && Map.IsRock(x * 2 + 1, y * 2 + 1);
        }

        // ------------------------------------------------------------------
        // Goteiras e nevoa
        // ------------------------------------------------------------------

        /// <summary>
        /// Espalha goteiras pelo teto de uma sala, a cada tantas colunas. A
        /// queda de cada uma e medida ate o primeiro chao (ou acido) embaixo.
        /// </summary>
        private static void DripsIn(Area room, int every, int seed, bool acid)
        {
            for (int x = room.X + 1 + LevelMap.Pick(seed, 9, every); x <= room.Right - 1; x += every)
            {
                // Desce do teto da sala ate achar a primeira celula de ar com teto em cima.
                for (int y = room.Top; y >= room.Y; y--)
                {
                    if (Map.IsSolid(x, y) || !Map.IsSolid(x, y + 1)) continue;

                    int fall = 0;
                    while (y - fall - 1 >= 0 && !Map.IsSolid(x, y - fall - 1) && !Map.IsAcid(x, y - fall - 1))
                    {
                        fall++;
                    }

                    if (fall >= 2) Drips.Add(new Drip(x, y, fall, acid));
                    break;
                }
            }
        }

        /// <summary>Fumaca verde em cima de uma poca de acido, e a luz que ela solta.</summary>
        private static void Fumes(string name, Area pool, float alpha = 0.16f)
        {
            Hazes.Add(new Haze("Fumes_" + name, pool.CenterX, pool.Top + 2f, pool.Width, 2f,
                alpha, pool.Width * 0.07f, true, Glow.Acid));

            for (float x = pool.X + 3f; x < pool.Right; x += 9f)
            {
                Shine(x, pool.Top + 1.5f, 7f, 0.7f, Glow.Acid);
            }
        }

        /// <summary>Vapor preso no alto de uma sala.</summary>
        private static void Steam(string name, Area room, float alpha = 0.10f)
        {
            Hazes.Add(new Haze("Steam_" + name, room.CenterX, room.Top - 1.5f, room.Width, 3f,
                alpha, room.Width * 0.05f, false, Glow.Steam));
        }

        // ------------------------------------------------------------------
        // Nomes de tiles
        // ------------------------------------------------------------------


        // objects_misc.png e bg_objects.png, os mesmos da Phase 1.
        private static string Crate(int variant) => $"misc_c{variant}_r0";
        private static string Debris(int variant) => $"misc_c{4 + variant}_r0";
        private static string Fence(int variant) => $"misc_c{variant}_r2";
        private static string GreyRock(int variant) => $"bgobj_c{variant}_r1";
        private static string DarkRock(int variant) => $"bgobj_c{variant}_r0";
        private static string DarkWeed(int variant) => $"bgobj_c{variant}_r2";

        /// <summary>O losango grande de minerio ou cristal: 2x2 tiles do cave_tileset.</summary>
        private static void Ore(int band, int x, int y, PropLayer layer)
        {
            Put(Phase01Layout.TileName(1, band + 2), x, y + 1, layer);
            Put(Phase01Layout.TileName(2, band + 2), x + 1, y + 1, layer);
            Put(Phase01Layout.TileName(1, band + 3), x, y, layer);
            Put(Phase01Layout.TileName(2, band + 3), x + 1, y, layer);
        }

        public static Material MaterialAt(int x, int y)
        {
            foreach ((Area area, Material material) in MaterialZones)
            {
                if (area.Contains(x, y)) return material;
            }

            return Material.Brass;
        }

        public static bool IsVoid(int x, int y)
        {
            foreach (Area area in Voids)
            {
                if (area.Contains(x, y)) return true;
            }

            return false;
        }

        public static bool IsRock(int x, int y) => Map.IsRock(x, y);
        public static bool IsSolid(int x, int y) => Map.IsSolid(x, y);
        public static bool IsAcid(int x, int y) => Map.IsAcid(x, y);
        public static bool IsStandable(int x, int y) => Map.IsStandable(x, y);

        /// <summary>O tile de terreno de uma celula, ou null se ela e ar ou acido.</summary>
        public static string GroundTileName(int x, int y)
        {
            LevelMap.TilePick pick = Map.PickGround(x, y);
            if (pick.Kind == LevelMap.TileKind.None) return null;

            Material material = MaterialAt(x, y);
            bool grate = Map[x, y] == LevelMap.Grate;

            // Grade so existe em latao.
            if (grate) return $"brass_c{pick.Column}_r{BrassArt.ThinRow}";

            switch (material)
            {
                case Material.Rock:
                    if (pick.Kind == LevelMap.TileKind.Core) return Phase01Layout.CoreTileName;
                    if (pick.Kind == LevelMap.TileKind.Thin) return Phase01Layout.TileName(4 + pick.Column, 13);
                    return Phase01Layout.TileName(pick.Column, Phase01Layout.RustBand + pick.Row);

                case Material.Crystal:
                    if (pick.Kind == LevelMap.TileKind.Core) return Phase01Layout.CoreTileName;
                    if (pick.Kind == LevelMap.TileKind.Thin) return Phase01Layout.TileName(pick.Column, 14);
                    return Phase01Layout.TileName(pick.Column, Phase01Layout.BlueBand + pick.Row);

                default:
                    if (pick.Kind == LevelMap.TileKind.Thin) return $"brass_c{4 + pick.Column}_r{BrassArt.ThinRow}";
                    if (pick.Kind == LevelMap.TileKind.Edge) return $"brass_c{pick.Column}_r{pick.Row}";
                    return BrassCoreName(x, y);
            }
        }

        /// <summary>
        /// O miolo de uma parede de latao. A maioria e chapa lisa; as outras
        /// sete variacoes entram aos poucos, senao a parede vira xadrez.
        /// </summary>
        private static string BrassCoreName(int x, int y)
        {
            int roll = LevelMap.Pick(x * 3 + 11, y * 5 + 7, 100);

            int variant;
            if (roll < 50) variant = 0;
            else if (roll < 62) variant = 7;
            else if (roll < 74) variant = 1;
            else if (roll < 82) variant = 2;
            else if (roll < 89) variant = 6;
            else if (roll < 94) variant = 3;
            else if (roll < 97) variant = 4;
            else variant = 5;

            return $"brass_c{variant % 4}_r{2 + variant / 4}";
        }

        /// <summary>
        /// A parede do fundo de uma celula, ou null onde nao ha: nos saloes
        /// abertos e atras de rocha macica (que e opaca, entao seria tile a toa).
        /// </summary>
        public static string BackgroundTileName(int x, int y)
        {
            if (IsVoid(x, y)) return null;
            if (IsDeepRock(x, y)) return null;

            switch (MaterialAt(x, y))
            {
                case Material.Rock:
                    return $"bgcave_c{LevelMap.Pick(x + 7, y + 13, 3)}_r6";

                case Material.Crystal:
                    return $"bgblue_c{LevelMap.Pick(x + 7, y + 13, 3)}_r6";
            }

            // De vez em quando um detalhe avulso; no resto, emendas a cada 3
            // colunas e 2 linhas, que formam paineis maiores que um tile.
            int roll = LevelMap.Pick(x * 5 + 1, y * 11 + 2, 60);
            if (roll == 0) return $"brass_c5_r{BrassArt.BackRow}";
            if (roll <= 2) return $"brass_c4_r{BrassArt.BackRow}";
            if (roll <= 4) return $"brass_c6_r{BrassArt.BackRow}";
            if (roll <= 8) return $"brass_c7_r{BrassArt.BackRow}";

            int seams = (x % 3 == 2 ? 1 : 0) + (y % 2 == 0 ? 2 : 0);
            return $"brass_c{seams}_r{BrassArt.BackRow}";
        }

        /// <summary>Rocha cercada de rocha por todos os lados, diagonais incluidas.</summary>
        private static bool IsDeepRock(int x, int y)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (!Map.IsRock(x + dx, y + dy)) return false;
                }
            }

            return true;
        }

        /// <summary>O tile de acido de uma celula, ou null. A linha de cima de cada poca e a superficie.</summary>
        public static string AcidTileName(int x, int y)
        {
            if (!Map.IsAcid(x, y)) return null;

            if (!Map.IsAcid(x, y + 1)) return $"acid_surface_{x % BrassArt.AcidWaveColumns}";

            int variant = LevelMap.Pick(x, y, BrassArt.AcidBodyVariants);
            return $"acid_c{variant}_r{BrassArt.AcidBodyRow}";
        }
    }
}
