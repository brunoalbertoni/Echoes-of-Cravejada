using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase04Layout
    {
        // ------------------------------------------------------------------
        // Ferramentas de construcao
        // ------------------------------------------------------------------

        static Phase04Layout()
        {
            BuildLobby();
            BuildGallery();
            BuildAtelier();

            BuildDistillery();
            BuildOptics();
            BuildIncubation();

            BuildBossRoute();
            BuildLaboratory();
            BuildGuardCorridor();
            BuildAmphitheater();
            BuildGrandStair();

            Furnish();
        }


        /// <summary>
        /// Espalha o que sobrou pelo chao livre de cada sala: um grupinho a cada
        /// tantos tiles, em chao de marmore, com ar em cima, longe de tudo o que
        /// se usa, do mercurio, dos circulos, espelhos, fechaduras, portoes,
        /// colunas de vapor e espinhos. O CheckPropPhysics confere o resultado.
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

            void BlockCells(Cells area, int margin)
            {
                for (int y = area.Y0 - margin; y <= area.Y1 + margin; y++)
                {
                    for (int x = area.X0 - margin; x <= area.X1 + margin; x++) blocked.Add((x, y));
                }
            }

            foreach (Marker marker in Markers) Block(marker.X, marker.Y, 2);
            foreach (LoreSpot lore in Lore) Block(lore.X, lore.Y, 2);
            foreach (LoreSpot lore in Purified) Block(lore.X, lore.Y, 2);
            foreach (Reagent reagent in Reagents) Block(reagent.X, reagent.Y, 2);
            foreach ((string _, int x, int y) in Workbenches)
            {
                for (int dx = -2; dx <= 4; dx++) Block(x + dx, y, 1);
            }

            foreach (Pool pool in Pools) BlockCells(pool.Area, 2);
            foreach (Block block in Blocks) BlockCells(block.Area, 1);
            foreach (Circle circle in Circles) Block(circle.X, circle.Y, 2);
            foreach (MirrorSpec mirror in Mirrors) Block(mirror.X, mirror.Y, 2);
            foreach (LockSpec lockSpec in Locks) Block(lockSpec.X, lockSpec.Y, 2);
            foreach (GateSpec gate in Gates) BlockCells(new Cells(gate.X, gate.Y, gate.X + gate.Width - 1, gate.Y + gate.Height - 1), 2);
            foreach ((string _, Cells area) in VaporLifts) BlockCells(area, 1);
            foreach ((string _, int x, int y) in ArenaCircles) Block(x, y, 2);
            foreach ((int x, int y) in ObjectCells) Block(x, y, 1);
            foreach ((int _, int x, int y) in Windows) Block(x + 1, y, 1);

            foreach (Prop prop in Props)
            {
                PixelCanvas c = LabCanvas(prop.Sprite);
                int w = c != null ? (int)Math.Ceiling(c.Width / 16f) : 2;
                int h = c != null ? (int)Math.Ceiling(c.Height / 16f) : 2;
                int x0 = (int)Math.Floor(prop.X - w / 2f);
                int y0 = (int)Math.Floor(prop.Hanging ? prop.Y - h : prop.Y);
                BlockCells(new Cells(x0, y0, x0 + w - 1, y0 + h - 1), 1);
            }

            bool Fits(int x, int y, int w, int h)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    if (!Map.IsRock(x + dx, y - 1) || ObjectCells.Contains((x + dx, y - 1))) return false;
                    for (int dy = 0; dy <= h; dy++)
                    {
                        if (Map[x + dx, y + dy] != LevelMap.Air || blocked.Contains((x + dx, y + dy))) return false;
                    }

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
                        PixelCanvas c = LabCanvas(sprite);
                        int w = (int)Math.Ceiling(c.Width / 16f);
                        int h = (int)Math.Ceiling(c.Height / 16f);
                        if (!Fits(x, y, w, h)) continue;

                        // O pivo do laboratorio e o meio da base.
                        Put(sprite, x + w / 2f, y, 4);
                        BlockCells(new Cells(x, y, x + w - 1, y + h - 1), 1);
                        pick++;
                        sinceLast = 0;
                    }
                }
            }
        }

        private static void Carve(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Air);

        private static void Solid(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Rock);

        private static void Ledge(int x0, int x1, int y) => Map.Fill(x0, y, x1, y, LevelMap.Platform);

        private static void Spikes(int x0, int x1, int y) => Map.Fill(x0, y, x1, y, LevelMap.Spike);

        private static void Room(string name, int x0, int y0, int x1, int y1) => Rooms.Add((name, x0, y0, x1, y1));

        private static void Mark(string group, string name, int x, int y, bool mustReach = true)
        {
            Markers.Add(new Marker(group, name, x, y, mustReach));
        }

        /// <summary>
        /// Uma poca de mercurio no chao: as celulas dela viram chao comum pro
        /// mapa (da pra andar por cima), e quem as desenha e o objeto.
        /// </summary>
        private static void Mercury(string name, int x0, int x1, int top, float current = 0f)
        {
            var area = new Cells(x0, top - 1, x1, top);

            Map.Fill(area.X0, area.Y0, area.X1, area.Y1, LevelMap.Platform);
            Pools.Add(new Pool { Name = name, Area = area, Current = current });

            for (int y = area.Y0; y <= area.Y1; y++)
            {
                for (int x = area.X0; x <= area.X1; x++) ObjectCells.Add((x, y));
            }
        }

        private static void Transmutable(string name, BlockKind kind, int x0, int y0, int x1, int y1)
        {
            Blocks.Add(new Block { Name = name, Kind = kind, Area = new Cells(x0, y0, x1, y1) });
        }

        private static void Rune(string group, string name, string effect, int x, int y, string[] vanish, string[] appear)
        {
            Circles.Add(new Circle { Name = name, Effect = effect, X = x, Y = y, Vanish = vanish, Appear = appear });
            Mark(group, name, x, y);
        }

        private static void Gate(string name, int x, int y, int width, int height, int moveX = 0, int moveY = 1, bool disguised = false)
        {
            Gates.Add(new GateSpec { Name = name, X = x, Y = y, Width = width, Height = height, MoveX = moveX, MoveY = moveY, Disguised = disguised });
        }

        /// <summary>Um emissor de luz-guia: sempre aceso, frio, pra dobrar nos espelhos.</summary>
        private static void GuideLight(string name, int x, int y, int dirX, int dirY)
        {
            Beams.Add(new Beam { Name = name, X = x, Y = y, DirX = dirX, DirY = dirY, Guide = true });
        }

        /// <summary>Um raio-perigo: liga e desliga em ritmo, e queima.</summary>
        private static void HazardBeam(string name, int x, int y, int dirX, int dirY, float on, float off, float offset)
        {
            Beams.Add(new Beam { Name = name, X = x, Y = y, DirX = dirX, DirY = dirY, On = on, Off = off, Offset = offset });
        }

        private static void Mirror(string group, string name, int x, int y, bool slash, int standX, int standY)
        {
            Mirrors.Add(new MirrorSpec { Name = name, X = x, Y = y, Slash = slash, StandX = standX, StandY = standY });
            Mark(group, name + "_Stand", standX, standY);
        }

        private static void CrystalLock(string name, int x, int y, string gate, bool hidden = false)
        {
            Locks.Add(new LockSpec { Name = name, X = x, Y = y, Gate = gate, Hidden = hidden });
        }

        private static void Enemy(FoeKind kind, string name, int x0, int x1, int y, int homeX = -1)
        {
            Foes.Add(new Foe { Kind = kind, Name = name, X0 = x0, X1 = x1, Y = y, HomeX = homeX < 0 ? (x0 + x1) / 2 : homeX });
        }

        private static void Tell(string kind, string group, string name, string title, int x, int y, params string[] lines)
        {
            Lore.Add(new LoreSpot { Kind = kind, Name = name, Title = title, X = x, Y = y, Lines = lines });
            Mark(group, name, x, y);
        }

        private static void Bench(string group, string name, int x, int y)
        {
            Workbenches.Add((name, x, y));
            Mark(group, name + "_Rest", x + 1, y);
        }

        private static void AddReagent(string group, string key, string name, int index, int x, int y)
        {
            Reagents.Add(new Reagent { Id = ReagentPrefix + key, Name = name, Index = index, X = x, Y = y });
            Mark(group, ReagentPrefix + key, x, y);
        }

        private static void Put(string sprite, float x, float y, int order = 5) => Props.Add(new Prop(sprite, x, y, order, false));

        /// <summary>Um enfeite pendurado: y e a primeira celula de rocha do teto.</summary>
        private static void Hang(string sprite, float x, float ceiling, int order = 4) => Props.Add(new Prop(sprite, x, ceiling, order, true));

        private static void Lamp(float x, float y, float radius, bool green = false) => Lamps.Add((x, y, radius, green));

        // ------------------------------------------------------------------
        // Consultas
        // ------------------------------------------------------------------

        public static bool IsRock(int x, int y) => Map.IsRock(x, y);
        public static bool IsSolid(int x, int y) => Map.IsSolid(x, y);
        public static bool IsSpike(int x, int y) => Map.IsSpike(x, y);
        public static bool IsStandable(int x, int y) => Map.IsStandable(x, y);

        /// <summary>O tile de terreno de uma celula, ou null se e ar, espinho ou objeto.</summary>
        public static string GroundTileName(int x, int y)
        {
            if (ObjectCells.Contains((x, y))) return null;

            LevelMap.TilePick pick = Map.PickGround(x, y);
            if (pick.Kind == LevelMap.TileKind.None) return null;

            // O marmore tem a mesma grade do latao.
            if (pick.Kind == LevelMap.TileKind.Thin) return $"marble_c{4 + pick.Column}_r{BrassArt.ThinRow}";
            if (pick.Kind == LevelMap.TileKind.Edge) return $"marble_c{pick.Column}_r{pick.Row}";

            // Miolo: quase sempre a chapa lisa; as outras variacoes entram aos poucos.
            int roll = LevelMap.Pick(x * 3 + 11, y * 5 + 7, 100);
            int variant = roll < 60 ? 0 : (roll < 74 ? 7 : (roll < 86 ? 1 : (roll < 94 ? 2 : 6)));

            return $"marble_c{variant % 4}_r{2 + variant / 4}";
        }

        /// <summary>A parede do fundo, ou null atras de rocha macica.</summary>
        public static string BackgroundTileName(int x, int y)
        {
            bool open = false;

            for (int dy = -1; dy <= 1 && !open; dy++)
            {
                for (int dx = -1; dx <= 1 && !open; dx++) open = !Map.IsRock(x + dx, y + dy);
            }

            if (!open) return null;

            // Paineis de tres colunas por duas linhas, como lambris.
            int seams = (x % 3 == 2 ? 1 : 0) + (y % 2 == 0 ? 2 : 0);
            return $"marble_c{seams}_r{BrassArt.BackRow}";
        }

        public static string SpikeTileName(int x, int y) => $"misc_c{3 + LevelMap.Pick(x, y, 2) * 2}_r2";
    }
}
