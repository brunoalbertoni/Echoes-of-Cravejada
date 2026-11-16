using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Um ponto de traco, em pixels do desenho.</summary>
    public struct P2
    {
        public float X;
        public float Y;

        public P2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Uma fonte vetorial de traco (Hershey, ver <see cref="HersheyData"/>):
    /// cada letra e um punhado de linhas, desenhadas depois com pena, giz,
    /// cinzel ou prego. Acentos do portugues sao montados por cima.
    /// </summary>
    public sealed class DocFont
    {
        public static readonly DocFont ScriptFormal = new DocFont(HersheyData.ScriptFormal);
        public static readonly DocFont ScriptQuick = new DocFont(HersheyData.ScriptQuick);
        public static readonly DocFont ScriptRound = new DocFont(HersheyData.ScriptRound);
        public static readonly DocFont RomanPrint = new DocFont(HersheyData.RomanPrint);
        public static readonly DocFont RomanCast = new DocFont(HersheyData.RomanCast);
        public static readonly DocFont RomanThin = new DocFont(HersheyData.RomanThin);
        public static readonly DocFont ItalicPrint = new DocFont(HersheyData.ItalicPrint);
        public static readonly DocFont Gothic = new DocFont(HersheyData.Gothic);

        private sealed class Glyph
        {
            public int Left;
            public int Right;
            public List<List<P2>> Strokes = new List<List<P2>>();
        }

        private readonly string[] _data;
        private readonly Dictionary<char, Glyph> _cache = new Dictionary<char, Glyph>();

        /// <summary>O topo das maiusculas e das minusculas, e a linha de base, em unidades da fonte.</summary>
        public readonly float CapTop;
        public readonly float XTop;
        public const float Baseline = 9f;

        private DocFont(string[] data)
        {
            _data = data;
            CapTop = Top('H');
            XTop = Top('x');
        }

        private float Top(char c)
        {
            float top = Baseline;
            foreach (List<P2> s in Get(c).Strokes)
                foreach (P2 p in s) top = Math.Min(top, p.Y);
            return top;
        }

        private Glyph Get(char c)
        {
            if (_cache.TryGetValue(c, out Glyph g)) return g;

            g = new Glyph();
            int index = c - 32;
            if (index >= 0 && index < _data.Length && _data[index].Length >= 2)
            {
                string d = _data[index];
                g.Left = d[0] - 'R';
                g.Right = d[1] - 'R';
                List<P2> current = null;
                for (int i = 2; i + 1 < d.Length; i += 2)
                {
                    if (d[i] == ' ' && d[i + 1] == 'R')
                    {
                        current = null;
                        continue;
                    }

                    if (current == null)
                    {
                        current = new List<P2>();
                        g.Strokes.Add(current);
                    }

                    current.Add(new P2(d[i] - 'R', d[i + 1] - 'R'));
                }
            }
            else
            {
                g.Left = -8;
                g.Right = 8;
            }

            _cache[c] = g;
            return g;
        }

        // ------------------------------------------------------------------
        // Acentos e simbolos que a Hershey nao tem
        // ------------------------------------------------------------------

        private static readonly Dictionary<char, (char Base, char Mark)> Accents = Build();

        private static Dictionary<char, (char, char)> Build()
        {
            var d = new Dictionary<char, (char, char)>();
            void Add(string chars, string bases, char mark)
            {
                for (int i = 0; i < chars.Length; i++) d[chars[i]] = (bases[i], mark);
            }

            Add("áéíóúÁÉÍÓÚ", "aeiouAEIOU", '\'');
            Add("àèìòùÀÈÌÒÙ", "aeiouAEIOU", '`');
            Add("âêîôûÂÊÎÔÛ", "aeiouAEIOU", '^');
            Add("ãõñÃÕÑ", "aonAON", '~');
            Add("äëïöüÄËÏÖÜ", "aeiouAEIOU", ':');
            Add("çÇ", "cC", ',');
            return d;
        }

        private static char Plain(char c)
        {
            switch (c)
            {
                case '“':
                case '”':
                case '«':
                case '»': return '"';
                case '‘':
                case '’': return '\'';
                case '–': return '-';
                case '·': return '.';
                default: return c;
            }
        }

        /// <summary>A largura de um texto, em unidades da fonte.</summary>
        public float Units(string text)
        {
            float w = 0f;
            foreach (char raw in text) w += Advance(raw);
            return w;
        }

        private float Advance(char raw)
        {
            if (raw == '—') return Advance('-') * 2f;
            if (raw == '…') return Advance('.') * 3f;
            if (raw == 'º' || raw == 'ª') return Advance('o') * 0.7f;
            char c = Accents.TryGetValue(raw, out (char Base, char Mark) a) ? a.Base : Plain(raw);
            Glyph g = Get(c);
            return g.Right - g.Left;
        }

        /// <summary>
        /// Os tracos de uma letra em unidades da fonte, com a origem na margem
        /// esquerda e a linha de base em y = 9. Devolve o avanco.
        /// </summary>
        public float Letter(char raw, float x0, List<List<P2>> into)
        {
            if (raw == '—')
            {
                Glyph dash = Get('-');
                float w = (dash.Right - dash.Left) * 2f;
                into.Add(new List<P2> { new P2(x0 + 2f, 2f), new P2(x0 + w - 2f, 2f) });
                return w;
            }

            if (raw == '…')
            {
                float step = Advance('.');
                for (int i = 0; i < 3; i++) Letter('.', x0 + step * i, into);
                return step * 3f;
            }

            if (raw == 'º' || raw == 'ª')
            {
                var small = new List<List<P2>>();
                float adv = Letter(raw == 'º' ? 'o' : 'a', 0f, small);
                foreach (List<P2> s in small)
                {
                    var t = new List<P2>();
                    foreach (P2 p in s) t.Add(new P2(x0 + p.X * 0.7f, CapTop + (p.Y - XTop) * 0.55f));
                    into.Add(t);
                }

                float bar = CapTop + (Baseline - XTop) * 0.55f + 2f;
                into.Add(new List<P2> { new P2(x0 + 1f, bar), new P2(x0 + adv * 0.7f - 1f, bar) });
                return adv * 0.7f;
            }

            char mark = '\0';
            char c = Plain(raw);
            if (Accents.TryGetValue(raw, out (char Base, char Mark) acc))
            {
                c = acc.Base;
                mark = acc.Mark;
            }

            Glyph g = Get(c);
            float ox = x0 - g.Left;
            bool upper = char.IsUpper(c);
            int skipDot = -1;

            // O i com acento perde o pingo.
            if (mark != '\0' && (c == 'i' || c == 'j'))
            {
                for (int i = 0; i < g.Strokes.Count; i++)
                {
                    float minY = 99f, maxY = -99f;
                    foreach (P2 p in g.Strokes[i])
                    {
                        minY = Math.Min(minY, p.Y);
                        maxY = Math.Max(maxY, p.Y);
                    }

                    if (maxY - minY < 3f && maxY < XTop + 1f) skipDot = i;
                }
            }

            float cx = 0f, count = 0f, top = 99f;
            for (int i = 0; i < g.Strokes.Count; i++)
            {
                if (i == skipDot) continue;
                var s = new List<P2>();
                foreach (P2 p in g.Strokes[i])
                {
                    s.Add(new P2(ox + p.X, p.Y));
                    top = Math.Min(top, p.Y);
                    if (p.Y < (upper ? CapTop + 4f : XTop + 3f))
                    {
                        cx += ox + p.X;
                        count++;
                    }
                }

                into.Add(s);
            }

            if (mark != '\0')
            {
                if (count == 0f) cx = x0 + (g.Right - g.Left) * 0.5f;
                else cx /= count;
                float t = (upper ? Math.Min(top, CapTop) : XTop) - 2.5f;
                AddMark(mark, cx, t, into);
            }

            return g.Right - g.Left;
        }

        private void AddMark(char mark, float cx, float t, List<List<P2>> into)
        {
            switch (mark)
            {
                case '\'':
                    into.Add(new List<P2> { new P2(cx - 1f, t), new P2(cx + 2.5f, t - 3.5f) });
                    break;
                case '`':
                    into.Add(new List<P2> { new P2(cx - 2.5f, t - 3.5f), new P2(cx + 1f, t) });
                    break;
                case '^':
                    into.Add(new List<P2> { new P2(cx - 3f, t), new P2(cx, t - 3f), new P2(cx + 3f, t) });
                    break;
                case '~':
                    into.Add(new List<P2>
                    {
                        new P2(cx - 3.5f, t - 0.5f), new P2(cx - 2.2f, t - 2.2f), new P2(cx - 0.8f, t - 2f),
                        new P2(cx + 0.8f, t - 0.8f), new P2(cx + 2.2f, t - 0.6f), new P2(cx + 3.5f, t - 2.4f),
                    });
                    break;
                case ':':
                    into.Add(new List<P2> { new P2(cx - 2f, t - 1f), new P2(cx - 1.6f, t - 1.4f) });
                    into.Add(new List<P2> { new P2(cx + 2f, t - 1f), new P2(cx + 2.4f, t - 1.4f) });
                    break;
                case ',':
                    into.Add(new List<P2> { new P2(cx, Baseline), new P2(cx + 1.5f, Baseline + 2f), new P2(cx, Baseline + 3.5f), new P2(cx - 2f, Baseline + 3.5f) });
                    break;
            }
        }
    }

    /// <summary>
    /// Como uma mao (ou uma maquina) escreve: tamanho, inclinacao, tremor,
    /// linha de base ondulada. Produz os tracos ja no lugar do desenho.
    /// </summary>
    public sealed class Hand
    {
        public DocFont Font = DocFont.RomanPrint;

        /// <summary>Altura da maiuscula, em pixels.</summary>
        public float Size = 10f;

        /// <summary>Inclinacao (cisalhamento) extra; positivo deita pra direita.</summary>
        public float Slant;

        /// <summary>Rotacao da linha inteira, em graus.</summary>
        public float Angle;

        /// <summary>Tremor de cada ponto, em pixels.</summary>
        public float Jitter;

        /// <summary>Onda da linha de base, em pixels.</summary>
        public float Wave;

        /// <summary>Variacao de tamanho de letra a letra (0..1).</summary>
        public float SizeJitter;

        /// <summary>Espaco extra entre letras, em unidades da fonte.</summary>
        public float Tracking;

        public float LineGap = 1.55f;
        public int Seed = 1;

        public float Scale => Size / (DocFont.Baseline - Font.CapTop);

        public Hand Clone()
        {
            return (Hand)MemberwiseClone();
        }

        public float Measure(string text)
        {
            return (Font.Units(text) + Tracking * text.Length) * Scale;
        }

        public float LineHeight => Size * LineGap;

        public List<string> Wrap(string text, float width)
        {
            var lines = new List<string>();
            foreach (string para in text.Split('\n'))
            {
                string line = "";
                foreach (string word in para.Split(' '))
                {
                    string test = line.Length == 0 ? word : line + " " + word;
                    if (line.Length > 0 && Measure(test) > width)
                    {
                        lines.Add(line);
                        line = word;
                    }
                    else line = test;
                }

                lines.Add(line);
            }

            return lines;
        }

        /// <summary>
        /// Os tracos de uma linha. <paramref name="x"/>,<paramref name="y"/> e o
        /// comeco da linha de base. align: 0 esquerda, 0.5 centro, 1 direita.
        /// </summary>
        public List<List<P2>> Line(string text, float x, float y, float align = 0f)
        {
            var units = new List<List<P2>>();
            float pen = 0f;
            float s = Scale;
            var letterScale = new List<(int From, float Scale, float Center)>();

            int salt = 0;
            foreach (char c in text)
            {
                int from = units.Count;
                float adv = Font.Letter(c, pen, units);
                float k = 1f + (Rand(salt++, 3) - 0.5f) * 2f * SizeJitter;
                letterScale.Add((from, k, pen + adv * 0.5f));
                pen += adv + Tracking;
            }

            float width = pen * s;
            float startX = x - width * align;
            double rad = Angle * Math.PI / 180.0;
            float cos = (float)Math.Cos(rad), sin = (float)Math.Sin(rad);

            var result = new List<List<P2>>();
            for (int li = 0; li < letterScale.Count; li++)
            {
                int from = letterScale[li].From;
                int to = li + 1 < letterScale.Count ? letterScale[li + 1].From : units.Count;
                float k = letterScale[li].Scale;
                float center = letterScale[li].Center;

                for (int si = from; si < to; si++)
                {
                    var stroke = new List<P2>();
                    int pi = 0;
                    foreach (P2 p in units[si])
                    {
                        float ux = center + (p.X - center) * k;
                        float uy = DocFont.Baseline + (p.Y - DocFont.Baseline) * k;
                        float lx = ux * s;
                        float ly = (uy - DocFont.Baseline) * s;
                        lx -= ly * Slant;
                        ly += (float)Math.Sin((lx + Seed * 13f) * 0.045f) * Wave;
                        lx += (Rand(si * 31 + pi, 7) - 0.5f) * 2f * Jitter;
                        ly += (Rand(si * 31 + pi, 11) - 0.5f) * 2f * Jitter;
                        float rx = lx * cos - ly * sin;
                        float ry = lx * sin + ly * cos;
                        stroke.Add(new P2(startX + rx, y + ry));
                        pi++;
                    }

                    result.Add(stroke);
                }
            }

            return result;
        }

        /// <summary>Varias linhas, quebrando na largura; devolve o y da proxima linha.</summary>
        public float Block(string text, float x, float y, float width, List<List<P2>> into, float align = 0f)
        {
            foreach (string line in Wrap(text, width))
            {
                float ax = align == 0f ? x : align == 1f ? x + width : x + width * 0.5f;
                into.AddRange(Line(line, ax, y, align));
                y += LineHeight;
            }

            return y;
        }

        private float Rand(int i, int salt)
        {
            return ArtKit.Hash(i, salt, Seed) % 1000 / 1000f;
        }
    }
}
