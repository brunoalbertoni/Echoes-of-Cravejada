using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// As cores que os corrompidos do District 4 dividem: o mercurio, a prata,
    /// o vidro, o jaleco e os frascos coloridos.
    /// </summary>
    internal static class D4
    {
        public static readonly Rgba[] Mercury = LabArt.Mercury;
        public static readonly Rgba[] Silver = LabArt.Silver;

        public static readonly Rgba[] Coat =
            { new Rgba(126, 130, 122), new Rgba(176, 180, 168), new Rgba(214, 216, 204), new Rgba(242, 242, 232) };

        public static readonly Rgba[] Glass =
            { new Rgba(70, 100, 132), new Rgba(130, 170, 200), new Rgba(190, 222, 240), new Rgba(244, 252, 255) };

        public static readonly Rgba[] Thread =
            { new Rgba(60, 40, 30), new Rgba(110, 80, 50), new Rgba(170, 130, 80), new Rgba(220, 190, 130) };

        public static readonly Rgba[] Trousers =
            { new Rgba(34, 32, 40), new Rgba(56, 52, 64), new Rgba(82, 78, 92), new Rgba(112, 108, 122) };

        public static readonly Rgba[] Oxide =
            { LabArt.OxideDark, new Rgba(70, 200, 126), LabArt.OxideGreen, new Rgba(200, 255, 220) };

        /// <summary>O vidro da sentinela: ciano, bem mais vivo que o dos frascos.</summary>
        public static readonly Rgba[] ArmorGlass =
            { new Rgba(46, 112, 142), new Rgba(96, 186, 214), new Rgba(164, 232, 246), new Rgba(240, 255, 255) };

        /// <summary>A prata polida da sentinela transmutada: clara, quase branca.</summary>
        public static readonly Rgba[] Polished =
            { new Rgba(92, 94, 110), new Rgba(160, 164, 178), new Rgba(222, 226, 236), new Rgba(255, 255, 255) };

        public static readonly Rgba[] Eye =
            { new Rgba(150, 156, 170), new Rgba(210, 214, 222), new Rgba(240, 242, 246), new Rgba(255, 255, 255) };

        /// <summary>As cores dos frascos da bandoleira: verde, azul, rosa, ambar.</summary>
        public static readonly Rgba[] Potions = { LabArt.OxideGreen, LabArt.Azure, LabArt.Rose, LabArt.Amber };

        public static readonly Rgba Ink = ArtKit.Outline;
        public static readonly Rgba Dark = new Rgba(24, 20, 28);

        /// <summary>Um frasquinho de vidro com o liquido de uma cor e a rolha.</summary>
        public static void Vial(PixelCanvas c, float x, float y, Rgba liquid, float size = 1f)
        {
            float r = 2.2f * size;
            ArtKit.Ball(c, x, y, r, r + 0.3f, Glass, 0.2f);
            for (int py = (int)y; py <= (int)(y + r); py++)
            {
                for (int px = (int)(x - r); px <= (int)(x + r); px++)
                {
                    if (c.Get(px, py).A > 0) c.Set(px, py, py == (int)y ? Rgba.Lerp(liquid, new Rgba(255, 255, 255), 0.4f) : liquid);
                }
            }

            c.Fill((int)x - 1, (int)(y - r - 2f), 2, 2, Glass[1]);
            c.Fill((int)x - 1, (int)(y - r - 3f), 2, 1, ArtKit.Leather[2]);
            c.Set((int)(x - r * 0.5f), (int)(y - r * 0.4f), Glass[3]);
        }

        /// <summary>Brilhinhos de vidro: pontinhos claros espalhados em volta de um ponto.</summary>
        public static void Sparkle(PixelCanvas c, float cx, float cy, float spread, int count, int seed, int alpha)
        {
            for (int i = 0; i < count; i++)
            {
                int x = (int)(cx - spread + ArtKit.Hash(i, seed, 7003) % (int)(spread * 2f + 1f));
                int y = (int)(cy - spread * 0.6f + ArtKit.Hash(i, seed, 7004) % (int)(spread * 1.2f + 1f));
                c.Blend(x, y, Glass[3].WithAlpha(alpha));
            }
        }

        public static void Splash(PixelCanvas c, float x, float y, float r, Rgba color, int seed, int count)
        {
            for (int i = 0; i < count; i++)
            {
                double a = Math.PI + (ArtKit.Hash(i, seed, 7001) % 100) / 100.0 * Math.PI;
                float d = r * (0.4f + (ArtKit.Hash(i, seed, 7002) % 60) / 100f);
                int px = (int)Math.Round(x + Math.Cos(a) * d);
                int py = (int)Math.Round(y + Math.Sin(a) * d * 0.7);
                c.Set(px, py, color);
                if (i % 2 == 0) c.Set(px + 1, py, color);
            }
        }
    }

    // ======================================================================
    // Gosma de Mercurio
    // ======================================================================

    /// <summary>
    /// A Gosma de Mercurio: uma bola gorda de mercurio vivo, espelhada — o
    /// laboratorio inteiro reflete nela. Dentro, uma engrenagem de latao gira;
    /// um tubo de ensaio quebrado ficou cravado no alto; boiando na frente,
    /// dois olhos de vidro com a iris verde do Oxide. Rasteja como lagarta:
    /// junta a de tras e estica a da frente. Morrendo, se esparrama numa poca
    /// e os olhos rolam pro chao. 48x36, pivo na base, no meio.
    /// </summary>
    public static class SlimeArt
    {
        public const int Width = 48;
        public const int Height = 36;
        public const int PixelsPerUnit = 32;
        public const float PivotY = 1f / Height;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Crawl = 4;
        public const int CrawlFrames = 6;
        public const int Hurt = 10;
        public const int Death = 11;
        public const int DeathFrames = 6;
        public const int FrameCount = 17;

        private const float Ground = 34f;

        private sealed class Pose
        {
            public float Squash;
            public float Lean;
            public float Gear;
            public float EyeY;
            public bool Squint;
            public float Puddle;
            public int Drops;
            public int Seed;
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            float[] wobble = { 0f, 0.05f, 0.1f, 0.05f };
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(new Pose { Squash = wobble[i], Gear = i * 15f, EyeY = i == 2 ? 1f : 0f });
            }

            // Rastejando: junta a de tras (alta e curta), estica a da frente (baixa e comprida).
            float[] squash = { -0.12f, -0.05f, 0.08f, 0.18f, 0.12f, 0f };
            float[] lean = { -1f, -0.4f, 0.4f, 1f, 0.6f, 0f };
            for (int i = 0; i < CrawlFrames; i++)
            {
                frames[Crawl + i] = Draw(new Pose { Squash = squash[i], Lean = lean[i], Gear = i * 25f, EyeY = -squash[i] * 6f, Drops = i % 3 == 0 ? 1 : 0, Seed = i });
            }

            frames[Hurt] = Draw(new Pose { Squash = 0.22f, Squint = true, Drops = 4, Seed = 9 });

            for (int i = 0; i < DeathFrames; i++)
            {
                frames[Death + i] = Draw(new Pose { Puddle = (i + 1) / (float)DeathFrames, Squint = true, Drops = i < 3 ? 5 : 0, Seed = 20 + i, Gear = 90f });
            }

            return frames;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var c = new PixelCanvas(Width, Height);
            float k = pose.Puddle;
            float cx = 24f + pose.Lean * 2f;
            float rx = 13f * (1f + pose.Squash) + k * 8f;
            float ry = Math.Max(2f, 11f * (1f - pose.Squash) * (1f - k * 0.85f));
            float cy = Ground - ry;

            ArtKit.Skin mirror = (x, y, light) =>
            {
                // O laboratorio refletido: a faixa escura do horizonte e a janela clara.
                if (y > -ry * 0.05f && y < ry * 0.2f) light -= 0.45f;
                if (x < -rx * 0.25f && x > -rx * 0.55f && y < -ry * 0.35f && y > -ry * 0.7f) return D4.Mercury[3];
                return ArtKit.Tone(D4.Mercury, light);
            };

            // As duas metades da lagarta e o corpo.
            if (k < 0.5f)
            {
                ArtKit.Ball(c, cx - rx * 0.6f - pose.Lean * 2f, Ground - ry * 0.4f, rx * 0.45f, ry * 0.42f, mirror, 0f);
                ArtKit.Ball(c, cx + rx * 0.55f + pose.Lean * 3f, Ground - ry * 0.45f, rx * 0.5f, ry * 0.5f, mirror, 0.1f);
            }

            ArtKit.Ball(c, cx, cy, rx, ry, mirror, 0.1f);

            // A base, espalhada no chao.
            int x0 = (int)(cx - rx - 2f - k * 4f);
            int x1 = (int)(cx + rx + 2f + k * 4f);
            for (int x = x0; x <= x1; x++)
            {
                c.Set(x, (int)Ground, D4.Mercury[0]);
                c.Set(x, (int)Ground - 1, D4.Mercury[1]);
            }

            // A engrenagem la dentro, vista pelo metal liquido.
            float gx = cx - 3f - k * 6f;
            float gy = k > 0.6f ? Ground - 3f : cy + 2f;
            DrawGear(c, gx, gy, 4f, pose.Gear, k > 0.6f ? 255 : 170);

            // O tubo de ensaio quebrado, cravado no alto.
            if (k < 0.4f)
            {
                float tx = cx - 6f;
                float ty = cy - ry + 3f;
                ArtKit.Taper(c, tx, ty, tx - 4f, ty - 8f, 2f, 1.8f, D4.Glass, 0.2f);
                c.Set((int)tx - 4, (int)ty - 9, D4.Glass[3]);
                c.Set((int)tx - 3, (int)ty - 2, LabArt.Rose);
                c.Set((int)tx - 2, (int)ty - 1, LabArt.Rose);
            }

            // O veio verde do Oxide.
            ArtKit.Crack(c, cx - 8f, cy + 3f, 20f, 7, 7101, 0.8f);

            // Os olhos de vidro: boiando na frente; morta, rolaram pro chao.
            if (k < 0.5f)
            {
                DrawEye(c, cx + 5f, cy - 3f + pose.EyeY, pose.Squint);
                DrawEye(c, cx + 10.5f, cy - 1.5f + pose.EyeY, pose.Squint);
            }
            else
            {
                DrawEye(c, 6f, Ground - 2.5f, true);
                DrawEye(c, 42f, Ground - 2.5f, true);
            }

            ArtKit.AddOutline(c);

            for (int i = 0; i < pose.Drops; i++)
            {
                float dx = (ArtKit.Hash(i, pose.Seed, 7102) % 30) - 15f;
                float dy = 6f + ArtKit.Hash(i, pose.Seed, 7103) % 10;
                ArtKit.Ball(c, cx + dx, cy - dy * (1f - k), 1.4f, 1.4f, D4.Mercury, 0.4f);
            }

            if (k < 0.5f)
            {
                ArtKit.Glow(c, cx + 5f, cy - 3f, 4f, LabArt.OxideGreen, 0.3f, true);
                ArtKit.Glow(c, cx + 10.5f, cy - 1.5f, 4f, LabArt.OxideGreen, 0.3f, true);
            }

            return c;
        }

        private static void DrawGear(PixelCanvas c, float x, float y, float r, float angle, int alpha)
        {
            for (int i = 0; i < 8; i++)
            {
                (float tx, float ty) = ArtKit.Along(x, y, angle + i * 45f, r + 1.2f);
                c.Blend((int)tx, (int)ty, ArtKit.Brass[1].WithAlpha(alpha));
            }

            for (int py = (int)(y - r); py <= (int)(y + r); py++)
            {
                for (int px = (int)(x - r); px <= (int)(x + r); px++)
                {
                    float dx = px + 0.5f - x;
                    float dy = py + 0.5f - y;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d > r) continue;
                    Rgba color = d < 1.2f ? ArtKit.Brass[0] : ArtKit.Tone(ArtKit.Brass, -dx / r * 0.5f - dy / r * 0.6f);
                    c.Blend(px, py, color.WithAlpha(alpha));
                }
            }
        }

        private static void DrawEye(PixelCanvas c, float x, float y, bool squint)
        {
            ArtKit.Ball(c, x, y, 2.8f, 2.8f, D4.Eye, 0.3f);
            if (squint)
            {
                ArtKit.Line(c, (int)x - 2, (int)y, (int)x + 2, (int)y, D4.Ink);
                return;
            }

            c.Fill((int)x, (int)y - 1, 2, 2, LabArt.OxideGreen);
            c.Set((int)x + 1, (int)y, D4.Dark);
            c.Set((int)x - 1, (int)y - 2, new Rgba(255, 255, 255));
        }
    }

    // ======================================================================
    // Arremessador de Frascos
    // ======================================================================

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

    // ======================================================================
    // Experimento Costurado
    // ======================================================================

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

    // ======================================================================
    // Sentinela de Vidro
    // ======================================================================

    /// <summary>
    /// A Sentinela de Vidro: uma armadura oca de vidro soprado, com uma
    /// alabarda de cristal. Por dentro, o que a move: uma chama verde de Oxide
    /// tremulando, que aparece pelo vidro. Golpeada, transmuta em prata macica
    /// — opaca, polida, so a fresta do elmo acesa. Duas folhas com os mesmos
    /// quadros: a de vidro e a de prata. Morrendo (so morre de vidro), racha
    /// inteira, vira um monte de cacos e a chama escapa. 56x64, pivo na base.
    /// </summary>
    public static class GlassSentinelArt
    {
        public const int Width = 56;
        public const int Height = 64;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 24f / Width;
        public const float PivotY = 1f / Height;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Walk = 4;
        public const int WalkFrames = 8;
        public const int Hurt = 12;
        public const int Death = 13;
        public const int DeathFrames = 6;
        public const int FrameCount = 19;

        private const float Ground = 62f;

        private sealed class Pose
        {
            public float Y;
            public float BackFoot = -4f;
            public float FrontFoot = 4f;
            public float BackLift;
            public float FrontLift;
            public float Flame;
            public float Spear;
            public float Tilt;
            public float Shatter;
        }

        public static PixelCanvas[] Frames(bool silver)
        {
            var frames = new PixelCanvas[FrameCount];

            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(silver, new Pose { Y = i == 2 ? 0.5f : 0f, Flame = i, Spear = (i % 2) * 2f });
            }

            for (int i = 0; i < WalkFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / WalkFrames;
                float swing = (float)Math.Sin(phase);
                float bob = (float)Math.Abs(Math.Cos(phase));
                frames[Walk + i] = Draw(silver, new Pose
                {
                    Y = (1f - bob) * 1.5f, BackFoot = -swing * 6f, FrontFoot = swing * 6f,
                    BackLift = swing > 0.2f ? swing * 3f : 0f, FrontLift = swing < -0.2f ? -swing * 3f : 0f,
                    Flame = i, Spear = swing * 3f,
                });
            }

            frames[Hurt] = Draw(silver, new Pose { Tilt = -6f, Flame = 2, Spear = -4f });

            for (int i = 0; i < DeathFrames; i++)
            {
                frames[Death + i] = Draw(silver, new Pose { Shatter = (i + 1) / (float)DeathFrames, Flame = i });
            }

            return frames;
        }

        private static PixelCanvas Draw(bool silver, Pose pose)
        {
            var c = new PixelCanvas(Width, Height);

            if (pose.Shatter > 0f)
            {
                DrawShatter(c, silver, pose);
                return c;
            }

            float hx = 24f + pose.Tilt * 0.2f;
            float hy = 38f + pose.Y;

            // A chama de dentro: desenhada antes, aparece pelo vidro.
            if (!silver)
            {
                DrawFlame(c, hx, hy - 12f, 1.2f, (int)pose.Flame);
                DrawFlame(c, hx + 2f, hy - 25f, 0.5f, (int)pose.Flame + 1);
            }

            // A armadura, numa camada propria: vidro e translucido, prata nao.
            var armor = new PixelCanvas(Width, Height);
            Rgba[] ramp = silver ? D4.Polished : D4.ArmorGlass;
            DrawArmor(armor, ramp, silver, pose, hx, hy);
            ArtKit.AddOutline(armor, silver ? ArtKit.Outline : new Rgba(30, 60, 84));

            for (int i = 0; i < armor.Pixels.Length; i++)
            {
                Rgba p = armor.Pixels[i];
                if (p.A == 0) continue;

                int x = i % Width;
                int y = i / Width;
                bool edge = p.R == 30 && p.G == 60;
                bool shine = p.R == ramp[3].R && p.G == ramp[3].G;
                int alpha = silver || edge || shine ? 255 : (p.R == ramp[0].R ? 200 : 115);
                c.Blend(x, y, p.WithAlpha(alpha));
            }

            // Prata: so a fresta do elmo acesa.
            if (silver)
            {
                c.Fill((int)hx + 1, (int)(hy - 24f), 5, 1, LabArt.OxideGreen);
            }
            else
            {
                ArtKit.Glow(c, hx, hy - 12f, 12f, LabArt.OxideGreen, 0.35f, true);
            }

            return c;
        }

        private static void DrawArmor(PixelCanvas c, Rgba[] ramp, bool silver, Pose pose, float hx, float hy)
        {
            ArtKit.Skin skin = silver ? ArtKit.Plain(ramp) : (x, y, light) => ArtKit.Tone(ramp, light + 0.1f);

            // A alabarda, atras do braco.
            float spearX = hx + 14f;
            float top = hy - 34f + pose.Spear * 0.3f;
            ArtKit.Limb(c, spearX, top + 6f, spearX - 1f, Ground - 2f, 1.1f, ramp, 0.1f);
            ArtKit.Polygon(c, new[] { (spearX, top - 4f), (spearX + 3f, top + 4f), (spearX, top + 7f), (spearX - 3f, top + 4f) },
                (x, y) => ArtKit.Tone(ramp, 0.6f - (x - spearX) * 0.2f));
            ArtKit.Polygon(c, new[] { (spearX - 1f, top + 5f), (spearX - 8f, top + 3f), (spearX - 7f, top + 10f), (spearX - 1f, top + 9f) },
                (x, y) => ArtKit.Tone(ramp, 0.3f - (y - top) * 0.05f));

            // As pernas: as grevas ocas, com o joelho redondo.
            foreach (bool back in new[] { true, false })
            {
                float footX = 24f + (back ? pose.BackFoot - 3f : pose.FrontFoot + 3f);
                float footY = Ground - 2f - (back ? pose.BackLift : pose.FrontLift);
                float hipX = hx + (back ? -3f : 3f);
                (float kx, float ky) = ArtKit.Joint(hipX, hy + 1f, footX, footY, 11f, 11f, -1);
                float bias = back ? -0.4f : 0f;
                ArtKit.Taper(c, hipX, hy + 1f, kx, ky, 3.2f, 2.8f, skin, bias);
                ArtKit.Taper(c, kx, ky, footX, footY, 3f, 2.6f, skin, bias);
                ArtKit.Ball(c, kx, ky, 3f, 3f, ramp, bias + 0.3f);
                ArtKit.Drum(c, footX + 1.5f, footY + 0.5f, 4.5f, 2.2f, 3f, skin, bias);
            }

            // A saia de placas e o peitoral oco.
            ArtKit.Drum(c, hx, hy - 1f, 8f, 4f, 3f, skin, 0f);
            for (int x = (int)hx - 7; x <= (int)hx + 7; x += 3) c.Set(x, (int)hy, ramp[0]);
            ArtKit.Drum(c, hx, hy - 12f, 9f, 9f, 2.6f, skin, 0.1f);
            ArtKit.Line(c, (int)hx + 1, (int)hy - 20, (int)hx + 2, (int)hy - 5, ramp[0]);
            ArtKit.Line(c, (int)hx - 6, (int)hy - 18, (int)hx - 3, (int)hy - 8, ramp[3]);

            // As ombreiras.
            ArtKit.Ball(c, hx - 7f, hy - 19f, 4.6f, 3.4f, skin, 0.2f);
            ArtKit.Ball(c, hx + 7f, hy - 19f, 4.6f, 3.4f, skin, 0.3f);

            // O elmo, com a viseira e a crista.
            float ex = hx + 2f;
            float ey = hy - 26f;
            ArtKit.Drum(c, ex, ey, 5.5f, 6f, 2.4f, skin, 0.2f);
            c.Fill((int)ex - 1, (int)ey + 1, 7, 2, silver ? ArtKit.Outline : ramp[0]);
            for (int i = 0; i < 5; i++) c.Set((int)ex - 3 + i, (int)ey - 6 - (i % 2), ramp[3]);

            // O braco, segurando a alabarda.
            ArtKit.Limb(c, hx + 7f, hy - 18f, hx + 11f, hy - 10f, 2.6f, skin, 0.2f);
            ArtKit.Limb(c, hx + 11f, hy - 10f, spearX, hy - 13f + pose.Spear * 0.3f, 2.2f, skin, 0.2f);
            ArtKit.Ball(c, spearX, hy - 13f + pose.Spear * 0.3f, 2.6f, 2.6f, ramp, 0.3f);

            // O brilho do vidro (ou da prata polida).
            ArtKit.Line(c, (int)hx - 4, (int)hy - 16, (int)hx - 2, (int)hy - 12, ramp[3]);
            c.Set((int)ex - 3, (int)ey - 3, ramp[3]);
        }

        /// <summary>A chama verde que move a armadura, tremulando.</summary>
        private static void DrawFlame(PixelCanvas c, float x, float y, float strength, int frame)
        {
            float sway = frame % 2 == 0 ? -1f : 1f;
            var tongues = new[] { (x - 3f + sway * 0.5f, 7f), (x + 1f - sway * 0.5f, 10f), (x + 4f + sway, 6f) };
            foreach ((float tx, float h) in tongues)
            {
                ArtKit.Taper(c, tx, y + 6f, tx + sway, y + 6f - h * strength, 2.8f, 0.4f, D4.Oxide, 0.3f);
            }

            ArtKit.Ball(c, x, y + 5f, 4f, 3f, D4.Oxide, 0.4f);
            c.Set((int)x, (int)y + 4, D4.Oxide[3]);
        }

        private static void DrawShatter(PixelCanvas c, bool silver, Pose pose)
        {
            float k = pose.Shatter;
            Rgba[] ramp = silver ? D4.Polished : D4.ArmorGlass;

            // A chama escapando pra cima, se apagando.
            if (k < 0.85f) DrawFlame(c, 24f, 30f - k * 22f, 1f - k, pose.Flame >= 0 ? (int)pose.Flame : 0);

            if (k < 0.25f)
            {
                // Rachada inteira, ainda de pe.
                var armor = new PixelCanvas(Width, Height);
                DrawArmor(armor, ramp, silver, new Pose(), 24f, 38f);
                for (int i = 0; i < 5; i++) ArtKit.Line(armor, 18 + i * 3, 16 + i * 4, 24 + i * 2, 44 - i * 3, new Rgba(255, 255, 255));
                ArtKit.AddOutline(armor, new Rgba(30, 60, 84));
                for (int i = 0; i < armor.Pixels.Length; i++)
                {
                    Rgba p = armor.Pixels[i];
                    if (p.A > 0) c.Blend(i % Width, i / Width, p.WithAlpha(200));
                }

                return;
            }

            // Os cacos caindo e o monte no chao.
            float t = (k - 0.25f) / 0.75f;
            for (int i = 0; i < 22; i++)
            {
                float sx = 12f + ArtKit.Hash(i, 0, 7401) % 26;
                float sy0 = 10f + ArtKit.Hash(i, 0, 7402) % 44;
                float sy = sy0 + (Ground - 2f - sy0) * Math.Min(1f, t * 1.4f);
                float px = sx + (sx - 24f) * t * 0.5f;
                float s = 1.5f + ArtKit.Hash(i, 1, 7403) % 3;
                ArtKit.Polygon(c, new[] { (px, sy - s), (px + s, sy), (px - s * 0.6f, sy + s * 0.6f) },
                    (x, y) => ramp[1 + i % 3].WithAlpha(silver ? 255 : 200));
            }

            if (t > 0.6f) D4.Sparkle(c, 24f, Ground - 4f, 14f, 10, (int)(t * 10f), 160);
        }
    }

}
