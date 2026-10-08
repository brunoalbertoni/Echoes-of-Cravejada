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
    ///
    /// As que ja vem na cena tem um id e nao voltam depois de pegas; as que um
    /// inimigo solta nao tem, e quem cuida de nao repetir e quem solta.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RewardPickup : MonoBehaviour
    {
        public enum Kind
        {
            Sapphire,
            ResonanceStrike,
            Chassis,
        }

        [SerializeField] private Kind kind;

        [Tooltip("So pras que ja vem na cena: o nome com que ela lembra que foi pega.")]
        [SerializeField] private string id;

        private static readonly HashSet<string> Taken = new HashSet<string>();

        private float _age;
        private Vector3 _origin;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Taken.Clear();
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Kind newKind, string newId)
        {
            kind = newKind;
            id = newId;
        }

        public static void Drop(Vector3 position, Sprite sprite, Kind kind, Color tint)
        {
            var pickup = new GameObject("Reward_" + kind);
            pickup.transform.position = position + Vector3.up * 0.8f;

            var renderer = pickup.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = -1;

            var trigger = pickup.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            pickup.AddComponent<RewardPickup>().kind = kind;
        }

        private void Awake()
        {
            _origin = transform.position;
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(id) && Taken.Contains(id)) Destroy(gameObject);
        }

        private void Update()
        {
            _age += Time.deltaTime;
            transform.position = _origin + Vector3.up * (Mathf.Sin(_age * 3f) * 0.15f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            switch (kind)
            {
                case Kind.Sapphire:
                    ScrapGems.GiveSapphire();
                    break;

                case Kind.ResonanceStrike:
                    ScrapChisel.UnlockResonanceStrike();
                    break;

                case Kind.Chassis:
                    var health = controller.GetComponent<ScrapHealth>();
                    if (health != null) health.UpgradeChassis();
                    ChassisTime = Time.time;
                    break;
            }

            if (!string.IsNullOrEmpty(id)) Taken.Add(id);

            // Pegar algo assim tilinta: acende em volta.
            NoiseSystem.Emit(transform.position, 8f, 0.2f);
            Destroy(gameObject);
        }

        /// <summary>Quando um reforco escondido foi pego. O HUD usa pra mostrar o aviso.</summary>
        public static float ChassisTime { get; private set; } = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetTime()
        {
            ChassisTime = -100f;
        }
    }
}
