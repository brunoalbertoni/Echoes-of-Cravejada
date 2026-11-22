using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O vidro aceso de um lampiao: aparece na mesma medida em que a luz dele
    /// acende. Os postes do District 1 ficam apagados ate a purificacao, e e
    /// a onda dela que sobe a luz (ver AreaPurifier) — o vidro so acompanha.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class LampGlass : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("lampLight")] [SerializeField] private Light2D luzLampada;

        [UnityEngine.Serialization.FormerlySerializedAs("fullIntensity")] [Tooltip("A intensidade da luz quando o lampiao esta todo aceso.")]
        [SerializeField] private float intensidadeCheio = 0.8f;

        private SpriteRenderer _vidro;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Light2D luz, float novoIntensidadeCheio)
        {
            luzLampada = luz;
            intensidadeCheio = novoIntensidadeCheio;
        }

        private void Awake()
        {
            _vidro = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (luzLampada == null) return;

            float aceso = Mathf.Clamp01(luzLampada.intensity / Mathf.Max(0.01f, intensidadeCheio));
            Color cor = _vidro.color;
            cor.a = aceso;
            _vidro.color = cor;
        }
    }
}
