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
        public static readonly string[] Distritos =
        {
            "Phase01_ScrapYard",
            "Phase02_BrassPipelines",
            "Phase03_SilentGeodes",
            "Phase04_AlchemicalAteliers",
        };

        public static int Porcentagem
        {
            get
            {
                int pronto = 0;
                foreach (string distrito in Distritos) if (AreaPurifier.FoiPurificado(distrito)) pronto++;
                return pronto * 100 / Distritos.Length;
            }
        }

        public static bool Completo => Porcentagem >= 100;
    }
}
