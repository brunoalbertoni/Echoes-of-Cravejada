using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os marcadores que flutuam em cima do que da pra usar (ver
    /// InteractCue): pequenos, em 16 pixels por tile, com contorno escuro pra
    /// ler em cima de qualquer fundo.
    ///
    ///   cue_talk — um balaozinho com tres pontos: da pra conversar.
    ///   cue_read — uma folhinha com linhas: da pra ler.
    ///   cue_item — um losango lapidado, com brilho: da pra pegar.
    ///   cue_use  — uma setinha pra baixo: da pra acionar (alavanca).
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class CueArt
    {
        public const string Talk = "cue_talk";
        public const string Read = "cue_read";
        public const string Item = "cue_item";
        public const string Use = "cue_use";

        public const int PixelsPerUnit = 16;

        private static readonly Rgba Ink = new Rgba(24, 18, 26);
        private static readonly Rgba Paper = new Rgba(246, 236, 206);
        private static readonly Rgba PaperShade = new Rgba(204, 186, 146);
        private static readonly Rgba Warm = new Rgba(255, 214, 120);
        private static readonly Rgba WarmDeep = new Rgba(214, 150, 60);

        public static (string Name, PixelCanvas Canvas)[] All() => new[]
        {
            (Talk, DrawTalk()),
            (Read, DrawRead()),
            (Item, DrawItem()),
            (Use, DrawUse()),
        };

        private static PixelCanvas DrawTalk()
        {
            var c = new PixelCanvas(13, 11);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 1; x < 12; x++)
                {
                    bool corner = (y == 0 || y == 7) && (x == 1 || x == 11);
                    if (!corner) c.Set(x, y, y >= 6 ? PaperShade : Paper);
                }
            }

            // O rabinho do balao.
            c.Set(3, 8, Paper);
            c.Set(4, 8, PaperShade);
            c.Set(3, 9, PaperShade);

            foreach (int x in new[] { 3, 6, 9 })
            {
                c.Set(x, 3, Ink);
                c.Set(x + 1, 3, Ink);
                c.Set(x, 4, Ink);
                c.Set(x + 1, 4, Ink);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static PixelCanvas DrawRead()
        {
            var c = new PixelCanvas(11, 13);
            for (int y = 1; y < 12; y++)
            {
                for (int x = 1; x < 10; x++)
                {
                    bool dog = x >= 7 && y <= 3 && (x - 7) + (3 - y) >= 2;
                    if (!dog) c.Set(x, y, x >= 8 ? PaperShade : Paper);
                }
            }

            c.Set(7, 1, PaperShade);
            c.Set(8, 2, PaperShade);
            c.Set(7, 2, PaperShade);

            for (int y = 4; y < 11; y += 2)
            {
                int end = y == 10 ? 5 : 7;
                for (int x = 2; x <= end; x++) c.Set(x, y, new Rgba(120, 96, 70));
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static PixelCanvas DrawItem()
        {
            var c = new PixelCanvas(11, 13);
            for (int y = 0; y < 13; y++)
            {
                for (int x = 0; x < 11; x++)
                {
                    float d = Math.Abs(x - 5f) / 5f + Math.Abs(y - 6f) / 6f;
                    if (d > 1f) continue;
                    bool upper = y < 6;
                    bool left = x < 5;
                    Rgba color = upper ? (left ? new Rgba(255, 246, 208) : Warm) : (left ? Warm : WarmDeep);
                    if (d < 0.35f) color = new Rgba(255, 252, 236);
                    c.Set(x, y, color);
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static PixelCanvas DrawUse()
        {
            var c = new PixelCanvas(11, 11);
            for (int y = 0; y < 5; y++)
            {
                for (int x = 4; x < 7; x++) c.Set(x, y, y == 0 ? Paper : Warm);
            }

            for (int i = 0; i < 5; i++)
            {
                for (int x = 1 + i; x < 10 - i; x++) c.Set(x, 5 + i, i == 0 ? Warm : (i < 3 ? Warm : WarmDeep));
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
