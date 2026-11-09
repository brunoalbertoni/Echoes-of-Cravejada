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
    ///   A ENTRADA     1 o Saguao (o elevador do District 3) e 2 a Galeria
    ///                 dos Mestres, com os retratos.
    ///   O CENTRO      3 o Atelie de Crisol: bancada, Crisol, a loja e o
    ///                 Grande Alambique, que tranca o caminho pro chefe e pede
    ///                 tres reagentes.
    ///   AS ALAS       em qualquer ordem, cada uma termina num reagente:
    ///                 4 a Destilaria (embaixo): mercurio e circulos.
    ///                 5 a Optica (em cima): espelhos e raio de luz.
    ///                 6 a Incubacao (a leste): tanques, bioterio e a Oficina
    ///                   de Prototipos, onde ficou a ficha do n.º 7.
    ///   O CHEFE       7 o pre-boss e 8 o Grande Laboratorio: o Alquimista de
    ///                 Mercurio, que so apanha transmutado em prata.
    ///   DEPOIS        9 o corredor de saida e 10 o Anfiteatro
    ///                 de Demonstracoes, onde o circulo do palco purifica o
    ///                 distrito. Do anfiteatro, a Grande Escadaria sobe ate o palacio (District 5).
    ///
    /// Tres coisas nao sao tile, e sim objeto que aparece ou some: as pocas de
    /// mercurio (pro mapa, chao comum) e o que os circulos de transmutacao
    /// mexem — a parede de vidro que vira ponte de prata, a poca que vira
    /// vapor, o bloco de marmore que vira areia.
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
        /// Um raio de luz alquimica que liga e desliga em ritmo: perigo, nao
        /// enigma. Sai do emissor e vai reto ate bater em parede. Fica On
        /// segundos aceso, Off apagado, comecando com Offset de atraso.
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
        }

        /// <summary>Um portao de grade: as celulas que fecha e pra onde corre ao abrir.</summary>
        public sealed class GateSpec
        {
            public string Name;
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int MoveX;
            public int MoveY = 1;
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

        public static (int X, int Y) CrisolTile => (76, 40);

        public static readonly string[] CrisolLines =
        {
            "...Scrap.",
            "Não mexa em nada. Esta bancada era minha. Aquele torno era meu. Aquela lupa, eu mesmo a poli.",
            "Eu cravei gemas para metade dos retratos que você viu na galeria. Depois me penduraram lá também.",
            "Quando a praga começou, os alquimistas vieram pedir-me quartzo bruto. Bruto. Eu achei que era piada.",
            "Não era. Eles estavam construindo alguma coisa. Eu vi os desenhos. Eu sabia o que eles tinham achado.",
            "E, quando o Rei mandou jogar tudo fora, eu não disse nada. Fiquei quieto, para continuar pendurado na parede.",
            "Aquele alambique ali pede três reagentes. Um na destilaria, embaixo; um na óptica, em cima; e o sal, lá na incubação.",
            "Não adiantou ficar quieto. E você está aqui. ...Descanse, Scrap. A bancada ainda funciona.",
        };

        /// <summary>O Grande Alambique: onde o Scrap entrega os reagentes (de pe, ao lado dele).</summary>
        public static (int X, int Y) AlembicTile => (94, 40);

        /// <summary>O portao que o alambique abre: no fim do mezanino, a leste.</summary>
        public const string AlembicGate = "Gate_Alembic";

        /// <summary>A grade da saida do Grande Laboratorio: abre quando o Alquimista cai.</summary>
        public const string BossExitGate = "Gate_BossExit";

        /// <summary>O reforco de chassi no nicho em cima da Destilaria.</summary>
        public static (int X, int Y) SecretChassisTile => (30, 35);

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
            "Scrap. ...Você ouviu? Nada escorrendo. Nenhuma gota. Pela primeira vez em anos, este lugar está quieto.",
            "Eu vi o círculo do palco acender. É o mesmo palco onde eles recusaram você, sabia? Eu estava na terceira fila.",
            "Não aplaudi. Também não disse nada. Acho que já contei a você que é isso que eu faço.",
            "A escadaria ali atrás sobe até o palácio. Até ele. ...Vá. Eu fico mais um pouco nesta poltrona.",
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

        private static void Gate(string name, int x, int y, int width, int height, int moveX = 0, int moveY = 1)
        {
            Gates.Add(new GateSpec { Name = name, X = x, Y = y, Width = width, Height = height, MoveX = moveX, MoveY = moveY });
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
        /// </summary>
        private static void BuildLobby()
        {
            const string room = "01_Lobby";
            Room(room, 3, 40, 30, 49);

            Carve(3, 40, 30, 49);

            Tell("sign", room, "Lore_Portal", "a placa do portão", 20, 40,
                "O portão era de prata maciça. Alguém o arrombou por dentro.",
                "Na placa, em letra de ourives: \"ATELIÊS ALQUÍMICOS DE SUA MAJESTADE. ENTRADA VEDADA A METAIS COMUNS.\"",
                "Embaixo, riscado a prego: \"e a quem mais?\"");

            // A cabine que subiu do District 3, parada onde o Scrap desce dela.
            Put("elevator", SpawnTile.X, SpawnTile.Y, -4);

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

            Tell("portrait0", room, "Lore_Portrait_1", "retrato de um Nobre", 35, 42,
                "Um Nobre de prata, de gola alta. O rosto foi riscado em X com alguma coisa verde.",
                "Na plaqueta: \"Mestre de Fundição\".");

            Tell("portrait1", room, "Lore_Portrait_2", "retrato de uma Nobre", 40, 42,
                "Uma Nobre de prata. Também riscada, com mais força: o risco rasgou a tela.",
                "Na plaqueta: \"Tesoureira da Coroa\".");

            Tell("portrait2", room, "Lore_Portrait_Crisol", "retrato de um jovem", 45, 42,
                "Este ninguém riscou. Um jovem de prata lisa, sem um arranhão, de monóculo.",
                "Na plaqueta: \"Crisol. Lapidário da Corte.\"",
                "Alguém arrancou a plaqueta de baixo, a que dizia o título de nobreza.");

            Tell("portrait0", room, "Lore_Portrait_3", "retrato de um Nobre", 50, 42,
                "Um Nobre de jaleco e máscara de bico. Riscado, mas só de leve — quase com carinho.",
                "Na plaqueta: \"Alquimista-Chefe\".");

            Tell("diary", room, "Lore_Diary_1", "diário de laboratório, folha do dia 12", 55, 42,
                "\"Dia 12. O mercúrio retarda a corrosão! Uma placa de prata mergulhada resistiu três dias a mais.\"",
                "\"Sua Majestade foi informada. Há esperança. Vamos curar a cidade inteira.\"",
                "A letra é caprichada, de quem está feliz.");

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

            // O poco de servico pra Destilaria, no chao.
            Carve(80, 21, 83, 39);

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
        /// Um salao comprido de alambiques e serpentinas, embaixo do atelie.
        /// O poco de servico cai no meio dele. A oeste, as pocas: a corrompida
        /// arrasta pros espinhos; a calma, transmutada em vapor, leva ao nicho
        /// do chassi. A leste, os dois circulos: o vidro vira ponte de prata
        /// sobre o fosso, e o marmore vira areia na porta do Grande Destilador,
        /// onde fica o Mercurio Destilado.
        /// </summary>
        private static void BuildDistillery()
        {
            const string west = "04a_Stills";
            const string east = "04b_GreatStill";
            Room(west, 18, 20, 78, 28);
            Room(east, 79, 20, 130, 28);

            Carve(18, 20, 130, 28);

            // As paredes do poco de servico: dentro do salao, sobem a partir do
            // pe (que fica livre pra passar).
            Solid(79, 24, 79, 28);
            Solid(84, 24, 84, 28);

            // As pocas.
            Mercury("Mercury_Still", 22, 29, 19);
            Carve(36, 17, 38, 19);
            Spikes(36, 38, 17);
            Mercury("Mercury_Flow", 40, 52, 19, -2.8f);

            Drips.Add((44, 28, 8));
            Drips.Add((49, 28, 8));
            Drips.Add((66, 28, 8));

            // O segredo: transmutar a poca calma em vapor. A coluna sobe por uma
            // chamine ate um nicho, embaixo do saguao.
            Carve(23, 29, 26, 37);
            Carve(27, 35, 32, 37);
            VaporLifts.Add(("Vapor_Still", new Cells(23, 18, 26, 37)));

            // A volta: o vapor nao deixa descer pela chamine, entao o fim do
            // nicho, logo depois do chassi, e um alcapao que cai de novo na
            // Destilaria. De baixo e alto demais pra subir.
            Carve(31, 29, 32, 34);

            Rune(west, "Circle_Vapor", "mercúrio em vapor", 32, 20,
                new[] { "Mercury_Still" }, new[] { "Vapor_Still" });

            Mark(west, "Secret_Chassis", SecretChassisTile.X, SecretChassisTile.Y);

            // O fosso de descarte, com um pilar no meio: atravessa-se pulando.
            Carve(96, 14, 105, 19);
            Spikes(96, 105, 14);
            Solid(100, 14, 101, 19);

            // A porta do Grande Destilador, baixa.
            Solid(119, 24, 120, 28);

            AddReagent(east, "Mercury", "MERCÚRIO DESTILADO", 0, 126, 20);

            Tell("chalk", west, "Lore_Chalk_Mercury", "um quadro-negro", 70, 20,
                "Um quadro-negro cheio de contas. No meio, sublinhado três vezes: \"PRATA + MERCÚRIO = PRATA QUE NÃO APODRECE?\"",
                "Embaixo, em outra letra: \"Não apodrece. Escorre.\"");

            Tell("diary", east, "Lore_Diary_Mercury", "diário de laboratório, folha do dia 40", 114, 20,
                "\"Dia 40. Sete destilações. O mercúrio sai tão puro que reflete o rosto de quem olha — e o rosto não tem uma mancha de Óxido.\"",
                "\"Os assistentes de cobre desmaiam com o vapor. O Mestre respira fundo e diz que é perfume.\"",
                "Há uma marca de dedo prateado no canto da folha.");

            // Os bichos.
            Enemy(FoeKind.Slime, "Slime_Still_1", 18, 35, 20);
            Enemy(FoeKind.Slime, "Slime_Still_2", 39, 95, 20, 54);
            Enemy(FoeKind.FailedExperiment, "Failed_Still", 39, 95, 20, 76);
            Ledge(58, 61, 23);
            Enemy(FoeKind.FlaskThrower, "Thrower_Still", 59, 59, 24);
            Enemy(FoeKind.FailedExperiment, "Failed_GreatStill", 106, 130, 20);
            Ledge(111, 114, 23);
            Enemy(FoeKind.FlaskThrower, "Thrower_GreatStill", 112, 112, 24);

            // O laboratorio: alambiques, serpentinas, bicos de gas.
            Put("lab_alembic", 19.5f, 20);
            Put("lab_shelf", 30.5f, 20, 3);
            Put("lab_burner", 34f, 20, 4);
            Put("lab_still", 56f, 20, 3);
            Put("lab_rack", 60.5f, 20, 4);
            Put("lab_alembic", 64f, 20);
            Put("lab_table", 69f, 20, 3);
            Put("lab_shelf", 75f, 20, 3);
            Put("lab_retort", 77.5f, 20, 4);
            Put("lab_burner", 87f, 20, 4);
            Put("lab_jar_1", 92.5f, 20, 4);
            Put("lab_jar_0", 107.5f, 20, 4);
            Put("lab_alembic", 113.5f, 20);
            Put("lab_jar_2", 117.5f, 20, 4);
            Put("lab_still", 125.5f, 20, 3);
            Put("lab_rack", 129f, 20, 4);
            Hang("lab_pipes", 29f, 29);
            Hang("lab_pipes", 52f, 29);
            Hang("lab_pipes", 72f, 29);
            Hang("lab_pipes", 112f, 29);

            Lamp(20f, 25f, 7f);
            Lamp(46f, 22f, 8f, true);
            Lamp(56f, 25f, 8f);
            Lamp(72f, 25f, 7f);
            Lamp(90f, 25f, 7f);
            Lamp(100f, 17f, 6f, true);
            Lamp(112f, 25f, 7f);
            Lamp(126f, 25f, 8f, true);
            Lamp(29f, 37f, 4f);
        }

        // ------------------------------------------------------------------
        // 5. A Optica (em cima)
        // ------------------------------------------------------------------

        /// <summary>
        /// Em cima da galeria, pela chamine do atelie. A Sala das Lentes, com
        /// dois raios caindo do teto, e a Camara dos Espelhos, alta, com dois
        /// raios atravessados: os raios queimam e ligam e desligam em ritmo, e
        /// o Scrap passa no tempo certo. No alto, o nicho da Luz Engarrafada.
        /// </summary>
        private static void BuildOptics()
        {
            const string lenses = "05a_Lenses";
            const string mirrors = "05b_MirrorChamber";
            Room(lenses, 30, 61, 70, 69);
            Room(mirrors, 3, 61, 29, 80);

            // A Sala das Lentes. A chamine do atelie chega pelo chao, a leste.
            Carve(30, 61, 70, 69);

            Beams.Add(new Beam { Name = "Beam_Lenses_1", X = 38, Y = 69, DirX = 0, DirY = -1, On = 1.6f, Off = 1.4f, Offset = 0f });
            Beams.Add(new Beam { Name = "Beam_Lenses_2", X = 54, Y = 69, DirX = 0, DirY = -1, On = 1.6f, Off = 1.4f, Offset = 1.5f });

            // A passagem pra Camara dos Espelhos: um degrau, que separa os dois chaos.
            Carve(29, 62, 29, 65);

            Tell("diary", lenses, "Lore_Diary_Light", "diário de laboratório, folha do dia 150", 45, 61,
                "\"Dia 150. A luz alquímica atravessa o Óxido e mostra o que já apodreceu por dentro. Nas placas de prata nobre, mostra muito.\"",
                "\"Sua Majestade proibiu que a luz fosse apontada para a Corte. Deixamos as lâmpadas acesas só aqui, onde ninguém de prata entra.\"",
                "\"Queimam. Andem entre um clarão e outro.\"");

            Tell("chalk", lenses, "Lore_Chalk_Light", "um quadro-negro", 60, 61,
                "Um quadro-negro com um desenho: um raio saindo de uma lâmpada, batendo num espelho inclinado e dobrando a esquina.",
                "Ao lado, em letra de aula: \"O espelho não cria a luz. Só escolhe para onde ela vai.\"");

            Enemy(FoeKind.Sentinel, "Sentinel_Lenses", 30, 63, 61);
            Ledge(42, 45, 64);
            Enemy(FoeKind.FlaskThrower, "Thrower_Lenses", 43, 43, 65);

            Put("lab_lens_bench", 34f, 61, 4);
            Put("lab_prism", 49f, 61, 4);
            Put("lab_lens_bench", 57f, 61, 4);
            Hang("lab_chandelier", 46f, 70);

            Lamp(34f, 66f, 7f);
            Lamp(60f, 66f, 7f);

            // A Camara dos Espelhos.
            Carve(9, 61, 28, 80);

            Beams.Add(new Beam { Name = "Beam_Mirrors_Low", X = 28, Y = 63, DirX = -1, DirY = 0, On = 1.5f, Off = 1.5f, Offset = 0.4f });
            Beams.Add(new Beam { Name = "Beam_Mirrors_High", X = 28, Y = 71, DirX = -1, DirY = 0, On = 1.5f, Off = 1.5f, Offset = 1.9f });

            // Os patamares, subindo em zigue-zague, cada um entre dois raios.
            Ledge(20, 24, 64);
            Ledge(14, 18, 68);
            Ledge(9, 12, 72);

            // O nicho do reagente, aberto.
            Carve(3, 73, 8, 76);
            AddReagent(mirrors, "Light", "LUZ ENGARRAFADA", 1, 5, 73);

            Enemy(FoeKind.Sentinel, "Sentinel_Mirror", 9, 28, 61);
            Enemy(FoeKind.Sentinel, "Sentinel_Mirror_Ledge", 20, 24, 65);

            Put("lab_prism", 13f, 61, 4);
            Put("lab_lens_bench", 17f, 61, 4);
            Put("lab_mirror", 25f, 61, 3);
            Hang("lab_chandelier", 17f, 81);

            Lamp(14f, 66f, 6f);
            Lamp(18f, 75f, 6f);
            Lamp(5f, 75f, 4f);
        }

        // ------------------------------------------------------------------
        // 6. A Incubacao (a leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Os tanques de cultura, cada um com uma cobaia de cobre dentro; o
        /// bioterio, mais fundo, com as gaiolas; e no fim a Oficina de
        /// Prototipos: seis mesas com seis tentativas desmontadas, e a setima,
        /// vazia. A Assistente se escondeu no armario de reagentes.
        /// </summary>
        private static void BuildIncubation()
        {
            const string tanks = "06a_Tanks";
            const string vivarium = "06b_Vivarium";
            const string workshop = "06c_PrototypeWorkshop";
            Room(tanks, 101, 41, 150, 46);
            Room(vivarium, 151, 34, 180, 46);
            Room(workshop, 181, 36, 215, 44);

            Carve(105, 41, 150, 46);

            for (int i = 0; i < 7; i++) Put("lab_tank_" + (i % 3), 108.5f + i * 6.5f, 41, 3);

            Ledge(118, 121, 44);
            Ledge(138, 141, 44);

            Enemy(FoeKind.FailedExperiment, "Failed_Tanks_1", 101, 150, 41, 116);
            Enemy(FoeKind.FailedExperiment, "Failed_Tanks_2", 101, 150, 41, 140);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tanks_1", 119, 119, 45);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tanks_2", 139, 139, 45);

            Tell("diary", tanks, "Lore_Diary_2", "diário de laboratório, folha do dia 90", 112, 41,
                "\"Dia 90. O mercúrio só retarda. Precisamos testar em corpo inteiro, não em placa.\"",
                "\"Sua Majestade autorizou o uso de voluntários de cobre. São abundantes, e ninguém sente falta deles.\"",
                "A palavra \"voluntários\" foi escrita por cima de outra, raspada.");

            Drips.Add((124, 46, 5));
            Drips.Add((133, 46, 5));

            Lamp(111f, 44f, 6f, true);
            Lamp(124f, 44f, 6f, true);
            Lamp(137f, 44f, 6f, true);
            Lamp(147f, 44f, 6f);

            // O bioterio, mais fundo.
            Carve(151, 34, 180, 46);
            Ledge(151, 154, 37);

            Enemy(FoeKind.Slime, "Slime_Vivarium_1", 151, 180, 34, 158);
            Enemy(FoeKind.Slime, "Slime_Vivarium_2", 151, 180, 34, 174);

            Tell("diary", vivarium, "Lore_Diary_3", "diário de laboratório, folha do dia 210", 172, 34,
                "\"Dia 210. O tanque 4 não acordou. O tanque 7 acordou errado. Costuramos o que sobrou do 7 no que sobrou do 9.\"",
                "\"O Mestre diz que estamos quase lá. Ele já não dorme. Ele já não é bem de prata.\"",
                "A letra piorou muito.");

            Put("lab_cage", 158f, 34, 4);
            Put("lab_cage_broken", 162.5f, 34, 4);
            Put("lab_cage", 167f, 34, 4);
            Put("lab_table", 177f, 34, 4);
            Hang("lab_pipes", 165f, 47);

            Lamp(157f, 40f, 6f, true);
            Lamp(168f, 40f, 7f);
            Lamp(177f, 39f, 6f);

            // A Oficina de Prototipos.
            Carve(181, 36, 215, 44);

            for (int n = 0; n < 6; n++) Put("lab_proto_table_" + n, 190f + n * 3.5f, 36, 4);
            Put("lab_proto_table_6", 211.5f, 36, 4);

            Tell("assistant", workshop, "Npc_Assistant", "Assistente", 184, 36,
                "N-não abra! ...Ah. Você não é um deles. Você é... de bronze? De pedra?",
                "Eu era a aprendiz do Mestre. Do Alquimista. Estou aqui dentro desde que ele começou a escorrer.",
                "Ele não era cruel. Você precisa saber disso. Ele chorou no primeiro tanque. Queria curar todo mundo, até nós, os de cobre.",
                "Mas o Rei mandava, e ele obedecia. Cada ordem, um pouco pior que a outra. No fim, ele obedecia antes de o Rei pedir.",
                "Ele está no Grande Laboratório, depois do alambique do ateliê. Ele não apanha enquanto é líquido. Os círculos do chão... eles o prendem em prata.");

            Tell("board", workshop, "Lore_PrototypeBoard", "o quadro das fichas", 189, 36,
                "Um quadro de cortiça com seis fichas presas por alfinete, uma para cada mesa.",
                "N.º 1: \"Ferro fundido. Pesado demais para andar.\" N.º 2: \"Latão. O Óxido pegou em três dias.\"",
                "N.º 3: \"Cobre.\" Riscado. Embaixo, menor: \"era um voluntário\".",
                "N.º 4: \"Vidro. Quebrou ao se ver no espelho.\" N.º 5: \"Prata nobre. Apodreceu antes de acordar.\"",
                "N.º 6: \"Quartzo lapidado. Perfeito. Não aguentou o primeiro passo.\"",
                "Há um sétimo alfinete, sem ficha nenhuma.");

            Tell("diary", workshop, "Lore_Diary_5", "diário de laboratório, folha do dia 300", 200, 36,
                "\"Dia 300. Os protótipos brutos são imunes. Quartzo não lapidado, bronze sem polimento: o Óxido não pega neles.\"",
                "\"Levei um até o trono. Andando. Imune. A cura da cidade inteira, de pé na frente dele.\"",
                "\"Sua Majestade recusou. Disse que a prata não será salva por sucata. Ordem de descarte assinada.\"");

            Tell("sheet", workshop, "Lore_PrototypeSheet", "ficha de protótipo", 208, 36,
                "Uma prancheta. Um desenho técnico, feito com capricho: cabeça de quartzo bruto, dois olhos, uma mola.",
                "É você. É o seu corpo, cotado em milímetros.",
                "\"PROTÓTIPO BRUTO N.º 7. IMUNE AO ÓXIDO. FUNCIONAL.\"",
                "E, por cima de tudo, carimbado em vermelho: \"REJEITADO — IMPERFEITO\".",
                "Embaixo, à mão: \"Destino: Poço de Sucata.\"");

            AddReagent(workshop, "Salt", "SAL DE QUARTZO", 2, 213, 36);

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

            Carve(100, 62, 128, 68);
            Bench(room, "Workbench_PreBoss", 116, 62);

            Tell("diary", room, "Lore_Diary_4", "diário de laboratório, folha do dia 255", 104, 62,
                "\"Dia 255. O Mestre entrou no tanque grande. Sozinho. Disse que, se o mercúrio retarda, então basta SER mercúrio.\"",
                "\"Ele saiu. Fala como antes, educado. Mas escorre. E, quando ri, o riso continua depois que a boca se fecha.\"",
                "\"Os guardas de prata perfeita passaram a vigiar o laboratório. Não sei se guardam a nós ou a ele.\"");

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
                "Uma placa de prata na beira do palco: \"ANFITEATRO DE DEMONSTRAÇÕES. AQUI A CIÊNCIA SE APRESENTA A SUA MAJESTADE.\"",
                "Nos degraus, almofadas de veludo, ainda arrumadas. Ninguém se levantou para ir embora; só pararam de vir.");

            Tell("decree", room, "Lore_DiscardOrder", "a ordem no púlpito", 218, 76,
                "Um papel timbrado com o selo de prata do Rei, preso no púlpito do palco.",
                "\"O protótipo bruto foi apresentado e recusado. A prata não será salva por sucata.\"",
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
                    "O Mestre... eu vi pelo vidro. Ele não gritou. Pediu desculpas, eu acho. Ao mercúrio, a si mesmo, não sei.",
                    "Você era o n.º 7, não era? Eu limpei a sua mesa no dia em que levaram você. Guardei a mola que sobrou.",
                    "Não guardei, na verdade. Joguei fora, como mandaram. Mas eu lembro dela. Isso conta alguma coisa?",
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

            void Need(LevelMap.Reach reach, int x, int y, string what)
            {
                if (!reach.CanStand(x, y)) problems.Add($"Nao da pra chegar em {what} ({x}, {y}).");
            }

            void Forbid(LevelMap.Reach reach, int x, int y, string what)
            {
                if (reach.CanStand(x, y)) problems.Add($"Da pra chegar em {what} ({x}, {y}) cedo demais.");
            }

            void NeedCircle(LevelMap.Reach reach, string name) => Need(reach, CircleNamed(name).X, CircleNamed(name).Y, name);

            void NeedReagent(LevelMap.Reach reach, string key) => Need(reach, ReagentNamed(key).X, ReagentNamed(key).Y, "o reagente " + key);

            void ForbidReagent(LevelMap.Reach reach, string key) => Forbid(reach, ReagentNamed(key).X, ReagentNamed(key).Y, "o reagente " + key);

            var none = new string[0];
            string[] vapor = { "Circle_Vapor" };

            // 1. Nada feito: o centro e as tres alas inteiras, ate os reagentes.
            // Nenhuma ala depende de outra: a ordem e livre.
            LevelMap.Reach start = Explore(none, none);
            Need(start, CrisolTile.X, CrisolTile.Y, "Crisol");
            Need(start, AlembicTile.X, AlembicTile.Y, "o Grande Alambique");
            NeedCircle(start, "Circle_Vapor");
            NeedReagent(start, "Mercury");
            NeedReagent(start, "Light");
            NeedReagent(start, "Salt");
            Forbid(start, SecretChassisTile.X, SecretChassisTile.Y, "o chassi");
            Forbid(start, BossTile.X, BossTile.Y, "o Grande Laboratorio");
            Forbid(start, 117, 62, "a bancada pre-boss");

            string[] wings = none;

            // 5. Alambique aberto: o chefe, e nao alem.
            var withAlembic = new List<string> { AlembicGate };
            LevelMap.Reach boss = Explore(wings, withAlembic);
            Need(boss, BossTile.X, BossTile.Y, "o Grande Laboratorio");
            Need(boss, 117, 62, "a bancada pre-boss");
            foreach ((string name, int x, int y) in ArenaCircles) Need(boss, x, y, name);
            Forbid(boss, StageCircle.X, StageCircle.Y, "o palco");

            // 6. O Alquimista caido: o corredor ate o palco e a Grande Escadaria.
            var all = new List<string>(withAlembic) { BossExitGate };
            LevelMap.Reach after = Explore(vapor, all);
            Need(after, StageCircle.X, StageCircle.Y, "o palco");
            Need(after, StairTop.X, StairTop.Y, "o alto da Grande Escadaria");

            // E da pra voltar do palco ao centro.
            LevelMap.Reach back = Explore(vapor, all, StageCircle);
            Need(back, CrisolTile.X, CrisolTile.Y, "o atelie, voltando do palco");

            // 7. O chassi: so com a poca transmutada em vapor.
            Forbid(Explore(none, all), SecretChassisTile.X, SecretChassisTile.Y, "o chassi (sem o vapor)");
            Need(after, SecretChassisTile.X, SecretChassisTile.Y, "o chassi");

            // E de la da pra voltar: o vapor so empurra pra cima.
            LevelMap.Reach fromChassis = Explore(vapor, all, SecretChassisTile);
            Need(fromChassis, CrisolTile.X, CrisolTile.Y, "o atelie, voltando do chassi");

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

            return problems;
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
