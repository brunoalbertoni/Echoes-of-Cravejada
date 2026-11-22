using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Rei de Prata, o chefe final, no mesmo traco do Coletor, do Mestre das
    /// Caldeiras e do Capataz — e maior que todos eles: alto, magro, de pe,
    /// todo de Silver 925.
    ///
    /// Peca por peca: o manto de veludo vermelho com a barra dourada e a gola
    /// de arminho, preso no peito por uma corrente de ouro; o peitoral de
    /// prata; a sobreveste vermelha com a marca 925; as ombreiras em tres
    /// laminas; o cinto de ouro e a saia de placas; as grevas e os sapatos de
    /// bico. O rosto e de prata esculpida, de nariz reto e barba pontuda; os
    /// olhos acesos. Na cabeca, a coroa de quartzo de novecentas e vinte e
    /// cinco facetas. Na mao, o cetro de ouro com o cristal de quartzo.
    ///
    /// Duas folhas com os mesmos quadros:
    ///   Radiante   (fase 1) a prata brilha; o Oxide so comeca: a rachadura
    ///              verde no peito e manchas na ombreira de tras.
    ///   Corrompido (fase 2) o Oxide tomou tudo: prata manchada de azinhavre,
    ///              o peito aberto com o Coracao verde, a coroa rachada, o
    ///              manto em farrapos, os olhos verdes. Ele flutua.
    ///
    /// A luta comeca com ele sentado no trono (importado a 24 px por tile: o
    /// assento fica a 1,8 tile do chao e o ultimo degrau a 1,3). Ele acorda, se
    /// levanta, desce os degraus e aponta o cetro.
    ///
    /// O desenho e feito em medidas de projeto (192x196, o mesmo traco do
    /// Capataz) e cada forma e ampliada por Scale na hora de pintar: o quadro
    /// sai com 264x270 e e importado a 32 pixels por tile (uns 5,8 tiles de
    /// altura de pe). O pivo fica no chao, embaixo dele (PivotX).
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static partial class KingArt
    {
        /// <summary>Quanto o desenho e ampliado: o Rei e maior que os outros chefes.</summary>
        private const float Scale = 1.375f;

        private const int DesignWidth = 192;
        private const int DesignHeight = 196;

        public const int Width = 264;
        public const int Height = 270;
        public const int PixelsPerUnit = 32;
        public const float PivotX = BodyX * Scale / Width;
        public const float PivotY = (Height - Ground * Scale) / Height;

        public const int Seated = 0;
        public const int SeatedFrames = 4;
        public const int Rise = 4;
        public const int RiseFrames = 10;
        public const int Idle = 14;
        public const int IdleFrames = 6;
        public const int Walk = 20;
        public const int WalkFrames = 8;
        public const int SweepWindup = 28;
        public const int SweepWindupFrames = 2;
        public const int Sweep = 30;
        public const int SweepFrames = 3;
        public const int Exposed = 33;
        public const int ExposedFrames = 2;
        public const int BeamRaise = 35;
        public const int BeamRaiseFrames = 2;
        public const int BeamFire = 37;
        public const int BeamFireFrames = 3;
        public const int Cast = 40;
        public const int CastFrames = 3;
        public const int Skyward = 43;
        public const int SkywardFrames = 3;
        public const int Inhale = 46;
        public const int InhaleFrames = 3;
        public const int Breath = 49;
        public const int BreathFrames = 3;
        public const int Hurt = 52;
        public const int Fall = 53;
        public const int FallFrames = 4;
        public const int Death = 57;
        public const int DeathFrames = 8;
        public const int FrameCount = 65;

        private const float Ground = 192f;
        private const float BodyX = 84f;
        private const float HipY = 136f;

        /// <summary>A quantos pixels por tile o trono (PalaceArt.Throne) e importado.</summary>
        public const int ThronePixelsPerUnit = 24;

        /// <summary>
        /// O assento do trono (44 px acima da base do desenho do trono) e o
        /// ultimo degrau (32 px), em medidas de projeto acima do chao.
        /// </summary>
        private const float SeatHeight = 44f / ThronePixelsPerUnit * PixelsPerUnit / Scale;
        private const float StepHeight = 32f / ThronePixelsPerUnit * PixelsPerUnit / Scale;

        /// <summary>Onde o golpe do cetro varre, em tiles a frente do pivo.</summary>
        public const float SweepReach = 3.1f;

        private static readonly Rgba[] Silver =
            { new Rgba(84, 90, 112), new Rgba(146, 154, 178), new Rgba(206, 214, 230), new Rgba(250, 252, 255) };

        private static readonly Rgba[] Patina =
            { new Rgba(26, 52, 44), new Rgba(48, 96, 76), new Rgba(82, 144, 110), new Rgba(130, 196, 150) };

        private static readonly Rgba[] Velvet =
            { new Rgba(56, 14, 26), new Rgba(98, 26, 42), new Rgba(146, 44, 62), new Rgba(194, 82, 96) };

        private static readonly Rgba[] RotVelvet =
            { new Rgba(36, 22, 26), new Rgba(64, 38, 40), new Rgba(96, 60, 56), new Rgba(130, 96, 84) };

        private static readonly Rgba[] Gold = ArtKit.Brass;

        /// <summary>O cabelo e a barba de prata escura, pra o rosto se destacar.</summary>
        private static readonly Rgba[] Hair =
            { new Rgba(52, 56, 76), new Rgba(96, 102, 128), new Rgba(146, 152, 178), new Rgba(196, 202, 222) };

        private static readonly Rgba[] SickHair =
            { new Rgba(24, 40, 36), new Rgba(44, 76, 64), new Rgba(76, 116, 96), new Rgba(120, 160, 136) };

        private static readonly Rgba[] Quartz =
            { new Rgba(130, 150, 186), new Rgba(186, 206, 232), new Rgba(230, 240, 252), new Rgba(255, 255, 255) };

        private static readonly Rgba[] SickQuartz =
            { new Rgba(40, 90, 70), new Rgba(80, 160, 120), new Rgba(150, 230, 180), new Rgba(230, 255, 238) };

        private static readonly Rgba[] Ermine =
            { new Rgba(180, 176, 168), new Rgba(214, 210, 200), new Rgba(238, 236, 228), new Rgba(255, 255, 250) };

        private static readonly Rgba ErmineDot = new Rgba(30, 28, 34);
        private static readonly Rgba Green = new Rgba(120, 255, 170);
        private static readonly Rgba GreenCore = new Rgba(232, 255, 238);
        private static readonly Rgba Light = new Rgba(255, 246, 210);
        private static readonly Rgba Dark = new Rgba(20, 16, 24);

        private sealed class Pose
        {
            public float X;
            public float Y;
            public float Lean;
            public float Head;
            public float Mouth;

            public float BackFoot;
            public float FrontFoot;
            public float BackLift;
            public float FrontLift;
            public float Kneel;

            /// <summary>Sentado no trono: 0 de pe, 1 sentado (o quadril no assento).</summary>
            public float Sit;

            /// <summary>A altura dos pes acima do chao (em pe ou sentado nos degraus do trono).</summary>
            public float StepY;

            /// <summary>O braco do cetro: braco, antebraco (graus, 90 = pra baixo) e o cetro.</summary>
            public float Arm = 80f;
            public float Fore = 40f;
            public float Scepter = -70f;
            public bool ScepterDropped;
            public float ScepterGlow = 0.4f;

            /// <summary>O braco livre, de tras.</summary>
            public float BackArm = 100f;
            public float BackFore = 80f;

            /// <summary>O manto: quanto ele voa pra tras (0 parado, 1 esvoacando).</summary>
            public float Cape;

            public float Eyes = 1f;
            public float CrownGlow = 0.5f;
            public float CrownCrack;
            public bool CrownOff;
            public float Rays;
            public float Chest = 1f;
            public float SmearFrom;
            public float SmearTo;
            public float Dust;
            public float Mist;
            public float Pillar;
            public bool Lying;
            public float Dull;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
        }

        // Quando so se quer saber onde ficam as pontas, nao desenha.
        private static bool _measure;
        private static (float X, float Y) _lastTip;
        private static (float X, float Y) _lastCrown;
        private static (float X, float Y) _lastMouth;

        private static Pose BeamPose(int i) => new Pose
        {
            X = -1f, Lean = -4f, Arm = -10f + i * 2f, Fore = 10f + i * 2f, Scepter = 12f + i * 4f, ScepterGlow = 2f, Cape = 0.4f,
            BackArm = 140f, BackFore = 160f, FrontFoot = 8f, BackFoot = -8f, Eyes = 1.5f, Rays = 0.8f,
        };

        private static Pose CastPose(int i) => new Pose
        {
            Y = -1f, Lean = -6f, Head = -14f - i * 2f, BackArm = -110f - i * 6f, BackFore = -120f - i * 8f,
            Arm = 70f, Fore = 30f, Scepter = -80f, CrownGlow = 1f + i * 0.4f, Rays = 0.6f + i * 0.3f, Cape = 0.3f, Eyes = 1.6f,
        };

        private static Pose SkywardPose(int i) => i == 0
            ? new Pose
            {
                Y = 6f, Lean = 10f, Head = 6f, Arm = 100f, Fore = 120f, Scepter = 110f, ScepterGlow = 1.6f,
                BackArm = 70f, BackFore = 40f, FrontFoot = 8f, BackFoot = -8f, Eyes = 1.8f, CrownGlow = 1.4f, Cape = 0.2f,
            }
            : new Pose
            {
                Y = -3f, Lean = -6f, Head = -22f, Mouth = 0.5f, Arm = -92f, Fore = -90f, Scepter = -90f, ScepterGlow = 2.6f - i * 0.4f,
                BackArm = -130f, BackFore = -150f, FrontFoot = 6f, BackFoot = -6f, Eyes = 2f, CrownGlow = 2f, Rays = 1.4f - i * 0.3f,
                Cape = 0.7f, Pillar = 1.2f - i * 0.4f,
            };

        private static Pose BreathPose(int i) => new Pose
        {
            Lean = 18f, Head = 18f, Mouth = 1f, Arm = 100f, Fore = 80f, Scepter = -60f, BackArm = 150f, BackFore = 170f,
            FrontFoot = 8f, BackFoot = -8f, Mist = 1f + i * 0.2f, Cape = 0.6f, Eyes = 1.6f,
        };

        /// <summary>A ponta do cetro no quadro do raio, em tiles a partir do pivo (olhando pra direita).</summary>
        public static (float X, float Y) ScepterTip => Measure(BeamPose(1), 0);

        /// <summary>A ponta do cetro erguido pro alto (o golpe contra quem pula em cima dele).</summary>
        public static (float X, float Y) SkywardTip => Measure(SkywardPose(1), 0);

        /// <summary>A coroa no quadro do brilho, de onde saem as facetas.</summary>
        public static (float X, float Y) CrownPoint => Measure(CastPose(2), 1);

        /// <summary>A boca no quadro do sopro.</summary>
        public static (float X, float Y) Mouth => Measure(BreathPose(1), 2);

        /// <summary>O peito (a rachadura na fase 1, o Coracao na fase 2), de pe e parado.</summary>
        public static (float X, float Y) Chest => (-6f * Scale / PixelsPerUnit, 21f * Scale / PixelsPerUnit + (Ground - HipY) * Scale / PixelsPerUnit);

        /// <summary>A altura do topo da coroa, de pe, em tiles: o tamanho dele.</summary>
        public static float StandingHeight => (Ground - (HipY - 34f - 17f - 26f)) * Scale / PixelsPerUnit;

        private static (float X, float Y) Measure(Pose pose, int which)
        {
            _measure = true;
            Draw(pose, false);
            _measure = false;
            (float x, float y) = which == 0 ? _lastTip : (which == 1 ? _lastCrown : _lastMouth);
            return ((x - BodyX) * Scale / PixelsPerUnit, (Ground - y) * Scale / PixelsPerUnit);
        }

        // ------------------------------------------------------------------
        // Os quadros
        // ------------------------------------------------------------------

        public static PixelCanvas[] Frames(bool corrupt)
        {
            var f = new PixelCanvas[FrameCount];

            // Sentado no trono, de cabeca baixa, o cetro em pe ao lado, respirando devagar.
            for (int i = 0; i < SeatedFrames; i++)
            {
                f[Seated + i] = Draw(SeatedPose(i % 2 * 0.5f, 26f, 0.2f, 0.2f + (i == 2 ? 0.5f : 0f)), corrupt);
            }

            DrawRise(f, corrupt);

            float[] breath = { 0f, 0.5f, 1f, 1f, 0.5f, 0f };
            for (int i = 0; i < IdleFrames; i++)
            {
                f[Idle + i] = Draw(new Pose
                {
                    Y = breath[i], Head = -breath[i] * 2f, Fore = 40f + breath[i] * 3f, Scepter = -70f + breath[i] * 2f,
                    BackFore = 80f - breath[i] * 4f, Cape = breath[i] * 0.15f, CrownGlow = 0.5f + breath[i] * 0.3f, Rays = 0.2f,
                }, corrupt);
            }

            // Andando: passo de rei, lento, o manto ondulando.
            for (int i = 0; i < WalkFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / WalkFrames;
                float swing = (float)Math.Sin(phase);
                float bob = (float)Math.Abs(Math.Cos(phase));
                f[Walk + i] = Draw(new Pose
                {
                    Y = (1f - bob) * 2f, Lean = 3f, BackFoot = -swing * 9f, FrontFoot = swing * 9f,
                    BackLift = swing > 0.2f ? swing * 5f : 0f, FrontLift = swing < -0.2f ? -swing * 5f : 0f,
                    Fore = 40f + swing * 4f, Scepter = -70f + swing * 3f, BackArm = 100f - swing * 12f, BackFore = 80f - swing * 10f,
                    Cape = 0.35f + (float)Math.Sin(phase * 2.0) * 0.1f,
                }, corrupt);
            }

            // O golpe do cetro: recolhe por cima do ombro...
            f[SweepWindup + 0] = Draw(new Pose
            {
                X = -2f, Lean = -6f, Head = -4f, Arm = -100f, Fore = -140f, Scepter = -170f, ScepterGlow = 1.2f,
                BackArm = 60f, BackFore = 20f, BackFoot = -6f, FrontFoot = 6f, Cape = 0.3f, Eyes = 1.4f,
            }, corrupt);
            f[SweepWindup + 1] = Draw(new Pose
            {
                X = -4f, Y = 1f, Lean = -10f, Head = -6f, Arm = -120f, Fore = -165f, Scepter = 170f, ScepterGlow = 1.6f,
                BackArm = 50f, BackFore = 10f, BackFoot = -8f, FrontFoot = 8f, Cape = 0.4f, Eyes = 1.8f,
            }, corrupt);

            // ...e varre rente ao chao, de tras pra frente.
            f[Sweep + 0] = Draw(new Pose
            {
                X = 2f, Lean = 14f, Arm = -20f, Fore = 0f, Scepter = 20f, ScepterGlow = 1.8f, SmearFrom = -170f, SmearTo = 20f,
                BackArm = 160f, BackFore = 170f, BackFoot = -9f, FrontFoot = 10f, Cape = 0.8f, Eyes = 1.8f,
            }, corrupt);
            Pose strike = new Pose
            {
                X = 4f, Y = 4f, Lean = 26f, Head = 6f, Arm = 40f, Fore = 50f, Scepter = 70f, ScepterGlow = 2f, SmearFrom = -30f, SmearTo = 70f,
                BackArm = 170f, BackFore = 190f, BackFoot = -11f, FrontFoot = 12f, Cape = 1f, Eyes = 1.8f, Dust = 1f,
            };
            strike.Sparks.Add((158, 186, 3));
            f[Sweep + 1] = Draw(strike, corrupt);
            f[Sweep + 2] = Draw(new Pose
            {
                X = 4f, Y = 4f, Lean = 26f, Head = 8f, Arm = 45f, Fore = 55f, Scepter = 80f, ScepterGlow = 1.2f,
                BackArm = 160f, BackFore = 180f, BackFoot = -11f, FrontFoot = 12f, Cape = 0.7f, Eyes = 1.4f, Dust = 2f,
            }, corrupt);

            // Exposto: o cetro fincado na frente, apoiado nele, arfando.
            for (int i = 0; i < ExposedFrames; i++)
            {
                f[Exposed + i] = Draw(new Pose
                {
                    X = 3f, Y = 5f + i, Lean = 28f, Head = 14f + i * 2f, Mouth = 0.4f, Arm = 50f, Fore = 70f, Scepter = 85f, ScepterGlow = 0.3f,
                    BackArm = 100f, BackFore = 90f, BackFoot = -10f, FrontFoot = 10f, Cape = 0.1f, Eyes = 0.8f, Chest = 1.4f,
                }, corrupt);
            }

            // O raio: ergue o cetro e aponta; o cristal acende.
            f[BeamRaise + 0] = Draw(new Pose { Lean = -4f, Arm = -40f, Fore = -60f, Scepter = -60f, ScepterGlow = 1.2f, BackArm = 120f, BackFore = 140f, Cape = 0.3f, Eyes = 1.4f, Rays = 0.4f }, corrupt);
            f[BeamRaise + 1] = Draw(new Pose { Lean = -6f, Arm = -30f, Fore = -20f, Scepter = -20f, ScepterGlow = 1.8f, BackArm = 130f, BackFore = 150f, Cape = 0.35f, Eyes = 1.6f, Rays = 0.6f }, corrupt);
            for (int i = 0; i < BeamFireFrames; i++) f[BeamFire + i] = Draw(BeamPose(i), corrupt);

            // O brilho da coroa: a mao de tras pro alto, a cabeca pra tras.
            for (int i = 0; i < CastFrames; i++) f[Cast + i] = Draw(CastPose(i), corrupt);

            // Pro alto: agacha com o cetro baixo e o enfia pro ceu — a coluna de luz.
            for (int i = 0; i < SkywardFrames; i++) f[Skyward + i] = Draw(SkywardPose(i), corrupt);

            // Inspira: o peito enche, a cabeca pra tras.
            for (int i = 0; i < InhaleFrames; i++)
            {
                f[Inhale + i] = Draw(new Pose
                {
                    Y = -i, Lean = -8f - i * 2f, Head = -16f - i * 4f, Arm = 80f, Fore = 40f, Scepter = -70f,
                    BackArm = 60f - i * 10f, BackFore = 30f - i * 10f, Chest = 1.2f + i * 0.3f, Cape = 0.2f, Eyes = 1.6f,
                }, corrupt);
            }

            // Sopra: inclinado, a boca aberta, a nevoa verde saindo.
            for (int i = 0; i < BreathFrames; i++) f[Breath + i] = Draw(BreathPose(i), corrupt);

            Pose hurt = new Pose { X = -4f, Lean = -12f, Head = -14f, Mouth = 0.6f, Arm = 110f, Fore = 60f, Scepter = -40f, FrontLift = 3f, Eyes = 0.6f, Cape = 0.5f };
            hurt.Sparks.Add((96, 112, 3));
            f[Hurt] = Draw(hurt, corrupt);

            // O fim da fase 1: cai de joelhos e a coroa racha.
            for (int i = 0; i < FallFrames; i++)
            {
                float k = (i + 1) / (float)FallFrames;
                f[Fall + i] = Draw(new Pose
                {
                    Kneel = k, Lean = 10f + k * 14f, Head = 10f + k * 14f, Mouth = 0.5f, Arm = 70f, Fore = 80f - k * 20f, Scepter = 90f,
                    BackArm = 90f, BackFore = 100f, Eyes = 1.4f - k, CrownGlow = 1f + k, CrownCrack = k, Cape = 0.1f, Chest = 1f + k,
                }, corrupt);
            }

            DrawDeath(f, corrupt);
            return f;
        }

        private static Pose SeatedPose(float breath, float head, float eyes, float crown) => new Pose
        {
            Sit = 1f, StepY = StepHeight, Y = breath, Lean = 6f, Head = head, Eyes = eyes, CrownGlow = crown,
            Arm = 75f, Fore = 15f, Scepter = -86f, BackArm = 40f, BackFore = 10f, FrontFoot = 10f, BackFoot = 6f,
        };

        /// <summary>
        /// Levantando do trono, quadro a quadro: os olhos acendem, a cabeca
        /// sobe e encara o Scrap, a coroa acende; ele firma o cetro, inclina,
        /// empurra o corpo pra cima, fica de pe no degrau, desce, pisa no chao
        /// e aponta o cetro.
        /// </summary>
        private static void DrawRise(PixelCanvas[] f, bool corrupt)
        {
            f[Rise + 0] = Draw(SeatedPose(0f, 22f, 0.8f, 0.4f), corrupt);
            f[Rise + 1] = Draw(SeatedPose(0f, 8f, 1.4f, 0.8f), corrupt);

            Pose stare = SeatedPose(0f, -4f, 1.8f, 1.4f);
            stare.Rays = 0.4f;
            f[Rise + 2] = Draw(stare, corrupt);

            Pose grip = SeatedPose(0f, -2f, 1.8f, 1.2f);
            grip.Lean = 14f;
            grip.Arm = 70f;
            grip.Fore = 40f;
            grip.Scepter = -86f;
            grip.BackArm = 60f;
            grip.BackFore = 60f;
            f[Rise + 3] = Draw(grip, corrupt);

            Pose push = SeatedPose(0f, 4f, 1.8f, 1.2f);
            push.Sit = 0.6f;
            push.Lean = 26f;
            push.Arm = 70f;
            push.Fore = 60f;
            push.Scepter = -84f;
            push.BackArm = 80f;
            push.BackFore = 80f;
            push.FrontFoot = 6f;
            f[Rise + 4] = Draw(push, corrupt);

            f[Rise + 5] = Draw(new Pose
            {
                StepY = StepHeight, Sit = 0.25f, Lean = 14f, Head = 0f, Arm = 75f, Fore = 50f, Scepter = -84f,
                BackArm = 95f, BackFore = 85f, FrontFoot = 3f, BackFoot = 0f, Eyes = 1.8f, CrownGlow = 1.3f, Cape = 0.2f,
            }, corrupt);

            Pose stand = new Pose
            {
                StepY = StepHeight, Head = -6f, Arm = 80f, Fore = 40f, Scepter = -70f, Eyes = 1.8f, CrownGlow = 1.6f, Rays = 0.7f, Cape = 0.4f,
            };
            f[Rise + 6] = Draw(stand, corrupt);

            // Desce os degraus: um pe embaixo, o outro ainda em cima.
            f[Rise + 7] = Draw(new Pose
            {
                StepY = StepHeight * 0.55f, Lean = 6f, FrontFoot = 8f, FrontLift = -StepHeight * 0.4f, BackFoot = -6f, BackLift = StepHeight * 0.4f,
                Arm = 80f, Fore = 40f, Scepter = -70f, Eyes = 1.8f, CrownGlow = 1.4f, Cape = 0.7f,
            }, corrupt);

            f[Rise + 8] = Draw(new Pose
            {
                Y = 3f, Lean = 4f, FrontFoot = 8f, BackFoot = -8f, Arm = 80f, Fore = 40f, Scepter = -70f, Eyes = 1.8f, CrownGlow = 1.4f,
                Cape = 0.9f, Dust = 0.6f,
            }, corrupt);

            // E aponta o cetro pro Scrap.
            f[Rise + 9] = Draw(new Pose
            {
                Lean = -4f, Head = -4f, Mouth = 0.4f, Arm = -20f, Fore = -4f, Scepter = 0f, ScepterGlow = 2f, BackArm = 130f, BackFore = 150f,
                FrontFoot = 8f, BackFoot = -8f, Eyes = 2f, CrownGlow = 1.8f, Rays = 1.2f, Cape = 0.5f,
            }, corrupt);
        }

        private static void DrawDeath(PixelCanvas[] f, bool corrupt)
        {
            Pose stagger = new Pose { X = -4f, Lean = -14f, Head = -18f, Mouth = 0.8f, Arm = 120f, Fore = 80f, Scepter = -30f, Eyes = 2f, Chest = 2f, Cape = 0.6f };
            stagger.Sparks.Add((92, 116, 3));
            f[Death + 0] = Draw(stagger, corrupt);
            f[Death + 1] = Draw(new Pose { X = -2f, Lean = 4f, Head = 6f, Mouth = 0.5f, ScepterDropped = true, Arm = 100f, Fore = 120f, BackArm = 80f, BackFore = 100f, Eyes = 1.2f, Chest = 1.6f }, corrupt);
            f[Death + 2] = Draw(new Pose { Kneel = 1f, Lean = 12f, Head = 14f, ScepterDropped = true, Arm = 90f, Fore = 100f, BackArm = 85f, BackFore = 95f, Eyes = 0.8f, Chest = 1.2f, Dust = 0.8f }, corrupt);
            f[Death + 3] = Draw(new Pose { Kneel = 1f, Lean = 16f, Head = 24f, ScepterDropped = true, CrownOff = true, Arm = 90f, Fore = 100f, BackArm = 85f, BackFore = 95f, Eyes = 0.5f, Chest = 0.8f }, corrupt);
            f[Death + 4] = Draw(new Pose { Kneel = 1f, Lean = 8f, Head = -4f, ScepterDropped = true, CrownOff = true, Arm = 60f, Fore = 20f, BackArm = 85f, BackFore = 95f, Eyes = 0.4f, Chest = 0.5f, Dull = 0.4f }, corrupt);
            f[Death + 5] = Draw(new Pose { Kneel = 1f, Y = 3f, Lean = 34f, Head = 30f, ScepterDropped = true, CrownOff = true, Arm = 100f, Fore = 100f, BackArm = 80f, BackFore = 90f, Eyes = 0.2f, Chest = 0.3f, Dull = 0.7f }, corrupt);
            f[Death + 6] = Draw(new Pose { Lying = true, Eyes = 0.1f, Chest = 0.2f, Dull = 0.9f }, corrupt);
            f[Death + 7] = Draw(new Pose { Lying = true, Eyes = 0f, Chest = 0f, Dull = 1f }, corrupt);
        }

        private static float Lerp(float a, float b, float k) => a + (b - a) * k;

        // ------------------------------------------------------------------
        // A ampliacao: cada forma e pintada maior, no quadro de verdade
        // ------------------------------------------------------------------

        private static int P(float v) => (int)Math.Floor(v * Scale);

        private static void Ball(PixelCanvas c, float x, float y, float rx, float ry, ArtKit.Skin skin, float bias = 0f) =>
            ArtKit.Ball(c, x * Scale, y * Scale, rx * Scale, ry * Scale, skin, bias);

        private static void Ball(PixelCanvas c, float x, float y, float rx, float ry, Rgba[] ramp, float bias = 0f) =>
            ArtKit.Ball(c, x * Scale, y * Scale, rx * Scale, ry * Scale, ramp, bias);

        private static void Drum(PixelCanvas c, float x, float y, float rx, float ry, float power, ArtKit.Skin skin, float bias = 0f) =>
            ArtKit.Drum(c, x * Scale, y * Scale, rx * Scale, ry * Scale, power, skin, bias);

        private static void Limb(PixelCanvas c, float x0, float y0, float x1, float y1, float half, ArtKit.Skin skin, float bias = 0f) =>
            ArtKit.Limb(c, x0 * Scale, y0 * Scale, x1 * Scale, y1 * Scale, half * Scale, skin, bias);

        private static void Limb(PixelCanvas c, float x0, float y0, float x1, float y1, float half, Rgba[] ramp, float bias = 0f) =>
            ArtKit.Limb(c, x0 * Scale, y0 * Scale, x1 * Scale, y1 * Scale, half * Scale, ramp, bias);

        private static void Taper(PixelCanvas c, float x0, float y0, float x1, float y1, float h0, float h1, ArtKit.Skin skin, float bias = 0f) =>
            ArtKit.Taper(c, x0 * Scale, y0 * Scale, x1 * Scale, y1 * Scale, h0 * Scale, h1 * Scale, skin, bias);

        private static void Taper(PixelCanvas c, float x0, float y0, float x1, float y1, float h0, float h1, Rgba[] ramp, float bias = 0f) =>
            ArtKit.Taper(c, x0 * Scale, y0 * Scale, x1 * Scale, y1 * Scale, h0 * Scale, h1 * Scale, ramp, bias);

        private static void Polygon(PixelCanvas c, (float X, float Y)[] points, Func<float, float, Rgba> shade)
        {
            var scaled = new (float X, float Y)[points.Length];
            for (int i = 0; i < points.Length; i++) scaled[i] = (points[i].X * Scale, points[i].Y * Scale);
            ArtKit.Polygon(c, scaled, (x, y) => shade(x / Scale, y / Scale));
        }

        private static void Line(PixelCanvas c, int x0, int y0, int x1, int y1, Rgba color) =>
            ArtKit.Line(c, P(x0 + 0.5f), P(y0 + 0.5f), P(x1 + 0.5f), P(y1 + 0.5f), color);

        private static void Rivet(PixelCanvas c, int x, int y, Rgba[] ramp) => ArtKit.Rivet(c, P(x), P(y), ramp);

        private static void Crack(PixelCanvas c, float x, float y, float angle, int length, int seed, float glow) =>
            ArtKit.Crack(c, x * Scale, y * Scale, angle, (int)(length * Scale), seed, glow);

        private static void Glow(PixelCanvas c, float x, float y, float radius, Rgba color, float strength, bool outside = false) =>
            ArtKit.Glow(c, x * Scale, y * Scale, radius * Scale, color, strength, outside);

        private static void Puff(PixelCanvas c, float x, float y, float r, Rgba light, Rgba dark, int alpha) =>
            ArtKit.Puff(c, x * Scale, y * Scale, r * Scale, light, dark, alpha);

        private static void Spark(PixelCanvas c, int x, int y, int size) => ArtKit.Spark(c, P(x), P(y), (int)Math.Round(size * Scale));

        /// <summary>Um pixel de projeto: vira o bloquinho de pixels de verdade que ele cobre.</summary>
        private static void Set(PixelCanvas c, int x, int y, Rgba color)
        {
            int x0 = P(x), x1 = P(x + 1), y0 = P(y), y1 = P(y + 1);
            c.Fill(x0, y0, Math.Max(1, x1 - x0), Math.Max(1, y1 - y0), color);
        }

        private static void Fill(PixelCanvas c, int x, int y, int w, int h, Rgba color)
        {
            int x0 = P(x), x1 = P(x + w), y0 = P(y), y1 = P(y + h);
            c.Fill(x0, y0, Math.Max(1, x1 - x0), Math.Max(1, y1 - y0), color);
        }

        private static Rgba Get(PixelCanvas c, int x, int y) => c.Get(P(x + 0.5f), P(y + 0.5f));

        private static void Blend(PixelCanvas c, int x, int y, Rgba color)
        {
            int x0 = P(x), x1 = P(x + 1), y0 = P(y), y1 = P(y + 1);
            for (int py = y0; py < Math.Max(y0 + 1, y1); py++)
            {
                for (int px = x0; px < Math.Max(x0 + 1, x1); px++) c.Blend(px, py, color);
            }
        }
    }
}
