using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// As ferramentas das carcacas dos chefes: pegar um quadro do proprio
    /// chefe, cortar em pedacos (cabeca, chamine, broca...), tombar cada um
    /// no chao, chamuscar a lataria, apagar o verde do Oxide, abrir frestas
    /// com brasa e jogar entulho em volta. Depois, Animate faz o loop da
    /// fumaca subindo e da brasa piscando.
    ///
    /// A carcaca nasce do desenho do chefe, entao se le na hora como ele.
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class WreckKit
    {
        public static readonly Rgba Soot = new Rgba(34, 28, 30);
        public static readonly Rgba EmberDeep = new Rgba(150, 40, 20);
        public static readonly Rgba Ember = new Rgba(236, 104, 36);
        public static readonly Rgba EmberHot = new Rgba(255, 196, 92);
        public static readonly Rgba SmokeLight = new Rgba(150, 144, 150);
        public static readonly Rgba SmokeDark = new Rgba(84, 78, 86);

        // ------------------------------------------------------------------
        // Cortar e tombar
        // ------------------------------------------------------------------

        /// <summary>Uma copia so com os pixels onde keep diz sim (e so os opacos: fumaca do quadro fica de fora).</summary>
        public static PixelCanvas Cut(PixelCanvas source, Func<int, int, bool> keep)
        {
            var piece = new PixelCanvas(source.Width, source.Height);
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Rgba color = source.Get(x, y);
                    if (color.A >= 200 && keep(x, y)) piece.Set(x, y, color);
                }
            }

            return piece;
        }

        /// <summary>
        /// Pinta piece girada (graus, horario na tela) em volta do ponto
        /// (pivotX, pivotY) dela, com esse ponto caindo em (atX, atY) no destino.
        /// </summary>
        public static void Place(PixelCanvas target, PixelCanvas piece, float degrees, float pivotX, float pivotY, float atX, float atY)
        {
            double a = degrees * Math.PI / 180.0;
            float cos = (float)Math.Cos(a);
            float sin = (float)Math.Sin(a);

            float reach = (float)Math.Sqrt(piece.Width * piece.Width + piece.Height * piece.Height);
            int x0 = (int)Math.Floor(atX - reach);
            int x1 = (int)Math.Ceiling(atX + reach);
            int y0 = (int)Math.Floor(atY - reach);
            int y1 = (int)Math.Ceiling(atY + reach);

            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    if (!target.Inside(x, y)) continue;

                    // Do destino de volta pra peca (rotacao inversa).
                    float dx = x + 0.5f - atX;
                    float dy = y + 0.5f - atY;
                    float sx = dx * cos + dy * sin + pivotX;
                    float sy = -dx * sin + dy * cos + pivotY;

                    Rgba color = piece.Get((int)Math.Floor(sx), (int)Math.Floor(sy));
                    if (color.A > 0) target.Blend(x, y, color);
                }
            }
        }

        // ------------------------------------------------------------------
        // A lataria morta
        // ------------------------------------------------------------------

        /// <summary>
        /// Apaga o que brilhava (olho, mostrador, Oxide) e chamusca: manchas
        /// de fuligem presas na peca e a cor perdendo um pouco da vida.
        /// </summary>
        public static void Scorch(PixelCanvas canvas, int seed, float soot = 0.35f, float drain = 0.25f)
        {
            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    Rgba c = canvas.Get(x, y);
                    if (c.A == 0) continue;

                    // O verde aceso (Oxide, olho, vidro) vira vidro apagado.
                    if (c.G > c.R + 50 && c.G > c.B + 10 && c.G > 120)
                    {
                        c = new Rgba(40 + c.G / 8, 58 + c.G / 6, 54 + c.G / 8, c.A);
                    }

                    int grey = (c.R * 3 + c.G * 5 + c.B * 2) / 10;
                    c = Rgba.Lerp(c, new Rgba(grey, grey, grey, c.A), drain);
                    c = c.Scaled(0.88f);

                    float n = ArtKit.Noise(x * 0.14f, y * 0.14f, seed);
                    if (n > 1f - soot) c = Rgba.Lerp(c, Soot.WithAlpha(c.A), Math.Min(0.8f, (n - (1f - soot)) * 3.2f + 0.25f));

                    canvas.Set(x, y, c);
                }
            }
        }

        /// <summary>
        /// Uma fresta com brasa: um caminho torto escuro com o miolo laranja.
        /// So pinta em cima do que ja existe. Devolve onde a brasa ficou
        /// (pra Animate fazer piscar).
        /// </summary>
        public static List<(int X, int Y)> EmberCrack(PixelCanvas canvas, float x, float y, float angle, int length, int seed)
        {
            var hot = new List<(int X, int Y)>();
            double a = angle * Math.PI / 180.0;

            for (int i = 0; i < length; i++)
            {
                int ix = (int)Math.Floor(x);
                int iy = (int)Math.Floor(y);

                if (canvas.Get(ix, iy).A > 200)
                {
                    bool glow = i % 4 != 3;
                    canvas.Set(ix, iy, glow ? (i % 3 == 0 ? EmberHot : Ember) : Soot);
                    if (glow) hot.Add((ix, iy));
                    if (canvas.Get(ix, iy + 1).A > 200) canvas.Set(ix, iy + 1, glow ? EmberDeep : Soot);
                }

                if (ArtKit.Hash(i, seed, 51) % 3 == 0) a += ArtKit.Hash(i, seed, 52) % 2 == 0 ? 0.7 : -0.7;
                x += (float)Math.Cos(a);
                y += (float)Math.Sin(a);
            }

            return hot;
        }

        /// <summary>Um furo de estilhaco: borda amassada clara, miolo escuro, brasa no fundo.</summary>
        public static void Hole(PixelCanvas canvas, float cx, float cy, float r, List<(int X, int Y)> hot)
        {
            for (int y = (int)(cy - r) - 2; y <= (int)(cy + r) + 2; y++)
            {
                for (int x = (int)(cx - r) - 2; x <= (int)(cx + r) + 2; x++)
                {
                    if (canvas.Get(x, y).A < 200) continue;

                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy) / r;

                    if (d <= 0.55f)
                    {
                        canvas.Set(x, y, dy > 0f ? EmberDeep : Soot);
                        if (dy > 0f && d < 0.4f) hot.Add((x, y));
                    }
                    else if (d <= 1f) canvas.Set(x, y, d < 0.8f ? new Rgba(16, 12, 14) : canvas.Get(x, y).Scaled(dx + dy < 0f ? 1.25f : 0.7f));
                }
            }
        }

        // ------------------------------------------------------------------
        // Entulho
        // ------------------------------------------------------------------

        /// <summary>Uma engrenagem solta: aro dentado, raios e o cubo.</summary>
        public static void Gear(PixelCanvas canvas, float cx, float cy, float r, Rgba[] ramp, int teeth = 8, float tilt = 0f)
        {
            for (int y = (int)(cy - r) - 3; y <= (int)(cy + r) + 3; y++)
            {
                for (int x = (int)(cx - r) - 3; x <= (int)(cx + r) + 3; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float ang = (float)Math.Atan2(dy, dx) + tilt;
                    bool tooth = Math.Cos(ang * teeth) > 0.3f;
                    float outer = r + (tooth ? 1.5f : 0f);
                    if (d > outer) continue;

                    bool rim = d > r * 0.62f;
                    bool spoke = Math.Abs(Math.Sin(ang * 2f)) < 0.3f;
                    bool hub = d < r * 0.3f;
                    if (!rim && !spoke && !hub) continue;
                    if (d < r * 0.12f) { canvas.Set(x, y, ramp[0]); continue; }

                    float light = -(dx + dy) / Math.Max(1f, d) * 0.6f + (rim ? 0.1f : -0.1f);
                    canvas.Set(x, y, ArtKit.Tone(ramp, light));
                }
            }
        }

        /// <summary>Uma chapa torta com rebites, girada.</summary>
        public static void Plate(PixelCanvas canvas, float cx, float cy, float w, float h, float degrees, Rgba[] ramp, int seed)
        {
            double a = degrees * Math.PI / 180.0;
            float cos = (float)Math.Cos(a);
            float sin = (float)Math.Sin(a);

            // Quatro cantos, um deles amassado pra dentro.
            float bite = 0.2f + (ArtKit.Hash(seed, 3, 7) % 3) * 0.12f;
            var local = new[] { (-w / 2, -h / 2), (w / 2, -h / 2), (w / 2, h / 2 - h * bite), (w / 2 - w * bite, h / 2), (-w / 2, h / 2) };
            var points = new (float X, float Y)[local.Length];
            for (int i = 0; i < local.Length; i++)
            {
                (float lx, float ly) = local[i];
                points[i] = (cx + lx * cos - ly * sin, cy + lx * sin + ly * cos);
            }

            ArtKit.Polygon(canvas, points, (x, y) =>
            {
                float lx = (x - cx) * cos + (y - cy) * sin;
                float ly = -(x - cx) * sin + (y - cy) * cos;
                bool edgeTop = ly < -h / 2 + 1.2f;
                bool edgeBottom = ly > h / 2 - 1.2f;
                if (edgeTop) return ramp[3];
                if (edgeBottom) return ramp[0];
                return ArtKit.Noise(lx * 0.3f, ly * 0.3f, seed) > 0.72f ? ramp[1] : ramp[2];
            });

            // Rebites nas pontas.
            foreach ((float lx, float ly) in new[] { (-w / 2 + 2f, -h / 2 + 2f), (w / 2 - 3f, -h / 2 + 2f) })
            {
                int rx = (int)Math.Floor(cx + lx * cos - ly * sin);
                int ry = (int)Math.Floor(cy + lx * sin + ly * cos);
                ArtKit.Rivet(canvas, rx, ry, ramp);
            }
        }

        /// <summary>Um pedaco de cano partido: o tubo e a boca escura numa ponta.</summary>
        public static void Pipe(PixelCanvas canvas, float x0, float y0, float x1, float y1, float half, Rgba[] ramp)
        {
            ArtKit.Limb(canvas, x0, y0, x1, y1, half, ramp);
            ArtKit.Ball(canvas, x1, y1, Math.Max(0.8f, half * 0.8f), Math.Max(0.8f, half * 0.8f), ArtKit.Plain(new[] { Soot, Soot, ramp[0], ramp[0] }));
        }

        /// <summary>Um parafuso solto, 3x2.</summary>
        public static void Bolt(PixelCanvas canvas, int x, int y, Rgba[] ramp)
        {
            canvas.Set(x, y, ramp[3]);
            canvas.Set(x + 1, y, ramp[2]);
            canvas.Set(x + 2, y, ramp[1]);
            canvas.Set(x, y + 1, ramp[1]);
            canvas.Set(x + 1, y + 1, ramp[0]);
        }

        /// <summary>
        /// Um monte de entulho no chao, de x0 a x1, com o pe em groundY:
        /// um monte baixo de cacos escuros e, por cima, chapas, engrenagens,
        /// canos e parafusos nas cores do chefe.
        /// </summary>
        public static void Rubble(PixelCanvas canvas, int x0, int x1, int groundY, int height, Rgba[][] ramps, int seed, float scale = 1f)
        {
            // O monte: um morrinho de cacos.
            float mid = (x0 + x1) * 0.5f;
            float half = (x1 - x0) * 0.5f;
            for (int x = x0; x <= x1; x++)
            {
                float k = 1f - Math.Abs(x + 0.5f - mid) / half;
                int top = groundY - (int)Math.Round(height * (float)Math.Sqrt(Math.Max(0f, k)) + (ArtKit.Hash(x, seed, 9) % 3) - 1);
                for (int y = top; y <= groundY; y++)
                {
                    Rgba[] ramp = ramps[ArtKit.Hash(x / 3, y / 3, seed) % ramps.Length];
                    int h = ArtKit.Hash(x, y, seed + 1) % 7;
                    Rgba c = y == top ? ramp[2] : h == 0 ? ramp[2] : h < 4 ? ramp[1] : ramp[0];
                    canvas.Set(x, y, c);
                }
            }

            int span = x1 - x0;
            int count = Math.Max(3, span / 9);

            for (int i = 0; i < count; i++)
            {
                Rgba[] ramp = ramps[ArtKit.Hash(i, seed, 2) % ramps.Length];
                float x = x0 + 3 + ArtKit.Hash(i, seed, 3) % Math.Max(1, span - 6);
                float k = 1f - Math.Abs(x - mid) / half;
                float y = groundY - height * (float)Math.Sqrt(Math.Max(0f, k)) + 1f;

                switch (ArtKit.Hash(i, seed, 4) % 5)
                {
                    case 0:
                        Gear(canvas, x, y - 2f * scale, (3f + ArtKit.Hash(i, seed, 5) % 3) * scale, ramp, 6 + ArtKit.Hash(i, seed, 6) % 4, i);
                        break;
                    case 1:
                    case 2:
                        Plate(canvas, x, y - 1f, (8f + ArtKit.Hash(i, seed, 7) % 6) * scale, (4f + ArtKit.Hash(i, seed, 8) % 3) * scale,
                            -35f + ArtKit.Hash(i, seed, 9) % 70, ramp, seed + i);
                        break;
                    case 3:
                        float len = (6f + ArtKit.Hash(i, seed, 10) % 6) * scale;
                        float ang = (-20f + ArtKit.Hash(i, seed, 11) % 40) * (float)Math.PI / 180f;
                        Pipe(canvas, x - (float)Math.Cos(ang) * len / 2, y - 1f - (float)Math.Sin(ang) * len / 2,
                            x + (float)Math.Cos(ang) * len / 2, y - 1f + (float)Math.Sin(ang) * len / 2, 1.4f * scale, ramp);
                        break;
                    default:
                        Bolt(canvas, (int)x, (int)y - 1, ramp);
                        Bolt(canvas, (int)x + 4, (int)y, ramp);
                        break;
                }
            }
        }

        /// <summary>Parafusos e cacos espalhados pelo chao, longe do monte.</summary>
        public static void Scatter(PixelCanvas canvas, int x0, int x1, int groundY, Rgba[][] ramps, int seed, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Rgba[] ramp = ramps[ArtKit.Hash(i, seed, 21) % ramps.Length];
                int x = x0 + ArtKit.Hash(i, seed, 22) % Math.Max(1, x1 - x0);
                if (ArtKit.Hash(i, seed, 23) % 2 == 0) Bolt(canvas, x, groundY - 1, ramp);
                else
                {
                    canvas.Set(x, groundY, ramp[1]);
                    canvas.Set(x + 1, groundY, ramp[0]);
                    canvas.Set(x, groundY - 1, ramp[2]);
                }
            }
        }

        // ------------------------------------------------------------------
        // O loop: fumaca, brasa e faisca
        // ------------------------------------------------------------------

        /// <summary>De onde sai fumaca: o ponto, quanto sobe e o tamanho da baforada.</summary>
        public readonly struct Vent
        {
            public readonly float X;
            public readonly float Y;
            public readonly float Rise;
            public readonly float Size;
            public readonly Rgba Light;
            public readonly Rgba Dark;

            public Vent(float x, float y, float rise, float size, Rgba? light = null, Rgba? dark = null)
            {
                X = x;
                Y = y;
                Rise = rise;
                Size = size;
                Light = light ?? SmokeLight;
                Dark = dark ?? SmokeDark;
            }
        }

        /// <summary>
        /// Os quadros do loop: a carcaca parada, a fumaca subindo de cada
        /// respiro (cada baforada nasce pequena, cresce e some, e o ciclo fecha
        /// sem pulo), a brasa das frestas acendendo e apagando e, de vez em
        /// quando, uma faisca.
        /// </summary>
        public static PixelCanvas[] Animate(PixelCanvas still, Vent[] vents, List<(int X, int Y)> embers, int frames, int seed, int puffs = 4, int strength = 170)
        {
            var result = new PixelCanvas[frames];

            for (int f = 0; f < frames; f++)
            {
                var canvas = new PixelCanvas(still.Width, still.Height);
                canvas.Blit(still, 0, 0);

                // A brasa: cada ponto respira no proprio ritmo.
                if (embers != null)
                {
                    foreach ((int x, int y) in embers)
                    {
                        int phase = (ArtKit.Hash(x, y, seed) % frames + f) % frames;
                        float k = (float)(0.5 + 0.5 * Math.Sin(phase * Math.PI * 2 / frames));
                        canvas.Set(x, y, k > 0.66f ? EmberHot : k > 0.25f ? Ember : EmberDeep);
                    }

                    // Uma faisca pulando de uma fresta.
                    if (embers.Count > 0)
                    {
                        (int sx, int sy) = embers[ArtKit.Hash(f / 2, seed, 77) % embers.Count];
                        int step = f % 2;
                        canvas.Set(sx + step, sy - 2 - step * 2, EmberHot);
                        canvas.Set(sx + step + 1, sy - 3 - step * 2, Ember.WithAlpha(180));
                    }
                }

                if (vents != null)
                {
                    for (int v = 0; v < vents.Length; v++)
                    {
                        Vent vent = vents[v];
                        for (int p = 0; p < puffs; p++)
                        {
                            float t = ((float)f / frames + (float)p / puffs + v * 0.37f) % 1f;
                            float drift = (float)Math.Sin((t * 2.2f + v) * Math.PI) * vent.Size * 0.6f + t * vent.Size * 1.2f;
                            float x = vent.X + drift;
                            float y = vent.Y - t * vent.Rise;
                            float r = vent.Size * (0.45f + t * 0.9f);
                            int alpha = (int)(strength * Math.Min(1f, t * 5f) * (1f - t));
                            if (alpha <= 10) continue;

                            // Uma nuvem de tres bolotas, nao uma bola: os lobulos giram com o tempo.
                            float spin = (p * 2.1f + v) + t * 3f;
                            ArtKit.Puff(canvas, x + (float)Math.Cos(spin) * r * 0.6f, y + r * 0.2f, r * 0.7f, vent.Light, vent.Dark, alpha * 3 / 4);
                            ArtKit.Puff(canvas, x - (float)Math.Cos(spin) * r * 0.5f, y + r * 0.1f, r * 0.6f, vent.Light, vent.Dark, alpha * 3 / 4);
                            ArtKit.Puff(canvas, x, y - r * 0.2f, r * 0.8f, vent.Light, vent.Dark, alpha);
                        }
                    }
                }

                result[f] = canvas;
            }

            return result;
        }
    }
}
