using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Toca uma lista de quadros em loop: o parado dos descartados do
    /// District 1 (respirando, com o detalhe de cada um mexendo).
    ///
    /// O quadro de piscar, se houver, so aparece um ciclo em tres; nos
    /// outros fica o anterior. Nao mexe no flipX: quem vira o rosto e o Npc.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteLoop : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 6f;

        [Tooltip("O quadro de olhos fechados. -1: nao pisca.")]
        [SerializeField] private int blinkFrame = -1;

        private SpriteRenderer _renderer;
        private float _offset;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite[] newFrames, float newFramesPerSecond, int newBlinkFrame)
        {
            frames = newFrames;
            framesPerSecond = newFramesPerSecond;
            blinkFrame = newBlinkFrame;
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();

            // Cada um num ponto do ciclo, pra nao respirarem todos juntos.
            _offset = Mathf.Abs(transform.position.x * 0.37f + transform.position.y * 0.11f);
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            float clock = (Time.time + _offset) * framesPerSecond;
            int frame = (int)clock % frames.Length;

            if (frame == blinkFrame && frame > 0 && (int)(clock / frames.Length) % 3 != 0) frame--;

            if (frames[frame] != null) _renderer.sprite = frames[frame];
        }
    }
}
