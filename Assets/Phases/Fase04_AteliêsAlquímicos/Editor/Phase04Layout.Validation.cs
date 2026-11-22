using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase04Layout
    {
        // ------------------------------------------------------------------
        // Validacao
        // ------------------------------------------------------------------

        /// <summary>
        /// As regras de alcance desta fase: as com folga, mais o Steam Dash, o
        /// Pressure Jump e o pulo de parede — as mesmas do District 3.
        /// </summary>
        private static LevelMap.JumpRules Rules()
        {
            LevelMap.JumpRules rules = LevelMap.JumpRules.Comfortable();
            rules.MaxRise = 4;
            rules.MaxRun += 5;
            rules.WallJump = true;
            return rules;
        }

        private static void Put(LevelMap world, Cells area, char what)
        {
            world.Fill(area.X0, area.Y0, area.X1, area.Y1, what);
        }

        private static Block BlockNamed(string name) => Blocks.Find(block => block.Name == name);

        /// <summary>
        /// O mapa num certo ponto do progresso. Cada circulo ja polido tira o
        /// que ele desfaz e poe o que ele cria; os portoes fora de open fecham.
        /// </summary>
        private static LevelMap World(ICollection<string> lit, ICollection<string> open, List<LevelMap.Lift> lifts)
        {
            LevelMap world = Map.Clone();

            foreach (Circle circle in Circles)
            {
                bool done = lit.Contains(circle.Name);

                // Antes de polir: o que ele desfaz ainda esta la, o que ele cria nao.
                foreach (string name in done ? circle.Appear : circle.Vanish)
                {
                    Block block = BlockNamed(name);
                    if (block != null) Put(world, block.Area, block.Kind == BlockKind.Silver ? LevelMap.Platform : LevelMap.Rock);

                    if (!done) continue;

                    (string Name, Cells Area) lift = VaporLifts.Find(vapor => vapor.Name == name);
                    if (lift.Name != null) lifts.Add(new LevelMap.Lift(lift.Area.X0, lift.Area.X1, lift.Area.Y0, lift.Area.Y1));
                }

                if (!done) continue;

                // Poca transmutada: sobra o buraco dela.
                foreach (string name in circle.Vanish)
                {
                    Pool pool = Pools.Find(candidate => candidate.Name == name);
                    if (pool != null) Put(world, pool.Area, LevelMap.Air);
                }
            }

            foreach (GateSpec gate in Gates)
            {
                if (!open.Contains(gate.Name)) world.Fill(gate.X, gate.Y, gate.X + gate.Width - 1, gate.Y + gate.Height - 1, LevelMap.Rock);
            }

            return world;
        }

        private static LevelMap.Reach Explore(ICollection<string> lit, ICollection<string> open, (int X, int Y)? from = null)
        {
            var lifts = new List<LevelMap.Lift>();
            LevelMap world = World(lit, open, lifts);
            return world.Explore(from ?? SpawnTile, Rules(), lifts);
        }

        private static Circle CircleNamed(string name) => Circles.Find(circle => circle.Name == name);

        private static Reagent ReagentNamed(string key) => Reagents.Find(reagent => reagent.Id == ReagentPrefix + key);

        /// <summary>
        /// Confere se a fase e jogavel de verdade, sem precisar jogar: segue a
        /// ordem em que as coisas abrem e, a cada passo, confere que o proximo
        /// passo esta ao alcance e que nao da pra pular nenhum. Cada ala e
        /// conferida sozinha, partindo do centro: a ordem das alas e livre.
        ///
        /// Devolve a lista de problemas. Vazia quer dizer que esta tudo certo.
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

            CheckElevator(problems);

            void Need(LevelMap.Reach reach, int x, int y, string what)
            {
                if (!reach.CanStand(x, y)) problems.Add($"Nao da pra chegar em {what} ({x}, {y}).");
            }

            void Forbid(LevelMap.Reach reach, int x, int y, string what)
            {
                if (reach.CanStand(x, y)) problems.Add($"Da pra chegar em {what} ({x}, {y}) cedo demais.");
            }

            void NeedMirror(LevelMap.Reach reach, string name)
            {
                MirrorSpec mirror = Mirrors.Find(m => m.Name == name);
                Need(reach, mirror.StandX, mirror.StandY, "o espelho " + name);
            }

            void ForbidMirror(LevelMap.Reach reach, string name)
            {
                MirrorSpec mirror = Mirrors.Find(m => m.Name == name);
                Forbid(reach, mirror.StandX, mirror.StandY, "o espelho " + name);
            }

            void NeedReagent(LevelMap.Reach reach, string key) => Need(reach, ReagentNamed(key).X, ReagentNamed(key).Y, "o reagente " + key);

            void ForbidReagent(LevelMap.Reach reach, string key) => Forbid(reach, ReagentNamed(key).X, ReagentNamed(key).Y, "o reagente " + key);

            // A luz de um emissor, com os espelhos de flipped virados do jeito
            // contrario ao inicial e os portoes de open abertos: em que fechadura bate.
            void ExpectLight(string beam, string[] flipped, ICollection<string> open, string expected, string what)
            {
                string hit = TraceLight(BeamNamed(beam), flipped, open, null);
                if (hit != expected) problems.Add($"{what}: a luz de {beam} bate em {hit ?? "nada"}, e devia bater em {expected ?? "nada"}.");
            }

            var none = new string[0];

            // 1. Nada feito: o centro, a Destilaria e a Incubacao inteiras, e a
            // Optica so ate a Sala das Lentes. Nenhuma ala depende de outra.
            LevelMap.Reach start = Explore(none, none);
            Need(start, CrisolTile.X, CrisolTile.Y, "Crisol");
            Need(start, AlembicTile.X, AlembicTile.Y, "o Grande Alambique");
            NeedReagent(start, "Mercury");
            NeedReagent(start, "Salt");
            NeedMirror(start, "Mirror_Lenses");
            ForbidMirror(start, "Mirror_A");
            ForbidReagent(start, "Light");
            Forbid(start, SecretChassisTile.X, SecretChassisTile.Y, "o chassi");
            Forbid(start, BossTile.X, BossTile.Y, "o Grande Laboratorio");
            Forbid(start, 117, 62, "a bancada pre-boss");

            // 2. O primeiro enigma: so o espelho do pedestal virado acende a fechadura.
            ExpectLight("Guide_Lenses", none, none, null, "Sala das Lentes, de inicio");
            ExpectLight("Guide_Lenses", new[] { "Mirror_Lenses" }, none, "Lock_Lenses", "Sala das Lentes, resolvida");

            var lenses = new List<string> { LensesGate };
            LevelMap.Reach chamber = Explore(none, lenses);
            foreach (string name in new[] { "Mirror_A", "Mirror_B", "Mirror_C", "Mirror_D", "Mirror_E" }) NeedMirror(chamber, name);
            ForbidReagent(chamber, "Light");
            Forbid(chamber, SecretChassisTile.X, SecretChassisTile.Y, "o chassi");

            // 3. A Camara dos Espelhos: de inicio a luz nao acende nada; um
            // espelho so nao basta; A, B, C e E virados levam ao nicho; D
            // virado no lugar do E leva ao cofre.
            string[] mainRoute = { "Mirror_A", "Mirror_B", "Mirror_C", "Mirror_E" };
            string[] vaultRoute = { "Mirror_A", "Mirror_B", "Mirror_C", "Mirror_D" };
            ExpectLight("Guide_Mirrors", none, lenses, null, "Camara dos Espelhos, de inicio");
            foreach (MirrorSpec mirror in Mirrors)
            {
                if (mirror.Name == "Mirror_Lenses") continue;
                string hit = TraceLight(BeamNamed("Guide_Mirrors"), new[] { mirror.Name }, lenses, null);
                if (hit != null) problems.Add($"Camara dos Espelhos: virar so o {mirror.Name} ja acende {hit}.");
            }

            ExpectLight("Guide_Mirrors", mainRoute, lenses, "Lock_Mirrors", "Camara dos Espelhos, resolvida");
            ExpectLight("Guide_Mirrors", vaultRoute, lenses, "Lock_Vault", "Camara dos Espelhos, o segredo");

            var optics = new List<string>(lenses) { MirrorsGate };
            LevelMap.Reach wings = Explore(none, optics);
            NeedReagent(wings, "Light");
            Forbid(wings, SecretChassisTile.X, SecretChassisTile.Y, "o chassi (sem o cofre)");
            Forbid(wings, BossTile.X, BossTile.Y, "o Grande Laboratorio (sem o alambique)");

            // 4. O cofre: so com a fechadura escondida acesa.
            LevelMap.Reach vault = Explore(none, new List<string>(lenses) { VaultGate });
            Need(vault, SecretChassisTile.X, SecretChassisTile.Y, "o chassi");

            // 5. Alambique aberto: o chefe, e nao alem.
            var withAlembic = new List<string>(optics) { AlembicGate };
            LevelMap.Reach boss = Explore(none, withAlembic);
            Need(boss, BossTile.X, BossTile.Y, "o Grande Laboratorio");
            Need(boss, 117, 62, "a bancada pre-boss");
            foreach ((string name, int x, int y) in ArenaCircles) Need(boss, x, y, name);
            Forbid(boss, StageCircle.X, StageCircle.Y, "o palco");

            // 6. O Alquimista caido: o corredor ate o palco e a Grande Escadaria.
            var all = new List<string>(withAlembic) { BossExitGate, VaultGate };
            LevelMap.Reach after = Explore(none, all);
            Need(after, StageCircle.X, StageCircle.Y, "o palco");
            Need(after, StairTop.X, StairTop.Y, "o alto da Grande Escadaria");

            // E da pra voltar do palco, do chassi e do fundo da Destilaria ao centro.
            Need(Explore(none, all, StageCircle), CrisolTile.X, CrisolTile.Y, "o atelie, voltando do palco");
            Need(Explore(none, all, SecretChassisTile), CrisolTile.X, CrisolTile.Y, "o atelie, voltando do chassi");
            Need(Explore(none, all, (ReagentNamed("Mercury").X, ReagentNamed("Mercury").Y)), CrisolTile.X, CrisolTile.Y,
                "o atelie, voltando do alto da Destilaria");

            foreach (Marker marker in Markers)
            {
                if (!marker.MustReach) continue;

                if (!Map.IsStandable(marker.X, marker.Y)) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) nao e um lugar onde da pra ficar em pe.");
                else if (!after.CanStand(marker.X, marker.Y)) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) esta fora de alcance.");
            }

            foreach (LoreSpot spot in Purified)
            {
                if (!after.CanStand(spot.X, spot.Y)) problems.Add($"{spot.Name} ({spot.X}, {spot.Y}) fica fora de alcance.");
            }

            foreach (Foe foe in Foes)
            {
                for (int x = foe.X0; x <= foe.X1; x++)
                {
                    if (!Map.IsStandable(x, foe.Y)) problems.Add($"{foe.Name} anda por ({x}, {foe.Y}), onde nao da pra ficar em pe.");
                }

                if (foe.X1 > foe.X0)
                {
                    // Patrulha usa o chao inteiro: nao para no meio de um trecho livre.
                    if (Map.IsStandable(foe.X0 - 1, foe.Y) || Map.IsStandable(foe.X1 + 1, foe.Y))
                    {
                        problems.Add($"{foe.Name} nao usa o chao inteiro ({foe.X0}..{foe.X1}, {foe.Y}).");
                    }
                }
            }

            foreach (Beam beam in Beams) CheckBeam(problems, beam);

            foreach (MirrorSpec mirror in Mirrors)
            {
                if (Map.IsSolid(mirror.X, mirror.Y)) problems.Add($"O espelho {mirror.Name} ({mirror.X}, {mirror.Y}) esta dentro do terreno.");
            }

            foreach (LockSpec spec in Locks)
            {
                if (Map.IsSolid(spec.X, spec.Y)) problems.Add($"A fechadura {spec.Name} ({spec.X}, {spec.Y}) esta dentro do terreno.");
                if (!Gates.Exists(gate => gate.Name == spec.Gate)) problems.Add($"A fechadura {spec.Name} abre {spec.Gate}, que nao existe.");
            }

            CheckPropPhysics(problems);
            return problems;
        }


        private static PixelCanvas LabCanvas(string sprite)
        {
            if (_labArt == null)
            {
                _labArt = new Dictionary<string, PixelCanvas>();
                foreach ((string name, Func<PixelCanvas> draw, bool _) in LabArt.Sprites()) _labArt[name] = draw();
                foreach ((string name, PixelCanvas canvas) in ExtraLabArt()) _labArt[name] = canvas;
            }

            return _labArt.TryGetValue(sprite, out PixelCanvas c) ? c : null;
        }

        /// <summary>As pecas novas do laboratorio (ver LabClutterArt), pra medir junto.</summary>
        private static IEnumerable<(string Name, PixelCanvas Canvas)> ExtraLabArt() => LabClutterArt.All();

        /// <summary>
        /// A fisica de quem esta parado, com o pivo de cada peca no meio da
        /// base (o do teto, no meio do alto). O que fica no chao tem apoio
        /// embaixo de toda a base — chao, ou o tampo de outra peca (o frasco
        /// em cima da bancada) — e nao entra na rocha; o que pende sai do teto.
        /// </summary>
        private static void CheckPropPhysics(List<string> problems)
        {
            const float ppu = 16f;
            var boxes = new List<(string Sprite, float X0, float X1, float Y0, float Y1)>();

            foreach (Prop prop in Props)
            {
                PixelCanvas c = LabCanvas(prop.Sprite);
                if (c == null) continue;
                float w = c.Width / ppu;
                float h = c.Height / ppu;
                float y0 = prop.Hanging ? prop.Y - h : prop.Y;
                boxes.Add((prop.Sprite, prop.X - w / 2f, prop.X + w / 2f, y0, y0 + h));
            }

            for (int i = 0; i < Props.Count; i++)
            {
                Prop prop = Props[i];
                PixelCanvas c = LabCanvas(prop.Sprite);
                if (c == null) continue;

                float left = prop.X - c.Width / ppu / 2f;
                float bottom = prop.Hanging ? prop.Y - c.Height / ppu : prop.Y;

                if (prop.Hanging)
                {
                    int cx = (int)Math.Floor(prop.X);
                    int cy = (int)Math.Round(prop.Y);
                    if (!Map.IsSolid(cx, cy) || Map.IsRock(cx, cy - 1))
                    {
                        problems.Add($"{prop.Sprite} em ({prop.X}, {prop.Y}) nao esta preso no teto.");
                    }

                    continue;
                }

                // A base: as colunas com pixel nas duas linhas de baixo.
                int unsupported = 0;
                int baseWidth = 0;
                for (int px = 0; px < c.Width; px++)
                {
                    bool foot = c.Get(px, c.Height - 1).A > 0 || c.Get(px, c.Height - 2).A > 0;
                    if (!foot) continue;
                    baseWidth++;

                    float wx = left + (px + 0.5f) / ppu;
                    bool onGround = Math.Abs(bottom - Math.Round(bottom)) < 0.02f
                        && Map.IsSolid((int)Math.Floor(wx), (int)Math.Round(bottom) - 1);

                    bool onProp = false;
                    for (int j = 0; j < boxes.Count && !onGround && !onProp; j++)
                    {
                        if (j == i) continue;
                        (string _, float bx0, float bx1, float by0, float by1) = boxes[j];
                        onProp = wx >= bx0 && wx <= bx1 && Math.Abs(by1 - bottom) < 0.35f && by0 < bottom;
                    }

                    if (!onGround && !onProp) unsupported++;
                }

                if (unsupported > Math.Max(1, baseWidth / 8))
                {
                    problems.Add($"{prop.Sprite} em ({prop.X}, {prop.Y}) tem {unsupported} de {baseWidth}px da base no ar.");
                }

                int buried = 0;
                int total = 0;
                for (int py = 0; py < c.Height - 2; py++)
                {
                    for (int px = 0; px < c.Width; px++)
                    {
                        if (c.Get(px, py).A == 0) continue;
                        total++;
                        float wx = left + (px + 0.5f) / ppu;
                        float wy = bottom + (c.Height - py - 0.5f) / ppu;
                        if (Map.IsRock((int)Math.Floor(wx), (int)Math.Floor(wy))) buried++;
                    }
                }

                if (buried > Math.Max(2, total / 20)) problems.Add($"{prop.Sprite} em ({prop.X}, {prop.Y}) entra na rocha ({buried}px).");
            }
        }

        /// <summary>
        /// O poco do elevador: aberto do fundo ate logo abaixo do andar, com
        /// rocha em volta (as paredes que a camera mostra subindo) e o chao da
        /// cabine tampando a boca.
        /// </summary>
        private static void CheckElevator(List<string> problems)
        {
            (int x0, int bottom, int stop, int _) = ElevatorShaft;

            if (SpawnTile.X < x0 || SpawnTile.X > x0 + 2 || SpawnTile.Y != stop)
            {
                problems.Add($"O Scrap chega em ({SpawnTile.X}, {SpawnTile.Y}), fora da cabine do elevador.");
            }

            for (int y = bottom; y <= stop - 2; y++)
            {
                for (int x = x0; x <= x0 + 2; x++)
                {
                    if (Map.IsSolid(x, y)) problems.Add($"O poco do elevador esta entupido em ({x}, {y}).");
                }

                if (!Map.IsRock(x0 - 1, y) || !Map.IsRock(x0 + 3, y)) problems.Add($"O poco do elevador vaza pro lado na linha {y}.");
            }

            for (int x = x0; x <= x0 + 2; x++)
            {
                if (!Map.IsRock(x, bottom - 1)) problems.Add($"O poco do elevador nao tem fundo em ({x}, {bottom - 1}).");
                if (!Map.IsRock(x, stop - 1) || !ObjectCells.Contains((x, stop - 1))) problems.Add($"O chao da cabine falta em ({x}, {stop - 1}).");
            }
        }

        private static Beam BeamNamed(string name) => Beams.Find(beam => beam.Name == name);

        /// <summary>
        /// Segue a luz de um emissor, celula a celula, como o LightBeam faz no
        /// jogo: num espelho dobra a esquina (com os de flipped virados ao
        /// contrario do inicio), numa fechadura acende e para, em terreno ou
        /// portao fechado para. Devolve o nome da fechadura acesa, ou null.
        /// Se cells nao for null, recebe as celulas por onde a luz passou.
        /// </summary>
        public static string TraceLight(Beam beam, ICollection<string> flipped, ICollection<string> open, List<(int X, int Y)> cells)
        {
            int x = beam.X;
            int y = beam.Y;
            int dx = beam.DirX;
            int dy = beam.DirY;

            bool ClosedGate(int cx, int cy)
            {
                foreach (GateSpec gate in Gates)
                {
                    if (open != null && open.Contains(gate.Name)) continue;
                    if (cx >= gate.X && cx < gate.X + gate.Width && cy >= gate.Y && cy < gate.Y + gate.Height) return true;
                }

                return false;
            }

            for (int step = 0; step < 400; step++)
            {
                x += dx;
                y += dy;

                if (Map.IsSolid(x, y) || ClosedGate(x, y)) return null;

                cells?.Add((x, y));

                LockSpec hit = Locks.Find(spec => spec.X == x && spec.Y == y);
                if (hit != null) return hit.Name;

                MirrorSpec mirror = Mirrors.Find(m => m.X == x && m.Y == y);
                if (mirror == null) continue;

                bool slash = mirror.Slash != (flipped != null && flipped.Contains(mirror.Name));

                // "/" troca x com y; "\" troca e inverte (o mesmo do SilverMirror).
                int ndx = slash ? dy : -dy;
                int ndy = slash ? dx : -dx;
                dx = ndx;
                dy = ndy;
            }

            return null;
        }

        /// <summary>O comprimento de um raio: quantas celulas ele anda ate bater em terreno.</summary>
        public static int BeamLength(Beam beam)
        {
            int x = beam.X;
            int y = beam.Y;

            for (int length = 0; length < 120; length++)
            {
                x += beam.DirX;
                y += beam.DirY;
                if (Map.IsSolid(x, y)) return length + 1;
            }

            return 120;
        }

        /// <summary>O emissor fica no ar, e o raio bate em parede logo adiante.</summary>
        private static void CheckBeam(List<string> problems, Beam beam)
        {
            if (Map.IsSolid(beam.X, beam.Y)) problems.Add($"O emissor de {beam.Name} ({beam.X}, {beam.Y}) esta dentro do terreno.");
            if (BeamLength(beam) >= 60) problems.Add($"O raio de {beam.Name} nao bate em nada perto.");
        }

        /// <summary>Os trechos de chao onde nao da pra chegar com tudo aberto.</summary>
        public static List<string> UnreachableFloors()
        {
            var lit = new List<string>();
            foreach (Circle circle in Circles) lit.Add(circle.Name);

            var open = new List<string>();
            foreach (GateSpec gate in Gates) open.Add(gate.Name);

            var lifts = new List<LevelMap.Lift>();
            LevelMap world = World(lit, open, lifts);
            LevelMap.Reach reach = world.Explore(SpawnTile, Rules(), lifts);

            var floors = new List<string>();

            foreach (LevelMap.Perch perch in world.FindPerches())
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
