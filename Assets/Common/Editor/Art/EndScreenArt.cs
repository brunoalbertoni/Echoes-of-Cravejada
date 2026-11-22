using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte da tela de fim (grade de 320x180, ampliada inteira, como a tela
    /// de morte): o titulo em latao, o friso, cada linha da historia em letra
    /// de giz palido com sombra, o credito da musica e a placa de voltar.
    /// O desenho do topo (o emblema de cada final) vem de quem chama.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class EndScreenArt
    {
        public const string MenuLabel = "VOLTAR AO MENU";
        public const string Credit = "MÚSICA: SCOTT BUCKLEY — SCOTTBUCKLEY.COM.AU (CC BY 4.0)";

        private static readonly Rgba Chalk = new Rgba(222, 214, 198);
        private static readonly Rgba Shadow = new Rgba(14, 10, 12);

        /// <summary>Uma linha da historia: giz palido, sombra de um pixel, sem moldura.</summary>
        public static PixelCanvas Line(string text, bool dim = false)
        {
            int w = PixelFont.Measure(text, 1) + 4;
            int h = PixelFont.GlyphHeight + 6;
            var c = new PixelCanvas(w, h);
            Rgba color = dim ? new Rgba(140, 132, 124) : Chalk;
            PixelFont.Draw(c, text, 2, 3, Shadow, default, PixelFont.Style.Ink, 1);
            var top = new PixelCanvas(w, h);
            PixelFont.Draw(top, text, 1, 2, color, default, PixelFont.Style.Ink, 1);

            // O giz falha aqui e ali: alguns pixels mais apagados.
            for (int i = 0; i < top.Pixels.Length; i++)
            {
                Rgba p = top.Pixels[i];
                if (p.A == 0) continue;
                if (ArtKit.Hash(i, 0, 12001) % 7 == 0) top.Pixels[i] = Rgba.Lerp(p, Shadow, 0.35f).WithAlpha(255);
            }

            c.Blit(top, 0, 0);
            return c;
        }
    }
}
