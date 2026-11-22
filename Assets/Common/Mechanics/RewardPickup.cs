using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma recompensa flutuando no lugar, que o Scrap pega ao encostar:
    ///
    ///   Safira            a gema que o Resonant guarda
    ///   ResonanceStrike   o que o Geode Drill deixa ao cair
    ///   Chassis           um reforco de chassi escondido (+1 de vida)
    ///   Gem               uma gema lapidada, escondida
    ///   ShardCache        um cofre de fragmentos: Amount de uma vez
    ///
    /// As que ja vem na cena tem um id e nao voltam depois de pegas; as que um
    /// inimigo solta nao tem, e quem cuida de nao repetir e quem solta.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RewardPickup : MonoBehaviour
    {
        public enum Kind
        {
            Safira,
            GolpeRessonancia,
            Chassi,
            Gema,
            CacheFragmento,
        }

        [UnityEngine.Serialization.FormerlySerializedAs("kind")] [SerializeField] private Kind tipo;

        [UnityEngine.Serialization.FormerlySerializedAs("amount")] [Tooltip("So pro cofre: quantos fragmentos ele guarda.")]
        [SerializeField] private int quantia = 1;

        [Tooltip("So pras que ja vem na cena: o nome com que ela lembra que foi pega.")]
        [SerializeField] private string id;

        [SaveProgress("Taken")]
        private static readonly HashSet<string> Pegos = new HashSet<string>();

        private const float AtrasoArmarQueda = 1.2f;

        private float _idade;
        private float _atrasoArmar;
        private Vector3 _pontoOrigem;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Pegos.Clear();
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Kind novoTipo, string novoId, int novoQuantia = 1)
        {
            tipo = novoTipo;
            id = novoId;
            quantia = novoQuantia;
        }

        /// <summary>Se a recompensa com esse id ja foi pega.</summary>
        public static bool FoiPego(string idRecompensa) => !string.IsNullOrEmpty(idRecompensa) && Pegos.Contains(idRecompensa);

        /// <summary>Solta uma recompensa com id: depois de pega, nao volta mais.</summary>
        public static void Soltar(Vector3 posicao, Sprite sprite, Kind tipo, Color tom, string idRecompensa)
        {
            if (FoiPego(idRecompensa)) return;
            Soltar(posicao, sprite, tipo, tom).id = idRecompensa;
        }

        public static RewardPickup Soltar(Vector3 posicao, Sprite sprite, Kind tipo, Color tom)
        {
            var coletavel = Prefabs.Criar("Reward", "Reward_" + tipo);
            coletavel.transform.position = DropSpot.PontoLivre(posicao + Vector3.up * 0.8f);

            var renderizador = Prefabs.Componente<SpriteRenderer>(coletavel);
            renderizador.sprite = sprite;
            renderizador.color = tom;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = -1;

            var gatilho = Prefabs.Componente<CircleCollider2D>(coletavel);
            gatilho.isTrigger = true;
            gatilho.radius = 0.7f;

            var recompensa = coletavel.AddComponent<RewardPickup>();
            recompensa.tipo = tipo;
            // Solta de um chefe: aparece e fica um instante no ar antes de
            // poder ser pega, pra nao entrar sozinha no Scrap que esta ao lado.
            recompensa._atrasoArmar = AtrasoArmarQueda;
            return recompensa;
        }

        private void Awake()
        {
            _pontoOrigem = transform.position;
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(id) && Pegos.Contains(id)) Destroy(gameObject);
        }

        private void Update()
        {
            _idade += Time.deltaTime;
            transform.position = _pontoOrigem + Vector3.up * (Mathf.Sin(_idade * 3f) * 0.15f);
        }

        private void OnTriggerEnter2D(Collider2D outro) => Coletar(outro);

        // Quem ja estava em cima quando ela armou pega sem precisar sair e voltar.
        private void OnTriggerStay2D(Collider2D outro) => Coletar(outro);

        private void Coletar(Collider2D outro)
        {
            if (_idade < _atrasoArmar) return;

            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador == null) return;

            switch (tipo)
            {
                case Kind.Safira:
                    ScrapGems.DarSafira();
                    break;

                case Kind.GolpeRessonancia:
                    // O som e o do card de habilidade, que abre em seguida.
                    ScrapChisel.DesbloquearGolpeRessonante();
                    break;

                case Kind.Chassi:
                    var vida = controlador.GetComponent<ScrapHealth>();
                    if (vida != null) vida.MelhorarChassi();
                    TempoChassi = Time.time;
                    Sfx.Chassi.Tocar();
                    var arte = GetComponentInChildren<SpriteRenderer>();
                    if (arte != null) ItemToast.Mostrar(arte.sprite, new Color(1f, 0.75f, 0.5f));
                    break;

                case Kind.Gema:
                    GemPurse.Adicionar(1);
                    Sfx.Gema.Tocar();
                    break;

                case Kind.CacheFragmento:
                    ScrapWallet.Adicionar(quantia);
                    Sfx.RecuperadosFragmentos.Tocar();
                    ItemToast.Mostrar(ItemToast.SpriteDoFragmento(), new Color(0.6f, 0.9f, 1f), quantia);
                    break;
            }

            if (!string.IsNullOrEmpty(id)) Pegos.Add(id);

            // Pegar algo assim tilinta: acende em volta.
            NoiseSystem.Emitir(transform.position, 8f, 0.2f);
            Destroy(gameObject);
        }

        /// <summary>Quando um reforco escondido foi pego. O HUD usa pra mostrar o aviso.</summary>
        public static float TempoChassi { get; private set; } = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarTempo()
        {
            TempoChassi = -100f;
        }
    }
}
