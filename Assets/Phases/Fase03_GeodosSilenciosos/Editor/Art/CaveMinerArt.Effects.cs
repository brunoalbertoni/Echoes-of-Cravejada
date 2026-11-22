using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class CaveMinerArt
    {
        // ------------------------------------------------------------------
        // Efeitos
        // ------------------------------------------------------------------

        private static void DrawGlows(Frame f)
        {
            Pose pose = f.Pose;
            if (!pose.HelmetOff && !pose.NoGem && pose.Gem > 0.05f)
            {
                ArtKit.Glow(f.Canvas, f.HeadX + 11f, f.HeadY - 9f, 10f, Sapphire[2], 0.3f * pose.Gem, true);
            }

            if (!pose.Freed)
            {
                float mx = (f.HX + f.SX) * 0.5f;
                float my = (f.HY + f.SY) * 0.5f;
                ArtKit.Glow(f.Canvas, mx + 8f, my, 7f, ArtKit.OxideGlow, 0.25f + 0.1f * Math.Min(1f, pose.Gem), true);
            }
        }

        private static void DrawSmear(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.SmearFrom == pose.SmearTo) return;
            float from = Math.Min(pose.SmearFrom, pose.SmearTo);
            float to = Math.Max(pose.SmearFrom, pose.SmearTo);
            float cx = f.SX + 6f;
            float cy = f.SY + 4f;

            for (int py = 0; py < Height; py++)
            {
                for (int px = 0; px < Width; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r < 44f || r > 66f) continue;

                    float a = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    if (a < from || a > to) continue;

                    float k = (a - from) / Math.Max(1f, to - from);
                    float edge = (r - 44f) / 22f;
                    int alpha = (int)(30 + 160 * k * edge);
                    if (alpha < 35) continue;
                    f.Canvas.Blend(px, py, new Rgba(230, 226, 255).WithAlpha(alpha));
                }
            }
        }

        /// <summary>As ondas de som: arcos claros saindo da boca, pra frente (apito) ou em leque (grito).</summary>
        private static void DrawRings(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.Rings <= 0f) return;

            float cx = f.MouthX + (pose.Whistle ? 5f : 1f);
            float cy = f.MouthY;
            float span = pose.Wide ? 75f : 40f;
            float center = pose.Wide ? -60f : -5f;
            int count = pose.Wide ? 3 : 2;

            for (int k = 0; k < count; k++)
            {
                float r = 6f + k * 7f + pose.Rings * 7f;
                int alpha = (int)(210 - k * 50 - pose.Rings * 40);
                if (alpha < 40) continue;

                for (int py = (int)(cy - r) - 2; py <= (int)(cy + r) + 2; py++)
                {
                    for (int px = (int)(cx - r) - 2; px <= (int)(cx + r) + 2; px++)
                    {
                        float dx = px + 0.5f - cx;
                        float dy = py + 0.5f - cy;
                        float d = (float)Math.Sqrt(dx * dx + dy * dy);
                        if (Math.Abs(d - r) > 0.75f) continue;

                        float a = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                        if (Math.Abs(a - center) > span) continue;
                        f.Canvas.Blend(px, py, new Rgba(226, 232, 255).WithAlpha(alpha));
                    }
                }
            }
        }

        private static void DrawDust(Frame f)
        {
            float dust = f.Pose.Dust;
            if (dust <= 0f) return;

            var light = new Rgba(150, 140, 176);
            var dark = new Rgba(92, 84, 116);
            float x = f.Pose.DustX >= 0f ? f.Pose.DustX : BodyX + f.Pose.X + SlamReach * PixelsPerUnit;
            int alpha = dust > 1.5f ? 120 : 190;

            for (int i = 0; i < 3; i++)
            {
                float r = 3f + dust * 2f - i * 0.5f;
                ArtKit.Puff(f.Canvas, x - 10f + i * 10f, Ground - r * 0.6f - dust, r, light, dark, alpha - i * 25);
            }

            for (int i = 0; i < 5; i++)
            {
                f.Canvas.Set((int)(x - 12f + i * 6f), (int)(Ground - 6f - (i % 3) * 3f - dust * 4f), new Rgba(78, 74, 100));
            }
        }

        /// <summary>Tonto: tres estrelinhas rodando em volta do capacete.</summary>
        private static void DrawStars(Frame f)
        {
            if (f.Pose.Stars < 0) return;

            float cx = f.HeadX + 2f;
            float cy = f.HeadY - 18f;
            for (int k = 0; k < 3; k++)
            {
                double a = f.Pose.Stars * 0.8 + k * Math.PI * 2.0 / 3.0;
                int x = (int)Math.Round(cx + Math.Cos(a) * 14.0);
                int y = (int)Math.Round(cy + Math.Sin(a) * 4.0);
                var star = new Rgba(255, 236, 150);
                f.Canvas.Set(x, y, new Rgba(255, 255, 230));
                f.Canvas.Set(x + 1, y, star);
                f.Canvas.Set(x - 1, y, star);
                f.Canvas.Set(x, y + 1, star);
                f.Canvas.Set(x, y - 1, star);
            }
        }

        /// <summary>O po violeta dos cristais se desfazendo, subindo.</summary>
        private static void DrawCrystalDust(Frame f, float cx, float cy)
        {
            float amount = f.Pose.CrystalDust;
            if (amount <= 0f) return;

            int count = (int)(10 + amount * 20f);
            for (int i = 0; i < count; i++)
            {
                int x = (int)(cx - 22f + ArtKit.Hash(i, (int)(amount * 10f), 5261) % 44);
                int y = (int)(cy - 16f - amount * 10f + ArtKit.Hash(i, (int)(amount * 10f), 5262) % 30);
                Rgba color = i % 3 == 0 ? Milky[3] : Amethyst[3];
                f.Canvas.Blend(x, y, color.WithAlpha((int)(120 + amount * 120f)));
            }
        }
    }
}
