using System;
using System.Collections.Generic;
using static EchoesOfCravejada.EditorTools.DocKit;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Pecas dos documentos usadas em mais de um distrito.</summary>
    public static partial class LoreArt
    {
        /// <summary>
        /// Uma placa de latao fundido: recorte (0 reto, 1 cantos cortados, 2
        /// topo em arco), letra em relevo, parafusos, azinhavre. Devolve a
        /// superficie pronta pra receber riscos; quem chama faz o Bake.
        /// </summary>
        private static DocSurface CastPlaque(int w, int h, string[] cast, float size, int seed, float patina, int shape,
            out float textBottom, float firstLine = 0f, C3? metal = null)
        {
            var s = new DocSurface(w, h);
            int m = 10;
            if (shape == 2)
            {
                s.Shape((x, y) =>
                {
                    if (x < m || x > w - m || y > h - m) return false;
                    float arc = m + 22f - (float)Math.Sqrt(Math.Max(0f, 1f - Math.Pow((x - w / 2f) / (w / 2f - m), 2))) * 22f;
                    return y >= arc;
                });
            }
            else if (shape == 1)
            {
                s.Shape((x, y) =>
                {
                    if (x < m || x > w - m || y < m || y > h - m) return false;
                    float cx = Math.Min(x - m, w - m - x), cy = Math.Min(y - m, h - m - y);
                    return cx + cy >= 12f;
                });
            }
            else s.Shape(DocSurface.Sheet(m, m, w - m, h - m, seed, corner: 6f));

            s.Metal(metal ?? Brass * 0.94f, seed + 1, 0f, bevel: 6f, gloss: 0.6f, wear: 0.7f);
            DocInk.Emboss(s, DocInk.Rect(m + 12, m + (shape == 2 ? 26 : 12), w - m - 12, h - m - 12), 1.4f, 1.3f, 1f, BrassBright, 0.2f);

            Hand hand = Cast(size, seed + 2);
            var letters = new List<List<P2>>();
            float y = firstLine > 0f ? firstLine : m + (shape == 2 ? 34 : 22) + size * 1.2f;
            foreach (string line in cast)
            {
                letters.AddRange(hand.Line(line, w / 2f, y, 0.5f));
                y += size * 1.75f;
            }

            DocInk.Emboss(s, letters, 1.35f, 2.2f, 1f, BrassBright, 0.55f);
            textBottom = y - size * 0.5f;

            s.Patina(patina, seed + 3, Verdigris, VerdigrisLight, 1f);

            float inset = m + 7;
            s.Rivet(inset, shape == 2 ? m + 30 : inset, 5.5f, BrassDark * 1.35f, 0.5f, true, 0.3f + seed % 3);
            s.Rivet(w - inset, shape == 2 ? m + 30 : inset, 5.5f, BrassDark * 1.35f, 0.5f, true, 1.1f + seed % 2);
            s.Rivet(inset, h - inset, 5.5f, BrassDark * 1.35f, 0.5f, true, 2.2f);
            s.Rivet(w - inset, h - inset, 5.5f, BrassDark * 1.35f, 0.5f, true, 0.9f);
            return s;
        }

        /// <summary>Riscar uma linha a prego, com a mao dada.</summary>
        private static void NailLine(DocSurface s, Hand hand, string text, float x, float y, C3 inner, int seed, float depth = 2.2f,
            float align = 0f, float width = 1.25f)
        {
            List<List<P2>> cut = DocInk.Scratchify(hand.Line(text, x, y, align), seed, 1.1f, 1.3f, 0.4f);
            DocInk.Carve(s, cut, width, depth, inner, 0.85f, seed + 1, chips: 0.3f);
        }

        /// <summary>Uma parede de chapa de ferro com rebites, emendas e ferrugem: o fundo dos rabiscos do District 2.</summary>
        private static DocSurface IronSheetWall(int w, int h, int seed, float rust = 0.5f, bool seams = true)
        {
            var s = new DocSurface(w, h);
            s.Shape(DocSurface.Sheet(8, 8, w - 8, h - 8, seed, 1.5f, 1.5f, 1.5f, 1.5f));
            s.Metal(C3.Hex(0x45454e), seed + 1, 1.5708f, bevel: 2f, gloss: 0.25f, wear: 1f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.025f, y * 0.025f, seed + 2, 4) - (0.62f - rust * 0.25f)) * 2.2f, Rust);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.08f, y * 0.08f, seed + 3, 3) - 0.66f) * 3f * rust, RustLight);
            if (seams)
            {
                for (int x = 120; x < w - 20; x += 150)
                {
                    for (int y = 8; y < h - 8; y++)
                    {
                        int i = x + y * w;
                        if (s.A[i] <= 0f) continue;
                        s.Ht[i] -= 2f;
                        s.Col[i] = s.Col[i] * 0.55f;
                        if (x + 1 < w) s.Ht[i + 1] += 0.8f;
                    }

                    for (int y = 24; y < h - 20; y += 28)
                    {
                        s.Rivet(x - 8, y, 4f, C3.Hex(0x55545c), 0.4f);
                        s.Rivet(x + 8, y, 4f, C3.Hex(0x55545c), 0.4f);
                    }
                }
            }

            // Escorridos de ferrugem de cima pra baixo.
            s.Patina(0.25f * rust, seed + 4, Rust * 0.8f, RustLight, 1.5f);
            return s;
        }
    }
}
