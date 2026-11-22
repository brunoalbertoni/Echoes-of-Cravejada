using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O que tem um destaque que some depois de usado: a alavanca solta, a
    /// valvula girada, a caldeira estourada.
    /// </summary>
    public interface ICueDone
    {
        bool CueDone { get; }
    }

    /// <summary>
    /// O destaque do que da pra usar: um marcador pequeno (balao, folha,
    /// losango ou seta, ver CueArt) que flutua em cima da coisa, sobe e desce
    /// devagar e pulsa junto com a luz dela. Quando o Scrap chega perto, o
    /// marcador encolhe e esmaece, pra nao brigar com o aviso de "E". O que
    /// ja foi usado (ICueDone) apaga o marcador e a luz volta ao normal.
    /// </summary>
    public class InteractCue : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer marker;
        [SerializeField] private Light2D glow;

        [Tooltip("Quanto o marcador sobe e desce, em unidades de mundo.")]
        [SerializeField] private float bob = 0.12f;

        [Tooltip("Segundos de uma subida e descida.")]
        [SerializeField] private float period = 1.6f;

        [Tooltip("Mais perto que isso, o marcador esmaece.")]
        [SerializeField] private float nearDistance = 2.2f;

        private Vector3 _basePosition;
        private Vector3 _baseScale = Vector3.one;
        private Color _baseColor;
        private float _baseIntensity;
        private float _phase;
        private float _near;
        private Transform _scrap;
        private ICueDone _done;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(SpriteRenderer newMarker, Light2D newGlow)
        {
            marker = newMarker;
            glow = newGlow;
        }

        private void Awake()
        {
            if (marker != null)
            {
                _basePosition = marker.transform.localPosition;
                _baseColor = marker.color;
                _baseScale = marker.transform.localScale;
            }

            if (glow != null) _baseIntensity = glow.intensity;
            _phase = Mathf.Repeat(transform.position.x * 0.37f, 1f);
            _done = GetComponent<ICueDone>();
        }

        private void Update()
        {
            if (_done != null && _done.CueDone)
            {
                if (marker != null) marker.enabled = false;
                if (glow != null) glow.intensity = _baseIntensity;
                enabled = false;
                return;
            }

            if (_scrap == null)
            {
                ScrapController scrap = FindAnyObjectByType<ScrapController>();
                if (scrap != null) _scrap = scrap.transform;
            }

            float target = 0f;
            if (_scrap != null)
            {
                Vector2 delta = _scrap.position - transform.position;
                target = delta.sqrMagnitude < nearDistance * nearDistance ? 1f : 0f;
            }

            _near = Mathf.MoveTowards(_near, target, Time.deltaTime * 4f);

            float wave = Mathf.Sin((Time.time / Mathf.Max(0.1f, period) + _phase) * Mathf.PI * 2f);

            if (marker != null)
            {
                // Pixel art: a subida anda de pixel em pixel, sem borrar.
                float y = Mathf.Round(wave * bob * 16f) / 16f;
                marker.transform.localPosition = _basePosition + new Vector3(0f, y, 0f);

                Color color = _baseColor;
                color.a = _baseColor.a * Mathf.Lerp(0.85f + 0.15f * wave, 0.25f, _near);
                marker.color = color;
                marker.transform.localScale = _baseScale * Mathf.Lerp(1f, 0.75f, _near);
            }

            if (glow != null) glow.intensity = _baseIntensity * (1f + 0.35f * wave);
        }
    }
}
