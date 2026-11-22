namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Quem se conversa no District 3, no mesmo padrao do District2NpcArt:
    /// os mineiros que sobraram, depois que a broca para.
    ///
    ///   0 Mineiro — atarracado, de cobre gasto, capacete com o lampiao aceso,
    ///               a picareta apoiada no ombro e batendo o pe no ritmo.
    ///   1 Mineira — mais alta e magra, de latao, capacete com o lampiao e um
    ///               lenco no pescoco, segurando a caneca de cafe fumegando.
    ///
    /// 48x64, olhando pra direita, 32 pixels por tile, oito quadros parados
    /// (respira, pisca uma vez, cada um com o seu gesto). Pivo na base.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class District3NpcArt
    {
        public const int Width = 48;
        public const int Height = 64;
        public const int PixelsPerUnit = 32;
        public const int Count = 2;
        public const int IdleFrames = 8;
        public const int BlinkFrame = 5;

        public const int Miner = 0;
        public const int MinerWoman = 1;

        private const float Ground = 62f;

        private static readonly float[] Breath = { 0f, 0f, 0.5f, 1f, 1f, 1f, 0.5f, 0f };
        private static readonly Rgba Warm = new Rgba(255, 206, 128);
        private static readonly Rgba WarmCore = new Rgba(255, 248, 226);
        private static readonly Rgba LampGlow = new Rgba(255, 236, 170);
        private static readonly Rgba LampCore = new Rgba(255, 255, 240);

        private static readonly Rgba[] Helmet =
            { new Rgba(92, 70, 26), new Rgba(150, 118, 40), new Rgba(202, 166, 64), new Rgba(240, 214, 120) };

        private static readonly Rgba[] Amethyst =
            { new Rgba(70, 40, 110), new Rgba(120, 70, 170), new Rgba(170, 120, 220), new Rgba(222, 196, 250) };

        public static string SheetName(int variant) => "d3_npc_" + variant + "_sheet";

        public static PixelCanvas[] Frames(int variant)
        {
            var frames = new PixelCanvas[IdleFrames];
            for (int i = 0; i < IdleFrames; i++)
            {
                var c = new PixelCanvas(Width, Height);
                if (variant == Miner) MinerMan(c, i);
                else MinerWomanArt(c, i);
                frames[i] = c;
            }

            return frames;
        }

        private static void Leg(PixelCanvas c, float hipX, float hipY, float footX, Rgba[] ramp, Rgba[] boot, float half, bool back, float lift = 0f)
        {
            float bias = back ? -0.4f : 0f;
            float footY = Ground - 2f - lift;
            (float kx, float ky) = ArtKit.Joint(hipX, hipY, footX, footY, (Ground - hipY) * 0.55f, (Ground - hipY) * 0.55f, -1);
            ArtKit.Limb(c, hipX, hipY, kx, ky, half, ramp, bias);
            ArtKit.Limb(c, kx, ky, footX, footY, half * 0.9f, ramp, bias);
            ArtKit.Ball(c, footX + 1f, footY + 0.5f, half + 1.8f, 2f, boot, bias);
            if (lift <= 0f) c.Fill((int)(footX - half), (int)Ground, (int)(half * 2f + 3f), 1, boot[0]);
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

        /// <summary>O capacete de mineiro: casco de latao, aba, e o lampiao aceso na frente.</summary>
        private static void HardHat(PixelCanvas c, float hx, float hy, int seed, bool flicker)
        {
            ArtKit.Ball(c, hx, hy - 5f, 7.5f, 5f, ArtKit.Corroded(Helmet, 0.1f, seed, 0.5f), 0.25f);
            c.Fill((int)hx - 9, (int)hy - 2, 19, 2, Helmet[1]);
            c.Fill((int)hx - 9, (int)hy - 2, 19, 1, Helmet[3]);
            ArtKit.Line(c, (int)hx - 5, (int)hy - 9, (int)hx + 4, (int)hy - 9, Helmet[3]);
            ArtKit.Rivet(c, (int)hx - 4, (int)hy - 5, Helmet);

            // O lampiao: um tambor de latao com o vidro aceso.
            ArtKit.Drum(c, hx + 6f, hy - 6f, 2.6f, 2.4f, 1.2f, ArtKit.Plain(ArtKit.Brass), 0.3f);
            c.Fill((int)hx + 7, (int)hy - 7, 3, 3, flicker ? LampGlow : LampCore);
            c.Set((int)hx + 8, (int)hy - 6, LampCore);
        }

        // ------------------------------------------------------------------
        // O mineiro
        // ------------------------------------------------------------------

        private static void MinerMan(PixelCanvas c, int frame)
        {
            float b = Breath[frame];
            bool blink = frame == BlinkFrame;
            ArtKit.Skin copper = ArtKit.Corroded(ArtKit.Copper, 0.14f, 901, 0.4f);

            // A picareta, apoiada no ombro de tras: cabo de madeira e a cabeca de ferro la atras.
            float sway = frame == 3 || frame == 4 ? 1f : 0f;
            ArtKit.Limb(c, 30f, 38f + b, 8f, 18f + b + sway, 1.4f, ArtKit.Leather, 0.1f);
            var head = new[] { (4f, 22f + b + sway), (8f, 15f + b + sway), (14f, 13f + b + sway), (9f, 18f + b + sway) };
            ArtKit.Polygon(c, head, (x, y) => ArtKit.Tone(ArtKit.Iron, 0.5f - (y - 13f) * 0.06f));
            c.Set(4, (int)(22f + b + sway), ArtKit.Iron[3]);

            // O braco de tras, segurando o cabo.
            ArtKit.Limb(c, 17f, 34f + b, 15f, 30f + b, 1.9f, ArtKit.Copper, -0.4f);
            ArtKit.Ball(c, 15f, 29f + b, 2.3f, 2.3f, ArtKit.Leather, -0.3f);

            // As pernas, curtas; a da frente bate o pe no ritmo.
            float tap = frame == 1 || frame == 5 ? 1.5f : 0f;
            Leg(c, 19f, 48f, 17f, ArtKit.Leather, ArtKit.Iron, 2.6f, true);
            Leg(c, 26f, 48f, 28f, ArtKit.Leather, ArtKit.Iron, 2.7f, false, tap);

            // O corpo largo, com o colete de couro e o cinto de ferramentas.
            float bx = 22f;
            float by = 39f + b;
            ArtKit.Ball(c, bx, by, 10f, 9.5f, copper, 0.05f);
            var vest = new[] { (bx - 3f, by - 8f), (bx + 6f, by - 8f), (bx + 9f, by + 7f), (bx - 2f, by + 8f) };
            ArtKit.Polygon(c, vest, (x, y) => ArtKit.Tone(ArtKit.Leather, 0.35f - (x - bx) * 0.05f - (y - by) * 0.03f));
            c.Fill((int)bx - 9, (int)by + 5, 19, 2, ArtKit.Leather[0]);
            c.Fill((int)bx - 1, (int)by + 4, 3, 4, ArtKit.Brass[2]);
            c.Set((int)bx, (int)by + 5, ArtKit.Brass[3]);
            for (int i = 0; i < 3; i++) ArtKit.Rivet(c, (int)bx + 3, (int)by - 6 + i * 4, ArtKit.Brass);

            // Um cristal roxo pendurado no cinto: lembranca da mina.
            ArtKit.Ball(c, bx + 7f, by + 9f, 1.5f, 2.2f, Amethyst, 0.3f);

            // A cabeca, redonda, com a barba de fios de cobre.
            float hx = 27f;
            float hy = 26f + b * 0.6f;
            ArtKit.Ball(c, hx, hy, 6.8f, 6.5f, ArtKit.Corroded(ArtKit.Copper, 0.1f, 902, 0.5f), 0.12f);
            Eye(c, (int)hx + 1, (int)hy - 1, blink, Warm, WarmCore);
            Eye(c, (int)hx + 5, (int)hy - 1, blink, Warm, WarmCore);
            var beard = new[] { (hx - 2f, hy + 2f), (hx + 7f, hy + 2f), (hx + 5f, hy + 8f), (hx, hy + 8f) };
            ArtKit.Polygon(c, beard, (x, y) => ArtKit.Tone(ArtKit.Copper, 0.2f - (y - hy) * 0.04f));
            for (int i = 0; i < 4; i++) c.Set((int)hx + i * 2, (int)hy + 4 + (i % 2), ArtKit.Copper[0]);
            HardHat(c, hx + 1f, hy - 3f, 903, frame == 2 || frame == 6);

            // O braco da frente, na cintura.
            ArtKit.Limb(c, 28f, 34f + b, 31f, 41f + b, 2f, ArtKit.Copper, 0.05f);
            ArtKit.Ball(c, 31f, 42f + b, 2.4f, 2.4f, ArtKit.Leather, 0.2f);

            ArtKit.AddOutline(c);

            // O brilho do lampiao.
            if (frame == 0 || frame == 4) ArtKit.Spark(c, (int)hx + 10, (int)hy - 10, 1);
        }

        // ------------------------------------------------------------------
        // A mineira
        // ------------------------------------------------------------------

        private static void MinerWomanArt(PixelCanvas c, int frame)
        {
            float b = Breath[frame];
            bool blink = frame == BlinkFrame;
            ArtKit.Skin brass = ArtKit.Corroded(ArtKit.Brass, 0.12f, 911, 0.4f);

            // O braco de tras, solto.
            ArtKit.Limb(c, 18f, 33f + b, 16f, 44f + b, 1.7f, ArtKit.Brass, -0.4f);
            ArtKit.Ball(c, 16f, 45f + b, 2.1f, 2.1f, ArtKit.Leather, -0.3f);

            Leg(c, 21f, 47f, 19f, ArtKit.Cloth, ArtKit.Iron, 2.1f, true);
            Leg(c, 26f, 47f, 28f, ArtKit.Cloth, ArtKit.Iron, 2.2f, false);

            // O corpo esguio, o macacao de lona e as joelheiras.
            float bx = 23f;
            float by = 37f + b;
            ArtKit.Ball(c, bx, by, 7.5f, 9.5f, brass, 0.05f);
            var overall = new[] { (bx - 6f, by - 2f), (bx + 6f, by - 3f), (bx + 7f, by + 10f), (bx - 7f, by + 10f) };
            ArtKit.Polygon(c, overall, (x, y) => ArtKit.Tone(ArtKit.Cloth, 0.25f - (x - bx) * 0.05f - (y - by) * 0.03f));
            ArtKit.Line(c, (int)bx - 3, (int)by - 2, (int)bx - 3, (int)by - 9, ArtKit.Cloth[1]);
            ArtKit.Line(c, (int)bx + 4, (int)by - 3, (int)bx + 4, (int)by - 9, ArtKit.Cloth[1]);
            c.Fill((int)bx - 1, (int)by + 2, 4, 3, ArtKit.Cloth[3]);
            c.Fill(26, 54, 4, 2, ArtKit.Leather[2]);

            // O lenco no pescoco, verde-musgo.
            var scarfColors = new[] { new Rgba(30, 60, 40), new Rgba(50, 96, 64), new Rgba(80, 136, 92), new Rgba(130, 180, 130) };
            var scarf = new[] { (bx - 4f, by - 10f), (bx + 6f, by - 10f), (bx + 5f, by - 6f), (bx - 3f, by - 6f) };
            ArtKit.Polygon(c, scarf, (x, y) => ArtKit.Tone(scarfColors, 0.6f - (y - by + 10f) * 0.1f));
            c.Fill((int)bx - 5, (int)by - 6, 2, 4, scarfColors[1]);

            // A cabeca, alta, de latao, com o capacete e o lampiao.
            float hx = 26f;
            float hy = 21f + b * 0.5f;
            float[] look = { 0f, 0f, 0f, 1f, 1f, 1f, 0f, 0f };
            ArtKit.Ball(c, hx, hy, 6.2f, 6.8f, ArtKit.Corroded(ArtKit.Brass, 0.1f, 912, 0.5f), 0.15f);
            Eye(c, (int)(hx + 1f + look[frame]), (int)hy, blink, Warm, WarmCore);
            Eye(c, (int)(hx + 5f + look[frame]), (int)hy, blink, Warm, WarmCore);
            c.Fill((int)hx + 2, (int)hy + 4, 3, 1, ArtKit.Brass[0]);
            HardHat(c, hx, hy - 3f, 913, frame == 1 || frame == 5);

            // A caneca de cafe, na mao da frente, fumegando.
            float lift = frame >= 3 && frame <= 5 ? -2f : 0f;
            float mx = 34f;
            float my = 35f + b + lift;
            ArtKit.Limb(c, 28f, 30f + b, mx - 1f, my + 1f, 1.8f, ArtKit.Brass, 0.05f);
            ArtKit.Drum(c, mx + 1f, my - 1f, 2.6f, 3.2f, 1f, ArtKit.Plain(ArtKit.Iron), 0.25f);
            c.Fill((int)mx + 4, (int)my - 2, 1, 3, ArtKit.Iron[2]);
            c.Fill((int)mx - 1, (int)my - 4, 5, 1, new Rgba(70, 44, 26));
            ArtKit.Ball(c, mx - 1f, my + 0.5f, 2.2f, 2.2f, ArtKit.Leather, 0.2f);

            ArtKit.AddOutline(c);

            // O vapor do cafe, subindo em ondas.
            var steam = new Rgba(230, 230, 236, 150);
            for (int i = 0; i < 3; i++)
            {
                int sy = (int)my - 6 - i * 3;
                int sx = (int)mx + ((frame + i) % 3) - 1;
                c.Set(sx, sy, steam);
                c.Set(sx + 1, sy - 1, steam);
            }

            if (frame == 0 || frame == 4) ArtKit.Spark(c, (int)hx + 9, (int)hy - 10, 1);
        }
    }
}
