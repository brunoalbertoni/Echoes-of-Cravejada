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
    /// (subir no escuro sem quebrar as laminas) e ao 11 Campanario, o Salao
    /// das Estatuas: apertadas na ordem da historia das gravuras, elas abrem a
    /// porta da 12 antessala da broca. E o enigma principal do distrito.
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
    public static partial class Phase03Layout
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
        /// Algo pra ler com E. Kind diz o desenho: "board" a tabua do turno,
        /// "crownsign" a placa da Coroa, "silverplaque" a placa de prata,
        /// "note" um bilhete preso na rocha, "helmet" o capacete, "page" uma
        /// pagina, "engraving0".."engraving3" as gravuras, "edge" nada (so a
        /// beirada), "miner" um mineiro Tarnished e "crisol" o proprio Crisol —
        /// esses dois so nas listas do distrito purificado.
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

            /// <summary>
            /// Porta que nao entrega a ordem: nao toca a previa e as gemas nao
            /// tem a cor das notas. A ordem vem de outro lugar (uma placa).
            /// </summary>
            public bool Hidden;
        }

        /// <summary>
        /// Uma estatua do Salao das Estatuas. Kind e o desenho (0 o Mineiro,
        /// 1 o Rei, 2 a Broca, 3 o Coracao); Order, a vez dela na sequencia —
        /// a mesma das gravuras da galeria. X e Y sao onde o Scrap fica pra
        /// apertar o botao.
        /// </summary>
        public sealed class StatueSpot
        {
            public string Name;
            public int Kind;
            public int Order;
            public int X;
            public int Y;
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

        /// <summary>As estatuas do enigma principal.</summary>
        public static readonly List<StatueSpot> Statues = new List<StatueSpot>();

        /// <summary>As rachaduras de onde nascem os mites quando uma estatua e acionada: a celula do chao na frente delas.</summary>
        public static readonly List<(int X, int Y)> SpawnCracks = new List<(int, int)>();

        /// <summary>O portao que as estatuas abrem.</summary>
        public const string StatueGate = "Porta_Campanario";

        /// <summary>A grade trancada na entrada do Salao das Estatuas. So a CHAVE ANTIGA abre.</summary>
        public const string StatueEntranceGate = "Porta_Salao";

        /// <summary>A chave que o Capataz solta ao morrer (estava na argola do cinto dele).</summary>
        public const string AncientKeyId = "D3_AncientKey";

        /// <summary>O nome do mini boss: o Capataz, o primeiro mineiro a descer, cego, com a safira no capacete.</summary>
        public const string EliteName = "O Capataz";

        /// <summary>As pedrinhas soltas caidas do poco, no chao da estacao (so enfeite).</summary>
        public static readonly List<(float X, float Y)> Pebbles = new List<(float, float)>();

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

        /// <summary>O enigma das estatuas, no Campanario: e ele que abre o caminho pra broca.</summary>
        public const string MainDoor = "Statues_Belfry";

        /// <summary>Onde o Scrap aparece: dentro da cabine do elevador que veio do District 2.</summary>
        public static (int X, int Y) SpawnTile => (8, 11);

        /// <summary>A cabine do elevador: canto de baixo, a esquerda, e largura (3x3 tiles).</summary>
        public static (int X, int Y, int Width) ElevatorCar => (6, 11, 3);

        /// <summary>
        /// Ate onde a cabine sobe no poco antes de a tela escurecer e o
        /// District 4 carregar: o alto do mapa. O poco corre colado na parede
        /// oeste, por tras da ala oeste, sem atravessar sala nenhuma.
        /// </summary>
        public const float ElevatorTopY = 116f;

        /// <summary>O fundo do poco embaixo da cabine, de onde ela parte na chegada.</summary>
        public const int ElevatorShaftBottom = 1;

        /// <summary>
        /// Os escombros que entopem o poco logo acima da cabine. Pro mapa sao
        /// rocha ate a broca cair (como as barreiras); quem os desenha e os
        /// quebra e o objeto deles, com o Resonance Strike.
        /// </summary>
        public static (int X, int Y, int Width, int Height) ElevatorRubble => (5, 14, 5, 8);

        /// <summary>O que aparece ao tentar subir com os escombros no lugar.</summary>
        public const string ElevatorBlockedNotice = "";

        public static (int X, int Y) CrisolTile => (140, 44);

        public static readonly string[] CrisolLines =
        {
            "Scrap. Você chegou até aqui. Admito que não apostei nisso.",
            "Olha em volta. Ametista, quartzo leitoso, cristal fumê... Isso aqui é o meu ofício. Aqui eu sei o nome de tudo.",
            "Reparou que eles só acendem com barulho? Era assim que os mineiros enxergavam: batiam na pedra e andavam enquanto ela brilhava.",
            "Eu trabalhei pros 925 Nobres. Eu tava lá quando decidiram furar até o Coração de Quartzo. Eu VI a escavação.",
            "Eu disse que era um erro. Disse na frente de todo mundo. Sabe o que fazem com prata que fala demais? Riscam.",
            "É por isso que eu sou todo arranhado, Scrap. Não foi o tempo. Foi de propósito. Me riscaram e me jogaram no Poço de Sucata.",
            "O elevador que te trouxe? Ele continuava subindo, até o Laboratório. Mas o poço desabou no dia em que a broca furou fundo demais.",
            "Os escombros entupiram tudo. Pedra assim não cede a cinzel. Cede a uma nota, a nota certa.",
            "E quem ainda canta essa nota é aquela máquina, lá em cima, na câmara de perfuração.",
            "...Chega. O seu polimento tá decente, aliás. Não conta pra ninguém que eu disse isso.",
        };

        // O Capataz: espera ajoelhado no fundo leste da toca e, acordado, anda
        // por ela inteira. EliteTile e onde o Scrap chega pra luta; EliteRange,
        // o trecho de chao onde caem os estilhacos e nascem os bichos;
        // EliteWake, a area que o acorda (e onde ele escuta).
        public static (int X, int Y) EliteTile => (182, 60);
        public static (int X0, int X1) EliteRange => (171, 202);
        public static (int X, int Y, int Width, int Height) EliteWake => (171, 60, 31, 14);

        /// <summary>A porta lateral, entre a saleta da chamine e a toca: fecha enquanto o Capataz luta.</summary>
        public static (int X, int Y, int Width, int Height) EliteSeal => (170, 60, 1, 4);

        /// <summary>Onde ele espera, ajoelhado (os pes), no fundo leste da toca.</summary>
        public static (float X, float Y) ForemanHome => (197f, 60f);

        /// <summary>
        /// Ate onde ele anda, em x de mundo: a toca inteira, da porta lateral
        /// (a oeste) ate rente a parede leste.
        /// </summary>
        public static (float X0, float X1) ForemanRange => (171.2f, 202.2f);

        /// <summary>O teto da toca: de onde caem os estilhacos quando ele grita ou bate na parede.</summary>
        public const float MonsterCeiling = 74.5f;

        /// <summary>Onde os bichos brotam do chao quando ele apita.</summary>
        public static readonly int[] MonsterBroods = { 172, 180, 188, 196 };

        // The Geode Drill: onde aparece, ate onde corre, a area que a acorda e
        // a altura do chao de cada uma das tres camadas da arena.
        public static (int X, int Y) BossTile => (55, 92);
        public static (int X0, int X1) BossRange => (34, 76);
        public static (int X, int Y, int Width, int Height) BossWake => (42, 92, 28, 14);
        public static readonly int[] BossFloors = { 92, 80, 68 };

        /// <summary>A grade que fecha a entrada leste da Camara de Perfuracao enquanto a broca luta.</summary>
        public static (int X, int Y, int Width, int Height) BossSeal => (77, 92, 1, 5);

        /// <summary>A placa gravada no chassi da broca, que so aparece quando ela para.</summary>
        public static readonly string[] DrillPlaqueLines =
        {
            "Por baixo da crosta de Óxido há uma placa de prata, rebitada no chassi.",
            "\"POR ORDEM DE SUA MAJESTADE, O REI DE PRATA.\"",
            "Embaixo, a data da primeira perfuração. E mais nada. Nenhum nome de mineiro.",
            "Então foi ele. Não foi acidente nem praga: foi uma ordem.",
        };

        /// <summary>O brilho verde no fundo da fenda da Galeria das Gravuras.</summary>
        public static (float X, float Y) HeartGlow => (164f, 4f);

        // ------------------------------------------------------------------
        // A Fenda
        // ------------------------------------------------------------------

        /// <summary>
        /// A Fenda: um abismo gigantesco no canto de baixo, a leste, embaixo da
        /// gruta e das galerias. Abre como um sino — estreita em cima, larga
        /// la embaixo — e o fundo e espinho e o verde do Coracao de Quartzo.
        /// Chega-se a ela por um tunel que sai do Poco Fragil, ate um mirante
        /// com uma placa. Nao ha como descer.
        /// </summary>
        public const int RiftBottom = 2;
        public const int RiftTop = 41;

        /// <summary>A borda oeste da fenda na linha y: abre pra oeste conforme desce.</summary>
        public static int RiftLeft(int y) => 122 + (y - RiftBottom) * 16 / (RiftTop - RiftBottom);

        /// <summary>A borda leste: abre pra leste conforme desce.</summary>
        public static int RiftRight(int y) => 206 - (y - RiftBottom) * 10 / (RiftTop - RiftBottom);

        /// <summary>Se a celula e o vazio da fenda: sem parede de fundo, pra parecer sem fim.</summary>
        public static bool IsRift(int x, int y) => y >= RiftBottom && y <= RiftTop && x >= RiftLeft(y) && x <= RiftRight(y);

        public const string RiftSignText = "Para trás, espaço reservado para mineração privada";

        /// <summary>
        /// O que cada sala tem largado no chao, pela historia dela. O tunel e
        /// as galerias tem ferramenta e pedregulho de escavacao; a Fenda (a
        /// mineracao privada da Coroa) tem geodo cortado e caixote com o selo;
        /// a gruta do Crisol, os sacos que nunca subiram. Fora da lista ficam o
        /// Poco Fragil e o Poco do Bilhete (so descida), o Campanario (o templo)
        /// e a Camara de Perfuracao (a arena).
        /// </summary>
        private static readonly Dictionary<string, string[]> Clutter = new Dictionary<string, string[]>
        {
            ["01_Arrival"] = new[] { "decor_crate", "decor_sack" },
            ["02_DarkTunnel"] = new[] { MineDressArt.Pickaxe, MineDressArt.Rubble, MineDressArt.Lantern, MineDressArt.Stalagmite },
            ["03_FirstHunter"] = new[] { MineDressArt.Rubble, MineDressArt.Pickaxe, MineDressArt.OreSack },
            ["04b_Rift"] = new[] { MineDressArt.CutGeode, MineDressArt.OreSack, "decor_crate", MineDressArt.Rubble },
            ["05_Grotto"] = new[] { MineDressArt.OreSack, MineDressArt.Lantern, "decor_crate" },
            ["06_EngravingGallery"] = new[] { MineDressArt.Stalagmite, MineDressArt.Rubble, MineDressArt.Lantern },
            ["07_EchoChamber"] = new[] { MineDressArt.Stalagmite, MineDressArt.CutGeode },
            ["07b_Ruins"] = new[] { MineDressArt.Rubble, MineDressArt.Stalagmite },
            ["08_HunterGallery"] = new[] { MineDressArt.Pickaxe, MineDressArt.Lantern, MineDressArt.OreSack },
            ["09_ResonantChamber"] = new[] { MineDressArt.Stalagmite, MineDressArt.CutGeode },
            ["10_FragileVault"] = new[] { MineDressArt.Rubble },
            ["10b_EastLoop"] = new[] { MineDressArt.Pickaxe, MineDressArt.OreSack },
            ["12_PreBoss"] = new[] { "decor_crate", "decor_toolbox", MineDressArt.OreSack },
        };
        // A chegada tambem: e de proposito um lugar que ninguem mais habita.
        private static readonly string[] PassageRooms =
            { "01_Arrival", "02_DarkTunnel", "04_FragileShaft", "06_EngravingGallery", "07_EchoChamber", "13_DrillChamber" };
    }
}
