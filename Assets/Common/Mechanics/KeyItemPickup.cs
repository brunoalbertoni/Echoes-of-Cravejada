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

        [Tooltip("O aviso na tela ao pegar.")]
        [SerializeField] private string message;

        [Tooltip("O comeco de id do conjunto, se houver. Vazio: item avulso.")]
        [SerializeField] private string group;

        [SerializeField] private int groupSize;

        private float _age;
        private Vector3 _origin;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(string newId, string newMessage, string newGroup = null, int newGroupSize = 0)
        {
            id = newId;
            message = newMessage;
            group = newGroup;
            groupSize = newGroupSize;
        }

        private void OnEnable()
        {
            // No OnEnable, e nao no Awake: os que so aparecem com a purificacao
            // ficam desligados ate la.
            _origin = transform.position;
        }

        private void Start()
        {
            if (KeyItems.WasFound(id)) Destroy(gameObject);
        }

        private void Update()
        {
            _age += Time.deltaTime;
            transform.position = _origin + Vector3.up * (Mathf.Sin(_age * 3f) * 0.15f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<ScrapController>() == null) return;

            KeyItems.Add(id);

            Sfx.KeyItem.Play();

            // Item avulso salta em cima do Scrap; os contados (ferramentas,
            // reagentes) nao mostram nada alem do som.
            bool counted = !string.IsNullOrEmpty(group) && groupSize > 0;
            var art = GetComponentInChildren<SpriteRenderer>();
            if (!counted && art != null) ItemToast.Show(art.sprite, new Color(1f, 0.88f, 0.55f));

            // Pegar algo assim tilinta: acende em volta.
            NoiseSystem.Emit(transform.position, 8f, 0.2f);
            Destroy(gameObject);
        }
    }
}
