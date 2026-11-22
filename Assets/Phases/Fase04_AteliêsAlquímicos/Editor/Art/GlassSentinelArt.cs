using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A Sentinela de Vidro: uma armadura oca de vidro soprado, com uma
    /// alabarda de cristal. Por dentro, o que a move: uma chama verde de Oxide
    /// tremulando, que aparece pelo vidro. Golpeada, transmuta em prata macica
    /// — opaca, polida, so a fresta do elmo acesa. Duas folhas com os mesmos
    /// quadros: a de vidro e a de prata. Morrendo (so morre de vidro), racha
    /// inteira, vira um monte de cacos e a chama escapa. 56x64, pivo na base.
    /// </summary>
    public static class GlassSentinelArt
    {
        public const int Width = 56;
        public const int Height = 64;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 24f / Width;
        public const float PivotY = 1f / Height;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Walk = 4;
        public const int WalkFrames = 8;
        public const int Hurt = 12;
        public const int Death = 13;
        public const int DeathFrames = 6;
        public const int FrameCount = 19;

        private const float Ground = 62f;

        private sealed class Pose
        {
            public float Y;
            public float BackFoot = -4f;
            public float FrontFoot = 4f;
            public float BackLift;
            public float FrontLift;
            public float Flame;
            public float Spear;
            public float Tilt;
            public float Shatter;
        }

        public static PixelCanvas[] Frames(bool silver)
        {
            var frames = new PixelCanvas[FrameCount];

            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(silver, new Pose { Y = i == 2 ? 0.5f : 0f, Flame = i, Spear = (i % 2) * 2f });
            }

            for (int i = 0; i < WalkFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / WalkFrames;
                float swing = (float)Math.Sin(phase);
                float bob = (float)Math.Abs(Math.Cos(phase));
                frames[Walk + i] = Draw(silver, new Pose
                {
                    Y = (1f - bob) * 1.5f, BackFoot = -swing * 6f, FrontFoot = swing * 6f,
                    BackLift = swing > 0.2f ? swing * 3f : 0f, FrontLift = swing < -0.2f ? -swing * 3f : 0f,
                    Flame = i, Spear = swing * 3f,
                });
            }

            frames[Hurt] = Draw(silver, new Pose { Tilt = -6f, Flame = 2, Spear = -4f });

            for (int i = 0; i < DeathFrames; i++)
            {
                frames[Death + i] = Draw(silver, new Pose { Shatter = (i + 1) / (float)DeathFrames, Flame = i });
            }

            return frames;
        }

        private static PixelCanvas Draw(bool silver, Pose pose)
        {
            var c = new PixelCanvas(Width, Height);

            if (pose.Shatter > 0f)
            {
                DrawShatter(c, silver, pose);
                return c;
            }

            float hx = 24f + pose.Tilt * 0.2f;
            float hy = 38f + pose.Y;

            // A chama de dentro: desenhada antes, aparece pelo vidro.
            if (!silver)
            {
                DrawFlame(c, hx, hy - 12f, 1.2f, (int)pose.Flame);
                DrawFlame(c, hx + 2f, hy - 25f, 0.5f, (int)pose.Flame + 1);
            }

            // A armadura, numa camada propria: vidro e translucido, prata nao.
            var armor = new PixelCanvas(Width, Height);
            Rgba[] ramp = silver ? D4.Polished : D4.ArmorGlass;
            DrawArmor(armor, ramp, silver, pose, hx, hy);
            ArtKit.AddOutline(armor, silver ? ArtKit.Outline : new Rgba(30, 60, 84));

            for (int i = 0; i < armor.Pixels.Length; i++)
            {
                Rgba p = armor.Pixels[i];
                if (p.A == 0) continue;

                int x = i % Width;
                int y = i / Width;
                bool edge = p.R == 30 && p.G == 60;
                bool shine = p.R == ramp[3].R && p.G == ramp[3].G;
                int alpha = silver || edge || shine ? 255 : (p.R == ramp[0].R ? 200 : 115);
                c.Blend(x, y, p.WithAlpha(alpha));
            }

            // Prata: so a fresta do elmo acesa.
            if (silver)
            {
                c.Fill((int)hx + 1, (int)(hy - 24f), 5, 1, LabArt.OxideGreen);
            }
            else
            {
                ArtKit.Glow(c, hx, hy - 12f, 12f, LabArt.OxideGreen, 0.35f, true);
            }

            return c;
        }

        private static void DrawArmor(PixelCanvas c, Rgba[] ramp, bool silver, Pose pose, float hx, float hy)
        {
            ArtKit.Skin skin = silver ? ArtKit.Plain(ramp) : (x, y, light) => ArtKit.Tone(ramp, light + 0.1f);

            // A alabarda, atras do braco.
            float spearX = hx + 14f;
            float top = hy - 34f + pose.Spear * 0.3f;
            ArtKit.Limb(c, spearX, top + 6f, spearX - 1f, Ground - 2f, 1.1f, ramp, 0.1f);
            ArtKit.Polygon(c, new[] { (spearX, top - 4f), (spearX + 3f, top + 4f), (spearX, top + 7f), (spearX - 3f, top + 4f) },
                (x, y) => ArtKit.Tone(ramp, 0.6f - (x - spearX) * 0.2f));
            ArtKit.Polygon(c, new[] { (spearX - 1f, top + 5f), (spearX - 8f, top + 3f), (spearX - 7f, top + 10f), (spearX - 1f, top + 9f) },
                (x, y) => ArtKit.Tone(ramp, 0.3f - (y - top) * 0.05f));

            // As pernas: as grevas ocas, com o joelho redondo.
            foreach (bool back in new[] { true, false })
            {
                float footX = 24f + (back ? pose.BackFoot - 3f : pose.FrontFoot + 3f);
                float footY = Ground - 2f - (back ? pose.BackLift : pose.FrontLift);
                float hipX = hx + (back ? -3f : 3f);
                (float kx, float ky) = ArtKit.Joint(hipX, hy + 1f, footX, footY, 11f, 11f, -1);
                float bias = back ? -0.4f : 0f;
                ArtKit.Taper(c, hipX, hy + 1f, kx, ky, 3.2f, 2.8f, skin, bias);
                ArtKit.Taper(c, kx, ky, footX, footY, 3f, 2.6f, skin, bias);
                ArtKit.Ball(c, kx, ky, 3f, 3f, ramp, bias + 0.3f);
                ArtKit.Drum(c, footX + 1.5f, footY + 0.5f, 4.5f, 2.2f, 3f, skin, bias);
            }

            // A saia de placas e o peitoral oco.
            ArtKit.Drum(c, hx, hy - 1f, 8f, 4f, 3f, skin, 0f);
            for (int x = (int)hx - 7; x <= (int)hx + 7; x += 3) c.Set(x, (int)hy, ramp[0]);
            ArtKit.Drum(c, hx, hy - 12f, 9f, 9f, 2.6f, skin, 0.1f);
            ArtKit.Line(c, (int)hx + 1, (int)hy - 20, (int)hx + 2, (int)hy - 5, ramp[0]);
            ArtKit.Line(c, (int)hx - 6, (int)hy - 18, (int)hx - 3, (int)hy - 8, ramp[3]);

            // As ombreiras.
            ArtKit.Ball(c, hx - 7f, hy - 19f, 4.6f, 3.4f, skin, 0.2f);
            ArtKit.Ball(c, hx + 7f, hy - 19f, 4.6f, 3.4f, skin, 0.3f);

            // O elmo, com a viseira e a crista.
            float ex = hx + 2f;
            float ey = hy - 26f;
            ArtKit.Drum(c, ex, ey, 5.5f, 6f, 2.4f, skin, 0.2f);
            c.Fill((int)ex - 1, (int)ey + 1, 7, 2, silver ? ArtKit.Outline : ramp[0]);
            for (int i = 0; i < 5; i++) c.Set((int)ex - 3 + i, (int)ey - 6 - (i % 2), ramp[3]);

            // O braco, segurando a alabarda.
            ArtKit.Limb(c, hx + 7f, hy - 18f, hx + 11f, hy - 10f, 2.6f, skin, 0.2f);
            ArtKit.Limb(c, hx + 11f, hy - 10f, spearX, hy - 13f + pose.Spear * 0.3f, 2.2f, skin, 0.2f);
            ArtKit.Ball(c, spearX, hy - 13f + pose.Spear * 0.3f, 2.6f, 2.6f, ramp, 0.3f);

            // O brilho do vidro (ou da prata polida).
            ArtKit.Line(c, (int)hx - 4, (int)hy - 16, (int)hx - 2, (int)hy - 12, ramp[3]);
            c.Set((int)ex - 3, (int)ey - 3, ramp[3]);
        }

        /// <summary>A chama verde que move a armadura, tremulando.</summary>
        private static void DrawFlame(PixelCanvas c, float x, float y, float strength, int frame)
        {
            float sway = frame % 2 == 0 ? -1f : 1f;
            var tongues = new[] { (x - 3f + sway * 0.5f, 7f), (x + 1f - sway * 0.5f, 10f), (x + 4f + sway, 6f) };
            foreach ((float tx, float h) in tongues)
            {
                ArtKit.Taper(c, tx, y + 6f, tx + sway, y + 6f - h * strength, 2.8f, 0.4f, D4.Oxide, 0.3f);
            }

            ArtKit.Ball(c, x, y + 5f, 4f, 3f, D4.Oxide, 0.4f);
            c.Set((int)x, (int)y + 4, D4.Oxide[3]);
        }

        private static void DrawShatter(PixelCanvas c, bool silver, Pose pose)
        {
            float k = pose.Shatter;
            Rgba[] ramp = silver ? D4.Polished : D4.ArmorGlass;

            // A chama escapando pra cima, se apagando.
            if (k < 0.85f) DrawFlame(c, 24f, 30f - k * 22f, 1f - k, pose.Flame >= 0 ? (int)pose.Flame : 0);

            if (k < 0.25f)
            {
                // Rachada inteira, ainda de pe.
                var armor = new PixelCanvas(Width, Height);
                DrawArmor(armor, ramp, silver, new Pose(), 24f, 38f);
                for (int i = 0; i < 5; i++) ArtKit.Line(armor, 18 + i * 3, 16 + i * 4, 24 + i * 2, 44 - i * 3, new Rgba(255, 255, 255));
                ArtKit.AddOutline(armor, new Rgba(30, 60, 84));
                for (int i = 0; i < armor.Pixels.Length; i++)
                {
                    Rgba p = armor.Pixels[i];
                    if (p.A > 0) c.Blend(i % Width, i / Width, p.WithAlpha(200));
                }

                return;
            }

            // Os cacos caindo e o monte no chao.
            float t = (k - 0.25f) / 0.75f;
            for (int i = 0; i < 22; i++)
            {
                float sx = 12f + ArtKit.Hash(i, 0, 7401) % 26;
                float sy0 = 10f + ArtKit.Hash(i, 0, 7402) % 44;
                float sy = sy0 + (Ground - 2f - sy0) * Math.Min(1f, t * 1.4f);
                float px = sx + (sx - 24f) * t * 0.5f;
                float s = 1.5f + ArtKit.Hash(i, 1, 7403) % 3;
                ArtKit.Polygon(c, new[] { (px, sy - s), (px + s, sy), (px - s * 0.6f, sy + s * 0.6f) },
                    (x, y) => ramp[1 + i % 3].WithAlpha(silver ? 255 : 200));
            }

            if (t > 0.6f) D4.Sparkle(c, 24f, Ground - 4f, 14f, 10, (int)(t * 10f), 160);
        }
    }
}
