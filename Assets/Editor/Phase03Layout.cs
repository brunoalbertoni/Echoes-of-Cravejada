using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O desenho da Phase 3 — District 3: The Silent Geodes.
    ///
    /// Tuneis escavados dentro de cristais colossais, no escuro. Aqui quase
    /// nao ha luz fixa: o que se ve e o que o proprio barulho do Scrap acende.
    /// E o mesmo barulho acorda o que mora ali.
    ///
    /// Sao dois andares. O de baixo vai de oeste pra leste; o de cima volta de
    /// leste pra oeste e termina na arena da broca:
    ///
    ///   embaixo   1 Fronteira de Cristal   2 Tunel Escuro      3 Primeiro Cacador
    ///             4 Ponte Fragil           5 Gruta de Crisol   6 Galeria das Gravuras
    ///
    ///   em cima   9 Poco da Broca          7 Porta Harmonica   8 Camara do Ressonante
    ///             10 Workbench pre-boss    11 Camara de Perfuracao   12 Saida
    ///
    /// Sobe-se de um andar pro outro por dois lugares: o poco no fim da galeria
    /// (o caminho normal) ou a chamine da Gruta de Crisol (o atalho).
    ///
    /// A arena da broca tem tres camadas, uma embaixo da outra. As duas de
    /// cima tem chao que desaba durante a luta; a saida pros Alchemical
    /// Ateliers fica na de baixo, atras de uma barreira de cristal que so o
    /// Resonance Strike (a recompensa do boss) quebra.
    ///
    /// Aqui o Scrap ja tem o Steam Dash e o Pressure Jump, e o mapa conta com
    /// os dois. Como os outros layouts, este arquivo nao depende de UnityEngine.
    /// </summary>
    public static class Phase03Layout
    {
        public const int Width = 276;
        public const int Height = 92;

        public const string SceneName = "Phase03_SilentGeodes";
        public const string FromDistrict2SpawnId = "FromDistrict2";

        // ------------------------------------------------------------------
        // Tipos
        // ------------------------------------------------------------------

        /// <summary>Um cristal de enfeite que acende com o som. Hanging e pendurado do teto.</summary>
        public readonly struct EchoSpot
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Size;
            public readonly int Hue;
            public readonly bool Hanging;

            public EchoSpot(int x, int y, int size, int hue, bool hanging)
            {
                X = x;
                Y = y;
                Size = size;
                Hue = hue;
                Hanging = hanging;
            }
        }

        /// <summary>Um cristal ressonante. Note 0 e so isca; de 1 pra cima sao as notas da porta.</summary>
        public sealed class ResonantSpot
        {
            public string Name;
            public int X;
            public int Y;
            public int Note;
            public bool Corrupted;

            /// <summary>Onde o Scrap para pra golpear (ou raspar).</summary>
            public int StandX;

            /// <summary>Em que chao desabavel ele esta apoiado (indice em Slabs), ou -1.</summary>
            public int Slab = -1;
        }

        public enum FoeKind
        {
            EchoHunter,
            CrystalMite,
            ShardSentinel,
        }

        /// <summary>Um inimigo: o trecho de chao por onde anda (a sentinela fica em X0) e a linha dos pes.</summary>
        public sealed class FoeSpot
        {
            public FoeKind Kind;
            public string Name;
            public int X0;
            public int X1;
            public int Y;

            /// <summary>Onde ele espera, dentro do trecho.</summary>
            public int HomeX;
        }

        /// <summary>
        /// Algo pra ler com E. Kind diz o desenho: "sign" e uma placa, "page"
        /// uma pagina, "engraving0".."engraving3" as gravuras, "edge" nada
        /// (so a beirada do poco), "miner" um mineiro Tarnished e "crisol" o
        /// proprio Crisol — esses dois so nas listas do distrito purificado.
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

        public static readonly List<EchoSpot> Echoes = new List<EchoSpot>();
        public static readonly List<ResonantSpot> Resonants = new List<ResonantSpot>();
        public static readonly List<FoeSpot> Foes = new List<FoeSpot>();
        public static readonly List<LoreSpot> Lore = new List<LoreSpot>();
        public static readonly List<Marker> Markers = new List<Marker>();

        /// <summary>As plataformas frageis: cada uma e um trecho deitado de celulas.</summary>
        public static readonly List<(int X0, int X1, int Y)> Fragiles = new List<(int, int, int)>();

        /// <summary>As Workbenches: canto de baixo, a esquerda (3x2 tiles).</summary>
        public static readonly List<(string Name, int X, int Y)> Workbenches = new List<(string, int, int)>();

        /// <summary>Lampioes: as unicas luzes paradas, nas salas seguras.</summary>
        public static readonly List<(float X, float Y, float Radius)> Lamps = new List<(float, float, float)>();

        /// <summary>
        /// Os chaos da arena que desabam durante a luta, de cima pra baixo.
        /// Como as plataformas frageis, pro mapa sao chao comum; quem os
        /// desenha e some com eles e o objeto de cada um.
        /// </summary>
        public static readonly List<(string Name, int X0, int X1, int Y0, int Y1)> Slabs =
            new List<(string, int, int, int, int)>();

        /// <summary>Barreiras de cristal: so o Resonance Strike as quebra.</summary>
        public static readonly List<(string Name, int X, int Y, int Height)> Barriers =
            new List<(string, int, int, int)>();

        /// <summary>O brilho verde do Oxide no fundo da arena: perto do Heart of Quartz.</summary>
        public static readonly List<(float X, float Y, float Radius)> OxideGlows = new List<(float, float, float)>();

        /// <summary>Quem aparece depois da purificacao: os mineiros e Crisol, no salao de concertos.</summary>
        public static readonly List<LoreSpot> Purified = new List<LoreSpot>();

        /// <summary>As celulas que sao objeto, e nao tile: plataformas frageis e chaos que desabam.</summary>
        private static readonly HashSet<(int, int)> FragileCells = new HashSet<(int, int)>();

        public static (int X, int Y) SpawnTile => (9, 40);

        public static (int X, int Y) CrisolTile => (182, 40);

        public static readonly string[] CrisolLines =
        {
            "Scrap. Você chegou até aqui. Admito que não apostei nisso.",
            "Olhe em volta. Ametista, quartzo leitoso, cristal fumê... isto aqui é o meu ofício. Aqui eu sei o nome de tudo.",
            "Você reparou que eles só acendem com barulho? Era assim que os mineiros enxergavam: batiam na pedra e andavam enquanto ela brilhava.",
            "Eu trabalhei para os 925 Nobres. Eu estava lá quando resolveram furar até o Coração de Quartzo. Eu VI a escavação.",
            "Eu disse que era um erro. Disse na frente de todos. Sabe o que fazem com prata que fala demais? Riscam.",
            "É por isso que sou arranhado assim, Scrap. Não foi o tempo. Foi de propósito. Riscaram-me e jogaram-me no Poço de Sucata.",
            "...Chega. O seu polimento está decente, aliás. Não conte a ninguém que eu disse isso.",
        };

        /// <summary>A sequencia da Porta Harmonica: as notas, na ordem em que precisam soar.</summary>
        public static readonly int[] DoorSequence = { 3, 1, 4, 2 };

        /// <summary>A coluna de celulas que a Porta Harmonica fecha.</summary>
        public static (int X, int Y, int Height) HarmonicDoor => (149, 56, 5);

        /// <summary>Chegar perto da porta, nesta area, faz ela tocar a sequencia.</summary>
        public static (int X, int Y, int Width, int Height) DoorPreview => (150, 56, 12, 6);

        // The Resonant: onde ele espera, ate onde anda e a area que o acorda.
        public static (int X, int Y) EliteTile => (123, 56);
        public static (int X0, int X1) EliteRange => (106, 141);
        public static (int X, int Y, int Width, int Height) EliteWake => (108, 56, 32, 14);

        // The Geode Drill: onde aparece, ate onde corre, a area que a acorda e
        // a altura do chao de cada uma das tres camadas da arena.
        public static (int X, int Y) BossTile => (55, 56);
        public static (int X0, int X1) BossRange => (36, 74);
        public static (int X, int Y, int Width, int Height) BossWake => (42, 56, 28, 14);
        public static readonly int[] BossFloors = { 56, 44, 32 };

        /// <summary>A area, no fundo da sala de saida, que leva ao District 4.</summary>
        public static (int X, int Y, int Width, int Height) District4Exit => (8, 32, 2, 5);

        /// <summary>O reforco de chassi escondido atras da barreira da Fronteira.</summary>
        public static (int X, int Y) SecretChassisTile => (2, 40);

        /// <summary>A placa gravada no chassi da broca, que so aparece quando ela para.</summary>
        public static readonly string[] DrillPlaqueLines =
        {
            "Por baixo da crosta de Óxido há uma placa de prata, rebitada no chassi.",
            "\"POR ORDEM DE SUA MAJESTADE, O REI DE PRATA.\"",
            "Embaixo, a data da primeira perfuração. E mais nada. Nenhum nome de mineiro.",
            "Então foi ele. Não foi acidente nem praga: foi uma ordem.",
        };

        /// <summary>O brilho verde no fundo do Poco da Broca.</summary>
        public static (float X, float Y) HeartGlow => (257.5f, 10f);

        // ------------------------------------------------------------------
        // Ferramentas de construcao
        // ------------------------------------------------------------------

        static Phase03Layout()
        {
            BuildFrontier();
            BuildDarkTunnel();
            BuildFirstHunter();
            BuildFragileBridge();
            BuildGrotto();
            BuildEngravingGallery();
            BuildDrillShaft();
            BuildHarmonicDoor();
            BuildResonantChamber();
            BuildPreBoss();
            BuildDrillChamber();
            BuildExit();
        }

        private static void Carve(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Air);

        private static void Solid(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Rock);

        private static void Ledge(int x0, int x1, int y) => Map.Fill(x0, y, x1, y, LevelMap.Platform);

        private static void Spikes(int x0, int x1, int y) => Map.Fill(x0, y, x1, y, LevelMap.Spike);

        /// <summary>
        /// Uma plataforma fragil. Pro mapa ela e uma plataforma comum (da pra
        /// pisar); quem a desenha e a faz quebrar e o objeto dela.
        /// </summary>
        private static void Fragile(int x0, int x1, int y)
        {
            Map.Fill(x0, y, x1, y, LevelMap.Platform);
            Fragiles.Add((x0, x1, y));

            for (int x = x0; x <= x1; x++) FragileCells.Add((x, y));
        }

        /// <summary>Um chao da arena que desaba. Devolve o indice dele em Slabs.</summary>
        private static int Slab(string name, int x0, int x1, int y0, int y1)
        {
            Map.Fill(x0, y0, x1, y1, LevelMap.Platform);
            Slabs.Add((name, x0, x1, y0, y1));

            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++) FragileCells.Add((x, y));
            }

            return Slabs.Count - 1;
        }

        /// <summary>Uma barreira de cristal fechando uma coluna de celulas (que tem que estar escavada).</summary>
        private static void Barrier(string name, int x, int y, int height)
        {
            Barriers.Add((name, x, y, height));
        }

        private static void Room(string name, int x0, int y0, int x1, int y1)
        {
            Rooms.Add((name, x0, y0, x1, y1));
        }

        private static void Mark(string group, string name, int x, int y, bool mustReach = true)
        {
            Markers.Add(new Marker(group, name, x, y, mustReach));
        }

        private static void Resonant(string group, string name, int x, int y, int note, bool corrupted = false, int slab = -1)
        {
            // Golpeia-se de lado: ele para na celula a esquerda do cristal.
            Resonants.Add(new ResonantSpot
            {
                Name = name, X = x, Y = y, Note = note, Corrupted = corrupted, StandX = x - 1, Slab = slab,
            });
            Mark(group, name + "_Stand", x - 1, y);
        }

        private static void Foe(FoeKind kind, string name, int x0, int x1, int y, int homeX = -1)
        {
            Foes.Add(new FoeSpot
            {
                Kind = kind,
                Name = name,
                X0 = x0,
                X1 = x1,
                Y = y,
                HomeX = homeX < 0 ? (x0 + x1) / 2 : homeX,
            });
        }

        private static void Tell(string kind, string group, string name, string title, int x, int y, params string[] lines)
        {
            Lore.Add(new LoreSpot { Kind = kind, Name = name, Title = title, X = x, Y = y, Lines = lines });
            Mark(group, name, x, y);
        }

        /// <summary>
        /// Espalha cristais de eco por um retangulo: um no chao a cada tantas
        /// colunas, e um pendurado no teto no meio do caminho entre dois. Sao
        /// eles que acendem com o som — sem eles a sala nao aparece nunca.
        /// </summary>
        private static void Scatter(int x0, int y0, int x1, int y1, int every, int seed)
        {
            for (int x = x0 + 1; x <= x1 - 1; x += every)
            {
                int column = x + LevelMap.Pick(x, seed, 2);

                // O primeiro chao de cima pra baixo.
                for (int y = y1; y >= y0; y--)
                {
                    if (!Map.IsStandable(column, y)) continue;

                    Echoes.Add(new EchoSpot(column, y, LevelMap.Pick(column, seed + 1, CrystalArt.SizeCount),
                        LevelMap.Pick(column, seed + 2, CrystalArt.HueCount), false));
                    break;
                }

                int hang = column + every / 2;
                if (hang > x1 - 1) continue;

                // O primeiro teto de cima pra baixo: ar com rocha em cima.
                for (int y = y1; y >= y0; y--)
                {
                    if (Map.IsSolid(hang, y) || !Map.IsRock(hang, y + 1)) continue;

                    Echoes.Add(new EchoSpot(hang, y, LevelMap.Pick(hang, seed + 3, 2),
                        LevelMap.Pick(hang, seed + 4, CrystalArt.HueCount), true));
                    break;
                }
            }
        }

        // ------------------------------------------------------------------
        // Consultas
        // ------------------------------------------------------------------

        public static bool IsRock(int x, int y) => Map.IsRock(x, y);
        public static bool IsSolid(int x, int y) => Map.IsSolid(x, y);
        public static bool IsSpike(int x, int y) => Map.IsSpike(x, y);
        public static bool IsStandable(int x, int y) => Map.IsStandable(x, y);

        /// <summary>Em que chao desabavel uma celula esta (indice em Slabs), ou -1.</summary>
        public static int SlabAt(int x, int y)
        {
            for (int i = 0; i < Slabs.Count; i++)
            {
                (string _, int x0, int x1, int y0, int y1) = Slabs[i];
                if (x >= x0 && x <= x1 && y >= y0 && y <= y1) return i;
            }

            return -1;
        }

        /// <summary>A fronteira ainda e latao: so as primeiras colunas da primeira sala.</summary>
        private static bool IsBrass(int x, int y) => x <= 15 && y >= 34 && y <= 56;

        /// <summary>O tile de terreno de uma celula, ou null se e ar, espinho ou plataforma fragil.</summary>
        public static string GroundTileName(int x, int y)
        {
            if (FragileCells.Contains((x, y))) return null;

            LevelMap.TilePick pick = Map.PickGround(x, y);
            if (pick.Kind == LevelMap.TileKind.None) return null;

            if (IsBrass(x, y))
            {
                if (pick.Kind == LevelMap.TileKind.Thin) return $"brass_c{4 + pick.Column}_r{BrassArt.ThinRow}";
                if (pick.Kind == LevelMap.TileKind.Edge) return $"brass_c{pick.Column}_r{pick.Row}";
                return "brass_c0_r2";
            }

            // Rocha azulada, a mesma banda da saida do District 2.
            if (pick.Kind == LevelMap.TileKind.Core) return Phase01Layout.CoreTileName;
            if (pick.Kind == LevelMap.TileKind.Thin) return Phase01Layout.TileName(pick.Column, 14);
            return Phase01Layout.TileName(pick.Column, Phase01Layout.BlueBand + pick.Row);
        }

        /// <summary>A parede do fundo, ou null atras de rocha macica e no vao do poco da broca.</summary>
        public static string BackgroundTileName(int x, int y)
        {
            if (x >= 247 && x <= 268 && y >= 6 && y <= 80) return null;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (!Map.IsRock(x + dx, y + dy)) return $"bgblue_c{LevelMap.Pick(x + 7, y + 13, 3)}_r6";
                }
            }

            return null;
        }

        /// <summary>Os espinhos de cristal usam o mesmo desenho dos de sucata; a cor muda no tilemap.</summary>
        public static string SpikeTileName(int x, int y) => $"misc_c{3 + LevelMap.Pick(x, y, 2) * 2}_r2";

        // ------------------------------------------------------------------
        // 1. A Fronteira de Cristal
        // ------------------------------------------------------------------

        private static void BuildFrontier()
        {
            const string room = "01_Frontier";
            Room(room, 6, 40, 40, 50);

            Carve(6, 40, 40, 50);

            // Um degrau de teto, pra a sala nao ser um caixote.
            Solid(20, 48, 30, 50);

            // Logo atras de onde ele chega: um nicho fechado por cristal macico.
            // Da pra ver desde o primeiro minuto, e so abre depois do boss.
            Carve(1, 40, 5, 43);
            Barrier("Barrier_Secret", 5, 40, 4);
            Mark(room, "Secret_Chassis", SecretChassisTile.X, SecretChassisTile.Y);

            Tell("sign", room, "Lore_Frontier", "o aviso dos mineiros", 14, 40,
                "Uma tábua pregada na última chapa de latão. A tinta descascou, mas dá para ler:",
                "\"DAQUI PARA A FRENTE NÃO HÁ LAMPIÃO. A PEDRA ACENDE COM BARULHO.\"",
                "E, mais embaixo, em letra menor: \"...e o barulho acorda os que ficaram.\"");

            Scatter(16, 40, 40, 50, 4, 11);
        }

        // ------------------------------------------------------------------
        // 2. O Tunel Escuro
        // ------------------------------------------------------------------

        /// <summary>
        /// Escuridao total e tres fossos de espinhos. Nao ha inimigo: e a sala
        /// que ensina a fazer barulho de proposito pra enxergar onde pisar.
        /// </summary>
        private static void BuildDarkTunnel()
        {
            const string room = "02_DarkTunnel";
            Room(room, 41, 34, 90, 50);

            Carve(41, 35, 90, 50);

            Solid(41, 35, 48, 39);
            Spikes(49, 54, 35);
            Solid(55, 35, 62, 37);
            Spikes(63, 67, 35);
            Solid(68, 35, 76, 36);
            Spikes(77, 80, 35);
            Solid(81, 35, 90, 39);

            // Teto irregular.
            Solid(52, 47, 60, 50);
            Solid(70, 45, 74, 50);

            Scatter(41, 35, 90, 50, 5, 21);
        }

        // ------------------------------------------------------------------
        // 3. O Primeiro Cacador
        // ------------------------------------------------------------------

        /// <summary>
        /// Um Echo Hunter parado no meio do corredor. Passa-se por ele so
        /// andando, ou manda-se ele pra uma ponta golpeando o cristal de la.
        /// Ha tambem uma passarela por cima.
        /// </summary>
        private static void BuildFirstHunter()
        {
            const string room = "03_FirstHunter";
            Room(room, 91, 40, 130, 49);

            Carve(91, 40, 130, 49);

            Ledge(100, 106, 43);
            Ledge(110, 116, 43);
            Ledge(120, 126, 43);

            Foe(FoeKind.EchoHunter, "EchoHunter_First", 93, 128, 40, 111);

            // Uma isca em cada ponta do corredor.
            Resonant(room, "Resonant_Hunter_West", 95, 40, 0);
            Resonant(room, "Resonant_Hunter_East", 127, 40, 0);

            Scatter(91, 40, 130, 49, 5, 31);
        }

        // ------------------------------------------------------------------
        // 4. A Ponte Fragil
        // ------------------------------------------------------------------

        /// <summary>
        /// Um abismo de espinhos cruzado por laminas de cristal fino. Andando e
        /// pulando baixo elas aguentam; dash, golpe ou queda alta as quebram.
        /// </summary>
        private static void BuildFragileBridge()
        {
            const string room = "04_FragileBridge";
            Room(room, 131, 26, 170, 48);

            Carve(131, 27, 170, 48);
            Solid(131, 27, 136, 39);
            Solid(165, 27, 170, 39);
            Spikes(137, 164, 27);

            Fragile(138, 141, 39);
            Fragile(144, 147, 39);
            Fragile(150, 153, 39);
            Fragile(156, 159, 39);
            Fragile(162, 164, 39);

            Scatter(131, 27, 170, 48, 5, 41);
        }

        // ------------------------------------------------------------------
        // 5. A Gruta de Crisol
        // ------------------------------------------------------------------

        private static void BuildGrotto()
        {
            const string room = "05_Grotto";
            Room(room, 171, 40, 190, 47);

            Carve(171, 40, 190, 47);

            // A chamine: o atalho pra sala da Porta Harmonica, la em cima.
            Carve(186, 48, 189, 55);

            Workbenches.Add(("Workbench_Grotto", 173, 40));
            Mark(room, "Workbench_Grotto_Rest", 174, 40);
            Mark(room, "Crisol", 182, 40);

            Lamps.Add((175f, 44f, 7f));
            Lamps.Add((182.5f, 44f, 6f));

            Scatter(171, 40, 185, 47, 5, 51);
        }

        // ------------------------------------------------------------------
        // 6. A Galeria das Gravuras
        // ------------------------------------------------------------------

        /// <summary>
        /// Quatro gravuras enormes contam a escavacao. So da pra ve-las com
        /// barulho — e ha uma Shard Sentinel no meio da sala e Crystal Mites
        /// dormindo no chao. Ler a historia custa.
        /// </summary>
        private static void BuildEngravingGallery()
        {
            const string room = "06_EngravingGallery";
            Room(room, 191, 40, 240, 52);

            Carve(191, 40, 234, 52);

            // O poco que sobe pro andar de cima, com tres degraus.
            Carve(235, 40, 240, 60);
            Ledge(235, 237, 43);
            Ledge(238, 240, 47);
            Ledge(235, 237, 51);

            Tell("engraving0", room, "Lore_Engraving_1", "a primeira gravura", 198, 40,
                "Riscada na pedra, do tamanho de uma parede: uma fila de mineiros de picareta no ombro.",
                "Estão descendo. Todos olham para baixo.");

            Tell("engraving1", room, "Lore_Engraving_2", "a segunda gravura", 208, 40,
                "Uma figura maior que as outras, de coroa. O Rei de Prata.",
                "Ele não segura ferramenta nenhuma. Só aponta. Para baixo.");

            Tell("engraving2", room, "Lore_Engraving_3", "a terceira gravura", 220, 40,
                "Uma broca em espiral, descendo pela rocha. Em volta dela, riscos de impacto.",
                "Os mineiros agora são menores. Estão atrás dela, não na frente.");

            Tell("engraving3", room, "Lore_Engraving_4", "a última gravura", 230, 40,
                "Um losango enorme no fundo da terra: o Coração de Quartzo.",
                "Há uma fenda nele, de cima a baixo. Quem gravou pintou a fenda de verde.",
                "Depois desta, a parede está lisa. Ninguém voltou para gravar o resto.");

            Foe(FoeKind.ShardSentinel, "Sentinel_Gallery", 214, 214, 40);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_1", 200, 206, 40, 201);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_2", 200, 206, 40, 203);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_3", 200, 206, 40, 205);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_4", 222, 227, 40, 223);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_5", 222, 227, 40, 226);

            Scatter(191, 40, 240, 52, 5, 61);
        }

        // ------------------------------------------------------------------
        // 9. O Poco da Broca
        // ------------------------------------------------------------------

        /// <summary>
        /// Um poco gigante, escavado pela broca rumo ao Heart of Quartz. La no
        /// fundo, muito longe, um brilho verde pulsa — a unica luz do distrito
        /// que nao depende de som. Nao se desce: so se ve.
        /// </summary>
        private static void BuildDrillShaft()
        {
            const string room = "09_DrillShaft";
            Room(room, 241, 6, 268, 80);

            Carve(241, 56, 246, 66);
            Carve(247, 6, 268, 80);
            Spikes(247, 268, 6);

            Tell("edge", room, "Lore_DrillShaft", "a beirada do poço", 245, 56,
                "O chão acaba aqui. O poço é redondo, liso e largo demais para ter sido feito à mão.",
                "Lá no fundo, muito longe, alguma coisa verde pulsa. Devagar. Como se respirasse.",
                "Não há como descer. Ainda.");

            Echoes.Add(new EchoSpot(243, 56, 1, 0, false));
        }

        // ------------------------------------------------------------------
        // 7. A Porta Harmonica
        // ------------------------------------------------------------------

        /// <summary>
        /// Quatro cristais ressonantes espalhados pela sala, dois deles com
        /// veio de Oxide, e dois Echo Hunters. A porta, a oeste, so abre com as
        /// quatro notas na ordem — e cada nota e um barulho que chama os dois.
        /// </summary>
        private static void BuildHarmonicDoor()
        {
            const string room = "07_HarmonicDoor";
            Room(room, 150, 56, 234, 74);

            Carve(150, 56, 234, 74);

            // Dois blocos e uma passarela, pra as notas ficarem em alturas diferentes.
            Solid(190, 56, 194, 59);
            Ledge(196, 202, 63);
            Solid(204, 56, 212, 59);

            Resonant(room, "Resonant_Door_1", 228, 56, 1);
            Resonant(room, "Resonant_Door_2", 209, 60, 2, true);
            Resonant(room, "Resonant_Door_3", 200, 64, 3);
            Resonant(room, "Resonant_Door_4", 162, 56, 4, true);

            Foe(FoeKind.EchoHunter, "EchoHunter_Door_East", 213, 232, 56, 220);
            Foe(FoeKind.EchoHunter, "EchoHunter_Door_West", 152, 185, 56, 172);

            // O corredor atras da porta.
            Carve(144, 56, 149, 60);

            Mark(room, "HarmonicDoor", 153, 56);

            Scatter(150, 56, 234, 74, 6, 71);
        }

        // ------------------------------------------------------------------
        // 8. A Camara do Ressonante
        // ------------------------------------------------------------------

        /// <summary>
        /// Uma camara larga e alta, onde mora The Resonant, o elite que guarda a
        /// safira. A luta e opcional: da pra atravessar correndo. Os Crystal
        /// Mites espalhados pelo chao acordam a cada batida do diapasao.
        ///
        /// Depois da purificacao vira o salao de concertos dos mineiros.
        /// </summary>
        private static void BuildResonantChamber()
        {
            const string room = "08_ResonantChamber";
            Room(room, 104, 56, 143, 72);

            Carve(104, 56, 143, 72);
            Ledge(112, 118, 59);
            Ledge(128, 134, 59);

            Mark(room, "Elite_TheResonant", EliteTile.X, EliteTile.Y);

            Foe(FoeKind.CrystalMite, "Mite_Chamber_1", 106, 116, 56, 108);
            Foe(FoeKind.CrystalMite, "Mite_Chamber_2", 106, 116, 56, 112);
            Foe(FoeKind.CrystalMite, "Mite_Chamber_3", 130, 141, 56, 136);
            Foe(FoeKind.CrystalMite, "Mite_Chamber_4", 130, 141, 56, 132);

            Purified.Add(new LoreSpot
            {
                Kind = "miner", Name = "Tarnished_Miner_1", Title = "Mineiro", X = 110, Y = 56,
                Lines = new[]
                {
                    "Escute. ...Ouviu? Ela afinou. A pedra inteira afinou.",
                    "A gente passou anos aqui embaixo sem poder dar um pio. Agora vamos fazer barulho de propósito.",
                    "Vai haver concerto. Você é convidado, pequeno. Foi você que desligou aquela coisa.",
                },
            });

            Purified.Add(new LoreSpot
            {
                Kind = "miner", Name = "Tarnished_Miner_2", Title = "Mineira", X = 136, Y = 56,
                Lines = new[]
                {
                    "Café? É de borra de caldeira, mas está quente.",
                    "Eu operei a broca, sabia? No começo, éramos seis lá dentro. Depois, ela não precisou mais da gente.",
                    "Ninguém nos disse o que havia lá embaixo. Só disseram: mais fundo.",
                },
            });

            Purified.Add(new LoreSpot
            {
                Kind = "crisol", Name = "Npc_Crisol_Purified", Title = "Crisol", X = 121, Y = 56,
                Lines = new[]
                {
                    "Você viu a placa. Então agora você sabe o que eu sabia.",
                    "Eu avisei, e riscaram-me. Você desceu lá, com esse corpo de sucata, e fez a máquina parar.",
                    "...Obrigado, Scrap.",
                    "Talvez você não seja só sucata, afinal. Não me faça repetir.",
                },
            });

            Scatter(104, 56, 143, 72, 5, 81);
        }

        // ------------------------------------------------------------------
        // 10. A Workbench antes do boss
        // ------------------------------------------------------------------

        private static void BuildPreBoss()
        {
            const string room = "10_PreBoss";
            Room(room, 80, 56, 100, 64);

            Carve(101, 56, 103, 60);
            Carve(80, 56, 100, 64);

            Workbenches.Add(("Workbench_PreBoss", 84, 56));
            Mark(room, "Workbench_PreBoss_Rest", 85, 56);
            Lamps.Add((86f, 60f, 7f));

            Tell("page", room, "Lore_MinerNote", "um bilhete de mineiro", 94, 56,
                "Um papel dobrado, preso debaixo de um capacete.",
                "\"Ela não para mais. Desligamos a chave, e ela não parou. Não há mais ninguém lá dentro, e ela não parou.\"",
                "\"Se você está lendo isto: não faça barulho perto dela. Ela não enxerga. Ela escuta.\"");

            Scatter(90, 56, 100, 64, 5, 91);
        }

        // ------------------------------------------------------------------
        // 11. A Camara de Perfuracao
        // ------------------------------------------------------------------

        /// <summary>
        /// A arena do Geode Drill, em tres camadas empilhadas. A luta comeca na
        /// de cima; a cada terco de vida a broca fura o chao e tudo desce uma.
        ///
        ///   camada 1 (chao 56)   quatro pilares ressonantes
        ///   camada 2 (chao 44)   dois
        ///   camada 3 (chao 32)   um so, e o brilho verde logo embaixo
        ///
        /// Na parede leste ha prateleiras em zigue-zague: e por onde se sobe de
        /// volta depois que o chao se foi.
        /// </summary>
        private static void BuildDrillChamber()
        {
            const string room = "11_DrillChamber";
            Room(room, 34, 32, 76, 76);

            Carve(77, 56, 79, 60);
            Carve(34, 32, 76, 76);

            // Cantos de cima chanfrados.
            Solid(34, 73, 38, 76);
            Solid(72, 73, 76, 76);

            int top = Slab("Slab_Drill_1", 34, 76, 54, 55);

            // Na camada 2 a ponta leste e rocha de verdade: faz parte da subida.
            Solid(73, 42, 76, 43);
            int middle = Slab("Slab_Drill_2", 34, 72, 42, 43);

            // A subida: 32 -> 36 -> 40 -> 44 -> 48 -> 52 -> o corredor de entrada.
            Ledge(73, 76, 35);
            Ledge(66, 69, 39);
            Ledge(66, 69, 47);
            Ledge(72, 75, 51);

            Resonant(room, "Resonant_Pillar_1", 44, 56, 0, false, top);
            Resonant(room, "Resonant_Pillar_2", 52, 56, 0, false, top);
            Resonant(room, "Resonant_Pillar_3", 60, 56, 0, false, top);
            Resonant(room, "Resonant_Pillar_4", 68, 56, 0, false, top);
            Resonant(room, "Resonant_Pillar_5", 48, 44, 0, false, middle);
            Resonant(room, "Resonant_Pillar_6", 62, 44, 0, false, middle);
            Resonant(room, "Resonant_Pillar_7", 55, 32, 0);

            Mark(room, "BossSpawn_GeodeDrill", BossTile.X, BossTile.Y);

            OxideGlows.Add((42f, 32.5f, 8f));
            OxideGlows.Add((55f, 32.5f, 9f));
            OxideGlows.Add((68f, 32.5f, 8f));

            Scatter(34, 56, 76, 76, 6, 101);
            Scatter(34, 44, 72, 53, 7, 102);
            Scatter(34, 32, 72, 41, 7, 103);
        }

        // ------------------------------------------------------------------
        // 12. A saida pros Alchemical Ateliers
        // ------------------------------------------------------------------

        /// <summary>
        /// No fundo da arena, a oeste. A barreira de cristal ensina a usar o
        /// Resonance Strike: e a primeira coisa que ele abre.
        /// </summary>
        private static void BuildExit()
        {
            const string room = "12_Exit";
            Room(room, 8, 32, 33, 37);

            Carve(27, 32, 33, 36);
            Carve(8, 32, 26, 37);

            Barrier("Barrier_Exit", 33, 32, 5);

            Tell("edge", room, "Lore_Exit", "o fim do cristal", 11, 32,
                "O cristal acaba aqui. Daqui para a frente, a parede é de mármore branco, lisa, com veios de ouro.",
                "Há vidro quebrado no chão e um cheiro doce, de laboratório. Os Ateliês Alquímicos.",
                "Depois de entrar, não há como voltar.");

            Lamps.Add((14f, 35f, 6f));

            Scatter(14, 32, 26, 37, 5, 111);
        }

        // ------------------------------------------------------------------
        // Validacao        // ------------------------------------------------------------------
        // Validacao
        // ------------------------------------------------------------------

        /// <summary>
        /// As regras de alcance desta fase: as com folga, mais o Steam Dash (uns
        /// 3 tiles a mais de lado), o Pressure Jump (uns 2 a mais de altura e 2
        /// de lado) e o pulo de parede.
        /// </summary>
        private static LevelMap.JumpRules Rules()
        {
            LevelMap.JumpRules rules = LevelMap.JumpRules.Comfortable();
            rules.MaxRise = 4;
            rules.MaxRun += 5;
            rules.WallJump = true;
            return rules;
        }

        private static void Close(LevelMap world, (int X, int Y, int Height) column)
        {
            world.Fill(column.X, column.Y, column.X, column.Y + column.Height - 1, LevelMap.Rock);
        }

        private static void CloseBarriers(LevelMap world)
        {
            foreach ((string _, int x, int y, int height) in Barriers) Close(world, (x, y, height));
        }

        /// <summary>O mapa com os primeiros chaos da arena ja desabados.</summary>
        private static LevelMap Collapsed(int slabsGone)
        {
            LevelMap world = Map.Clone();

            for (int i = 0; i < slabsGone && i < Slabs.Count; i++)
            {
                (string _, int x0, int x1, int y0, int y1) = Slabs[i];
                world.Fill(x0, y0, x1, y1, LevelMap.Air);
            }

            return world;
        }

        /// <summary>
        /// Confere se a fase e jogavel de verdade, sem precisar jogar.
        ///
        /// Comeca com a Porta Harmonica e as barreiras de cristal fechadas. Com
        /// a porta fechada tem que dar pra alcancar os quatro cristais dela; so
        /// entao ela abre. Depois confere a arena em cada estagio do desabamento
        /// e, com o chao todo no fundo, a subida de volta e a saida.
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

            LevelMap.JumpRules rules = Rules();

            // 1. Porta e barreiras fechadas: as notas tem que estar ao alcance.
            LevelMap closed = Map.Clone();
            Close(closed, HarmonicDoor);
            CloseBarriers(closed);

            LevelMap.Reach locked = closed.Explore(SpawnTile, rules);

            bool canPlay = true;
            foreach (ResonantSpot spot in Resonants)
            {
                if (spot.Note == 0) continue;

                if (!locked.CanStand(spot.StandX, spot.Y))
                {
                    problems.Add($"{spot.Name}: com a porta fechada nao da pra chegar em ({spot.StandX}, {spot.Y}) pra toca-lo.");
                    canPlay = false;
                }
            }

            foreach (int note in DoorSequence)
            {
                if (!Resonants.Exists(spot => spot.Note == note)) problems.Add($"A porta pede a nota {note}, e nenhum cristal a toca.");
            }

            if (locked.CanStand(146, 56)) problems.Add("Da pra passar pela Porta Harmonica sem abri-la.");
            if (!canPlay) return problems;

            // 2. Porta aberta, barreiras fechadas, em cada estagio do desabamento.
            var stages = new List<LevelMap.Reach>();

            for (int gone = 0; gone <= Slabs.Count; gone++)
            {
                LevelMap world = Collapsed(gone);
                CloseBarriers(world);
                stages.Add(world.Explore(SpawnTile, rules));
            }

            foreach ((string name, int x, int y, int height) in Barriers)
            {
                // O que cada barreira guarda fica logo a oeste dela.
                foreach (LevelMap.Reach stage in stages)
                {
                    if (stage.CanStand(x - 2, y)) problems.Add($"Da pra passar pela {name} sem quebra-la.");
                }
            }

            // Cada pilar tem que estar ao alcance na camada dele.
            foreach (ResonantSpot spot in Resonants)
            {
                if (spot.Note != 0) continue;

                int stage = 0;
                for (int i = 0; i < BossFloors.Length; i++)
                {
                    if (BossFloors[i] == spot.Y && spot.X >= 34 && spot.X <= 76) stage = i;
                }

                if (!stages[stage].CanStand(spot.StandX, spot.Y)) problems.Add($"{spot.Name}: nao da pra chegar em ({spot.StandX}, {spot.Y}).");
            }

            // 3. Chao todo desabado: do fundo tem que dar pra subir de volta.
            LevelMap bottom = Collapsed(Slabs.Count);
            CloseBarriers(bottom);

            int deepest = BossFloors[BossFloors.Length - 1];
            LevelMap.Reach climb = bottom.Explore((BossTile.X, deepest), rules);

            if (!climb.CanStand(78, 56)) problems.Add("Do fundo da arena nao da pra subir de volta ao corredor de entrada.");

            // 4. Tudo aberto: o resto do mapa.
            LevelMap.Reach before = Map.Explore(SpawnTile, rules);
            LevelMap.Reach reach = Collapsed(Slabs.Count).Explore(SpawnTile, rules);

            foreach (Marker marker in Markers)
            {
                if (!marker.MustReach) continue;

                bool reached = before.CanStand(marker.X, marker.Y) || reach.CanStand(marker.X, marker.Y);
                for (int i = 0; i < stages.Count && !reached; i++) reached = stages[i].CanStand(marker.X, marker.Y);

                if (!Map.IsStandable(marker.X, marker.Y)) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) nao e um lugar onde da pra ficar em pe.");
                else if (!reached) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) esta fora de alcance.");
            }

            foreach (LoreSpot spot in Purified)
            {
                if (!before.CanStand(spot.X, spot.Y)) problems.Add($"{spot.Name} ({spot.X}, {spot.Y}) esta fora de alcance.");
            }

            if (!reach.CanStand(CrisolTile.X, CrisolTile.Y)) problems.Add("Nao da pra chegar em Crisol.");

            foreach (FoeSpot foe in Foes)
            {
                int from = foe.Kind == FoeKind.ShardSentinel ? foe.X0 : foe.X0;
                int to = foe.Kind == FoeKind.ShardSentinel ? foe.X0 : foe.X1;

                for (int x = from; x <= to; x++)
                {
                    if (!Map.IsStandable(x, foe.Y) && !Map.IsSolid(x, foe.Y))
                    {
                        // Passarela e degrau no meio do trecho nao atrapalham
                        // quem anda no chao; buraco, sim.
                        if (!Map.IsSolid(x, foe.Y - 1)) problems.Add($"{foe.Name} anda por ({x}, {foe.Y}), que nao tem chao.");
                    }
                }
            }

            foreach ((int x0, int x1, int y) in Fragiles)
            {
                // Uma ponte fragil precisa de ar em cima, senao ninguem pisa nela.
                for (int x = x0; x <= x1; x++)
                {
                    if (Map.IsSolid(x, y + 1)) problems.Add($"A plataforma fragil em ({x}, {y}) tem terreno em cima.");
                }
            }

            return problems;
        }

        /// <summary>Os trechos de chao onde nao da pra chegar nem com tudo aberto e a arena desabada.</summary>
        public static List<string> UnreachableFloors()
        {
            LevelMap world = Collapsed(Slabs.Count);

            LevelMap.Reach reach = world.Explore(SpawnTile, Rules());
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
