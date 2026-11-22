using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um espelho de prata num eixo, inclinado a 45 graus. Um raio de luz que
    /// bate nele dobra a esquina. Um golpe de cinzel o faz girar pro outro
    /// lado, de "/" pra "\" e de volta, numa meia volta rapida no eixo.
    ///
    /// O espelho tem dois colisores: a area grande onde o Scrap o alcanca (a
    /// deste objeto) e a face pequena onde o raio bate (um filho).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SilverMirror : MonoBehaviour, IDamageable
    {
        [UnityEngine.Serialization.FormerlySerializedAs("slash")] [Tooltip("Verdadeiro se ele esta em \"/\" (sobe pra direita); falso se em \"\\\".")]
        [SerializeField] private bool corte = true;

        [UnityEngine.Serialization.FormerlySerializedAs("face")] [Tooltip("A face onde o raio bate: um colisor pequeno, num filho.")]
        [SerializeField] private Collider2D rosto;

        [UnityEngine.Serialization.FormerlySerializedAs("sprite")] [SerializeField] private SpriteRenderer sprite;

        private float _giro = -1f;
        private Vector3 _escalaSprite = Vector3.one;

        public Collider2D Rosto => rosto;

        /// <summary>O ponto de onde o raio sai depois de refletir.</summary>
        public Vector2 Centro => rosto != null ? (Vector2)rosto.bounds.center : (Vector2)transform.position;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(bool corteComeca, Collider2D novoRosto, SpriteRenderer novoSprite)
        {
            corte = corteComeca;
            rosto = novoRosto;
            sprite = novoSprite;
        }

        private void Start()
        {
            if (sprite != null) _escalaSprite = sprite.transform.localScale;
            Mostrar();
        }

        /// <summary>Pra onde vai um raio que chega nesta direcao.</summary>
        public Vector2 Refletir(Vector2 direcao)
        {
            // "/" troca x com y; "\" troca e inverte.
            return corte
                ? new Vector2(direcao.y, direcao.x)
                : new Vector2(-direcao.y, -direcao.x);
        }

        private void Mostrar()
        {
            // A arte e o espelho em "/".
            if (sprite != null) sprite.flipX = !corte;
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            // Ainda girando: o golpe so sacode.
            if (_giro >= 0f) return;

            _giro = 0f;
            NoiseSystem.Emitir(transform.position, 5f, 0.2f);
            Sfx.GiroEspelho.Tocar(transform.position);
        }

        private void Update()
        {
            if (_giro < 0f || sprite == null)
            {
                if (_giro >= 0f) { corte = !corte; Mostrar(); _giro = -1f; }
                return;
            }

            // Meia volta no eixo: a face fecha, troca de lado e abre de novo,
            // com um balanco no fim.
            const float tempo = 0.3f;
            float antes = _giro;
            _giro += Time.deltaTime;
            if (antes < tempo * 0.5f && _giro >= tempo * 0.5f)
            {
                corte = !corte;
                Mostrar();
            }

            float k = Mathf.Clamp01(_giro / tempo);
            float largura = Mathf.Abs(Mathf.Cos(k * Mathf.PI));
            float oscilacao = k >= 1f ? 0f : Mathf.Sin(k * Mathf.PI * 3f) * 0.06f * (1f - k);
            sprite.transform.localScale = new Vector3(_escalaSprite.x * Mathf.Max(0.08f, largura), _escalaSprite.y * (1f + oscilacao), _escalaSprite.z);

            if (_giro < tempo + 0.05f) return;

            sprite.transform.localScale = _escalaSprite;
            _giro = -1f;
        }
    }
}
