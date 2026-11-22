using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class KingArt
    {
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

            DrawDroppedScepter(new Frame { Canvas = c, Pose = f.Pose, Corrupt = f.Corrupt, HX = -8f });
        }
    }
}
