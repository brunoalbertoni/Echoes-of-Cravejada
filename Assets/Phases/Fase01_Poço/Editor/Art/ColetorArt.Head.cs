using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class ColetorArt
    {
        // ------------------------------------------------------------------
        // A cabeca
        // ------------------------------------------------------------------

        private static void DrawHead(Frame f)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;
            float hx = f.HeadX;
            float hy = f.HeadY;

            // O colar de latao onde a cabine gira, preso na beirada da cacamba.
            ArtKit.Limb(c, f.TX + 12f, f.Top + 6f, hx - 4f, hy + 7f, 3.4f, ArtKit.Rubber, 0.1f);
            ArtKit.Drum(c, hx - 3f, hy + 8f, 6.5f, 2.6f, 3f, ArtKit.Plain(ArtKit.Brass), 0.15f);

            // A pa dentada embaixo da cabine: abre pra baixo.
            float jawAngle = 10f + pose.Jaw * 45f;
            float pivotX = hx - 3f;
            float pivotY = hy + 6f;
            (float jx, float jy) = ArtKit.Along(pivotX, pivotY, jawAngle, 15f);
            (float jbx, float jby) = ArtKit.Along(pivotX, pivotY, jawAngle + 28f, 9f);
            var jaw = new[] { (pivotX, pivotY - 1f), (jx, jy - 1f), (jx, jy + 2f), (jbx, jby + 1f), (pivotX - 1f, pivotY + 3f) };
            ArtKit.Polygon(c, jaw, (x, y) =>
            {
                float t = (x - pivotX) / 15f;
                return ArtKit.Tone(ArtKit.Iron, 0.4f - t * 0.6f - (y - pivotY) * 0.08f);
            });

            // Os dentes da pa.
            for (int i = 0; i < 4; i++)
            {
                float k = 0.35f + i * 0.2f;
                float tx = pivotX + (jx - pivotX) * k;
                float ty = pivotY + (jy - pivotY) * k - 1.5f;
                (float ux, float uy) = ArtKit.Along(tx, ty, jawAngle - 90f, 2.5f);
                ArtKit.Taper(c, tx, ty, ux, uy, 1.1f, 0.4f, ArtKit.Tin, 0.3f);
            }

            // A cabine: uma cupula de lampiao, ferro com ferrugem.
            ArtKit.Ball(c, hx, hy, 11f, 10f, ArtKit.Corroded(ArtKit.Iron, 0.18f, 71, 0.25f, ArtKit.Rust), 0.1f);

            // As faixas verticais da cupula, como gomos.
            for (int i = -1; i <= 1; i++)
            {
                for (int py = (int)(hy - 9f); py <= (int)(hy + 9f); py++)
                {
                    float dy = (py + 0.5f - hy) / 10f;
                    float half = 11f * (float)Math.Sqrt(Math.Max(0f, 1f - dy * dy));
                    int px = (int)(hx + i * half * 0.55f - 2f);
                    if (c.Get(px, py).A == 0 || Math.Abs(dy) > 0.9f) continue;
                    c.Set(px, py, ArtKit.Shift(ArtKit.Iron, c.Get(px, py), -1));
                }
            }

            // A aba do visor, por cima do olho.
            ArtKit.Taper(c, hx - 4f, hy - 7f, hx + 12f, hy - 4f, 2.2f, 1.2f, ArtKit.Rust, 0.4f);

            // O apito de latao no alto.
            ArtKit.Limb(c, hx - 4f, hy - 9f, hx - 4f, hy - 15f, 1.4f, ArtKit.Brass, 0.2f);
            ArtKit.Drum(c, hx - 4f, hy - 16f, 2.6f, 1.6f, 3f, ArtKit.Plain(ArtKit.Brass), 0.3f);

            // O olho: uma vigia de latao com o vidro aceso pelo Oxide.
            float ex = hx + 5f;
            float ey = hy + 1f;
            ArtKit.Ball(c, ex, ey, 6f, 6f, ArtKit.Brass, 0.15f);
            for (int i = 0; i < 6; i++)
            {
                (float rx, float ry) = ArtKit.Along(ex, ey, i * 60f + 30f, 5f);
                c.Set((int)rx, (int)ry, ArtKit.Brass[0]);
            }

            float lens = pose.Squint ? 3.2f : 4.2f;
            Rgba[] glass = pose.Eye <= 0.05f
                ? new[] { new Rgba(20, 30, 28), new Rgba(30, 44, 40), new Rgba(40, 60, 54), new Rgba(70, 90, 84) }
                : pose.Eye < 0.8f
                    ? new[] { new Rgba(20, 60, 44), new Rgba(30, 100, 70), new Rgba(50, 140, 96), new Rgba(90, 190, 140) }
                    : new[] { ArtKit.OxideDeep, new Rgba(80, 210, 130), ArtKit.OxideGlow, ArtKit.OxideCore };
            ArtKit.Ball(c, ex, ey, 4.2f, lens, glass, pose.Eye > 1.5f ? 0.45f : 0.15f);

            // A pupila: um risco escuro, que vira fenda quando aperta.
            if (pose.Eye > 0.05f)
            {
                c.Fill((int)ex, (int)(ey - lens + 1.5f), 1, (int)(lens * 2f - 2f), ArtKit.OxideDeep);
                c.Set((int)ex - 2, (int)(ey - 2f), ArtKit.OxideCore);
            }

            // Reflexo no vidro.
            c.Set((int)(ex - 2f), (int)(ey - 3f), new Rgba(240, 255, 245));

            if (f.Cracked)
            {
                ArtKit.Crack(c, hx - 7f, hy - 6f, 60f, 9, 17, 1f);
                c.Set((int)(ex + 2f), (int)(ey - 2f), ArtKit.Outline);
                c.Set((int)(ex + 3f), (int)(ey - 1f), ArtKit.Outline);
            }
        }

        // ------------------------------------------------------------------
        // Os bracos
        // ------------------------------------------------------------------

        private static void DrawBackArm(Frame f, float sx, float sy, float ex, float ey)
        {
            Pose pose = f.Pose;
            const float bias = -0.4f;

            ArtKit.Limb(f.Canvas, sx, sy, ex, ey, 3.6f, ArtKit.Corroded(ArtKit.Iron, 0.2f, 81, 0.3f, ArtKit.Rust), bias);
            ArtKit.Limb(f.Canvas, ex, ey, f.BackHandX, f.BackHandY, 2.8f, ArtKit.Rust, bias);
            ArtKit.Ball(f.Canvas, ex, ey, 3.6f, 3.6f, ArtKit.Brass, bias);

            // A mao de dois dedos, e o bolo de sucata quando esta segurando.
            (float ax, float ay) = ArtKit.Along(f.BackHandX, f.BackHandY, pose.BackFore - 30f, 5f);
            (float bx, float by) = ArtKit.Along(f.BackHandX, f.BackHandY, pose.BackFore + 30f, 5f);
            ArtKit.Taper(f.Canvas, f.BackHandX, f.BackHandY, ax, ay, 1.8f, 0.8f, ArtKit.Iron, bias);
            ArtKit.Taper(f.Canvas, f.BackHandX, f.BackHandY, bx, by, 1.8f, 0.8f, ArtKit.Iron, bias);
            ArtKit.Ball(f.Canvas, f.BackHandX, f.BackHandY, 2.8f, 2.8f, ArtKit.Iron, bias + 0.1f);

            if (pose.Holding)
            {
                (float cx, float cy) = ArtKit.Along(f.BackHandX, f.BackHandY, pose.BackFore, 5f);
                ArtKit.Ball(f.Canvas, cx, cy, 5f, 4.4f, ArtKit.Corroded(ArtKit.Rust, 0.35f, 91, 0.5f), 0.2f);
                f.Canvas.Set((int)cx + 2, (int)cy - 2, ArtKit.Tin[3]);
                f.Canvas.Set((int)cx - 2, (int)cy + 1, ArtKit.Iron[0]);
            }
        }

        /// <summary>O braco da garra: ombro, braco, antebraco hidraulico e a garra de tres dentes.</summary>
        private static void DrawArm(Frame f, float ex, float ey)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;

            // O dente de tras da garra, por tras de tudo.
            DrawProng(f, pose.Fore, 0f, -0.45f);

            ArtKit.Taper(c, f.ShoulderX, f.ShoulderY, ex, ey, 5.2f, 4f, ArtKit.Corroded(ArtKit.Iron, 0.15f, 101, 0.28f, ArtKit.Rust), 0.15f);

            // O antebraco: o cilindro hidraulico e a haste brilhante saindo dele.
            (float mx, float my) = (ex + (f.WristX - ex) * 0.6f, ey + (f.WristY - ey) * 0.6f);
            ArtKit.Limb(c, ex, ey, mx, my, 3.8f, ArtKit.Iron, 0.1f);
            ArtKit.Limb(c, mx, my, f.WristX, f.WristY, 2.2f, ArtKit.Tin, 0.3f);
            ArtKit.Drum(c, mx, my, 2.5f, 2.5f, 2f, ArtKit.Plain(ArtKit.Brass), 0.1f);

            ArtKit.Ball(c, ex, ey, 4.6f, 4.6f, ArtKit.Brass, 0.1f);
            ArtKit.Rivet(c, (int)ex - 1, (int)ey - 1, ArtKit.Iron);

            // A ombreira: uma calota de chapa rebitada.
            ArtKit.Ball(c, f.ShoulderX, f.ShoulderY - 1f, 8f, 7f, ArtKit.Corroded(ArtKit.Rust, 0.2f, 102, 0.3f), 0.3f);
            for (int i = 0; i < 5; i++)
            {
                (float rx, float ry) = ArtKit.Along(f.ShoulderX, f.ShoulderY - 1f, 160f + i * 40f, 6f);
                ArtKit.Rivet(c, (int)rx, (int)ry, ArtKit.Iron);
            }
            ArtKit.Rivet(c, (int)f.ShoulderX - 2, (int)f.ShoulderY - 3, ArtKit.Brass);
            ArtKit.Rivet(c, (int)f.ShoulderX + 2, (int)f.ShoulderY + 1, ArtKit.Brass);

            // O cubo da garra e os dois dentes da frente.
            ArtKit.Ball(c, f.WristX, f.WristY, 5f, 5f, ArtKit.Brass, 0.1f);
            DrawProng(f, pose.Fore, -1f, 0.1f);
            DrawProng(f, pose.Fore, 1f, 0f);

            if (f.Cracked) ArtKit.Crack(c, f.ShoulderX - 3f, f.ShoulderY - 3f, pose.Arm, 8, 21, 0.9f);
        }

        /// <summary>Um dente da garra: sai do cubo abrindo e a ponta curva pra dentro.</summary>
        private static void DrawProng(Frame f, float direction, float side, float bias)
        {
            float spread = 14f + f.Pose.Open * 34f;
            float angle = direction + side * spread;
            if (side == 0f) angle = direction + 4f;

            (float mx, float my) = ArtKit.Along(f.WristX, f.WristY, angle, 10f);
            float hook = side == 0f ? 0f : -side * (45f + f.Pose.Open * 25f);
            (float tx, float ty) = ArtKit.Along(mx, my, angle + hook, 9f);

            ArtKit.Taper(f.Canvas, f.WristX, f.WristY, mx, my, 3.2f, 2.4f, side == 0f ? ArtKit.Iron : ArtKit.Tin, bias);
            ArtKit.Taper(f.Canvas, mx, my, tx, ty, 2.4f, 0.6f, side == 0f ? ArtKit.Iron : ArtKit.Tin, bias + 0.15f);
            ArtKit.Ball(f.Canvas, mx, my, 2.4f, 2.4f, ArtKit.Iron, bias);
        }

        /// <summary>Na queda, o remendo de cobre voando (ou caido no chao).</summary>
        private static void DrawFlyingPlate(Frame f)
        {
            float x = f.TX - 15f + f.Pose.PlateFlyX;
            float y = f.TY - 7f + f.Pose.PlateFlyY;
            y = Math.Min(y, GroundY - 3f);

            var corners = new[] { (x, y), (x + 10f, y - 2f), (x + 11f, y + 4f), (x + 1f, y + 6f) };
            ArtKit.Polygon(f.Canvas, corners, (px, py) => ArtKit.Tone(ArtKit.Copper, 0.5f - (py - y) * 0.15f));
            f.Canvas.Set((int)x + 2, (int)y + 1, ArtKit.Iron[3]);
            f.Canvas.Set((int)x + 8, (int)y, ArtKit.Iron[3]);
        }
    }
}
