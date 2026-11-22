using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase02Layout
    {
        // ------------------------------------------------------------------
        // O material largado dos operarios
        // ------------------------------------------------------------------


        /// <summary>
        /// Espalha o material de trabalho pelo chao livre de cada sala: um
        /// grupinho a cada tantos tiles, sempre em chao de rocha, com ar em
        /// cima, longe (pelo menos dois tiles) de tudo o que se usa, das
        /// colunas de vapor, do acido, das portas e das saidas. O
        /// CheckDecorPhysics confere o resultado.
        /// </summary>
        private static void Furnish()
        {
            var blocked = new HashSet<(int, int)>();

            void Block(int x, int y, int margin)
            {
                for (int dy = -margin; dy <= margin; dy++)
                {
                    for (int dx = -margin; dx <= margin; dx++) blocked.Add((x + dx, y + dy));
                }
            }

            void BlockArea(Area area, int margin)
            {
                for (int y = area.Y - margin; y < area.Y + area.Height + margin; y++)
                {
                    for (int x = area.X - margin; x < area.X + area.Width + margin; x++) blocked.Add((x, y));
                }
            }

            foreach (Marker marker in Markers) Block(marker.X, marker.Y, 2);
            foreach (LoreSpec lore in Lore) Block(lore.X, lore.Y, 2);
            foreach (RewardSpec reward in Rewards) Block(reward.X, reward.Y, 2);
            foreach ((string _, int x, int y) in Workbenches)
            {
                for (int dx = -2; dx <= 4; dx++) Block(x + dx, y, 1);
            }

            foreach (ValveSpec valve in Valves)
            {
                Block(valve.StandX, valve.StandY, 2);
                Block(valve.X, valve.Y, 2);
            }

            foreach (BoilerSpec boiler in PressureBoilers)
            {
                Block(boiler.StandX, boiler.StandY, 2);
                BlockArea(Area.Between(boiler.X, boiler.Y, boiler.X + 3, boiler.Y + 5), 1);
            }

            foreach (Vent vent in Vents) BlockArea(vent.Zone, 1);
            foreach ((string _, Area area) in AcidPools) BlockArea(area, 2);
            foreach (GateSpec gate in Gates) BlockArea(gate.Area, 2);
            foreach (Exit exit in Exits) BlockArea(exit.Area, 2);
            foreach ((string _, Area area) in Veils) BlockArea(area, 2);
            foreach ((string _, Area area) in BreakableWalls) BlockArea(area, 2);
            foreach (Prop prop in Props) Block(prop.X, prop.Y, 0);
            foreach (Fixture fixture in Fixtures)
            {
                if (fixture.Kind == FixtureKind.Lamp || fixture.Kind == FixtureKind.Chain || fixture.Kind == FixtureKind.Railing) continue;
                (int w, int h) = FixtureSize(fixture);
                BlockArea(new Area(fixture.X, fixture.Y, w, h), 1);
            }

            Block(SpawnTile.X, SpawnTile.Y, 3);

            bool Fits(int x, int y, int w, int h)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    if (!Map.IsSolid(x + dx, y - 1)) return false;
                    for (int dy = 0; dy <= h; dy++)
                    {
                        if (Map[x + dx, y + dy] != LevelMap.Air || Map.IsAcid(x + dx, y + dy)) return false;
                        if (blocked.Contains((x + dx, y + dy))) return false;
                    }
                }

                return true;
            }

            void Place(FixtureKind kind, int x, int y)
            {
                // Um armario em cada tres esta aberto e vazio, como os de sempre.
                bool open = kind == FixtureKind.Locker && LevelMap.Pick(x, y, 3) == 1;
                Fixtures.Add(new Fixture(kind, DecorName(kind.ToString()), x, y, 0, open));
                (int w, int h) = FixtureSize(new Fixture(kind, "", x, y));
                BlockArea(new Area(x, y, w, h), 1);
            }

            foreach ((string name, Area room) in Rooms)
            {
                if (!Clutter.TryGetValue(name, out FixtureKind[] palette)) continue;

                int pick = 0;
                for (int y = room.Y; y < room.Y + room.Height; y++)
                {
                    int sinceLast = 6;
                    for (int x = room.X; x < room.X + room.Width; x++, sinceLast++)
                    {
                        if (sinceLast < 7) continue;

                        FixtureKind kind = palette[(pick + LevelMap.Pick(x, y, 7)) % palette.Length];
                        (int w, int h) = FixtureSize(new Fixture(kind, "", x, y));
                        if (!Fits(x, y, w, h)) continue;

                        Place(kind, x, y);
                        pick++;
                        sinceLast = 0;

                        // Um caixote ganha outro em cima ou uma caixa de
                        // ferramentas; um saco, um vizinho.
                        if (kind == FixtureKind.Crate && Map[x, y + 2] == LevelMap.Air && !blocked.Contains((x, y + 2)))
                        {
                            Fixtures.Add(new Fixture(LevelMap.Pick(x, y, 2) == 0 ? FixtureKind.Crate : FixtureKind.Toolbox,
                                DecorName("Stack"), x, y + 1));
                        }
                        else if (kind == FixtureKind.Sack && Fits(x + 1, y, 1, 1))
                        {
                            Place(FixtureKind.Sack, x + 1, y);
                        }
                    }
                }
            }

            LightDarkCeilings();
        }

        /// <summary>
        /// Os tetos compridos sem nenhuma luz ganham lampioes de turno, um a
        /// cada tantos tiles, longe dos que ja existem e fora das colunas de
        /// vapor (o vapor sobe por onde ele ficaria pendurado).
        /// </summary>
        private static void LightDarkCeilings()
        {
            const int spacing = 13;
            var lamps = new List<int[]>();
            foreach (Fixture fixture in Fixtures)
            {
                if (fixture.Kind == FixtureKind.Lamp) lamps.Add(new[] { fixture.X, fixture.Y });
            }

            bool NearLamp(int x, int y)
            {
                foreach (int[] lamp in lamps)
                {
                    if (Math.Abs(lamp[0] - x) < spacing && Math.Abs(lamp[1] - y) < 8) return true;
                }

                return false;
            }

            bool InVent(int x, int y)
            {
                foreach (Vent vent in Vents)
                {
                    Area zone = vent.Zone;
                    if (x >= zone.X - 1 && x <= zone.X + zone.Width && y >= zone.Y - 1 && y <= zone.Y + zone.Height) return true;
                }

                return false;
            }

            foreach ((string name, Area room) in Rooms)
            {
                if (name == "09_BoilerRoom") continue;

                for (int y = room.Y + 3; y < room.Y + room.Height + 1; y++)
                {
                    for (int x = room.X + 1; x < room.X + room.Width - 1; x++)
                    {
                        // O lampiao ocupa (x, y-2) e (x, y-1), pendurado da rocha em y.
                        if (!Map.IsRock(x, y) || Map[x, y - 1] != LevelMap.Air || Map[x, y - 2] != LevelMap.Air) continue;
                        if (Map[x, y - 3] != LevelMap.Air || Map.IsAcid(x, y - 3)) continue;
                        if (NearLamp(x, y - 2) || InVent(x, y - 2)) continue;

                        Lamp(x, y - 1, 0.65f);
                        lamps.Add(new[] { x, y - 2 });
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Decoracao dos operarios
        // ------------------------------------------------------------------


        private static string DecorName(string kind) => $"{kind}_{_decor++}";

        /// <summary>Um lampiao pendurado, encostado no teto. topAir e a ultima linha de ar embaixo do teto.</summary>
        private static void Lamp(int x, int topAir, float intensity = 0.8f)
        {
            Fix(FixtureKind.Lamp, DecorName("Lamp"), x, topAir - 1);
            Shine(x + 0.5f, topAir - 1.2f, 6.5f, intensity, Glow.Lamp);
        }

        /// <summary>Guarda-corpo em cima de um chao (y e a linha onde se pisa).</summary>
        private static void Rail(int x0, int x1, int y)
        {
            Fixtures.Add(new Fixture(FixtureKind.Railing, DecorName("Rail"), x0, y, x1 - x0 + 1));
        }

        /// <summary>Corrente pendurada do teto, de topAir pra baixo.</summary>
        private static void HangChain(int x, int topAir, int length)
        {
            Fixtures.Add(new Fixture(FixtureKind.Chain, DecorName("Chain"), x, topAir - length + 1, length));
        }

        /// <summary>Uma fileira de armarios de operario no chao. Um em cada tres esta aberto e vazio.</summary>
        private static void Lockers(int x0, int y, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Fixtures.Add(new Fixture(FixtureKind.Locker, DecorName("Locker"), x0 + i, y, 0, i % 3 == 1));
            }
        }

        private static void Decor(FixtureKind kind, int x, int y) => Fix(kind, DecorName(kind.ToString()), x, y);
    }
}
