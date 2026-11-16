using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte da tela de morte, toda em pixel (a tela e montada numa grade de
    /// 320x180 e ampliada inteira):
    ///   - o coracao de sucata grande: inteiro, rachado, partido em dois com a
    ///     brasa da vida morrendo no meio (quadros);
    ///   - o titulo "A GLORIA DE PRATA" em letras de latao com volume, contorno,
    ///     sombra e uma rachadura atravessando;
    ///   - as placas de latao dos botoes (apagada e escolhida);
    ///   - o friso ornamental com a engrenagem.
    ///
    /// Nao depende de UnityEngine: da pra renderizar pelo console.
    /// </summary>
    public static class DeathScreenArt
    {
        public const int ScreenWidth = 320;
        public const int ScreenHeight = 180;

        /// <summary>Os quadros do coracao: inteiro, rachado, partido e quatro de brasa morrendo.</summary>
        public const int HeartWhole = 0;
        public const int HeartCracked = 1;
        public const int HeartSplit = 2;
        public const int HeartEmbers = 3;
        public const int HeartEmberFrames = 4;
        public const int HeartSize = 56;

        public const int ButtonWidth = 156;
        public const int ButtonHeight = 21;

        public static readonly string[] ButtonLabels = { "VOLTAR PARA A BANCADA", "SAIR" };

        private static readonly Rgba Outline = new Rgba(20, 10, 10);

        private static readonly Rgba[] Rust =
            { new Rgba(62, 26, 20), new Rgba(112, 50, 30), new Rgba(160, 84, 46), new Rgba(204, 128, 76) };

        private static readonly Rgba[] Brass =
            { new Rgba(84, 52, 26), new Rgba(140, 96, 44), new Rgba(198, 150, 68), new Rgba(246, 214, 136) };

        private static readonly Rgba[] Tarnish =
            { new Rgba(40, 30, 24), new Rgba(70, 52, 36), new Rgba(98, 76, 50), new Rgba(130, 104, 70) };

        private static readonly Rgba Verdigris = new Rgba(70, 120, 100);
        private static readonly Rgba Ember = new Rgba(255, 118, 48);
        private static readonly Rgba EmberHot = new Rgba(255, 222, 140);
        private static readonly Rgba EmberDark = new Rgba(150, 40, 20);
        private static readonly Rgba Ruby = new Rgba(220, 40, 56);
        private static readonly Rgba RubyLight = new Rgba(255, 140, 140);

        // ------------------------------------------------------------------
        // O coracao
        // ------------------------------------------------------------------

        /// <summary>Todos os quadros do coracao, na ordem das constantes.</summary>
        public static PixelCanvas[] HeartFrames()
        {
            var frames = new List<PixelCanvas>
            {
                Heart(0, 1f, 0),
                Heart(1, 1f, 0),
                Heart(2, 0.9f, 0),
            };

            float[] dying = { 0.75f, 0.55f, 0.7f, 0.45f };
            for (int i = 0; i < HeartEmberFrames; i++) frames.Add(Heart(2, dying[i], i + 1));
            return frames.ToArray();
        }

        /// <param name="stage">0 inteiro, 1 rachado, 2 partido.</param>
        /// <param name="ember">O quanto a brasa ainda brilha (0 a 1).</param>
        /// <param name="seed">Muda as fagulhas de um quadro pro outro.</param>
        private static PixelCanvas Heart(int stage, float ember, int seed)
        {
            const int size = HeartSize;
            var canvas = new PixelCanvas(size, size);

            // As metades se afastam quando ele parte: a esquerda tomba pra esquerda, a direita desce um pouco.
            int spread = stage == 2 ? 4 : 0;

            // A rachadura: um zigue-zague do V de cima ate a ponta de baixo.
            int CrackX(int y) => 28 + (int)Math.Round(Math.Sin(y * 0.55) * 1.6 + ((y / 5) % 2 == 0 ? 1 : -1));

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // O formato classico do coracao, pela equacao (x²+y²-1)³ - x²y³ <= 0.
                    double hx = (x + 0.5 - 28) / 21.0;
                    double hy = -((y + 0.5 - 27) / 21.0) + 0.15;
                    double f = Math.Pow(hx * hx + hy * hy - 1, 3) - hx * hx * hy * hy * hy;
                    if (f > 0) continue;

                    bool left = x < CrackX(y);
                    int dx = stage == 2 ? (left ? -spread : spread) : 0;
                    int dy = stage == 2 && !left ? 1 : 0;

                    // Luz de cima a esquerda, como uma chapa estufada.
                    double nx = hx * 0.9, ny = hy * 0.9;
                    float light = (float)(-0.55 * nx + 0.75 * ny + 0.15);

                    // Ferrugem em manchas: umas chapas mais escuras, outras mais claras.
                    int stain = ((x / 3) * 7 + (y / 3) * 11 + (x / 3) * (y / 3)) % 13;
                    if (stain == 0) light -= 0.6f;
                    else if (stain == 5) light += 0.35f;

                    canvas.Set(x + dx, y + dy, ArtKit.Tone(Rust, light));
                }
            }

            // A costura soldada que corta o coracao de cima a baixo (antes de partir).
            if (stage == 0)
            {
                for (int y = 12; y < 50; y++)
                {
                    int x = 28 + ((y / 4) % 2 == 0 ? 0 : 1);
                    if (canvas.Get(x, y).A > 0) canvas.Set(x, y, Rust[0]);
                    if (y % 4 == 0 && canvas.Get(x - 2, y).A > 0) canvas.Set(x - 2, y, Brass[3]);
                }
            }

            // Os remendos de lata aparafusados e os rebites, presos em cada metade.
            Patch(canvas, 33 + (stage == 2 ? spread : 0), 15 + (stage == 2 ? 1 : 0), 8, 7);
            Patch(canvas, 11 - (stage == 2 ? spread : 0), 26, 6, 6);
            Rivet(canvas, 9 - (stage == 2 ? spread : 0), 13);
            Rivet(canvas, 18 - (stage == 2 ? spread : 0), 38);
            Rivet(canvas, 44 + (stage == 2 ? spread : 0), 30 + (stage == 2 ? 1 : 0));

            // O brilho da chapa no lobo da esquerda.
            for (int i = 0; i < 4; i++) canvas.Set(9 + i - (stage == 2 ? spread : 0), 8 - (i > 1 ? 1 : 0), Rust[3].Scaled(1.15f));

            if (stage >= 1)
            {
                // A rachadura: escura nas bordas, com a brasa da vida no fundo.
                for (int y = 9; y < 52; y++)
                {
                    int x = CrackX(y);
                    if (stage == 1)
                    {
                        if (!InsideAny(canvas, x, y)) continue;
                        canvas.Set(x, y, Outline);
                        canvas.Set(x + 1, y, Rgba.Lerp(EmberDark, Ember, ember));
                    }
                    else
                    {
                        // Partido: o vao entre as metades, com a brasa morrendo no fundo.
                        for (int g = x - spread; g <= x + spread; g++)
                        {
                            bool between = canvas.Get(g - 1, y).A > 0 || canvas.Get(g + 1, y).A > 0 || g == x;
                            if (!between || y < 11 || y > 48) continue;
                            float k = ember * (1f - Math.Abs(g - x) / (float)(spread + 1));
                            Rgba glow = k > 0.55f ? EmberHot : k > 0.3f ? Ember : EmberDark;
                            if (canvas.Get(g, y).A == 0) canvas.Set(g, y, glow.WithAlpha((int)(120 + 135 * Math.Min(1f, k + 0.2f))));
                        }
                    }
                }
            }

            Outline1(canvas);

            // Fagulhas subindo do vao, partido.
            if (stage == 2)
            {
                var rng = new Random(seed * 31 + 7);
                int count = (int)(3 + ember * 4);
                for (int i = 0; i < count; i++)
                {
                    int x = 28 + rng.Next(-5, 6);
                    int y = rng.Next(2, 20);
                    canvas.Blend(x, y, (rng.Next(2) == 0 ? EmberHot : Ember).WithAlpha(rng.Next(140, 255)));
                }
            }

            return canvas;
        }

        private static bool InsideAny(PixelCanvas canvas, int x, int y) => canvas.Get(x, y).A > 0 && canvas.Get(x + 1, y).A > 0;

        private static void Patch(PixelCanvas canvas, int x, int y, int w, int h)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    if (canvas.Get(px, py).A == 0) continue;
                    Rgba tone = py == y ? Tarnish[3] : px == x + w - 1 || py == y + h - 1 ? Tarnish[0] : Tarnish[2];
                    canvas.Set(px, py, tone);
                }
            }

            Rivet(canvas, x + 1, y + 1);
            Rivet(canvas, x + w - 3, y + h - 3);
        }

        private static void Rivet(PixelCanvas canvas, int x, int y)
        {
            if (canvas.Get(x, y).A == 0) return;
            canvas.Set(x, y, Brass[3]);
            canvas.Set(x + 1, y, Brass[1]);
            canvas.Set(x, y + 1, Brass[1]);
            canvas.Set(x + 1, y + 1, Brass[0]);
        }

        // ------------------------------------------------------------------
        // O titulo
        // ------------------------------------------------------------------

        public const string TitleText = "À GLÓRIA DE PRATA";

        /// <summary>O titulo da tela de morte.</summary>
        public static PixelCanvas Title() => Title(TitleText, 3, true);

        /// <summary>
        /// Um texto em latao com volume: degrade de cima pra baixo, brilho,
        /// contorno, sombra e (se <paramref name="cracked"/>) uma rachadura
        /// atravessando. Serve a tela de morte e o menu.
        /// </summary>
        public static PixelCanvas Title(string text, int scale, bool cracked)
        {
            // Em cima, a folga dos acentos (tres pixels da fonte).
            int top = 3 * scale + 2;
            int width = PixelFont.Measure(text, scale) + 8;
            int height = top + PixelFont.GlyphHeight * scale + 6;

            // A mascara das letras.
            var mask = new PixelCanvas(width, height);
            PixelFont.Draw(mask, text, 3, top, new Rgba(255, 255, 255), default, PixelFont.Style.Ink, scale);

            var canvas = new PixelCanvas(width, height);
            int glyphTop = top;
            int glyphBottom = top + PixelFont.GlyphHeight * scale;

            // A sombra, dois pixels embaixo e um a direita, cor de ferrugem queimada.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask.Get(x, y).A == 0) continue;
                    canvas.Set(x + 1, y + 2, new Rgba(70, 20, 14));
                }
            }

            // A rachadura: uma linha torta atravessando as letras, de cima a esquerda pra baixo a direita.
            bool Crack(int x, int y)
            {
                double line = glyphTop + 6 + (x * 0.06) + Math.Sin(x * 0.35) * 1.4;
                return Math.Abs(y - line) < 0.6;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mask.Get(x, y).A == 0 || (cracked && Crack(x, y))) continue;

                    // O degrade do latao: claro em cima, escuro embaixo.
                    float t = Math.Max(0f, (y - glyphTop) / (float)(glyphBottom - glyphTop));
                    Rgba tone = t < 0.3f ? Brass[3] : t < 0.6f ? Brass[2] : Brass[1];

                    // A borda de cima de cada traco acende; a de baixo escurece.
                    if (mask.Get(x, y - 1).A == 0) tone = new Rgba(255, 238, 180);
                    else if (mask.Get(x, y + 1).A == 0) tone = Brass[0];

                    // Ferrugem comendo a base das letras.
                    if (t > 0.7f && ((x * 5 + y * 3) % 7 == 0)) tone = Rust[2];

                    canvas.Set(x, y, tone);
                }
            }

            Outline1(canvas);
            return canvas;
        }

        // ------------------------------------------------------------------
        // As placas
        // ------------------------------------------------------------------

        /// <summary>A placa de um botao: apagada (latao oxidado) ou escolhida (polida, com rubis acesos nas pontas).</summary>
        public static PixelCanvas Button(string label, bool selected)
        {
            const int w = ButtonWidth;
            const int h = ButtonHeight;
            var canvas = new PixelCanvas(w, h);
            Rgba[] ramp = selected ? Brass : Tarnish;

            // A placa, com os cantos chanfrados.
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int cut = 3;
                    bool corner = (x + y < cut) || (w - 1 - x + y < cut) || (x + h - 1 - y < cut) || (w - 1 - x + h - 1 - y < cut);
                    if (corner) continue;

                    // Polida, com um reflexo diagonal; oxidada, manchada de azinhavre.
                    Rgba tone = ramp[2];
                    if (y <= 1) tone = ramp[3];
                    else if (y >= h - 2) tone = ramp[0];
                    else if (selected && ((x - y * 2) % 41 + 41) % 41 < 3) tone = ramp[3];
                    else if (!selected && ((x * 7 + y * 13) % 23 == 0)) tone = Verdigris;
                    else if (y >= h - 5) tone = ramp[1];

                    canvas.Set(x, y, tone);
                }
            }

            // O friso de dentro.
            for (int x = 4; x < w - 4; x++)
            {
                canvas.Set(x, 3, ramp[1]);
                canvas.Set(x, h - 4, ramp[3]);
            }

            // Os rebites nos quatro cantos.
            foreach ((int x, int y) in new[] { (5, 5), (w - 7, 5), (5, h - 7), (w - 7, h - 7) })
            {
                canvas.Set(x, y, ramp[3]);
                canvas.Set(x + 1, y, ramp[1]);
                canvas.Set(x, y + 1, ramp[1]);
                canvas.Set(x + 1, y + 1, ramp[0]);
            }

            // Escolhida: um rubi aceso em cada ponta.
            if (selected)
            {
                Gem(canvas, 12, h / 2);
                Gem(canvas, w - 13, h / 2);
            }

            // O texto gravado no meio.
            int textWidth = PixelFont.Measure(label);
            int tx = (w - textWidth) / 2;
            int ty = (h - PixelFont.GlyphHeight) / 2;
            Rgba ink = selected ? new Rgba(62, 30, 14) : new Rgba(196, 170, 120);
            Rgba edge = selected ? new Rgba(255, 236, 170) : new Rgba(26, 18, 14);
            PixelFont.Draw(canvas, label, tx, ty, ink, edge, PixelFont.Style.Engraved);

            Outline1(canvas);
            return canvas;
        }

        private static void Gem(PixelCanvas canvas, int cx, int cy)
        {
            int[] widths = { 1, 3, 5, 3, 1 };
            for (int i = 0; i < widths.Length; i++)
            {
                int y = cy - 2 + i;
                for (int x = cx - widths[i] / 2; x <= cx + widths[i] / 2; x++)
                {
                    canvas.Set(x, y, i < 2 ? RubyLight : Ruby);
                }
            }

            canvas.Set(cx - 2, cy, new Rgba(140, 16, 30));
            canvas.Set(cx, cy - 2, new Rgba(255, 245, 240));
        }

        // ------------------------------------------------------------------
        // O friso, as linhas e a dica
        // ------------------------------------------------------------------

        /// <summary>Um friso de latao: duas linhas que somem nas pontas e uma engrenagem no meio.</summary>
        public static PixelCanvas Divider()
        {
            const int w = 200;
            const int h = 11;
            var canvas = new PixelCanvas(w, h);
            int cy = h / 2;

            for (int x = 0; x < w; x++)
            {
                float fade = 1f - Math.Abs(x - w / 2f) / (w / 2f);
                if (fade <= 0.05f) continue;

                int alpha = (int)(255 * Math.Min(1f, fade * 1.8f));
                canvas.Set(x, cy - 2, Brass[2].WithAlpha(alpha));
                canvas.Set(x, cy + 2, Brass[1].WithAlpha(alpha));

                // Pontinhos entre as linhas, de tantos em tantos.
                if (x % 6 == 0 && fade > 0.3f) canvas.Set(x, cy, Brass[3].WithAlpha(alpha));
            }

            // A engrenagem no meio.
            for (int y = 0; y < h; y++)
            {
                for (int x = w / 2 - 6; x <= w / 2 + 6; x++)
                {
                    double dx = x + 0.5 - w / 2.0, dy = y + 0.5 - (cy + 0.5);
                    double r = Math.Sqrt(dx * dx + dy * dy);
                    double angle = Math.Atan2(dy, dx);
                    bool tooth = r < 5.4 && r >= 4 && Math.Cos(angle * 8) > 0.1;
                    if (r < 1.5) canvas.Set(x, y, Ruby);
                    else if (r < 4) canvas.Set(x, y, dy < 0 ? Brass[3] : Brass[2]);
                    else if (tooth) canvas.Set(x, y, Brass[1]);
                }
            }

            return canvas;
        }

        // ------------------------------------------------------------------
        // O menu
        // ------------------------------------------------------------------

        public static readonly string[] MenuLabels = { "CONTINUAR", "NOVO JOGO", "SAIR" };

        public static PixelCanvas MenuTitleSmall() => Title("ECHOES OF", 2, false);
        public static PixelCanvas MenuTitleBig() => Title("CRAVEJADA", 3, false);

        // ------------------------------------------------------------------
        // Contas
        // ------------------------------------------------------------------

        /// <summary>Contorno escuro de um pixel em volta do que e solido.</summary>
        private static void Outline1(PixelCanvas canvas)
        {
            var solid = new bool[canvas.Pixels.Length];
            for (int i = 0; i < solid.Length; i++) solid[i] = canvas.Pixels[i].A >= 200;

            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    if (canvas.Pixels[x + y * canvas.Width].A > 0) continue;

                    bool edge = (canvas.Inside(x - 1, y) && solid[x - 1 + y * canvas.Width])
                        || (canvas.Inside(x + 1, y) && solid[x + 1 + y * canvas.Width])
                        || (canvas.Inside(x, y - 1) && solid[x + (y - 1) * canvas.Width])
                        || (canvas.Inside(x, y + 1) && solid[x + (y + 1) * canvas.Width]);

                    if (edge) canvas.Set(x, y, Outline);
                }
            }
        }
    }
}
