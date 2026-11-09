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
    public static class ColetorArt
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

        // ------------------------------------------------------------------
        // As costas: a caldeira, a chamine e os pistoes
        // ------------------------------------------------------------------

        private static void DrawMotor(Frame f)
        {
            Pose pose = f.Pose;
            float bx = f.TX - 25f;
            float by = f.TY + 2f;

            // A chamine: sobe da caldeira, um pouco torta pra tras; na queda, dobrada.
            float topX = bx - 5f;
            float topY = f.Top - 20f;
            float midX = bx - 2f;
            float midY = (by + topY) * 0.5f;
            if (pose.ChimneyBent)
            {
                topX = bx - 18f;
                topY = f.Top - 4f;
            }

            ArtKit.Skin chimney = ArtKit.Corroded(ArtKit.Iron, 0.25f, 31, 0.3f, ArtKit.Rust);
            ArtKit.Limb(f.Canvas, bx, by - 8f, midX, midY, 3.4f, chimney, 0.05f);
            ArtKit.Limb(f.Canvas, midX, midY, topX, topY, 3.2f, chimney, 0.05f);
            ArtKit.Ball(f.Canvas, midX, midY, 3.8f, 2.2f, ArtKit.Iron, 0.2f);

            // O chapeu da chamine: aba larga.
            ArtKit.Drum(f.Canvas, topX, topY - 1f, 5.5f, 2.2f, 3f, ArtKit.Plain(ArtKit.Rust), 0.1f);

            // A caldeira: um cilindro de latao com azinhavre, cintas e o manometro.
            ArtKit.Drum(f.Canvas, bx, by, 9f, 14f, 2.6f, ArtKit.Corroded(ArtKit.Brass, 0.3f, 23, 0.22f), 0.1f);
            for (int band = -1; band <= 1; band += 2)
            {
                float y = by + band * 7f;
                for (float x = bx - 8.5f; x <= bx + 8.5f; x += 1f)
                {
                    f.Canvas.Set((int)x, (int)y, ArtKit.Iron[1]);
                    f.Canvas.Set((int)x, (int)y + 1, ArtKit.Iron[0]);
                }

                for (float x = bx - 6f; x <= bx + 6f; x += 4f) f.Canvas.Set((int)x, (int)y, ArtKit.Iron[3]);
            }

            // O manometro: mostrador creme, ponteiro.
            ArtKit.Ball(f.Canvas, bx - 3f, by - 1f, 3.4f, 3.4f, ArtKit.Brass, 0.3f);
            ArtKit.Ball(f.Canvas, bx - 3f, by - 1f, 2.3f, 2.3f, new[] { new Rgba(150, 140, 110), new Rgba(190, 180, 150), new Rgba(222, 214, 186), new Rgba(240, 236, 214) }, 0.4f);
            float needle = -140f + pose.Furnace * 120f;
            (float nx, float ny) = ArtKit.Along(bx - 3f, by - 1f, needle, 2f);
            ArtKit.Line(f.Canvas, (int)(bx - 3f), (int)(by - 1f), (int)nx, (int)ny, new Rgba(160, 40, 30));

            // Os dois pistoes que descem pro quadril de tras.
            float hipX = f.TX - 9f;
            float hipY = f.TY + 19f;
            for (int i = 0; i < 2; i++)
            {
                float sx = bx + 4f + i * 3f;
                float sy = by + 10f;
                ArtKit.Limb(f.Canvas, sx, sy, hipX - 2f + i * 3f, hipY - 2f, 1.6f, ArtKit.Iron, -0.1f);
                ArtKit.Limb(f.Canvas, sx, sy, (sx + hipX) * 0.5f, (sy + hipY) * 0.5f, 2.2f, ArtKit.Brass, 0f);
            }
        }

        // ------------------------------------------------------------------
        // O tambor-cacamba
        // ------------------------------------------------------------------

        private static void DrawBin(Frame f)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;
            float tx = f.TX;
            float ty = f.TY;

            // A chapa: ferro enferrujado, com azinhavre escorrendo.
            ArtKit.Skin sheet = (x, y, light) =>
            {
                float n = ArtKit.Noise(x * 0.24f, y * 0.15f, 5);
                float drip = 0.18f + 0.4f * Math.Max(0f, (y + 10f) / 40f);
                if (n > 1f - drip * 0.4f) return ArtKit.Tone(ArtKit.Verdigris, light - 0.1f);
                if (ArtKit.Hash((int)Math.Floor(x), (int)Math.Floor(y), 8) % 17 == 0) return ArtKit.Tone(ArtKit.Rust, light - 0.35f);
                return ArtKit.Tone(ArtKit.Rust, light);
            };

            ArtKit.Drum(c, tx, ty, f.RX, f.RY, 3.2f, sheet, 0.05f);

            // As costuras verticais da chapa, com rebites.
            foreach (float u in new[] { -0.55f, 0.15f, 0.7f })
            {
                int x = (int)(tx + u * f.RX);
                for (int y = (int)(f.Top + 5f); y < (int)(ty + f.RY - 3f); y++)
                {
                    if (c.Get(x, y).A == 0) continue;
                    c.Set(x, y, ArtKit.Shift(ArtKit.Rust, ArtKit.Tone(ArtKit.Rust, -0.6f * u), -1));
                    if (y % 5 == 0) ArtKit.Rivet(c, x + 1, y, ArtKit.Iron);
                }
            }

            // As cintas de ferro, rebitadas.
            foreach (float v in new[] { -0.5f, 0.62f })
            {
                int y = (int)(ty + v * f.RY);
                float half = f.RX * (float)Math.Pow(1.0 - Math.Pow(Math.Abs(v), 3.2), 1.0 / 3.2);

                for (int x = (int)(tx - half) + 1; x <= (int)(tx + half) - 1; x++)
                {
                    float u = (x - tx) / f.RX;
                    float light = -0.75f * u + 0.2f;
                    c.Set(x, y - 1, ArtKit.Tone(ArtKit.Iron, light + 0.4f));
                    c.Set(x, y, ArtKit.Tone(ArtKit.Iron, light));
                    c.Set(x, y + 1, ArtKit.Tone(ArtKit.Iron, light - 0.5f));
                    if (x % 5 == 0) c.Set(x, y, ArtKit.Iron[3]);
                }
            }

            // Os remendos: um de cobre, atras (arrancado na queda e no rachado),
            // e um de lata amassada, na frente.
            bool patchGone = pose.PlateOff;
            DrawPatch(c, tx - 15f, ty - 7f, 11, 9, ArtKit.Copper, patchGone, f.Cracked);
            DrawPatch(c, tx + 5f, ty - 18f, 10, 7, ArtKit.Tin, false, false);

            // A fornalha na barriga: grade de ferro com o verde aceso atras.
            DrawFurnace(f, tx + 6f, ty + 4f);

            // A boca da cacamba e a sucata saindo dela.
            float mouthY = f.Top + 3f;
            for (int py = (int)(mouthY - 4f); py <= (int)(mouthY + 4f); py++)
            {
                for (int px = (int)(tx - f.RX); px <= (int)(tx + f.RX); px++)
                {
                    float dx = (px + 0.5f - tx) / (f.RX - 1f);
                    float dy = (py + 0.5f - mouthY) / 3.6f;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;
                    c.Set(px, py, d > 0.62f ? ArtKit.Tone(ArtKit.Iron, dy < 0 ? 0.5f : -0.2f) : ArtKit.Iron[0]);
                }
            }

            DrawJunk(f, mouthY);

            // A beirada da frente da boca, por cima da sucata.
            for (int px = (int)(tx - f.RX) + 1; px <= (int)(tx + f.RX) - 1; px++)
            {
                float dx = (px + 0.5f - tx) / (f.RX - 1f);
                if (dx * dx > 1f) continue;
                int py = (int)(mouthY + 3.6f * (float)Math.Sqrt(1f - dx * dx));
                c.Set(px, py, ArtKit.Tone(ArtKit.Iron, 0.3f - dx * 0.5f));
                c.Set(px, py + 1, ArtKit.Tone(ArtKit.Iron, 0.7f - dx * 0.6f));
            }

            if (f.Cracked)
            {
                ArtKit.Crack(c, tx + 14f, f.Top + 8f, 100f, 14, 3, 1f);
                ArtKit.Crack(c, tx - 4f, ty + 15f, 70f, 9, 9, 0.9f);
                ArtKit.Crack(c, tx + 18f, ty + 6f, 120f, 8, 12, 1f);

                // Verde escorrendo pela chapa.
                foreach (float u in new[] { -0.3f, 0.45f })
                {
                    int x = (int)(tx + u * f.RX);
                    int length = 5 + (int)(Math.Abs(u) * 8f);
                    for (int i = 0; i < length; i++) c.Set(x, (int)(f.Top + 6f) + i, i % 3 == 0 ? ArtKit.OxideDeep : ArtKit.OxideGlow);
                }
            }
        }

        private static void DrawPatch(PixelCanvas c, float x, float y, int w, int h, Rgba[] ramp, bool gone, bool cracked)
        {
            int x0 = (int)x;
            int y0 = (int)y;

            if (gone)
            {
                // O buraco: escuro por dentro, com fiacao e o verde la no fundo.
                for (int py = y0; py < y0 + h; py++)
                {
                    for (int px = x0; px < x0 + w; px++)
                    {
                        if (c.Get(px, py).A == 0) continue;
                        bool edge = px == x0 || py == y0 || px == x0 + w - 1 || py == y0 + h - 1;
                        float d = Math.Abs(px - (x0 + w * 0.5f)) / w + Math.Abs(py - (y0 + h * 0.5f)) / h;
                        c.Set(px, py, edge ? ArtKit.Rust[0] : (d < 0.35f ? ArtKit.OxideGlow : ArtKit.Iron[0]));
                    }
                }

                c.Set(x0 + w / 2, y0 + h / 2, ArtKit.OxideCore);
                ArtKit.Line(c, x0 + 1, y0 + 2, x0 + w - 3, y0 + h - 2, ArtKit.Copper[2]);
                return;
            }

            for (int py = y0; py < y0 + h; py++)
            {
                for (int px = x0; px < x0 + w; px++)
                {
                    if (c.Get(px, py).A == 0) continue;
                    float light = py == y0 ? 0.8f : (px == x0 ? 0.4f : (py == y0 + h - 1 || px == x0 + w - 1 ? -0.6f : 0.05f));

                    // Amassados: faixas diagonais um tom abaixo.
                    if ((px - py) % 7 == 0 && light > -0.5f && light < 0.5f) light -= 0.45f;
                    c.Set(px, py, ArtKit.Tone(ramp, light));
                }
            }

            ArtKit.Rivet(c, x0 + 1, y0 + 1, ArtKit.Iron);
            ArtKit.Rivet(c, x0 + w - 3, y0 + 1, ArtKit.Iron);
            ArtKit.Rivet(c, x0 + 1, y0 + h - 3, ArtKit.Iron);
            ArtKit.Rivet(c, x0 + w - 3, y0 + h - 3, ArtKit.Iron);

            if (cracked) ArtKit.Crack(c, x0 + w * 0.5f, y0 + 1, 80f, h - 2, 2, 0.9f);
        }

        private static void DrawFurnace(Frame f, float cx, float cy)
        {
            PixelCanvas c = f.Canvas;
            float glow = f.Pose.Furnace;
            const int w = 15;
            const int h = 11;
            int x0 = (int)(cx - w / 2f);
            int y0 = (int)(cy - h / 2f);

            for (int py = y0; py < y0 + h; py++)
            {
                for (int px = x0; px < x0 + w; px++)
                {
                    bool corner = (px == x0 || px == x0 + w - 1) && (py == y0 || py == y0 + h - 1);
                    if (corner) continue;

                    bool frame = px <= x0 + 1 || px >= x0 + w - 2 || py <= y0 + 1 || py >= y0 + h - 2;
                    if (frame)
                    {
                        float light = py <= y0 + 1 ? 0.5f : (px <= x0 + 1 ? 0.2f : -0.5f);
                        c.Set(px, py, ArtKit.Tone(ArtKit.Iron, light));
                        continue;
                    }

                    // Atras da grade: o fogo verde, mais forte embaixo.
                    float depth = (py - y0) / (float)h;
                    Rgba fire = glow < 0.3f
                        ? ArtKit.Iron[0]
                        : (depth > 0.6f && glow > 0.5f ? ArtKit.OxideCore : (depth > 0.3f ? ArtKit.OxideGlow : ArtKit.OxideDeep));
                    c.Set(px, py, fire);
                }
            }

            // As barras da grade.
            for (int bar = 0; bar < 3; bar++)
            {
                int x = x0 + 4 + bar * 3;
                for (int py = y0 + 2; py < y0 + h - 2; py++) c.Set(x, py, ArtKit.Iron[py == y0 + 2 ? 3 : 1]);
            }

            // A portinhola: dobradica do lado.
            c.Set(x0 - 1, y0 + 3, ArtKit.Iron[2]);
            c.Set(x0 - 1, y0 + h - 4, ArtKit.Iron[2]);
        }

        /// <summary>A sucata saindo da boca: um cano torto, meia engrenagem, uma lata, uma haste e um bracinho de autômato.</summary>
        private static void DrawJunk(Frame f, float mouthY)
        {
            PixelCanvas c = f.Canvas;
            float tx = f.TX;

            // O cano torto, pra tras.
            ArtKit.Limb(c, tx - 11f, mouthY, tx - 17f, mouthY - 11f, 2.2f, ArtKit.Tin, 0.1f);
            ArtKit.Ball(c, tx - 17f, mouthY - 12f, 2.6f, 2.2f, ArtKit.Tin, 0.3f);
            c.Set((int)(tx - 17f), (int)(mouthY - 12f), ArtKit.Iron[0]);

            // Meia engrenagem de latao.
            for (int py = (int)(mouthY - 9f); py <= (int)mouthY; py++)
            {
                for (int px = (int)(tx - 9f); px <= (int)(tx + 5f); px++)
                {
                    float dx = px + 0.5f - (tx - 2f);
                    float dy = py + 0.5f - (mouthY + 1f);
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    bool tooth = Math.Cos(Math.Atan2(dy, dx) * 9) > 0.3;
                    if (r > (tooth ? 7.6f : 6.2f) || r < 2f) continue;
                    c.Set(px, py, ArtKit.Tone(ArtKit.Brass, -(dx * 0.5f + dy * 0.7f) / Math.Max(r, 0.1f)));
                }
            }

            // Uma haste de ferro espetada.
            ArtKit.Limb(c, tx + 5f, mouthY, tx + 7f, mouthY - 13f, 1f, ArtKit.Iron, 0.3f);

            // Uma lata.
            ArtKit.Drum(c, tx + 10f, mouthY - 3f, 3.4f, 4f, 3f, ArtKit.Plain(ArtKit.Tin), 0.1f);
            c.Set((int)(tx + 9f), (int)(mouthY - 4f), new Rgba(170, 60, 50));
            c.Set((int)(tx + 10f), (int)(mouthY - 4f), new Rgba(170, 60, 50));

            // O bracinho de autômato, com a mao aberta pra cima.
            float ax = tx + 1f;
            float ay = mouthY - 2f;
            ArtKit.Limb(c, ax, ay, ax + 2f, ay - 9f, 1.4f, ArtKit.Copper, 0.1f);
            ArtKit.Ball(c, ax + 2f, ay - 10f, 1.8f, 1.6f, ArtKit.Copper, 0.2f);
            c.Set((int)(ax + 1f), (int)(ay - 13f), ArtKit.Copper[2]);
            c.Set((int)(ax + 2f), (int)(ay - 13f), ArtKit.Copper[3]);
            c.Set((int)(ax + 3f), (int)(ay - 13f), ArtKit.Copper[2]);
            c.Set((int)(ax + 4f), (int)(ay - 12f), ArtKit.Copper[1]);
        }

        // ------------------------------------------------------------------
        // Pernas
        // ------------------------------------------------------------------

        private static void DrawLeg(Frame f, bool back)
        {
            Pose pose = f.Pose;
            float hipX = f.TX + (back ? -9f : 8f);
            float hipY = f.TY + f.RY - 6f;

            float footX = TorsoX + pose.X * 0.4f + (back ? -11f + pose.BackFootX : 10f + pose.FrontFootX);
            float footY = GroundY - 3f - (back ? pose.BackLift : pose.FrontLift);

            float bias = back ? -0.4f : 0f;
            ArtKit.Skin metal = ArtKit.Corroded(ArtKit.Iron, 0.2f, back ? 61 : 62, 0.3f, ArtKit.Rust);

            (float kx, float ky) = ArtKit.Joint(hipX, hipY, footX, footY, 13f, 13f, -1);

            // Ajoelhado: o joelho de tras vai ao chao.
            if (back && pose.Kneel)
            {
                kx = hipX + 6f;
                ky = GroundY - 4f;
                footX = kx - 13f;
                footY = GroundY - 3f;
            }

            ArtKit.Limb(f.Canvas, hipX, hipY, kx, ky, 5.2f, metal, bias);
            ArtKit.Ball(f.Canvas, hipX, hipY, 5.5f, 5.5f, ArtKit.Iron, bias + 0.1f);

            // A canela: um pistao, cilindro largo em cima e haste brilhante embaixo.
            (float mx, float my) = (kx + (footX - kx) * 0.55f, ky + (footY - ky) * 0.55f);
            ArtKit.Limb(f.Canvas, kx, ky, mx, my, 4.4f, ArtKit.Corroded(ArtKit.Rust, 0.3f, back ? 63 : 64, 0.3f), bias);
            ArtKit.Limb(f.Canvas, mx, my, footX, footY, 2.6f, ArtKit.Tin, bias + 0.2f);
            ArtKit.Ball(f.Canvas, kx, ky, 4.6f, 4.6f, ArtKit.Brass, bias);
            ArtKit.Rivet(f.Canvas, (int)kx - 1, (int)ky - 1, ArtKit.Iron);

            // O pe: uma sapata pesada, com dois dedos de ferro na frente.
            float shoeX = footX + 3f;
            float shoeY = footY + 0.5f;
            ArtKit.Drum(f.Canvas, shoeX, shoeY, 9f, 3.4f, 3f, ArtKit.Plain(ArtKit.Iron), bias + 0.15f);
            f.Canvas.Fill((int)(shoeX - 8f), (int)(shoeY + 2f), 17, 1, ArtKit.Iron[0]);
            ArtKit.Taper(f.Canvas, shoeX + 7f, shoeY, shoeX + 12f, shoeY + 2.5f, 1.6f, 0.6f, ArtKit.Tin, bias);
            ArtKit.Taper(f.Canvas, shoeX + 5f, shoeY + 1f, shoeX + 9f, shoeY + 3f, 1.4f, 0.6f, ArtKit.Tin, bias - 0.2f);
        }

        // ------------------------------------------------------------------
        // A cabeca
        // ------------------------------------------------------------------

        private static void DrawHead(Frame f)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;
            float hx = f.HeadX;
            float hy = f.HeadY;

            // O colar de latao onde a cabine gira, preso na beirada da cacamba.
            ArtKit.Limb(c, f.TX + 12f, f.Top + 6f, hx - 4f, hy + 7f, 3.4f, ArtKit.Rubber, 0.1f);
            ArtKit.Drum(c, hx - 3f, hy + 8f, 6.5f, 2.6f, 3f, ArtKit.Plain(ArtKit.Brass), 0.15f);

            // A pa dentada embaixo da cabine: abre pra baixo.
            float jawAngle = 10f + pose.Jaw * 45f;
            float pivotX = hx - 3f;
            float pivotY = hy + 6f;
            (float jx, float jy) = ArtKit.Along(pivotX, pivotY, jawAngle, 15f);
            (float jbx, float jby) = ArtKit.Along(pivotX, pivotY, jawAngle + 28f, 9f);
            var jaw = new[] { (pivotX, pivotY - 1f), (jx, jy - 1f), (jx, jy + 2f), (jbx, jby + 1f), (pivotX - 1f, pivotY + 3f) };
            ArtKit.Polygon(c, jaw, (x, y) =>
            {
                float t = (x - pivotX) / 15f;
                return ArtKit.Tone(ArtKit.Iron, 0.4f - t * 0.6f - (y - pivotY) * 0.08f);
            });

            // Os dentes da pa.
            for (int i = 0; i < 4; i++)
            {
                float k = 0.35f + i * 0.2f;
                float tx = pivotX + (jx - pivotX) * k;
                float ty = pivotY + (jy - pivotY) * k - 1.5f;
                (float ux, float uy) = ArtKit.Along(tx, ty, jawAngle - 90f, 2.5f);
                ArtKit.Taper(c, tx, ty, ux, uy, 1.1f, 0.4f, ArtKit.Tin, 0.3f);
            }

            // A cabine: uma cupula de lampiao, ferro com ferrugem.
            ArtKit.Ball(c, hx, hy, 11f, 10f, ArtKit.Corroded(ArtKit.Iron, 0.18f, 71, 0.25f, ArtKit.Rust), 0.1f);

            // As faixas verticais da cupula, como gomos.
            for (int i = -1; i <= 1; i++)
            {
                for (int py = (int)(hy - 9f); py <= (int)(hy + 9f); py++)
                {
                    float dy = (py + 0.5f - hy) / 10f;
                    float half = 11f * (float)Math.Sqrt(Math.Max(0f, 1f - dy * dy));
                    int px = (int)(hx + i * half * 0.55f - 2f);
                    if (c.Get(px, py).A == 0 || Math.Abs(dy) > 0.9f) continue;
                    c.Set(px, py, ArtKit.Shift(ArtKit.Iron, c.Get(px, py), -1));
                }
            }

            // A aba do visor, por cima do olho.
            ArtKit.Taper(c, hx - 4f, hy - 7f, hx + 12f, hy - 4f, 2.2f, 1.2f, ArtKit.Rust, 0.4f);

            // O apito de latao no alto.
            ArtKit.Limb(c, hx - 4f, hy - 9f, hx - 4f, hy - 15f, 1.4f, ArtKit.Brass, 0.2f);
            ArtKit.Drum(c, hx - 4f, hy - 16f, 2.6f, 1.6f, 3f, ArtKit.Plain(ArtKit.Brass), 0.3f);

            // O olho: uma vigia de latao com o vidro aceso pelo Oxide.
            float ex = hx + 5f;
            float ey = hy + 1f;
            ArtKit.Ball(c, ex, ey, 6f, 6f, ArtKit.Brass, 0.15f);
            for (int i = 0; i < 6; i++)
            {
                (float rx, float ry) = ArtKit.Along(ex, ey, i * 60f + 30f, 5f);
                c.Set((int)rx, (int)ry, ArtKit.Brass[0]);
            }

            float lens = pose.Squint ? 3.2f : 4.2f;
            Rgba[] glass = pose.Eye <= 0.05f
                ? new[] { new Rgba(20, 30, 28), new Rgba(30, 44, 40), new Rgba(40, 60, 54), new Rgba(70, 90, 84) }
                : pose.Eye < 0.8f
                    ? new[] { new Rgba(20, 60, 44), new Rgba(30, 100, 70), new Rgba(50, 140, 96), new Rgba(90, 190, 140) }
                    : new[] { ArtKit.OxideDeep, new Rgba(80, 210, 130), ArtKit.OxideGlow, ArtKit.OxideCore };
            ArtKit.Ball(c, ex, ey, 4.2f, lens, glass, pose.Eye > 1.5f ? 0.45f : 0.15f);

            // A pupila: um risco escuro, que vira fenda quando aperta.
            if (pose.Eye > 0.05f)
            {
                c.Fill((int)ex, (int)(ey - lens + 1.5f), 1, (int)(lens * 2f - 2f), ArtKit.OxideDeep);
                c.Set((int)ex - 2, (int)(ey - 2f), ArtKit.OxideCore);
            }

            // Reflexo no vidro.
            c.Set((int)(ex - 2f), (int)(ey - 3f), new Rgba(240, 255, 245));

            if (f.Cracked)
            {
                ArtKit.Crack(c, hx - 7f, hy - 6f, 60f, 9, 17, 1f);
                c.Set((int)(ex + 2f), (int)(ey - 2f), ArtKit.Outline);
                c.Set((int)(ex + 3f), (int)(ey - 1f), ArtKit.Outline);
            }
        }

        // ------------------------------------------------------------------
        // Os bracos
        // ------------------------------------------------------------------

        private static void DrawBackArm(Frame f, float sx, float sy, float ex, float ey)
        {
            Pose pose = f.Pose;
            const float bias = -0.4f;

            ArtKit.Limb(f.Canvas, sx, sy, ex, ey, 3.6f, ArtKit.Corroded(ArtKit.Iron, 0.2f, 81, 0.3f, ArtKit.Rust), bias);
            ArtKit.Limb(f.Canvas, ex, ey, f.BackHandX, f.BackHandY, 2.8f, ArtKit.Rust, bias);
            ArtKit.Ball(f.Canvas, ex, ey, 3.6f, 3.6f, ArtKit.Brass, bias);

            // A mao de dois dedos, e o bolo de sucata quando esta segurando.
            (float ax, float ay) = ArtKit.Along(f.BackHandX, f.BackHandY, pose.BackFore - 30f, 5f);
            (float bx, float by) = ArtKit.Along(f.BackHandX, f.BackHandY, pose.BackFore + 30f, 5f);
            ArtKit.Taper(f.Canvas, f.BackHandX, f.BackHandY, ax, ay, 1.8f, 0.8f, ArtKit.Iron, bias);
            ArtKit.Taper(f.Canvas, f.BackHandX, f.BackHandY, bx, by, 1.8f, 0.8f, ArtKit.Iron, bias);
            ArtKit.Ball(f.Canvas, f.BackHandX, f.BackHandY, 2.8f, 2.8f, ArtKit.Iron, bias + 0.1f);

            if (pose.Holding)
            {
                (float cx, float cy) = ArtKit.Along(f.BackHandX, f.BackHandY, pose.BackFore, 5f);
                ArtKit.Ball(f.Canvas, cx, cy, 5f, 4.4f, ArtKit.Corroded(ArtKit.Rust, 0.35f, 91, 0.5f), 0.2f);
                f.Canvas.Set((int)cx + 2, (int)cy - 2, ArtKit.Tin[3]);
                f.Canvas.Set((int)cx - 2, (int)cy + 1, ArtKit.Iron[0]);
            }
        }

        /// <summary>O braco da garra: ombro, braco, antebraco hidraulico e a garra de tres dentes.</summary>
        private static void DrawArm(Frame f, float ex, float ey)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;

            // O dente de tras da garra, por tras de tudo.
            DrawProng(f, pose.Fore, 0f, -0.45f);

            ArtKit.Taper(c, f.ShoulderX, f.ShoulderY, ex, ey, 5.2f, 4f, ArtKit.Corroded(ArtKit.Iron, 0.15f, 101, 0.28f, ArtKit.Rust), 0.15f);

            // O antebraco: o cilindro hidraulico e a haste brilhante saindo dele.
            (float mx, float my) = (ex + (f.WristX - ex) * 0.6f, ey + (f.WristY - ey) * 0.6f);
            ArtKit.Limb(c, ex, ey, mx, my, 3.8f, ArtKit.Iron, 0.1f);
            ArtKit.Limb(c, mx, my, f.WristX, f.WristY, 2.2f, ArtKit.Tin, 0.3f);
            ArtKit.Drum(c, mx, my, 2.5f, 2.5f, 2f, ArtKit.Plain(ArtKit.Brass), 0.1f);

            ArtKit.Ball(c, ex, ey, 4.6f, 4.6f, ArtKit.Brass, 0.1f);
            ArtKit.Rivet(c, (int)ex - 1, (int)ey - 1, ArtKit.Iron);

            // A ombreira: uma calota de chapa rebitada.
            ArtKit.Ball(c, f.ShoulderX, f.ShoulderY - 1f, 8f, 7f, ArtKit.Corroded(ArtKit.Rust, 0.2f, 102, 0.3f), 0.3f);
            for (int i = 0; i < 5; i++)
            {
                (float rx, float ry) = ArtKit.Along(f.ShoulderX, f.ShoulderY - 1f, 160f + i * 40f, 6f);
                ArtKit.Rivet(c, (int)rx, (int)ry, ArtKit.Iron);
            }
            ArtKit.Rivet(c, (int)f.ShoulderX - 2, (int)f.ShoulderY - 3, ArtKit.Brass);
            ArtKit.Rivet(c, (int)f.ShoulderX + 2, (int)f.ShoulderY + 1, ArtKit.Brass);

            // O cubo da garra e os dois dentes da frente.
            ArtKit.Ball(c, f.WristX, f.WristY, 5f, 5f, ArtKit.Brass, 0.1f);
            DrawProng(f, pose.Fore, -1f, 0.1f);
            DrawProng(f, pose.Fore, 1f, 0f);

            if (f.Cracked) ArtKit.Crack(c, f.ShoulderX - 3f, f.ShoulderY - 3f, pose.Arm, 8, 21, 0.9f);
        }

        /// <summary>Um dente da garra: sai do cubo abrindo e a ponta curva pra dentro.</summary>
        private static void DrawProng(Frame f, float direction, float side, float bias)
        {
            float spread = 14f + f.Pose.Open * 34f;
            float angle = direction + side * spread;
            if (side == 0f) angle = direction + 4f;

            (float mx, float my) = ArtKit.Along(f.WristX, f.WristY, angle, 10f);
            float hook = side == 0f ? 0f : -side * (45f + f.Pose.Open * 25f);
            (float tx, float ty) = ArtKit.Along(mx, my, angle + hook, 9f);

            ArtKit.Taper(f.Canvas, f.WristX, f.WristY, mx, my, 3.2f, 2.4f, side == 0f ? ArtKit.Iron : ArtKit.Tin, bias);
            ArtKit.Taper(f.Canvas, mx, my, tx, ty, 2.4f, 0.6f, side == 0f ? ArtKit.Iron : ArtKit.Tin, bias + 0.15f);
            ArtKit.Ball(f.Canvas, mx, my, 2.4f, 2.4f, ArtKit.Iron, bias);
        }

        /// <summary>Na queda, o remendo de cobre voando (ou caido no chao).</summary>
        private static void DrawFlyingPlate(Frame f)
        {
            float x = f.TX - 15f + f.Pose.PlateFlyX;
            float y = f.TY - 7f + f.Pose.PlateFlyY;
            y = Math.Min(y, GroundY - 3f);

            var corners = new[] { (x, y), (x + 10f, y - 2f), (x + 11f, y + 4f), (x + 1f, y + 6f) };
            ArtKit.Polygon(f.Canvas, corners, (px, py) => ArtKit.Tone(ArtKit.Copper, 0.5f - (py - y) * 0.15f));
            f.Canvas.Set((int)x + 2, (int)y + 1, ArtKit.Iron[3]);
            f.Canvas.Set((int)x + 8, (int)y, ArtKit.Iron[3]);
        }

        // ------------------------------------------------------------------
        // O que vem por cima: brilhos, rastro, fumaca, vapor, poeira
        // ------------------------------------------------------------------

        private static void DrawGlows(Frame f)
        {
            Pose pose = f.Pose;

            if (pose.Eye >= 1f)
            {
                float strength = (f.Cracked ? 0.45f : 0.3f) * (pose.Eye - 0.5f);
                ArtKit.Glow(f.Canvas, f.HeadX + 5f, f.HeadY + 1f, 9f + (f.Cracked ? 3f : 0f), ArtKit.OxideGlow, strength, true);
            }

            if (pose.Furnace > 0.5f)
            {
                ArtKit.Glow(f.Canvas, f.TX + 6f, f.TY + 6f, 11f, ArtKit.OxideGlow, 0.35f * pose.Furnace, f.Cracked);
            }

            if (f.Cracked) ArtKit.Glow(f.Canvas, f.TX + 12f, f.Top + 14f, 9f, ArtKit.OxideGlow, 0.3f);
        }

        private static void DrawSmear(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.SmearFrom == pose.SmearTo) return;

            float from = Math.Min(pose.SmearFrom, pose.SmearTo);
            float to = Math.Max(pose.SmearFrom, pose.SmearTo);

            for (int py = 0; py < Height; py++)
            {
                for (int px = 0; px < Width; px++)
                {
                    float dx = px + 0.5f - f.ShoulderX;
                    float dy = py + 0.5f - f.ShoulderY;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r < 26f || r > 44f) continue;

                    float a = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    if (a < from || a > to) continue;

                    float k = (a - from) / Math.Max(1f, to - from);
                    float edge = (r - 26f) / 18f;
                    int alpha = (int)(30 + 150 * k * edge);
                    if (alpha < 35) continue;

                    f.Canvas.Blend(px, py, new Rgba(220, 255, 230).WithAlpha(alpha));
                }
            }
        }

        private static void DrawSmoke(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.Puffs <= 0) return;

            float baseX = f.TX - 30f;
            float baseY = f.Top - 24f;
            if (pose.ChimneyBent)
            {
                baseX = f.TX - 43f;
                baseY = f.Top - 7f;
            }

            Rgba light = f.Cracked ? new Rgba(170, 220, 180) : new Rgba(170, 166, 156);
            Rgba dark = f.Cracked ? new Rgba(90, 140, 110) : new Rgba(96, 94, 90);

            for (int i = 0; i < pose.Puffs; i++)
            {
                // Cada baforada sobe e cresce; a de cima ja esta sumindo.
                float t = (pose.Smoke + i / (float)Math.Max(1, pose.Puffs)) % 1f;
                float x = baseX - t * 8f + (float)Math.Sin((t + i) * 6f) * 1.5f;
                float y = baseY - t * 20f;
                float r = 2.5f + t * 4.5f;
                int alpha = (int)(170 * (1f - t * 0.8f));

                if (y - r < 0f) continue;
                ArtKit.Puff(f.Canvas, x, y, r, light, dark, alpha);
            }
        }

        private static void DrawSteam(Frame f)
        {
            float steam = f.Pose.Steam;
            if (steam <= 0f) return;

            float x = f.HeadX - 4f;
            float y = f.HeadY - 19f;
            var white = new Rgba(240, 244, 240);
            var grey = new Rgba(190, 200, 196);

            ArtKit.Puff(f.Canvas, x, y, 2.5f + steam * 1.5f, white, grey, (int)(200 * steam));
            ArtKit.Puff(f.Canvas, x - 2f, y - 5f * steam, 2f + steam * 2.5f, white, grey, (int)(140 * steam));
            if (steam > 0.7f) ArtKit.Puff(f.Canvas, x + 2f, y - 10f, 3.5f, white, grey, 90);
        }

        private static void DrawDust(Frame f)
        {
            float dust = f.Pose.Dust;
            if (dust <= 0f) return;

            var light = new Rgba(196, 170, 130);
            var dark = new Rgba(130, 104, 80);
            float spread = 10f + dust * 12f;
            int alpha = dust > 1.5f ? 110 : 200;

            foreach (float side in new[] { -1f, 1f })
            {
                for (int i = 0; i < 3; i++)
                {
                    float x = TorsoX + f.Pose.X + side * (spread + i * 7f);
                    float y = GroundY - 3f - i * 2f - dust * 2f;
                    ArtKit.Puff(f.Canvas, x, y, 3.5f + i + dust, light, dark, alpha - i * 30);
                }
            }
        }

        /// <summary>O verde tomando o corpo antes do estouro: tudo clareia pro Oxide.</summary>
        private static void DrawBurst(Frame f)
        {
            float burst = f.Pose.Burst;
            PixelCanvas c = f.Canvas;

            for (int i = 0; i < c.Pixels.Length; i++)
            {
                Rgba p = c.Pixels[i];
                if (p.A < 200) continue;

                int x = i % c.Width;
                int y = i / c.Width;
                float wobble = ArtKit.Noise(x * 0.2f, y * 0.2f, 99);
                float k = burst * (0.6f + 0.6f * wobble);
                Rgba target = k > 0.75f ? ArtKit.OxideCore : ArtKit.OxideGlow;
                c.Pixels[i] = Rgba.Lerp(p, target, Math.Min(1f, k));
            }

            ArtKit.Glow(c, f.TX, f.TY, 40f, ArtKit.OxideGlow, burst * 0.6f, true);
        }
    }
}
