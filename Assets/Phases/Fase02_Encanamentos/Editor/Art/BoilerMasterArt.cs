using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Mestre das Caldeiras (The Boiler Master), no mesmo detalhe do Scrap:
    /// o chefe de manutencao do District 2, fundido a caldeira que manteve
    /// acesa pra os operarios nao congelarem. O corpo e a caldeira de latao
    /// rebitada; a cabeca, o capacete de soldador dele, com o visor verde de
    /// Oxide; no peito, o manometro (o ponteiro sobe com a luta); nas costas,
    /// a fornalha (aberta, e o ponto fraco) e duas chamines; na mao, o
    /// martelo de chave inglesa; no outro braco, o bico de vapor.
    ///
    /// Cada quadro tem 160x128, olha pra direita e e importado com 32 pixels
    /// por tile (5 x 4 tiles). O pivo fica na sola, embaixo do corpo (ver PivotX).
    ///
    /// Dois jogos dos mesmos quadros: o normal e o de sobrepressao (da metade
    /// da vida pra baixo), com as costuras em brasa, vapor escapando das juntas
    /// e o ponteiro no vermelho.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class BoilerMasterArt
    {
        public const int Width = 160;
        public const int Height = 128;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 68f / Width;
        public const float PivotY = 1f / Height;

        public const int Dormant = 0;
        public const int DormantFrames = 4;
        public const int Wake = 4;
        public const int WakeFrames = 4;
        public const int Idle = 8;
        public const int IdleFrames = 6;
        public const int Walk = 14;
        public const int WalkFrames = 8;
        public const int HammerWindup = 22;
        public const int HammerWindupFrames = 2;
        public const int HammerStrike = 24;
        public const int HammerStrikeFrames = 3;
        public const int ChargeWindup = 27;
        public const int ChargeWindupFrames = 2;
        public const int Charge = 29;
        public const int ChargeFrames = 4;
        public const int BurstWindup = 33;
        public const int BurstWindupFrames = 2;
        public const int Burst = 35;
        public const int BurstFrames = 3;
        public const int Geyser = 38;
        public const int GeyserFrames = 3;
        public const int LeapCrouch = 41;
        public const int LeapCrouchFrames = 2;
        public const int LeapRise = 43;
        public const int LeapAir = 44;
        public const int LeapFall = 45;
        public const int LeapImpact = 46;
        public const int LeapImpactFrames = 2;
        public const int Exposed = 48;
        public const int ExposedFrames = 2;
        public const int Hurt = 50;
        public const int Roar = 51;
        public const int RoarFrames = 4;
        public const int Death = 55;
        public const int DeathFrames = 8;
        public const int FrameCount = 63;

        private const float Ground = 125f;
        private const float BodyX = 66f;
        private const float BodyY = 72f;
        private const float BodyRX = 27f;
        private const float BodyRY = 31f;

        private sealed class Pose
        {
            public float X;
            public float Y;
            public float Lean;
            public float Squash = 1f;

            public float BackFoot;
            public float FrontFoot;
            public float BackLift;
            public float FrontLift;
            public bool Kneel;

            /// <summary>O braco do martelo: braco, antebraco e o angulo do cabo (0 = pra frente, 90 = pra baixo).</summary>
            public float Arm = 70f;
            public float Fore = 20f;
            public float Hammer = 60f;

            /// <summary>O braco do bico de vapor.</summary>
            public float Back = 100f;
            public float BackFore = 70f;
            public bool Nozzle;

            public float HeadY;
            public float Visor = 1f;

            /// <summary>Pressao no manometro, de 0 a 1 (1 e o fim do vermelho).</summary>
            public float Pressure = 0.3f;

            /// <summary>A porta da fornalha: 0 fechada, 1 escancarada.</summary>
            public float Door;
            public float Fire = 0.5f;

            public float Smoke;
            public int Puffs = 2;

            public float SmearFrom;
            public float SmearTo;
            public float Dust;
            public bool Charging;
            public List<(float X, float Y, float R, int Alpha)> Steam = new List<(float, float, float, int)>();
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();

            public bool GaugeCracked;
            public bool PlateOff;
            public float Burst;
        }

        public static PixelCanvas[] Frames(bool overpressure)
        {
            var frames = new PixelCanvas[FrameCount];

            // Dormindo: arriado, o visor apagado, o martelo largado no chao.
            for (int i = 0; i < DormantFrames; i++)
            {
                frames[Dormant + i] = Draw(overpressure, new Pose
                {
                    Y = 6f + (i == 1 || i == 2 ? 0.5f : 0f), Lean = 3f, HeadY = 6f, Visor = i == 2 ? 0.3f : 0f,
                    Arm = 85f, Fore = 80f, Hammer = 5f, Back = 110f, BackFore = 95f, Pressure = 0.05f, Fire = 0.15f, Puffs = 0,
                });
            }

            // Acordando: o visor acende, ele se ergue, o ponteiro pula, as chamines tossem.
            float[] wakeVisor = { 0.5f, 0f, 1.3f, 1.9f };
            for (int i = 0; i < WakeFrames; i++)
            {
                float k = (i + 1) / (float)WakeFrames;
                var pose = new Pose
                {
                    Y = 6f * (1f - k), Lean = 3f * (1f - k), HeadY = 6f * (1f - k) - (i == 3 ? 2f : 0f), Visor = wakeVisor[i],
                    Arm = Lerp(85f, 70f, k), Fore = Lerp(80f, 20f, k), Hammer = Lerp(5f, 60f, k),
                    Pressure = Lerp(0.05f, 0.45f, k), Fire = Lerp(0.15f, 0.7f, k), Puffs = i >= 2 ? 2 : 0, Smoke = i * 0.25f,
                };
                if (i == 3) pose.Steam.Add((60f, 42f, 5f, 180));
                frames[Wake + i] = Draw(overpressure, pose);
            }

            float[] breath = { 0f, 0.5f, 1f, 1f, 0.5f, 0f };
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(overpressure, new Pose
                {
                    Y = breath[i], Squash = 1f + breath[i] * 0.012f, HeadY = breath[i] * 0.5f,
                    Pressure = 0.32f + (i % 2) * 0.03f, Fire = 0.5f + 0.2f * (float)Math.Sin(i * Math.PI / 3.0), Smoke = i / 6f,
                    Hammer = 60f + breath[i] * 2f,
                });
            }

            for (int i = 0; i < WalkFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / WalkFrames;
                float swing = (float)Math.Sin(phase);
                float bob = (float)Math.Abs(Math.Cos(phase));
                frames[Walk + i] = Draw(overpressure, new Pose
                {
                    Y = (1f - bob) * 3f, Lean = 1.5f, HeadY = (1f - bob),
                    BackFoot = -swing * 9f, FrontFoot = swing * 9f,
                    BackLift = swing > 0.2f ? swing * 6f : 0f, FrontLift = swing < -0.2f ? -swing * 6f : 0f,
                    Arm = 70f + swing * 6f, Hammer = 55f + swing * 8f, Back = 100f - swing * 10f,
                    Pressure = 0.35f, Fire = 0.6f, Smoke = i / 8f,
                });
            }

            // O martelo: ergue por cima da cabeca...
            frames[HammerWindup + 0] = Draw(overpressure, new Pose
            {
                X = -2f, Lean = -2f, Arm = -60f, Fore = -100f, Hammer = -110f, BackFoot = -4f, FrontFoot = 4f, Visor = 1.5f, Pressure = 0.5f, Fire = 0.7f,
            });
            frames[HammerWindup + 1] = Draw(overpressure, new Pose
            {
                X = -3f, Y = 1f, Lean = -4f, HeadY = -1f, Arm = -100f, Fore = -140f, Hammer = -150f, BackFoot = -6f, FrontFoot = 5f, Visor = 2f, Pressure = 0.6f, Fire = 0.8f,
            });

            // ...e desce com tudo na frente: rastro, poeira, faisca.
            frames[HammerStrike + 0] = Draw(overpressure, new Pose
            {
                X = 3f, Lean = 3f, Arm = -20f, Fore = 0f, Hammer = -5f, SmearFrom = -150f, SmearTo = -5f, BackFoot = -7f, FrontFoot = 7f, Visor = 2f, Pressure = 0.55f,
            });
            Pose smash = new Pose
            {
                X = 5f, Y = 3f, Lean = 6f, HeadY = 1f, Arm = 40f, Fore = 60f, Hammer = 82f, SmearFrom = -40f, SmearTo = 82f,
                BackFoot = -8f, FrontFoot = 8f, Visor = 2f, Dust = 1f, Pressure = 0.5f,
            };
            smash.Sparks.Add((140, 120, 3));
            smash.Sparks.Add((130, 116, 2));
            frames[HammerStrike + 1] = Draw(overpressure, smash);
            frames[HammerStrike + 2] = Draw(overpressure, new Pose
            {
                X = 4f, Y = 2f, Lean = 5f, Arm = 45f, Fore = 65f, Hammer = 85f, BackFoot = -7f, FrontFoot = 7f, Visor = 1.5f, Dust = 2f, Pressure = 0.45f,
            });

            // A investida: agacha, solta vapor pelas costas e dispara, o martelo arrastando.
            Pose crouch = new Pose
            {
                X = -2f, Y = 4f, Squash = 0.95f, Lean = 5f, Arm = 110f, Fore = 160f, Hammer = 172f, BackFoot = -10f, FrontFoot = 5f, Visor = 1.6f, Pressure = 0.6f, Fire = 0.9f,
            };
            crouch.Steam.Add((34f, 70f, 5f, 170));
            frames[ChargeWindup + 0] = Draw(overpressure, crouch);
            Pose coil = new Pose
            {
                X = -4f, Y = 6f, Squash = 0.92f, Lean = 7f, Arm = 115f, Fore = 165f, Hammer = 175f, BackFoot = -12f, FrontFoot = 6f, Visor = 2f, Pressure = 0.7f, Fire = 1f,
            };
            coil.Steam.Add((32f, 66f, 6f, 200));
            coil.Steam.Add((28f, 80f, 4f, 160));
            frames[ChargeWindup + 1] = Draw(overpressure, coil);

            for (int i = 0; i < ChargeFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / ChargeFrames;
                float swing = (float)Math.Sin(phase);
                Pose run = new Pose
                {
                    X = 3f, Y = 2f, Lean = 10f, HeadY = 2f, Arm = 120f, Fore = 170f, Hammer = 178f,
                    BackFoot = -swing * 12f, FrontFoot = swing * 12f, BackLift = swing > 0f ? swing * 7f : 0f, FrontLift = swing < 0f ? -swing * 7f : 0f,
                    Visor = 2f, Pressure = 0.7f, Fire = 1f, Charging = true, Smoke = i / 4f, Puffs = 3,
                };
                run.Steam.Add((26f - i * 2f, 68f + (i % 2) * 6f, 6f + i, 180 - i * 20));
                run.Steam.Add((16f, 82f, 5f, 120));
                frames[Charge + i] = Draw(overpressure, run);
            }

            // A rajada: o bico de vapor apontado pra frente.
            frames[BurstWindup + 0] = Draw(overpressure, new Pose
            {
                Lean = -1f, Back = -20f, BackFore = -10f, Nozzle = true, Arm = 50f, Hammer = 40f, Visor = 1.5f, Pressure = 0.55f, Fire = 0.8f,
            });
            Pose aim = new Pose { Lean = -2f, Back = 0f, BackFore = 5f, Nozzle = true, Arm = 45f, Hammer = 35f, Visor = 2f, Pressure = 0.7f, Fire = 0.9f };
            aim.Steam.Add((112f, 66f, 2.5f, 160));
            frames[BurstWindup + 1] = Draw(overpressure, aim);
            for (int i = 0; i < BurstFrames; i++)
            {
                Pose fire = new Pose
                {
                    X = -i % 2, Lean = -2f, Back = 5f - i * 5f, BackFore = 5f, Nozzle = true, Arm = 45f, Hammer = 35f, Visor = 2f, Pressure = 0.6f - i * 0.1f, Fire = 0.9f,
                };
                fire.Steam.Add((118f + i * 4f, 64f - i * 2f, 6f - i, 220 - i * 40));
                fire.Steam.Add((106f, 60f, 3f, 150));
                frames[Burst + i] = Draw(overpressure, fire);
            }

            // Os geiseres: pisa forte e abre as valvulas.
            Pose lift = new Pose { FrontLift = 10f, FrontFoot = 4f, Lean = -2f, Arm = -20f, Fore = -40f, Hammer = -60f, Back = -40f, BackFore = -60f, Visor = 1.6f, Pressure = 0.6f };
            frames[Geyser + 0] = Draw(overpressure, lift);
            Pose stomp = new Pose { Y = 3f, Squash = 0.95f, FrontFoot = 6f, Lean = 2f, Arm = -10f, Fore = -30f, Hammer = -50f, Back = -30f, BackFore = -50f, Visor = 2f, Dust = 1f, Pressure = 0.7f };
            stomp.Steam.Add((50f, 46f, 5f, 190));
            stomp.Steam.Add((96f, 46f, 5f, 190));
            frames[Geyser + 1] = Draw(overpressure, stomp);
            Pose vent = new Pose { Y = 1f, FrontFoot = 6f, Arm = 0f, Fore = -20f, Hammer = -40f, Back = -20f, BackFore = -40f, Visor = 1.8f, Dust = 2f, Pressure = 0.6f, Puffs = 3 };
            vent.Steam.Add((44f, 36f, 6f, 170));
            vent.Steam.Add((100f, 40f, 6f, 170));
            frames[Geyser + 2] = Draw(overpressure, vent);

            // O salto da fase 2: agacha, sobe esticado, paira com o martelo no alto, despenca.
            frames[LeapCrouch + 0] = Draw(overpressure, new Pose
            {
                Y = 5f, Squash = 0.93f, Arm = -70f, Fore = -110f, Hammer = -120f, BackFoot = -4f, FrontFoot = 4f, Visor = 1.6f, Pressure = 0.7f,
            });
            frames[LeapCrouch + 1] = Draw(overpressure, new Pose
            {
                Y = 9f, Squash = 0.86f, Arm = -95f, Fore = -135f, Hammer = -145f, BackFoot = -6f, FrontFoot = 6f, Visor = 2f, Pressure = 0.8f, Fire = 1f,
            });
            Pose rise = new Pose { Y = -2f, Squash = 1.06f, Arm = -80f, Fore = -110f, Hammer = -115f, BackLift = 9f, FrontLift = 8f, Visor = 1.8f, Pressure = 0.8f, Puffs = 3 };
            rise.Steam.Add((60f, 118f, 7f, 180));
            frames[LeapRise] = Draw(overpressure, rise);
            frames[LeapAir] = Draw(overpressure, new Pose
            {
                Y = -2f, Lean = 2f, Arm = -100f, Fore = -150f, Hammer = -160f, BackLift = 8f, FrontLift = 10f, BackFoot = -4f, Visor = 2f, Pressure = 0.85f, Puffs = 3,
            });
            frames[LeapFall] = Draw(overpressure, new Pose
            {
                Y = -1f, Squash = 1.06f, Lean = 4f, Arm = 20f, Fore = 50f, Hammer = 70f, SmearFrom = -150f, SmearTo = 70f, BackLift = 3f, FrontLift = 2f, Visor = 2f, Pressure = 0.8f,
            });
            Pose land = new Pose
            {
                Y = 9f, Squash = 0.85f, Lean = 5f, HeadY = 2f, Arm = 45f, Fore = 70f, Hammer = 88f, BackFoot = -7f, FrontFoot = 7f, Visor = 2f, Dust = 1f, Pressure = 0.7f,
            };
            land.Sparks.Add((138, 120, 3));
            frames[LeapImpact + 0] = Draw(overpressure, land);
            frames[LeapImpact + 1] = Draw(overpressure, new Pose
            {
                Y = 4f, Squash = 0.93f, Lean = 3f, HeadY = 1f, Arm = 50f, Fore = 70f, Hammer = 86f, BackFoot = -6f, FrontFoot = 6f, Visor = 1.4f, Dust = 2f, Pressure = 0.6f,
            });

            // Exposto: arfando, a porta da fornalha escancarada, o fogo vivo la dentro.
            for (int i = 0; i < ExposedFrames; i++)
            {
                Pose open = new Pose
                {
                    Y = 2f + i, Lean = 4f, HeadY = 3f, Arm = 75f, Fore = 60f, Hammer = 80f, Door = 1f, Fire = i == 0 ? 1f : 0.85f,
                    Visor = 0.8f, Pressure = 0.25f, Puffs = 1,
                };
                open.Steam.Add((96f, 30f - i * 3f, 3f + i, 140));
                frames[Exposed + i] = Draw(overpressure, open);
            }

            Pose hurt = new Pose { X = -3f, Lean = -4f, HeadY = -2f, Arm = 85f, Fore = 50f, Hammer = 60f, FrontLift = 3f, Visor = 0.6f, Pressure = 0.7f };
            hurt.Sparks.Add((70, 56, 3));
            hurt.Sparks.Add((82, 70, 2));
            hurt.Steam.Add((74f, 50f, 4f, 170));
            frames[Hurt] = Draw(overpressure, hurt);

            // A sobrepressao: cabeca pra tras, o ponteiro no fim, vapor por todas as juntas.
            for (int i = 0; i < RoarFrames; i++)
            {
                Pose roar = new Pose
                {
                    X = -1f, Lean = -3f, HeadY = -4f - (i == 1 || i == 2 ? 1f : 0f), Arm = -30f, Fore = -50f, Hammer = -70f,
                    Back = -50f, BackFore = -70f, Visor = 2f, Pressure = 0.85f + i * 0.05f, Fire = 1f, Puffs = 3, Smoke = i * 0.25f,
                };
                roar.Steam.Add((46f, 50f - i * 2f, 5f + i, 200));
                roar.Steam.Add((92f, 54f - i * 2f, 5f + i, 200));
                roar.Steam.Add((66f, 100f, 4f + i, 170));
                if (i >= 1) roar.Steam.Add((100f, 18f - i * 2f, 5f, 180));
                frames[Roar + i] = Draw(overpressure, roar);
            }

            DrawDeath(frames);
            return frames;
        }

        private static void DrawDeath(PixelCanvas[] frames)
        {
            Pose spin = new Pose { X = -3f, Lean = -4f, HeadY = -2f, Arm = 90f, Fore = 100f, Hammer = 90f, Visor = 2f, Pressure = 1f, Fire = 1f, GaugeCracked = true };
            spin.Sparks.Add((86, 60, 3));
            spin.Steam.Add((60f, 46f, 6f, 200));
            frames[Death + 0] = Draw(true, spin);

            Pose pop = new Pose { X = -2f, Y = 2f, Lean = -2f, Arm = 95f, Fore = 105f, Hammer = 92f, Visor = 0.6f, Pressure = 0f, Fire = 1f, GaugeCracked = true, PlateOff = true };
            pop.Steam.Add((44f, 60f, 7f, 210));
            pop.Steam.Add((90f, 82f, 5f, 180));
            pop.Sparks.Add((50, 60, 3));
            frames[Death + 1] = Draw(true, pop);

            Pose sag = new Pose { Y = 10f, Lean = 2f, HeadY = 3f, Arm = 95f, Fore = 110f, Hammer = 95f, Kneel = true, BackFoot = -12f, Visor = 1.6f, Fire = 1f, GaugeCracked = true, PlateOff = true, Puffs = 3 };
            sag.Steam.Add((50f, 50f, 7f, 200));
            frames[Death + 2] = Draw(true, sag);

            Pose kneel = new Pose { Y = 16f, Lean = 4f, HeadY = 8f, Arm = 100f, Fore = 110f, Hammer = 100f, Kneel = true, BackFoot = -14f, Visor = 0.3f, Fire = 0.8f, GaugeCracked = true, PlateOff = true };
            kneel.Steam.Add((56f, 60f, 6f, 170));
            frames[Death + 3] = Draw(true, kneel);

            float[] burst = { 0.25f, 0.45f, 0.68f, 0.9f };
            for (int i = 0; i < 4; i++)
            {
                frames[Death + 4 + i] = Draw(true, new Pose
                {
                    Y = 16f + i * 0.7f, Lean = 4f + i, HeadY = 8f + i * 0.5f, Arm = 100f, Fore = 110f, Hammer = 100f, Kneel = true, BackFoot = -14f,
                    Visor = i % 2 == 0 ? 2f : 0.5f, Fire = 1f, Door = 0.6f, GaugeCracked = true, PlateOff = true, Burst = burst[i],
                });
            }
        }

        private static float Lerp(float a, float b, float k) => a + (b - a) * k;

        // ------------------------------------------------------------------
        // Montagem
        // ------------------------------------------------------------------

        private sealed class Frame
        {
            public PixelCanvas Canvas = new PixelCanvas(Width, Height);
            public Pose Pose;
            public bool Hot;
            public float BX;
            public float BY;
            public float RX;
            public float RY;
            public float Top => BY - RY;
            public float HeadX;
            public float HeadY;
            public float ShoulderX;
            public float ShoulderY;
            public float HandX;
            public float HandY;
        }

        private static PixelCanvas Draw(bool hot, Pose pose)
        {
            var f = new Frame { Pose = pose, Hot = hot };
            f.RX = BodyRX / (float)Math.Sqrt(pose.Squash);
            f.RY = BodyRY * pose.Squash;
            f.BX = BodyX + pose.X + pose.Lean * 0.4f;
            f.BY = BodyY + pose.Y + (1f - pose.Squash) * 26f;
            f.HeadX = f.BX + 16f + pose.Lean;
            f.HeadY = f.Top - 11f + pose.HeadY + pose.Lean * 0.5f;
            f.ShoulderX = f.BX + 24f + pose.Lean * 0.6f;
            f.ShoulderY = f.BY - 10f + pose.Lean * 0.3f;

            (float ex, float ey) = ArtKit.Along(f.ShoulderX, f.ShoulderY, pose.Arm, 15f);
            (f.HandX, f.HandY) = ArtKit.Along(ex, ey, pose.Fore, 14f);

            DrawChimneys(f);
            DrawBackArm(f);
            DrawLeg(f, true);
            DrawBoiler(f);
            DrawLeg(f, false);
            DrawHead(f);
            DrawHammerArm(f, ex, ey);

            ArtKit.AddOutline(f.Canvas);

            DrawGlows(f);
            DrawSmear(f);
            DrawSmoke(f);
            foreach ((float X, float Y, float R, int Alpha) puff in pose.Steam) D2.Steam(f.Canvas, puff.X + pose.X, puff.Y + pose.Y, puff.R, puff.Alpha);
            if (hot) DrawLeaks(f);
            DrawDust(f);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(f.Canvas, spark.X, spark.Y, spark.Size);
            if (pose.Burst > 0f) DrawBurst(f);
            return f.Canvas;
        }

        private static void DrawChimneys(Frame f)
        {
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Iron, 0.2f, 701, 0.3f, ArtKit.Rust);
            foreach ((float ox, float height, float half) in new[] { (-14f, 30f, 3.6f), (-4f, 22f, 3f) })
            {
                float x = f.BX + ox;
                float bottom = f.Top + 8f;
                float top = f.Top - height;
                ArtKit.Limb(f.Canvas, x, bottom, x - 2f, top, half, iron, 0.1f);
                ArtKit.Drum(f.Canvas, x - 2f, top - 1f, half + 2.2f, 2f, 3f, ArtKit.Plain(ArtKit.Brass), 0.2f);
                ArtKit.Ball(f.Canvas, x - 1f, (bottom + top) * 0.5f, half + 0.8f, 1.6f, ArtKit.Brass, 0.1f);
            }
        }

        private static void DrawBoiler(Frame f)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;

            ArtKit.Skin brass = (x, y, light) =>
            {
                float n = ArtKit.Noise(x * 0.2f, y * 0.14f, 711);
                float drip = 0.15f + 0.35f * Math.Max(0f, (y + 10f) / 40f);
                if (n > 1f - drip * 0.5f) return ArtKit.Tone(ArtKit.Verdigris, light - 0.1f);
                if (ArtKit.Hash((int)Math.Floor(x), (int)Math.Floor(y), 712) % 19 == 0) return ArtKit.Tone(ArtKit.Brass, light - 0.4f);
                return ArtKit.Tone(ArtKit.Brass, light);
            };

            ArtKit.Drum(c, f.BX, f.BY, f.RX, f.RY, 2.6f, brass, -0.12f);

            // O domo de cima.
            ArtKit.Ball(c, f.BX - 2f, f.Top + 3f, f.RX * 0.75f, 6f, ArtKit.Corroded(ArtKit.Copper, 0.25f, 713, 0.3f), 0.25f);

            // As cintas rebitadas e a costura do meio.
            foreach (float v in new[] { -0.55f, 0.1f, 0.68f })
            {
                int y = (int)(f.BY + v * f.RY);
                float half = f.RX * (float)Math.Pow(1.0 - Math.Pow(Math.Abs(v), 2.6), 1.0 / 2.6);
                for (int x = (int)(f.BX - half) + 1; x <= (int)(f.BX + half) - 1; x++)
                {
                    float u = (x - f.BX) / f.RX;
                    float light = -0.75f * u + 0.2f;
                    c.Set(x, y - 1, ArtKit.Tone(ArtKit.Iron, light + 0.45f));
                    c.Set(x, y, ArtKit.Tone(ArtKit.Iron, light));
                    c.Set(x, y + 1, ArtKit.Tone(ArtKit.Iron, light - 0.5f));
                    if (x % 5 == 0) c.Set(x, y, ArtKit.Iron[3]);

                    if (f.Hot && x % 3 != 0) c.Set(x, y + 1, Rgba.Lerp(D2.Fire[1], D2.Fire[2], (x % 7) / 7f));
                }
            }

            // As valvulas de alivio no ombro.
            for (int i = 0; i < 2; i++)
            {
                float vx = f.BX + 4f + i * 9f;
                float vy = f.Top + 6f;
                ArtKit.Limb(c, vx, vy, vx, vy - 5f, 1.4f, ArtKit.Brass, 0.2f);
                ArtKit.Drum(c, vx, vy - 6f, 2.4f, 1.2f, 3f, ArtKit.Plain(D2.RedPaint), 0.2f);
            }

            DrawFurnace(f);
            DrawGauge(f);

            if (pose.PlateOff)
            {
                // Uma chapa arrancada: o buraco com o verde por dentro.
                int x0 = (int)(f.BX - 8f);
                int y0 = (int)(f.BY - 6f);
                for (int y = y0; y < y0 + 10; y++)
                {
                    for (int x = x0; x < x0 + 9; x++)
                    {
                        if (c.Get(x, y).A == 0) continue;
                        bool edge = x == x0 || y == y0 || x == x0 + 8 || y == y0 + 9;
                        c.Set(x, y, edge ? ArtKit.Brass[0] : ((x + y) % 3 == 0 ? ArtKit.OxideGlow : ArtKit.Iron[0]));
                    }
                }
            }

            if (f.Hot)
            {
                ArtKit.Crack(c, f.BX - 14f, f.BY - 20f, 70f, 12, 715, 0.9f);
                ArtKit.Crack(c, f.BX + 6f, f.BY + 14f, 100f, 9, 716, 0.9f);
            }
        }

        /// <summary>A fornalha nas costas: a porta de ferro com a grade; aberta, o fogo vivo — o ponto fraco.</summary>
        private static void DrawFurnace(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            float cx = f.BX - 14f;
            float cy = f.BY + 12f;
            const int w = 15;
            const int h = 14;
            int x0 = (int)(cx - w / 2f);
            int y0 = (int)(cy - h / 2f);

            // A boca: o fogo la dentro.
            for (int y = y0; y < y0 + h; y++)
            {
                for (int x = x0; x < x0 + w; x++)
                {
                    bool corner = (x == x0 || x == x0 + w - 1) && (y == y0 || y == y0 + h - 1);
                    if (corner) continue;

                    bool frame = x <= x0 + 1 || x >= x0 + w - 2 || y <= y0 + 1 || y >= y0 + h - 2;
                    if (frame)
                    {
                        c.Set(x, y, ArtKit.Tone(ArtKit.Iron, y <= y0 + 1 ? 0.5f : -0.4f));
                        continue;
                    }

                    float depth = (y - y0) / (float)h;
                    float heat = pose.Fire * (0.5f + depth * 0.7f) + (pose.Door > 0.5f ? 0.4f : 0f);
                    Rgba fire = heat > 1f ? D2.Fire[3] : (heat > 0.7f ? D2.Fire[2] : (heat > 0.4f ? D2.Fire[1] : (heat > 0.2f ? D2.Fire[0] : ArtKit.Iron[0])));
                    c.Set(x, y, fire);
                }
            }

            if (pose.Door < 0.5f)
            {
                // A porta fechada: chapa com grade, o fogo aparecendo pelas frestas.
                for (int y = y0 + 2; y < y0 + h - 2; y++)
                {
                    for (int x = x0 + 2; x < x0 + w - 2; x++)
                    {
                        bool slot = (x - x0) % 3 == 0;
                        if (!slot) c.Set(x, y, ArtKit.Tone(ArtKit.Iron, 0.1f - (y - y0) * 0.05f));
                    }
                }

                ArtKit.Rivet(c, x0 + 2, y0 + 2, ArtKit.Iron);
                ArtKit.Rivet(c, x0 + w - 4, y0 + h - 4, ArtKit.Iron);
            }
            else
            {
                // A porta escancarada pra tras, presa na dobradica.
                var door = new[] { (x0 + 0f, y0 + 0f), (x0 - 7f, y0 + 2f), (x0 - 7f, y0 + h - 1f), (x0 + 0f, y0 + h + 0f) };
                ArtKit.Polygon(c, door, (x, y) => ArtKit.Tone(ArtKit.Iron, -0.2f - (y - y0) * 0.03f));
            }
        }

        /// <summary>O manometro no peito: aro de latao, mostrador creme, a faixa vermelha e o ponteiro.</summary>
        private static void DrawGauge(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float gx = f.BX + 14f;
            float gy = f.BY - 6f;
            ArtKit.Ball(c, gx, gy, 9f, 9f, ArtKit.Brass, 0.25f);
            ArtKit.Ball(c, gx, gy, 7f, 7f, D2.Gauge, 0.5f);

            // A faixa vermelha, no fim da escala.
            for (int i = 0; i <= 10; i++)
            {
                double a = (-200.0 + 220.0 * (0.75 + i * 0.025)) * Math.PI / 180.0;
                c.Set((int)Math.Round(gx + Math.Cos(a) * 5.6), (int)Math.Round(gy + Math.Sin(a) * 5.6), new Rgba(190, 40, 30));
            }

            // As marcas da escala.
            for (int i = 0; i <= 6; i++)
            {
                double a = (-200.0 + 220.0 * i / 6.0) * Math.PI / 180.0;
                c.Set((int)Math.Round(gx + Math.Cos(a) * 6), (int)Math.Round(gy + Math.Sin(a) * 6), ArtKit.Iron[1]);
            }

            float needle = -200f + 220f * Math.Max(0f, Math.Min(1f, f.Pose.Pressure));
            (float nx, float ny) = ArtKit.Along(gx, gy, needle, 5.5f);
            ArtKit.Line(c, (int)Math.Round(gx), (int)Math.Round(gy), (int)Math.Round(nx), (int)Math.Round(ny), new Rgba(150, 30, 26));
            c.Set((int)gx, (int)gy, ArtKit.Brass[1]);

            if (f.Pose.GaugeCracked)
            {
                ArtKit.Line(c, (int)gx - 5, (int)gy - 3, (int)gx + 3, (int)gy + 5, new Rgba(90, 90, 90));
                ArtKit.Line(c, (int)gx + 1, (int)gy - 5, (int)gx - 2, (int)gy + 4, new Rgba(120, 120, 120));
            }

            c.Set((int)(gx - 3f), (int)(gy - 4f), new Rgba(255, 255, 250));
        }

        private static void DrawLeg(Frame f, bool back)
        {
            Pose pose = f.Pose;
            float hipX = f.BX + (back ? -10f : 9f);
            float hipY = f.BY + f.RY - 6f;
            float footX = BodyX + pose.X * 0.4f + (back ? -12f + pose.BackFoot : 11f + pose.FrontFoot);
            float footY = Ground - 4f - (back ? pose.BackLift : pose.FrontLift);
            float bias = back ? -0.4f : 0f;

            (float kx, float ky) = ArtKit.Joint(hipX, hipY, footX, footY, 15f, 15f, -1);
            if (back && pose.Kneel)
            {
                kx = hipX + 7f;
                ky = Ground - 5f;
                footX = kx - 15f;
                footY = Ground - 4f;
            }

            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Iron, 0.18f, back ? 721 : 722, 0.3f, ArtKit.Rust);
            ArtKit.Limb(f.Canvas, hipX, hipY, kx, ky, 6.2f, iron, bias);
            ArtKit.Ball(f.Canvas, hipX, hipY, 6.5f, 6.5f, ArtKit.Iron, bias + 0.1f);

            (float mx, float my) = (kx + (footX - kx) * 0.55f, ky + (footY - ky) * 0.55f);
            ArtKit.Limb(f.Canvas, kx, ky, mx, my, 5f, ArtKit.Corroded(ArtKit.Brass, 0.3f, back ? 723 : 724, 0.35f), bias);
            ArtKit.Limb(f.Canvas, mx, my, footX, footY, 3f, ArtKit.Tin, bias + 0.2f);
            ArtKit.Ball(f.Canvas, kx, ky, 5.5f, 5.5f, ArtKit.Brass, bias);
            ArtKit.Rivet(f.Canvas, (int)kx - 1, (int)ky - 1, ArtKit.Iron);

            float shoeX = footX + 3f;
            ArtKit.Drum(f.Canvas, shoeX, footY + 1f, 11f, 4f, 3f, ArtKit.Plain(ArtKit.Iron), bias + 0.15f);
            f.Canvas.Fill((int)(shoeX - 10f), (int)(footY + 4f), 21, 1, ArtKit.Iron[0]);
            for (int i = 0; i < 3; i++) ArtKit.Rivet(f.Canvas, (int)(shoeX - 6f + i * 6f), (int)(footY), ArtKit.Brass);
        }

        /// <summary>O capacete de soldador do chefe, com a fresta verde e a crista de latao.</summary>
        private static void DrawHead(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            float hx = f.HeadX;
            float hy = f.HeadY;

            // O pescoco sanfonado.
            for (int i = 0; i <= 3; i++)
            {
                float k = i / 3f;
                ArtKit.Ball(c, f.BX + 10f + (hx - 4f - f.BX - 10f) * k, f.Top + 8f + (hy + 6f - f.Top - 8f) * k, 4.5f, 3.5f, ArtKit.Rubber, i % 2 == 0 ? 0.2f : -0.2f);
            }

            ArtKit.Drum(c, hx, hy, 12f, 12.5f, 2.8f, ArtKit.Corroded(ArtKit.Iron, 0.2f, 731, 0.3f, ArtKit.Rust), 0.15f);

            // A crista de latao e as dobradicas.
            for (int y = (int)(hy - 13f); y <= (int)(hy + 6f); y++)
            {
                c.Set((int)(hx - 3f), y, ArtKit.Brass[2]);
                c.Set((int)(hx - 2f), y, ArtKit.Brass[1]);
            }

            ArtKit.Ball(c, hx - 9f, hy + 2f, 2.6f, 2.6f, ArtKit.Brass, 0.2f);

            // O visor: a fresta larga, com o verde de Oxide.
            int vx = (int)(hx + 1f);
            int vy = (int)(hy - 3f);
            c.Fill(vx, vy, 12, 5, ArtKit.Outline);
            Rgba glow = pose.Visor <= 0.05f ? ArtKit.Iron[0] : (pose.Visor < 0.8f ? ArtKit.OxideDeep : ArtKit.OxideGlow);
            c.Fill(vx + 1, vy + 1, 10, 3, glow);
            if (pose.Visor > 1.4f) c.Fill(vx + 3, vy + 2, 6, 1, ArtKit.OxideCore);
            if (f.Hot && pose.Visor > 0.8f) c.Fill(vx + 1, vy + 1, 10, 1, D2.Fire[2]);

            // A grade da boca.
            for (int i = 0; i < 4; i++) c.Fill((int)(hx + 2f + i * 2.5f), (int)(hy + 5f), 1, 4, ArtKit.Iron[0]);
        }

        private static void DrawHammerArm(Frame f, float ex, float ey)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;

            // O martelo: o cabo de cano e a cabeca de chave inglesa.
            (float bx, float by) = ArtKit.Along(f.HandX, f.HandY, pose.Hammer, -9f);
            (float tx, float ty) = ArtKit.Along(f.HandX, f.HandY, pose.Hammer, 34f);
            ArtKit.Limb(c, bx, by, tx, ty, 3.2f, ArtKit.Corroded(ArtKit.Iron, 0.15f, 741, 0.4f, ArtKit.Rust), 0.2f);

            double a = pose.Hammer * Math.PI / 180.0;
            float ux = (float)Math.Cos(a);
            float uy = (float)Math.Sin(a);
            float px = -uy;
            float py = ux;

            // A cabeca: um bloco perpendicular ao cabo, com a boca da chave aberta de um lado.
            (float, float) P(float along, float across) => (tx + ux * along + px * across, ty + uy * along + py * across);
            var head = new[] { P(-7f, -14f), P(9f, -14f), P(9f, 12f), P(-7f, 12f) };
            ArtKit.Polygon(c, head, (x, y) =>
            {
                float rx = x - tx;
                float ry = y - ty;
                float along = rx * ux + ry * uy;
                float across = rx * px + ry * py;

                // A boca da chave: um corte no lado de cima.
                if (across < -5f && along > -1.5f && along < 4.5f) return Rgba.Clear;
                float light = -(px * 0.55f + py * 0.65f) * (across / 14f) + (along > 6.5f ? -0.4f : 0.15f);
                return ArtKit.Tone(ArtKit.Iron, light);
            });

            // O parafuso de regulagem.
            (float sx, float sy) = P(1f, 4f);
            ArtKit.Ball(c, sx, sy, 2.6f, 2.6f, ArtKit.Brass, 0.2f);
            for (int i = -1; i <= 1; i++)
            {
                (float rx2, float ry2) = P(-4.5f, i * 7f);
                ArtKit.Rivet(c, (int)rx2, (int)ry2, ArtKit.Iron);
            }

            // O braco: ombreira, braco, antebraco e a manopla fechada no cabo.
            ArtKit.Taper(c, f.ShoulderX, f.ShoulderY, ex, ey, 6.5f, 5f, ArtKit.Corroded(ArtKit.Brass, 0.25f, 742, 0.3f), 0.1f);
            ArtKit.Limb(c, ex, ey, f.HandX, f.HandY, 4.4f, ArtKit.Iron, 0.1f);
            ArtKit.Ball(c, ex, ey, 5f, 5f, ArtKit.Brass, 0.1f);
            ArtKit.Ball(c, f.ShoulderX, f.ShoulderY - 1f, 9f, 8f, ArtKit.Corroded(ArtKit.Copper, 0.25f, 743, 0.3f), 0.3f);
            for (int i = 0; i < 5; i++)
            {
                (float rx, float ry) = ArtKit.Along(f.ShoulderX, f.ShoulderY - 1f, 170f + i * 40f, 7f);
                ArtKit.Rivet(c, (int)rx, (int)ry, ArtKit.Iron);
            }

            ArtKit.Ball(c, f.HandX, f.HandY, 5f, 5f, ArtKit.Leather, 0.2f);
        }

        /// <summary>O braco de tras, com o bico de vapor na ponta.</summary>
        private static void DrawBackArm(Frame f)
        {
            Pose pose = f.Pose;
            float sx = f.BX - 16f;
            float sy = f.BY - 16f;
            if (pose.Nozzle)
            {
                // Apontando: o ombro vem pra frente, por baixo do martelo.
                sx = f.BX + 8f;
                sy = f.BY + 2f;
            }

            (float ex, float ey) = ArtKit.Along(sx, sy, pose.Back, 15f);
            (float hx, float hy) = ArtKit.Along(ex, ey, pose.BackFore, 14f);
            const float bias = -0.35f;

            ArtKit.Limb(f.Canvas, sx, sy, ex, ey, 4.5f, ArtKit.Brass, bias);
            ArtKit.Limb(f.Canvas, ex, ey, hx, hy, 3.6f, ArtKit.Iron, bias);
            ArtKit.Ball(f.Canvas, ex, ey, 4.2f, 4.2f, ArtKit.Brass, bias);

            (float nx, float ny) = ArtKit.Along(hx, hy, pose.BackFore, 11f);
            ArtKit.Taper(f.Canvas, hx, hy, nx, ny, 4.2f, 3f, ArtKit.Brass, bias + 0.2f);
            ArtKit.Ball(f.Canvas, nx, ny, 2.6f, 4f, ArtKit.Iron, bias + 0.1f);
        }

        private static void DrawGlows(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.Visor >= 1f) ArtKit.Glow(f.Canvas, f.HeadX + 7f, f.HeadY - 1f, 12f, ArtKit.OxideGlow, 0.3f * (pose.Visor - 0.5f), true);

            float furnace = pose.Door > 0.5f ? 1f : pose.Fire * 0.4f;
            if (furnace > 0.2f) ArtKit.Glow(f.Canvas, f.BX - 14f, f.BY + 12f, 14f + pose.Door * 8f, D2.Fire[2], 0.55f * furnace, pose.Door > 0.5f);

            if (f.Hot) ArtKit.Glow(f.Canvas, f.BX, f.BY, 30f, D2.Fire[1], 0.12f);
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
                    if (r < 40f || r > 64f) continue;

                    float a = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    if (a < from || a > to) continue;

                    float k = (a - from) / Math.Max(1f, to - from);
                    float edge = (r - 40f) / 24f;
                    int alpha = (int)(30 + 160 * k * edge);
                    if (alpha < 35) continue;
                    f.Canvas.Blend(px, py, new Rgba(255, 240, 210).WithAlpha(alpha));
                }
            }
        }

        private static void DrawSmoke(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.Puffs <= 0) return;

            Rgba light = f.Hot ? new Rgba(196, 150, 130) : new Rgba(176, 172, 164);
            Rgba dark = f.Hot ? new Rgba(120, 80, 70) : new Rgba(104, 100, 96);

            for (int i = 0; i < pose.Puffs; i++)
            {
                float t = (pose.Smoke + i / (float)pose.Puffs) % 1f;
                float x = f.BX - 16f - t * 10f - (pose.Charging ? t * 16f : 0f);
                float y = f.Top - 32f - t * 22f;
                float r = 3f + t * 5f;
                if (y - r < 0f) continue;
                ArtKit.Puff(f.Canvas, x, y, r, light, dark, (int)(170 * (1f - t * 0.8f)));
            }
        }

        /// <summary>Na sobrepressao: fiapos de vapor escapando das juntas.</summary>
        private static void DrawLeaks(Frame f)
        {
            (float, float)[] joints =
            {
                (f.BX - 20f, f.BY - 16f), (f.BX + 20f, f.BY + 4f), (f.BX - 6f, f.BY + 22f), (f.HeadX - 10f, f.HeadY + 8f),
            };

            for (int i = 0; i < joints.Length; i++)
            {
                (float x, float y) = joints[i];
                int seed = (int)(x * 7f + y * 3f);
                float drift = (ArtKit.Hash(seed, i, 751) % 5) - 2f;
                D2.Steam(f.Canvas, x + drift, y - 4f, 2.5f, 150);
                D2.Steam(f.Canvas, x + drift * 2f, y - 9f, 2f, 90);
            }
        }

        private static void DrawDust(Frame f)
        {
            float dust = f.Pose.Dust;
            if (dust <= 0f) return;
            var light = new Rgba(196, 170, 130);
            var dark = new Rgba(130, 104, 80);
            float spread = 14f + dust * 14f;
            int alpha = dust > 1.5f ? 110 : 200;

            foreach (float side in new[] { -1f, 1f })
            {
                for (int i = 0; i < 3; i++)
                {
                    float x = BodyX + 18f + f.Pose.X + side * (spread + i * 8f);
                    float y = Ground - 3f - i * 2f - dust * 2f;
                    ArtKit.Puff(f.Canvas, x, y, 4f + i + dust, light, dark, alpha - i * 30);
                }
            }
        }

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
                float k = burst * (0.6f + 0.6f * ArtKit.Noise(x * 0.2f, y * 0.2f, 99));
                c.Pixels[i] = Rgba.Lerp(p, k > 0.75f ? ArtKit.OxideCore : ArtKit.OxideGlow, Math.Min(1f, k));
            }

            ArtKit.Glow(c, f.BX, f.BY, 48f, ArtKit.OxideGlow, burst * 0.6f, true);
        }
    }
}
