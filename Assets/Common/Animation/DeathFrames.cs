using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A animacao de morte de quem e animado pelo MotionFrames: quando o
    /// Damageable morre, desliga o cerebro e o andar, toca os quadros de
    /// morte e so entao deixa ele sumir (Damageable.DeathAnimation). Quando o
    /// Scrap morre e todos voltam, religa tudo.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class DeathFrames : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 10f;

        [Tooltip("O que para de pensar e andar enquanto ele morre (o cerebro e o MotionFrames).")]
        [SerializeField] private Behaviour[] silence;

        private Damageable _damageable;
        private SpriteRenderer _sprite;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite[] newFrames, float newFramesPerSecond, Behaviour[] newSilence)
        {
            frames = newFrames;
            framesPerSecond = newFramesPerSecond;
            silence = newSilence;
        }

        private void Awake()
        {
            _damageable = GetComponent<Damageable>();
            _sprite = GetComponent<SpriteRenderer>();

            _damageable.DeathAnimation = Play;
            _damageable.Revived += WakeUp;
        }

        private void OnDestroy()
        {
            if (_damageable != null) _damageable.Revived -= WakeUp;
        }

        private IEnumerator Play()
        {
            SetSilenced(true);

            if (frames != null)
            {
                float step = 1f / Mathf.Max(1f, framesPerSecond);
                foreach (Sprite frame in frames)
                {
                    if (frame != null && _sprite != null) _sprite.sprite = frame;
                    yield return new WaitForSeconds(step);
                }
            }
        }

        private void WakeUp()
        {
            SetSilenced(false);
            if (_sprite != null && frames != null && frames.Length > 0) _sprite.color = Color.white;
        }

        private void SetSilenced(bool silenced)
        {
            if (silence == null) return;
            foreach (Behaviour part in silence)
            {
                if (part != null) part.enabled = !silenced;
            }
        }
    }
}
