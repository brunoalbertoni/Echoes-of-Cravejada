using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
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
}
