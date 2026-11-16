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
        [SerializeField] private FinaleDirector director;

        [Tooltip("O desenho que pulsa enquanto da pra escolher (pode ser vazio).")]
        [SerializeField] private SpriteRenderer art;

        [Tooltip("A luz que pulsa junto (pode ser vazia).")]
        [SerializeField] private Light2D glow;

        [SerializeField] private Color pulseColor = new Color(1f, 0.97f, 0.85f);

        private Color _baseColor = Color.white;
        private float _baseGlow;
        private bool _taken;

        /// <summary>Se ja da pra escolher: o Rei caiu e o Crisol ja falou.</summary>
        public static bool Ready { get; set; }

        /// <summary>Se alguma das escolhas ja foi feita.</summary>
        public static bool Chosen { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Ready = false;
            Chosen = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(FinaleDirector newDirector, SpriteRenderer newArt, Light2D newGlow)
        {
            director = newDirector;
            art = newArt;
            glow = newGlow;
        }

        /// <summary>A cena abriu de novo: ninguem escolheu ainda nesta. Quem chama e o FinaleCrisol, que sempre esta ligado.</summary>
        public static void ResetAll()
        {
            Ready = false;
            Chosen = false;
        }

        private void Awake()
        {
            if (art != null) _baseColor = art.color;
            if (glow != null) _baseGlow = glow.intensity;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void Damage(int amount, Vector2 from)
        {
            if (!Ready || Chosen || _taken) return;

            _taken = true;
            Chosen = true;
            Sfx.HeartGive.Play();
            ScrapCameraFollow.Shake(0.25f, 0.5f);
            if (director != null) director.Begin();
        }

        private void Update()
        {
            bool pulsing = Ready && !Chosen;
            float k = pulsing ? 0.5f + 0.5f * Mathf.Sin(Time.time * 3.2f) : 0f;
            if (_taken) k = 1f;

            if (art != null) art.color = Color.Lerp(_baseColor, pulseColor, k * 0.7f);
            if (glow != null) glow.intensity = Mathf.Lerp(glow.intensity, _baseGlow + k * 0.9f, Time.deltaTime * 6f);
        }
    }
}
