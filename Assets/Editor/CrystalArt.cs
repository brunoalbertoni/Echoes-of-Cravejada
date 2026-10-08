using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte do District 3, desenhada por codigo: cristais de varios tamanhos
    /// e cores, o cristal ressonante e o veio de Oxide dele, a plataforma
    /// fragil, as gravuras da escavacao e os tres inimigos do escuro.
    ///
    /// Como o resto da arte gerada, sao placeholders de 16 px por tile e nao
    /// dependem de UnityEngine.
    /// </summary>
    public static class CrystalArt
    {
        private static readonly Rgba Outline = new Rgba(24, 18, 34);

        // Tres familias de cristal: ametista, quartzo leitoso e cristal fume.
        // Em cada uma: sombra, corpo, luz e brilho.
        private static readonly Rgba[][] Hues =
        {
            new[] { new Rgba(70, 40, 120), new Rgba(128, 82, 190), new Rgba(186, 142, 236), new Rgba(236, 216, 255) },
            new[] { new Rgba(130, 138, 168), new Rgba(190, 198, 224), new Rgba(228, 234, 248), new Rgba(255, 255, 255) },
            new[] { new Rgba(52, 50, 66), new Rgba(96, 92, 114), new Rgba(146, 140, 164), new Rgba(210, 204, 226) },
        };

        public const int HueCount = 3;
        public const int SizeCount = 3;

        private static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                return (h ^ (h >> 16)) & 0x7fffffff;
            }
        }

        /// <summary>
        /// Um caco de cristal em pe: um prisma que afina na ponta, com a face
        /// da esquerda clara e a da direita na sombra.
        /// </summary>
        private static void Shard(PixelCanvas canvas, int baseX, int baseY, int halfWidth, int height, int lean, Rgba[] hue)
        {
            for (int i = 0; i < height; i++)
            {
                float t = i / (float)Math.Max(1, height - 1);

                // Reto ate dois tercos, depois fecha em ponta.
                float taper = t < 0.65f ? 1f : 1f - (t - 0.65f) / 0.35f;
                int half = Math.Max(0, (int)Math.Round(halfWidth * taper));
                int center = baseX + (int)Math.Round(lean * t);
                int y = baseY - i;

                for (int dx = -half; dx <= half; dx++)
                {
                    Rgba color = dx < 0 ? hue[2] : (dx == 0 ? hue[3] : hue[1]);
                    if (dx == half && half > 0) color = hue[0];
                    canvas.Set(center + dx, y, color);
                }
            }
        }

        private static void AddOutline(PixelCanvas canvas)
        {
            int width = canvas.Width;
            int height = canvas.Height;
            var filled = new bool[width * height];
            for (int i = 0; i < filled.Length; i++) filled[i] = canvas.Pixels[i].A != 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (filled[x + y * width]) continue;

                    bool touches = (x > 0 && filled[x - 1 + y * width])
                        || (x < width - 1 && filled[x + 1 + y * width])
                        || (y > 0 && filled[x + (y - 1) * width])
                        || (y < height - 1 && filled[x + (y + 1) * width]);

                    if (touches) canvas.Set(x, y, Outline);
                }
            }
        }

        /// <summary>
        /// Um tufo de cristais brotando do chao. Tamanho 0 e 16x16, 1 e 16x24,
        /// 2 e 32x32. A base fica na ultima linha.
        /// </summary>
        public static PixelCanvas Cluster(int size, int hueIndex)
        {
            Rgba[] hue = Hues[hueIndex % HueCount];

            int width = size == 2 ? 32 : 16;
            int height = size == 0 ? 16 : (size == 1 ? 24 : 32);
            var canvas = new PixelCanvas(width, height);

            int count = size == 2 ? 6 : 3 + size;
            int seed = size * 10 + hueIndex;

            for (int i = 0; i < count; i++)
            {
                int x = 3 + (width - 6) * i / Math.Max(1, count - 1);
                int tall = height / 3 + Hash(i, 1, seed) % (height * 2 / 3 - 1);
                int half = 1 + Hash(i, 2, seed) % (size == 2 ? 3 : 2);
                int lean = Hash(i, 3, seed) % 5 - 2;

                Shard(canvas, x, height - 1, half, tall, lean, hue);
            }

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>
        /// Um cristal ressonante, 16x32: um prisma so, grande e limpo, em tons
        /// de cinza claro — a cor da nota entra por cima, no jogo.
        /// </summary>
        public static PixelCanvas Resonant()
        {
            var canvas = new PixelCanvas(16, 32);

            var tone = new[] { new Rgba(120, 120, 130), new Rgba(190, 190, 200), new Rgba(232, 232, 240), new Rgba(255, 255, 255) };
            Shard(canvas, 8, 31, 5, 30, 0, tone);

            // Uma base de pedra, pra ele nao parecer solto.
            canvas.Fill(2, 29, 12, 3, new Rgba(60, 56, 70));
            canvas.Fill(2, 29, 12, 1, new Rgba(92, 88, 104));

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>O veio de Oxide de um cristal corrompido, 16x32: uma raiz verde subindo por dentro.</summary>
        public static PixelCanvas Vein()
        {
            var canvas = new PixelCanvas(16, 32);

            var green = new Rgba(90, 220, 140);
            var dark = new Rgba(40, 120, 84);

            int x = 8;
            for (int y = 28; y > 8; y--)
            {
                canvas.Set(x, y, green);
                canvas.Set(x + 1, y, dark);

                int roll = Hash(y, 1, 77) % 5;
                if (roll == 0) x--;
                else if (roll == 1) x++;
                x = Math.Max(5, Math.Min(10, x));

                // Um ramo de vez em quando.
                if (y % 6 == 0)
                {
                    canvas.Set(x - 2, y - 1, dark);
                    canvas.Set(x + 3, y - 2, green);
                }
            }

            return canvas;
        }

        /// <summary>Um trecho de plataforma fragil, 16x8, pra repetir: uma lamina fina e quase transparente.</summary>
        public static PixelCanvas FragileTile()
        {
            var canvas = new PixelCanvas(16, 8);

            for (int x = 0; x < 16; x++)
            {
                canvas.Set(x, 0, new Rgba(240, 232, 255, 230));
                canvas.Set(x, 1, new Rgba(196, 170, 240, 190));
                canvas.Set(x, 2, new Rgba(150, 120, 210, 150));

                // Pontas irregulares embaixo, que fecham em 16 px.
                int depth = 3 + (x * 5 % 7 == 0 ? 2 : (x % 4 == 1 ? 1 : 0));
                for (int y = 3; y < depth; y++) canvas.Set(x, y, new Rgba(120, 96, 190, 120));
            }

            // Trincas de nascenca.
            canvas.Set(5, 1, new Rgba(255, 255, 255));
            canvas.Set(6, 2, new Rgba(255, 255, 255, 200));
            canvas.Set(11, 1, new Rgba(255, 255, 255, 200));

            return canvas;
        }

        /// <summary>Uma gema de marcacao, 8x8, branca: a cor da nota entra no jogo.</summary>
        public static PixelCanvas Glyph()
        {
            var canvas = new PixelCanvas(8, 8);

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    float distance = Math.Abs(x + 0.5f - 4f) + Math.Abs(y + 0.5f - 4f);
                    if (distance > 3.6f) continue;

                    canvas.Set(x, y, distance < 1.6f ? new Rgba(255, 255, 255) : new Rgba(214, 214, 224));
                }
            }

            AddOutline(canvas);
            return canvas;
        }

        public const int EngravingCount = 4;

        /// <summary>
        /// Uma gravura da antiga escavacao, 64x40, riscada na pedra. Quatro
        /// cenas, na ordem da historia: os mineiros, o Rei de Prata apontando
        /// pra baixo, a broca descendo, e o coracao rachado.
        /// </summary>
        public static PixelCanvas Engraving(int scene)
        {
            var canvas = new PixelCanvas(64, 40);

            var stone = new Rgba(58, 52, 74);
            var stoneLight = new Rgba(78, 70, 98);
            var stoneDark = new Rgba(40, 36, 54);
            var cut = new Rgba(176, 164, 206);

            canvas.Fill(0, 0, 64, 40, stone);
            canvas.Fill(0, 0, 64, 1, stoneLight);
            canvas.Fill(0, 0, 1, 40, stoneLight);
            canvas.Fill(0, 39, 64, 1, stoneDark);
            canvas.Fill(63, 0, 1, 40, stoneDark);

            for (int y = 2; y < 38; y++)
            {
                for (int x = 2; x < 62; x++)
                {
                    if (Hash(x, y, 90 + scene) % 19 == 0) canvas.Set(x, y, stoneDark);
                }
            }

            // Moldura gotica: um arco pontudo.
            for (int x = 6; x < 58; x++)
            {
                int arch = 5 + Math.Abs(x - 32) / 3;
                canvas.Set(x, arch, cut);
            }

            canvas.Fill(6, 13, 1, 22, cut);
            canvas.Fill(57, 13, 1, 22, cut);
            canvas.Fill(6, 35, 52, 1, cut);

            // Um boneco de palito: cabeca, tronco, pernas.
            void Figure(int x, int y, bool crown)
            {
                canvas.Fill(x - 1, y, 3, 3, cut);
                canvas.Fill(x, y + 3, 1, 6, cut);
                canvas.Set(x - 1, y + 10, cut);
                canvas.Set(x + 1, y + 10, cut);
                canvas.Set(x - 1, y + 9, cut);
                canvas.Set(x + 1, y + 9, cut);

                if (!crown) return;

                canvas.Set(x - 2, y - 2, cut);
                canvas.Set(x, y - 2, cut);
                canvas.Set(x + 2, y - 2, cut);
                canvas.Fill(x - 2, y - 1, 5, 1, cut);
            }

            switch (scene % EngravingCount)
            {
                case 0:
                    // Os mineiros, em fila, com picaretas.
                    for (int i = 0; i < 4; i++)
                    {
                        int x = 16 + i * 10;
                        Figure(x, 20, false);
                        canvas.Fill(x + 1, 22, 4, 1, cut);
                        canvas.Fill(x + 4, 20, 1, 4, cut);
                    }

                    break;

                case 1:
                    // O Rei de Prata, maior que todos, apontando pra baixo.
                    Figure(32, 13, true);
                    canvas.Fill(33, 18, 6, 1, cut);
                    canvas.Fill(38, 18, 1, 8, cut);
                    canvas.Set(37, 25, cut);
                    canvas.Set(39, 25, cut);
                    Figure(16, 22, false);
                    Figure(48, 22, false);
                    break;

                case 2:
                    // A broca descendo: um cone em espiral, com raios de impacto.
                    for (int i = 0; i < 16; i++)
                    {
                        int half = (16 - i) / 3;
                        canvas.Fill(32 - half, 13 + i, half * 2 + 1, 1, i % 3 == 0 ? cut : stoneLight);
                    }

                    canvas.Fill(22, 30, 5, 1, cut);
                    canvas.Fill(38, 30, 5, 1, cut);
                    canvas.Set(27, 32, cut);
                    canvas.Set(37, 32, cut);
                    break;

                default:
                    // O Heart of Quartz, rachado: um losango com uma fenda verde.
                    for (int y = 13; y < 34; y++)
                    {
                        int half = 10 - Math.Abs(y - 23);
                        if (half < 0) continue;

                        canvas.Set(32 - half, y, cut);
                        canvas.Set(32 + half, y, cut);
                    }

                    for (int y = 15; y < 32; y++)
                    {
                        canvas.Set(32 + (y % 4 < 2 ? 0 : 1), y, new Rgba(90, 220, 140));
                    }

                    break;
            }

            return canvas;
        }

        /// <summary>
        /// Echo Hunter, 16x24, olhando pra direita: um mineiro curvado, sem
        /// olhos, com cristais brotando da cabeca.
        /// </summary>
        public static PixelCanvas EchoHunter()
        {
            var canvas = new PixelCanvas(16, 24);

            var cloth = new Rgba(74, 66, 82);
            var clothLight = new Rgba(104, 94, 114);
            var skin = new Rgba(150, 142, 160);
            var dark = new Rgba(40, 34, 48);

            // Pernas.
            canvas.Fill(4, 17, 3, 7, dark);
            canvas.Fill(9, 17, 3, 7, dark);
            canvas.Fill(5, 17, 1, 6, cloth);
            canvas.Fill(10, 17, 1, 6, cloth);

            // Tronco curvado pra frente.
            canvas.Fill(4, 9, 8, 9, cloth);
            canvas.Fill(5, 9, 3, 8, clothLight);
            canvas.Fill(11, 11, 3, 2, cloth);

            // As garras, estendidas.
            canvas.Fill(13, 12, 3, 1, skin);
            canvas.Set(15, 13, skin);
            canvas.Set(15, 11, skin);

            // A cabeca: lisa, sem rosto — so uma boca.
            canvas.Fill(6, 4, 7, 6, skin);
            canvas.Fill(6, 4, 2, 5, new Rgba(186, 178, 196));
            canvas.Fill(10, 7, 3, 1, dark);

            // Os cristais crescendo por cima.
            Shard(canvas, 7, 4, 1, 5, -1, Hues[0]);
            Shard(canvas, 10, 4, 1, 4, 1, Hues[0]);
            Shard(canvas, 12, 5, 0, 3, 1, Hues[1]);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>Crystal Mite, 12x10, olhando pra direita: um caco de cristal com perninhas.</summary>
        public static PixelCanvas Mite()
        {
            var canvas = new PixelCanvas(12, 10);
            Rgba[] hue = Hues[0];

            Shard(canvas, 4, 7, 1, 6, -1, hue);
            Shard(canvas, 7, 7, 2, 7, 1, hue);

            canvas.Fill(2, 7, 9, 1, hue[0]);

            // Perninhas.
            canvas.Set(2, 8, Outline);
            canvas.Set(5, 8, Outline);
            canvas.Set(8, 8, Outline);
            canvas.Set(10, 8, Outline);

            // Dois olhos, que so se ve quando ele acende.
            canvas.Set(9, 5, new Rgba(255, 240, 255));
            canvas.Set(10, 5, new Rgba(255, 240, 255));

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>
        /// Shard Sentinel, 16x32: uma estatua de cristal em forma de gente, de
        /// bracos cruzados, com um nucleo aceso no peito.
        /// </summary>
        public static PixelCanvas Sentinel()
        {
            var canvas = new PixelCanvas(16, 32);
            Rgba[] hue = Hues[1];

            // Pedestal.
            canvas.Fill(2, 28, 12, 4, new Rgba(60, 56, 70));
            canvas.Fill(2, 28, 12, 1, new Rgba(92, 88, 104));

            // Corpo: um prisma largo.
            Shard(canvas, 8, 28, 4, 20, 0, hue);

            // Ombros: dois cacos pra fora.
            Shard(canvas, 3, 18, 1, 7, -1, hue);
            Shard(canvas, 13, 18, 1, 7, 1, hue);

            // Cabeca.
            Shard(canvas, 8, 9, 2, 8, 0, hue);

            // O nucleo.
            canvas.Fill(7, 16, 2, 3, new Rgba(236, 150, 255));
            canvas.Set(7, 16, new Rgba(255, 236, 255));

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>Um estilhaco lancado, 10x4, apontando pra direita.</summary>
        public static PixelCanvas ShardShot()
        {
            var canvas = new PixelCanvas(10, 4);

            for (int x = 0; x < 10; x++)
            {
                int half = x < 7 ? 1 : 0;

                for (int y = 2 - half - (half > 0 ? 0 : 0); y < 2 + half + (half > 0 ? 0 : 1); y++)
                {
                    canvas.Set(x, y, x > 6 ? new Rgba(255, 255, 255) : (y < 2 ? new Rgba(228, 214, 255) : new Rgba(160, 130, 220)));
                }
            }

            return canvas;
        }

        /// <summary>
        /// The Resonant, 32x40, olhando pra direita: um automato largo, de
        /// latao escuro, com um diapasao de cristal no lugar do braco da frente.
        /// </summary>
        public static PixelCanvas TheResonant()
        {
            var canvas = new PixelCanvas(32, 40);

            var dark = new Rgba(44, 40, 56);
            var metal = new Rgba(92, 84, 108);
            var light = new Rgba(136, 126, 154);
            var rim = new Rgba(178, 168, 196);
            Rgba[] crystal = Hues[1];

            // Pernas curtas e grossas.
            canvas.Fill(8, 30, 6, 10, dark);
            canvas.Fill(17, 30, 6, 10, dark);
            canvas.Fill(9, 30, 2, 9, metal);
            canvas.Fill(18, 30, 2, 9, metal);
            canvas.Fill(7, 38, 8, 2, metal);
            canvas.Fill(16, 38, 8, 2, metal);

            // Tronco: um barril.
            canvas.Fill(6, 13, 19, 18, metal);
            canvas.Fill(7, 13, 5, 17, light);
            canvas.Fill(6, 13, 19, 1, rim);
            canvas.Fill(6, 21, 19, 1, dark);
            canvas.Fill(6, 30, 19, 1, dark);

            // Rebites.
            for (int x = 8; x < 24; x += 4)
            {
                canvas.Set(x, 15, rim);
                canvas.Set(x, 28, rim);
            }

            // O nucleo no peito: uma lente azulada.
            canvas.Fill(13, 16, 5, 4, dark);
            canvas.Fill(14, 17, 3, 2, new Rgba(150, 210, 255));
            canvas.Set(14, 17, new Rgba(236, 248, 255));

            // Cabeca pequena, enterrada nos ombros, com uma fenda so.
            canvas.Fill(11, 6, 9, 7, metal);
            canvas.Fill(12, 6, 3, 6, light);
            canvas.Fill(15, 9, 5, 1, new Rgba(150, 210, 255));

            // Braco de tras, caido.
            canvas.Fill(3, 15, 4, 12, dark);
            canvas.Fill(3, 26, 4, 3, metal);

            // O braco da frente: o cabo do diapasao.
            canvas.Fill(24, 15, 3, 4, metal);
            canvas.Fill(26, 10, 2, 9, dark);

            // E os dois dentes de cristal.
            Shard(canvas, 25, 10, 1, 10, -1, crystal);
            Shard(canvas, 29, 10, 1, 10, 1, crystal);
            canvas.Fill(25, 10, 5, 1, crystal[0]);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>
        /// The Geode Drill, 64x40, olhando pra direita: a broca de cristal na
        /// frente, a cabine vazia no meio, o motor (o ponto fraco) nas costas e
        /// as esteiras embaixo. Veios de Oxide por toda parte.
        /// </summary>
        public static PixelCanvas GeodeDrill()
        {
            var canvas = new PixelCanvas(64, 40);

            var dark = new Rgba(40, 36, 50);
            var metal = new Rgba(88, 80, 100);
            var light = new Rgba(130, 120, 146);
            var rim = new Rgba(176, 166, 190);
            var oxide = new Rgba(92, 242, 153);
            var oxideDark = new Rgba(40, 150, 96);
            Rgba[] crystal = Hues[0];

            // Esteiras.
            canvas.Fill(4, 31, 40, 8, dark);
            canvas.Fill(6, 30, 36, 1, dark);
            canvas.Fill(5, 39, 38, 1, dark);
            for (int x = 6; x < 43; x += 4) canvas.Fill(x, 33, 2, 5, metal);
            for (int x = 9; x < 40; x += 10) canvas.Fill(x, 34, 3, 3, light);

            // Corpo.
            canvas.Fill(6, 12, 34, 19, metal);
            canvas.Fill(6, 12, 34, 1, rim);
            canvas.Fill(7, 13, 32, 3, light);
            canvas.Fill(6, 22, 34, 1, dark);

            // A cabine: seis lugares, ninguem.
            canvas.Fill(20, 6, 14, 7, metal);
            canvas.Fill(20, 6, 14, 1, rim);
            canvas.Fill(22, 8, 4, 4, dark);
            canvas.Fill(28, 8, 4, 4, dark);

            // O motor nas costas: uma grade com brasa atras.
            canvas.Fill(2, 14, 7, 14, dark);
            canvas.Fill(3, 15, 5, 12, new Rgba(190, 96, 54));
            for (int y = 16; y < 27; y += 2) canvas.Fill(3, y, 5, 1, dark);
            canvas.Fill(1, 9, 3, 6, metal);
            canvas.Fill(0, 7, 5, 2, dark);

            // A placa do chassi, ainda coberta.
            canvas.Fill(12, 24, 12, 5, dark);
            canvas.Fill(13, 25, 10, 3, new Rgba(120, 124, 138));

            // O colar onde a broca encaixa.
            canvas.Fill(40, 10, 4, 22, dark);
            canvas.Fill(41, 10, 1, 22, rim);

            // A broca: um cone de cristal, com a rosca marcada.
            for (int x = 0; x < 20; x++)
            {
                int half = 11 - x * 11 / 20;

                for (int dy = -half; dy <= half; dy++)
                {
                    Rgba color = dy < -half / 2 ? crystal[2] : (dy > half / 2 ? crystal[0] : crystal[1]);
                    if ((x + dy + 40) % 5 == 0) color = crystal[3];
                    canvas.Set(44 + x, 21 + dy, color);
                }
            }

            // Veios de Oxide subindo pelo corpo e pela broca.
            int[] veinY = { 30, 28, 27, 25, 24, 22, 20, 19, 18, 16 };
            for (int i = 0; i < veinY.Length; i++)
            {
                canvas.Set(26 + i, veinY[i], oxide);
                canvas.Set(26 + i, veinY[i] + 1, oxideDark);
            }

            for (int x = 46; x < 58; x += 2)
            {
                canvas.Set(x, 21 + (x % 4 == 0 ? 1 : -1), oxide);
            }

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>A safira bruta, 10x10: uma gema azul, lascada.</summary>
        public static PixelCanvas Sapphire()
        {
            var canvas = new PixelCanvas(10, 10);

            var deep = new Rgba(30, 60, 150);
            var body = new Rgba(60, 110, 220);
            var light = new Rgba(130, 180, 255);
            var shine = new Rgba(236, 246, 255);

            for (int y = 1; y < 9; y++)
            {
                int half = y < 4 ? y : (y < 6 ? 3 : 8 - y);

                for (int dx = -half; dx <= half; dx++)
                {
                    Rgba color = dx < -1 ? light : (dx > 1 ? deep : body);
                    canvas.Set(5 + dx, y, color);
                }
            }

            canvas.Set(4, 3, shine);
            canvas.Set(3, 4, shine);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>
        /// Uma barreira de cristal macico, 16x16, pra repetir em pe: prismas
        /// encostados, sem fresta entre eles.
        /// </summary>
        public static PixelCanvas Barrier()
        {
            var canvas = new PixelCanvas(16, 16);
            Rgba[] hue = Hues[1];

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    // Tres faces por prisma, e os prismas inclinados.
                    int band = ((x + y / 4) % 8 + 8) % 8;
                    Rgba color = band < 2 ? hue[3] : (band < 5 ? hue[2] : (band < 7 ? hue[1] : hue[0]));

                    if (Hash(x, y, 77) % 23 == 0) color = hue[3];
                    canvas.Set(x, y, color);
                }
            }

            return canvas;
        }
    }
}
