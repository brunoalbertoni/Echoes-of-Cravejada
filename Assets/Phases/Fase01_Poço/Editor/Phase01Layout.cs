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
    /// alavanca emperrada: bater com o cinzel ate o portao abrir. Atras dele, a bancada.
    ///
    /// O ANDAR 2 (mais acima, voltando pra leste). O desafio misto: tudo junto.
    ///
    /// A TRIAGEM (o andar 2 continuando pra leste). A galeria onde o Coletor
    /// separava as pecas: dois fossos de espinhos e mais corrompidos. Termina
    /// num buraco no chao.
    ///
    /// A ANTESSALA (embaixo da Triagem, voltando pra oeste). Um corredor com
    /// espinhos que acaba na segunda bancada, do lado do poco que desce na
    /// arena: quem morre pro boss renasce perto dele.
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
    public static partial class Phase01Layout
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
            "####....................................#####...................................#####..........#############",
            "####....................................#####...................................#####..........#############",
            "####.........................................................................==................#############",
            "####...........................................................................................#############",
            "####.............................###.....................................==....................#############",
            "####............==..==...........###...........................................................#############",
            "####....######^^^^^^^^^#####################################^^^^######^^^^^^^^############.....#############",
            "####....##################################################################################==...#############",
            "####....##################################################################################.....###....######",
            "####....##################################################################################...==###....######",
            "####....##............................####################################################.....###....######",
            "####....##............................####################################################==...###....######",
            "####....##............................###########...######################################.....###....######",
            "####..................................###########............................................==###....######",
            "####..................................###########..............................................###....######",
            "####..................................###########.........................................==...###....######",
            "#################^^^######^^^#####....###########..............................................###....######",
            "##################################....###########............................................==###....######",
            "##################################....###########..............................................###....######",
            "##################################....###########...#...##########^^^^############################....######",
            "##################################....###########...#...##########################################....######",
            "##################################....###########...#...##########################################....######",
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
            "#####............................###############...==....................==...#########.......##############",
            "#####............................###############..............................#########.......##############",
            "#####.....................=====.........###############################################.......##############",
            "####....................................###############################################.......##############",
            "####...............=====................###########################...............#####.......##############",
            "####...................................############################...............#####^^^^^^^##############",
            "####........=====......................############################...............##########################",
            "####...................................#####...............####...................##########################",
            "##########............................######...............####...................##########################",
            "##########............................######...............####....###.....#################################",
            "####........=====.....................######...............####....###.....#################################",
            "####..................................######...............####....###.....#################################",
            "####................=====..............#####...............####....###.....#################################",
            "#####..................................#...........................###.....#################################",
            "#####.......................=====..................................###.....#################################",
            "#####......................................................###########.....#################################",
            "#####.............########.........#############^^^^^^^^^^^###########.....#################################",
            "#########.......############.......###################################.....#################################",
            "######################################################################.....#################################",
            "######################################################################^^^^^#################################",
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
        /// e ar; quem a fecha e o objeto do portao, ate a alavanca ser solta.
        /// </summary>
        public static (int X, int Y, int Height) Gate => (10, 40, 6);

        /// <summary>A alavanca emperrada, do lado de ca do portao.</summary>
        public static (int X, int Y) LeverTile => (13, 40);

        /// <summary>A Bancada de Ourives, do lado de la. Canto de baixo, a esquerda.</summary>
        public static (int X, int Y) WorkbenchTile => (5, 40);

        /// <summary>A segunda bancada, no fim da Antessala, do lado do poco que desce na arena.</summary>
        public static (int X, int Y) PreBossWorkbenchTile => (57, 37);

        /// <summary>Todas as bancadas do distrito, na ordem em que se acha.</summary>
        public static (int X, int Y)[] Workbenches => new[] { WorkbenchTile, PreBossWorkbenchTile };

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

            // A Triagem: um entre os dois fossos, outro antes do buraco.
            new Patrol("Crawler_Sorting_1", 64, 69, 50),
            new Patrol("Crawler_Sorting_2", 79, 88, 50),

            // A Antessala: o ultimo antes da bancada.
            new Patrol("Crawler_Antechamber", 72, 86, 37),
        };

        /// <summary>O que um personagem faz alem de conversar.</summary>
        public enum NpcRole
        {
            /// <summary>So conversa.</summary>
            Plain,

            /// <summary>O Ferrolho: monta no Scrap o motor do Coletor, que vira o Steam Dash.</summary>
            MotorFitter,

            /// <summary>O Rebite: pede as tres ferramentas do mestre e paga com uma gema.</summary>
            ToolsQuest,
        }

        /// <summary>
        /// Um personagem com quem da pra conversar. As falas ficam aqui, uma
        /// por tela; quais delas ele diz depende de como o distrito esta.
        /// </summary>
        public sealed class NpcSpot
        {
            public string Name;
            public int X;
            public int Y;

            /// <summary>Qual corpo de lata (ver DiscardedArt): 0 Refugo, 1 Rebite, 2 Ferrolho, 3 Pequena.</summary>
            public int Variant;

            /// <summary>A escala do desenho. O da Pequena ja e pequeno.</summary>
            public float Scale = 1f;

            public NpcRole Role = NpcRole.Plain;

            /// <summary>So aparece depois que o Coletor cai, com a purificacao.</summary>
            public bool AfterBoss;

            /// <summary>O que ele diz com o Oxide no ar.</summary>
            public string[] Lines;

            /// <summary>O que ele diz depois da purificacao. Vazio repete Lines.</summary>
            public string[] PurifiedLines;

            // --- MotorFitter ---

            /// <summary>Depois da purificacao, enquanto o Scrap nao traz o motor.</summary>
            public string[] WaitingLines;

            /// <summary>Com o motor na mao: ao fim desta conversa o Scrap ganha o Steam Dash.</summary>
            public string[] InstallLines;

            // --- ToolsQuest ---

            /// <summary>A abertura de qualquer conversa depois da purificacao.</summary>
            public string PurifiedGreeting;

            /// <summary>Depois de ouvir o pedido: com nenhuma, uma ou duas ferramentas na mao.</summary>
            public string[] NoToolLines;
            public string[] OneToolLines;
            public string[] TwoToolLines;

            /// <summary>Com as tres: ao fim desta conversa ele entrega a gema.</summary>
            public string[] CompleteLines;

            // --- Os dois ---

            /// <summary>Depois que o servico acabou: o motor montado, as ferramentas entregues.</summary>
            public string[] DoneLines;
        }

        /// <summary>
        /// Os primeiros personagens do jogo: autômatos velhos, de lataria
        /// amassada, descartados no poco antes do Scrap.
        /// </summary>
        public static readonly NpcSpot[] Npcs =
        {
            // No fundo do poco, ao lado de onde o Scrap acorda: a historia
            // inicial. Da o nome ao Scrap, conta a divisao da cidade (prata em
            // cima, cobre e latao embaixo) e a onda de Oxide que desceu pros
            // pobres. De passagem, fala do Noble riscado: e o Crisol do
            // District 2. Depois da purificacao, aponta a obra abandonada.
            new NpcSpot
            {
                Name = "Refugo", X = 13, Y = 4, Variant = 0,
                Lines = new[]
                {
                    "Ei... mais um caiu lá de cima. E esse ainda tá com os olhos acesos.",
                    "Olha só pra você, todo amassado. Parece um monte de sucata andando. Vou te chamar de Scrap.",
                    "Bem-vindo ao Poço de Sucata, Scrap. É aqui que Cravejada, a Cidade de Prata, joga tudo que não quer mais.",
                    "Me chamam de Refugo. Eu acendia os lampiões da Avenida das Facetas, lá em cima.",
                    "Lá em cima quem manda é a prata. Os 925 Nobres vivem nos palácios do alto. A gente de cobre e latão mora embaixo e faz o serviço pesado.",
                    "Mas pro Rei de Prata nunca era o bastante. Ele queria mais poder, e mandou cavar até o Coração de Quartzo, a pedra que move a cidade.",
                    "Quando a broca furou o Coração, saiu uma onda verde enorme. O Óxido.",
                    "Ele escorreu pra baixo, feito água. Os palácios quase nem sentiram. Os bairros de baixo foram engolidos.",
                    "O Óxido come o metal, apaga os cristais e mexe com a cabeça da gente até sobrar só raiva.",
                    "Aí a Coroa começou a jogar aqui quem pegava. Depois, quem só rangia. Depois, qualquer um de cobre ou latão.",
                    "Uma vez caiu até um de prata, todo riscado. Não ficou nem um dia. Subiu pelos canos falando sozinho de pedras e coroas.",
                    "Mas você... o verde passa por você e não gruda. Nunca vi isso.",
                    "Sobe, Scrap. Daqui de baixo, o único jeito é pra cima.",
                },
                PurifiedLines = new[]
                {
                    "Tá sentindo? O ar ficou leve. Fazia tanto tempo que eu não via o poço sem verde que já tinha esquecido a cor da pedra.",
                    "Ouvi um estrondo enorme lá de cima. Achei que era a calha jogando mais um... mas era você, né?",
                    "A névoa que tampava o túnel aqui do lado também sumiu. Lá atrás tem uma obra da Coroa.",
                    "Iam construir um elevador de carga ali. Trouxeram ferro, andaime, caixa de peça nova... e um dia largaram tudo e foram embora.",
                    "Foi no mesmo dia em que o verde desceu. Ninguém voltou pra buscar nada.",
                    "Dizem que ainda tem coisa boa lá no fundo. Mas o último vão é largo demais pra um pulo comum.",
                },
            },

            // Na sala da bancada: de onde ela veio, o que ela faz e o que
            // acontece ao morrer. No fim, pede as ferramentas do mestre.
            new NpcSpot
            {
                Name = "Rebite", X = 8, Y = 40, Variant = 1, Role = NpcRole.ToolsQuest,
                Lines = new[]
                {
                    "Você soltou aquela alavanca na pancada? Ela tava emperrada desde que eu cheguei aqui.",
                    "Isso aqui é uma Bancada de Ourives. Aperta E nela, e ela guarda onde você tá e conserta seu corpo.",
                    "Ela caiu pela calha junto com meu mestre, um ourives de latão. O Óxido pegou primeiro nas mãos dele. Eu era o aprendiz, vim atrás.",
                    "Ele não levantou mais. A bancada, sim. Continua funcionando, como se ainda esperasse por ele.",
                    "Se você quebrar lá fora, volta pra cá. Mas os cacos que tava carregando ficam onde você caiu.",
                    "Vai lá buscar antes de quebrar de novo. Se quebrar duas vezes, eles somem de vez.",
                    "E fica esperto: enquanto você se monta de novo, os corrompidos também levantam. Todos eles.",
                    "Mais uma coisa. As ferramentas do meu mestre se espalharam por aí quando ele caiu: um buril, uma lupa e uma pinça.",
                    "Se achar, traz pra mim. Não quero elas largadas no meio da sucata.",
                    "O caminho continua por cima. Pula de uma parede pra outra, igual você fez no poço.",
                },
                PurifiedGreeting = "A luz voltou. Olha a bancada: o latão tá brilhando igual no dia em que o mestre poliu.",
                NoToolLines = new[]
                {
                    "Nada ainda? O buril, a lupa e a pinça. Ele nunca largava as três.",
                    "Procura nos cantos onde ninguém vai. A sucata esconde bem as coisas pequenas.",
                },
                OneToolLines = new[]
                {
                    "Você achou uma! Faltam duas.",
                    "Guarda com você até achar as outras. Quero ver o conjunto inteiro de novo.",
                },
                TwoToolLines = new[]
                {
                    "Duas! Só falta uma.",
                    "Ele dizia que ferramenta separada é ferramenta perdida. Acha a última, por favor.",
                },
                CompleteLines = new[]
                {
                    "O buril, a lupa e a pinça... São dele mesmo. O cabo da lupa ainda tem a marca dos dedos.",
                    "Vou deixar aqui na bancada. É o lugar delas.",
                    "Toma. É a última gema que ele lapidou. Nunca chegou a entregar pra ninguém.",
                    "Aqui embaixo ela não compra nada. Mas guarda: lá em cima, uma gema lapidada vale muito.",
                },
                DoneLines = new[]
                {
                    "As ferramentas tão no lugar delas agora. Valeu, Scrap.",
                    "Às vezes eu olho a sucata pela lupa. Tudo parece maior. Até a gente.",
                },
            },

            // No comeco do andar 2: quem era o Coletor. Era mecanico de
            // caldeira: e ele quem monta o motor do Coletor no Scrap, e o
            // motor e o Steam Dash.
            new NpcSpot
            {
                Name = "Ferrolho", X = 12, Y = 50, Variant = 2, Role = NpcRole.MotorFitter,
                Lines = new[]
                {
                    "Psiu. Fala baixo. Lá embaixo mora o Coletor.",
                    "Eu conheci ele antes do Óxido. Era um varredor grande, a vapor. Recolhia o que caía da calha e separava as peças boas das ruins.",
                    "Ele cantarolava enquanto trabalhava. Um zumbido de caldeira. A gente até gostava.",
                    "Aí o verde entrou nas engrenagens dele. Ele continuou recolhendo... só que esqueceu a diferença entre peça solta e gente.",
                    "Ele tem um motor de vapor nas costas, dos bons. Eu era mecânico de caldeira, sei montar um desses.",
                    "Se você der um jeito nele, me traz o motor. Eu coloco em você.",
                },
                WaitingLines = new[]
                {
                    "O verde sumiu... Então você conseguiu. O Coletor caiu mesmo.",
                    "E o motor? Ficou lá na arena, no meio da carcaça. Vai buscar que eu coloco em você.",
                },
                InstallLines = new[]
                {
                    "Isso! O motor do Coletor, ainda quentinho. Segura firme, que vai tremer.",
                    "Pronto, os pistões tão nas suas pernas. Solta o vapor e você dispara pra frente, reto, sem cair.",
                    "Com isso dá pra cruzar o abismo depois da arena. Do outro lado, o vapor sobe até os canos de latão.",
                    "Ele cantarolava quando arrancava. Se ouvir um zumbido nas costas, não se assusta: é só o motor lembrando.",
                },
                DoneLines = new[]
                {
                    "O motor gostou de você. Nem parece que era de outro.",
                    "Se um vão parecer grande demais, dispara no meio do pulo. O vapor te segura no ar um pouquinho.",
                },
            },

            // Na boca da obra abandonada, depois da purificacao: a que se
            // escondia dentro da nevoa, com medo do Coletor. E a "Pequena" do
            // Muro dos Nomes, e da a pista da pinca que esta la dentro.
            new NpcSpot
            {
                Name = "Pequena", X = 37, Y = 6, Variant = 3, AfterBoss = true,
                Lines = new[]
                {
                    "Ai! Não me pega! ...Ah. Você não é ele. Ufa.",
                    "Eu sou a Pequena. Fui eu que escrevi meu nome no muro, lá embaixo. Ficou meio torto, mas é meu.",
                    "Eu tinha muito, muito medo do Coletor. Ele fazia um barulhão, CLANC, CLANC, e levava todo mundo embora.",
                    "Aí eu me escondi dentro da névoa. Ela ardia, e minha lata foi ficando toda verde... mas lá dentro ele nunca entrava.",
                    "Toda vez que eu ouvia ele chegando, tampava os ouvidos e ficava bem quietinha. Mesmo doendo.",
                    "Lá na obra tem uns ferros pontudos no chão, compridos demais pra pular. Dá pra passar quicando neles, com o cinzel virado pra baixo.",
                    "Depois tem um buraco bem fundo, e lá no fim um vão enorme. Eu tentei pular um montão de vezes. Nunca deu.",
                    "Ah! Lá no fundo do buraco tem uma pinça brilhante. Não é minha, juro.",
                    "Agora que ele não vem mais, vou lá no muro escrever embaixo do meu nome: \"ainda aqui\".",
                },
            },
        };

        /// <summary>O desenho de uma coisa que se le com E.</summary>
        public enum LoreKind
        {
            /// <summary>Um papel no chao.</summary>
            Page,

            /// <summary>Riscos na parede, feitos pelos descartados.</summary>
            Graffiti,

            /// <summary>Uma placa de latao pregada na rocha.</summary>
            Plaque,
        }

        /// <summary>
        /// Um pedaco da historia do distrito, pra achar e ler: chegar perto e
        /// apertar E. X e Y sao a celula onde o Scrap para.
        /// </summary>
        public readonly struct LoreSpot
        {
            public readonly LoreKind Kind;
            public readonly string Name;

            /// <summary>O que aparece no aviso, depois de "E: ler".</summary>
            public readonly string Title;

            public readonly int X;
            public readonly int Y;

            /// <summary>So aparece depois que o Rusty Scrapper cai, com a purificacao.</summary>
            public readonly bool AfterBoss;

            public readonly string[] Lines;

            /// <summary>O que se le depois da purificacao, se mudou. Nulo repete Lines.</summary>
            public readonly string[] PurifiedLines;

            public LoreSpot(LoreKind kind, string name, string title, int x, int y, bool afterBoss, string[] lines,
                string[] purifiedLines = null)
            {
                Kind = kind;
                Name = name;
                Title = title;
                X = x;
                Y = y;
                AfterBoss = afterBoss;
                Lines = lines;
                PurifiedLines = purifiedLines;
            }
        }

        /// <summary>
        /// O que se le pelo distrito. Conta, na ordem em que se acha, quem
        /// joga a sucata aqui e por que: a calha da Coroa, o decreto dos
        /// Nobles, o muro dos que ja foram e o que o Coletor era antes.
        /// A etiqueta ao lado do Scrap so faz sentido no District 4, quando
        /// ele acha a propria ficha de prototipo.
        /// </summary>
        public static readonly LoreSpot[] Lore =
        {
            new LoreSpot(LoreKind.Page, "Lore_TornTag", "uma etiqueta rasgada", 22, 6, false, new[]
            {
                "Um pedaço de etiqueta preso num arame partido, caído bem onde você acordou.",
                "A tinta borrou quase toda. Só dá pra ler o fim: \"...TO N.º 7 — DESTINO: POÇO DE SUCATA.\"",
                "No verso, um carimbo vermelho pela metade: \"REJEI...\"",
            }),

            new LoreSpot(LoreKind.Plaque, "Lore_ChutePlaque", "a placa da calha", 31, 4, false, new[]
            {
                "Uma placa de latão pregada na rocha, verde de azinhavre. As letras em relevo ainda dão pra ler:",
                "\"CALHA DE DESCARTE N.º 1 — SERVIÇO DE LIMPEZA DA COROA.\"",
                "\"O QUE NÃO BRILHA, DESCE.\"",
            }),

            new LoreSpot(LoreKind.Graffiti, "Lore_WallOfNames", "o muro dos nomes", 6, 5, false, new[]
            {
                "A rocha está coberta de riscos, uns por cima dos outros: números de série, apelidos, pequenas engrenagens desenhadas.",
                "\"LAMPIÃO-12. AINDA ACENDO.\" \"BIGORNA. TRABALHEI QUARENTA ANOS.\" \"PEQUENA. NUNCA TIVE NÚMERO.\"",
                "Embaixo de todos, em letra grande: \"QUEM LER ISTO, LEMBRE-SE DE NÓS.\"",
                "No fim da parede ainda há espaço. Alguém deixou um prego pendurado ali, à espera do próximo nome.",
            },
            // Depois da purificacao, a Pequena voltou ao muro, e alguem usou o prego.
            new[]
            {
                "A rocha está coberta de riscos, uns por cima dos outros: números de série, apelidos, pequenas engrenagens desenhadas.",
                "Embaixo de \"PEQUENA. NUNCA TIVE NÚMERO.\", alguém acrescentou, em letra fresca: \"AINDA AQUI.\"",
                "O prego que estava pendurado no fim da parede foi usado. No espaço vazio, recém-riscado: \"COLETOR N.º 1. TAMBÉM ERA UM DE NÓS.\"",
                "Embaixo de todos, em letra grande: \"QUEM LER ISTO, LEMBRE-SE DE NÓS.\"",
            }),

            // Logo depois da nevoa: a obra que a Coroa largou no dia em que o
            // Oxide desceu. So se le depois da purificacao, porque antes nao
            // da pra passar.
            new LoreSpot(LoreKind.Plaque, "Lore_TrialPlaque", "a placa da obra", 45, 6, false, new[]
            {
                "Uma placa de obra, de latão novinho. Nem deu tempo de o verde pegar nela.",
                "\"OBRA DA COROA — ELEVADOR DE CARGA N.º 2. LEVARÁ A SUCATA BOA DE VOLTA À CIDADE.\"",
                "\"CRAVEJADA NÃO PARA DE CRESCER.\"",
                "Por cima de tudo, um carimbo vermelho, batido às pressas: \"OBRA SUSPENSA.\"",
            }),

            new LoreSpot(LoreKind.Page, "Lore_Decree", "um decreto dos Nobres", 32, 40, false, new[]
            {
                "Um papel timbrado, com o selo dos 925 em cera prateada. Caiu pela calha junto com alguém.",
                "\"DECRETO DO CONSELHO DOS 925 NOBRES. A corrosão verde não atinge a prata pura.\"",
                "\"Todo autômato de cobre ou de latão que apresentar manchas, rangidos ou desobediência será recolhido e descartado.\"",
                "\"Não há cura. Há descarte. Por ordem de Sua Majestade, o Rei de Prata.\"",
            }),

            // Na entrada da Triagem: o que o Coletor fazia antes do Oxide, nas
            // palavras dele. Confirma o que o Ferrolho conta no andar 2.
            new LoreSpot(LoreKind.Page, "Lore_SortingList", "a lista da triagem", 55, 50, false, new[]
            {
                "Uma folha presa num prego, com duas colunas escritas a giz em letra de máquina, toda igual.",
                "\"PEÇAS BOAS: engrenagens, molas, rebites, lentes. SOBEM PELA CALHA.\"",
                "\"PEÇAS RUINS: todo o resto. FICAM NO POÇO.\"",
                "Embaixo, uma terceira coluna, começada depois, numa letra tremida: \"PEÇAS QUE SE MEXEM:\". Está em branco.",
                "No pé da folha, na mesma letra de máquina, a assinatura: \"COLETOR DE REFUGO N.º 1.\"",
            }),

            // No meio do fosso da arena: aparece quando o Coletor cai. E dela
            // que sai o aro de prata que o Crisol pede no District 2.
            new LoreSpot(LoreKind.Plaque, "Lore_CollectorPlate", "a placa do Coletor", 62, 20, true, new[]
            {
                "Onde o Coletor caiu, sobrou um pedaço de chassi com uma placa de latão rebitada. A ferrugem se soltou com a queda.",
                "\"COLETOR DE REFUGO N.º 1 — SERVIÇO DE LIMPEZA DA COROA. MANTER O POÇO LIMPO. NÃO PARAR.\"",
                "Ninguém nunca desceu pra mandar ele parar.",
                "Preso entre as chapas há algo que não é dele: um aro de prata pequeno, bem polido. Você o guarda.",
            }),
        };

        // ------------------------------------------------------------------
        // As placas de tutorial
        // ------------------------------------------------------------------

        /// <summary>O movimento que a placa ensina. O numero e tambem o desenho dela.</summary>
        public enum TutorialKind
        {
            WallJump = 0,
            Pogo = 1,
        }

        /// <summary>
        /// Uma placa de tutorial: ficar na area dela abre o painel com a mini
        /// cena do movimento. X e Y sao a celula onde ela esta fincada; a area
        /// vai de AreaX0 a AreaX1 (inclusive), da linha Y ate duas acima.
        /// </summary>
        public readonly struct TutorialSpot
        {
            public readonly TutorialKind Kind;
            public readonly string Name;
            public readonly int X;
            public readonly int Y;
            public readonly int AreaX0;
            public readonly int AreaX1;

            public TutorialSpot(TutorialKind kind, string name, int x, int y, int areaX0, int areaX1)
            {
                Kind = kind;
                Name = name;
                X = x;
                Y = y;
                AreaX0 = areaX0;
                AreaX1 = areaX1;
            }
        }

        public static readonly TutorialSpot[] Tutorials =
        {
            // No patamar do topo do poco, embaixo da chamine: o primeiro trecho
            // em que so se sobe de parede em parede. A area e o patamar todo;
            // ao entrar na chamine, o painel recolhe.
            new TutorialSpot(TutorialKind.WallJump, "Tutorial_WallJump", 39, 22, 33, 39),

            // Na obra abandonada, que abre com a purificacao: a beirada logo
            // antes dos espinhos rentes ao chao, que so se cruzam quicando.
            new TutorialSpot(TutorialKind.Pogo, "Tutorial_Pogo", 47, 6, 43, 47),
        };

        // ------------------------------------------------------------------
        // O que se pega
        // ------------------------------------------------------------------

        public enum ItemKind
        {
            /// <summary>Uma das ferramentas do mestre do Rebite. Art diz qual desenho.</summary>
            MasterTool,

            /// <summary>O motor de vapor do Coletor, pra levar ao Ferrolho.</summary>
            Motor,

            /// <summary>Um reforco de chassi: +1 de vida.</summary>
            Chassis,
        }

        /// <summary>Algo que flutua no lugar e o Scrap pega ao encostar. X e Y sao a celula onde ele para.</summary>
        public readonly struct ItemSpot
        {
            public readonly ItemKind Kind;

            /// <summary>Com que nome ele lembra que ja foi pego (e que ja foi entregue).</summary>
            public readonly string Id;

            /// <summary>O aviso na tela ao pegar.</summary>
            public readonly string Message;

            public readonly int X;
            public readonly int Y;

            /// <summary>So aparece depois que o Coletor cai, com a purificacao.</summary>
            public readonly bool AfterBoss;

            /// <summary>Pras ferramentas: 0 buril, 1 lupa, 2 pinca.</summary>
            public readonly int Art;

            public ItemSpot(ItemKind kind, string id, string message, int x, int y, bool afterBoss = false, int art = 0)
            {
                Kind = kind;
                Id = id;
                Message = message;
                X = x;
                Y = y;
                AfterBoss = afterBoss;
                Art = art;
            }
        }

        /// <summary>O comeco do id de cada ferramenta do mestre: e por ele que se contam.</summary>
        public const string MasterToolPrefix = "Tool_";

        public const int MasterToolCount = 3;

        public const string MotorId = "Motor_Collector";

        public static readonly ItemSpot[] Items =
        {
            // As ferramentas do mestre: todas depois do Rebite (o pedido vem
            // antes da busca), menos a da obra abandonada, que fica no poco mas
            // so abre depois do boss. Uma no andar 2, em cima do bloco de
            // rocha; uma na Triagem; uma no fundo da chamine da obra.
            new ItemSpot(ItemKind.MasterTool, MasterToolPrefix + "Graver", "Buril do ourives encontrado", 34, 52, art: 0),
            new ItemSpot(ItemKind.MasterTool, MasterToolPrefix + "Loupe", "Lupa do ourives encontrada", 78, 54, art: 1),
            new ItemSpot(ItemKind.MasterTool, MasterToolPrefix + "Tweezers", "Pinça do ourives encontrada", 64, 7, art: 2),

            // O motor fica no fosso da arena quando o Coletor cai, longe da placa dele.
            new ItemSpot(ItemKind.Motor, MotorId, "Motor de vapor do Coletor: leve-o ao Ferrolho", 67, 20, afterBoss: true),

            // O premio do fim da obra abandonada.
            new ItemSpot(ItemKind.Chassis, "D1_Trial_Chassis", null, 79, 13),
        };

        // ------------------------------------------------------------------
        // A nevoa da obra e o selo da arena
        // ------------------------------------------------------------------

        /// <summary>
        /// O tunel da obra abandonada, a direita do poco: as celulas que a
        /// nevoa densa de Oxide fecha. No mapa sao ar; quem fecha e o objeto,
        /// ate a purificacao.
        /// </summary>
        public static (int X, int Y, int Width, int Height) TrialVeil => (41, 6, 2, 3);

        /// <summary>Uma celula dentro da obra: nao da pra chegar nela com a nevoa no lugar.</summary>
        public static (int X, int Y) TrialInsideTile => (46, 6);

        /// <summary>
        /// A boca do poco que desce na arena: a grade que fecha quando o
        /// Coletor acorda e abre quando ele cai (ou quando o Scrap cai).
        /// </summary>
        public static (int X, int Y, int Width) ArenaSeal => (49, 34, 3);

        /// <summary>
        /// A coluna de vapor que sobe do fosso da arena ate a Antessala, por
        /// um buraco proprio no chao dela: o poco do selo fica so pra descer.
        /// So aparece com a purificacao — antes dela nao se sai da arena.
        /// </summary>
        public static readonly LevelMap.Lift ArenaReturnVent = new LevelMap.Lift(53, 55, 21, 38);

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
        /// O pe do poco que desce da Antessala, ja no chao da arena. A arena e
        /// plana de parede a parede: nao ha patamar de onde bater no Coletor
        /// sem ele alcancar. Quem fecha o poco durante a luta e o ArenaSeal.
        /// </summary>
        public static (int X, int Y) EntryGateTile => (50, 20);

        /// <summary>
        /// Ate onde o Coletor anda: o chao inteiro da arena, de parede a
        /// parede (x 48 a 77), menos meia largura dele de cada lado.
        /// </summary>
        public static (float Min, float Max) BossRange => (48f + 1.7f, 78f - 1.7f);

        /// <summary>O comeco da saida, do outro lado. O portao que o boss destranca.</summary>
        public static (int X, int Y) ExitGateTile => (78, 22);

        /// <summary>
        /// As plataformas baixas da arena, a um tile do chao: tem colisao so
        /// numa tabua fina no topo (ver Phase01Generator.CreateThinSteps), pra
        /// o Scrap passar por baixo sem travar. Por cima continuam degrau.
        /// </summary>
        public static readonly (int X0, int X1, int Y)[] ThinSteps = { (51, 52, 21), (73, 74, 21) };

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
        public static readonly Prop[] Props = BuildProps();
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
            // A da frente fica no meio do fosso, sem chegar nos degraus dos lados.
            new FogBank("Fog_Arena_Pit_Front", 63f, 20.5f, 18f, 1.5f, 0.12f, 1.4f, true),

            // Os andares de cima: um veu baixo, so pra o chao nao ficar limpo.
            new FogBank("Fog_Floor1", 22f, 41f, 30f, 2f, 0.12f, 1.6f, false),
            new FogBank("Fog_Floor2", 28f, 51f, 44f, 2f, 0.12f, 2.2f, false),
            new FogBank("Fog_Sorting", 73f, 51f, 42f, 2f, 0.12f, 2.0f, false),

            // A nevoa que tampa o tunel da obra abandonada: grossa a ponto de
            // tapar o que esta atras, na frente do Scrap. Some com o resto.
            new FogBank("Fog_TrialVeil", 41.5f, 7.5f, 2.5f, 3f, 0.5f, 7f, true),
            new FogBank("Fog_Antechamber", 78f, 38f, 30f, 2f, 0.10f, 1.4f, false),
        };
    }
}
