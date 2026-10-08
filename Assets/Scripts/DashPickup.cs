using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Os pistoes que o Rusty Scrapper deixa ao desmontar. Encostar da o Steam
    /// Dash ao Scrap.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DashPickup : MonoBehaviour
    {
        private float _age;
        private Vector3 _origin;

        public static void Drop(Vector3 position, Sprite sprite)
        {
            var pickup = new GameObject("SteamDashModule");
            pickup.transform.position = position + Vector3.up * 0.6f;

            var renderer = pickup.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = -1;

            var trigger = pickup.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            pickup.AddComponent<DashPickup>();
        }

        private void Awake()
        {
            _origin = transform.position;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            transform.position = _origin + Vector3.up * (Mathf.Sin(_age * 3f) * 0.15f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<ScrapController>() == null) return;

            ScrapDash.Unlock();
            Destroy(gameObject);
        }
    }
}
