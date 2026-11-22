using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A Gosma de Mercurio: uma bola gorda de mercurio vivo, espelhada — o
    /// laboratorio inteiro reflete nela. Dentro, uma engrenagem de latao gira;
    /// um tubo de ensaio quebrado ficou cravado no alto; boiando na frente,
    /// dois olhos de vidro com a iris verde do Oxide. Rasteja como lagarta:
    /// junta a de tras e estica a da frente. Morrendo, se esparrama numa poca
    /// e os olhos rolam pro chao. 48x36, pivo na base, no meio.
    /// </summary>
    public static class SlimeArt
    {
        public const int Width = 48;
        public const int Height = 36;
        public const int PixelsPerUnit = 32;
        public const float PivotY = 1f / Height;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Crawl = 4;
        public const int CrawlFrames = 6;
        public const int Hurt = 10;
        public const int Death = 11;
        public const int DeathFrames = 6;
        public const int FrameCount = 17;

        private const float Ground = 34f;

        private sealed class Pose
        {
            public float Squash;
            public float Lean;
            public float Gear;
            public float EyeY;
            public bool Squint;
            public float Puddle;
            public int Drops;
            public int Seed;
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            float[] wobble = { 0f, 0.05f, 0.1f, 0.05f };
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(new Pose { Squash = wobble[i], Gear = i * 15f, EyeY = i == 2 ? 1f : 0f });
            }

            // Rastejando: junta a de tras (alta e curta), estica a da frente (baixa e comprida).
            float[] squash = { -0.12f, -0.05f, 0.08f, 0.18f, 0.12f, 0f };
            float[] lean = { -1f, -0.4f, 0.4f, 1f, 0.6f, 0f };
            for (int i = 0; i < CrawlFrames; i++)
            {
                frames[Crawl + i] = Draw(new Pose { Squash = squash[i], Lean = lean[i], Gear = i * 25f, EyeY = -squash[i] * 6f, Drops = i % 3 == 0 ? 1 : 0, Seed = i });
            }

            frames[Hurt] = Draw(new Pose { Squash = 0.22f, Squint = true, Drops = 4, Seed = 9 });

            for (int i = 0; i < DeathFrames; i++)
            {
                frames[Death + i] = Draw(new Pose { Puddle = (i + 1) / (float)DeathFrames, Squint = true, Drops = i < 3 ? 5 : 0, Seed = 20 + i, Gear = 90f });
            }

            return frames;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var c = new PixelCanvas(Width, Height);
            float k = pose.Puddle;
            float cx = 24f + pose.Lean * 2f;
            float rx = 13f * (1f + pose.Squash) + k * 8f;
            float ry = Math.Max(2f, 11f * (1f - pose.Squash) * (1f - k * 0.85f));
            float cy = Ground - ry;

            ArtKit.Skin mirror = (x, y, light) =>
            {
                // O laboratorio refletido: a faixa escura do horizonte e a janela clara.
                if (y > -ry * 0.05f && y < ry * 0.2f) light -= 0.45f;
                if (x < -rx * 0.25f && x > -rx * 0.55f && y < -ry * 0.35f && y > -ry * 0.7f) return D4.Mercury[3];
                return ArtKit.Tone(D4.Mercury, light);
            };

            // As duas metades da lagarta e o corpo.
            if (k < 0.5f)
            {
                ArtKit.Ball(c, cx - rx * 0.6f - pose.Lean * 2f, Ground - ry * 0.4f, rx * 0.45f, ry * 0.42f, mirror, 0f);
                ArtKit.Ball(c, cx + rx * 0.55f + pose.Lean * 3f, Ground - ry * 0.45f, rx * 0.5f, ry * 0.5f, mirror, 0.1f);
            }

            ArtKit.Ball(c, cx, cy, rx, ry, mirror, 0.1f);

            // A base, espalhada no chao.
            int x0 = (int)(cx - rx - 2f - k * 4f);
            int x1 = (int)(cx + rx + 2f + k * 4f);
            for (int x = x0; x <= x1; x++)
            {
                c.Set(x, (int)Ground, D4.Mercury[0]);
                c.Set(x, (int)Ground - 1, D4.Mercury[1]);
            }

            // A engrenagem la dentro, vista pelo metal liquido.
            float gx = cx - 3f - k * 6f;
            float gy = k > 0.6f ? Ground - 3f : cy + 2f;
            DrawGear(c, gx, gy, 4f, pose.Gear, k > 0.6f ? 255 : 170);

            // O tubo de ensaio quebrado, cravado no alto.
            if (k < 0.4f)
            {
                float tx = cx - 6f;
                float ty = cy - ry + 3f;
                ArtKit.Taper(c, tx, ty, tx - 4f, ty - 8f, 2f, 1.8f, D4.Glass, 0.2f);
                c.Set((int)tx - 4, (int)ty - 9, D4.Glass[3]);
                c.Set((int)tx - 3, (int)ty - 2, LabArt.Rose);
                c.Set((int)tx - 2, (int)ty - 1, LabArt.Rose);
            }

            // O veio verde do Oxide.
            ArtKit.Crack(c, cx - 8f, cy + 3f, 20f, 7, 7101, 0.8f);

            // Os olhos de vidro: boiando na frente; morta, rolaram pro chao.
            if (k < 0.5f)
            {
                DrawEye(c, cx + 5f, cy - 3f + pose.EyeY, pose.Squint);
                DrawEye(c, cx + 10.5f, cy - 1.5f + pose.EyeY, pose.Squint);
            }
            else
            {
                DrawEye(c, 6f, Ground - 2.5f, true);
                DrawEye(c, 42f, Ground - 2.5f, true);
            }

            ArtKit.AddOutline(c);

            for (int i = 0; i < pose.Drops; i++)
            {
                float dx = (ArtKit.Hash(i, pose.Seed, 7102) % 30) - 15f;
                float dy = 6f + ArtKit.Hash(i, pose.Seed, 7103) % 10;
                ArtKit.Ball(c, cx + dx, cy - dy * (1f - k), 1.4f, 1.4f, D4.Mercury, 0.4f);
            }

            if (k < 0.5f)
            {
                ArtKit.Glow(c, cx + 5f, cy - 3f, 4f, LabArt.OxideGreen, 0.3f, true);
                ArtKit.Glow(c, cx + 10.5f, cy - 1.5f, 4f, LabArt.OxideGreen, 0.3f, true);
            }

            return c;
        }

        private static void DrawGear(PixelCanvas c, float x, float y, float r, float angle, int alpha)
        {
            for (int i = 0; i < 8; i++)
            {
                (float tx, float ty) = ArtKit.Along(x, y, angle + i * 45f, r + 1.2f);
                c.Blend((int)tx, (int)ty, ArtKit.Brass[1].WithAlpha(alpha));
            }

            for (int py = (int)(y - r); py <= (int)(y + r); py++)
            {
                for (int px = (int)(x - r); px <= (int)(x + r); px++)
                {
                    float dx = px + 0.5f - x;
                    float dy = py + 0.5f - y;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d > r) continue;
                    Rgba color = d < 1.2f ? ArtKit.Brass[0] : ArtKit.Tone(ArtKit.Brass, -dx / r * 0.5f - dy / r * 0.6f);
                    c.Blend(px, py, color.WithAlpha(alpha));
                }
            }
        }

        private static void DrawEye(PixelCanvas c, float x, float y, bool squint)
        {
            ArtKit.Ball(c, x, y, 2.8f, 2.8f, D4.Eye, 0.3f);
            if (squint)
            {
                ArtKit.Line(c, (int)x - 2, (int)y, (int)x + 2, (int)y, D4.Ink);
                return;
            }

            c.Fill((int)x, (int)y - 1, 2, 2, LabArt.OxideGreen);
            c.Set((int)x + 1, (int)y, D4.Dark);
            c.Set((int)x - 1, (int)y - 2, new Rgba(255, 255, 255));
        }
    }
}
