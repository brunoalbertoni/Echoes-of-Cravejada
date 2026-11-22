using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class ColetorArt
    {
        // ------------------------------------------------------------------
        // O que vem por cima: brilhos, rastro, fumaca, vapor, poeira
        // ------------------------------------------------------------------

        private static void DrawGlows(Frame f)
        {
            Pose pose = f.Pose;

            if (pose.Eye >= 1f)
            {
                float strength = (f.Cracked ? 0.45f : 0.3f) * (pose.Eye - 0.5f);
                ArtKit.Glow(f.Canvas, f.HeadX + 5f, f.HeadY + 1f, 9f + (f.Cracked ? 3f : 0f), ArtKit.OxideGlow, strength, true);
            }

            if (pose.Furnace > 0.5f)
            {
                ArtKit.Glow(f.Canvas, f.TX + 6f, f.TY + 6f, 11f, ArtKit.OxideGlow, 0.35f * pose.Furnace, f.Cracked);
            }

            if (f.Cracked) ArtKit.Glow(f.Canvas, f.TX + 12f, f.Top + 14f, 9f, ArtKit.OxideGlow, 0.3f);
        }

        private static void DrawSmear(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.SmearFrom == pose.SmearTo) return;

            float from = Math.Min(pose.SmearFrom, pose.SmearTo);
            float to = Math.Max(pose.SmearFrom, pose.SmearTo);

            for (int py = 0; py < Height; py++)
            {
                for (int px = 0; px < Width; px++)
                {
                    float dx = px + 0.5f - f.ShoulderX;
                    float dy = py + 0.5f - f.ShoulderY;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r < 26f || r > 44f) continue;

                    float a = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    if (a < from || a > to) continue;

                    float k = (a - from) / Math.Max(1f, to - from);
                    float edge = (r - 26f) / 18f;
                    int alpha = (int)(30 + 150 * k * edge);
                    if (alpha < 35) continue;

                    f.Canvas.Blend(px, py, new Rgba(220, 255, 230).WithAlpha(alpha));
                }
            }
        }

        private static void DrawSmoke(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.Puffs <= 0) return;

            float baseX = f.TX - 30f;
            float baseY = f.Top - 24f;
            if (pose.ChimneyBent)
            {
                baseX = f.TX - 43f;
                baseY = f.Top - 7f;
            }

            Rgba light = f.Cracked ? new Rgba(170, 220, 180) : new Rgba(170, 166, 156);
            Rgba dark = f.Cracked ? new Rgba(90, 140, 110) : new Rgba(96, 94, 90);

            for (int i = 0; i < pose.Puffs; i++)
            {
                // Cada baforada sobe e cresce; a de cima ja esta sumindo.
                float t = (pose.Smoke + i / (float)Math.Max(1, pose.Puffs)) % 1f;
                float x = baseX - t * 8f + (float)Math.Sin((t + i) * 6f) * 1.5f;
                float y = baseY - t * 20f;
                float r = 2.5f + t * 4.5f;
                int alpha = (int)(170 * (1f - t * 0.8f));

                if (y - r < 0f) continue;
                ArtKit.Puff(f.Canvas, x, y, r, light, dark, alpha);
            }
        }

        private static void DrawSteam(Frame f)
        {
            float steam = f.Pose.Steam;
            if (steam <= 0f) return;

            float x = f.HeadX - 4f;
            float y = f.HeadY - 19f;
            var white = new Rgba(240, 244, 240);
            var grey = new Rgba(190, 200, 196);

            ArtKit.Puff(f.Canvas, x, y, 2.5f + steam * 1.5f, white, grey, (int)(200 * steam));
            ArtKit.Puff(f.Canvas, x - 2f, y - 5f * steam, 2f + steam * 2.5f, white, grey, (int)(140 * steam));
            if (steam > 0.7f) ArtKit.Puff(f.Canvas, x + 2f, y - 10f, 3.5f, white, grey, 90);
        }

        private static void DrawDust(Frame f)
        {
            float dust = f.Pose.Dust;
            if (dust <= 0f) return;

            var light = new Rgba(196, 170, 130);
            var dark = new Rgba(130, 104, 80);
            float spread = 10f + dust * 12f;
            int alpha = dust > 1.5f ? 110 : 200;

            foreach (float side in new[] { -1f, 1f })
            {
                for (int i = 0; i < 3; i++)
                {
                    float x = TorsoX + f.Pose.X + side * (spread + i * 7f);
                    float y = GroundY - 3f - i * 2f - dust * 2f;
                    ArtKit.Puff(f.Canvas, x, y, 3.5f + i + dust, light, dark, alpha - i * 30);
                }
            }
        }

        /// <summary>O verde tomando o corpo antes do estouro: tudo clareia pro Oxide.</summary>
        private static void DrawBurst(Frame f)
        {
            float burst = f.Pose.Burst;
            PixelCanvas c = f.Canvas;

            for (int i = 0; i < c.Pixels.Length; i++)
            {
                Rgba p = c.Pixels[i];
                if (p.A < 200) continue;

                int x = i % c.Width;
                int y = i / c.Width;
                float wobble = ArtKit.Noise(x * 0.2f, y * 0.2f, 99);
                float k = burst * (0.6f + 0.6f * wobble);
                Rgba target = k > 0.75f ? ArtKit.OxideCore : ArtKit.OxideGlow;
                c.Pixels[i] = Rgba.Lerp(p, target, Math.Min(1f, k));
            }

            ArtKit.Glow(c, f.TX, f.TY, 40f, ArtKit.OxideGlow, burst * 0.6f, true);
        }
    }
}
