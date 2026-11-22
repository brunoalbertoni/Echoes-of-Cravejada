namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Quem se conversa no District 4, no mesmo padrao do District3NpcArt:
    /// a Assistente do Alquimista, que sai do armario depois da purificacao.
    ///
    /// Esguia, de cobre polido, jaleco claro ate o joelho, cabelo de fio de
    /// cobre preso num coque, oculos de lente azul na testa e a prancheta
    /// abracada; de vez em quando confere a prancheta.
    ///
    /// 48x64, olhando pra direita, 32 pixels por tile, oito quadros parados
    /// (respira, pisca uma vez). Pivo na base. Nao depende de UnityEngine.
    /// </summary>
    public static class District4NpcArt
    {
        public const int Width = 48;
        public const int Height = 64;
        public const int PixelsPerUnit = 32;
        public const int IdleFrames = 8;
        public const int BlinkFrame = 5;

        public const string SheetName = "d4_npc_assistant_sheet";

        private const float Ground = 62f;

        private static readonly float[] Breath = { 0f, 0f, 0.5f, 1f, 1f, 1f, 0.5f, 0f };
        private static readonly Rgba Warm = new Rgba(255, 206, 128);
        private static readonly Rgba WarmCore = new Rgba(255, 248, 226);

        private static readonly Rgba[] Coat =
            { new Rgba(110, 116, 110), new Rgba(168, 174, 162), new Rgba(210, 214, 200), new Rgba(242, 242, 232) };

        private static readonly Rgba[] Hair =
            { new Rgba(70, 36, 26), new Rgba(110, 56, 36), new Rgba(150, 82, 52), new Rgba(196, 118, 76) };

        private static readonly Rgba[] Lens =
            { new Rgba(60, 90, 130), new Rgba(110, 160, 200), new Rgba(170, 214, 240), new Rgba(240, 252, 255) };

        private static readonly Rgba[] Paper =
            { new Rgba(150, 136, 104), new Rgba(196, 182, 142), new Rgba(224, 212, 172), new Rgba(246, 238, 210) };

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[IdleFrames];
            for (int i = 0; i < IdleFrames; i++)
            {
                var c = new PixelCanvas(Width, Height);
                Assistant(c, i);
                frames[i] = c;
            }

            return frames;
        }

        private static void Leg(PixelCanvas c, float hipX, float hipY, float footX, Rgba[] ramp, Rgba[] boot, float half, bool back)
        {
            float bias = back ? -0.4f : 0f;
            float footY = Ground - 2f;
            (float kx, float ky) = ArtKit.Joint(hipX, hipY, footX, footY, (Ground - hipY) * 0.55f, (Ground - hipY) * 0.55f, -1);
            ArtKit.Limb(c, hipX, hipY, kx, ky, half, ramp, bias);
            ArtKit.Limb(c, kx, ky, footX, footY, half * 0.9f, ramp, bias);
            ArtKit.Ball(c, footX + 1f, footY + 0.5f, half + 1.5f, 1.8f, boot, bias);
            c.Fill((int)(footX - half), (int)Ground, (int)(half * 2f + 3f), 1, boot[0]);
        }

        private static void Eye(PixelCanvas c, int x, int y, bool shut)
        {
            if (shut)
            {
                c.Fill(x, y + 1, 2, 1, Warm);
                return;
            }

            c.Fill(x - 1, y - 1, 4, 4, ArtKit.Outline);
            c.Fill(x, y, 2, 2, Warm);
            c.Set(x, y, WarmCore);
        }

        private static void Assistant(PixelCanvas c, int frame)
        {
            float b = Breath[frame];
            bool blink = frame == BlinkFrame;
            ArtKit.Skin copper = ArtKit.Corroded(ArtKit.Copper, 0.06f, 921, 0.3f);

            // O braco de tras, solto ao lado do corpo.
            ArtKit.Limb(c, 19f, 32f + b, 17f, 43f + b, 1.6f, Coat, -0.4f);
            ArtKit.Ball(c, 17f, 44f + b, 2f, 2f, ArtKit.Copper, -0.3f);

            // As pernas finas, de cobre, e as botinhas de couro.
            Leg(c, 21f, 48f, 20f, ArtKit.Copper, ArtKit.Leather, 1.9f, true);
            Leg(c, 26f, 48f, 28f, ArtKit.Copper, ArtKit.Leather, 2f, false);

            // O corpo esguio.
            float bx = 23f;
            float by = 37f + b;
            ArtKit.Ball(c, bx, by, 7f, 9.5f, copper, 0.05f);

            // O jaleco, aberto embaixo e descendo ate o joelho.
            var coat = new[] { (bx - 7f, by - 9f), (bx + 7f, by - 9f), (bx + 9f, by + 14f), (bx - 9f, by + 14f) };
            ArtKit.Polygon(c, coat, (x, y) => ArtKit.Tone(Coat, 0.7f - (x - bx + 8f) / 22f - (y - by) * 0.01f));
            ArtKit.Line(c, (int)bx + 1, (int)by - 8, (int)bx + 2, (int)by + 13, Coat[0]);
            c.Fill((int)bx + 2, (int)by + 9, 3, 5, ArtKit.Copper[1]);
            for (int i = 0; i < 3; i++) ArtKit.Rivet(c, (int)bx + 3, (int)by - 5 + i * 4, ArtKit.Brass);

            // A gola e o bolso com as canetas.
            ArtKit.Line(c, (int)bx - 3, (int)by - 9, (int)bx + 1, (int)by - 4, Coat[1]);
            ArtKit.Line(c, (int)bx + 5, (int)by - 9, (int)bx + 2, (int)by - 4, Coat[1]);
            c.Fill((int)bx - 5, (int)by - 2, 4, 3, Coat[1]);
            c.Set((int)bx - 4, (int)by - 3, ArtKit.Brass[2]);
            c.Set((int)bx - 3, (int)by - 4, new Rgba(170, 60, 50));

            // A cabeca, de cobre polido.
            float hx = 26f;
            float hy = 20f + b * 0.5f;
            float[] look = { 0f, 0f, 0f, 1f, 1f, 1f, 0f, 0f };
            // O cabelo de fio de cobre: o coque atras, desenhado antes da cabeca.
            ArtKit.Ball(c, hx - 7f, hy - 4f, 3.2f, 3.2f, ArtKit.Plain(Hair), 0.2f);
            ArtKit.Ball(c, hx - 3f, hy - 1f, 4.5f, 5.5f, ArtKit.Plain(Hair), 0.1f);
            ArtKit.Ball(c, hx, hy, 6f, 6.6f, ArtKit.Corroded(ArtKit.Copper, 0.05f, 922, 0.3f), 0.15f);

            // A franja, so no alto da cabeca, puxada pra tras.
            for (int x = (int)hx - 5; x <= (int)hx + 4; x++)
            {
                int top = (int)hy - 6 + (x > hx ? 1 : 0);
                c.Fill(x, top, 1, x < hx - 1 ? 4 : 2, Hair[(x & 1) == 0 ? 2 : 1]);
            }
            c.Fill((int)hx - 6, (int)hy - 3, 2, 6, Hair[1]);

            Eye(c, (int)(hx + 1f + look[frame]), (int)hy, blink);
            Eye(c, (int)(hx + 5f + look[frame]), (int)hy, blink);
            c.Fill((int)hx + 3, (int)hy + 4, 2, 1, ArtKit.Copper[0]);

            // Os oculos na testa: aro de latao e as lentes azuis.
            float gy = hy - 5f;
            c.Fill((int)hx - 2, (int)gy, 10, 1, ArtKit.Leather[1]);
            ArtKit.Ball(c, hx + 2f, gy, 1.8f, 1.6f, Lens, 0.4f);
            ArtKit.Ball(c, hx + 6f, gy, 1.8f, 1.6f, Lens, 0.4f);

            // A prancheta, abracada na frente; nos quadros 3 e 4 ela a ergue um pouco, conferindo.
            bool fix = false;
            float px = 31f;
            float py = 36f + b - (frame == 3 || frame == 4 ? 1f : 0f);
            c.Fill((int)px - 3, (int)py - 6, 8, 11, Paper[1]);
            c.Fill((int)px - 2, (int)py - 5, 6, 9, Paper[3]);
            for (int i = 0; i < 4; i++) c.Fill((int)px - 1, (int)py - 3 + i * 2, 4, 1, Paper[0]);
            c.Fill((int)px - 1, (int)py - 7, 4, 2, ArtKit.Brass[2]);

            if (fix)
            {
                ArtKit.Limb(c, 28f, 30f + b, hx + 7f, gy + 2f, 1.7f, Coat, 0.05f);
                ArtKit.Ball(c, hx + 7f, gy + 2f, 1.9f, 1.9f, ArtKit.Copper, 0.2f);
            }
            else
            {
                ArtKit.Limb(c, 28f, 30f + b, px - 1f, py + 2f, 1.7f, Coat, 0.05f);
                ArtKit.Ball(c, px - 1f, py + 2f, 1.9f, 1.9f, ArtKit.Copper, 0.2f);
            }

            ArtKit.AddOutline(c);

            // O brilho nas lentes.
            if (frame == 0 || frame == 6) ArtKit.Spark(c, (int)hx + 6, (int)gy - 2, 1);
        }
    }
}
