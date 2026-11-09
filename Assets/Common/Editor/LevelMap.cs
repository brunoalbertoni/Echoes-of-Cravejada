using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A grade de celulas de uma fase e o que da pra perguntar a ela: o que e
    /// solido, que tile vai em cada celula e ate onde o Scrap consegue chegar.
    ///
    /// E compartilhada pelos layouts de todas as fases. Nao depende de
    /// UnityEngine de proposito: assim da pra rodar a validacao fora do editor.
    /// </summary>
    public sealed class LevelMap
    {
        public const char Rock = '#';
        public const char Air = '.';

        /// <summary>Plataforma fina, de 1 tile de espessura. Solida.</summary>
        public const char Platform = '=';

        /// <summary>Piso de grade: solido como a plataforma, mas vazado — da pra ver o que ha embaixo.</summary>
        public const char Grate = 'g';

        /// <summary>Acido. Nao e solido: o Scrap afunda nele e e jogado de volta.</summary>
        public const char Acid = '~';

        /// <summary>Espinhos. Como o acido: nao seguram, machucam e jogam de volta.</summary>
        public const char Spike = '^';

        public readonly int Width;
        public readonly int Height;

        private readonly char[] _cells;

        /// <summary>Uma grade toda de rocha, pra ser escavada.</summary>
        public LevelMap(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new char[width * height];

            for (int i = 0; i < _cells.Length; i++) _cells[i] = Rock;
        }

        /// <summary>Uma copia independente, pra testar "e se" sem estragar o mapa.</summary>
        public LevelMap Clone()
        {
            var copy = new LevelMap(Width, Height);
            Array.Copy(_cells, copy._cells, _cells.Length);
            return copy;
        }

        /// <summary>
        /// Le um mapa desenhado em texto, de cima pra baixo. As linhas precisam
        /// ter todas a mesma largura; confira com <see cref="RowProblems"/> antes.
        /// </summary>
        public static LevelMap FromRows(string[] rows)
        {
            int width = rows.Length == 0 ? 0 : rows[0].Length;
            var map = new LevelMap(width, rows.Length);

            for (int row = 0; row < rows.Length; row++)
            {
                int y = rows.Length - 1 - row;

                for (int x = 0; x < width && x < rows[row].Length; x++)
                {
                    map._cells[x + y * width] = rows[row][x];
                }
            }

            return map;
        }

        /// <summary>Linhas com largura diferente da primeira.</summary>
        public static List<string> RowProblems(string[] rows, int width, int height)
        {
            var problems = new List<string>();

            if (rows.Length != height)
            {
                problems.Add($"O mapa tem {rows.Length} linhas, mas Height diz {height}.");
            }

            for (int row = 0; row < rows.Length; row++)
            {
                if (rows[row].Length != width)
                {
                    problems.Add($"A linha {row} do mapa tem {rows[row].Length} colunas, mas Width diz {width}.");
                }
            }

            return problems;
        }

        // ------------------------------------------------------------------
        // Celulas
        // ------------------------------------------------------------------

        public char this[int x, int y]
        {
            get
            {
                // Fora do mapa conta como rocha: e o que faz as bordas da fase
                // fecharem com miolo, e nao com uma aresta virada pro nada.
                if (x < 0 || x >= Width || y < 0 || y >= Height) return Rock;
                return _cells[x + y * Width];
            }
        }

        public void Set(int x, int y, char cell)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            _cells[x + y * Width] = cell;
        }

        /// <summary>Preenche um retangulo, com os dois cantos incluidos.</summary>
        public void Fill(int x0, int y0, int x1, int y1, char cell)
        {
            for (int y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++)
            {
                for (int x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++)
                {
                    Set(x, y, cell);
                }
            }
        }

        /// <summary>Rocha macica (ou chapa, ou cristal: o material quem escolhe e a fase).</summary>
        public bool IsRock(int x, int y) => this[x, y] == Rock;

        /// <summary>Plataforma fina ou grade.</summary>
        public bool IsThin(int x, int y)
        {
            char cell = this[x, y];
            return cell == Platform || cell == Grate;
        }

        public bool IsAcid(int x, int y) => this[x, y] == Acid;

        public bool IsSpike(int x, int y) => this[x, y] == Spike;

        /// <summary>Acido ou espinho: encostar machuca.</summary>
        public bool IsHazard(int x, int y)
        {
            char cell = this[x, y];
            return cell == Acid || cell == Spike;
        }

        /// <summary>Qualquer coisa em que da pra pisar ou bater a cabeca.</summary>
        public bool IsSolid(int x, int y)
        {
            char cell = this[x, y];
            return cell == Rock || cell == Platform || cell == Grate;
        }

        /// <summary>
        /// Se o Scrap consegue ficar em pe com os pes nesta celula: chao embaixo
        /// e duas celulas livres pra ele caber (ele tem ~1.2 tile de altura).
        /// </summary>
        public bool IsStandable(int x, int y)
        {
            return this[x, y] == Air && !IsSolid(x, y + 1) && IsSolid(x, y - 1);
        }

        // ------------------------------------------------------------------
        // Escolha de tiles
        // ------------------------------------------------------------------

        public enum TileKind
        {
            /// <summary>Nada pra pintar: ar ou acido.</summary>
            None,

            /// <summary>Miolo: cercado de rocha por todos os lados.</summary>
            Core,

            /// <summary>
            /// Borda, quina ou face, no arranjo de uma banda de cor do
            /// cave_tileset (8 colunas por 4 linhas):
            ///   linha 0, colunas 0-3: borda de cima      linha 1, colunas 0-3: borda de baixo
            ///   colunas 4-5, linhas 0-1: quinas pra fora  colunas 4-5, linhas 2-3: quinas pra dentro
            ///   coluna 6: face virada pra direita         coluna 7: face virada pra esquerda
            /// </summary>
            Edge,

            /// <summary>Plataforma fina ou grade. Coluna 0 = ponta esquerda, 1-2 = miolo, 3 = ponta direita.</summary>
            Thin,
        }

        public readonly struct TilePick
        {
            public readonly TileKind Kind;
            public readonly int Column;
            public readonly int Row;

            public TilePick(TileKind kind, int column, int row)
            {
                Kind = kind;
                Column = column;
                Row = row;
            }
        }

        /// <summary>
        /// Que tile de terreno vai nesta celula, a partir da vizinhanca dela.
        /// So diz a forma; o nome do tile (e portanto o material) fica com a fase.
        /// </summary>
        public TilePick PickGround(int x, int y)
        {
            char cell = this[x, y];

            if (cell == Platform || cell == Grate)
            {
                if (this[x - 1, y] != cell) return new TilePick(TileKind.Thin, 0, 0);
                if (this[x + 1, y] != cell) return new TilePick(TileKind.Thin, 3, 0);
                return new TilePick(TileKind.Thin, 1 + Pick(x, y, 2), 0);
            }

            if (cell != Rock) return new TilePick(TileKind.None, 0, 0);

            bool openUp = !IsRock(x, y + 1);
            bool openDown = !IsRock(x, y - 1);
            bool openLeft = !IsRock(x - 1, y);
            bool openRight = !IsRock(x + 1, y);

            if (openUp && openRight) return new TilePick(TileKind.Edge, 4, 0);
            if (openUp && openLeft) return new TilePick(TileKind.Edge, 5, 0);
            if (openDown && openRight) return new TilePick(TileKind.Edge, 4, 1);
            if (openDown && openLeft) return new TilePick(TileKind.Edge, 5, 1);

            if (openUp) return new TilePick(TileKind.Edge, Pick(x, y, 4), 0);
            if (openDown) return new TilePick(TileKind.Edge, Pick(x, y, 4), 1);
            if (openRight) return new TilePick(TileKind.Edge, 6, Pick(x, y, 4));
            if (openLeft) return new TilePick(TileKind.Edge, 7, Pick(x, y, 4));

            // Cercada pelos quatro lados: so sobra a quina de dentro, quando a
            // rocha faz um L e o ar encosta na diagonal.
            if (!IsRock(x - 1, y + 1)) return new TilePick(TileKind.Edge, 4, 2);
            if (!IsRock(x + 1, y + 1)) return new TilePick(TileKind.Edge, 5, 2);
            if (!IsRock(x - 1, y - 1)) return new TilePick(TileKind.Edge, 4, 3);
            if (!IsRock(x + 1, y - 1)) return new TilePick(TileKind.Edge, 5, 3);

            return new TilePick(TileKind.Core, 0, 0);
        }

        /// <summary>
        /// Variacao estavel: a mesma celula sempre sorteia o mesmo numero, entao
        /// regerar a fase nao embaralha a arte.
        /// </summary>
        public static int Pick(int x, int y, int count)
        {
            unchecked
            {
                int hash = (x * 73856093) ^ (y * 19349663);
                return Math.Abs(hash % count);
            }
        }

        // ------------------------------------------------------------------
        // Vapor
        // ------------------------------------------------------------------

        /// <summary>Uma coluna de vapor que levanta o Scrap. Os cantos estao incluidos.</summary>
        public readonly struct Lift
        {
            public readonly int X0;
            public readonly int X1;
            public readonly int Bottom;
            public readonly int Top;

            public Lift(int x0, int x1, int bottom, int top)
            {
                X0 = x0;
                X1 = x1;
                Bottom = bottom;
                Top = top;
            }

            /// <summary>Se o Scrap, com os pes em (x, y), encosta na coluna.</summary>
            public bool Touches(int x, int y) => x >= X0 && x <= X1 && y <= Top && y + 1 >= Bottom;
        }

        /// <summary>Um jato de vapor deitado, que carrega o Scrap numa direcao so.</summary>
        public readonly struct Jet
        {
            public readonly int X0;
            public readonly int X1;
            public readonly int Y0;
            public readonly int Y1;

            /// <summary>+1 empurra pra leste, -1 pra oeste.</summary>
            public readonly int Direction;

            public Jet(int x0, int x1, int y0, int y1, int direction)
            {
                X0 = x0;
                X1 = x1;
                Y0 = y0;
                Y1 = y1;
                Direction = direction;
            }

            public bool Touches(int x, int y) => x >= X0 && x <= X1 && y <= Y1 && y + 1 >= Y0;
        }

        // ------------------------------------------------------------------
        // Alcance
        // ------------------------------------------------------------------

        /// <summary>
        /// Quanto o Scrap pula, em tiles. Os numeros saem do ScrapController:
        ///
        /// - jumpHeight 3.0 poe os pes a ~2.85 tiles do chao (a integracao da
        ///   fisica come um pedaco), entao um degrau de 3 nao e alcancavel de
        ///   verdade. O limite e 2.
        /// - um pulo no plano, correndo, leva ~4 tiles: da pra cruzar um vao
        ///   de 4. Pousando 2 tiles mais alto sobra menos tempo de voo, e o
        ///   vao que da pra cruzar cai pra 3.
        ///
        /// Os valores padrao sao esse limite. <see cref="Comfortable"/> tira um
        /// tile de cada vao, pra quem quer um mapa com folga.
        /// </summary>
        public sealed class JumpRules
        {
            /// <summary>Quantos tiles ele sobe num pulo.</summary>
            public int MaxRise = 2;

            /// <summary>
            /// Quantas celulas ele anda de lado num pulo, pousando na mesma
            /// altura. Cruzar um vao de N celulas custa N + 1, entao 5 e um
            /// vao de 4.
            /// </summary>
            public int MaxRun = 5;

            /// <summary>
            /// A partir de qual tile de subida cada tile a mais custa uma
            /// celula de alcance lateral. Com 2, pousar 2 tiles acima deixa
            /// cruzar um vao a menos.
            /// </summary>
            public int RiseCostsRunFrom = 2;

            /// <summary>
            /// Se ele sabe pular de parede (Wall Grip). O pulo de parede joga
            /// o Scrap pra longe dela, entao uma parede sozinha nao serve de
            /// escada: so da pra subir entre duas, quicando de uma na outra.
            /// </summary>
            public bool WallJump;

            /// <summary>
            /// A maior distancia entre as duas paredes, em celulas de ar, pra
            /// ele conseguir subir entre elas. Mais largo que isso ele cai
            /// antes de alcancar a outra.
            /// </summary>
            public int WallJumpMaxGap = 4;

            /// <summary>As mesmas regras com um tile a menos em cada vao.</summary>
            public static JumpRules Comfortable() => new JumpRules { MaxRun = 4 };

            /// <summary>Quanto ele anda de lado ao so cair de uma beirada, sem pular.</summary>
            public int WalkOffRun = 1;

            /// <summary>
            /// Se ele quica nos espinhos com o cinzel pra baixo (o pogo). Conta
            /// so com o espinho logo embaixo dos pes, e por baixo do que o
            /// jogo da: o pogo sobe uns 2.2 tiles e anda uns 3.5 de lado.
            /// </summary>
            public bool Pogo;

            public int PogoRise = 1;
            public int PogoRun = 3;

            /// <summary>A cada tantos tiles de queda ele ganha mais um de alcance lateral.</summary>
            public int FallPerExtraRun = 3;

            /// <summary>Alcance lateral ao sair de uma coluna ou de um jato de vapor.</summary>
            public int SteamExitRun = 3;

            /// <summary>Quanto o embalo ainda sobe depois que ele sai por cima de uma coluna de vapor.</summary>
            public int LiftExitRise = 1;
        }

        /// <summary>O resultado de <see cref="Explore"/>.</summary>
        public sealed class Reach
        {
            private readonly int _width;
            private readonly int _height;
            private readonly bool[] _standing;
            private readonly bool[] _touched;

            internal Reach(int width, int height)
            {
                _width = width;
                _height = height;
                _standing = new bool[width * height];
                _touched = new bool[width * height];
            }

            /// <summary>Se da pra chegar e ficar em pe com os pes nesta celula.</summary>
            public bool CanStand(int x, int y) => Inside(x, y) && _standing[x + y * _width];

            /// <summary>Se o Scrap passa por esta celula em algum momento, em pe ou no ar.</summary>
            public bool CanTouch(int x, int y) => Inside(x, y) && _touched[x + y * _width];

            internal void MarkStanding(int x, int y)
            {
                if (Inside(x, y)) _standing[x + y * _width] = true;
            }

            internal void MarkTouched(int x, int y)
            {
                if (Inside(x, y)) _touched[x + y * _width] = true;
                if (Inside(x, y + 1)) _touched[x + (y + 1) * _width] = true;
            }

            private bool Inside(int x, int y) => x >= 0 && x < _width && y >= 0 && y < _height;
        }

        /// <summary>
        /// Descobre onde o Scrap consegue chegar a partir de uma celula,
        /// andando, pulando, caindo e pegando carona no vapor.
        ///
        /// E uma busca em largura num modelo simples do movimento: ele ocupa
        /// duas celulas de altura; no chao anda de lado ou pula; no ar tem um
        /// tanto de subida e um tanto de deslocamento lateral pra gastar, e
        /// depois cai. Paredes, tetos e beiradas contam de verdade — diferente
        /// de so medir a distancia entre duas plataformas.
        /// </summary>
        public Reach Explore((int X, int Y) start, JumpRules rules,
            IReadOnlyList<Lift> lifts = null, IReadOnlyList<Jet> jets = null,
            IReadOnlyList<Lift> clouds = null)
        {
            var reach = new Reach(Width, Height);

            int rises = rules.MaxRise + 1;
            int runs = Math.Max(rules.MaxRun, rules.SteamExitRun) + 1;
            int falls = Math.Max(1, rules.FallPerExtraRun);
            int airStates = rises * runs * falls;

            // Estado 0 e "em pe"; os demais sao as combinacoes de (subida que
            // sobra, lateral que sobra, tiles caidos desde o ultimo bonus).
            int statesPerCell = 1 + airStates;
            var seen = new bool[Width * Height * statesPerCell];
            var queue = new Queue<int>();

            int Encode(int x, int y, int state) => (x + y * Width) * statesPerCell + state;
            int AirState(int rise, int run, int fallen) => 1 + (rise * runs + run) * falls + fallen;

            bool Fits(int x, int y)
            {
                if (x < 0 || x >= Width || y < 0 || y + 1 >= Height) return false;
                return !IsSolid(x, y) && !IsSolid(x, y + 1);
            }

            // Cair no acido ou nos espinhos nao leva a lugar nenhum: ele e jogado de volta.
            bool Burns(int x, int y) => IsHazard(x, y) || IsHazard(x, y + 1);

            void Push(int x, int y, int state)
            {
                if (!Fits(x, y) || Burns(x, y)) return;

                int code = Encode(x, y, state);
                if (seen[code]) return;

                seen[code] = true;
                queue.Enqueue(code);
            }

            // Se nao houver chao embaixo de onde ele nasce, ele comeca caindo.
            if (IsStandable(start.X, start.Y)) Push(start.X, start.Y, 0);
            else Push(start.X, start.Y, AirState(0, 0, 0));

            while (queue.Count > 0)
            {
                int code = queue.Dequeue();
                int state = code % statesPerCell;
                int cell = code / statesPerCell;
                int x = cell % Width;
                int y = cell / Width;

                reach.MarkTouched(x, y);

                if (state == 0)
                {
                    reach.MarkStanding(x, y);

                    for (int step = -1; step <= 1; step += 2)
                    {
                        int nx = x + step;
                        if (!Fits(nx, y)) continue;

                        if (IsSolid(nx, y - 1)) Push(nx, y, 0);
                        else Push(nx, y, AirState(0, rules.WalkOffRun, 0));
                    }

                    Push(x, y, AirState(rules.MaxRise, rules.MaxRun, 0));
                    continue;
                }

                int air = state - 1;
                int fallen = air % falls;
                int run = air / falls % runs;
                int rise = air / falls / runs;

                // Dentro de um jato ele so vai pra onde o jato manda, e nao cai.
                bool carried = false;
                if (jets != null)
                {
                    foreach (Jet jet in jets)
                    {
                        if (!jet.Touches(x, y)) continue;

                        int nx = x + jet.Direction;
                        if (!Fits(nx, y)) continue;

                        Push(nx, y, AirState(0, rules.SteamExitRun, 0));
                        carried = true;
                    }
                }

                if (carried) continue;

                // Numa coluna de vapor ele sobe de graca, e sai dela com embalo.
                // E nao desce: o vapor empurra mais forte do que a gravidade,
                // entao pra descer por ali e preciso sair de lado primeiro.
                bool lifted = false;
                if (lifts != null)
                {
                    foreach (Lift lift in lifts)
                    {
                        if (!lift.Touches(x, y)) continue;

                        int boostedRise = Math.Max(rise, rules.LiftExitRise);
                        int boostedRun = Math.Max(run, rules.SteamExitRun);

                        Push(x, y + 1, AirState(rules.LiftExitRise, rules.SteamExitRun, 0));
                        Push(x, y, AirState(boostedRise, boostedRun, 0));
                        lifted = true;
                    }
                }

                // Numa nuvem de vapor parada o dash esfria: o alcance lateral
                // volta inteiro, e da pra emendar outro.
                if (clouds != null)
                {
                    foreach (Lift cloud in clouds)
                    {
                        if (cloud.Touches(x, y)) Push(x, y, AirState(rise, runs - 1, 0));
                    }
                }

                if (rules.WallJump && CanWallJump(x, y, rules.WallJumpMaxGap))
                {
                    Push(x, y, AirState(rules.MaxRise, rules.MaxRun, 0));
                }

                // O pogo: no ar, com espinho logo embaixo, ele quica pra cima.
                if (rules.Pogo && IsSpike(x, y - 1))
                {
                    int pogoRise = Math.Min(rules.PogoRise, rules.MaxRise);
                    int pogoRun = Math.Min(Math.Max(run, rules.PogoRun), rules.MaxRun);
                    Push(x, y, AirState(pogoRise, pogoRun, 0));
                }

                if (rise > 0)
                {
                    // Qual tile de subida e este, contando do chao.
                    int riseStep = rules.MaxRise - rise + 1;
                    int cost = riseStep >= rules.RiseCostsRunFrom ? 1 : 0;

                    Push(x, y + 1, AirState(rise - 1, Math.Max(0, run - cost), 0));

                    // Soltar o botao (ou bater a cabeca) corta a subida.
                    Push(x, y, AirState(0, run, fallen));
                }

                if (run > 0)
                {
                    Push(x - 1, y, AirState(rise, run - 1, fallen));
                    Push(x + 1, y, AirState(rise, run - 1, fallen));
                }

                if (rise == 0)
                {
                    if (IsSolid(x, y - 1))
                    {
                        Push(x, y, 0);
                    }
                    else if (!lifted)
                    {
                        int nextFallen = fallen + 1;
                        int nextRun = run;

                        if (nextFallen >= falls)
                        {
                            nextFallen = 0;
                            nextRun = Math.Min(runs - 1, run + 1);
                        }

                        Push(x, y - 1, AirState(0, nextRun, nextFallen));
                    }
                }
            }

            return reach;
        }

        /// <summary>
        /// Se, com os pes em (x, y), ele esta encostado numa parede de rocha e
        /// ha outra do lado oposto perto o bastante pra quicar ate ela.
        /// </summary>
        private bool CanWallJump(int x, int y, int maxGap)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                if (!IsRock(x + side, y) && !IsRock(x + side, y + 1)) continue;

                // Procura a parede oposta, sem nada no caminho.
                for (int distance = 1; distance <= maxGap; distance++)
                {
                    int far = x - side * distance;

                    if (IsRock(far, y) || IsRock(far, y + 1)) return true;
                    if (IsSolid(far, y) || IsSolid(far, y + 1)) break;
                }
            }

            return false;
        }

        /// <summary>Um trecho continuo de chao onde da pra andar sem pular.</summary>
        public readonly struct Perch
        {
            public readonly int X;
            public readonly int Right;

            /// <summary>Linha onde ficam os pes.</summary>
            public readonly int Y;

            public Perch(int x, int right, int y)
            {
                X = x;
                Right = right;
                Y = y;
            }

            public int Length => Right - X + 1;

            public bool Contains(int x, int y) => y == Y && x >= X && x <= Right;

            public override string ToString() => $"x={X}..{Right}, y={Y}";
        }

        /// <summary>Todos os trechos de chao do mapa.</summary>
        public List<Perch> FindPerches()
        {
            var perches = new List<Perch>();

            for (int y = 1; y < Height - 1; y++)
            {
                int start = -1;

                for (int x = 0; x <= Width; x++)
                {
                    bool standable = x < Width && IsStandable(x, y);

                    if (standable && start < 0) start = x;

                    if (!standable && start >= 0)
                    {
                        perches.Add(new Perch(start, x - 1, y));
                        start = -1;
                    }
                }
            }

            return perches;
        }

        /// <summary>O mapa em texto, de cima pra baixo. Serve pra conferir o desenho no olho.</summary>
        public string[] ToRows()
        {
            var rows = new string[Height];

            for (int row = 0; row < Height; row++)
            {
                rows[row] = new string(_cells, (Height - 1 - row) * Width, Width);
            }

            return rows;
        }
    }
}
