using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase01Layout
    {
        // ------------------------------------------------------------------
        // Janelas na parede do fundo
        // ------------------------------------------------------------------

        /// <summary>
        /// Um pedaco pronto de parede de caverna com um buraco no meio, por onde
        /// se ve o que esta la atras: as silhuetas dos geodos e a nevoa.
        /// </summary>
        public readonly struct Window
        {
            /// <summary>Nome do sprite, fatiado pelo Phase01Assets.</summary>
            public readonly string Sprite;

            /// <summary>Canto de baixo, a esquerda.</summary>
            public readonly int X;
            public readonly int Y;

            public readonly int Width;
            public readonly int Height;

            /// <summary>
            /// Multiplica a cor do pedaco. Os pedacos grandes sao de cor chapada
            /// e saem um tom mais claros que os tiles da parede em volta; isso
            /// disfarca a emenda.
            /// </summary>
            public readonly float Shade;

            public Window(string sprite, int x, int y, int width, int height, float shade = 1f)
            {
                Sprite = sprite;
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Shade = shade;
            }

            public bool Contains(int x, int y)
            {
                return x >= X && x < X + Width && y >= Y && y < Y + Height;
            }
        }



        public static bool IsWindow(int x, int y)
        {
            foreach (Window window in Windows)
            {
                if (window.Contains(x, y)) return true;
            }

            return false;
        }

        // ------------------------------------------------------------------
        // Sucata, minerio e mato
        // ------------------------------------------------------------------

        public enum PropLayer
        {
            /// <summary>Atras da rocha e esmaecido: faz parte do fundo.</summary>
            Back,

            /// <summary>Na frente da rocha, atras do Scrap. E onde mora a sucata.</summary>
            Middle,

            /// <summary>Na frente do Scrap, quase em silhueta.</summary>
            Front,
        }

        /// <summary>Um sprite de 1 tile carimbado numa celula.</summary>
        public readonly struct Prop
        {
            public readonly string Tile;
            public readonly int X;
            public readonly int Y;
            public readonly PropLayer Layer;

            /// <summary>
            /// Se precisa de chao embaixo. Falso pra o que esta empilhado em
            /// cima de outra coisa ou incrustado na rocha.
            /// </summary>
            public readonly bool Grounded;

            public Prop(string tile, int x, int y, PropLayer layer, bool grounded)
            {
                Tile = tile;
                X = x;
                Y = y;
                Layer = layer;
                Grounded = grounded;
            }
        }

        // objects_misc.png: linha 2 sao pedacos de cerca (0-2). Os caixotes da
        // linha 0 sairam: no meio da sucata eles destoavam.
        private static string Fence(int variant) => $"misc_c{variant}_r2";

        // bg_objects.png: linha 0 pedras escuras, 1 pedras cinza, 2 mato seco
        // escuro, 3 mato seco cinza. Em todas, as colunas vao do maior pro menor.
        private static string DarkRock(int variant) => $"bgobj_c{variant}_r0";
        private static string GreyRock(int variant) => $"bgobj_c{variant}_r1";
        private static string DarkWeed(int variant) => $"bgobj_c{variant}_r2";
        private static string GreyWeed(int variant) => $"bgobj_c{variant}_r3";

        /// <summary>
        /// Os geodos: o losango grande de minerio, 2x2 tiles, com o canto de
        /// baixo a esquerda em (X, Y). So moram dentro da rocha, com pelo menos
        /// um tile de rocha em volta (o Validate cobra): na beirada ou no ar
        /// eles parecem pedras soltas.
        /// </summary>
        public static readonly (int Band, int X, int Y)[] Geodes =
        {
            // O teto dos andares de cima.
            (BlueBand, 20, 57),
            (BlueBand, 58, 57),
            (RustBand, 60, 44),
            (GreyBand, 70, 46),
            (RustBand, 11, 47),

            // As paredes e o chao do poco.
            (BlueBand, 41, 17),
            (BlueBand, 7, 27),
            (BlueBand, 30, 31),
            (GreyBand, 1, 15),
            (RustBand, 20, 1),
            (RustBand, 1, 31),
            (BlueBand, 9, 1),

            // O macico de rocha em volta da arena, da obra e do abismo.
            (BlueBand, 43, 29),
            (GreyBand, 55, 16),
            (RustBand, 66, 3),
            (RustBand, 76, 8),
            (BlueBand, 81, 28),
            (RustBand, 90, 12),
            (BlueBand, 104, 30),
            (GreyBand, 104, 18),
        };


        private static Prop[] BuildProps()
        {
            var props = new List<Prop>();

            void Ground(string tile, int x, int y, PropLayer layer = PropLayer.Middle)
            {
                props.Add(new Prop(tile, x, y, layer, true));
            }

            // --- O monte de sucata onde o Scrap acorda ---
            Ground(GreyRock(4), 16, 5);
            Ground(GreyRock(2), 18, 6);
            Ground(GreyRock(1), 25, 6);
            Ground(GreyRock(3), 27, 5);

            // --- Chao, lado oeste ---
            Ground(Fence(0), 6, 5);
            Ground(Fence(1), 7, 5);
            Ground(GreyRock(0), 11, 4);
            Ground(GreyRock(2), 12, 4);
            Ground(GreyWeed(0), 9, 4, PropLayer.Back);
            Ground(DarkWeed(1), 5, 5, PropLayer.Front);
            Ground(DarkRock(1), 10, 4, PropLayer.Front);
            Ground(DarkWeed(2), 15, 4, PropLayer.Front);

            // --- Chao, lado leste ---
            Ground(Fence(2), 33, 4);
            Ground(GreyRock(1), 34, 4);
            Ground(DarkRock(2), 28, 4, PropLayer.Front);
            Ground(DarkWeed(0), 32, 4, PropLayer.Front);

            // --- Degrau da parede leste ---
            Ground(GreyWeed(3), 36, 6, PropLayer.Back);
            Ground(GreyRock(4), 39, 6);

            // --- Saliencia da parede oeste, na boca do tunel ---
            Ground(GreyRock(3), 9, 14);
            Ground(DarkWeed(3), 4, 14, PropLayer.Front);

            // --- Patamar do topo. A cerca fica na beirada do vao, e nao do
            //     lado do corredor, senao parece que a passagem esta fechada ---
            Ground(Fence(0), 33, 22);
            Ground(Fence(1), 34, 22);
            Ground(GreyWeed(1), 35, 22, PropLayer.Back);
            Ground(DarkWeed(4), 38, 22, PropLayer.Front);

            // --- Andar 1 ---
            Ground(GreyRock(2), 31, 40);
            Ground(DarkWeed(1), 23, 40, PropLayer.Front);

            // --- Andar 2 ---
            Ground(GreyRock(1), 10, 50);
            Ground(GreyRock(3), 34, 52);
            Ground(Fence(0), 46, 50);
            Ground(Fence(1), 47, 50);
            Ground(DarkRock(1), 38, 50, PropLayer.Front);

            // --- A Triagem: as pecas que o Coletor separou e nunca subiram ---
            Ground(GreyRock(3), 57, 50);
            Ground(DarkWeed(2), 67, 50, PropLayer.Front);

            // --- A Antessala ---
            Ground(Fence(0), 61, 37);
            Ground(Fence(1), 62, 37);
            Ground(GreyRock(1), 64, 37);
            Ground(GreyWeed(2), 76, 37, PropLayer.Back);
            Ground(DarkRock(2), 88, 37, PropLayer.Front);

            // --- O abismo ---
            Ground(GreyRock(3), 82, 22);
            Ground(Fence(0), 95, 22);
            Ground(Fence(1), 96, 22);
            Ground(DarkWeed(2), 94, 22, PropLayer.Front);

            // --- Arena: o degrau da saida. Os patamares de cada lado do fosso
            //     sairam: de cima deles dava pra bater no Coletor sem ele alcancar ---
            Ground(Fence(1), 79, 22);
            Ground(Fence(2), 80, 22);

            // --- Arena: o fosso. So mato baixo, atras, longe das beiradas das
            //     plataformas; nada na frente do Scrap nem colado embaixo delas,
            //     pra luta ficar legivel ---
            Ground(GreyWeed(1), 56, 20, PropLayer.Back);
            Ground(GreyWeed(3), 69, 20, PropLayer.Back);

            // --- Mato nas plataformas, so pra elas nao ficarem peladas ---
            Ground(GreyWeed(4), 30, 8, PropLayer.Back);
            Ground(GreyWeed(4), 13, 12, PropLayer.Back);
            Ground(GreyWeed(2), 22, 18, PropLayer.Back);

            // --- Os geodos, incrustados na rocha. O losango ocupa 2x2 tiles:
            //     colunas 1-2, linhas 2-3 da banda de cor (a 2 e a metade de cima) ---
            foreach ((int band, int x, int y) in Geodes)
            {
                props.Add(new Prop(TileName(1, band + 2), x, y + 1, PropLayer.Middle, false));
                props.Add(new Prop(TileName(2, band + 2), x + 1, y + 1, PropLayer.Middle, false));
                props.Add(new Prop(TileName(1, band + 3), x, y, PropLayer.Middle, false));
                props.Add(new Prop(TileName(2, band + 3), x + 1, y, PropLayer.Middle, false));
            }

            return props.ToArray();
        }

        // ------------------------------------------------------------------
        // Luz, nevoa e poeira
        // ------------------------------------------------------------------

        /// <summary>
        /// A luz em volta de onde o Scrap acorda. E a unica quente do poco: ele
        /// tem que ser o ponto mais vivo da tela.
        /// </summary>
        public static (float X, float Y) WakeGlowPosition => (21.5f, 7.5f);

        /// <summary>
        /// A luz fria que desce pela chamine. Fica bem em cima do patamar do
        /// topo, entao mostra de longe pra onde e a subida — e a saida.
        /// </summary>
        public static (float X, float Y) ChimneyLightPosition => (36f, 28.5f);

        /// <summary>
        /// Um resto de brasa cor de ferrugem na boca do tunel da arena. E a
        /// unica luz dessa cor no mapa: algo mora ali.
        /// </summary>
        public static (float X, float Y) LairGlowPosition => (62.5f, 21.5f);

        /// <summary>Onde o Oxide brilha verde-cobre: no chao e atras das janelas.</summary>
        public static readonly (float X, float Y, float Radius)[] OxideGlows =
        {
            // Poco
            (8f, 6f, 9f),
            (34f, 9f, 8f),
            (21f, 24f, 9f),
            (7f, 16f, 5f),

            // Arena: um em cada patamar e um atras da janela grande. O meio do
            // fosso fica de proposito mais escuro que as beiradas.
            (50f, 24f, 7f),
            (76f, 24f, 7f),
            (63f, 29f, 9f),

            // Os dois andares de cima e os pocos que os ligam.
            (35.5f, 34f, 7f),
            (27f, 43f, 8f),
            (18f, 43f, 8f),
            (6f, 49f, 6f),
            (18f, 53f, 9f),
            (30f, 53f, 8f),
            (42f, 52f, 8f),
            (50f, 40f, 8f),

            // A Triagem e a Antessala. O canto da bancada fica de fora: e a
            // luz quente do lampiao que tem que mandar ali.
            (62f, 52f, 7f),
            (74f, 52f, 8f),
            (88f, 52f, 7f),
            (92f, 40f, 7f),
            (78f, 39f, 8f),

            // A obra abandonada: nao se ve antes da purificacao, e depois
            // viram os lampioes que a iluminam.
            (52f, 9f, 8f),
            (65f, 11f, 7f),
            (79f, 15f, 7f),

            // O abismo: o verde sobe do fundo, por entre os espinhos.
            (90f, 18f, 9f),
            (84f, 25f, 6f),
            (98f, 26f, 7f),
        };

        /// <summary>Lampioes: a luz quente e parada das salas seguras. So as bancadas tem.</summary>
        public static readonly (float X, float Y, float Radius)[] LampGlows =
        {
            (6.5f, 43f, 7f),
            (58.5f, 40f, 7f),
        };

        /// <summary>Uma faixa deitada de nevoa do Oxide.</summary>
        public readonly struct FogBank
        {
            public readonly string Name;

            /// <summary>Centro da faixa.</summary>
            public readonly float X;
            public readonly float Y;

            public readonly float Width;
            public readonly float Thickness;

            /// <summary>Opacidade de cada borrao. Acima de 0.2 ja tapa o cenario.</summary>
            public readonly float Alpha;

            /// <summary>Borroes novos por segundo: e o que da a densidade.</summary>
            public readonly float Rate;

            /// <summary>Se passa na frente do Scrap, em vez de atras do terreno.</summary>
            public readonly bool InFront;

            public FogBank(string name, float x, float y, float width, float thickness,
                float alpha, float rate, bool inFront)
            {
                Name = name;
                X = x;
                Y = y;
                Width = width;
                Thickness = thickness;
                Alpha = alpha;
                Rate = rate;
                InFront = inFront;
            }
        }


        /// <summary>
        /// Onde cai poeira de metal: o centro e a largura da faixa do teto de
        /// onde ela sai, a altura dessa faixa, quantos tiles ela cai ate o chao
        /// e quantos graos nascem por segundo.
        /// </summary>
        public static readonly (string Name, float X, float Width, float Top, float Fall, float Rate)[] DustFalls =
        {
            ("MetalDust_Pit", 22f, 44f, 29f, 25f, 14f),
            ("MetalDust_Arena", 63f, 30f, 33f, 13f, 10f),

            // Um fio de poeira caindo da boca da Calha N.º 1 em cima do monte onde o Scrap acorda.
            ("MetalDust_Chute", 21.5f, 1.6f, 25.4f, 19f, 6f),
        };
    }
}
