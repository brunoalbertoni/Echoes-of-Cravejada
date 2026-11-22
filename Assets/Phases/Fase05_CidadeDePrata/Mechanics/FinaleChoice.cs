using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma das duas escolhas do fim, na propria Sala do Trono. Depois que o Rei
    /// cai e o Crisol termina de falar, as duas pulsam; um golpe de cinzel em
    /// uma decide o final e apaga a outra. Nada aparece escrito na tela.
    ///
    ///   a Coluna (atras do trono)   o Scrap entrega o proprio nucleo: a luz
    ///                               desce, cura o Coracao, e ele se apaga.
    ///   a Coroa (caida no chao)     ele quebra as 925 facetas: a luz presa
    ///                               volta pra cidade, e ele continua de pe.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FinaleChoice : MonoBehaviour, IDamageable
    {
        [UnityEngine.Serialization.FormerlySerializedAs("director")] [SerializeField] private FinaleDirector diretor;

        [UnityEngine.Serialization.FormerlySerializedAs("art")] [Tooltip("O desenho que pulsa enquanto da pra escolher (pode ser vazio).")]
        [SerializeField] private SpriteRenderer arte;

        [UnityEngine.Serialization.FormerlySerializedAs("glow")] [Tooltip("A luz que pulsa junto (pode ser vazia).")]
        [SerializeField] private Light2D brilho;

        [UnityEngine.Serialization.FormerlySerializedAs("pulseColor")] [SerializeField] private Color corPulso = new Color(1f, 0.97f, 0.85f);

        private Color _corBase = Color.white;
        private float _brilhoBase;
        private bool _pegos;

        /// <summary>Se ja da pra escolher: o Rei caiu e o Crisol ja falou.</summary>
        public static bool Pronto { get; set; }

        /// <summary>Se alguma das escolhas ja foi feita.</summary>
        public static bool Escolhido { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Pronto = false;
            Escolhido = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(FinaleDirector novoDiretor, SpriteRenderer novoArte, Light2D novoBrilho)
        {
            diretor = novoDiretor;
            arte = novoArte;
            brilho = novoBrilho;
        }

        /// <summary>A cena abriu de novo: ninguem escolheu ainda nesta. Quem chama e o FinaleCrisol, que sempre esta ligado.</summary>
        public static void ReiniciarTudo()
        {
            Pronto = false;
            Escolhido = false;
        }

        private void Awake()
        {
            if (arte != null) _corBase = arte.color;
            if (brilho != null) _brilhoBase = brilho.intensity;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (!Pronto || Escolhido || _pegos) return;

            _pegos = true;
            Escolhido = true;
            Sfx.DarCoracao.Tocar();
            ScrapCameraFollow.Tremer(0.25f, 0.5f);
            if (diretor != null) diretor.Iniciar();
        }

        private void Update()
        {
            bool pulsando = Pronto && !Escolhido;
            float k = pulsando ? 0.5f + 0.5f * Mathf.Sin(Time.time * 3.2f) : 0f;
            if (_pegos) k = 1f;

            if (arte != null) arte.color = Color.Lerp(_corBase, corPulso, k * 0.7f);
            if (brilho != null) brilho.intensity = Mathf.Lerp(brilho.intensity, _brilhoBase + k * 0.9f, Time.deltaTime * 6f);
        }
    }
}
