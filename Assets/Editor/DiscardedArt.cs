using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os descartados do District 1: autômatos velhos, de lataria amassada,
    /// jogados no poco antes do Scrap. Cada um tem o corpo do que fazia:
    ///
    ///   0 Refugo   — carregava lampioes: o corpo e uma lanterna de vidro
    ///                rachado, com um caco de cristal ainda aceso, e ele se
    ///                apoia na vara de gancho.
    ///   1 Rebite   — aprendiz de ourives: redondo e cheio de rebites, de
    ///                capuz e avental de couro, com a lupa de joalheiro no olho.
    ///   2 Ferrolho — mecanico de caldeira: atarracado, corpo de caldeira com
    ///                manometro no peito, cabeca de parafuso, oculos na testa,
    ///                uma chave inglesa na mao e o outro braco perdido.
    ///   3 Pequena  — uma lata de conserva com pernas, de olhos grandes,
    ///                antena torta e o giz com que escreve no muro.
    ///
    /// Cada quadro tem 48x48, olha pra direita e e importado com 32 pixels
    /// por tile, como o Scrap. Sao oito quadros parados (respirando, piscando
    /// uma vez, cada um com o seu detalhe mexendo). Os pes ficam na base — e o pivo.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class DiscardedArt
    {
        public const int Size = 48;
        public const int PixelsPerUnit = 32;
        public const int Count = 4;
        public const int IdleFrames = 8;

        /// <summary>O quadro de olhos fechados, dentro do ciclo.</summary>
        public const int BlinkFrame = 5;

        private const float Ground = 46f;

        private static readonly Rgba EyeCore = new Rgba(255, 248, 226);
        private static readonly Rgba EyeGlow = new Rgba(255, 206, 128);
        private static readonly Rgba Glass = new Rgba(110, 150, 150);
        private static readonly Rgba GlassDark = new Rgba(60, 88, 92);

        private static readonly float[] Breath = { 0f, 0f, 0.5f, 1f, 1f, 1f, 0.5f, 0f };

        public static PixelCanvas[] Frames(int variant)
        {
            var frames = new PixelCanvas[IdleFrames];

            for (int i = 0; i < IdleFrames; i++)
            {
                var canvas = new PixelCanvas(Size, Size);

                switch (variant)
                {
                    case 0: Refugo(canvas, i); break;
                    case 1: Rebite(canvas, i); break;
                    case 2: Ferrolho(canvas, i); break;
                    default: Pequena(canvas, i); break;
                }

                frames[i] = canvas;
            }

            return frames;
        }

        // ------------------------------------------------------------------
        // Pecas em comum
        // ------------------------------------------------------------------

        /// <summary>Uma perna: coxa e canela num membro so, com a bota redonda.</summary>
        private static void Leg(PixelCanvas c, float hipX, float hipY, float footX, Rgba[] ramp, Rgba[] boot, float half, bool back)
        {
            float bias = back ? -0.4f : 0f;
            (float kx, float ky) = ArtKit.Joint(hipX, hipY, footX, Ground - 2f, (Ground - hipY) * 0.55f, (Ground - hipY) * 0.55f, -1);
            ArtKit.Limb(c, hipX, hipY, kx, ky, half, ramp, bias);
            ArtKit.Limb(c, kx, ky, footX, Ground - 2f, half * 0.9f, ramp, bias);
            ArtKit.Ball(c, footX + 1f, Ground - 1.5f, half + 1.6f, 1.8f, boot, bias);
            c.Fill((int)(footX - half), (int)Ground, (int)(half * 2f + 3f), 1, boot[0]);
        }

        /// <summary>Um olho aceso: oval de miolo claro, ou um risco quando fechado.</summary>
        private static void Eye(PixelCanvas c, int x, int y, int w, int h, bool shut, Rgba socket)
        {
            if (shut)
            {
                c.Fill(x, y + h / 2, w, 1, EyeGlow);
                return;
            }

            c.Fill(x - 1, y - 1, w + 2, h + 2, socket);
            c.Fill(x, y, w, h, EyeGlow);
            c.Set(x, y, EyeCore);
            if (w > 1 && h > 1) c.Set(x + 1, y, EyeCore);
        }

        /// <summary>Uns pontos de azinhavre e ferrugem, presos a um lugar, so em cima do que ja existe.</summary>
        private static void Grime(PixelCanvas c, int x0, int y0, int w, int h, int seed, int density)
        {
            for (int py = y0; py < y0 + h; py++)
            {
                for (int px = x0; px < x0 + w; px++)
                {
                    Rgba under = c.Get(px, py);
                    if (under.A < 250 || under.R == ArtKit.Outline.R && under.G == ArtKit.Outline.G) continue;

                    int roll = ArtKit.Hash(px - x0, py - y0, seed) % 100;
                    if (roll < density) c.Set(px, py, roll % 2 == 0 ? ArtKit.Verdigris[1] : ArtKit.Verdigris[2]);
                    else if (roll < density + 3) c.Set(px, py, ArtKit.Rust[1]);
                }
            }
        }

        // ------------------------------------------------------------------
        // 0. Refugo, o lampioneiro
        // ------------------------------------------------------------------

        private static void Refugo(PixelCanvas c, int frame)
        {
            float b = Breath[frame];
            bool blink = frame == BlinkFrame;

            // A vara de gancho, fincada no chao, atras de tudo.
            ArtKit.Limb(c, 37f, Ground, 35f, 9f, 1f, ArtKit.Leather, 0.2f);
            ArtKit.Limb(c, 35f, 9f, 38f, 6f, 0.9f, ArtKit.Iron, 0.3f);
            ArtKit.Limb(c, 38f, 6f, 40f, 8f, 0.8f, ArtKit.Iron, 0.3f);

            // O braco de tras, pendurado.
            ArtKit.Limb(c, 18f, 21f + b, 15f, 33f + b, 1.3f, ArtKit.Iron, -0.4f);
            ArtKit.Ball(c, 15f, 34f + b, 1.8f, 1.8f, ArtKit.Tin, -0.4f);

            // Pernas finas, de joelho dobrado (ele ja nao fica reto).
            Leg(c, 20f, 33f + b, 18f, ArtKit.Iron, ArtKit.Rubber, 1.4f, true);
            Leg(c, 25f, 33f + b, 27f, ArtKit.Iron, ArtKit.Rubber, 1.5f, false);

            // O corpo: a lanterna. Moldura de latao, vidros escuros e rachados,
            // e o caco de cristal la dentro, ainda tremulando.
            float bx = 22f;
            float by = 26f + b;
            ArtKit.Drum(c, bx, by, 7.5f, 8.5f, 4f, ArtKit.Corroded(ArtKit.Brass, 0.3f, 201, 0.35f), 0.1f);

            float[] flicker = { 0.8f, 0.6f, 0.9f, 0.7f, 1f, 0.5f, 0.85f, 0.65f };
            float lit = flicker[frame];

            for (int py = (int)(by - 5f); py <= (int)(by + 5f); py++)
            {
                for (int px = (int)(bx - 5f); px <= (int)(bx + 4f); px++)
                {
                    // Montantes da moldura no meio do vidro.
                    if (px == (int)bx) continue;

                    float dx = px + 0.5f - bx;
                    float dy = py + 0.5f - (by + 1f);
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    float warm = Math.Max(0f, 1f - d / 6f) * lit;

                    Rgba color = Rgba.Lerp(py < by - 2f ? Glass : GlassDark, new Rgba(255, 200, 120), warm * 0.85f);
                    c.Set(px, py, color);
                }
            }

            // O caco de cristal.
            c.Set((int)bx - 1, (int)by, new Rgba(255, 246, 220));
            c.Set((int)bx + 1, (int)by + 1, new Rgba(255, 230, 170));
            c.Set((int)bx - 1, (int)by + 1, new Rgba(255, 220, 150));
            c.Set((int)bx + 1, (int)by - 1, new Rgba(255, 236, 190));

            // A rachadura do vidro e o reflexo.
            ArtKit.Line(c, (int)bx - 4, (int)by - 4, (int)bx - 2, (int)by + 1, new Rgba(30, 40, 44, 200));
            c.Set((int)bx + 3, (int)by - 4, new Rgba(220, 240, 240));
            c.Set((int)bx + 3, (int)by - 3, new Rgba(180, 210, 214));

            // O telhadinho da lanterna, que e o ombro.
            ArtKit.Drum(c, bx, by - 8.5f, 8.5f, 2f, 3f, ArtKit.Plain(ArtKit.Rust), 0.3f);

            // O lenco velho no pescoco.
            for (int px = (int)bx - 6; px <= (int)bx + 4; px++)
            {
                c.Set(px, (int)(by - 10f), ArtKit.Tone(ArtKit.Cloth, 0.4f - (px - bx) * 0.08f));
                c.Set(px, (int)(by - 9f), ArtKit.Tone(ArtKit.Cloth, -0.2f));
            }

            c.Set((int)bx - 6, (int)(by - 8f), ArtKit.Cloth[1]);
            c.Set((int)bx - 7, (int)(by - 7f), ArtKit.Cloth[0]);

            // A cabeca: um balde amassado, com a argola da lanterna em cima.
            float hx = 24f;
            float hy = 13f + b * 0.5f + (frame == 6 ? 0.5f : 0f);
            ArtKit.Drum(c, hx, hy, 6.5f, 5.5f, 3f, ArtKit.Corroded(ArtKit.Tin, 0.2f, 202, 0.4f), 0.15f);
            c.Set((int)hx - 3, (int)hy - 4, ArtKit.Tin[1]);
            c.Set((int)hx - 2, (int)hy - 3, ArtKit.Tin[1]);

            for (int i = 0; i <= 8; i++)
            {
                double a = Math.PI + i * Math.PI / 8.0;
                c.Set((int)(hx + Math.Cos(a) * 3.5), (int)(hy - 6f + Math.Sin(a) * 3.0), ArtKit.Iron[2]);
            }

            // Dois olhos redondos, fundos.
            Eye(c, (int)hx + 1, (int)hy - 1, 2, 2, blink, ArtKit.Iron[0]);
            Eye(c, (int)hx + 4, (int)hy - 1, 2, 2, blink, ArtKit.Iron[0]);

            Grime(c, 14, 4, 20, 40, 203, 4);

            // O braco da frente, segurando a vara.
            ArtKit.Limb(c, 27f, 20f + b, 33f, 25f, 1.5f, ArtKit.Iron, 0.1f);
            ArtKit.Ball(c, 27f, 20f + b, 2.1f, 2.1f, ArtKit.Rust, 0.2f);
            ArtKit.Ball(c, 35f, 25f, 2.2f, 2.2f, ArtKit.Tin, 0.1f);

            ArtKit.AddOutline(c);

            // A luz da lanterna vazando em volta.
            ArtKit.Glow(c, bx, by + 1f, 9f, new Rgba(255, 200, 120), 0.25f * lit, true);
        }

        // ------------------------------------------------------------------
        // 1. Rebite, o aprendiz de ourives
        // ------------------------------------------------------------------

        private static void Rebite(PixelCanvas c, int frame)
        {
            float b = Breath[frame];
            bool blink = frame == BlinkFrame;

            // A barra do capuz caindo pelas costas.
            ArtKit.Taper(c, 18f, 17f + b, 15f, 29f + b, 3.5f, 2.2f, ArtKit.Cloth, -0.2f);

            Leg(c, 21f, 37f, 19f, ArtKit.Copper, ArtKit.Leather, 2f, true);
            Leg(c, 27f, 37f, 28f, ArtKit.Copper, ArtKit.Leather, 2.1f, false);

            // O corpo: redondo, de cobre, rebitado em fileiras.
            float bx = 24f;
            float by = 31f + b;
            ArtKit.Ball(c, bx, by, 8.5f, 8f, ArtKit.Corroded(ArtKit.Copper, 0.18f, 211, 0.35f), 0.05f);
            for (int row = -1; row <= 1; row++)
            {
                for (int i = -2; i <= 2; i++)
                {
                    int x = (int)(bx + i * 3f - 1f);
                    int y = (int)(by + row * 4f - 1f);
                    float dx = (x - bx) / 8f;
                    float dy = (y - by) / 8f;
                    if (dx * dx + dy * dy < 0.75f) ArtKit.Rivet(c, x, y, ArtKit.Copper);
                }
            }

            // O avental de couro, com o bolso.
            var apron = new[] { (bx - 1f, by - 3f), (bx + 8f, by - 2f), (bx + 9f, by + 8f), (bx - 2f, by + 9f) };
            ArtKit.Polygon(c, apron, (x, y) => ArtKit.Tone(ArtKit.Leather, 0.3f - (x - bx) * 0.06f - (y - by) * 0.04f));
            c.Fill((int)bx + 2, (int)by + 3, 4, 3, ArtKit.Leather[0]);
            c.Fill((int)bx + 2, (int)by + 3, 4, 1, ArtKit.Leather[3]);
            ArtKit.Line(c, (int)bx, (int)by - 3, (int)bx + 3, (int)by - 8, ArtKit.Leather[1]);

            // As maos, mexendo numa pecinha na frente do peito.
            float[] fidget = { 0f, 0.5f, 1f, 0.5f, 0f, 0f, -0.5f, 0f };
            float hx = 33f + fidget[frame];
            float hy = 30f + b - Math.Abs(fidget[frame]);
            ArtKit.Limb(c, bx + 4f, by - 4f, hx, hy, 1.6f, ArtKit.Copper, 0.05f);
            ArtKit.Ball(c, hx, hy, 2.2f, 2.2f, ArtKit.Copper, 0.2f);
            c.Set((int)hx + 2, (int)hy - 2, ArtKit.Brass[3]);
            c.Set((int)hx + 3, (int)hy - 3, ArtKit.Brass[2]);

            // O capuz e a cabeca de lata dentro dele.
            float cx = 24f;
            float cy = 16f + b * 0.5f;
            ArtKit.Ball(c, cx, cy - 0.5f, 8f, 7.5f, ArtKit.Cloth, 0f);
            ArtKit.Ball(c, cx + 2f, cy + 1f, 6.5f, 6f, ArtKit.Corroded(ArtKit.Tin, 0.15f, 212, 0.4f), 0.2f);

            // A borda do capuz em volta do rosto.
            for (int i = 0; i < 12; i++)
            {
                double a = -Math.PI * 0.6 + i * Math.PI * 1.2 / 11.0;
                int x = (int)(cx + 2f + Math.Cos(a) * 6.8);
                int y = (int)(cy + 1f + Math.Sin(a) * 6.4);
                if (Math.Cos(a) > 0.2) continue;
                c.Set(x, y, ArtKit.Cloth[3]);
            }

            // O olho de tras, pequeno; o da frente atras da lupa.
            Eye(c, (int)cx, (int)cy, 2, 2, blink, ArtKit.Iron[0]);

            // A lupa de joalheiro: um tubo de latao saindo do olho, com o vidro na ponta.
            int lx = (int)cx + 4;
            int ly = (int)cy - 1;
            ArtKit.Limb(c, lx, ly + 1.5f, lx + 5f, ly + 1f, 2.3f, ArtKit.Brass, 0.2f);
            ArtKit.Ball(c, lx + 6f, ly + 1f, 1.6f, 2.8f, new[] { GlassDark, Glass, new Rgba(170, 210, 214), new Rgba(230, 250, 250) }, 0.2f);
            if (!blink) c.Set(lx + 6, ly + 1, EyeGlow);

            bool glint = frame == 2 || frame == 3;
            if (glint) c.Set(lx + 6, ly - 1, new Rgba(255, 255, 255));

            Grime(c, 10, 22, 28, 24, 213, 3);
            ArtKit.AddOutline(c);
            if (glint) ArtKit.Spark(c, lx + 7, ly - 2, 1);
        }

        // ------------------------------------------------------------------
        // 2. Ferrolho, o mecanico de caldeira
        // ------------------------------------------------------------------

        private static void Ferrolho(PixelCanvas c, int frame)
        {
            float b = Breath[frame];
            bool blink = frame == BlinkFrame;

            // O toco do braco perdido, atras: um coto tampado e um fio solto.
            ArtKit.Limb(c, 15f, 25f + b, 12f, 28f + b, 2.4f, ArtKit.Iron, -0.4f);
            ArtKit.Ball(c, 11.5f, 28.5f + b, 2.3f, 2.3f, ArtKit.Rust, -0.3f);
            ArtKit.Line(c, 11, 30 + (int)b, 10, 34 + (int)b, new Rgba(180, 60, 40));
            ArtKit.Line(c, 10, 34 + (int)b, 11, 36 + (int)b, new Rgba(140, 50, 36));

            Leg(c, 19f, 38f, 17f, ArtKit.Iron, ArtKit.Rubber, 2.6f, true);
            Leg(c, 27f, 38f, 29f, ArtKit.Iron, ArtKit.Rubber, 2.7f, false);

            // O corpo: uma caldeirinha atarracada, com cintas e o manometro no peito.
            float bx = 23f;
            float by = 30f + b;
            ArtKit.Drum(c, bx, by, 10.5f, 9f, 2.6f, ArtKit.Corroded(ArtKit.Iron, 0.15f, 221, 0.35f, ArtKit.Rust), 0.1f);

            foreach (float v in new[] { -5f, 6f })
            {
                for (float x = bx - 9f; x <= bx + 9f; x += 1f)
                {
                    c.Set((int)x, (int)(by + v), ArtKit.Tone(ArtKit.Brass, 0.4f - (x - bx) * 0.07f));
                }

                for (float x = bx - 7f; x <= bx + 8f; x += 4f) c.Set((int)x, (int)(by + v), ArtKit.Brass[3]);
            }

            // O manometro, com o ponteiro tremendo.
            float gx = bx + 3f;
            float gy = by + 0.5f;
            ArtKit.Ball(c, gx, gy, 4f, 4f, ArtKit.Brass, 0.3f);
            ArtKit.Ball(c, gx, gy, 2.8f, 2.8f, new[] { new Rgba(150, 140, 110), new Rgba(196, 186, 156), new Rgba(226, 218, 190), new Rgba(244, 240, 220) }, 0.5f);
            float[] needle = { -60f, -50f, -64f, -40f, -55f, -70f, -45f, -58f };
            (float nx, float ny) = ArtKit.Along(gx, gy, needle[frame], 2.4f);
            ArtKit.Line(c, (int)gx, (int)gy, (int)Math.Round(nx), (int)Math.Round(ny), new Rgba(170, 40, 30));

            // A valvula de alivio no ombro.
            ArtKit.Limb(c, bx - 6f, by - 8f, bx - 6f, by - 11f, 1.2f, ArtKit.Brass, 0.2f);
            ArtKit.Drum(c, bx - 6f, by - 11.5f, 2.2f, 1f, 3f, ArtKit.Plain(ArtKit.Copper), 0.3f);

            // A cabeca: um parafuso sextavado, com os oculos na testa.
            float hx = 24f;
            float hy = 14f + b * 0.5f;
            var hex = new (float, float)[6];
            for (int i = 0; i < 6; i++)
            {
                (float x, float y) = ArtKit.Along(hx, hy, i * 60f, 7.5f);
                hex[i] = (x, y);
            }

            ArtKit.Polygon(c, hex, (x, y) =>
            {
                // Cada face do sextavado com um tom.
                double a = Math.Atan2(y - hy, x - hx) * 180.0 / Math.PI;
                int face = (int)Math.Floor((a + 360.0 + 30.0) / 60.0) % 6;
                float[] light = { -0.1f, -0.5f, -0.3f, 0.3f, 0.75f, 0.4f };
                return ArtKit.Tone(ArtKit.Tin, light[face]);
            });

            // A faixa dos oculos e as duas lentes, empurradas pra cima.
            c.Fill((int)hx - 7, (int)hy - 4, 15, 1, ArtKit.Leather[1]);
            ArtKit.Ball(c, hx + 1f, hy - 4f, 2.2f, 2.2f, ArtKit.Brass, 0.2f);
            ArtKit.Ball(c, hx + 5f, hy - 4f, 2.2f, 2.2f, ArtKit.Brass, 0.2f);
            c.Set((int)hx + 1, (int)hy - 4, new Rgba(140, 200, 200));
            c.Set((int)hx + 5, (int)hy - 4, new Rgba(140, 200, 200));

            // Os olhos: duas frestas largas, de quem ja viu muita caldeira estourar.
            Eye(c, (int)hx + 1, (int)hy + 1, 3, 1, blink, ArtKit.Iron[0]);
            Eye(c, (int)hx + 5, (int)hy + 1, 2, 1, blink, ArtKit.Iron[0]);

            // A boca: uma grade de tres frestas.
            for (int i = 0; i < 3; i++) c.Set((int)hx + 2 + i * 2, (int)hy + 4, ArtKit.Iron[0]);

            Grime(c, 10, 6, 28, 40, 223, 3);

            // O braco da frente e a chave inglesa, apoiada no ombro.
            ArtKit.Ball(c, 32f, 25f + b, 3f, 3f, ArtKit.Rust, 0.2f);
            ArtKit.Limb(c, 32f, 25f + b, 36f, 31f + b, 2.2f, ArtKit.Iron, 0.1f);
            ArtKit.Limb(c, 37f, 40f + b, 37f, 20f + b, 1.6f, ArtKit.Tin, 0.2f);
            ArtKit.Ball(c, 37f, 18.5f + b, 3f, 2.5f, ArtKit.Tin, 0.2f);
            c.Fill(36, 15 + (int)b, 2, 3, Rgba.Clear);
            ArtKit.Ball(c, 37f, 31f + b, 2.6f, 2.6f, ArtKit.Iron, 0.2f);

            ArtKit.AddOutline(c);

            // De vez em quando a valvula solta um fiapo de vapor.
            if (frame == 3 || frame == 4)
            {
                var white = new Rgba(236, 240, 236);
                var grey = new Rgba(186, 194, 192);
                ArtKit.Puff(c, bx - 6f, by - 15f - (frame - 3) * 3f, 2f + (frame - 3), white, grey, frame == 3 ? 170 : 110);
            }
        }

        // ------------------------------------------------------------------
        // 3. Pequena, a lata de conserva
        // ------------------------------------------------------------------

        private static void Pequena(PixelCanvas c, int frame)
        {
            float b = Breath[frame] * 0.5f;
            bool blink = frame == BlinkFrame;

            Leg(c, 22f, 40f, 21f, ArtKit.Tin, ArtKit.Rubber, 1.4f, true);
            Leg(c, 26f, 40f, 27f, ArtKit.Tin, ArtKit.Rubber, 1.5f, false);

            // O corpo: a lata, com os frisos e o rotulo desbotado.
            float bx = 24f;
            float by = 35f + b;
            ArtKit.Drum(c, bx, by, 6f, 6.5f, 4f, ArtKit.Corroded(ArtKit.Tin, 0.12f, 231, 0.45f), 0.15f);
            for (float x = bx - 5f; x <= bx + 5f; x += 1f)
            {
                c.Set((int)x, (int)(by - 5f), ArtKit.Tin[3]);
                c.Set((int)x, (int)(by + 5f), ArtKit.Tin[0]);
                c.Set((int)x, (int)(by - 1f), ArtKit.Tone(new[] { new Rgba(110, 50, 44), new Rgba(150, 70, 56), new Rgba(186, 96, 74), new Rgba(210, 130, 100) }, 0.4f - (x - bx) * 0.12f));
                c.Set((int)x, (int)by, ArtKit.Tone(new[] { new Rgba(110, 50, 44), new Rgba(150, 70, 56), new Rgba(186, 96, 74), new Rgba(210, 130, 100) }, 0.1f - (x - bx) * 0.12f));
                c.Set((int)x, (int)(by + 1f), new Rgba(200, 180, 130));
            }

            // O lencinho vermelho.
            c.Fill((int)bx - 5, (int)(by - 7f), 10, 2, new Rgba(170, 60, 50));
            c.Fill((int)bx - 5, (int)(by - 6f), 10, 1, new Rgba(120, 40, 36));
            c.Set((int)bx - 6, (int)(by - 5f), new Rgba(150, 52, 44));
            c.Set((int)bx - 7, (int)(by - 4f), new Rgba(120, 40, 36));

            // A cabeca: uma tampa de lata, redonda, de olhos grandes.
            float hx = 24.5f;
            float hy = 24.5f + b;
            ArtKit.Ball(c, hx, hy, 6.5f, 5.5f, ArtKit.Corroded(ArtKit.Copper, 0.1f, 232, 0.5f), 0.2f);
            c.Fill((int)hx - 6, (int)hy + 3, 13, 1, ArtKit.Copper[0]);

            // A antena torta, com a lampadinha.
            float[] sway = { 0f, 0.5f, 1f, 1.5f, 1f, 0.5f, 0f, -0.5f };
            float ax = hx - 2f + sway[frame];
            ArtKit.Line(c, (int)hx - 2, (int)hy - 5, (int)hx - 3, (int)hy - 8, ArtKit.Iron[2]);
            ArtKit.Line(c, (int)hx - 3, (int)hy - 8, (int)Math.Round(ax), (int)hy - 11, ArtKit.Iron[2]);
            ArtKit.Ball(c, ax + 0.5f, hy - 12f, 1.5f, 1.5f, new[] { new Rgba(170, 120, 40), new Rgba(230, 180, 80), new Rgba(255, 220, 130), new Rgba(255, 246, 210) }, 0.4f);

            // Os olhos grandes.
            if (blink)
            {
                c.Fill((int)hx, (int)hy, 3, 1, EyeGlow);
                c.Fill((int)hx + 4, (int)hy, 2, 1, EyeGlow);
            }
            else
            {
                c.Fill((int)hx - 1, (int)hy - 2, 5, 5, ArtKit.Iron[0]);
                c.Fill((int)hx + 3, (int)hy - 2, 4, 5, ArtKit.Iron[0]);
                c.Fill((int)hx, (int)hy - 1, 3, 3, new Rgba(150, 230, 250));
                c.Fill((int)hx + 4, (int)hy - 1, 2, 3, new Rgba(150, 230, 250));
                c.Set((int)hx, (int)hy - 1, new Rgba(255, 255, 255));
                c.Set((int)hx + 4, (int)hy - 1, new Rgba(255, 255, 255));
            }

            // O braco segurando o giz, que ela gira entre os dedos.
            float[] twirl = { 60f, 40f, 20f, 0f, -20f, 0f, 30f, 50f };
            float hx2 = 32f;
            float hy2 = 33f + b;
            ArtKit.Limb(c, bx + 4f, by - 3f, hx2, hy2, 1.2f, ArtKit.Tin, 0.1f);
            ArtKit.Ball(c, hx2, hy2, 1.7f, 1.7f, ArtKit.Copper, 0.2f);
            (float gx, float gy) = ArtKit.Along(hx2, hy2, twirl[frame] - 90f, 4f);
            ArtKit.Limb(c, hx2, hy2, gx, gy, 0.8f, new[] { new Rgba(180, 176, 168), new Rgba(214, 210, 200), new Rgba(236, 234, 226), new Rgba(250, 250, 246) }, 0.3f);

            ArtKit.AddOutline(c);

            // A lampadinha acesa.
            ArtKit.Glow(c, ax + 0.5f, hy - 12f, 3.5f, new Rgba(255, 220, 130), 0.4f, true);
        }
    }
}
