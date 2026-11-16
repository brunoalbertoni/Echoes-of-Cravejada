using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Uma cor em ponto flutuante (0..1), pra misturar luz sem perder degrau.</summary>
    public struct PCol
    {
        public float R, G, B;

        public PCol(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static PCol Hex(int rgb) => new PCol(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
        public static PCol Lerp(PCol a, PCol b, float t) => new PCol(a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t);
        public static PCol operator *(PCol a, float k) => new PCol(a.R * k, a.G * k, a.B * k);
        public static PCol operator *(PCol a, PCol b) => new PCol(a.R * b.R, a.G * b.G, a.B * b.B);
        public static PCol operator +(PCol a, PCol b) => new PCol(a.R + b.R, a.G + b.G, a.B + b.B);

        public Rgba ToRgba(int alpha = 255) => new Rgba((int)Math.Round(Clamp(R) * 255f), (int)Math.Round(Clamp(G) * 255f), (int)Math.Round(Clamp(B) * 255f), alpha);

        private static float Clamp(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }

    /// <summary>
    /// O relevo de um retrato: cada peca e pintada com cor, altura e brilho
    /// (cupula, caixa chanfrada, cilindro, anel, poligono), por cima das
    /// anteriores, e so no fim a luz e calculada pela altura (<see cref="Bake"/>):
    /// por isso um rebite tem volume, uma placa por cima de outra faz sombra
    /// e o metal brilha onde vira pra luz.
    ///
    /// A luz vem de cima e da esquerda, como em toda a arte do jogo; o retrato
    /// olha pra direita, pro texto, e ganha um contraluz na borda direita.
    /// A luz e quantizada em degraus, pra continuar pixel art.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public sealed class PortraitRelief
    {
        public readonly int W, H;

        private readonly PCol[] _col;
        private readonly float[] _ht;
        private readonly float[] _gloss;
        private readonly bool[] _on;
        private readonly bool[] _emit;
        private readonly bool[] _seam;
        private readonly int[] _part;
        private int _parts;

        private readonly List<(float X, float Y, float R, PCol Color, float Strength)> _glows =
            new List<(float, float, float, PCol, float)>();

        /// <summary>A cor do contraluz na borda direita.</summary>
        public PCol Rim = PCol.Hex(0x9fd8d0);

        /// <summary>Quanto do contraluz entra (0 desliga).</summary>
        public float RimStrength = 0.42f;

        public PortraitRelief(int w, int h)
        {
            W = w;
            H = h;
            _col = new PCol[w * h];
            _ht = new float[w * h];
            _gloss = new float[w * h];
            _on = new bool[w * h];
            _emit = new bool[w * h];
            _seam = new bool[w * h];
            _part = new int[w * h];
        }

        public bool On(int x, int y) => x >= 0 && y >= 0 && x < W && y < H && _on[x + y * W];
        public float Height(int x, int y) => On(x, y) ? _ht[x + y * W] : 0f;

        // ------------------------------------------------------------------
        // Pecas
        // ------------------------------------------------------------------

        private void Put(int x, int y, float h, PCol c, float gloss, bool emit = false)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            int i = x + y * W;
            _on[i] = true;
            _ht[i] = h;
            _col[i] = c;
            _gloss[i] = gloss;
            _emit[i] = emit;
            _seam[i] = false;
            _part[i] = _parts;
        }

        private static float Smooth(float t)
        {
            t = t < 0f ? 0f : (t > 1f ? 1f : t);
            return t * t * (3f - 2f * t);
        }

        /// <summary>Uma cupula eliptica: altura z na borda, z + bulge no meio.</summary>
        public void Dome(float cx, float cy, float rx, float ry, float z, float bulge, PCol c, float gloss = 0.5f, bool emit = false)
        {
            _parts++;
            for (int y = (int)Math.Floor(cy - ry) - 1; y <= (int)Math.Ceiling(cy + ry) + 1; y++)
                for (int x = (int)Math.Floor(cx - rx) - 1; x <= (int)Math.Ceiling(cx + rx) + 1; x++)
                {
                    float dx = (x + 0.5f - cx) / rx;
                    float dy = (y + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;
                    Put(x, y, z + bulge * (float)Math.Sqrt(1f - d), c, gloss, emit);
                }
        }

        /// <summary>
        /// Uma caixa de cantos redondos, chanfrada: sobe bevel nas bordas
        /// (largura bevelWidth) e pode ser abaulada na horizontal (pillow).
        /// </summary>
        public void Box(float x0, float y0, float x1, float y1, float radius, float z, float bevel, PCol c,
            float gloss = 0.5f, float pillow = 0f, float bevelWidth = 3f, bool emit = false)
        {
            _parts++;
            float cx = (x0 + x1) * 0.5f, cy = (y0 + y1) * 0.5f;
            float hw = (x1 - x0) * 0.5f, hh = (y1 - y0) * 0.5f;
            radius = Math.Min(radius, Math.Min(hw, hh));

            for (int y = (int)Math.Floor(y0); y <= (int)Math.Ceiling(y1); y++)
                for (int x = (int)Math.Floor(x0); x <= (int)Math.Ceiling(x1); x++)
                {
                    float px = Math.Abs(x + 0.5f - cx) - (hw - radius);
                    float py = Math.Abs(y + 0.5f - cy) - (hh - radius);
                    float ox = Math.Max(px, 0f), oy = Math.Max(py, 0f);
                    float sdf = (float)Math.Sqrt(ox * ox + oy * oy) + Math.Min(Math.Max(px, py), 0f) - radius;
                    if (sdf > 0f) continue;

                    float h = z + bevel * Smooth(-sdf / bevelWidth);
                    if (pillow != 0f)
                    {
                        float u = (x + 0.5f - cx) / hw;
                        h += pillow * (float)Math.Sqrt(Math.Max(0f, 1f - u * u));
                    }

                    Put(x, y, h, c, gloss, emit);
                }
        }

        /// <summary>Um cilindro (ou cone) deitado de um ponto a outro, de pontas redondas.</summary>
        public void Tube(float x0, float y0, float x1, float y1, float r0, float r1, float z, float bulge, PCol c,
            float gloss = 0.5f, bool emit = false)
        {
            _parts++;
            float dx = x1 - x0, dy = y1 - y0;
            float len2 = Math.Max(0.0001f, dx * dx + dy * dy);
            float most = Math.Max(r0, r1);

            for (int y = (int)Math.Floor(Math.Min(y0, y1) - most) - 1; y <= (int)Math.Ceiling(Math.Max(y0, y1) + most) + 1; y++)
                for (int x = (int)Math.Floor(Math.Min(x0, x1) - most) - 1; x <= (int)Math.Ceiling(Math.Max(x0, x1) + most) + 1; x++)
                {
                    float rx = x + 0.5f - x0, ry = y + 0.5f - y0;
                    float t = Math.Max(0f, Math.Min(1f, (rx * dx + ry * dy) / len2));
                    float qx = rx - dx * t, qy = ry - dy * t;
                    float r = r0 + (r1 - r0) * t;
                    float d = (float)Math.Sqrt(qx * qx + qy * qy) / Math.Max(0.35f, r);
                    if (d > 1f) continue;
                    Put(x, y, z + bulge * (float)Math.Sqrt(1f - d * d), c, gloss, emit);
                }
        }

        /// <summary>Um anel (aro de lente, arruela): abaulado no meio da espessura.</summary>
        public void Ring(float cx, float cy, float rIn, float rOut, float z, float bulge, PCol c, float gloss = 0.6f, float squash = 1f)
        {
            _parts++;
            float mid = (rIn + rOut) * 0.5f, half = (rOut - rIn) * 0.5f;
            for (int y = (int)Math.Floor(cy - rOut * squash) - 1; y <= (int)Math.Ceiling(cy + rOut * squash) + 1; y++)
                for (int x = (int)Math.Floor(cx - rOut) - 1; x <= (int)Math.Ceiling(cx + rOut) + 1; x++)
                {
                    float dx = x + 0.5f - cx, dy = (y + 0.5f - cy) / squash;
                    float t = ((float)Math.Sqrt(dx * dx + dy * dy) - mid) / half;
                    if (Math.Abs(t) > 1f) continue;
                    Put(x, y, z + bulge * (float)Math.Sqrt(1f - t * t), c, gloss);
                }
        }

        /// <summary>Um poligono chanfrado: sobe bevel ate bevelWidth pra dentro da borda.</summary>
        public void Poly((float X, float Y)[] pts, float z, float bevel, PCol c, float gloss = 0.5f, float bevelWidth = 3f,
            bool emit = false)
        {
            _parts++;
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            foreach ((float px, float py) in pts)
            {
                minX = Math.Min(minX, px);
                maxX = Math.Max(maxX, px);
                minY = Math.Min(minY, py);
                maxY = Math.Max(maxY, py);
            }

            for (int y = (int)Math.Floor(minY); y <= (int)Math.Ceiling(maxY); y++)
                for (int x = (int)Math.Floor(minX); x <= (int)Math.Ceiling(maxX); x++)
                {
                    float fx = x + 0.5f, fy = y + 0.5f;
                    if (!ArtKit.InsidePolygon(pts, fx, fy)) continue;
                    float e = float.MaxValue;
                    for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
                        e = Math.Min(e, SegDist(fx, fy, pts[j].X, pts[j].Y, pts[i].X, pts[i].Y));
                    Put(x, y, z + bevel * Smooth(e / bevelWidth), c, gloss, emit);
                }
        }

        private static float SegDist(float px, float py, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax, dy = by - ay;
            float t = Math.Max(0f, Math.Min(1f, ((px - ax) * dx + (py - ay) * dy) / Math.Max(0.0001f, dx * dx + dy * dy)));
            float qx = px - ax - dx * t, qy = py - ay - dy * t;
            return (float)Math.Sqrt(qx * qx + qy * qy);
        }

        /// <summary>Um rebite: cupulinha por cima do que ja esta ali.</summary>
        public void Rivet(float cx, float cy, float r, PCol c, float gloss = 0.8f)
        {
            float z = Height((int)cx, (int)cy);
            Dome(cx, cy, r, r, z, r * 0.9f + 0.6f, c, gloss);
        }

        /// <summary>Uma fileira de rebites de a ate b.</summary>
        public void Rivets(float x0, float y0, float x1, float y1, int count, float r, PCol c)
        {
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                Rivet(x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, r, c);
            }
        }

        // ------------------------------------------------------------------
        // Marcas por cima (nao criam peca)
        // ------------------------------------------------------------------

        /// <summary>Uma emenda de chapa: um sulco de um pixel, que vira traco escuro.</summary>
        public void Seam(float x0, float y0, float x1, float y1, float depth = 1.2f)
        {
            int steps = (int)Math.Ceiling(Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0))) + 1;
            for (int s = 0; s <= steps; s++)
            {
                float t = s / (float)steps;
                int x = (int)Math.Floor(x0 + (x1 - x0) * t), y = (int)Math.Floor(y0 + (y1 - y0) * t);
                if (!On(x, y)) continue;
                int i = x + y * W;
                _ht[i] -= depth;
                _seam[i] = true;
            }
        }

        /// <summary>Um amassado.</summary>
        public void Dent(float cx, float cy, float r, float depth)
        {
            for (int y = (int)(cy - r) - 1; y <= (int)(cy + r) + 1; y++)
                for (int x = (int)(cx - r) - 1; x <= (int)(cx + r) + 1; x++)
                {
                    if (!On(x, y)) continue;
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    float d = (dx * dx + dy * dy) / (r * r);
                    if (d > 1f) continue;
                    _ht[x + y * W] -= depth * (1f - d);
                }
        }

        /// <summary>Riscos claros e finos (a prata arranhada).</summary>
        public void Scratches(float x0, float y0, float x1, float y1, int count, int seed, PCol color, float strength = 0.45f)
        {
            for (int k = 0; k < count; k++)
            {
                float sx = x0 + (x1 - x0) * Rand(k, 1, seed);
                float sy = y0 + (y1 - y0) * Rand(k, 2, seed);
                float a = (float)(Rand(k, 3, seed) * Math.PI);
                int len = 3 + (int)(Rand(k, 4, seed) * 7f);
                for (int s = 0; s < len; s++)
                {
                    int x = (int)(sx + Math.Cos(a) * s), y = (int)(sy + Math.Sin(a) * s);
                    if (!On(x, y) || _emit[x + y * W]) continue;
                    int i = x + y * W;
                    _col[i] = PCol.Lerp(_col[i], color, strength);
                }
            }
        }

        /// <summary>
        /// Azinhavre (o Oxide comendo o metal): manchas pelo ruido, dentro da
        /// area, foscas e um pouco altas, como crosta.
        /// </summary>
        public void Patina(float x0, float y0, float x1, float y1, float amount, int seed, PCol crust, float scale = 0.12f)
        {
            for (int y = (int)y0; y <= (int)y1; y++)
                for (int x = (int)x0; x <= (int)x1; x++)
                {
                    if (!On(x, y)) continue;
                    int i = x + y * W;
                    if (_emit[i]) continue;
                    float n = ArtKit.Noise(x * scale, y * scale, seed) * 0.7f + ArtKit.Noise(x * scale * 3f, y * scale * 3f, seed + 9) * 0.3f;
                    if (n < 1f - amount) continue;
                    float k = Math.Min(1f, (n - (1f - amount)) * 6f);
                    _col[i] = PCol.Lerp(_col[i], crust, 0.55f + 0.45f * k);
                    _gloss[i] *= 0.2f;
                    _ht[i] += 0.5f * k;
                }
        }

        /// <summary>Pintas soltas (ferrugem, fuligem, poeira).</summary>
        public void Speckle(float x0, float y0, float x1, float y1, int per1000, int seed, PCol color, float strength = 0.6f)
        {
            for (int y = (int)y0; y <= (int)y1; y++)
                for (int x = (int)x0; x <= (int)x1; x++)
                {
                    if (!On(x, y) || _emit[x + y * W]) continue;
                    if (ArtKit.Hash(x, y, seed) % 1000 >= per1000) continue;
                    int i = x + y * W;
                    _col[i] = PCol.Lerp(_col[i], color, strength);
                }
        }

        /// <summary>Pinta por cima (faixa, mancha), sem mexer na altura.</summary>
        public void Tint(Func<int, int, float> amount, PCol color)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    if (!On(x, y)) continue;
                    float k = amount(x, y);
                    if (k <= 0f) continue;
                    int i = x + y * W;
                    _col[i] = PCol.Lerp(_col[i], color, Math.Min(1f, k));
                }
        }

        /// <summary>Apaga (pra recortar um buraco ou um entalhe).</summary>
        public void Erase(Func<int, int, bool> where)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (where(x, y)) _on[x + y * W] = false;
        }

        /// <summary>Um brilho em volta (olho, cristal, lampada), aplicado depois da luz.</summary>
        public void Glow(float cx, float cy, float r, PCol color, float strength)
        {
            _glows.Add((cx, cy, r, color, strength));
        }

        public static float Rand(int x, int y, int seed) => (ArtKit.Hash(x, y, seed) & 0xffff) / 65535f;

        // ------------------------------------------------------------------
        // Luz
        // ------------------------------------------------------------------

        private static readonly float[] Bayer = { 0f, 0.5f, 0.75f, 0.25f };

        /// <summary>
        /// Calcula a luz pela altura e devolve o retrato: degraus de luz com
        /// sombra puxada pro frio, brilho de metal, cavidades escurecidas,
        /// traco escuro onde uma peca passa por cima da outra, contraluz a
        /// direita, brilhos e o contorno.
        /// </summary>
        public PixelCanvas Bake(int levels = 6)
        {
            var canvas = new PixelCanvas(W, H);

            float lx = -0.55f, ly = -0.68f, lz = 0.62f;
            float ln = (float)Math.Sqrt(lx * lx + ly * ly + lz * lz);
            lx /= ln; ly /= ln; lz /= ln;
            float hx = lx, hy = ly, hz = lz + 1f;
            float hn = (float)Math.Sqrt(hx * hx + hy * hy + hz * hz);
            hx /= hn; hy /= hn; hz /= hn;

            float[] blur = BlurHeight(5);

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = x + y * W;
                    if (!_on[i]) continue;

                    PCol albedo = _col[i];
                    if (_emit[i])
                    {
                        canvas.Set(x, y, albedo.ToRgba());
                        continue;
                    }

                    float h = _ht[i];
                    float hl = Sample(x - 1, y, h), hr = Sample(x + 1, y, h);
                    float hu = Sample(x, y - 1, h), hd = Sample(x, y + 1, h);
                    float nx = -(hr - hl) * 0.5f, ny = -(hd - hu) * 0.5f, nz = 1f;
                    float nn = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    nx /= nn; ny /= nn; nz /= nn;

                    float diff = nx * lx + ny * ly + nz * lz;
                    float cav = Math.Max(0f, Math.Min(1f, (blur[i] - h) / 7f));
                    float v = 0.30f + 0.92f * Math.Max(0f, diff) - 0.42f * cav;

                    // Degraus, com um pontilhado leve na passagem de um pro outro.
                    float dither = (Bayer[(x & 1) + (y & 1) * 2] - 0.375f) * 0.22f;
                    float q = (float)Math.Round(v * (levels - 1) + dither) / (levels - 1);
                    q = Math.Max(0.12f, Math.Min(1.28f, q * 1.08f));

                    PCol c;
                    if (q < 1f)
                    {
                        c = albedo * q;
                        // A sombra puxa pro frio (violeta), como na arte do jogo.
                        float cool = (1f - q) * 0.55f;
                        c = PCol.Lerp(c, new PCol(c.R * 0.72f + 0.02f, c.G * 0.78f + 0.02f, c.B * 1.05f + 0.07f), cool);
                    }
                    else
                    {
                        c = PCol.Lerp(albedo, albedo * 1.15f + new PCol(0.10f, 0.08f, 0.03f), (q - 1f) / 0.28f);
                    }

                    float spec = (float)Math.Pow(Math.Max(0f, nx * hx + ny * hy + nz * hz), 28f) * _gloss[i];
                    if (spec > 0.42f) c = PCol.Lerp(c, new PCol(1f, 0.98f, 0.92f), 0.78f);
                    else if (spec > 0.16f) c = PCol.Lerp(c, new PCol(1f, 0.97f, 0.88f), 0.36f);

                    // Traco onde a peca vizinha esta por cima (ou na emenda).
                    if (_seam[i] || Over(x, y, i, x - 1, y) || Over(x, y, i, x + 1, y) || Over(x, y, i, x, y - 1) || Over(x, y, i, x, y + 1))
                    {
                        c = c * 0.42f + new PCol(0.03f, 0.02f, 0.05f);
                    }

                    // O contraluz: a borda direita pega a luz fria da caixa de texto.
                    if (RimStrength > 0f && (!On(x + 1, y) || !On(x + 2, y)) && nx > -0.2f)
                    {
                        c = PCol.Lerp(c, Rim, RimStrength * (On(x + 1, y) ? 0.55f : 1f));
                    }

                    canvas.Set(x, y, c.ToRgba());
                }

            foreach ((float gx, float gy, float gr, PCol gc, float gs) in _glows)
            {
                ArtKit.Glow(canvas, gx, gy, gr, gc.ToRgba(), gs, true);
            }

            ArtKit.AddOutline(canvas);
            return canvas;
        }

        private bool Over(int x, int y, int i, int nx, int ny)
        {
            if (!On(nx, ny)) return false;
            int j = nx + ny * W;
            if (_part[j] == _part[i] || _emit[j]) return false;
            return _ht[j] - _ht[i] > 1.4f;
        }

        private float Sample(int x, int y, float fallback)
        {
            return On(x, y) ? _ht[x + y * W] : fallback - 2.5f;
        }

        private float[] BlurHeight(int r)
        {
            var src = new float[W * H];
            for (int i = 0; i < src.Length; i++) src[i] = _on[i] ? _ht[i] : 0f;

            var tmp = new float[W * H];
            var dst = new float[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float s = 0f;
                    int n = 0;
                    for (int k = -r; k <= r; k++)
                    {
                        int xx = Math.Max(0, Math.Min(W - 1, x + k));
                        s += src[xx + y * W];
                        n++;
                    }

                    tmp[x + y * W] = s / n;
                }

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float s = 0f;
                    int n = 0;
                    for (int k = -r; k <= r; k++)
                    {
                        int yy = Math.Max(0, Math.Min(H - 1, y + k));
                        s += tmp[x + yy * W];
                        n++;
                    }

                    dst[x + y * W] = s / n;
                }

            return dst;
        }
    }
}
