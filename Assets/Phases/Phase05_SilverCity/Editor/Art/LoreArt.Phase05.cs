using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que se le no palacio: os retratos da nobreza no corredor, limpos,
    /// como a Corte se via.
    /// </summary>
    public static partial class LoreArt
    {
        private static Dictionary<string, Piece> _phase05;

        public static IReadOnlyDictionary<string, Piece> Phase05 => _phase05 ?? (_phase05 = BuildPhase05());

        private static Dictionary<string, Piece> BuildPhase05()
        {
            return Index(new[]
            {
                Make("Lore_Portrait_1", "palace_noble_portrait_0", () => PalaceDecorArt.Portrait(0), 4.5f, true, PortraitDoc(0, 0, "MESTRE DE FUNDIÇÃO", 4411)),
                Make("Lore_Portrait_2", "palace_noble_portrait_1", () => PalaceDecorArt.Portrait(1), 4.5f, true, PortraitDoc(1, 0, "TESOUREIRA DA COROA", 4412)),
                Make("Lore_Portrait_Crisol", "palace_noble_portrait_2", () => PalaceDecorArt.Portrait(2), 4.5f, true, PortraitDoc(2, 0, "CRISOL. LAPIDÁRIO DA CORTE.", 4413)),
                Make("Lore_Portrait_3", "palace_noble_portrait_0", () => PalaceDecorArt.Portrait(0), 4.5f, true, PortraitDoc(3, 0, "ALQUIMISTA-CHEFE", 4414)),
            });
        }
    }
}
