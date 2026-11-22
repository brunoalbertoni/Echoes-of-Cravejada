using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A alavanca emperrada: a ferrugem travou ela no lugar. Cada golpe de
    /// cinzel a sacode e solta um pouco; no ultimo, ela desce e o portao abre.
    ///
    /// E o primeiro "use o cinzel no mundo" do jogo: o mesmo golpe que derruba
    /// corrompido tambem destrava coisa.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RustyLever : MonoBehaviour, IDamageable, ICueDone
    {
        [UnityEngine.Serialization.FormerlySerializedAs("hitsToFree")] [Tooltip("Quantos golpes ate soltar.")]
        [SerializeField] private int golpesParaLivre = 3;

        [UnityEngine.Serialization.FormerlySerializedAs("gate")] [SerializeField] private Gate portao;
        [UnityEngine.Serialization.FormerlySerializedAs("leverSprite")] [SerializeField] private SpriteRenderer spriteAlavanca;
        [UnityEngine.Serialization.FormerlySerializedAs("cleanSprite")] [SerializeField] private Sprite spriteLimpo;

        private int _golpes;
        private bool _pronto;
        private float _tremor;
        private Vector3 _origemSprite;
        private ScrapController _perto;

        /// <summary>A alavanca perto da qual o Scrap esta, se ainda emperrada. O HUD usa pra mostrar o aviso.</summary>
        public static RustyLever Proximo { get; private set; }

        /// <summary>Quanto ja soltou, de 0 a 1.</summary>
        public float Progresso2 => Mathf.Clamp01(_golpes / (float)Mathf.Max(1, golpesParaLivre));

        public bool EstaPronto => _pronto;

        bool ICueDone.ProntoDeixa => _pronto;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Proximo = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Gate novoPortao, SpriteRenderer novoSpriteAlavanca, Sprite novoSpriteLimpo)
        {
            portao = novoPortao;
            spriteAlavanca = novoSpriteAlavanca;
            spriteLimpo = novoSpriteLimpo;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            _origemSprite = transform.localPosition;
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (_pronto) return;

            _golpes++;
            _tremor = 0.25f;

            if (_golpes < golpesParaLivre) return;

            _pronto = true;
            if (spriteAlavanca != null && spriteLimpo != null) spriteAlavanca.sprite = spriteLimpo;
            if (portao != null) portao.Abrir();
            Sfx.ProntoRaspagem.Tocar();
            if (Proximo == this) Proximo = null;
        }

        private void Update()
        {
            if (_tremor > 0f)
            {
                _tremor -= Time.deltaTime;
                float deslocamento = Mathf.Sin(Time.time * 90f) * 0.06f * Mathf.Clamp01(_tremor / 0.25f);
                transform.localPosition = _origemSprite + new Vector3(deslocamento, 0f, 0f);
            }
            else if (transform.localPosition != _origemSprite)
            {
                transform.localPosition = _origemSprite;
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador == null) return;

            _perto = controlador;
            if (!_pronto) Proximo = this;
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (_perto == null || outro.GetComponentInParent<ScrapController>() != _perto) return;

            _perto = null;
            if (Proximo == this) Proximo = null;
        }
    }
}
