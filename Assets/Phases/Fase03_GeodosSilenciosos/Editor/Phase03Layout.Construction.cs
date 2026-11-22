using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase03Layout
    {
        // ------------------------------------------------------------------
        // Ferramentas de construcao
        // ------------------------------------------------------------------

        static Phase03Layout()
        {
            BuildArrival();
            BuildDarkTunnel();
            BuildFirstHunter();
            BuildFragileShaft();
            BuildRift();
            BuildGrotto();
            BuildNoteShaft();

            BuildEngravingGallery();
            BuildEchoChamber();
            BuildRuins();
            BuildHunterGallery();
            BuildResonantChamber();
            BuildFragileVault();
            BuildEastLoop();
            BuildBelfry();

            BuildPreBoss();
            BuildDrillChamber();

            BuildMineDressing();
            Furnish();
        }


        /// <summary>
        /// Espalha o material da mina pelo chao livre de cada sala: um grupinho
        /// a cada tantos tiles, sempre em chao de rocha, com ar em cima, longe
        /// (pelo menos um ou dois tiles) de tudo o que se usa, dos cristais, das
        /// plataformas frageis, dos espinhos e dos portoes. O CheckDecorPhysics
        /// confere o resultado.
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

            void BlockRect(int x, int y, int w, int h, int margin)
            {
                for (int yy = y - margin; yy < y + h + margin; yy++)
                {
                    for (int xx = x - margin; xx < x + w + margin; xx++) blocked.Add((xx, yy));
                }
            }

            foreach (Marker marker in Markers) Block(marker.X, marker.Y, 2);
            foreach (LoreSpot lore in Lore) Block(lore.X, lore.Y, 2);
            foreach (LoreSpot lore in Purified) Block(lore.X, lore.Y, 2);
            foreach (RewardSpot reward in Rewards) Block(reward.X, reward.Y, 2);
            foreach (EchoSpot echo in Echoes) Block(echo.X, echo.Y, 1);
            foreach (ResonantSpot resonant in Resonants) Block(resonant.X, resonant.Y, 2);
            foreach (StatueSpot statue in Statues) Block(statue.X, statue.Y, 2);
            foreach (GateSpec gate in Gates) BlockRect(gate.X, gate.Y, gate.Width, gate.Height, 2);
            foreach ((int x0, int x1, int y) in Fragiles) BlockRect(x0, y, x1 - x0 + 1, 1, 2);
            foreach ((string _, int x0, int x1, int y0, int y1) in Slabs) BlockRect(x0, y0, x1 - x0 + 1, y1 - y0 + 1, 2);
            foreach ((string _, int x, int y, int h) in Barriers) BlockRect(x, y, 1, h, 2);
            foreach ((int x, int y) in SpawnCracks) Block(x, y, 2);
            foreach ((string _, int x, int y) in Workbenches) BlockRect(x - 2, y, 7, 3, 0);
            Block(SpawnTile.X, SpawnTile.Y, 3);

            foreach (DecorSpot spot in Decor)
            {
                if (spot.Sprite == "mine_post" || spot.Sprite == "mine_rail") continue;
                int w = Math.Max(1, (int)Math.Ceiling(spot.Tiled ? spot.Width : 2f));
                int h = Math.Max(1, (int)Math.Ceiling(spot.Tiled ? spot.Height : 2f));
                BlockRect((int)Math.Floor(spot.X), (int)Math.Floor(spot.Y), w, h, 1);
            }

            bool Fits(int x, int y, int w, int h)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    if (!Map.IsRock(x + dx, y - 1)) return false;
                    for (int dy = 0; dy <= h; dy++)
                    {
                        if (Map[x + dx, y + dy] != LevelMap.Air) return false;
                        if (blocked.Contains((x + dx, y + dy))) return false;
                    }

                    // Nada colado num espinho do lado.
                    if (Map.IsSpike(x + dx - 1, y) || Map.IsSpike(x + dx + 1, y)) return false;
                }

                return true;
            }

            foreach ((string name, int rx0, int ry0, int rx1, int ry1) in Rooms)
            {
                if (!Clutter.TryGetValue(name, out string[] palette)) continue;

                int pick = 0;
                for (int y = ry0; y <= ry1; y++)
                {
                    int sinceLast = 5;
                    for (int x = rx0; x <= rx1; x++, sinceLast++)
                    {
                        if (sinceLast < 6) continue;

                        string sprite = palette[(pick + LevelMap.Pick(x, y, 7)) % palette.Length];
                        GroundedSize(sprite, out int w, out int h);
                        if (!Fits(x, y, w, h)) continue;

                        Put(sprite, x, y, 4);
                        BlockRect(x, y, w, h, 1);
                        pick++;
                        sinceLast = 0;
                    }
                }
            }
        }

        /// <summary>
        /// A mina por cima de tudo, depois de escavado: os escoramentos de
        /// madeira nos corredores baixos, os trilhos por onde os vagonetes
        /// corriam e um vagonete tombado na galeria. So enfeite — nao mexe no
        /// mapa.
        /// </summary>
        private static void BuildMineDressing()
        {
            Support(44, 9);

            Support(81, 11);
            Support(99, 11);
            Rail(79, 111, 11);

            Support(14, 44);
            Support(22, 44);

            Rail(75, 108, 44);
            Put("mine_cart", 97, 44, 6);

            Support(124, 34);

            Support(176, 85);
            Support(191, 85);
            Support(172, 101);
            Support(186, 101);

            Support(100, 92);
            Support(116, 92);
        }

        private static void Carve(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Air);

        private static void Solid(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Rock);

        private static void Ledge(int x0, int x1, int y)
        {
            Map.Fill(x0, y, x1, y, LevelMap.Platform);
            LedgeSpans.Add((x0, x1, y));
        }

        /// <summary>Os ressaltos (plataformas de pedra), pra contar quantos ha por sala.</summary>
        private static readonly List<(int X0, int X1, int Y)> LedgeSpans = new List<(int, int, int)>();

        private static int LedgeCount(int x0, int y0, int x1, int y1)
        {
            int count = 0;
            foreach ((int lx0, int _, int ly) in LedgeSpans)
            {
                if (lx0 >= x0 && lx0 <= x1 && ly >= y0 && ly <= y1) count++;
            }

            return count;
        }

        private static void Spikes(int x0, int x1, int y) => Map.Fill(x0, y, x1, y, LevelMap.Spike);

        /// <summary>
        /// Uma plataforma fragil. Pro mapa ela e uma plataforma comum (da pra
        /// pisar); quem a desenha e a faz quebrar e o objeto dela.
        /// </summary>
        private static void Fragile(int x0, int x1, int y)
        {
            Map.Fill(x0, y, x1, y, LevelMap.Platform);
            Fragiles.Add((x0, x1, y));

            for (int x = x0; x <= x1; x++) FragileCells.Add((x, y));
        }

        /// <summary>Um chao da arena que desaba. Devolve o indice dele em Slabs.</summary>
        private static int Slab(string name, int x0, int x1, int y0, int y1)
        {
            Map.Fill(x0, y0, x1, y1, LevelMap.Platform);
            Slabs.Add((name, x0, x1, y0, y1));

            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++) FragileCells.Add((x, y));
            }

            return Slabs.Count - 1;
        }

        /// <summary>Uma barreira de cristal fechando uma coluna de celulas (que tem que estar escavada).</summary>
        private static void Barrier(string name, int x, int y, int height)
        {
            Barriers.Add((name, x, y, height));
        }

        private static void Room(string name, int x0, int y0, int x1, int y1)
        {
            Rooms.Add((name, x0, y0, x1, y1));
        }

        private static void Mark(string group, string name, int x, int y, bool mustReach = true)
        {
            Markers.Add(new Marker(group, name, x, y, mustReach));
        }

        private static void Resonant(string group, string name, int x, int y, int note, bool corrupted = false,
            int slab = -1, string door = null)
        {
            // Golpeia-se de lado: ele para na celula a esquerda do cristal.
            Resonants.Add(new ResonantSpot
            {
                Name = name, X = x, Y = y, Note = note, Corrupted = corrupted, StandX = x - 1, Slab = slab, Door = door,
            });
            Mark(group, name + "_Stand", x - 1, y);
        }

        private static void Statue(string group, string name, int kind, int order, int x, int y)
        {
            Statues.Add(new StatueSpot { Name = name, Kind = kind, Order = order, X = x, Y = y });
            Mark(group, name, x, y);
        }

        private static void Foe(FoeKind kind, string name, int x0, int x1, int y, int homeX = -1)
        {
            Foes.Add(new FoeSpot
            {
                Kind = kind,
                Name = name,
                X0 = x0,
                X1 = x1,
                Y = y,
                HomeX = homeX < 0 ? (x0 + x1) / 2 : homeX,
            });
        }

        private static void Tell(string kind, string group, string name, string title, int x, int y, params string[] lines)
        {
            Lore.Add(new LoreSpot { Kind = kind, Name = name, Title = title, X = x, Y = y, Lines = lines });
            Mark(group, name, x, y);
        }

        private static void Gate(string name, int x, int y, int width, int height, int moveX = 0, int moveY = 1)
        {
            Gates.Add(new GateSpec { Name = name, X = x, Y = y, Width = width, Height = height, MoveX = moveX, MoveY = moveY });
        }

        private static void Reward(string group, string id, int x, int y, int amount)
        {
            Rewards.Add(new RewardSpot { Id = id, X = x, Y = y, Amount = amount });
            Mark(group, id, x, y);
        }

        private static void Bench(string group, string name, int x, int y)
        {
            Workbenches.Add((name, x, y));
            Mark(group, name + "_Rest", x + 1, y);
        }

        private static void Put(string sprite, float x, float y, int order = 5) => Decor.Add(new DecorSpot(sprite, x, y, order));

        private static void Barrel(int x, int y) => Put("decor_barrel", x, y, 6);

        private static void Sign(int x, int y) => Put("decor_sign", x, y, 5);

        private static void Shelf(int x, int y) => Put("decor_shelf", x, y, 5);

        /// <summary>
        /// Um escoramento de mina: dois postes de madeira do chao ate o teto e a
        /// viga em cima, com x de largura (de x ate x + 3). O teto e procurado
        /// no mapa; se o vao for alto ou baixo demais, nao poe nada.
        /// </summary>
        private static void Support(int x, int floorY)
        {
            int ceiling = int.MaxValue;
            for (int column = x; column <= x + 3; column++)
            {
                int y = floorY;
                while (y < Height && !Map.IsRock(column, y)) y++;
                ceiling = Math.Min(ceiling, y);
            }

            int height = ceiling - floorY;
            if (height < 3 || height > 10) return;

            Decor.Add(new DecorSpot("mine_post", x, floorY, 3, 1f, height - 1));
            Decor.Add(new DecorSpot("mine_post", x + 3, floorY, 3, 1f, height - 1));
            Decor.Add(new DecorSpot("mine_beam", x, ceiling - 1, 4, 4f, 1f));
        }

        /// <summary>O trilho da mina, rente ao chao, de x0 ate x1.</summary>
        private static void Rail(int x0, int x1, int y) => Decor.Add(new DecorSpot("mine_rail", x0, y, 4, x1 - x0 + 1, 0.375f));

        /// <summary>Corrente pendurada do teto, de topAir pra baixo.</summary>
        private static void HangChain(int x, int topAir, int length)
        {
            Decor.Add(new DecorSpot("decor_chain", x, topAir - length + 1, 5, 1f, length));
        }

        /// <summary>
        /// Espalha cristais de eco por um retangulo: um no chao a cada tantas
        /// colunas, e um pendurado no teto no meio do caminho entre dois. Sao
        /// eles que acendem com o som — sem eles a sala nao aparece nunca.
        /// </summary>
        private static void Scatter(int x0, int y0, int x1, int y1, int every, int seed)
        {
            for (int x = x0 + 1; x <= x1 - 1; x += every)
            {
                int column = x + LevelMap.Pick(x, seed, 2);

                // O primeiro chao de cima pra baixo.
                for (int y = y1; y >= y0; y--)
                {
                    if (!Map.IsStandable(column, y)) continue;

                    Echoes.Add(new EchoSpot(column, y, LevelMap.Pick(column, seed + 1, CrystalArt.SizeCount),
                        LevelMap.Pick(column, seed + 2, CrystalArt.HueCount), false));
                    break;
                }

                int hang = column + every / 2;
                if (hang > x1 - 1) continue;

                // O primeiro teto de cima pra baixo: ar com rocha em cima.
                for (int y = y1; y >= y0; y--)
                {
                    if (Map.IsSolid(hang, y) || !Map.IsRock(hang, y + 1)) continue;

                    Echoes.Add(new EchoSpot(hang, y, LevelMap.Pick(hang, seed + 3, 2),
                        LevelMap.Pick(hang, seed + 4, CrystalArt.HueCount), true));
                    break;
                }
            }
        }

        // ------------------------------------------------------------------
        // Consultas
        // ------------------------------------------------------------------

        public static bool IsRock(int x, int y) => Map.IsRock(x, y);
        public static bool IsSolid(int x, int y) => Map.IsSolid(x, y);
        public static bool IsSpike(int x, int y) => Map.IsSpike(x, y);
        public static bool IsStandable(int x, int y) => Map.IsStandable(x, y);

        /// <summary>Em que chao desabavel uma celula esta (indice em Slabs), ou -1.</summary>
        public static int SlabAt(int x, int y)
        {
            for (int i = 0; i < Slabs.Count; i++)
            {
                (string _, int x0, int x1, int y0, int y1) = Slabs[i];
                if (x >= x0 && x <= x1 && y >= y0 && y <= y1) return i;
            }

            return -1;
        }

        /// <summary>A estacao de chegada ainda e latao: o elevador veio de la.</summary>
        private static bool IsBrass(int x, int y) => x <= 17 && y >= 8 && y <= 24;

        /// <summary>O tile de terreno de uma celula, ou null se e ar, espinho ou plataforma fragil.</summary>
        public static string GroundTileName(int x, int y)
        {
            if (FragileCells.Contains((x, y))) return null;

            LevelMap.TilePick pick = Map.PickGround(x, y);
            if (pick.Kind == LevelMap.TileKind.None) return null;

            if (IsBrass(x, y))
            {
                if (pick.Kind == LevelMap.TileKind.Thin) return $"brass_c{4 + pick.Column}_r{BrassArt.ThinRow}";
                if (pick.Kind == LevelMap.TileKind.Edge) return $"brass_c{pick.Column}_r{pick.Row}";
                return "brass_c0_r2";
            }

            // Rocha azulada, a mesma banda da saida do District 2.
            if (pick.Kind == LevelMap.TileKind.Core) return Phase01Layout.CoreTileName;
            if (pick.Kind == LevelMap.TileKind.Thin) return Phase01Layout.TileName(pick.Column, 14);
            return Phase01Layout.TileName(pick.Column, Phase01Layout.BlueBand + pick.Row);
        }

        /// <summary>A parede do fundo, ou null atras de rocha macica e na fenda da galeria.</summary>
        public static string BackgroundTileName(int x, int y)
        {
            if (IsRift(x, y)) return null;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (!Map.IsRock(x + dx, y + dy)) return $"bgblue_c{LevelMap.Pick(x + 7, y + 13, 3)}_r6";
                }
            }

            return null;
        }

        /// <summary>Os espinhos de cristal usam o mesmo desenho dos de sucata; a cor muda no tilemap.</summary>
        public static string SpikeTileName(int x, int y) => $"misc_c{3 + LevelMap.Pick(x, y, 2) * 2}_r2";
    }
}
