using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Uma cor em ponto flutuante (0..1), pra misturar sem perder degrau.</summary>
    public struct C3
    {
        public float R, G, B;

        public C3(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static C3 Hex(int rgb) => new C3(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
        public static C3 Lerp(C3 a, C3 b, float t) => new C3(a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t);
        public static C3 operator *(C3 a, float k) => new C3(a.R * k, a.G * k, a.B * k);
        public static C3 operator *(C3 a, C3 b) => new C3(a.R * b.R, a.G * b.G, a.B * b.B);
        public static C3 operator +(C3 a, C3 b) => new C3(a.R + b.R, a.G + b.G, a.B + b.B);
    }

    /// <summary>
    /// A superficie de um documento: cor, altura e brilho por pixel. Tudo e
    /// pintado aqui (papel, metal, pedra, tinta, sulcos) e so no fim a luz
    /// e calculada pela altura (<see cref="Bake"/>): por isso um risco de
    /// prego tem borda clara e fundo escuro, um rebite tem volume e uma dobra
    /// de papel faz sombra de verdade.
    /// </summary>
    public sealed class DocSurface
    {
        public readonly int W, H;
        public readonly C3[] Col;
        public readonly float[] A;
        public readonly float[] Ht;
        public readonly float[] Gloss;

        /// <summary>Distancia ate a borda do recorte (0 fora), pra envelhecer bordas.</summary>
        public float[] Edge;

        public DocSurface(int w, int h)
        {
            W = w;
            H = h;
            Col = new C3[w * h];
            A = new float[w * h];
            Ht = new float[w * h];
            Gloss = new float[w * h];
            Edge = new float[w * h];
        }

        public bool In(int x, int y) => x >= 0 && y >= 0 && x < W && y < H && A[x + y * W] > 0f;

        // ------------------------------------------------------------------
        // Ruido
        // ------------------------------------------------------------------

        public static float Fbm(float x, float y, int seed, int octaves = 4)
        {
            float sum = 0f, amp = 0.5f, norm = 0f;
            for (int o = 0; o < octaves; o++)
            {
                sum += ArtKit.Noise(x, y, seed + o * 101) * amp;
                norm += amp;
                x *= 2.03f;
                y *= 2.03f;
                amp *= 0.5f;
            }

            return sum / norm;
        }

        public static float Rand(int x, int y, int seed) => (ArtKit.Hash(x, y, seed) & 0xffff) / 65535f;

        // ------------------------------------------------------------------
        // Recorte
        // ------------------------------------------------------------------

        /// <summary>Recorta a forma: o que fica dentro ganha alfa; calcula a distancia ate a borda.</summary>
        public void Shape(Func<int, int, bool> inside)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    A[x + y * W] = inside(x, y) ? 1f : 0f;
            ComputeEdge();
        }

        /// <summary>Junta mais uma forma por cima (outra peca no mesmo desenho).</summary>
        public void AddShape(Func<int, int, bool> inside)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (inside(x, y)) A[x + y * W] = 1f;
            ComputeEdge();
        }

        public void ComputeEdge()
        {
            const float big = 1e6f;
            for (int i = 0; i < Edge.Length; i++) Edge[i] = A[i] > 0f ? big : 0f;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (Edge[i] == 0f) continue;
                    float d = Edge[i];
                    if (x > 0) d = Math.Min(d, Edge[i - 1] + 1f);
                    else d = Math.Min(d, 1f);
                    if (y > 0) d = Math.Min(d, Edge[i - W] + 1f);
                    else d = Math.Min(d, 1f);
                    if (x > 0 && y > 0) d = Math.Min(d, Edge[i - W - 1] + 1.414f);
                    if (x < W - 1 && y > 0) d = Math.Min(d, Edge[i - W + 1] + 1.414f);
                    Edge[i] = d;
                }

            for (int y = H - 1; y >= 0; y--)
                for (int x = W - 1; x >= 0; x--)
                {
                    int i = x + y * W;
                    if (Edge[i] == 0f) continue;
                    float d = Edge[i];
                    if (x < W - 1) d = Math.Min(d, Edge[i + 1] + 1f);
                    else d = Math.Min(d, 1f);
                    if (y < H - 1) d = Math.Min(d, Edge[i + W] + 1f);
                    else d = Math.Min(d, 1f);
                    if (x < W - 1 && y < H - 1) d = Math.Min(d, Edge[i + W + 1] + 1.414f);
                    if (x > 0 && y < H - 1) d = Math.Min(d, Edge[i + W - 1] + 1.414f);
                    Edge[i] = d;
                }
        }

        /// <summary>Um retangulo de beirada irregular: cada lado pode ser liso, rasgado ou serrilhado (papel feito a mao).</summary>
        public static Func<int, int, bool> Sheet(float x0, float y0, float x1, float y1, int seed,
            float left = 0f, float top = 0f, float right = 0f, float bottom = 0f, float corner = 0f)
        {
            return (x, y) =>
            {
                float l = x0 + Edgy(y, left, seed + 1);
                float r = x1 - Edgy(y, right, seed + 2);
                float t = y0 + Edgy(x, top, seed + 3);
                float b = y1 - Edgy(x, bottom, seed + 4);
                if (x < l || x > r || y < t || y > b) return false;
                if (corner > 0f)
                {
                    // Cantos gastos, arredondados.
                    float cx = Math.Min(x - l, r - x), cy = Math.Min(y - t, b - y);
                    if (cx < corner && cy < corner)
                    {
                        float dx = corner - cx, dy = corner - cy;
                        if (dx * dx + dy * dy > corner * corner) return false;
                    }
                }

                return true;
            };
        }

        /// <summary>O quanto uma beirada entra: 0 liso; ate 3 serrilhado; acima rasgado.</summary>
        private static float Edgy(int t, float amount, int seed)
        {
            if (amount <= 0f) return 0f;
            float tear = Fbm(t * 0.035f, 0.5f, seed, 3) * amount * 1.6f;
            float fibre = Rand(t, 3, seed) * Math.Min(2.5f, amount * 0.4f);
            return tear + fibre;
        }

        // ------------------------------------------------------------------
        // Materiais
        // ------------------------------------------------------------------

        /// <summary>
        /// Papel: o tom manchado em nuvem, fibras, pintas de mofo (foxing),
        /// bordas tostadas pelo tempo e o grao que pega luz.
        /// </summary>
        public void Paper(C3 light, C3 dark, int seed, float age = 0.5f, float fibres = 1f, float foxing = 0.5f)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float cloud = Fbm(x * 0.012f, y * 0.012f, seed, 4);
                    float blot = Math.Max(0f, Fbm(x * 0.03f, y * 0.03f, seed + 7, 3) - 0.55f) * 2.2f;
                    float edge = Math.Max(0f, 1f - Edge[i] / (10f + age * 14f));
                    float t = (cloud - 0.35f) * 0.7f * age + blot * age * 0.6f + edge * edge * (0.45f + age * 0.5f);
                    float grain = (Rand(x, y, seed + 3) - 0.5f) * 0.05f;
                    Col[i] = C3.Lerp(light, dark, Clamp01(t)) * (1f + grain);
                    Ht[i] = (Fbm(x * 0.5f, y * 0.5f, seed + 9, 2) - 0.5f) * 0.25f;
                    Gloss[i] = 0.02f;
                }

            // Fibras: tracinhos claros e escuros em toda direcao.
            int count = (int)(W * H * 0.004f * fibres);
            for (int f = 0; f < count; f++)
            {
                float fx = Rand(f, 1, seed + 20) * W, fy = Rand(f, 2, seed + 20) * H;
                float ang = Rand(f, 3, seed + 20) * 6.283f;
                int len = 3 + (int)(Rand(f, 4, seed + 20) * 7f);
                bool darkFibre = Rand(f, 5, seed + 20) < 0.4f;
                for (int s = 0; s < len; s++)
                {
                    int px = (int)(fx + Math.Cos(ang + s * 0.15f) * s), py = (int)(fy + Math.Sin(ang + s * 0.15f) * s);
                    if (!In(px, py)) continue;
                    int i = px + py * W;
                    Col[i] = C3.Lerp(Col[i], darkFibre ? dark : light * 1.06f, 0.25f);
                }
            }

            // Foxing: pintas de mofo ferrugem, em cachos.
            int clusters = (int)(W * H * 0.00004f * foxing * 10f);
            for (int c = 0; c < clusters; c++)
            {
                float cx = Rand(c, 1, seed + 40) * W, cy = Rand(c, 2, seed + 40) * H;
                int spots = 3 + (int)(Rand(c, 3, seed + 40) * 9f);
                for (int s = 0; s < spots; s++)
                {
                    float sx = cx + (Rand(c, s * 3, seed + 41) - 0.5f) * 30f;
                    float sy = cy + (Rand(c, s * 3 + 1, seed + 41) - 0.5f) * 30f;
                    float r = 0.6f + Rand(c, s * 3 + 2, seed + 41) * 2.2f;
                    Spot(sx, sy, r, C3.Hex(0x8a5a32), 0.35f + Rand(c, s, seed + 42) * 0.3f);
                }
            }
        }

        /// <summary>Uma pinta macia de cor (mancha, mofo, sujeira).</summary>
        public void Spot(float cx, float cy, float r, C3 color, float strength)
        {
            for (int y = (int)(cy - r - 1); y <= cy + r + 1; y++)
                for (int x = (int)(cx - r - 1); x <= cx + r + 1; x++)
                {
                    if (!In(x, y)) continue;
                    float d = (float)Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d > r) continue;
                    int i = x + y * W;
                    Col[i] = C3.Lerp(Col[i], Col[i] * color * 1.6f, strength * (1f - d / r * 0.7f));
                }
        }

        /// <summary>A marca de um copo ou de agua: o anel escuro da borda e o miolo lavado.</summary>
        public void Ring(float cx, float cy, float r, C3 color, float strength, int seed, bool fullRing = true)
        {
            for (int y = (int)(cy - r - 4); y <= cy + r + 4; y++)
                for (int x = (int)(cx - r - 4); x <= cx + r + 4; x++)
                {
                    if (!In(x, y)) continue;
                    float ang = (float)Math.Atan2(y - cy, x - cx);
                    float wob = (Fbm(ang * 2f + 10f, 0.3f, seed, 3) - 0.5f) * r * 0.25f;
                    float d = (float)Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - wob;
                    int i = x + y * W;
                    if (!fullRing && Fbm(ang * 1.5f + 3f, 0.7f, seed + 5, 2) < 0.45f) continue;
                    float rim = Math.Max(0f, 1f - Math.Abs(d - r) / 1.6f);
                    float inside = d < r ? 0.25f : 0f;
                    float k = Math.Max(rim, inside * Fbm(x * 0.08f, y * 0.08f, seed + 2, 2));
                    if (k > 0f) Col[i] = C3.Lerp(Col[i], Col[i] * color * 1.5f, k * strength);
                }
        }

        /// <summary>A dobra: um vinco reto entre dois pontos, com sombra de um lado e luz do outro.</summary>
        public void Fold(float x0, float y0, float x1, float y1, float depth = 1.2f, float width = 3f)
        {
            float dx = x1 - x0, dy = y1 - y0;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float nx = -dy / len, ny = dx / len;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float d = (x - x0) * nx + (y - y0) * ny;
                    float ad = Math.Abs(d);
                    // O vinco e uma crista suave; os paineis levemente inclinados.
                    Ht[i] += depth * Math.Max(0f, 1f - ad / width) + Math.Min(ad, 60f) / 60f * -depth * 0.6f;
                    if (ad < 0.8f) Col[i] = Col[i] * 0.93f;
                }
        }

        /// <summary>Queima a borda: tosta, carboniza e come o papel.</summary>
        public void Burn(float reach, int seed, Func<int, int, float> where = null)
        {
            var cut = new List<int>();
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float k = where != null ? where(x, y) : 1f;
                    if (k <= 0f) continue;
                    float n = Fbm(x * 0.04f, y * 0.04f, seed, 4);
                    float depth = Edge[i] - reach * k * (0.4f + n * 1.2f);
                    if (depth < 0f) cut.Add(i);
                    else if (depth < 2f) Col[i] = C3.Hex(0x1a120c);
                    else if (depth < 9f) Col[i] = C3.Lerp(C3.Hex(0x3a2210), Col[i], (depth - 2f) / 7f);
                    else if (depth < 16f) Col[i] = C3.Lerp(Col[i] * C3.Hex(0xc89a60), Col[i], (depth - 9f) / 7f);
                }

            foreach (int i in cut) A[i] = 0f;
            ComputeEdge();
        }

        /// <summary>Metal: tom base, escovado na direcao dada, bisel nas bordas e brilho.</summary>
        public void Metal(C3 baseColor, int seed, float brushedAngle = 0f, float bevel = 4f, float gloss = 0.7f,
            float wear = 0.4f)
        {
            float ca = (float)Math.Cos(brushedAngle), sa = (float)Math.Sin(brushedAngle);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float u = x * ca + y * sa, v = -x * sa + y * ca;
                    float brushed = Fbm(u * 0.02f, v * 0.9f, seed, 3);
                    float cloud = Fbm(x * 0.015f, y * 0.015f, seed + 3, 3);
                    float t = (brushed - 0.5f) * 0.18f + (cloud - 0.5f) * 0.35f * wear;
                    Col[i] = baseColor * (1f + t);
                    float e = Edge[i];
                    Ht[i] = e < bevel ? (float)Math.Sqrt(Math.Max(0f, 1f - (bevel - e) * (bevel - e) / (bevel * bevel))) * bevel * 0.6f
                        : bevel * 0.6f;
                    Ht[i] += (brushed - 0.5f) * 0.15f;
                    Gloss[i] = gloss * (0.8f + cloud * 0.4f);
                }

            // Riscos finos de uso.
            int lines = (int)(W * H * 0.0006f * wear);
            for (int k = 0; k < lines; k++)
            {
                float x0 = Rand(k, 1, seed + 60) * W, y0 = Rand(k, 2, seed + 60) * H;
                float ang = Rand(k, 3, seed + 60) * 6.283f;
                float len = 6f + Rand(k, 4, seed + 60) * 26f;
                for (float s = 0; s < len; s += 0.7f)
                {
                    int px = (int)(x0 + Math.Cos(ang) * s), py = (int)(y0 + Math.Sin(ang) * s);
                    if (!In(px, py)) continue;
                    int i = px + py * W;
                    Ht[i] -= 0.35f;
                    Col[i] = Col[i] * 1.08f;
                }
            }
        }

        /// <summary>Azinhavre: crostas verdes e escorridos pra baixo, foscos e um pouco em relevo.</summary>
        public void Patina(float amount, int seed, C3 crust, C3 crustLight, float drips = 1f)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float n = Fbm(x * 0.035f, y * 0.035f, seed, 4) * 0.7f + Fbm(x * 0.15f, y * 0.15f, seed + 5, 2) * 0.3f;
                    float edge = Math.Max(0f, 1f - Edge[i] / 18f) * 0.25f;
                    float k = n + edge - (1f - amount);
                    if (k <= 0f) continue;
                    float t = Clamp01(k * 6f);
                    C3 c = C3.Lerp(crust, crustLight, Fbm(x * 0.2f, y * 0.2f, seed + 9, 2));
                    Col[i] = C3.Lerp(Col[i], c, t * 0.9f);
                    Ht[i] += t * 0.6f;
                    Gloss[i] *= 1f - t;
                }

            // Escorridos verticais.
            int count = (int)(W * 0.08f * drips * amount * 2f);
            for (int d = 0; d < count; d++)
            {
                float x = Rand(d, 1, seed + 30) * W;
                float y0 = Rand(d, 2, seed + 30) * H * 0.7f;
                float len = 10f + Rand(d, 3, seed + 30) * 50f;
                float wdt = 0.8f + Rand(d, 4, seed + 30) * 1.6f;
                for (float s = 0; s < len; s += 0.5f)
                {
                    float px = x + (float)Math.Sin(s * 0.15f + d) * 0.6f;
                    float k = (1f - s / len) * 0.55f;
                    for (int ox = (int)(-wdt); ox <= wdt; ox++)
                    {
                        int ix = (int)px + ox, iy = (int)(y0 + s);
                        if (!In(ix, iy)) continue;
                        int i = ix + iy * W;
                        Col[i] = C3.Lerp(Col[i], crust, k);
                        Gloss[i] *= 1f - k;
                    }
                }
            }
        }

        /// <summary>Pedra: relevo em camadas, veios, pontos de mineral e rachaduras finas.</summary>
        public void Stone(C3 dark, C3 light, int seed, float strata = 0.5f, float roughness = 1f)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float big = Fbm(x * 0.012f, y * 0.012f, seed, 4);
                    float mid = Fbm(x * 0.05f, y * 0.05f, seed + 3, 3);
                    float fine = Fbm(x * 0.25f, y * 0.25f, seed + 6, 2);
                    float layer = (float)Math.Sin(y * 0.09f + big * 6f) * 0.5f + 0.5f;
                    float t = big * 0.45f + mid * 0.3f + fine * 0.1f + layer * strata * 0.25f;
                    Col[i] = C3.Lerp(dark, light, Clamp01((t - 0.3f) * 1.7f));
                    float e = Edge[i];
                    float rim = e < 6f ? (e / 6f) : 1f;
                    Ht[i] = (mid * 2.2f + fine * 1.2f + big * 2f) * roughness * rim - (1f - rim) * 1.5f;
                    if (Rand(x, y, seed + 9) < 0.004f) Col[i] = Col[i] * 1.35f;
                    Gloss[i] = 0.05f;
                }

            // Rachaduras: caminhos tortos, sulcos fundos.
            for (int k = 0; k < (int)(W * H / 9000f) + 2; k++)
            {
                float x = Rand(k, 1, seed + 50) * W, y = Rand(k, 2, seed + 50) * H;
                float ang = Rand(k, 3, seed + 50) * 6.283f;
                int len = 20 + (int)(Rand(k, 4, seed + 50) * 60f);
                for (int s = 0; s < len; s++)
                {
                    ang += (Rand(k, s, seed + 51) - 0.5f) * 0.7f;
                    x += (float)Math.Cos(ang);
                    y += (float)Math.Sin(ang);
                    if (!In((int)x, (int)y)) continue;
                    int i = (int)x + (int)y * W;
                    Ht[i] -= 2.5f;
                    Col[i] = Col[i] * 0.55f;
                }
            }
        }

        /// <summary>Madeira: veio comprido, nos e frestas entre tabuas.</summary>
        public void Wood(C3 dark, C3 light, int seed, float plank = 0f, bool vertical = false)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float u = vertical ? y : x, v = vertical ? x : y;
                    float warp = Fbm(u * 0.01f, v * 0.04f, seed, 3) * 8f;
                    float grain = (float)Math.Sin((v + warp) * 0.9f) * 0.5f + 0.5f;
                    float fine = Fbm(u * 0.04f, v * 0.8f, seed + 3, 2);
                    float t = grain * 0.35f + fine * 0.55f;
                    Col[i] = C3.Lerp(dark, light, Clamp01(t));
                    Ht[i] = grain * 0.4f + fine * 0.3f;
                    if (plank > 0f && ((int)v % (int)plank) == 0)
                    {
                        Col[i] = dark * 0.5f;
                        Ht[i] -= 2f;
                    }

                    Gloss[i] = 0.12f;
                }
        }

        /// <summary>Lousa: ardosia escura com a nevoa do giz apagado.</summary>
        public void Slate(C3 baseColor, int seed, float smudge = 0.6f)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float n = Fbm(x * 0.02f, y * 0.06f, seed, 4);
                    float wipe = Math.Max(0f, Fbm(x * 0.008f + y * 0.004f, y * 0.03f, seed + 3, 3) - 0.45f) * smudge;
                    Col[i] = C3.Lerp(baseColor * (0.9f + n * 0.25f), C3.Hex(0xb8bcb0), wipe * 0.5f);
                    Ht[i] = (Rand(x, y, seed + 5) - 0.5f) * 0.2f;
                    Gloss[i] = 0.04f;
                }
        }

        // ------------------------------------------------------------------
        // Pecas: rebites, pregos, mossas
        // ------------------------------------------------------------------

        /// <summary>Uma cabeca abaulada (rebite, prego, parafuso), com fenda se slot.</summary>
        public void Rivet(float cx, float cy, float r, C3 color, float gloss = 0.8f, bool slot = false, float slotAngle = 0.6f)
        {
            for (int y = (int)(cy - r - 2); y <= cy + r + 2; y++)
                for (int x = (int)(cx - r - 2); x <= cx + r + 2; x++)
                {
                    if (x < 0 || y < 0 || x >= W || y >= H) continue;
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    int i = x + y * W;
                    if (d <= r)
                    {
                        A[i] = 1f;
                        Col[i] = color;
                        Ht[i] = Math.Max(Ht[i], 2f + (float)Math.Sqrt(Math.Max(0f, r * r - d * d)) * 0.9f);
                        Gloss[i] = gloss;
                        if (slot)
                        {
                            float along = dx * (float)Math.Cos(slotAngle) + dy * (float)Math.Sin(slotAngle);
                            float across = -dx * (float)Math.Sin(slotAngle) + dy * (float)Math.Cos(slotAngle);
                            if (Math.Abs(across) < Math.Max(0.6f, r * 0.15f) && Math.Abs(along) < r * 0.8f)
                            {
                                Ht[i] -= r * 0.8f;
                                Col[i] = color * 0.5f;
                            }
                        }
                    }
                    else if (d <= r + 1.5f && In(x, y))
                    {
                        // A sombra de contato em volta da cabeca.
                        Col[i] = Col[i] * 0.7f;
                    }
                }
        }

        public void Dent(float cx, float cy, float r, float depth)
        {
            for (int y = (int)(cy - r); y <= cy + r; y++)
                for (int x = (int)(cx - r); x <= cx + r; x++)
                {
                    if (!In(x, y)) continue;
                    float d = (float)Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / r;
                    if (d < 1f) Ht[x + y * W] -= depth * (1f - d * d);
                }
        }

        /// <summary>Uma camada de cor por cima de uma forma, sem mexer na altura (tinta de parede, sujeira).</summary>
        public void Tint(Func<int, int, float> amount, C3 color)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float k = amount(x, y);
                    if (k > 0f) Col[i] = C3.Lerp(Col[i], color, Clamp01(k));
                }
        }

        // ------------------------------------------------------------------
        // Luz
        // ------------------------------------------------------------------

        /// <summary>
        /// Calcula a luz (de cima a esquerda) pela altura, com oclusao nas
        /// cavidades e brilho especular, e devolve o desenho com sombra
        /// projetada e contorno.
        /// </summary>
        public PixelCanvas Bake(float relief = 1f, bool shadow = true, bool outline = true, float ambientOcclusion = 1f)
        {
            float lx = -0.55f, ly = -0.65f, lz = 0.52f;
            float ln = (float)Math.Sqrt(lx * lx + ly * ly + lz * lz);
            lx /= ln;
            ly /= ln;
            lz /= ln;

            // Media local da altura, pra escurecer o fundo dos sulcos.
            float[] blur = BoxBlur(Ht, 3);

            var lit = new C3[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (A[i] <= 0f) continue;
                    float hl = x > 0 && A[i - 1] > 0f ? Ht[i - 1] : Ht[i];
                    float hr = x < W - 1 && A[i + 1] > 0f ? Ht[i + 1] : Ht[i];
                    float hu = y > 0 && A[i - W] > 0f ? Ht[i - W] : Ht[i];
                    float hd = y < H - 1 && A[i + W] > 0f ? Ht[i + W] : Ht[i];
                    float nx = (hl - hr) * 0.5f * relief, ny = (hu - hd) * 0.5f * relief, nz = 1f;
                    float n = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    nx /= n;
                    ny /= n;
                    nz /= n;
                    float diffuse = (nx * lx + ny * ly + nz * lz) / lz;
                    float shade = 0.35f + 0.65f * Math.Max(0f, diffuse);
                    float cavity = Math.Max(0f, blur[i] - Ht[i]) * 0.12f * ambientOcclusion;
                    shade *= 1f - Math.Min(0.55f, cavity);

                    // Especular: reflexo da luz pra quem olha de frente.
                    float rz = 2f * (nx * lx + ny * ly + nz * lz) * nz - lz;
                    float spec = (float)Math.Pow(Math.Max(0f, rz), 18f) * Gloss[i] * 0.9f;

                    C3 c = Col[i] * shade;
                    lit[i] = new C3(c.R + spec, c.G + spec * 0.97f, c.B + spec * 0.9f);
                }

            var canvas = new PixelCanvas(W, H);

            if (shadow)
            {
                // Sombra macia, deslocada pra baixo e pra direita.
                var sh = new float[W * H];
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        int sx = x - 3, sy = y - 5;
                        if (sx >= 0 && sy >= 0 && A[sx + sy * W] > 0f) sh[x + y * W] = 1f;
                    }

                sh = BoxBlur(BoxBlur(sh, 2), 2);
                for (int i = 0; i < sh.Length; i++)
                    if (sh[i] > 0.01f) canvas.Pixels[i] = new Rgba(8, 6, 10, (int)(sh[i] * 150f));
            }

            for (int i = 0; i < W * H; i++)
            {
                if (A[i] <= 0f) continue;
                C3 c = lit[i];
                canvas.Pixels[i] = new Rgba(To255(c.R), To255(c.G), To255(c.B), 255);
            }

            if (outline)
            {
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        int i = x + y * W;
                        if (A[i] > 0f) continue;
                        bool touch = (x > 0 && A[i - 1] > 0f) || (x < W - 1 && A[i + 1] > 0f) || (y > 0 && A[i - W] > 0f) || (y < H - 1 && A[i + W] > 0f);
                        if (touch) canvas.Pixels[i] = new Rgba(18, 14, 18, 255);
                    }
            }

            return canvas;
        }

        public float[] BoxBlur(float[] src, int r)
        {
            var tmp = new float[src.Length];
            var dst = new float[src.Length];
            for (int y = 0; y < H; y++)
            {
                float sum = 0f;
                int n = 0;
                for (int x = -r; x < W + r; x++)
                {
                    if (x + r < W)
                    {
                        sum += src[x + r + y * W];
                        n++;
                    }

                    if (x - r - 1 >= 0)
                    {
                        sum -= src[x - r - 1 + y * W];
                        n--;
                    }

                    if (x >= 0 && x < W) tmp[x + y * W] = sum / Math.Max(1, n);
                }
            }

            for (int x = 0; x < W; x++)
            {
                float sum = 0f;
                int n = 0;
                for (int y = -r; y < H + r; y++)
                {
                    if (y + r < H)
                    {
                        sum += tmp[x + (y + r) * W];
                        n++;
                    }

                    if (y - r - 1 >= 0)
                    {
                        sum -= tmp[x + (y - r - 1) * W];
                        n--;
                    }

                    if (y >= 0 && y < H) dst[x + y * W] = sum / Math.Max(1, n);
                }
            }

            return dst;
        }

        public static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
        private static int To255(float v) => (int)(Clamp01(v) * 255f + 0.5f);
    }
}
