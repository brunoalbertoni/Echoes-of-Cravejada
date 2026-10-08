using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Shard Sentinel: uma estatua de cristal animada. Nao sai do lugar. Quando
    /// ouve barulho por perto, vira pra origem do som e lanca um leque de
    /// estilhacos naquela direcao. Quebra com alguns golpes.
    ///
    /// Como atira no som, e nao no Scrap, da pra faze-la gastar a rajada num
    /// cristal ressonante e passar enquanto ela recarrega.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class ShardSentinel : MonoBehaviour
    {
        [Tooltip("Ate onde ela escuta, em tiles.")]
        [SerializeField] private float hearingRange = 9f;

        [Tooltip("Segundos entre ouvir e atirar: o tempo de sair da frente.")]
        [SerializeField] private float windup = 0.45f;

        [Tooltip("Segundos ate poder atirar de novo.")]
        [SerializeField] private float cooldown = 2.2f;

        [SerializeField] private float shardSpeed = 11f;
        [SerializeField] private Sprite shardSprite;

        private SpriteRenderer _sprite;
        private float _readyAt;
        private bool _firing;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite newShardSprite)
        {
            shardSprite = newShardSprite;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;
            _firing = false;
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;
        }

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            if (_firing || Time.time < _readyAt || loudness < 0.3f) return;

            Vector2 muzzle = (Vector2)transform.position + Vector2.up * 1.2f;
            if (Vector2.Distance(origin, muzzle) > Mathf.Min(hearingRange, radius + 1f)) return;

            StartCoroutine(Fire(origin + Vector2.up * 0.6f));
        }

        private IEnumerator Fire(Vector2 target)
        {
            _firing = true;

            // O aviso: ela acende.
            if (_sprite != null) _sprite.color = new Color(1f, 0.85f, 1f);
            yield return new WaitForSeconds(windup);
            if (_sprite != null) _sprite.color = Color.white;

            Vector2 muzzle = (Vector2)transform.position + Vector2.up * 1.2f;
            Vector2 aim = (target - muzzle).normalized;
            if (aim.sqrMagnitude < 0.01f) aim = Vector2.right;

            // Tres estilhacos em leque.
            for (int i = -1; i <= 1; i++)
            {
                Vector2 direction = Quaternion.Euler(0f, 0f, i * 14f) * aim;
                ShardShot.Fire(muzzle, direction * shardSpeed, 14f, shardSprite);
            }

            // O proprio tiro faz barulho: acende a sala em volta dela.
            NoiseSystem.Emit(muzzle, 6f, 0.2f);

            _readyAt = Time.time + cooldown;
            _firing = false;
        }
    }
}
