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
        [SerializeField] private Light2D lampLight;

        [Tooltip("A intensidade da luz quando o lampiao esta todo aceso.")]
        [SerializeField] private float fullIntensity = 0.8f;

        private SpriteRenderer _glass;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Light2D light, float newFullIntensity)
        {
            lampLight = light;
            fullIntensity = newFullIntensity;
        }

        private void Awake()
        {
            _glass = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (lampLight == null) return;

            float lit = Mathf.Clamp01(lampLight.intensity / Mathf.Max(0.01f, fullIntensity));
            Color color = _glass.color;
            color.a = lit;
            _glass.color = color;
        }
    }
}
