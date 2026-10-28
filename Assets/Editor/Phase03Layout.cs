using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O desenho da Phase 3 — District 3: The Silent Geodes.
    ///
    /// Tuneis escavados dentro de cristais colossais, no escuro. Aqui quase
    /// nao ha luz fixa: o que se ve e o que o proprio barulho do Scrap acende.
    /// E o mesmo barulho acorda o que mora ali. O dash e o golpe sao altos; o
    /// distrito pede, o tempo todo, que se ande leve.
    ///
    /// Como o District 2, e uma subida, organizada em volta de um centro:
    ///
    /// A ENTRADA (no fundo, linear). 1 a Estacao de Chegada, onde o elevador
    /// do District 2 para; 2 o Tunel Escuro (o som acende o caminho); 3 o
    /// Primeiro Cacador (passar sem barulho, ou com uma isca); 4 o Poco Fragil
    /// (subir em cristal fino, sem dash).
    ///
    /// O CENTRO: 5 a Gruta de Crisol — bancada, Crisol e a loja.
    ///
    /// O CAMINHO, pra cima: a chamine da gruta leva a 10 Abobada Fragil
    /// (subir no escuro sem quebrar as laminas) e ao 11 Campanario, cujo
    /// quebra-cabeca de notas abre a porta da 12 antessala da broca.
    ///
    /// OS DESVIOS, opcionais, cada um terminando num quebra-cabeca de notas
    /// que abre alguma coisa ali mesmo:
    ///   Oeste (6 Galeria das Gravuras, 7 Camara do Eco)   a historia da escavacao, e um nicho
    ///   Leste (8 Galeria dos Cacadores, 9 Camara do Ressonante)   o elite e a safira
    ///
    /// O FIO DA FASE: o elevador. Ele veio do District 2 e seguia subindo ate
    /// os Alchemical Ateliers, mas o poco desabou logo acima da cabine. Os
    /// escombros ficam a vista desde o primeiro segundo, e nada do que o Scrap
    /// tem os tira dali: so o Resonance Strike, que a broca deixa ao cair. O
    /// distrito inteiro e o desvio pra buscar essa onda e voltar.
    ///
    /// O BOSS E DEPOIS: a 13 Camara de Perfuracao, em tres camadas que
    /// desabam durante a luta. Da antessala, um poco largo cai direto na gruta:
    /// e o atalho de volta (pra cima, por ele, nao se sobe). Da gruta se desce
    /// ate a estacao, os escombros cedem a onda, e o elevador sobe.
    ///
    /// Aqui o Scrap ja tem o Steam Dash e o Pressure Jump, e o mapa conta com
    /// os dois. Como os outros layouts, este arquivo nao depende de UnityEngine.
    /// </summary>
    public static class Phase03Layout
    {
        public const int Width = 210;
        public const int Height = 124;

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

        /// <summary>
        /// Um cristal ressonante. Note 0 e so isca (ou pilar da arena); de 1
        /// pra cima sao as notas de uma porta — a de nome Door.
        /// </summary>
        public sealed class ResonantSpot
        {
            public string Name;
            public int X;
            public int Y;
            public int Note;
            public bool Corrupted;
            public string Door;

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
        /// (so a beirada), "miner" um mineiro Tarnished e "crisol" o proprio
        /// Crisol — esses dois so nas listas do distrito purificado.
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

        /// <summary>
        /// Um quebra-cabeca de notas: os cristais com Door igual a Name, a
        /// sequencia, o portao que abre, a area que faz a porta tocar a
        /// sequencia e onde ficam as gemas gravadas (uma por nota).
        /// </summary>
        public sealed class DoorSpec
        {
            public string Name;
            public string Gate;
            public int[] Sequence;
            public (int X, int Y, int Width, int Height) Preview;
            public (float X, float Y) Glyphs;
        }

        /// <summary>Um saco de fragmentos escondido.</summary>
        public sealed class RewardSpot
        {
            public string Id;
            public int X;
            public int Y;
            public int Amount;
        }

        /// <summary>
        /// Um sprite de enfeite (da arte de latao: tambor, corrente, placa,
        /// prateleira, a cabine do elevador...). X e Y sao o canto de baixo;
        /// com Tiled, ele se repete ate Width x Height tiles.
        /// </summary>
        public readonly struct DecorSpot
        {
            public readonly string Sprite;
            public readonly float X;
            public readonly float Y;
            public readonly int Order;
            public readonly float Width;
            public readonly float Height;

            public DecorSpot(string sprite, float x, float y, int order, float width = 0f, float height = 0f)
            {
                Sprite = sprite;
                X = x;
                Y = y;
                Order = order;
                Width = width;
                Height = height;
            }

            public bool Tiled => Width > 0f && Height > 0f;
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
        public static readonly List<GateSpec> Gates = new List<GateSpec>();
        public static readonly List<DoorSpec> Doors = new List<DoorSpec>();
        public static readonly List<RewardSpot> Rewards = new List<RewardSpot>();
        public static readonly List<DecorSpot> Decor = new List<DecorSpot>();

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

        /// <summary>O brilho verde do Oxide: no fundo da arena e no fundo da fenda da galeria.</summary>
        public static readonly List<(float X, float Y, float Radius)> OxideGlows = new List<(float, float, float)>();

        /// <summary>Quem aparece depois da purificacao: os mineiros e Crisol, no salao de concertos.</summary>
        public static readonly List<LoreSpot> Purified = new List<LoreSpot>();

        /// <summary>As celulas que sao objeto, e nao tile: plataformas frageis e chaos que desabam.</summary>
        private static readonly HashSet<(int, int)> FragileCells = new HashSet<(int, int)>();

        /// <summary>A porta do Campanario: e ela que abre o caminho pra broca.</summary>
        public const string MainDoor = "Door_High";

        /// <summary>Onde o Scrap aparece: dentro da cabine do elevador que veio do District 2.</summary>
        public static (int X, int Y) SpawnTile => (8, 11);

        /// <summary>A cabine do elevador: canto de baixo, a esquerda, e largura (3x3 tiles).</summary>
        public static (int X, int Y, int Width) ElevatorCar => (6, 11, 3);

        /// <summary>Ate onde a cabine sobe no poco antes de a tela escurecer e o District 4 carregar.</summary>
        public const float ElevatorTopY = 38f;

        /// <summary>
        /// Os escombros que entopem o poco logo acima da cabine. Pro mapa sao
        /// rocha ate a broca cair (como as barreiras); quem os desenha e os
        /// quebra e o objeto deles, com o Resonance Strike.
        /// </summary>
        public static (int X, int Y, int Width, int Height) ElevatorRubble => (5, 14, 5, 8);

        /// <summary>O que aparece ao tentar subir com os escombros no lugar.</summary>
        public const string ElevatorBlockedNotice = "O poço está entupido de escombros. Nada que você tem os move.";

        public static (int X, int Y) CrisolTile => (140, 44);

        public static readonly string[] CrisolLines =
        {
            "Scrap. Você chegou até aqui. Admito que não apostei nisso.",
            "Olhe em volta. Ametista, quartzo leitoso, cristal fumê... Isto aqui é o meu ofício. Aqui eu sei o nome de tudo.",
            "Você reparou que eles só acendem com barulho? Era assim que os mineiros enxergavam: batiam na pedra e andavam enquanto ela brilhava.",
            "Eu trabalhei para os 925 Nobres. Eu estava lá quando resolveram furar até o Coração de Quartzo. Eu VI a escavação.",
            "Eu disse que era um erro. Disse na frente de todos. Sabe o que fazem com prata que fala demais? Riscam.",
            "É por isso que sou arranhado assim, Scrap. Não foi o tempo. Foi de propósito. Riscaram-me e jogaram-me no Poço de Sucata.",
            "O elevador que trouxe você? Ele seguia subindo, até os Ateliês dos Nobres. O poço desabou no dia em que a broca furou fundo demais.",
            "Pedra assim não cede a cinzel. Cede a uma nota, a nota certa. E quem ainda canta essa nota é aquela máquina, lá em cima, na câmara de perfuração.",
            "...Chega. O seu polimento está decente, aliás. Não conte a ninguém que eu disse isso.",
        };

        // The Resonant: onde ele espera, ate onde anda e a area que o acorda.
        public static (int X, int Y) EliteTile => (182, 60);
        public static (int X0, int X1) EliteRange => (167, 197);
        public static (int X, int Y, int Width, int Height) EliteWake => (169, 60, 27, 12);

        // The Geode Drill: onde aparece, ate onde corre, a area que a acorda e
        // a altura do chao de cada uma das tres camadas da arena.
        public static (int X, int Y) BossTile => (55, 92);
        public static (int X0, int X1) BossRange => (36, 74);
        public static (int X, int Y, int Width, int Height) BossWake => (42, 92, 28, 14);
        public static readonly int[] BossFloors = { 92, 80, 68 };

        /// <summary>A placa gravada no chassi da broca, que so aparece quando ela para.</summary>
        public static readonly string[] DrillPlaqueLines =
        {
            "Por baixo da crosta de Óxido há uma placa de prata, rebitada no chassi.",
            "\"POR ORDEM DE SUA MAJESTADE, O REI DE PRATA.\"",
            "Embaixo, a data da primeira perfuração. E mais nada. Nenhum nome de mineiro.",
            "Então foi ele. Não foi acidente nem praga: foi uma ordem.",
        };

        /// <summary>O brilho verde no fundo da fenda da Galeria das Gravuras.</summary>
        public static (float X, float Y) HeartGlow => (58.5f, 31f);

        /// <summary>A fenda da galeria: sem parede de fundo, pra parecer funda.</summary>
        private static bool IsRift(int x, int y) => x >= 56 && x <= 61 && y >= 30 && y <= 43;

        // ------------------------------------------------------------------
        // Ferramentas de construcao
        // ------------------------------------------------------------------

        static Phase03Layout()
        {
            BuildArrival();
            BuildDarkTunnel();
            BuildFirstHunter();
            BuildFragileShaft();
            BuildGrotto();
            BuildNoteShaft();

            BuildEngravingGallery();
            BuildEchoChamber();
            BuildHunterGallery();
            BuildResonantChamber();
            BuildFragileVault();
            BuildBelfry();

            BuildPreBoss();
            BuildDrillChamber();
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

        private static void Resonant(string group, string name, int x, int y, int note, bool corrupted = false,
            int slab = -1, string door = null)
        {
            // Golpeia-se de lado: ele para na celula a esquerda do cristal.
            Resonants.Add(new ResonantSpot
            {
                Name = name, X = x, Y = y, Note = note, Corrupted = corrupted, StandX = x - 1, Slab = slab, Door = door,
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

        private static void Gate(string name, int x, int y, int width, int height, int moveX = 0, int moveY = 1)
        {
            Gates.Add(new GateSpec { Name = name, X = x, Y = y, Width = width, Height = height, MoveX = moveX, MoveY = moveY });
        }

        private static void Reward(string group, string id, int x, int y, int amount)
        {
            Rewards.Add(new RewardSpot { Id = id, X = x, Y = y, Amount = amount });
            Mark(group, id, x, y);
        }

        private static void Bench(string group, string name, int x, int y)
        {
            Workbenches.Add((name, x, y));
            Mark(group, name + "_Rest", x + 1, y);
        }

        private static void Put(string sprite, float x, float y, int order = 5) => Decor.Add(new DecorSpot(sprite, x, y, order));

        private static void Barrel(int x, int y) => Put("decor_barrel", x, y, 6);

        private static void Sign(int x, int y) => Put("decor_sign", x, y, 5);

        private static void Shelf(int x, int y) => Put("decor_shelf", x, y, 5);

        /// <summary>Corrente pendurada do teto, de topAir pra baixo.</summary>
        private static void HangChain(int x, int topAir, int length)
        {
            Decor.Add(new DecorSpot("decor_chain", x, topAir - length + 1, 5, 1f, length));
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

        /// <summary>A estacao de chegada ainda e latao: o elevador veio de la.</summary>
        private static bool IsBrass(int x, int y) => x <= 17 && y >= 8 && y <= 24;

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

        /// <summary>A parede do fundo, ou null atras de rocha macica e na fenda da galeria.</summary>
        public static string BackgroundTileName(int x, int y)
        {
            if (IsRift(x, y)) return null;

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
        // 1. A Estacao de Chegada
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde o elevador do District 2 para: a ultima chapa de latao, uma
        /// lanterna e o aviso dos mineiros. O poco segue pra cima, ate os
        /// Ateliers, mas desabou logo acima da cabine: os escombros sao a
        /// primeira coisa que se ve, e o que a fase inteira vai buscar. Logo
        /// atras da cabine, um nicho fechado por cristal macico — tambem so
        /// abre com o que a broca deixa.
        /// </summary>
        private static void BuildArrival()
        {
            const string room = "01_Arrival";
            Room(room, 5, 11, 28, 21);

            Carve(5, 11, 28, 21);
            Solid(16, 19, 22, 21);

            // O poco, subindo acima da cabine ate onde a tela escurece. Os
            // trilhos vao ate o alto; a cabine em si e o objeto do elevador.
            (int carX, int carY, int carWidth) = ElevatorCar;
            Carve(carX, 22, carX + carWidth - 1, (int)ElevatorTopY + 4);
            Decor.Add(new DecorSpot("elevator_rail", carX, carY + 3, 4, carWidth, ElevatorTopY + 5 - (carY + 3)));
            Sign(10, 16);
            Barrel(12, 11);

            Tell("sign", room, "Lore_ElevatorShaft", "o aviso do elevador", 11, 11,
                "Uma chapa de latão aparafusada ao lado da cabine: \"ELEVADOR DE SERVIÇO — SEGUE ATÉ OS ATELIÊS ALQUÍMICOS.\"",
                "Por cima, pintado às pressas: \"INTERDITADO. O POÇO DESABOU COM A BROCA.\"",
                "E, a giz, numa letra miúda: \"Pá não resolve. Já tentamos. A pedra só ouve a máquina.\"");

            // O nicho do segredo.
            Carve(1, 11, 3, 14);
            Barrier("Barrier_Secret", 4, 11, 4);
            Reward(room, "D3_Secret_Cache", 2, 11, 20);

            Tell("sign", room, "Lore_Frontier", "o aviso dos mineiros", 15, 11,
                "Uma tábua pregada na última chapa de latão. A tinta descascou, mas dá para ler:",
                "\"DAQUI PARA A FRENTE NÃO HÁ LAMPIÃO. A PEDRA ACENDE COM BARULHO.\"",
                "E, mais embaixo, em letra menor: \"...E o barulho acorda os que ficaram.\"");

            Lamps.Add((9f, 15f, 7f));
            Lamps.Add((15.5f, 15f, 6f));

            Scatter(18, 11, 28, 21, 4, 11);
        }

        // ------------------------------------------------------------------
        // 2. O Tunel Escuro
        // ------------------------------------------------------------------

        /// <summary>
        /// Escuridao total e tres fossos de espinhos. Nao ha inimigo: e a sala
        /// que ensina a fazer barulho de proposito pra enxergar onde pisar. O
        /// primeiro fosso e largo: pede o segundo pulo, que e silencioso.
        /// </summary>
        private static void BuildDarkTunnel()
        {
            const string room = "02_DarkTunnel";
            Room(room, 29, 6, 78, 21);

            Carve(29, 6, 78, 21);

            Solid(29, 6, 36, 10);
            Spikes(37, 42, 6);
            Solid(43, 6, 50, 8);
            Spikes(51, 55, 6);
            Solid(56, 6, 64, 7);
            Spikes(65, 68, 6);
            Solid(69, 6, 78, 10);

            // Teto irregular.
            Solid(40, 18, 48, 21);
            Solid(58, 16, 62, 21);

            Scatter(29, 6, 78, 21, 5, 21);
        }

        // ------------------------------------------------------------------
        // 3. O Primeiro Cacador
        // ------------------------------------------------------------------

        /// <summary>
        /// Um Echo Hunter parado no meio do corredor. Passa-se por ele so
        /// andando, ou manda-se ele pra uma ponta golpeando o cristal de la.
        /// Ha tambem uma passarela por cima, e nela o capacete de quem correu.
        /// </summary>
        private static void BuildFirstHunter()
        {
            const string room = "03_FirstHunter";
            Room(room, 79, 11, 108, 20);

            Carve(79, 11, 108, 20);

            Ledge(85, 90, 14);
            Ledge(93, 98, 14);
            Ledge(101, 105, 14);

            Foe(FoeKind.EchoHunter, "EchoHunter_First", 80, 107, 11, 95);

            Resonant(room, "Resonant_Hunter_West", 82, 11, 0);
            Resonant(room, "Resonant_Hunter_East", 107, 11, 0);

            Tell("page", room, "Lore_Helmet", "um capacete amassado", 87, 15,
                "Um capacete de mineiro, amassado, esquecido em cima da passarela.",
                "Por dentro, riscado a prego: \"NÃO CORRA. ELES ESCUTAM.\"");

            Scatter(79, 11, 108, 20, 5, 31);
        }

        // ------------------------------------------------------------------
        // 4. O Poco Fragil
        // ------------------------------------------------------------------

        /// <summary>
        /// O caminho pro centro e pra cima, por laminas de cristal fino
        /// penduradas sobre espinhos. Pular e silencioso e elas aguentam; o
        /// dash, o golpe e o tombo as quebram. Dois patamares de rocha firme
        /// no meio, pra respirar.
        /// </summary>
        private static void BuildFragileShaft()
        {
            const string room = "04_FragileShaft";
            Room(room, 109, 7, 120, 43);

            Carve(109, 7, 120, 38);
            Carve(116, 39, 120, 43);

            // A entrada, na altura do corredor do cacador, e os espinhos do fundo.
            Solid(109, 7, 111, 10);
            Spikes(112, 120, 7);

            Fragile(114, 117, 13);
            Fragile(118, 120, 17);
            Fragile(113, 116, 21);
            Solid(109, 23, 111, 25);
            Fragile(114, 117, 29);
            Fragile(118, 120, 33);
            Fragile(113, 116, 37);
            Fragile(117, 120, 41);

            Scatter(109, 8, 120, 43, 4, 41);
        }

        // ------------------------------------------------------------------
        // 5. A Gruta de Crisol
        // ------------------------------------------------------------------

        /// <summary>
        /// O centro do distrito, e a unica luz parada dele. A oeste, o patamar
        /// do Poco Fragil e a porta da Galeria das Gravuras; a leste, a
        /// Galeria dos Cacadores; no teto, a chamine da Abobada Fragil e o Poco
        /// das Notas, que sobe pra broca.
        /// </summary>
        private static void BuildGrotto()
        {
            const string room = "05_Grotto";
            Room(room, 109, 44, 152, 54);

            // O patamar do poco e a gruta.
            Carve(109, 44, 121, 48);
            Carve(122, 44, 152, 54);
            Solid(147, 52, 152, 54);

            Bench(room, "Workbench_Grotto", 124, 44);
            Mark(room, "Crisol", CrisolTile.X, CrisolTile.Y);

            Lamps.Add((126f, 49f, 7f));
            Lamps.Add((140.5f, 49f, 7f));
            Lamps.Add((115f, 47f, 5f));

            Barrel(144, 44);
            Barrel(145, 44);
            Shelf(141, 48);
            Sign(113, 46);

            // A porta da Galeria dos Cacadores.
            Carve(153, 44, 153, 48);

            Scatter(122, 44, 152, 54, 6, 51);
        }

        /// <summary>
        /// O atalho de volta: um poco largo, liso, que cai da antessala da
        /// broca direto no teto da gruta. Largo demais pra subir quicando nas
        /// paredes, e sem ressalto nenhum — so serve pra descer.
        /// </summary>
        private static void BuildNoteShaft()
        {
            Carve(128, 55, 134, 91);
            HangChain(131, 91, 6);
            HangChain(129, 80, 4);
            Scatter(128, 55, 134, 91, 3, 55);
        }

        // ------------------------------------------------------------------
        // 6. A Galeria das Gravuras (Ala Oeste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Quatro gravuras enormes contam a escavacao, de leste pra oeste. So
        /// da pra ve-las com barulho — e uma Shard Sentinel, no meio, atira no
        /// barulho. Os Crystal Mites dormem entre as gravuras. No meio do chao,
        /// a fenda: la no fundo, o verde do Coracao de Quartzo.
        /// </summary>
        private static void BuildEngravingGallery()
        {
            const string room = "06_EngravingGallery";
            Room(room, 36, 44, 108, 56);

            Carve(36, 44, 108, 56);
            Solid(36, 54, 44, 56);
            Solid(70, 44, 74, 46);

            // A fenda.
            Carve(56, 30, 61, 43);
            Spikes(56, 61, 30);
            OxideGlows.Add((58.5f, 31f, 6f));

            Tell("edge", room, "Lore_Rift", "a beirada da fenda", 54, 44,
                "O chão se abre aqui. A fenda é estreita, lisa e funda demais para ter sido feita à mão.",
                "Lá no fundo, muito longe, alguma coisa verde pulsa. Devagar. Como se respirasse.",
                "Não há como descer. Ainda.");

            Tell("engraving0", room, "Lore_Engraving_1", "a primeira gravura", 100, 44,
                "Riscada na pedra, do tamanho de uma parede: uma fila de mineiros de picareta no ombro.",
                "Estão descendo. Todos olham para baixo.");

            Tell("engraving1", room, "Lore_Engraving_2", "a segunda gravura", 86, 44,
                "Uma figura maior que as outras, de coroa: o Rei de Prata.",
                "Ele não segura ferramenta nenhuma. Só aponta. Para baixo.");

            Tell("engraving2", room, "Lore_Engraving_3", "a terceira gravura", 64, 44,
                "Uma broca em espiral, descendo pela rocha. Em volta dela, riscos de impacto.",
                "Os mineiros agora são menores. Estão atrás dela, não na frente.");

            Tell("engraving3", room, "Lore_Engraving_4", "a última gravura", 47, 44,
                "Um losango enorme no fundo da terra: o Coração de Quartzo.",
                "Há uma fenda nele, de cima a baixo. Quem gravou pintou a fenda de verde.",
                "Depois desta, a parede está lisa. Ninguém voltou para gravar o resto.");

            // A sentinela fica num pedestal no meio, e os mites entre as gravuras.
            Foe(FoeKind.ShardSentinel, "Sentinel_Gallery", 72, 72, 47);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_1", 91, 96, 44, 93);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_2", 91, 96, 44, 95);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_3", 76, 81, 44, 78);
            Foe(FoeKind.CrystalMite, "Mite_Gallery_4", 76, 81, 44, 80);

            // No fundo da galeria, depois da ultima gravura.
            Reward(room, "D3_Gallery_Cache", 38, 44, 10);

            Scatter(36, 44, 108, 56, 5, 61);
        }

        // ------------------------------------------------------------------
        // 7. A Camara do Eco (fim da Ala Oeste)
        // ------------------------------------------------------------------

        /// <summary>
        /// O quebra-cabeca da Ala Oeste: tres cristais em tres alturas e um
        /// cacador que corre pra cada nota tocada. Toca-se, sai-se de perto, e
        /// toca-se a proxima. A sequencia certa abre o nicho da parede oeste.
        /// </summary>
        private static void BuildEchoChamber()
        {
            const string room = "07_EchoChamber";
            Room(room, 4, 44, 35, 60);

            Carve(8, 44, 35, 60);

            // O nicho que a sequencia abre, na parede oeste.
            Carve(4, 44, 7, 47);
            Gate("Porta_Eco", 7, 44, 1, 4);
            Reward(room, "D3_Echo_Cache", 5, 44, 20);
            Ledge(17, 22, 47);
            Ledge(25, 30, 51);

            Resonant(room, "Resonant_West_1", 12, 44, 1, false, -1, "Door_West");
            Resonant(room, "Resonant_West_2", 21, 48, 2, false, -1, "Door_West");
            Resonant(room, "Resonant_West_3", 29, 52, 3, false, -1, "Door_West");

            Doors.Add(new DoorSpec
            {
                Name = "Door_West", Gate = "Porta_Eco", Sequence = new[] { 2, 3, 1 },
                Preview = (31, 44, 5, 5), Glyphs = (31f, 51f),
            });

            Foe(FoeKind.EchoHunter, "EchoHunter_Echo", 9, 34, 44, 17);

            Tell("sign", room, "Lore_EchoChamber", "a placa de afinação", 33, 44,
                "Uma placa de prata, pregada na entrada: \"CÂMARA DE AFINAÇÃO.\"",
                "Embaixo, à mão: \"Toque as notas na ordem das gemas. E depois corra — devagar.\"");

            Scatter(8, 44, 35, 60, 5, 71);
        }

        // ------------------------------------------------------------------
        // 8. A Galeria dos Cacadores (Ala Leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Um corredor longo com tres cacadores dormindo no chao e uma
        /// passarela de cristal fino por cima. Por baixo, o silencio; por cima,
        /// o peso. As notas da trava estao espalhadas pelo corredor: cada uma
        /// tocada chama quem estiver perto. Uma delas tem veio de Oxide e
        /// precisa ser raspada antes — de costas pros cacadores. A sequencia
        /// certa abre a grade do teto, no fim do corredor, que leva ao elite.
        /// </summary>
        private static void BuildHunterGallery()
        {
            const string room = "08_HunterGallery";
            Room(room, 154, 44, 202, 56);

            Carve(154, 44, 202, 56);

            Fragile(160, 165, 47);
            Fragile(169, 174, 47);
            Fragile(178, 183, 47);
            Fragile(187, 191, 47);

            Foe(FoeKind.EchoHunter, "EchoHunter_Gallery_1", 155, 175, 44, 163);
            Foe(FoeKind.EchoHunter, "EchoHunter_Gallery_2", 168, 190, 44, 180);
            Foe(FoeKind.EchoHunter, "EchoHunter_Gallery_3", 183, 201, 44, 195);

            Resonant(room, "Resonant_East_1", 158, 44, 1, false, -1, "Door_East");
            Resonant(room, "Resonant_East_2", 188, 44, 2, true, -1, "Door_East");
            Resonant(room, "Resonant_East_3", 172, 44, 3, false, -1, "Door_East");

            Doors.Add(new DoorSpec
            {
                Name = "Door_East", Gate = "Porta_Ressonante", Sequence = new[] { 3, 1, 2 },
                Preview = (154, 44, 4, 5), Glyphs = (155f, 53f),
            });

            Tell("sign", room, "Lore_HunterRoster", "a lista de turno", 156, 44,
                "Uma lista de turno pregada na rocha: doze nomes.",
                "Ao lado de nove deles, alguém desenhou um pequeno cristal.",
                "Os outros três foram riscados com tanta força que a tábua rachou.");

            // A subida pra Camara do Ressonante, no fim do corredor: um vao
            // estreito no teto, fechado pela grade que a sequencia abre.
            Ledge(194, 196, 51);
            Ledge(197, 200, 52);
            Carve(197, 57, 200, 59);
            Gate("Porta_Ressonante", 197, 57, 4, 1, 1, 0);

            Reward(room, "D3_Hunter_Cache", 189, 48, 10);

            Scatter(154, 44, 202, 56, 5, 81);
        }

        // ------------------------------------------------------------------
        // 9. A Camara do Ressonante (Ala Leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Em cima da Galeria dos Cacadores: a camara larga e alta onde mora
        /// The Resonant, o elite que guarda a safira. A luta e opcional. Depois
        /// da purificacao vira o salao de concertos dos mineiros. Na entrada, a
        /// bancada da ala.
        /// </summary>
        private static void BuildResonantChamber()
        {
            const string room = "09_ResonantChamber";
            Room(room, 164, 60, 202, 74);

            Carve(164, 60, 202, 74);
            Ledge(172, 177, 63);
            Ledge(187, 192, 63);

            Bench(room, "Workbench_Chamber", 193, 60);
            Lamps.Add((194.5f, 64f, 6f));

            Mark(room, "Elite_TheResonant", EliteTile.X, EliteTile.Y);

            Foe(FoeKind.CrystalMite, "Mite_Chamber_1", 166, 175, 60, 168);
            Foe(FoeKind.CrystalMite, "Mite_Chamber_2", 166, 175, 60, 172);
            Foe(FoeKind.CrystalMite, "Mite_Chamber_3", 184, 190, 60, 187);

            Purified.Add(new LoreSpot
            {
                Kind = "miner", Name = "Tarnished_Miner_1", Title = "Mineiro", X = 170, Y = 60,
                Lines = new[]
                {
                    "Escute. ...Ouviu? Ela afinou. A pedra inteira afinou.",
                    "A gente passou anos aqui embaixo sem poder dar um pio. Agora vamos fazer barulho de propósito.",
                    "Vai haver concerto. Você é convidado, pequeno. Foi você que desligou aquela coisa.",
                },
            });

            Purified.Add(new LoreSpot
            {
                Kind = "miner", Name = "Tarnished_Miner_2", Title = "Mineira", X = 190, Y = 60,
                Lines = new[]
                {
                    "Café? É de borra de caldeira, mas está quente.",
                    "Eu operei a broca, sabia? No começo, éramos seis lá dentro. Depois, ela não precisou mais da gente.",
                    "Ninguém nos disse o que havia lá embaixo. Só disseram: mais fundo.",
                },
            });

            Purified.Add(new LoreSpot
            {
                Kind = "crisol", Name = "Npc_Crisol_Purified", Title = "Crisol", X = 180, Y = 60,
                Lines = new[]
                {
                    "Você viu a placa. Então agora você sabe o que eu sabia.",
                    "Eu avisei, e riscaram-me. Você desceu lá, com esse corpo de sucata, e fez a máquina parar.",
                    "...Obrigado, Scrap.",
                    "Talvez você não seja só sucata, afinal. Não me faça repetir.",
                    "E agora essa onda é sua. Volte à estação: os escombros do elevador não vão resistir a ela. Lá em cima ficam os Ateliês.",
                },
            });

            Scatter(164, 60, 202, 74, 5, 91);
        }

        // ------------------------------------------------------------------
        // 10. A Abobada Fragil (Ala Alta)
        // ------------------------------------------------------------------

        /// <summary>
        /// Em cima da gruta, pela chamine: uma abobada alta, escura, de laminas
        /// finas sobre espinhos. Pra enxergar e preciso barulho; o barulho
        /// quebra as laminas. Os cristais ressonantes nos patamares de rocha
        /// firme sao a resposta: golpeia-se um de la, a abobada inteira acende,
        /// e sobe-se no escuro que vem depois, lembrando o caminho.
        /// </summary>
        private static void BuildFragileVault()
        {
            const string room = "10_FragileVault";
            Room(room, 137, 58, 162, 99);

            // A chamine, da gruta pra abobada.
            Carve(143, 55, 146, 59);
            Ledge(141, 143, 47);
            Ledge(145, 146, 51);

            Carve(138, 60, 162, 97);
            Solid(137, 58, 152, 59);
            Carve(143, 58, 146, 59);
            Spikes(147, 162, 60);

            // A subida: um patamar firme e tres laminas, repetido.
            Solid(137, 60, 142, 62);
            Fragile(147, 151, 66);
            Fragile(154, 158, 70);
            Fragile(147, 151, 74);
            Solid(137, 75, 142, 77);
            Fragile(147, 151, 81);
            Fragile(154, 158, 85);
            Fragile(147, 151, 89);
            Solid(137, 90, 142, 92);
            Fragile(147, 151, 95);

            // As lanternas de som: um cristal em cada patamar firme.
            Resonant(room, "Resonant_Vault_1", 141, 63, 0);
            Resonant(room, "Resonant_Vault_2", 141, 78, 0);
            Resonant(room, "Resonant_Vault_3", 141, 93, 0);

            Tell("page", room, "Lore_VaultPage", "uma folha dobrada", 138, 78,
                "Uma folha dobrada, presa numa fresta da rocha:",
                "\"A abóbada canta quando a gente pisa errado. Aprendemos a pisar como quem reza.\"");

            Foe(FoeKind.CrystalMite, "Mite_Vault_1", 137, 140, 93, 138);

            Scatter(137, 60, 162, 99, 4, 101);
        }

        // ------------------------------------------------------------------
        // 11. O Campanario (fim da Ala Alta)
        // ------------------------------------------------------------------

        /// <summary>
        /// No alto da abobada: quatro cristais pendurados em alturas
        /// diferentes, como sinos. A sequencia mais longa do distrito abre a
        /// porta oeste, que da na antessala da broca. Um saco de fragmentos,
        /// pra quem sobe ate o ultimo ressalto.
        /// </summary>
        private static void BuildBelfry()
        {
            const string room = "11_Belfry";
            Room(room, 137, 100, 166, 112);

            Carve(137, 100, 166, 112);
            Carve(147, 98, 151, 99);
            Ledge(146, 150, 103);
            Ledge(154, 158, 107);
            Ledge(161, 165, 103);

            Bench(room, "Workbench_Belfry", 138, 100);
            Lamps.Add((139.5f, 104f, 5f));

            Resonant(room, "Resonant_High_1", 144, 100, 1, false, -1, "Door_High");
            Resonant(room, "Resonant_High_2", 149, 104, 2, false, -1, "Door_High");
            Resonant(room, "Resonant_High_3", 157, 108, 3, false, -1, "Door_High");
            Resonant(room, "Resonant_High_4", 165, 100, 4, true, -1, "Door_High");

            Doors.Add(new DoorSpec
            {
                Name = "Door_High", Gate = "Porta_Campanario", Sequence = new[] { 4, 2, 1, 3 },
                Preview = (137, 100, 6, 4), Glyphs = (142f, 103f),
            });

            Reward(room, "D3_Belfry_Cache", 163, 104, 20);

            // A porta, na parede oeste: atras dela, um poco curto que cai na
            // antessala da broca.
            Carve(135, 92, 136, 103);
            Gate("Porta_Campanario", 136, 100, 1, 4);

            Foe(FoeKind.CrystalMite, "Mite_Belfry_1", 152, 160, 100, 156);

            Scatter(137, 100, 166, 112, 5, 111);
        }

        // ------------------------------------------------------------------
        // 12. A antessala da broca
        // ------------------------------------------------------------------

        private static void BuildPreBoss()
        {
            const string room = "12_PreBoss";
            Room(room, 80, 92, 134, 98);

            Carve(80, 92, 134, 98);

            Bench(room, "Workbench_PreBoss", 84, 92);
            Lamps.Add((86f, 96f, 7f));

            Tell("page", room, "Lore_MinerNote", "um bilhete de mineiro", 96, 92,
                "Um papel dobrado, preso debaixo de um capacete.",
                "\"Ela não para mais. Desligamos a chave, e ela não parou. Não há mais ninguém lá dentro, e ela não parou.\"",
                "\"Se você está lendo isto: não faça barulho perto dela. Ela não enxerga. Ela escuta.\"");

            Barrel(90, 92);
            Barrel(91, 92);
            Shelf(104, 95);
            Sign(110, 95);

            Scatter(92, 92, 126, 98, 5, 121);
        }

        // ------------------------------------------------------------------
        // 13. A Camara de Perfuracao
        // ------------------------------------------------------------------

        /// <summary>
        /// A arena do Geode Drill, em tres camadas empilhadas. A luta comeca na
        /// de cima; a cada terco de vida a broca fura o chao e tudo desce uma.
        ///
        ///   camada 1 (chao 92)   quatro pilares ressonantes
        ///   camada 2 (chao 80)   dois
        ///   camada 3 (chao 68)   um so, e o brilho verde logo embaixo
        ///
        /// Na parede leste ha prateleiras em zigue-zague: e por onde se sobe de
        /// volta depois que o chao se foi — e dali, pelo atalho, ate a gruta e
        /// a estacao, onde a onda que a broca deixa abre o elevador.
        /// </summary>
        private static void BuildDrillChamber()
        {
            const string room = "13_DrillChamber";
            Room(room, 34, 68, 76, 112);

            Carve(77, 92, 79, 96);
            Carve(34, 68, 76, 112);

            Solid(34, 109, 38, 112);
            Solid(72, 109, 76, 112);

            int top = Slab("Slab_Drill_1", 34, 76, 90, 91);

            Solid(73, 78, 76, 79);
            int middle = Slab("Slab_Drill_2", 34, 72, 78, 79);

            Ledge(73, 76, 71);
            Ledge(66, 69, 75);
            Ledge(66, 69, 83);
            Ledge(72, 75, 87);

            Resonant(room, "Resonant_Pillar_1", 44, 92, 0, false, top);
            Resonant(room, "Resonant_Pillar_2", 52, 92, 0, false, top);
            Resonant(room, "Resonant_Pillar_3", 60, 92, 0, false, top);
            Resonant(room, "Resonant_Pillar_4", 68, 92, 0, false, top);
            Resonant(room, "Resonant_Pillar_5", 48, 80, 0, false, middle);
            Resonant(room, "Resonant_Pillar_6", 62, 80, 0, false, middle);
            Resonant(room, "Resonant_Pillar_7", 55, 68, 0);

            Mark(room, "BossSpawn_GeodeDrill", BossTile.X, BossTile.Y);

            OxideGlows.Add((42f, 68.5f, 8f));
            OxideGlows.Add((55f, 68.5f, 9f));
            OxideGlows.Add((68f, 68.5f, 8f));

            Scatter(34, 92, 76, 112, 6, 131);
            Scatter(34, 80, 72, 89, 7, 132);
            Scatter(34, 68, 72, 77, 7, 133);
        }

        // ------------------------------------------------------------------
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

        /// <summary>A sala que contem a celula, ou null.</summary>
        public static string RoomAt(int x, int y)
        {
            foreach ((string name, int x0, int y0, int x1, int y1) in Rooms)
            {
                if (x >= x0 && x <= x1 && y >= y0 && y <= y1) return name;
            }

            return null;
        }

        private sealed class WorldState
        {
            public LevelMap Map;
            public readonly HashSet<string> OpenDoors = new HashSet<string>();
            public bool BossDown;
        }

        private static void Fill(LevelMap world, GateSpec gate, char cell)
        {
            world.Fill(gate.X, gate.Y, gate.X + gate.Width - 1, gate.Y + gate.Height - 1, cell);
        }

        /// <summary>Fecha as barreiras de cristal e os escombros do elevador: o que so a onda abre.</summary>
        private static void CloseBarriers(LevelMap world) => SetBarriers(world, LevelMap.Rock);

        private static void SetBarriers(LevelMap world, char cell)
        {
            foreach ((string _, int x, int y, int height) in Barriers) world.Fill(x, y, x, y + height - 1, cell);

            (int rx, int ry, int rw, int rh) = ElevatorRubble;
            world.Fill(rx, ry, rx + rw - 1, ry + rh - 1, cell);
        }

        /// <summary>O mapa com os primeiros chaos da arena ja desabados.</summary>
        private static LevelMap Collapsed(LevelMap source, int slabsGone)
        {
            LevelMap world = source.Clone();

            for (int i = 0; i < slabsGone && i < Slabs.Count; i++)
            {
                (string _, int x0, int x1, int y0, int y1) = Slabs[i];
                world.Fill(x0, y0, x1, y1, LevelMap.Air);
            }

            return world;
        }

        /// <summary>
        /// Joga a fase na cabeca: portoes e barreiras fechados; abre cada porta
        /// cujos cristais todos se alcancam; derruba a broca quando se chega
        /// nela (a arena desaba inteira e a barreira quebra com o Resonance
        /// Strike). Repete ate nada mais mudar.
        /// </summary>
        private static WorldState Play(out LevelMap.Reach reach, bool bossFalls = true, Func<string, bool> allowDoor = null,
            Action<WorldState, LevelMap.Reach> visit = null)
        {
            var world = new WorldState { Map = Map.Clone() };
            foreach (GateSpec gate in Gates) Fill(world.Map, gate, LevelMap.Rock);
            CloseBarriers(world.Map);

            LevelMap.JumpRules rules = Rules();

            while (true)
            {
                reach = world.Map.Explore(SpawnTile, rules);
                visit?.Invoke(world, reach);
                bool changed = false;

                foreach (DoorSpec door in Doors)
                {
                    if (world.OpenDoors.Contains(door.Name)) continue;
                    if (allowDoor != null && !allowDoor(door.Name)) continue;

                    bool all = true;
                    foreach (ResonantSpot spot in Resonants)
                    {
                        if (spot.Door == door.Name && !reach.CanStand(spot.StandX, spot.Y)) all = false;
                    }

                    if (!all) continue;

                    world.OpenDoors.Add(door.Name);
                    changed = true;

                    foreach (GateSpec gate in Gates)
                    {
                        if (gate.Name == door.Gate) Fill(world.Map, gate, LevelMap.Air);
                    }
                }

                if (bossFalls && !world.BossDown && reach.CanStand(BossTile.X, BossTile.Y))
                {
                    world.BossDown = true;
                    changed = true;

                    world.Map = Collapsed(world.Map, Slabs.Count);
                    SetBarriers(world.Map, LevelMap.Air);
                }

                if (!changed) return world;
            }
        }

        /// <summary>
        /// Confere se a fase e jogavel de verdade, sem precisar jogar. Devolve a
        /// lista de problemas. Vazia quer dizer que esta tudo certo.
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
            WorldState world = Play(out LevelMap.Reach reach);

            foreach (DoorSpec door in Doors)
            {
                if (!world.OpenDoors.Contains(door.Name)) problems.Add($"{door.Name}: os cristais dela nao se alcancam todos.");
                if (!Gates.Exists(gate => gate.Name == door.Gate)) problems.Add($"{door.Name} abre '{door.Gate}', que nao existe.");

                foreach (int note in door.Sequence)
                {
                    if (!Resonants.Exists(spot => spot.Door == door.Name && spot.Note == note))
                    {
                        problems.Add($"{door.Name} pede a nota {note}, e nenhum cristal dela a toca.");
                    }
                }
            }

            if (!world.BossDown) problems.Add("Nao da pra chegar na broca.");

            Play(out LevelMap.Reach withoutBoss, false);

            foreach ((string name, int x, int y, int height) in Barriers)
            {
                if (withoutBoss.CanStand(x - 2, y)) problems.Add($"Da pra passar pela {name} sem quebra-la.");
            }

            CheckElevator(world, withoutBoss, problems);

            CheckArena(problems);

            // O caminho: sem a porta do Campanario, a broca fica fora de alcance.
            WorldState withoutBelfry = Play(out _, true, name => name != MainDoor);
            if (withoutBelfry.BossDown) problems.Add("Da pra chegar na broca sem o quebra-cabeca do Campanario.");
            CheckQuietSpots(problems);
            CheckEveryRoomPays(problems);
            CheckNoTraps(problems);

            foreach (Marker marker in Markers)
            {
                if (!marker.MustReach) continue;
                if (marker.Group == "13_DrillChamber") continue;

                bool reached = reach.CanStand(marker.X, marker.Y) || withoutBoss.CanStand(marker.X, marker.Y);

                if (!Map.IsStandable(marker.X, marker.Y)) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) nao e um lugar onde da pra ficar em pe.");
                else if (!reached) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) esta fora de alcance.");
            }

            foreach (LoreSpot spot in Purified)
            {
                if (!reach.CanStand(spot.X, spot.Y)) problems.Add($"{spot.Name} ({spot.X}, {spot.Y}) esta fora de alcance.");
            }

            if (!withoutBoss.CanStand(CrisolTile.X, CrisolTile.Y)) problems.Add("Nao da pra chegar em Crisol.");

            foreach (FoeSpot foe in Foes)
            {
                int to = foe.Kind == FoeKind.ShardSentinel ? foe.X0 : foe.X1;

                for (int x = foe.X0; x <= to; x++)
                {
                    if (!Map.IsSolid(x, foe.Y - 1)) problems.Add($"{foe.Name} anda por ({x}, {foe.Y}), que nao tem chao.");
                }
            }

            foreach ((int x0, int x1, int y) in Fragiles)
            {
                for (int x = x0; x <= x1; x++)
                {
                    if (Map.IsSolid(x, y + 1)) problems.Add($"A plataforma fragil em ({x}, {y}) tem terreno em cima.");
                }
            }

            return problems;
        }

        /// <summary>
        /// O fio da fase: a cabine se alcanca desde a chegada; os escombros
        /// fecham o poco inteiro (nao se passa por eles, nem por cima); o
        /// Scrap, de pe na cabine ou ao lado dela, fica ao alcance da onda; e
        /// do fundo da arena, com a broca no chao, da pra voltar ate a cabine.
        /// </summary>
        private static void CheckElevator(WorldState world, LevelMap.Reach withoutBoss, List<string> problems)
        {
            (int carX, int carY, int carWidth) = ElevatorCar;
            (int rx, int ry, int rw, int rh) = ElevatorRubble;

            if (!withoutBoss.CanStand(carX + 1, carY)) problems.Add("Nao da pra entrar na cabine do elevador.");

            // Os escombros cobrem a largura toda do poco, de parede a parede.
            for (int x = carX; x < carX + carWidth; x++)
            {
                if (x < rx || x > rx + rw - 1) problems.Add($"Os escombros nao tampam a coluna {x} do poco.");
            }

            for (int y = ry + rh; y <= ElevatorTopY + 4; y++)
            {
                for (int x = carX; x < carX + carWidth; x++)
                {
                    if (withoutBoss.CanStand(x, y)) problems.Add($"Da pra entrar no poco do elevador ({x}, {y}) sem tirar os escombros.");
                }
            }

            // A onda do Resonance Strike alcanca uns 3,5 tiles a partir do peito do Scrap.
            const float radius = 3.5f;
            float chestY = carY + 0.75f;
            float nearestX = Math.Max(rx, Math.Min(carX + 1.5f, rx + rw));
            float distance = (float)Math.Sqrt(Math.Pow(nearestX - (carX + 1.5f), 2) + Math.Pow(ry - chestY, 2));
            if (distance > radius - 0.5f) problems.Add($"De dentro da cabine, a onda nao chega nos escombros ({distance:0.0} tiles).");

            // Depois da luta: do fundo da arena desabada ate a cabine.
            LevelMap.Reach afterFight = world.Map.Explore((BossTile.X, BossFloors[BossFloors.Length - 1]), Rules());
            if (!afterFight.CanStand(carX + 1, carY)) problems.Add("Do fundo da arena, depois da broca, nao da pra voltar ate o elevador.");
        }

        /// <summary>
        /// A arena: com o chao inteiro, cada pilar se alcanca na camada dele;
        /// e, com tudo desabado, do fundo se sobe de volta ao corredor.
        /// </summary>
        private static void CheckArena(List<string> problems)
        {
            LevelMap.JumpRules rules = Rules();

            for (int gone = 0; gone < BossFloors.Length; gone++)
            {
                LevelMap world = Collapsed(Map, gone);
                foreach (GateSpec gate in Gates) Fill(world, gate, LevelMap.Air);
                CloseBarriers(world);

                LevelMap.Reach stage = world.Explore((BossTile.X, BossFloors[gone]), rules);

                foreach (ResonantSpot spot in Resonants)
                {
                    if (spot.Note != 0 || spot.Y != BossFloors[gone] || spot.X < BossRange.X0 || spot.X > BossRange.X1) continue;
                    if (!stage.CanStand(spot.StandX, spot.Y)) problems.Add($"{spot.Name}: nao da pra chegar em ({spot.StandX}, {spot.Y}) na camada dele.");
                }
            }

            LevelMap bottom = Collapsed(Map, Slabs.Count);
            CloseBarriers(bottom);
            LevelMap.Reach climb = bottom.Explore((BossTile.X, BossFloors[BossFloors.Length - 1]), rules);
            if (!climb.CanStand(78, 92)) problems.Add("Do fundo da arena nao da pra subir de volta ao corredor de entrada.");
        }

        /// <summary>
        /// Os lugares onde se para pra ler, sentar na bancada, falar com Crisol
        /// ou pegar recompensa nao ficam no ninho de ninguem: longe da casa de
        /// cacadores, do trecho dos mites e do pedestal das sentinelas.
        /// </summary>
        private static void CheckQuietSpots(List<string> problems)
        {
            var spots = new List<(string, int, int)>();
            foreach (LoreSpot spot in Lore) spots.Add((spot.Name, spot.X, spot.Y));
            foreach (RewardSpot spot in Rewards) spots.Add((spot.Id, spot.X, spot.Y));
            foreach ((string name, int x, int y) in Workbenches) spots.Add((name, x + 1, y));
            spots.Add(("Crisol", CrisolTile.X, CrisolTile.Y));

            foreach ((string name, int x, int y) in spots)
            {
                foreach (FoeSpot foe in Foes)
                {
                    if (foe.Y != y) continue;

                    bool near;
                    switch (foe.Kind)
                    {
                        case FoeKind.EchoHunter: near = Math.Abs(x - foe.HomeX) <= 3; break;
                        case FoeKind.ShardSentinel: near = Math.Abs(x - foe.X0) <= 2; break;
                        default: near = x >= foe.X0 - 2 && x <= foe.X1 + 2; break;
                    }

                    if (near) problems.Add($"{name} ({x}, {y}) fica no ninho de {foe.Name}.");
                }
            }
        }

        private static readonly string[] PassageRooms = { "02_DarkTunnel", "04_FragileShaft", "13_DrillChamber" };

        private static void CheckEveryRoomPays(List<string> problems)
        {
            foreach ((string name, int x0, int y0, int x1, int y1) in Rooms)
            {
                if (Array.IndexOf(PassageRooms, name) >= 0) continue;

                bool Inside(int x, int y) => x >= x0 && x <= x1 && y >= y0 && y <= y1;

                bool pays = Lore.Exists(spot => Inside(spot.X, spot.Y))
                    || Rewards.Exists(spot => Inside(spot.X, spot.Y))
                    || Inside(CrisolTile.X, CrisolTile.Y)
                    || Inside(EliteTile.X, EliteTile.Y)
                    || Doors.Exists(door => Inside(door.Preview.X, door.Preview.Y));

                if (!pays) problems.Add($"A sala {name} nao tem nada pra achar.");
            }
        }

        /// <summary>
        /// Nenhuma armadilha: depois de chegar na gruta, de todo chao alcancavel
        /// da pra voltar pra ela. A arena fica de fora (tranca na luta).
        /// </summary>
        private static void CheckNoTraps(List<string> problems)
        {
            LevelMap.JumpRules rules = Rules();
            var reported = new HashSet<string>();

            Play(out _, true, null, (world, reach) =>
            {
                if (!reach.CanStand(CrisolTile.X, CrisolTile.Y)) return;

                foreach (LevelMap.Perch perch in world.Map.FindPerches())
                {
                    int x = -1;
                    for (int cx = perch.X; cx <= perch.Right; cx++)
                    {
                        if (reach.CanStand(cx, perch.Y)) { x = cx; break; }
                    }

                    if (x < 0) continue;

                    string room = RoomAt(x, perch.Y) ?? "fora das salas";
                    if (room == "13_DrillChamber") continue;

                    string key = $"{room}:{perch}";
                    if (reported.Contains(key)) continue;

                    LevelMap.Reach back = world.Map.Explore((x, perch.Y), rules);
                    if (back.CanStand(CrisolTile.X, CrisolTile.Y)) continue;

                    reported.Add(key);
                    problems.Add($"Armadilha em {room}, {perch}: dali nao da pra voltar pra gruta.");
                }
            });
        }

        /// <summary>Os trechos de chao onde nao da pra chegar nem com tudo aberto.</summary>
        public static List<string> UnreachableFloors()
        {
            WorldState world = Play(out LevelMap.Reach reach);
            var floors = new List<string>();

            foreach (LevelMap.Perch perch in world.Map.FindPerches())
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
