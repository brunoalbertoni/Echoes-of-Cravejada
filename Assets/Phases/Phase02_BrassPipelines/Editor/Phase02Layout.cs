using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O desenho da Phase 2 — District 2: The Brass Pipelines.
    ///
    /// O sistema circulatorio de Cravejada, organizado em volta de um centro.
    ///
    /// A ENTRADA (linear e curta): 1 Galeria de Transicao, 2 Primeiros Canos,
    /// 3 Valvula Emperrada. Cada corrompido novo aparece uma vez, sozinho, e a
    /// primeira pagina do livro de turnos fica no caminho.
    ///
    /// O CENTRO: 5 a Praca (o Salao dos Welders) e, embaixo dela, 4 a
    /// Cisterna, com Crisol, a loja, a bancada e o cofre do velho encanador,
    /// que pede as paginas. Da Praca sobe a Escadaria das Tres Travas, que
    /// leva ao boss e fica fechada ate as tres alas acabarem.
    ///
    /// AS TRES ALAS, em qualquer ordem. Cada uma conta um pedaco da historia,
    /// guarda uma pagina do livro, termina numa valvula que abre uma trava e
    /// tem um atalho de mao unica de volta pra Praca:
    ///   Alta  (B Casa de Maquinas, C Sotao)     a culpa da Coroa
    ///   Baixa (A Esgoto, D Deposito, E Torre)   os operarios e as mascaras
    ///   Leste (6 Cadeia de Vapor, 7 Labirinto)  quem ficou pra tras
    ///
    /// O BOSS E DEPOIS: 8 pre-boss, 9 Sala de Caldeiras. A queda do Boiler
    /// Master purifica o distrito e abre a grade leste: um tunel curto e a
    /// coluna pra F Galeria Alta (o desafio do Salto de Pressao). A direita
    /// dela, G a subida; por cima dela, a passarela ate H a estacao do
    /// elevador, que fica em cima do poco da Escadaria — cuja escada, la em
    /// cima, desabou. O elevador e a saida pro District 3: a fase inteira e
    /// uma subida.
    ///
    /// A dificuldade e a mesma do comeco ao fim: calibrada pro Scrap que sai
    /// do District 1 com o Steam Dash, e que pode comprar na loja de Crisol.
    /// O mapa conta com o dash: ha vaos que so se cruzam com ele, e alguns so
    /// emendando dash em nuvem de vapor.
    ///
    /// Gemas sao raras: a fase tem exatamente duas (o nicho secreto da Cadeia
    /// de Vapor e a do encanador), e a validacao cobra.
    ///
    /// Diferente da Phase 1, este mapa nao e um desenho em texto: com 290x113
    /// tiles ele ficaria impossivel de ler. Cada sala e um metodo que escava a
    /// rocha e, no mesmo lugar, declara o que mora nela — vapor, acido,
    /// valvulas, inimigos, canos, luzes. Mexer na sala e mexer no metodo dela.
    ///
    /// Este arquivo nao depende de UnityEngine de proposito: assim da pra rodar
    /// Validate() fora do editor e conferir que o mapa e navegavel de verdade.
    /// </summary>
    public static class Phase02Layout
    {
        public const int Width = 216;
        public const int Height = 140;

        // Nomes usados pra ligar a saida de uma cena a chegada na outra.
        public const string SceneName = "Phase02_BrassPipelines";
        public const string FromDistrict1SpawnId = "FromDistrict1";

        /// <summary>
        /// As tres alas que se abrem da Praca, e as salas de cada uma. O
        /// jogador faz na ordem que quiser; cada uma acaba numa valvula que
        /// abre uma trava da Escadaria. A validacao confere que nenhuma ala
        /// depende de outra e que o boss pede as tres.
        /// </summary>
        public static readonly (string Wing, string[] Rooms)[] Wings =
        {
            ("Alta", new[] { "B_MachineHall", "C_GaugeAttic" }),
            ("Baixa", new[] { "A_Sewer", "D_Depot", "E_ValveTower" }),
            ("Leste", new[] { "06_SteamChain", "07_ValveMaze" }),
        };

        /// <summary>A sala que contem a celula, ou null.</summary>
        public static string RoomAt(int x, int y)
        {
            foreach ((string name, Area area) in Rooms)
            {
                if (area.Contains(x, y)) return name;
            }

            return null;
        }

        /// <summary>A ala de uma sala, ou null se ela e da entrada, do centro ou do boss.</summary>
        public static string WingOf(string room)
        {
            foreach ((string wing, string[] rooms) in Wings)
            {
                if (Array.IndexOf(rooms, room) >= 0) return wing;
            }

            return null;
        }

        // ------------------------------------------------------------------
        // Tipos
        // ------------------------------------------------------------------

        /// <summary>Um retangulo de celulas. X e Y sao o canto de baixo, a esquerda.</summary>
        public readonly struct Area
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Width;
            public readonly int Height;

            public Area(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            /// <summary>Monta a partir dos dois cantos, os dois incluidos.</summary>
            public static Area Between(int x0, int y0, int x1, int y1)
            {
                return new Area(Math.Min(x0, x1), Math.Min(y0, y1), Math.Abs(x1 - x0) + 1, Math.Abs(y1 - y0) + 1);
            }

            public int Right => X + Width - 1;
            public int Top => Y + Height - 1;
            public float CenterX => X + Width * 0.5f;
            public float CenterY => Y + Height * 0.5f;

            public bool Contains(int x, int y) => x >= X && x <= Right && y >= Y && y <= Top;
        }

        /// <summary>De que e feita a parede de um trecho do mapa.</summary>
        public enum Material
        {
            /// <summary>Chapa de latao rebitada: e o distrito.</summary>
            Brass,

            /// <summary>Rocha cor de ferrugem, a do Scrap Pit. So na entrada.</summary>
            Rock,

            /// <summary>Rocha azulada com cristal, a dos Silent Geodes. So na saida.</summary>
            Crystal,
        }

        public enum VentKind
        {
            /// <summary>Coluna em pe: levanta o Scrap.</summary>
            Lift,

            /// <summary>Jato deitado: carrega o Scrap numa direcao.</summary>
            Jet,
        }

        public sealed class Vent
        {
            public string Name;
            public VentKind Kind;

            /// <summary>As celulas onde o vapor age.</summary>
            public Area Zone;

            /// <summary>So pro jato: +1 empurra pra leste, -1 pra oeste.</summary>
            public int Direction = 1;

            /// <summary>Segundos ligado e desligado. Com OffTime zero, fica sempre ligado.</summary>
            public float OnTime;
            public float OffTime;

            /// <summary>Se comeca desligado, esperando uma valvula.</summary>
            public bool StartsOff;

            public bool Pulses => OffTime > 0f;
        }

        /// <summary>
        /// Uma valvula: onde fica o volante, onde o Scrap para pra raspar, e o
        /// que muda quando ela gira. As valvulas so abrem coisas, nunca fecham
        /// — entao girar uma nunca prende ninguem.
        /// </summary>
        public sealed class ValveSpec
        {
            public string Group;
            public string Name;

            /// <summary>Canto de baixo, a esquerda, do volante (2x2 tiles).</summary>
            public int X;
            public int Y;

            public int StandX;
            public int StandY;

            /// <summary>Os nomes do que ela liga, seca e abre.</summary>
            public string[] Vents = new string[0];
            public string[] Pools = new string[0];
            public string[] Gates = new string[0];

            /// <summary>Se e uma das tres da arena, que atordoam o Boiler Master.</summary>
            public bool Arena;

            /// <summary>
            /// Se e a valvula do fim de uma ala: abre uma das tres travas da
            /// Escadaria, e o jogo avisa no HUD.
            /// </summary>
            public bool Lock;

            /// <summary>
            /// O que ela muda longe da vista, dito no HUD quando gira. Toda
            /// valvula que liga vapor em outra sala precisa de um.
            /// </summary>
            public string Notice;
        }

        public enum GateOpener
        {
            /// <summary>Uma valvula (ver ValveSpec.Gates).</summary>
            Valve,

            /// <summary>A queda do Boiler Master.</summary>
            Boss,

            /// <summary>A purificacao do distrito: sao os atalhos de depois.</summary>
            Purification,
        }

        /// <summary>Um portao de grade: as celulas que ele fecha e pra onde ele corre ao abrir.</summary>
        public sealed class GateSpec
        {
            public string Name;
            public Area Area;
            public int MoveX;
            public int MoveY = 1;
            public GateOpener Opener;
        }

        public enum EnemyKind
        {
            PipeLeech,
            ValveSentry,
            BrassWelder,
            AcidBloater,
        }

        /// <summary>
        /// Um inimigo. X0..X1 e o trecho de chao por onde ele anda e Y a linha
        /// dos pes. A sentinela fica parada em X0 e atira pra Direction; a
        /// sanguessuga fica pendurada em (X0, CeilingY) e cai ate Y.
        /// </summary>
        public sealed class EnemySpec
        {
            public EnemyKind Kind;
            public string Name;
            public int X0;
            public int X1;
            public int Y;
            public int Direction = 1;
            public int Range = 14;
            public int CeilingY;

            /// <summary>
            /// A celula onde ele comeca, pra quem anda. -1 e o meio do trecho.
            /// Quando dois dividem o mesmo chao, cada um comeca num ponto.
            /// </summary>
            public int StartX = -1;
        }

        /// <summary>
        /// Uma caldeira corrompida que explode com tres golpes, e as paredes
        /// rachadas que a explosao derruba. X e Y sao o canto de baixo (4x6 tiles).
        /// </summary>
        public sealed class BoilerSpec
        {
            /// <summary>A sala, pra saber a que ala ela pertence.</summary>
            public string Group;

            public string Name;
            public int X;
            public int Y;
            public int StandX;
            public int StandY;
            public string[] Walls = new string[0];
        }

        /// <summary>A cor de uma luz ou de uma nevoa. O gerador traduz pra cor de verdade.</summary>
        public enum Glow
        {
            /// <summary>Laranja de fornalha.</summary>
            Furnace,

            /// <summary>Verde-amarelado do acido.</summary>
            Acid,

            /// <summary>Branco quente do vapor.</summary>
            Steam,

            /// <summary>Amarelo de lampiao: so nas salas seguras.</summary>
            Lamp,

            /// <summary>Azul frio dos cristais.</summary>
            Crystal,

            /// <summary>Verde-cobre do Oxide, o mesmo do Scrap Pit.</summary>
            Oxide,
        }

        public readonly struct Light
        {
            public readonly float X;
            public readonly float Y;
            public readonly float Radius;
            public readonly float Intensity;
            public readonly Glow Glow;

            /// <summary>Se a luz pulsa, como um coracao. E o que da vida as caldeiras.</summary>
            public readonly bool Pulses;

            public Light(float x, float y, float radius, float intensity, Glow glow, bool pulses)
            {
                X = x;
                Y = y;
                Radius = radius;
                Intensity = intensity;
                Glow = glow;
                Pulses = pulses;
            }
        }

        /// <summary>Uma faixa deitada de nevoa: vapor, fumaca de acido ou Oxide.</summary>
        public readonly struct Haze
        {
            public readonly string Name;
            public readonly float X;
            public readonly float Y;
            public readonly float Width;
            public readonly float Thickness;
            public readonly float Alpha;
            public readonly float Rate;
            public readonly bool InFront;
            public readonly Glow Glow;

            public Haze(string name, float x, float y, float width, float thickness,
                float alpha, float rate, bool inFront, Glow glow)
            {
                Name = name;
                X = x;
                Y = y;
                Width = width;
                Thickness = thickness;
                Alpha = alpha;
                Rate = rate;
                InFront = inFront;
                Glow = glow;
            }
        }

        public enum FixtureKind
        {
            /// <summary>Caldeira, 4x6 tiles, com fornalha que pulsa.</summary>
            Boiler,

            /// <summary>Manometro grande, 2x2, com ponteiro tremendo.</summary>
            GaugeBig,

            /// <summary>Manometro pequeno, 1x1.</summary>
            GaugeSmall,

            /// <summary>Volante de valvula enferrujado, 2x2. E o que se raspa e gira.</summary>
            Valve,

            /// <summary>Fornalha de parede, 2x2, com fogo animado dentro.</summary>
            Furnace,

            /// <summary>Escotilha, 2x3.</summary>
            Hatch,

            /// <summary>Goldsmith Workbench, 3x2.</summary>
            Workbench,

            /// <summary>Bocal de vapor virado pra cima. Size e a largura em tiles.</summary>
            NozzleUp,

            /// <summary>Bocal de vapor virado de lado. Size e a altura em tiles.</summary>
            NozzleSide,

            /// <summary>Lampiao pendurado, 1x2. X e Y sao o canto de baixo; o topo encosta no teto.</summary>
            Lamp,

            /// <summary>Armario de operario, 1x2. Flipped: aberto e vazio.</summary>
            Locker,

            /// <summary>O cabide das mascaras, 2x1.5: uma mascara so, e as marcas das outras.</summary>
            MaskRack,

            /// <summary>Prateleira vazia do deposito, 2x2.</summary>
            Shelf,

            /// <summary>Tambor de ferro, 1x1.5.</summary>
            Barrel,

            /// <summary>Guarda-corpo de passarela. Size e a largura em tiles.</summary>
            Railing,

            /// <summary>Cartaz da Coroa, 2x2.</summary>
            Poster,

            /// <summary>Placa de sala, 3x1.</summary>
            Sign,

            /// <summary>Corrente pendurada. Size e a altura em tiles.</summary>
            Chain,

            /// <summary>Os trilhos do poco do elevador, 3 de largura. Size e a altura em tiles.</summary>
            ElevatorRail,
        }

        /// <summary>Um objeto de cenario que nao cabe num tile. X e Y sao o canto de baixo, a esquerda.</summary>
        public readonly struct Fixture
        {
            public readonly FixtureKind Kind;
            public readonly string Name;
            public readonly int X;
            public readonly int Y;
            public readonly int Size;

            /// <summary>So pro bocal de lado: se sopra pra oeste.</summary>
            public readonly bool Flipped;

            public Fixture(FixtureKind kind, string name, int x, int y, int size = 0, bool flipped = false)
            {
                Kind = kind;
                Name = name;
                X = x;
                Y = y;
                Size = size;
                Flipped = flipped;
            }
        }

        public enum PropLayer
        {
            /// <summary>Encostado na parede do fundo, esmaecido.</summary>
            Back,

            /// <summary>Na frente do terreno, atras do Scrap.</summary>
            Middle,

            /// <summary>Na frente do Scrap, quase em silhueta.</summary>
            Front,

            /// <summary>Sem luz: fogo e tudo o que brilha sozinho.</summary>
            Glow,
        }

        /// <summary>Um sprite de 1 tile carimbado numa celula.</summary>
        public readonly struct Prop
        {
            public readonly string Tile;
            public readonly int X;
            public readonly int Y;
            public readonly PropLayer Layer;

            public Prop(string tile, int x, int y, PropLayer layer)
            {
                Tile = tile;
                X = x;
                Y = y;
                Layer = layer;
            }
        }

        public enum PipeLayer
        {
            /// <summary>Tubulacao colossal la no fundo, desenhada em dobro. As celulas valem 2x2 tiles.</summary>
            Far,

            /// <summary>Canos na parede do fundo, escurecidos.</summary>
            Back,

            /// <summary>Canos na frente da parede, com a cor cheia.</summary>
            Mid,
        }

        /// <summary>Um ponto vazio com nome, pra ligar uma mecanica depois.</summary>
        public readonly struct Marker
        {
            /// <summary>A sala, pra agrupar na hierarquia da cena.</summary>
            public readonly string Group;
            public readonly string Name;
            public readonly int X;
            public readonly int Y;

            /// <summary>Se o Scrap precisa conseguir chegar e ficar em pe aqui.</summary>
            public readonly bool MustReach;

            public Marker(string group, string name, int x, int y, bool mustReach)
            {
                Group = group;
                Name = name;
                X = x;
                Y = y;
                MustReach = mustReach;
            }
        }

        /// <summary>Uma passagem pra outra cena.</summary>
        public readonly struct Exit
        {
            public readonly string Name;
            public readonly Area Area;
            public readonly string Scene;
            public readonly string SpawnId;

            public Exit(string name, Area area, string scene, string spawnId)
            {
                Name = name;
                Area = area;
                Scene = scene;
                SpawnId = spawnId;
            }
        }

        /// <summary>Uma goteira: de onde pinga e quantos tiles cai.</summary>
        public readonly struct Drip
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Fall;
            public readonly bool Acid;

            public Drip(int x, int y, int fall, bool acid)
            {
                X = x;
                Y = y;
                Fall = fall;
                Acid = acid;
            }
        }

        /// <summary>Um fio de vapor escapando de uma junta. So enfeite.</summary>
        public readonly struct Leak
        {
            public readonly float X;
            public readonly float Y;
            public readonly float DirectionX;
            public readonly float DirectionY;

            public Leak(float x, float y, float directionX, float directionY)
            {
                X = x;
                Y = y;
                DirectionX = directionX;
                DirectionY = directionY;
            }
        }

        /// <summary>Uma faixa do horizonte de caldeiras, la no fundo dos saloes abertos.</summary>
        public readonly struct Skyline
        {
            public readonly string Name;

            /// <summary>Altura da base da faixa.</summary>
            public readonly float Y;

            public readonly bool Near;
            public readonly float Scale;

            /// <summary>0 = infinitamente longe, 1 = no plano do jogo.</summary>
            public readonly float Parallax;

            public Skyline(string name, float y, bool near, float scale, float parallax)
            {
                Name = name;
                Y = y;
                Near = near;
                Scale = scale;
                Parallax = parallax;
            }
        }

        // ------------------------------------------------------------------
        // O que o gerador le
        // ------------------------------------------------------------------

        private static readonly LevelMap Map = new LevelMap(Width, Height);

        public static readonly List<(string Name, Area Area)> Rooms = new List<(string, Area)>();
        public static readonly List<Vent> Vents = new List<Vent>();
        public static readonly List<(string Name, Area Area)> AcidPools = new List<(string, Area)>();
        public static readonly List<Prop> Props = new List<Prop>();
        public static readonly List<Fixture> Fixtures = new List<Fixture>();
        public static readonly List<Light> Lights = new List<Light>();
        public static readonly List<Haze> Hazes = new List<Haze>();
        public static readonly List<Drip> Drips = new List<Drip>();
        public static readonly List<Leak> Leaks = new List<Leak>();
        public static readonly List<Marker> Markers = new List<Marker>();
        public static readonly List<Exit> Exits = new List<Exit>();
        public static readonly List<Skyline> Skylines = new List<Skyline>();
        public static readonly List<ValveSpec> Valves = new List<ValveSpec>();
        public static readonly List<GateSpec> Gates = new List<GateSpec>();
        public static readonly List<EnemySpec> Enemies = new List<EnemySpec>();
        public static readonly List<BoilerSpec> PressureBoilers = new List<BoilerSpec>();

        /// <summary>Paredes rachadas: fecham uma passagem ate uma caldeira explodir do lado.</summary>
        public static readonly List<(string Name, Area Area)> BreakableWalls = new List<(string, Area)>();

        /// <summary>Nuvens de vapor paradas, que so devolvem o dash.</summary>
        public static readonly List<(string Name, Area Area)> Clouds = new List<(string, Area)>();

        /// <summary>As Goldsmith Workbenches: canto de baixo, a esquerda (3x2 tiles).</summary>
        public static readonly List<(string Name, int X, int Y)> Workbenches = new List<(string, int, int)>();

        public enum LoreKind
        {
            /// <summary>Uma pagina solta do livro de turnos, caida no chao.</summary>
            Page,

            /// <summary>Uma pichacao de operario, em letra.</summary>
            Graffiti,

            /// <summary>Uma pichacao de operario, em desenho: a mascara de soldador.</summary>
            GraffitiMask,

            /// <summary>Uma inscricao gravada em latao.</summary>
            Plaque,

            /// <summary>Um Tarnished com quem se conversa.</summary>
            Tarnished,
        }

        /// <summary>
        /// Um pedaco da historia do distrito, pra achar e ler: chegar perto e
        /// apertar E. X e Y sao a celula onde o Scrap para.
        /// </summary>
        public sealed class LoreSpec
        {
            public LoreKind Kind;
            public string Name;
            public string Title;
            public int X;
            public int Y;
            public string[] Lines;

            /// <summary>O que se le (ou ouve) depois da purificacao, se mudou. Nulo repete Lines.</summary>
            public string[] PurifiedLines;

            /// <summary>Se ler leva a coisa embora: o id dela no KeyItems e o aviso. Nulo: so se le.</summary>
            public string CollectId;
            public string CollectMessage;

            /// <summary>So aparece depois da purificacao.</summary>
            public bool AfterPurification;

            /// <summary>Se ele pede coisas espalhadas pelo distrito e paga com gema.</summary>
            public QuestSpec Quest;
        }

        /// <summary>Um pedido: o que ele quer (KeyItems com esse comeco de id), quanto paga e o que diz.</summary>
        public sealed class QuestSpec
        {
            public string ItemPrefix;
            public int ItemCount;
            public int GemReward = 1;
            public string PurifiedGreeting;

            /// <summary>Depois do pedido, por quantas ele ja tem: 0, 1, 2...</summary>
            public string[][] Progress;

            public string[] Complete;
            public string[] Done;
        }

        /// <summary>
        /// A historia do District 2, espalhada pelo mapa: o livro de turnos da
        /// caldeira em quatro paginas soltas (que o velho encanador quer de
        /// volta), as pichacoes dos operarios, a placa do cano-mestre, o velho
        /// encanador escondido e o que se acha nos esconderijos: a carta, o
        /// relatorio, a ficha do deposito, o diario do chefe de manutencao.
        /// </summary>
        public static readonly List<LoreSpec> Lore = new List<LoreSpec>();

        /// <summary>As paginas do livro de turnos: o comeco do id delas e quantas sao.</summary>
        public const string LogbookPrefix = "Logbook_";
        public const int LogbookCount = 4;

        public enum RewardKind
        {
            /// <summary>Uma gema lapidada.</summary>
            Gem,

            /// <summary>Um cofre de fragmentos: Amount de uma vez.</summary>
            ShardCache,

            /// <summary>Um reforco de chassi: +1 de vida.</summary>
            Chassis,
        }

        /// <summary>Uma recompensa escondida, pra quem explora. X e Y sao a celula onde o Scrap para.</summary>
        public sealed class RewardSpec
        {
            public RewardKind Kind;
            public string Id;
            public int X;
            public int Y;
            public int Amount;
        }

        /// <summary>O que se acha nos esconderijos: gemas, cofres de fragmentos e reforcos de chassi.</summary>
        public static readonly List<RewardSpec> Rewards = new List<RewardSpec>();

        /// <summary>
        /// Nevoa densa de Oxide: fecha uma passagem ate a purificacao, como a
        /// da Pista de Provas do District 1. No mapa e ar; quem fecha e o objeto.
        /// </summary>
        public static readonly List<(string Name, Area Area)> Veils = new List<(string, Area)>();

        /// <summary>Onde Crisol fica.</summary>
        public static (int X, int Y) CrisolTile => (90, 25);

        public static readonly string[] CrisolLines =
        {
            "Hum. Mais um pedaço de sucata que desceu pelos canos errados.",
            "Eu sou Crisol, o Lapidário. Já cravei pedra em coroa. Hoje conserto lata para quem não sabe o que é brilho.",
            "Esse aro de prata que você carrega... o Coletor deixou cair, não foi? Dê-me isso aqui. Você nem sabe o que é isso.",
            "É um engaste. Serve para segurar uma gema no chassi. Tome: um rubi bruto. Vamos ver se essa sucata aguenta uma pedra de verdade.",
            "Leve-o até uma bancada e crave-o. O cinzel vai sair em brasa. Se você derreter, eu fico com o rubi de volta.",
            "E, se quiser que eu reforce essa carcaça, traga fragmentos. Eu não trabalho de graça para Scrap nenhum.",
            "Gemas lapidadas também servem. Essas, sim, eu sei apreciar. Por uma delas, até conto uma história.",
        };

        /// <summary>O que ele diz das outras vezes, antes de abrir a loja.</summary>
        public static readonly string[] CrisolRepeatLines =
        {
            "Você de novo. Diga logo o que quer.",
        };

        /// <summary>O que ele diz depois da purificacao.</summary>
        public static readonly string[] CrisolPurifiedLines =
        {
            "Sentiu? O vapor está limpo. Fazia anos que esta cisterna só cheirava a verde.",
            "Não pense que isso me comove. ...Mas a bancada está trabalhando melhor. Diga o que quer.",
        };

        /// <summary>A memoria que ele vende por uma gema: como lapidou a coroa do Rei de Prata.</summary>
        public static readonly string[] CrisolMemoryLines =
        {
            "Quer saber da Coroa de Facetas? Então ouça e não interrompa.",
            "O Rei de Prata me deu um bloco de quartzo do tamanho da minha cabeça e disse: novecentas e vinte e cinco facetas. Uma para cada Nobre.",
            "Levei três anos. Cada faceta tinha que refletir a luz de todas as outras. Se uma ficasse torta, a coroa inteira ficava cega.",
            "No dia da coroação, a luz de Cravejada inteira passou por ela. Os geodos acenderam de ponta a ponta. Eu vi do fundo do salão, de pé, atrás dos Nobres.",
            "Ele nem olhou para mim. Olhou para o próprio reflexo, multiplicado novecentas e vinte e cinco vezes.",
            "...Foi a coisa mais bonita que eu já fiz. E foi feita para alguém que só sabia olhar para si mesmo.",
        };

        /// <summary>A area de onde o acido do poco de fuga parte, e ate que altura ele sobe.</summary>

        /// <summary>Onde o Boiler Master fica, ate onde ele anda e a area que o acorda.</summary>
        public static (int X, int Y) BossTile => (149, 59);

        /// <summary>
        /// Ate onde ele anda: a sala inteira, de parede a parede. Nao sobra
        /// canto onde se fique na altura da cabeca dele sem que ele alcance.
        /// </summary>
        public static (int X0, int X1) BossRange => (135, 176);

        /// <summary>Metade da largura do corpo dele (o colisor tem 2.8).</summary>
        public const float BossHalfWidth = 1.4f;

        public static Area BossWakeArea => Area.Between(136, 59, 174, 69);
        public const string BossGate = "Gate_ArenaExit";

        /// <summary>
        /// A passagem da sala pre-boss pra arena: a coluna que o selo fecha
        /// enquanto ele luta. Com a grade da saida, a arena fica trancada.
        /// </summary>
        public static Area ArenaSeal => Area.Between(131, 59, 131, 63);

        /// <summary>A arena por dentro: do selo da entrada (x 131) a grade da saida (x 180).</summary>
        public static Area ArenaInside => Area.Between(132, 59, 179, 74);

        /// <summary>Onde nao ha parede de fundo: os saloes abertos, por onde se ve o horizonte.</summary>
        public static readonly List<Area> Voids = new List<Area>();

        private static readonly List<(Area Area, Material Material)> MaterialZones = new List<(Area, Material)>();

        /// <summary>Onde o Scrap aparece quando chega do District 1.</summary>
        public static (int X, int Y) SpawnTile => (12, 6);

        /// <summary>O fim do mapa: o toco que leva aos Silent Geodes.</summary>
        public static (int X, int Y) District3Tile => (121, 100);

        /// <summary>
        /// O elevador da estacao, em cima do poco da Escadaria: a cabine (3 de
        /// largura, piso na linha Y) e ate que altura ela sobe antes de a cena
        /// trocar. E a unica saida do distrito.
        /// </summary>
        public static Area ElevatorCar => Area.Between(120, 100, 122, 102);
        public const float ElevatorTopY = 133f;

        // Cada camada de cano guarda, por celula, pra que lados ele continua
        // e se e de cobre velho. Guardar a mascara (e nao so "tem cano aqui")
        // e o que impede dois canos vizinhos de se grudarem sem querer.
        private static readonly Dictionary<(int, int), int>[] PipeMasks =
        {
            new Dictionary<(int, int), int>(),
            new Dictionary<(int, int), int>(),
            new Dictionary<(int, int), int>(),
        };

        private static readonly HashSet<(int, int)>[] AgedPipes =
        {
            new HashSet<(int, int)>(),
            new HashSet<(int, int)>(),
            new HashSet<(int, int)>(),
        };

        static Phase02Layout()
        {
            BuildGallery();
            BuildFirstPipes();
            BuildStuckValve();
            BuildCistern();
            BuildWelderHall();
            BuildSteamChain();
            BuildValveMaze();
            BuildWorkbenchRoom();
            BuildBoilerRoom();
            BuildExitTunnel();

            BuildSewer();
            BuildMachineHall();
            BuildGaugeAttic();
            BuildDepot();
            BuildValveTower();
            BuildHighGallery();
            BuildElevatorClimb();
            BuildElevatorStation();
        }

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

        /// <summary>Uma pagina do livro de turnos: se le e se leva, pro velho encanador.</summary>
        private static void Logbook(string group, int number, string title, int x, int y, params string[] lines)
        {
            LoreSpec page = Tell(LoreKind.Page, group, "Lore_Logbook_" + number, title, x, y, lines);
            page.CollectId = LogbookPrefix + number;
            page.CollectMessage = "Página do livro de turnos guardada";
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
            Fix(FixtureKind.GaugeSmall, name + "_Gauge", x + 2, y + 1);
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

        /// <summary>O fogo animado do pack (objects_misc_animated, linha 3).</summary>
        public const string FireTile = "furnace_fire";

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

        // ------------------------------------------------------------------
        // Decoracao dos operarios
        // ------------------------------------------------------------------

        private static int _decor;

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

        // ------------------------------------------------------------------
        // 1. A Galeria de Transicao
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde o Scrap chega do District 1, por um poco gradeado. A metade
        /// oeste ainda e a rocha cor de ferrugem do Scrap Pit; a leste o latao
        /// comeca, com o tablado onde ele ganha o primeiro polimento. Um bloater
        /// sozinho na saida ensina o primeiro corrompido do distrito.
        /// </summary>
        private static void BuildGallery()
        {
            const string room = "01_Gallery";
            Room(room, 3, 6, 37, 13);

            MaterialZones.Add((Area.Between(0, 0, 18, 20), Material.Rock));

            Carve(3, 6, 36, 13);
            Solid(3, 11, 9, 13);
            Solid(16, 13, 21, 13);

            // O poco por onde se chega, ja com a grade fechada por cima.
            Carve(7, 1, 9, 5);
            Grate(7, 9, 5);
            Shine(8.5f, 3f, 6f, 0.8f, Glow.Oxide);
            Leaks.Add(new Leak(8.5f, 5f, 0f, 1f));
            Hazes.Add(new Haze("Oxide_Gallery", 12f, 7f, 16f, 2.5f, 0.14f, 1.1f, false, Glow.Oxide));

            // O tablado do polimento, com dois lampioes: a primeira luz limpa.
            Solid(22, 6, 27, 6);
            Mark(room, "Tarnished_1", 23, 7);
            Mark(room, "Tarnished_2", 26, 7);
            Lamp(23, 13);
            Lamp(26, 13);
            Decor(FixtureKind.Sign, 23, 10);

            // A saida pros Primeiros Canos.
            Carve(37, 6, 37, 9);

            Enemy(EnemyKind.AcidBloater, "Bloater_Gallery", 28, 46, 6, start: 32);

            // Lado da sucata.
            Put(GreyRock(3), 4, 6);
            Put(Debris(1), 5, 6);
            Put(Crate(1), 11, 6);
            Put(Crate(0), 14, 6);
            Put(Crate(2), 14, 7);
            Put(Debris(0), 15, 6);
            Put(Fence(0), 17, 6);
            Put(Fence(1), 18, 6);
            Put(DarkRock(1), 12, 6, PropLayer.Front);
            Put(DarkWeed(2), 19, 6, PropLayer.Front);
            Ore(Phase01Layout.RustBand, 15, 9, PropLayer.Back);
            Ore(Phase01Layout.GreyBand, 1, 8, PropLayer.Middle);

            // Lado do latao.
            Furnace("Furnace_Gallery", 35, 6);
            Fix(FixtureKind.GaugeBig, "Gauge_Gallery", 30, 10);
            Decor(FixtureKind.Barrel, 20, 6);
            Decor(FixtureKind.Barrel, 21, 6);
            HangChain(31, 13, 3);

            RunPipe(PipeLayer.Back, false, (19, 12), (37, 12));
            RunPipe(PipeLayer.Back, true, (28, 12), (28, 7));
            RunPipe(PipeLayer.Mid, false, (33, 13), (33, 10), (37, 10));
            Steam(room, Area.Between(19, 6, 36, 13), 0.07f);
            DripsIn(Area.Between(19, 6, 36, 13), 6, 11, false);
        }

        // ------------------------------------------------------------------
        // 2. Os Primeiros Canos
        // ------------------------------------------------------------------

        /// <summary>
        /// Um poco de tres colunas de vapor, alternando de lado: ensina a subir
        /// no vapor, sem perigo. No alto, uma prateleira leva pra Valvula
        /// Emperrada. A coluna que continua subindo (ate o Sotao) so liga com a
        /// valvula de la: e um atalho de volta, nao um caminho.
        /// </summary>
        private static void BuildFirstPipes()
        {
            const string room = "02_FirstPipes";
            Room(room, 38, 6, 46, 39);

            Carve(38, 6, 46, 39);

            Lift("Lift_FirstPipes_A", 38, 40, 6, 16);
            PipeFloor(43, 46, 14);
            Rail(43, 46, 15);

            Lift("Lift_FirstPipes_B", 44, 46, 15, 26);
            PipeFloor(38, 41, 23);
            Rail(38, 41, 24);

            Lift("Lift_FirstPipes_C", 38, 40, 24, 39);

            // A prateleira de cima, na altura da porta da Valvula Emperrada.
            Solid(42, 33, 46, 35);

            // A coluna do Sotao: continua a C pra cima, desligada.
            Carve(38, 40, 40, 61);
            Lift("Lift_ToAttic", 38, 40, 40, 64, false).StartsOff = true;

            Fix(FixtureKind.GaugeSmall, "Gauge_FirstPipes_A", 45, 9);
            Fix(FixtureKind.GaugeSmall, "Gauge_FirstPipes_B", 39, 19);
            Fix(FixtureKind.GaugeSmall, "Gauge_FirstPipes_C", 45, 28);
            Lamp(44, 39, 0.6f);

            RunPipe(PipeLayer.Mid, false, (36, 8), (38, 8), (38, 5));
            RunPipe(PipeLayer.Mid, false, (48, 17), (46, 17), (46, 14));
            RunPipe(PipeLayer.Mid, false, (36, 25), (38, 25), (38, 23));
            RunPipe(PipeLayer.Back, true, (42, 6), (42, 13));
            RunPipe(PipeLayer.Back, false, (43, 16), (43, 32));
            Steam(room, Area.Between(38, 30, 46, 39), 0.12f);
        }

        // ------------------------------------------------------------------
        // 3. A Valvula Emperrada
        // ------------------------------------------------------------------

        /// <summary>
        /// Uma sala de bombas cortada por uma poca de acido larga demais pro
        /// dash. A valvula fica antes dela, a vista: girar seca a poca. Do
        /// outro lado, uma sentinela atira por cima do buraco, uma sanguessuga
        /// espera no teto, e numa prateleira a primeira pagina do livro de
        /// turnos — o encanador, na Cisterna, vai querer de volta.
        /// </summary>
        private static void BuildStuckValve()
        {
            const string room = "03_StuckValve";
            Room(room, 47, 36, 80, 44);

            Carve(47, 36, 80, 44);
            Solid(47, 43, 52, 44);
            Solid(74, 44, 80, 44);

            // A poca, com um degrau de cada lado pra descer depois de seca.
            Carve(55, 32, 66, 35);
            Solid(55, 32, 56, 33);
            Solid(65, 32, 66, 33);
            Acid("Acid_StuckValve", 57, 32, 64, 33);

            Valve(room, "Valve_StuckValve", 48, 38, 49, 36, "pool:Acid_StuckValve");
            RunPipe(PipeLayer.Mid, true, (49, 38), (49, 37), (58, 37), (58, 35));

            Enemy(EnemyKind.ValveSentry, "Sentry_StuckValve", 69, 69, 36, -1, 9);
            Leech("Leech_StuckValve", 72, 44, 36, 71, 75);

            // A prateleira da pagina, e a porta pra Praca.
            Solid(76, 36, 80, 37);
            Logbook(room, 1, "página do livro de turnos", 79, 38,
                "LIVRO DE TURNOS DA CALDEIRA — turno da noite.",
                "Pressão acima do normal de novo. Abri a válvula de alívio duas vezes, e a pressão voltou a subir.",
                "Avisei lá em cima. Disseram que é o tempo. Aqui embaixo nem existe tempo.");
            Carve(81, 38, 81, 41);

            Fix(FixtureKind.GaugeBig, "Gauge_StuckValve", 52, 40);
            Decor(FixtureKind.Sign, 59, 41);
            Furnace("Furnace_StuckValve", 77, 38);
            Decor(FixtureKind.Barrel, 53, 36);
            Decor(FixtureKind.Barrel, 67, 36);
            Lamp(50, 42);
            Lamp(70, 44);
            Lamp(78, 43, 0.6f);

            Fumes("StuckValve", Area.Between(57, 32, 64, 34));
            RunPipe(PipeLayer.Back, false, (47, 42), (81, 42));
            RunPipe(PipeLayer.Back, true, (61, 42), (61, 37));
            RunPipe(PipeLayer.Back, false, (68, 42), (68, 38), (72, 38));
            RunPipe(PipeLayer.Mid, true, (60, 46), (60, 43), (64, 43), (64, 46));
            Steam(room, Area.Between(47, 36, 80, 44));
            DripsIn(Area.Between(55, 36, 66, 44), 3, 31, true);
        }

        // ------------------------------------------------------------------
        // 4. A Cisterna de Crisol
        // ------------------------------------------------------------------

        /// <summary>
        /// A sala segura, embaixo da Praca: desce-se por uma escada de canos
        /// no chao dela. A bancada, Crisol e o estoque dele, o alcapao da Ala
        /// Baixa, e o cofre do velho encanador atras de uma parede rachada —
        /// a caldeira corrompida do lado ensina que elas derrubam parede.
        /// </summary>
        private static void BuildCistern()
        {
            const string room = "04_Cistern";
            Room(room, 84, 25, 112, 31);

            Carve(84, 25, 112, 31);
            Solid(84, 30, 86, 31);
            Solid(110, 30, 112, 31);

            // A escada de canos que desce da Praca.
            Carve(94, 25, 99, 33);
            PipeFloor(98, 99, 27);
            PipeFloor(94, 95, 29);
            PipeFloor(98, 99, 31);
            Fix(FixtureKind.Hatch, "Hatch_Cistern", 92, 25);

            Bench(room, "Workbench_Cistern", 85, 25);
            Mark(room, "Crisol", CrisolTile.X, CrisolTile.Y);

            // O estoque de Crisol, no canto dele.
            Decor(FixtureKind.Shelf, 87, 28);
            Put(Crate(0), 91, 25);
            Put(Crate(2), 92, 26);

            // O alcapao da Ala Baixa: so desce.
            Carve(102, 15, 103, 24);
            Decor(FixtureKind.Barrel, 100, 25);

            // A caldeira corrompida, a parede rachada e o cofre.
            PressureBoiler(room, "Boiler_PlumberVault", 105, 25, 104, 25, "Wall_PlumberVault");
            Cracked("Wall_PlumberVault", 113, 25, 113, 27);
            Carve(114, 25, 121, 29);
            Put(Crate(3), 120, 25);
            Put(Crate(0), 121, 25);
            Put(Crate(2), 121, 26);
            Put(Debris(1), 115, 25);
            Lamp(117, 29, 0.6f);

            OldPlumber(room, 118, 25);

            Lamp(89, 31, 1f);
            Lamp(108, 31, 0.8f);
            Fix(FixtureKind.GaugeSmall, "Gauge_Cistern", 101, 28);

            RunPipe(PipeLayer.Back, false, (83, 30), (113, 30));
            RunPipe(PipeLayer.Back, true, (101, 30), (101, 27));
            RunPipe(PipeLayer.Back, true, (116, 28), (122, 28));
        }

        /// <summary>
        /// O velho encanador: conta por que sobrou operario aqui embaixo e
        /// pede as quatro paginas do livro de turnos, que e dele. Uma esta na
        /// entrada; as outras tres, uma em cada ala. Paga com uma das duas
        /// gemas do distrito.
        /// </summary>
        private static void OldPlumber(string room, int x, int y)
        {
            Tell(LoreKind.Tarnished, room, "Lore_OldPlumber", "o velho encanador", x, y,
                "...Não bata. Eu ouvi a caldeira estourar. Achei que era o fim, e era só você.",
                "Sou encanador. Era. Estou neste cofre desde que o verde subiu pelos canos.",
                "Sabe por que sobrou operário aqui embaixo? Porque a gente já usava máscara. Para soldar. Ninguém nos deu as máscaras por bondade: era o serviço.",
                "O Óxido veio pelo vapor, e fazia anos que a gente já não respirava direito aquele vapor.",
                "Os de prata, lá em cima, nunca precisaram de máscara...",
                "...até precisarem.",
                "Uma coisa, já que você anda pelos canos: o meu livro de turnos. Arrancaram as folhas quando o verde subiu, e o vento das caldeiras as espalhou.",
                "São quatro páginas. Se achar, traga-as para mim. Quero o livro inteiro de novo antes de... bom. Antes.")
                .Quest = new QuestSpec
            {
                ItemPrefix = LogbookPrefix,
                ItemCount = LogbookCount,
                PurifiedGreeting = "O verde foi embora dos canos. Pela primeira vez em anos, abri o respiro da máscara.",
                Progress = new[]
                {
                    new[] { "Nada ainda? O vento das caldeiras espalhou as folhas pelo distrito inteiro: lá em cima, lá embaixo e do outro lado do lago." },
                    new[] { "Uma página! Faltam três. É a minha letra: reconheço até de longe. As outras devem estar nas alas: em cima, embaixo e do outro lado do lago." },
                    new[] { "Duas. Metade do livro. Ainda faltam duas." },
                    new[] { "Três! Falta só uma. Procure na ala em que você ainda não esteve." },
                },
                Complete = new[]
                {
                    "O livro inteiro... Turno da noite, turno da manhã e a última página. Eu escrevi tudo isso e tinha esquecido.",
                    "Não tenho nada de prata para lhe dar. Mas tenho isto: uma gema que o chefe de manutenção me deu no dia em que se trancou com as caldeiras.",
                    "Ele disse para guardar até alguém merecer. Acho que é você.",
                },
                Done = new[]
                {
                    "Leio o livro toda noite. Não há mais noite aqui embaixo, mas eu leio assim mesmo.",
                },
            };
        }

        // ------------------------------------------------------------------
        // 5. A Praca dos Operarios
        // ------------------------------------------------------------------

        /// <summary>
        /// O centro do distrito: o salao onde os operarios trocavam de turno.
        /// Armarios, o cabide das mascaras, as pichacoes. Chega-se por um
        /// balcao a oeste, vindo da Valvula Emperrada.
        ///
        /// Daqui sai tudo: a coluna do balcao sobe pra Casa de Maquinas (Ala
        /// Alta); a escada no chao desce pra Cisterna e, dela, pro Esgoto (Ala
        /// Baixa); a porta leste da na Cadeia de Vapor (Ala Leste). E, a vista
        /// desde o balcao, a Escadaria das Tres Travas sobe pro boss.
        /// </summary>
        private static void BuildWelderHall()
        {
            const string room = "05_WelderHall";
            Room(room, 82, 34, 130, 46);

            Carve(82, 34, 130, 46);

            // O balcao de chegada, e o degrau que desce pro salao.
            Solid(82, 34, 89, 37);
            Solid(90, 34, 91, 35);
            Rail(86, 89, 38);

            // Ala Alta: a coluna do balcao, que atravessa o teto. Ocupa so
            // metade do buraco: pela outra metade se desce de volta.
            Carve(83, 47, 86, 51);
            Lift("Lift_ToMachineHall", 83, 84, 38, 53);

            // Ala Baixa: o buraco no chao por onde a coluna do Esgoto devolve
            // quem sobe de la. (A entrada da ala e o alcapao da Cisterna.)

            // Ala Leste: a porta.
            Carve(131, 34, 131, 38);
            Decor(FixtureKind.Sign, 127, 39);

            // O caminho por cima dos canos, ate a pichacao do alto.
            PipeFloor(100, 104, 36);
            PipeFloor(106, 110, 38);
            PipeFloor(112, 115, 40);
            Rail(112, 115, 41);

            // --- A Escadaria das Tres Travas ---
            //
            // Um poco que sobe do teto da Praca ate a sala antes da arena, com
            // uma coluna sempre ligada do lado oeste e tres grades atravessadas.
            // Cada grade abre com a valvula do fim de uma ala: foram os operarios
            // que trancaram a porta por fora, como o chefe pediu. O lado leste do
            // poco fica livre pra descer de volta.
            Carve(118, 47, 122, 58);
            Lift("Lift_LockStair", 118, 119, 34, 60);
            Gate("Trava_Alta", 118, 49, 122, 49, GateOpener.Valve, 1, 0);
            Gate("Trava_Baixa", 118, 53, 122, 53, GateOpener.Valve, 1, 0);
            Gate("Trava_Leste", 118, 57, 122, 57, GateOpener.Valve, 1, 0);

            // Os escombros da escadaria la de cima, caidos no pe do poco.
            Put(Debris(0), 116, 34);
            Put(Debris(1), 117, 34);
            Put(Crate(3), 121, 34);
            Put(Debris(0), 122, 34);
            Put(Debris(1), 123, 35, PropLayer.Front);
            Fixtures.Add(new Fixture(FixtureKind.Railing, DecorName("RailFallen"), 120, 34, 2));

            // O painel: um manometro por trava, do lado do poco.
            Fix(FixtureKind.GaugeBig, "Gauge_Lock_Alta", 123, 44);
            Fix(FixtureKind.GaugeBig, "Gauge_Lock_Baixa", 125, 44);
            Fix(FixtureKind.GaugeBig, "Gauge_Lock_Leste", 127, 44);
            Shine(120.5f, 47f, 5f, 0.8f, Glow.Furnace, true);
            Shine(120.5f, 55f, 5f, 0.8f, Glow.Furnace, true);

            // O vestiario: armarios, o cabide das mascaras com uma so, e as
            // pichacoes de quem ficou.
            Lockers(106, 34, 6);
            Decor(FixtureKind.MaskRack, 86, 40);
            Decor(FixtureKind.Poster, 123, 39);
            Decor(FixtureKind.Sign, 104, 43);

            Tell(LoreKind.Graffiti, room, "Lore_Graffiti_1", "a pichação", 92, 34,
                "Riscado na chapa, em letra de quem tinha pressa:",
                "\"A PRATA BEBE, O LATÃO SUA.\"");

            Tell(LoreKind.GraffitiMask, room, "Lore_Graffiti_2", "o desenho", 88, 38,
                "Um desenho tosco, feito a giz, ao lado do cabide: uma máscara de soldador.",
                "Embaixo, outra mão escreveu: \"Quem tem máscara tem cara.\"",
                "E, mais embaixo, uma terceira: \"E quem não tem?\"");

            Tell(LoreKind.Graffiti, room, "Lore_Graffiti_3", "a pichação", 114, 41,
                "Lá no alto, onde só chega quem anda pelos canos:",
                "\"ELES NUNCA DESCERAM ATÉ AQUI. NEM PARA VER.\"",
                "E, mais abaixo, outra mão, de outro dia: \"DAQUI EU VIA AS LUZES DA AVENIDA DAS FACETAS PELO RESPIRO DO TETO. APAGARAM-SE UMA A UMA.\"")
                .PurifiedLines = new[]
            {
                "Lá no alto, onde só chega quem anda pelos canos:",
                "\"ELES NUNCA DESCERAM ATÉ AQUI. NEM PARA VER.\"",
                "Embaixo, em giz novo, numa letra que ainda treme: \"Um desceu. Era de sucata.\"",
            };

            // Depois da purificacao, uma soldadora tira a mascara aqui, na
            // frente dos armarios: e quem conta quem era o Mestre das Caldeiras.
            LoreSpec welder = Tell(LoreKind.Tarnished, room, "Npc_Solda", "Solda", 106, 34,
                "Pode chegar perto. Tirei a máscara. É a primeira vez que respiro sem ela desde que vim soldar aqui embaixo.",
                "Você derrubou o Mestre das Caldeiras? ...Então precisa saber quem ele era. Era o nosso chefe de manutenção.",
                "Quando a Coroa mandou recolher as nossas máscaras para os de prata, foi ele quem escondeu as caixas e jurou que não havia nenhuma.",
                "Depois se trancou com as caldeiras para manter a pressão. Sem vapor, aqui embaixo, a gente congelava antes de o Óxido chegar.",
                "Ele nos salvou duas vezes. Na segunda, já não sabia quem era. Obrigada por deixá-lo descansar.");
            welder.AfterPurification = true;

            Enemy(EnemyKind.BrassWelder, "Welder_Hall_1", 100, 125, 34, start: 106);
            Enemy(EnemyKind.BrassWelder, "Welder_Hall_2", 100, 125, 34, start: 115);
            Leech("Leech_Hall", 124, 46, 34, 100, 125);

            Boiler("Boiler_Hall_West", 100, 34);
            Furnace("Furnace_Hall", 122, 34);
            Lamp(90, 46);
            Lamp(100, 46);
            Lamp(110, 46);
            Lamp(126, 46);
            HangChain(96, 46, 4);
            HangChain(116, 46, 3);

            RunPipe(PipeLayer.Back, false, (82, 44), (117, 44));
            RunPipe(PipeLayer.Back, true, (97, 44), (97, 36));
            RunPipe(PipeLayer.Back, false, (123, 42), (131, 42));
            RunPipe(PipeLayer.Mid, false, (111, 47), (111, 44), (117, 44));
            Leaks.Add(new Leak(97.5f, 42f, 1f, 0f));
            Steam(room, Area.Between(82, 34, 130, 46), 0.10f);
            DripsIn(Area.Between(90, 34, 117, 46), 8, 51, false);
        }

        // ------------------------------------------------------------------
        // 6. A Cadeia de Vapor (Ala Leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Um lago de acido que se cruza no vapor: coluna, ilha, jato, ilha,
        /// coluna que pulsa e o patamar do outro lado, com a bancada da ala e a
        /// porta do Labirinto. A volta e por baixo, por canos baixos ate a
        /// primeira coluna.
        ///
        /// O segredo da ala — e uma das duas gemas do distrito — fica num
        /// nicho no alto da parede oeste, em cima da porta: so se chega
        /// saindo do topo da coluna que pulsa e emendando dash em duas nuvens.
        /// </summary>
        private static void BuildSteamChain()
        {
            const string room = "06_SteamChain";
            Room(room, 132, 24, 162, 48);

            Carve(132, 24, 162, 48);
            Voids.Add(Area.Between(132, 24, 162, 48));

            Solid(132, 24, 135, 33);
            Solid(159, 24, 162, 43);
            Acid("Acid_SteamChain", 136, 24, 158, 26);

            // A travessia.
            Lift("Lift_Chain_1", 136, 138, 27, 40);
            PipeFloor(140, 143, 36);
            Rail(140, 143, 37);
            Jet("Jet_Chain", 144, 150, 37, 40, 1);
            PipeFloor(150, 154, 33);
            Rail(150, 154, 34);

            Vent pulsing = Lift("Lift_Chain_2", 155, 157, 27, 47);
            pulsing.OnTime = 2.6f;
            pulsing.OffTime = 1.3f;

            // A volta, por baixo: do patamar se cai na ilha do jato, e dali num
            // cano baixo ate a primeira coluna.
            PipeFloor(144, 147, 30);

            // Rente ao acido, uma ilha de respiro pra quem cai — e pra quem desce
            // de proposito.
            PipeFloor(146, 148, 27);
            Tell(LoreKind.GraffitiMask, room, "Lore_LakeMask", "uma máscara de solda", 147, 28,
                "Uma máscara de solda, rachada ao meio, presa entre o cano e a grade.",
                "Por dentro, gravado a prego, em letra pequena: \"PARA A PEQUENA, QUANDO CRESCER.\"",
                "A máscara é do tamanho de uma cabeça de criança.");

            // O patamar, a bancada da ala e a porta do Labirinto.
            Bench(room, "Workbench_Chain", 159, 44);
            Carve(163, 44, 163, 47);

            // O segredo: duas nuvens no alto e o nicho em cima da porta oeste.
            Cloud("Cloud_Chain_1", 149, 44, 151, 47);
            Cloud("Cloud_Chain_2", 141, 44, 143, 47);
            PipeFloor(132, 135, 44);
            Reward(room, RewardKind.Gem, "D2_Chain_Gem", 133, 45);
            Tell(LoreKind.Page, room, "Lore_Letter", "uma carta dobrada", 135, 45,
                "Uma carta dobrada em quatro, presa atrás de um cano com um rebite.",
                "\"Pequena, o vapor está mais pesado esta semana. Não venha me visitar no turno. Fique no distrito de cobre, perto das janelas.\"",
                "\"Se mandarem todo mundo descer pela calha, não tenha medo. Eu desço os canos atrás de você. Prometo.\"",
                "A carta nunca foi enviada.");

            Enemy(EnemyKind.ValveSentry, "Sentry_Chain_2", 143, 143, 37, -1, 5);
            Enemy(EnemyKind.AcidBloater, "Bloater_Chain", 150, 154, 34, start: 153);

            Decor(FixtureKind.Sign, 133, 40);
            Lamp(160, 48);
            HangChain(139, 48, 5);
            HangChain(153, 48, 4);

            RunPipe(PipeLayer.Mid, true, (137, 22), (137, 26));
            RunPipe(PipeLayer.Mid, true, (156, 22), (156, 26));
            RunPipe(PipeLayer.Mid, false, (143, 49), (143, 41), (144, 41));
            RunPipe(PipeLayer.Far, false, (66, 18), (81, 18));
            RunPipe(PipeLayer.Far, true, (70, 11), (70, 24));
            RunPipe(PipeLayer.Far, true, (78, 11), (78, 24));

            Fumes("SteamChain", Area.Between(136, 24, 158, 26), 0.18f);
            Hazes.Add(new Haze("Steam_SteamChain", 147f, 40f, 28f, 10f, 0.06f, 2.4f, false, Glow.Steam));
            Skylines.Add(new Skyline("Skyline_Chain_Far", 25f, false, 3f, 0.25f));
            Skylines.Add(new Skyline("Skyline_Chain_Near", 23f, true, 2f, 0.5f));
            DripsIn(Area.Between(136, 27, 158, 48), 5, 61, true);
        }

        // ------------------------------------------------------------------
        // 7. O Labirinto de Valvulas (Ala Leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Tres valvulas em sequencia, cada uma abrindo a proxima, todas a
        /// vista. Entra-se pelo alto de um poco e cai-se no corredor de baixo:
        ///   A, no pe do poco, abre o portao do corredor;
        ///   B, no fim do corredor, liga a coluna pro andar do meio;
        ///   C, no andar do meio, e a da trava.
        /// Um buraco no andar do meio devolve ao corredor, e a coluna do poco
        /// (sempre ligada) leva de volta a porta: da pra sair a qualquer hora.
        /// O corredor de baixo e onde os colegas do livro de turnos sumiram.
        /// </summary>
        private static void BuildValveMaze()
        {
            const string room = "07_ValveMaze";
            Room(room, 164, 27, 185, 47);

            // O poco de entrada: a porta da no alto dele, sobre um degrau. Uma
            // coluna sempre ligada sobe o poco: e por ela que se sai, a
            // qualquer hora.
            Carve(164, 27, 170, 47);
            Solid(164, 43, 165, 43);
            Lift("Lift_Maze_Elevator", 168, 170, 27, 46);

            // Corredor de baixo, o portao no meio, e o beco da valvula B.
            Carve(171, 27, 185, 31);
            Gate("Gate_Maze", 176, 27, 176, 31, GateOpener.Valve);
            Valve(room, "Valve_Maze_A", 166, 29, 167, 27, "gate:Gate_Maze");
            Valve(room, "Valve_Maze_B", 184, 29, 185, 27, "vent:Lift_Maze_Up");
            Carve(181, 32, 183, 35);
            Lift("Lift_Maze_Up", 181, 183, 27, 38).StartsOff = true;

            Logbook(room, 3, "página do livro de turnos", 172, 27,
                "LIVRO DE TURNOS DA CALDEIRA — a letra está tremida.",
                "Três colegas não voltaram do turno. Fui procurá-los no corredor de baixo.",
                "Achei as ferramentas deles. Só as ferramentas. Estavam verdes.");

            // Andar do meio: a valvula da trava, e um buraco no chao que devolve
            // ao corredor de baixo (o portao ja esta aberto quando se chega aqui).
            Carve(172, 36, 185, 41);
            Carve(175, 32, 176, 35);
            Valve(room, "Valve_Maze_C", 172, 38, 173, 36, "gate:Trava_Leste").Lock = true;

            Enemy(EnemyKind.AcidBloater, "Bloater_Maze", 177, 185, 27, start: 180);
            Leech("Leech_Maze", 180, 31, 27, 177, 185);
            Enemy(EnemyKind.BrassWelder, "Welder_Maze", 177, 180, 36);
            Enemy(EnemyKind.ValveSentry, "Sentry_Maze", 185, 185, 36, -1, 4);

            Decor(FixtureKind.Sign, 166, 45);
            Fix(FixtureKind.GaugeBig, "Gauge_Maze_1", 168, 34);
            Fix(FixtureKind.GaugeBig, "Gauge_Maze_2", 182, 39);
            Decor(FixtureKind.Barrel, 174, 27);
            Lamp(167, 47, 0.7f);
            Lamp(180, 31, 0.5f);
            Lamp(183, 41, 0.6f);
            HangChain(169, 47, 6);

            Plumb(Area.Between(171, 27, 185, 31), 701, PipeLayer.Back, false, 0);
            Plumb(Area.Between(172, 36, 185, 41), 702, PipeLayer.Back, true, 0);
            RunPipe(PipeLayer.Mid, true, (170, 33), (164, 33));
            Leaks.Add(new Leak(170f, 33.5f, -1f, 0f));
            Steam(room, Area.Between(172, 36, 185, 41), 0.12f);
            DripsIn(Area.Between(171, 27, 185, 31), 5, 71, true);
        }

        // ------------------------------------------------------------------
        // 8. A sala antes da arena
        // ------------------------------------------------------------------

        /// <summary>
        /// No alto da Escadaria das Travas: a bancada de antes da luta e a
        /// porta da sala das caldeiras, trancada por fora. O poco da Escadaria
        /// abre no chao, e por ele se desce de volta pra Praca. Depois da
        /// purificacao, o atalho da Galeria Alta cai aqui.
        /// </summary>
        private static void BuildWorkbenchRoom()
        {
            const string room = "08_PreBoss";
            Room(room, 110, 59, 128, 65);

            Carve(110, 59, 128, 65);

            // Passagem pra arena, a leste. O selo fecha a coluna do lado dela.
            Carve(129, 59, 131, 63);

            Bench(room, "Workbench_PreBoss", 111, 59);
            Fix(FixtureKind.Hatch, "Hatch_PreBoss", 128, 59);

            // O poco da Escadaria nao acaba aqui: continua subindo ate a
            // estacao do elevador, la no alto. Mas a escadaria desabou — os
            // degraus que sobraram ficam pendurados, longe demais um do outro
            // pra subir, e os pedacos cairam aqui e na Praca.
            BrokenStairway();
            Fix(FixtureKind.GaugeBig, "Gauge_PreBoss", 124, 62);
            Decor(FixtureKind.Poster, 114, 62);

            Tell(LoreKind.Graffiti, room, "Lore_BoilerDoor", "a porta da sala das caldeiras", 126, 59,
                "A porta da sala das caldeiras tem uma tranca soldada pelo lado de fora.",
                "Riscado no latão, em letras de mãos diferentes: \"NÃO ABRAM.\" \"OUÇAM O QUE OUVIREM.\" \"ELE PEDIU.\"",
                "E, mais embaixo, mais fundo, como quem riscou muitas vezes no mesmo lugar: \"Desculpe, chefe.\"");

            // Os escombros da escada, no chao da antessala.
            Put(Debris(0), 114, 59);
            Put(Debris(1), 115, 59);
            Put(Crate(2), 116, 59);
            Put(Debris(0), 124, 59);
            Put(Crate(0), 125, 59);
            Put(Crate(1), 125, 60);
            Fixtures.Add(new Fixture(FixtureKind.Railing, DecorName("RailFallen"), 113, 59, 2));
            Lamp(111, 65);
            Lamp(127, 65, 0.6f);

            RunPipe(PipeLayer.Back, false, (108, 64), (131, 64));
            RunPipe(PipeLayer.Back, true, (121, 64), (121, 61));
        }

        /// <summary>
        /// A escadaria quebrada: o poco da Escadaria das Travas continua acima
        /// da antessala ate o piso da estacao do elevador. Sobraram quatro
        /// lances, alternando de lado, cada um cinco tiles acima do outro —
        /// nem com o Salto de Pressao se sobe por eles. Descer, da: e o atalho
        /// de volta de quem chega na estacao.
        /// </summary>
        private static void BrokenStairway()
        {
            Carve(114, 66, 126, 98);

            int[,] flights = { { 121, 125, 77 }, { 115, 119, 83 }, { 121, 125, 89 }, { 115, 119, 95 } };
            for (int i = 0; i < flights.GetLength(0); i++)
            {
                int x0 = flights[i, 0];
                int x1 = flights[i, 1];
                int y = flights[i, 2];

                PipeFloor(x0, x1, y);

                // O guarda-corpo, partido no meio de cada lance.
                Rail(x0, x0 + 1, y + 1);
                Put(Debris(i % 2), x1, y + 1);
            }

            // Os pedacos dos lances que faltam, pendurados nas paredes.
            Put(Debris(1), 114, 72, PropLayer.Back);
            Put(Debris(0), 126, 80, PropLayer.Back);
            Put(Debris(1), 114, 88, PropLayer.Back);
            HangChain(117, 98, 6);
            HangChain(124, 98, 9);
            HangChain(120, 88, 4);
            Lamp(126, 98, 0.5f);

            Fix(FixtureKind.GaugeBig, "Gauge_Stairway_1", 113, 78);
            Fix(FixtureKind.GaugeBig, "Gauge_Stairway_2", 126, 92);
            RunPipe(PipeLayer.Back, true, (113, 66), (113, 98));
            RunPipe(PipeLayer.Back, false, (127, 66), (127, 98));
            Hazes.Add(new Haze("Steam_Stairway", 120f, 94f, 12f, 5f, 0.07f, 1.5f, false, Glow.Steam));
            DripsIn(Area.Between(114, 66, 126, 98), 4, 141, false);
        }

        // ------------------------------------------------------------------
        // 9. A Sala de Caldeiras
        // ------------------------------------------------------------------

        private static void BuildBoilerRoom()
        {
            const string room = "09_BoilerRoom";
            Room(room, 132, 59, 178, 74);

            // Entra-se a oeste, vindo do pre-boss (o selo fecha x 131); sai-se a
            // leste, pro poco de fuga (a grade fica em x 180).
            Carve(132, 59, 178, 74);
            Voids.Add(Area.Between(132, 59, 178, 74));

            Solid(175, 73, 178, 74);
            Solid(177, 71, 178, 72);
            Solid(132, 73, 135, 74);
            Solid(132, 71, 133, 72);

            Carve(138, 53, 172, 57);
            Acid("Acid_BoilerRoom", 138, 53, 172, 56);
            Grate(138, 172, 58);

            PipeFloor(172, 173, 60);
            PipeFloor(137, 138, 60);
            PipeFloor(165, 170, 62);
            PipeFloor(140, 145, 62);
            PipeFloor(158, 162, 64);
            PipeFloor(148, 152, 64);
            PipeFloor(154, 156, 66);

            Carve(179, 59, 181, 62);

            Mark(room, "BossSpawn_BoilerMaster", BossTile.X, BossTile.Y);
            Mark(room, "Gate_Entry", 132, 59);

            Gate(BossGate, 180, 59, 180, 62, GateOpener.Boss);

            Boiler("Boiler_Arena_West", 142, 59);
            Boiler("Boiler_Arena_Center", 154, 59);
            Boiler("Boiler_Arena_East", 166, 59);

            Fix(FixtureKind.GaugeBig, "Gauge_Arena_West", 148, 69);
            Fix(FixtureKind.GaugeBig, "Gauge_Arena_East", 162, 69);
            HangChain(140, 74, 4);
            HangChain(170, 74, 4);

            RunPipe(PipeLayer.Mid, true, (168, 65), (168, 76));
            RunPipe(PipeLayer.Mid, false, (155, 65), (155, 68));
            RunPipe(PipeLayer.Mid, true, (143, 65), (143, 76));
            RunPipe(PipeLayer.Mid, false, (180, 72), (173, 72), (173, 69));
            RunPipe(PipeLayer.Mid, false, (130, 72), (135, 72), (135, 69));
            RunPipe(PipeLayer.Far, false, (66, 34), (89, 34));
            RunPipe(PipeLayer.Far, true, (71, 28), (71, 38));
            RunPipe(PipeLayer.Far, true, (84, 28), (84, 38));

            Fumes("BoilerRoom", Area.Between(138, 53, 172, 56), 0.14f);
            Hazes.Add(new Haze("Steam_BoilerRoom", 156f, 69f, 44f, 6f, 0.09f, 2.2f, false, Glow.Steam));
            Skylines.Add(new Skyline("Skyline_Arena_Far", 56f, false, 3f, 0.25f));
            Skylines.Add(new Skyline("Skyline_Arena_Near", 55.5f, true, 2f, 0.5f));
            DripsIn(Area.Between(138, 59, 172, 74), 7, 91, false);
        }

        // ------------------------------------------------------------------
        // O tunel da caldeira
        // ------------------------------------------------------------------

        /// <summary>
        /// Atras da grade leste da arena, que abre quando o Boiler Master cai
        /// (e o distrito se purifica): um tunel curto e, no fim dele, a coluna
        /// que sobe pra Galeria Alta. Dali em diante e so subir.
        /// </summary>
        private static void BuildExitTunnel()
        {
            Carve(182, 59, 188, 62);

            // A coluna ocupa so metade do poco: pela outra metade se desce.
            Carve(183, 63, 186, 77);
            Lift("Lift_ToHighGallery", 183, 184, 59, 80);

            Decor(FixtureKind.Sign, 185, 66);
            Decor(FixtureKind.Barrel, 187, 59);
            Lamp(187, 62, 0.6f);
            Shine(184f, 70f, 5f, 0.6f, Glow.Steam);
            RunPipe(PipeLayer.Back, false, (181, 62), (188, 62));
            RunPipe(PipeLayer.Back, true, (187, 63), (187, 77));
            Fix(FixtureKind.GaugeSmall, "Gauge_ExitTunnel", 182, 61);
        }

        // ------------------------------------------------------------------
        // A. O Esgoto (Ala Baixa)
        // ------------------------------------------------------------------

        /// <summary>
        /// O comeco da Ala Baixa, embaixo da Cisterna: cai-se pelo alcapao numa
        /// ilha e cruza-se o canal de ilha em ilha, com o dash, ate a coluna
        /// que (religada no fim da ala) devolve o Scrap a Praca. Dali um tunel
        /// curto segue pro Deposito. A oeste, um patamar seco com as marcas de
        /// quem ficou preso aqui.
        /// </summary>
        private static void BuildSewer()
        {
            const string room = "A_Sewer";
            Room(room, 86, 6, 129, 14);

            Carve(86, 6, 129, 14);

            // O canal: ilhas largas e vaos de tres, que se pulam sem dash. Cada
            // vao e uma poca so dele, encaixada entre as ilhas — o acido nunca
            // passa por baixo de onde se pisa.
            int[,] gaps = { { 89, 91 }, { 96, 98 }, { 107, 109 }, { 114, 116 }, { 121, 123 } };
            for (int i = 0; i < gaps.GetLength(0); i++)
            {
                Carve(gaps[i, 0], 3, gaps[i, 1], 5);
                Acid("Acid_Sewer_" + (i + 1), gaps[i, 0], 3, gaps[i, 1], 5);
            }

            // Uma ilha mais alta no meio: pisa-se em 7.
            Solid(110, 6, 113, 6);

            Tell(LoreKind.Graffiti, room, "Lore_SewerTally", "as marcas de giz", 86, 6,
                "Riscos de giz na parede, em grupos de cinco. Dezenas de grupos.",
                "Em cima deles, numa letra cansada: \"DIAS SEM SUBIR.\"",
                "O último grupo tem só três riscos.");

            // A coluna de volta pra Praca: sempre ligada, e o jeito de sair do
            // Esgoto a qualquer hora.
            Carve(126, 15, 128, 33);
            Lift("Lift_Sewer_Up", 126, 128, 6, 36);

            Tell(LoreKind.Plaque, room, "Lore_SewerPlaque", "a placa do coletor", 124, 6,
                "Uma placa de latão, meio afundada na lama do esgoto:",
                "\"COLETOR DE CONDENSADO N.º 4. NÃO ENTRAR SEM MÁSCARA.\"",
                "Embaixo, alguém riscou nomes, um por linha. Doze. O último está pela metade.");

            // O tunel pro Deposito.
            Carve(130, 6, 140, 9);
            Tell(LoreKind.Graffiti, room, "Lore_TunnelArrow", "a seta pintada", 131, 6,
                "Uma seta pintada às pressas na parede, apontando para o leste: \"DEPÓSITO — MÁSCARAS\".",
                "Alguém riscou \"MÁSCARAS\" com tinta fresca e escreveu por cima: \"NADA\".",
                "A tinta escorreu antes de secar.");

            // So sanguessugas aqui embaixo, cada uma no meio de uma ilha larga:
            // da pra ve-las do vao anterior e esperar ela cair antes de pular.
            Leech("Leech_Sewer_1", 94, 14, 6, 92, 95);
            Leech("Leech_Sewer_2", 112, 14, 7, 110, 113);
            Leech("Leech_Sewer_3", 119, 14, 6, 117, 120);
            Leech("Leech_Tunnel", 137, 9, 6, 124, 152);

            Decor(FixtureKind.Barrel, 87, 6);
            Decor(FixtureKind.Sign, 100, 11);
            HangChain(105, 14, 4);
            HangChain(122, 14, 3);
            Lamp(87, 14, 0.5f);
            Lamp(107, 14, 0.5f);

            Fumes("Sewer", Area.Between(89, 3, 123, 5), 0.20f);
            RunPipe(PipeLayer.Mid, true, (84, 12), (130, 12));
            RunPipe(PipeLayer.Back, true, (90, 13), (90, 7));
            RunPipe(PipeLayer.Back, false, (115, 13), (115, 8));
            RunPipe(PipeLayer.Back, true, (130, 9), (141, 9));
            RunPipe(PipeLayer.Far, true, (44, 4), (64, 4));
            DripsIn(Area.Between(86, 6, 129, 14), 5, 81, true);
            DripsIn(Area.Between(130, 6, 140, 9), 4, 82, true);
        }

        // ------------------------------------------------------------------
        // B. A Casa de Maquinas (Ala Alta)
        // ------------------------------------------------------------------

        /// <summary>
        /// A Ala Alta: um salao de quatro andares de passarela por cima da
        /// Praca, com as caldeiras grandes no fundo. Chega-se pela coluna do
        /// balcao da Praca, que abre no chao, a leste.
        ///
        /// O caminho: chao → coluna sempre ligada → andar 1, cuja valvula (no
        /// fim leste, a vista) liga a coluna pro andar 2 → no andar 2, um vao
        /// cruzado por uma nuvem e a coluna que pulsa → andar 3, com a bancada
        /// da ala e a valvula da trava, que abre tambem a escotilha do chao,
        /// de volta pro balcao da Praca.
        ///
        /// Fora do caminho: a oeste do andar 1, a porta do Sotao; no fim do
        /// andar 2, o cartaz da Coroa; e, depois de outra nuvem no andar 3, um
        /// cofre.
        /// </summary>
        private static void BuildMachineHall()
        {
            const string room = "B_MachineHall";
            Room(room, 44, 52, 92, 84);

            Carve(44, 52, 92, 84);

            // Chao: o buraco da coluna que sobe da Praca. Pela metade leste
            // dele se cai de volta no balcao, a qualquer hora.
            Lift("Lift_Hall_1", 70, 72, 52, 64);

            Logbook(room, 2, "página do livro de turnos", 91, 52,
                "LIVRO DE TURNOS DA CALDEIRA — turno da manhã.",
                "A água está com gosto de cobre. Não é do cano: troquei o cano.",
                "O manômetro três não sai do vermelho. Parei de olhar para ele.");

            // Andar 1.
            Grate(44, 68, 61);
            Grate(74, 92, 61);
            Rail(74, 92, 62);
            Carve(43, 62, 43, 65);
            Valve(room, "Valve_Hall_1", 89, 64, 90, 62, "vent:Lift_Hall_2");
            Lift("Lift_Hall_2", 80, 82, 62, 74).StartsOff = true;

            // Andar 2: o vao do meio pede a nuvem.
            Grate(47, 57, 71);
            Grate(66, 78, 71);
            Grate(84, 92, 71);
            Rail(66, 78, 72);
            Cloud("Cloud_Hall", 60, 72, 63, 75);
            Tell(LoreKind.Plaque, room, "Lore_CrownPoster", "um cartaz da Coroa", 54, 72,
                "Um cartaz desbotado, com o selo dos 925 Nobres:",
                "\"O VAPOR É O SANGUE DA CIDADE. CADA OPERÁRIO, UMA GOTA.\"",
                "Alguém completou embaixo, a carvão: \"E eles bebem.\"");
            Decor(FixtureKind.Poster, 52, 74);

            Vent pulsing = Lift("Lift_Hall_3", 44, 46, 72, 83);
            pulsing.OnTime = 2.4f;
            pulsing.OffTime = 1.2f;

            // Andar 3: a bancada da ala, a valvula da trava, e, depois de uma
            // nuvem, o cofre.
            Grate(47, 70, 80);
            Rail(47, 70, 81);
            Bench(room, "Workbench_MachineHall", 49, 81);

            Valve(room, "Valve_Hall_2", 65, 83, 66, 81, "gate:Trava_Alta").Lock = true;

            Cloud("Cloud_Hall_Cache", 74, 81, 76, 84);
            Grate(81, 92, 80);
            Reward(room, RewardKind.ShardCache, "D2_Hall_Cache", 90, 81, 20);
            Mark(room, "Secret_SilverFragment_Hall", 90, 81);

            Enemy(EnemyKind.BrassWelder, "Welder_Machine_1", 44, 82, 52, start: 56);
            Leech("Leech_Machine_1", 76, 60, 52, 44, 82);
            Enemy(EnemyKind.AcidBloater, "Bloater_Machine", 74, 92, 62, start: 81);
            Enemy(EnemyKind.ValveSentry, "Sentry_Machine_1", 45, 45, 62, 1, 8);
            Enemy(EnemyKind.ValveSentry, "Sentry_Machine_2", 92, 92, 72, -1, 6);
            Leech("Leech_Machine_2", 70, 79, 72, 66, 78);
            Enemy(EnemyKind.BrassWelder, "Welder_Machine_2", 53, 70, 81, start: 58);

            // As caldeiras grandes no fundo, as fornalhas e o resto.
            Boiler("Boiler_Machine_1", 52, 52);
            Boiler("Boiler_Machine_2", 61, 52);
            Boiler("Boiler_Machine_3", 76, 52);
            Furnace("Furnace_Machine_1", 66, 52);
            Furnace("Furnace_Machine_2", 84, 62);
            Decor(FixtureKind.Sign, 79, 57);
            Decor(FixtureKind.Poster, 86, 74);
            Fix(FixtureKind.GaugeBig, "Gauge_Machine_1", 57, 64);
            Fix(FixtureKind.GaugeBig, "Gauge_Machine_2", 88, 66);
            Fix(FixtureKind.GaugeBig, "Gauge_Machine_3", 72, 76);
            Decor(FixtureKind.Barrel, 69, 81);
            Lamp(58, 60);
            Lamp(86, 60);
            Lamp(52, 70);
            Lamp(72, 70);
            Lamp(56, 84);
            Lamp(86, 84);
            HangChain(64, 84, 6);
            HangChain(78, 84, 4);

            RunPipe(PipeLayer.Mid, false, (57, 51), (57, 86));
            RunPipe(PipeLayer.Mid, true, (82, 51), (82, 61));
            RunPipe(PipeLayer.Back, false, (44, 69), (92, 69));
            RunPipe(PipeLayer.Back, true, (44, 78), (92, 78));
            RunPipe(PipeLayer.Back, false, (68, 52), (68, 60));
            RunPipe(PipeLayer.Far, false, (22, 35), (46, 35));
            RunPipe(PipeLayer.Far, true, (30, 26), (30, 42));
            Leaks.Add(new Leak(57.5f, 66f, 1f, 0f));
            Leaks.Add(new Leak(82.5f, 58f, -1f, 0f));

            Hazes.Add(new Haze("Steam_MachineHall", 68f, 80f, 48f, 7f, 0.08f, 3.2f, false, Glow.Steam));
            DripsIn(Area.Between(44, 52, 92, 84), 9, 91, false);
        }

        // ------------------------------------------------------------------
        // C. O Sotao dos Manometros (Ala Alta)
        // ------------------------------------------------------------------

        /// <summary>
        /// A oeste do andar 1 da Casa de Maquinas: uma sala baixa e comprida,
        /// forrada de manometros. E um desvio — os corrompidos guardam a
        /// entrada; no fundo, em paz, o relatorio do Rei de Prata, um cofre e a
        /// valvula que liga a coluna dos Primeiros Canos (o atalho de volta da
        /// entrada ate aqui).
        /// </summary>
        private static void BuildGaugeAttic()
        {
            const string room = "C_GaugeAttic";
            Room(room, 4, 62, 42, 72);

            Carve(4, 62, 42, 72);
            Solid(4, 70, 10, 72);
            Solid(16, 62, 18, 63);

            Valve(room, "Valve_Attic", 5, 64, 6, 62, "vent:Lift_ToAttic").Notice =
                "Ao longe, o vapor volta a subir pelos Primeiros Canos, até o sótão.";

            Reward(room, RewardKind.ShardCache, "D2_Attic_Cache", 11, 62, 10);
            Mark(room, "Secret_SilverFragment_Attic", 11, 62);
            Tell(LoreKind.Page, room, "Lore_InspectionReport", "um relatório de inspeção", 8, 62,
                "Um relatório com o selo dos 925, esquecido debaixo de um manômetro.",
                "\"INSPEÇÃO DAS CALDEIRAS DO DISTRITO DE LATÃO. Pressão acima do limite em todas as linhas. Recomendo desligar metade das caldeiras.\"",
                "Carimbado por cima, em vermelho: \"NEGADO. A CIDADE NÃO PODE ESCURECER. — O REI DE PRATA.\"",
                "Na margem, a lápis: \"Então, que escureça aqui embaixo.\"");

            Enemy(EnemyKind.BrassWelder, "Welder_Attic", 19, 34, 62, start: 28);
            Enemy(EnemyKind.ValveSentry, "Sentry_Attic", 36, 36, 62, -1, 7);
            Leech("Leech_Attic_1", 28, 72, 62, 19, 34);
            Leech("Leech_Attic_2", 20, 72, 62, 19, 34);

            // A parede de manometros.
            for (int x = 12; x <= 40; x += 5)
            {
                Fix(FixtureKind.GaugeBig, $"Gauge_Attic_{x}", x, 66 + (x / 5) % 2 * 2);
            }

            Decor(FixtureKind.Shelf, 13, 62);
            Decor(FixtureKind.Poster, 33, 67);
            Furnace("Furnace_Attic", 30, 62);
            Lamp(9, 69, 0.8f);
            Lamp(26, 72, 0.6f);
            Lamp(38, 72, 0.6f);

            Plumb(Area.Between(11, 62, 42, 72), 1001, PipeLayer.Back, false, 0);
            RunPipe(PipeLayer.Mid, true, (2, 66), (14, 66), (14, 63));
            Steam(room, Area.Between(4, 62, 42, 72), 0.09f);
            DripsIn(Area.Between(11, 62, 42, 72), 8, 101, false);
        }

        // ------------------------------------------------------------------
        // D. O Deposito (Ala Baixa)
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde as mascaras ficavam. Entra-se por baixo de um patamar (o tunel
        /// do Esgoto); a coluna do lado sobe ate ele, com a ficha da Coroa e um
        /// cofre. Uma caldeira corrompida derruba a parede rachada pra metade
        /// leste: prateleiras vazias, a ultima pagina do livro, a bancada da
        /// ala e a porta da Torre.
        /// </summary>
        private static void BuildDepot()
        {
            const string room = "D_Depot";
            Room(room, 141, 6, 182, 19);

            Carve(141, 6, 182, 19);

            Solid(141, 10, 146, 14);
            Lift("Lift_Depot_West", 147, 149, 6, 17);
            Reward(room, RewardKind.ShardCache, "D2_Depot_Cache", 142, 15, 10);
            Mark(room, "Secret_SilverFragment_Depot", 142, 15);
            Tell(LoreKind.Page, room, "Lore_DepotSheet", "a ficha do depósito", 145, 15,
                "Uma ficha de inventário, pregada na porta do depósito.",
                "\"ORDEM DA COROA: recolher todas as máscaras de solda e enviá-las ao Distrito de Prata. Prioridade: Nobres.\"",
                "Logo abaixo, à mão: \"Recolhidas: nenhuma. Não há máscaras neste depósito. — O chefe de manutenção.\"",
                "Nas prateleiras atrás da ficha, só restam as marcas de caixas retiradas às pressas.");

            Solid(160, 10, 161, 19);
            Cracked("Wall_Depot", 160, 6, 161, 9);
            PressureBoiler(room, "Boiler_Depot", 153, 6, 151, 6, "Wall_Depot");

            Logbook(room, 4, "última página do livro de turnos", 164, 6,
                "LIVRO DE TURNOS DA CALDEIRA — a última página.",
                "Está quase em branco. No meio dela, uma linha só:",
                "\"As máscaras funcionam.\"");

            Bench(room, "Workbench_Depot", 179, 6);
            Carve(183, 6, 193, 9);

            Enemy(EnemyKind.AcidBloater, "Bloater_Depot", 162, 177, 6, start: 173);
            Enemy(EnemyKind.BrassWelder, "Welder_Depot", 162, 177, 6, start: 166);
            Leech("Leech_Depot", 172, 19, 6, 162, 177);

            // As prateleiras vazias, de onde as caixas sairam as pressas.
            for (int x = 163; x <= 177; x += 3) Decor(FixtureKind.Shelf, x, 12);
            Decor(FixtureKind.Shelf, 142, 16);
            Decor(FixtureKind.Barrel, 150, 6);
            Decor(FixtureKind.Barrel, 157, 6);
            Decor(FixtureKind.Barrel, 158, 6);
            Put(Crate(0), 177, 6);
            Put(Debris(0), 166, 6);
            Decor(FixtureKind.Sign, 152, 15);
            Lamp(144, 19, 0.7f);
            Lamp(156, 19);
            Lamp(170, 19);
            Lamp(180, 19, 0.9f);

            Furnace("Furnace_Depot", 174, 6);
            Fix(FixtureKind.GaugeBig, "Gauge_Depot", 166, 16);
            RunPipe(PipeLayer.Back, true, (141, 18), (182, 18));
            RunPipe(PipeLayer.Mid, false, (162, 17), (184, 17));
            DripsIn(Area.Between(147, 6, 182, 19), 7, 111, false);
        }

        // ------------------------------------------------------------------
        // E. A Torre das Valvulas (Ala Baixa)
        // ------------------------------------------------------------------

        /// <summary>
        /// O fim da Ala Baixa: um poco de quatro andares, a leste do Deposito.
        /// Na base, a valvula da primeira coluna; no andar 1, a da segunda; no
        /// andar 2, a placa da linha de vapor e a coluna que pulsa; no alto, o
        /// diario do chefe de manutencao e a valvula da trava.
        /// </summary>
        private static void BuildValveTower()
        {
            const string room = "E_ValveTower";
            Room(room, 194, 6, 213, 74);

            Carve(194, 6, 213, 74);

            // Base.
            Valve(room, "Valve_Tower_1", 210, 8, 211, 6, "vent:Lift_Tower_1");
            Lift("Lift_Tower_1", 195, 197, 6, 27).StartsOff = true;
            Enemy(EnemyKind.AcidBloater, "Bloater_Tower", 183, 213, 6, start: 198);
            Leech("Leech_Tower_1", 204, 24, 6, 183, 213);

            // Andar 1.
            Grate(200, 213, 25);
            Rail(203, 213, 26);
            Valve(room, "Valve_Tower_2", 211, 28, 212, 26, "vent:Lift_Tower_2");
            Lift("Lift_Tower_2", 200, 202, 26, 47).StartsOff = true;

            // Andar 2.
            Grate(194, 198, 45);
            Grate(206, 213, 45);
            Tell(LoreKind.Plaque, room, "Lore_TowerLine", "a placa da linha", 195, 46,
                "Cada andar da torre tem uma placa com o bairro que a sua linha de vapor aquecia. Esta diz:",
                "\"LINHA 3 — AVENIDA DAS FACETAS. PRIORIDADE: NOBRES.\"",
                "Embaixo, a giz, a mesma pergunta em três letras diferentes: \"E a nossa?\"");
            Vent pulsing = Lift("Lift_Tower_3", 207, 209, 46, 66);
            pulsing.OnTime = 2.5f;
            pulsing.OffTime = 1.2f;
            Enemy(EnemyKind.ValveSentry, "Sentry_Tower_1", 213, 213, 46, -1, 6);

            // O alto.
            Grate(194, 205, 65);
            Grate(211, 213, 65);
            Rail(194, 205, 66);

            Valve(room, "Valve_Tower_3", 194, 68, 195, 66, "gate:Trava_Baixa").Lock = true;

            Enemy(EnemyKind.BrassWelder, "Welder_Tower", 194, 205, 66, start: 201);
            Leech("Leech_Tower_3", 201, 74, 66, 194, 205);

            Tell(LoreKind.Page, room, "Lore_ChiefDiary", "o diário do chefe de manutenção", 211, 66,
                "Um caderno de capa de couro, deixado no alto da torre, virado para a sala das caldeiras.",
                "\"Se as caldeiras pararem, o vapor para. Sem vapor, os de baixo congelam antes de o Óxido chegar.\"",
                "\"Vou ficar com elas. Alguém tem que manter a pressão. Tranquem a porta por fora e não a abram, ouçam o que ouvirem.\"",
                "A última página tem só a marca de uma mão, queimada no papel.");

            Boiler("Boiler_Tower_Base", 205, 6);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_1", 199, 14);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_2", 204, 36);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_3", 198, 56);
            Decor(FixtureKind.Sign, 203, 30);
            Lamp(208, 24, 0.7f);
            Lamp(196, 44, 0.7f);
            Lamp(212, 64, 0.7f);
            Lamp(212, 74, 0.8f);
            HangChain(196, 80, 7);
            HangChain(212, 44, 5);

            for (int x = 199; x <= 211; x += 6)
            {
                RunPipe(PipeLayer.Back, x % 12 == 3, (x, 4), (x, 76));
            }

            RunPipe(PipeLayer.Back, false, (192, 40), (215, 40));
            RunPipe(PipeLayer.Back, true, (192, 60), (215, 60));
            Hazes.Add(new Haze("Steam_Tower", 204f, 70f, 20f, 6f, 0.09f, 2f, false, Glow.Steam));
            DripsIn(Area.Between(194, 6, 213, 24), 6, 121, false);
        }

        // ------------------------------------------------------------------
        // F. A Galeria Alta
        // ------------------------------------------------------------------

        /// <summary>
        /// Por cima da Sala de Caldeiras: chega-se pela coluna do tunel, a
        /// leste. Na chegada, a placa do cano-mestre. A oeste, o desafio do
        /// Salto de Pressao: canos longe e alto demais um do outro ate o
        /// mirante, de onde se ve a cidade la em cima, e um cofre. A leste, a
        /// porta da subida pro elevador.
        /// </summary>
        private static void BuildHighGallery()
        {
            const string room = "F_HighGallery";
            Room(room, 130, 78, 188, 92);

            Carve(130, 78, 188, 92);
            Voids.Add(Area.Between(130, 78, 188, 92));

            Solid(130, 78, 140, 89);
            PipeFloor(160, 163, 79);
            PipeFloor(152, 155, 83);
            PipeFloor(144, 147, 87);

            // A porta da subida, a leste.
            Carve(189, 78, 189, 81);

            Tell(LoreKind.Plaque, room, "Lore_MasterPipe", "a inscrição do cano-mestre", 178, 78,
                "Um cano da grossura de uma casa, subindo até sumir na rocha. Está seco e corroído por dentro.",
                "Na placa, em letra de fundição:",
                "\"PARA A CIDADE DE PRATA.\"",
                "Alguém riscou por cima, a prego: \"Nada mais sobe.\"");
            RunPipe(PipeLayer.Far, true, (88, 39), (88, 47));
            RunPipe(PipeLayer.Far, true, (89, 39), (89, 47));

            Reward(room, RewardKind.ShardCache, "D2_Gallery_Cache", 133, 90, 20);
            Mark(room, "Secret_SilverFragment_Gallery", 133, 90);
            Tell(LoreKind.Plaque, room, "Lore_GalleryView", "a placa do mirante", 137, 90,
                "Daqui, por uma fresta na rocha, dá para ver lá em cima pela primeira vez: os geodos de Cravejada, imensos e apagados.",
                "Uma placa de latão, aparafusada na beirada: \"GALERIA ALTA — ACESSO RESTRITO AOS INSPETORES DA COROA.\"",
                "Riscado embaixo, a prego: \"Agora é de quem subir.\"");

            Boiler("Boiler_Gallery", 166, 78);
            Fix(FixtureKind.GaugeBig, "Gauge_Gallery_1", 172, 84);
            Fix(FixtureKind.GaugeBig, "Gauge_Gallery_2", 148, 88);
            Furnace("Furnace_Gallery_High", 174, 78);
            Decor(FixtureKind.Sign, 134, 91);
            Decor(FixtureKind.Poster, 156, 86);
            Lamp(150, 92);
            Lamp(170, 92);
            Lamp(184, 92, 0.6f);
            Rail(130, 140, 90);

            RunPipe(PipeLayer.Mid, false, (128, 91), (190, 91));
            RunPipe(PipeLayer.Mid, true, (164, 91), (164, 84), (160, 84));

            Skylines.Add(new Skyline("Skyline_Gallery_Far", 75f, false, 3f, 0.25f));
            Skylines.Add(new Skyline("Skyline_Gallery_Near", 74.5f, true, 2f, 0.5f));

            Hazes.Add(new Haze("Steam_HighGallery", 159f, 88f, 58f, 5f, 0.08f, 3f, false, Glow.Steam));
            DripsIn(Area.Between(142, 78, 188, 92), 9, 131, false);
        }

        // ------------------------------------------------------------------
        // G. A Subida
        // ------------------------------------------------------------------

        /// <summary>
        /// A direita da Galeria Alta: o acesso de servico ao elevador, ja que
        /// a escadaria desabou. Um poco de passarelas, cada uma quatro tiles
        /// acima da outra, alternando de lado — feito pro Salto de Pressao. E
        /// a continuacao do poco da Torre das Valvulas, aberto embaixo: da
        /// torre se ve a subida, mas nao se alcanca. No
        /// alto, a passarela dos inspetores volta pra oeste, por cima da
        /// galeria, ate a estacao.
        /// </summary>
        private static void BuildElevatorClimb()
        {
            const string room = "G_ElevatorClimb";
            Room(room, 190, 78, 207, 103);

            Carve(190, 78, 207, 103);

            // O fundo e aberto pra Torre das Valvulas, logo embaixo: do alto
            // dela se ve a subida, mas e alto demais pra chegar (ate com o
            // Salto de Pressao). So sobra um patamar, na porta da galeria. De
            // cima, da pra se deixar cair ate a torre.
            Carve(194, 75, 207, 77);
            Rail(190, 193, 78);

            int[,] decks = { { 190, 197, 81 }, { 199, 207, 85 }, { 190, 197, 89 }, { 199, 207, 93 }, { 190, 197, 97 } };
            for (int i = 0; i < decks.GetLength(0); i++)
            {
                Grate(decks[i, 0], decks[i, 1], decks[i, 2]);
                Rail(decks[i, 0], decks[i, 1], decks[i, 2] + 1);
            }

            Tell(LoreKind.Graffiti, room, "Lore_ClimbArrow", "a seta de giz", 193, 78,
                "Uma seta a giz, subindo pela parede, e uma palavra só: \"ELEVADOR\".",
                "Embaixo, em outra letra: \"A escada caiu. Subam por aqui.\"");

            Decor(FixtureKind.Sign, 200, 79);
            Put(Crate(1), 190, 78);
            Lamp(194, 88, 0.7f);
            Lamp(203, 84, 0.7f);
            Lamp(203, 92, 0.7f);
            Lamp(198, 103, 0.8f);
            HangChain(198, 103, 8);
            HangChain(207, 103, 5);
            Fix(FixtureKind.GaugeBig, "Gauge_Climb_1", 205, 88);
            Fix(FixtureKind.GaugeBig, "Gauge_Climb_2", 191, 94);

            RunPipe(PipeLayer.Back, false, (198, 76), (198, 105));
            RunPipe(PipeLayer.Back, true, (204, 76), (204, 105));
            RunPipe(PipeLayer.Mid, false, (188, 101), (209, 101));
            Leaks.Add(new Leak(198.5f, 91f, 1f, 0f));
            Steam(room, Area.Between(190, 96, 207, 103), 0.10f);
            DripsIn(Area.Between(190, 78, 207, 103), 6, 151, false);
        }

        // ------------------------------------------------------------------
        // H. A Estacao do Elevador
        // ------------------------------------------------------------------

        /// <summary>
        /// O topo do distrito: a passarela dos inspetores, por cima da Galeria
        /// Alta, e a estacao, em cima do poco da Escadaria das Travas. A escada
        /// que subia da antessala chega aqui quebrada — um buraco no piso, com
        /// o guarda-corpo partido. Por ele se desce de volta. A cabine do
        /// elevador fica bem em cima do poco: sobe ate o alto, onde o latao vira
        /// cristal, e leva pro District 3.
        /// </summary>
        private static void BuildElevatorStation()
        {
            const string room = "H_ElevatorStation";
            Room(room, 104, 100, 189, 110);

            // A passarela dos inspetores.
            Carve(133, 100, 189, 103);
            Rail(133, 189, 100);
            Decor(FixtureKind.Poster, 146, 102);
            Decor(FixtureKind.Poster, 170, 102);
            Lamp(140, 103, 0.6f);
            Lamp(158, 103, 0.6f);
            Lamp(176, 103, 0.6f);
            RunPipe(PipeLayer.Back, false, (131, 102), (191, 102));

            // A estacao.
            Carve(104, 100, 132, 110);
            Solid(104, 108, 108, 110);
            Solid(128, 108, 132, 110);

            // O buraco da escada quebrada, a oeste da cabine.
            Carve(114, 99, 118, 99);
            Rail(112, 113, 100);
            Rail(119, 119, 100);
            Put(Debris(0), 113, 100);

            // A cabine e o poco dela, que sobe ate o alto do mapa.
            Carve(120, 111, 122, ElevatorTopY + 4 < Height - 1 ? (int)ElevatorTopY + 4 : Height - 2);
            Mark(room, "Exit_District3", District3Tile.X, District3Tile.Y);
            Fixtures.Add(new Fixture(FixtureKind.ElevatorRail, "Elevator_Rails", 120, 100, (int)ElevatorTopY + 4 - 100));

            // No alto do poco o latao acaba: e rocha azulada e cristal.
            MaterialZones.Add((Area.Between(114, 120, 128, Height - 1), Material.Crystal));
            Ore(Phase01Layout.BlueBand, 123, 124, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 118, 128, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 123, 132, PropLayer.Middle);
            Shine(121.5f, 126f, 6f, 0.8f, Glow.Crystal);
            Shine(121.5f, 134f, 6f, 1.0f, Glow.Crystal);

            Tell(LoreKind.Plaque, room, "Lore_ElevatorSign", "a placa do elevador", 126, 100,
                "Uma placa de latão ao lado da cabine: \"ELEVADOR DE SERVIÇO — DO DISTRITO DE LATÃO AO DISTRITO DE CRISTAL.\"",
                "Abaixo, um aviso pregado às pressas: \"ESCADARIA INTERDITADA. USE O ACESSO DAS CALDEIRAS.\"",
                "Alguém acrescentou, a giz: \"Que acesso?\"");

            Decor(FixtureKind.Sign, 119, 107);
            Fix(FixtureKind.GaugeBig, "Gauge_Station_1", 124, 104);
            Fix(FixtureKind.GaugeBig, "Gauge_Station_2", 110, 104);
            Decor(FixtureKind.Barrel, 128, 100);
            Decor(FixtureKind.Barrel, 129, 100);
            Put(Crate(0), 106, 100);
            Put(Crate(2), 107, 100);
            Put(Crate(1), 106, 101);
            Lamp(112, 110);
            Lamp(126, 110);
            HangChain(110, 110, 3);
            Furnace("Furnace_Station", 130, 100);

            RunPipe(PipeLayer.Back, true, (102, 106), (134, 106));
            RunPipe(PipeLayer.Back, false, (119, 99), (119, 112));
            Steam(room, Area.Between(104, 100, 132, 110), 0.08f);
        }

        // ------------------------------------------------------------------
        // Validacao
        // ------------------------------------------------------------------

        /// <summary>O que ja mudou no mapa, enquanto a validacao joga a fase na cabeca.</summary>
        private sealed class WorldState
        {
            public LevelMap Map;
            public readonly HashSet<string> TurnedValves = new HashSet<string>();
            public readonly HashSet<string> BurstBoilers = new HashSet<string>();
            public readonly HashSet<string> LiveVents = new HashSet<string>();
            public bool BossDown;
            public bool Purified;

            public LevelMap.Lift[] Lifts()
            {
                var lifts = new List<LevelMap.Lift>();

                foreach (Vent vent in Vents)
                {
                    if (vent.Kind != VentKind.Lift || !LiveVents.Contains(vent.Name)) continue;
                    lifts.Add(new LevelMap.Lift(vent.Zone.X, vent.Zone.Right, vent.Zone.Y, vent.Zone.Top));
                }

                return lifts.ToArray();
            }

            public LevelMap.Jet[] Jets()
            {
                var jets = new List<LevelMap.Jet>();

                foreach (Vent vent in Vents)
                {
                    if (vent.Kind != VentKind.Jet || !LiveVents.Contains(vent.Name)) continue;
                    jets.Add(new LevelMap.Jet(vent.Zone.X, vent.Zone.Right, vent.Zone.Y, vent.Zone.Top, vent.Direction));
                }

                return jets.ToArray();
            }
        }

        private static LevelMap.Lift[] CloudAreas()
        {
            var clouds = new List<LevelMap.Lift>();

            foreach ((string _, Area area) in Clouds)
            {
                clouds.Add(new LevelMap.Lift(area.X, area.Right, area.Y, area.Top));
            }

            return clouds.ToArray();
        }

        /// <summary>
        /// As regras de alcance desta fase: as com folga, mais o Steam Dash, que
        /// soma uns 3 tiles a um pulo.
        /// </summary>
        private static LevelMap.JumpRules Rules()
        {
            LevelMap.JumpRules rules = LevelMap.JumpRules.Comfortable();
            rules.MaxRun += 3;
            return rules;
        }

        /// <summary>
        /// Depois do boss: o Salto de Pressao, um segundo pulo no ar. Conta como
        /// no District 3 — sobe 4 tiles e vai uns 9 de lado, com o dash.
        /// </summary>
        private static LevelMap.JumpRules RulesAfterBoss()
        {
            LevelMap.JumpRules rules = LevelMap.JumpRules.Comfortable();
            rules.MaxRise = 4;
            rules.MaxRun += 5;
            return rules;
        }

        private static void Open(LevelMap map, Area area)
        {
            map.Fill(area.X, area.Y, area.Right, area.Top, LevelMap.Air);
        }

        /// <summary>
        /// Joga a fase na cabeca: explora, gira toda valvula e explode toda
        /// caldeira que alcancar, e repete com o mapa mudado, ate nada mais
        /// mudar. Devolve o estado final e o alcance nele.
        /// </summary>
        private static WorldState Play(out LevelMap.Reach reach, List<string> log, bool bossFalls = true,
            Func<string, bool> allowRoom = null, Action<WorldState, LevelMap.Reach, LevelMap.JumpRules> visit = null)
        {
            // Com allowRoom, so se mexe nas valvulas e caldeiras das salas
            // permitidas: e assim que se joga uma ala sem as outras.
            bool Allowed(string room) => allowRoom == null || allowRoom(room);

            var world = new WorldState { Map = Map.Clone() };

            // No comeco, portoes e paredes rachadas estao fechados, e so o vapor
            // que nao depende de valvula esta ligado.
            foreach (GateSpec gate in Gates)
            {
                world.Map.Fill(gate.Area.X, gate.Area.Y, gate.Area.Right, gate.Area.Top, LevelMap.Rock);
            }

            foreach ((string _, Area area) in BreakableWalls)
            {
                world.Map.Fill(area.X, area.Y, area.Right, area.Top, LevelMap.Rock);
            }

            // A nevoa densa e parede ate a purificacao.
            foreach ((string _, Area area) in Veils)
            {
                world.Map.Fill(area.X, area.Y, area.Right, area.Top, LevelMap.Rock);
            }

            foreach (Vent vent in Vents)
            {
                if (!vent.StartsOff) world.LiveVents.Add(vent.Name);
            }

            LevelMap.Lift[] clouds = CloudAreas();

            while (true)
            {
                // Depois do boss o Scrap tem o Salto de Pressao.
                LevelMap.JumpRules rules = world.BossDown ? RulesAfterBoss() : Rules();
                reach = world.Map.Explore(SpawnTile, rules, world.Lifts(), world.Jets(), clouds);
                visit?.Invoke(world, reach, rules);
                bool changed = false;

                foreach (ValveSpec valve in Valves)
                {
                    if (world.TurnedValves.Contains(valve.Name)) continue;
                    if (!Allowed(valve.Group)) continue;
                    if (!reach.CanStand(valve.StandX, valve.StandY)) continue;

                    world.TurnedValves.Add(valve.Name);
                    changed = true;
                    log?.Add($"gira {valve.Name}");

                    foreach (string vent in valve.Vents) world.LiveVents.Add(vent);

                    foreach (string pool in valve.Pools)
                    {
                        foreach ((string name, Area area) in AcidPools)
                        {
                            if (name != pool) continue;

                            // Seca: o que era acido vira ar. O que era degrau continua degrau.
                            for (int y = area.Y; y <= area.Top; y++)
                            {
                                for (int x = area.X; x <= area.Right; x++)
                                {
                                    if (world.Map.IsAcid(x, y)) world.Map.Set(x, y, LevelMap.Air);
                                }
                            }
                        }
                    }

                    foreach (string gateName in valve.Gates)
                    {
                        foreach (GateSpec gate in Gates)
                        {
                            if (gate.Name == gateName) Open(world.Map, gate.Area);
                        }
                    }
                }

                foreach (BoilerSpec boiler in PressureBoilers)
                {
                    if (world.BurstBoilers.Contains(boiler.Name)) continue;
                    if (!Allowed(boiler.Group)) continue;
                    if (!reach.CanStand(boiler.StandX, boiler.StandY)) continue;

                    world.BurstBoilers.Add(boiler.Name);
                    changed = true;
                    log?.Add($"explode {boiler.Name}");

                    foreach (string wallName in boiler.Walls)
                    {
                        foreach ((string name, Area area) in BreakableWalls)
                        {
                            if (name == wallName) Open(world.Map, area);
                        }
                    }
                }

                if (bossFalls && !world.BossDown && reach.CanStand(BossTile.X, BossTile.Y))
                {
                    world.BossDown = true;
                    changed = true;
                    log?.Add("derrota o Boiler Master");

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Opener == GateOpener.Boss) Open(world.Map, gate.Area);
                    }

                    // A queda dele purifica o distrito na hora.
                    world.Purified = true;
                    log?.Add("purifica o distrito");

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Opener == GateOpener.Purification) Open(world.Map, gate.Area);
                    }

                    foreach ((string _, Area area) in Veils) Open(world.Map, area);
                }

                if (!changed) return world;
            }
        }

        /// <summary>
        /// Confere se a fase e jogavel de verdade, sem precisar jogar.
        ///
        /// Simula a fase inteira (ver <see cref="Play"/>): parte de onde o
        /// Scrap chega, com tudo fechado, e so abre o que ele consegue alcancar
        /// pra abrir. No fim, toda valvula tem que ter sido girada, toda
        /// caldeira explodida, o boss alcancado, o distrito purificado e todo
        /// marcador visitado. E, do fim do mapa, tem que dar pra voltar ao comeco.
        ///
        /// Devolve a lista de problemas. Vazia quer dizer que esta tudo certo.
        /// </summary>
        public static List<string> Validate()
        {
            var problems = new List<string>();

            // O mapa e fechado: se a borda tiver ar, a camera mostra o vazio.
            for (int x = 0; x < Width; x++)
            {
                if (!Map.IsRock(x, 0) || !Map.IsRock(x, Height - 1))
                {
                    problems.Add($"A coluna {x} tem buraco no chao ou no teto do mapa.");
                }
            }

            for (int y = 0; y < Height; y++)
            {
                if (!Map.IsRock(0, y) || !Map.IsRock(Width - 1, y))
                {
                    problems.Add($"A linha {y} tem buraco numa das paredes do mapa.");
                }
            }

            if (!Map.IsStandable(SpawnTile.X, SpawnTile.Y))
            {
                problems.Add($"O Scrap chega em ({SpawnTile.X}, {SpawnTile.Y}) e nao ha chao ali.");
                return problems;
            }

            WorldState world = Play(out LevelMap.Reach reach, null);

            foreach (ValveSpec valve in Valves)
            {
                if (!world.Map.IsStandable(valve.StandX, valve.StandY))
                {
                    problems.Add($"{valve.Name}: ({valve.StandX}, {valve.StandY}) nao e um lugar onde da pra ficar em pe pra raspar.");
                }
                else if (!world.TurnedValves.Contains(valve.Name))
                {
                    problems.Add($"{valve.Name} ({valve.StandX}, {valve.StandY}) nunca e alcancada.");
                }

                foreach (string vent in valve.Vents)
                {
                    if (!Vents.Exists(candidate => candidate.Name == vent)) problems.Add($"{valve.Name} liga '{vent}', que nao existe.");
                }

                foreach (string pool in valve.Pools)
                {
                    if (!AcidPools.Exists(candidate => candidate.Name == pool)) problems.Add($"{valve.Name} seca '{pool}', que nao existe.");
                }

                foreach (string gate in valve.Gates)
                {
                    if (!Gates.Exists(candidate => candidate.Name == gate)) problems.Add($"{valve.Name} abre '{gate}', que nao existe.");
                }
            }

            foreach (Vent vent in Vents)
            {
                if (vent.StartsOff && !world.LiveVents.Contains(vent.Name))
                {
                    problems.Add($"{vent.Name} comeca desligado e nenhuma valvula alcancavel o liga.");
                }

                if (!vent.StartsOff) continue;

                // Uma coluna desligada tem que ter a valvula a vista: na mesma
                // sala. Se a valvula fica longe, ela avisa no HUD o que ligou.
                string ventRoom = RoomAt(vent.Zone.X, vent.Zone.Y);

                foreach (ValveSpec valve in Valves)
                {
                    if (Array.IndexOf(valve.Vents, vent.Name) < 0) continue;
                    if (valve.Group == ventRoom || !string.IsNullOrEmpty(valve.Notice)) continue;

                    problems.Add($"{vent.Name} ({ventRoom}) so liga pela {valve.Name}, em {valve.Group}, longe e sem aviso.");
                }
            }

            foreach (GateSpec gate in Gates)
            {
                if (gate.Opener == GateOpener.Valve && !Valves.Exists(valve => Array.IndexOf(valve.Gates, gate.Name) >= 0))
                {
                    problems.Add($"{gate.Name} espera uma valvula, e nenhuma o abre.");
                }
            }

            foreach (BoilerSpec boiler in PressureBoilers)
            {
                if (!world.BurstBoilers.Contains(boiler.Name))
                {
                    problems.Add($"{boiler.Name} ({boiler.StandX}, {boiler.StandY}) nunca e alcancada.");
                }

                foreach (string wall in boiler.Walls)
                {
                    if (!BreakableWalls.Exists(candidate => candidate.Name == wall)) problems.Add($"{boiler.Name} derruba '{wall}', que nao existe.");
                }
            }

            if (!world.BossDown) problems.Add("Nao da pra chegar no Boiler Master.");

            // A saida e so pela sala do boss: sem derrota-lo, nada da fase
            // (nem caminho alternativo) chega no tunel da saida.
            Play(out LevelMap.Reach withoutBoss, null, bossFalls: false);
            if (withoutBoss.CanStand(District3Tile.X, District3Tile.Y))
            {
                problems.Add("Da pra chegar na saida pros Silent Geodes sem passar pelo Boiler Master.");
            }

            CheckArena(world, problems);
            CheckQuietSpots(reach, problems);
            CheckEveryRoomPays(problems);
            CheckPools(problems);
            CheckNoTraps(problems);
            CheckWings(problems);
            CheckReturnAfterBoss(world, problems);
            CheckBrokenStairway(world, problems);

            int gems = Rewards.FindAll(reward => reward.Kind == RewardKind.Gem).Count;
            foreach (LoreSpec spec in Lore)
            {
                if (spec.Quest != null) gems += spec.Quest.GemReward;
            }

            if (gems != 2) problems.Add($"A fase tem {gems} gemas; tem que ter exatamente 2. Gema e rara.");

            // O mirante da Galeria Alta e o desafio de depois do boss: sem o
            // Salto de Pressao, nao se chega nele.
            LevelMap.Reach withoutJump = world.Map.Explore(SpawnTile, Rules(), world.Lifts(), world.Jets(), CloudAreas());
            foreach (RewardSpec reward in Rewards)
            {
                if (reward.Id == "D2_Gallery_Cache" && withoutJump.CanStand(reward.X, reward.Y))
                {
                    problems.Add("Da pra chegar no mirante da Galeria Alta sem o Salto de Pressao: o desafio nao cobra nada.");
                }
            }

            foreach (Marker marker in Markers)
            {
                if (!marker.MustReach) continue;

                if (!world.Map.IsStandable(marker.X, marker.Y))
                {
                    problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) nao e um lugar onde da pra ficar em pe.");
                }
                else if (!reach.CanStand(marker.X, marker.Y))
                {
                    problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) esta fora de alcance.");
                }
            }

            if (!reach.CanStand(CrisolTile.X, CrisolTile.Y)) problems.Add("Nao da pra chegar em Crisol.");

            foreach (EnemySpec enemy in Enemies)
            {
                if (enemy.Kind == EnemyKind.ValveSentry)
                {
                    if (!Map.IsStandable(enemy.X0, enemy.Y)) problems.Add($"{enemy.Name} esta em ({enemy.X0}, {enemy.Y}), que nao e chao.");
                    continue;
                }

                for (int x = enemy.X0; x <= enemy.X1; x++)
                {
                    if (!Map.IsStandable(x, enemy.Y)) problems.Add($"{enemy.Name} anda por ({x}, {enemy.Y}), que nao e chao.");
                }
            }

            // A volta: do fim do mapa ate onde se chega. A fase nao pode prender ninguem.
            if (Map.IsStandable(District3Tile.X, District3Tile.Y))
            {
                LevelMap.Reach backward = world.Map.Explore(District3Tile, Rules(),
                    world.Lifts(), world.Jets(), CloudAreas());

                if (!backward.CanStand(SpawnTile.X, SpawnTile.Y))
                {
                    problems.Add("Saindo do fim do mapa nao da pra voltar ate a Galeria de Transicao.");
                }
            }

            foreach (Vent vent in Vents)
            {
                for (int y = vent.Zone.Y; y <= vent.Zone.Top; y++)
                {
                    for (int x = vent.Zone.X; x <= vent.Zone.Right; x++)
                    {
                        if (Map.IsSolid(x, y)) problems.Add($"{vent.Name} passa por dentro de terreno em ({x}, {y}).");
                    }
                }

                if (vent.Zone.Contains(SpawnTile.X, SpawnTile.Y)) problems.Add($"O Scrap chega dentro de {vent.Name}.");
            }

            foreach (Fixture fixture in Fixtures)
            {
                if (fixture.X < 0 || fixture.X >= Width || fixture.Y < 0 || fixture.Y >= Height)
                {
                    problems.Add($"{fixture.Name} esta fora do mapa, em ({fixture.X}, {fixture.Y}).");
                }

                // Decoracao nao flutua: lampiao pende de algo, e o que fica no
                // chao esta apoiado nele.
                switch (fixture.Kind)
                {
                    case FixtureKind.Lamp:
                        if (!Map.IsSolid(fixture.X, fixture.Y + 2) || Map.IsRock(fixture.X, fixture.Y))
                        {
                            problems.Add($"{fixture.Name} ({fixture.X}, {fixture.Y}) nao esta pendurado em nada.");
                        }
                        break;

                    case FixtureKind.Locker:
                    case FixtureKind.Barrel:
                    case FixtureKind.Boiler:
                    case FixtureKind.Furnace:
                        if (!Map.IsSolid(fixture.X, fixture.Y - 1) || Map.IsRock(fixture.X, fixture.Y))
                        {
                            problems.Add($"{fixture.Name} ({fixture.X}, {fixture.Y}) nao esta apoiado no chao.");
                        }
                        break;
                }
            }

            return problems;
        }

        /// <summary>
        /// A arena do Mestre das Caldeiras, como a do Coletor no District 1:
        /// durante a luta (o selo da entrada e a grade da saida fechados) nao
        /// se sai dela; as plataformas se alcancam do chao; e nao ha patamar na
        /// altura da cabeca dele fora de onde o corpo dele passa.
        /// </summary>
        private static void CheckArena(WorldState world, List<string> problems)
        {
            LevelMap fight = world.Map.Clone();

            Area seal = ArenaSeal;
            fight.Fill(seal.X, seal.Y, seal.Right, seal.Top, LevelMap.Rock);

            foreach (GateSpec gate in Gates)
            {
                if (gate.Name == BossGate) fight.Fill(gate.Area.X, gate.Area.Y, gate.Area.Right, gate.Area.Top, LevelMap.Rock);
            }

            LevelMap.Reach inFight = fight.Explore(BossTile, Rules());
            Area arena = ArenaInside;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (!arena.Contains(x, y) && inFight.CanStand(x, y))
                    {
                        problems.Add($"Durante a luta da pra sair da arena: ({x}, {y}).");
                        return;
                    }
                }
            }

            int floor = BossTile.Y;
            float reachLeft = BossRange.X0 + 0.5f - BossHalfWidth;
            float reachRight = BossRange.X1 + 0.5f + BossHalfWidth;

            for (int y = floor + 1; y <= floor + 3; y++)
            {
                for (int x = arena.X; x <= arena.Right; x++)
                {
                    bool underBoss = x + 1f > reachLeft && x < reachRight;
                    if (fight.IsStandable(x, y) && !underBoss)
                    {
                        problems.Add($"Ha um patamar na altura da cabeca do Mestre das Caldeiras, fora do alcance dele, em ({x}, {y}).");
                    }
                }
            }

            foreach (LevelMap.Perch perch in fight.FindPerches())
            {
                if (perch.Y <= floor + 3 || !arena.Contains(perch.X, perch.Y) || !arena.Contains(perch.Right, perch.Y)) continue;

                bool reached = false;
                for (int x = perch.X; x <= perch.Right && !reached; x++) reached = inFight.CanStand(x, perch.Y);

                if (!reached) problems.Add($"A plataforma da arena em {perch} nao se alcanca do chao durante a luta.");
            }
        }

        /// <summary>
        /// Os lugares onde o Scrap para pra fazer alguma coisa: raspar uma
        /// valvula, ler uma lore, falar com alguem, pegar uma recompensa,
        /// sentar na bancada. Nenhum desses pode ficar na linha de tiro de uma
        /// sentinela — ninguem le uma carta levando jato de vapor nas costas.
        /// Quem anda pode passar por eles (todos morrem); so as bancadas e o
        /// Crisol ficam fora do caminho.
        /// </summary>
        private static List<(string Name, int X, int Y)> QuietSpots()
        {
            var spots = new List<(string, int, int)>();

            foreach (ValveSpec valve in Valves)
            {
                // As da arena sao raspadas no meio da luta, de proposito.
                if (!valve.Arena) spots.Add((valve.Name, valve.StandX, valve.StandY));
            }

            // Quem so aparece depois da purificacao nao conta: os corrompidos
            // somem com ela.
            foreach (LoreSpec spec in Lore)
            {
                if (!spec.AfterPurification) spots.Add((spec.Name, spec.X, spec.Y));
            }
            foreach (RewardSpec spec in Rewards) spots.Add((spec.Id, spec.X, spec.Y));
            foreach ((string name, int x, int y) in Workbenches) spots.Add((name, x + 1, y));
            spots.Add(("Crisol", CrisolTile.X, CrisolTile.Y));
            return spots;
        }

        /// <summary>Se o lugar calmo e de descanso (bancada ou Crisol), e nao so de parar.</summary>
        private static bool RestSpot(string name)
        {
            if (name == "Crisol") return true;
            foreach (var (bench, _, _) in Workbenches)
            {
                if (bench == name) return true;
            }

            return false;
        }

        /// <summary>
        /// Confere os lugares calmos (ver <see cref="QuietSpots"/>) e que toda
        /// sentinela pode ser morta: tem chao do lado dela onde o Scrap alcanca.
        /// </summary>
        private static void CheckQuietSpots(LevelMap.Reach reach, List<string> problems)
        {
            const int margin = 2;

            foreach ((string name, int x, int y) in QuietSpots())
            {
                foreach (EnemySpec enemy in Enemies)
                {
                    if (enemy.Kind == EnemyKind.ValveSentry)
                    {
                        // O jato sai na altura do peito: pega quem esta no
                        // mesmo chao, ou um degrau abaixo. Para na rocha.
                        if (y != enemy.Y && y != enemy.Y - 1) continue;

                        int step = enemy.Direction > 0 ? 1 : -1;
                        bool hit = Math.Abs(x - enemy.X0) <= 1;

                        for (int i = 1; i <= enemy.Range && !hit; i++)
                        {
                            int cx = enemy.X0 + step * i;
                            if (Map.IsRock(cx, enemy.Y)) break;
                            if (cx == x) hit = true;
                        }

                        if (hit) problems.Add($"{name} ({x}, {y}) fica na linha de tiro de {enemy.Name}.");
                        continue;
                    }

                    // Quem anda (ou cai do teto e anda) usa o chao inteiro: todos
                    // morrem, entao podem passar por valvula, lore e recompensa.
                    // So as bancadas e o Crisol ficam fora: ali o Scrap descansa e compra.
                    if (!RestSpot(name)) continue;
                    if (y == enemy.Y && x >= enemy.X0 - margin && x <= enemy.X1 + margin)
                    {
                        problems.Add($"{name} ({x}, {y}) fica no caminho de {enemy.Name}.");
                    }
                }
            }

            // E quem anda nao atravessa sentinela, caldeira de pressao, portao
            // nem parede rachada, e comeca dentro do proprio trecho.
            foreach (EnemySpec enemy in Enemies)
            {
                if (enemy.Kind == EnemyKind.ValveSentry) continue;

                if (enemy.StartX >= 0 && (enemy.StartX < enemy.X0 || enemy.StartX > enemy.X1))
                {
                    problems.Add($"{enemy.Name} comeca em {enemy.StartX}, fora do trecho dele ({enemy.X0} a {enemy.X1}).");
                }

                foreach (EnemySpec sentry in Enemies)
                {
                    if (sentry.Kind != EnemyKind.ValveSentry || sentry.Y != enemy.Y) continue;
                    if (sentry.X0 >= enemy.X0 - 1 && sentry.X0 <= enemy.X1 + 1)
                    {
                        problems.Add($"{enemy.Name} anda por cima de {sentry.Name}.");
                    }
                }

                foreach (BoilerSpec boiler in PressureBoilers)
                {
                    bool level = enemy.Y >= boiler.Y && enemy.Y < boiler.Y + 6;
                    if (level && boiler.X <= enemy.X1 && boiler.X + 3 >= enemy.X0)
                    {
                        problems.Add($"{enemy.Name} atravessa a caldeira {boiler.Name}.");
                    }
                }

                var closed = new List<(string Name, Area Area)>(BreakableWalls);
                foreach (GateSpec gate in Gates) closed.Add((gate.Name, gate.Area));
                foreach ((string closedName, Area area) in closed)
                {
                    bool level = enemy.Y >= area.Y - 1 && enemy.Y <= area.Top;
                    if (level && area.X <= enemy.X1 && area.Right >= enemy.X0)
                    {
                        problems.Add($"{enemy.Name} atravessa {closedName}.");
                    }
                }
            }

            foreach (EnemySpec enemy in Enemies)
            {
                if (enemy.Kind != EnemyKind.ValveSentry) continue;

                bool killable = reach.CanStand(enemy.X0 - 1, enemy.Y) || reach.CanStand(enemy.X0 + 1, enemy.Y);

                if (!killable) problems.Add($"{enemy.Name} ({enemy.X0}, {enemy.Y}) nao tem onde o Scrap pare do lado pra bater nela.");
            }
        }

        /// <summary>
        /// Toda poca de acido tem que estar encaixada: so acido por dentro (o
        /// objeto da poca desenha e queima o retangulo inteiro), parede dos
        /// dois lados e chao embaixo. Senao o acido aparece — e machuca — em
        /// cima de onde se pisa, ou fica pendurado no ar.
        /// </summary>
        private static void CheckPools(List<string> problems)
        {
            foreach ((string name, Area area) in AcidPools)
            {
                bool full = true;
                bool sides = true;
                bool floor = true;

                for (int y = area.Y; y <= area.Top; y++)
                {
                    for (int x = area.X; x <= area.Right; x++)
                    {
                        if (!Map.IsAcid(x, y)) full = false;
                    }

                    if (!Map.IsSolid(area.X - 1, y) || !Map.IsSolid(area.Right + 1, y)) sides = false;
                }

                for (int x = area.X; x <= area.Right; x++)
                {
                    if (!Map.IsSolid(x, area.Y - 1)) floor = false;
                }

                if (!full) problems.Add($"{name}: o retangulo da poca tem terreno dentro; o acido vai aparecer por cima dele.");
                if (!sides) problems.Add($"{name}: falta parede de um dos lados da poca.");
                if (!floor) problems.Add($"{name}: falta chao embaixo da poca.");
            }
        }

        /// <summary>
        /// Nenhuma armadilha: em cada momento da fase (a cada valvula girada),
        /// de todo chao alcancavel da pra voltar ate Crisol sem girar mais nada.
        /// Coluna que so sobe, alcapao que so desce e portao que so abre no fim
        /// nao podem prender ninguem. So a arena fica de fora: durante a luta ela
        /// tranca de proposito (ver CheckArena).
        /// </summary>
        private static void CheckNoTraps(List<string> problems)
        {
            Area[] exempt =
            {
                ArenaInside,
            };

            var reported = new HashSet<string>();

            Play(out _, null, true, null, (world, reach, rules) =>
            {
                // Antes de chegar no centro, nao ha pra onde voltar ainda.
                if (!reach.CanStand(CrisolTile.X, CrisolTile.Y)) return;

                LevelMap.Lift[] lifts = world.Lifts();
                LevelMap.Jet[] jets = world.Jets();
                LevelMap.Lift[] clouds = CloudAreas();

                foreach (LevelMap.Perch perch in world.Map.FindPerches())
                {
                    int x = -1;
                    for (int cx = perch.X; cx <= perch.Right; cx++)
                    {
                        if (reach.CanStand(cx, perch.Y)) { x = cx; break; }
                    }

                    if (x < 0) continue;

                    bool skip = false;
                    foreach (Area area in exempt) skip |= area.Contains(x, perch.Y);
                    if (skip) continue;

                    string room = RoomAt(x, perch.Y) ?? "fora das salas";
                    string key = $"{room}:{perch}";
                    if (reported.Contains(key)) continue;

                    LevelMap.Reach back = world.Map.Explore((x, perch.Y), rules, lifts, jets, clouds);
                    if (back.CanStand(CrisolTile.X, CrisolTile.Y)) continue;

                    reported.Add(key);
                    problems.Add($"Armadilha em {room}, {perch}: dali nao da pra voltar pra Cisterna sem girar mais nada.");
                }
            });
        }

        /// <summary>
        /// Salas de passagem: so se atravessam, sem nada a achar. Todas as
        /// outras tem que ter pelo menos uma lore, um NPC ou uma recompensa.
        /// </summary>
        private static readonly string[] PassageRooms =
        {
            "01_Gallery", "02_FirstPipes", "09_BoilerRoom",
        };

        /// <summary>Toda sala que pede esforco paga: lore, alguem pra ouvir, ou recompensa.</summary>
        private static void CheckEveryRoomPays(List<string> problems)
        {
            foreach ((string name, Area area) in Rooms)
            {
                if (Array.IndexOf(PassageRooms, name) >= 0) continue;

                bool pays = Lore.Exists(spec => area.Contains(spec.X, spec.Y))
                    || Rewards.Exists(spec => area.Contains(spec.X, spec.Y))
                    || area.Contains(CrisolTile.X, CrisolTile.Y);

                if (!pays) problems.Add($"A sala {name} nao tem nada pra achar: nem lore, nem NPC, nem recompensa.");
            }
        }

        /// <summary>
        /// A ordem livre das alas: cada uma se completa partindo do centro sem
        /// nada das outras duas, e o boss nao se alcanca se faltar qualquer uma.
        /// </summary>
        private static void CheckWings(List<string> problems)
        {
            foreach ((string wing, string[] _) in Wings)
            {
                List<ValveSpec> locks = Valves.FindAll(valve => valve.Lock && WingOf(valve.Group) == wing);
                if (locks.Count != 1) problems.Add($"A ala {wing} tem {locks.Count} valvulas de trava; tem que ter uma.");

                // Sozinha: so a entrada, o centro e esta ala.
                WorldState alone = Play(out _, null, false, room => WingOf(room) == null || WingOf(room) == wing);

                foreach (ValveSpec valve in Valves)
                {
                    if (WingOf(valve.Group) == wing && !alone.TurnedValves.Contains(valve.Name))
                    {
                        problems.Add($"Ala {wing} feita sozinha: {valve.Name} nao se alcanca sem outra ala.");
                    }
                }

                foreach (BoilerSpec boiler in PressureBoilers)
                {
                    if (WingOf(boiler.Group) == wing && !alone.BurstBoilers.Contains(boiler.Name))
                    {
                        problems.Add($"Ala {wing} feita sozinha: {boiler.Name} nao se alcanca sem outra ala.");
                    }
                }

                // Sem ela: o boss tem que ficar fora de alcance.
                WorldState without = Play(out _, null, true, room => WingOf(room) != wing);
                if (without.BossDown) problems.Add($"Da pra chegar no Boiler Master sem fazer a ala {wing}.");
            }

            int lockCount = Valves.FindAll(valve => valve.Lock).Count;
            if (lockCount != Wings.Length) problems.Add($"Ha {lockCount} valvulas de trava; tem que haver {Wings.Length}.");
        }

        /// <summary>
        /// Depois do boss, da saida (e da Galeria Alta, por cima dela) tem que
        /// dar pra voltar pra Praca e pra Cisterna: a Solda e as falas novas
        /// ficam la.
        /// </summary>
        private static void CheckReturnAfterBoss(WorldState world, List<string> problems)
        {
            LevelMap.Reach back = world.Map.Explore(District3Tile, RulesAfterBoss(), world.Lifts(), world.Jets(), CloudAreas());

            LoreSpec solda = Lore.Find(spec => spec.Name == "Npc_Solda");
            if (solda != null && !back.CanStand(solda.X, solda.Y))
            {
                problems.Add("Da saida, depois do boss, nao da pra voltar ate a Solda na Praca.");
            }

            if (!back.CanStand(CrisolTile.X, CrisolTile.Y))
            {
                problems.Add("Da saida, depois do boss, nao da pra voltar ate Crisol.");
            }
        }

        /// <summary>
        /// A escadaria quebrada nao pode servir de caminho: com a porta da
        /// subida tampada, nem com o Salto de Pressao se chega no elevador.
        /// </summary>
        private static void CheckBrokenStairway(WorldState world, List<string> problems)
        {
            LevelMap blocked = world.Map.Clone();
            blocked.Fill(189, 78, 189, 81, LevelMap.Rock);

            LevelMap.Reach reach = blocked.Explore(SpawnTile, RulesAfterBoss(), world.Lifts(), world.Jets(), CloudAreas());
            if (reach.CanStand(District3Tile.X, District3Tile.Y))
            {
                problems.Add("Da pra chegar no elevador sem a subida da direita: a escadaria quebrada esta servindo de caminho.");
            }
        }

        /// <summary>A ordem em que a validacao abriu a fase. Serve pra conferir o desenho.</summary>
        public static List<string> Walkthrough()
        {
            var log = new List<string>();
            Play(out _, log);
            return log;
        }

        private static bool TouchesAny(LevelMap.Reach reach, Area area)
        {
            for (int y = area.Y; y <= area.Top; y++)
            {
                for (int x = area.X; x <= area.Right; x++)
                {
                    if (reach.CanTouch(x, y)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Os trechos de chao onde nao da pra chegar nem com a fase toda
        /// aberta. Nao e erro — tampo de caldeira e canto de teto entram aqui —
        /// mas vale olhar a lista quando mexer no mapa: um trecho grande nela e
        /// sala isolada.
        /// </summary>
        public static List<string> UnreachableFloors()
        {
            WorldState world = Play(out LevelMap.Reach reach, null);
            var floors = new List<string>();

            foreach (LevelMap.Perch perch in world.Map.FindPerches())
            {
                if (perch.Length < 2) continue;

                bool reached = false;
                for (int x = perch.X; x <= perch.Right && !reached; x++)
                {
                    reached = reach.CanStand(x, perch.Y);
                }

                if (!reached) floors.Add(perch.ToString());
            }

            return floors;
        }

        /// <summary>O mapa em texto, de cima pra baixo, pra conferir no olho.</summary>
        public static string[] ToRows() => Map.ToRows();
    }
}
