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
        bool ProntoDeixa { get; }
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
        [UnityEngine.Serialization.FormerlySerializedAs("marker")] [SerializeField] private SpriteRenderer marcador;
        [UnityEngine.Serialization.FormerlySerializedAs("glow")] [SerializeField] private Light2D brilho;

        [UnityEngine.Serialization.FormerlySerializedAs("bob")] [Tooltip("Quanto o marcador sobe e desce, em unidades de mundo.")]
        [SerializeField] private float balanco = 0.12f;

        [UnityEngine.Serialization.FormerlySerializedAs("period")] [Tooltip("Segundos de uma subida e descida.")]
        [SerializeField] private float periodo = 1.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("nearDistance")] [Tooltip("Mais perto que isso, o marcador esmaece.")]
        [SerializeField] private float distanciaPerto = 2.2f;

        private Vector3 _posicaoBase;
        private Vector3 _escalaBase = Vector3.one;
        private Color _corBase;
        private float _intensidadeBase;
        private float _fase;
        private float _perto;
        private Transform _sucata;
        private ICueDone _pronto;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(SpriteRenderer novoMarcador, Light2D novoBrilho)
        {
            marcador = novoMarcador;
            brilho = novoBrilho;
        }

        private void Awake()
        {
            if (marcador != null)
            {
                _posicaoBase = marcador.transform.localPosition;
                _corBase = marcador.color;
                _escalaBase = marcador.transform.localScale;
            }

            if (brilho != null) _intensidadeBase = brilho.intensity;
            _fase = Mathf.Repeat(transform.position.x * 0.37f, 1f);
            _pronto = GetComponent<ICueDone>();
        }

        private void Update()
        {
            if (_pronto != null && _pronto.ProntoDeixa)
            {
                if (marcador != null) marcador.enabled = false;
                if (brilho != null) brilho.intensity = _intensidadeBase;
                enabled = false;
                return;
            }

            if (_sucata == null)
            {
                ScrapController sucata = ScrapRef.Controlador;
                if (sucata != null) _sucata = sucata.transform;
            }

            float alvo = 0f;
            if (_sucata != null)
            {
                Vector2 delta = _sucata.position - transform.position;
                alvo = delta.sqrMagnitude < distanciaPerto * distanciaPerto ? 1f : 0f;
            }

            _perto = Mathf.MoveTowards(_perto, alvo, Time.deltaTime * 4f);

            float onda = Mathf.Sin((Time.time / Mathf.Max(0.1f, periodo) + _fase) * Mathf.PI * 2f);

            if (marcador != null)
            {
                // Pixel art: a subida anda de pixel em pixel, sem borrar.
                float y = Mathf.Round(onda * balanco * 16f) / 16f;
                marcador.transform.localPosition = _posicaoBase + new Vector3(0f, y, 0f);

                Color cor = _corBase;
                cor.a = _corBase.a * Mathf.Lerp(0.85f + 0.15f * onda, 0.25f, _perto);
                marcador.color = cor;
                marcador.transform.localScale = _escalaBase * Mathf.Lerp(1f, 0.75f, _perto);
            }

            if (brilho != null) brilho.intensity = _intensidadeBase * (1f + 0.35f * onda);
        }
    }
}
