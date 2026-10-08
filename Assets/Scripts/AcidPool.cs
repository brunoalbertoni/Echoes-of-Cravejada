using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma poca de acido com nivel. Encostar tira vida e joga o Scrap de volta
    /// pro ultimo chao seguro; o Scrap e imune ao Oxide, nao ao acido.
    ///
    /// O nivel muda: uma valvula ou a purificacao secam a poca, e no poco do
    /// acido ascendente ela sobe. O desenho e o gatilho acompanham o nivel.
    ///
    /// O objeto fica no canto de baixo, a esquerda, do tanque.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class AcidPool : MonoBehaviour
    {
        [SerializeField] private float width = 4f;

        [Tooltip("Altura do acido quando a cena comeca.")]
        [SerializeField] private float height = 2f;

        [Tooltip("Tiles por segundo ao secar.")]
        [SerializeField] private float drainSpeed = 1.5f;

        [SerializeField] private int damage = 1;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer surface;
        [SerializeField] private Transform bubbles;

        private BoxCollider2D _trigger;
        private float _level;
        private float _startLevel;
        private Coroutine _moving;

        /// <summary>A altura atual do acido, a partir do fundo do tanque.</summary>
        public float Level => _level;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newWidth, float newHeight, SpriteRenderer newBody,
            SpriteRenderer newSurface, Transform newBubbles)
        {
            width = newWidth;
            height = newHeight;
            body = newBody;
            surface = newSurface;
            bubbles = newBubbles;
        }

        private void Awake()
        {
            _trigger = GetComponent<BoxCollider2D>();
            _trigger.isTrigger = true;

            _level = height;
            _startLevel = height;
            Apply();

            // Quando o Scrap morre, o acido que estava subindo volta pro lugar.
            ScrapHealth.Died += ResetLevel;
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= ResetLevel;
        }

        private void ResetLevel()
        {
            // Poca que ja secou continua seca: so desfaz subida.
            if (_level <= _startLevel) return;

            Stop();
            _level = _startLevel;
            Apply();
        }

        /// <summary>Seca a poca ate o fundo.</summary>
        public void Drain()
        {
            MoveTo(0f, drainSpeed);
            _startLevel = 0f;
        }

        /// <summary>Faz o acido subir ate uma altura, numa velocidade.</summary>
        public void RiseTo(float targetHeight, float speed)
        {
            if (targetHeight <= _level) return;
            MoveTo(targetHeight, speed);
        }

        private void MoveTo(float target, float speed)
        {
            if (!isActiveAndEnabled)
            {
                _level = target;
                Apply();
                return;
            }

            Stop();
            _moving = StartCoroutine(Move(target, speed));
        }

        private void Stop()
        {
            if (_moving != null) StopCoroutine(_moving);
            _moving = null;
        }

        private IEnumerator Move(float target, float speed)
        {
            while (!Mathf.Approximately(_level, target))
            {
                _level = Mathf.MoveTowards(_level, target, speed * Time.deltaTime);
                Apply();
                yield return null;
            }

            _moving = null;
        }

        /// <summary>Poe o desenho e o gatilho na altura do nivel atual.</summary>
        private void Apply()
        {
            bool any = _level > 0.05f;

            // A superficie fica uns 5 px abaixo do topo, entao o gatilho tambem:
            // so queima quando o pe afunda de verdade.
            float hurt = Mathf.Max(0f, _level - 0.35f);

            _trigger.enabled = any && hurt > 0.01f;
            _trigger.size = new Vector2(Mathf.Max(0.1f, width - 0.2f), Mathf.Max(0.01f, hurt));
            _trigger.offset = new Vector2(width * 0.5f, hurt * 0.5f);

            if (body != null)
            {
                body.enabled = any;
                body.size = new Vector2(width, Mathf.Max(0.01f, _level - 0.3f));
            }

            if (surface != null)
            {
                surface.enabled = any;
                surface.transform.localPosition = new Vector3(0f, Mathf.Max(0f, _level - 0.3f), 0f);
            }

            if (bubbles != null)
            {
                bubbles.gameObject.SetActive(any);
                bubbles.localPosition = new Vector3(width * 0.5f, Mathf.Max(0f, _level - 0.5f), 0f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Burn(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Burn(other);
        }

        private void Burn(Collider2D other)
        {
            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(damage, transform.position, true);
        }
    }
}
