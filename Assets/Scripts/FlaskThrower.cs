using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Flask Thrower: um assistente de laboratorio corrompido. Nao chega perto:
    /// fica onde esta e arremessa frascos em arco, que estouram no chao e
    /// deixam uma poca de Oxide por alguns segundos. Fragil — o problema e
    /// chegar nele.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class FlaskThrower : MonoBehaviour
    {
        [Tooltip("Ate onde ele enxerga o Scrap, em tiles.")]
        [SerializeField] private float range = 11f;

        [SerializeField] private float cooldown = 2.6f;

        [SerializeField] private Sprite flaskSprite;
        [SerializeField] private Sprite puddleSprite;

        private SpriteRenderer _sprite;
        private Transform _player;
        private float _readyAt;
        private bool _throwing;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite newFlaskSprite, Sprite newPuddleSprite)
        {
            flaskSprite = newFlaskSprite;
            puddleSprite = newPuddleSprite;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void OnEnable()
        {
            _throwing = false;
            _readyAt = Time.time + 1f;
        }

        private void Update()
        {
            if (_player == null || _throwing || Time.time < _readyAt) return;

            Vector2 offset = _player.position - transform.position;
            if (Mathf.Abs(offset.x) > range || Mathf.Abs(offset.y) > 6f) return;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = offset.x < 0f;

            StartCoroutine(Throw());
        }

        private IEnumerator Throw()
        {
            _throwing = true;

            // O aviso: ele ergue o braco.
            if (_sprite != null) _sprite.color = new Color(0.75f, 1f, 0.75f);
            yield return new WaitForSeconds(0.45f);
            if (_sprite != null) _sprite.color = Color.white;

            if (_player != null)
            {
                Vector2 hand = (Vector2)transform.position + Vector2.up * 1.3f;
                Flask.Throw(hand, _player.position, flaskSprite, puddleSprite);
            }

            _readyAt = Time.time + cooldown;
            _throwing = false;
        }
    }
}
