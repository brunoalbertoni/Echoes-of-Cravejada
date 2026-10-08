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
        [SerializeField] private int value = 1;

        private Vector2 _velocity;
        private float _restY;
        private bool _resting;
        private float _age;

        /// <summary>Cria um shard solto, saltando pra um lado.</summary>
        public static void Spawn(Vector3 position, Sprite sprite, int value, int index)
        {
            var shard = new GameObject("OpaqueShard");
            shard.transform.position = position + Vector3.up * 0.3f;

            var renderer = shard.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = -1;

            var trigger = shard.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.45f;

            var pickup = shard.AddComponent<ShardPickup>();
            pickup.value = value;

            // Espalha em leque, alternando os lados.
            float side = index % 2 == 0 ? 1f : -1f;
            pickup._velocity = new Vector2(side * (1f + index * 0.5f), 5.5f);
            pickup._restY = position.y + 0.3f;
        }

        private void Update()
        {
            _age += Time.deltaTime;

            if (_resting)
            {
                transform.position += Vector3.up * (Mathf.Sin(_age * 4f) * 0.1f * Time.deltaTime);
                return;
            }

            _velocity.y -= 22f * Time.deltaTime;
            transform.position += (Vector3)(_velocity * Time.deltaTime);

            // Assenta na altura de onde saiu: e la que esta o chao.
            if (_velocity.y < 0f && transform.position.y <= _restY)
            {
                Vector3 position = transform.position;
                position.y = _restY;
                transform.position = position;
                _resting = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Collect(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Collect(other);
        }

        private void Collect(Collider2D other)
        {
            // Um instante de carencia, senao ele some antes de aparecer.
            if (_age < 0.25f) return;
            if (other.GetComponentInParent<ScrapController>() == null) return;

            ScrapWallet.Add(value);
            Destroy(gameObject);
        }
    }
}
