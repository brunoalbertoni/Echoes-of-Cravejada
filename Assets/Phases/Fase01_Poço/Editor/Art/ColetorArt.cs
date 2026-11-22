using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Coletor (The Rusty Scrapper), no mesmo detalhe do Scrap: o varredor
    /// a vapor do poco. O corpo e a propria cacamba de sucata — um tambor de
    /// chapa remendada, cheio de lixo saindo pela boca (e um bracinho de
    /// autômato no meio: ele esqueceu a diferenca entre peca solta e peca
    /// viva). Nas costas, a caldeira com a chamine e os pistoes: o motor que
    /// vira o Steam Dash. Na frente, a cabine de lampiao com um olho so,
    /// verde de Oxide, o apito em cima e a pa dentada embaixo, e o braco da
    /// garra de tres dentes.
    ///
    /// Cada quadro tem 128x112, olha pra direita e e importado com 32 pixels
    /// por tile (4 x 3,5 tiles). Os pes ficam na base, no meio — e o pivo.
    ///
    /// Ha dois jogos dos mesmos quadros: o inteiro e o rachado (da metade da
    /// vida pra baixo), com rachaduras vazando verde, um remendo arrancado e
    /// fumaca esverdeada.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static partial class ColetorArt
    {
        public const int Width = 128;
        public const int Height = 112;
        public const int PixelsPerUnit = 32;

        /// <summary>O pivo, em fracao da altura: na sola das sapatas, 1 px acima da base.</summary>
        public const float PivotY = 1f / Height;

        // A ordem dos quadros. O RustyScrapper, no jogo, usa os mesmos numeros.
        public const int Dormant = 0;
        public const int DormantFrames = 4;
        public const int Wake = 4;
        public const int WakeFrames = 4;
        public const int Idle = 8;
        public const int IdleFrames = 6;
        public const int Walk = 14;
        public const int WalkFrames = 8;
        public const int ClawWindup = 22;
        public const int ClawWindupFrames = 2;
        public const int ClawStrike = 24;
        public const int ClawStrikeFrames = 3;
        public const int ThrowReach = 27;
        public const int ThrowReachFrames = 2;
        public const int ThrowRelease = 29;
        public const int ThrowReleaseFrames = 2;
        public const int SlamCrouch = 31;
        public const int SlamCrouchFrames = 2;
        public const int SlamRise = 33;
        public const int SlamAir = 34;
        public const int SlamFall = 35;
        public const int SlamImpact = 36;
        public const int SlamImpactFrames = 2;
        public const int Hurt = 38;
        public const int Roar = 39;
        public const int RoarFrames = 4;
        public const int Death = 43;
        public const int DeathFrames = 8;
        public const int FrameCount = 51;

        private sealed class Pose
        {
            public float X;
            public float Y;

            /// <summary>Inclinacao pra frente (positivo) ou pra tras: leva a cabeca e o ombro junto.</summary>
            public float Lean;

            /// <summary>Menor que 1 achata o tambor, maior estica.</summary>
            public float Squash = 1f;

            public float BackFootX;
            public float BackLift;
            public float FrontFootX;
            public float FrontLift;

            /// <summary>Joelho de tras no chao (ajoelhado).</summary>
            public bool Kneel;

            /// <summary>O braco da garra: angulo do braco e do antebraco (0 = pra frente, 90 = pra baixo) e a abertura.</summary>
            public float Arm = 65f;
            public float Fore = 15f;
            public float Open = 0.3f;

            /// <summary>O braco de tras, o que pega sucata na cacamba.</summary>
            public float BackArm = 112f;
            public float BackFore = 95f;
            public bool Holding;

            public float HeadX;
            public float HeadY;
            public float Jaw = 0.1f;
            public float Eye = 1f;
            public bool Squint;

            /// <summary>A fumaca da chamine: em que ponto do ciclo e quantas baforadas.</summary>
            public float Smoke;
            public int Puffs = 2;

            /// <summary>O vapor do apito (0 nada, 1 apitando).</summary>
            public float Steam;

            /// <summary>O brilho da fornalha.</summary>
            public float Furnace = 0.6f;

            /// <summary>O rastro da garra: do angulo inicial ao final, em volta do ombro (iguais = sem rastro).</summary>
            public float SmearFrom;
            public float SmearTo;

            public float Dust;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();

            // A queda.
            public bool PlateOff;
            public float PlateFlyX;
            public float PlateFlyY;
            public bool ChimneyBent;
            public float Burst;
        }

        /// <summary>Todos os quadros de um dos dois jogos, na ordem das constantes.</summary>
        public static PixelCanvas[] Frames(bool cracked)
        {
            var frames = new PixelCanvas[FrameCount];

            // Dormindo: arriado, o olho apagado, a garra largada no chao. So
            // um piscar fraco do olho no terceiro quadro.
            for (int i = 0; i < DormantFrames; i++)
            {
                frames[Dormant + i] = Draw(cracked, new Pose
                {
                    Y = 6f + (i == 1 || i == 2 ? 0.5f : 0f), Lean = 4f, HeadY = 5f, HeadX = -1f,
                    Arm = 82f, Fore = 78f, Open = 0.65f, BackArm = 100f, BackFore = 90f,
                    Jaw = 0.45f, Eye = i == 2 ? 0.35f : 0f, Puffs = 0, Furnace = 0.15f,
                    BackFootX = -1f, FrontFootX = 1f,
                });
            }

            // Acordando: o olho tremula e acende, ele se levanta, a chamine
            // tosse a primeira fumaca e o apito solta um assobio.
            float[] wakeEye = { 0.5f, 0f, 1.2f, 1.8f };
            for (int i = 0; i < WakeFrames; i++)
            {
                float k = (i + 1) / (float)WakeFrames;
                frames[Wake + i] = Draw(cracked, new Pose
                {
                    Y = 6f * (1f - k), Lean = 4f * (1f - k) - (i == 3 ? 1f : 0f), HeadY = 5f * (1f - k) - (i == 3 ? 2f : 0f),
                    Arm = Lerp(82f, 55f, k), Fore = Lerp(78f, 5f, k), Open = Lerp(0.65f, 0.9f, k),
                    BackArm = Lerp(100f, 112f, k), BackFore = 95f,
                    Jaw = Lerp(0.45f, i == 3 ? 0.6f : 0.1f, k), Eye = wakeEye[i], Puffs = i >= 2 ? 1 : 0, Smoke = i * 0.2f,
                    Steam = i == 3 ? 1f : 0f, Furnace = Lerp(0.15f, 0.9f, k),
                });
            }

            // Parado: respira pela caldeira, a fumaca sobe, a garra abre e fecha devagar.
            float[] breath = { 0f, 0.5f, 1f, 1f, 0.5f, 0f };
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(cracked, new Pose
                {
                    Y = breath[i], HeadY = breath[i] * 0.5f, Arm = 65f + breath[i] * 3f, Fore = 15f + breath[i] * 4f,
                    Open = 0.25f + 0.15f * (float)Math.Sin(i * Math.PI / 3.0),
                    Smoke = i / (float)IdleFrames, Furnace = 0.55f + 0.25f * (float)Math.Sin(i * Math.PI / 3.0),
                });
            }

            // Andando: passada pesada, o tambor afunda no apoio e os bracos balancam.
            for (int i = 0; i < WalkFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / WalkFrames;
                float swing = (float)Math.Sin(phase);
                float bob = (float)Math.Abs(Math.Cos(phase));

                frames[Walk + i] = Draw(cracked, new Pose
                {
                    Y = (1f - bob) * 2.5f, Lean = 1.5f, HeadY = (1f - bob) * 1f,
                    BackFootX = -swing * 8f, FrontFootX = swing * 8f,
                    BackLift = swing > 0.2f ? swing * 5f : 0f, FrontLift = swing < -0.2f ? -swing * 5f : 0f,
                    Arm = 62f + swing * 10f, Fore = 15f + swing * 6f, Open = 0.3f,
                    BackArm = 112f - swing * 10f,
                    Smoke = i / (float)WalkFrames, Furnace = 0.7f,
                });
            }

            // A garra: ergue o braco por cima da cabeca, a garra escancarada...
            frames[ClawWindup + 0] = Draw(cracked, new Pose
            {
                X = -2f, Lean = -2f, HeadX = -1f, Arm = -55f, Fore = -95f, Open = 0.75f,
                BackFootX = -3f, FrontFootX = 3f, Eye = 1.4f, Smoke = 0.3f, Furnace = 0.8f,
            });
            frames[ClawWindup + 1] = Draw(cracked, new Pose
            {
                X = -3f, Y = 1.5f, Lean = -4f, HeadX = -2f, HeadY = -1f, Arm = -105f, Fore = -150f, Open = 1f,
                BackFootX = -5f, FrontFootX = 4f, Eye = 1.8f, Squint = true, Jaw = 0.3f, Smoke = 0.4f, Furnace = 1f,
            });

            // ...e avanca baixando a garra, que fecha com uma faisca.
            frames[ClawStrike + 0] = Draw(cracked, new Pose
            {
                X = 4f, Lean = 3f, HeadX = 1f, Arm = -15f, Fore = 5f, Open = 0.9f, SmearFrom = -150f, SmearTo = -10f,
                BackFootX = -7f, FrontFootX = 6f, Eye = 1.6f, Squint = true, Jaw = 0.4f, Smoke = 0.5f,
            });
            Pose snap = new Pose
            {
                X = 6f, Y = 1f, Lean = 5f, HeadX = 2f, Arm = 18f, Fore = 22f, Open = 0f, SmearFrom = -60f, SmearTo = 22f,
                BackFootX = -8f, FrontFootX = 7f, Eye = 1.6f, Squint = true, Jaw = 0.5f, Smoke = 0.6f,
            };
            snap.Sparks.Add((119, 72, 3));
            snap.Sparks.Add((114, 80, 2));
            frames[ClawStrike + 1] = Draw(cracked, snap);
            frames[ClawStrike + 2] = Draw(cracked, new Pose
            {
                X = 4f, Y = 1f, Lean = 3f, HeadX = 1f, Arm = 45f, Fore = 35f, Open = 0.15f,
                BackFootX = -6f, FrontFootX = 6f, Eye = 1.2f, Jaw = 0.2f, Smoke = 0.7f,
            });

            // O arremesso: enfia a mao de tras na cacamba, tira um bolo de sucata...
            frames[ThrowReach + 0] = Draw(cracked, new Pose
            {
                Lean = -1f, BackArm = -70f, BackFore = 40f, Arm = 50f, Fore = 10f, Open = 0.4f, Eye = 1.4f, Smoke = 0.2f,
            });
            frames[ThrowReach + 1] = Draw(cracked, new Pose
            {
                X = -2f, Lean = -3f, HeadX = -1f, BackArm = -110f, BackFore = -165f, Holding = true,
                Arm = 40f, Fore = -5f, Open = 0.6f, Eye = 1.8f, Squint = true, BackFootX = -4f, FrontFootX = 4f, Smoke = 0.4f,
            });

            // ...e joga por cima da cabeca.
            frames[ThrowRelease + 0] = Draw(cracked, new Pose
            {
                X = 2f, Lean = 3f, HeadX = 1f, BackArm = -40f, BackFore = -15f,
                Arm = 70f, Fore = 40f, Open = 0.5f, Eye = 1.6f, Jaw = 0.5f, BackFootX = -6f, FrontFootX = 5f, Smoke = 0.6f,
            });
            frames[ThrowRelease + 1] = Draw(cracked, new Pose
            {
                X = 3f, Y = 1f, Lean = 4f, HeadX = 1f, BackArm = 15f, BackFore = 50f,
                Arm = 75f, Fore = 45f, Open = 0.3f, Eye = 1.2f, Jaw = 0.2f, BackFootX = -6f, FrontFootX = 5f, Smoke = 0.8f,
            });

            // O salto: agacha com os dois bracos pra cima...
            frames[SlamCrouch + 0] = Draw(cracked, new Pose
            {
                Y = 4f, Squash = 0.93f, HeadY = 1f, Arm = -70f, Fore = -95f, Open = 0.8f, BackArm = -100f, BackFore = -120f,
                BackFootX = -3f, FrontFootX = 3f, Eye = 1.5f, Smoke = 0.2f, Puffs = 3, Furnace = 0.9f,
            });
            frames[SlamCrouch + 1] = Draw(cracked, new Pose
            {
                Y = 8f, Squash = 0.86f, HeadY = 2f, Arm = -95f, Fore = -125f, Open = 1f, BackArm = -110f, BackFore = -140f,
                BackFootX = -5f, FrontFootX = 5f, Eye = 2f, Squint = true, Jaw = 0.4f, Smoke = 0.4f, Puffs = 3, Furnace = 1f,
                Steam = 0.6f,
            });

            // ...sobe esticado, as pernas encolhidas...
            frames[SlamRise] = Draw(cracked, new Pose
            {
                Y = -2f, Squash = 1.07f, HeadY = -1f, Arm = -80f, Fore = -100f, Open = 1f, BackArm = -105f, BackFore = -120f,
                BackFootX = -3f, FrontFootX = 3f, BackLift = 7f, FrontLift = 6f, Eye = 1.6f, Smoke = 0.7f, Puffs = 3,
            });

            // ...paira no alto, a garra fechada em punho...
            frames[SlamAir] = Draw(cracked, new Pose
            {
                Y = -2f, Lean = 2f, Arm = -60f, Fore = -40f, Open = 0f, BackArm = -90f, BackFore = -60f,
                BackFootX = -4f, FrontFootX = 2f, BackLift = 6f, FrontLift = 8f, Eye = 2f, Squint = true, Jaw = 0.7f,
                Smoke = 0.9f, Puffs = 3,
            });

            // ...despenca com os bracos descendo...
            frames[SlamFall] = Draw(cracked, new Pose
            {
                Y = -1f, Squash = 1.08f, Lean = 3f, Arm = 40f, Fore = 75f, Open = 0f, BackArm = 30f, BackFore = 80f,
                BackFootX = -2f, FrontFootX = 3f, BackLift = 2f, FrontLift = 1f, Eye = 2f, Squint = true, Jaw = 0.8f,
                Smoke = 0.1f, Puffs = 1,
            });

            // ...e bate no chao: tudo achata, poeira e faisca.
            Pose impact = new Pose
            {
                Y = 8f, Squash = 0.84f, Lean = 4f, HeadY = 2f, Arm = 72f, Fore = 88f, Open = 0f, BackArm = 70f, BackFore = 95f,
                BackFootX = -6f, FrontFootX = 6f, Eye = 1.6f, Squint = true, Jaw = 0.6f, Dust = 1f, Smoke = 0.3f, Puffs = 3,
                Furnace = 1f,
            };
            impact.Sparks.Add((100, 106, 3));
            impact.Sparks.Add((92, 102, 2));
            frames[SlamImpact + 0] = Draw(cracked, impact);
            frames[SlamImpact + 1] = Draw(cracked, new Pose
            {
                Y = 4f, Squash = 0.93f, Lean = 2f, HeadY = 1f, Arm = 68f, Fore = 70f, Open = 0.2f, BackArm = 90f, BackFore = 95f,
                BackFootX = -5f, FrontFootX = 5f, Eye = 1.2f, Jaw = 0.3f, Dust = 2f, Smoke = 0.5f, Puffs = 3,
            });

            // Apanhando: tranco pra tras, olho apertado, faisca no tambor.
            Pose hurt = new Pose
            {
                X = -3f, Lean = -4f, HeadX = -2f, HeadY = -2f, Arm = 85f, Fore = 60f, Open = 0.7f, BackArm = 125f,
                BackFootX = -2f, FrontFootX = 4f, FrontLift = 2f, Eye = 0.8f, Squint = true, Jaw = 0.55f, Smoke = 0.5f,
            };
            hurt.Sparks.Add((60, 52, 3));
            hurt.Sparks.Add((70, 62, 2));
            frames[Hurt] = Draw(cracked, hurt);

            // O urro da metade da luta: ergue a cabeca, abre a pa, apita, o
            // olho estoura e a chamine cospe.
            float[] roarJaw = { 0.5f, 1f, 1f, 0.7f };
            float[] roarSteam = { 0.5f, 1f, 1f, 0.6f };
            for (int i = 0; i < RoarFrames; i++)
            {
                frames[Roar + i] = Draw(cracked, new Pose
                {
                    X = -1f, Lean = -3f, HeadY = -4f - (i == 1 || i == 2 ? 1f : 0f), HeadX = 1f,
                    Arm = -5f - i * 3f, Fore = -20f - i * 4f, Open = 1f, BackArm = -120f, BackFore = -100f,
                    BackFootX = -6f, FrontFootX = 6f, Jaw = roarJaw[i], Steam = roarSteam[i], Eye = 2f,
                    Smoke = i * 0.25f, Puffs = 3, Furnace = 1f,
                });
            }

            DrawDeath(frames, cracked);
            return frames;
        }

        /// <summary>
        /// A queda: cambaleia, um remendo salta, os joelhos cedem, ele
        /// ajoelha com a cabeca caida e o verde vai tomando o corpo ate o estouro.
        /// </summary>
        private static void DrawDeath(PixelCanvas[] frames, bool cracked)
        {
            Pose stagger = new Pose
            {
                X = -3f, Lean = -4f, HeadX = -2f, HeadY = -2f, Arm = 92f, Fore = 100f, Open = 0.9f, BackArm = 130f,
                BackFootX = -3f, FrontFootX = 5f, Eye = 2f, Jaw = 0.8f, Smoke = 0.2f, Puffs = 3, Furnace = 1f,
            };
            stagger.Sparks.Add((52, 50, 3));
            stagger.Sparks.Add((76, 60, 2));
            frames[Death + 0] = Draw(true, stagger);

            Pose pop = new Pose
            {
                X = -2f, Y = 2f, Lean = -2f, HeadX = -1f, Arm = 95f, Fore = 105f, Open = 1f, BackArm = 125f,
                BackFootX = -4f, FrontFootX = 5f, Eye = 0.5f, Jaw = 0.9f, Smoke = 0.4f, Puffs = 3, Furnace = 1f,
                PlateOff = true, PlateFlyX = -14f, PlateFlyY = -22f,
            };
            pop.Sparks.Add((42, 50, 2));
            pop.Sparks.Add((48, 44, 3));
            frames[Death + 1] = Draw(true, pop);

            frames[Death + 2] = Draw(true, new Pose
            {
                Y = 8f, Lean = 1f, HeadY = 2f, Arm = 95f, Fore = 110f, Open = 0.8f, BackArm = 115f,
                BackFootX = -10f, FrontFootX = 6f, Kneel = true, Eye = 1.6f, Jaw = 0.7f, Steam = 1f,
                Smoke = 0.6f, Puffs = 3, Furnace = 1f, PlateOff = true, PlateFlyX = -26f, PlateFlyY = 30f,
            });

            frames[Death + 3] = Draw(true, new Pose
            {
                Y = 13f, Lean = 3f, HeadY = 7f, Arm = 96f, Fore = 112f, Open = 1f, BackArm = 105f,
                BackFootX = -12f, FrontFootX = 6f, Kneel = true, Eye = 0.3f, Jaw = 0.9f, ChimneyBent = true,
                Smoke = 0.8f, Puffs = 2, Furnace = 1f, PlateOff = true, PlateFlyX = -26f, PlateFlyY = 40f,
            });

            float[] burst = { 0.25f, 0.45f, 0.68f, 0.9f };
            for (int i = 0; i < 4; i++)
            {
                frames[Death + 4 + i] = Draw(true, new Pose
                {
                    Y = 13f + i * 0.7f, Lean = 3f + i, HeadY = 7f + i * 0.5f, Arm = 96f, Fore = 112f, Open = 1f, BackArm = 105f,
                    BackFootX = -12f, FrontFootX = 6f, Kneel = true, Eye = i % 2 == 0 ? 2f : 0.6f, Jaw = 1f,
                    ChimneyBent = true, Smoke = i * 0.25f, Puffs = 3, Furnace = 1f, Burst = burst[i], Steam = i % 2 == 0 ? 1f : 0.5f,
                    PlateOff = true, PlateFlyX = -26f, PlateFlyY = 40f,
                });
            }
        }

        private static float Lerp(float a, float b, float k) => a + (b - a) * k;

        // ------------------------------------------------------------------
        // Montagem de um quadro
        // ------------------------------------------------------------------

        private const float TorsoX = 56f;
        private const float TorsoY = 67f;
        private const float TorsoRX = 23f;
        private const float TorsoRY = 25f;
        private const float GroundY = 109f;

        private sealed class Frame
        {
            public PixelCanvas Canvas = new PixelCanvas(Width, Height);
            public Pose Pose;
            public bool Cracked;

            public float TX;
            public float TY;
            public float RX;
            public float RY;

            public float Top => TY - RY;

            public float HeadX;
            public float HeadY;
            public float ShoulderX;
            public float ShoulderY;
            public float WristX;
            public float WristY;
            public float BackHandX;
            public float BackHandY;
        }

        private static PixelCanvas Draw(bool cracked, Pose pose)
        {
            var f = new Frame { Pose = pose, Cracked = cracked };

            f.RX = TorsoRX / (float)Math.Sqrt(pose.Squash);
            f.RY = TorsoRY * pose.Squash;
            f.TX = TorsoX + pose.X;
            f.TY = TorsoY + pose.Y + (1f - pose.Squash) * 22f;

            f.HeadX = f.TX + 27f + pose.Lean + pose.HeadX;
            f.HeadY = f.Top - 1f + pose.Lean * 0.8f + pose.HeadY;
            f.ShoulderX = f.TX + 15f + pose.Lean * 0.5f;
            f.ShoulderY = f.TY - 7f + pose.Lean * 0.3f;

            (float ex, float ey) = ArtKit.Along(f.ShoulderX, f.ShoulderY, pose.Arm, 17f);
            (f.WristX, f.WristY) = ArtKit.Along(ex, ey, pose.Fore, 18f);

            float bsx = f.TX - 10f;
            float bsy = f.TY - 13f;
            (float bex, float bey) = ArtKit.Along(bsx, bsy, pose.BackArm, 16f);
            (f.BackHandX, f.BackHandY) = ArtKit.Along(bex, bey, pose.BackFore, 15f);

            DrawMotor(f);
            DrawBackArm(f, bsx, bsy, bex, bey);
            DrawLeg(f, true);
            DrawBin(f);
            DrawLeg(f, false);
            DrawHead(f);
            DrawArm(f, ex, ey);
            if (pose.PlateOff) DrawFlyingPlate(f);

            ArtKit.AddOutline(f.Canvas);

            DrawGlows(f);
            DrawSmear(f);
            DrawSmoke(f);
            DrawSteam(f);
            DrawDust(f);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(f.Canvas, spark.X, spark.Y, spark.Size);
            if (pose.Burst > 0f) DrawBurst(f);

            return f.Canvas;
        }
    }
}
