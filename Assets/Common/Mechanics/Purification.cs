using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Quanto do reino o Scrap ja purificou: cada um dos quatro distritos vale
    /// 25%. Cada purificacao poliu um pouco mais o nucleo dele; com os quatro,
    /// o nucleo esta limpo — e com ele que o Scrap chega ao final (O Grande Polimento).
    /// </summary>
    public static class Purification
    {
        public static readonly string[] Districts =
        {
            "Phase01_ScrapYard",
            "Phase02_BrassPipelines",
            "Phase03_SilentGeodes",
            "Phase04_AlchemicalAteliers",
        };

        public static int Percent
        {
            get
            {
                int done = 0;
                foreach (string district in Districts) if (AreaPurifier.WasPurified(district)) done++;
                return done * 100 / Districts.Length;
            }
        }

        public static bool Complete => Percent >= 100;
    }
}
