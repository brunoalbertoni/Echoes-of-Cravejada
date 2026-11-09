using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A aparencia do Scrap: sucata (Fase 1 — The Cute Scrap) ou polido
    /// (Fase 2 — The Respected Artisan).
    ///
    /// Quem o leva de um pro outro e a purificacao do District 1: quando o
    /// Coletor explode e o Oxide se desfaz, a crosta sai do corpo do Scrap
    /// junto (ver a cena de queda do RustyScrapper).
    ///
    /// Uma vez polido, fica polido em todas as cenas ate o jogo fechar. Ainda
    /// nao ha save, entao isso mora num campo estatico.
    /// </summary>
    public class ScrapAppearance : MonoBehaviour
    {
        [SerializeField] private Sprite scrapSprite;
        [SerializeField] private Sprite polishedSprite;

        [Tooltip("Se nesta cena ele ja chega polido. Vale pras fases depois do District 1, pra dar Play direto nelas.")]
        [SerializeField] private bool polishedFromStart;

        private static bool _polished;

        private SpriteRenderer _renderer;

        public static bool IsPolished => _polished;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _polished = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite scrap, Sprite polished)
        {
            scrapSprite = scrap;
            polishedSprite = polished;
        }

        /// <summary>Usado pelos geradores das fases depois do District 1.</summary>
        public void SetPolishedFromStart(bool polished)
        {
            polishedFromStart = polished;
        }

        private void Awake()
        {
            // Sem cerimonia: aqui ele nao esta mudando, so ja chegou assim.
            if (polishedFromStart) _polished = true;

            _renderer = GetComponentInChildren<SpriteRenderer>();
            Apply();
        }

        /// <summary>O primeiro polimento. Devolve falso se ele ja estava polido.</summary>
        public bool Polish()
        {
            if (_polished) return false;

            _polished = true;
            Apply();
            return true;
        }

        private void Apply()
        {
            if (_renderer == null) return;

            Sprite sprite = _polished ? polishedSprite : scrapSprite;
            if (sprite != null) _renderer.sprite = sprite;
        }
    }
}
