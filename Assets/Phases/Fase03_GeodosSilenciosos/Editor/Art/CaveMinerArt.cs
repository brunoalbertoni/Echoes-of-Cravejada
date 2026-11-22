using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Capataz, o mini chefe do District 3, no mesmo traco do Coletor e do
    /// Mestre das Caldeiras: o primeiro mineiro a descer, o que "ouviu a pedra
    /// cantar". E um homem — grandalhao, curvado, de barba grisalha — que o
    /// Oxide foi tomando por dentro.
    ///
    /// Peca por peca: o casacao de capataz com remendo costurado, botoes de
    /// latao e a barra esfarrapada; o lenco vermelho no pescoco; o apito de
    /// latao na corrente (e com ele que ele chama o turno); o cinto com a
    /// fivela, a argola de chaves (a do salao esta ali) e a corda enrolada; a
    /// picareta de cabo de madeira. O capacete de latao nao cabe mais: rachou
    /// e os cristais furaram por cima, e no lugar da lanterna esta a safira.
    /// Os olhos viraram placa de quartzo leitoso — ele e cego. O braco de tras
    /// e tomado de ametista; nas costelas, a rachadura verde do Oxide.
    ///
    /// Morrendo, larga a picareta, ajoelha, o capacete cai, os cristais se
    /// desfazem e o rosto volta a ser de gente.
    ///
    /// Cada quadro tem 160x128, olha pra direita e e importado com 32 pixels
    /// por tile (5 x 4 tiles). O pivo fica no chao, embaixo dele (PivotX).
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static partial class CaveMinerArt
    {
        public const int Width = 160;
        public const int Height = 128;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 60f / Width;
        public const float PivotY = 1f / Height;

        public const int Dormant = 0;
        public const int DormantFrames = 4;
        public const int Wake = 4;
        public const int WakeFrames = 4;
        public const int Idle = 8;
        public const int IdleFrames = 6;
        public const int Walk = 14;
        public const int WalkFrames = 8;
        public const int SlamWindup = 22;
        public const int SlamWindupFrames = 2;
        public const int Slam = 24;
        public const int SlamFrames = 3;
        public const int Stuck = 27;
        public const int StuckFrames = 4;
        public const int ChargeWindup = 31;
        public const int ChargeWindupFrames = 2;
        public const int Charge = 33;
        public const int ChargeFrames = 4;
        public const int Dazed = 37;
        public const int DazedFrames = 4;
        public const int Whistle = 41;
        public const int WhistleFrames = 4;
        public const int Roar = 45;
        public const int RoarFrames = 4;
        public const int Hurt = 49;
        public const int Death = 50;
        public const int DeathFrames = 8;
        public const int FrameCount = 58;

        /// <summary>
        /// Onde a ponta da picareta crava no chao, em tiles a frente do pivo:
        /// o script poe a area da pancada ai.
        /// </summary>
        public const float SlamReach = 2.1f;

        private const float Ground = 125f;
        private const float BodyX = 60f;
        private const float HipY = 80f;

        private static readonly Rgba[] Coat =
            { new Rgba(40, 32, 38), new Rgba(70, 56, 56), new Rgba(102, 82, 74), new Rgba(136, 112, 92) };

        private static readonly Rgba[] Patch =
            { new Rgba(70, 58, 44), new Rgba(104, 88, 62), new Rgba(140, 120, 86), new Rgba(176, 156, 114) };

        private static readonly Rgba[] Trousers =
            { new Rgba(30, 32, 46), new Rgba(50, 54, 74), new Rgba(76, 82, 106), new Rgba(106, 114, 138) };

        private static readonly Rgba[] Skin =
            { new Rgba(84, 78, 102), new Rgba(124, 116, 142), new Rgba(160, 152, 178), new Rgba(196, 190, 212) };

        /// <summary>A pele de quando o Oxide sai: de gente de novo.</summary>
        private static readonly Rgba[] Human =
            { new Rgba(110, 72, 60), new Rgba(160, 110, 88), new Rgba(204, 152, 120), new Rgba(234, 196, 160) };

        private static readonly Rgba[] Beard =
            { new Rgba(70, 66, 76), new Rgba(112, 106, 114), new Rgba(156, 150, 156), new Rgba(204, 198, 200) };

        private static readonly Rgba[] Red =
            { new Rgba(84, 24, 30), new Rgba(138, 38, 40), new Rgba(186, 64, 54), new Rgba(222, 110, 84) };

        private static readonly Rgba[] Amethyst =
            { new Rgba(70, 40, 120), new Rgba(128, 82, 190), new Rgba(186, 142, 236), new Rgba(236, 216, 255) };

        private static readonly Rgba[] Milky =
            { new Rgba(130, 138, 168), new Rgba(190, 198, 224), new Rgba(228, 234, 248), new Rgba(255, 255, 255) };

        private static readonly Rgba[] Sapphire =
            { new Rgba(18, 34, 100), new Rgba(36, 84, 196), new Rgba(86, 148, 240), new Rgba(214, 236, 255) };

        private static readonly Rgba[] Wood =
            { new Rgba(52, 34, 26), new Rgba(88, 58, 36), new Rgba(124, 86, 54), new Rgba(160, 118, 76) };

        private static readonly Rgba[] Rope =
            { new Rgba(92, 76, 50), new Rgba(130, 108, 70), new Rgba(164, 140, 92), new Rgba(196, 172, 120) };

        private static readonly Rgba Mouth = new Rgba(30, 14, 20);
        private static readonly Rgba Teeth = new Rgba(214, 206, 186);
        private static readonly Rgba Thread = new Rgba(196, 180, 140);

        private sealed class Pose
        {
            public float X;
            public float Y;

            /// <summary>O quanto o tronco tomba pra frente, em graus (negativo: pra tras).</summary>
            public float Lean = 8f;

            /// <summary>A cabeca: angulo a mais em relacao ao tronco (negativo: pra tras, pra cima).</summary>
            public float Head;
            public float Jaw;

            public float BackFoot;
            public float FrontFoot;
            public float BackLift;
            public float FrontLift;

            /// <summary>0 em pe, 1 com o joelho de tras no chao.</summary>
            public float Kneel;

            /// <summary>O braco da picareta: braco, antebraco e o cabo (0 = pra frente, 90 = pra baixo).</summary>
            public float Arm = 80f;
            public float Fore = 62f;
            public float Pick = 70f;
            public bool PickBehind;

            /// <summary>O braco de cristal, por angulos — ou com a mao num ponto (no cabo, na boca).</summary>
            public float BackArm = 100f;
            public float BackFore = 85f;
            public bool TwoHands;
            public bool Whistle;
            public float WhistleLift;

            /// <summary>As ondas de som saindo da boca: 0 nenhuma; o apito e o grito.</summary>
            public float Rings;
            public bool Wide;

            public float Gem = 1f;
            public bool NoGem;
            public bool HelmetOff;
            public bool PickDropped;
            public bool Freed;
            public bool Clutch;
            public bool Lying;
            public bool Buried;

            public float SmearFrom;
            public float SmearTo;
            public float Dust;
            public float DustX = -1f;
            public int Stars = -1;
            public float CrystalDust;
            public float Shake;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            // Esperando: ajoelhado, a cabeca encostada no cabo, batendo a picareta
            // na pedra devagar pra ouvir o eco.
            for (int i = 0; i < DormantFrames; i++)
            {
                bool tap = i == 1;
                frames[Dormant + i] = Draw(new Pose
                {
                    Kneel = 1f, Lean = 22f, Head = 18f, FrontFoot = 6f, Gem = 0.35f + (i == 2 ? 0.2f : 0f),
                    PickBehind = false, Arm = 55f, Fore = tap ? 70f : 55f, Pick = tap ? 82f : 70f,
                    BackArm = 70f, BackFore = 60f, Dust = tap ? 0.6f : 0f, DustX = 100f,
                });
            }

            // Acordando: ergue a cabeca, fareja, se levanta e poe a picareta no ombro.
            float[] rise = { 0.9f, 0.6f, 0.25f, 0f };
            for (int i = 0; i < WakeFrames; i++)
            {
                float k = rise[i];
                frames[Wake + i] = Draw(new Pose
                {
                    Kneel = k, Lean = Lerp(8f, 22f, k), Head = i == 1 ? -18f : Lerp(0f, 10f, k), FrontFoot = 6f * k,
                    Jaw = i == 1 ? 0.5f : 0f, Gem = Lerp(1.2f, 0.5f, k),
                    Arm = Lerp(80f, 55f, k), Fore = Lerp(62f, 55f, k), Pick = Lerp(70f, 70f, k),
                    BackArm = Lerp(100f, 70f, k), BackFore = Lerp(85f, 60f, k),
                });
            }

            // Parado: respira fundo e vira a cabeca de lado, escutando.
            float[] breath = { 0f, 0.5f, 1f, 1f, 0.5f, 0f };
            float[] listen = { 0f, -4f, -8f, -8f, -4f, 0f };
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(new Pose
                {
                    Y = breath[i], Head = listen[i], Gem = 0.8f + breath[i] * 0.3f,
                    Arm = 80f + breath[i], Fore = 62f + breath[i] * 2f, Pick = 70f + breath[i] * 2f, BackFore = 85f - breath[i] * 3f,
                });
            }

            // Andando: pesado, a picareta balancando no ombro.
            for (int i = 0; i < WalkFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / WalkFrames;
                float swing = (float)Math.Sin(phase);
                float bob = (float)Math.Abs(Math.Cos(phase));
                frames[Walk + i] = Draw(new Pose
                {
                    Y = (1f - bob) * 2.5f, Lean = 10f, Head = 2f,
                    BackFoot = -swing * 9f, FrontFoot = swing * 9f,
                    BackLift = swing > 0.2f ? swing * 6f : 0f, FrontLift = swing < -0.2f ? -swing * 6f : 0f,
                    Arm = 80f + swing * 6f, Fore = 62f + swing * 6f, Pick = 66f + swing * 8f, BackArm = 100f - swing * 14f, BackFore = 85f - swing * 10f,
                });
            }

            // A picaretada: ergue com as duas maos por cima da cabeca...
            frames[SlamWindup + 0] = Draw(new Pose
            {
                X = -2f, Lean = -4f, Head = -6f, Jaw = 0.3f, PickBehind = false, TwoHands = true,
                Arm = -100f, Fore = -125f, Pick = -150f, BackFoot = -4f, FrontFoot = 5f, Gem = 1.4f,
            });
            frames[SlamWindup + 1] = Draw(new Pose
            {
                X = -4f, Y = 1f, Lean = -10f, Head = -10f, Jaw = 0.5f, PickBehind = false, TwoHands = true,
                Arm = -110f, Fore = -150f, Pick = -178f, BackFoot = -7f, FrontFoot = 6f, Gem = 1.7f,
            });

            // ...e desce com tudo: rastro, a ponta cravada, poeira e faisca.
            frames[Slam + 0] = Draw(new Pose
            {
                X = 2f, Lean = 18f, Head = 6f, Jaw = 0.6f, PickBehind = false, TwoHands = true,
                Arm = -30f, Fore = -5f, Pick = 10f, SmearFrom = -170f, SmearTo = 10f, BackFoot = -8f, FrontFoot = 9f, Gem = 1.7f,
            });
            Pose impact = new Pose
            {
                X = 4f, Y = 4f, Lean = 30f, Head = 10f, Jaw = 0.4f, PickBehind = false, TwoHands = true, Buried = true,
                Arm = 40f, Fore = 35f, Pick = 72f, SmearFrom = -60f, SmearTo = 72f, BackFoot = -10f, FrontFoot = 10f, Dust = 1f, Gem = 1.5f,
            };
            impact.Sparks.Add((128, 121, 3));
            impact.Sparks.Add((118, 117, 2));
            frames[Slam + 1] = Draw(impact);
            frames[Slam + 2] = Draw(new Pose
            {
                X = 4f, Y = 4f, Lean = 30f, Head = 12f, Jaw = 0.2f, PickBehind = false, TwoHands = true, Buried = true,
                Arm = 40f, Fore = 35f, Pick = 72f, BackFoot = -10f, FrontFoot = 10f, Dust = 2f, Gem = 1.2f,
            });

            // Preso: a ponta enterrada, ele puxa o cabo e treme — a hora de bater.
            for (int i = 0; i < StuckFrames; i++)
            {
                float pull = i % 2 == 0 ? 0f : 1f;
                frames[Stuck + i] = Draw(new Pose
                {
                    X = 2f - pull * 2f, Y = 4f, Lean = 22f - pull * 10f, Head = 14f - pull * 6f, Jaw = 0.15f + pull * 0.3f,
                    PickBehind = false, TwoHands = true, Buried = true,
                    Arm = 40f - pull * 6f, Fore = 35f - pull * 8f, Pick = 72f, BackFoot = -12f, FrontFoot = 10f,
                    Shake = i % 2 == 0 ? 1f : -1f, Dust = i < 2 ? 1f : 0.4f, Gem = 0.7f,
                });
            }

            // A investida: abaixa a cabeca como um carneiro, raspa o pe e dispara,
            // a picareta arrastando atras e soltando faisca.
            Pose crouch = new Pose
            {
                Y = 5f, Lean = 30f, Head = 14f, Jaw = 0.3f, PickBehind = false, Arm = 140f, Fore = 160f, Pick = 168f,
                BackFoot = -12f, FrontFoot = 6f, Gem = 1.4f, Dust = 0.8f, DustX = 36f,
            };
            frames[ChargeWindup + 0] = Draw(crouch);
            Pose coil = new Pose
            {
                X = -3f, Y = 7f, Lean = 36f, Head = 16f, Jaw = 0.5f, PickBehind = false, Arm = 150f, Fore = 165f, Pick = 170f,
                BackFoot = -15f, BackLift = 2f, FrontFoot = 7f, Gem = 1.8f, Dust = 1.4f, DustX = 30f,
            };
            frames[ChargeWindup + 1] = Draw(coil);

            for (int i = 0; i < ChargeFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / ChargeFrames;
                float swing = (float)Math.Sin(phase);
                Pose run = new Pose
                {
                    X = 3f, Y = 3f, Lean = 40f, Head = 14f, Jaw = 0.4f, PickBehind = false, Arm = 145f, Fore = 168f, Pick = 172f,
                    BackFoot = -swing * 13f, FrontFoot = swing * 13f, BackLift = swing > 0f ? swing * 7f : 0f, FrontLift = swing < 0f ? -swing * 7f : 0f,
                    BackArm = 60f + swing * 30f, BackFore = 20f + swing * 30f, Gem = 1.8f, Dust = 0.6f, DustX = 22f,
                };
                run.Sparks.Add((14 + (i % 2) * 3, 122 - (i % 2), 2));
                frames[Charge + i] = Draw(run);
            }

            // Bateu na parede: o capacete amassa, ele recua tonto, estrelinhas.
            Pose crash = new Pose
            {
                X = -4f, Lean = -16f, Head = -16f, Jaw = 0.7f, PickBehind = false, Arm = 110f, Fore = 120f, Pick = 140f,
                BackFoot = -8f, FrontLift = 4f, FrontFoot = 4f, Gem = 0.4f, Stars = 0,
            };
            crash.Sparks.Add((96, 30, 3));
            crash.Sparks.Add((102, 42, 2));
            frames[Dazed + 0] = Draw(crash);
            for (int i = 1; i < DazedFrames; i++)
            {
                float sway = i == 2 ? 4f : -2f;
                frames[Dazed + i] = Draw(new Pose
                {
                    X = sway * 0.5f, Lean = 4f + sway, Head = 14f + sway, Jaw = 0.45f, PickBehind = false,
                    Arm = 85f, Fore = 85f, Pick = 80f, BackArm = 95f, BackFore = 92f, Gem = 0.3f, Stars = i,
                });
            }

            // O apito: leva a mao ao apito e chama o turno.
            frames[Whistle + 0] = Draw(new Pose { Lean = 2f, Head = -4f, Whistle = true, WhistleLift = 0.5f, Gem = 1f });
            for (int i = 1; i < WhistleFrames; i++)
            {
                frames[Whistle + i] = Draw(new Pose
                {
                    Y = -1f, Lean = -2f - i, Head = -8f, Whistle = true, WhistleLift = 1f, Rings = i / 3f, Gem = 1.2f + i * 0.15f,
                });
            }

            // O grito: a cabeca pra tras, a boca inteira aberta, a picareta pro alto.
            for (int i = 0; i < RoarFrames; i++)
            {
                frames[Roar + i] = Draw(new Pose
                {
                    Y = -1f, Lean = -12f, Head = -26f - (i % 2) * 3f, Jaw = 1f, PickBehind = false,
                    Arm = -40f, Fore = -55f, Pick = -65f, BackArm = -135f, BackFore = -150f,
                    BackFoot = -6f, FrontFoot = 6f, Rings = (i + 1) / 4f, Wide = true, Shake = i % 2 == 0 ? 1.5f : -1.5f, Gem = 2f,
                });
            }

            Pose hurt = new Pose
            {
                X = -3f, Lean = -10f, Head = -12f, Jaw = 0.6f, PickBehind = false, Arm = 95f, Fore = 60f, Pick = -40f,
                FrontLift = 3f, Gem = 0.6f,
            };
            hurt.Sparks.Add((70, 52, 3));
            frames[Hurt] = Draw(hurt);

            DrawDeath(frames);
            return frames;
        }

        private static void DrawDeath(PixelCanvas[] frames)
        {
            // Cambaleia pra tras.
            Pose stagger = new Pose
            {
                X = -4f, Lean = -14f, Head = -18f, Jaw = 0.8f, PickBehind = false, Arm = 100f, Fore = 70f, Pick = -20f,
                FrontLift = 4f, Gem = 2f,
            };
            stagger.Sparks.Add((66, 54, 3));
            frames[Death + 0] = Draw(stagger);

            // Larga a picareta e leva a mao ao peito.
            frames[Death + 1] = Draw(new Pose
            {
                X = -2f, Lean = 6f, Head = 8f, Jaw = 0.5f, PickDropped = true, Clutch = true, BackArm = 95f, BackFore = 90f, Gem = 0.8f,
            });

            // Cai de joelhos.
            frames[Death + 2] = Draw(new Pose
            {
                Kneel = 1f, Lean = 14f, Head = 14f, Jaw = 0.3f, PickDropped = true, Clutch = true, FrontFoot = 6f,
                BackArm = 90f, BackFore = 95f, Gem = 0.5f, Dust = 0.7f, DustX = 56f,
            });

            // O capacete cai e a safira se apaga.
            frames[Death + 3] = Draw(new Pose
            {
                Kneel = 1f, Lean = 12f, Head = 20f, PickDropped = true, HelmetOff = true, NoGem = true, Clutch = true, FrontFoot = 6f,
                BackArm = 90f, BackFore = 95f, Gem = 0f, CrystalDust = 0.3f,
            });

            // Os cristais se desfazem; o rosto volta a ser de gente.
            frames[Death + 4] = Draw(new Pose
            {
                Kneel = 1f, Lean = 10f, Head = -6f, PickDropped = true, HelmetOff = true, NoGem = true, Freed = true, FrontFoot = 6f,
                Arm = 95f, Fore = 90f, BackArm = 92f, BackFore = 92f, Gem = 0f, CrystalDust = 0.8f,
            });

            frames[Death + 5] = Draw(new Pose
            {
                Kneel = 1f, Y = 3f, Lean = 30f, Head = 24f, PickDropped = true, HelmetOff = true, NoGem = true, Freed = true, FrontFoot = 6f,
                Arm = 100f, Fore = 100f, BackArm = 80f, BackFore = 90f, Gem = 0f, CrystalDust = 0.5f,
            });

            // Deitado, em paz.
            frames[Death + 6] = Draw(new Pose { Lying = true, CrystalDust = 0.3f });
            frames[Death + 7] = Draw(new Pose { Lying = true });
        }

        private static float Lerp(float a, float b, float k) => a + (b - a) * k;
    }
}
