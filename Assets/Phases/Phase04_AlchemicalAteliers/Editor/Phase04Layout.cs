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
    public static class Phase04Layout
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

        // ------------------------------------------------------------------
        // 1. O Saguao
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde a cabine do District 3 para. Um saguao de recepcao: o balcao,
        /// o cabide de jalecos, o portao de prata arrombado por dentro.
        ///
        /// A cabine chega de baixo: o poco desce do chao do saguao ate bem
        /// fundo, com os trilhos e uns lampioes de servico, e a camera sobe
        /// com o Scrap o caminho inteiro (ElevatorArrival).
        /// </summary>
        private static void BuildLobby()
        {
            const string room = "01_Lobby";
            Room(room, 3, 40, 30, 49);

            Carve(3, 40, 30, 49);

            // O poco. O chao da cabine, parada no andar, tampa a boca dele.
            (int shaftX, int bottom, int stop, int _) = ElevatorShaft;
            Carve(shaftX, bottom, shaftX + 2, stop - 2);
            for (int x = shaftX; x <= shaftX + 2; x++) ObjectCells.Add((x, stop - 1));

            for (int y = bottom + 5; y < stop - 2; y += 8) Lamp(shaftX + 1.5f, y, 3.5f);

            Tell("sign", room, "Lore_Portal", "a placa do portão", 20, 40,
                "O portão era de prata maciça. Alguém arrombou ele por dentro.",
                "Na placa, em letra de ourives: \"ATELIÊS ALQUÍMICOS DE SUA MAJESTADE. ENTRADA PROIBIDA A METAIS COMUNS.\"");

            Put("lab_coats", 14.5f, 40);
            Put("lab_lectern", 26f, 40);
            Put("lab_shelf", 29f, 40, 4);
            Hang("lab_chandelier", 17f, 50);

            Windows.Add((0, 23, 44));

            Lamp(13f, 45f, 8f);
            Lamp(24f, 45f, 8f);
        }

        // ------------------------------------------------------------------
        // 2. A Galeria dos Mestres
        // ------------------------------------------------------------------

        private static void BuildGallery()
        {
            const string room = "02_MastersGallery";
            Room(room, 31, 42, 63, 50);

            Carve(31, 42, 58, 50);

            // A porta do atelie: baixa, com o batente de marmore.
            Carve(59, 42, 63, 44);

            Tell("diary", room, "Lore_Diary_1", "diário de laboratório, folha do dia 12", 55, 42,
                "\"Dia 12. Descoberta: o mercúrio retarda a corrosão do Óxido. Mergulhei uma placa de prata no mercúrio e deixei do lado de outra, seca, no mesmo vapor verde.\"",
                "\"A seca começou a manchar no primeiro dia. A mergulhada aguentou três dias a mais. É pouco, mas é a primeira coisa que funciona.\"",
                "\"Sua Majestade foi informada. Há esperança. Vamos curar a cidade inteira.\"",
                "A letra é caprichada, de quem tá feliz.");

            Enemy(FoeKind.Slime, "Slime_Gallery", 31, 63, 42);

            Put("lab_bust", 32.5f, 42, 4);
            Put("lab_rack", 58f, 42, 4);
            Hang("lab_chandelier", 42.5f, 51);

            Lamp(37.5f, 47f, 7f);
            Lamp(47.5f, 47f, 7f);
            Lamp(56f, 46f, 5f);
        }

        // ------------------------------------------------------------------
        // 3. O Atelie de Crisol (o centro)
        // ------------------------------------------------------------------

        /// <summary>
        /// O centro: a bancada, Crisol e o Grande Alambique. Daqui saem as tres
        /// alas — a chamine da Optica no canto oeste, o poco da Destilaria no
        /// chao e a porta da Incubacao a leste — e, pelo mezanino, a grade do
        /// alambique, que leva ao chefe.
        /// </summary>
        private static void BuildAtelier()
        {
            const string room = "03_Atelier";
            Room(room, 64, 40, 100, 54);

            Carve(64, 40, 100, 54);

            // A chamine da Optica, no canto oeste: a parede de leste desce do teto
            // e deixa o pe livre pra passar.
            Solid(68, 44, 68, 54);
            Carve(64, 55, 67, 60);

            Bench(room, "Workbench_Atelier", 70, 40);
            Mark(room, "Crisol", CrisolTile.X, CrisolTile.Y);

            // O poco de servico pra Destilaria, no chao: cai no Salao dos Alambiques.
            Carve(80, 31, 83, 39);

            // O mezanino do alambique: um degrau e o patamar ate a grade.
            Ledge(86, 88, 43);
            Ledge(91, 100, 47);

            Mark(room, "Alembic", AlembicTile.X, AlembicTile.Y);
            Put("lab_great_alembic", 95.5f, 40, 3);

            // A porta da Incubacao, a leste, um degrau acima.
            Carve(101, 41, 104, 44);

            Put("lab_lens_bench", 79f, 40, 4);
            Put("lab_retort", 85.5f, 40, 5);
            Put("lab_shelf", 99f, 40, 4);
            Hang("lab_chandelier", 82f, 55);

            Lamp(71f, 44f, 8f);
            Lamp(78f, 45f, 8f);
            Lamp(94f, 46f, 7f, true);
        }

        // ------------------------------------------------------------------
        // 4. A Destilaria (embaixo)
        // ------------------------------------------------------------------

        /// <summary>
        /// Tres andares de laboratorio embaixo do atelie, feitos pra dar a volta:
        ///
        ///   a o Salao dos Alambiques, onde o poco de servico cai. Dali so se
        ///     sai pela porta oeste (e de volta pelo poco, quicando nas paredes).
        ///   b a Serpentina, a oeste: desce em zigue-zague pelas prateleiras,
        ///     com goteiras de mercurio e cacos no chao.
        ///   c os Condensadores, no fundo: corredor baixo de canos, de volta pro
        ///     leste, com tres fossos (o ultimo com um pilar no meio).
        ///   d a Torre do Grande Destilador, a leste: sobe-se por patamares ate
        ///     o Mercurio Destilado, no alto.
        ///
        /// Do alto da torre, uma passarela volta ao salao por cima, num
        /// patamar que de baixo e alto demais pra alcancar: o atalho de volta.
        /// </summary>
        private static void BuildDistillery()
        {
            const string hall = "04a_StillHall";
            const string serpentine = "04b_Serpentine";
            const string condensers = "04c_Condensers";
            const string tower = "04d_GreatStill";
            Room(hall, 48, 18, 100, 30);
            Room(serpentine, 16, 6, 47, 22);
            Room(condensers, 47, 1, 111, 12);
            Room(tower, 101, 6, 132, 31);

            // a. O Salao dos Alambiques.
            Carve(48, 18, 100, 30);

            // As paredes do poco descem dentro do salao e param a quatro tiles do
            // chao: da pra passar por baixo, e de baixo pular pra dentro do poco.
            Solid(79, 22, 79, 30);
            Solid(84, 22, 84, 30);

            Ledge(60, 63, 21);
            Enemy(FoeKind.FlaskThrower, "Thrower_Hall", 61, 61, 22);
            Enemy(FoeKind.FailedExperiment, "Failed_Hall", 38, 100, 18, 72);

            // O patamar do atalho, a leste: so se chega por cima, pela passarela.
            Ledge(92, 100, 25);

            // A porta oeste, pra Serpentina.
            Carve(47, 18, 47, 21);

            Put("lab_still", 52f, 18, 3);
            Put("lab_alembic", 56.5f, 18);
            Put("lab_burner", 66f, 18, 4);
            Put("lab_still", 71f, 18, 3);
            Put("lab_retort", 76.5f, 18, 4);
            Put("lab_rack", 88f, 18, 4);
            Put("lab_alembic", 92.5f, 18);
            Put("lab_still", 97f, 18, 3);
            Hang("lab_pipes", 58f, 31);
            Hang("lab_pipes", 70f, 31);
            Hang("lab_pipes", 95f, 31);

            Lamp(54f, 23f, 7f);
            Lamp(66f, 24f, 7f, true);
            Lamp(81.5f, 23f, 6f);
            Lamp(94f, 22f, 7f, true);
            Lamp(97f, 28f, 5f);

            // b. A Serpentina: entra-se numa prateleira alta e desce-se em
            // zigue-zague. No chao, uma faixa de cacos de vidro.
            Carve(16, 6, 46, 22);
            Solid(38, 14, 46, 17);
            Ledge(29, 34, 14);
            Ledge(19, 24, 10);
            Spikes(28, 34, 6);

            Drips.Add((22, 22, 11));
            Drips.Add((31, 22, 7));
            Drips.Add((41, 22, 3));

            Tell("chalk", serpentine, "Lore_Chalk_Mercury", "um quadro-negro", 43, 18,
                "Um quadro-negro cheio de contas. No meio, sublinhado três vezes: \"PRATA + MERCÚRIO = PRATA QUE NÃO APODRECE?\"");

            Enemy(FoeKind.FlaskThrower, "Thrower_Serpentine", 32, 32, 15);
            Enemy(FoeKind.Slime, "Slime_Serpentine", 16, 27, 6, 20);

            Put("lab_shelf", 39.5f, 18, 3);
            Put("lab_jar_1", 21.5f, 11, 4);
            Put("lab_alembic", 18.5f, 6);
            Put("lab_still", 24f, 6, 3);
            Hang("lab_pipes", 26f, 23);
            Hang("lab_chandelier", 36f, 23);

            Lamp(42f, 21f, 6f);
            Lamp(31f, 18f, 6f, true);
            Lamp(21f, 14f, 6f);
            Lamp(31f, 8f, 6f, true);

            // c. Os Condensadores: corredor baixo, de volta pro leste.
            Carve(47, 6, 111, 12);

            Carve(56, 3, 60, 5);
            Spikes(56, 60, 3);
            Carve(70, 3, 74, 5);
            Spikes(70, 74, 3);

            // O fosso de descarte, com um pilar no meio: atravessa-se pulando.
            Carve(88, 1, 99, 5);
            Spikes(88, 99, 1);
            Solid(93, 1, 94, 5);

            Ledge(77, 80, 9);

            Drips.Add((64, 12, 6));
            Drips.Add((83, 12, 6));
            Drips.Add((97, 12, 6));
            Drips.Add((106, 12, 6));

            Tell("diary", condensers, "Lore_Diary_Mercury", "diário de laboratório, folha do dia 40", 66, 6,
                "\"Dia 40. Sete destilações seguidas. O mercúrio sai tão puro que reflete o rosto de quem olha, e no reflexo não aparece mancha nenhuma de Óxido.\"",
                "\"Efeito colateral: o vapor do mercúrio é venenoso. Os assistentes de cobre desmaiam perto dos alambiques. O Mestre respira fundo e diz que é perfume.\"",
                "Tem uma marca de dedo prateado no canto da folha.");

            Enemy(FoeKind.FailedExperiment, "Failed_Condensers", 35, 55, 6, 50);
            Enemy(FoeKind.Slime, "Slime_Condensers", 61, 69, 6);
            Enemy(FoeKind.FlaskThrower, "Thrower_Condensers", 78, 78, 10);
            Enemy(FoeKind.Slime, "Slime_Condensers_2", 75, 87, 6, 84);

            Put("lab_burner", 50f, 6, 4);
            Put("lab_jar_0", 64f, 6, 4);
            Put("lab_rack", 82f, 6, 4);
            Put("lab_jar_2", 104f, 6, 4);
            Hang("lab_pipes", 52f, 13);
            Hang("lab_pipes", 66f, 13);
            Hang("lab_pipes", 85f, 13);
            Hang("lab_pipes", 103f, 13);

            Lamp(52f, 10f, 6f);
            Lamp(65f, 10f, 6f, true);
            Lamp(80f, 10f, 6f);
            Lamp(93.5f, 5f, 5f, true);
            Lamp(105f, 10f, 6f);

            // d. A Torre do Grande Destilador: patamares alternados ate o alto.
            Carve(112, 6, 132, 31);
            Ledge(126, 130, 9);
            Ledge(118, 122, 13);
            Ledge(126, 130, 17);
            Ledge(118, 122, 21);

            // O alto: o bloco do Grande Destilador, com o reagente.
            Solid(124, 22, 132, 25);
            AddReagent(tower, "Mercury", "MERCÚRIO DESTILADO", 0, 130, 26);

            // O atalho: a passarela que volta ao salao, por cima.
            Ledge(112, 117, 25);
            Carve(101, 26, 111, 29);

            Enemy(FoeKind.FailedExperiment, "Failed_Tower", 100, 132, 6, 122);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tower", 128, 128, 18);
            Enemy(FoeKind.Sentinel, "Sentinel_GreatStill", 124, 132, 26);

            Put("lab_still", 115f, 6, 3);
            Put("lab_great_tank", 122f, 6, 2);
            Put("lab_alembic", 126.5f, 26);
            Put("lab_shelf", 108f, 26, 3);
            Hang("lab_pipes", 120f, 32);
            Hang("lab_chandelier", 127f, 32);

            Lamp(116f, 10f, 6f);
            Lamp(126f, 13f, 6f, true);
            Lamp(120f, 20f, 6f);
            Lamp(128f, 29f, 7f, true);
            Lamp(106f, 28f, 5f);
        }

        // ------------------------------------------------------------------
        // 5. A Optica (em cima)
        // ------------------------------------------------------------------

        /// <summary>
        /// Em cima da galeria, pela chamine do atelie. Aqui a luz alquimica e
        /// dobrada em espelhos de prata que giram (E): a luz-guia, fria, so
        /// acende fechaduras de cristal; os raios-perigo, quentes, piscam em
        /// ritmo e queimam.
        ///
        ///   a a Sala das Lentes: dois raios-perigo na entrada e o primeiro
        ///     enigma, de um espelho so. A luz cai do teto num espelho em cima
        ///     de um pedestal; girado, ele a manda pra fechadura em cima da
        ///     grade da Camara dos Espelhos. O quadro-negro ensina.
        ///   b a Camara dos Espelhos, alta: cinco espelhos em dois lados de uma
        ///     escalada. A luz entra rente ao chao e tem que subir em
        ///     zigue-zague ate a fechadura do nicho da Luz Engarrafada, no
        ///     alto. Um desvio no penultimo espelho leva a luz pra uma
        ///     fechadura escondida na parede leste: o cofre do chassi.
        /// </summary>
        private static void BuildOptics()
        {
            const string lenses = "05a_Lenses";
            const string mirrors = "05b_MirrorChamber";
            Room(lenses, 30, 61, 70, 69);
            Room(mirrors, 3, 61, 29, 80);

            // a. A Sala das Lentes. A chamine do atelie chega pelo chao, a leste.
            Carve(30, 61, 70, 69);

            HazardBeam("Beam_Lenses_1", 50, 69, 0, -1, 1.6f, 1.4f, 0f);
            HazardBeam("Beam_Lenses_2", 54, 69, 0, -1, 1.6f, 1.4f, 1.5f);

            // O primeiro enigma: a luz cai do teto no espelho do pedestal. Em
            // "\" vai pro leste, contra a parede; em "/", pro oeste, ate a
            // fechadura em cima da grade.
            Solid(39, 61, 41, 63);
            GuideLight("Guide_Lenses", 40, 68, 0, -1);
            Mirror(lenses, "Mirror_Lenses", 40, 66, false, 40, 64);
            CrystalLock("Lock_Lenses", 30, 66, LensesGate);

            // A grade pra Camara dos Espelhos, um degrau acima do chao.
            Carve(29, 62, 29, 64);
            Gate(LensesGate, 29, 62, 1, 3);

            Tell("chalk", lenses, "Lore_Chalk_Light", "um quadro-negro", 35, 61,
                "Um quadro-negro com um desenho: um raio saindo de uma lâmpada, batendo num espelho inclinado e dobrando a esquina até um cristal.",
                "Do lado, em letra de aula: \"O espelho não cria a luz. Só escolhe pra onde ela vai.\"",
                "Embaixo, menor, com uma seta em volta do espelho: \"Gire. Siga o raio. O cristal abre.\"");

            Tell("diary", lenses, "Lore_Diary_Light", "diário de laboratório, folha do dia 150", 62, 61,
                "\"Dia 150. A luz alquímica atravessa o Óxido e mostra o que já apodreceu por dentro do metal. Testamos em placas de prata nobre: por dentro, estavam podres.\"",
                "\"Ou seja: a prata também tá apodrecendo, a cidade inteira tá. Só demora mais pra aparecer. Sua Majestade proibiu que a luz fosse apontada pra Corte.\"",
                "\"Aviso pra quem trabalha aqui: as lâmpadas quentes queimam, andem entre um clarão e outro. A fria, a dos espelhos, só aponta o caminho.\"");

            Enemy(FoeKind.Sentinel, "Sentinel_Lenses", 30, 38, 61, 34);
            Ledge(56, 59, 64);
            Enemy(FoeKind.FlaskThrower, "Thrower_Lenses", 57, 57, 65);

            Put("lab_lens_bench", 45f, 61, 4);
            Put("lab_prism", 59f, 61, 4);
            Hang("lab_chandelier", 46f, 70);

            Lamp(34f, 66f, 7f);
            Lamp(45f, 66f, 6f);
            Lamp(60f, 66f, 7f);

            // b. A Camara dos Espelhos.
            Carve(9, 61, 28, 80);

            // A escalada: patamares nos dois lados, deixando livres as colunas
            // e as linhas por onde a luz passa (coluna 12, coluna 24, linhas
            // 62, 70 e 78).
            Ledge(16, 20, 64);
            Ledge(13, 15, 68);
            Ledge(25, 27, 68);
            Ledge(16, 20, 72);
            Ledge(13, 15, 76);
            Ledge(9, 11, 76);
            Ledge(25, 28, 76);

            // A luz entra rente ao chao, pelo leste.
            GuideLight("Guide_Mirrors", 27, 62, -1, 0);

            //   A (12,62)  "/" manda pro chao;      "\" sobe a coluna 12.
            //   B (12,70)  "\" manda pra parede;    "/" vai pro leste.
            //   C (24,70)  "\" manda pro chao;      "/" sobe a coluna 24.
            //   D (24,78)  "\" vai pro oeste;       "/" vai pro leste: o segredo.
            //   E (12,78)  "/" desce de volta;      "\" sobe: a fechadura do nicho.
            Mirror(mirrors, "Mirror_A", 12, 62, true, 13, 61);
            Mirror(mirrors, "Mirror_B", 12, 70, false, 13, 69);
            Mirror(mirrors, "Mirror_C", 24, 70, false, 25, 69);
            Mirror(mirrors, "Mirror_D", 24, 78, false, 25, 77);
            Mirror(mirrors, "Mirror_E", 12, 78, true, 13, 77);

            // O nicho da Luz Engarrafada, gradeado; a fechadura em cima da entrada.
            Carve(3, 77, 8, 79);
            Gate(MirrorsGate, 8, 77, 1, 3);
            CrystalLock("Lock_Mirrors", 12, 80, MirrorsGate);
            AddReagent(mirrors, "Light", "LUZ ENGARRAFADA", 1, 5, 77);

            // O cofre: um bloco falso na parede leste e, na frente dele, uma
            // fechadura apagada, quase da cor do marmore.
            Carve(30, 77, 36, 80);
            Carve(29, 77, 29, 79);
            Gate(VaultGate, 29, 77, 1, 3, 0, 1, true);
            CrystalLock("Lock_Vault", 28, 78, VaultGate, true);
            Mark(mirrors, "Secret_Chassis", SecretChassisTile.X, SecretChassisTile.Y);

            Enemy(FoeKind.Sentinel, "Sentinel_Mirror", 9, 28, 61, 20);
            Enemy(FoeKind.Sentinel, "Sentinel_Mirror_Ledge", 16, 20, 65);
            Enemy(FoeKind.FlaskThrower, "Thrower_Mirror", 26, 26, 69);

            Put("lab_prism", 22f, 61, 4);
            Put("lab_lens_bench", 16.5f, 61, 4);
            Put("lab_mirror", 26f, 61, 3);
            Put("lab_lens_bench", 18f, 73, 4);
            Put("lab_shelf", 33f, 77, 3);
            Hang("lab_chandelier", 18f, 81);

            Lamp(18f, 64f, 6f);
            Lamp(19f, 71f, 6f);
            Lamp(18f, 78f, 6f);
            Lamp(5f, 79f, 4f);
            Lamp(33f, 79f, 3f);
        }

        // ------------------------------------------------------------------
        // 6. A Incubacao (a leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Os tanques de cultura, cada um com uma cobaia de cobre dentro (dois
        /// rachados, com os cacos no fosso); o bioterio, mais fundo, com uma
        /// vala de cacos e as gaiolas por cima; e no fim a Oficina de
        /// Prototipos: seis mesas com seis tentativas desmontadas, e a setima,
        /// vazia. A Assistente se escondeu no armario de reagentes.
        /// </summary>
        private static void BuildIncubation()
        {
            const string tanks = "06a_Tanks";
            const string vivarium = "06b_Vivarium";
            const string workshop = "06c_PrototypeWorkshop";
            Room(tanks, 101, 38, 150, 46);
            Room(vivarium, 151, 34, 180, 46);
            Room(workshop, 181, 36, 215, 44);

            Carve(105, 41, 150, 46);

            // Os tanques rachados: o vidro escorreu pro fosso embaixo deles.
            Carve(116, 38, 119, 40);
            Spikes(116, 119, 38);
            Carve(130, 38, 133, 40);
            Spikes(130, 133, 38);

            for (int i = 0; i < 7; i++) Put("lab_tank_" + (i % 3), 108.5f + i * 6.5f, 41, 3);

            Ledge(118, 121, 44);
            Ledge(138, 141, 44);

            Enemy(FoeKind.FailedExperiment, "Failed_Tanks_1", 120, 129, 41, 125);
            Enemy(FoeKind.FailedExperiment, "Failed_Tanks_2", 134, 150, 41, 142);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tanks_1", 119, 119, 45);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tanks_2", 139, 139, 45);

            Tell("diary", tanks, "Lore_Diary_2", "diário de laboratório, folha do dia 90", 110, 41,
                "\"Dia 90. Conclusão ruim: o mercúrio só atrasa o Óxido, não cura. Em placa, a gente ganha dias. Precisamos saber o que acontece num corpo inteiro.\"",
                "\"Sua Majestade autorizou testes em cobaias de cobre. Disse que tem muitos, e que ninguém vai sentir falta.\"");

            Drips.Add((124, 46, 5));
            Drips.Add((145, 46, 5));

            Lamp(111f, 44f, 6f, true);
            Lamp(117.5f, 40f, 4f, true);
            Lamp(124f, 44f, 6f, true);
            Lamp(131.5f, 40f, 4f, true);
            Lamp(137f, 44f, 6f, true);
            Lamp(147f, 44f, 6f);

            // O bioterio, mais fundo, com a vala de cacos no meio e uma gaiola
            // grande por cima dela.
            Carve(151, 34, 180, 46);
            Ledge(151, 154, 37);
            Spikes(160, 169, 34);
            Ledge(163, 166, 37);

            Enemy(FoeKind.Slime, "Slime_Vivarium_1", 151, 159, 34, 155);
            Enemy(FoeKind.Slime, "Slime_Vivarium_2", 170, 180, 34, 175);

            Tell("diary", vivarium, "Lore_Diary_3", "diário de laboratório, folha do dia 210", 177, 34,
                "\"Dia 210. Teste em corpo inteiro. O tanque 4 não acordou. O tanque 7 acordou errado. Juntamos o que sobrou do 7 com o que sobrou do 9.\"",
                "\"O mercúrio entra no corpo e não sai mais. O Mestre diz que estamos quase lá. Ele não dorme. E a prata dele tá ficando mole, brilhante demais.\"",
                "A letra piorou muito.");

            Put("lab_cage", 156f, 34, 4);
            Put("lab_cage_broken", 164.5f, 38, 4);
            Put("lab_cage", 173f, 34, 4);
            Put("lab_table", 178f, 34, 4);
            Hang("lab_pipes", 165f, 47);

            Lamp(157f, 40f, 6f, true);
            Lamp(164.5f, 41f, 7f);
            Lamp(176f, 39f, 6f);

            // A Oficina de Prototipos.
            Carve(181, 36, 215, 44);

            for (int n = 0; n < 6; n++) Put("lab_proto_table_" + n, 190f + n * 3.5f, 36, 4);
            Put("lab_proto_table_6", 211.5f, 36, 4);

            Tell("assistant", workshop, "Npc_Assistant", "Assistente", 184, 36,
                "N-não abre! ...Ah. Você não é um deles. Você é... de bronze? De pedra?",
                "Eu era a aprendiz do Mestre. Do Alquimista. Tô trancada aqui desde que ele começou a escorrer.",
                "Ele não era cruel, tá? Você precisa saber disso. Ele chorou no primeiro tanque. Queria curar todo mundo, até a gente, de cobre.",
                "Mas o Rei mandava, e ele obedecia. Cada ordem um pouco pior que a outra. No fim, ele obedecia antes de o Rei pedir.",
                "Ele tá no Grande Laboratório, depois do alambique. Enquanto tá líquido, nada acerta ele. Mas os círculos do chão... eles prendem ele em prata.");

            Tell("board", workshop, "Lore_PrototypeBoard", "o quadro das fichas", 189, 36,
                "Um quadro de cortiça com seis fichas presas por alfinete, uma pra cada mesa.",
                "N.º 1: \"Ferro fundido. Pesado demais pra andar.\" N.º 2: \"Latão. O Óxido pegou em três dias.\"",
                "N.º 3: \"Cobre. Cobaia forçada.\"",
                "N.º 4: \"Vidro. Quebrou ao se ver no espelho.\" N.º 5: \"Prata nobre. Apodreceu antes de acordar.\"",
                "N.º 6: \"Quartzo lapidado. Perfeito. Não aguentou o primeiro passo.\"",
                "Tem um sétimo alfinete, sem ficha nenhuma.");

            Tell("diary", workshop, "Lore_Diary_5", "diário de laboratório, folha do dia 300", 200, 36,
                "\"Dia 300. Descoberta principal: os protótipos brutos são imunes. Quartzo sem lapidar, bronze sem polimento. O Óxido não tem onde se agarrar neles.\"",
                "\"Isso é a cura. Um corpo bruto pra cada um da cidade, e ninguém mais apodrece.\"",
                "\"Levei o n.º 7 até o trono, andando. Imune. Sua Majestade recusou: disse que a prata não vai ser salva por sucata. Ordem de descarte assinada.\"");

            Tell("sheet", workshop, "Lore_PrototypeSheet", "ficha de protótipo", 208, 36,
                "Uma prancheta. Um desenho técnico, feito com capricho: cabeça de quartzo bruto, dois olhos, uma mola.",
                "É você. É o seu corpo, cotado em milímetros.",
                "\"PROTÓTIPO BRUTO N.º 7. IMUNE AO ÓXIDO. FUNCIONAL.\"",
                "E, por cima de tudo, carimbado em vermelho: \"REJEITADO — IMPERFEITO\".",
                "No campo de destino, impresso: \"POÇO DE SUCATA\".");

            AddReagent(workshop, "Salt", "SAL DE QUARTZO", 2, 213, 36);

            Enemy(FoeKind.Sentinel, "Sentinel_Workshop", 181, 215, 36, 196);

            Put("lab_shelf", 182.5f, 36, 3);
            Hang("lab_chandelier", 200f, 45);

            Lamp(186f, 40f, 6f);
            Lamp(198f, 41f, 8f);
            Lamp(210f, 40f, 7f);
        }

        // ------------------------------------------------------------------
        // 7. A subida pro chefe
        // ------------------------------------------------------------------

        /// <summary>
        /// Do mezanino do atelie, a grade do alambique; atras dela um corredor
        /// curto e uma chamine ate a antessala do Grande Laboratorio, com a
        /// bancada.
        /// </summary>
        private static void BuildBossRoute()
        {
            const string room = "07_PreBoss";
            Room(room, 100, 48, 128, 68);

            Carve(101, 48, 112, 51);
            Gate(AlembicGate, 101, 48, 1, 4);

            Carve(109, 52, 112, 61);

            // Dois raios-perigo atravessam a chamine: sobe-se quicando entre um clarao e outro.
            HazardBeam("Beam_Chimney_Low", 109, 55, 1, 0, 1.2f, 1.6f, 0f);
            HazardBeam("Beam_Chimney_High", 109, 59, 1, 0, 1.2f, 1.6f, 1.4f);

            Carve(100, 62, 128, 68);
            Bench(room, "Workbench_PreBoss", 116, 62);

            Tell("diary", room, "Lore_Diary_4", "diário de laboratório, folha do dia 255", 104, 62,
                "\"Dia 255. O Mestre entrou no tanque grande, sozinho. A ideia dele: se o mercúrio atrasa o Óxido, então quem for feito de mercúrio não apodrece nunca.\"",
                "\"Ele saiu. Fala como antes, educado. Mas escorre. E quando ri, o riso continua depois que a boca fecha.\"",
                "\"Os guardas de prata perfeita passaram a vigiar o laboratório. Não sei se tão protegendo a gente ou ele.\"");

            Put("lab_shelf", 122f, 62, 4);
            Put("lab_jar_1", 125.5f, 62, 4);

            Lamp(106f, 65f, 6f);
            Lamp(118f, 66f, 7f);
        }

        // ------------------------------------------------------------------
        // 8. O Grande Laboratorio
        // ------------------------------------------------------------------

        /// <summary>
        /// A arena do Alquimista: alta, com tanques nas paredes e tres estrados
        /// de marmore, cada um com um circulo de transmutacao. No meio, o ralo.
        /// Na segunda metade da luta o chao entre os estrados vira mercurio.
        /// </summary>
        private static void BuildLaboratory()
        {
            const string room = "08_GrandLaboratory";
            Cells arena = ArenaRoom;
            Room(room, arena.X0, arena.Y0, arena.X1, arena.Y1);

            Carve(arena.X0, arena.Y0, arena.X1, arena.Y1);

            // Cantos de cima chanfrados.
            Solid(129, 77, 132, 80);
            Solid(167, 77, 170, 80);

            int[] daises = { 132, 147, 162 };
            for (int i = 0; i < daises.Length; i++)
            {
                int x = daises[i];
                Solid(x, 62, x + 4, 63);
                ArenaCircles.Add(("ArenaCircle_" + (i + 1), x + 2, 64));
                Mark(room, "ArenaCircle_" + (i + 1), x + 2, 64);
            }

            ArenaFloodPools.Add(new Cells(129, 61, 131, 61));
            ArenaFloodPools.Add(new Cells(137, 61, 146, 61));
            ArenaFloodPools.Add(new Cells(152, 61, 161, 61));
            ArenaFloodPools.Add(new Cells(167, 61, 170, 61));

            Mark(room, "BossSpawn_MercuryAlchemist", BossTile.X, BossTile.Y);

            Put("lab_tank_2", 140f, 62, 2);
            Put("lab_tank_0", 156f, 62, 2);
            Put("lab_great_tank", 149.5f, 64, 1);
            Put("lab_drain", DrainTile.X + 0.5f, DrainTile.Y, 6);
            Hang("lab_chandelier", 149.5f, 81);
            Hang("lab_pipes", 138f, 81);
            Hang("lab_pipes", 161f, 81);

            Windows.Add((1, 141, 70));
            Windows.Add((0, 157, 70));

            Lamp(135f, 70f, 8f, true);
            Lamp(149.5f, 74f, 11f);
            Lamp(164f, 70f, 8f, true);

            // A saida, gradeada ate o Alquimista cair.
            Carve(171, 62, 171, 66);
            Gate(BossExitGate, 171, 62, 1, 5);
        }

        // ------------------------------------------------------------------
        // 9. O corredor de saida
        // ------------------------------------------------------------------

        /// <summary>
        /// Da saida da arena ao anfiteatro: um corredor com dois fossos e, no
        /// fim, uma chamine.
        /// </summary>
        private static void BuildGuardCorridor()
        {
            const string room = "09_GuardCorridor";
            Room(room, 172, 59, 226, 75);

            Carve(172, 62, 225, 67);

            Carve(182, 59, 186, 61);
            Spikes(182, 186, 59);
            Carve(196, 59, 201, 61);
            Spikes(196, 201, 59);

            // A chamine pro anfiteatro: a parede de oeste desce ate a altura da cabeca.
            Solid(221, 66, 221, 67);
            Carve(222, 68, 225, 75);

            Put("lab_tank_2", 177f, 62, 3);
            Put("lab_cage_broken", 191f, 62, 4);
            Put("lab_shelf", 207f, 62, 4);
            Put("lab_rack", 215f, 62, 4);

            Lamp(178f, 65f, 6f);
            Lamp(191f, 65f, 6f);
            Lamp(206f, 65f, 6f);
            Lamp(223.5f, 71f, 5f);
        }

        // ------------------------------------------------------------------
        // 10. O Anfiteatro de Demonstracoes
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde a nobreza assistia aos experimentos, e onde o Rei recusou o
        /// prototipo. A arquibancada a oeste, o palco no meio com o grande
        /// circulo (o coracao do distrito), o pulpito, e a leste a passagem pra
        /// Grande Escadaria.
        /// </summary>
        private static void BuildAmphitheater()
        {
            const string room = "10_Amphitheater";
            Room(room, 190, 76, 236, 88);

            Carve(190, 76, 236, 88);

            // A arquibancada, em degraus.
            Solid(190, 76, 195, 79);
            Solid(196, 76, 201, 77);

            Mark(room, "StageCircle", StageCircle.X, StageCircle.Y);

            Tell("plaque", room, "Lore_Amphitheater", "a placa do palco", 205, 76,
                "Uma placa de prata na beira do palco: \"TRIBUNAL DE INOVAÇÕES. AQUI A CIÊNCIA SE APRESENTA A SUA MAJESTADE.\"",
                "Nos degraus, almofadas de veludo, ainda arrumadas. Ninguém se levantou pra ir embora; só pararam de vir.");

            Tell("decree", room, "Lore_DiscardOrder", "a ordem no púlpito", 218, 76,
                "Um papel timbrado com o selo de prata do Rei, preso no púlpito do palco.",
                "\"O protótipo bruto foi apresentado a este Tribunal. É imune ao Óxido. Isso não muda nada.\"",
                "\"Cravejada está apodrecendo, distrito por distrito. Que apodreça. Prefiro uma cidade podre a uma cidade salva por sucata.\"",
                "\"Destino de todo o lote: Poço de Sucata. Que não se fale mais nisso.\"",
                "Assinado, embaixo, numa letra que não treme.");

            // A passagem pra Grande Escadaria, na parede leste.
            Carve(237, 76, 239, 81);

            Put("lab_seats", 192.5f, 80, 3);
            Put("lab_seats", 198.5f, 78, 3);
            Put("lab_stage", StageCircle.X + 0.5f, StageCircle.Y, 2);
            Put("lab_lectern", 218f, 76, 4);
            Hang("lab_chandelier", 212.5f, 89);

            Windows.Add((1, 205, 81));
            Windows.Add((0, 222, 81));

            Lamp(193f, 84f, 7f);
            Lamp(212.5f, 84f, 9f);
            Lamp(229f, 82f, 7f);

            // Depois da purificacao: a Assistente sai do armario e senta na
            // arquibancada; Crisol vem assistir.
            Purified.Add(new LoreSpot
            {
                Kind = "standing", Name = "Npc_Assistant_Purified", Title = "Assistente", X = 193, Y = 80,
                Lines = new[]
                {
                    "Saí do armário. As pernas ainda não acreditam.",
                    "O Mestre... eu vi pelo vidro. Ele não gritou. Pediu desculpa, eu acho. Pro mercúrio, pra ele mesmo, sei lá.",
                    "Você era o n.º 7, né? Eu limpei a sua mesa no dia em que te levaram. Guardei a mola que sobrou.",
                    "Mentira, não guardei. Joguei fora, como mandaram. Mas eu lembro dela. Isso conta alguma coisa?",
                },
            });

            Purified.Add(new LoreSpot
            {
                Kind = "crisol", Name = "Npc_Crisol_Purified", Title = "Crisol", X = 199, Y = 78,
                Lines = CrisolPurifiedLines,
            });
        }

        // ------------------------------------------------------------------
        // 11. A Grande Escadaria
        // ------------------------------------------------------------------

        /// <summary>
        /// Um salao alto de marmore, a leste do anfiteatro: dois lances largos
        /// de degraus baixos, com a passadeira vermelha e a balaustrada
        /// dourada, um patamar no meio e, no alto, a porta do palacio. Os
        /// degraus sao de meio tile: o Scrap sobe andando.
        /// </summary>
        private static void BuildGrandStair()
        {
            const string room = "11_GrandStair";
            (int x0, int y0, int x1, int y1) = StairHall;
            Room(room, x0, y0, x1, y1);

            Carve(x0 + 3, y0, x1, y1);

            Flight(243, 18, y0);
            Landing(261, 264, 85);
            Flight(265, 18, 85);
            Landing(283, 286, 94);

            Mark(room, "Exit_District5", StairTop.X, StairTop.Y);

            Hang("lab_chandelier", 252f, 98);
            Hang("lab_chandelier", 272f, 98);
            Windows.Add((1, 246, 84));
            Windows.Add((0, 256, 90));
            Windows.Add((1, 268, 92));

            Lamp(244f, 80f, 6f);
            Lamp(256f, 88f, 7f);
            Lamp(270f, 92f, 7f);
            Lamp(284f, 96f, 6f);
        }

        /// <summary>
        /// Um lance subindo pra direita: cada coluna meio tile acima da
        /// anterior. Por baixo e marmore macico (tile); a metade de cima de
        /// cada degrau impar e um meio-degrau (objeto).
        /// </summary>
        private static void Flight(int x0, int steps, int floor)
        {
            Flights.Add((x0, steps, floor));

            for (int i = 0; i < steps; i++)
            {
                int x = x0 + i;
                int halves = i + 1;
                int full = halves / 2;

                // Marmore macico do chao do salao ate o degrau.
                int solidTop = floor + full - 1;
                if (solidTop >= StairHall.Y0) Solid(x, StairHall.Y0, x, solidTop);

                if (halves % 2 == 1)
                {
                    int cell = floor + full;
                    Solid(x, cell, x, cell);
                    ObjectCells.Add((x, cell));
                    HalfSteps.Add((x, cell));
                }
            }
        }

        private static void Landing(int x0, int x1, int y)
        {
            Landings.Add((x0, x1, y));
            Solid(x0, StairHall.Y0, x1, y - 1);
        }

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

            return problems;
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
