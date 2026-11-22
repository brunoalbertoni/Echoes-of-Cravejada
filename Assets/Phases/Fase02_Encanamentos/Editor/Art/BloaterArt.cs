using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Acid Bloater: um tanque de cobre com uma vigia de vidro onde o acido
    /// balanca, quatro pernas curtas, uma cupulinha com o olho e um bico de
    /// valvula na frente. Ataca borrifando acido; ao morrer, incha, o vidro
    /// racha e ele estoura (o estouro e a poca ficam com o BloaterBurst).
    /// 80x56, pivo na base, embaixo dele.
    /// </summary>
    public static class BloaterArt
    {
        public const int Width = 80;
        public const int Height = 56;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 26f / Width;
        public const float PivotY = 1f / Height;

        public const int Walk = 0;
        public const int WalkFrames = 6;
        public const int Windup = 6;
        public const int WindupFrames = 2;
        public const int Spray = 8;
        public const int SprayFrames = 3;
        public const int Hurt = 11;
        public const int Swell = 12;
        public const int SwellFrames = 4;
        public const int FrameCount = 16;

        private const float Ground = 54f;

        private sealed class Pose
        {
            public float Y;
            public float Slosh;
            public float[] Feet = new float[4];
            public float[] Lift = new float[4];
            public float Scale = 1f;
            public float Valve;
            public float Jet;
            public float Cracks;
            public float Foam;
            public float Eye = 1f;
            public bool Squint;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            for (int i = 0; i < WalkFrames; i++)
            {
                var pose = new Pose { Y = i % 3 == 1 ? -1f : 0f, Slosh = (float)Math.Sin(i * Math.PI / 3.0) * 2f };
                for (int leg = 0; leg < 4; leg++)
                {
                    int step = (i + (leg % 2 == 0 ? 0 : 3)) % 6;
                    pose.Feet[leg] = step < 4 ? 2f - step * 1.3f : -2f + (step - 4) * 2f;
                    pose.Lift[leg] = step >= 4 ? 2f : 0f;
                }

                frames[Walk + i] = Draw(pose);
            }

            frames[Windup + 0] = Draw(new Pose { Y = 1f, Valve = 45f, Slosh = 1f, Foam = 0.4f, Eye = 1.5f });
            frames[Windup + 1] = Draw(new Pose { Y = 2f, Valve = 90f, Slosh = -1f, Foam = 0.8f, Eye = 2f, Scale = 1.03f });

            frames[Spray + 0] = Draw(new Pose { Y = 0f, Valve = 120f, Jet = 0.6f, Slosh = -2f, Eye = 2f });
            frames[Spray + 1] = Draw(new Pose { Y = 0f, Valve = 130f, Jet = 1f, Slosh = -2.5f, Eye = 2f });
            frames[Spray + 2] = Draw(new Pose { Y = 0.5f, Valve = 140f, Jet = 0.45f, Slosh = -1f, Eye = 1.5f });

            Pose hurt = new Pose { Y = -1f, Slosh = 3f, Squint = true, Eye = 0.6f };
            hurt.Sparks.Add((22, 22, 2));
            frames[Hurt] = Draw(hurt);

            // Inchando antes de estourar: o vidro racha, o acido ferve.
            for (int i = 0; i < SwellFrames; i++)
            {
                frames[Swell + i] = Draw(new Pose
                {
                    Scale = 1.03f + i * 0.04f, Cracks = (i + 1) / (float)SwellFrames, Foam = 0.5f + i * 0.15f,
                    Slosh = (i % 2 == 0 ? 2f : -2f), Eye = 2f, Squint = i % 2 == 1,
                });
            }

            return frames;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var c = new PixelCanvas(Width, Height);
            float cx = 26f;
            float cy = 32f + pose.Y;
            float rx = 15f * pose.Scale;
            float ry = 14f * pose.Scale;

            // As quatro pernas: as de la primeiro, mais escuras.
            float[] hipX = { cx - 8f, cx + 6f, cx - 6f, cx + 9f };
            for (int leg = 0; leg < 4; leg++)
            {
                bool far = leg < 2;
                float bias = far ? -0.45f : 0f;
                float legX = hipX[leg] + (far ? 2f : 0f);
                float legY = cy + ry * 0.6f;
                float fx = hipX[leg] + pose.Feet[leg];
                float fy = Ground - 2f - pose.Lift[leg] - (far ? 1f : 0f);
                ArtKit.Limb(c, legX, legY, fx, fy, 2.4f, ArtKit.Iron, bias);
                ArtKit.Ball(c, fx + 1f, fy + 0.5f, 3f, 1.8f, ArtKit.Iron, bias + 0.1f);
            }

            // O bico de valvula na frente, com o volantinho.
            float nozzleY = cy + 2f;
            ArtKit.Limb(c, cx + rx - 2f, nozzleY, cx + rx + 7f, nozzleY, 2.4f, ArtKit.Brass, 0.15f);
            ArtKit.Drum(c, cx + rx + 7.5f, nozzleY, 1.6f, 3f, 3f, ArtKit.Plain(ArtKit.Iron), 0.1f);
            float wx = cx + rx + 3f;
            for (int s = 0; s < 4; s++)
            {
                (float x, float y) = ArtKit.Along(wx, nozzleY - 4f, pose.Valve + s * 90f, 2.6f);
                ArtKit.Line(c, (int)wx, (int)(nozzleY - 4f), (int)Math.Round(x), (int)Math.Round(y), D2.RedPaint[2]);
            }

            // O tanque de cobre, com cintas rebitadas.
            ArtKit.Ball(c, cx, cy, rx, ry, ArtKit.Corroded(ArtKit.Copper, 0.25f, 661, 0.25f), 0.05f);
            foreach (float u in new[] { -0.55f, 0.55f })
            {
                int x = (int)(cx + u * rx);
                for (int y = (int)(cy - ry); y <= (int)(cy + ry); y++)
                {
                    if (c.Get(x, y).A == 0) continue;
                    c.Set(x, y, ArtKit.Iron[1]);
                    if (y % 4 == 0) c.Set(x, y, ArtKit.Iron[3]);
                }
            }

            // A vigia de vidro, com o acido balancando la dentro.
            float gx = cx + 3f;
            float gy = cy - 2f;
            float gr = 8.5f * pose.Scale;
            ArtKit.Ball(c, gx, gy, gr + 1.6f, gr + 1.6f, ArtKit.Brass, 0.2f);
            for (int y = (int)(gy - gr) - 1; y <= (int)(gy + gr) + 1; y++)
            {
                for (int x = (int)(gx - gr) - 1; x <= (int)(gx + gr) + 1; x++)
                {
                    float dx = x + 0.5f - gx;
                    float dy = y + 0.5f - gy;
                    float d = (dx * dx + dy * dy) / (gr * gr);
                    if (d > 1f) continue;

                    float surface = gy - 1f + pose.Slosh * (dx / gr);
                    Rgba color;
                    if (y + 0.5f < surface) color = ArtKit.Tone(D2.Glass, -dy / gr * 0.6f - 0.1f);
                    else
                    {
                        float depth = (y + 0.5f - surface) / gr;
                        color = ArtKit.Tone(D2.Acid, 0.5f - depth * 1.2f - dx / gr * 0.3f);
                        if (Math.Abs(y + 0.5f - surface) < 1f) color = D2.Acid[3];
                        if (pose.Foam > 0f && ArtKit.Hash(x, y, 662 + (int)(pose.Foam * 10)) % 9 == 0) color = D2.Acid[3];
                    }

                    c.Set(x, y, color);
                }
            }

            c.Set((int)(gx - gr * 0.5f), (int)(gy - gr * 0.6f), new Rgba(240, 255, 250));
            c.Set((int)(gx - gr * 0.4f), (int)(gy - gr * 0.5f), new Rgba(200, 230, 228));

            if (pose.Cracks > 0f)
            {
                int cracks = 1 + (int)(pose.Cracks * 4f);
                for (int i = 0; i < cracks; i++)
                {
                    double a = i * 1.7 + 0.4;
                    ArtKit.Line(c, (int)gx, (int)gy, (int)(gx + Math.Cos(a) * gr), (int)(gy + Math.Sin(a) * gr), new Rgba(230, 255, 240, 220));
                }
            }

            // A cupulinha de cima, com o olho.
            float hx = cx - 6f;
            float hy = cy - ry - 2f;
            ArtKit.Ball(c, hx, hy, 4.5f, 4f, ArtKit.Iron, 0.15f);
            ArtKit.Limb(c, hx - 1f, hy - 4f, hx - 1f, hy - 7f, 0.7f, ArtKit.Brass, 0.3f);
            Rgba eye = pose.Eye < 0.5f ? ArtKit.OxideDeep : ArtKit.OxideGlow;
            c.Fill((int)(hx + 1f), (int)(hy - 1f), 3, pose.Squint ? 1 : 2, eye);
            if (pose.Eye > 1.5f) c.Set((int)(hx + 3f), (int)(hy - 1f), ArtKit.OxideCore);

            ArtKit.AddOutline(c);

            // O borrifo: um jato de acido em leque, saindo do bico.
            if (pose.Jet > 0f)
            {
                float jx = cx + rx + 9f;
                float length = 22f * pose.Jet;
                for (int i = 0; i < 26; i++)
                {
                    float t = i / 25f * length;
                    float spread = 1f + t * 0.18f;
                    for (int k = -1; k <= 1; k++)
                    {
                        int px = (int)(jx + t);
                        int py = (int)(nozzleY + t * 0.12f + k * spread * 0.6f + Math.Sin(t * 0.8 + k) * 0.6f);
                        Rgba color = k == 0 ? D2.Acid[3] : D2.Acid[2];
                        c.Blend(px, py, color.WithAlpha(t > length * 0.8f ? 160 : 240));
                    }
                }

                for (int d = 0; d < 6; d++)
                {
                    int px = (int)(jx + length * (0.4f + d * 0.11f));
                    int py = (int)(nozzleY + 3f + (d % 3) * 2f);
                    c.Set(px, py, D2.Acid[2]);
                }
            }

            if (pose.Cracks > 0.5f) ArtKit.Glow(c, gx, gy, gr + 6f, D2.Acid[2], 0.4f * pose.Cracks, true);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(c, spark.X, spark.Y, spark.Size);
            return c;
        }
    }
}
