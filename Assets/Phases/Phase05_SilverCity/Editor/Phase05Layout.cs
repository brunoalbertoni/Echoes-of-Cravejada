using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O desenho da Phase 5 — The Silver Palace, a ultima: o palacio do Rei de
    /// Prata, feito de Silver 925. Sem inimigos: so o luxo e ele.
    ///
    ///   1 o Corredor Real: o Scrap chega pela porta da Grande Escadaria e
    ///     atravessa um corredor longo e muito luxuoso — colunas de marmore,
    ///     janeloes com a cidade ao luar, quadros, bustos, lustres de cristal,
    ///     tapete vermelho. Nada pra ler: so o luxo. No fim, a bancada (e
    ///     Crisol, que tem a loja e a ultima conversa).
    ///   2 a Sala do Trono: o Rei de Prata, de pe diante do trono.
    ///   3 o Altar do Coracao: atras da sala, a Coluna desce pelo chao ate o
    ///     Coracao de Quartzo; o Scrap entrega o nucleo limpo e a luz desce
    ///     (O Grande Polimento, o final).
    ///
    /// Como os outros layouts, este arquivo nao depende de UnityEngine.
    /// </summary>
    public static class Phase05Layout
    {
        public const int Width = 176;
        public const int Height = 30;

        public const string SceneName = Phase04Layout.NextSceneName;
        public const string FromDistrict4SpawnId = Phase04Layout.ToDistrict5SpawnId;

        public const string BossName = "O Rei de Prata";

        /// <summary>A altura do chao do palacio inteiro.</summary>
        public const int FloorY = 6;

        // ------------------------------------------------------------------
        // Tipos
        // ------------------------------------------------------------------

        public readonly struct Marker
        {
            public readonly string Group;
            public readonly string Name;
            public readonly int X;
            public readonly int Y;

            public Marker(string group, string name, int x, int y)
            {
                Group = group;
                Name = name;
                X = x;
                Y = y;
            }
        }

        /// <summary>Um enfeite do palacio. X e o meio, Y a base (ou o teto, pendurado).</summary>
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

        /// <summary>Uma faixa emendavel (lambri, friso): de X0 a X1, a partir de Y, com a altura em tiles.</summary>
        public readonly struct Strip
        {
            public readonly string Sprite;
            public readonly int X0;
            public readonly int X1;
            public readonly float Y;
            public readonly float Height;
            public readonly int Order;

            public Strip(string sprite, int x0, int x1, float y, float height, int order)
            {
                Sprite = sprite;
                X0 = x0;
                X1 = x1;
                Y = y;
                Height = height;
                Order = order;
            }
        }

        // ------------------------------------------------------------------
        // O que o gerador le
        // ------------------------------------------------------------------

        private static readonly LevelMap Map = new LevelMap(Width, Height);

        public static readonly List<(string Name, int X0, int Y0, int X1, int Y1)> Rooms =
            new List<(string, int, int, int, int)>();

        public static readonly List<Marker> Markers = new List<Marker>();
        public static readonly List<Prop> Props = new List<Prop>();
        public static readonly List<Strip> Strips = new List<Strip>();
        public static readonly List<(string Name, int X, int Y)> Workbenches = new List<(string, int, int)>();
        public static readonly List<(float X, float Y, float Radius, bool Green)> Lamps = new List<(float, float, float, bool)>();

        /// <summary>Os tapetes vermelhos: trecho de chao (x0, x1, y dos pes).</summary>
        public static readonly List<(int X0, int X1, int Y)> Carpets = new List<(int, int, int)>();

        /// <summary>Onde o Scrap aparece: logo depois da porta da Grande Escadaria.</summary>
        public static (int X, int Y) SpawnTile => (7, FloorY);

        /// <summary>A porta da Grande Escadaria, por onde ele chegou (o meio dela).</summary>
        public static (float X, float Y) StairDoor => (4.5f, FloorY);

        public static (int X, int Y) CrisolTile => (93, FloorY);

        public static readonly string[] CrisolLines =
        {
            "Scrap. Você subiu até aqui. Até ele.",
            "Olhe este corredor. Cada coluna, cada moldura... tudo pago com a luz do Coração. A cidade escurecia lá embaixo e aqui dentro nunca faltou vela.",
            "Sabe o que ele tem na cabeça? Uma coroa de quartzo com novecentas e vinte e cinco facetas. Eu lapidei cada uma.",
            "Ele me pediu luz que nunca acabasse. Eu disse que pedra nenhuma dá isso. Ele mandou furar até achar uma que desse.",
            "Atrás do trono fica a Coluna do Coração. Ela desce pela cidade inteira até o Coração de Quartzo, lá no fundo, embaixo das geodes.",
            "Cada distrito que você limpou poliu o seu núcleo. Eu vejo daqui: você está mais limpo do que qualquer prata deste palácio.",
            "Quando ele cair, entregue o seu núcleo à Coluna. A luz dele desce até o Coração e lava o verde de lá de baixo.",
            "Fiquei quieto vezes demais na vida, Scrap. Desta vez eu digo: você consegue. ...Vá. A bancada é sua.",
        };

        // A Sala do Trono.
        public static (int X0, int Y0, int X1, int Y1) ThroneRoom => (100, FloorY, 146, 26);
        public const int ArenaFloorY = FloorY;
        public const float ArenaCeilingY = 26f;

        /// <summary>Onde o Rei espera, sentado no trono (o pivo dele, no chao, no meio do trono).</summary>
        public static (float X, float Y) KingTile => (123.5f, FloorY);

        /// <summary>O trono, no fundo da sala, atras do Rei.</summary>
        public static (float X, float Y) ThroneTile => (123.5f, FloorY);

        /// <summary>Ate onde o Rei anda, em x de mundo.</summary>
        public static (float X0, float X1) KingRange => (102.5f, 144.5f);

        public static (int X, int Y, int Width, int Height) KingWake => (100, FloorY, 47, 20);

        /// <summary>Os espelhos da sala: inteiros na fase 1, quebrados na 2.</summary>
        public static readonly float[] ThroneMirrors = { 105.5f, 141.5f };

        /// <summary>Os cacos de espelho que viram escudo contra o sopro, na fase 2.</summary>
        public static readonly float[] BreathCovers = { 110.5f, 117.5f, 129.5f, 136.5f };

        /// <summary>A grade da entrada da sala (fecha durante a luta) e a do Altar (abre com o Rei caido).</summary>
        public static (int X, int Y, int Height) ArenaSeal => (99, FloorY, 4);
        public static (int X, int Y, int Height) AltarGate => (147, FloorY, 4);

        // O Altar do Coracao.
        public static (int X, int Y) ConduitTile => (160, FloorY);
        public static (int X, int Y) CoreSocketTile => (154, FloorY);

        // ------------------------------------------------------------------
        // Ferramentas
        // ------------------------------------------------------------------

        static Phase05Layout()
        {
            BuildCorridor();
            BuildThroneRoom();
            BuildAltar();
        }

        private static void Carve(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Air);
        private static void Solid(int x0, int y0, int x1, int y1) => Map.Fill(x0, y0, x1, y1, LevelMap.Rock);
        private static void Ledge(int x0, int x1, int y) => Map.Fill(x0, y, x1, y, LevelMap.Platform);
        private static void Room(string name, int x0, int y0, int x1, int y1) => Rooms.Add((name, x0, y0, x1, y1));
        private static void Mark(string group, string name, int x, int y) => Markers.Add(new Marker(group, name, x, y));

        private static void Put(string sprite, float x, float y, int order = 5) => Props.Add(new Prop(sprite, x, y, order, false));
        private static void Hang(string sprite, float x, float ceiling, int order = 4) => Props.Add(new Prop(sprite, x, ceiling, order, true));
        private static void Lamp(float x, float y, float radius, bool green = false) => Lamps.Add((x, y, radius, green));

        public static bool IsRock(int x, int y) => Map.IsRock(x, y);
        public static bool IsSolid(int x, int y) => Map.IsSolid(x, y);
        public static bool IsSpike(int x, int y) => Map.IsSpike(x, y);
        public static bool IsStandable(int x, int y) => Map.IsStandable(x, y);

        /// <summary>O tile de terreno: o mesmo marmore dos Ateliers, que no palacio o gerador pinta de prata.</summary>
        public static string GroundTileName(int x, int y)
        {
            LevelMap.TilePick pick = Map.PickGround(x, y);
            if (pick.Kind == LevelMap.TileKind.None) return null;

            if (pick.Kind == LevelMap.TileKind.Thin) return $"marble_c{4 + pick.Column}_r{BrassArt.ThinRow}";
            if (pick.Kind == LevelMap.TileKind.Edge) return $"marble_c{pick.Column}_r{pick.Row}";

            int roll = LevelMap.Pick(x * 3 + 5, y * 7 + 3, 100);
            int variant = roll < 70 ? 0 : (roll < 85 ? 1 : 2);
            return $"marble_c{variant}_r2";
        }

        public static string BackgroundTileName(int x, int y)
        {
            bool open = false;
            for (int dy = -1; dy <= 1 && !open; dy++)
            {
                for (int dx = -1; dx <= 1 && !open; dx++) open = !Map.IsRock(x + dx, y + dy);
            }

            if (!open) return null;

            int seams = (x % 4 == 3 ? 1 : 0) + (y % 3 == 0 ? 2 : 0);
            return $"marble_c{seams}_r{BrassArt.BackRow}";
        }

        // ------------------------------------------------------------------
        // 1. O Corredor Real
        // ------------------------------------------------------------------

        /// <summary>
        /// Um corredor longo, reto e alto, todo de prata e marmore: a cada
        /// trecho, um par de colunas emoldurando um janelao com a cidade ao
        /// luar; entre elas, quadros, bustos, vasos, arandelas e bancos de
        /// veludo; lustres de cristal no teto, tapete vermelho no chao, o
        /// lambri de nogueira embaixo e o friso dourado em cima. No fim, a
        /// bancada e Crisol, e a porta da Sala do Trono. Nada de interagir no
        /// caminho, alem deles.
        /// </summary>
        private static void BuildCorridor()
        {
            const string room = "01_RoyalCorridor";
            const int x0 = 3;
            const int x1 = 98;
            const int ceiling = 17;
            Room(room, x0, FloorY, x1, ceiling);

            Carve(x0, FloorY, x1, ceiling);

            // A porta da sala do trono.
            Carve(99, FloorY, 99, FloorY + 3);

            Put("palace_stair_door", StairDoor.X, StairDoor.Y, 3);

            // O lambri embaixo e o friso dourado em cima, o corredor inteiro.
            Strips.Add(new Strip("palace_wainscot", x0, x1, FloorY, 2f, 1));
            Strips.Add(new Strip("palace_frieze", x0, x1, ceiling + 1 - 10f / 16f, 10f / 16f, 6));

            // Os trechos: duas colunas e o janelao no meio; os enfeites entre um trecho e outro.
            int section = 0;
            for (int x = 12; x <= 72; x += 12)
            {
                Put("palace_column", x - 3.5f, FloorY, 4);
                Put("palace_column", x + 3.5f, FloorY, 4);
                Put("palace_window", x, FloorY + 3, 2);
                Put("palace_sconce", x - 3.5f, FloorY + 7.5f, 5);
                Put("palace_sconce", x + 3.5f, FloorY + 7.5f, 5);
                Hang("palace_chandelier", x + 6f, ceiling + 1, 4);

                float between = x + 6f;

                switch (section % 4)
                {
                    case 0: Put("palace_bust", between, FloorY, 5); break;
                    case 1: Put("palace_vase", between - 1.2f, FloorY, 5); Put("palace_vase", between + 1.2f, FloorY, 5); break;
                    case 2: Put("palace_bench", between, FloorY, 6); break;
                    default: Put("palace_statue", between, FloorY, 5); break;
                }

                // Um quadro alto entre as colunas de trechos vizinhos.
                if (section % 2 == 0) Put("palace_portrait_" + (section / 2 % 3), between, FloorY + 4.5f, 3);
                else Put("palace_banner", between, FloorY + 5f, 3);

                Lamp(x, FloorY + 6f, 7f);
                section++;
            }

            Hang("palace_chandelier", 6f, ceiling + 1, 4);
            Carpets.Add((x0, x1, FloorY));

            // No fim do corredor: a bancada e Crisol, antes da porta.
            Workbenches.Add(("Workbench_Corridor", 87, FloorY));
            Mark(room, "Workbench_Corridor_Rest", 88, FloorY);
            Mark(room, "Crisol", CrisolTile.X, CrisolTile.Y);

            Put("palace_candelabra", 85f, FloorY, 5);
            Put("palace_candelabra", 97f, FloorY, 5);
            Lamp(90f, FloorY + 5f, 7f);
            Lamp(6f, FloorY + 5f, 6f);
        }

        // ------------------------------------------------------------------
        // 2. A Sala do Trono
        // ------------------------------------------------------------------

        /// <summary>
        /// Alta e larga, de chao reto. O trono no fundo, no meio; o Rei de pe
        /// na frente dele. Duas sacadas baixas e duas altas pra fugir do
        /// cetro e do sopro; os espelhos nos cantos (na fase 2 eles quebram e
        /// os cacos viram escudo).
        /// </summary>
        private static void BuildThroneRoom()
        {
            const string room = "02_ThroneRoom";
            (int x0, int y0, int x1, int y1) = ThroneRoom;
            Room(room, x0, y0, x1, y1);

            Carve(x0, y0, x1, y1);

            // Os cantos de cima chanfrados.
            Solid(x0, 22, x0 + 3, y1);
            Solid(x1 - 3, 22, x1, y1);

            Ledge(105, 109, 9);
            Ledge(138, 142, 9);
            Ledge(112, 116, 13);
            Ledge(131, 135, 13);

            Mark(room, "KingSpawn", (int)KingTile.X, (int)KingTile.Y);

            Strips.Add(new Strip("palace_wainscot", x0, x1, FloorY, 2f, 1));
            Strips.Add(new Strip("palace_frieze", x0 + 4, x1 - 4, y1 + 1 - 10f / 16f, 10f / 16f, 6));

            foreach (float x in new[] { 101.5f, 145.5f }) Put("palace_column", x, FloorY, 3);
            Put("palace_banner", 110f, FloorY + 9f, 2);
            Put("palace_banner", 137f, FloorY + 9f, 2);
            Hang("palace_chandelier", 112f, y1 + 1, 4);
            Hang("palace_chandelier", 135f, y1 + 1, 4);
            Carpets.Add((104, 143, FloorY));

            Lamp(108f, FloorY + 8f, 9f);
            Lamp(139f, FloorY + 8f, 9f);
            Lamp(KingTile.X, FloorY + 12f, 12f);

            // A luz verde da Coluna atras do trono, que so aparece na fase 2.
            Lamp(KingTile.X, FloorY + 4f, 9f, true);
        }

        // ------------------------------------------------------------------
        // 3. O Altar do Coracao
        // ------------------------------------------------------------------

        private static void BuildAltar()
        {
            const string room = "03_HeartAltar";
            Room(room, 148, FloorY, 170, 21);

            Carve(148, FloorY, 170, 21);
            Carve(147, FloorY, 147, FloorY + 3);

            Mark(room, "CoreSocket", CoreSocketTile.X, CoreSocketTile.Y);

            Put("palace_conduit", ConduitTile.X + 0.5f, ConduitTile.Y - 1, 1);
            Put("palace_core_socket", CoreSocketTile.X + 0.5f, CoreSocketTile.Y, 4);
            Put("palace_candelabra", 168f, FloorY, 4);

            Lamp(160f, FloorY + 5f, 9f, true);
            Lamp(152f, FloorY + 4f, 5f);
            Lamp(168f, FloorY + 4f, 5f);
        }

        // ------------------------------------------------------------------
        // Validacao
        // ------------------------------------------------------------------

        private static LevelMap.JumpRules Rules()
        {
            LevelMap.JumpRules rules = LevelMap.JumpRules.Comfortable();
            rules.MaxRise = 4;
            rules.MaxRun += 5;
            rules.WallJump = true;
            return rules;
        }

        private static LevelMap.Reach Explore(bool altarOpen, (int X, int Y)? from = null)
        {
            LevelMap world = Map.Clone();
            if (!altarOpen) world.Fill(AltarGate.X, AltarGate.Y, AltarGate.X, AltarGate.Y + AltarGate.Height - 1, LevelMap.Rock);
            return world.Explore(from ?? SpawnTile, Rules());
        }

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
                problems.Add("O Scrap chega num lugar sem chao.");
                return problems;
            }

            void Need(LevelMap.Reach reach, int x, int y, string what)
            {
                if (!reach.CanStand(x, y)) problems.Add($"Nao da pra chegar em {what} ({x}, {y}).");
            }

            LevelMap.Reach before = Explore(false);
            Need(before, CrisolTile.X, CrisolTile.Y, "Crisol");
            Need(before, 88, FloorY, "a bancada");
            Need(before, (int)KingTile.X, ArenaFloorY, "a Sala do Trono");
            Need(before, 107, 10, "a sacada baixa da esquerda");
            Need(before, 140, 10, "a sacada baixa da direita");
            Need(before, 114, 14, "a sacada alta da esquerda");
            Need(before, 133, 14, "a sacada alta da direita");
            if (before.CanStand(CoreSocketTile.X, CoreSocketTile.Y)) problems.Add("Da pra chegar no Altar antes do Rei cair.");

            LevelMap.Reach after = Explore(true);
            Need(after, CoreSocketTile.X, CoreSocketTile.Y, "o encaixe do nucleo");

            foreach (Marker marker in Markers)
            {
                if (!Map.IsStandable(marker.X, marker.Y)) problems.Add($"{marker.Group}/{marker.Name} ({marker.X}, {marker.Y}) nao e lugar de ficar em pe.");
                else if (!after.CanStand(marker.X, marker.Y)) problems.Add($"{marker.Group}/{marker.Name} fora de alcance.");
            }

            return problems;
        }

        public static string[] ToRows() => Map.ToRows();
    }
}
