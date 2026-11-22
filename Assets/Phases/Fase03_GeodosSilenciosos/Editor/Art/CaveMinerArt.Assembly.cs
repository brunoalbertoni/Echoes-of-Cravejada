using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class CaveMinerArt
    {
        // ------------------------------------------------------------------
        // Montagem
        // ------------------------------------------------------------------

        private sealed class Frame
        {
            public PixelCanvas Canvas = new PixelCanvas(Width, Height);
            public Pose Pose;
            public Rgba[] Skin;
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
            public float MouthX => HeadX + 10f;
            public float MouthY => HeadY + 6f;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var f = new Frame { Pose = pose, Skin = pose.Freed ? Human : Skin };

            if (pose.Lying)
            {
                DrawLying(f);
                ArtKit.AddOutline(f.Canvas);
                DrawCrystalDust(f, 70f, Ground - 14f);
                return f.Canvas;
            }

            f.HX = BodyX + pose.X + pose.Shake;
            f.HY = HipY + pose.Y + pose.Kneel * 14f;
            float torso = -90f + pose.Lean;
            (f.SX, f.SY) = ArtKit.Along(f.HX, f.HY, torso, 30f);
            (f.HeadX, f.HeadY) = ArtKit.Along(f.SX, f.SY, -90f + pose.Lean * 1.3f + 44f + pose.Head, 17f);

            float fsx = f.SX + 6f;
            float fsy = f.SY + 4f;
            (f.ElbowX, f.ElbowY) = ArtKit.Along(fsx, fsy, pose.Arm, 17f);
            (f.HandX, f.HandY) = ArtKit.Along(f.ElbowX, f.ElbowY, pose.Fore, 15f);
            if (pose.Clutch)
            {
                // A mao no peito, onde fica a rachadura.
                f.HandX = f.SX + 10f;
                f.HandY = f.SY + 12f;
                (f.ElbowX, f.ElbowY) = ArtKit.Joint(fsx, fsy, f.HandX, f.HandY, 17f, 15f, -1);
            }

            if (!pose.PickDropped && pose.PickBehind) DrawPick(f);
            if (!pose.Whistle) DrawBackArm(f);
            DrawLeg(f, true);
            DrawLeg(f, false);
            DrawTorso(f);
            DrawHead(f);
            if (pose.Whistle) DrawBackArm(f);
            if (!pose.PickDropped && !pose.PickBehind) DrawPick(f);
            if (pose.PickDropped) DrawDroppedPick(f);
            if (pose.HelmetOff) DrawFallenHelmet(f);
            DrawFrontArm(f);

            ArtKit.AddOutline(f.Canvas);

            DrawGlows(f);
            DrawSmear(f);
            DrawRings(f);
            DrawDust(f);
            DrawStars(f);
            DrawCrystalDust(f, f.SX, f.SY);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(f.Canvas, spark.X, spark.Y, spark.Size);
            return f.Canvas;
        }

        private static void DrawLeg(Frame f, bool back)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;
            float hipX = f.HX + (back ? -5f : 5f);
            float hipY = f.HY + 2f;
            float footX = BodyX + pose.X * 0.4f + (back ? -8f + pose.BackFoot : 9f + pose.FrontFoot);
            float footY = Ground - 4f - (back ? pose.BackLift : pose.FrontLift);
            float bias = back ? -0.45f : 0f;

            float kx, ky;
            if (back && pose.Kneel > 0.5f)
            {
                kx = hipX + 6f;
                ky = Ground - 5f;
                footX = kx - 20f;
                footY = Ground - 4f;
            }
            else
            {
                if (!back && pose.Kneel > 0.5f) footX = f.HX + 16f;
                (kx, ky) = ArtKit.Joint(hipX, hipY, footX, footY, 22f, 22f, -1);
            }

            ArtKit.Skin cloth = ArtKit.Corroded(Trousers, 0.06f, back ? 5201 : 5202, 0.25f, Coat);
            ArtKit.Taper(c, hipX, hipY, kx, ky, 7f, 6f, cloth, bias);
            ArtKit.Taper(c, kx, ky, footX, footY - 3f, 6f, 5f, cloth, bias);

            // O remendo no joelho.
            ArtKit.Ball(c, kx, ky, 4f, 3.6f, Patch, bias + 0.1f);
            c.Set((int)kx - 3, (int)ky - 3, Thread);
            c.Set((int)kx + 2, (int)ky + 3, Thread);

            // A bota de couro, o cano dobrado e a biqueira de ferro.
            float bootX = footX + 3f;
            ArtKit.Drum(c, footX, footY - 4f, 6f, 4f, 3f, ArtKit.Plain(ArtKit.Leather), bias + 0.1f);
            ArtKit.Drum(c, bootX, footY + 1f, 9f, 4f, 3f, ArtKit.Plain(ArtKit.Leather), bias);
            ArtKit.Ball(c, bootX + 6f, footY + 1f, 3.5f, 3.2f, ArtKit.Iron, bias + 0.2f);
            c.Fill((int)(bootX - 8f), (int)(footY + 4f), 18, 1, ArtKit.Leather[0]);
            ArtKit.Rivet(c, (int)bootX - 3, (int)footY - 1, ArtKit.Brass);
        }

        /// <summary>
        /// O tronco: o casacao com a barra esfarrapada, o remendo, os botoes, o
        /// cinto com a fivela, as chaves e a corda; a corcunda furada de cristal;
        /// a rachadura verde nas costelas.
        /// </summary>
        private static void DrawTorso(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            float hx = f.HX;
            float hy = f.HY;
            float sx = f.SX;
            float sy = f.SY;
            float mx = (hx + sx) * 0.5f;
            float my = (hy + sy) * 0.5f;
            float torso = -90f + pose.Lean;

            ArtKit.Skin coat = ArtKit.Corroded(Coat, 0.07f, 5211, 0.22f, Patch);

            // A corcunda e os cristais que furaram o casaco, nas costas.
            if (!pose.Freed)
            {
                foreach ((float angle, float length, float half, Rgba[] ramp) in new[]
                {
                    (torso - 62f, 16f, 3.6f, Amethyst), (torso - 38f, 21f, 4.2f, Amethyst), (torso - 18f, 13f, 3f, Milky),
                })
                {
                    float bx = sx - 11f;
                    float by = sy + 3f;
                    (float tx, float ty) = ArtKit.Along(bx, by, angle, length);
                    ArtKit.Taper(c, bx, by, tx, ty, half, 0.5f, ramp, 0.25f);
                }
            }
            else
            {
                // So os tocos, partidos.
                ArtKit.Taper(c, sx - 11f, sy + 3f, sx - 15f, sy - 3f, 3f, 2.2f, Amethyst, -0.2f);
            }

            // A barra do casacao, comprida, esfarrapada.
            float tail = -pose.Lean * 0.25f;
            var hem = new List<(float, float)> { (hx - 15f, hy - 8f), (hx + 13f, hy - 8f), (hx + 14f + tail * 0.3f, hy + 12f) };
            for (int i = 0; i <= 8; i++)
            {
                float x = hx + 14f - i * 3.8f + tail * (i / 8f);
                hem.Add((x, hy + 16f + (i % 2 == 0 ? 6f : 1f) + (i % 3) - pose.Kneel * 6f));
            }

            hem.Add((hx - 18f + tail, hy + 10f));
            ArtKit.Polygon(c, hem.ToArray(), (x, y) =>
            {
                float light = 0.1f - (x - hx) * 0.03f - (y - hy) * 0.03f;
                return ArtKit.Tone(Coat, light);
            });

            // O corpo: quadril, barriga, peito largo.
            ArtKit.Ball(c, hx - 1f, hy - 6f, 15f, 12f, coat, -0.1f);
            ArtKit.Ball(c, mx + 1f, my, 18f, 14f, coat, 0f);
            ArtKit.Ball(c, sx + 1f, sy + 5f, 19f, 13f, coat, 0.15f);

            // O remendo costurado no ombro.
            int px0 = (int)(sx - 6f);
            int py0 = (int)(sy - 2f);
            for (int y = py0; y < py0 + 8; y++)
            {
                for (int x = px0; x < px0 + 9; x++)
                {
                    if (c.Get(x, y).A == 0) continue;
                    float light = y == py0 ? 0.6f : (x == px0 + 8 || y == py0 + 7 ? -0.5f : 0f);
                    c.Set(x, y, ArtKit.Tone(Patch, light));
                }
            }

            for (int i = 0; i < 9; i += 2)
            {
                c.Set(px0 + i, py0 - 1, Thread);
                c.Set(px0 + i, py0 + 8, Thread);
            }

            // A lapela e os botoes de latao, descendo pela frente.
            (float lx0, float ly0) = (sx + 13f, sy + 2f);
            (float lx1, float ly1) = (hx + 12f, hy - 6f);
            ArtKit.Line(c, (int)lx0, (int)ly0, (int)lx1, (int)ly1, Coat[0]);
            ArtKit.Line(c, (int)lx0 - 1, (int)ly0, (int)lx1 - 1, (int)ly1, Coat[3]);
            for (int i = 1; i <= 3; i++)
            {
                float t = i / 4f;
                ArtKit.Rivet(c, (int)(lx0 + (lx1 - lx0) * t) - 4, (int)(ly0 + (ly1 - ly0) * t), ArtKit.Brass);
            }

            // O cinto, a fivela.
            int beltY = (int)(hy - 9f);
            for (int x = (int)(hx - 18f); x <= (int)(hx + 18f); x++)
            {
                if (c.Get(x, beltY).A == 0) continue;
                c.Set(x, beltY, ArtKit.Leather[2]);
                c.Set(x, beltY + 1, ArtKit.Leather[1]);
                c.Set(x, beltY + 2, ArtKit.Leather[0]);
            }

            c.Fill((int)hx + 5, beltY - 1, 5, 5, ArtKit.Brass[2]);
            c.Fill((int)hx + 6, beltY, 3, 3, ArtKit.Leather[1]);
            c.Set((int)hx + 5, beltY - 1, ArtKit.Brass[3]);

            // A corda enrolada no quadril, atras.
            for (int i = 0; i < 3; i++) ArtKit.Ball(c, hx - 12f + i, beltY + 6f + i * 1.5f, 6f - i, 3.2f, Rope, 0.1f);
            ArtKit.Line(c, (int)hx - 9, beltY + 10, (int)hx - 8, beltY + 16, Rope[1]);

            // A argola de chaves, pendurada na frente do cinto.
            DrawKeys(c, hx + 13f, beltY + 4f);

            // A rachadura verde nas costelas.
            ArtKit.Crack(c, mx + 6f, my - 4f, 100f, 11, 5212, pose.Freed ? 0f : 0.85f);
            if (!pose.Freed) ArtKit.Crack(c, mx + 8f, my - 2f, 50f, 6, 5213, 0.6f);

            // O lenco vermelho no pescoco, com o no caindo na frente.
            ArtKit.Ball(c, sx + 7f, sy - 2f, 8f, 4.5f, Red, 0.2f);
            ArtKit.Polygon(c, new[] { (sx + 9f, sy + 1f), (sx + 15f, sy + 1f), (sx + 12f, sy + 9f) },
                (x, y) => ArtKit.Tone(Red, 0.3f - (y - sy) * 0.08f));

            // O apito de latao na corrente.
            if (!pose.Whistle)
            {
                for (int i = 0; i < 6; i++) c.Set((int)(sx + 4f + i * 0.6f), (int)(sy + 2f + i * 1.6f), i % 2 == 0 ? ArtKit.Brass[2] : ArtKit.Iron[2]);
                ArtKit.Drum(c, sx + 8f, sy + 12f, 3f, 1.6f, 3f, ArtKit.Plain(ArtKit.Brass), 0.3f);
                c.Set((int)(sx + 10f), (int)(sy + 11f), ArtKit.Brass[3]);
            }
        }

        /// <summary>A argola de ferro com tres chaves: a ultima, grande e trabalhada, e a do salao.</summary>
        private static void DrawKeys(PixelCanvas c, float x, float y)
        {
            for (int a = 0; a < 16; a++)
            {
                double t = a * Math.PI / 8.0;
                c.Set((int)Math.Round(x + Math.Cos(t) * 2.6), (int)Math.Round(y + Math.Sin(t) * 2.6), a < 8 ? ArtKit.Iron[3] : ArtKit.Iron[1]);
            }

            for (int k = 0; k < 3; k++)
            {
                int kx = (int)x - 2 + k * 2;
                int length = k == 2 ? 7 : 5;
                for (int i = 0; i < length; i++) c.Set(kx, (int)y + 3 + i, k == 2 ? ArtKit.Rust[2] : ArtKit.Iron[2]);
                c.Set(kx + 1, (int)y + 2 + length, k == 2 ? ArtKit.Rust[1] : ArtKit.Iron[1]);
            }
        }

        /// <summary>
        /// A cabeca: o rosto cinza com os olhos virados placa de quartzo, a
        /// barba grisalha, o nariz grande; o capacete pequeno demais, rachado,
        /// furado de cristal, com a safira no lugar da lanterna.
        /// </summary>
        private static void DrawHead(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            Rgba[] skin = f.Skin;
            float hx = f.HeadX;
            float hy = f.HeadY;

            // O pescoco grosso.
            ArtKit.Taper(c, f.SX + 4f, f.SY - 1f, hx - 1f, hy + 6f, 7f, 6f, skin, -0.1f);
            ArtKit.Ball(c, f.SX + 7f, f.SY - 2f, 8f, 4.5f, Red, 0.2f);

            // Os cristais que brotam da nuca (o capacete cobre a base deles).
            if (!pose.Freed)
            {
                ArtKit.Taper(c, hx - 6f, hy - 3f, hx - 16f, hy - 8f, 2.8f, 0.4f, Amethyst, 0.25f);
                ArtKit.Taper(c, hx - 6f, hy + 2f, hx - 14f, hy + 2f, 2.2f, 0.4f, Milky, 0.2f);
            }

            // O cranio e a orelha.
            ArtKit.Ball(c, hx, hy, 9.5f, 10f, skin, 0.15f);
            ArtKit.Ball(c, hx - 4f, hy + 1f, 2.6f, 3.6f, skin, -0.1f);
            c.Set((int)hx - 4, (int)hy + 1, skin[0]);

            if (pose.HelmetOff)
            {
                // Careca, com uns fios ralos; e os tocos dos cristais, se ainda houver.
                for (int i = 0; i < 4; i++) c.Set((int)hx - 4 + i * 3, (int)hy - 10 + (i % 2), Beard[2]);
                if (!pose.Freed)
                {
                    ArtKit.Taper(c, hx - 2f, hy - 8f, hx - 5f, hy - 13f, 2.2f, 1.4f, Amethyst, -0.2f);
                    ArtKit.Taper(c, hx + 4f, hy - 9f, hx + 6f, hy - 14f, 2.2f, 1.4f, Milky, -0.2f);
                }
            }

            // O nariz grande e a sobrancelha pesada.
            ArtKit.Ball(c, hx + 9f, hy + 1f, 3f, 2.8f, skin, 0.25f);
            ArtKit.Line(c, (int)hx + 3, (int)hy - 4, (int)hx + 8, (int)hy - 3, skin[0]);

            // O olho: placa de quartzo leitoso (cego) — ou a palpebra fechada, em paz.
            int ex = (int)(hx + 4f);
            int ey = (int)(hy - 2f);
            if (pose.Freed)
            {
                ArtKit.Line(c, ex - 1, ey + 1, ex + 2, ey + 1, skin[0]);
                c.Set(ex, ey + 2, skin[1]);
            }
            else
            {
                c.Fill(ex - 1, ey - 1, 5, 4, Milky[1]);
                c.Fill(ex - 1, ey - 1, 3, 1, Milky[3]);
                c.Set(ex + 3, ey + 2, Milky[0]);
                c.Set(ex + 1, ey + 1, Milky[2]);
                c.Set(ex + 2, ey, Milky[0]);
                ArtKit.Taper(c, hx + 6f, hy + 1f, hx + 9f, hy - 5f, 1.4f, 0.3f, Amethyst, 0.3f);
            }

            // A barba grisalha, cheia, e o bigode.
            ArtKit.Ball(c, hx + 3f, hy + 10f, 7.5f, 6f, Beard, 0.05f);
            ArtKit.Taper(c, hx + 1f, hy + 13f, hx - 1f, hy + 19f, 3.2f, 0.6f, Beard, -0.1f);
            ArtKit.Taper(c, hx + 6f, hy + 13f, hx + 6f, hy + 20f, 3.2f, 0.6f, Beard, 0.05f);
            ArtKit.Taper(c, hx + 9f, hy + 11f, hx + 11f, hy + 16f, 2.4f, 0.5f, Beard, 0.15f);
            for (int i = 0; i < 4; i++) c.Set((int)hx + 1 + i * 3, (int)hy + 9 + (i % 2) * 2, Beard[0]);

            // A boca abre por dentro da barba.
            if (pose.Jaw > 0.05f)
            {
                float mx = hx + 8f;
                float my = hy + 8f + pose.Jaw * 1.5f;
                ArtKit.Ball(c, mx, my, 2.4f + pose.Jaw * 1.4f, 1f + pose.Jaw * 3.2f, new[] { Mouth, Mouth, Mouth, Mouth });
                c.Fill((int)(mx - 2f), (int)(my - pose.Jaw * 3f), 4, 1, Teeth);
            }

            ArtKit.Ball(c, hx + 8f, hy + 5.5f, 4.2f, 1.8f, Beard, 0.3f);

            // O nariz por cima do bigode.
            ArtKit.Ball(c, hx + 10f, hy + 2f, 2.8f, 2.6f, skin, 0.3f);

            if (!pose.HelmetOff) DrawHelmet(f, hx, hy);
        }

        private static void DrawHelmet(Frame f, float hx, float hy)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            ArtKit.Skin brass = ArtKit.Corroded(ArtKit.Brass, 0.4f, 5221, 0.25f, ArtKit.Rust);

            // A cupula, pequena demais pra cabeca, rachada.
            float cx = hx + 1f;
            float cy = hy - 9f;
            ArtKit.Ball(c, cx, cy, 10.5f, 6.5f, brass, 0.2f);

            int bandY = (int)cy + 2;
            for (int x = (int)(cx - 10f); x <= (int)(cx + 10f); x++)
            {
                if (c.Get(x, bandY).A == 0) continue;
                c.Set(x, bandY, ArtKit.Iron[2]);
                c.Set(x, bandY + 1, ArtKit.Iron[1]);
                if (x % 4 == 0) c.Set(x, bandY, ArtKit.Iron[3]);
            }

            for (int x = (int)(cx - 12f); x <= (int)(cx + 13f); x++)
            {
                c.Set(x, (int)cy + 5, ArtKit.Brass[2]);
                c.Set(x, (int)cy + 6, ArtKit.Brass[0]);
            }

            // A rachadura e os cristais furando por ela.
            ArtKit.Line(c, (int)cx + 1, (int)cy - 6, (int)cx + 3, (int)cy + 1, Mouth);
            ArtKit.Taper(c, cx + 1f, cy - 4f, cx - 6f, cy - 13f, 2.6f, 0.4f, Amethyst, 0.2f);

            // O soquete da lanterna, com a safira cravada.
            float gx = cx + 10f;
            float gy = cy;
            ArtKit.Drum(c, gx, gy, 3.6f, 4.4f, 3f, ArtKit.Plain(ArtKit.Brass), 0.3f);
            if (!pose.NoGem)
            {
                ArtKit.Polygon(c, new[] { (gx, gy - 3.5f), (gx + 3f, gy), (gx, gy + 3.5f), (gx - 2.5f, gy) }, (x, y) =>
                    ArtKit.Tone(Sapphire, (gx - x) * 0.3f + (gy - y) * 0.35f + 0.1f * pose.Gem));
                c.Set((int)gx - 1, (int)gy - 2, Sapphire[3]);
            }
            else
            {
                c.Fill((int)gx - 1, (int)gy - 2, 3, 4, Mouth);
            }
        }

        /// <summary>O capacete caido no chao, na frente dele, de boca pra cima.</summary>
        private static void DrawFallenHelmet(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float x = f.HX + 34f;
            float y = Ground - 5f;
            ArtKit.Ball(c, x, y, 10f, 5f, ArtKit.Corroded(ArtKit.Brass, 0.3f, 5222, 0.3f), 0.1f);
            for (int i = -11; i <= 11; i++) c.Set((int)x + i, (int)y - 4, ArtKit.Brass[1]);
            ArtKit.Drum(c, x + 9f, y - 1f, 3f, 3.5f, 3f, ArtKit.Plain(ArtKit.Brass), 0.2f);
            c.Fill((int)x + 8, (int)y - 2, 2, 3, Mouth);
            ArtKit.Taper(c, x - 3f, y - 3f, x - 6f, y - 9f, 1.6f, 0.4f, Amethyst, -0.2f);
        }

        /// <summary>
        /// O braco de tras, tomado de ametista do cotovelo pra baixo. Por
        /// angulos, ou com a mao num ponto: no cabo da picareta, ou no apito.
        /// </summary>
        private static void DrawBackArm(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            float bsx = f.SX - 6f;
            float bsy = f.SY + 3f;
            float ex, ey, hx, hy;

            if (pose.Whistle)
            {
                hx = Lerp(f.SX + 12f, f.MouthX + 1f, pose.WhistleLift);
                hy = Lerp(f.SY + 12f, f.MouthY + 1f, pose.WhistleLift);
                (ex, ey) = ArtKit.Joint(bsx, bsy, hx, hy, 17f, 16f, -1);
            }
            else if (pose.TwoHands && !pose.PickDropped)
            {
                (hx, hy) = ArtKit.Along(f.HandX, f.HandY, pose.Pick, -8f);
                (ex, ey) = ArtKit.Joint(bsx, bsy, hx, hy, 17f, 16f, 1);
            }
            else
            {
                (ex, ey) = ArtKit.Along(bsx, bsy, pose.BackArm, 17f);
                (hx, hy) = ArtKit.Along(ex, ey, pose.BackFore, 16f);
            }

            float bias = pose.Whistle ? 0f : -0.4f;
            ArtKit.Taper(c, bsx, bsy, ex, ey, 7f, 6f, ArtKit.Corroded(Coat, 0.08f, 5231, 0.25f, Patch), bias);
            ArtKit.Ball(c, ex, ey, 6.5f, 5.5f, Coat, bias + 0.1f);
            ArtKit.Limb(c, ex, ey, hx, hy, 4.6f, f.Skin, bias);
            ArtKit.Ball(c, hx, hy, 5f, 4.6f, f.Skin, bias + 0.1f);

            // A ametista crescendo pelo antebraco e pelos nos dos dedos.
            if (!pose.Freed)
            {
                float dx = hx - ex;
                float dy = hy - ey;
                float length = Math.Max(0.01f, (float)Math.Sqrt(dx * dx + dy * dy));
                float nx = dy / length;
                float ny = -dx / length;
                if (ny > 0f) { nx = -nx; ny = -ny; }

                for (int i = 0; i < 3; i++)
                {
                    float t = 0.25f + i * 0.28f;
                    float bx = ex + dx * t;
                    float by = ey + dy * t;
                    float size = 6f + i * 2f;
                    ArtKit.Taper(c, bx, by, bx + nx * size + dx / length * 2f, by + ny * size + dy / length * 2f,
                        2.4f + i * 0.3f, 0.4f, i == 1 ? Milky : Amethyst, bias + 0.3f);
                }

                ArtKit.Taper(c, hx, hy, hx + dx / length * 6f, hy + dy / length * 6f, 2f, 0.4f, Amethyst, bias + 0.2f);
            }

            if (pose.Whistle)
            {
                // O apito na boca, a corrente esticada.
                ArtKit.Drum(c, f.MouthX + 3f, f.MouthY, 3.4f, 1.8f, 3f, ArtKit.Plain(ArtKit.Brass), 0.3f);
                c.Set((int)(f.MouthX + 5f), (int)f.MouthY - 1, ArtKit.Brass[3]);
                ArtKit.Line(c, (int)f.SX + 4, (int)f.SY + 1, (int)f.MouthX + 1, (int)f.MouthY + 1, ArtKit.Iron[2]);
            }
        }

        /// <summary>A picareta: cabo de madeira com a virola de latao e a cabeca de ferro de duas pontas.</summary>
        private static void DrawPick(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;

            (float bx, float by) = ArtKit.Along(f.HandX, f.HandY, pose.Pick, -10f);
            (float tx, float ty) = ArtKit.Along(f.HandX, f.HandY, pose.Pick, 30f);
            ArtKit.Limb(c, bx, by, tx, ty, 1.9f, Wood, 0.15f);
            ArtKit.Drum(c, bx, by, 2.4f, 2.4f, 2f, ArtKit.Plain(ArtKit.Iron), 0.1f);

            DrawPickHead(c, tx, ty, pose.Pick);

            // A virola de latao, logo abaixo da cabeca.
            (float vx, float vy) = ArtKit.Along(f.HandX, f.HandY, pose.Pick, 25f);
            ArtKit.Ball(c, vx, vy, 2.6f, 2.6f, ArtKit.Brass, 0.3f);

            if (pose.Buried)
            {
                // A ponta enterrada: pedras soltas por cima.
                (float px, float py) = ArtKit.Along(tx, ty, pose.Pick + 90f, 13f);
                float rx = Math.Max(px, tx);
                for (int i = 0; i < 4; i++)
                {
                    ArtKit.Ball(c, rx - 6f + i * 4f, Ground - 1f - (i % 2) * 1.5f, 3f, 2.2f, ArtKit.Iron, 0.1f);
                }
            }
        }

        private static void DrawPickHead(PixelCanvas c, float tx, float ty, float angle)
        {
            double a = angle * Math.PI / 180.0;
            float ux = (float)Math.Cos(a);
            float uy = (float)Math.Sin(a);
            float px = -uy;
            float py = ux;
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Iron, 0.12f, 5241, 0.4f, ArtKit.Rust);

            // As duas pontas, curvadas pro lado do cabo.
            foreach ((float side, float length) in new[] { (1f, 15f), (-1f, 12f) })
            {
                float mx = tx + px * side * length * 0.55f + ux * -1f;
                float my = ty + py * side * length * 0.55f + uy * -1f;
                float ex = tx + px * side * length - ux * 4.5f;
                float ey = ty + py * side * length - uy * 4.5f;
                ArtKit.Taper(c, tx, ty, mx, my, 3.4f, 2.4f, iron, 0.2f);
                ArtKit.Taper(c, mx, my, ex, ey, 2.4f, side > 0f ? 0.4f : 1.2f, iron, 0.2f);
            }

            ArtKit.Ball(c, tx, ty, 3.6f, 3.6f, ArtKit.Iron, 0.25f);
            c.Set((int)tx, (int)ty, ArtKit.Iron[3]);
            c.Set((int)(tx + px * 6f), (int)(ty + py * 6f), ArtKit.Rust[1]);
        }

        /// <summary>A picareta largada no chao, na frente.</summary>
        private static void DrawDroppedPick(Frame f)
        {
            float x0 = f.HX + 4f;
            float x1 = x0 + 38f;
            ArtKit.Limb(f.Canvas, x0, Ground - 2f, x1, Ground - 3f, 1.9f, Wood, 0.1f);
            DrawPickHead(f.Canvas, x1 + 1f, Ground - 9f, -90f);
        }

        /// <summary>O braco da frente: a manga, o antebraco de fora, a luva sem dedos fechada no cabo.</summary>
        private static void DrawFrontArm(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float fsx = f.SX + 6f;
            float fsy = f.SY + 4f;

            ArtKit.Taper(c, fsx, fsy, f.ElbowX, f.ElbowY, 8f, 6.5f, ArtKit.Corroded(Coat, 0.08f, 5251, 0.25f, Patch), 0.1f);
            ArtKit.Ball(c, fsx, fsy, 7f, 6.5f, Coat, 0.25f);

            // A manga arregacada no cotovelo.
            ArtKit.Ball(c, f.ElbowX, f.ElbowY, 6.5f, 5.5f, Coat, 0.2f);
            ArtKit.Limb(c, f.ElbowX, f.ElbowY, f.HandX, f.HandY, 5f, f.Skin, 0.1f);
            ArtKit.Ball(c, f.ElbowX, f.ElbowY, 5f, 4f, Coat, 0.35f);

            ArtKit.Ball(c, f.HandX, f.HandY, 5.6f, 5f, ArtKit.Leather, 0.2f);
            c.Set((int)f.HandX + 2, (int)f.HandY - 2, ArtKit.Leather[3]);
            for (int i = 0; i < 3; i++) c.Set((int)f.HandX - 1 + i * 2, (int)f.HandY + 3, f.Skin[2]);
        }

        /// <summary>Deitado de costas, em paz: o rosto de gente, a picareta e o capacete do lado.</summary>
        private static void DrawLying(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float g = Ground;

            // A picareta e o capacete, atras e na frente.
            ArtKit.Limb(c, 104f, g - 2f, 140f, g - 3f, 1.9f, Wood, 0.1f);
            DrawPickHead(c, 141f, g - 9f, -90f);
            ArtKit.Ball(c, 20f, g - 5f, 10f, 5f, ArtKit.Corroded(ArtKit.Brass, 0.3f, 5222, 0.3f), 0.1f);
            for (int i = -11; i <= 11; i++) c.Set(20 + i, (int)g - 9, ArtKit.Brass[1]);

            // As pernas, as botas pra cima.
            ArtKit.Taper(c, 58f, g - 7f, 38f, g - 6f, 7f, 6f, Trousers, 0f);
            ArtKit.Ball(c, 40f, g - 6f, 4f, 3.6f, Patch, 0.1f);
            ArtKit.Drum(c, 32f, g - 9f, 4f, 8f, 3f, ArtKit.Plain(ArtKit.Leather), 0.1f);

            // O casacao aberto no chao, o corpo.
            ArtKit.Polygon(c, new[] { (52f, g - 2f), (98f, g - 2f), (94f, g - 12f), (56f, g - 12f) },
                (x, y) => ArtKit.Tone(Coat, -0.3f));
            ArtKit.Ball(c, 74f, g - 10f, 20f, 9f, Coat, 0.1f);
            for (int i = 0; i < 3; i++) ArtKit.Rivet(c, 66 + i * 8, (int)g - 18, ArtKit.Brass);
            int beltX = 58;
            for (int y = (int)g - 18; y <= (int)g - 3; y++)
            {
                if (c.Get(beltX, y).A == 0) continue;
                c.Set(beltX, y, ArtKit.Leather[2]);
                c.Set(beltX + 1, y, ArtKit.Leather[0]);
            }

            DrawKeys(c, 56f, g - 4f);

            // O braco caido do lado, a mao aberta.
            ArtKit.Limb(c, 80f, g - 6f, 98f, g - 3f, 4.5f, Coat, 0f);
            ArtKit.Ball(c, 101f, g - 3f, 4f, 3f, Human, 0.1f);

            // O lenco, a cabeca virada pro alto, olhos fechados, a barba.
            ArtKit.Ball(c, 94f, g - 12f, 5f, 6f, Red, 0.2f);
            ArtKit.Ball(c, 103f, g - 11f, 9.5f, 9f, Human, 0.15f);
            ArtKit.Ball(c, 105f, g - 19f, 3f, 2.6f, Human, 0.3f);
            ArtKit.Line(c, 107, (int)g - 14, 110, (int)g - 14, Human[0]);
            ArtKit.Ball(c, 98f, g - 17f, 7f, 5.5f, Beard, 0.1f);
            ArtKit.Taper(c, 95f, g - 19f, 90f, g - 24f, 3f, 0.6f, Beard, 0.1f);
            ArtKit.Ball(c, 102f, g - 19f, 4f, 2f, Beard, 0.3f);
            for (int i = 0; i < 3; i++) c.Set(108 + i * 2, (int)g - 6 + (i % 2), Beard[2]);

            // O que sobrou dos cristais, apagado, espalhado.
            ArtKit.Taper(c, 120f, g - 1f, 122f, g - 6f, 1.6f, 0.4f, Amethyst, -0.3f);
            ArtKit.Taper(c, 66f, g - 1f, 63f, g - 5f, 1.6f, 0.4f, Milky, -0.3f);
        }
    }
}
