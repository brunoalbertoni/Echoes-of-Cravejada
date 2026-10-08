using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A "old scrap": o que fica no lugar onde o Scrap morreu, guardando os
    /// Opaque Shards que ele carregava. Encostar devolve tudo.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class OldScrap : MonoBehaviour
    {
        private int _shards;
        private float _age;

        public static OldScrap Drop(Vector3 position, int shards, Sprite sprite)
        {
            var scrap = new GameObject("OldScrap");
            scrap.transform.position = position;

            var renderer = scrap.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = -2;

            var trigger = scrap.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;
            trigger.offset = new Vector2(0f, 0.5f);

            var old = scrap.AddComponent<OldScrap>();
            old._shards = shards;
            return old;
        }

        private void Update()
        {
            _age += Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Recover(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Recover(other);
        }

        private void Recover(Collider2D other)
        {
            // Ele renasce longe dali, mas a carencia evita pegar no mesmo quadro.
            if (_age < 0.5f) return;
            if (other.GetComponentInParent<ScrapController>() == null) return;

            ScrapWallet.Add(_shards);
            Destroy(gameObject);
        }
    }
}
