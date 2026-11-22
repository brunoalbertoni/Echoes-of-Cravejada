using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class DecorArt
    {
        // ------------------------------------------------------------------
        // Na parede
        // ------------------------------------------------------------------

        /// <summary>
        /// A Calha de Descarte N.º 1, 64x64: um tubo grosso de ferro rebitado
        /// que desce do teto e abre num funil amassado, com a faixa de perigo,
        /// a plaquinha do numero e o verde escorrendo da boca.
        /// </summary>
        private static PixelCanvas DrawChute()
        {
            var c = new PixelCanvas(64, 64);
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Iron, 0.22f, 301, 0.18f, ArtKit.Rust);

            // O tubo, descendo do teto.
            for (int py = 0; py < 38; py++)
            {
                for (int px = 20; px < 44; px++)
                {
                    float u = (px + 0.5f - 32f) / 12f;
                    float light = -0.9f * u + 0.35f * (float)Math.Sqrt(Math.Max(0f, 1f - u * u)) - 0.1f;
                    c.Set(px, py, iron(px - 32f, py, light));
                }
            }

            // As cintas rebitadas do tubo.
            foreach (int band in new[] { 6, 22 })
            {
                for (int px = 19; px < 45; px++)
                {
                    float u = (px + 0.5f - 32f) / 13f;
                    c.Set(px, band, ArtKit.Tone(ArtKit.Iron, 0.6f - u));
                    c.Set(px, band + 1, ArtKit.Tone(ArtKit.Iron, 0.1f - u));
                    c.Set(px, band + 2, ArtKit.Tone(ArtKit.Iron, -0.5f - u));
                    if (px % 4 == 1) c.Set(px, band + 1, ArtKit.Iron[3]);
                }
            }

            // O funil: abre ate 60 de largura, amassado de um lado.
            for (int py = 38; py < 56; py++)
            {
                float t = (py - 38f) / 17f;
                float half = 12f + t * t * 18f;
                float dent = py > 48 ? (py - 48) * 0.6f : 0f;

                for (int px = (int)(32f - half); px < (int)(32f + half - dent); px++)
                {
                    float u = (px + 0.5f - 32f) / half;
                    float light = -0.8f * u + 0.3f - t * 0.25f;
                    c.Set(px, py, iron(px - 32f, py, light));
                }
            }

            // A boca: o escuro de dentro e a borda.
            for (int px = 4; px < 56; px++)
            {
                float u = (px + 0.5f - 30f) / 26f;
                if (Math.Abs(u) > 1f) continue;
                int lip = 55 + (int)(Math.Sqrt(1f - u * u) * 4f);
                for (int py = 55; py <= lip; py++) c.Set(px, py, py == lip ? ArtKit.Iron[2] : ArtKit.Iron[0]);
                c.Set(px, 54, ArtKit.Tone(ArtKit.Iron, 0.7f - u));
            }

            // A faixa de perigo, em diagonais amarelas e pretas, ja descascando.
            var yellow = new Rgba(196, 160, 60);
            var black = new Rgba(40, 34, 36);
            for (int py = 28; py < 34; py++)
            {
                for (int px = 21; px < 43; px++)
                {
                    bool stripe = ((px + py) / 4) % 2 == 0;
                    if (ArtKit.Hash(px, py, 307) % 9 == 0) continue;
                    c.Set(px, py, stripe ? yellow : black);
                }
            }

            // A plaquinha do numero 1.
            c.Fill(27, 12, 10, 8, ArtKit.Brass[1]);
            c.Fill(27, 12, 10, 1, ArtKit.Brass[3]);
            c.Fill(27, 19, 10, 1, ArtKit.Brass[0]);
            c.Fill(31, 13, 2, 6, Ink);
            c.Set(30, 14, Ink);
            c.Fill(30, 18, 4, 1, Ink);

            // O verde escorrendo pela boca.
            foreach ((int x, int length) in new[] { (12, 6), (19, 9), (37, 5), (46, 7) })
            {
                for (int i = 0; i < length; i++) c.Set(x, 56 + i, i == length - 1 ? ArtKit.OxideCore : ArtKit.OxideGlow);
                c.Set(x + 1, 56, ArtKit.OxideDeep);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma viga em I rebitada, 16x16, em pe ou deitada. Repete.</summary>
        private static PixelCanvas DrawGirder(bool vertical)
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Rust, 0.18f, vertical ? 311 : 312, 0.3f);

            for (int a = 0; a < 16; a++)
            {
                for (int b = 2; b < 14; b++)
                {
                    // b atravessa a viga; a corre ao longo dela.
                    bool flange = b < 5 || b > 10;
                    float light = flange ? (b < 5 ? 0.5f - b * 0.15f : -0.3f - (b - 11) * 0.15f) : -0.35f;
                    if (b == 2) light = 0.8f;
                    if (b == 13) light = -0.9f;

                    Rgba color = iron(a, b, light);

                    // Os rebites das abas, a cada 4.
                    if (flange && (b == 3 || b == 12) && a % 4 == 1) color = ArtKit.Rust[3];

                    // A alma vazada em losango, a cada 8.
                    if (!flange && Math.Abs((a % 8) - 4) + Math.Abs(b - 7.5f) < 2.6f) color = Rgba.Clear;

                    if (vertical) c.Set(b, a, color);
                    else c.Set(a, b, color);
                }
            }

            return c;
        }

        /// <summary>Um trecho de cano grosso, 16x16, em pe ou deitado. Repete.</summary>
        private static PixelCanvas DrawPipe(bool vertical)
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Skin skin = ArtKit.Corroded(ArtKit.Copper, 0.35f, vertical ? 321 : 322, 0.35f);

            for (int a = 0; a < 16; a++)
            {
                for (int b = 3; b < 13; b++)
                {
                    float u = (b + 0.5f - 8f) / 5f;
                    float light = -0.9f * u + 0.3f * (float)Math.Sqrt(Math.Max(0f, 1f - u * u));
                    if (b == 3 || b == 12) light = -0.9f;
                    Rgba color = skin(a, b, light);
                    if (vertical) c.Set(b, a, color);
                    else c.Set(a, b, color);
                }
            }

            return c;
        }

        /// <summary>A junta de dois canos: um flange de parafusos e uma valvula de volante.</summary>
        private static PixelCanvas DrawFlange()
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Drum(c, 8f, 8f, 7f, 4f, 3f, ArtKit.Plain(ArtKit.Iron), 0.1f);
            for (int i = 0; i < 4; i++) ArtKit.Rivet(c, 3 + i * 3, 7, ArtKit.Brass);

            // O volante da valvula, por cima.
            ArtKit.Limb(c, 8f, 4f, 8f, 1f, 0.8f, ArtKit.Iron, 0.2f);
            for (int i = 0; i < 16; i++)
            {
                double a = i * Math.PI / 8.0;
                c.Set((int)Math.Round(8 + Math.Cos(a) * 4), (int)Math.Round(1 + Math.Sin(a) * 1.3), ArtKit.Brass[i < 8 ? 1 : 3]);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>A curva do cano, 16x16: entra por baixo e sai pra direita.</summary>
        private static PixelCanvas DrawElbow()
        {
            var c = new PixelCanvas(16, 16);
            for (int py = 0; py < 16; py++)
            {
                for (int px = 0; px < 16; px++)
                {
                    float dx = px + 0.5f - 16f;
                    float dy = py + 0.5f - 16f;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r < 3f || r > 13f) continue;
                    float u = (r - 8f) / 5f;
                    c.Set(px, py, ArtKit.Tone(ArtKit.Copper, u * 0.9f + 0.1f));
                }
            }

            return c;
        }

        /// <summary>Uma engrenagem enorme, presa na rocha, com raios vazados e o cubo rebitado.</summary>
        private static PixelCanvas DrawGear(int size, int teeth, float tip, float root, int seed)
        {
            var c = new PixelCanvas(size, size);
            float cx = size * 0.5f;
            float cy = size * 0.5f;
            float rim = root - size * 0.12f;
            float hub = size * 0.12f;
            ArtKit.Skin metal = ArtKit.Corroded(ArtKit.Iron, 0.25f, seed, 0.2f, ArtKit.Rust);

            for (int py = 0; py < size; py++)
            {
                for (int px = 0; px < size; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    double angle = Math.Atan2(dy, dx);
                    bool tooth = Math.Cos(angle * teeth) > 0.25;
                    if (r > (tooth ? tip : root)) continue;

                    // Os raios: seis, com o vazio entre eles.
                    bool spoke = Math.Abs(Math.IEEERemainder(angle, Math.PI / 3.0)) * r < size * 0.06f;
                    if (r < rim && r > hub && !spoke) continue;

                    float light = -(dx * 0.55f + dy * 0.65f) / Math.Max(1f, r);
                    float bevel = r > rim - 1.5f && r < rim + 1.5f ? -light : light * 0.6f;
                    c.Set(px, py, metal(dx, dy, bevel));
                }
            }

            // Os rebites do cubo.
            for (int i = 0; i < 6; i++)
            {
                double a = i * Math.PI / 3.0 + Math.PI / 6.0;
                ArtKit.Rivet(c, (int)(cx + Math.Cos(a) * hub * 0.6f) - 1, (int)(cy + Math.Sin(a) * hub * 0.6f) - 1, ArtKit.Brass);
            }

            c.Set((int)cx, (int)cy, Ink);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Um trecho de corrente, 16x16, com os elos no meio. Repete na vertical.</summary>
        private static PixelCanvas DrawChain()
        {
            var c = new PixelCanvas(16, 16);
            for (int link = 0; link < 2; link++)
            {
                int y = link * 8;
                if (link == 0)
                {
                    // Elo de frente: oval vazado.
                    for (int py = 0; py < 8; py++)
                    {
                        for (int px = 5; px < 11; px++)
                        {
                            float dx = (px + 0.5f - 8f) / 3f;
                            float dy = (py + 0.5f - 4f) / 4f;
                            float d = dx * dx + dy * dy;
                            if (d > 1f || d < 0.35f) continue;
                            c.Set(px, y + py, ArtKit.Tone(ArtKit.Iron, -dx * 0.8f + 0.2f));
                        }
                    }
                }
                else
                {
                    // Elo de lado: uma barra.
                    for (int py = -1; py < 9; py++) c.Set(7, y + py, ArtKit.Iron[3]);
                    for (int py = -1; py < 9; py++) c.Set(8, y + py, ArtKit.Iron[1]);
                }
            }

            return c;
        }

        /// <summary>O gancho de guindaste na ponta da corrente, 16x16, pendurado pelo alto.</summary>
        private static PixelCanvas DrawHook()
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Limb(c, 8f, 0f, 8f, 6f, 1.3f, ArtKit.Iron, 0.2f);
            ArtKit.Drum(c, 8f, 4f, 3f, 2f, 3f, ArtKit.Plain(ArtKit.Rust), 0.2f);

            for (int i = 0; i <= 12; i++)
            {
                double a = Math.PI * 0.1 + i * Math.PI * 1.15 / 12.0;
                float x = 8f - (float)Math.Cos(a) * 3.6f;
                float y = 10f + (float)Math.Sin(a) * 3.6f;
                ArtKit.Ball(c, x, y, 1.3f, 1.3f, ArtKit.Iron, 0.1f);
            }

            c.Set(12, 8, ArtKit.Tin[3]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma gaiola pendurada, 16x24: barras de ferro, o fundo amassado e uma cabeca de autômato la dentro.</summary>
        private static PixelCanvas DrawCage()
        {
            var c = new PixelCanvas(16, 24);

            // A cabeca la dentro.
            ArtKit.Ball(c, 8f, 18f, 3.5f, 3f, ArtKit.Corroded(ArtKit.Copper, 0.3f, 331, 0.5f), 0.1f);
            c.Set(7, 18, Ink);
            c.Set(10, 18, Ink);

            // O argolao, o teto em cupula e as barras.
            ArtKit.Limb(c, 8f, 0f, 8f, 3f, 0.8f, ArtKit.Iron, 0.2f);
            for (int px = 2; px <= 13; px++)
            {
                float u = (px - 7.5f) / 6f;
                int top = 3 + (int)(Math.Abs(u) * Math.Abs(u) * 3f);
                c.Set(px, top, ArtKit.Tone(ArtKit.Iron, 0.5f - u));
                c.Set(px, top + 1, ArtKit.Iron[1]);
            }

            foreach (int x in new[] { 2, 5, 8, 11, 13 })
            {
                for (int py = 6; py < 22; py++) c.Set(x, py, x < 8 ? ArtKit.Iron[3] : ArtKit.Iron[2]);
            }

            c.Fill(2, 21, 12, 2, ArtKit.Rust[1]);
            c.Fill(2, 21, 12, 1, ArtKit.Rust[2]);
            c.Fill(1, 13, 14, 1, ArtKit.Iron[1]);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// Um cartaz da Coroa, 24x32, rasgado e manchado. O da coroa mostra a
        /// coroa de prata e o "925"; o do decreto, o selo e linhas de texto.
        /// </summary>
        private static PixelCanvas DrawPoster(bool decree)
        {
            var c = new PixelCanvas(24, 32);
            var paper = decree ? new Rgba(196, 184, 150) : new Rgba(70, 70, 96);
            var paperDark = decree ? new Rgba(150, 136, 104) : new Rgba(46, 46, 68);
            var silver = new Rgba(214, 220, 228);
            var silverDark = new Rgba(150, 156, 170);

            for (int py = 0; py < 32; py++)
            {
                for (int px = 0; px < 24; px++)
                {
                    // Rasgado: a ponta de baixo a direita arrancada, a borda irregular.
                    if (px + (31 - py) * 0.9f < 6f - ArtKit.Hash(px, py, 341) % 3) continue;
                    if (py > 25 && px > 15 && (px - 15) + (py - 25) > 6) continue;
                    if ((px == 0 || px == 23 || py == 0 || py == 31) && ArtKit.Hash(px, py, 342) % 3 == 0) continue;

                    Rgba color = (px + py) % 11 == 0 ? paperDark : paper;
                    if (ArtKit.Noise(px * 0.3f, py * 0.3f, 343) > 0.78f) color = Rgba.Lerp(color, ArtKit.Verdigris[1], 0.5f);
                    c.Set(px, py, color);
                }
            }

            // A borda de prata.
            for (int px = 2; px < 22; px++)
            {
                if (c.Get(px, 2).A > 0) c.Set(px, 2, silverDark);
                if (c.Get(px, 29).A > 0) c.Set(px, 29, silverDark);
            }

            if (!decree)
            {
                // A coroa: tres pontas sobre a faixa.
                c.Fill(6, 11, 12, 3, silver);
                c.Fill(6, 13, 12, 1, silverDark);
                foreach (int x in new[] { 6, 11, 16 })
                {
                    c.Fill(x, 7, 2, 4, silver);
                    c.Set(x, 6, silver);
                }

                c.Set(11, 12, new Rgba(120, 180, 230));
                c.Set(7, 12, new Rgba(120, 180, 230));
                c.Set(16, 12, new Rgba(120, 180, 230));

                // 925, em algarismos de 3x5.
                DrawDigits(c, "925", 6, 17, silver);

                // Duas linhas de letra miuda.
                for (int x = 4; x < 20; x++)
                {
                    if (x % 4 != 3) c.Set(x, 24, silverDark);
                    if (x % 5 != 4 && x < 17) c.Set(x, 26, silverDark);
                }
            }
            else
            {
                // O selo de cera prateada e as linhas do texto.
                ArtKit.Ball(c, 12f, 8f, 4f, 4f, new[] { silverDark, new Rgba(176, 182, 194), silver, new Rgba(240, 244, 250) }, 0.2f);
                c.Set(12, 8, silverDark);
                var ink = new Rgba(60, 50, 44);
                for (int line = 0; line < 6; line++)
                {
                    int y = 15 + line * 2;
                    int length = line == 5 ? 9 : 16;
                    for (int x = 4; x < 4 + length; x++)
                    {
                        if ((x + line * 2) % 6 == 5) continue;
                        if (c.Get(x, y).A > 0) c.Set(x, y, ink);
                    }
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }


        private static void DrawDigits(PixelCanvas c, string text, int x, int y, Rgba color)
        {
            foreach (char ch in text)
            {
                string[] glyph = Font[ch];
                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (glyph[row][col] == '#') c.Set(x + col, y + row, color);
                    }
                }

                x += 4;
            }
        }

        /// <summary>Uma chapa de perigo, 32x8: diagonais amarelas e pretas, descascadas.</summary>
        private static PixelCanvas DrawHazard()
        {
            var c = new PixelCanvas(32, 8);
            var yellow = new Rgba(196, 160, 60);
            var black = new Rgba(40, 34, 36);

            for (int py = 1; py < 7; py++)
            {
                for (int px = 0; px < 32; px++)
                {
                    bool stripe = ((px + py) / 4) % 2 == 0;
                    Rgba color = stripe ? yellow : black;
                    if (ArtKit.Noise(px * 0.4f, py * 0.4f, 351) > 0.72f) color = ArtKit.Rust[1];
                    c.Set(px, py, color);
                }
            }

            for (int px = 0; px < 32; px++)
            {
                c.Set(px, 0, ArtKit.Iron[2]);
                c.Set(px, 7, ArtKit.Iron[0]);
            }

            ArtKit.Rivet(c, 1, 3, ArtKit.Iron);
            ArtKit.Rivet(c, 29, 3, ArtKit.Iron);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Uma seta pintada a mao na rocha, 16x16, apontando pra cima: a saida.</summary>
        private static PixelCanvas DrawArrow()
        {
            var c = new PixelCanvas(16, 16);
            var paint = new Rgba(220, 210, 180, 220);
            var faint = new Rgba(220, 210, 180, 120);

            for (int py = 2; py < 15; py++)
            {
                int w = py < 8 ? py - 1 : 2;
                for (int px = 8 - w; px < 8 + w; px++)
                {
                    bool drip = ArtKit.Hash(px, py, 361) % 7 == 0;
                    c.Set(px, py, drip ? faint : paint);
                }
            }

            c.Set(6, 15, faint);
            return c;
        }

        /// <summary>Um respiro de parede, 32x16: grade de ferro com o verde do Oxide pulsando atras.</summary>
        private static PixelCanvas DrawVent()
        {
            var c = new PixelCanvas(32, 16);
            for (int py = 1; py < 15; py++)
            {
                for (int px = 1; px < 31; px++)
                {
                    bool frame = px < 3 || px > 28 || py < 3 || py > 12;
                    if (frame)
                    {
                        c.Set(px, py, ArtKit.Tone(ArtKit.Iron, py < 3 ? 0.6f : (px < 3 ? 0.2f : -0.5f)));
                        continue;
                    }

                    bool slat = (px - 3) % 4 == 0;
                    float depth = (py - 3) / 9f;
                    Rgba inside = depth > 0.75f ? ArtKit.OxideGlow : (depth > 0.4f ? ArtKit.OxideDeep : ArtKit.Iron[0]);
                    c.Set(px, py, slat ? ArtKit.Iron[2] : inside);
                }
            }

            ArtKit.Rivet(c, 1, 1, ArtKit.Iron);
            ArtKit.Rivet(c, 29, 1, ArtKit.Iron);
            ArtKit.Rivet(c, 1, 13, ArtKit.Iron);
            ArtKit.Rivet(c, 29, 13, ArtKit.Iron);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Um trecho da esteira da Triagem, 16x16: lona remendada em cima dos roletes. Repete na horizontal.</summary>
        private static PixelCanvas DrawConveyor()
        {
            var c = new PixelCanvas(16, 16);

            // A lona.
            for (int px = 0; px < 16; px++)
            {
                c.Set(px, 5, ArtKit.Rubber[3]);
                c.Set(px, 6, ArtKit.Rubber[2]);
                c.Set(px, 7, px % 4 == 0 ? ArtKit.Rubber[0] : ArtKit.Rubber[1]);
            }

            // Os roletes e a armacao.
            ArtKit.Ball(c, 4f, 9.5f, 2.2f, 2.2f, ArtKit.Iron, 0.2f);
            ArtKit.Ball(c, 12f, 9.5f, 2.2f, 2.2f, ArtKit.Iron, 0.2f);
            for (int px = 0; px < 16; px++)
            {
                c.Set(px, 12, ArtKit.Rust[2]);
                c.Set(px, 13, ArtKit.Rust[1]);
            }

            // As pernas da armacao.
            for (int py = 13; py < 16; py++)
            {
                c.Set(1, py, ArtKit.Rust[1]);
                c.Set(2, py, ArtKit.Rust[0]);
            }

            // Uma pecinha esquecida em cima da lona.
            c.Set(10, 4, ArtKit.Brass[2]);
            c.Set(11, 4, ArtKit.Brass[1]);
            return c;
        }

        /// <summary>O motor da ponta da esteira, 24x24: caixa de engrenagens com a manivela.</summary>
        private static PixelCanvas DrawConveyorMotor()
        {
            var c = new PixelCanvas(24, 24);
            ArtKit.Drum(c, 12f, 15f, 9f, 7f, 4f, ArtKit.Corroded(ArtKit.Iron, 0.2f, 371, 0.3f, ArtKit.Rust), 0.1f);
            ArtKit.Ball(c, 12f, 13f, 4.5f, 4.5f, ArtKit.Brass, 0.1f);
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4.0;
                c.Set((int)(12 + Math.Cos(a) * 5), (int)(13 + Math.Sin(a) * 5), ArtKit.Brass[0]);
            }

            ArtKit.Limb(c, 12f, 13f, 19f, 6f, 1f, ArtKit.Iron, 0.2f);
            ArtKit.Ball(c, 19f, 5.5f, 1.6f, 1.6f, ArtKit.Leather, 0.2f);
            c.Fill(3, 22, 18, 2, ArtKit.Iron[0]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Tres cestos de triagem, 48x24, com marcas de giz: o bom, o ruim e o que ninguem escreveu.</summary>
        private static PixelCanvas DrawBins()
        {
            var c = new PixelCanvas(48, 24);
            var chalk = new Rgba(226, 220, 204);

            for (int bin = 0; bin < 3; bin++)
            {
                int x0 = bin * 16;
                ArtKit.Skin wood = ArtKit.Corroded(ArtKit.Leather, 0.12f, 381 + bin, 0.4f);

                for (int py = 8; py < 24; py++)
                {
                    for (int px = x0 + 1; px < x0 + 15; px++)
                    {
                        bool plank = (py - 8) % 4 == 3;
                        float light = px == x0 + 1 ? 0.6f : (px == x0 + 14 ? -0.6f : (plank ? -0.4f : 0.1f));
                        c.Set(px, py, wood(px, py, light));
                    }
                }

                // O que tem dentro, saindo pela boca.
                if (bin == 0)
                {
                    ArtKit.Ball(c, x0 + 5f, 7f, 3f, 3f, ArtKit.Brass, 0.2f);
                    ArtKit.Ball(c, x0 + 10f, 6f, 2.5f, 2.5f, ArtKit.Brass, 0.1f);
                }
                else if (bin == 1)
                {
                    ArtKit.Limb(c, x0 + 3f, 8f, x0 + 11f, 3f, 1.3f, ArtKit.Iron, 0.1f);
                    ArtKit.Ball(c, x0 + 9f, 7f, 3f, 2f, ArtKit.Rust, 0f);
                }

                // As marcas de giz: um circulo, um X, e um ponto de interrogacao borrado.
                int mx = x0 + 6;
                if (bin == 0)
                {
                    foreach ((int dx, int dy) in new[] { (1, 0), (2, 0), (0, 1), (3, 1), (0, 2), (3, 2), (1, 3), (2, 3) }) c.Set(mx + dx, 14 + dy, chalk);
                }
                else if (bin == 1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        c.Set(mx + i, 14 + i, chalk);
                        c.Set(mx + 3 - i, 14 + i, chalk);
                    }
                }
                else
                {
                    foreach ((int dx, int dy) in new[] { (0, 0), (1, 0), (2, 0), (2, 1), (1, 2), (1, 4) }) c.Set(mx + dx, 13 + dy, chalk.WithAlpha(150));
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Um andaime de ferro, 32x48: dois montantes, travessas e um X de reforco, um pedaco caido.</summary>
        private static PixelCanvas DrawScaffold()
        {
            var c = new PixelCanvas(32, 48);
            ArtKit.Skin iron = ArtKit.Corroded(ArtKit.Rust, 0.2f, 391, 0.3f);

            ArtKit.Limb(c, 3f, 0f, 3f, 48f, 1.6f, iron, 0.2f);
            ArtKit.Limb(c, 28f, 4f, 28f, 48f, 1.6f, iron, 0f);

            foreach (int y in new[] { 6, 22, 38 })
            {
                ArtKit.Limb(c, 3f, y, 28f, y, 1.2f, iron, 0.1f);
                ArtKit.Rivet(c, 2, y - 1, ArtKit.Iron);
                ArtKit.Rivet(c, 27, y - 1, ArtKit.Iron);
            }

            ArtKit.Line(c, 4, 7, 27, 21, ArtKit.Rust[1]);
            ArtKit.Line(c, 27, 7, 4, 21, ArtKit.Rust[2]);
            ArtKit.Line(c, 4, 23, 27, 37, ArtKit.Rust[1]);

            // A tabua do patamar de cima, partida.
            c.Fill(4, 4, 14, 2, ArtKit.Leather[2]);
            c.Fill(4, 5, 14, 1, ArtKit.Leather[0]);
            ArtKit.Line(c, 18, 5, 24, 12, ArtKit.Leather[1]);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Riscos de garra na rocha, 32x24: tres sulcos fundos, em diagonal.</summary>
        private static PixelCanvas DrawClawMarks()
        {
            var c = new PixelCanvas(32, 24);
            var deep = new Rgba(20, 16, 22, 230);
            var lip = new Rgba(160, 170, 160, 140);

            for (int claw = 0; claw < 3; claw++)
            {
                for (int i = 0; i < 20; i++)
                {
                    int x = 4 + claw * 8 + i / 2;
                    int y = 2 + i;
                    int w = i < 3 || i > 16 ? 1 : 2;
                    for (int k = 0; k < w; k++) c.Set(x + k, y, deep);
                    c.Set(x - 1, y, lip);
                }
            }

            return c;
        }

        /// <summary>Marcas de contagem riscadas na parede, 32x16: grupos de cinco, centenas deles.</summary>
        private static PixelCanvas DrawTally()
        {
            var c = new PixelCanvas(32, 16);
            var chalk = new Rgba(210, 206, 190, 130);

            for (int row = 0; row < 2; row++)
            {
                for (int group = 0; group < 5; group++)
                {
                    int x0 = 1 + group * 6 + row;
                    int y0 = 1 + row * 8 + (group % 2);
                    if (row == 1 && group == 3) break;

                    // Quatro riscos tortos, de tamanhos diferentes, e o quinto cortando.
                    for (int i = 0; i < 4; i++)
                    {
                        int length = 4 + ArtKit.Hash(group, row * 4 + i, 501) % 3;
                        int lean = ArtKit.Hash(group, row * 4 + i, 502) % 2;
                        ArtKit.Line(c, x0 + i, y0, x0 + i + lean, y0 + length, chalk);
                    }

                    ArtKit.Line(c, x0 - 1, y0 + 4, x0 + 4, y0 + 1, chalk);
                }
            }

            return c;
        }

        /// <summary>Um cabo caido entre dois isoladores, 64x24, fazendo barriga.</summary>
        private static PixelCanvas DrawCable()
        {
            var c = new PixelCanvas(64, 24);

            for (int px = 2; px < 62; px++)
            {
                float t = (px - 2f) / 59f;
                float sag = 4f + 14f * 4f * t * (1f - t);
                int y = (int)sag;
                c.Set(px, y, ArtKit.Rubber[3]);
                c.Set(px, y + 1, ArtKit.Rubber[1]);
            }

            foreach (int x in new[] { 2, 61 })
            {
                ArtKit.Ball(c, x, 3.5f, 2f, 2.5f, new[] { new Rgba(110, 80, 60), new Rgba(160, 120, 90), new Rgba(200, 160, 120), new Rgba(230, 200, 160) }, 0.2f);
            }

            // Uma ponta partida pendurada, soltando faisca de vez em quando.
            ArtKit.Line(c, 40, 16, 42, 21, ArtKit.Rubber[2]);
            c.Set(42, 22, ArtKit.Copper[3]);
            c.Set(43, 22, ArtKit.Copper[2]);
            return c;
        }

        /// <summary>Uma arandela de cristal quebrada, 16x16, presa na rocha.</summary>
        private static PixelCanvas DrawWallLamp()
        {
            // 16x24: a chapa presa na rocha com dois rebites, o braco de ferro
            // e a lanterna de servico pendurada: chapeu, gaiola de arame e o
            // vidro apagado, rachado, com um caco faltando.
            var c = new PixelCanvas(16, 24);
            c.Fill(1, 4, 4, 10, ArtKit.Iron[1]);
            c.Fill(1, 4, 1, 10, ArtKit.Iron[3]);
            ArtKit.Rivet(c, 2, 5, ArtKit.Iron);
            ArtKit.Rivet(c, 2, 11, ArtKit.Iron);
            ArtKit.Limb(c, 4f, 7f, 11f, 7f, 0.8f, ArtKit.Iron, 0.3f);
            ArtKit.Limb(c, 5f, 11f, 9f, 7.5f, 0.6f, ArtKit.Iron, 0.1f);
            ArtKit.Limb(c, 11f, 7f, 11f, 9f, 0.5f, ArtKit.Brass, 0.3f);

            // O chapeu.
            ArtKit.Drum(c, 11f, 10.5f, 4f, 1.5f, 3f, ArtKit.Plain(ArtKit.Brass), 0.2f);

            // A gaiola com o vidro.
            for (int py = 12; py < 21; py++)
            {
                for (int px = 8; px < 15; px++)
                {
                    bool bar = px == 8 || px == 14 || px == 11 || py == 12 || py == 20;
                    Rgba glass = Rgba.Lerp(new Rgba(70, 92, 96), new Rgba(40, 54, 58), (py - 12) / 8f);
                    c.Set(px, py, bar ? ArtKit.Tone(ArtKit.Brass, px == 8 ? 0.7f : 0.1f) : glass);
                }
            }

            ArtKit.Line(c, 9, 14, 10, 18, new Rgba(26, 34, 38));
            ArtKit.Line(c, 12, 13, 13, 16, new Rgba(26, 34, 38));
            c.Set(9, 13, new Rgba(180, 210, 210));
            c.Set(12, 18, Rgba.Clear);
            c.Set(13, 18, Rgba.Clear);
            c.Set(13, 19, Rgba.Clear);

            // A base e a argola de baixo.
            c.Fill(9, 21, 5, 1, ArtKit.Brass[1]);
            c.Set(11, 22, ArtKit.Brass[2]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
