using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Experimento Costurado: metade cobre (a de tras — pesada, a perna
    /// grossa arrastando, o braco de tenaz pendurado), metade prata (a da
    /// frente — magra, rapida, o braco de garra), unidas por uma costura de
    /// linha grossa com o verde do Oxide vazando pelos pontos. Na cabeca, um
    /// capacete de mergulho de latao costurado a uma mascara de prata: um olho
    /// verde e um branco. Arrasta, trava tremendo e dispara na perna de prata.
    /// Morrendo, a costura arrebenta e as metades caem cada uma pra um lado.
    /// 64x64, pivo na base, embaixo dele.
    /// </summary>
    public static class ExperimentArt
    {
        public const int Size = 64;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 28f / Size;
        public const float PivotY = 1f / Size;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Drag = 4;
        public const int DragFrames = 8;
        public const int Charge = 12;
        public const int ChargeFrames = 2;
        public const int Lunge = 14;
        public const int LungeFrames = 4;
        public const int Hurt = 18;
        public const int Death = 19;
        public const int DeathFrames = 7;
        public const int FrameCount = 26;

        private const float Ground = 62f;

        private sealed class Pose
        {
            public float X;
            public float Y;
            public float Lean = 6f;
            public float CopperFoot = -6f;
            public float SilverFoot = 6f;
            public float CopperLift;
            public float SilverLift;
            public float Claw = 40f;
            public float Tongs = 100f;
            public float Eye = 1f;
            public float Shake;
            public bool Speed;
            public float Split;
            public float Leak;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(new Pose { Y = i % 2 * 0.6f, Claw = 40f + (i == 1 ? 10f : 0f), Tongs = 100f - (i % 2) * 6f, Eye = i == 2 ? 1.5f : 1f, Leak = 0.3f });
            }

            // Mancando: a perna de prata da o passo, a de cobre vem arrastada.
            for (int i = 0; i < DragFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / DragFrames;
                float swing = (float)Math.Sin(phase);
                frames[Drag + i] = Draw(new Pose
                {
                    Y = swing > 0f ? swing * 1.5f : 0f, Lean = 8f + swing * 3f,
                    SilverFoot = 6f + swing * 6f, SilverLift = swing < -0.3f ? -swing * 4f : 0f,
                    CopperFoot = -8f - swing * 2f, Claw = 40f + swing * 10f, Tongs = 100f + swing * 12f, Leak = 0.3f,
                });
            }

            // Trava na perna de prata, treme, o olho branco acende.
            for (int i = 0; i < ChargeFrames; i++)
            {
                Pose charge = new Pose { Y = 3f, Lean = 18f, SilverFoot = 9f, CopperFoot = -11f, Claw = 10f, Tongs = 130f, Eye = 2f, Shake = i == 0 ? 1f : -1f, Leak = 0.6f };
                charge.Sparks.Add((44 + i * 2, 12, 1));
                frames[Charge + i] = Draw(charge);
            }

            // Dispara: corre na perna de prata, a de cobre no ar, arrastada.
            for (int i = 0; i < LungeFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / LungeFrames;
                float swing = (float)Math.Sin(phase);
                frames[Lunge + i] = Draw(new Pose
                {
                    X = 2f, Y = 2f, Lean = 30f, SilverFoot = 4f + swing * 10f, SilverLift = swing < 0f ? -swing * 5f : 0f,
                    CopperFoot = -14f, CopperLift = 3f + (i % 2) * 2f, Claw = -10f, Tongs = 160f, Eye = 2f, Speed = true, Leak = 0.4f,
                });
            }

            Pose hurt = new Pose { X = -2f, Lean = -10f, Claw = 80f, Tongs = 60f, Eye = 0.4f, Leak = 1f };
            hurt.Sparks.Add((30, 26, 2));
            frames[Hurt] = Draw(hurt);

            float[] split = { 0f, 0.15f, 0.3f, 0.5f, 0.7f, 0.9f, 1f };
            for (int i = 0; i < DeathFrames; i++)
            {
                Pose dying = new Pose { Split = split[i], Eye = i < 2 ? 2f : 0f, Leak = 1f, Lean = i == 0 ? -12f : 6f };
                if (i == 0) dying.Sparks.Add((28, 30, 3));
                frames[Death + i] = Draw(dying);
            }

            return frames;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var c = new PixelCanvas(Size, Size);

            if (pose.Split > 0f)
            {
                DrawSplit(c, pose);
                return c;
            }

            float hx = 28f + pose.X + pose.Shake;
            float hy = 40f + pose.Y;
            (float sx, float sy) = ArtKit.Along(hx, hy, -90f + pose.Lean, 17f);

            // O braco de tenaz, de cobre, pendurado atras.
            DrawTongs(c, sx - 5f, sy + 3f, pose.Tongs, -0.4f);

            // A perna de cobre, grossa, arrastada.
            float cfx = 28f + pose.X * 0.4f + pose.CopperFoot;
            float cfy = Ground - 2f - pose.CopperLift;
            (float ckx, float cky) = ArtKit.Joint(hx - 3f, hy + 1f, cfx, cfy, 11f, 11f, 1);
            ArtKit.Skin copper = ArtKit.Corroded(ArtKit.Copper, 0.25f, 7301, 0.3f);
            ArtKit.Taper(c, hx - 3f, hy + 1f, ckx, cky, 4.6f, 4f, copper, -0.3f);
            ArtKit.Taper(c, ckx, cky, cfx, cfy, 4f, 3.4f, copper, -0.3f);
            ArtKit.Ball(c, ckx, cky, 3.4f, 3.4f, ArtKit.Brass, -0.2f);
            ArtKit.Drum(c, cfx, cfy + 0.5f, 6f, 2.4f, 3f, ArtKit.Plain(ArtKit.Iron), -0.2f);

            // O tronco: costas de cobre, frente de prata, a costura no meio.
            ArtKit.Skin halves = (x, y, light) =>
            {
                float seam = y * -0.15f;
                if (x < seam - 0.5f) return ArtKit.Tone(ArtKit.Copper, light - 0.05f);
                if (x < seam + 0.5f) return D4.Thread[1];
                return ArtKit.Tone(D4.Silver, light + 0.15f);
            };
            float mx = (hx + sx) * 0.5f;
            float my = (hy + sy) * 0.5f;
            ArtKit.Ball(c, mx, my, 9.5f, 12f, halves, 0f);
            ArtKit.Ball(c, sx, sy + 2f, 9f, 6f, halves, 0.1f);

            // Os pontos da costura, e o verde vazando entre eles.
            for (int i = -5; i <= 5; i++)
            {
                float y = my + i * 2.2f;
                float x = mx - (y - my) * 0.15f;
                if (c.Get((int)x, (int)y).A == 0) continue;
                c.Set((int)x - 1, (int)y, D4.Thread[3]);
                c.Set((int)x + 1, (int)y, D4.Thread[3]);
                c.Set((int)x, (int)y + 1, i % 3 == 0 && pose.Leak > 0.2f ? LabArt.OxideGreen : D4.Thread[0]);
            }

            // Os parafusos do cobre e o brilho da prata.
            ArtKit.Rivet(c, (int)mx - 6, (int)my - 6, ArtKit.Brass);
            ArtKit.Rivet(c, (int)mx - 7, (int)my + 2, ArtKit.Brass);
            ArtKit.Rivet(c, (int)mx - 5, (int)my + 8, ArtKit.Brass);
            ArtKit.Line(c, (int)mx + 4, (int)my - 7, (int)mx + 6, (int)my - 1, D4.Silver[3]);

            // A mangueira ligando as metades.
            for (int i = 0; i <= 8; i++)
            {
                float t = i / 8f;
                float x = mx - 7f + t * 12f;
                float y = my + 10f + (float)Math.Sin(t * Math.PI) * 3f;
                c.Set((int)x, (int)y, ArtKit.Rubber[2]);
                c.Set((int)x, (int)y + 1, ArtKit.Rubber[0]);
            }

            // A perna de prata, fina e rapida.
            float sfx = 28f + pose.X * 0.4f + pose.SilverFoot;
            float sfy = Ground - 2f - pose.SilverLift;
            (float skx, float sky) = ArtKit.Joint(hx + 3f, hy + 1f, sfx, sfy, 11f, 11f, -1);
            ArtKit.Taper(c, hx + 3f, hy + 1f, skx, sky, 2.8f, 2.2f, D4.Silver, 0.15f);
            ArtKit.Taper(c, skx, sky, sfx, sfy, 2.2f, 1.6f, D4.Silver, 0.15f);
            ArtKit.Ball(c, skx, sky, 2.2f, 2.2f, D4.Silver, 0.4f);
            ArtKit.Taper(c, sfx - 2f, sfy + 1f, sfx + 5f, sfy + 1.5f, 1.8f, 0.6f, D4.Silver, 0.2f);

            DrawHead(c, pose, sx + 4f, sy - 8f);

            // O braco de garra, de prata, na frente.
            (float ex, float ey) = ArtKit.Along(sx + 5f, sy + 2f, pose.Claw, 9f);
            (float px, float py) = ArtKit.Along(ex, ey, pose.Claw + 20f, 8f);
            ArtKit.Limb(c, sx + 5f, sy + 2f, ex, ey, 2.2f, D4.Silver, 0.15f);
            ArtKit.Limb(c, ex, ey, px, py, 1.8f, D4.Silver, 0.2f);
            ArtKit.Ball(c, sx + 5f, sy + 2f, 3.2f, 3.2f, D4.Silver, 0.35f);
            for (int i = -1; i <= 1; i++)
            {
                (float tx, float ty) = ArtKit.Along(px, py, pose.Claw + 20f + i * 30f, 5f);
                ArtKit.Taper(c, px, py, tx, ty, 1.1f, 0.3f, D4.Silver, 0.4f);
            }

            ArtKit.AddOutline(c);

            if (pose.Eye > 1.4f) ArtKit.Glow(c, sx + 9f, sy - 9f, 6f, new Rgba(255, 255, 255), 0.4f, true);
            if (pose.Leak > 0.5f) ArtKit.Glow(c, mx, my, 8f, LabArt.OxideGreen, 0.2f * pose.Leak, true);

            if (pose.Speed)
            {
                for (int i = 0; i < 3; i++)
                {
                    int y = (int)(sy + 4f + i * 7f);
                    ArtKit.Line(c, 2, y, 14 - i * 3, y, new Rgba(220, 226, 240, 120));
                }
            }

            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(c, spark.X, spark.Y, spark.Size);
            return c;
        }

        /// <summary>O capacete de mergulho de latao, costurado a mascara de prata: um olho verde, um branco.</summary>
        private static void DrawHead(PixelCanvas c, Pose pose, float x, float y)
        {
            ArtKit.Ball(c, x, y, 7f, 7f, ArtKit.Corroded(ArtKit.Brass, 0.25f, 7302, 0.35f), 0.15f);

            // A mascara de prata cobrindo a frente.
            ArtKit.Polygon(c, new[] { (x, y - 7f), (x + 7f, y - 4f), (x + 7.5f, y + 4f), (x + 1f, y + 7f) },
                (px, py) => ArtKit.Tone(D4.Silver, 0.6f - (px - x) * 0.05f - (py - y) * 0.05f));

            // A costura de cima a baixo.
            for (int i = -6; i <= 6; i += 2)
            {
                c.Set((int)x, (int)y + i, D4.Thread[3]);
                c.Set((int)x - 1, (int)y + i + 1, D4.Thread[1]);
                c.Set((int)x + 1, (int)y + i + 1, D4.Thread[1]);
            }

            // A vigia do capacete (o olho verde) e o olho branco da mascara.
            ArtKit.Ball(c, x - 3.5f, y - 1f, 2.6f, 2.6f, ArtKit.Brass, 0.3f);
            Rgba green = pose.Eye <= 0.05f ? ArtKit.Iron[0] : LabArt.OxideGreen;
            c.Fill((int)x - 4, (int)y - 2, 2, 2, green);
            Rgba white = pose.Eye <= 0.05f ? D4.Silver[0] : (pose.Eye > 1.4f ? new Rgba(255, 255, 255) : D4.Eye[1]);
            c.Fill((int)x + 3, (int)y - 2, 3, 2, D4.Ink);
            c.Fill((int)x + 4, (int)y - 2, 2, 1, white);

            // A grade da boca e os parafusos do capacete.
            for (int i = 0; i < 3; i++) c.Set((int)x + 3 + i, (int)y + 3, D4.Silver[0]);
            ArtKit.Rivet(c, (int)x - 6, (int)y + 2, ArtKit.Brass);
            ArtKit.Rivet(c, (int)x - 4, (int)y - 6, ArtKit.Brass);
        }

        /// <summary>O braco de cobre que acaba numa tenaz de laboratorio.</summary>
        private static void DrawTongs(PixelCanvas c, float sx, float sy, float angle, float bias)
        {
            (float ex, float ey) = ArtKit.Along(sx, sy, angle, 10f);
            (float hx, float hy) = ArtKit.Along(ex, ey, angle - 10f, 9f);
            ArtKit.Limb(c, sx, sy, ex, ey, 3.4f, ArtKit.Copper, bias);
            ArtKit.Limb(c, ex, ey, hx, hy, 3f, ArtKit.Copper, bias);
            ArtKit.Ball(c, ex, ey, 3f, 3f, ArtKit.Brass, bias + 0.1f);

            for (int side = -1; side <= 1; side += 2)
            {
                (float tx, float ty) = ArtKit.Along(hx, hy, angle - 10f + side * 18f, 7f);
                ArtKit.Taper(c, hx, hy, tx, ty, 1.6f, 0.8f, ArtKit.Iron, bias + 0.2f);
            }
        }

        /// <summary>A costura arrebenta: a metade de cobre cai pra tras, a de prata pra frente.</summary>
        private static void DrawSplit(PixelCanvas c, Pose pose)
        {
            float k = pose.Split;
            float fall = Math.Min(1f, k * 1.3f);
            float g = Ground;

            // A metade de cobre, tombando pra tras.
            float cx = 24f - k * 8f;
            (float ctx, float cty) = ArtKit.Along(cx, g - 3f, -90f - fall * 85f, 20f);
            ArtKit.Taper(c, cx, g - 3f, ctx, cty, 6f, 7f, ArtKit.Corroded(ArtKit.Copper, 0.3f, 7303), -0.1f);
            ArtKit.Ball(c, ctx, cty, 5.5f, 5.5f, ArtKit.Corroded(ArtKit.Brass, 0.3f, 7304), 0f);
            if (pose.Eye > 0.5f) c.Fill((int)ctx, (int)cty, 2, 2, LabArt.OxideGreen);

            // A metade de prata, pra frente.
            float px = 32f + k * 8f;
            (float ptx, float pty) = ArtKit.Along(px, g - 3f, -90f + fall * 85f, 20f);
            ArtKit.Taper(c, px, g - 3f, ptx, pty, 5f, 6f, D4.Silver, 0.1f);
            ArtKit.Ball(c, ptx, pty, 5f, 5f, D4.Silver, 0.3f);

            // Os fios soltos.
            for (int i = 0; i < 6; i++)
            {
                c.Set(26 + i * 2, (int)g - 6 - (i % 3) * 2, D4.Thread[2]);
            }

            ArtKit.AddOutline(c);

            // A poca verde que escorre.
            int half = (int)(4f + k * 14f);
            for (int x = 28 - half; x <= 28 + half; x++)
            {
                c.Set(x, (int)g, LabArt.OxideDark);
                if (Math.Abs(x - 28) < half - 2) c.Set(x, (int)g - 1, LabArt.OxideGreen.WithAlpha(210));
            }

            if (k < 0.4f) ArtKit.Glow(c, 28f, g - 20f, 14f, LabArt.OxideGreen, 0.6f, true);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(c, spark.X, spark.Y, spark.Size);
        }
    }
}
