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
            pickup.transform.position = DropSpot.Free(position + Vector3.up * 0.8f);

            var renderer = pickup.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = -1;

            var trigger = pickup.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            pickup.AddComponent<PressureJumpPickup>();
            Sfx.ModuleDrop.Play(position);
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

        // Fica um instante no ar antes de poder ser pego: nao entra sozinho no Scrap ao lado.
        private const float ArmDelay = 1.2f;

        private void OnTriggerEnter2D(Collider2D other) => Collect(other);
        private void OnTriggerStay2D(Collider2D other) => Collect(other);

        private void Collect(Collider2D other)
        {
            if (_age < ArmDelay || other.GetComponentInParent<ScrapController>() == null) return;

            Sfx.SteamRefresh.Play();
            ScrapController.UnlockPressureJump();
            Destroy(gameObject);
        }
    }
}
