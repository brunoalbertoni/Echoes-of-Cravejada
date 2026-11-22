using static EchoesOfCravejada.EditorTools.FilmPaint;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Os desenhos do topo da tela de fim, um pra cada final.</summary>
    public static partial class FinaleFilms
    {
        /// <summary>O Scrap apagado: a cabeca de quartzo de olhos escuros, numa faixa de luz fria.</summary>
        public static PixelCanvas ColumnEmblem()
        {
            var c = new PixelCanvas(120, 50);

            // A faixa de luz que desce, atras dele.
            for (int y = 0; y < 50; y++)
                for (int x = 44; x < 76; x++)
                {
                    float k = (1f - System.Math.Abs(x - 60f) / 16f) * 0.45f * (1f - y / 70f);
                    if (k > Bayer(x, y) * 0.6f) c.Set(x, y, Pure.WithAlpha((int)(k * 200f)));
                }

            PixelCanvas body = HeroArt.DrawPose(ScrapLook(0f), new HeroArt.Pose { HeadY = 2f, BodyY = 1f, EyesShut = true });
            PixelCanvas big = Scaled(Relight(body, C(150, 156, 176), Pure, 0f, -1f, 1, 0.7f), 2);
            Stamp(c, Crop(big, 0, 22, 96, 50), 12, 0);
            return c;
        }

        /// <summary>A coroa quebrada no chao, com o Oxide ainda boiando em volta.</summary>
        public static PixelCanvas CrownEmblem()
        {
            var c = new PixelCanvas(120, 50);
            PixelCanvas crown = Crown(0.3f, 0.9f, 0.45f, 0.03f, false);
            Stamp(c, crown, (120 - crown.Width) / 2, 50 - crown.Height + 2);

            for (int i = 0; i < 26; i++)
            {
                int x = ArtKit.Hash(i, 0, 12101) % 120;
                int y = ArtKit.Hash(i, 1, 12101) % 46;
                c.Blend(x, y, Oxide.WithAlpha(120 + ArtKit.Hash(i, 2, 12101) % 120));
            }

            return c;
        }
    }
}
