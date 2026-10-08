using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um espelho de prata num eixo, inclinado a 45 graus. Um raio de luz que
    /// bate nele dobra a esquina. Chegar perto e apertar o botao o vira pro
    /// outro lado: de "/" pra "\" e de volta.
    ///
    /// O espelho tem dois colisores: a area grande onde o Scrap o alcanca (a
    /// deste objeto) e a face pequena onde o raio bate (um filho).
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SilverMirror : MonoBehaviour
    {
        [Tooltip("Verdadeiro se ele esta em \"/\" (sobe pra direita); falso se em \"\\\".")]
        [SerializeField] private bool slash = true;

        [Tooltip("A face onde o raio bate: um colisor pequeno, num filho.")]
        [SerializeField] private Collider2D face;

        [SerializeField] private SpriteRenderer sprite;

        private bool _near;

        public Collider2D Face => face;

        /// <summary>O ponto de onde o raio sai depois de refletir.</summary>
        public Vector2 Center => face != null ? (Vector2)face.bounds.center : (Vector2)transform.position;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(bool startsSlash, Collider2D newFace, SpriteRenderer newSprite)
        {
            slash = startsSlash;
            face = newFace;
            sprite = newSprite;
        }

        private void Start()
        {
            Show();
        }

        /// <summary>Pra onde vai um raio que chega nesta direcao.</summary>
        public Vector2 Reflect(Vector2 direction)
        {
            // "/" troca x com y; "\" troca e inverte.
            return slash
                ? new Vector2(direction.y, direction.x)
                : new Vector2(-direction.y, -direction.x);
        }

        private void Show()
        {
            // A arte e o espelho em "/".
            if (sprite != null) sprite.flipX = !slash;
        }

        private void Update()
        {
            if (!_near) return;

            HoldAction.Show(this, "E: girar o espelho", 0f);

            if (!InteractPressed()) return;

            slash = !slash;
            Show();
            NoiseSystem.Emit(transform.position, 5f, 0.2f);
        }

        private static bool InteractPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame)) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonNorth.wasPressedThisFrame;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<ScrapController>() != null) _near = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<ScrapController>() == null) return;

            _near = false;
            HoldAction.Hide(this);
        }
    }
}
