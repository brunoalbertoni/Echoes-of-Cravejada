using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O desenho da Phase 2 — District 2: The Brass Pipelines.
    ///
    /// O sistema circulatorio de Cravejada. Nao e um corredor: e uma rede de
    /// dezessete salas onde quase toda sala leva a mais de uma.
    ///
    /// O caminho principal, que e o do documento de design:
    ///   1  Galeria de Transicao     a sucata vira latao; o primeiro polimento
    ///   2  Os Primeiros Canos       tres colunas de vapor pra subir, sem perigo
    ///   3  A Valvula Emperrada      uma poca de acido; a valvula seca
    ///   4  Cisterna de Crisol       sala segura: bancada, Crisol e a loja
    ///   5  Salao dos Welders        combate, com um caminho por cima dos canos
    ///   6  A Cadeia de Vapor        um lago de acido cruzado de coluna em jato
    ///   7  O Labirinto de Valvulas  tres valvulas em sequencia ligam o elevador
    ///   8  Workbench pre-boss       respiro antes da arena
    ///   9  A Sala de Caldeiras      a arena do Boiler Master
    ///   10 O Acido Ascendente       a fuga, com o acido subindo atras
    ///   11 Saida pros Silent Geodes o latao vira cristal
    ///
    /// E as areas fora dele, que ligam umas salas nas outras:
    ///   A  O Esgoto                 por baixo de tudo: cisterna, primeiros canos e salao
    ///   B  A Casa de Maquinas       tres andares de passarela por cima do salao
    ///   C  A Galeria dos Manometros o sotao, com um atalho de volta pro comeco
    ///   D  O Deposito               embaixo do labirinto, ate o lago e a torre
    ///   E  A Torre das Valvulas     quatro andares, um por valvula, do deposito ao alto
    ///   F  A Galeria Alta           por cima da arena, da torre ao poco de fuga
    ///
    /// Diferente da Phase 1, este mapa nao e um desenho em texto: com 290x110
    /// tiles ele ficaria impossivel de ler. Cada sala e um metodo que escava a
    /// rocha e, no mesmo lugar, declara o que mora nela — vapor, acido,
    /// valvulas, inimigos, canos, luzes. Mexer na sala e mexer no metodo dela.
    ///
    /// Aqui o Scrap ja tem o Steam Dash, e o mapa conta com ele: ha vaos que
    /// so se cruzam com dash, e alguns so emendando dash em nuvem de vapor.
    ///
    /// Este arquivo nao depende de UnityEngine de proposito: assim da pra rodar
    /// Validate() fora do editor e conferir que o mapa e navegavel de verdade.
    /// </summary>
    public static class Phase02Layout
    {
        public const int Width = 290;
        public const int Height = 110;

        // Nomes usados pra ligar a saida de uma cena a chegada na outra.
        public const string SceneName = "Phase02_BrassPipelines";
        public const string FromDistrict1SpawnId = "FromDistrict1";

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
        }

        /// <summary>
        /// Uma caldeira corrompida que explode com tres golpes, e as paredes
        /// rachadas que a explosao derruba. X e Y sao o canto de baixo (4x6 tiles).
        /// </summary>
        public sealed class BoilerSpec
        {
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
        }

        /// <summary>
        /// A historia do District 2, espalhada pelo mapa: o livro de turnos da
        /// caldeira em quatro paginas soltas, as pichacoes dos operarios, a
        /// placa do cano-mestre e o velho encanador escondido.
        /// </summary>
        public static readonly List<LoreSpec> Lore = new List<LoreSpec>();

        /// <summary>Onde Crisol fica.</summary>
        public static (int X, int Y) CrisolTile => (112, 19);

        public static readonly string[] CrisolLines =
        {
            "Hm. Mais um pedaco de sucata que desceu pelos canos errados.",
            "Eu sou Crisol, o Lapidario. Ja cravei pedra em coroa. Hoje conserto lata pra quem nao sabe o que e brilho.",
            "Esse aro de prata que voce carrega... o Coletor deixou cair, nao foi? Me de aqui. Voce nem sabe o que isso e.",
            "E um engaste. Serve pra segurar uma gema no chassi. Tome: um rubi bruto. Vamos ver se essa sucata aguenta uma pedra de verdade.",
            "Leve ate uma bancada e crave. O cinzel vai sair em brasa. Se voce derreter, eu fico com o rubi de volta.",
            "E se quiser que eu reforce essa carcaca, traga shards. Eu nao trabalho de graca pra Scrap nenhum.",
        };

        /// <summary>A area de onde o acido do poco de fuga parte, e ate que altura ele sobe.</summary>
        public const string RisingAcidPool = "Acid_RisingAcid";
        public static Area RisingAcidTrigger => Area.Between(142, 56, 151, 60);
        public const float RisingAcidHeight = 44f;

        /// <summary>Chegar aqui depois do boss purifica o distrito: e o topo da fuga.</summary>
        public static Area PurifyArea => Area.Between(128, 96, 136, 100);

        /// <summary>Onde o Boiler Master fica, ate onde ele anda e a area que o acorda.</summary>
        public static (int X, int Y) BossTile => (181, 56);
        public static (int X0, int X1) BossRange => (160, 190);
        public static Area BossWakeArea => Area.Between(156, 56, 194, 66);
        public const string BossGate = "Gate_ArenaExit";

        /// <summary>Onde nao ha parede de fundo: os saloes abertos, por onde se ve o horizonte.</summary>
        public static readonly List<Area> Voids = new List<Area>();

        private static readonly List<(Area Area, Material Material)> MaterialZones = new List<(Area, Material)>();

        /// <summary>Onde o Scrap aparece quando chega do District 1.</summary>
        public static (int X, int Y) SpawnTile => (15, 6);

        /// <summary>O tablado onde os Tarnished fazem o primeiro polimento.</summary>
        public static Area PolishStation => new Area(35, 8, 5, 4);

        /// <summary>O fim do mapa: o toco que leva aos Silent Geodes.</summary>
        public static (int X, int Y) District3Tile => (108, 96);

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
            BuildRisingAcid();
            BuildGeodeExit();

            BuildSewer();
            BuildMachineHall();
            BuildGaugeAttic();
            BuildDepot();
            BuildValveTower();
            BuildHighGallery();
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
            int direction = 1, int range = 14, int ceilingY = 0)
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

        private static void PressureBoiler(string name, int x, int y, int standX, int standY, params string[] walls)
        {
            PressureBoilers.Add(new BoilerSpec
            {
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

        private static void Tell(LoreKind kind, string group, string name, string title, int x, int y,
            params string[] lines)
        {
            Lore.Add(new LoreSpec { Kind = kind, Name = name, Title = title, X = x, Y = y, Lines = lines });

            // Entra como marcador tambem, pra a validacao conferir que da pra chegar.
            Mark(group, name, x, y);
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
        // 1. A Galeria de Transicao
        // ------------------------------------------------------------------

        private static void BuildGallery()
        {
            const string room = "01_Gallery";
            Room(room, 6, 6, 50, 15);

            // A metade oeste ainda e o Scrap Pit: rocha cor de ferrugem.
            MaterialZones.Add((Area.Between(0, 0, 23, 22), Material.Rock));

            Carve(6, 6, 28, 14);
            Carve(29, 7, 50, 15);

            // Um pedaco de teto mais baixo de cada lado, pra a galeria nao ser um caixote.
            Solid(14, 13, 20, 14);
            Solid(40, 14, 46, 15);

            // O poco por onde se chega do Scrap Pit, ja com a tampa fechada: uma
            // grade por cima. Da pra ver o Oxide la embaixo, mas nao ha volta.
            Carve(10, 2, 12, 5);
            Grate(10, 12, 5);

            // O tablado do polimento.
            Solid(35, 7, 39, 7);

            // Passagem pros Primeiros Canos.
            Carve(51, 7, 51, 11);

            Mark(room, "Tarnished_1", 33, 7);
            Mark(room, "Tarnished_2", 37, 8);
            Mark(room, "Tarnished_3", 41, 7);

            // --- Lado da sucata ---
            Put(GreyRock(3), 6, 6);
            Put(Debris(1), 7, 6);
            Put(Crate(1), 8, 6);
            Put(Crate(0), 16, 6);
            Put(Crate(2), 16, 7);
            Put(Debris(0), 17, 6);
            Put(GreyRock(1), 18, 6);
            Put(Fence(0), 20, 6);
            Put(Fence(1), 21, 6);
            Put(GreyRock(2), 23, 6);
            Put(DarkRock(1), 13, 6, PropLayer.Front);
            Put(DarkWeed(2), 22, 6, PropLayer.Front);
            Ore(Phase01Layout.RustBand, 18, 6, PropLayer.Back);
            Ore(Phase01Layout.GreyBand, 2, 9, PropLayer.Middle);
            Ore(Phase01Layout.RustBand, 16, 17, PropLayer.Middle);

            // O Oxide ainda sobe pelo poco, e clareia o lugar onde se chega.
            Shine(11.5f, 4f, 6f, 0.8f, Glow.Oxide);
            Shine(16f, 9.5f, 9f, 0.7f, Glow.Oxide);
            Leaks.Add(new Leak(11.5f, 5f, 0f, 1f));
            Hazes.Add(new Haze("Oxide_Gallery", 14f, 7f, 18f, 2.5f, 0.14f, 1.1f, false, Glow.Oxide));

            // --- Lado do latao ---
            Put(Crate(3), 26, 6);
            Put(Crate(1), 47, 7);
            Put(Crate(2), 48, 7);
            Put(Crate(0), 48, 8);

            Fix(FixtureKind.GaugeBig, "Gauge_Gallery", 30, 11);
            Furnace("Furnace_Gallery", 44, 7);

            // Dois lampioes em cima do tablado: e a unica luz limpa da sala.
            Shine(35f, 11.5f, 6f, 0.75f, Glow.Lamp);
            Shine(40f, 11.5f, 6f, 0.75f, Glow.Lamp);

            Plumb(Area.Between(25, 7, 50, 15), 101, PipeLayer.Back, false);
            RunPipe(PipeLayer.Mid, false, (24, 13), (33, 13), (33, 11));
            RunPipe(PipeLayer.Mid, true, (26, 9), (26, 12), (30, 12));
            RunPipe(PipeLayer.Far, false, (12, 6), (26, 6));

            Steam(room, Area.Between(29, 7, 50, 15), 0.07f);
            DripsIn(Area.Between(25, 7, 50, 15), 7, 11, false);
        }

        // ------------------------------------------------------------------
        // 2. Os Primeiros Canos
        // ------------------------------------------------------------------

        private static void BuildFirstPipes()
        {
            const string room = "02_FirstPipes";
            Room(room, 52, 7, 60, 40);

            Carve(52, 7, 60, 40);

            // Cada coluna sobe um andar e larga o Scrap do lado da proxima. A
            // plataforma de chegada fica 3 tiles abaixo do topo do vapor, entao
            // sair cedo da coluna ainda da certo — aqui nao e pra ter erro.
            Lift("Lift_FirstPipes_A", 53, 55, 7, 17);

            PipeFloor(56, 60, 14);
            Lift("Lift_FirstPipes_B", 58, 60, 15, 25);

            PipeFloor(52, 55, 22);
            Lift("Lift_FirstPipes_C", 52, 54, 23, 33);

            // A prateleira que recebe a ultima coluna e leva pra sala da valvula.
            Solid(57, 29, 60, 30);

            Fix(FixtureKind.GaugeSmall, "Gauge_FirstPipes_A", 52, 10);
            Fix(FixtureKind.GaugeSmall, "Gauge_FirstPipes_B", 57, 18);
            Fix(FixtureKind.GaugeSmall, "Gauge_FirstPipes_C", 55, 26);

            // O cano que alimenta cada bocal.
            RunPipe(PipeLayer.Mid, false, (50, 9), (52, 9), (52, 7));
            RunPipe(PipeLayer.Mid, false, (62, 16), (60, 16), (60, 15));
            RunPipe(PipeLayer.Mid, false, (50, 24), (52, 24), (52, 23));

            RunPipe(PipeLayer.Back, false, (56, 5), (56, 13));
            RunPipe(PipeLayer.Back, true, (57, 16), (57, 28));
            RunPipe(PipeLayer.Back, false, (59, 26), (59, 28));
            RunPipe(PipeLayer.Back, true, (55, 34), (55, 42));
            RunPipe(PipeLayer.Back, false, (50, 36), (62, 36));
            RunPipe(PipeLayer.Back, false, (50, 20), (56, 20), (56, 21));
            RunPipe(PipeLayer.Far, true, (28, 2), (28, 22));

            Steam(room, Area.Between(52, 30, 60, 40), 0.12f);
        }

        // ------------------------------------------------------------------
        // 3. A Valvula Emperrada
        // ------------------------------------------------------------------

        private static void BuildStuckValve()
        {
            const string room = "03_StuckValve";
            Room(room, 61, 31, 96, 40);

            Carve(61, 31, 96, 40);

            // A poca que fecha o caminho: larga demais ate pro dash. A valvula a
            // seca, e os dois degraus do fundo deixam sair do outro lado.
            Carve(68, 27, 79, 30);
            Solid(68, 27, 69, 28);
            Solid(78, 27, 79, 28);
            Acid("Acid_StuckValve", 68, 27, 79, 29);
            Solid(68, 27, 69, 28);
            Solid(78, 27, 79, 28);

            // A mesma valvula liga a coluna de vapor que sobe pra Casa de Maquinas.
            Carve(64, 41, 66, 47);
            Lift("Lift_ToMachineHall", 64, 66, 33, 49, false).StartsOff = true;

            Valve(room, "Valve_StuckValve", 61, 33, 62, 31, "pool:Acid_StuckValve", "vent:Lift_ToMachineHall");
            RunPipe(PipeLayer.Mid, true, (62, 33), (62, 32), (67, 32), (67, 30));

            Enemy(EnemyKind.ValveSentry, "Sentry_StuckValve", 82, 82, 31, -1, 13);

            Tell(LoreKind.Page, room, "Lore_Logbook_1", "pagina do livro de turnos", 95, 31,
                "LIVRO DE TURNOS DA CALDEIRA  -  turno da noite.",
                "Pressao acima do normal de novo. Abri a valvula de alivio duas vezes e ela voltou a subir.",
                "Avisei la em cima. Disseram que e o tempo. Aqui embaixo nao tem tempo.");

            // A escada que desce pra cisterna. Uma ponte de cano mantem o
            // caminho da sala reto por cima dela.
            //
            // A ponte comeca uma coluna depois do primeiro degrau de proposito:
            // e por essa fresta que se desce. Com a ponte cobrindo o degrau
            // inteiro o Scrap nao caberia de pe pra entrar embaixo dela.
            Carve(84, 29, 86, 30);
            Carve(87, 27, 87, 30);
            Carve(88, 27, 89, 29);
            Carve(90, 25, 92, 29);
            PipeFloor(88, 92, 30);
            Carve(93, 23, 95, 27);
            Carve(96, 21, 98, 25);

            Fix(FixtureKind.GaugeBig, "Gauge_StuckValve", 61, 36);
            Furnace("Furnace_StuckValve", 94, 32);

            Fumes("StuckValve", Area.Between(68, 27, 79, 29));
            Plumb(Area.Between(61, 31, 96, 40), 301, PipeLayer.Back, false);
            Plumb(Area.Between(66, 31, 82, 40), 302, PipeLayer.Back, true, 3);
            RunPipe(PipeLayer.Mid, false, (73, 42), (73, 37), (80, 37), (80, 42));
            RunPipe(PipeLayer.Far, false, (30, 18), (50, 18));

            Steam(room, Area.Between(61, 31, 96, 40));
            DripsIn(Area.Between(68, 31, 79, 40), 3, 31, true);
            DripsIn(Area.Between(82, 31, 96, 40), 6, 32, false);
        }

        // ------------------------------------------------------------------
        // 4. A Cisterna de Crisol
        // ------------------------------------------------------------------

        private static void BuildCistern()
        {
            const string room = "04_Cistern";
            Room(room, 99, 19, 120, 27);

            Carve(99, 19, 120, 27);

            // Cantos de cima arredondados: e uma cisterna, nao um corredor.
            Solid(99, 26, 101, 27);
            Solid(117, 26, 120, 27);
            Solid(119, 24, 120, 25);

            // A escotilha fica no pe da escada. A sala e "selada contra o gas".
            Fix(FixtureKind.Hatch, "Hatch_Cistern", 96, 21);

            Bench(room, "Workbench_Cistern", 103, 19);
            Mark(room, "Crisol", 112, 19);

            // Um alcapao no chao: por ele se desce pro Esgoto. So desce.
            Carve(107, 15, 108, 18);

            // O estoque de Crisol.
            Put(Crate(0), 115, 19);
            Put(Crate(2), 116, 19);
            Put(Crate(1), 116, 20);
            Put(Crate(3), 117, 19);
            Put(Crate(0), 118, 19);
            Put(Crate(3), 118, 20);

            Fix(FixtureKind.GaugeSmall, "Gauge_Cistern", 110, 23);

            // Lampiao: a primeira luz quente e parada do distrito.
            Shine(113.5f, 23f, 8f, 1.0f, Glow.Lamp);

            RunPipe(PipeLayer.Back, false, (97, 25), (122, 25));
            RunPipe(PipeLayer.Back, false, (108, 25), (108, 21));
            RunPipe(PipeLayer.Back, true, (102, 25), (102, 22), (100, 22));
        }

        // ------------------------------------------------------------------
        // 5. O Salao dos Welders
        // ------------------------------------------------------------------

        private static void BuildWelderHall()
        {
            const string room = "05_WelderHall";
            Room(room, 100, 31, 149, 43);

            Carve(97, 31, 99, 35);
            Carve(100, 31, 149, 43);

            // Dois blocos no chao: cobertura na luta, e o primeiro degrau do
            // caminho de cima.
            Solid(112, 31, 115, 32);
            Solid(131, 31, 135, 32);

            // O caminho alternativo, por cima dos canos.
            PipeFloor(118, 122, 34);
            PipeFloor(125, 130, 36);
            PipeFloor(133, 138, 38);
            PipeFloor(141, 146, 38);

            // Ele termina num mirante que da direto pro lago da Cadeia de Vapor.
            Carve(150, 39, 152, 42);
            Mark(room, "Secret_Overlook", 151, 39);

            Carve(150, 31, 152, 35);

            Tell(LoreKind.Graffiti, room, "Lore_Graffiti_1", "a pichacao", 105, 31,
                "Riscado na chapa, em letra de quem tinha pressa:",
                "\"A PRATA BEBE, O LATAO SUA.\"");

            Tell(LoreKind.GraffitiMask, room, "Lore_Graffiti_2", "o desenho", 139, 31,
                "Um desenho tosco, feito a giz: uma mascara de soldador.",
                "Embaixo, outra mao escreveu: \"quem tem mascara, tem cara\".",
                "E, mais embaixo, uma terceira: \"e quem nao tem?\"");

            Tell(LoreKind.Graffiti, room, "Lore_Graffiti_3", "a pichacao", 144, 39,
                "La no alto, onde so chega quem anda pelos canos:",
                "\"ELES NUNCA DESCERAM AQUI. NEM PRA VER.\"");

            Enemy(EnemyKind.BrassWelder, "Welder_Hall_1", 117, 129, 31);
            Enemy(EnemyKind.BrassWelder, "Welder_Hall_2", 137, 145, 31);
            Leech("Leech_Hall_1", 108, 43, 31, 101, 111);
            Leech("Leech_Hall_2", 124, 43, 31, 117, 130);
            Leech("Leech_Hall_3", 140, 43, 31, 136, 146);

            Boiler("Boiler_Hall_West", 102, 31);
            Boiler("Boiler_Hall_East", 126, 31);
            Furnace("Furnace_Hall", 119, 31);
            Furnace("Furnace_Hall_East", 143, 31);

            Fix(FixtureKind.GaugeBig, "Gauge_Hall_1", 108, 37);
            Fix(FixtureKind.GaugeBig, "Gauge_Hall_2", 138, 33);
            Fix(FixtureKind.GaugeSmall, "Gauge_Hall_3", 123, 39);

            Put(Crate(1), 136, 31);
            Put(Crate(3), 137, 31);
            Put(Debris(1), 110, 31);
            Put(Crate(0), 113, 33);

            Plumb(Area.Between(100, 31, 149, 43), 501, PipeLayer.Back, false);
            Plumb(Area.Between(104, 31, 146, 43), 502, PipeLayer.Back, true, 3);
            RunPipe(PipeLayer.Mid, false, (98, 41), (116, 41), (116, 44));
            RunPipe(PipeLayer.Mid, true, (132, 44), (132, 41), (151, 41));
            RunPipe(PipeLayer.Far, false, (50, 17), (76, 17));
            RunPipe(PipeLayer.Far, true, (58, 14), (58, 22));
            RunPipe(PipeLayer.Far, true, (68, 14), (68, 22));

            Leaks.Add(new Leak(116.5f, 41f, 0f, -1f));
            Leaks.Add(new Leak(140f, 41.5f, 0f, -1f));

            Steam(room, Area.Between(100, 31, 149, 43), 0.11f);
            DripsIn(Area.Between(100, 31, 149, 43), 8, 51, false);
        }

        // ------------------------------------------------------------------
        // 6. A Cadeia de Vapor
        // ------------------------------------------------------------------

        private static void BuildSteamChain()
        {
            const string room = "06_SteamChain";
            Room(room, 153, 12, 203, 45);

            Carve(153, 12, 203, 45);
            Voids.Add(Area.Between(153, 12, 203, 45));

            // A sacada de onde se parte, e o patamar onde se chega.
            Solid(153, 12, 156, 30);
            Solid(196, 12, 203, 34);

            Acid("Acid_SteamChain", 157, 12, 195, 15);

            // A travessia, de oeste pra leste: coluna, ilha, jato, ilha,
            // coluna que pulsa, ilha, patamar.
            Lift("Lift_Chain_1", 158, 160, 24, 36);
            PipeFloor(162, 165, 33);

            // O jato comeca rente a ponta da ilha: e so andar pra dentro dele.
            Jet("Jet_Chain", 166, 176, 34, 37, 1);
            PipeFloor(178, 184, 32);

            Vent pulsing = Lift("Lift_Chain_2", 186, 188, 18, 40);
            pulsing.OnTime = 2.6f;
            pulsing.OffTime = 1.3f;

            // La embaixo, rente ao acido, a plataforma do tunel que vem do Deposito.
            PipeFloor(190, 194, 17);
            Enemy(EnemyKind.ValveSentry, "Sentry_Chain", 202, 202, 35, -1, 8);

            PipeFloor(190, 193, 37);

            // O caminho de volta, por baixo. Sao degraus altos demais pra subir,
            // entao so serve pra voltar: a ida continua sendo pelo vapor.
            PipeFloor(190, 193, 30);
            PipeFloor(173, 177, 27);
            PipeFloor(167, 170, 27);
            PipeFloor(162, 164, 27);

            // O segredo: uma coluna que so pega quem pula, e leva a um nicho no alto.
            Lift("Lift_Chain_Secret", 200, 202, 38, 43);
            Carve(204, 41, 207, 44);
            Mark(room, "Secret_SilverFragment", 206, 41);

            Carve(204, 35, 207, 38);

            // Os canos que saem do acido e alimentam cada bocal.
            RunPipe(PipeLayer.Mid, true, (159, 10), (159, 22));
            RunPipe(PipeLayer.Mid, true, (187, 10), (187, 16));
            RunPipe(PipeLayer.Mid, false, (165, 47), (165, 38));
            RunPipe(PipeLayer.Mid, false, (205, 37), (201, 37));

            // Tubulacao colossal atravessando o salao.
            RunPipe(PipeLayer.Far, false, (75, 20), (103, 20));
            RunPipe(PipeLayer.Far, true, (84, 4), (84, 24));
            RunPipe(PipeLayer.Far, false, (95, 4), (95, 20));
            RunPipe(PipeLayer.Far, true, (75, 11), (95, 11));

            Fumes("SteamChain", Area.Between(157, 12, 195, 15), 0.18f);
            Hazes.Add(new Haze("Steam_SteamChain", 178f, 30f, 46f, 14f, 0.06f, 2.4f, false, Glow.Steam));

            Skylines.Add(new Skyline("Skyline_Chain_Far", 13f, false, 3f, 0.25f));
            Skylines.Add(new Skyline("Skyline_Chain_Near", 11f, true, 2f, 0.5f));

            DripsIn(Area.Between(157, 12, 195, 45), 5, 61, true);
        }

        // ------------------------------------------------------------------
        // 7. O Labirinto de Valvulas
        // ------------------------------------------------------------------

        private static void BuildValveMaze()
        {
            const string room = "07_ValveMaze";
            Room(room, 208, 28, 237, 40);

            // Vestibulo, no andar do meio, e o poco de escada do lado dele.
            Carve(208, 35, 210, 40);
            Carve(211, 28, 217, 40);

            // Andar de baixo: um corredor comprido, com duas pocas rasas.
            Carve(209, 28, 237, 32);

            // A escada do poco: um bloco no chao e dois canos.
            Solid(211, 28, 213, 29);
            PipeFloor(215, 217, 31);
            PipeFloor(211, 213, 33);

            Carve(222, 27, 224, 27);
            Acid("Acid_Maze_1", 222, 27, 224, 27);
            Carve(231, 27, 233, 27);
            Acid("Acid_Maze_2", 231, 27, 233, 27);

            // Um portao no meio do corredor de baixo.
            Gate("Gate_Maze", 227, 28, 227, 32, GateOpener.Valve);

            // No fim do corredor, uma coluna sobe pro andar do meio, lado leste.
            Carve(235, 33, 237, 34);
            Lift("Lift_Maze_Up", 235, 237, 28, 36).StartsOff = true;

            // Andar do meio, lado leste. Um buraco no chao devolve pro corredor:
            // e o que deixa a volta possivel.
            Carve(223, 35, 237, 40);
            Carve(228, 33, 229, 34);

            // O elevador de vapor, que leva ao andar de cima.
            Carve(223, 41, 226, 60);
            Lift("Lift_Maze_Elevator", 223, 225, 35, 57).StartsOff = true;

            // Tres valvulas, em sequencia: a do vestibulo abre o portao; a do
            // beco de baixo liga a coluna do fim do corredor; a do andar do
            // meio, que so se alcanca por essa coluna, liga o elevador.
            Valve(room, "Valve_Maze_A", 208, 37, 209, 35, "gate:Gate_Maze");
            Valve(room, "Valve_Maze_B", 209, 30, 210, 28, "vent:Lift_Maze_Up");
            Valve(room, "Valve_Maze_C", 231, 37, 232, 35, "vent:Lift_Maze_Elevator");

            Tell(LoreKind.Page, room, "Lore_Logbook_3", "pagina do livro de turnos", 227, 35,
                "LIVRO DE TURNOS DA CALDEIRA  -  a letra esta tremida.",
                "Tres colegas nao voltaram do turno. Fui procurar no corredor de baixo.",
                "Achei as ferramentas deles. So as ferramentas. Estavam verdes.");

            Enemy(EnemyKind.AcidBloater, "Bloater_Maze", 214, 218, 28);
            Leech("Leech_Maze", 232, 40, 35, 230, 234);

            Furnace("Furnace_Maze", 216, 28);
            Fix(FixtureKind.GaugeBig, "Gauge_Maze_Elevator", 227, 37);

            Fumes("Maze_1", Area.Between(222, 27, 224, 27), 0.10f);
            Fumes("Maze_2", Area.Between(231, 27, 233, 27), 0.10f);

            // Aqui o encanamento e o proprio labirinto: duas camadas, bem cheias.
            Plumb(Area.Between(209, 28, 237, 32), 701, PipeLayer.Back, false, 0);
            Plumb(Area.Between(223, 35, 237, 40), 702, PipeLayer.Back, true, 0);
            Plumb(Area.Between(211, 28, 217, 40), 703, PipeLayer.Back, true, 2);
            RunPipe(PipeLayer.Mid, false, (210, 39), (207, 39));
            RunPipe(PipeLayer.Mid, true, (211, 31), (209, 31));
            RunPipe(PipeLayer.Mid, false, (233, 39), (239, 39));
            RunPipe(PipeLayer.Mid, false, (226, 42), (226, 58));
            RunPipe(PipeLayer.Mid, true, (218, 34), (221, 34), (221, 38), (218, 38));
            RunPipe(PipeLayer.Far, false, (104, 15), (119, 15));
            RunPipe(PipeLayer.Far, true, (114, 10), (114, 22));

            Leaks.Add(new Leak(221f, 36f, -1f, 0f));
            Steam(room, Area.Between(223, 35, 237, 40), 0.12f);
            DripsIn(Area.Between(214, 28, 237, 32), 6, 71, true);
        }

        // ------------------------------------------------------------------
        // 8. A Workbench antes do boss
        // ------------------------------------------------------------------

        private static void BuildWorkbenchRoom()
        {
            const string room = "08_PreBoss";
            Room(room, 204, 56, 221, 63);

            Carve(204, 56, 221, 63);

            // Chegada do elevador.
            Carve(222, 56, 222, 60);

            // Uma calha de volta pro labirinto: o elevador so sobe.
            Carve(214, 41, 215, 55);

            // Passagem pra arena.
            Carve(199, 56, 203, 60);

            Bench(room, "Workbench_PreBoss", 206, 56);

            Tell(LoreKind.Page, room, "Lore_Logbook_4", "ultima pagina do livro de turnos", 211, 56,
                "LIVRO DE TURNOS DA CALDEIRA  -  a ultima pagina.",
                "Esta quase em branco. No meio dela, uma linha so:",
                "\"As mascaras funcionam.\"");

            // Do topo do elevador tambem sai um tunel pra Torre das Valvulas.
            Carve(227, 56, 245, 59);

            Fix(FixtureKind.Hatch, "Hatch_PreBoss", 204, 56);
            Fix(FixtureKind.GaugeBig, "Gauge_PreBoss", 210, 60);

            Put(Crate(2), 219, 56);
            Put(Crate(0), 220, 56);
            Put(Crate(1), 220, 57);

            Shine(217f, 60f, 7f, 0.7f, Glow.Lamp);

            RunPipe(PipeLayer.Back, false, (202, 62), (223, 62));
            RunPipe(PipeLayer.Back, true, (212, 62), (212, 58));
            RunPipe(PipeLayer.Back, false, (218, 62), (218, 59), (223, 59));
        }

        // ------------------------------------------------------------------
        // 9. A Sala de Caldeiras
        // ------------------------------------------------------------------

        private static void BuildBoilerRoom()
        {
            const string room = "09_BoilerRoom";
            Room(room, 152, 56, 198, 71);

            Carve(152, 56, 198, 71);
            Voids.Add(Area.Between(152, 56, 198, 71));

            // Cantos de cima chanfrados: a sala e quase redonda.
            Solid(152, 70, 155, 71);
            Solid(152, 68, 153, 69);
            Solid(195, 70, 198, 71);
            Solid(197, 68, 198, 69);

            // O tanque de acido e o piso de grade por cima dele.
            Carve(158, 50, 192, 54);
            Acid("Acid_BoilerRoom", 158, 50, 192, 53);
            Grate(158, 192, 55);

            // Um bloco em cada entrada. E onde ficam as valvulas das paredes, e
            // o primeiro degrau pras plataformas.
            Solid(155, 56, 157, 57);
            Solid(193, 56, 195, 57);

            PipeFloor(160, 165, 59);
            PipeFloor(185, 190, 59);
            PipeFloor(168, 172, 61);
            PipeFloor(178, 182, 61);
            PipeFloor(174, 176, 63);

            // Saida pro poco do acido ascendente.
            Carve(149, 56, 151, 59);

            Mark(room, "BossSpawn_BoilerMaster", BossTile.X, BossTile.Y);
            Mark(room, "Gate_Entry", 198, 56);

            // A saida fica gradeada ate ele cair.
            Gate(BossGate, 150, 56, 150, 59, GateOpener.Boss);

            // As tres valvulas da luta: girar uma devolve a pressao pro boss.
            Valve(room, "Valve_Arena_West", 155, 60, 156, 58).Arena = true;
            Valve(room, "Valve_Arena_East", 193, 60, 194, 58).Arena = true;
            Valve(room, "Valve_Arena_Top", 174, 66, 175, 64).Arena = true;

            // Tres caldeiras atras da grade, cada uma no seu compasso.
            Boiler("Boiler_Arena_West", 161, 56);
            Boiler("Boiler_Arena_Center", 173, 56);
            Boiler("Boiler_Arena_East", 185, 56);

            Fix(FixtureKind.GaugeBig, "Gauge_Arena_West", 167, 66);
            Fix(FixtureKind.GaugeBig, "Gauge_Arena_East", 181, 66);

            // As mangueiras: das caldeiras ate o teto.
            RunPipe(PipeLayer.Mid, true, (162, 62), (162, 73));
            RunPipe(PipeLayer.Mid, false, (175, 62), (175, 65));
            RunPipe(PipeLayer.Mid, true, (187, 62), (187, 73));
            RunPipe(PipeLayer.Mid, false, (150, 69), (157, 69), (157, 66));
            RunPipe(PipeLayer.Mid, false, (200, 69), (195, 69), (195, 66));
            RunPipe(PipeLayer.Far, false, (74, 33), (100, 33));
            RunPipe(PipeLayer.Far, true, (80, 27), (80, 36));
            RunPipe(PipeLayer.Far, true, (94, 27), (94, 36));

            // O acido ilumina a sala de baixo pra cima, por entre as barras.
            Fumes("BoilerRoom", Area.Between(158, 50, 192, 53), 0.14f);
            Hazes.Add(new Haze("Steam_BoilerRoom", 175f, 66f, 44f, 6f, 0.09f, 2.2f, false, Glow.Steam));

            Skylines.Add(new Skyline("Skyline_Arena_Far", 53f, false, 3f, 0.25f));
            Skylines.Add(new Skyline("Skyline_Arena_Near", 52.5f, true, 2f, 0.5f));

            DripsIn(Area.Between(158, 56, 192, 71), 7, 91, false);
        }

        // ------------------------------------------------------------------
        // 10. O Acido Ascendente
        // ------------------------------------------------------------------

        private static void BuildRisingAcid()
        {
            const string room = "10_RisingAcid";
            Room(room, 138, 50, 148, 99);

            Carve(138, 50, 148, 99);

            // A beirada de onde se entra, e o poco de acido do lado dela.
            Solid(142, 50, 148, 55);
            Acid("Acid_RisingAcid", 138, 50, 141, 53);

            // Cinco colunas empilhadas, alternando de lado, como nos Primeiros
            // Canos — so que agora com acido embaixo e uma delas pulsando.
            Lift("Lift_Rising_1", 138, 140, 56, 66);

            PipeFloor(142, 146, 63);
            Lift("Lift_Rising_2", 144, 146, 64, 74);

            PipeFloor(138, 141, 71);
            Vent pulsing = Lift("Lift_Rising_3", 138, 140, 72, 82);
            pulsing.OnTime = 2.4f;
            pulsing.OffTime = 1.1f;

            PipeFloor(142, 146, 79);
            Lift("Lift_Rising_4", 144, 146, 80, 90);

            PipeFloor(138, 141, 87);
            Lift("Lift_Rising_5", 138, 140, 88, 98);

            // Onde o acido comeca a subir, e ate onde ele chega.
            Mark(room, "RisingAcid_Start", 139, 54, false);
            Mark(room, "RisingAcid_Top", 143, 95, false);

            RunPipe(PipeLayer.Mid, true, (139, 48), (139, 54));
            RunPipe(PipeLayer.Back, false, (147, 56), (147, 101));
            RunPipe(PipeLayer.Back, true, (143, 48), (143, 62));
            RunPipe(PipeLayer.Back, false, (142, 64), (142, 78));
            RunPipe(PipeLayer.Back, true, (141, 88), (141, 101));
            RunPipe(PipeLayer.Back, false, (136, 68), (150, 68));
            RunPipe(PipeLayer.Back, false, (136, 84), (150, 84));
            RunPipe(PipeLayer.Far, true, (71, 26), (71, 50));

            Fix(FixtureKind.GaugeBig, "Gauge_Rising_1", 147, 60);
            Fix(FixtureKind.GaugeBig, "Gauge_Rising_2", 142, 75);
            Fix(FixtureKind.GaugeBig, "Gauge_Rising_3", 147, 92);

            Fumes("RisingAcid", Area.Between(138, 50, 141, 53), 0.16f);
            Steam(room, Area.Between(138, 90, 148, 99), 0.12f);
        }

        // ------------------------------------------------------------------
        // 11. A saida pros Silent Geodes
        // ------------------------------------------------------------------

        private static void BuildGeodeExit()
        {
            const string room = "11_GeodeExit";
            Room(room, 110, 96, 137, 104);

            // Daqui pra oeste o latao acabou: e rocha azulada e cristal.
            MaterialZones.Add((Area.Between(98, 88, 128, 109), Material.Crystal));

            Carve(110, 96, 137, 104);

            // Um degrau no chao e um pedaco de teto mais baixo.
            Solid(116, 96, 121, 96);
            Solid(124, 103, 130, 104);

            // Os dois atalhos que a purificacao abre chegam no poco de fuga.

            // O toco: no fundo dele comeca o District 3. Nao ha volta.
            Carve(107, 96, 109, 99);
            Mark(room, "Exit_District3", 108, 96);

            Exits.Add(new Exit("Exit_District3", Area.Between(107, 96, 107, 99),
                Phase03Layout.SceneName, Phase03Layout.FromDistrict2SpawnId));

            // Cristais: em pe no chao, e incrustados na rocha em volta.
            Ore(Phase01Layout.BlueBand, 111, 96, PropLayer.Back);
            Ore(Phase01Layout.BlueBand, 118, 97, PropLayer.Back);
            Ore(Phase01Layout.BlueBand, 123, 96, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 114, 100, PropLayer.Back);
            Ore(Phase01Layout.BlueBand, 103, 98, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 105, 102, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 112, 106, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 120, 92, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 126, 106, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 108, 92, PropLayer.Middle);
            Ore(Phase01Layout.GreyBand, 127, 96, PropLayer.Back);

            Put(DarkRock(0), 113, 96, PropLayer.Front);
            Put(DarkRock(2), 125, 96, PropLayer.Front);
            Put(GreyRock(4), 122, 96);

            Shine(112f, 98f, 7f, 0.9f, Glow.Crystal);
            Shine(119f, 99.5f, 6f, 0.8f, Glow.Crystal);
            Shine(108.5f, 98f, 6f, 1.0f, Glow.Crystal);
            Shine(124f, 97.5f, 5f, 0.6f, Glow.Crystal);

            // O cano-mestre: enorme, seco, saindo do distrito pra cima.
            RunPipe(PipeLayer.Far, true, (66, 46), (66, 54));
            RunPipe(PipeLayer.Far, true, (67, 46), (67, 54));

            Tell(LoreKind.Plaque, room, "Lore_MasterPipe", "a inscricao do cano-mestre", 132, 96,
                "Um cano da grossura de uma casa, subindo ate sumir na rocha. Esta seco, e corroido por dentro.",
                "Na placa, em letra de fundicao:",
                "\"PARA A CIDADE DE PRATA.\"",
                "Alguem riscou por cima, a prego: \"nada mais sobe\".");

            // Os ultimos canos, ainda do lado do latao.
            RunPipe(PipeLayer.Back, false, (139, 102), (130, 102), (130, 99));
            RunPipe(PipeLayer.Back, true, (135, 106), (135, 100));
            Fix(FixtureKind.GaugeSmall, "Gauge_GeodeExit", 133, 100);
        }

        // ------------------------------------------------------------------
        // A. O Esgoto
        // ------------------------------------------------------------------

        /// <summary>
        /// Por baixo de tudo: um canal de acido comprido, cruzado de ilha em
        /// ilha com o dash. Desce-se pra ca pelo alcapao da cisterna; sai-se a
        /// oeste, no pe dos Primeiros Canos, ou a leste, por uma coluna de
        /// vapor que brota no fim do Salao dos Welders.
        /// </summary>
        private static void BuildSewer()
        {
            const string room = "A_Sewer";
            Room(room, 62, 4, 150, 14);

            Carve(62, 6, 150, 14);
            Carve(64, 4, 148, 5);
            Acid("Acid_Sewer", 64, 4, 148, 5);

            // As ilhas: vaos de 5 tiles, um pulo com dash cada.
            int[,] islands =
            {
                { 68, 71 }, { 77, 80 }, { 86, 89 }, { 95, 98 }, { 104, 108 },
                { 114, 117 }, { 123, 126 }, { 132, 135 }, { 141, 144 },
            };

            for (int i = 0; i < islands.GetLength(0); i++)
            {
                Solid(islands[i, 0], 4, islands[i, 1], 6);
            }

            // Oeste: uma porta pro pe dos Primeiros Canos.
            Carve(61, 7, 61, 10);

            // Leste: uma coluna de vapor que sobe ate o chao do Salao dos Welders.
            Carve(147, 15, 149, 30);
            Lift("Lift_Sewer_Up", 147, 149, 6, 32);

            Enemy(EnemyKind.AcidBloater, "Bloater_Sewer_1", 86, 89, 7);
            Enemy(EnemyKind.AcidBloater, "Bloater_Sewer_2", 123, 126, 7);
            Enemy(EnemyKind.ValveSentry, "Sentry_Sewer", 62, 62, 6, 1, 30);
            Leech("Leech_Sewer_1", 97, 14, 7, 95, 98);
            Leech("Leech_Sewer_2", 133, 14, 7, 132, 135);

            Mark(room, "Secret_SilverFragment_Sewer", 142, 7);

            Fumes("Sewer", Area.Between(64, 4, 148, 5), 0.20f);
            Plumb(Area.Between(62, 6, 150, 14), 801, PipeLayer.Back, true, 0);
            RunPipe(PipeLayer.Mid, true, (60, 12), (152, 12));
            RunPipe(PipeLayer.Far, true, (31, 5), (75, 5));
            DripsIn(Area.Between(62, 6, 150, 14), 6, 81, true);
        }

        // ------------------------------------------------------------------
        // B. A Casa de Maquinas
        // ------------------------------------------------------------------

        /// <summary>
        /// Um salao enorme por cima do Salao dos Welders, com tres andares de
        /// passarela de grade. Chega-se pela coluna que a Valvula Emperrada
        /// liga. Daqui sai-se pro sotao (oeste), de volta pro salao por uma
        /// escotilha (que uma valvula abre) e, depois da purificacao, direto
        /// pro poco de fuga.
        /// </summary>
        private static void BuildMachineHall()
        {
            const string room = "B_MachineHall";
            Room(room, 62, 48, 132, 90);

            Carve(62, 48, 132, 90);

            // Andar 1. Os vaos entre as passarelas pedem dash.
            Grate(62, 80, 57);
            Grate(87, 104, 57);
            Grate(111, 132, 57);

            // Andar 2. O vao do meio e largo demais pra um dash so: ha uma nuvem
            // de vapor parada nele, que devolve o dash no meio do caminho.
            Grate(62, 96, 67);
            Grate(110, 132, 67);
            Cloud("Cloud_Hall", 102, 68, 105, 71);

            // Andar 3.
            Grate(62, 90, 77);
            Grate(98, 125, 77);

            // Do chao ao andar 1, sempre ligada, no vao entre as passarelas.
            Lift("Lift_Hall_1", 82, 84, 48, 60);

            // Do andar 1 ao 2, ligada pela valvula do andar 1.
            Lift("Lift_Hall_2", 98, 100, 58, 70).StartsOff = true;
            Valve(room, "Valve_Hall_1", 62, 60, 64, 58, "vent:Lift_Hall_2");

            // Do andar 2 ao 3, pulsando.
            Vent pulsing = Lift("Lift_Hall_3", 93, 95, 68, 80);
            pulsing.OnTime = 2.4f;
            pulsing.OffTime = 1.2f;

            // A valvula do andar 3 abre a escotilha do chao, que cai no Salao dos Welders.
            Carve(120, 44, 121, 47);
            Gate("Gate_HallHatch", 120, 47, 121, 47, GateOpener.Valve, 1, 0);
            Valve(room, "Valve_Hall_2", 121, 80, 123, 78, "gate:Gate_HallHatch");

            // Uma caldeira corrompida no andar 3: explodi-la derruba a parede do cofre.
            Carve(54, 78, 60, 82);
            Cracked("Wall_HallVault", 61, 78, 61, 80);
            PressureBoiler("Boiler_HallVault", 63, 78, 68, 78, "Wall_HallVault");
            Mark(room, "Secret_SilverFragment_Hall", 56, 78);

            Tell(LoreKind.Tarnished, room, "Lore_OldPlumber", "o velho encanador", 58, 78,
                "...Nao bata. Eu ouvi a caldeira estourar. Achei que era o fim, e era so voce.",
                "Sou encanador. Era. Estou neste cofre desde que o verde subiu pelos canos.",
                "Sabe por que sobrou operario aqui embaixo? Porque a gente ja usava mascara. Pra soldar. Ninguem nos deu: era o servico.",
                "O Oxide veio pelo vapor, e o vapor a gente ja nao respirava direito fazia anos.",
                "Os de prata, la em cima, nunca precisaram de mascara...",
                "...ate precisarem.");

            Tell(LoreKind.Page, room, "Lore_Logbook_2", "pagina do livro de turnos", 96, 48,
                "LIVRO DE TURNOS DA CALDEIRA  -  turno da manha.",
                "A agua esta com gosto de cobre. Nao e do cano: troquei o cano.",
                "O manometro tres nao sai do vermelho. Parei de olhar pra ele.");

            // Oeste, no andar 2: a passagem pro sotao.
            Carve(57, 68, 61, 71);

            // Leste, no andar 2: o atalho pro poco de fuga, que so abre depois.
            Carve(133, 68, 137, 70);
            Gate("Gate_HallToRising", 135, 68, 135, 70, GateOpener.Purification);

            Enemy(EnemyKind.BrassWelder, "Welder_Machine_1", 68, 80, 48);
            Enemy(EnemyKind.BrassWelder, "Welder_Machine_2", 100, 118, 48);
            Enemy(EnemyKind.AcidBloater, "Bloater_Machine", 112, 130, 58);
            Enemy(EnemyKind.ValveSentry, "Sentry_Machine_1", 131, 131, 68, -1, 20);
            Enemy(EnemyKind.ValveSentry, "Sentry_Machine_2", 100, 100, 78, 1, 24);
            Leech("Leech_Machine_1", 92, 56, 48, 88, 96);
            Leech("Leech_Machine_2", 116, 66, 58, 112, 130);
            Leech("Leech_Machine_3", 74, 76, 68, 66, 90);

            Boiler("Boiler_Machine_1", 70, 48);
            Boiler("Boiler_Machine_2", 104, 48);
            Boiler("Boiler_Machine_3", 126, 48);
            Furnace("Furnace_Machine_1", 90, 48);
            Furnace("Furnace_Machine_2", 114, 48);

            Fix(FixtureKind.GaugeBig, "Gauge_Machine_1", 76, 62);
            Fix(FixtureKind.GaugeBig, "Gauge_Machine_2", 108, 72);
            Fix(FixtureKind.GaugeBig, "Gauge_Machine_3", 86, 82);
            Fix(FixtureKind.GaugeBig, "Gauge_Machine_4", 118, 84);

            Plumb(Area.Between(62, 48, 132, 90), 901, PipeLayer.Back, false);
            Plumb(Area.Between(66, 48, 128, 90), 902, PipeLayer.Back, true, 4);
            Plumb(Area.Between(62, 48, 132, 56), 903, PipeLayer.Back, false, 0);
            Plumb(Area.Between(62, 58, 132, 66), 904, PipeLayer.Back, true, 0);
            RunPipe(PipeLayer.Mid, false, (72, 54), (72, 92));
            RunPipe(PipeLayer.Mid, true, (106, 54), (106, 92));
            RunPipe(PipeLayer.Mid, false, (128, 54), (128, 92));
            RunPipe(PipeLayer.Far, false, (30, 36), (67, 36));
            RunPipe(PipeLayer.Far, true, (30, 42), (67, 42));
            RunPipe(PipeLayer.Far, true, (44, 23), (44, 46));
            RunPipe(PipeLayer.Far, false, (58, 23), (58, 46));

            Leaks.Add(new Leak(72.5f, 66f, 1f, 0f));
            Leaks.Add(new Leak(106.5f, 76f, -1f, 0f));

            Hazes.Add(new Haze("Steam_MachineHall", 97f, 84f, 70f, 8f, 0.08f, 3.2f, false, Glow.Steam));
            DripsIn(Area.Between(62, 48, 132, 90), 9, 91, false);
        }

        // ------------------------------------------------------------------
        // C. A Galeria dos Manometros
        // ------------------------------------------------------------------

        /// <summary>
        /// O sotao, a oeste da Casa de Maquinas: uma sala baixa e comprida,
        /// forrada de manometros. A valvula daqui liga uma coluna que sobe
        /// direto dos Primeiros Canos — depois de girada, e o atalho do comeco
        /// da fase ate ca em cima. E um buraco no chao cai na Galeria de Transicao.
        /// </summary>
        private static void BuildGaugeAttic()
        {
            const string room = "C_GaugeAttic";
            Room(room, 8, 50, 56, 62);

            Carve(8, 50, 56, 62);

            // O patamar leste, na altura da porta da Casa de Maquinas, e a
            // escada que desce dele.
            Solid(48, 50, 56, 57);
            Solid(44, 50, 47, 55);
            Solid(40, 50, 43, 53);
            Solid(36, 50, 39, 51);
            Carve(57, 58, 61, 61);

            // A coluna que vem dos Primeiros Canos sai por um buraco no patamar.
            Carve(53, 41, 55, 57);
            Lift("Lift_ToAttic", 53, 55, 34, 59, false).StartsOff = true;
            Valve(room, "Valve_Attic", 9, 52, 11, 50, "vent:Lift_ToAttic");

            // O buraco que cai na Galeria de Transicao. So desce.
            Carve(21, 15, 22, 49);

            Enemy(EnemyKind.BrassWelder, "Welder_Attic", 24, 34, 50);
            Enemy(EnemyKind.ValveSentry, "Sentry_Attic", 9, 9, 50, 1, 11);
            Leech("Leech_Attic_1", 28, 62, 50, 24, 34);
            Leech("Leech_Attic_2", 15, 62, 50, 10, 19);

            Mark(room, "Secret_SilverFragment_Attic", 13, 50);

            // A parede de manometros.
            for (int x = 12; x <= 44; x += 6)
            {
                Fix(FixtureKind.GaugeBig, $"Gauge_Attic_{x}", x, 56 + (x / 6) % 2 * 2);
            }

            Furnace("Furnace_Attic", 30, 50);
            Shine(20f, 58f, 8f, 0.6f, Glow.Steam);
            Shine(40f, 58f, 8f, 0.6f, Glow.Steam);

            Plumb(Area.Between(8, 50, 47, 62), 1001, PipeLayer.Back, false, 0);
            RunPipe(PipeLayer.Mid, true, (6, 54), (18, 54), (18, 51));
            RunPipe(PipeLayer.Far, false, (4, 30), (28, 30));
            Steam(room, Area.Between(8, 50, 56, 62), 0.09f);
            DripsIn(Area.Between(8, 50, 47, 62), 8, 101, false);
        }

        // ------------------------------------------------------------------
        // D. O Deposito
        // ------------------------------------------------------------------

        /// <summary>
        /// Embaixo do Labirinto de Valvulas. Cai-se aqui por um buraco do
        /// corredor de baixo. O deposito e partido em dois por uma parede
        /// rachada: so explodindo a caldeira do lado se chega a metade leste,
        /// de onde sai a coluna de volta pro labirinto e a porta da Torre das
        /// Valvulas. A oeste, um tunel vai dar no lago da Cadeia de Vapor.
        /// </summary>
        private static void BuildDepot()
        {
            const string room = "D_Depot";
            Room(room, 206, 6, 238, 20);

            Carve(206, 6, 238, 20);

            // O buraco do labirinto.
            Carve(219, 21, 220, 27);

            // A parede que parte o deposito, com a passagem rachada embaixo.
            Solid(222, 6, 223, 20);
            Cracked("Wall_Depot", 222, 6, 223, 9);
            PressureBoiler("Boiler_Depot", 215, 6, 220, 6, "Wall_Depot");

            // Metade leste: a coluna de volta pro corredor do labirinto, e a porta da torre.
            Carve(225, 21, 226, 27);
            Lift("Lift_Depot_Up", 225, 226, 6, 29);
            Carve(239, 6, 245, 9);

            // Metade oeste: um patamar alto, a coluna que sobe ate ele, e o tunel
            // que atravessa a rocha ate a plataforma rente ao lago.
            Solid(206, 6, 209, 17);
            Lift("Lift_Depot_West", 210, 212, 6, 19);
            Carve(195, 18, 205, 21);

            Enemy(EnemyKind.AcidBloater, "Bloater_Depot", 228, 236, 6);
            Enemy(EnemyKind.BrassWelder, "Welder_Depot", 227, 237, 6);
            Leech("Leech_Depot", 232, 20, 6, 228, 236);

            Put(Crate(0), 213, 6);
            Put(Crate(2), 214, 6);
            Put(Crate(1), 214, 7);
            Put(Crate(3), 236, 6);
            Put(Crate(0), 237, 6);
            Put(Crate(2), 237, 7);
            Put(Crate(1), 238, 6);
            Put(Debris(0), 230, 6);

            Mark(room, "Secret_SilverFragment_Depot", 207, 18);

            Furnace("Furnace_Depot", 232, 6);
            Fix(FixtureKind.GaugeBig, "Gauge_Depot", 228, 12);

            Plumb(Area.Between(206, 6, 238, 20), 1101, PipeLayer.Back, true);
            RunPipe(PipeLayer.Mid, false, (224, 18), (240, 18));
            DripsIn(Area.Between(210, 6, 238, 20), 7, 111, false);
        }

        // ------------------------------------------------------------------
        // E. A Torre das Valvulas
        // ------------------------------------------------------------------

        /// <summary>
        /// A leste de tudo: um poco largo de quatro andares de grade. Cada
        /// andar tem uma valvula que liga a coluna pro andar de cima, e um vao
        /// no meio que so se cruza emendando dash numa nuvem de vapor.
        ///
        /// Entra-se por tres alturas: pela base (do Deposito), pelo meio (do
        /// Labirinto) e pelo terceiro andar (do topo do elevador). Do alto sai
        /// o tunel pra Galeria Alta. Descer e so se deixar cair pelos vaos.
        /// </summary>
        private static void BuildValveTower()
        {
            const string room = "E_ValveTower";
            Room(room, 246, 6, 284, 92);

            Carve(246, 6, 284, 92);

            // As tres entradas (a da base e a porta que o Deposito escavou).
            Carve(238, 35, 245, 38);

            // Base: uma poca de acido no meio.
            Carve(260, 4, 270, 5);
            Acid("Acid_Tower", 260, 4, 270, 5);
            Fumes("Tower", Area.Between(260, 4, 270, 5), 0.14f);

            // Andar do meio (35).
            Grate(246, 247, 34);
            Grate(251, 262, 34);
            Grate(270, 284, 34);
            Cloud("Cloud_Tower_1", 265, 35, 267, 38);

            // Terceiro andar (56).
            Grate(246, 258, 55);
            Grate(266, 279, 55);
            Grate(283, 284, 55);
            Cloud("Cloud_Tower_2", 261, 56, 263, 59);

            // Quarto andar (76).
            Grate(246, 247, 75);
            Grate(251, 270, 75);
            Grate(277, 284, 75);
            Carve(227, 76, 245, 79);

            // Cada valvula liga a coluna que sai do andar dela.
            Lift("Lift_Tower_1", 248, 250, 6, 37).StartsOff = true;
            Valve(room, "Valve_Tower_1", 278, 8, 280, 6, "vent:Lift_Tower_1");

            Lift("Lift_Tower_2", 280, 282, 35, 58).StartsOff = true;
            Valve(room, "Valve_Tower_2", 272, 37, 274, 35, "vent:Lift_Tower_2");

            Lift("Lift_Tower_3", 248, 250, 56, 78).StartsOff = true;
            Valve(room, "Valve_Tower_3", 254, 58, 256, 56, "vent:Lift_Tower_3");

            Enemy(EnemyKind.AcidBloater, "Bloater_Tower", 272, 283, 6);
            Enemy(EnemyKind.ValveSentry, "Sentry_Tower_1", 284, 284, 35, -1, 14);
            Enemy(EnemyKind.ValveSentry, "Sentry_Tower_2", 246, 246, 56, 1, 12);
            Enemy(EnemyKind.BrassWelder, "Welder_Tower", 252, 268, 76);
            Leech("Leech_Tower_1", 256, 33, 6, 251, 259);
            Leech("Leech_Tower_2", 276, 54, 35, 271, 279);
            Leech("Leech_Tower_3", 260, 92, 76, 252, 268);

            Mark(room, "Secret_SilverFragment_Tower", 281, 76);

            Boiler("Boiler_Tower_Base", 254, 6);
            Boiler("Boiler_Tower_Top", 262, 76);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_1", 264, 14);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_2", 258, 44);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_3", 272, 64);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_4", 270, 84);
            Furnace("Furnace_Tower", 274, 6);

            // Tubulacao em pe, de cima a baixo: e uma torre de canos.
            for (int x = 252; x <= 282; x += 6)
            {
                RunPipe(PipeLayer.Back, x % 12 == 0, (x, 4), (x, 94));
            }

            RunPipe(PipeLayer.Back, false, (244, 46), (286, 46));
            RunPipe(PipeLayer.Back, true, (244, 66), (286, 66));
            RunPipe(PipeLayer.Back, false, (244, 86), (286, 86));
            RunPipe(PipeLayer.Mid, true, (264, 20), (264, 96));
            RunPipe(PipeLayer.Far, false, (132, 2), (132, 48));
            RunPipe(PipeLayer.Far, true, (138, 2), (138, 48));

            Hazes.Add(new Haze("Steam_Tower", 265f, 88f, 38f, 6f, 0.09f, 2f, false, Glow.Steam));
            DripsIn(Area.Between(246, 6, 284, 33), 7, 121, true);
            DripsIn(Area.Between(246, 76, 284, 92), 8, 122, false);
        }

        // ------------------------------------------------------------------
        // F. A Galeria Alta
        // ------------------------------------------------------------------

        /// <summary>
        /// Por cima da Sala de Caldeiras: um corredor largo que liga o alto da
        /// Torre das Valvulas ao poco de fuga. A porta do poco so abre com a
        /// purificacao — antes disso e um beco com vista, e depois e o atalho
        /// de volta.
        /// </summary>
        private static void BuildHighGallery()
        {
            const string room = "F_HighGallery";
            Room(room, 150, 76, 226, 90);

            Carve(150, 76, 226, 90);
            Voids.Add(Area.Between(150, 76, 226, 90));

            // O patamar oeste, na altura da ultima plataforma do poco, e os
            // degraus de cano que sobem ate ele.
            Solid(150, 76, 160, 87);
            PipeFloor(184, 187, 77);
            PipeFloor(178, 181, 79);
            PipeFloor(172, 175, 81);
            PipeFloor(166, 169, 83);
            PipeFloor(161, 164, 85);

            Carve(149, 88, 149, 91);
            Gate("Gate_GalleryToRising", 149, 88, 149, 91, GateOpener.Purification);

            Enemy(EnemyKind.BrassWelder, "Welder_Gallery_1", 190, 205, 76);
            Enemy(EnemyKind.BrassWelder, "Welder_Gallery_2", 208, 224, 76);
            Enemy(EnemyKind.ValveSentry, "Sentry_Gallery", 151, 151, 88, 1, 9);
            Leech("Leech_Gallery_1", 196, 90, 76, 190, 205);
            Leech("Leech_Gallery_2", 216, 90, 76, 208, 224);

            Mark(room, "Secret_SilverFragment_Gallery", 153, 88);

            Boiler("Boiler_Gallery", 198, 76);
            Fix(FixtureKind.GaugeBig, "Gauge_Gallery_1", 212, 82);
            Fix(FixtureKind.GaugeBig, "Gauge_Gallery_2", 168, 86);
            Furnace("Furnace_Gallery_High", 220, 76);

            RunPipe(PipeLayer.Mid, false, (148, 89), (228, 89));
            RunPipe(PipeLayer.Mid, true, (190, 89), (190, 82), (184, 82));
            RunPipe(PipeLayer.Far, false, (75, 42), (113, 42));
            RunPipe(PipeLayer.Far, true, (100, 38), (100, 46));

            Skylines.Add(new Skyline("Skyline_Gallery_Far", 73f, false, 3f, 0.25f));
            Skylines.Add(new Skyline("Skyline_Gallery_Near", 72.5f, true, 2f, 0.5f));

            Hazes.Add(new Haze("Steam_HighGallery", 188f, 86f, 76f, 5f, 0.08f, 3f, false, Glow.Steam));
            DripsIn(Area.Between(162, 76, 226, 90), 9, 131, false);
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

        private static void Open(LevelMap map, Area area)
        {
            map.Fill(area.X, area.Y, area.Right, area.Top, LevelMap.Air);
        }

        /// <summary>
        /// Joga a fase na cabeca: explora, gira toda valvula e explode toda
        /// caldeira que alcancar, e repete com o mapa mudado, ate nada mais
        /// mudar. Devolve o estado final e o alcance nele.
        /// </summary>
        private static WorldState Play(out LevelMap.Reach reach, List<string> log)
        {
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

            foreach (Vent vent in Vents)
            {
                if (!vent.StartsOff) world.LiveVents.Add(vent.Name);
            }

            LevelMap.JumpRules rules = Rules();
            LevelMap.Lift[] clouds = CloudAreas();

            while (true)
            {
                reach = world.Map.Explore(SpawnTile, rules, world.Lifts(), world.Jets(), clouds);
                bool changed = false;

                foreach (ValveSpec valve in Valves)
                {
                    if (world.TurnedValves.Contains(valve.Name)) continue;
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

                if (!world.BossDown && reach.CanStand(BossTile.X, BossTile.Y))
                {
                    world.BossDown = true;
                    changed = true;
                    log?.Add("derrota o Boiler Master");

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Opener == GateOpener.Boss) Open(world.Map, gate.Area);
                    }
                }

                if (world.BossDown && !world.Purified && TouchesAny(reach, PurifyArea))
                {
                    world.Purified = true;
                    changed = true;
                    log?.Add("purifica o distrito");

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Opener == GateOpener.Purification) Open(world.Map, gate.Area);
                    }
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
            if (!world.Purified) problems.Add("Nao da pra chegar no topo do poco de fuga: o distrito nunca e purificado.");

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
            if (!TouchesAny(reach, PolishStation)) problems.Add("Nao da pra chegar no tablado do polimento.");

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
            }

            return problems;
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
