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
            Sapphire,
            ResonanceStrike,
            Chassis,
            Gem,
            ShardCache,
        }

        [SerializeField] private Kind kind;

        [Tooltip("So pro cofre: quantos fragmentos ele guarda.")]
        [SerializeField] private int amount = 1;

        [Tooltip("So pras que ja vem na cena: o nome com que ela lembra que foi pega.")]
        [SerializeField] private string id;

        private static readonly HashSet<string> Taken = new HashSet<string>();

        private const float DropArmDelay = 1.2f;

        private float _age;
        private float _armDelay;
        private Vector3 _origin;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Taken.Clear();
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Kind newKind, string newId, int newAmount = 1)
        {
            kind = newKind;
            id = newId;
            amount = newAmount;
        }

        /// <summary>Se a recompensa com esse id ja foi pega.</summary>
        public static bool WasTaken(string rewardId) => !string.IsNullOrEmpty(rewardId) && Taken.Contains(rewardId);

        /// <summary>Solta uma recompensa com id: depois de pega, nao volta mais.</summary>
        public static void Drop(Vector3 position, Sprite sprite, Kind kind, Color tint, string rewardId)
        {
            if (WasTaken(rewardId)) return;
            Drop(position, sprite, kind, tint).id = rewardId;
        }

        public static RewardPickup Drop(Vector3 position, Sprite sprite, Kind kind, Color tint)
        {
            var pickup = new GameObject("Reward_" + kind);
            pickup.transform.position = DropSpot.Free(position + Vector3.up * 0.8f);

            var renderer = pickup.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = -1;

            var trigger = pickup.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            var reward = pickup.AddComponent<RewardPickup>();
            reward.kind = kind;
            // Solta de um chefe: aparece e fica um instante no ar antes de
            // poder ser pega, pra nao entrar sozinha no Scrap que esta ao lado.
            reward._armDelay = DropArmDelay;
            return reward;
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

        private void OnTriggerEnter2D(Collider2D other) => Collect(other);

        // Quem ja estava em cima quando ela armou pega sem precisar sair e voltar.
        private void OnTriggerStay2D(Collider2D other) => Collect(other);

        private void Collect(Collider2D other)
        {
            if (_age < _armDelay) return;

            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            switch (kind)
            {
                case Kind.Sapphire:
                    ScrapGems.GiveSapphire();
                    break;

                case Kind.ResonanceStrike:
                    // O som e o do card de habilidade, que abre em seguida.
                    ScrapChisel.UnlockResonanceStrike();
                    break;

                case Kind.Chassis:
                    var health = controller.GetComponent<ScrapHealth>();
                    if (health != null) health.UpgradeChassis();
                    ChassisTime = Time.time;
                    Sfx.Chassis.Play();
                    var art = GetComponentInChildren<SpriteRenderer>();
                    if (art != null) ItemToast.Show(art.sprite, new Color(1f, 0.75f, 0.5f));
                    break;

                case Kind.Gem:
                    GemPurse.Add(1);
                    Sfx.Gem.Play();
                    break;

                case Kind.ShardCache:
                    ScrapWallet.Add(amount);
                    Sfx.ShardsRecovered.Play();
                    ItemToast.Show(ItemToast.ShardSprite(), new Color(0.6f, 0.9f, 1f), amount);
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
