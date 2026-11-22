using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um Opaque Shard no chao. Salta um pouco ao nascer, assenta e fica
    /// balancando ate o Scrap encostar.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShardPickup : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("value")] [SerializeField] private int valor = 1;

        private Vector2 _velocidadeVetor;
        private float _descansoY;
        private bool _descansando;
        private float _idade;

        /// <summary>Cria um shard solto, saltando pra um lado.</summary>
        public static void Criar(Vector3 posicao, Sprite sprite, int valor, int indice)
        {
            var fragmento = Prefabs.Criar("OpaqueShard");
            fragmento.transform.position = posicao + Vector3.up * 0.3f;

            var renderizador = Prefabs.Componente<SpriteRenderer>(fragmento);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = -1;

            var gatilho = Prefabs.Componente<CircleCollider2D>(fragmento);
            gatilho.isTrigger = true;
            gatilho.radius = 0.45f;

            var coletavel = fragmento.AddComponent<ShardPickup>();
            coletavel.valor = valor;

            // Espalha em leque, alternando os lados.
            float lado = indice % 2 == 0 ? 1f : -1f;
            coletavel._velocidadeVetor = new Vector2(lado * (1f + indice * 0.5f), 5.5f);
            coletavel._descansoY = posicao.y + 0.3f;
        }

        private void Update()
        {
            _idade += Time.deltaTime;

            if (_descansando)
            {
                transform.position += Vector3.up * (Mathf.Sin(_idade * 4f) * 0.1f * Time.deltaTime);
                return;
            }

            _velocidadeVetor.y -= 22f * Time.deltaTime;
            transform.position += (Vector3)(_velocidadeVetor * Time.deltaTime);

            // Assenta na altura de onde saiu: e la que esta o chao.
            if (_velocidadeVetor.y < 0f && transform.position.y <= _descansoY)
            {
                Vector3 posicao = transform.position;
                posicao.y = _descansoY;
                transform.position = posicao;
                _descansando = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Coletar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Coletar(outro);
        }

        private void Coletar(Collider2D outro)
        {
            // Um instante de carencia, senao ele some antes de aparecer.
            if (_idade < 0.25f) return;
            if (outro.GetComponentInParent<ScrapController>() == null) return;

            ScrapWallet.Adicionar(valor);
            TocarTilintar();
            Destroy(gameObject);
        }

        // Varios pegos em seguida sobem de tom, nota a nota (escala maior).
        private static readonly int[] PassosCorrente = { 0, 2, 4, 5, 7, 9, 11, 12 };
        private static float _ultimoTilintar = -100f;
        private static int _corrente;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _ultimoTilintar = -100f;
            _corrente = 0;
        }

        private static void TocarTilintar()
        {
            _corrente = Time.time - _ultimoTilintar < 0.45f ? Mathf.Min(_corrente + 1, PassosCorrente.Length - 1) : 0;
            _ultimoTilintar = Time.time;
            Sfx.Fragmento.Tocar(tom2: Mathf.Pow(2f, PassosCorrente[_corrente] / 12f));
        }
    }
}
