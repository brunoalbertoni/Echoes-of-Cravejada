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
    public static class KingArt
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

        // ------------------------------------------------------------------
        // Montagem
        // ------------------------------------------------------------------

        private sealed class Frame
        {
            public PixelCanvas Canvas = new PixelCanvas(Width, Height);
            public Pose Pose;
            public bool Corrupt;
            public float Float;
            public float HX;
            public float HY;
            public float SX;
            public float SY;
            public float HeadX;
            public float HeadY;
            public float ElbowX;
            public float ElbowY;
            public float HandX;
            public float HandY;

            public Rgba[] Metal => Corrupt ? Patina : Silver;
            public float MouthX => HeadX + 9f;
            public float MouthY => HeadY + 6f;
        }

        private static ArtKit.Skin MetalSkin(Frame f, int seed)
        {
            float dull = f.Pose.Dull;
            if (f.Corrupt)
            {
                // Prata tomada: manchas grandes de azinhavre, picadas, fendas.
                return (x, y, light) =>
                {
                    float n = ArtKit.Noise(x * 0.16f, y * 0.16f, seed);
                    if (n > 0.5f) return ArtKit.Tone(Patina, light - 0.05f - dull * 0.4f);
                    if (ArtKit.Hash((int)Math.Floor(x), (int)Math.Floor(y), seed + 3) % 17 == 0) return Patina[0];
                    return ArtKit.Tone(Silver, light - 0.25f - dull * 0.4f);
                };
            }

            return (x, y, light) => ArtKit.Tone(Silver, light + 0.12f - dull * 0.5f);
        }

        private static PixelCanvas Draw(Pose pose, bool corrupt)
        {
            var f = new Frame { Pose = pose, Corrupt = corrupt };

            if (pose.Lying)
            {
                DrawLying(f);
                if (!_measure) ArtKit.AddOutline(f.Canvas);
                return f.Canvas;
            }

            // Na fase 2 ele flutua: o corpo inteiro sobe um pouco.
            f.Float = corrupt && pose.Kneel < 0.5f && pose.Sit <= 0f && pose.StepY <= 0f ? 6f : 0f;
            f.HX = BodyX + pose.X + pose.Sit * 2f;
            float standing = HipY + pose.Y + pose.Kneel * 18f - f.Float - pose.StepY;
            float seated = Ground - SeatHeight - 2f + pose.Y;
            f.HY = Lerp(standing, seated, pose.Sit);
            (f.SX, f.SY) = ArtKit.Along(f.HX, f.HY, -90f + pose.Lean, 34f);
            (f.HeadX, f.HeadY) = ArtKit.Along(f.SX, f.SY, -90f + pose.Lean * 1.2f + 12f + pose.Head, 17f);

            float fsx = f.SX + 4f;
            float fsy = f.SY + 4f;
            (f.ElbowX, f.ElbowY) = ArtKit.Along(fsx, fsy, pose.Arm, 18f);
            (f.HandX, f.HandY) = ArtKit.Along(f.ElbowX, f.ElbowY, pose.Fore, 17f);

            DrawCape(f);
            DrawBackArm(f);
            DrawLeg(f, true);
            DrawLeg(f, false);
            DrawBody(f);
            DrawHead(f);
            if (!pose.ScepterDropped) DrawScepter(f);
            else DrawDroppedScepter(f);
            if (pose.CrownOff) DrawFallenCrown(f, f.HX + 40f, Ground - 4f);
            DrawFrontArm(f);

            if (_measure) return f.Canvas;

            ArtKit.AddOutline(f.Canvas);

            DrawGlows(f);
            DrawPillar(f);
            DrawSmear(f);
            DrawMist(f);
            DrawWisps(f);
            DrawDust(f);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) Spark(f.Canvas, spark.X, spark.Y, spark.Size);
            return f.Canvas;
        }

        /// <summary>O manto: cai das costas ate o chao, a barra dourada; voa pra tras quando ele se mexe.</summary>
        private static void DrawCape(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            float fly = pose.Cape;
            float bottom = Ground - 1f - f.Float * 0.5f - pose.StepY;
            if (pose.Kneel > 0.5f) bottom = Ground - 1f;

            var points = new List<(float, float)>
            {
                (f.SX - 6f, f.SY - 3f),
                (f.SX + 4f, f.SY - 2f),
                (f.HX - 2f, f.HY + 8f),
                (f.HX - 4f - fly * 4f, bottom),
            };

            // A barra, ondulada, indo pra tras.
            int folds = 6;
            for (int i = 1; i <= folds; i++)
            {
                float t = i / (float)folds;
                float x = f.HX - 4f - fly * 4f - t * (26f + fly * 22f);
                float y = bottom - t * fly * 18f + (i % 2 == 0 ? 0f : 3f);
                points.Add((x, y));
            }

            points.Add((f.SX - 18f - fly * 8f, f.SY + 20f));
            points.Add((f.SX - 12f, f.SY + 2f));

            Rgba[] ramp = f.Corrupt ? RotVelvet : Velvet;
            (float, float)[] shape = points.ToArray();
            Polygon(c, shape, (x, y) =>
            {
                // Pregas verticais: faixas mais escuras.
                float fold = (float)Math.Sin((x - f.HX) * 0.45f + (y - f.SY) * 0.05f);
                float light = 0.05f + fold * 0.35f - (y - f.SY) * 0.004f;

                // Na fase 2, furos e farrapos perto da barra.
                if (f.Corrupt && y > bottom - 26f && ArtKit.Noise(x * 0.3f, y * 0.3f, 9301) > 0.72f) return Rgba.Clear;
                return ArtKit.Tone(ramp, light);
            });

            // A barra dourada.
            for (int i = 4; i < points.Count - 2; i++)
            {
                (float x0, float y0) = points[i - 1];
                (float x1, float y1) = points[i];
                Line(c, (int)x0, (int)y0 - 1, (int)x1, (int)y1 - 1, Gold[2]);
                Line(c, (int)x0, (int)y0, (int)x1, (int)y1, f.Corrupt ? Gold[0] : Gold[1]);
            }
        }

        private static void DrawLeg(Frame f, bool back)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;
            float hipX = f.HX + (back ? -4f : 4f);
            float hipY = f.HY + 2f;
            float footX = BodyX + pose.X * 0.4f + (back ? -7f + pose.BackFoot : 8f + pose.FrontFoot) + pose.Sit * 20f;
            float footY = Ground - 4f - (back ? pose.BackLift : pose.FrontLift) - f.Float - pose.StepY;
            float bias = back ? -0.45f : 0f;

            // Flutuando, as pernas pendem: os pes apontam pra baixo.
            if (f.Float > 0f)
            {
                footX = f.HX + (back ? -6f : 4f) + (back ? pose.BackFoot : pose.FrontFoot) * 0.3f;
                footY = f.HY + 50f - (back ? pose.BackLift : pose.FrontLift) * 0.5f;
            }

            float kx, ky;
            if (back && pose.Kneel > 0.5f)
            {
                kx = hipX + 8f;
                ky = Ground - 5f;
                footX = kx - 24f;
                footY = Ground - 4f;
            }
            else
            {
                if (!back && pose.Kneel > 0.5f) footX = f.HX + 20f;
                (kx, ky) = ArtKit.Joint(hipX, hipY, footX, footY, 27f, 27f, -1);
            }

            ArtKit.Skin metal = MetalSkin(f, back ? 9311 : 9312);
            Taper(c, hipX, hipY, kx, ky, 6.5f, 5.5f, metal, bias);
            Taper(c, kx, ky, footX, footY, 5.5f, 4f, metal, bias);

            // A joelheira, com a roseta dourada.
            Ball(c, kx, ky, 5.5f, 5f, f.Metal, bias + 0.2f);
            Set(c, (int)kx, (int)ky, Gold[2]);

            // O sapato de bico, de placas.
            Drum(c, footX + 2f, footY + 1f, 7f, 3.6f, 3f, metal, bias + 0.1f);
            Taper(c, footX + 6f, footY + 2f, footX + 13f, footY + 3f, 2.6f, 0.4f, f.Metal, bias + 0.15f);
            Fill(c, (int)footX - 4, (int)footY + 4, 16, 1, Gold[0]);
        }

        /// <summary>
        /// O tronco: o peitoral de prata com a marca 925 e a rachadura (ou o
        /// peito aberto com o Coracao), a saia de placas, o cinto de ouro, a
        /// gola de arminho e a corrente do manto.
        /// </summary>
        private static void DrawBody(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            float hx = f.HX;
            float hy = f.HY;
            float sx = f.SX;
            float sy = f.SY;
            float mx = (hx + sx) * 0.5f;
            float my = (hy + sy) * 0.5f;

            ArtKit.Skin metal = MetalSkin(f, 9321);

            // A saia de placas, ate o joelho.
            var skirt = new[] { (hx - 10f, hy - 6f), (hx + 10f, hy - 6f), (hx + 13f, hy + 18f), (hx - 12f, hy + 18f) };
            Polygon(c, skirt, (x, y) =>
            {
                bool seam = ((int)(y - hy) % 7) == 0;
                float light = 0.35f - (x - hx + 13f) / 26f * 0.9f - (seam ? 0.5f : 0f);
                return metal(x - hx, y - hy, light);
            });

            for (int i = 0; i < 4; i++)
            {
                int y = (int)(hy - 4f + i * 7f);
                Set(c, (int)hx + 10, y + 3, Gold[2]);
            }

            // A sobreveste de veludo na frente, com a barra dourada e a marca 925.
            Rgba[] cloth = f.Corrupt ? RotVelvet : Velvet;
            var tabard = new[] { (hx - 3f, hy - 9f), (hx + 9f, hy - 9f), (hx + 11f, hy + 21f), (hx + 4f, hy + 23f), (hx - 4f, hy + 21f) };
            Polygon(c, tabard, (x, y) =>
            {
                if (f.Corrupt && y > hy + 12f && ArtKit.Noise(x * 0.4f, y * 0.4f, 9325) > 0.7f) return Rgba.Clear;
                bool trim = x < hx - 1.5f || x > hx + 7.5f + (y - hy) * 0.1f;
                return trim ? Gold[f.Corrupt ? 0 : 2] : ArtKit.Tone(cloth, 0.3f - (x - hx) * 0.06f);
            });

            int tx = (int)hx;
            int ty = (int)hy + 4;
            Fill(c, tx, ty, 7, 4, Gold[2]);
            for (int d = 0; d < 3; d++) Set(c, tx + 1 + d * 2, ty + 1 + d % 2, Gold[0]);
            Set(c, tx + 3, ty - 2, Gold[3]);
            Set(c, tx + 2, ty - 1, Gold[2]);
            Set(c, tx + 4, ty - 1, Gold[2]);

            // O tronco: o peito largo e a cintura fina.
            Ball(c, hx, hy - 6f, 11f, 9f, metal, 0f);
            Ball(c, mx + 1f, my, 14f, 15f, metal, 0.1f);
            Ball(c, sx + 2f, sy + 6f, 15f, 11f, metal, 0.2f);

            // As linhas das placas do peitoral.
            Line(c, (int)mx - 8, (int)my + 6, (int)mx + 12, (int)my + 4, f.Metal[0]);
            Line(c, (int)mx - 6, (int)my - 6, (int)mx + 13, (int)my - 8, f.Metal[1]);

            if (f.Corrupt)
            {
                // O peito aberto: as bordas rasgadas e o Coracao aceso la dentro.
                float cx = mx - 6f;
                float cy = my - 4f;
                Ball(c, cx, cy, 6.5f, 7.5f, new[] { Dark, Dark, Patina[0], Patina[1] }, -0.3f);
                float r = 3f + pose.Chest * 0.8f;
                Polygon(c, new[] { (cx, cy - r - 1f), (cx + r, cy), (cx, cy + r + 1f), (cx - r, cy) },
                    (x, y) => ArtKit.Tone(new[] { Patina[1], Green, Green, GreenCore }, 0.7f - (x - cx + y - cy) * 0.12f));
                Set(c, (int)cx - 1, (int)cy - 2, GreenCore);
                for (int i = 0; i < 4; i++) Crack(c, cx + 4f, cy - 6f + i * 4f, 20f + i * 30f, 9, 9322 + i, 0.9f);
            }
            else
            {
                // A rachadura por onde o verde entrou.
                Crack(c, mx + 9f, my - 9f, 100f, 14, 9323, 0.9f);
                Crack(c, mx + 10f, my - 2f, 40f, 6, 9324, 0.6f);
            }

            // O cinto de ouro, a fivela com a pedra.
            int beltY = (int)(hy - 8f);
            for (int x = (int)(hx - 14f); x <= (int)(hx + 14f); x++)
            {
                if (Get(c, x, beltY).A == 0) continue;
                Set(c, x, beltY, Gold[3]);
                Set(c, x, beltY + 1, Gold[2]);
                Set(c, x, beltY + 2, Gold[0]);
            }

            Ball(c, hx + 9f, beltY + 1f, 3f, 2.6f, Gold, 0.3f);
            Set(c, (int)hx + 9, beltY + 1, f.Corrupt ? Green : Velvet[3]);

            // A gola de arminho em volta dos ombros, com as pintas pretas.
            Ball(c, sx - 2f, sy - 1f, 13f, 6f, Ermine, 0.3f);
            for (int i = 0; i < 6; i++)
            {
                int px = (int)(sx - 11f + i * 4.5f);
                int py = (int)(sy - 2f + (i % 2) * 3f);
                if (Get(c, px, py).A > 0) Set(c, px, py, ErmineDot);
            }

            // A corrente de ouro que prende o manto, atravessando o peito.
            for (int i = 0; i <= 8; i++)
            {
                float t = i / 8f;
                float x = sx - 8f + t * 18f;
                float y = sy + 4f + (float)Math.Sin(t * Math.PI) * 5f;
                Set(c, (int)x, (int)y, i % 2 == 0 ? Gold[3] : Gold[1]);
            }

            Ball(c, sx + 10f, sy + 4f, 2.4f, 2.4f, Gold, 0.3f);
        }

        /// <summary>
        /// A cabeca: o rosto de prata esculpida, o cabelo penteado pra tras, a
        /// barba pontuda, o olho aceso e a coroa de quartzo.
        /// </summary>
        private static void DrawHead(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            float hx = f.HeadX;
            float hy = f.HeadY;
            ArtKit.Skin metal = MetalSkin(f, 9331);

            // O pescoco, alto.
            Taper(c, f.SX + 2f, f.SY - 2f, hx - 1f, hy + 6f, 5.5f, 4.5f, metal, 0f);

            // O cabelo de prata, penteado pra tras, caindo na nuca.
            Rgba[] hair = f.Corrupt ? SickHair : Hair;
            Ball(c, hx - 5f, hy - 1f, 8f, 9f, hair, -0.1f);
            Taper(c, hx - 8f, hy + 2f, hx - 11f, hy + 12f, 4f, 1.5f, hair, -0.2f);

            // O rosto: cranio, nariz reto, queixo.
            Ball(c, hx, hy, 9f, 10f, metal, 0.2f);
            Taper(c, hx + 6f, hy - 3f, hx + 11f, hy + 3f, 2f, 1.6f, metal, 0.4f);

            // A sobrancelha severa e o olho aceso.
            Line(c, (int)hx + 2, (int)hy - 4, (int)hx + 8, (int)hy - 3, f.Metal[0]);
            Rgba eye = pose.Eyes <= 0.1f ? f.Metal[0] : (f.Corrupt ? (pose.Eyes > 1.2f ? GreenCore : Green) : (pose.Eyes > 1.2f ? new Rgba(255, 255, 255) : new Rgba(170, 220, 255)));
            Fill(c, (int)hx + 3, (int)hy - 3, 5, 3, Dark);
            if (pose.Eyes > 0.1f) Fill(c, (int)hx + 4, (int)hy - 2, 3, 2, eye);
            if (pose.Eyes > 1.2f) Set(c, (int)hx + 6, (int)hy - 2, f.Corrupt ? GreenCore : new Rgba(255, 255, 255));

            // A barba pontuda e o bigode, de prata esculpida.
            Taper(c, hx + 2f, hy + 6f, hx + 6f, hy + 17f, 5f, 0.6f, hair, 0.2f);
            Ball(c, hx + 6f, hy + 5f, 4f, 1.6f, hair, 0.4f);
            for (int i = 0; i < 3; i++) Set(c, (int)hx + 3 + i, (int)hy + 9 + i * 2, hair[0]);

            // A boca: uma fenda; aberta no sopro e no grito.
            if (pose.Mouth > 0.05f)
            {
                float open = 1f + pose.Mouth * 3f;
                Ball(c, f.MouthX - 1f, f.MouthY, 2.4f, open, new[] { Dark, Dark, Dark, Dark });
                if (f.Corrupt) Set(c, (int)f.MouthX - 1, (int)f.MouthY, Green);
            }

            if (f.Corrupt) Crack(c, hx - 2f, hy - 8f, 70f, 12, 9332, 0.85f);

            _lastMouth = (f.MouthX + 2f, f.MouthY);

            if (!pose.CrownOff) DrawCrown(f, hx, hy - 9f);
        }

        /// <summary>A coroa: a tiara de ouro com as pedras e as cinco pontas de quartzo facetado.</summary>
        private static void DrawCrown(Frame f, float cx, float cy)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            Rgba[] quartz = f.Corrupt ? SickQuartz : Quartz;
            _lastCrown = (cx + 1f, cy - 12f);

            // As pontas, de tras pra frente: a do meio e a mais alta.
            float[] heights = { 9f, 13f, 17f, 13f, 9f };
            for (int i = 0; i < heights.Length; i++)
            {
                float px = cx - 9f + i * 4.6f;
                float h = heights[i];

                // Rachada: uma ponta partida pela metade.
                if (pose.CrownCrack > 0.5f && i == 3) h *= 0.45f;
                if (f.Corrupt && i == 1) h *= 0.6f;

                Polygon(c, new[] { (px - 2.6f, cy), (px, cy - h), (px + 2.6f, cy) },
                    (x, y) => ArtKit.Tone(quartz, (px - x) * 0.25f + 0.3f + (cy - y) / h * 0.4f));
                Set(c, (int)px, (int)(cy - h + 1f), quartz[3]);
            }

            // A tiara, com as pedras.
            for (int x = (int)(cx - 11f); x <= (int)(cx + 11f); x++)
            {
                Set(c, x, (int)cy, Gold[3]);
                Set(c, x, (int)cy + 1, Gold[2]);
                Set(c, x, (int)cy + 2, Gold[1]);
                Set(c, x, (int)cy + 3, Gold[0]);
            }

            for (int i = 0; i < 4; i++)
            {
                int gx = (int)(cx - 8f + i * 5.5f);
                Set(c, gx, (int)cy + 1, i % 2 == 0 ? Velvet[3] : new Rgba(90, 150, 240));
                Set(c, gx + 1, (int)cy + 1, i % 2 == 0 ? Velvet[2] : new Rgba(50, 100, 200));
            }

            if (pose.CrownCrack > 0f || f.Corrupt)
            {
                Line(c, (int)cx + 3, (int)cy - 10, (int)cx + 5, (int)cy + 3, f.Corrupt ? Green : Dark);
            }
        }

        private static void DrawFallenCrown(Frame f, float x, float y)
        {
            PixelCanvas c = f.Canvas;
            Rgba[] quartz = f.Corrupt ? SickQuartz : Quartz;
            for (int i = 0; i < 4; i++)
            {
                float px = x - 7f + i * 4.6f;
                float h = i == 2 ? 4f : 8f - i;
                Polygon(c, new[] { (px - 2.4f, y), (px - 1f + i * 0.5f, y - h), (px + 2.4f, y) }, (cx, cy) => ArtKit.Tone(quartz, 0.4f));
            }

            for (int px = (int)x - 9; px <= (int)x + 9; px++)
            {
                Set(c, px, (int)y, Gold[2]);
                Set(c, px, (int)y + 1, Gold[0]);
            }
        }

        /// <summary>O cetro: a vara de ouro, os aneis, e o cristal de quartzo na ponta.</summary>
        private static void DrawScepter(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            (float bx, float by) = ArtKit.Along(f.HandX, f.HandY, pose.Scepter, -10f);
            (float tx, float ty) = ArtKit.Along(f.HandX, f.HandY, pose.Scepter, 34f);

            Limb(c, bx, by, tx, ty, 1.6f, Gold, 0.2f);
            for (int i = 0; i < 3; i++)
            {
                (float rx, float ry) = ArtKit.Along(f.HandX, f.HandY, pose.Scepter, 8f + i * 9f);
                Ball(c, rx, ry, 2.4f, 2.4f, Gold, 0.4f);
            }

            Ball(c, bx, by, 2.4f, 2.4f, Gold, 0.2f);

            // O cristal: um prisma de quartzo de duas pontas, numa garra de ouro.
            (float cx, float cy) = ArtKit.Along(tx, ty, pose.Scepter, 5f);
            Rgba[] quartz = f.Corrupt ? SickQuartz : Quartz;
            (float px, float py) = ArtKit.Along(cx, cy, pose.Scepter, 7f);
            (float qx, float qy) = ArtKit.Along(cx, cy, pose.Scepter, -4f);
            (float lx, float ly) = ArtKit.Along(cx, cy, pose.Scepter + 90f, 4f);
            (float rx2, float ry2) = ArtKit.Along(cx, cy, pose.Scepter - 90f, 4f);
            Polygon(c, new[] { (px, py), (lx, ly), (qx, qy), (rx2, ry2) },
                (x, y) => ArtKit.Tone(quartz, (cx - x) * 0.2f + (cy - y) * 0.2f + 0.3f));
            Taper(c, tx, ty, lx, ly, 1f, 0.4f, Gold, 0.3f);
            Taper(c, tx, ty, rx2, ry2, 1f, 0.4f, Gold, 0.3f);
            Set(c, (int)cx, (int)cy, quartz[3]);

            _lastTip = (px, py);
        }

        private static void DrawDroppedScepter(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float x0 = f.HX + 12f;
            Limb(c, x0, Ground - 2f, x0 + 40f, Ground - 3f, 1.6f, Gold, 0.1f);
            Rgba[] quartz = f.Corrupt ? SickQuartz : Quartz;
            Polygon(c, new[] { (x0 + 52f, Ground - 4f), (x0 + 46f, Ground - 8f), (x0 + 40f, Ground - 4f), (x0 + 46f, Ground - 1f) },
                (x, y) => ArtKit.Tone(quartz, 0.3f));
        }

        /// <summary>O braco livre, de tras: ombreira, manga de cota, a manopla.</summary>
        private static void DrawBackArm(Frame f)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;
            float sx = f.SX - 6f;
            float sy = f.SY + 3f;
            (float ex, float ey) = ArtKit.Along(sx, sy, pose.BackArm, 17f);
            (float hx, float hy) = ArtKit.Along(ex, ey, pose.BackFore, 16f);
            ArtKit.Skin metal = MetalSkin(f, 9341);

            Taper(c, sx, sy, ex, ey, 5.5f, 4.5f, metal, -0.45f);
            Ball(c, ex, ey, 4.5f, 4.5f, f.Metal, -0.3f);
            Taper(c, ex, ey, hx, hy, 4.5f, 3.5f, metal, -0.4f);
            Ball(c, hx, hy, 4f, 3.6f, f.Metal, -0.3f);

            // Manchas de azinhavre na ombreira de tras: por onde o Oxide comecou.
            if (!f.Corrupt) Ball(c, sx + 1f, sy - 2f, 4f, 3f, Patina, -0.2f);
        }

        /// <summary>O braco do cetro: a ombreira de tres laminas, a cota, a manopla fechada no cetro.</summary>
        private static void DrawFrontArm(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float fsx = f.SX + 4f;
            float fsy = f.SY + 4f;
            ArtKit.Skin metal = MetalSkin(f, 9351);

            Taper(c, fsx, fsy, f.ElbowX, f.ElbowY, 5.5f, 4.5f, metal, 0.1f);
            Ball(c, f.ElbowX, f.ElbowY, 4.6f, 4.6f, f.Metal, 0.3f);
            Taper(c, f.ElbowX, f.ElbowY, f.HandX, f.HandY, 4.6f, 3.6f, metal, 0.15f);

            // A ombreira em tres laminas, com o filete de ouro.
            for (int i = 2; i >= 0; i--)
            {
                Ball(c, fsx - 1f, fsy - 3f + i * 4f, 9f - i, 4.5f, metal, 0.35f - i * 0.1f);
                for (int x = (int)(fsx - 8f + i); x <= (int)(fsx + 6f - i); x++)
                {
                    int y = (int)(fsy + i * 4f + 1f);
                    if (Get(c, x, y).A > 0) Set(c, x, y, Gold[1]);
                }
            }

            Ball(c, f.HandX, f.HandY, 4.6f, 4.2f, f.Metal, 0.25f);
            Set(c, (int)f.HandX + 1, (int)f.HandY - 2, Gold[2]);
        }

        /// <summary>Deitado de costas, a coroa e o cetro no chao, a prata apagada.</summary>
        private static void DrawLying(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float g = Ground;
            Rgba[] ramp = f.Corrupt ? RotVelvet : Velvet;
            ArtKit.Skin metal = MetalSkin(f, 9361);

            // O manto espalhado por baixo.
            Polygon(c, new[] { (24f, g - 1f), (150f, g - 1f), (140f, g - 8f), (40f, g - 10f) }, (x, y) => ArtKit.Tone(ramp, -0.1f));

            // As pernas, os sapatos de bico.
            Taper(c, 84f, g - 9f, 52f, g - 7f, 6f, 4.5f, metal, 0f);
            Taper(c, 52f, g - 8f, 40f, g - 14f, 3.6f, 0.6f, f.Metal, 0.1f);

            // O corpo e a gola.
            Ball(c, 100f, g - 11f, 22f, 9f, metal, 0.1f);
            Ball(c, 122f, g - 12f, 7f, 6f, Ermine, 0.2f);
            if (f.Corrupt)
            {
                Ball(c, 104f, g - 15f, 4f, 3f, new[] { Dark, Dark, Patina[0], Patina[1] }, 0f);
                if (f.Pose.Chest > 0.1f) Fill(c, 103, (int)g - 16, 2, 2, Patina[2]);
            }

            // A cabeca de lado, a barba pro alto.
            Ball(c, 132f, g - 11f, 9f, 9f, metal, 0.15f);
            Taper(c, 136f, g - 16f, 146f, g - 22f, 4f, 0.6f, f.Metal, 0.2f);
            Line(c, 132, (int)g - 15, 136, (int)g - 15, f.Metal[0]);

            DrawFallenCrown(f, 160f, g - 3f);
            DrawDroppedScepter(new Frame { Canvas = c, Pose = f.Pose, Corrupt = f.Corrupt, HX = -8f });
        }

        // ------------------------------------------------------------------
        // Efeitos
        // ------------------------------------------------------------------

        private static void DrawGlows(Frame f)
        {
            Pose pose = f.Pose;
            if (!pose.CrownOff && pose.CrownGlow > 0.1f)
            {
                Glow(f.Canvas, f.HeadX, f.HeadY - 18f, 14f, f.Corrupt ? Green : Light, 0.25f * pose.CrownGlow, true);
            }

            if (!pose.ScepterDropped && pose.ScepterGlow > 0.5f)
            {
                Glow(f.Canvas, _lastTip.X, _lastTip.Y, 6f + pose.ScepterGlow * 3f, f.Corrupt ? Green : Light, 0.35f * pose.ScepterGlow, true);
            }

            if (f.Corrupt && pose.Chest > 0.05f)
            {
                float mx = (f.HX + f.SX) * 0.5f - 6f;
                float my = (f.HY + f.SY) * 0.5f - 4f;
                Glow(f.Canvas, mx, my, 12f, Green, 0.3f * pose.Chest, true);
            }

            // Os raios da prata (fase 1): finos, saindo das bordas do corpo.
            if (!f.Corrupt && pose.Rays > 0.1f)
            {
                for (int i = 0; i < 8; i++)
                {
                    double a = -Math.PI * 0.9 + i * Math.PI * 0.8 / 7.0;
                    float r0 = 30f;
                    float r1 = 30f + pose.Rays * 14f;
                    float cx = f.SX;
                    float cy = f.SY + 4f;
                    Line(f.Canvas, (int)(cx + Math.Cos(a) * r0), (int)(cy + Math.Sin(a) * r0),
                        (int)(cx + Math.Cos(a) * r1), (int)(cy + Math.Sin(a) * r1), Light.WithAlpha((int)(60 + pose.Rays * 60f)));
                }
            }
        }

        /// <summary>O golpe pro alto: uma coluna de luz subindo da ponta do cetro ate o teto.</summary>
        private static void DrawPillar(Frame f)
        {
            float k = f.Pose.Pillar;
            if (k <= 0f) return;

            Rgba tint = f.Corrupt ? Green : Light;
            int x0 = (int)_lastTip.X;
            for (int y = 0; y < (int)_lastTip.Y; y++)
            {
                for (int dx = -5; dx <= 5; dx++)
                {
                    float edge = Math.Abs(dx) / 5f;
                    int alpha = (int)(k * (220 - edge * 170));
                    if (alpha < 30) continue;
                    Blend(f.Canvas, x0 + dx, y, (Math.Abs(dx) <= 1 ? new Rgba(255, 255, 255) : tint).WithAlpha(Math.Min(255, alpha)));
                }
            }

            Glow(f.Canvas, _lastTip.X, _lastTip.Y, 14f, tint, 0.6f * k, true);
            for (int i = 0; i < 4; i++) Spark(f.Canvas, x0 - 8 + i * 5, (int)_lastTip.Y - 10 - i * 14, 2);
        }

        private static void DrawSmear(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.SmearFrom == pose.SmearTo) return;
            float from = Math.Min(pose.SmearFrom, pose.SmearTo);
            float to = Math.Max(pose.SmearFrom, pose.SmearTo);
            float cx = f.SX + 4f;
            float cy = f.SY + 4f;
            Rgba tint = f.Corrupt ? new Rgba(170, 255, 200) : new Rgba(255, 246, 210);

            for (int py = 0; py < DesignHeight; py++)
            {
                for (int px = 0; px < DesignWidth; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r < 50f || r > 76f) continue;

                    float a = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    if (a < from || a > to) continue;

                    float k = (a - from) / Math.Max(1f, to - from);
                    float edge = (r - 50f) / 26f;
                    int alpha = (int)(30 + 170 * k * edge);
                    if (alpha < 35) continue;
                    Blend(f.Canvas, px, py, tint.WithAlpha(alpha));
                }
            }
        }

        /// <summary>O sopro: a nevoa verde saindo da boca, rente pra frente.</summary>
        private static void DrawMist(Frame f)
        {
            float mist = f.Pose.Mist;
            if (mist <= 0f) return;

            for (int i = 0; i < 6; i++)
            {
                float x = f.MouthX + 6f + i * 9f * mist;
                float y = f.MouthY + 4f + i * 5f;
                Puff(f.Canvas, x, y, 4f + i * 1.4f, new Rgba(150, 240, 180), new Rgba(50, 130, 90), 170 - i * 18);
            }
        }

        /// <summary>Flutuando: fiapos de Oxide escorrendo dos pes.</summary>
        private static void DrawWisps(Frame f)
        {
            if (!f.Corrupt || f.Float <= 0f) return;

            for (int i = 0; i < 5; i++)
            {
                float x = f.HX - 10f + i * 6f;
                float y = Ground - 3f - (i % 2) * 3f;
                Puff(f.Canvas, x, y, 4f + (i % 3), new Rgba(130, 230, 160), new Rgba(40, 110, 76), 110);
            }
        }

        private static void DrawDust(Frame f)
        {
            float dust = f.Pose.Dust;
            if (dust <= 0f) return;

            var light = new Rgba(220, 222, 236);
            var dark = new Rgba(150, 154, 176);
            float x = BodyX + f.Pose.X + SweepReach * PixelsPerUnit / Scale;
            for (int i = 0; i < 3; i++)
            {
                float r = 3f + dust * 2f - i * 0.5f;
                Puff(f.Canvas, x - 10f + i * 10f, Ground - r * 0.6f - dust, r, light, dark, dust > 1.5f ? 120 : 190);
            }
        }

        // ------------------------------------------------------------------
        // As pecas soltas: as facetas, a fumaca e o brilho (16 px por tile)
        // ------------------------------------------------------------------

        /// <summary>Uma faceta de luz da coroa, 14x14: um losango de quartzo aceso.</summary>
        public static PixelCanvas Facet(bool corrupt)
        {
            var c = new PixelCanvas(14, 14);
            Rgba[] ramp = corrupt ? new[] { Patina[1], Green.Scaled(0.7f), Green, GreenCore } : Quartz;
            ArtKit.Polygon(c, new[] { (7f, 0f), (14f, 7f), (7f, 14f), (0f, 7f) }, (x, y) => ArtKit.Tone(ramp, 0.6f - (x + y) / 18f));
            c.Set(5, 5, new Rgba(255, 255, 255));
            ArtKit.Glow(c, 7f, 7f, 7f, corrupt ? Green : Light, 0.6f, true);
            return c;
        }

        /// <summary>Uma baforada de Oxide, 48x40: o pedaco da nuvem do sopro, translucida.</summary>
        public static PixelCanvas Puff(int seed)
        {
            var c = new PixelCanvas(48, 40);
            for (int i = 0; i < 6; i++)
            {
                float px = 12f + ArtKit.Hash(i, seed, 9201) % 24;
                float py = 12f + ArtKit.Hash(i, seed, 9202) % 16;
                ArtKit.Puff(c, px, py, 9f + i % 3 * 2f, new Rgba(130, 230, 160), new Rgba(40, 110, 76), 110);
            }

            return c;
        }

        /// <summary>O brilho que avisa onde a faceta vai cair, 16x16.</summary>
        public static PixelCanvas Glint()
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Spark(c, 8, 8, 6);
            ArtKit.Glow(c, 8f, 8f, 8f, Green, 0.8f, true);
            return c;
        }
    }
}
