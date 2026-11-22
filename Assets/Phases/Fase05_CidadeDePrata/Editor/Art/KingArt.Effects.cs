using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class KingArt
    {
        // ------------------------------------------------------------------
        // Efeitos
        // ------------------------------------------------------------------

        private static void DrawGlows(Frame f)
        {
            Pose pose = f.Pose;
            if (!pose.CrownOff && pose.CrownGlow > 0.1f)
            {
                Glow(f.Canvas, f.HeadX, f.HeadY - 18f, 14f, f.Corrupt ? Green : Light, 0.25f * pose.CrownGlow, true);
            }

            if (!pose.ScepterDropped && pose.ScepterGlow > 0.5f)
            {
                Glow(f.Canvas, _lastTip.X, _lastTip.Y, 6f + pose.ScepterGlow * 3f, f.Corrupt ? Green : Light, 0.35f * pose.ScepterGlow, true);
            }

            if (f.Corrupt && pose.Chest > 0.05f)
            {
                float mx = (f.HX + f.SX) * 0.5f - 6f;
                float my = (f.HY + f.SY) * 0.5f - 4f;
                Glow(f.Canvas, mx, my, 12f, Green, 0.3f * pose.Chest, true);
            }

            // Os raios da prata (fase 1): finos, saindo das bordas do corpo.
            if (!f.Corrupt && pose.Rays > 0.1f)
            {
                for (int i = 0; i < 8; i++)
                {
                    double a = -Math.PI * 0.9 + i * Math.PI * 0.8 / 7.0;
                    float r0 = 30f;
                    float r1 = 30f + pose.Rays * 14f;
                    float cx = f.SX;
                    float cy = f.SY + 4f;
                    Line(f.Canvas, (int)(cx + Math.Cos(a) * r0), (int)(cy + Math.Sin(a) * r0),
                        (int)(cx + Math.Cos(a) * r1), (int)(cy + Math.Sin(a) * r1), Light.WithAlpha((int)(60 + pose.Rays * 60f)));
                }
            }
        }

        /// <summary>O golpe pro alto: uma coluna de luz subindo da ponta do cetro ate o teto.</summary>
        private static void DrawPillar(Frame f)
        {
            float k = f.Pose.Pillar;
            if (k <= 0f) return;

            Rgba tint = f.Corrupt ? Green : Light;
            int x0 = (int)_lastTip.X;
            for (int y = 0; y < (int)_lastTip.Y; y++)
            {
                for (int dx = -5; dx <= 5; dx++)
                {
                    float edge = Math.Abs(dx) / 5f;
                    int alpha = (int)(k * (220 - edge * 170));
                    if (alpha < 30) continue;
                    Blend(f.Canvas, x0 + dx, y, (Math.Abs(dx) <= 1 ? new Rgba(255, 255, 255) : tint).WithAlpha(Math.Min(255, alpha)));
                }
            }

            Glow(f.Canvas, _lastTip.X, _lastTip.Y, 14f, tint, 0.6f * k, true);
            for (int i = 0; i < 4; i++) Spark(f.Canvas, x0 - 8 + i * 5, (int)_lastTip.Y - 10 - i * 14, 2);
        }

        private static void DrawSmear(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.SmearFrom == pose.SmearTo) return;
            float from = Math.Min(pose.SmearFrom, pose.SmearTo);
            float to = Math.Max(pose.SmearFrom, pose.SmearTo);
            float cx = f.SX + 4f;
            float cy = f.SY + 4f;
            Rgba tint = f.Corrupt ? new Rgba(170, 255, 200) : new Rgba(255, 246, 210);

            for (int py = 0; py < DesignHeight; py++)
            {
                for (int px = 0; px < DesignWidth; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r < 50f || r > 76f) continue;

                    float a = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    if (a < from || a > to) continue;

                    float k = (a - from) / Math.Max(1f, to - from);
                    float edge = (r - 50f) / 26f;
                    int alpha = (int)(30 + 170 * k * edge);
                    if (alpha < 35) continue;
                    Blend(f.Canvas, px, py, tint.WithAlpha(alpha));
                }
            }
        }

        /// <summary>O sopro: a nevoa verde saindo da boca, rente pra frente.</summary>
        private static void DrawMist(Frame f)
        {
            float mist = f.Pose.Mist;
            if (mist <= 0f) return;

            for (int i = 0; i < 6; i++)
            {
                float x = f.MouthX + 6f + i * 9f * mist;
                float y = f.MouthY + 4f + i * 5f;
                Puff(f.Canvas, x, y, 4f + i * 1.4f, new Rgba(150, 240, 180), new Rgba(50, 130, 90), 170 - i * 18);
            }
        }

        /// <summary>Flutuando: fiapos de Oxide escorrendo dos pes.</summary>
        private static void DrawWisps(Frame f)
        {
            if (!f.Corrupt || f.Float <= 0f) return;

            for (int i = 0; i < 5; i++)
            {
                float x = f.HX - 10f + i * 6f;
                float y = Ground - 3f - (i % 2) * 3f;
                Puff(f.Canvas, x, y, 4f + (i % 3), new Rgba(130, 230, 160), new Rgba(40, 110, 76), 110);
            }
        }

        private static void DrawDust(Frame f)
        {
            float dust = f.Pose.Dust;
            if (dust <= 0f) return;

            var light = new Rgba(220, 222, 236);
            var dark = new Rgba(150, 154, 176);
            float x = BodyX + f.Pose.X + SweepReach * PixelsPerUnit / Scale;
            for (int i = 0; i < 3; i++)
            {
                float r = 3f + dust * 2f - i * 0.5f;
                Puff(f.Canvas, x - 10f + i * 10f, Ground - r * 0.6f - dust, r, light, dark, dust > 1.5f ? 120 : 190);
            }
        }

        // ------------------------------------------------------------------
        // As pecas soltas: as facetas, a fumaca e o brilho (16 px por tile)
        // ------------------------------------------------------------------

        /// <summary>Uma faceta de luz da coroa, 14x14: um losango de quartzo aceso.</summary>
        public static PixelCanvas Facet(bool corrupt)
        {
            var c = new PixelCanvas(14, 14);
            Rgba[] ramp = corrupt ? new[] { Patina[1], Green.Scaled(0.7f), Green, GreenCore } : Quartz;
            ArtKit.Polygon(c, new[] { (7f, 0f), (14f, 7f), (7f, 14f), (0f, 7f) }, (x, y) => ArtKit.Tone(ramp, 0.6f - (x + y) / 18f));
            c.Set(5, 5, new Rgba(255, 255, 255));
            ArtKit.Glow(c, 7f, 7f, 7f, corrupt ? Green : Light, 0.6f, true);
            return c;
        }

        /// <summary>Uma baforada de Oxide, 48x40: o pedaco da nuvem do sopro, translucida.</summary>
        public static PixelCanvas Puff(int seed)
        {
            var c = new PixelCanvas(48, 40);
            for (int i = 0; i < 6; i++)
            {
                float px = 12f + ArtKit.Hash(i, seed, 9201) % 24;
                float py = 12f + ArtKit.Hash(i, seed, 9202) % 16;
                ArtKit.Puff(c, px, py, 9f + i % 3 * 2f, new Rgba(130, 230, 160), new Rgba(40, 110, 76), 110);
            }

            return c;
        }

        /// <summary>O brilho que avisa onde a faceta vai cair, 16x16.</summary>
        public static PixelCanvas Glint()
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Spark(c, 8, 8, 6);
            ArtKit.Glow(c, 8f, 8f, 8f, Green, 0.8f, true);
            return c;
        }
    }
}
