using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma goteira de mercurio corrompido no teto: de tempos em tempos solta
    /// uma gota gorda, que cai reto. Se acerta o Scrap, gruda no corpo e ele
    /// fica lento por alguns segundos. Nao tira vida — tira o ritmo.
    /// </summary>
    public class MercuryDrip : MonoBehaviour
    {
        [SerializeField] private float interval = 2.4f;
        [SerializeField] private float delay;

        [Tooltip("Quantos tiles a gota cai antes de bater no chao.")]
        [SerializeField] private float fall = 6f;

        [SerializeField] private float slowTime = 3f;
        [SerializeField] private Sprite dropSprite;

        private float _timer;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newInterval, float newDelay, float newFall, Sprite newDropSprite)
        {
            interval = newInterval;
            delay = newDelay;
            fall = newFall;
            dropSprite = newDropSprite;
        }

        private void Start()
        {
            _timer = delay;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            _timer = interval;
            MercuryDrop.Fall(transform.position, fall, slowTime, dropSprite);
        }
    }
}
