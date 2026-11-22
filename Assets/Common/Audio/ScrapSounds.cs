using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Os sons do corpo do Scrap que saem de olhar o que ele esta fazendo, e
    /// nao de um momento so no codigo: os passos (no quadro em que o pe pisa),
    /// a queda (mais pesada quanto mais alto ele caiu), agarrar a parede e a
    /// lixa enquanto ele raspa uma alavanca, valvula, veio ou runa.
    ///
    /// Pulo, golpe, dash e dano tocam direto de onde acontecem.
    /// O SfxPlayer prende este componente no Scrap quando a cena abre.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ScrapSounds : MonoBehaviour
    {
        // Os quadros da passada em que um pe assenta no chao (ver HeroArt).
        private const int AQuadroPasso = ScrapAnimator.QuadroAndar;
        private const int BQuadroPasso = ScrapAnimator.QuadroAndar + 4;

        /// <summary>Queda mais lenta que isto (tiles/s) nem faz barulho de aterrissar.</summary>
        private const float PousoMacio = 2f;

        /// <summary>A partir daqui e queda de verdade: um pulo comum chega a uns 18.</summary>
        private const float Pouso = 5f;

        /// <summary>A partir daqui, queda de muito alto (umas 6 tiles ou mais).</summary>
        private const float PousoPesado = 24f;

        private ScrapController _controlador;
        private ScrapAnimator _animador;
        private Rigidbody2D _corpo;

        private int _ultimoQuadro = -1;
        private bool _estavaNoChao = true;
        private bool _estavaLigadoParede;
        private float _velocidadeQueda;
        private float _nascidoEm;

        private AudioSource _raspagem;
        private float _nivelRaspagem;
        private Object _donoSegurar;
        private float _progressoSegurar;
        private bool _avancando;

        private void Awake()
        {
            _controlador = GetComponent<ScrapController>();
            _animador = GetComponent<ScrapAnimator>();
            _corpo = GetComponent<Rigidbody2D>();
            _nascidoEm = Time.time;

            _raspagem = gameObject.AddComponent<AudioSource>();
            _raspagem.clip = SfxPlayer.Carregar(Sfx.RepeticaoRaspagem.Clipe);
            _raspagem.loop = true;
            _raspagem.playOnAwake = false;
            _raspagem.spatialBlend = 0f;
            _raspagem.volume = 0f;

            ScrapHealth.Morreu += AoMorrer;
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= AoMorrer;
        }

        // Renascer na bancada nao e cair nela.
        private void AoMorrer()
        {
            _nascidoEm = Time.time;
            _velocidadeQueda = 0f;
        }

        private void Update()
        {
            AtualizarRaspagem();
        }

        // Depois do ScrapAnimator escolher o quadro.
        private void LateUpdate()
        {
            if (_controlador == null) return;

            AtualizarPassos();
            AtualizarPouso();
            AtualizarParede();
        }

        private void AtualizarPassos()
        {
            int quadro = _animador != null ? _animador.Quadro : -1;
            if (quadro == _ultimoQuadro) return;
            _ultimoQuadro = quadro;

            if (!_controlador.EstaNoChao || (quadro != AQuadroPasso && quadro != BQuadroPasso)) return;

            // Com a safira cravada ele pisa leve. No mercurio, o passo chapinha.
            float volume = Mathf.Lerp(0.6f, 1f, _controlador.VelocidadeNormalizada) * (ScrapGems.EncaixadoSafira ? 0.6f : 1f);
            (_controlador.EstaLigadoMercurio ? Sfx.PassoMercurio : Sfx.Passo2).Tocar(volume: volume);
        }

        private void AtualizarPouso()
        {
            bool noChao = _controlador.EstaNoChao;

            if (!noChao && _corpo != null) _velocidadeQueda = Mathf.Min(_velocidadeQueda, _corpo.linearVelocity.y);

            // Logo que a cena abre ele assenta no chao: isso nao e queda.
            if (noChao && !_estavaNoChao && Time.time - _nascidoEm > 0.4f)
            {
                float velocidade = -_velocidadeQueda;
                float quieto = ScrapGems.EncaixadoSafira ? 0.6f : 1f;

                if (velocidade >= PousoPesado) Sfx.PesadoPousar.Tocar(volume: quieto);
                else if (velocidade >= Pouso) Sfx.Pousar.Tocar(volume: Mathf.Lerp(0.55f, 1f, Mathf.InverseLerp(Pouso, 18f, velocidade)) * quieto);
                else if (velocidade >= PousoMacio) Sfx.Passo2.Tocar(volume: 0.8f * quieto);
            }

            if (noChao) _velocidadeQueda = 0f;
            _estavaNoChao = noChao;
        }

        private void AtualizarParede()
        {
            bool ligadoParede = _controlador.EstaDeslizandoParede;
            if (ligadoParede && !_estavaLigadoParede) Sfx.AgarradaParede.Tocar();
            _estavaLigadoParede = ligadoParede;
        }

        /// <summary>
        /// A lixa: toca enquanto a barra de raspar de alguma coisa esta
        /// enchendo, e da o brilho de polido quando ela enche.
        /// </summary>
        private void AtualizarRaspagem()
        {
            Object dono = null;
            float progresso = 0f;

            if (HoldAction.Dono != null)
            {
                // A escolha do final se segura, mas nao se raspa.
                dono = HoldAction.Dono;
                progresso = HoldAction.Progresso2;
            }

            bool avancando = dono != null && dono == _donoSegurar && progresso > _progressoSegurar + 0.0001f;

            // Terminou: estava raspando quase no fim e a barra encheu (ou o alvo saiu, que e o que fazem ao terminar).
            bool terminado = _avancando && _progressoSegurar >= 0.95f && (dono != _donoSegurar || progresso >= 0.999f);
            if (terminado)
            {
                Sfx.ProntoRaspagem.Tocar();
                avancando = false;
            }

            _avancando = avancando;
            _donoSegurar = dono;
            _progressoSegurar = progresso;

            float alvo = avancando ? Sfx.Volume * Sfx.RepeticaoRaspagem.Volume : 0f;
            _nivelRaspagem = Mathf.MoveTowards(_nivelRaspagem, alvo, Time.unscaledDeltaTime * (avancando ? 3f : 5f));

            if (_raspagem.clip == null) return;
            _raspagem.volume = _nivelRaspagem;

            if (_nivelRaspagem > 0.001f && !_raspagem.isPlaying)
            {
                _raspagem.pitch = Random.Range(0.95f, 1.05f);
                _raspagem.time = Random.Range(0f, _raspagem.clip.length * 0.9f);
                _raspagem.Play();
            }
            else if (_nivelRaspagem <= 0.001f && _raspagem.isPlaying)
            {
                _raspagem.Stop();
            }
        }
    }
}
