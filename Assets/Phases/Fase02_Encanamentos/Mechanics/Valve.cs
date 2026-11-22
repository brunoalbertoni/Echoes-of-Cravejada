using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma valvula emperrada: a purificacao local do District 2. O volante
    /// travou de ferrugem; cada golpe de cinzel o sacode e solta um pouco, e no
    /// ultimo ele gira e muda a sala — liga vapor, seca acido, abre passagem.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Valve : MonoBehaviour, IDamageable, ICueDone
    {
        [UnityEngine.Serialization.FormerlySerializedAs("hitsToTurn")] [Tooltip("Quantos golpes ate o volante girar.")]
        [SerializeField] private int golpesParaGiro = 3;

        [UnityEngine.Serialization.FormerlySerializedAs("ventsToTurnOn")] [Header("O que ela muda")]
        [SerializeField] private SteamVent[] saidasParaGiroLigado;
        [UnityEngine.Serialization.FormerlySerializedAs("poolsToDrain")] [SerializeField] private AcidPool[] pocasParaDreno;
        [UnityEngine.Serialization.FormerlySerializedAs("gatesToOpen")] [SerializeField] private Gate[] portoesParaAberto;

        [UnityEngine.Serialization.FormerlySerializedAs("wheel")] [Header("Visual")]
        [SerializeField] private SpriteRenderer roda;
        [UnityEngine.Serialization.FormerlySerializedAs("cleanSprite")] [SerializeField] private Sprite spriteLimpo;

        private Sprite _spriteEnferrujado;
        private ScrapController _controlador;
        private int _golpes;
        private bool _girado;
        private float _tremor;

        /// <summary>A valvula perto da qual o Scrap esta, se houver. O HUD usa pro aviso e pra barra.</summary>
        public static Valve Proximo { get; private set; }

        public float Progresso2 => Mathf.Clamp01(_golpes / (float)Mathf.Max(1, golpesParaGiro));

        /// <summary>
        /// Quem abre os portoes e outro (a cena das travas): o volante gira, mas
        /// os portoes ficam esperando a camera chegar neles.
        /// </summary>
        public bool PortoesAdiar { get; set; }

        public Gate[] Portoes => portoesParaAberto;

        public bool EstaGirado => _girado;

        bool ICueDone.ProntoDeixa => _girado;

        /// <summary>Avisa quando o volante gira. O Boiler Master escuta as valvulas da arena.</summary>
        public event System.Action<Valve> Girado;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Proximo = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(SteamVent[] saidas, AcidPool[] pocas, Gate[] portoes,
            SpriteRenderer novoRoda, Sprite novoSpriteLimpo)
        {
            saidasParaGiroLigado = saidas;
            pocasParaDreno = pocas;
            portoesParaAberto = portoes;
            roda = novoRoda;
            spriteLimpo = novoSpriteLimpo;
        }

        private void Awake()
        {
            if (roda != null) _spriteEnferrujado = roda.sprite;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        /// <summary>
        /// Volta a ficar enferrujada, sem desfazer o que ja mudou. O Boiler
        /// Master usa nas valvulas da arena, pra poderem ser raspadas de novo.
        /// </summary>
        public void Enferrujar()
        {
            _girado = false;
            _golpes = 0;
            if (roda != null && _spriteEnferrujado != null) roda.sprite = _spriteEnferrujado;
            if (_controlador != null) Proximo = this;
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (_girado) return;

            _golpes++;
            _tremor = 0.25f;
            if (_golpes < golpesParaGiro) return;

            _girado = true;
            _tremor = 0f;
            if (Proximo == this) Proximo = null;

            if (roda != null && spriteLimpo != null) roda.sprite = spriteLimpo;
            if (roda != null) StartCoroutine(Girar());
            Sfx.GiroValvula.Tocar(transform.position);

            if (saidasParaGiroLigado != null)
            {
                foreach (SteamVent saida in saidasParaGiroLigado)
                {
                    if (saida != null) saida.DefinirLigado(true);
                }
            }

            if (pocasParaDreno != null)
            {
                foreach (AcidPool poca in pocasParaDreno)
                {
                    if (poca != null) poca.Drenar();
                }
            }

            if (portoesParaAberto != null && !PortoesAdiar)
            {
                foreach (Gate portao in portoesParaAberto)
                {
                    if (portao != null) portao.Abrir();
                }
            }

            Girado?.Invoke(this);
        }

        private void Update()
        {
            if (roda == null || _tremor <= 0f) return;

            _tremor -= Time.deltaTime;
            float angulo = Mathf.Sin(Time.time * 80f) * 8f * Mathf.Clamp01(_tremor / 0.25f);
            roda.transform.localRotation = Quaternion.Euler(0f, 0f, _tremor > 0f ? angulo : 0f);
        }

        private IEnumerator Girar()
        {
            const float tempo = 0.8f;

            for (float t = 0f; t < tempo; t += Time.deltaTime)
            {
                roda.transform.localRotation = Quaternion.Euler(0f, 0f, -360f * (t / tempo));
                yield return null;
            }

            roda.transform.localRotation = Quaternion.identity;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador == null) return;

            _controlador = controlador;
            if (!_girado) Proximo = this;
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (_controlador == null || outro.GetComponentInParent<ScrapController>() != _controlador) return;

            _controlador = null;
            if (Proximo == this) Proximo = null;
        }
    }
}
