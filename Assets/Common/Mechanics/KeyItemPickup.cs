using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um item-chave flutuando no lugar: encostar guarda ele no KeyItems.
    /// Depois de pego (ou entregue) nao volta mais.
    ///
    /// Item avulso (o motor, a chave) salta em cima do Scrap (ItemToast); os
    /// de um conjunto (as ferramentas do ourives, os reagentes) so tilintam.
    /// A mensagem fica guardada, mas nao vai mais pra tela.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KeyItemPickup : MonoBehaviour
    {
        [SerializeField] private string id;

        [UnityEngine.Serialization.FormerlySerializedAs("message")] [Tooltip("O aviso na tela ao pegar.")]
        [SerializeField] private string mensagem;

        [UnityEngine.Serialization.FormerlySerializedAs("group")] [Tooltip("O comeco de id do conjunto, se houver. Vazio: item avulso.")]
        [SerializeField] private string grupo;

        [UnityEngine.Serialization.FormerlySerializedAs("groupSize")] [SerializeField] private int tamanhoGrupo;

        private float _idade;
        private Vector3 _pontoOrigem;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(string novoId, string novoMensagem, string novoGrupo = null, int novoTamanhoGrupo = 0)
        {
            id = novoId;
            mensagem = novoMensagem;
            grupo = novoGrupo;
            tamanhoGrupo = novoTamanhoGrupo;
        }

        private void OnEnable()
        {
            // No OnEnable, e nao no Awake: os que so aparecem com a purificacao
            // ficam desligados ate la.
            _pontoOrigem = transform.position;
        }

        private void Start()
        {
            if (KeyItems.FoiEncontrado(id)) Destroy(gameObject);
        }

        private void Update()
        {
            _idade += Time.deltaTime;
            transform.position = _pontoOrigem + Vector3.up * (Mathf.Sin(_idade * 3f) * 0.15f);
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (outro.GetComponentInParent<ScrapController>() == null) return;

            KeyItems.Adicionar(id);

            Sfx.ItemChave.Tocar();

            // Item avulso salta em cima do Scrap; os contados (ferramentas,
            // reagentes) nao mostram nada alem do som.
            bool contado = !string.IsNullOrEmpty(grupo) && tamanhoGrupo > 0;
            var arte = GetComponentInChildren<SpriteRenderer>();
            if (!contado && arte != null) ItemToast.Mostrar(arte.sprite, new Color(1f, 0.88f, 0.55f));

            // Pegar algo assim tilinta: acende em volta.
            NoiseSystem.Emitir(transform.position, 8f, 0.2f);
            Destroy(gameObject);
        }
    }
}
