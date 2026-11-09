using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Fica na cena do palacio. Dando Play direto nela no Editor (sem ter
    /// passado pelos distritos), conta os quatro como purificados, como num
    /// jogo de verdade (quem chega ao palacio ja limpou os quatro). Num jogo de
    /// verdade nao faz nada.
    /// </summary>
    public class PurificationGrant : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_EDITOR
            if (Purification.Percent > 0) return;
            foreach (string district in Purification.Districts) AreaPurifier.MarkPurified(district);
            Debug.Log("[Phase 5] Play direto no palacio: os quatro distritos contam como purificados (so no Editor).");
#endif
        }
    }
}
