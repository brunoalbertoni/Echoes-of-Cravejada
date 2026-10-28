using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O desenho da Phase 4 — District 4: The Alchemical Ateliers.
    ///
    /// Os laboratorios da nobreza: marmore branco, vidro, prata e mercurio. E
    /// o primeiro lugar rico do jogo, e o Scrap parece um intruso nele.
    ///
    /// Como no District 3, sao dois andares. O de baixo vai de oeste pra leste;
    /// a Camara dos Espelhos, alta, liga os dois; o de cima volta pra oeste:
    ///
    ///   embaixo   1 Portal de Marmore        2 Salao de Mercurio
    ///             3 Primeira Transmutacao    4 Galeria dos Retratos
    ///             5 Atelie de Crisol         6 Os Tanques
    ///
    ///   a leste   7 Camara dos Espelhos      8 Arquivo Secreto
    ///
    ///   em cima   9 Corredor do Guarda       10 Workbench pre-boss
    ///             11 Grande Laboratorio      12 Forja Modular
    ///             13 Cerimonia               14 Elevador pra Silver City
    ///
    /// Tres coisas nao sao tile, e sim objeto que aparece ou some: as pocas de
    /// mercurio (pro mapa, chao comum), e o que os circulos de transmutacao
    /// mexem — a parede de vidro que vira ponte de prata, a poca que vira
    /// vapor, o bloco de marmore que vira areia.
    ///
    /// Esta e a etapa A. O corredor do guarda, o laboratorio, a forja, a sala
    /// da cerimonia e o elevador ja estao escavados; o que acontece neles vem
    /// na etapa B, e ate la uma grade fecha o caminho depois do laboratorio.
    ///
    /// Como os outros layouts, este arquivo nao depende de UnityEngine.
    /// </summary>
    public static class Phase04Layout
    {
        public const int Width = 258;
        public const int Height = 80;

        public const string SceneName = "Phase04_AlchemicalAteliers";
        public const string FromDistrict3SpawnId = "FromDistrict3";

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

        public sealed class Mirror
        {
            public string Name;
            public int X;
            public int Y;

            /// <summary>Verdadeiro se comeca em "/".</summary>
            public bool StartsSlash;

            /// <summary>Onde o Scrap para pra gira-lo.</summary>
            public int StandX;
            public int StandY;
        }

        public enum FoeKind
        {
            Slime,
            FlaskThrower,
            FailedExperiment,
            Sentinel,
        }

        public sealed class Foe
        {
            public FoeKind Kind;
            public string Name;
            public int X0;
            public int X1;
            public int Y;
        }

        /// <summary>
        /// Algo pra ler ou alguem pra ouvir. Kind diz o desenho: "sign",
        /// "diary", "portrait0".."portrait2", "sheet" (a ficha do prototipo) e
        /// "assistant" (a Assistente, no armario).
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

        // ------------------------------------------------------------------
        // O que o gerador le
        // ------------------------------------------------------------------

        private static readonly LevelMap Map = new LevelMap(Width, Height);

        public static readonly List<(string Name, int X0, int Y0, int X1, int Y1)> Rooms =
            new List<(string, int, int, int, int)>();

        public static readonly List<Pool> Pools = new List<Pool>();
        public static readonly List<Block> Blocks = new List<Block>();
        public static readonly List<Circle> Circles = new List<Circle>();
        public static readonly List<Mirror> Mirrors = new List<Mirror>();
        public static readonly List<Foe> Foes = new List<Foe>();
        public static readonly List<LoreSpot> Lore = new List<LoreSpot>();
        public static readonly List<Marker> Markers = new List<Marker>();

        /// <summary>Colunas de vapor que so existem depois de uma transmutacao.</summary>
        public static readonly List<(string Name, Cells Area)> VaporLifts = new List<(string, Cells)>();

        /// <summary>Goteiras de mercurio: a celula do teto de onde pinga e quantos tiles cai.</summary>
        public static readonly List<(int X, int Y, int Fall)> Drips = new List<(int, int, int)>();

        /// <summary>Enfeites grandes: "tank", "window0", "window1", "shelf". A celula e a do pe.</summary>
        public static readonly List<(string Kind, int X, int Y)> Decor = new List<(string, int, int)>();

        /// <summary>As Workbenches: canto de baixo, a esquerda (3x2 tiles).</summary>
        public static readonly List<(string Name, int X, int Y)> Workbenches = new List<(string, int, int)>();

        /// <summary>Lampioes e brilhos: Green e o verde do Oxide nos frascos e tanques.</summary>
        public static readonly List<(float X, float Y, float Radius, bool Green)> Lamps =
            new List<(float, float, float, bool)>();

        /// <summary>Celulas que sao objeto, e nao tile: as pocas de mercurio.</summary>
        private static readonly HashSet<(int, int)> ObjectCells = new HashSet<(int, int)>();

        public static (int X, int Y) SpawnTile => (7, 40);

        public static (int X, int Y) CrisolTile => (152, 40);

        public static readonly string[] CrisolLines =
        {
            "...Scrap.",
            "Não mexa em nada. Esta bancada era minha. Aquele torno era meu. Aquela lupa, eu mesmo a poli.",
            "Eu cravei gemas para metade dos retratos que você viu no corredor. Depois me penduraram lá também, e depois me riscaram.",
            "Quando a praga começou, os alquimistas vieram pedir-me quartzo bruto. Bruto. Eu achei que era piada.",
            "Não era. Eles estavam construindo alguma coisa. Eu vi os desenhos. Eu sabia o que eles tinham achado.",
            "E, quando o Rei mandou jogar tudo fora, eu não disse nada. Fiquei quieto, para continuar pendurado na parede.",
            "Não adiantou. E você está aqui. ...Descanse, Scrap. A bancada ainda funciona.",
        };

        /// <summary>O emissor do raio da Camara dos Espelhos: a celula e a direcao.</summary>
        public static (int X, int Y, int DirX, int DirY) Emitter => (220, 44, 1, 0);

        /// <summary>A fechadura de cristal que o raio precisa alcancar.</summary>
        public static (int X, int Y) LockTile => (215, 61);

        /// <summary>O portao que a fechadura abre: a saida de cima da Camara dos Espelhos.</summary>
        public static (int X, int Y, int Height) MirrorGate => (205, 56, 5);

        /// <summary>A grade depois do Grande Laboratorio: so abre com a queda do Mercury Alchemist (etapa B).</summary>
        public static (int X, int Y, int Height) BossGate => (83, 56, 5);

        /// <summary>A barreira de cristal alquimico do Arquivo Secreto: Resonance Strike.</summary>
        public static (string Name, int X, int Y, int Height) ArchiveBarrier => ("Barrier_Archive", 241, 56, 5);

        /// <summary>O reforco de chassi no nicho em cima do Salao de Mercurio.</summary>
        public static (int X, int Y) SecretChassisTile => (77, 50);

        // ------------------------------------------------------------------
        // Ferramentas de construcao
        // ------------------------------------------------------------------

        static Phase04Layout()
        {
            BuildPortal();
            BuildMercuryHall();
            BuildFirstTransmutation();
            BuildGallery();
            BuildAtelier();
            BuildTanks();
            BuildMirrorChamber();
            BuildArchive();
            BuildGuardCorridor();
            BuildPreBoss();
            BuildLaboratory();
            BuildAftermath();
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

        private static void Enemy(FoeKind kind, string name, int x0, int x1, int y)
        {
            Foes.Add(new Foe { Kind = kind, Name = name, X0 = x0, X1 = x1, Y = y });
        }

        private static void Tell(string kind, string group, string name, string title, int x, int y, params string[] lines)
        {
            Lore.Add(new LoreSpot { Kind = kind, Name = name, Title = title, X = x, Y = y, Lines = lines });
            Mark(group, name, x, y);
        }

        private static void Lamp(float x, float y, float radius, bool green = false) => Lamps.Add((x, y, radius, green));

        // ------------------------------------------------------------------
        // Consultas
        // ------------------------------------------------------------------

        public static bool IsRock(int x, int y) => Map.IsRock(x, y);
        public static bool IsSolid(int x, int y) => Map.IsSolid(x, y);
        public static bool IsSpike(int x, int y) => Map.IsSpike(x, y);
        public static bool IsStandable(int x, int y) => Map.IsStandable(x, y);

        /// <summary>A entrada ainda e cristal: so as primeiras colunas do portal.</summary>
        private static bool IsCrystal(int x, int y) => x <= 12 && y >= 34 && y <= 54;

        /// <summary>O tile de terreno de uma celula, ou null se e ar, espinho ou objeto.</summary>
        public static string GroundTileName(int x, int y)
        {
            if (ObjectCells.Contains((x, y))) return null;

            LevelMap.TilePick pick = Map.PickGround(x, y);
            if (pick.Kind == LevelMap.TileKind.None) return null;

            if (IsCrystal(x, y))
            {
                if (pick.Kind == LevelMap.TileKind.Core) return Phase01Layout.CoreTileName;
                if (pick.Kind == LevelMap.TileKind.Thin) return Phase01Layout.TileName(pick.Column, 14);
                return Phase01Layout.TileName(pick.Column, Phase01Layout.BlueBand + pick.Row);
            }

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

            if (IsCrystal(x, y)) return $"bgblue_c{LevelMap.Pick(x + 7, y + 13, 3)}_r6";

            // Paineis de tres colunas por duas linhas, como lambris.
            int seams = (x % 3 == 2 ? 1 : 0) + (y % 2 == 0 ? 2 : 0);
            return $"marble_c{seams}_r{BrassArt.BackRow}";
        }

        public static string SpikeTileName(int x, int y) => $"misc_c{3 + LevelMap.Pick(x, y, 2) * 2}_r2";

        // ------------------------------------------------------------------
        // 1. O Portal de Marmore
        // ------------------------------------------------------------------

        private static void BuildPortal()
        {
            const string room = "01_MarblePortal";
            Room(room, 4, 40, 30, 50);

            Carve(4, 40, 30, 50);

            Tell("sign", room, "Lore_Portal", "a placa do portão", 16, 40,
                "O portão era de prata maciça. Alguém o arrombou por dentro.",
                "Na placa, em letra de ourives: \"ATELIÊS ALQUÍMICOS DE SUA MAJESTADE. ENTRADA VEDADA A METAIS COMUNS.\"",
                "Embaixo, riscado a prego: \"e a quem mais?\"");

            Decor.Add(("window0", 20, 43));
            Decor.Add(("window1", 26, 43));

            // A cabine que subiu do District 3, parada onde o Scrap desce dela.
            Decor.Add(("elevator", SpawnTile.X - 1, SpawnTile.Y));

            Lamp(14f, 45f, 9f);
            Lamp(24f, 45f, 9f);
        }

        // ------------------------------------------------------------------
        // 2. O Salao de Mercurio
        // ------------------------------------------------------------------

        /// <summary>
        /// Tres pocas grandes. A primeira ensina que ele flutua e escorrega; a
        /// do meio e corrompida e arrasta pra cima de espinhos; a ultima e
        /// calma — e esconde um segredo pra quem voltar com olho de alquimista.
        /// </summary>
        private static void BuildMercuryHall()
        {
            const string room = "02_MercuryHall";
            Room(room, 31, 40, 80, 53);

            Carve(31, 40, 80, 48);

            Mercury("Mercury_Hall_1", 37, 45, 39);
            Mercury("Mercury_Hall_2", 50, 61, 39, 3.2f);
            Spikes(62, 64, 40);
            Mercury("Mercury_Hall_3", 68, 75, 39);

            Drips.Add((41, 48, 8));
            Drips.Add((55, 48, 8));
            Drips.Add((58, 48, 8));

            // O segredo: transmutar a ultima poca em vapor. A coluna sobe por
            // uma chamine ate um nicho, em cima do salao.
            Carve(70, 49, 73, 53);
            Carve(74, 50, 79, 53);

            VaporLifts.Add(("Vapor_Hall", new Cells(70, 38, 73, 53)));

            Rune(room, "Circle_Vapor", "mercúrio em vapor", 78, 40,
                new[] { "Mercury_Hall_3" }, new[] { "Vapor_Hall" });

            Mark(room, "Secret_Chassis", SecretChassisTile.X, SecretChassisTile.Y);

            Decor.Add(("window0", 34, 43));
            Decor.Add(("window1", 47, 43));
            Decor.Add(("window0", 66, 43));

            Lamp(34f, 45f, 8f);
            Lamp(47.5f, 45f, 8f);
            Lamp(56f, 41f, 7f, true);
            Lamp(66f, 45f, 8f);
            Lamp(77f, 52f, 5f);
        }

        // ------------------------------------------------------------------
        // 3. A Primeira Transmutacao
        // ------------------------------------------------------------------

        /// <summary>
        /// Um abismo, e antes dele uma parede de vidro de cima a baixo. Polir a
        /// runa transmuta o vidro em prata: a parede some e vira a ponte.
        /// </summary>
        private static void BuildFirstTransmutation()
        {
            const string room = "03_FirstTransmutation";
            Room(room, 81, 31, 110, 45);

            Carve(81, 40, 110, 45);
            Carve(92, 31, 101, 39);
            Spikes(92, 101, 31);

            Transmutable("Glass_Wall", BlockKind.Glass, 91, 40, 91, 45);
            Transmutable("Silver_Bridge", BlockKind.Silver, 92, 39, 101, 39);

            Rune(room, "Circle_Glass", "vidro em prata", 86, 40,
                new[] { "Glass_Wall" }, new[] { "Silver_Bridge" });

            Decor.Add(("shelf", 82, 40));
            Decor.Add(("shelf", 106, 40));

            Lamp(86f, 43f, 8f);
            Lamp(96f, 38f, 6f, true);
            Lamp(106f, 43f, 8f);
        }

        // ------------------------------------------------------------------
        // 4. A Galeria dos Retratos
        // ------------------------------------------------------------------

        private static void BuildGallery()
        {
            const string room = "04_PortraitGallery";
            Room(room, 111, 40, 140, 47);

            Carve(111, 40, 140, 47);

            Tell("diary", room, "Lore_Diary_1", "diário de laboratório, folha 1", 113, 40,
                "\"Dia 12. O mercúrio retarda a corrosão! Uma placa de prata mergulhada resistiu três dias a mais.\"",
                "\"Sua Majestade foi informada. Há esperança. Vamos curar a cidade inteira.\"",
                "A letra é caprichada, de quem está feliz.");

            Tell("portrait0", room, "Lore_Portrait_1", "retrato de um Nobre", 118, 40,
                "Um Nobre de prata, de gola alta. O rosto foi riscado em X com alguma coisa verde.",
                "Na plaqueta: \"Mestre de Fundição\".");

            Tell("portrait1", room, "Lore_Portrait_2", "retrato de uma Nobre", 123, 40,
                "Uma Nobre de prata. Também riscada, com mais força: o risco rasgou a tela.",
                "Na plaqueta: \"Tesoureira da Coroa\".");

            Tell("portrait2", room, "Lore_Portrait_Crisol", "retrato de um jovem", 129, 40,
                "Este ninguém riscou. Um jovem de prata lisa, sem um arranhão, de monóculo.",
                "Na plaqueta: \"Crisol. Lapidário da Corte.\"",
                "Alguém arrancou a plaqueta de baixo, a que dizia o título de nobreza.");

            Tell("portrait0", room, "Lore_Portrait_3", "retrato de um Nobre", 135, 40,
                "Um Nobre de jaleco e máscara de bico. Riscado, mas só de leve — quase com carinho.",
                "Na plaqueta: \"Alquimista-Chefe\".");

            Enemy(FoeKind.Slime, "Slime_Gallery", 114, 138, 40);

            Lamp(118f, 44f, 7f);
            Lamp(126f, 44f, 7f);
            Lamp(134f, 44f, 7f);
        }

        // ------------------------------------------------------------------
        // 5. O Atelie de Crisol
        // ------------------------------------------------------------------

        private static void BuildAtelier()
        {
            const string room = "05_CrisolAtelier";
            Room(room, 141, 40, 160, 46);

            Carve(141, 40, 160, 46);

            Workbenches.Add(("Workbench_Atelier", 144, 40));
            Mark(room, "Workbench_Atelier_Rest", 145, 40);
            Mark(room, "Crisol", CrisolTile.X, CrisolTile.Y);

            Decor.Add(("shelf", 156, 40));

            Lamp(146f, 44f, 8f);
            Lamp(153f, 44f, 8f);
        }

        // ------------------------------------------------------------------
        // 6. Os Tanques
        // ------------------------------------------------------------------

        /// <summary>
        /// Um corredor de tanques de vidro, cada um com alguem dentro. Os que
        /// sairam dos tanques errados andam soltos; os assistentes arremessam
        /// de cima. No fim, um armario de reagentes — e a Assistente.
        /// </summary>
        private static void BuildTanks()
        {
            const string room = "06_Tanks";
            Room(room, 161, 40, 205, 48);

            Carve(161, 40, 205, 48);

            Ledge(174, 178, 43);
            Ledge(188, 192, 43);

            for (int x = 164; x <= 199; x += 7) Decor.Add(("tank", x, 40));

            Enemy(FoeKind.FailedExperiment, "Failed_Tanks_1", 163, 182, 40);
            Enemy(FoeKind.FailedExperiment, "Failed_Tanks_2", 184, 198, 40);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tanks_1", 176, 176, 44);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tanks_2", 190, 190, 44);

            Drips.Add((169, 48, 8));
            Drips.Add((183, 48, 8));

            Tell("diary", room, "Lore_Diary_2", "diário de laboratório, folha 2", 168, 40,
                "\"Dia 90. O mercúrio só retarda. Precisamos testar em corpo inteiro, não em placa.\"",
                "\"Sua Majestade autorizou o uso de voluntários de cobre. São abundantes, e ninguém sente falta deles.\"",
                "A palavra \"voluntários\" foi escrita por cima de outra, raspada.");

            Tell("diary", room, "Lore_Diary_3", "diário de laboratório, folha 3", 195, 40,
                "\"Dia 210. O tanque 4 não acordou. O tanque 7 acordou errado. Costuramos o que sobrou do 7 no que sobrou do 9.\"",
                "\"O Mestre diz que estamos quase lá. Ele já não dorme. Ele já não é bem de prata.\"",
                "A letra piorou muito.");

            Tell("assistant", room, "Npc_Assistant", "Assistente", 202, 40,
                "N-não abra! ...Ah. Você não é um deles. Você é... de bronze? De pedra?",
                "Eu era a aprendiz do Mestre. Do Alquimista. Estou aqui dentro desde que ele começou a escorrer.",
                "Ele não era cruel. Você precisa saber disso. Ele chorou no primeiro tanque. Queria curar todo mundo, até nós, os de cobre.",
                "Mas o Rei mandava, e ele obedecia. Cada ordem, um pouco pior que a outra. No fim, ele obedecia antes de o Rei pedir.",
                "A sala dos espelhos fica ali na frente. A luz queima. Não pule na frente dela.");

            Lamp(168f, 44f, 7f, true);
            Lamp(182f, 44f, 7f, true);
            Lamp(196f, 44f, 7f, true);
            Lamp(202f, 43f, 5f);
        }

        // ------------------------------------------------------------------
        // 7. A Camara dos Espelhos
        // ------------------------------------------------------------------

        /// <summary>
        /// Uma sala alta. Um emissor solta um raio pra leste; dois espelhos, um
        /// embaixo e um em cima, precisam ser virados pra o raio subir, voltar
        /// pra oeste e acender a fechadura de cristal — que abre a saida de
        /// cima. A subida fica a oeste, longe do raio.
        /// </summary>
        private static void BuildMirrorChamber()
        {
            const string room = "07_MirrorChamber";
            Room(room, 206, 40, 240, 64);

            Carve(206, 40, 240, 64);

            // O emissor, numa prateleira: passa-se por baixo.
            Ledge(219, 220, 43);

            // A subida, a oeste.
            Ledge(210, 214, 43);
            Ledge(206, 209, 47);
            Ledge(211, 215, 51);
            Ledge(206, 212, 55);

            // O andar de cima, em ilhas, ate o espelho de la.
            Ledge(217, 221, 55);
            Ledge(224, 228, 55);
            Ledge(231, 235, 55);
            Ledge(237, 240, 55);

            // O apoio do espelho de baixo.
            Ledge(233, 235, 41);

            Mirrors.Add(new Mirror { Name = "Mirror_Low", X = 236, Y = 44, StartsSlash = false, StandX = 235, StandY = 42 });
            Mirrors.Add(new Mirror { Name = "Mirror_High", X = 236, Y = 61, StartsSlash = true, StandX = 235, StandY = 56 });

            Mark(room, "Mirror_Low_Stand", 235, 42);
            Mark(room, "Mirror_High_Stand", 235, 56);

            // A saida de cima.
            Carve(205, 56, 205, 60);

            Enemy(FoeKind.Sentinel, "Sentinel_Mirror_Low", 222, 231, 40);
            Enemy(FoeKind.Sentinel, "Sentinel_Mirror_High", 224, 228, 56);

            Lamp(208f, 44f, 6f);
            Lamp(208f, 59f, 6f);
            Lamp(238f, 59f, 5f);
        }

        // ------------------------------------------------------------------
        // 8. O Arquivo Secreto
        // ------------------------------------------------------------------

        private static void BuildArchive()
        {
            const string room = "08_SecretArchive";
            Room(room, 241, 56, 254, 61);

            Carve(241, 56, 254, 61);

            Tell("diary", room, "Lore_Diary_5", "diário de laboratório, última folha", 245, 56,
                "\"Dia 300. Os protótipos brutos são imunes. Quartzo não lapidado, bronze sem polimento: o Óxido não pega neles.\"",
                "\"Levei um até o trono. Andando. Imune. A cura da cidade inteira, de pé na frente dele.\"",
                "\"Sua Majestade recusou. Disse que a prata não será salva por sucata. Ordem de descarte assinada.\"");

            // A etiqueta rasgada ao lado de onde o Scrap acorda, no District 1,
            // e o pedaco que faltava desta ficha.
            Tell("sheet", room, "Lore_PrototypeSheet", "ficha de protótipo", 250, 56,
                "Uma prancheta. Um desenho técnico, feito com capricho: cabeça de quartzo bruto, dois olhos, uma mola.",
                "É você. É o seu corpo, cotado em milímetros.",
                "\"PROTÓTIPO BRUTO N.º 7. IMUNE AO ÓXIDO. FUNCIONAL.\"",
                "E, por cima de tudo, carimbado em vermelho: \"REJEITADO — IMPERFEITO\".",
                "Embaixo, à mão: \"Destino: Poço de Sucata.\"");

            Decor.Add(("shelf", 252, 56));
            Lamp(248f, 59f, 7f);
        }

        // ------------------------------------------------------------------
        // 9. O Corredor do Guarda
        // ------------------------------------------------------------------

        /// <summary>
        /// Um corredor comprido voltando pra oeste, com uma poca de mercurio no
        /// meio e, no fim, um bloco de marmore tapando a passagem — que um
        /// circulo desfaz em areia. Na etapa B e aqui que o Perfect Silver
        /// Guard aparece, e o corredor vira fuga.
        /// </summary>
        private static void BuildGuardCorridor()
        {
            const string room = "09_GuardCorridor";
            Room(room, 150, 56, 204, 61);

            Carve(150, 56, 204, 61);

            Mercury("Mercury_Corridor", 170, 182, 55, -2.6f);

            Transmutable("Marble_Plug", BlockKind.Marble, 152, 56, 153, 61);

            Rune(room, "Circle_Sand", "mármore em areia", 158, 56,
                new[] { "Marble_Plug" }, new string[0]);

            Enemy(FoeKind.Slime, "Slime_Corridor_1", 185, 200, 56);
            Enemy(FoeKind.Slime, "Slime_Corridor_2", 160, 168, 56);

            Tell("diary", room, "Lore_Diary_4", "diário de laboratório, folha 4", 192, 56,
                "\"Dia 255. O Mestre entrou no tanque grande. Sozinho. Disse que, se o mercúrio retarda, então basta SER mercúrio.\"",
                "\"Ele saiu. Fala como antes, educado. Mas escorre. E, quando ri, o riso continua depois que a boca se fecha.\"",
                "\"Os guardas de prata perfeita passaram a vigiar o corredor. Não sei se guardam a nós ou a ele.\"");

            Mark(room, "Guard_Spawn", 200, 56, false);

            Drips.Add((176, 61, 5));

            Lamp(196f, 59f, 6f);
            Lamp(176f, 57f, 6f, true);
            Lamp(160f, 59f, 6f);
        }

        // ------------------------------------------------------------------
        // 10. A Workbench antes do boss
        // ------------------------------------------------------------------

        private static void BuildPreBoss()
        {
            const string room = "10_PreBoss";
            Room(room, 130, 56, 149, 63);

            Carve(130, 56, 149, 63);
            Carve(127, 56, 129, 60);

            Workbenches.Add(("Workbench_PreBoss", 134, 56));
            Mark(room, "Workbench_PreBoss_Rest", 135, 56);

            Decor.Add(("shelf", 142, 56));
            Decor.Add(("window1", 139, 59));

            Lamp(136f, 60f, 8f);
            Lamp(145f, 60f, 6f);
        }

        // ------------------------------------------------------------------
        // 11. O Grande Laboratorio
        // ------------------------------------------------------------------

        /// <summary>
        /// A arena do Mercury Alchemist: redonda, com tanques nas paredes. O
        /// boss entra na etapa B.
        /// </summary>
        private static void BuildLaboratory()
        {
            const string room = "11_GrandLaboratory";
            Room(room, 84, 56, 126, 72);

            Carve(84, 56, 126, 72);

            // Cantos de cima chanfrados.
            Solid(84, 69, 88, 72);
            Solid(122, 69, 126, 72);

            Mark(room, "BossSpawn_MercuryAlchemist", 105, 56);

            Decor.Add(("tank", 88, 56));
            Decor.Add(("tank", 96, 56));
            Decor.Add(("tank", 112, 56));
            Decor.Add(("tank", 120, 56));
            Decor.Add(("window0", 104, 62));

            Lamp(92f, 60f, 8f, true);
            Lamp(105f, 64f, 10f);
            Lamp(118f, 60f, 8f, true);

            // A saida, gradeada ate o boss cair.
            Carve(83, 56, 83, 60);
        }

        // ------------------------------------------------------------------
        // 12 a 14. Forja, Cerimonia e Elevador
        // ------------------------------------------------------------------

        /// <summary>O que vem depois do boss. Escavado e marcado; o que acontece ali e da etapa B.</summary>
        private static void BuildAftermath()
        {
            Room("12_ModularForge", 62, 56, 82, 63);
            Carve(62, 56, 82, 63);
            Mark("12_ModularForge", "Forge", 72, 56, false);

            Carve(59, 56, 61, 60);

            Room("13_Ceremony", 36, 56, 58, 64);
            Carve(36, 56, 58, 64);
            Mark("13_Ceremony", "Ceremony", 47, 56, false);

            Carve(33, 56, 35, 60);

            Room("14_Elevator", 10, 56, 32, 76);
            Carve(10, 56, 32, 62);
            Carve(18, 63, 23, 76);
            Mark("14_Elevator", "Exit_District5", 20, 56, false);
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

        private static void Close(LevelMap world, (int X, int Y, int Height) column)
        {
            world.Fill(column.X, column.Y, column.X, column.Y + column.Height - 1, LevelMap.Rock);
        }

        private static Block BlockNamed(string name) => Blocks.Find(block => block.Name == name);

        /// <summary>
        /// O mapa num certo ponto do progresso. Cada circulo ja polido tira o
        /// que ele desfaz e poe o que ele cria; o resto continua fechado.
        /// </summary>
        private static LevelMap World(ICollection<string> litCircles, bool mirrorGateOpen, bool barrierOpen,
            List<LevelMap.Lift> lifts)
        {
            LevelMap world = Map.Clone();

            foreach (Circle circle in Circles)
            {
                bool lit = litCircles.Contains(circle.Name);

                // Antes de polir: o que ele desfaz ainda esta la, o que ele cria nao.
                foreach (string name in lit ? circle.Appear : circle.Vanish)
                {
                    Block block = BlockNamed(name);
                    if (block != null) Put(world, block.Area, block.Kind == BlockKind.Silver ? LevelMap.Platform : LevelMap.Rock);

                    if (!lit) continue;

                    (string Name, Cells Area) lift = VaporLifts.Find(vapor => vapor.Name == name);
                    if (lift.Name != null) lifts.Add(new LevelMap.Lift(lift.Area.X0, lift.Area.X1, lift.Area.Y0, lift.Area.Y1));
                }

                if (!lit) continue;

                // Poca transmutada: sobra o buraco dela.
                foreach (string name in circle.Vanish)
                {
                    Pool pool = Pools.Find(candidate => candidate.Name == name);
                    if (pool != null) Put(world, pool.Area, LevelMap.Air);
                }
            }

            if (!mirrorGateOpen) Close(world, MirrorGate);
            if (!barrierOpen) Close(world, (ArchiveBarrier.X, ArchiveBarrier.Y, ArchiveBarrier.Height));

            Close(world, BossGate);
            return world;
        }

        private static LevelMap.Reach Explore(ICollection<string> lit, bool gate, bool barrier)
        {
            var lifts = new List<LevelMap.Lift>();
            LevelMap world = World(lit, gate, barrier, lifts);
            return world.Explore(SpawnTile, Rules(), lifts);
        }

        /// <summary>
        /// Confere se a fase e jogavel de verdade, sem precisar jogar: segue a
        /// ordem em que as coisas abrem e, a cada passo, confere que o proximo
        /// passo esta ao alcance e que nao da pra pular nenhum.
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

            Circle Named(string name) => Circles.Find(circle => circle.Name == name);

            void Need(LevelMap.Reach reach, int x, int y, string what)
            {
                if (!reach.CanStand(x, y)) problems.Add($"Nao da pra chegar em {what} ({x}, {y}).");
            }

            void Forbid(LevelMap.Reach reach, int x, int y, string what)
            {
                if (reach.CanStand(x, y)) problems.Add($"Da pra chegar em {what} ({x}, {y}) cedo demais.");
            }

            var lit = new HashSet<string>();

            // 1. Nada polido: so ate a parede de vidro.
            LevelMap.Reach reach = Explore(lit, false, false);
            Need(reach, Named("Circle_Glass").X, Named("Circle_Glass").Y, "o circulo do vidro");
            Need(reach, Named("Circle_Vapor").X, Named("Circle_Vapor").Y, "o circulo do vapor");
            Forbid(reach, 105, 40, "o outro lado do abismo");

            // 2. Vidro em prata: ate a Camara dos Espelhos, com a saida de cima fechada.
            lit.Add("Circle_Glass");
            reach = Explore(lit, false, false);

            foreach (Mirror mirror in Mirrors) Need(reach, mirror.StandX, mirror.StandY, mirror.Name);

            Need(reach, CrisolTile.X, CrisolTile.Y, "Crisol");
            Forbid(reach, 200, 56, "o corredor do guarda");
            Forbid(reach, 246, 56, "o Arquivo Secreto");

            // 3. Fechadura acesa: o corredor, ate o bloco de marmore.
            reach = Explore(lit, true, false);
            Need(reach, Named("Circle_Sand").X, Named("Circle_Sand").Y, "o circulo da areia");
            Forbid(reach, 140, 56, "a bancada pre-boss");

            // 4. Marmore em areia: ate o laboratorio, e nao alem.
            lit.Add("Circle_Sand");
            reach = Explore(lit, true, false);
            Need(reach, 105, 56, "o Grande Laboratorio");
            Forbid(reach, 72, 56, "a Forja Modular");
            Forbid(reach, 246, 56, "o Arquivo Secreto");

            // 5. Barreira quebrada e mercurio em vapor: o que e opcional.
            LevelMap.Reach open = Explore(lit, true, true);

            lit.Add("Circle_Vapor");
            LevelMap.Reach vapor = Explore(lit, true, true);
            Need(vapor, SecretChassisTile.X, SecretChassisTile.Y, "o nicho em cima do Salao de Mercurio");
            Need(vapor, 105, 40, "o outro lado do salao, com a poca transmutada");

            foreach (Marker marker in Markers)
            {
                if (!marker.MustReach) continue;

                if (!Map.IsStandable(marker.X, marker.Y)) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) nao e um lugar onde da pra ficar em pe.");
                else if (!open.CanStand(marker.X, marker.Y) && !vapor.CanStand(marker.X, marker.Y)) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) esta fora de alcance.");
            }

            foreach (Foe foe in Foes)
            {
                for (int x = foe.X0; x <= foe.X1; x++)
                {
                    if (!Map.IsStandable(x, foe.Y)) problems.Add($"{foe.Name} anda por ({x}, {foe.Y}), onde nao da pra ficar em pe.");
                }
            }

            // O raio: do emissor ao espelho de baixo, dele ao de cima, dele a fechadura.
            CheckBeam(problems, Emitter.X + 1, Emitter.Y, Mirrors[0].X - 1, Mirrors[0].Y, "do emissor ao espelho de baixo");
            CheckBeam(problems, Mirrors[0].X, Mirrors[0].Y + 1, Mirrors[1].X, Mirrors[1].Y - 1, "entre os dois espelhos");
            CheckBeam(problems, LockTile.X + 1, LockTile.Y, Mirrors[1].X - 1, Mirrors[1].Y, "do espelho de cima a fechadura");

            return problems;
        }

        /// <summary>Confere que nao ha terreno no caminho de um trecho reto do raio.</summary>
        private static void CheckBeam(List<string> problems, int x0, int y0, int x1, int y1, string what)
        {
            for (int y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++)
            {
                for (int x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++)
                {
                    if (Map.IsSolid(x, y)) problems.Add($"O raio {what} bate em terreno em ({x}, {y}).");
                }
            }
        }

        /// <summary>Os trechos de chao onde nao da pra chegar com tudo aberto. O que fica depois do boss entra aqui.</summary>
        public static List<string> UnreachableFloors()
        {
            var all = new HashSet<string>();
            foreach (Circle circle in Circles) all.Add(circle.Name);

            var lifts = new List<LevelMap.Lift>();
            LevelMap world = World(all, true, true, lifts);
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
