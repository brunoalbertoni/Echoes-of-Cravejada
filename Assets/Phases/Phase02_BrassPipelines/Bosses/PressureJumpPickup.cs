using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O que o Boiler Master deixa ao cair. Encostar da o Pressure Jump ao
    /// Scrap: um segundo pulo, no ar.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressureJumpPickup : MonoBehaviour
    {
        private float _age;
        private Vector3 _origin;

        public static void Drop(Vector3 position, Sprite sprite)
        {
            var pickup = new GameObject("PressureJumpModule");
            pickup.transform.position = position + Vector3.up * 0.8f;

            var renderer = pickup.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.75f, 0.95f, 1f);
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = -1;

            var trigger = pickup.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            pickup.AddComponent<PressureJumpPickup>();
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

            ScrapController.UnlockPressureJump();
            Destroy(gameObject);
        }
    }
}
