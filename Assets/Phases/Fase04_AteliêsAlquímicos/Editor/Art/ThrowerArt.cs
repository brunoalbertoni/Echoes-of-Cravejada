using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Arremessador: um assistente de laboratorio de cobre, careca e cheio
    /// de rebites, corcunda, de jaleco manchado de verde. Oculos de protecao
    /// com as lentes acesas no verde do Oxide, um pano amarrado na boca, a
    /// bandoleira de frascos coloridos no peito e a bolsa de couro no quadril.
    /// Parado, mexe na bolsa; no arremesso, puxa um frasco, joga o braco pra
    /// tras e arremessa. Morrendo, cai de joelhos e os frascos estouram.
    /// 48x56, pivo na base, embaixo dele.
    /// </summary>
    public static class ThrowerArt
    {
        public const int Width = 48;
        public const int Height = 56;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 22f / Width;
        public const float PivotY = 1f / Height;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Windup = 4;
        public const int WindupFrames = 3;
        public const int Throw = 7;
        public const int ThrowFrames = 2;
        public const int Hurt = 9;
        public const int Death = 10;
        public const int DeathFrames = 6;
        public const int FrameCount = 16;

        /// <summary>De onde o frasco sai, em tiles acima do pivo.</summary>
        public const float HandHeight = 1.3f;

        private const float Ground = 54f;

        private sealed class Pose
        {
            public float Y;
            public float Lean = 10f;
            public float Arm = 80f;
            public float Fore = 70f;
            public float BackArm = 110f;
            public int Flask = -1;
            public float Kneel;
            public bool Lying;
            public float Spill;
            public float Lens = 1f;
            public bool Smear;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            // Mexendo na bolsa, os frascos tilintando.
            float[] dig = { 120f, 128f, 116f, 124f };
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(new Pose { Y = i % 2 * 0.5f, BackArm = dig[i], Arm = 85f, Fore = 80f + (i % 2) * 8f, Lens = 0.8f + (i % 2) * 0.3f });
            }

            // Puxa um frasco e joga o braco pra tras.
            frames[Windup + 0] = Draw(new Pose { Lean = 6f, Arm = 60f, Fore = -20f, Flask = 0, Lens = 1.3f });
            frames[Windup + 1] = Draw(new Pose { Lean = -2f, Arm = -120f, Fore = -150f, Flask = 0, Lens = 1.6f });
            frames[Windup + 2] = Draw(new Pose { Lean = -8f, Y = -0.5f, Arm = -150f, Fore = -175f, Flask = 0, Lens = 2f });

            // Arremessa: o braco vem por cima e solta.
            frames[Throw + 0] = Draw(new Pose { Lean = 18f, Arm = -40f, Fore = -10f, Flask = 0, Smear = true, Lens = 2f });
            frames[Throw + 1] = Draw(new Pose { Lean = 22f, Arm = 20f, Fore = 40f, Lens = 1.4f });

            Pose hurt = new Pose { Lean = -14f, Arm = 120f, Fore = 150f, Lens = 0.5f };
            hurt.Sparks.Add((24, 22, 2));
            frames[Hurt] = Draw(hurt);

            frames[Death + 0] = Draw(new Pose { Lean = -16f, Arm = -60f, Fore = -90f, BackArm = 200f, Lens = 2f, Spill = 0.1f });
            frames[Death + 1] = Draw(new Pose { Kneel = 0.6f, Lean = 10f, Arm = 100f, Fore = 110f, Lens = 1f, Spill = 0.3f });
            frames[Death + 2] = Draw(new Pose { Kneel = 1f, Lean = 30f, Arm = 100f, Fore = 100f, Lens = 0.5f, Spill = 0.55f });
            for (int i = 0; i < 3; i++) frames[Death + 3 + i] = Draw(new Pose { Lying = true, Lens = 0.4f - i * 0.2f, Spill = 0.7f + i * 0.15f });

            return frames;
        }

        private sealed class Frame
        {
            public PixelCanvas Canvas = new PixelCanvas(Width, Height);
            public Pose Pose;
            public float HX;
            public float HY;
            public float SX;
            public float SY;
            public float HeadX;
            public float HeadY;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var f = new Frame { Pose = pose };

            if (pose.Lying)
            {
                DrawLying(f);
                ArtKit.AddOutline(f.Canvas);
                DrawSpill(f);
                return f.Canvas;
            }

            f.HX = 22f;
            f.HY = 36f + pose.Y + pose.Kneel * 8f;
            (f.SX, f.SY) = ArtKit.Along(f.HX, f.HY, -90f + pose.Lean, 14f);
            (f.HeadX, f.HeadY) = ArtKit.Along(f.SX, f.SY, -90f + pose.Lean + 30f, 8f);

            DrawBackArm(f);
            DrawLeg(f, true);
            DrawLeg(f, false);
            DrawBody(f);
            DrawHead(f);
            DrawArm(f);

            ArtKit.AddOutline(f.Canvas);

            if (pose.Lens > 1.2f) ArtKit.Glow(f.Canvas, f.HeadX + 3f, f.HeadY - 1f, 6f, LabArt.OxideGreen, 0.2f * pose.Lens, true);
            DrawSpill(f);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(f.Canvas, spark.X, spark.Y, spark.Size);
            return f.Canvas;
        }

        private static void DrawLeg(Frame f, bool back)
        {
            Pose pose = f.Pose;
            float hipX = f.HX + (back ? -2f : 2f);
            float hipY = f.HY + 1f;
            float footX = 22f + (back ? -4f : 4f);
            float footY = Ground - 2f;
            float bias = back ? -0.45f : 0f;
            float kx, ky;

            if (pose.Kneel > 0.5f && back)
            {
                kx = hipX + 5f;
                ky = Ground - 3f;
                footX = kx - 10f;
            }
            else
            {
                (kx, ky) = ArtKit.Joint(hipX, hipY, footX, footY, 9f, 9f, -1);
            }

            ArtKit.Taper(f.Canvas, hipX, hipY, kx, ky, 2.8f, 2.4f, D4.Trousers, bias);
            ArtKit.Taper(f.Canvas, kx, ky, footX, footY, 2.4f, 2f, D4.Trousers, bias);
            ArtKit.Drum(f.Canvas, footX + 1.5f, footY + 0.5f, 4f, 2f, 3f, ArtKit.Plain(ArtKit.Leather), bias);
        }

        private static void DrawBody(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float hx = f.HX;
            float hy = f.HY;
            float sx = f.SX;
            float sy = f.SY;

            // O jaleco comprido, aberto embaixo, manchado.
            var coat = new[]
            {
                (sx - 6f, sy - 1f), (sx + 6f, sy), (hx + 7f, hy + 3f), (hx + 6f, hy + 10f), (hx + 1f, hy + 8f), (hx - 4f, hy + 11f), (hx - 8f, hy + 9f),
            };
            ArtKit.Polygon(c, coat, (x, y) =>
            {
                float light = 0.5f - (x - hx + 6f) / 14f;
                if (ArtKit.Noise(x * 0.35f, y * 0.35f, 7201) > 0.78f) return ArtKit.Tone(D4.Oxide, light - 0.2f);
                return ArtKit.Tone(D4.Coat, light);
            });

            // O colete escuro por baixo, na abertura da frente.
            ArtKit.Line(c, (int)sx + 3, (int)sy + 1, (int)hx + 4, (int)hy + 2, D4.Trousers[1]);
            ArtKit.Line(c, (int)sx + 4, (int)sy + 1, (int)hx + 5, (int)hy + 2, D4.Trousers[2]);

            // A bolsa de couro no quadril, de tras.
            ArtKit.Drum(c, hx - 7f, hy + 1f, 4f, 4f, 3f, ArtKit.Plain(ArtKit.Leather), 0f);
            c.Fill((int)hx - 10, (int)hy - 3, 7, 1, ArtKit.Leather[3]);
            ArtKit.Rivet(c, (int)hx - 7, (int)hy - 1, ArtKit.Brass);

            // A bandoleira com os frascos coloridos, atravessando o peito.
            ArtKit.Line(c, (int)sx - 4, (int)sy, (int)hx + 5, (int)hy - 1, ArtKit.Leather[1]);
            ArtKit.Line(c, (int)sx - 4, (int)sy + 1, (int)hx + 5, (int)hy, ArtKit.Leather[2]);
            for (int i = 0; i < 4; i++)
            {
                float t = 0.15f + i * 0.22f;
                float vx = sx - 4f + (hx + 5f - sx + 4f) * t;
                float vy = sy + (hy - 1f - sy) * t;
                D4.Vial(c, vx, vy - 1f, D4.Potions[i], 0.6f);
            }
        }

        /// <summary>A cabeca de cobre, careca e rebitada, os oculos de protecao e o pano na boca.</summary>
        private static void DrawHead(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float hx = f.HeadX;
            float hy = f.HeadY;

            ArtKit.Limb(c, f.SX + 1f, f.SY, hx - 1f, hy + 2f, 2.4f, ArtKit.Copper, 0f);
            ArtKit.Ball(c, hx, hy, 5.5f, 5.5f, ArtKit.Corroded(ArtKit.Copper, 0.18f, 7202, 0.4f), 0.15f);
            ArtKit.Rivet(c, (int)hx - 3, (int)hy - 4, ArtKit.Copper);
            ArtKit.Rivet(c, (int)hx, (int)hy - 5, ArtKit.Copper);

            // A tira dos oculos e as lentes redondas.
            for (int x = (int)hx - 5; x <= (int)hx + 5; x++)
            {
                if (c.Get(x, (int)hy - 1).A == 0) continue;
                c.Set(x, (int)hy - 1, ArtKit.Leather[1]);
            }

            Rgba lens = f.Pose.Lens < 0.5f ? LabArt.OxideDark : LabArt.OxideGreen;
            ArtKit.Ball(c, hx + 3f, hy - 1f, 2.4f, 2.4f, ArtKit.Brass, 0.2f);
            c.Fill((int)hx + 2, (int)hy - 2, 2, 2, lens);
            if (f.Pose.Lens > 1.5f) c.Set((int)hx + 2, (int)hy - 2, D4.Oxide[3]);

            // O pano amarrado na boca, manchado.
            ArtKit.Polygon(c, new[] { (hx - 1f, hy + 1f), (hx + 6f, hy + 1f), (hx + 5f, hy + 5f), (hx + 1f, hy + 5f) },
                (x, y) => ArtKit.Tone(D4.Coat, 0.2f - (y - hy) * 0.1f));
            c.Set((int)hx + 3, (int)hy + 3, LabArt.OxideDark);
            ArtKit.Line(c, (int)hx - 1, (int)hy + 1, (int)hx - 4, (int)hy + 4, D4.Coat[1]);
        }

        /// <summary>O braco de tras, enfiado na bolsa.</summary>
        private static void DrawBackArm(Frame f)
        {
            Pose pose = f.Pose;
            float sx = f.SX - 2f;
            float sy = f.SY + 1f;
            (float ex, float ey) = ArtKit.Along(sx, sy, pose.BackArm, 7f);
            (float hx, float hy) = ArtKit.Along(ex, ey, pose.BackArm - 30f, 6f);
            ArtKit.Limb(f.Canvas, sx, sy, ex, ey, 2.2f, D4.Coat, -0.45f);
            ArtKit.Limb(f.Canvas, ex, ey, hx, hy, 1.8f, ArtKit.Copper, -0.4f);
            ArtKit.Ball(f.Canvas, hx, hy, 1.8f, 1.8f, ArtKit.Copper, -0.3f);
        }

        /// <summary>O braco do arremesso, e o frasco na mao.</summary>
        private static void DrawArm(Frame f)
        {
            Pose pose = f.Pose;
            float sx = f.SX + 2f;
            float sy = f.SY + 1f;
            (float ex, float ey) = ArtKit.Along(sx, sy, pose.Arm, 7f);
            (float hx, float hy) = ArtKit.Along(ex, ey, pose.Fore, 7f);

            if (pose.Smear)
            {
                for (int i = 0; i < 3; i++)
                {
                    (float ax, float ay) = ArtKit.Along(sx, sy, pose.Arm - 40f - i * 30f, 14f);
                    ArtKit.Line(f.Canvas, (int)ax, (int)ay, (int)hx, (int)hy, new Rgba(240, 240, 230, 90));
                }
            }

            ArtKit.Limb(f.Canvas, sx, sy, ex, ey, 2.4f, D4.Coat, 0.15f);
            ArtKit.Limb(f.Canvas, ex, ey, hx, hy, 2f, ArtKit.Copper, 0.1f);
            ArtKit.Ball(f.Canvas, hx, hy, 2.2f, 2.2f, ArtKit.Copper, 0.2f);

            if (pose.Flask >= 0) D4.Vial(f.Canvas, hx + 1f, hy - 3f, D4.Potions[pose.Flask], 1.1f);
        }

        private static void DrawLying(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float g = Ground;
            ArtKit.Limb(c, 6f, g - 2f, 18f, g - 3f, 2.6f, D4.Trousers, -0.1f);
            ArtKit.Drum(c, 5f, g - 2.5f, 3f, 2f, 3f, ArtKit.Plain(ArtKit.Leather), 0f);
            ArtKit.Polygon(c, new[] { (14f, g - 1f), (34f, g - 1f), (34f, g - 9f), (16f, g - 8f) }, (x, y) => ArtKit.Tone(D4.Coat, 0.3f - (y - g + 9f) * 0.05f));
            ArtKit.Ball(c, 38f, g - 5f, 5f, 4.6f, ArtKit.Copper, 0.1f);
            c.Fill(39, (int)g - 7, 3, 2, LabArt.OxideDark);
            ArtKit.Limb(c, 30f, g - 3f, 42f, g - 1f, 1.8f, ArtKit.Copper, 0f);
        }

        /// <summary>O que vaza dos frascos quebrados: uma poca verde e respingos coloridos.</summary>
        private static void DrawSpill(Frame f)
        {
            float k = f.Pose.Spill;
            if (k <= 0f) return;

            int half = (int)(4f + k * 14f);
            for (int x = 24 - half; x <= 24 + half; x++)
            {
                f.Canvas.Set(x, (int)Ground, LabArt.OxideDark);
                if (Math.Abs(x - 24) < half - 2) f.Canvas.Set(x, (int)Ground - 1, LabArt.OxideGreen.WithAlpha(210));
            }

            for (int i = 0; i < 4; i++) D4.Splash(f.Canvas, 24f, Ground - 2f, 6f + k * 10f, D4.Potions[i], (int)(k * 10f) + i, 3);
        }
    }
}
