using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Uma fonte de pixel 5x7, so de maiusculas, com os acentos do portugues e
    /// o cedilha. E o que escreve as placas, os papeis e os riscos que o Scrap
    /// le (ver LoreArt).
    ///
    /// Cada linha de texto ocupa LineHeight pixels: tres em cima pro acento
    /// (dois dele e um de folga), sete da letra e dois embaixo pro rabo do
    /// cedilha. O y passado e o do topo da letra (sem o acento): deixe pelo
    /// menos tres pixels livres em cima.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class PixelFont
    {
        public const int GlyphHeight = 7;
        public const int LineHeight = 12;

        private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
        {
            ['A'] = new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
            ['B'] = new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." },
            ['C'] = new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." },
            ['D'] = new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." },
            ['E'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" },
            ['F'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." },
            ['G'] = new[] { ".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".###." },
            ['H'] = new[] { "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
            ['I'] = new[] { "###", ".#.", ".#.", ".#.", ".#.", ".#.", "###" },
            ['J'] = new[] { "..###", "...#.", "...#.", "...#.", "...#.", "#..#.", ".##.." },
            ['K'] = new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" },
            ['L'] = new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" },
            ['M'] = new[] { "#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#" },
            ['N'] = new[] { "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#", "#...#" },
            ['O'] = new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
            ['P'] = new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." },
            ['Q'] = new[] { ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#" },
            ['R'] = new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" },
            ['S'] = new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." },
            ['T'] = new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." },
            ['U'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
            ['V'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.." },
            ['W'] = new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#" },
            ['X'] = new[] { "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#" },
            ['Y'] = new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." },
            ['Z'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####" },

            ['0'] = new[] { ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###." },
            ['1'] = new[] { ".#.", "##.", ".#.", ".#.", ".#.", ".#.", "###" },
            ['2'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####" },
            ['3'] = new[] { "####.", "....#", "....#", ".###.", "....#", "....#", "####." },
            ['4'] = new[] { "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#." },
            ['5'] = new[] { "#####", "#....", "####.", "....#", "....#", "#...#", ".###." },
            ['6'] = new[] { ".###.", "#....", "#....", "####.", "#...#", "#...#", ".###." },
            ['7'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..." },
            ['8'] = new[] { ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###." },
            ['9'] = new[] { ".###.", "#...#", "#...#", ".####", "....#", "....#", ".###." },

            [' '] = new[] { "..", "..", "..", "..", "..", "..", ".." },
            ['.'] = new[] { ".", ".", ".", ".", ".", ".", "#" },
            [','] = new[] { ".", ".", ".", ".", ".", "#", "#" },
            [':'] = new[] { ".", ".", "#", ".", ".", "#", "." },
            [';'] = new[] { ".", ".", "#", ".", ".", "#", "#" },
            ['!'] = new[] { "#", "#", "#", "#", "#", ".", "#" },
            ['?'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#.." },
            ['-'] = new[] { "...", "...", "...", "###", "...", "...", "..." },
            ['—'] = new[] { ".....", ".....", ".....", "#####", ".....", ".....", "....." },
            ['"'] = new[] { "#.#", "#.#", "...", "...", "...", "...", "..." },
            ['\''] = new[] { "#", "#", ".", ".", ".", ".", "." },
            ['/'] = new[] { "....#", "...#.", "...#.", "..#..", ".#...", ".#...", "#...." },
            ['('] = new[] { ".#", "#.", "#.", "#.", "#.", "#.", ".#" },
            [')'] = new[] { "#.", ".#", ".#", ".#", ".#", ".#", "#." },
            ['º'] = new[] { ".#.", "#.#", ".#.", "###", "...", "...", "..." },
            ['…'] = new[] { ".....", ".....", ".....", ".....", ".....", ".....", "#.#.#" },
        };

        // Os acentos: duas linhas, um pixel acima da letra, centradas nos 5 pixels.
        private static readonly string[] Acute = { "...#.", "..#.." };
        private static readonly string[] Grave = { ".#...", "..#.." };
        private static readonly string[] Circumflex = { "..#..", ".#.#." };
        private static readonly string[] Tilde = { ".##.#", "#..#." };

        private static readonly Dictionary<char, (char Base, string[] Mark)> Accented =
            new Dictionary<char, (char, string[])>
            {
                ['Á'] = ('A', Acute), ['À'] = ('A', Grave), ['Â'] = ('A', Circumflex), ['Ã'] = ('A', Tilde),
                ['É'] = ('E', Acute), ['Ê'] = ('E', Circumflex),
                ['Í'] = ('I', Acute),
                ['Ó'] = ('O', Acute), ['Ô'] = ('O', Circumflex), ['Õ'] = ('O', Tilde),
                ['Ú'] = ('U', Acute),
            };

        /// <summary>Como desenhar um texto. Cada estilo e um jeito de a letra existir na superficie.</summary>
        public enum Style
        {
            /// <summary>Tinta chapada.</summary>
            Ink,

            /// <summary>Em relevo: a letra clara, a sombra escura embaixo a direita.</summary>
            Embossed,

            /// <summary>Gravada: o sulco escuro, a borda clara embaixo a direita.</summary>
            Engraved,

            /// <summary>Riscada a mao: cada letra sobe ou desce um pixel, ao acaso.</summary>
            Scratched,
        }

        /// <summary>A largura de um texto em pixels, ja em maiusculas.</summary>
        public static int Measure(string text, int scale = 1)
        {
            int width = 0;
            foreach (char raw in Normalize(text))
            {
                width += (GlyphOf(raw).Rows[0].Length + 1) * scale;
            }

            return Math.Max(0, width - scale);
        }

        /// <summary>
        /// Quebra o texto em linhas que caibam na largura, sem partir palavra.
        /// </summary>
        public static List<string> Wrap(string text, int maxWidth, int scale = 1)
        {
            var lines = new List<string>();
            string current = "";

            foreach (string word in text.Split(' '))
            {
                string attempt = current.Length == 0 ? word : current + " " + word;
                if (current.Length > 0 && Measure(attempt, scale) > maxWidth)
                {
                    lines.Add(current);
                    current = word;
                }
                else
                {
                    current = attempt;
                }
            }

            if (current.Length > 0) lines.Add(current);
            return lines;
        }

        /// <summary>
        /// Escreve o texto com o canto de cima a esquerda da primeira letra em
        /// (x, y). Devolve a largura escrita.
        /// </summary>
        public static int Draw(PixelCanvas canvas, string text, int x, int y, Rgba color, Rgba accent = default,
            Style style = Style.Ink, int scale = 1, int seed = 0)
        {
            int start = x;
            int index = 0;

            foreach (char raw in Normalize(text))
            {
                (string[] rows, string[] mark, bool cedilla) = GlyphOf(raw);

                // Riscada a mao: a letra sobe ou desce um pixel, ao acaso.
                int dy = 0;
                if (style == Style.Scratched)
                {
                    int roll = ArtKit.Hash(index, seed, 811) % 5;
                    dy = roll == 0 ? -1 : (roll == 1 ? 1 : 0);
                }

                var dots = new List<(int, int)>();
                for (int row = 0; row < rows.Length; row++)
                {
                    for (int col = 0; col < rows[row].Length; col++)
                    {
                        if (rows[row][col] == '#') dots.Add((col, row));
                    }
                }

                if (mark != null)
                {
                    int offset = (rows[0].Length - 5) / 2;
                    for (int row = 0; row < 2; row++)
                    {
                        for (int col = 0; col < 5; col++)
                        {
                            if (mark[row][col] == '#') dots.Add((col + offset, row - 3));
                        }
                    }
                }

                if (cedilla)
                {
                    dots.Add((2, 7));
                    dots.Add((1, 8));
                    dots.Add((2, 8));
                }

                // Duas passadas: todas as sombras primeiro, pra a sombra de um
                // pixel nao cobrir a letra do vizinho.
                bool shaded = style != Style.Ink && accent.A > 0;
                for (int pass = shaded ? 0 : 1; pass < 2; pass++)
                {
                    foreach ((int gx, int gy) in dots)
                    {
                        for (int sy = 0; sy < scale; sy++)
                        {
                            for (int sx = 0; sx < scale; sx++)
                            {
                                int px = x + gx * scale + sx;
                                int py = y + (gy + dy) * scale + sy;

                                if (pass == 0) canvas.Blend(px + 1, py + 1, accent);
                                else canvas.Blend(px, py, color);
                            }
                        }
                    }
                }

                x += (rows[0].Length + 1) * scale;
                index++;
            }

            return x - start - scale;
        }

        /// <summary>Escreve o texto centrado em cx.</summary>
        public static void DrawCentered(PixelCanvas canvas, string text, int cx, int y, Rgba color, Rgba accent = default,
            Style style = Style.Ink, int scale = 1, int seed = 0)
        {
            Draw(canvas, text, cx - Measure(text, scale) / 2, y, color, accent, style, scale, seed);
        }

        private static string Normalize(string text) => text.ToUpperInvariant().Replace("Ç", "\u0001");

        private static (string[] Rows, string[] Mark, bool Cedilla) GlyphOf(char raw)
        {
            if (raw == '\u0001') return (Glyphs['C'], null, true);
            if (Glyphs.TryGetValue(raw, out string[] rows)) return (rows, null, false);
            if (Accented.TryGetValue(raw, out (char Base, string[] Mark) accented)) return (Glyphs[accented.Base], accented.Mark, false);

            // O que a fonte nao tem vira um quadradinho, pra aparecer na previa.
            return (new[] { "###", "#.#", "#.#", "#.#", "#.#", "#.#", "###" }, null, false);
        }
    }
}
