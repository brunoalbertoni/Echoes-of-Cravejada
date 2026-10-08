using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O desenho da Phase 1 — District 1: The Scrap Pit.
    ///
    /// E o tutorial do jogo: cada trecho do mapa existe pra ensinar uma coisa.
    ///
    /// O POCO (embaixo, a oeste). O Scrap liga em cima de um monte de sucata e
    /// sobe em ziguezague. Ensina a andar e pular.
    ///
    /// O POCO VERTICAL. No patamar do topo o teto abre numa chamine estreita.
    /// A unica saida e subir pelas paredes: Wall Grip.
    ///
    /// O ANDAR 1 (por cima do poco, indo pra oeste). Os primeiros corrompidos e
    /// os espinhos de sucata — atacar com o cinzel, quicar por cima. No fim, a
    /// alavanca enferrujada: raspar ate o portao abrir. Atras dele, a bancada.
    ///
    /// O ANDAR 2 (mais acima, voltando pra leste). O desafio misto: tudo junto.
    /// Termina num poco que desce direto na arena.
    ///
    /// A ARENA (embaixo, a leste). A sala do Rusty Scrapper: um fosso rebaixado
    /// entre dois patamares. A saida fica gradeada ate ele cair.
    ///
    /// O ABISMO (depois da arena). Um fosso de espinhos largo demais pro pulo:
    /// so se cruza com o Steam Dash, que o boss deixa. Do outro lado, o vapor
    /// que aparece com a purificacao sobe ate o District 2.
    ///
    /// Este arquivo nao depende de UnityEngine de proposito: assim da pra rodar
    /// Validate() fora do editor e conferir que o mapa e navegavel de verdade,
    /// sem precisar abrir o jogo.
    /// </summary>
    public static class Phase01Layout
    {
        // ------------------------------------------------------------------
        // O mapa
        // ------------------------------------------------------------------

        /// <summary>
        /// O mapa, desenhado de cima pra baixo.
        ///
        /// # = rocha    = = plataforma fina (1 tile)    ^ = espinhos    . = ar
        ///
        /// Mexer aqui e mexer no mapa: as bordas, os cantos e o miolo da rocha
        /// sao escolhidos sozinhos a partir da vizinhanca de cada celula. Depois
        /// rode Validate() (menu Echoes > Validar layout da Phase 1) pra conferir
        /// que nada ficou fora de alcance.
        /// </summary>
        private static readonly string[] Rows =
        {
            "############################################################################################################",
            "############################################################################################################",
            "############################################################################################################",
            "############################################################################################################",
            "####....................................#####.......########################################################",
            "####....................................#####.......########################################################",
            "####................................................########################################################",
            "####................................................########################################################",
            "####.............................###................########################################################",
            "####............==..==...........###................########################################################",
            "####....######^^^^^^^^^##########################...########################################################",
            "####....#########################################...########################################################",
            "####....#########################################...##############################################....######",
            "####....#########################################...##############################################....######",
            "####....##............................###########...##############################################....######",
            "####....##............................###########...##############################################....######",
            "####....##............................###########...##############################################....######",
            "####..................................###########...##############################################....######",
            "####..................................###########...##############################################....######",
            "####..................................###########...##############################################....######",
            "#################^^^######^^^#####....###########...##############################################....######",
            "##################################....###########...##############################################....######",
            "##################################....###########...##############################################....######",
            "##################################....###########...##############################################....######",
            "##################################....###########...##############################################....######",
            "##################################....###########...##############################################....######",
            "##################################....###########.......###.##.####..####.########################....######",
            "##################################....###########.......###..........####.########################....######",
            "##################################....###########.........................########################....######",
            "##################################....###########............................########..................#####",
            "##################.######...######....###########............................########..................#####",
            "##################..........######....###########............................########..................#####",
            "###############..................#....###########............................########..................#####",
            "###############..................#....###########............................########..................#####",
            "###########......................#....##########.............====.............#######..................#####",
            "###########......................#....##########.......................................................#####",
            "######..................................########.......=====......=====................................#####",
            "######..................................########.......................................................#####",
            "#####............................###################......................#############.......##############",
            "#####............................###################......................#############.......##############",
            "#####.....................=====.........###############################################.......##############",
            "####....................................###############################################.......##############",
            "####...............=====................###############################################.......##############",
            "####...................................################################################^^^^^^^##############",
            "####........=====......................#####################################################################",
            "####...................................#####################################################################",
            "##########............................######################################################################",
            "##########............................######################################################################",
            "####........=====.....................######################################################################",
            "####..................................######################################################################",
            "####................=====..............#####################################################################",
            "#####..................................#####################################################################",
            "#####.......................=====.......####################################################################",
            "#####...................................####################################################################",
            "#####.............########.........#########################################################################",
            "#########.......############.......#########################################################################",
            "############################################################################################################",
            "############################################################################################################",
            "############################################################################################################",
            "############################################################################################################",
        };

        public const int Width = 108;
        public const int Height = 60;

        /// <summary>Onde o Scrap nasce: em cima do monte de sucata, um tiquinho no ar.</summary>
        public static (int X, int Y) SpawnTile => (21, 7);

        /// <summary>
        /// Uma celula do patamar do topo do poco, de onde sai o corredor. Se nao
        /// der pra chegar aqui, nao da pra sair do poco.
        /// </summary>
        public static (int X, int Y) LandingTile => (35, 22);

        /// <summary>
        /// O poco vertical: a chamine estreita no teto do poco, por onde se sobe
        /// pulando de parede em parede ate o andar 1.
        /// </summary>
        public static (int X, int Y, int Width, int Height) Chimney => (34, 28, 4, 12);

        /// <summary>
        /// Onde o Scrap aparece quando volta do District 2: no patamar do topo.
        /// A ida pro District 2 esta desligada por enquanto — ela volta junto
        /// com o mini boss, que e quem a destranca.
        /// </summary>
        public static (int X, int Y) ReturnTile => (38, 22);

        // ------------------------------------------------------------------
        // O que as mecanicas precisam saber
        // ------------------------------------------------------------------

        /// <summary>
        /// O portao do andar 1: a coluna de celulas que ele fecha. No mapa ela
        /// e ar; quem a fecha e o objeto do portao, ate a alavanca ser raspada.
        /// </summary>
        public static (int X, int Y, int Height) Gate => (10, 40, 6);

        /// <summary>A alavanca enferrujada, do lado de ca do portao.</summary>
        public static (int X, int Y) LeverTile => (13, 40);

        /// <summary>A Goldsmith Workbench, do lado de la. Canto de baixo, a esquerda.</summary>
        public static (int X, int Y) WorkbenchTile => (5, 40);

        /// <summary>Um autômato corrompido que anda de um lado pro outro num trecho de chao.</summary>
        public readonly struct Patrol
        {
            public readonly string Name;
            public readonly int X0;
            public readonly int X1;

            /// <summary>Linha onde ficam os pes.</summary>
            public readonly int Y;

            public Patrol(string name, int x0, int x1, int y)
            {
                Name = name;
                X0 = x0;
                X1 = x1;
                Y = y;
            }
        }

        public static readonly Patrol[] Crawlers =
        {
            // Andar 1: o primeiro que se ve, e o que guarda a alavanca.
            new Patrol("Crawler_First", 20, 25, 40),
            new Patrol("Crawler_LeverGuard", 11, 16, 40),

            // Andar 2: o desafio misto.
            new Patrol("Crawler_Mixed_1", 24, 32, 50),
            new Patrol("Crawler_Mixed_2", 36, 47, 50),
        };

        /// <summary>Um personagem com quem da pra conversar.</summary>
        public readonly struct NpcSpot
        {
            public readonly string Name;
            public readonly int X;
            public readonly int Y;

            /// <summary>Qual esqueleto: 0 de lenco, 1 de capuz.</summary>
            public readonly int Variant;

            public readonly string[] Lines;

            public NpcSpot(string name, int x, int y, int variant, string[] lines)
            {
                Name = name;
                X = x;
                Y = y;
                Variant = variant;
                Lines = lines;
            }
        }

        /// <summary>
        /// Os dois primeiros personagens do jogo: esqueletos de autômatos
        /// descartados antes do Scrap. As falas ficam aqui, uma por tela.
        /// </summary>
        public static readonly NpcSpot[] Npcs =
        {
            // No fundo do poco, ao lado de onde o Scrap acorda: a historia inicial.
            new NpcSpot("Refugo", 13, 4, 0, new[]
            {
                "Olha so... mais um caiu do alto. E esse ainda acende os olhos.",
                "Bem-vindo ao Poco de Sucata, pequeno. E pra ca que Cravejada joga tudo o que nao brilha mais.",
                "Esta vendo a nevoa verde? E o Oxide. Ele come o metal, apaga os cristais e deixa a gente... assim.",
                "A cidade la em cima era uma joia. Agora esta sem polimento, e ninguem que desceu aqui voltou pra contar.",
                "Mas voce e diferente. O Oxide passa por voce e nao gruda. Nunca vi isso.",
                "Suba. O unico caminho e pra cima. E cuidado com o Coletor: e ele quem empurra os descartados pra ca.",
            }),

            // Na sala da bancada: o que ela faz e o que acontece ao morrer.
            new NpcSpot("Rebite", 8, 40, 1, new[]
            {
                "Chegou ate aqui raspando a alavanca? Entao voce sabe polir. Isso e raro, aqui embaixo.",
                "Essa e uma Goldsmith Workbench. Encoste nela e ela guarda o seu lugar e conserta o seu corpo.",
                "Se voce quebrar la fora, e pra ca que volta. Mas os cacos que carregava ficam onde voce caiu.",
                "Volte la e pegue antes de quebrar de novo. Se quebrar duas vezes, eles se perdem de vez.",
                "E nao se engane: enquanto voce se remonta, os corrompidos tambem se levantam. Todos eles.",
                "O caminho continua por cima. Pule de uma parede na outra, como fez no poco.",
            }),
        };

        /// <summary>Onde ha espinhos de sucata.</summary>
        public static bool IsSpike(int x, int y) => Map.IsSpike(x, y);

        /// <summary>O tile dos espinhos (objects_misc, linha 2: o cinza, o escuro e o ensanguentado).</summary>
        public static string SpikeTileName(int x, int y) => $"misc_c{3 + LevelMap.Pick(x, y, 3)}_r2";

        // Nomes usados pra ligar a saida de uma cena a chegada na outra. Sao
        // os mesmos dos dois lados: ver Phase02Layout.
        public const string SceneName = "Phase01_ScrapYard";
        public const string StartSpawnId = "Start";
        public const string FromDistrict2SpawnId = "FromDistrict2";

        // ------------------------------------------------------------------
        // A arena do mini boss
        // ------------------------------------------------------------------

        /// <summary>O vao da arena, do chao do fosso ate o teto.</summary>
        public static (int X, int Y, int Width, int Height) ArenaBounds => (48, 20, 30, 14);

        /// <summary>
        /// Onde o mini boss aparece: no meio do fosso, na frente da boca de
        /// tunel da parede do fundo.
        /// </summary>
        public static (int X, int Y) BossSpawnTile => (62, 20);

        /// <summary>
        /// O pe do poco que desce do andar 2, na entrada da arena. A sala "se
        /// fecha ao entrar": quando essa mecanica existir, o portao vai aqui.
        /// </summary>
        public static (int X, int Y) EntryGateTile => (50, 22);

        /// <summary>O comeco da saida, do outro lado. O portao que o boss destranca.</summary>
        public static (int X, int Y) ExitGateTile => (78, 22);

        /// <summary>O corredor que sai da arena e leva ao abismo.</summary>
        public static (int X, int Y, int Width, int Height) ExitStub => (78, 22, 7, 3);

        /// <summary>
        /// A grade que fecha a saida da arena ate o Rusty Scrapper cair: a
        /// coluna de celulas que ela ocupa.
        /// </summary>
        public static (int X, int Y, int Height) ExitGate => (78, 22, 3);

        // ------------------------------------------------------------------
        // O abismo e a saida pro District 2
        // ------------------------------------------------------------------

        /// <summary>O fosso de espinhos: as colunas sem chao entre as duas beiradas.</summary>
        public static (int X0, int X1) Abyss => (87, 93);

        /// <summary>Uma celula da beirada de la. So se chega aqui com o Steam Dash.</summary>
        public static (int X, int Y) FarLedgeTile => (96, 22);

        /// <summary>
        /// A coluna de vapor que sobe ate o District 2. So aparece depois da
        /// purificacao, quando o Rusty Scrapper cai.
        /// </summary>
        public static readonly LevelMap.Lift District2Vent = new LevelMap.Lift(98, 101, 23, 45);

        /// <summary>O topo dessa coluna. Encostar aqui carrega o District 2.</summary>
        public static (int X, int Y, int Width, int Height) District2Exit => (98, 44, 4, 3);

        // ------------------------------------------------------------------
        // Consultas do terreno
        // ------------------------------------------------------------------

        private static readonly LevelMap Map = LevelMap.FromRows(Rows);

        /// <summary>Rocha macica.</summary>
        public static bool IsRock(int x, int y) => Map.IsRock(x, y);

        /// <summary>Plataforma fina, de 1 tile de espessura.</summary>
        public static bool IsPlatform(int x, int y) => Map.IsThin(x, y);

        /// <summary>Qualquer coisa em que da pra pisar ou bater a cabeca.</summary>
        public static bool IsSolid(int x, int y) => Map.IsSolid(x, y);

        /// <summary>
        /// Se o Scrap consegue ficar em pe com os pes nesta celula: chao embaixo
        /// e duas celulas livres pra ele caber (ele tem ~1.2 tile de altura).
        /// </summary>
        public static bool IsStandable(int x, int y) => Map.IsStandable(x, y);

        // ------------------------------------------------------------------
        // Escolha de tiles
        // ------------------------------------------------------------------

        /// <summary>
        /// Paleta do tileset: a linha onde comeca a banda de cor.
        /// 0 = cinza, 4 = marrom (ferrugem), 8 = azul.
        /// </summary>
        public const int PaletteRow = 4;

        public const int GreyBand = 0;
        public const int RustBand = 4;
        public const int BlueBand = 8;

        /// <summary>
        /// Linha das plataformas finas da cor de ferrugem (colunas 4-7:
        /// ponta esquerda, dois miolos, ponta direita).
        /// </summary>
        private const int ThinPlatformRow = 13;

        /// <summary>
        /// O miolo escuro da rocha. O tileset nao tem um tile todo escuro — so
        /// bordas que esmaecem pra essa cor — entao o gerador cria um.
        /// </summary>
        public const string CoreTileName = "rock_core";

        /// <summary>
        /// O tile de terreno de uma celula, ou null se ela e ar. A forma (borda,
        /// quina, face, miolo) quem escolhe e o LevelMap, pela vizinhanca; aqui
        /// so entra a cor.
        /// </summary>
        public static string GroundTileName(int x, int y)
        {
            LevelMap.TilePick pick = Map.PickGround(x, y);

            switch (pick.Kind)
            {
                case LevelMap.TileKind.Core:
                    return CoreTileName;

                case LevelMap.TileKind.Edge:
                    return TileName(pick.Column, PaletteRow + pick.Row);

                case LevelMap.TileKind.Thin:
                    return TileName(4 + pick.Column, ThinPlatformRow);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Espelha Phase01Assets.TileName. Duplicado aqui pra este arquivo
        /// continuar compilando sozinho, sem UnityEditor, pro Validate().
        /// </summary>
        public static string TileName(int column, int rowFromTop)
        {
            return $"cave_c{column}_r{rowFromTop}";
        }

        /// <summary>
        /// Parede de caverna atras da sala, ou null onde ha uma janela.
        ///
        /// Na textura bg_cave_greenish_16x16 (grade 3x7) so a linha 6 e solida
        /// de ponta a ponta; as outras tem transparencia e deixariam buraco no
        /// fundo. Por isso o fundo usa so essa linha, variando a coluna.
        /// </summary>
        public static string BackgroundTileName(int x, int y)
        {
            if (IsWindow(x, y)) return null;
            return $"bgcave_c{LevelMap.Pick(x + 7, y + 13, 3)}_r6";
        }

        // ------------------------------------------------------------------
        // Janelas na parede do fundo
        // ------------------------------------------------------------------

        /// <summary>
        /// Um pedaco pronto de parede de caverna com um buraco no meio, por onde
        /// se ve o que esta la atras: as silhuetas dos geodos e a nevoa.
        /// </summary>
        public readonly struct Window
        {
            /// <summary>Nome do sprite, fatiado pelo Phase01Assets.</summary>
            public readonly string Sprite;

            /// <summary>Canto de baixo, a esquerda.</summary>
            public readonly int X;
            public readonly int Y;

            public readonly int Width;
            public readonly int Height;

            /// <summary>
            /// Multiplica a cor do pedaco. Os pedacos grandes sao de cor chapada
            /// e saem um tom mais claros que os tiles da parede em volta; isso
            /// disfarca a emenda.
            /// </summary>
            public readonly float Shade;

            public Window(string sprite, int x, int y, int width, int height, float shade = 1f)
            {
                Sprite = sprite;
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Shade = shade;
            }

            public bool Contains(int x, int y)
            {
                return x >= X && x < X + Width && y >= Y && y < Y + Height;
            }
        }

        public const string WindowTall = "window_tall";
        public const string WindowWide = "window_wide";
        public const string WindowTunnel = "window_tunnel";
        public const string WindowGreat = "window_great";

        public static readonly Window[] Windows =
        {
            // --- Poco ---

            // A grande, no alto do vao: e por ela que se ve os geodos colossais.
            new Window(WindowGreat, 15, 21, 12, 6, 0.93f),

            new Window(WindowTall, 9, 6, 4, 5),
            new Window(WindowWide, 30, 11, 5, 4),

            // Uma boca de tunel escura, em cima da saliencia da parede oeste.
            new Window(WindowTunnel, 5, 14, 5, 4),

            // --- Arena ---

            // A boca de tunel no meio da parede do fundo, rente ao chao do
            // fosso: e de la que o mini boss sai.
            new Window(WindowTunnel, 60, 20, 5, 4),

            // E a janela grande por cima dela, dominando a sala.
            new Window(WindowGreat, 57, 26, 12, 6, 0.93f),

            new Window(WindowWide, 52, 21, 5, 4),
            new Window(WindowTall, 71, 22, 4, 5),
        };

        public static bool IsWindow(int x, int y)
        {
            foreach (Window window in Windows)
            {
                if (window.Contains(x, y)) return true;
            }

            return false;
        }

        // ------------------------------------------------------------------
        // Sucata, minerio e mato
        // ------------------------------------------------------------------

        public enum PropLayer
        {
            /// <summary>Atras da rocha e esmaecido: faz parte do fundo.</summary>
            Back,

            /// <summary>Na frente da rocha, atras do Scrap. E onde mora a sucata.</summary>
            Middle,

            /// <summary>Na frente do Scrap, quase em silhueta.</summary>
            Front,
        }

        /// <summary>Um sprite de 1 tile carimbado numa celula.</summary>
        public readonly struct Prop
        {
            public readonly string Tile;
            public readonly int X;
            public readonly int Y;
            public readonly PropLayer Layer;

            /// <summary>
            /// Se precisa de chao embaixo. Falso pra o que esta empilhado em
            /// cima de outra coisa ou incrustado na rocha.
            /// </summary>
            public readonly bool Grounded;

            public Prop(string tile, int x, int y, PropLayer layer, bool grounded)
            {
                Tile = tile;
                X = x;
                Y = y;
                Layer = layer;
                Grounded = grounded;
            }
        }

        // objects_misc.png: linha 0 sao caixotes (0-3) e restos de caixote (4-5),
        // linha 2 sao pedacos de cerca (0-2).
        private static string Crate(int variant) => $"misc_c{variant}_r0";
        private static string Debris(int variant) => $"misc_c{4 + variant}_r0";
        private static string Fence(int variant) => $"misc_c{variant}_r2";

        // bg_objects.png: linha 0 pedras escuras, 1 pedras cinza, 2 mato seco
        // escuro, 3 mato seco cinza. Em todas, as colunas vao do maior pro menor.
        private static string DarkRock(int variant) => $"bgobj_c{variant}_r0";
        private static string GreyRock(int variant) => $"bgobj_c{variant}_r1";
        private static string DarkWeed(int variant) => $"bgobj_c{variant}_r2";
        private static string GreyWeed(int variant) => $"bgobj_c{variant}_r3";

        public static readonly Prop[] Props = BuildProps();

        private static Prop[] BuildProps()
        {
            var props = new List<Prop>();

            void Ground(string tile, int x, int y, PropLayer layer = PropLayer.Middle)
            {
                props.Add(new Prop(tile, x, y, layer, true));
            }

            void Stacked(string tile, int x, int y)
            {
                props.Add(new Prop(tile, x, y, PropLayer.Middle, false));
            }

            // O losango grande de minerio ocupa 2x2 tiles: colunas 1-2, linhas
            // 2-3 da banda de cor. A linha 2 e a metade de cima.
            void Ore(int band, int x, int y, PropLayer layer)
            {
                props.Add(new Prop(TileName(1, band + 2), x, y + 1, layer, false));
                props.Add(new Prop(TileName(2, band + 2), x + 1, y + 1, layer, false));
                props.Add(new Prop(TileName(1, band + 3), x, y, layer, false));
                props.Add(new Prop(TileName(2, band + 3), x + 1, y, layer, false));
            }

            // --- O monte de sucata onde o Scrap acorda ---
            Ground(GreyRock(4), 16, 5);
            Ground(Debris(1), 17, 5);
            Ground(GreyRock(2), 18, 6);
            Ground(Crate(0), 19, 6);
            Stacked(Crate(3), 19, 7);
            Ground(Crate(2), 20, 6);
            Ground(Debris(0), 23, 6);
            Ground(Crate(1), 24, 6);
            Stacked(Debris(0), 24, 7);
            Ground(GreyRock(1), 25, 6);
            Ground(Crate(3), 26, 5);
            Ground(GreyRock(3), 27, 5);

            // --- Chao, lado oeste ---
            Ground(Fence(0), 6, 5);
            Ground(Fence(1), 7, 5);
            Ground(GreyRock(0), 11, 4);
            Ground(GreyRock(2), 12, 4);
            Ground(Debris(1), 13, 4);
            Ore(GreyBand, 14, 4, PropLayer.Back);
            Ground(GreyWeed(0), 9, 4, PropLayer.Back);
            Ground(DarkWeed(1), 5, 5, PropLayer.Front);
            Ground(DarkRock(1), 10, 4, PropLayer.Front);
            Ground(DarkWeed(2), 15, 4, PropLayer.Front);

            // --- Chao, lado leste ---
            Ground(Crate(0), 29, 4);
            Ground(Debris(0), 30, 4);
            Ore(RustBand, 31, 4, PropLayer.Back);
            Ground(Fence(2), 33, 4);
            Ground(GreyRock(1), 34, 4);
            Ground(DarkRock(2), 28, 4, PropLayer.Front);
            Ground(DarkWeed(0), 32, 4, PropLayer.Front);

            // --- Degrau da parede leste ---
            Ground(GreyWeed(3), 36, 6, PropLayer.Back);
            Ground(Crate(2), 38, 6);
            Ground(GreyRock(4), 39, 6);

            // --- Saliencia da parede oeste, na boca do tunel ---
            Ground(GreyRock(3), 9, 14);
            Ground(DarkWeed(3), 4, 14, PropLayer.Front);

            // --- Patamar do topo. A cerca fica na beirada do vao, e nao do
            //     lado do corredor, senao parece que a passagem esta fechada ---
            Ground(Fence(0), 33, 22);
            Ground(Fence(1), 34, 22);
            Ground(GreyWeed(1), 35, 22, PropLayer.Back);
            Ore(BlueBand, 36, 22, PropLayer.Back);
            Ground(DarkWeed(4), 38, 22, PropLayer.Front);

            // --- Andar 1 ---
            Ground(GreyRock(2), 31, 40);
            Ground(Debris(1), 30, 40);
            Ground(DarkWeed(1), 23, 40, PropLayer.Front);
            Ground(Crate(0), 15, 40);
            Ground(Debris(0), 14, 40);
            Ground(Crate(1), 9, 40);
            Ore(GreyBand, 21, 40, PropLayer.Back);
            Ore(RustBand, 29, 43, PropLayer.Back);

            // --- Andar 2 ---
            Ground(GreyRock(1), 10, 50);
            Ground(Crate(3), 11, 50);
            Ground(Debris(0), 27, 50);
            Ground(GreyRock(3), 34, 52);
            Ground(Fence(0), 46, 50);
            Ground(Fence(1), 47, 50);
            Ground(DarkRock(1), 38, 50, PropLayer.Front);
            Ore(BlueBand, 29, 50, PropLayer.Back);
            Ore(GreyBand, 42, 50, PropLayer.Back);
            Ore(BlueBand, 20, 57, PropLayer.Middle);
            Ore(RustBand, 60, 44, PropLayer.Middle);
            Ore(GreyBand, 70, 52, PropLayer.Middle);
            Ore(BlueBand, 58, 55, PropLayer.Middle);
            Ore(RustBand, 12, 47, PropLayer.Middle);

            // --- O abismo ---
            Ground(GreyRock(3), 82, 22);
            Ground(Debris(1), 85, 22);
            Ground(Fence(0), 95, 22);
            Ground(Fence(1), 96, 22);
            Ground(DarkWeed(2), 94, 22, PropLayer.Front);
            Ore(BlueBand, 104, 30, PropLayer.Middle);
            Ore(RustBand, 90, 12, PropLayer.Middle);
            Ore(GreyBand, 104, 18, PropLayer.Middle);

            // --- Arena: os dois patamares ---
            Ground(Fence(2), 50, 22);
            Ground(Fence(0), 51, 22);
            Ground(Fence(1), 74, 22);
            Ground(Fence(2), 75, 22);
            Ground(GreyRock(3), 79, 22);
            Ground(DarkWeed(3), 77, 22, PropLayer.Front);

            // --- Arena: o fosso. A sucata fica amontoada nos cantos, encostada
            //     nos patamares; o meio fica livre pra luta ---
            Ground(Crate(1), 52, 20);
            Stacked(Crate(0), 52, 21);
            Ground(Debris(0), 53, 20);
            Ground(GreyRock(2), 54, 20);
            Ground(Debris(1), 71, 20);
            Ground(Crate(3), 72, 20);
            Stacked(Debris(1), 72, 21);
            Ground(GreyRock(0), 73, 20);
            Ground(GreyWeed(1), 56, 20, PropLayer.Back);
            Ore(RustBand, 57, 20, PropLayer.Back);
            Ore(GreyBand, 67, 20, PropLayer.Back);
            Ground(GreyWeed(3), 69, 20, PropLayer.Back);
            Ground(DarkWeed(1), 59, 20, PropLayer.Front);
            Ground(DarkWeed(4), 65, 20, PropLayer.Front);
            Ground(DarkRock(3), 66, 20, PropLayer.Front);

            // --- Mato nas plataformas, so pra elas nao ficarem peladas ---
            Ground(GreyWeed(4), 30, 8, PropLayer.Back);
            Ground(GreyWeed(4), 13, 12, PropLayer.Back);
            Ground(GreyWeed(2), 22, 18, PropLayer.Back);

            // --- Cristais apagados e veios, incrustados na rocha ---
            Ore(BlueBand, 41, 17, PropLayer.Middle);
            Ore(BlueBand, 7, 27, PropLayer.Middle);
            Ore(BlueBand, 30, 31, PropLayer.Middle);
            Ore(GreyBand, 1, 15, PropLayer.Middle);
            Ore(GreyBand, 41, 6, PropLayer.Middle);
            Ore(RustBand, 20, 1, PropLayer.Middle);
            Ore(RustBand, 1, 31, PropLayer.Middle);
            Ore(BlueBand, 9, 1, PropLayer.Middle);

            // --- O mesmo, no macico de rocha embaixo e em volta da arena ---
            Ore(BlueBand, 46, 12, PropLayer.Middle);
            Ore(RustBand, 50, 5, PropLayer.Middle);
            Ore(GreyBand, 55, 15, PropLayer.Middle);
            Ore(BlueBand, 61, 9, PropLayer.Middle);
            Ore(RustBand, 66, 3, PropLayer.Middle);
            Ore(GreyBand, 70, 13, PropLayer.Middle);
            Ore(RustBand, 76, 8, PropLayer.Middle);
            Ore(BlueBand, 81, 28, PropLayer.Middle);
            Ore(GreyBand, 81, 14, PropLayer.Middle);
            Ore(BlueBand, 43, 29, PropLayer.Middle);

            return props.ToArray();
        }

        // ------------------------------------------------------------------
        // Luz, nevoa e poeira
        // ------------------------------------------------------------------

        /// <summary>
        /// A luz em volta de onde o Scrap acorda. E a unica quente do poco: ele
        /// tem que ser o ponto mais vivo da tela.
        /// </summary>
        public static (float X, float Y) WakeGlowPosition => (21.5f, 7.5f);

        /// <summary>
        /// A luz fria que desce pela chamine. Fica bem em cima do patamar do
        /// topo, entao mostra de longe pra onde e a subida — e a saida.
        /// </summary>
        public static (float X, float Y) ChimneyLightPosition => (36f, 28.5f);

        /// <summary>
        /// Um resto de brasa cor de ferrugem na boca do tunel da arena. E a
        /// unica luz dessa cor no mapa: algo mora ali.
        /// </summary>
        public static (float X, float Y) LairGlowPosition => (62.5f, 21.5f);

        /// <summary>Onde o Oxide brilha verde-cobre: no chao e atras das janelas.</summary>
        public static readonly (float X, float Y, float Radius)[] OxideGlows =
        {
            // Poco
            (8f, 6f, 9f),
            (34f, 9f, 8f),
            (21f, 24f, 9f),
            (7f, 16f, 5f),

            // Arena: um em cada patamar e um atras da janela grande. O meio do
            // fosso fica de proposito mais escuro que as beiradas.
            (50f, 24f, 7f),
            (76f, 24f, 7f),
            (63f, 29f, 9f),

            // Os dois andares de cima e os pocos que os ligam.
            (35.5f, 34f, 7f),
            (27f, 43f, 8f),
            (18f, 43f, 8f),
            (6f, 49f, 6f),
            (18f, 53f, 9f),
            (30f, 53f, 8f),
            (42f, 52f, 8f),
            (50f, 40f, 8f),

            // O abismo: o verde sobe do fundo, por entre os espinhos.
            (90f, 18f, 9f),
            (84f, 25f, 6f),
            (98f, 26f, 7f),
        };

        /// <summary>Lampioes: a luz quente e parada das salas seguras. So a bancada tem.</summary>
        public static readonly (float X, float Y, float Radius)[] LampGlows =
        {
            (6.5f, 43f, 7f),
        };

        /// <summary>Uma faixa deitada de nevoa do Oxide.</summary>
        public readonly struct FogBank
        {
            public readonly string Name;

            /// <summary>Centro da faixa.</summary>
            public readonly float X;
            public readonly float Y;

            public readonly float Width;
            public readonly float Thickness;

            /// <summary>Opacidade de cada borrao. Acima de 0.2 ja tapa o cenario.</summary>
            public readonly float Alpha;

            /// <summary>Borroes novos por segundo: e o que da a densidade.</summary>
            public readonly float Rate;

            /// <summary>Se passa na frente do Scrap, em vez de atras do terreno.</summary>
            public readonly bool InFront;

            public FogBank(string name, float x, float y, float width, float thickness,
                float alpha, float rate, bool inFront)
            {
                Name = name;
                X = x;
                Y = y;
                Width = width;
                Thickness = thickness;
                Alpha = alpha;
                Rate = rate;
                InFront = inFront;
            }
        }

        /// <summary>
        /// A nevoa, sala por sala. Cada faixa tem a largura da sua sala: uma
        /// faixa so, do tamanho do mapa, passaria por cima do macico de rocha
        /// que fica entre o poco e a arena.
        /// </summary>
        public static readonly FogBank[] FogBanks =
        {
            // Poco: um banco grosso deitado no chao, atras e na frente do
            // Scrap, e um veu fino no meio da subida.
            new FogBank("Fog_Pit_Floor_Back", 22f, 6f, 44f, 4f, 0.16f, 2.5f, false),
            new FogBank("Fog_Pit_Floor_Front", 22f, 4.5f, 44f, 2f, 0.10f, 1.5f, true),
            new FogBank("Fog_Pit_Veil", 22f, 18f, 44f, 3f, 0.07f, 1.2f, false),

            // Arena: mais densa que a do poco, e empocada no fosso.
            new FogBank("Fog_Arena_Pit_Back", 63f, 22f, 30f, 3f, 0.20f, 2.7f, false),
            new FogBank("Fog_Arena_Pit_Front", 63f, 20.5f, 22f, 1.5f, 0.12f, 1.4f, true),

            // Os andares de cima: um veu baixo, so pra o chao nao ficar limpo.
            new FogBank("Fog_Floor1", 22f, 41f, 30f, 2f, 0.12f, 1.6f, false),
            new FogBank("Fog_Floor2", 28f, 51f, 44f, 2f, 0.12f, 2.2f, false),
        };

        /// <summary>
        /// Onde cai poeira de metal: o centro e a largura da faixa do teto de
        /// onde ela sai, a altura dessa faixa, quantos tiles ela cai ate o chao
        /// e quantos graos nascem por segundo.
        /// </summary>
        public static readonly (string Name, float X, float Width, float Top, float Fall, float Rate)[] DustFalls =
        {
            ("MetalDust_Pit", 22f, 44f, 29f, 25f, 14f),
            ("MetalDust_Arena", 63f, 30f, 33f, 13f, 10f),
        };

        // ------------------------------------------------------------------
        // Validacao
        // ------------------------------------------------------------------

        /// <summary>
        /// Confere se o mapa e realmente navegavel, sem precisar jogar.
        ///
        /// Solta o LevelMap.Explore a partir de onde o Scrap nasce — ele anda,
        /// pula, cai e pega carona no vapor da chamine, com as mesmas regras de
        /// alcance pra todas as fases — e confere que todo trecho de chao foi
        /// alcancado.
        ///
        /// Devolve a lista de problemas. Vazia quer dizer que da pra andar pelo
        /// mapa inteiro: do monte de sucata ate a saida da arena e ate a
        /// chamine que leva ao District 2.
        /// </summary>
        public static List<string> Validate()
        {
            List<string> problems = LevelMap.RowProblems(Rows, Width, Height);
            if (problems.Count > 0) return problems;

            // A sala e fechada: se a borda tiver ar, a camera mostra o vazio.
            for (int x = 0; x < Width; x++)
            {
                if (!IsRock(x, 0) || !IsRock(x, Height - 1))
                {
                    problems.Add($"A coluna {x} tem buraco no chao ou no teto da sala.");
                }
            }

            for (int y = 0; y < Height; y++)
            {
                if (!IsRock(0, y) || !IsRock(Width - 1, y))
                {
                    problems.Add($"A linha {y} tem buraco numa das paredes da sala.");
                }
            }

            // A partir do poco vertical o caminho conta com o pulo de parede.
            // Sem o Steam Dash o abismo tem que barrar; com ele (que soma uns
            // 3 tiles a um pulo) e com o vapor da saida, o mapa todo abre.
            var noDash = new LevelMap.JumpRules { WallJump = true };
            var rules = new LevelMap.JumpRules { WallJump = true, MaxRun = 8 };

            LevelMap.Reach beforeBoss = Map.Explore(SpawnTile, noDash);
            LevelMap.Reach reach = Map.Explore(SpawnTile, rules, new[] { District2Vent });

            if (beforeBoss.CanStand(FarLedgeTile.X, FarLedgeTile.Y))
            {
                problems.Add("Da pra cruzar o abismo sem o Steam Dash: ele nao esta barrando nada.");
            }

            if (!beforeBoss.CanStand(BossSpawnTile.X, BossSpawnTile.Y))
            {
                problems.Add("Nao da pra chegar na arena sem o Steam Dash, e e la que ele se ganha.");
            }

            bool exitTouched = false;
            for (int y = District2Exit.Y; y < District2Exit.Y + District2Exit.Height; y++)
            {
                for (int x = District2Exit.X; x < District2Exit.X + District2Exit.Width; x++)
                {
                    exitTouched |= reach.CanTouch(x, y);
                }
            }

            if (!exitTouched)
            {
                problems.Add("Nem com o Steam Dash e o vapor da pra chegar na saida pro District 2.");
            }

            List<LevelMap.Perch> perches = Map.FindPerches();

            foreach (LevelMap.Perch perch in perches)
            {
                // Um toco de 1 tile nao e lugar que o desenho precise: sao as
                // pontas das saliencias das paredes.
                if (perch.Length < 2) continue;

                bool reached = false;
                for (int x = perch.X; x <= perch.Right && !reached; x++)
                {
                    reached = reach.CanStand(x, perch.Y);
                }

                if (!reached)
                {
                    problems.Add($"O trecho de chao em {perch} esta fora de alcance a partir de onde o Scrap nasce.");
                }
            }

            void MustReach((int X, int Y) tile, string what)
            {
                if (!IsStandable(tile.X, tile.Y))
                {
                    problems.Add($"{what} ({tile.X}, {tile.Y}) nao e um lugar onde da pra ficar em pe.");
                }
                else if (!reach.CanStand(tile.X, tile.Y))
                {
                    problems.Add($"{what} ({tile.X}, {tile.Y}) esta fora de alcance a partir de onde o Scrap nasce.");
                }
            }

            MustReach(LandingTile, "O patamar do topo do poco");
            MustReach(ReturnTile, "A chegada de quem volta do District 2");
            MustReach(LeverTile, "A alavanca");
            MustReach((WorkbenchTile.X + 1, WorkbenchTile.Y), "A bancada");
            MustReach(EntryGateTile, "A entrada da arena");

            foreach (NpcSpot npc in Npcs)
            {
                MustReach((npc.X, npc.Y), $"O personagem {npc.Name}");
            }

            foreach (Patrol crawler in Crawlers)
            {
                for (int x = crawler.X0; x <= crawler.X1; x++)
                {
                    if (!IsStandable(x, crawler.Y))
                    {
                        problems.Add($"{crawler.Name} patrulha ({x}, {crawler.Y}), que nao e chao.");
                    }
                }
            }

            // O portao tem que importar: com ele fechado da pra chegar na
            // alavanca, e nao da pra chegar na bancada.
            Map.Fill(Gate.X, Gate.Y, Gate.X, Gate.Y + Gate.Height - 1, LevelMap.Rock);
            LevelMap.Reach locked = Map.Explore(SpawnTile, noDash);
            Map.Fill(Gate.X, Gate.Y, Gate.X, Gate.Y + Gate.Height - 1, LevelMap.Air);

            if (!locked.CanStand(LeverTile.X, LeverTile.Y))
            {
                problems.Add("Com o portao fechado nao da pra chegar na alavanca que o abre.");
            }

            if (locked.CanStand(WorkbenchTile.X + 1, WorkbenchTile.Y))
            {
                problems.Add("Da pra chegar na bancada com o portao fechado: ele nao esta fechando nada.");
            }
            MustReach(BossSpawnTile, "O ponto do mini boss");
            MustReach(ExitGateTile, "A saida da arena");
            MustReach(FarLedgeTile, "A beirada de la do abismo");

            void MustBeOpen((int X, int Y, int Width, int Height) area, string what)
            {
                for (int y = area.Y; y < area.Y + area.Height; y++)
                {
                    for (int x = area.X; x < area.X + area.Width; x++)
                    {
                        if (IsSolid(x, y)) problems.Add($"{what} esta entupida em ({x}, {y}).");
                    }
                }
            }

            MustBeOpen(Chimney, "O poco vertical");
            MustBeOpen(ExitStub, "A saida da arena");

            // O mini boss precisa de espaco pra andar: o fosso tem que ser um
            // chao so, sem degrau no meio.
            LevelMap.Perch arenaFloor = perches.Find(perch => perch.Contains(BossSpawnTile.X, BossSpawnTile.Y));
            if (arenaFloor.Length < 16)
            {
                problems.Add($"O fosso da arena tem so {arenaFloor.Length} tiles de chao continuo: e pouco pra uma luta.");
            }

            for (int i = 0; i < Windows.Length; i++)
            {
                Window window = Windows[i];

                // Uma janela atras de rocha aparece cortada, e some sem aviso.
                for (int y = window.Y; y < window.Y + window.Height; y++)
                {
                    for (int x = window.X; x < window.X + window.Width; x++)
                    {
                        if (IsRock(x, y))
                        {
                            problems.Add($"A janela {window.Sprite} em ({window.X}, {window.Y}) " +
                                         $"fica atras de rocha em ({x}, {y}).");
                        }
                    }
                }

                // E duas janelas uma em cima da outra desenham dois pedacos de
                // parede no mesmo lugar.
                for (int j = i + 1; j < Windows.Length; j++)
                {
                    Window other = Windows[j];

                    bool apart = window.X + window.Width <= other.X
                        || other.X + other.Width <= window.X
                        || window.Y + window.Height <= other.Y
                        || other.Y + other.Height <= window.Y;

                    if (!apart)
                    {
                        problems.Add($"As janelas em ({window.X}, {window.Y}) e ({other.X}, {other.Y}) se sobrepoem.");
                    }
                }
            }

            var taken = new HashSet<(int, int, PropLayer)>();
            foreach (Prop prop in Props)
            {
                if (!taken.Add((prop.X, prop.Y, prop.Layer)))
                {
                    problems.Add($"Dois objetos na mesma celula ({prop.X}, {prop.Y}) da camada {prop.Layer}.");
                }

                if (!prop.Grounded) continue;

                if (IsSolid(prop.X, prop.Y))
                {
                    problems.Add($"{prop.Tile} em ({prop.X}, {prop.Y}) esta dentro do terreno.");
                }
                else if (!IsSolid(prop.X, prop.Y - 1))
                {
                    problems.Add($"{prop.Tile} em ({prop.X}, {prop.Y}) esta flutuando: nao ha chao embaixo.");
                }
            }

            return problems;
        }
    }
}
