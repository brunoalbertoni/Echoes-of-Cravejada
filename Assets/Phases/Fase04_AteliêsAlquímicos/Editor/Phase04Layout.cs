using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O desenho da Phase 4 — District 4: The Alchemical Ateliers, a regiao
    /// dos forjadores e laboratorios nobres. E daqui que o Scrap veio: ele e
    /// o Prototipo Bruto n.º 7, desenhado numa destas mesas e jogado fora.
    ///
    /// Marmore e prata por fora; por dentro, laboratorio: vidraria,
    /// alambiques, tanques com cobaias, quadros-negros, mesas de trabalho.
    ///
    ///   A ENTRADA     1 o Saguao (a cabine do District 3 sobe pelo poco e
    ///                 para ali) e 2 a Galeria dos Mestres, com os retratos.
    ///   O CENTRO      3 o Atelie de Crisol: bancada, Crisol, a loja e o
    ///                 Grande Alambique, que tranca o caminho pro chefe e pede
    ///                 tres reagentes.
    ///   AS ALAS       em qualquer ordem, cada uma termina num reagente:
    ///                 4 a Destilaria (embaixo): tres andares em volta, com
    ///                   goteiras, fossos e um atalho de volta por cima.
    ///                 5 a Optica (em cima): espelhos que giram e luz-guia ate
    ///                   fechaduras de cristal; o cofre secreto do chassi.
    ///                 6 a Incubacao (a leste): tanques, bioterio e a Oficina
    ///                   de Prototipos, onde ficou a ficha do n.º 7.
    ///   O CHEFE       7 o pre-boss e 8 o Grande Laboratorio: o Alquimista de
    ///                 Mercurio, que so apanha transmutado em prata.
    ///   DEPOIS        9 o corredor de saida e 10 o Anfiteatro
    ///                 de Demonstracoes, onde o circulo do palco purifica o
    ///                 distrito. Do anfiteatro, a Grande Escadaria sobe ate o palacio (District 5).
    ///
    /// Algumas celulas sao rocha pro mapa mas objeto no jogo: o chao da cabine
    /// do elevador, os meios-degraus da escadaria e as pocas de mercurio (as
    /// listas de pocas, blocos e circulos seguem no formato, embora esta
    /// versao da fase nao use mais pocas no caminho nem circulos fora da arena).
    ///
    /// Como os outros layouts, este arquivo nao depende de UnityEngine.
    /// </summary>
    public static partial class Phase04Layout
    {
        public const int Width = 290;
        public const int Height = 100;

        public const string SceneName = "Phase04_AlchemicalAteliers";
        public const string FromDistrict3SpawnId = "FromDistrict3";

        /// <summary>A cena seguinte: o palacio, no alto da Grande Escadaria.</summary>
        public const string NextSceneName = "Phase05_SilverCity";
        public const string ToDistrict5SpawnId = "FromDistrict4";

        public const string BossName = "O Alquimista de Mercúrio";

        // ------------------------------------------------------------------
        // Tipos
        // ------------------------------------------------------------------

        /// <summary>Um retangulo de celulas, cantos incluidos.</summary>
        public readonly struct Cells
        {
            public readonly int X0;
            public readonly int Y0;
            public readonly int X1;
            public readonly int Y1;

            public Cells(int x0, int y0, int x1, int y1)
            {
                X0 = x0;
                Y0 = y0;
                X1 = x1;
                Y1 = y1;
            }

            public int Width => X1 - X0 + 1;
            public int Height => Y1 - Y0 + 1;
        }

        /// <summary>Uma poca de mercurio. Current e a correnteza (0 se parada).</summary>
        public sealed class Pool
        {
            public string Name;
            public Cells Area;
            public float Current;
        }

        public enum BlockKind
        {
            Glass,
            Silver,
            Marble,
        }

        /// <summary>Um bloco que um circulo de transmutacao cria ou desfaz.</summary>
        public sealed class Block
        {
            public string Name;
            public BlockKind Kind;
            public Cells Area;
        }

        /// <summary>
        /// Um circulo de transmutacao: onde o Scrap para pra polir a runa, o
        /// que some e o que aparece. Os nomes sao de Blocks, de Pools ou de
        /// VaporLifts.
        /// </summary>
        public sealed class Circle
        {
            public string Name;
            public string Effect;
            public int X;
            public int Y;
            public string[] Vanish;
            public string[] Appear;
        }

        /// <summary>
        /// Um raio de luz alquimica. Dois tipos:
        ///
        ///   perigo   liga e desliga em ritmo e queima: fica On segundos aceso,
        ///            Off apagado, comecando com Offset de atraso. Vai reto ate
        ///            bater em parede.
        ///   guia     (Guide) a luz fria dos enigmas: sempre acesa, nao queima,
        ///            dobra nos espelhos e acende as fechaduras de cristal.
        /// </summary>
        public sealed class Beam
        {
            public string Name;
            public int X;
            public int Y;
            public int DirX;
            public int DirY;
            public float On;
            public float Off;
            public float Offset;
            public bool Guide;
        }

        /// <summary>
        /// Um espelho de prata num eixo, a 45 graus. Slash e como ele comeca:
        /// "/" (verdadeiro) ou "\" (falso). O Scrap o gira com E, de pe em
        /// (StandX, StandY).
        /// </summary>
        public sealed class MirrorSpec
        {
            public string Name;
            public int X;
            public int Y;
            public bool Slash;
            public int StandX;
            public int StandY;
        }

        /// <summary>Uma fechadura de cristal: luz-guia em cima dela abre o portao Gate. Hidden e a do segredo.</summary>
        public sealed class LockSpec
        {
            public string Name;
            public int X;
            public int Y;
            public string Gate;
            public bool Hidden;
        }

        /// <summary>
        /// Um portao: as celulas que fecha e pra onde corre ao abrir. O
        /// disfarcado (Disguised) parece um bloco de marmore da parede.
        /// </summary>
        public sealed class GateSpec
        {
            public string Name;
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int MoveX;
            public int MoveY = 1;
            public bool Disguised;
        }

        public enum FoeKind
        {
            Slime,
            FlaskThrower,
            FailedExperiment,
            Sentinel,
        }

        /// <summary>Um inimigo: o trecho de chao por onde anda (o arremessador fica em X0) e a linha dos pes.</summary>
        public sealed class Foe
        {
            public FoeKind Kind;
            public string Name;
            public int X0;
            public int X1;
            public int Y;

            /// <summary>Onde ele comeca, dentro do trecho (pra dois no mesmo chao nao nascerem juntos).</summary>
            public int HomeX;
        }

        /// <summary>
        /// Algo pra ler ou alguem pra ouvir. Kind diz o desenho: "sign" a placa
        /// do portao, "portrait0".."portrait2" os retratos, "diary" uma folha
        /// do diario de laboratorio, "chalk" um quadro-negro, "board" o quadro
        /// das fichas dos prototipos, "sheet" a ficha do n.º 7, "plaque" a
        /// placa do anfiteatro, "decree" a ordem de descarte, "assistant" a
        /// Assistente (no armario) e, so nas listas do distrito purificado,
        /// "standing" a Assistente de pe e "crisol" o proprio Crisol.
        /// </summary>
        public sealed class LoreSpot
        {
            public string Kind;
            public string Name;
            public string Title;
            public int X;
            public int Y;
            public string[] Lines;
        }

        public readonly struct Marker
        {
            public readonly string Group;
            public readonly string Name;
            public readonly int X;
            public readonly int Y;
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

        /// <summary>
        /// Um enfeite de laboratorio (da arte do LabArt). X e o meio, Y a base
        /// (ou, pendurado, o teto de onde ele desce).
        /// </summary>
        public readonly struct Prop
        {
            public readonly string Sprite;
            public readonly float X;
            public readonly float Y;
            public readonly int Order;
            public readonly bool Hanging;

            public Prop(string sprite, float x, float y, int order, bool hanging)
            {
                Sprite = sprite;
                X = x;
                Y = y;
                Order = order;
                Hanging = hanging;
            }
        }

        /// <summary>Um reagente: o item-chave que vai no Grande Alambique.</summary>
        public sealed class Reagent
        {
            public string Id;
            public string Name;
            public int Index;
            public int X;
            public int Y;
        }

        // ------------------------------------------------------------------
        // O que o gerador le
        // ------------------------------------------------------------------

        private static readonly LevelMap Map = new LevelMap(Width, Height);

        public static readonly List<(string Name, int X0, int Y0, int X1, int Y1)> Rooms =
            new List<(string, int, int, int, int)>();

        public static readonly List<Pool> Pools = new List<Pool>();
        public static readonly List<Block> Blocks = new List<Block>();
        public static readonly List<Circle> Circles = new List<Circle>();
        public static readonly List<Beam> Beams = new List<Beam>();
        public static readonly List<MirrorSpec> Mirrors = new List<MirrorSpec>();
        public static readonly List<LockSpec> Locks = new List<LockSpec>();
        public static readonly List<GateSpec> Gates = new List<GateSpec>();
        public static readonly List<Foe> Foes = new List<Foe>();
        public static readonly List<LoreSpot> Lore = new List<LoreSpot>();
        public static readonly List<LoreSpot> Purified = new List<LoreSpot>();
        public static readonly List<Marker> Markers = new List<Marker>();
        public static readonly List<Prop> Props = new List<Prop>();
        public static readonly List<Reagent> Reagents = new List<Reagent>();

        /// <summary>Colunas de vapor que so existem depois de uma transmutacao.</summary>
        public static readonly List<(string Name, Cells Area)> VaporLifts = new List<(string, Cells)>();

        /// <summary>Goteiras de mercurio: a celula do teto de onde pinga e quantos tiles cai.</summary>
        public static readonly List<(int X, int Y, int Fall)> Drips = new List<(int, int, int)>();

        /// <summary>As Workbenches: canto de baixo, a esquerda (3x2 tiles).</summary>
        public static readonly List<(string Name, int X, int Y)> Workbenches = new List<(string, int, int)>();

        /// <summary>Lampioes e brilhos: Green e o verde do Oxide nos frascos e tanques.</summary>
        public static readonly List<(float X, float Y, float Radius, bool Green)> Lamps =
            new List<(float, float, float, bool)>();

        /// <summary>Vitrais na parede do fundo: variante e o pe.</summary>
        public static readonly List<(int Variant, int X, int Y)> Windows = new List<(int, int, int)>();

        /// <summary>Celulas que sao objeto, e nao tile: as pocas de mercurio.</summary>
        private static readonly HashSet<(int, int)> ObjectCells = new HashSet<(int, int)>();

        public const string ReagentPrefix = "D4_Reagent_";

        public static (int X, int Y) SpawnTile => (8, 40);

        /// <summary>
        /// O poco do elevador do District 3, embaixo do saguao: a coluna da
        /// esquerda (X, 3 de largura), o fundo (de onde a cabine parte), o
        /// andar (onde ela para, o chao do saguao) e o teto do saguao (ate onde
        /// vao os trilhos). A celula logo abaixo do andar e o chao da cabine:
        /// rocha pro mapa, mas quem a desenha e a segura e a cabine.
        /// </summary>
        public static (int X, int Bottom, int Stop, int Top) ElevatorShaft => (7, 12, 40, 49);

        public static (int X, int Y) CrisolTile => (76, 40);

        public static readonly string[] CrisolLines =
        {
            "...Scrap.",
            "Não mexe em nada. Essa bancada era minha. Aquele torno era meu. Aquela lupa, fui eu que poli.",
            "Eu cravei gema pra metade dos Nobres dessa cidade. Cheguei a ter retrato pendurado no palácio.",
            "Quando a praga começou, os alquimistas vieram me pedir quartzo bruto. Bruto. Achei que era piada.",
            "Não era. Eles tavam construindo alguma coisa. Eu vi os desenhos. Eu sabia o que eles tinham achado.",
            "E quando o Rei mandou jogar tudo fora, eu fiquei quieto. Já tinha falado demais uma vez, contra a escavação. Tive medo.",
            "Não adiantou. Me riscaram do mesmo jeito e me jogaram no poço, junto com a sucata que eu não defendi.",
            "Aquele alambique ali pede três reagentes. Um na destilaria, embaixo; um na sala das lentes, em cima; e o sal, lá na incubação.",
            "...Descansa, Scrap. A bancada ainda funciona.",
        };

        /// <summary>O Grande Alambique: onde o Scrap entrega os reagentes (de pe, ao lado dele).</summary>
        public static (int X, int Y) AlembicTile => (94, 40);

        /// <summary>O portao que o alambique abre: no fim do mezanino, a leste.</summary>
        public const string AlembicGate = "Gate_Alembic";

        /// <summary>A grade da saida do Grande Laboratorio: abre quando o Alquimista cai.</summary>
        public const string BossExitGate = "Gate_BossExit";

        /// <summary>A grade da Sala das Lentes: abre com o primeiro enigma de espelho.</summary>
        public const string LensesGate = "Gate_Lenses";

        /// <summary>A grade do nicho da Luz Engarrafada, no alto da Camara dos Espelhos.</summary>
        public const string MirrorsGate = "Gate_Mirrors";

        /// <summary>O bloco falso da parede da Camara dos Espelhos: o cofre do chassi.</summary>
        public const string VaultGate = "Gate_Vault";

        /// <summary>O reforco de chassi, no cofre atras da parede da Camara dos Espelhos.</summary>
        public static (int X, int Y) SecretChassisTile => (34, 77);

        // O Grande Laboratorio.
        public static Cells ArenaRoom => new Cells(129, 62, 170, 80);
        public static (int X, int Y) BossTile => (141, 62);
        public static (int X, int Y, int Width, int Height) BossWake => (131, 62, 40, 12);
        public const int ArenaFloorY = 62;
        public const float ArenaCeilingY = 79.5f;

        /// <summary>Os circulos da arena, nos estrados: e polindo um deles que o Alquimista vira prata.</summary>
        public static readonly List<(string Name, int X, int Y)> ArenaCircles = new List<(string, int, int)>();

        /// <summary>O chao entre os estrados, que vira mercurio na segunda metade da luta.</summary>
        public static readonly List<Cells> ArenaFloodPools = new List<Cells>();

        /// <summary>O ralo no meio da arena, por onde ele escorre ao cair.</summary>
        public static (int X, int Y) DrainTile => (142, 62);

        // A Cerimonia.
        public static (int X, int Y) StageCircle => (212, 76);

        // A Grande Escadaria, a leste do anfiteatro: sobe dos Ateliers ate o
        // palacio do Rei. Dois lances de degraus de meio tile, com um patamar
        // no meio; no alto, a porta do palacio (a saida pro District 5).
        public static (int X0, int Y0, int X1, int Y1) StairHall => (237, 76, 286, 97);

        /// <summary>Os lances: a primeira coluna, quantos degraus e a altura do chao de onde saem. Sobem pra direita.</summary>
        public static readonly List<(int X0, int Steps, int Base)> Flights = new List<(int, int, int)>();

        /// <summary>Os patamares: as colunas e a altura do piso.</summary>
        public static readonly List<(int X0, int X1, int Y)> Landings = new List<(int, int, int)>();

        /// <summary>
        /// Os meios-degraus: a coluna e a celula de baixo do meio tile. Pro
        /// mapa sao rocha; quem os desenha e os segura e o objeto da escadaria.
        /// </summary>
        public static readonly List<(int X, int Y)> HalfSteps = new List<(int, int)>();

        /// <summary>Onde o Scrap chega no alto, e a porta do palacio (a saida), em celulas.</summary>
        public static (int X, int Y) StairTop => (284, 94);
        public static (int X, int Y, int Width, int Height) PalaceDoor => (285, 94, 2, 3);

        public static readonly string[] CrisolPurifiedLines =
        {
            "Scrap. ...Ouviu? Nada escorrendo. Nenhuma gota. Pela primeira vez em anos, esse lugar tá quieto.",
            "Tá vendo esse palco? Foi aqui que recusaram você, sabia? Eu tava na terceira fila.",
            "Não aplaudi. Mas também não disse nada. Acho que já te contei que é isso que eu faço.",
            "A escadaria ali atrás sobe até o palácio. Até ele. ...Vai. Eu fico mais um pouco nessa poltrona.",
        };

        /// <summary>
        /// O que cada sala tem largado no chao, pela historia dela: livros de
        /// formula onde se estudava (o saguao, a galeria dos mestres, o ateliê
        /// do Crisol, as lentes), garrafoes e caixotes de reagente na
        /// destilaria, cacos onde os testes deram errado (tanques, viveiro). O
        /// Grande Laboratorio (a arena), o anfiteatro e a escadaria ficam como
        /// estao.
        /// </summary>
        private static readonly Dictionary<string, string[]> Clutter = new Dictionary<string, string[]>
        {
            ["01_Lobby"] = new[] { LabClutterArt.Books, LabClutterArt.Stool },
            ["02_MastersGallery"] = new[] { LabClutterArt.Books, LabClutterArt.Carboy },
            ["03_Atelier"] = new[] { LabClutterArt.Books, LabClutterArt.Stool, LabClutterArt.ReagentCrate },
            ["04a_StillHall"] = new[] { LabClutterArt.Carboy, LabClutterArt.ReagentCrate, LabClutterArt.Shards },
            ["04b_Serpentine"] = new[] { LabClutterArt.Carboy, LabClutterArt.Shards },
            ["04c_Condensers"] = new[] { LabClutterArt.Carboy, LabClutterArt.ReagentCrate },
            ["04d_GreatStill"] = new[] { LabClutterArt.ReagentCrate, LabClutterArt.Carboy },
            ["05a_Lenses"] = new[] { LabClutterArt.Books, LabClutterArt.Shards },
            ["05b_MirrorChamber"] = new[] { LabClutterArt.Stool, LabClutterArt.Shards },
            ["06a_Tanks"] = new[] { LabClutterArt.Shards, LabClutterArt.ReagentCrate },
            ["06b_Vivarium"] = new[] { LabClutterArt.Shards, LabClutterArt.Stool },
            ["06c_PrototypeWorkshop"] = new[] { LabClutterArt.ReagentCrate, LabClutterArt.Stool },
            ["07_PreBoss"] = new[] { LabClutterArt.Carboy, LabClutterArt.Books },
            ["09_GuardCorridor"] = new[] { LabClutterArt.Shards },
        };
        /// <summary>O desenho de cada peca do laboratorio, pelo nome (pra medir).</summary>
        private static Dictionary<string, PixelCanvas> _labArt;
    }
}
