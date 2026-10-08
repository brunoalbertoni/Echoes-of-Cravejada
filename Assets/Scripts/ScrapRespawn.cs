using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Lembra o ultimo chao seguro em que o Scrap pisou e o devolve pra la
    /// quando ele cai no acido ou nos espinhos.
    ///
    /// "Seguro" e ter ficado no chao por um instante: assim a beirada de onde
    /// ele acabou de pular vale, e um toque de raspao no meio de uma queda nao.
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapRespawn : MonoBehaviour
    {
        [Tooltip("Quanto tempo no chao ate o lugar contar como seguro.")]
        [SerializeField] private float settleTime = 0.2f;

        [Tooltip("Depois de voltar, por quanto tempo ele nao pode ser jogado de novo.")]
        [SerializeField] private float immunityTime = 0.6f;

        private ScrapController _controller;
        private Rigidbody2D _body;
        private SpriteRenderer _sprite;

        private Vector2 _safePosition;
        private float _groundedFor;
        private float _immuneUntil;

        /// <summary>O ultimo chao seguro. E tambem onde a old scrap fica quando ele morre.</summary>
        public Vector2 SafePosition => _safePosition;

        private void Awake()
        {
            _controller = GetComponent<ScrapController>();
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _safePosition = transform.position;
        }

        private void Update()
        {
            _groundedFor = _controller.IsGrounded ? _groundedFor + Time.deltaTime : 0f;

            if (_groundedFor >= settleTime && Time.time >= _immuneUntil)
            {
                _safePosition = _body.position;
            }
        }

        /// <summary>Chamado por quem move o Scrap de uma vez, pra o lugar novo virar o seguro.</summary>
        public void SetSafePosition(Vector2 position)
        {
            _safePosition = position;
            _groundedFor = 0f;
        }

        /// <summary>
        /// Joga o Scrap de volta pro ultimo chao seguro. Com
        /// <paramref name="silent"/> ele nao pisca: quem chamou ja cuida disso.
        /// </summary>
        public void Respawn(bool silent = false)
        {
            if (Time.time < _immuneUntil) return;
            _immuneUntil = Time.time + immunityTime;

            // Um tiquinho acima, pra ele assentar em vez de nascer cravado no chao.
            Vector2 target = _safePosition + Vector2.up * 0.05f;

            _body.position = target;
            transform.position = target;
            _body.linearVelocity = Vector2.zero;
            _groundedFor = 0f;

            if (!silent && _sprite != null)
            {
                StopAllCoroutines();
                StartCoroutine(Blink());
            }
        }

        private IEnumerator Blink()
        {
            float end = Time.time + immunityTime;

            while (Time.time < end)
            {
                _sprite.enabled = !_sprite.enabled;
                yield return new WaitForSeconds(0.07f);
            }

            _sprite.enabled = true;
        }
    }
}
