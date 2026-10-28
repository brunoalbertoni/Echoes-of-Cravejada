using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os quatro inimigos do District 2 no mesmo detalhe do Scrap (32 pixels
    /// por tile, sombreado em quatro tons, contorno escuro), cada um com o
    /// seu ataque, o tranco de apanhar e a morte. Olham pra direita.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    internal static class D2
    {
        public static readonly Rgba[] Acid =
            { new Rgba(70, 120, 26), new Rgba(124, 184, 36), new Rgba(190, 232, 66), new Rgba(238, 255, 168) };

        public static readonly Rgba[] Fire =
            { new Rgba(176, 60, 20), new Rgba(236, 120, 30), new Rgba(255, 186, 70), new Rgba(255, 244, 196) };

        public static readonly Rgba[] RedPaint =
            { new Rgba(80, 26, 26), new Rgba(134, 42, 36), new Rgba(178, 66, 52), new Rgba(214, 108, 84) };

        public static readonly Rgba[] Gauge =
            { new Rgba(150, 140, 110), new Rgba(192, 182, 152), new Rgba(224, 216, 190), new Rgba(242, 238, 218) };

        public static readonly Rgba[] Glass =
            { new Rgba(40, 60, 64), new Rgba(70, 100, 104), new Rgba(120, 160, 160), new Rgba(200, 230, 228) };

        public static readonly Rgba SteamLight = new Rgba(240, 244, 240);
        public static readonly Rgba SteamDark = new Rgba(186, 196, 194);

        public static void Steam(PixelCanvas c, float x, float y, float r, int alpha)
        {
            ArtKit.Puff(c, x, y, r, SteamLight, SteamDark, alpha);
        }

        /// <summary>Um ponto numa curva de Bezier quadratica.</summary>
        public static (float X, float Y) Bezier((float X, float Y) a, (float X, float Y) b, (float X, float Y) c, float t)
        {
            float u = 1f - t;
            return (u * u * a.X + 2f * u * t * b.X + t * t * c.X, u * u * a.Y + 2f * u * t * b.Y + t * t * c.Y);
        }

        /// <summary>Um cone de fogo de um ponto, numa direcao: miolo branco, borda laranja, pontas soltas.</summary>
        public static void Flame(PixelCanvas c, float x, float y, float angle, float length, float width, int seed)
        {
            if (length <= 0.5f) return;
            double a = angle * Math.PI / 180.0;
            float ux = (float)Math.Cos(a);
            float uy = (float)Math.Sin(a);

            for (int py = (int)(y - length - width) - 2; py <= (int)(y + length + width) + 2; py++)
            {
                for (int px = (int)(x - length - width) - 2; px <= (int)(x + length + width) + 2; px++)
                {
                    float rx = px + 0.5f - x;
                    float ry = py + 0.5f - y;
                    float along = (rx * ux + ry * uy) / length;
                    if (along < 0f || along > 1f) continue;

                    float across = Math.Abs(-rx * uy + ry * ux);
                    float half = width * (0.35f + 0.65f * (float)Math.Sin(Math.Min(1f, along * 1.6f) * Math.PI * 0.5)) * (1f - along * 0.55f);
                    half += (ArtKit.Noise(along * 9f, seed, seed) - 0.5f) * 1.6f;
                    if (across > half) continue;

                    float k = across / Math.Max(0.5f, half) + along * 0.7f;
                    Rgba color = k < 0.45f ? Fire[3] : (k < 0.8f ? Fire[2] : (k < 1.15f ? Fire[1] : Fire[0]));
                    int alpha = along > 0.8f ? 170 : 245;
                    c.Blend(px, py, color.WithAlpha(alpha));
                }
            }
        }
    }

    // ======================================================================
    // Pipe Leech
    // ======================================================================

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

    // ======================================================================
    // Valve Sentry
    // ======================================================================

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

    // ======================================================================
    // Brass Welder
    // ======================================================================

    /// <summary>
    /// O Brass Welder: um Tarnished corrompido, de corpo de barril de latao,
    /// avental de couro, mascara de soldador com o visor verde, botijao nas
    /// costas e o macarico na mao. Apara de frente com a manopla (faisca);
    /// ataca com a chama. Ao morrer, larga o macarico e a mascara cai,
    /// mostrando o rosto tomado de verde. 80x56, pivo na base, embaixo dele.
    /// </summary>
    public static class WelderArt
    {
        public const int Width = 80;
        public const int Height = 56;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 24f / Width;
        public const float PivotY = 1f / Height;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Walk = 4;
        public const int WalkFrames = 8;
        public const int Windup = 12;
        public const int WindupFrames = 2;
        public const int Flame = 14;
        public const int FlameFrames = 3;
        public const int Block = 17;
        public const int BlockFrames = 2;
        public const int Hurt = 19;
        public const int Death = 20;
        public const int DeathFrames = 6;
        public const int FrameCount = 26;

        private const float Ground = 54f;

        private sealed class Pose
        {
            public float X;
            public float Y;
            public float Lean;
            public float BackFoot = -3f;
            public float FrontFoot = 3f;
            public float BackLift;
            public float FrontLift;

            /// <summary>Onde fica a mao do macarico (relativo ao ombro) e pra onde ele aponta.</summary>
            public float HandX = 7f;
            public float HandY = 6f;
            public float TorchAngle = 10f;
            public float FlameLength = 3f;

            public bool Guard;
            public float Eye = 1f;
            public bool TorchDropped;
            public bool MaskOff;
            public float Kneel;
            public float Slump;
            public bool Lying;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            float[] breath = { 0f, 0.5f, 1f, 0.5f };
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(new Pose { Y = breath[i], HandY = 6f + breath[i], FlameLength = 2.5f + (i % 2) });
            }

            for (int i = 0; i < WalkFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / WalkFrames;
                float swing = (float)Math.Sin(phase);
                float bob = (float)Math.Abs(Math.Cos(phase));
                frames[Walk + i] = Draw(new Pose
                {
                    Y = 1f - bob, Lean = 1f, BackFoot = -swing * 4f, FrontFoot = swing * 4f,
                    BackLift = swing > 0.2f ? swing * 2.5f : 0f, FrontLift = swing < -0.2f ? -swing * 2.5f : 0f,
                    HandY = 6f + (1f - bob), TorchAngle = 10f + swing * 4f, FlameLength = 2.5f + (i % 2),
                });
            }

            // O ataque: recolhe o macarico, a chama cresce, e solta o jato.
            frames[Windup + 0] = Draw(new Pose { X = -1f, Lean = -1f, HandX = 2f, HandY = 4f, TorchAngle = -20f, FlameLength = 6f, Eye = 1.5f, BackFoot = -4f });
            frames[Windup + 1] = Draw(new Pose { X = -2f, Lean = -2f, HandX = 0f, HandY = 2f, TorchAngle = -35f, FlameLength = 9f, Eye = 2f, BackFoot = -5f });
            frames[Flame + 0] = Draw(new Pose { X = 2f, Lean = 2f, HandX = 10f, HandY = 6f, TorchAngle = 5f, FlameLength = 24f, Eye = 2f, BackFoot = -5f, FrontFoot = 5f });
            frames[Flame + 1] = Draw(new Pose { X = 2f, Lean = 2f, HandX = 10f, HandY = 7f, TorchAngle = 8f, FlameLength = 28f, Eye = 2f, BackFoot = -5f, FrontFoot = 5f });
            frames[Flame + 2] = Draw(new Pose { X = 1f, Lean = 1f, HandX = 9f, HandY = 7f, TorchAngle = 10f, FlameLength = 14f, Eye = 1.5f, BackFoot = -4f, FrontFoot = 4f });

            // Aparando: a manopla na frente da mascara, faisca no metal.
            Pose guard = new Pose { X = -1f, Guard = true, HandX = 4f, HandY = -6f, TorchAngle = -80f, FlameLength = 2f, BackFoot = -4f };
            guard.Sparks.Add((38, 18, 3));
            guard.Sparks.Add((40, 24, 1));
            frames[Block + 0] = Draw(guard);
            frames[Block + 1] = Draw(new Pose { X = -1f, Guard = true, HandX = 4f, HandY = -5f, TorchAngle = -80f, FlameLength = 2f, BackFoot = -4f });

            Pose hurt = new Pose { X = -3f, Lean = -3f, HandX = 4f, HandY = 9f, TorchAngle = 40f, FlameLength = 1f, Eye = 0.6f, FrontLift = 2f };
            hurt.Sparks.Add((24, 22, 2));
            frames[Hurt] = Draw(hurt);

            // A morte: cambaleia, larga o macarico, ajoelha, a mascara cai, tomba.
            frames[Death + 0] = Draw(new Pose { X = -3f, Lean = -4f, HandX = 6f, HandY = 10f, TorchAngle = 70f, FlameLength = 0f, Eye = 2f });
            frames[Death + 1] = Draw(new Pose { X = -2f, Lean = -2f, Kneel = 0.5f, HandX = 6f, HandY = 12f, TorchDropped = true, Eye = 1.2f });
            frames[Death + 2] = Draw(new Pose { Kneel = 1f, Slump = 2f, HandX = 6f, HandY = 14f, TorchDropped = true, MaskOff = true, Eye = 1f });
            frames[Death + 3] = Draw(new Pose { Kneel = 1f, Slump = 5f, HandX = 7f, HandY = 16f, TorchDropped = true, MaskOff = true, Eye = 0.5f });
            frames[Death + 4] = Draw(new Pose { Lying = true, TorchDropped = true, MaskOff = true, Eye = 0.25f });
            frames[Death + 5] = Draw(new Pose { Lying = true, TorchDropped = true, MaskOff = true, Eye = 0f });

            return frames;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var c = new PixelCanvas(Width, Height);

            if (pose.Lying)
            {
                DrawLying(c, pose);
                ArtKit.AddOutline(c);
                return c;
            }

            ArtKit.Skin brass = ArtKit.Corroded(ArtKit.Brass, 0.22f, 631, 0.35f);
            float drop = pose.Kneel * 9f;
            float bx = 24f + pose.X;
            float by = 33f + pose.Y + drop + pose.Slump * 0.4f;
            float shoulderX = bx + 4f + pose.Lean * 0.5f + pose.Slump * 0.6f;
            float shoulderY = by - 6f + pose.Slump * 0.5f;
            float handX = shoulderX + pose.HandX;
            float handY = shoulderY + pose.HandY;

            // O botijao nas costas e a mangueira.
            ArtKit.Drum(c, bx - 9f, by - 3f, 4f, 8f, 2.5f, ArtKit.Corroded(ArtKit.Copper, 0.3f, 632, 0.5f), 0.1f);
            ArtKit.Ball(c, bx - 9f, by - 11.5f, 1.8f, 1.2f, ArtKit.Brass, 0.3f);
            if (!pose.TorchDropped)
            {
                (float, float) mid = (bx - 4f, by + 8f);
                for (int i = 0; i <= 10; i++)
                {
                    (float x, float y) = D2.Bezier((bx - 9f, by + 5f), mid, (handX - 2f, handY + 1f), i / 10f);
                    c.Set((int)x, (int)y, ArtKit.Rubber[2]);
                    c.Set((int)x, (int)y + 1, ArtKit.Rubber[0]);
                }
            }

            // O braco de tras.
            ArtKit.Limb(c, bx - 4f, by - 5f, bx - 5f + pose.Slump, by + 3f, 1.8f, ArtKit.Brass, -0.4f);
            ArtKit.Ball(c, bx - 5f + pose.Slump, by + 4f, 2.2f, 2.2f, ArtKit.Leather, -0.3f);

            // As pernas.
            DrawLeg(c, pose, bx - 2.5f, by + 6f, true);
            DrawLeg(c, pose, bx + 2.5f, by + 6f, false);

            // O tronco de barril e o avental.
            ArtKit.Ball(c, bx, by, 8f, 9f, brass, 0.05f);
            var apron = new[] { (bx - 1f, by - 4f), (bx + 7f, by - 3f), (bx + 7.5f, by + 9f), (bx - 2f, by + 9.5f) };
            ArtKit.Polygon(c, apron, (x, y) => ArtKit.Tone(ArtKit.Leather, 0.25f - (x - bx) * 0.05f - (y - by) * 0.04f));
            for (int i = 0; i < 4; i++) c.Set((int)(bx + i * 2f - 1f), (int)(by + 4f), ArtKit.Leather[0]);
            ArtKit.Rivet(c, (int)(bx - 5f), (int)(by - 4f), ArtKit.Brass);
            ArtKit.Rivet(c, (int)(bx - 5f), (int)(by + 2f), ArtKit.Brass);

            DrawHead(c, pose, bx + 1.5f + pose.Lean + pose.Slump, by - 15f + pose.Slump * 0.8f);

            // O macarico: o cano de latao e o bico; caido, no chao.
            if (pose.TorchDropped)
            {
                ArtKit.Limb(c, bx + 6f, Ground - 1.5f, bx + 17f, Ground - 2.5f, 1.4f, ArtKit.Brass, 0.1f);
                ArtKit.Ball(c, bx + 17.5f, Ground - 2.5f, 1.4f, 1.4f, ArtKit.Iron, 0f);
            }
            else
            {
                (float tx, float ty) = ArtKit.Along(handX, handY, pose.TorchAngle, 10f);
                ArtKit.Limb(c, handX - 1f, handY, tx, ty, 1.5f, ArtKit.Brass, 0.15f);
                ArtKit.Taper(c, tx, ty, ArtKit.Along(tx, ty, pose.TorchAngle, 3f).X, ArtKit.Along(tx, ty, pose.TorchAngle, 3f).Y, 1.4f, 0.8f, ArtKit.Iron, 0.1f);
            }

            // O braco da frente com a manopla de couro.
            ArtKit.Ball(c, shoulderX, shoulderY, 2.6f, 2.6f, brass, 0.1f);
            ArtKit.Limb(c, shoulderX, shoulderY, handX, handY, 1.9f, ArtKit.Brass, 0.05f);
            ArtKit.Ball(c, handX, handY, pose.Guard ? 3.4f : 2.6f, pose.Guard ? 3.4f : 2.6f, ArtKit.Leather, 0.15f);

            ArtKit.AddOutline(c);

            if (!pose.TorchDropped)
            {
                (float tx, float ty) = ArtKit.Along(handX, handY, pose.TorchAngle, 13.5f);
                D2.Flame(c, tx, ty, pose.TorchAngle, pose.FlameLength, pose.FlameLength > 10f ? 5f : 2f, 641 + (int)pose.FlameLength);
                if (pose.FlameLength > 10f) ArtKit.Glow(c, tx + 6f, ty, 10f, D2.Fire[2], 0.5f, true);
            }

            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(c, spark.X, spark.Y, spark.Size);
            return c;
        }

        private static void DrawLeg(PixelCanvas c, Pose pose, float hipX, float hipY, bool back)
        {
            float bias = back ? -0.4f : 0f;
            float footX = 24f + pose.X * 0.4f + (back ? pose.BackFoot - 1f : pose.FrontFoot + 1f);
            float footY = Ground - 2f - (back ? pose.BackLift : pose.FrontLift);

            if (pose.Kneel > 0f)
            {
                // Ajoelhado: o joelho no chao, o pe pra tras.
                float kneeX = hipX + (back ? -1f : 4f);
                float kneeY = Ground - 2f;
                ArtKit.Limb(c, hipX, hipY, kneeX, kneeY, 2.2f, ArtKit.Leather, bias);
                ArtKit.Limb(c, kneeX, kneeY, kneeX - 7f, Ground - 1.5f, 2f, ArtKit.Leather, bias);
                ArtKit.Ball(c, kneeX - 7f, Ground - 1.5f, 2.6f, 1.6f, ArtKit.Iron, bias);
                return;
            }

            (float kx, float ky) = ArtKit.Joint(hipX, hipY, footX, footY, 7f, 7.5f, -1);
            ArtKit.Limb(c, hipX, hipY, kx, ky, 2.2f, ArtKit.Leather, bias);
            ArtKit.Limb(c, kx, ky, footX, footY, 2f, ArtKit.Leather, bias);
            ArtKit.Ball(c, footX + 1f, footY + 0.5f, 3.2f, 1.8f, ArtKit.Iron, bias);
        }

        /// <summary>A cabeca: a mascara de soldador com o visor verde, ou o rosto, sem ela.</summary>
        private static void DrawHead(PixelCanvas c, Pose pose, float hx, float hy)
        {
            // O capuz de couro por tras.
            ArtKit.Ball(c, hx - 2f, hy + 1f, 7.5f, 7.5f, ArtKit.Leather, -0.1f);

            if (pose.MaskOff)
            {
                // O rosto de latao, as juntas tomadas de verde, os olhos apagando.
                ArtKit.Ball(c, hx + 1f, hy, 6.5f, 6.5f, ArtKit.Corroded(ArtKit.Brass, 0.5f, 651, 0.6f), 0.1f);
                Rgba eye = pose.Eye <= 0.05f ? ArtKit.Iron[0] : (pose.Eye < 0.5f ? ArtKit.OxideDeep : ArtKit.OxideGlow);
                c.Fill((int)(hx + 2f), (int)(hy - 1f), 2, 2, eye);
                c.Fill((int)(hx + 5f), (int)(hy - 1f), 2, 2, eye);
                ArtKit.Line(c, (int)(hx - 2f), (int)(hy - 5f), (int)(hx + 1f), (int)(hy + 2f), ArtKit.OxideDeep);

                // A mascara caida no chao, na frente.
                ArtKit.Drum(c, 40f, Ground - 3.5f, 5.5f, 3.5f, 3f, ArtKit.Plain(ArtKit.Iron), 0.1f);
                c.Fill(37, (int)Ground - 4, 6, 1, ArtKit.OxideDeep);
                return;
            }

            // A mascara: chapa de ferro arredondada, com a fresta do visor.
            ArtKit.Drum(c, hx + 1.5f, hy, 7f, 7.5f, 3f, ArtKit.Corroded(ArtKit.Iron, 0.15f, 652, 0.4f, ArtKit.Rust), 0.15f);
            int vx = (int)(hx + 1f);
            int vy = (int)(hy - 2f);
            c.Fill(vx, vy, 8, 3, ArtKit.Outline);
            Rgba glow = pose.Eye < 0.5f ? ArtKit.OxideDeep : ArtKit.OxideGlow;
            c.Fill(vx + 1, vy + 1, 6, 1, glow);
            if (pose.Eye > 1.5f) c.Fill(vx + 2, vy + 1, 4, 1, ArtKit.OxideCore);
            ArtKit.Rivet(c, (int)(hx - 4f), (int)(hy - 1f), ArtKit.Brass);
            for (int i = 0; i < 3; i++) c.Set((int)(hx + 2f + i * 2f), (int)(hy + 4f), ArtKit.Iron[0]);
        }

        /// <summary>Caido de bruços, o macarico e a mascara no chao.</summary>
        private static void DrawLying(PixelCanvas c, Pose pose)
        {
            ArtKit.Skin brass = ArtKit.Corroded(ArtKit.Brass, 0.3f, 631, 0.35f);
            ArtKit.Drum(c, 12f, Ground - 4f, 4.5f, 3.5f, 2.5f, ArtKit.Corroded(ArtKit.Copper, 0.3f, 632, 0.5f), 0f);
            ArtKit.Limb(c, 14f, Ground - 3f, 6f, Ground - 2f, 2.2f, ArtKit.Leather, -0.2f);
            ArtKit.Ball(c, 22f, Ground - 5f, 9f, 5f, brass, 0f);
            ArtKit.Ball(c, 33f, Ground - 4f, 5.5f, 4.5f, ArtKit.Corroded(ArtKit.Brass, 0.5f, 651, 0.6f), 0.05f);
            Rgba eye = pose.Eye <= 0.05f ? ArtKit.Iron[0] : ArtKit.OxideDeep;
            c.Fill(35, (int)Ground - 5, 2, 1, eye);
            ArtKit.Limb(c, 26f, Ground - 3f, 36f, Ground - 1.5f, 1.6f, ArtKit.Brass, 0f);
            ArtKit.Limb(c, 40f, Ground - 1.5f, 50f, Ground - 2f, 1.4f, ArtKit.Brass, 0.1f);
            ArtKit.Drum(c, 54f, Ground - 3.5f, 5.5f, 3.5f, 3f, ArtKit.Plain(ArtKit.Iron), 0.1f);
            c.Fill(51, (int)Ground - 4, 6, 1, ArtKit.OxideDeep);
        }
    }

    // ======================================================================
    // Acid Bloater
    // ======================================================================

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
