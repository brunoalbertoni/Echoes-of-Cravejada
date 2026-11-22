using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A Valve Sentry: uma caldeirinha de latao num pedestal de cano, com um
    /// volante vermelho do lado, o manometro no pe e o bocal apontado pra
    /// frente. Antes de atirar o volante gira, o ponteiro sobe e o bocal
    /// esquenta; o tiro da coice. Morre estourando: o volante voa e o vapor
    /// escapa por todas as juntas. 48x56, pivo na base.
    /// </summary>
    public static class SentryArt
    {
        public const int Width = 48;
        public const int Height = 56;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 22f / Width;
        public const float PivotY = 1f / Height;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Charge = 4;
        public const int ChargeFrames = 3;
        public const int Fire = 7;
        public const int FireFrames = 3;
        public const int Hurt = 10;
        public const int Death = 11;
        public const int DeathFrames = 6;
        public const int FrameCount = 17;

        /// <summary>A boca do bocal, em tiles, a partir do pivo (olhando pra direita).</summary>
        public const float MuzzleX = 0.66f;
        public const float MuzzleY = 1.06f;

        private sealed class Pose
        {
            public float Recoil;
            public float Wheel;
            public float Needle = 0.2f;
            public float Heat;
            public float Tilt;
            public float Drop;
            public bool Flash;
            public float Eye = 1f;
            public bool WheelOff;
            public float WheelX;
            public float WheelY;
            public bool Fallen;
            public List<(float X, float Y, float R, int Alpha)> Steam = new List<(float, float, float, int)>();
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            for (int i = 0; i < IdleFrames; i++)
            {
                var pose = new Pose { Needle = 0.2f + (i % 2) * 0.05f, Wheel = 0f };
                pose.Steam.Add((12f, 18f - i * 2f, 1.5f + i * 0.4f, 120 - i * 25));
                frames[Idle + i] = Draw(pose);
            }

            // Carregando: o volante gira, o ponteiro sobe, o bocal esquenta, vaza vapor.
            for (int i = 0; i < ChargeFrames; i++)
            {
                var pose = new Pose { Wheel = 30f * (i + 1), Needle = 0.45f + i * 0.22f, Heat = (i + 1) / 3f, Eye = 1.4f + i * 0.3f };
                pose.Steam.Add((11f, 14f - i, 2f + i * 0.7f, 150));
                if (i == 2) pose.Steam.Add((30f, 13f, 2.2f, 140));
                frames[Charge + i] = Draw(pose);
            }

            // O tiro: coice, clarao na boca, e volta.
            Pose shot = new Pose { Recoil = 3f, Wheel = 100f, Needle = 0.3f, Heat = 1f, Flash = true, Eye = 2f };
            shot.Steam.Add((45f, 21f, 4f, 200));
            shot.Steam.Add((41f, 17f, 2.5f, 160));
            frames[Fire + 0] = Draw(shot);
            Pose after = new Pose { Recoil = 2f, Wheel = 110f, Needle = 0.2f, Heat = 0.6f };
            after.Steam.Add((46f, 18f, 3f, 130));
            frames[Fire + 1] = Draw(after);
            frames[Fire + 2] = Draw(new Pose { Recoil = 0.5f, Wheel = 115f, Needle = 0.2f, Heat = 0.3f });

            Pose hurt = new Pose { Recoil = 2f, Tilt = -12f, Needle = 0.9f, Eye = 0.6f };
            hurt.Sparks.Add((24, 14, 2));
            hurt.Steam.Add((14f, 22f, 2.5f, 160));
            frames[Hurt] = Draw(hurt);

            // A morte: o volante salta, o vapor escapa, a cabeca tomba e cai.
            Pose burst = new Pose { Needle = 1f, Eye = 2f, WheelOff = true, WheelX = 12f, WheelY = 10f, Wheel = 40f };
            burst.Steam.Add((16f, 21f, 4f, 210));
            burst.Steam.Add((22f, 38f, 3f, 180));
            burst.Sparks.Add((18, 18, 3));
            frames[Death + 0] = Draw(burst);

            Pose leak = new Pose { Needle = 1f, Eye = 1f, Tilt = 12f, Drop = 2f, WheelOff = true, WheelX = 6f, WheelY = 6f, Wheel = 160f };
            leak.Steam.Add((14f, 16f, 5f, 190));
            leak.Steam.Add((24f, 36f, 3.5f, 170));
            leak.Steam.Add((32f, 22f, 3f, 150));
            frames[Death + 1] = Draw(leak);

            Pose droop = new Pose { Needle = 0.6f, Eye = 0.5f, Tilt = 30f, Drop = 5f, WheelOff = true, WheelX = 4f, WheelY = 46f, Wheel = 260f };
            droop.Steam.Add((12f, 12f, 5.5f, 150));
            droop.Steam.Add((26f, 34f, 3f, 130));
            frames[Death + 2] = Draw(droop);

            for (int i = 0; i < 3; i++)
            {
                Pose fallen = new Pose
                {
                    Fallen = true, Needle = 0f, Eye = i == 0 ? 0.4f : (i == 1 ? 0.15f : 0f), WheelOff = true, WheelX = 4f, WheelY = 49f, Wheel = 300f,
                };
                if (i < 2) fallen.Steam.Add((30f, 34f - i * 6f, 4f + i, 130 - i * 50));
                frames[Death + 3 + i] = Draw(fallen);
            }

            return frames;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var c = new PixelCanvas(Width, Height);
            ArtKit.Skin brass = ArtKit.Corroded(ArtKit.Brass, 0.28f, 621, 0.3f);

            // A base aparafusada no chao.
            ArtKit.Drum(c, 22f, 52f, 11f, 3.2f, 3f, ArtKit.Plain(ArtKit.Iron), 0.15f);
            for (int i = 0; i < 4; i++) ArtKit.Rivet(c, 13 + i * 5, 51, ArtKit.Iron);

            float headX = 22f - pose.Recoil;
            float headY = 21f + pose.Drop;

            if (pose.Fallen)
            {
                // A cabeca tombada no chao, o pedestal torto.
                ArtKit.Limb(c, 22f, 50f, 24f, 40f, 3.2f, brass, 0.1f);
                headX = 32f;
                headY = 44f;
            }
            else
            {
                ArtKit.Limb(c, 22f, 50f, 22f, headY + 6f, 3.2f, brass, 0.1f);
            }

            // O manometro no pe.
            ArtKit.Ball(c, 22f, 42f, 3.8f, 3.8f, ArtKit.Brass, 0.3f);
            ArtKit.Ball(c, 22f, 42f, 2.6f, 2.6f, D2.Gauge, 0.5f);
            (float nx, float ny) = ArtKit.Along(22f, 42f, -200f + pose.Needle * 220f, 2.2f);
            ArtKit.Line(c, 22, 42, (int)Math.Round(nx), (int)Math.Round(ny), new Rgba(170, 40, 30));

            double tilt = (pose.Fallen ? 80f : pose.Tilt) * Math.PI / 180.0;
            float Rx(float ox, float oy) => headX + ox * (float)Math.Cos(tilt) - oy * (float)Math.Sin(tilt);
            float Ry(float ox, float oy) => headY + ox * (float)Math.Sin(tilt) + oy * (float)Math.Cos(tilt);

            // O bocal, por tras da cabeca.
            float bx0 = Rx(6f, 1f), by0 = Ry(6f, 1f);
            float bx1 = Rx(19f, 1f), by1 = Ry(19f, 1f);
            ArtKit.Limb(c, bx0, by0, bx1, by1, 2.8f, ArtKit.Brass, 0.15f);
            ArtKit.Ball(c, bx1, by1, 2.2f, 3.6f, ArtKit.Iron, 0.1f);
            if (pose.Heat > 0f) ArtKit.Glow(c, bx1 - 2f, by1, 5f, D2.Fire[2], 0.5f * pose.Heat);

            // A cabeca: uma caldeirinha redonda, rebitada, com o olho verde.
            ArtKit.Ball(c, headX, headY, 9.5f, 9f, brass, 0.1f);
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4.0 + tilt;
                ArtKit.Rivet(c, (int)(headX + Math.Cos(a) * 7.3f), (int)(headY + Math.Sin(a) * 7f), ArtKit.Brass);
            }

            float ex = Rx(4.5f, -4f), ey = Ry(4.5f, -4f);
            Rgba[] lens = pose.Eye <= 0.05f
                ? new[] { ArtKit.Iron[0], ArtKit.Iron[1], ArtKit.Iron[1], ArtKit.Iron[2] }
                : new[] { ArtKit.OxideDeep, new Rgba(80, 210, 130), ArtKit.OxideGlow, ArtKit.OxideCore };
            ArtKit.Ball(c, ex, ey, 2.6f, 2.6f, lens, pose.Eye > 1.5f ? 0.5f : 0.1f);

            // O volante vermelho do lado, ou o buraco onde ele estava.
            if (!pose.WheelOff)
            {
                DrawWheel(c, Rx(-3f, 1f), Ry(-3f, 1f), pose.Wheel);
            }
            else
            {
                ArtKit.Ball(c, Rx(-3f, 1f), Ry(-3f, 1f), 1.8f, 1.8f, ArtKit.Iron, -0.3f);
                DrawWheel(c, pose.WheelX + 6f, pose.WheelY, pose.Wheel);
            }

            ArtKit.AddOutline(c);

            if (pose.Flash)
            {
                ArtKit.Glow(c, bx1 + 3f, by1, 7f, D2.Fire[3], 0.9f, true);
                ArtKit.Spark(c, (int)bx1 + 3, (int)by1, 3);
            }

            if (pose.Eye > 1.4f) ArtKit.Glow(c, ex, ey, 5f, ArtKit.OxideGlow, 0.35f * (pose.Eye - 1f), true);
            foreach ((float X, float Y, float R, int Alpha) puff in pose.Steam) D2.Steam(c, puff.X, puff.Y, puff.R, puff.Alpha);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(c, spark.X, spark.Y, spark.Size);
            return c;
        }

        /// <summary>O volante: aro vermelho, quatro raios girando e o cubo.</summary>
        private static void DrawWheel(PixelCanvas c, float cx, float cy, float angle)
        {
            for (int s = 0; s < 4; s++)
            {
                (float x, float y) = ArtKit.Along(cx, cy, angle + s * 90f, 5.5f);
                ArtKit.Limb(c, cx, cy, x, y, 0.9f, D2.RedPaint, 0.1f);
            }

            // O aro: um anel grosso, de dois pixels.
            for (int y = (int)(cy - 8f); y <= (int)(cy + 8f); y++)
            {
                for (int x = (int)(cx - 8f); x <= (int)(cx + 8f); x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d < 5f || d > 7f) continue;
                    c.Set(x, y, ArtKit.Tone(D2.RedPaint, -(dx * 0.55f + dy * 0.65f) / d * 0.9f));
                }
            }

            ArtKit.Ball(c, cx, cy, 2f, 2f, ArtKit.Brass, 0.3f);
        }
    }
}
