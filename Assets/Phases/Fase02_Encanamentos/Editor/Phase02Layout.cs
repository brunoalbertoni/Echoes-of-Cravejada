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
    public static partial class Phase02Layout
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

            /// <summary>Caixote de peca da Coroa, 1x1. Empilha.</summary>
            Crate,

            /// <summary>Caixa de ferramentas de operario, aberta, 1x1. Empilha em caixote.</summary>
            Toolbox,

            /// <summary>Palete com canos novos amarrados, que nunca foram instalados, 2x1.</summary>
            Pallet,

            /// <summary>Monte de carvao das fornalhas, 2x1, com a pa fincada.</summary>
            CoalPile,

            /// <summary>Saco de areia de vedacao, 1x1. Empilha.</summary>
            Sack,

            /// <summary>Carrinho de carvao, 2x2, de mao, tombado de lado.</summary>
            Cart,
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
        /// da obra abandonada do District 1. No mapa e ar; quem fecha e o objeto.
        /// </summary>
        public static readonly List<(string Name, Area Area)> Veils = new List<(string, Area)>();

        /// <summary>Onde Crisol fica.</summary>
        public static (int X, int Y) CrisolTile => (90, 25);

        public static readonly string[] CrisolLines =
        {
            "Hum. Mais um pedaço de sucata que desceu pelos canos errados.",
            "Eu sou Crisol, o Lapidário. Já encaixei pedra em coroa de rei. Hoje conserto lata velha.",
            "Esse aro de prata que você carrega... caiu do Coletor, né? Me dá isso aqui. Você nem sabe o que é.",
            "É um engaste. Serve pra prender uma gema no corpo. Toma: um rubi bruto. Vamos ver se essa sucata aguenta uma pedra de verdade.",
            "Leva até uma bancada e encaixa ele no cinzel. Seus golpes vão sair em brasa.",
            "Se quiser que eu reforce essa lataria, me traz fragmentos. Eu não trabalho de graça.",
            "Gemas lapidadas também servem. Dessas eu gosto. Por uma delas, até conto uma história.",
        };

        /// <summary>O que ele diz das outras vezes, antes de abrir a loja.</summary>
        public static readonly string[] CrisolRepeatLines =
        {
            "Você de novo. Fala logo o que quer.",
        };

        /// <summary>O que ele diz depois da purificacao.</summary>
        public static readonly string[] CrisolPurifiedLines =
        {
            "Sentiu? O vapor tá limpo. Fazia anos que essa cisterna só cheirava a verde.",
            "Não pense que isso me comove. ...Mas a bancada tá funcionando melhor. Fala o que quer.",
        };

        /// <summary>A memoria que ele vende por uma gema: como lapidou a coroa do Rei de Prata.</summary>
        public static readonly string[] CrisolMemoryLines =
        {
            "Quer saber da Coroa de Facetas? Então escuta e não interrompe.",
            "O Rei de Prata me deu um bloco de quartzo do tamanho da minha cabeça e mandou: novecentas e vinte e cinco facetas. Uma pra cada Nobre.",
            "Levei três anos. Cada faceta tinha que refletir a luz de todas as outras. Se uma ficasse torta, a coroa inteira apagava.",
            "No dia da coroação, a luz de Cravejada inteira passou por ela. A cidade acendeu de ponta a ponta. Eu vi lá do fundo do salão, em pé, atrás dos Nobres.",
            "Ele nem olhou pra mim. Ficou olhando o próprio reflexo, repetido novecentas e vinte e cinco vezes.",
            "...Foi a coisa mais bonita que eu já fiz. E fiz pra alguém que só sabia olhar pra si mesmo.",
        };

        /// <summary>A area de onde o acido do poco de fuga parte, e ate que altura ele sobe.</summary>

        /// <summary>Onde o Boiler Master fica, ate onde ele anda e a area que o acorda.</summary>
        public static (int X, int Y) BossTile => (149, 59);

        /// <summary>
        /// Ate onde ele anda: a sala inteira, da grade da entrada (x 131) a
        /// grade da saida (x 180). Nao sobra canto onde ele nao chegue.
        /// </summary>
        public static (int X0, int X1) BossRange => (133, 178);

        /// <summary>Metade da largura do corpo dele (o colisor tem 3.0, puxado pra frente).</summary>
        public const float BossHalfWidth = 1.5f;

        /// <summary>
        /// As plataformas baixas da arena, a um tile do chao: colisao so numa
        /// tabua fina no topo (ver Phase01Generator.CreateThinSteps), pra o
        /// Scrap passar por baixo sem travar. Por cima continuam degrau.
        /// </summary>
        public static readonly (int X0, int X1, int Y)[] ThinSteps = { (137, 138, 60), (172, 173, 60) };

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

            Furnish();
        }

        /// <summary>
        /// O que cada sala tem largado pelo chao, pela historia dela. Fora da
        /// lista, a sala fica como esta: o poco dos Primeiros Canos e a Escalada
        /// sao so subida, e a Sala das Caldeiras e a arena. O Deposito nao tem
        /// caixote: a ficha diz que as caixas sairam as pressas, so ficaram
        /// sacos e um palete esquecido.
        /// </summary>
        private static readonly Dictionary<string, FixtureKind[]> Clutter = new Dictionary<string, FixtureKind[]>
        {
            ["01_Gallery"] = new[] { FixtureKind.Sack, FixtureKind.Crate, FixtureKind.Locker },
            ["03_StuckValve"] = new[] { FixtureKind.Toolbox, FixtureKind.Pallet },
            ["04_Cistern"] = new[] { FixtureKind.Sack, FixtureKind.Crate },
            ["05_WelderHall"] = new[] { FixtureKind.Toolbox, FixtureKind.Locker, FixtureKind.Crate, FixtureKind.Pallet },
            ["06_SteamChain"] = new[] { FixtureKind.Sack },
            ["07_ValveMaze"] = new[] { FixtureKind.Toolbox, FixtureKind.Sack },
            ["08_PreBoss"] = new[] { FixtureKind.CoalPile, FixtureKind.Cart },
            ["A_Sewer"] = new[] { FixtureKind.Sack, FixtureKind.Toolbox },
            ["B_MachineHall"] = new[] { FixtureKind.CoalPile, FixtureKind.Cart, FixtureKind.Toolbox },
            ["C_GaugeAttic"] = new[] { FixtureKind.Toolbox, FixtureKind.Locker, FixtureKind.Crate },
            ["D_Depot"] = new[] { FixtureKind.Sack, FixtureKind.Pallet },
            ["E_ValveTower"] = new[] { FixtureKind.Toolbox, FixtureKind.Sack },
            ["F_HighGallery"] = new[] { FixtureKind.Pallet, FixtureKind.Crate, FixtureKind.Sack },
            ["H_ElevatorStation"] = new[] { FixtureKind.Crate, FixtureKind.Pallet, FixtureKind.Sack },
        };
        /// <summary>O fogo animado do pack (objects_misc_animated, linha 3).</summary>
        public const string FireTile = "furnace_fire";
        private static int _decor;
        /// <summary>
        /// Salas de passagem: so se atravessam, sem nada a achar. Todas as
        /// outras tem que ter pelo menos uma lore, um NPC ou uma recompensa.
        /// </summary>
        private static readonly string[] PassageRooms =
        {
            "01_Gallery", "02_FirstPipes", "09_BoilerRoom",
        };
    }
}
