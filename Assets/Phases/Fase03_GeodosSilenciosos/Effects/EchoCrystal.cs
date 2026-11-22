using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um cristal que reage ao som. No escuro ele quase nao brilha; quando um
    /// ruido chega nele, acende, e vai apagando devagar.
    ///
    /// E a "ecolocalizacao" do District 3: o jogador so ve o mundo pelos
    /// cristais que o proprio barulho acendeu. Quanto mais longe do som, mais
    /// fraco e mais atrasado ele acende — a luz se espalha em onda.
    /// </summary>
    public class EchoCrystal : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("glow")] [SerializeField] private Light2D brilho;
        [UnityEngine.Serialization.FormerlySerializedAs("sprite")] [SerializeField] private SpriteRenderer sprite;

        [UnityEngine.Serialization.FormerlySerializedAs("restIntensity")] [Tooltip("Brilho no escuro, sem som nenhum.")]
        [SerializeField] private float intensidadeDescanso = 0.04f;

        [UnityEngine.Serialization.FormerlySerializedAs("peakIntensity")] [Tooltip("Brilho no auge, com um som bem em cima.")]
        [SerializeField] private float intensidadePico = 1.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("fadeTime")] [Tooltip("Segundos ate voltar ao escuro.")]
        [SerializeField] private float tempoEsmaecer = 2.8f;

        private float _nivel;
        private float _alvo;
        private float _subidaEm;
        private bool _acesoSempre;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Light2D novoBrilho, SpriteRenderer novoSprite, float novoPico)
        {
            brilho = novoBrilho;
            sprite = novoSprite;
            intensidadePico = novoPico;
        }

        /// <summary>Deixa o cristal aceso pra sempre. E o que a purificacao faz.</summary>
        public void AcenderParaSempre()
        {
            _acesoSempre = true;
        }

        /// <summary>Acende agora, a toda. Quem chama e o proprio cristal ressonante, ao ser golpeado.</summary>
        public void Piscada()
        {
            _nivel = 1f;
            _alvo = 0f;
        }

        private void OnEnable()
        {
            NoiseSystem.Ouvido += AoOuvirRuido;
            AreaPurifier.Purificando += AcenderParaSempre;
            Aplicar();
        }

        private void OnDisable()
        {
            NoiseSystem.Ouvido -= AoOuvirRuido;
            AreaPurifier.Purificando -= AcenderParaSempre;
        }

        private void Start()
        {
            // Distrito ja purificado numa visita anterior: ja chega aceso.
            if (AreaPurifier.EstaPurificado)
            {
                _acesoSempre = true;
                _nivel = 0.45f;
                Aplicar();
            }
        }

        private void AoOuvirRuido(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            float distancia = Vector2.Distance(pontoOrigem, transform.position);
            if (distancia > raioArea) return;

            // Mais perto do som, mais forte. Ninguem dentro do raio fica no zero.
            float forca2 = Mathf.Lerp(1f, 0.35f, distancia / Mathf.Max(0.1f, raioArea));
            if (forca2 <= _alvo) return;

            _alvo = forca2;

            // A onda leva um instante pra chegar.
            _subidaEm = Time.time + distancia * 0.025f;
        }

        private void Update()
        {
            if (_acesoSempre)
            {
                _nivel = Mathf.MoveTowards(_nivel, 0.45f, Time.deltaTime);
                Aplicar();
                return;
            }

            if (_alvo > 0f && Time.time >= _subidaEm)
            {
                _nivel = Mathf.Max(_nivel, _alvo);
                _alvo = 0f;
            }

            if (_nivel <= 0f) return;

            _nivel = Mathf.Max(0f, _nivel - Time.deltaTime / Mathf.Max(0.1f, tempoEsmaecer));
            Aplicar();
        }

        private void Aplicar()
        {
            // Apaga mais devagar no comeco: fica um tempo claro e some no fim.
            float suavizado = _nivel * _nivel * (3f - 2f * _nivel);

            if (brilho != null) brilho.intensity = Mathf.Lerp(intensidadeDescanso, intensidadePico, suavizado);

            if (sprite != null)
            {
                float sombra = Mathf.Lerp(0.35f, 1f, suavizado);
                Color cor = sprite.color;
                sprite.color = new Color(sombra, sombra, sombra, cor.a);
            }
        }
    }
}
