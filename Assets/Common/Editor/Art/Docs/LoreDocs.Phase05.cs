using System;
using System.Collections.Generic;
using static EchoesOfCravejada.EditorTools.DocKit;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Os documentos do District 5 (Palacio de Prata), desenhados com o DocKit.</summary>
    public static partial class LoreArt
    {
        private static readonly C3 Quartz = C3.Hex(0xc8dcf4);
        private static readonly C3 QuartzDeep = C3.Hex(0x7a90b8);
        private static readonly C3 OxideGreen = C3.Hex(0x3ab070);

        /// <summary>Um espinho de quartzo lapidado: duas faces, aresta clara no meio, muito brilho.</summary>
        private static void QuartzSpike(DocSurface s, float baseX, float baseY, float half, float height, int seed)
        {
            for (int y = (int)(baseY - height); y <= baseY; y++)
                for (int x = (int)(baseX - half); x <= baseX + half; x++)
                {
                    if (x < 0 || y < 0 || x >= s.W || y >= s.H) continue;
                    float t = (baseY - y) / height;
                    float w = half * (1f - t);
                    float dx = x - baseX;
                    if (Math.Abs(dx) > w) continue;
                    int i = x + y * s.W;
                    s.A[i] = 1f;
                    float face = dx < 0 ? 1f : 0.62f;
                    s.Col[i] = C3.Lerp(QuartzDeep, Quartz, face * (0.7f + DocSurface.Fbm(x * 0.1f, y * 0.1f, seed, 2) * 0.4f));
                    s.Ht[i] = 14f + (w - Math.Abs(dx)) * 1.4f;
                    s.Gloss[i] = 1.5f;
                }
        }
    }
}
