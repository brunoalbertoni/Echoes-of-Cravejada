using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Guarda de Prata Perfeita, no traco dos chefes: uma armadura de prata
    /// sem um risco, sem uma mancha de Oxide — a unica coisa limpa dos
    /// Ateliers, e e ela que vem descartar o prototipo. Elmo fechado com a
    /// fresta acesa em branco, gorjal, ombreiras em leque, o tabardo de
    /// veludo da Coroa com o 925 bordado, manoplas, grevas, e a alabarda.
    ///
    /// Cada quadro tem 128x160, olha pra direita e e importado com 32 pixels
    /// por tile (4 x 5 tiles). O pivo e o meio dos pes.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class GuardArt
    {
        public const int Width = 128;
        public const int Height = 160;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 0.5f;
        public const float PivotY = 3f / Height;

        public const int Dormant = 0;
        public const int DormantFrames = 1;
        public const int Wake = 1;
        public const int WakeFrames = 3;
        public const int March = 4;
        public const int MarchFrames = 6;
        public const int Reach = 10;
        public const int ReachFrames = 2;
        public const int Dissolve = 12;
        public const int DissolveFrames = 6;

        // As poses de luta dos guardas do palacio (os que morrem).
        public const int SweepWindup = 18;
        public const int SweepWindupFrames = 2;
        public const int Sweep = 20;
        public const int SweepFrames = 3;
        public const int ThrustWindup = 23;
        public const int Thrust = 24;
        public const int ThrustFrames = 2;
        public const int Block = 26;
        public const int Hurt = 27;
        public const int FrameCount = 28;

        private const float Ground = 157f;
        private const float CenterX = 60f;

        private static readonly Rgba[] Plate =
            { new Rgba(92, 100, 124), new Rgba(160, 170, 194), new Rgba(222, 230, 244), new Rgba(255, 255, 255) };

        private static readonly Rgba[] Velvet = LabArt.Velvet;
        private static readonly Rgba[] Gold = ArtKit.Brass;
        private static readonly Rgba Slit = new Rgba(220, 240, 255);

        private sealed class Pose
        {
            public float X;
            public float Y;
            public float Lean;
            public float FrontFoot = 6f;
            public float BackFoot = -6f;
            public float FrontLift;
            public float BackLift;
            public float Arm = 80f;
            public float Fore = 70f;
            public float Back = 100f;
            public float Halberd = -84f;
            public float Visor = 1f;

            /// <summary>Desfazendo: 0 inteiro, 1 um monte de placas no chao.</summary>
            public float Fall;

            /// <summary>O rastro da alabarda no golpe, e o brilho do espelho na guarda.</summary>
            public bool Smear;
            public bool Glint;

            /// <summary>Quanto da alabarda fica a frente da mao (o resto e cabo, pra tras).</summary>
            public float Reach = 70f;
            public int Phase;
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            frames[Dormant] = Draw(new Pose { Visor = 0f });

            for (int i = 0; i < WakeFrames; i++)
            {
                frames[Wake + i] = Draw(new Pose { Visor = (i + 1) / (float)WakeFrames, Lean = i == 2 ? 3f : 0f, Halberd = -84f + i * 6f, Phase = i });
            }

            for (int i = 0; i < MarchFrames; i++)
            {
                double a = i * Math.PI * 2.0 / MarchFrames;
                float swing = (float)Math.Sin(a);
                frames[March + i] = Draw(new Pose
                {
                    Lean = 6f, Y = Math.Abs((float)Math.Cos(a)) * -2f + 2f,
                    FrontFoot = swing * 12f, BackFoot = -swing * 12f,
                    FrontLift = swing < -0.2f ? -swing * 7f : 0f, BackLift = swing > 0.2f ? swing * 7f : 0f,
                    Arm = 60f - swing * 10f, Fore = 30f, Back = 110f + swing * 12f, Halberd = -70f + swing * 4f, Phase = i,
                });
            }

            for (int i = 0; i < ReachFrames; i++)
            {
                frames[Reach + i] = Draw(new Pose
                {
                    Lean = 10f + i * 3f, X = i * 2f, Arm = -5f + i * 5f, Fore = -5f, Back = 140f, Halberd = -60f, FrontFoot = 14f, BackFoot = -10f, Phase = i,
                });
            }

            for (int i = 0; i < DissolveFrames; i++)
            {
                frames[Dissolve + i] = Draw(new Pose { Fall = (i + 1) / (float)DissolveFrames, Visor = Math.Max(0f, 1f - i * 0.4f), Phase = i });
            }

            // A varredura: a alabarda vai pra tras, por cima do ombro, e desce em arco na frente.
            frames[SweepWindup] = Draw(new Pose { Lean = -4f, Arm = -100f, Fore = -130f, Halberd = -150f, FrontFoot = 8f, BackFoot = -8f, Visor = 1f });
            frames[SweepWindup + 1] = Draw(new Pose { Lean = -7f, X = -2f, Arm = -120f, Fore = -150f, Halberd = -175f, FrontFoot = 9f, BackFoot = -9f, Visor = 1f, Phase = 1 });
            float[] sweep = { -60f, -10f, 25f };
            for (int i = 0; i < SweepFrames; i++)
            {
                frames[Sweep + i] = Draw(new Pose
                {
                    Lean = 8f + i * 2f, X = 3f, Arm = -20f + i * 25f, Fore = -10f + i * 25f, Halberd = sweep[i], FrontFoot = 14f, BackFoot = -10f, Visor = 1f, Phase = i, Smear = i < 2,
                });
            }

            // A estocada: recolhe, e dispara a ponta pra frente.
            frames[ThrustWindup] = Draw(new Pose { Lean = -3f, X = -3f, Arm = 130f, Fore = 170f, Halberd = 0f, FrontFoot = 6f, BackFoot = -10f, Visor = 1f });
            for (int i = 0; i < ThrustFrames; i++)
            {
                frames[Thrust + i] = Draw(new Pose { Lean = 10f, X = 2f + i * 2f, Arm = 20f, Fore = 5f, Halberd = 0f, Reach = 14f, FrontFoot = 16f, BackFoot = -12f, Visor = 1f, Phase = i, Smear = i == 0 });
            }

            // A guarda: a alabarda atravessada na frente, o espelho do peito brilhando.
            frames[Block] = Draw(new Pose { Lean = -2f, Arm = 40f, Fore = -60f, Halberd = -100f, FrontFoot = 8f, BackFoot = -8f, Visor = 1f, Glint = true });
            frames[Hurt] = Draw(new Pose { Lean = -8f, X = -3f, Visor = 0.6f, Phase = 1 });

            return frames;
        }

        private static PixelCanvas Draw(Pose p)
        {
            var c = new PixelCanvas(Width, Height);

            if (p.Fall >= 0.5f)
            {
                DrawHeap(c, p);
                ArtKit.AddOutline(c);
                return c;
            }

            float sink = p.Fall * 30f;
            float hipX = CenterX + p.X;
            float hipY = Ground - 48f + p.Y + sink;
            float shX = hipX + p.Lean;
            float shY = hipY - 42f + sink * 0.3f;

            // A perna de tras e o braco de tras.
            Leg(c, hipX - 4f, hipY, hipX + p.BackFoot, p.BackLift, -0.35f);
            Arm(c, shX - 12f, shY + 6f, p.Back, 80f, -0.35f, false);

            // O tabardo, atras das pernas da frente.
            ArtKit.Polygon(c, new[] { (shX - 12f, shY + 10f), (shX + 12f, shY + 10f), (hipX + 12f, hipY + 26f), (hipX - 12f, hipY + 26f) },
                (x, y) => ArtKit.Tone(Velvet, 0.35f - (x - hipX + 12f) / 30f));
            c.Fill((int)hipX - 12, (int)hipY + 23, 25, 3, Gold[2]);
            Emblem(c, (shX + hipX) * 0.5f, shY + 26f);

            // O tronco: o peitoral com a quilha e o faixa de cintura.
            ArtKit.Drum(c, shX, shY + 14f, 15f, 17f, 3f, ArtKit.Plain(Plate), 0.15f);
            ArtKit.Line(c, (int)shX + 2, (int)shY + 1, (int)shX + 1, (int)shY + 28, Plate[3]);
            c.Fill((int)hipX - 14, (int)hipY - 4, 29, 4, Plate[1]);
            c.Fill((int)hipX - 14, (int)hipY - 4, 29, 1, Plate[3]);
            for (int x = (int)hipX - 12; x < hipX + 14; x += 5) ArtKit.Rivet(c, x, (int)hipY - 3, Gold);

            Leg(c, hipX + 4f, hipY, hipX + p.FrontFoot, p.FrontLift, 0.1f);

            // O gorjal e o elmo.
            ArtKit.Drum(c, shX + 1f, shY - 2f, 9f, 4f, 3f, ArtKit.Plain(Plate), 0.2f);
            float hx = shX + 2f + p.Lean * 0.2f;
            float hy = shY - 14f;
            ArtKit.Drum(c, hx, hy, 10f, 12f, 2.6f, ArtKit.Plain(Plate), 0.2f);
            c.Fill((int)hx - 10, (int)hy - 3, 21, 1, Plate[0]);
            for (int i = 0; i < 3; i++) c.Fill((int)hx + 2 + i * 3, (int)hy + 3, 1, 6, Plate[0]);

            // A fresta da viseira, acesa.
            Rgba slit = p.Visor > 0.05f ? Rgba.Lerp(Plate[0], Slit, p.Visor) : new Rgba(20, 22, 30);
            c.Fill((int)hx - 2, (int)hy - 1, 13, 2, slit);
            if (p.Visor > 0.6f) ArtKit.Glow(c, hx + 6f, hy, 9f, Slit, 0.4f * p.Visor);

            // A crista de crina vermelha.
            for (int i = 0; i < 9; i++)
            {
                float t = i / 8f;
                ArtKit.Ball(c, hx - 8f + t * 10f, hy - 12f - (float)Math.Sin(t * Math.PI) * 5f + (p.Phase + i) % 2, 2.6f, 2.6f, ArtKit.Plain(Velvet), 0.2f);
            }

            // As ombreiras, em leque.
            for (int k = 0; k < 3; k++) ArtKit.Ball(c, shX + 9f - k * 1.5f, shY + 3f + k * 4f, 8f - k, 3.4f, ArtKit.Plain(Plate), 0.3f);

            Arm(c, shX + 9f, shY + 6f, p.Arm, p.Fore, 0.15f, true, p.Halberd, p.Reach);

            // O desfazer: placas soltando.
            if (p.Fall > 0f)
            {
                for (int i = 0; i < (int)(p.Fall * 20f); i++)
                {
                    int x = (int)hipX - 16 + ArtKit.Hash(i, 0, 7101) % 34;
                    int y = (int)shY + ArtKit.Hash(i, 0, 7102) % 70;
                    c.Set(x, y, Rgba.Clear);
                    c.Set(x + 1, y, Rgba.Clear);
                }
            }

            ArtKit.AddOutline(c);

            // O rastro do golpe.
            if (p.Smear)
            {
                for (int i = 0; i < 18; i++)
                {
                    (float px, float py) = ArtKit.Along(shX + 10f, shY + 10f, p.Halberd - 40f + i * 3f, 60f + i % 3 * 6f);
                    c.Blend((int)px, (int)py, new Rgba(240, 248, 255, 150));
                    c.Blend((int)px + 1, (int)py, new Rgba(240, 248, 255, 90));
                }
            }

            if (p.Glint) ArtKit.Glow(c, shX, shY + 14f, 18f, new Rgba(240, 250, 255), 0.7f, true);

            // O brilho da prata perfeita.
            ArtKit.Spark(c, (int)shX - 6, (int)shY + 8, 2);
            if (p.Phase % 3 == 0) ArtKit.Spark(c, (int)hx - 5, (int)hy - 6, 1);
            return c;
        }

        private static void Leg(PixelCanvas c, float hx, float hy, float footX, float lift, float bias)
        {
            float footY = Ground - 4f - lift;
            (float kx, float ky) = ArtKit.Joint(hx, hy, footX, footY, 22f, 22f, -1);
            ArtKit.Limb(c, hx, hy, kx, ky, 6f, Plate, bias);
            ArtKit.Ball(c, kx, ky, 5f, 4.4f, ArtKit.Plain(Plate), bias + 0.3f);
            ArtKit.Taper(c, kx, ky, footX, footY, 5.4f, 4.6f, ArtKit.Plain(Plate), bias);
            ArtKit.Drum(c, footX + 3f, footY + 1.5f, 8f, 3f, 3f, ArtKit.Plain(Plate), bias);
        }

        private static void Arm(PixelCanvas c, float sx, float sy, float arm, float fore, float bias, bool front, float halberd = 0f, float reach = 70f)
        {
            (float ex, float ey) = ArtKit.Along(sx, sy, arm, 18f);
            (float hx, float hy) = ArtKit.Along(ex, ey, fore, 16f);

            if (front)
            {
                // A alabarda, presa na manopla.
                (float topX, float topY) = ArtKit.Along(hx, hy, halberd, reach);
                (float butX, float butY) = ArtKit.Along(hx, hy, halberd + 180f, 104f - reach);
                ArtKit.Limb(c, butX, butY, topX, topY, 1.8f, LabArt.Walnut, 0.1f);
                ArtKit.Polygon(c, new[]
                {
                    (topX, topY), ArtKit.Along(topX, topY, halberd + 160f, 18f), ArtKit.Along(topX, topY, halberd + 120f, 14f),
                }, (x, y) => Plate[3]);
                ArtKit.Polygon(c, new[]
                {
                    ArtKit.Along(topX, topY, halberd + 180f, 8f), ArtKit.Along(topX, topY, halberd + 230f, 14f), ArtKit.Along(topX, topY, halberd + 180f, 20f),
                }, (x, y) => Plate[2]);
            }

            ArtKit.Limb(c, sx, sy, ex, ey, 5f, Plate, bias);
            ArtKit.Ball(c, ex, ey, 4.6f, 4.2f, ArtKit.Plain(Plate), bias + 0.3f);
            ArtKit.Limb(c, ex, ey, hx, hy, 4.6f, Plate, bias);
            ArtKit.Ball(c, hx, hy, 5f, 4.6f, ArtKit.Plain(Plate), bias + 0.2f);
            for (int f = 0; f < 3; f++) c.Fill((int)hx - 3 + f * 2, (int)hy + 2, 1, 3, Plate[0]);
        }

        /// <summary>O 925 da Coroa, bordado a ouro no tabardo.</summary>
        private static void Emblem(PixelCanvas c, float cx, float cy)
        {
            ArtKit.Ball(c, cx, cy, 6f, 6f, ArtKit.Plain(Gold), 0.2f);
            ArtKit.Ball(c, cx, cy, 4f, 4f, ArtKit.Plain(Velvet), -0.1f);
            PixelFont.DrawCentered(c, "925", (int)cx, (int)cy - 2, Gold[3]);
            for (int i = 0; i < 3; i++) c.Fill((int)cx - 4 + i * 4, (int)cy - 9, 2, 3, Gold[2]);
        }

        /// <summary>O fim: as placas no chao, o elmo virado, a crista e a alabarda atravessada.</summary>
        private static void DrawHeap(PixelCanvas c, Pose p)
        {
            float spread = (p.Fall - 0.5f) * 2f;
            ArtKit.Limb(c, 20f, Ground - 3f, 110f, Ground - 6f, 1.8f, LabArt.Walnut, 0.1f);

            for (int i = 0; i < 12; i++)
            {
                float x = CenterX - 30f + ArtKit.Hash(i, 0, 7111) % 60 + (i % 2 == 0 ? -1 : 1) * spread * 6f;
                float y = Ground - 4f - ArtKit.Hash(i, 0, 7112) % 8;
                ArtKit.Ball(c, x, y, 5f + i % 3, 3f, ArtKit.Plain(Plate), 0.2f);
            }

            ArtKit.Polygon(c, new[] { (CenterX - 10f, Ground - 3f), (CenterX + 10f, Ground - 5f), (CenterX + 14f, Ground - 1f), (CenterX - 12f, Ground) },
                (x, y) => ArtKit.Tone(Velvet, 0.2f));

            ArtKit.Drum(c, CenterX + 18f, Ground - 10f, 9f, 8f, 2.6f, ArtKit.Plain(Plate), 0.2f);
            c.Fill((int)CenterX + 12, (int)Ground - 11, 12, 2, new Rgba(20, 22, 30));
        }
    }
}
