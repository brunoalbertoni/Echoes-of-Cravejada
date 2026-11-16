using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Grava as folhas da abertura (IntroFallArt) e poe a cena da queda no
    /// Scrap da fase 1, ja com os pontos da Calha N.º 1.
    /// </summary>
    public static class IntroFallAssets
    {
        public const string ScrapSheet = "intro_fall_sheet";
        public const string FxSheet = "intro_fall_fx_sheet";

        /// <summary>A boca da calha: a peca fica em (19.5, 25.2), 4x4 tiles, e o funil acaba meio tile acima da base.</summary>
        public static readonly Vector2 ChuteMouth = new Vector2(21.5f, 25.7f);

        /// <summary>
        /// Onde ele bate no monte: a coluna x 18 da calha ao monte passa ao lado
        /// das plataformas finas (y 17 em x 19-23 e y 9 em x 20-24).
        /// </summary>
        public const float ImpactX = 18.2f;

        /// <summary>Onde ele para de costas e se levanta: no monte, embaixo da plataforma de y 9.</summary>
        public const float RestX = 20.6f;

        public static void Attach(GameObject scrap)
        {
            Phase02Assets.WriteSheet(ScrapSheet, IntroFallArt.Frames(), new Vector2(0.5f, 0f), IntroFallArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(FxSheet, IntroFallArt.Fx(), new Vector2(0.5f, 0.5f), IntroFallArt.PixelsPerUnit);

            var cutscene = scrap.GetComponent<WakeUpCutscene>();
            if (cutscene == null) cutscene = scrap.AddComponent<WakeUpCutscene>();

            cutscene.ConfigureFall(
                Phase02Assets.LoadSheet(ScrapSheet, IntroFallArt.FrameCount),
                Phase02Assets.LoadSheet(FxSheet, IntroFallArt.FxCount),
                ChuteMouth, ImpactX, RestX);
        }
    }
}
