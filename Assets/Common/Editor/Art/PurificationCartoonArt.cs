using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os quadros da cutscene desenhada da purificacao do Scrap (fim da Phase 1):
    /// de costas, o braco, o cinzel, o tubo de ar e a pose final.
    ///
    /// Cada quadro tem 320x180 e sai em tres imagens: o "antes" (sucata), o
    /// "depois" (polido) e o mapa de ordem — no canal vermelho, de 0 a 254, quem
    /// e mais escuro se transforma primeiro; 255 nunca muda. No jogo, o
    /// PurificationCartoon varre esse mapa com um limite que sobe, e a borda da
    /// transformacao brilha.
    ///
    /// As faixas pretas de cinema cobrem ~12 pixels em cima e embaixo: o que
    /// importa fica entre y 14 e 166.
    ///
    /// Tudo e desenhado com um "pintor" simples: formas (elipse, capsula,
    /// poligono, gema lapidada) com normal, luz difusa e especular, contorno,
    /// luz de borda e textura do material (ferrugem, pedra, cristal, pano,
    /// couro, metal, madeira). As cores sao as do HeroArt.
    ///
    /// Nao depende de UnityEngine, como o resto da arte gerada.
    /// </summary>
    public static partial class PurificationCartoonArt
    {
        public const int Width = 320;
        public const int Height = 180;

        /// <summary>Valor do mapa de ordem pra pixel que nunca muda.</summary>
        public const byte Static = 255;

        public enum EmitterKind { Flakes, Sparks, Steam, Glint }

        /// <summary>Uma fonte de particulas do quadro, em pixels do quadro (y pra baixo).</summary>
        public readonly struct Emitter
        {
            public readonly EmitterKind Kind;
            public readonly int X;
            public readonly int Y;
            public readonly int W;
            public readonly int H;

            /// <summary>Em que ponto da transformacao (0 a 1) ela comeca.</summary>
            public readonly float StartAt;

            public Emitter(EmitterKind kind, int x, int y, int w, int h, float startAt)
            {
                Kind = kind;
                X = x;
                Y = y;
                W = w;
                H = h;
                StartAt = startAt;
            }
        }

        public sealed class Shot
        {
            public string Name;
            public PixelCanvas Before;
            public PixelCanvas After;
            public PixelCanvas Order;

            /// <summary>Quadros que se repetem depois da transformacao (a capa na pose final). Nulo: nenhum.</summary>
            public PixelCanvas[] Loop;

            public Emitter[] Emitters;
            public float Duration;

            /// <summary>Quanto a camera desliza ao longo do quadro, em pixels.</summary>
            public float PanX;
            public float PanY;

            public float ZoomFrom = 1f;
            public float ZoomTo = 1f;
        }

        public static Shot[] All()
        {
            return new[] { Back(), Arm(), Chisel(), Vent(), Hero() };
        }

        // ------------------------------------------------------------------
        // Cores e materiais
        // ------------------------------------------------------------------

        private static readonly Rgba Ink = new Rgba(18, 14, 22);

        private static readonly Rgba[] CaveRock =
        {
            new Rgba(8, 11, 17), new Rgba(15, 21, 28), new Rgba(24, 33, 40), new Rgba(36, 48, 54),
        };

        private static readonly Rgba RustOrange = new Rgba(176, 84, 38);
        private static readonly Rgba DeepRust = new Rgba(84, 36, 22);
        private static readonly Rgba Purify = new Rgba(150, 232, 246);
        private static readonly Rgba PurifyCore = new Rgba(255, 252, 236);
        private static readonly Rgba Teal = new Rgba(86, 220, 220);

        private enum Surface { Plain, Rust, Rock, Crystal, Cloth, Leather, Metal, Wood, Blade, Moss }

        private sealed class Mat
        {
            public readonly Rgba[] Tones;
            public readonly Surface Surface;
            public readonly Rgba Accent;

            /// <summary>Forca do brilho especular (0: fosco).</summary>
            public readonly float Shine;

            public Mat(Rgba[] tones, Surface surface, Rgba accent, float shine)
            {
                Tones = tones;
                Surface = surface;
                Accent = accent;
                Shine = shine;
            }
        }

        private static readonly HeroArt.Look S = HeroArt.Scrap();
        private static readonly HeroArt.Look P = HeroArt.Polished();

        private static readonly Rgba[] RustyTones =
        {
            new Rgba(52, 30, 24), new Rgba(96, 54, 36), new Rgba(140, 82, 50), new Rgba(178, 116, 70),
        };

        private static readonly Rgba[] BronzeTones =
        {
            new Rgba(70, 44, 30), new Rgba(128, 86, 50), new Rgba(184, 136, 80), new Rgba(226, 186, 120),
        };

        private static readonly Rgba[] RustBladeTones =
        {
            new Rgba(58, 42, 38), new Rgba(98, 70, 56), new Rgba(138, 100, 76), new Rgba(172, 136, 104),
        };

        private static readonly Rgba[] SilverTones =
        {
            new Rgba(62, 72, 94), new Rgba(108, 122, 148), new Rgba(160, 176, 198), new Rgba(208, 220, 236),
        };

        private static readonly Rgba[] GemTones =
        {
            new Rgba(76, 102, 140), new Rgba(130, 166, 204), new Rgba(186, 214, 238), new Rgba(232, 244, 255),
        };

        private static Mat RustMetal() => new Mat(RustyTones, Surface.Rust, RustOrange, 0.05f);
        private static Mat Bronze() => new Mat(BronzeTones, Surface.Metal, new Rgba(255, 244, 210), 0.65f);
        private static Mat Rock() => new Mat(S.Stone, Surface.Rock, S.Fleck, 0.05f);
        private static Mat Quartz() => new Mat(GemTones, Surface.Crystal, new Rgba(255, 255, 255), 0.55f);
        private static Mat RawCloth() => new Mat(S.Cloth, Surface.Cloth, S.Trim, 0f);
        private static Mat Leather() => new Mat(P.Cloth, Surface.Leather, P.Trim, 0.12f);
        private static Mat RustSpring() => new Mat(S.Spring, Surface.Rust, RustOrange, 0.05f);
        private static Mat BrightSpring() => new Mat(P.Spring, Surface.Metal, new Rgba(255, 240, 200), 0.6f);
        private static Mat Gold() => new Mat(new[] { new Rgba(110, 72, 28), new Rgba(176, 124, 46), new Rgba(226, 178, 80), new Rgba(255, 228, 150) }, Surface.Metal, new Rgba(255, 250, 220), 0.7f);
        private static Mat Wood() => new Mat(S.Wood, Surface.Wood, S.Wood[3], 0.05f);
        private static Mat RustBlade() => new Mat(RustBladeTones, Surface.Rust, RustOrange, 0.05f);
        private static Mat Silver() => new Mat(SilverTones, Surface.Blade, new Rgba(255, 255, 255), 0.3f);
        private static Mat Shade(Rgba[] tones) => new Mat(tones, Surface.Plain, tones[3], 0f);

        // ------------------------------------------------------------------
        // Contas
        // ------------------------------------------------------------------

        private static float Dist(float x, float y, float cx, float cy)
        {
            float dx = x - cx, dy = y - cy;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Onde (x, y) cai no segmento A-B, de 0 a 1.</summary>
        private static float Along(float x, float y, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax, dy = by - ay;
            float t = ((x - ax) * dx + (y - ay) * dy) / Math.Max(0.0001f, dx * dx + dy * dy);
            return t < 0 ? 0 : (t > 1 ? 1 : t);
        }

        private static void Normalize(ref float x, ref float y, ref float z)
        {
            float len = (float)Math.Sqrt(x * x + y * y + z * z);
            if (len < 0.0001f) { x = 0; y = 0; z = 1; return; }
            x /= len; y /= len; z /= len;
        }

        private static bool InsidePoly((float x, float y)[] pts, float x, float y)
        {
            bool inside = false;
            for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
            {
                if ((pts[i].y > y) != (pts[j].y > y) &&
                    x < (pts[j].x - pts[i].x) * (y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777216f;
            }
        }

        private static float Noise(float x, float y, int seed)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3 - 2 * fx);
            fy = fy * fy * (3 - 2 * fy);

            float a = Hash(x0, y0, seed), b = Hash(x0 + 1, y0, seed);
            float c = Hash(x0, y0 + 1, seed), d = Hash(x0 + 1, y0 + 1, seed);
            return (a + (b - a) * fx) + ((c + (d - c) * fx) - (a + (b - a) * fx)) * fy;
        }

        /// <summary>Distancia ate a borda da celula de Voronoi mais proxima (0 = na junta).</summary>
        private static float CellEdge(float x, float y, int seed)
        {
            int cx = (int)Math.Floor(x), cy = (int)Math.Floor(y);
            float best = float.MaxValue, second = float.MaxValue;

            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    float px = cx + i + Hash(cx + i, cy + j, seed);
                    float py = cy + j + Hash(cx + i, cy + j, seed + 1);
                    float d = Dist(x, y, px, py);
                    if (d < best) { second = best; best = d; }
                    else if (d < second) second = d;
                }
            }

            return second - best;
        }

        private static readonly float[] BayerMatrix =
        {
            0f, 8f, 2f, 10f,
            12f, 4f, 14f, 6f,
            3f, 11f, 1f, 9f,
            15f, 7f, 13f, 5f,
        };

        private static float Bayer(int x, int y) => BayerMatrix[(x & 3) + (y & 3) * 4] / 16f;
    }
}
