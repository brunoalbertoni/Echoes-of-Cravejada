using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Alquimista de Mercurio, no mesmo traco do Coletor e do Mestre das
    /// Caldeiras: o Alquimista-Chefe dos Ateliers, que entrou no tanque grande
    /// pra SER mercurio. Da cintura pra cima ainda e ele: o jaleco comprido
    /// manchado de prata, o manto de couro com rebites, o cinto de frascos
    /// coloridos, a bolsa, as luvas; a mascara de bico, com as lentes acesas no
    /// verde do Oxide e a mangueira ate o tanque respirador das costas. Da
    /// cintura pra baixo e uma coluna de mercurio que escorre numa poca.
    ///
    /// Cada quadro tem 160x160, olha pra direita e e importado com 32 pixels
    /// por tile (5 x 5 tiles). O pivo e o meio da poca, no chao.
    ///
    /// Em prata (o que os circulos fazem com ele), o mesmo desenho e refeito
    /// inteiro em prata macica, parado, com brilho: e a unica hora em que ele
    /// apanha.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class AlchemistArt
    {
        public const int Width = 160;
        public const int Height = 160;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 0.5f;
        public const float PivotY = 4f / Height;

        public const int Idle = 0;
        public const int IdleFrames = 6;
        public const int Glide = 6;
        public const int GlideFrames = 6;
        public const int BellCall = 12;
        public const int BellCallFrames = 3;
        public const int Distill = 15;
        public const int DistillFrames = 3;
        public const int Vapor = 18;
        public const int VaporFrames = 3;
        public const int Condense = 21;
        public const int CondenseFrames = 3;
        public const int WhipWindup = 24;
        public const int WhipWindupFrames = 2;
        public const int Whip = 26;
        public const int WhipFrames = 3;
        public const int Silver = 29;
        public const int SilverFrames = 2;
        public const int BreakFree = 31;
        public const int BreakFreeFrames = 3;
        public const int Smother = 34;
        public const int SmotherFrames = 2;
        public const int Hurt = 36;
        public const int Laugh = 37;
        public const int LaughFrames = 4;
        public const int Death = 41;
        public const int DeathFrames = 8;
        public const int FrameCount = 49;

        /// <summary>Onde a mao da frente fica no golpe da pipeta, em tiles a partir do pivo: de la sai o chicote.</summary>
        public static (float X, float Y) WhipHand => ((HandOf(WhipPose(2)).X - Width * PivotX) / PixelsPerUnit, (Ground - HandOf(WhipPose(2)).Y) / PixelsPerUnit);

        private const float Ground = 156f;
        private const float CenterX = 80f;

        /// <summary>O mercurio dele: cromado, mais escuro que o jaleco, com reflexo forte.</summary>
        private static readonly Rgba[] Mercury =
            { new Rgba(46, 52, 72), new Rgba(98, 108, 134), new Rgba(170, 182, 206), new Rgba(246, 250, 255) };
        private static readonly Rgba[] SilverRamp = LabArt.Silver;
        private static readonly Rgba[] Leather = ArtKit.Leather;
        private static readonly Rgba[] Brass = ArtKit.Brass;
        private static readonly Rgba[] Copper = ArtKit.Copper;

        private static readonly Rgba[] Coat =
            { new Rgba(118, 122, 120), new Rgba(170, 174, 168), new Rgba(210, 212, 202), new Rgba(240, 240, 230) };

        private static readonly Rgba[] Lens =
            { new Rgba(20, 70, 46), new Rgba(40, 150, 96), new Rgba(120, 255, 170), new Rgba(232, 255, 238) };

        private static readonly Rgba Ink = ArtKit.Outline;

        private sealed class Pose
        {
            public float X;
            public float Y;
            public float Lean;

            /// <summary>Quanto de altura sobra (1 em pe, 0 todo na poca).</summary>
            public float Height = 1f;

            /// <summary>A largura da poca (1 normal).</summary>
            public float Puddle = 1f;

            /// <summary>O braco da frente (graus, 0 = pra frente, 90 = pra baixo) e o antebraco.</summary>
            public float Arm = 70f;
            public float Fore = 40f;

            /// <summary>O braco de tras.</summary>
            public float Back = 100f;
            public float BackFore = 80f;

            public float HeadTilt;
            public float Glow = 0.6f;

            /// <summary>Uma bola de mercurio juntando na mao da frente (raio em px).</summary>
            public float Blob;

            /// <summary>O chicote saindo da mao, em px (o resto e um objeto a parte).</summary>
            public float Whip;

            /// <summary>Evaporando: 0 nada, 1 tudo nuvem.</summary>
            public float Vapor;

            /// <summary>Fervendo: bolhas na coluna.</summary>
            public float Boil;

            public bool Silver;
            public float Cracks;
            public bool Glint;
            public bool Chip;

            /// <summary>Mercurio derramando das maos, pro chao (apagando o circulo).</summary>
            public bool Pour;

            /// <summary>O riso: a boca do bico aberta e os ombros sacudindo.</summary>
            public float Laugh;

            /// <summary>Escorrendo pelo ralo: so a poca, encolhendo, e a mascara no chao.</summary>
            public float Drain;

            public int Phase;
        }

        // ------------------------------------------------------------------
        // Os quadros
        // ------------------------------------------------------------------

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            float[] breath = { 0f, 0.6f, 1.1f, 1.2f, 0.7f, 0.2f };
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(new Pose { Y = breath[i], HeadTilt = breath[i] * 1.5f, Arm = 72f + breath[i] * 2f, Phase = i, Glow = 0.5f + i % 3 * 0.15f });
            }

            for (int i = 0; i < GlideFrames; i++)
            {
                float sway = (float)Math.Sin(i * Math.PI / 3.0);
                frames[Glide + i] = Draw(new Pose
                {
                    Lean = 7f, Y = Math.Abs(sway), Puddle = 1.15f, Arm = 95f + sway * 10f, Fore = 70f, Back = 120f - sway * 10f, Phase = i, HeadTilt = -3f,
                });
            }

            // O chamado da redoma: aponta pro teto, a luva acende.
            for (int i = 0; i < BellCallFrames; i++)
            {
                frames[BellCall + i] = Draw(new Pose { Lean = -4f, Arm = -80f + i * 2f, Fore = -95f, HeadTilt = -8f, Glow = 0.8f + i * 0.1f, Phase = i, Blob = i * 1.2f });
            }

            // Destilando: ferve, encolhe e sobe em vapor.
            for (int i = 0; i < DistillFrames; i++)
            {
                float k = (i + 1) / (float)(DistillFrames + 1);
                frames[Distill + i] = Draw(new Pose { Boil = 0.5f + k, Vapor = k * 0.7f, Height = 1f - k * 0.6f, Arm = 40f, Fore = -20f, Back = 160f, Phase = i, Glow = 1f });
            }

            for (int i = 0; i < VaporFrames; i++) frames[Vapor + i] = Draw(new Pose { Vapor = 1f, Phase = i });

            for (int i = 0; i < CondenseFrames; i++)
            {
                float k = 1f - (i + 1) / (float)(CondenseFrames + 1);
                frames[Condense + i] = Draw(new Pose { Vapor = k * 0.7f, Height = 1f - k * 0.6f, Boil = k, Arm = 50f, Fore = 0f, Phase = i + 2, Glow = 0.9f });
            }

            for (int i = 0; i < WhipWindupFrames; i++) frames[WhipWindup + i] = Draw(WhipWindupPose(i));
            for (int i = 0; i < WhipFrames; i++) frames[Whip + i] = Draw(WhipPose(i));

            // Em prata: a pose de quem ia atacar, parada.
            for (int i = 0; i < SilverFrames; i++) frames[Silver + i] = Draw(SilverPose(i == 1, false, 0f));

            for (int i = 0; i < BreakFreeFrames; i++)
            {
                Pose pose = SilverPose(false, false, 0.35f + i * 0.3f);
                pose.X = i % 2 == 0 ? -1.5f : 1.5f;
                if (i == BreakFreeFrames - 1) { pose.Silver = false; pose.Boil = 1f; }
                frames[BreakFree + i] = Draw(pose);
            }

            // Apagando o circulo: curvado, derramando mercurio das duas maos.
            for (int i = 0; i < SmotherFrames; i++)
            {
                frames[Smother + i] = Draw(new Pose { Lean = 14f, Y = 6f, Arm = 75f, Fore = 85f, Back = 70f, BackFore = 85f, HeadTilt = 12f, Pour = true, Phase = i, Puddle = 1.25f });
            }

            frames[Hurt] = Draw(SilverPose(false, true, 0.2f));

            // O riso, quando os tanques racham.
            for (int i = 0; i < LaughFrames; i++)
            {
                float shake = i % 2 == 0 ? 1.5f : -1.5f;
                frames[Laugh + i] = Draw(new Pose
                {
                    Lean = -6f, Y = Math.Abs(shake), X = shake * 0.5f, Arm = -40f + shake * 4f, Fore = -60f, Back = -140f, BackFore = -110f,
                    HeadTilt = -12f + shake, Laugh = 1f, Glow = 1f, Phase = i, Puddle = 1.2f + i * 0.05f,
                });
            }

            // A morte: escorre, o jaleco murcha, a mascara cai por ultimo.
            for (int i = 0; i < DeathFrames; i++)
            {
                float k = (i + 1) / (float)DeathFrames;
                frames[Death + i] = Draw(new Pose
                {
                    Height = Math.Max(0f, 1f - k * 1.4f), Lean = k * 10f, Arm = 90f, Fore = 100f, Back = 110f, BackFore = 100f,
                    HeadTilt = k * 30f, Glow = Math.Max(0f, 0.8f - k), Puddle = 1f + k * 0.6f, Drain = k, Phase = i,
                });
            }

            return frames;
        }

        private static Pose WhipWindupPose(int i) => new Pose
        {
            Lean = -6f - i * 3f, X = -2f - i, Arm = 150f + i * 15f, Fore = 120f, HeadTilt = -4f, Blob = 3f + i * 2f, Glow = 0.9f, Phase = i,
        };

        private static Pose WhipPose(int i) => new Pose
        {
            Lean = 8f, X = 3f, Arm = 5f, Fore = 0f, Back = 150f, HeadTilt = 2f, Whip = 26f + i * 4f, Glow = 1f, Phase = i, Puddle = 1.1f,
        };

        private static Pose SilverPose(bool glint, bool chip, float cracks) => new Pose
        {
            Lean = 2f, Arm = 20f, Fore = -10f, Back = 130f, HeadTilt = 0f, Silver = true, Glint = glint, Chip = chip, Cracks = cracks, Glow = 0f,
        };

        // ------------------------------------------------------------------
        // O desenho
        // ------------------------------------------------------------------

        private static (float X, float Y) HipOf(Pose p) => (CenterX + p.X, Ground - 4f - 52f * p.Height + p.Y);

        private static (float X, float Y) ShoulderOf(Pose p)
        {
            (float hx, float hy) = HipOf(p);
            return (hx + p.Lean * 0.9f, hy - 36f);
        }

        private static (float X, float Y) HandOf(Pose p)
        {
            (float sx, float sy) = ShoulderOf(p);
            (float ex, float ey) = ArtKit.Along(sx + 7f, sy + 3f, p.Arm, 15f);
            return ArtKit.Along(ex, ey, p.Fore, 14f);
        }

        private static PixelCanvas Draw(Pose p)
        {
            var c = new PixelCanvas(Width, Height);

            if (p.Vapor >= 1f)
            {
                DrawCloud(c, p.Phase);
                return c;
            }

            DrawPuddle(c, p);

            if (p.Drain >= 0.75f)
            {
                DrawFallenMask(c, p);
                ArtKit.AddOutline(c);
                return c;
            }

            DrawTank(c, p);
            DrawBackArm(c, p);
            DrawColumn(c, p);
            DrawCoat(c, p);
            DrawHead(c, p);
            DrawFrontArm(c, p);

            if (p.Silver) Silverize(c, p);
            if (p.Cracks > 0f) DrawCracks(c, p);

            ArtKit.AddOutline(c);

            if (p.Vapor > 0f) DrawSteam(c, p);
            if (p.Glint) DrawGlint(c, p);
            return c;
        }

        /// <summary>A poca no chao, com os aneis de onda.</summary>
        private static void DrawPuddle(PixelCanvas c, Pose p)
        {
            float rx = 26f * p.Puddle * (p.Drain > 0f ? Math.Max(0.25f, 1f - p.Drain * 0.6f) : 1f);
            float cx = CenterX + p.X;
            ArtKit.Ball(c, cx, Ground - 2f, rx, 4f, ArtKit.Plain(Mercury), 0.3f);

            for (int ring = 0; ring < 2; ring++)
            {
                float r = rx * (0.45f + ((p.Phase + ring * 3) % 6) * 0.09f);
                for (int x = (int)(cx - r); x <= (int)(cx + r); x += 2) c.Set(x, (int)Ground - 3 + (x % 4 == 0 ? 0 : 1), Mercury[3]);
            }

            // As gotas soltas em volta.
            for (int i = 0; i < 4; i++)
            {
                float bx = cx + (i < 2 ? -1 : 1) * (rx + 3f + (p.Phase + i) % 3 * 2f);
                ArtKit.Ball(c, bx, Ground - 1.5f, 1.8f, 1.4f, ArtKit.Plain(Mercury), 0.3f);
            }
        }

        /// <summary>O tanque respirador nas costas, de latao, com o manometro.</summary>
        private static void DrawTank(PixelCanvas c, Pose p)
        {
            (float sx, float sy) = ShoulderOf(p);
            float tx = sx - 15f;
            float ty = sy + 10f;
            ArtKit.Limb(c, tx, ty - 9f, tx - 1f, ty + 13f, 6f, ArtKit.Corroded(Brass, 0.12f, 6101, 0.3f), 0.1f);
            for (int y = (int)ty - 6; y < ty + 12; y += 6) c.Fill((int)tx - 6, y, 12, 1, Brass[0]);
            ArtKit.Ball(c, tx - 1f, ty - 14f, 3f, 3f, ArtKit.Plain(Copper), 0.2f);
            ArtKit.Ball(c, tx + 3f, ty, 2.4f, 2.4f, ArtKit.Plain(SilverRamp), 0.3f);
            c.Set((int)tx + 3, (int)ty, Ink);
        }

        private static void DrawBackArm(PixelCanvas c, Pose p)
        {
            (float sx, float sy) = ShoulderOf(p);
            float bx = sx - 6f;
            float by = sy + 3f;
            (float ex, float ey) = ArtKit.Along(bx, by, p.Back, 14f);
            (float hx, float hy) = ArtKit.Along(ex, ey, p.BackFore, 13f);

            ArtKit.Limb(c, bx, by, ex, ey, 4f, Coat, -0.35f);
            ArtKit.Limb(c, ex, ey, hx, hy, 3.4f, Coat, -0.35f);
            ArtKit.Ball(c, hx, hy, 3.4f, 3.2f, ArtKit.Plain(Leather), -0.2f);

            if (p.Pour) DrawPourStream(c, hx, hy);
            else DrawDrip(c, hx, hy + 3f, 3 + p.Phase % 3);
        }

        /// <summary>A coluna de mercurio que e a metade de baixo dele, escorrendo na poca.</summary>
        private static void DrawColumn(PixelCanvas c, Pose p)
        {
            (float hx, float hy) = HipOf(p);
            float bottom = Ground - 4f;
            if (hy >= bottom - 2f) return;

            ArtKit.Taper(c, hx, hy, CenterX + p.X - p.Lean * 0.2f, bottom, 13f, 19f * p.Puddle, ArtKit.Plain(Mercury), 0.1f);

            // Os filetes escorrendo e os reflexos, que andam com a fase.
            for (int i = 0; i < 6; i++)
            {
                float x = hx - 10f + i * 4f + (p.Phase % 2);
                for (float y = hy + 4f + (i * 7 + p.Phase * 3) % 9; y < bottom - 2f; y += 1f)
                {
                    if (((int)y + i) % 5 == 0) continue;
                    if (c.Get((int)x, (int)y).A == 0) continue;
                    c.Set((int)x, (int)y, i % 2 == 0 ? Mercury[3] : Mercury[0]);
                }
            }

            // As bolhas, fervendo.
            if (p.Boil > 0f)
            {
                for (int i = 0; i < (int)(p.Boil * 8f); i++)
                {
                    float bx = hx - 9f + ArtKit.Hash(i, p.Phase, 6111) % 18;
                    float by = hy + 4f + ArtKit.Hash(i, p.Phase, 6112) % (int)Math.Max(4f, bottom - hy - 6f);
                    ArtKit.Ball(c, bx, by, 1.6f, 1.6f, ArtKit.Plain(new[] { Mercury[0], Mercury[2], Mercury[3], new Rgba(255, 255, 255) }), 0.5f);
                }
            }
        }

        /// <summary>O jaleco comprido, o manto de couro, o cinto de frascos e a bolsa.</summary>
        private static void DrawCoat(PixelCanvas c, Pose p)
        {
            (float hx, float hy) = HipOf(p);
            (float sx, float sy) = ShoulderOf(p);
            float hem = Math.Min(Ground - 6f, hy + 14f);
            float shrink = p.Drain > 0f ? 1f - p.Drain * 0.4f : 1f;

            // O corpo do jaleco: do ombro a barra, abrindo.
            ArtKit.Polygon(c, new[]
            {
                (sx - 11f * shrink, sy), (sx + 10f * shrink, sy), (hx + 14f * shrink, hem), (hx - 15f * shrink, hem),
            }, (x, y) => ArtKit.Tone(Coat, 0.45f - (x - (sx - 11f)) / 24f));

            // A barra manchada de prata, virando mercurio, e as gotas.
            for (int x = (int)(hx - 14f * shrink); x < hx + 13f * shrink; x++)
            {
                int drip = ArtKit.Hash(x, 0, 6121) % 6 + (p.Phase + x) % 3;
                for (int y = (int)hem - 3; y < hem + drip && y < Ground - 4f; y++)
                {
                    if (c.Get(x, y).A == 0 && y > hem) c.Set(x, y, Mercury[(x + y) % 3 + 1]);
                    else if (y >= hem - 3) c.Set(x, y, Mercury[2]);
                }
            }

            // A abertura da frente e os botoes de latao.
            ArtKit.Line(c, (int)(sx + 3f), (int)sy + 4, (int)(hx + 4f), (int)hem - 2, Coat[0]);
            for (int i = 0; i < 4; i++)
            {
                float t = (i + 1) / 5f;
                ArtKit.Rivet(c, (int)(sx + 5f + (hx - sx + 1f) * t), (int)(sy + 4f + (hem - sy - 6f) * t), Brass);
            }

            // O cinto e os frascos.
            float beltY = hy - 4f;
            c.Fill((int)(hx - 13f), (int)beltY, 26, 3, Leather[1]);
            c.Fill((int)(hx - 13f), (int)beltY, 26, 1, Leather[3]);
            ArtKit.Rivet(c, (int)hx - 1, (int)beltY, Brass);
            Rgba[] vials = { LabArt.OxideGreen, LabArt.Azure, LabArt.Amber, LabArt.Rose };
            for (int i = 0; i < 4; i++)
            {
                int vx = (int)(hx - 11f) + i * 6;
                c.Fill(vx, (int)beltY - 4, 3, 7, new Rgba(200, 228, 240));
                c.Fill(vx, (int)beltY - 1, 3, 4, vials[i]);
                c.Set(vx + 1, (int)beltY - 5, Brass[2]);
            }

            // A bolsa de couro, na anca.
            ArtKit.Drum(c, hx - 15f, beltY + 6f, 4f, 4.5f, 4f, ArtKit.Plain(Leather), 0f);
            c.Fill((int)hx - 19, (int)beltY + 2, 8, 2, Leather[3]);
            ArtKit.Rivet(c, (int)hx - 16, (int)beltY + 4, Brass);

            // A mancha verde de Oxide no peito.
            c.Set((int)sx + 5, (int)sy + 12, LabArt.OxideGreen);
            c.Set((int)sx + 6, (int)sy + 13, LabArt.OxideDark);
            c.Set((int)sx + 4, (int)sy + 14, LabArt.OxideGreen);

            // O manto de couro, com os rebites.
            float rise = p.Laugh > 0f ? -1.5f : 0f;
            ArtKit.Polygon(c, new[]
            {
                (sx - 13f, sy + 9f + rise), (sx - 8f, sy - 4f + rise), (sx + 8f, sy - 4f + rise), (sx + 13f, sy + 9f + rise), (sx, sy + 13f + rise),
            }, (x, y) => ArtKit.Tone(Leather, 0.4f - (x - sx + 12f) / 26f));
            for (int i = -2; i <= 2; i++) ArtKit.Rivet(c, (int)(sx + i * 5f), (int)(sy + 7f - Math.Abs(i) + rise), Brass);
        }

        /// <summary>A mascara de bico, com as lentes acesas, o capuz e a mangueira.</summary>
        private static void DrawHead(PixelCanvas c, Pose p)
        {
            (float sx, float sy) = ShoulderOf(p);
            float hx = sx + 1f + p.Lean * 0.25f;
            float hy = sy - 13f + (p.Drain > 0f ? p.Drain * 8f : 0f);
            float tilt = p.HeadTilt;

            // O capuz.
            ArtKit.Ball(c, hx - 2f, hy, 12f, 12.5f, ArtKit.Plain(Leather), -0.1f);
            ArtKit.Ball(c, hx - 1f, hy - 1f, 10f, 10.5f, ArtKit.Plain(new[] { Leather[0], Leather[1], Leather[2], Leather[3] }), 0.1f);

            // O bico, longo, com a costura e os rebites.
            (float tipX, float tipY) = ArtKit.Along(hx + 6f, hy + 3f, 18f + tilt, 24f);
            ArtKit.Taper(c, hx + 6f, hy + 3f, tipX, tipY, 6.5f, 1.2f, ArtKit.Plain(Leather), 0.2f);
            ArtKit.Line(c, (int)hx + 7, (int)hy + 4, (int)tipX, (int)tipY, Leather[0]);
            for (int i = 1; i <= 3; i++)
            {
                (float rx, float ry) = ArtKit.Along(hx + 6f, hy + 3f, 18f + tilt, i * 6f);
                ArtKit.Rivet(c, (int)rx, (int)ry - 2, Brass);
            }

            if (p.Laugh > 0f)
            {
                // O bico aberto embaixo: a fresta escura.
                (float mx, float my) = ArtKit.Along(hx + 8f, hy + 7f, 30f + tilt, 12f);
                ArtKit.Line(c, (int)hx + 8, (int)hy + 7, (int)mx, (int)my, Ink);
                ArtKit.Line(c, (int)hx + 8, (int)hy + 8, (int)mx, (int)my + 1, new Rgba(16, 10, 20));
            }

            // As lentes: aro de latao, o vidro verde aceso.
            for (int eye = 0; eye < 2; eye++)
            {
                float ex = hx - 1f + eye * 8f;
                float ey = hy - 2f;
                ArtKit.Ball(c, ex, ey, 4f, 4f, ArtKit.Plain(Brass), 0.2f);
                Rgba[] glass = p.Glow > 0.05f ? Lens : new[] { new Rgba(30, 34, 40), new Rgba(44, 50, 58), new Rgba(70, 76, 86), new Rgba(120, 126, 136) };
                ArtKit.Ball(c, ex, ey, 2.7f, 2.7f, ArtKit.Plain(glass), 0.2f + p.Glow * 0.3f);
                if (p.Glow > 0.5f) c.Set((int)ex - 1, (int)ey - 1, Lens[3]);
            }

            // A mangueira, do queixo ate o tanque.
            (float tx, float ty) = (sx - 16f, sy - 3f);
            for (int i = 0; i <= 12; i++)
            {
                float t = i / 12f;
                float px = hx - 4f + (tx - hx + 4f) * t;
                float py = hy + 9f + (ty - hy - 9f) * t + (float)Math.Sin(t * Math.PI) * 7f;
                ArtKit.Ball(c, px, py, 1.8f, 1.8f, ArtKit.Plain(ArtKit.Rubber), 0.2f);
                if (i % 3 == 0) c.Set((int)px, (int)py - 1, ArtKit.Rubber[3]);
            }
        }

        private static void DrawFrontArm(PixelCanvas c, Pose p)
        {
            (float sx, float sy) = ShoulderOf(p);
            (float ex, float ey) = ArtKit.Along(sx + 7f, sy + 3f, p.Arm, 15f);
            (float hx, float hy) = HandOf(p);

            ArtKit.Limb(c, sx + 7f, sy + 3f, ex, ey, 4.4f, Coat, 0.1f);
            c.Fill((int)ex - 2, (int)ey - 1, 4, 2, Coat[0]);
            ArtKit.Limb(c, ex, ey, hx, hy, 3.8f, Coat, 0.15f);

            // O punho da luva e a mao.
            (float cx, float cy) = ArtKit.Along(ex, ey, p.Fore, 10f);
            ArtKit.Limb(c, cx, cy, hx, hy, 4.2f, Leather, 0.2f);
            ArtKit.Ball(c, hx, hy, 3.6f, 3.4f, ArtKit.Plain(Leather), 0.25f);
            for (int f = -1; f <= 1; f++)
            {
                (float fx, float fy) = ArtKit.Along(hx, hy, p.Fore + f * 22f, 4.5f);
                ArtKit.Limb(c, hx, hy, fx, fy, 1f, Leather, 0.3f);
            }

            if (p.Blob > 0f)
            {
                ArtKit.Ball(c, hx + 1f, hy - p.Blob * 0.5f, p.Blob + 1.5f, p.Blob + 1.5f, ArtKit.Plain(Mercury), 0.4f);
            }

            if (p.Whip > 0f)
            {
                // O comeco do chicote: o resto da arena e o objeto do golpe.
                ArtKit.Taper(c, hx, hy, hx + p.Whip, hy + 1f, 3.4f, 2.6f, ArtKit.Plain(Mercury), 0.3f);
                for (int x = (int)hx + 2; x < hx + p.Whip; x += 3) c.Set(x, (int)hy - 1, Mercury[3]);
            }

            if (p.Pour) DrawPourStream(c, hx, hy);
            else if (p.Whip <= 0f) DrawDrip(c, hx, hy + 3f, 2 + (p.Phase + 1) % 3);
        }

        private static void DrawDrip(PixelCanvas c, float x, float y, int length)
        {
            if (y + length >= Ground - 4f) return;
            for (int i = 0; i < length; i++) c.Set((int)x, (int)y + i, Mercury[2]);
            ArtKit.Ball(c, x, y + length + 1f, 1.4f, 1.6f, ArtKit.Plain(Mercury), 0.3f);
        }

        private static void DrawPourStream(PixelCanvas c, float x, float y)
        {
            for (float yy = y + 2f; yy < Ground - 4f; yy += 1f)
            {
                float wave = (float)Math.Sin(yy * 0.4f) * 0.7f;
                c.Set((int)(x + wave), (int)yy, Mercury[2]);
                c.Set((int)(x + wave) + 1, (int)yy, Mercury[1]);
            }
        }

        /// <summary>Tudo em prata macica: cada pixel pela luz dele, numa rampa so.</summary>
        private static void Silverize(PixelCanvas c, Pose p)
        {
            for (int i = 0; i < c.Pixels.Length; i++)
            {
                Rgba px = c.Pixels[i];
                if (px.A < 200) continue;

                float lum = (px.R * 0.3f + px.G * 0.55f + px.B * 0.15f) / 255f;
                Rgba s = lum > 0.72f ? SilverRamp[3] : lum > 0.5f ? SilverRamp[2] : lum > 0.3f ? SilverRamp[1] : SilverRamp[0];
                c.Pixels[i] = s;
            }

            if (p.Chip)
            {
                (float sx, float sy) = ShoulderOf(p);
                for (int i = 0; i < 6; i++) c.Set((int)sx + 3 + i % 3, (int)sy + 6 + i / 3, SilverRamp[0]);
            }
        }

        private static void DrawCracks(PixelCanvas c, Pose p)
        {
            (float sx, float sy) = ShoulderOf(p);
            int cracks = 1 + (int)(p.Cracks * 4f);
            for (int i = 0; i < cracks; i++)
            {
                float a = 40f + i * 70f;
                ArtKit.Crack(c, sx - 4f + i * 3f, sy + 6f + i * 9f, a, 10 + (int)(p.Cracks * 14f), 6131 + i, 0f);
            }

            // O mercurio vazando pelas fendas.
            if (p.Cracks > 0.5f)
            {
                for (int i = 0; i < cracks * 2; i++)
                {
                    float bx = sx - 12f + ArtKit.Hash(i, 0, 6132) % 26;
                    float by = sy - 6f + ArtKit.Hash(i, 0, 6133) % 50;
                    ArtKit.Ball(c, bx, by, 2f, 2f, ArtKit.Plain(Mercury), 0.4f);
                }
            }
        }

        private static void DrawGlint(PixelCanvas c, Pose p)
        {
            (float sx, float sy) = ShoulderOf(p);
            ArtKit.Spark(c, (int)sx - 6, (int)sy - 16, 4);
            ArtKit.Spark(c, (int)sx + 10, (int)sy + 22, 2);
        }

        /// <summary>O vapor saindo dele quando destila: nuvenzinhas prateadas subindo.</summary>
        private static void DrawSteam(PixelCanvas c, Pose p)
        {
            (float sx, float sy) = ShoulderOf(p);
            int puffs = 4 + (int)(p.Vapor * 10f);
            for (int i = 0; i < puffs; i++)
            {
                float px = sx - 18f + ArtKit.Hash(i, p.Phase, 6141) % 40;
                float py = sy - 20f + ArtKit.Hash(i, p.Phase, 6142) % 60 - p.Vapor * 20f;
                ArtKit.Puff(c, px, py, 3f + i % 3 * 2f, Mercury[3], Mercury[1], 120 + (int)(p.Vapor * 80f));
            }
        }

        /// <summary>Ele todo em vapor: uma nuvem prateada, gorda, com as duas lentes acesas no meio.</summary>
        private static void DrawCloud(PixelCanvas c, int phase)
        {
            for (int i = 0; i < 14; i++)
            {
                float a = i * 0.45f + phase * 0.3f;
                float px = CenterX + (float)Math.Cos(a) * (14f + i % 4 * 6f);
                float py = Ground - 22f + (float)Math.Sin(a * 1.3f) * 9f;
                ArtKit.Puff(c, px, py, 9f + i % 3 * 3f, Mercury[3], Mercury[1], 200);
            }

            ArtKit.Ball(c, CenterX - 4f, Ground - 24f, 3f, 3f, ArtKit.Plain(Lens), 0.5f);
            ArtKit.Ball(c, CenterX + 5f, Ground - 24f, 3f, 3f, ArtKit.Plain(Lens), 0.5f);

            // As gotas que ja estao caindo.
            for (int i = 0; i < 4; i++)
            {
                float bx = CenterX - 18f + i * 12f + phase * 2f;
                ArtKit.Ball(c, bx, Ground - 6f + (i + phase) % 3 * 2f, 1.6f, 2f, ArtKit.Plain(Mercury), 0.3f);
            }
        }

        /// <summary>O fim: so a poca, o jaleco murcho em cima dela e a mascara caida de lado.</summary>
        private static void DrawFallenMask(PixelCanvas c, Pose p)
        {
            float cx = CenterX + p.X;
            ArtKit.Polygon(c, new[] { (cx - 20f, Ground - 3f), (cx - 8f, Ground - 9f), (cx + 12f, Ground - 8f), (cx + 22f, Ground - 3f) },
                (x, y) => ArtKit.Tone(Coat, 0.3f - (x - cx) / 50f));

            float mx = cx + 6f;
            float my = Ground - 12f + (p.Drain - 0.75f) * 8f;
            ArtKit.Ball(c, mx, my, 7f, 6f, ArtKit.Plain(Leather), 0.1f);
            ArtKit.Taper(c, mx + 4f, my + 2f, mx + 20f, my + 5f, 4.5f, 1f, ArtKit.Plain(Leather), 0.2f);
            ArtKit.Ball(c, mx - 1f, my - 1f, 2.6f, 2.6f, ArtKit.Plain(Brass), 0.2f);
            c.Set((int)mx - 1, (int)my - 1, new Rgba(30, 34, 40));
        }
    }
}
