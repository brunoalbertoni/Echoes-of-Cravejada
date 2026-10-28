using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Quem se conversa no District 2, no detalhe do Scrap: os Tarnished que
    /// nao foram tomados (operarios de latao que ja usavam mascara pra soldar)
    /// e o Crisol, o Lapidario.
    ///
    ///   0 O velho encanador — curvado, a mascara levantada na testa, os olhos
    ///                         cansados, apoiado na chave de cano como bengala.
    ///   1 Solda             — a soldadora que tirou a mascara: a mascara
    ///                         debaixo do braco, o lenco na cabeca, o macarico
    ///                         na cinta.
    ///   2 Crisol            — o Nobre riscado que desceu: alto, de prata
    ///                         arranhada, monoculo, a bigorna de lapidario nas
    ///                         costas e a muleta; lustra uma gema sem parar.
    ///
    /// 48x64, olhando pra direita, 32 pixels por tile, oito quadros parados
    /// (respira, pisca uma vez, cada um com o seu gesto). Pivo na base.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class District2NpcArt
    {
        public const int Width = 48;
        public const int Height = 64;
        public const int PixelsPerUnit = 32;
        public const int Count = 3;
        public const int IdleFrames = 8;
        public const int BlinkFrame = 5;

        public const int OldPlumber = 0;
        public const int Solda = 1;
        public const int Crisol = 2;

        private const float Ground = 62f;

        private static readonly float[] Breath = { 0f, 0f, 0.5f, 1f, 1f, 1f, 0.5f, 0f };
        private static readonly Rgba Warm = new Rgba(255, 206, 128);
        private static readonly Rgba WarmCore = new Rgba(255, 248, 226);

        private static readonly Rgba[] Silver =
            { new Rgba(78, 86, 104), new Rgba(126, 134, 152), new Rgba(174, 182, 198), new Rgba(222, 228, 240) };

        public static PixelCanvas[] Frames(int variant)
        {
            var frames = new PixelCanvas[IdleFrames];
            for (int i = 0; i < IdleFrames; i++)
            {
                var c = new PixelCanvas(Width, Height);
                switch (variant)
                {
                    case OldPlumber: Plumber(c, i); break;
                    case Solda: Welder(c, i); break;
                    default: Lapidary(c, i); break;
                }

                frames[i] = c;
            }

            return frames;
        }

        private static void Leg(PixelCanvas c, float hipX, float hipY, float footX, Rgba[] ramp, Rgba[] boot, float half, bool back)
        {
            float bias = back ? -0.4f : 0f;
            (float kx, float ky) = ArtKit.Joint(hipX, hipY, footX, Ground - 2f, (Ground - hipY) * 0.55f, (Ground - hipY) * 0.55f, -1);
            ArtKit.Limb(c, hipX, hipY, kx, ky, half, ramp, bias);
            ArtKit.Limb(c, kx, ky, footX, Ground - 2f, half * 0.9f, ramp, bias);
            ArtKit.Ball(c, footX + 1f, Ground - 1.5f, half + 1.8f, 2f, boot, bias);
            c.Fill((int)(footX - half), (int)Ground, (int)(half * 2f + 3f), 1, boot[0]);
        }

        private static void Eye(PixelCanvas c, int x, int y, bool shut, Rgba color, Rgba core)
        {
            if (shut)
            {
                c.Fill(x, y + 1, 2, 1, color);
                return;
            }

            c.Fill(x - 1, y - 1, 4, 4, ArtKit.Outline);
            c.Fill(x, y, 2, 2, color);
            c.Set(x, y, core);
        }

        // ------------------------------------------------------------------
        // O velho encanador
        // ------------------------------------------------------------------

        private static void Plumber(PixelCanvas c, int frame)
        {
            float b = Breath[frame];
            bool blink = frame == BlinkFrame;
            ArtKit.Skin brass = ArtKit.Corroded(ArtKit.Brass, 0.12f, 801, 0.4f);

            // A chave de cano, fincada no chao, como bengala.
            float tap = frame == 3 ? -1f : 0f;
            ArtKit.Limb(c, 36f, Ground + tap, 35f, 34f, 1.5f, ArtKit.Iron, 0.2f);
            var jaw = new[] { (31f, 30f), (40f, 29f), (40f, 34f), (37f, 34f), (37f, 32f), (31f, 33f) };
            ArtKit.Polygon(c, jaw, (x, y) => ArtKit.Tone(ArtKit.Iron, 0.4f - (y - 29f) * 0.15f));

            // O braco de tras.
            ArtKit.Limb(c, 17f, 36f + b, 14f, 46f + b, 1.8f, ArtKit.Brass, -0.4f);
            ArtKit.Ball(c, 14f, 47f + b, 2.2f, 2.2f, ArtKit.Leather, -0.3f);

            Leg(c, 20f, 48f, 18f, ArtKit.Leather, ArtKit.Iron, 2.3f, true);
            Leg(c, 26f, 48f, 27f, ArtKit.Leather, ArtKit.Iron, 2.4f, false);

            // O corpo curvado, pra frente, com o macacao remendado.
            float bx = 23f;
            float by = 41f + b;
            ArtKit.Ball(c, bx, by, 9f, 9.5f, brass, 0f);
            var overall = new[] { (bx - 4f, by - 2f), (bx + 7f, by - 3f), (bx + 8f, by + 9f), (bx - 6f, by + 9f) };
            ArtKit.Polygon(c, overall, (x, y) => ArtKit.Tone(ArtKit.Cloth, 0.2f - (x - bx) * 0.06f - (y - by) * 0.03f));
            c.Fill((int)bx - 2, (int)by + 2, 5, 4, ArtKit.Leather[2]);
            c.Fill((int)bx - 2, (int)by + 2, 5, 1, ArtKit.Leather[3]);
            ArtKit.Line(c, (int)bx - 2, (int)by - 2, (int)bx - 3, (int)by - 10, ArtKit.Cloth[1]);
            ArtKit.Line(c, (int)bx + 5, (int)by - 3, (int)bx + 5, (int)by - 10, ArtKit.Cloth[1]);

            // A cabeca, baixa, com a mascara levantada na testa.
            float hx = 29f;
            float hy = 27f + b * 0.6f + (frame == 6 ? 0.5f : 0f);
            ArtKit.Ball(c, hx, hy, 6.5f, 6.5f, ArtKit.Corroded(ArtKit.Brass, 0.1f, 802, 0.5f), 0.1f);
            Eye(c, (int)hx + 1, (int)hy, blink, Warm, WarmCore);
            Eye(c, (int)hx + 5, (int)hy, blink, Warm, WarmCore);
            c.Fill((int)hx + 1, (int)hy - 2, 7, 1, ArtKit.Brass[0]);
            for (int i = 0; i < 3; i++) c.Set((int)hx + 2 + i * 2, (int)hy + 4, ArtKit.Brass[0]);

            // A mascara de soldador, virada pra cima.
            ArtKit.Drum(c, hx - 1f, hy - 8f, 7f, 3.5f, 3f, ArtKit.Corroded(ArtKit.Iron, 0.2f, 803, 0.5f, ArtKit.Rust), 0.2f);
            c.Fill((int)hx - 4, (int)hy - 9, 7, 2, ArtKit.Outline);
            c.Fill((int)hx - 3, (int)hy - 9, 5, 1, new Rgba(70, 100, 104));
            ArtKit.Rivet(c, (int)hx - 8, (int)hy - 7, ArtKit.Brass);

            // O braco da frente, na chave.
            ArtKit.Limb(c, 29f, 36f + b, 34f, 37f, 1.9f, ArtKit.Brass, 0.05f);
            ArtKit.Ball(c, 35f, 36.5f, 2.6f, 2.6f, ArtKit.Leather, 0.2f);

            ArtKit.AddOutline(c);
        }

        // ------------------------------------------------------------------
        // Solda
        // ------------------------------------------------------------------

        private static void Welder(PixelCanvas c, int frame)
        {
            float b = Breath[frame];
            bool blink = frame == BlinkFrame;
            ArtKit.Skin brass = ArtKit.Corroded(ArtKit.Brass, 0.15f, 811, 0.4f);

            ArtKit.Limb(c, 17f, 33f + b, 15f, 44f + b, 1.8f, ArtKit.Brass, -0.4f);
            ArtKit.Ball(c, 15f, 45f + b, 2.2f, 2.2f, ArtKit.Leather, -0.3f);

            Leg(c, 20f, 47f, 18f, ArtKit.Leather, ArtKit.Iron, 2.2f, true);
            Leg(c, 26f, 47f, 28f, ArtKit.Leather, ArtKit.Iron, 2.3f, false);

            // O corpo, mais esguio, de latao limpo; o avental e a cinta com o macarico.
            float bx = 23f;
            float by = 38f + b;
            ArtKit.Ball(c, bx, by, 8f, 9.5f, brass, 0.05f);
            var apron = new[] { (bx - 2f, by - 4f), (bx + 7f, by - 3f), (bx + 7.5f, by + 9.5f), (bx - 3f, by + 10f) };
            ArtKit.Polygon(c, apron, (x, y) => ArtKit.Tone(ArtKit.Leather, 0.3f - (x - bx) * 0.05f - (y - by) * 0.03f));
            c.Fill((int)bx - 7, (int)by + 3, 15, 2, ArtKit.Leather[0]);
            ArtKit.Limb(c, bx - 6f, by + 4f, bx - 6f, by + 12f, 1.2f, ArtKit.Brass, 0.2f);
            c.Set((int)bx - 6, (int)by + 13, ArtKit.Iron[2]);

            // A cabeca, sem mascara: rosto de cobre, olhos quentes, o lenco amarrado.
            float hx = 26f;
            float hy = 22f + b * 0.5f;
            float[] look = { 0f, 0f, 0f, 1f, 1f, 1f, 0f, 0f };
            ArtKit.Ball(c, hx, hy, 6.5f, 6.8f, ArtKit.Corroded(ArtKit.Copper, 0.12f, 812, 0.5f), 0.15f);
            Eye(c, (int)(hx + 1f + look[frame]), (int)hy, blink, Warm, WarmCore);
            Eye(c, (int)(hx + 5f + look[frame]), (int)hy, blink, Warm, WarmCore);
            c.Fill((int)hx + 2, (int)hy + 4, 4, 1, ArtKit.Copper[0]);

            var scarf = new (float, float)[] { (hx - 7f, hy - 1f), (hx - 4f, hy - 8f), (hx + 4f, hy - 8f), (hx + 7f, hy - 3f), (hx - 2f, hy - 3f) };
            ArtKit.Polygon(c, scarf, (x, y) => ArtKit.Tone(new[] { new Rgba(90, 30, 30), new Rgba(140, 46, 40), new Rgba(186, 72, 56), new Rgba(220, 116, 90) }, 0.5f - (y - hy + 8f) * 0.12f));
            c.Set((int)hx - 8, (int)hy, new Rgba(140, 46, 40));
            c.Set((int)hx - 9, (int)hy + 2, new Rgba(90, 30, 30));

            // A mascara debaixo do braco da frente.
            ArtKit.Drum(c, 32f, 40f + b, 5f, 5.5f, 3f, ArtKit.Corroded(ArtKit.Iron, 0.15f, 813, 0.5f, ArtKit.Rust), 0.15f);
            c.Fill(30, (int)(38f + b), 6, 2, ArtKit.Outline);
            c.Fill(31, (int)(38f + b), 4, 1, new Rgba(70, 100, 104));
            ArtKit.Limb(c, 28f, 31f + b, 33f, 36f + b, 1.9f, ArtKit.Brass, 0.05f);
            ArtKit.Ball(c, 34f, 37f + b, 2.4f, 2.4f, ArtKit.Leather, 0.2f);

            ArtKit.AddOutline(c);
        }

        // ------------------------------------------------------------------
        // Crisol, o Lapidario
        // ------------------------------------------------------------------

        private static void Lapidary(PixelCanvas c, int frame)
        {
            float b = Breath[frame];
            bool blink = frame == BlinkFrame;
            ArtKit.Skin silver = (x, y, light) =>
            {
                // A prata riscada: arranhoes escuros em diagonal.
                int scratch = ((int)Math.Floor(x * 1.3f + y * 0.7f) + 40) % 11;
                if (scratch == 0 && ArtKit.Hash((int)x, (int)y, 821) % 3 != 0) return ArtKit.Shift(Silver, ArtKit.Tone(Silver, light), -1);
                return ArtKit.Tone(Silver, light);
            };

            // A bigorna de lapidario nas costas.
            ArtKit.Drum(c, 11f, 30f + b, 6f, 3f, 3f, ArtKit.Plain(ArtKit.Iron), 0.2f);
            ArtKit.Limb(c, 11f, 32f + b, 11f, 37f + b, 2.2f, ArtKit.Iron, 0f);
            ArtKit.Limb(c, 5f, 30f + b, 3f, 31f + b, 1.6f, ArtKit.Iron, 0.1f);
            c.Fill(8, (int)(39f + b), 7, 2, ArtKit.Leather[1]);

            // A muleta, debaixo do braco de tras.
            ArtKit.Limb(c, 15f, 34f + b, 13f, Ground, 1.3f, ArtKit.Leather, -0.2f);
            ArtKit.Limb(c, 12f, 34f + b, 18f, 34f + b, 1.2f, ArtKit.Leather, 0f);

            Leg(c, 21f, 46f, 19f, Silver, ArtKit.Iron, 2.1f, true);
            Leg(c, 27f, 46f, 28f, Silver, ArtKit.Iron, 2.2f, false);

            // O corpo alto e magro, com a gola de nobre.
            float bx = 24f;
            float by = 36f + b;
            ArtKit.Drum(c, bx, by, 7.5f, 11f, 3f, silver, 0.1f);
            for (int i = 0; i < 3; i++) ArtKit.Rivet(c, (int)bx + 4, (int)by - 6 + i * 5, Silver);
            var collar = new[] { (bx - 8f, by - 11f), (bx + 8f, by - 11f), (bx + 10f, by - 15f), (bx - 10f, by - 15f) };
            ArtKit.Polygon(c, collar, (x, y) => ArtKit.Tone(Silver, 0.6f - (x - bx) * 0.06f));

            // A cabeca, alta, de prata, com o monoculo no olho da frente.
            float hx = 26f;
            float hy = 17f + b * 0.5f;
            ArtKit.Drum(c, hx, hy, 6.5f, 7.5f, 2.6f, silver, 0.2f);
            var pale = new Rgba(170, 220, 255);
            Eye(c, (int)hx - 1, (int)hy, blink, pale, new Rgba(240, 250, 255));
            ArtKit.Ball(c, hx + 4.5f, hy + 0.5f, 3.4f, 3.4f, ArtKit.Brass, 0.3f);
            ArtKit.Ball(c, hx + 4.5f, hy + 0.5f, 2.3f, 2.3f, new[] { new Rgba(90, 140, 170), new Rgba(140, 200, 230), new Rgba(190, 230, 250), new Rgba(240, 250, 255) }, 0.3f);
            if (!blink) c.Fill((int)(hx + 4f), (int)hy, 2, 2, pale);
            ArtKit.Line(c, (int)(hx + 7f), (int)(hy + 3f), (int)(hx + 6f), (int)(hy + 10f), ArtKit.Brass[2]);
            c.Fill((int)hx - 2, (int)hy + 5, 6, 1, Silver[0]);

            // O que sobrou da coroa: tres pontas tortas, uma partida.
            c.Fill((int)hx - 5, (int)hy - 9, 10, 2, Silver[2]);
            c.Fill((int)hx - 5, (int)hy - 10, 2, 1, Silver[2]);
            c.Fill((int)hx - 1, (int)hy - 11, 2, 2, Silver[3]);
            c.Set((int)hx + 3, (int)hy - 10, Silver[1]);
            c.Set((int)hx, (int)hy - 9, new Rgba(120, 190, 230));

            // O braco da frente, lustrando uma gema com um pano.
            float[] polish = { 0f, 1f, 2f, 1f, 0f, -1f, 0f, 1f };
            float gx = 34f + polish[frame] * 0.5f;
            float gy = 33f + b;
            ArtKit.Limb(c, 29f, 27f + b, gx - 1f, gy, 1.8f, Silver, 0.05f);
            ArtKit.Ball(c, gx, gy - 2f, 2f, 2f, new[] { new Rgba(50, 120, 170), new Rgba(110, 200, 230), new Rgba(170, 236, 250), new Rgba(240, 255, 255) }, 0.3f);
            c.Fill((int)gx - 2, (int)gy, 5, 2, ArtKit.Cloth[3]);
            c.Set((int)(gx - 2f + polish[frame]), (int)gy + 1, ArtKit.Cloth[1]);

            ArtKit.AddOutline(c);

            // O brilho da gema (e, de vez em quando, no monoculo).
            if (frame == 2 || frame == 6) ArtKit.Spark(c, (int)gx + 1, (int)gy - 3, 1);
            if (frame == 3) c.Set((int)(hx + 3f), (int)(hy - 1f), new Rgba(255, 255, 255));
        }
    }
}
