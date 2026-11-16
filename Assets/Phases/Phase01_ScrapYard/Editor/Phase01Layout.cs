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

        // objects_misc.png: linha 2 sao pedacos de cerca (0-2). Os caixotes da
        // linha 0 sairam: no meio da sucata eles destoavam.
        private static string Fence(int variant) => $"misc_c{variant}_r2";

        // bg_objects.png: linha 0 pedras escuras, 1 pedras cinza, 2 mato seco
        // escuro, 3 mato seco cinza. Em todas, as colunas vao do maior pro menor.
        private static string DarkRock(int variant) => $"bgobj_c{variant}_r0";
        private static string GreyRock(int variant) => $"bgobj_c{variant}_r1";
        private static string DarkWeed(int variant) => $"bgobj_c{variant}_r2";
        private static string GreyWeed(int variant) => $"bgobj_c{variant}_r3";

        /// <summary>
        /// Os geodos: o losango grande de minerio, 2x2 tiles, com o canto de
        /// baixo a esquerda em (X, Y). So moram dentro da rocha, com pelo menos
        /// um tile de rocha em volta (o Validate cobra): na beirada ou no ar
        /// eles parecem pedras soltas.
        /// </summary>
        public static readonly (int Band, int X, int Y)[] Geodes =
        {
            // O teto dos andares de cima.
            (BlueBand, 20, 57),
            (BlueBand, 58, 57),
            (RustBand, 60, 44),
            (GreyBand, 70, 46),
            (RustBand, 11, 47),

            // As paredes e o chao do poco.
            (BlueBand, 41, 17),
            (BlueBand, 7, 27),
            (BlueBand, 30, 31),
            (GreyBand, 1, 15),
            (RustBand, 20, 1),
            (RustBand, 1, 31),
            (BlueBand, 9, 1),

            // O macico de rocha em volta da arena, da obra e do abismo.
            (BlueBand, 43, 29),
            (GreyBand, 55, 16),
            (RustBand, 66, 3),
            (RustBand, 76, 8),
            (BlueBand, 81, 28),
            (RustBand, 90, 12),
            (BlueBand, 104, 30),
            (GreyBand, 104, 18),
        };

        public static readonly Prop[] Props = BuildProps();

        private static Prop[] BuildProps()
        {
            var props = new List<Prop>();

            void Ground(string tile, int x, int y, PropLayer layer = PropLayer.Middle)
            {
                props.Add(new Prop(tile, x, y, layer, true));
            }

            // --- O monte de sucata onde o Scrap acorda ---
            Ground(GreyRock(4), 16, 5);
            Ground(GreyRock(2), 18, 6);
            Ground(GreyRock(1), 25, 6);
            Ground(GreyRock(3), 27, 5);

            // --- Chao, lado oeste ---
            Ground(Fence(0), 6, 5);
            Ground(Fence(1), 7, 5);
            Ground(GreyRock(0), 11, 4);
            Ground(GreyRock(2), 12, 4);
            Ground(GreyWeed(0), 9, 4, PropLayer.Back);
            Ground(DarkWeed(1), 5, 5, PropLayer.Front);
            Ground(DarkRock(1), 10, 4, PropLayer.Front);
            Ground(DarkWeed(2), 15, 4, PropLayer.Front);

            // --- Chao, lado leste ---
            Ground(Fence(2), 33, 4);
            Ground(GreyRock(1), 34, 4);
            Ground(DarkRock(2), 28, 4, PropLayer.Front);
            Ground(DarkWeed(0), 32, 4, PropLayer.Front);

            // --- Degrau da parede leste ---
            Ground(GreyWeed(3), 36, 6, PropLayer.Back);
            Ground(GreyRock(4), 39, 6);

            // --- Saliencia da parede oeste, na boca do tunel ---
            Ground(GreyRock(3), 9, 14);
            Ground(DarkWeed(3), 4, 14, PropLayer.Front);

            // --- Patamar do topo. A cerca fica na beirada do vao, e nao do
            //     lado do corredor, senao parece que a passagem esta fechada ---
            Ground(Fence(0), 33, 22);
            Ground(Fence(1), 34, 22);
            Ground(GreyWeed(1), 35, 22, PropLayer.Back);
            Ground(DarkWeed(4), 38, 22, PropLayer.Front);

            // --- Andar 1 ---
            Ground(GreyRock(2), 31, 40);
            Ground(DarkWeed(1), 23, 40, PropLayer.Front);

            // --- Andar 2 ---
            Ground(GreyRock(1), 10, 50);
            Ground(GreyRock(3), 34, 52);
            Ground(Fence(0), 46, 50);
            Ground(Fence(1), 47, 50);
            Ground(DarkRock(1), 38, 50, PropLayer.Front);

            // --- A Triagem: as pecas que o Coletor separou e nunca subiram ---
            Ground(GreyRock(3), 57, 50);
            Ground(DarkWeed(2), 67, 50, PropLayer.Front);

            // --- A Antessala ---
            Ground(Fence(0), 61, 37);
            Ground(Fence(1), 62, 37);
            Ground(GreyRock(1), 64, 37);
            Ground(GreyWeed(2), 76, 37, PropLayer.Back);
            Ground(DarkRock(2), 88, 37, PropLayer.Front);

            // --- O abismo ---
            Ground(GreyRock(3), 82, 22);
            Ground(Fence(0), 95, 22);
            Ground(Fence(1), 96, 22);
            Ground(DarkWeed(2), 94, 22, PropLayer.Front);

            // --- Arena: o degrau da saida. Os patamares de cada lado do fosso
            //     sairam: de cima deles dava pra bater no Coletor sem ele alcancar ---
            Ground(Fence(1), 79, 22);
            Ground(Fence(2), 80, 22);

            // --- Arena: o fosso. So mato baixo, atras, longe das beiradas das
            //     plataformas; nada na frente do Scrap nem colado embaixo delas,
            //     pra luta ficar legivel ---
            Ground(GreyWeed(1), 56, 20, PropLayer.Back);
            Ground(GreyWeed(3), 69, 20, PropLayer.Back);

            // --- Mato nas plataformas, so pra elas nao ficarem peladas ---
            Ground(GreyWeed(4), 30, 8, PropLayer.Back);
            Ground(GreyWeed(4), 13, 12, PropLayer.Back);
            Ground(GreyWeed(2), 22, 18, PropLayer.Back);

            // --- Os geodos, incrustados na rocha. O losango ocupa 2x2 tiles:
            //     colunas 1-2, linhas 2-3 da banda de cor (a 2 e a metade de cima) ---
            foreach ((int band, int x, int y) in Geodes)
            {
                props.Add(new Prop(TileName(1, band + 2), x, y + 1, PropLayer.Middle, false));
                props.Add(new Prop(TileName(2, band + 2), x + 1, y + 1, PropLayer.Middle, false));
                props.Add(new Prop(TileName(1, band + 3), x, y, PropLayer.Middle, false));
                props.Add(new Prop(TileName(2, band + 3), x + 1, y, PropLayer.Middle, false));
            }

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

            // A Triagem e a Antessala. O canto da bancada fica de fora: e a
            // luz quente do lampiao que tem que mandar ali.
            (62f, 52f, 7f),
            (74f, 52f, 8f),
            (88f, 52f, 7f),
            (92f, 40f, 7f),
            (78f, 39f, 8f),

            // A obra abandonada: nao se ve antes da purificacao, e depois
            // viram os lampioes que a iluminam.
            (52f, 9f, 8f),
            (65f, 11f, 7f),
            (79f, 15f, 7f),

            // O abismo: o verde sobe do fundo, por entre os espinhos.
            (90f, 18f, 9f),
            (84f, 25f, 6f),
            (98f, 26f, 7f),
        };

        /// <summary>Lampioes: a luz quente e parada das salas seguras. So as bancadas tem.</summary>
        public static readonly (float X, float Y, float Radius)[] LampGlows =
        {
            (6.5f, 43f, 7f),
            (58.5f, 40f, 7f),
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

        /// <summary>
        /// Onde cai poeira de metal: o centro e a largura da faixa do teto de
        /// onde ela sai, a altura dessa faixa, quantos tiles ela cai ate o chao
        /// e quantos graos nascem por segundo.
        /// </summary>
        public static readonly (string Name, float X, float Width, float Top, float Fall, float Rate)[] DustFalls =
        {
            ("MetalDust_Pit", 22f, 44f, 29f, 25f, 14f),
            ("MetalDust_Arena", 63f, 30f, 33f, 13f, 10f),

            // Um fio de poeira caindo da boca da Calha N.º 1 em cima do monte onde o Scrap acorda.
            ("MetalDust_Chute", 21.5f, 1.6f, 25.4f, 19f, 6f),
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

            // A partir do poco vertical o caminho conta com o pulo de parede, e
            // o pogo nos espinhos ele tem desde o comeco. Sem o Steam Dash o
            // abismo tem que barrar; com ele (que soma uns 3 tiles a um pulo) e
            // com o vapor da saida, o mapa todo abre.
            var noDash = new LevelMap.JumpRules { WallJump = true, Pogo = true };
            var rules = new LevelMap.JumpRules { WallJump = true, Pogo = true, MaxRun = 8 };

            LevelMap.Reach beforeBoss = Map.Explore(SpawnTile, noDash);
            // Depois do boss: o vapor pro District 2 e o da volta da arena.
            LevelMap.Lift[] purifiedLifts = { District2Vent, ArenaReturnVent };
            LevelMap.Reach reach = Map.Explore(SpawnTile, rules, purifiedLifts);

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
            MustReach((PreBossWorkbenchTile.X + 1, PreBossWorkbenchTile.Y), "A bancada da Antessala");

            // A da Antessala existe pra quem morre pro boss: tem que dar pra
            // chegar nela antes de ganhar o Steam Dash.
            if (!beforeBoss.CanStand(PreBossWorkbenchTile.X + 1, PreBossWorkbenchTile.Y))
            {
                problems.Add("A bancada da Antessala so e alcancada com o Steam Dash, depois do boss.");
            }
            MustReach(EntryGateTile, "A entrada da arena");

            foreach (NpcSpot npc in Npcs)
            {
                MustReach((npc.X, npc.Y), $"O personagem {npc.Name}");
            }

            foreach (LoreSpot lore in Lore)
            {
                MustReach((lore.X, lore.Y), $"O texto {lore.Name}");
            }

            foreach (TutorialSpot sign in Tutorials)
            {
                MustReach((sign.X, sign.Y), $"A placa {sign.Name}");
            }

            // A placa do pulo de parede tem que estar embaixo da chamine que ela ensina.
            foreach (TutorialSpot sign in Tutorials)
            {
                if (sign.Kind != TutorialKind.WallJump) continue;
                if (sign.AreaX1 < Chimney.X || sign.AreaX0 > Chimney.X + Chimney.Width - 1)
                {
                    problems.Add($"A area de {sign.Name} nao fica embaixo da chamine.");
                }
            }

            // E a do golpe pra baixo, na beirada dos espinhos da obra.
            foreach (TutorialSpot sign in Tutorials)
            {
                if (sign.Kind != TutorialKind.Pogo) continue;
                if (!IsSpike(sign.X + 1, sign.Y - 1)) problems.Add($"A placa {sign.Name} nao fica na beirada dos espinhos.");
            }

            // Tudo o que se le ou com quem se conversa: o HUD so mostra um
            // aviso por vez, entao as areas nao podem se encostar. E conversar
            // trava o Scrap, entao nada disso pode ficar no caminho de uma patrulha.
            var talkSpots = new List<(string Name, int X, int Y)>();
            foreach (NpcSpot npc in Npcs) talkSpots.Add((npc.Name, npc.X, npc.Y));
            foreach (LoreSpot lore in Lore) talkSpots.Add((lore.Name, lore.X, lore.Y));

            for (int i = 0; i < talkSpots.Count; i++)
            {
                for (int j = i + 1; j < talkSpots.Count; j++)
                {
                    if (Math.Abs(talkSpots[i].X - talkSpots[j].X) < 3 && Math.Abs(talkSpots[i].Y - talkSpots[j].Y) < 3)
                    {
                        problems.Add($"{talkSpots[i].Name} e {talkSpots[j].Name} estao perto demais: os avisos se sobrepoem.");
                    }
                }

                foreach (Patrol crawler in Crawlers)
                {
                    if (talkSpots[i].Y == crawler.Y && talkSpots[i].X >= crawler.X0 - 2 && talkSpots[i].X <= crawler.X1 + 2)
                    {
                        problems.Add($"{talkSpots[i].Name} fica na patrulha de {crawler.Name}.");
                    }
                }
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

            foreach (ItemSpot item in Items)
            {
                MustReach((item.X, item.Y), $"O item {item.Id}");
            }

            // A obra abandonada: com a nevoa no lugar, nao se entra.
            (int X, int Y, int Width, int Height) veil = TrialVeil;
            Map.Fill(veil.X, veil.Y, veil.X + veil.Width - 1, veil.Y + veil.Height - 1, LevelMap.Rock);
            LevelMap.Reach veiled = Map.Explore(SpawnTile, rules);
            Map.Fill(veil.X, veil.Y, veil.X + veil.Width - 1, veil.Y + veil.Height - 1, LevelMap.Air);

            if (veiled.CanStand(TrialInsideTile.X, TrialInsideTile.Y))
            {
                problems.Add("Da pra entrar na obra abandonada com a nevoa no lugar: ela nao esta fechando nada.");
            }

            // A primeira parte da obra e de pogo: sem ele, nao se passa.
            var noPogo = new LevelMap.JumpRules { WallJump = true, MaxRun = 8 };
            LevelMap.Reach withoutPogo = Map.Explore(SpawnTile, noPogo, purifiedLifts);

            foreach (ItemSpot item in Items)
            {
                bool inTrial = !veiled.CanStand(item.X, item.Y);

                // O que fica fora da obra e nao depende do boss tem que dar
                // pra pegar antes dele, sem o Steam Dash.
                if (!inTrial && !item.AfterBoss && !beforeBoss.CanStand(item.X, item.Y))
                {
                    problems.Add($"O item {item.Id} so e alcancado com o Steam Dash, e ele nao depende do boss.");
                }

                // As ferramentas vem depois do pedido: atras do portao do
                // Rebite. So a da obra fica antes dele no mapa.
                if (item.Kind == ItemKind.MasterTool && !inTrial && locked.CanStand(item.X, item.Y))
                {
                    problems.Add($"A ferramenta {item.Id} da pra pegar antes de passar pelo Rebite.");
                }

                // O premio da obra e de quem ja tem o motor, e sabe quicar.
                if (item.Kind == ItemKind.Chassis && inTrial && beforeBoss.CanStand(item.X, item.Y))
                {
                    problems.Add($"Da pra chegar no premio da obra ({item.Id}) sem o Steam Dash.");
                }

                if (inTrial && withoutPogo.CanStand(item.X, item.Y))
                {
                    problems.Add($"Da pra chegar em {item.Id}, na obra, sem o pogo: os espinhos nao estao cobrando ele.");
                }

                // E de qualquer lugar da pra voltar pro poco.
                LevelMap.Reach back = Map.Explore((item.X, item.Y), rules, purifiedLifts);
                if (!back.CanStand(Npcs[0].X, Npcs[0].Y))
                {
                    problems.Add($"Quem pega o item {item.Id} nao consegue voltar pro poco.");
                }
            }

            // Depois do boss da pra voltar da arena ate o poco e ate o Ferrolho,
            // mesmo antes de ele montar o motor.
            LevelMap.Reach fromArena = Map.Explore(BossSpawnTile, noDash, purifiedLifts);
            foreach (NpcSpot npc in Npcs)
            {
                if (!fromArena.CanStand(npc.X, npc.Y))
                {
                    problems.Add($"Da arena, sem o Steam Dash, nao se volta ate {npc.Name}.");
                }
            }

            // Mas durante a luta o selo fecha o poco: nao se volta pra bancada.
            (int X, int Y, int Width) seal = ArenaSeal;
            Map.Fill(seal.X, seal.Y, seal.X + seal.Width - 1, seal.Y, LevelMap.Rock);
            LevelMap.Reach duringFight = Map.Explore(BossSpawnTile, rules);
            Map.Fill(seal.X, seal.Y, seal.X + seal.Width - 1, seal.Y, LevelMap.Air);

            if (duringFight.CanStand(PreBossWorkbenchTile.X + 1, PreBossWorkbenchTile.Y))
            {
                problems.Add("Com o selo fechado ainda da pra sair da arena pelo poco.");
            }

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

            // Nada de patamar na altura da cabeca dele fora de onde o corpo dele
            // passa: dali se bate no Coletor sem que ele alcance. Dentro do
            // caminho dele pode (os degraus): quando ele passa por baixo, encosta.
            // Mais alto (as plataformas do meio) pode, que o salto com pancada
            // dele chega la.
            (int X, int Y, int Width, int Height) arenaArea = ArenaBounds;
            const float bossHalfWidth = 1.5f;
            for (int y = arenaArea.Y + 1; y <= arenaArea.Y + 3; y++)
            {
                for (int x = arenaArea.X; x < arenaArea.X + arenaArea.Width; x++)
                {
                    bool underBoss = x + 1f > BossRange.Min - bossHalfWidth && x < BossRange.Max + bossHalfWidth;
                    if (IsStandable(x, y) && !underBoss)
                    {
                        problems.Add($"Ha um patamar na altura da cabeca do Coletor, fora do alcance dele, em ({x}, {y}).");
                    }
                }
            }

            // E no meio da luta (o selo fechado, sem o vapor da volta) da pra
            // subir nas plataformas de cima: e por elas que se passa por cima dele.
            Map.Fill(seal.X, seal.Y, seal.X + seal.Width - 1, seal.Y, LevelMap.Rock);
            LevelMap.Reach inFight = Map.Explore(BossSpawnTile, noDash);
            Map.Fill(seal.X, seal.Y, seal.X + seal.Width - 1, seal.Y, LevelMap.Air);

            foreach (LevelMap.Perch perch in perches)
            {
                bool inArena = perch.Y > arenaArea.Y + 3 && perch.Y < arenaArea.Y + arenaArea.Height
                    && perch.X >= arenaArea.X && perch.Right < arenaArea.X + arenaArea.Width;
                if (!inArena) continue;

                bool reached = false;
                for (int x = perch.X; x <= perch.Right && !reached; x++) reached = inFight.CanStand(x, perch.Y);

                if (!reached) problems.Add($"A plataforma da arena em {perch} nao se alcanca do chao durante a luta.");
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

            // Os geodos so dentro da rocha, com um tile de folga em volta.
            foreach (var (_, gx, gy) in Geodes)
            {
                bool buried = true;
                for (int y = gy - 1; y <= gy + 2 && buried; y++)
                {
                    for (int x = gx - 1; x <= gx + 2 && buried; x++)
                    {
                        buried = IsRock(x, y);
                    }
                }

                if (!buried)
                {
                    problems.Add($"O geodo em ({gx}, {gy}) nao esta inteiro dentro da rocha: parece uma pedra solta.");
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
