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
        [UnityEngine.Serialization.FormerlySerializedAs("scrapSprite")] [SerializeField] private Sprite spriteSucata;
        [UnityEngine.Serialization.FormerlySerializedAs("polishedSprite")] [SerializeField] private Sprite spritePolido;

        [UnityEngine.Serialization.FormerlySerializedAs("polishedFromStart")] [Tooltip("Se nesta cena ele ja chega polido. Vale pras fases depois do District 1, pra dar Play direto nelas.")]
        [SerializeField] private bool polidoDeInicio;

        [SaveProgress("_polished")]
        private static bool _polido;

        private SpriteRenderer _renderizador;

        public static bool EstaPolido => _polido;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _polido = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Sprite sucata, Sprite polido)
        {
            spriteSucata = sucata;
            spritePolido = polido;
        }

        /// <summary>Usado pelos geradores das fases depois do District 1.</summary>
        public void DefinirPolidoDesdeInicio(bool polido)
        {
            polidoDeInicio = polido;
        }

        private void Awake()
        {
            // Sem cerimonia: aqui ele nao esta mudando, so ja chegou assim.
            if (polidoDeInicio) _polido = true;

            _renderizador = GetComponentInChildren<SpriteRenderer>();
            Aplicar();
        }

        /// <summary>O primeiro polimento. Devolve falso se ele ja estava polido.</summary>
        public bool Polir()
        {
            if (_polido) return false;

            _polido = true;
            Aplicar();
            return true;
        }

        private void Aplicar()
        {
            if (_renderizador == null) return;

            Sprite sprite = _polido ? spritePolido : spriteSucata;
            if (sprite != null) FrameAnimator.Exibir(_renderizador, sprite);
        }
    }
}
