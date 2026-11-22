using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A nevoa densa de Oxide que tampa a obra abandonada. O Scrap e imune ao
    /// Oxide, mas esta e grossa demais: e parede ate a purificacao, quando o
    /// AreaPurifier a desliga. Esbarrar mostra o porque.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class OxideVeil : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("message")] [SerializeField] private string mensagem = "A névoa de Óxido é densa demais para atravessar.";

        private void OnCollisionEnter2D(Collision2D colisao)
        {
            if (colisao.collider.GetComponentInParent<ScrapController>() == null) return;

            // So o som: a nevoa ja diz que nao passa.
            Sfx.SolavancoOxido.Tocar();
        }
    }
}
