using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A Pipe Leech: uma sanguessuga de mangueira de borracha, com aneis de
    /// cobre entre os gomos e uma ventosa dentada na boca. Fica grudada no
    /// cano do teto, pulsando; despenca em cima de quem passa, se arrasta
    /// como lagarta e da o bote. Ao morrer, fura e murcha vazando Oxide.
    /// 48x48; o pivo e a base, no meio.
    /// </summary>
    public static class LeechArt
    {
        public const int Size = 48;
        public const int PixelsPerUnit = 32;
        public const float PivotY = 1f / Size;

        public const int Crawl = 0;
        public const int CrawlFrames = 6;
        public const int Cling = 6;
        public const int ClingFrames = 4;
        public const int Drop = 10;
        public const int DropFrames = 2;
        public const int Land = 12;
        public const int BiteWindup = 13;
        public const int BiteWindupFrames = 2;
        public const int Bite = 15;
        public const int BiteFrames = 3;
        public const int Hurt = 18;
        public const int Death = 19;
        public const int DeathFrames = 6;
        public const int FrameCount = 25;

        private const float Ground = 46f;
        private const int Segments = 7;

        private sealed class Pose
        {
            public (float X, float Y)[] Spine;
            public float Scale = 1f;
            public float SquashY = 1f;
            public float Mouth = 0.3f;
            public float Eye = 1f;
            public bool Squint;
            public List<(float X, float Y, float Angle, float Length)> Leaks = new List<(float, float, float, float)>();
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
            public float Puddle;
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            // Rastejando como lagarta: arqueia, junta o rabo, estica a cabeca.
            float[] arch = { 0f, 3f, 7f, 9f, 7f, 3f };
            for (int i = 0; i < CrawlFrames; i++)
            {
                float tail = 9f + arch[i] * 0.7f;
                float head = 38f - arch[i] * 0.25f;
                frames[Crawl + i] = Draw(new Pose { Spine = Flat(tail, head, arch[i]), Mouth = 0.25f + (i % 3) * 0.05f });
            }

            // No cano: pendurada pela ventosa, balancando e pulsando.
            float[] sway = { -2f, 0f, 2f, 0f };
            for (int i = 0; i < ClingFrames; i++)
            {
                var spine = new (float, float)[Segments];
                for (int s = 0; s < Segments; s++)
                {
                    float t = s / (Segments - 1f);
                    float down = 1f - t;
                    spine[s] = (24f + sway[i] * down * down + (float)Math.Sin(t * 3.0 + i) * 0.6f, 7f + down * 27f);
                }

                frames[Cling + i] = Draw(new Pose { Spine = spine, Scale = 0.95f + (i % 2) * 0.05f, Mouth = 0.1f });
            }

            // Solta do cano, se enrolando.
            frames[Drop + 0] = Draw(new Pose { Spine = Arc(24f, 22f, 10f, 160f, -40f), Mouth = 0.6f });
            frames[Drop + 1] = Draw(new Pose { Spine = Arc(24f, 30f, 8.5f, 230f, -10f), Mouth = 0.4f, Squint = true });

            // Esborrachada no chao.
            frames[Land] = Draw(new Pose { Spine = Flat(7f, 41f, 0f), SquashY = 0.7f, Mouth = 0.5f, Squint = true });

            // O bote: empina a cabeca com a ventosa aberta...
            frames[BiteWindup + 0] = Draw(new Pose
            {
                Spine = Curve((8f, 42f), (22f, 44f), (28f, 26f)), Mouth = 0.7f, Eye = 1.6f,
            });
            frames[BiteWindup + 1] = Draw(new Pose
            {
                Spine = Curve((6f, 42f), (18f, 45f), (24f, 20f)), Mouth = 1f, Eye = 2f,
            });

            // ...e da o bote rente ao chao, fechando a boca.
            frames[Bite + 0] = Draw(new Pose { Spine = Curve((10f, 42f), (26f, 40f), (41f, 35f)), Mouth = 1f, Eye = 2f });
            Pose snap = new Pose { Spine = Curve((12f, 42f), (28f, 41f), (43f, 37f)), Mouth = 0f, Eye = 2f };
            snap.Sparks.Add((47, 36, 2));
            frames[Bite + 1] = Draw(snap);
            frames[Bite + 2] = Draw(new Pose { Spine = Curve((10f, 42f), (24f, 42f), (37f, 38f)), Mouth = 0.2f });

            // Apanhando: retorcida.
            Pose hurt = new Pose { Spine = Curve((8f, 41f), (16f, 28f), (33f, 33f)), Mouth = 0.8f, Squint = true, Eye = 0.6f };
            hurt.Sparks.Add((20, 28, 2));
            frames[Hurt] = Draw(hurt);

            // A morte: fura, vaza verde, murcha e achata.
            float[] scale = { 1f, 0.92f, 0.82f, 0.7f, 0.58f, 0.5f };
            float[] squash = { 1f, 0.9f, 0.75f, 0.6f, 0.45f, 0.38f };
            float[] eye = { 2f, 1f, 0.6f, 0.3f, 0.1f, 0f };
            for (int i = 0; i < DeathFrames; i++)
            {
                Pose dying = new Pose
                {
                    Spine = Flat(7f + i * 0.3f, 40f - i * 0.5f, i == 0 ? 6f : Math.Max(0f, 3f - i)),
                    Scale = scale[i], SquashY = squash[i], Mouth = i < 2 ? 1f : 0.6f, Eye = eye[i], Squint = i > 0,
                    Puddle = i < 2 ? 0f : (i - 1) / 4f,
                };

                if (i < 4)
                {
                    dying.Leaks.Add((18f, 34f + i * 2f, -100f + i * 15f, 7f - i));
                    dying.Leaks.Add((28f, 35f + i * 2f, -70f - i * 10f, 6f - i));
                }

                if (i == 0) dying.Sparks.Add((22, 30, 2));
                frames[Death + i] = Draw(dying);
            }

            return frames;
        }

        private static (float, float)[] Flat(float tail, float head, float arch)
        {
            var spine = new (float, float)[Segments];
            for (int s = 0; s < Segments; s++)
            {
                float t = s / (Segments - 1f);
                float r = Radius(t);
                spine[s] = (tail + (head - tail) * t, Ground - r - arch * (float)Math.Sin(t * Math.PI));
            }

            return spine;
        }

        private static (float, float)[] Curve((float X, float Y) tail, (float X, float Y) control, (float X, float Y) head)
        {
            var spine = new (float, float)[Segments];
            for (int s = 0; s < Segments; s++) spine[s] = D2.Bezier(tail, control, head, s / (Segments - 1f));
            return spine;
        }

        private static (float, float)[] Arc(float cx, float cy, float r, float from, float to)
        {
            var spine = new (float, float)[Segments];
            for (int s = 0; s < Segments; s++)
            {
                double a = (from + (to - from) * s / (Segments - 1f)) * Math.PI / 180.0;
                spine[s] = (cx + (float)Math.Cos(a) * r, cy + (float)Math.Sin(a) * r);
            }

            return spine;
        }

        private static float Radius(float t) => 2.8f + 3.6f * (float)Math.Pow(t, 0.7);

        private static PixelCanvas Draw(Pose pose)
        {
            var c = new PixelCanvas(Size, Size);

            if (pose.Puddle > 0f)
            {
                float w = 10f + pose.Puddle * 12f;
                for (int px = (int)(24f - w); px <= (int)(24f + w); px++)
                {
                    c.Set(px, 45, ArtKit.OxideDeep);
                    if (Math.Abs(px - 24f) < w - 2f) c.Set(px, 44, ArtKit.OxideGlow.WithAlpha(200));
                }
            }

            ArtKit.Skin rubber = ArtKit.Corroded(ArtKit.Rubber, 0.08f, 601, 0.5f, new[]
            {
                new Rgba(44, 60, 56), new Rgba(62, 84, 76), new Rgba(84, 110, 98), new Rgba(110, 140, 124),
            });

            (float X, float Y)[] spine = pose.Spine;
            for (int s = 0; s < spine.Length; s++)
            {
                float t = s / (spine.Length - 1f);
                float r = Radius(t) * pose.Scale;
                float ry = r * pose.SquashY;
                (float x, float y) = spine[s];
                if (pose.SquashY < 1f) y += r - ry;

                // O anel de cobre, um pouco maior, atras de gomo sim, gomo nao.
                if (s % 2 == 1 && s < spine.Length - 1)
                {
                    ArtKit.Ball(c, x, y, r + 0.9f, ry + 0.9f, ArtKit.Corroded(ArtKit.Copper, 0.3f, 602 + s, 0.6f), 0.15f);
                }

                ArtKit.Ball(c, x, y, r, ry, rubber, 0f);

                // O brilho molhado da borracha.
                c.Set((int)(x - r * 0.4f), (int)(y - ry * 0.5f), ArtKit.Rubber[3]);
            }

            DrawHead(c, pose, spine);
            ArtKit.AddOutline(c);

            foreach ((float X, float Y, float Angle, float Length) leak in pose.Leaks)
            {
                (float tx, float ty) = ArtKit.Along(leak.X, leak.Y, leak.Angle, leak.Length);
                ArtKit.Line(c, (int)leak.X, (int)leak.Y, (int)tx, (int)ty, ArtKit.OxideGlow);
                c.Set((int)tx, (int)ty, ArtKit.OxideCore);
            }

            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(c, spark.X, spark.Y, spark.Size);
            return c;
        }

        /// <summary>A cabeca: a ventosa dentada na ponta e os dois olhinhos verdes em cima.</summary>
        private static void DrawHead(PixelCanvas c, Pose pose, (float X, float Y)[] spine)
        {
            (float hx, float hy) = spine[spine.Length - 1];
            (float px, float py) = spine[spine.Length - 2];
            float r = Radius(1f) * pose.Scale;
            if (pose.SquashY < 1f) hy += r - r * pose.SquashY;

            float dx = hx - px;
            float dy = hy - py;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            if (length < 0.01f) { dx = 1f; dy = 0f; length = 1f; }
            dx /= length;
            dy /= length;

            // A ventosa: um disco de lado, perpendicular ao corpo.
            float mx = hx + dx * (r - 0.5f);
            float my = hy + dy * (r - 0.5f);
            float major = (3.4f + pose.Mouth * 2.4f) * pose.Scale;
            float minor = 1.4f + pose.Mouth * 1.6f;

            for (int y = (int)(my - major) - 2; y <= (int)(my + major) + 2; y++)
            {
                for (int x = (int)(mx - major) - 2; x <= (int)(mx + major) + 2; x++)
                {
                    float rx = x + 0.5f - mx;
                    float ry = y + 0.5f - my;
                    float along = rx * dx + ry * dy;
                    float across = -rx * dy + ry * dx;
                    float d = along * along / (minor * minor) + across * across / (major * major);
                    if (d > 1f) continue;

                    Rgba color = d < 0.45f && pose.Mouth > 0.15f
                        ? new Rgba(60, 20, 26)
                        : (d > 0.7f ? ArtKit.Copper[2] : ArtKit.Copper[1]);

                    // Os dentes em volta da boca.
                    if (d > 0.45f && d < 0.7f && pose.Mouth > 0.15f && ArtKit.Hash(x, y, 611) % 2 == 0) color = ArtKit.Tin[3];
                    c.Set(x, y, color);
                }
            }

            // Os olhinhos, no lado de cima da cabeca.
            float ux = dy;
            float uy = -dx;
            if (uy > 0f) { ux = -ux; uy = -uy; }
            for (int e = 0; e < 2; e++)
            {
                // Dois pontinhos de 2x2 no alto da cabeca, um atras do outro.
                int ex = (int)Math.Floor(hx + ux * r * 0.35f + dx * (e * 3f - 2f));
                int ey = (int)Math.Floor(hy + uy * r * 0.35f + dy * (e * 3f - 2f));
                Rgba glow = pose.Eye <= 0.05f ? ArtKit.Rubber[0] : (pose.Eye < 0.5f ? ArtKit.OxideDeep : ArtKit.OxideGlow);
                c.Fill(ex, ey, 2, pose.Squint ? 1 : 2, glow);
                if (pose.Eye >= 1f && !pose.Squint) c.Set(ex, ey, ArtKit.OxideCore);
            }
        }
    }
}
