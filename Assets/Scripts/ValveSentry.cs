using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Valve Sentry: uma torre corrompida, parada, que cospe jatos de vapor
    /// escaldante em linha reta, num ritmo fixo. Da pra contar o tempo e passar
    /// entre um jato e outro, ou pular por cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ValveSentry : MonoBehaviour
    {
        [Tooltip("+1 atira pra direita, -1 pra esquerda.")]
        [SerializeField] private int direction = 1;

        [Tooltip("Segundos entre um jato e o proximo.")]
        [SerializeField] private float interval = 2.2f;

        [Tooltip("Ate onde o jato vai, em tiles.")]
        [SerializeField] private float range = 16f;

        [SerializeField] private float boltSpeed = 8f;
        [SerializeField] private Sprite boltSprite;

        private SpriteRenderer _sprite;
        private float _timer;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(int newDirection, float newRange, Sprite newBoltSprite)
        {
            direction = newDirection;
            range = newRange;
            boltSprite = newBoltSprite;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = direction < 0;

            // Cada torre comeca num ponto diferente do ciclo.
            _timer = Mathf.Repeat(transform.position.x * 0.37f, interval);
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            // O aviso: ela acende um instante antes de atirar.
            if (_sprite != null)
            {
                _sprite.color = interval - _timer < 0.4f ? new Color(1f, 0.85f, 0.6f) : Color.white;
            }

            if (_timer < interval) return;
            _timer = 0f;

            Vector3 muzzle = transform.position + new Vector3(direction * 0.7f, 0.9f, 0f);
            SteamBolt.Fire(muzzle, direction * boltSpeed, range, boltSprite);
        }
    }
}
