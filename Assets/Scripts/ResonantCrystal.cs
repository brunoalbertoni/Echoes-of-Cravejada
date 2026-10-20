using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um cristal ressonante: golpeado com o cinzel, ele soa uma nota — um
    /// ruido forte, que vem dele e nao do Scrap. Por isso serve de isca: os
    /// que cacam pelo som correm pro cristal.
    ///
    /// Cada um tem a sua nota (um numero e uma cor). A Porta Harmonica escuta
    /// a ordem em que eles sao tocados.
    ///
    /// Um cristal corrompido tem um veio de Oxide dentro e nao soa: da so um
    /// baque surdo. Raspar o veio (segurar E, vulneravel) o afina de volta.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ResonantCrystal : MonoBehaviour, IDamageable
    {
        [Tooltip("Qual nota ele toca. A Porta Harmonica compara com a sequencia dela.")]
        [SerializeField] private int note;

        [SerializeField] private Color noteColor = Color.white;

        [Tooltip("Ate onde a nota se ouve, em tiles.")]
        [SerializeField] private float radius = 14f;

        [Tooltip("Se comeca com um veio de Oxide, sem soar.")]
        [SerializeField] private bool corrupted;

        [SerializeField] private float scrapeTime = 2.2f;

        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private SpriteRenderer vein;
        [SerializeField] private EchoCrystal echo;

        private ScrapController _controller;
        private ScrapHealth _health;
        private float _progress;
        private bool _scraping;
        private float _flashUntil;

        public int Note => note;
        public Color NoteColor => noteColor;
        public bool IsCorrupted => corrupted;

        /// <summary>Avisa que ele soou. A Porta Harmonica escuta.</summary>
        public event System.Action<ResonantCrystal> Rang;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(int newNote, Color newColor, bool startsCorrupted,
            SpriteRenderer newSprite, SpriteRenderer newVein, EchoCrystal newEcho)
        {
            note = newNote;
            noteColor = newColor;
            corrupted = startsCorrupted;
            sprite = newSprite;
            vein = newVein;
            echo = newEcho;
        }

        private void OnEnable()
        {
            AreaPurifier.Purifying += Cleanse;
        }

        private void OnDisable()
        {
            AreaPurifier.Purifying -= Cleanse;
        }

        private void Start()
        {
            if (AreaPurifier.IsPurified) corrupted = false;
            if (vein != null) vein.enabled = corrupted;
        }

        /// <summary>O veio de Oxide se dissolve: e o que a purificacao faz com todos de uma vez.</summary>
        public void Cleanse()
        {
            if (_scraping && _controller != null) _controller.ControlLocked = false;

            corrupted = false;
            _scraping = false;
            if (vein != null) vein.enabled = false;
            HoldAction.Hide(this);
        }

        /// <summary>
        /// Faz o cristal acender na cor dele sem soar. E como a porta "toca" a
        /// sequencia pro jogador ver.
        /// </summary>
        public void Preview()
        {
            _flashUntil = Time.time + 0.45f;
            if (echo != null) echo.Flash();
        }

        public void Damage(int amount, Vector2 from)
        {
            if (corrupted)
            {
                // Desafinado: um baque curto, que mal se ouve.
                NoiseSystem.Emit(transform.position, 3f, 0.3f);
                return;
            }

            _flashUntil = Time.time + 0.5f;
            if (echo != null) echo.Flash();

            NoiseSystem.Emit(transform.position, radius, 1f);
            Rang?.Invoke(this);
        }

        private void Update()
        {
            if (sprite != null)
            {
                // Aceso ele mostra a cor da nota; apagado, um resto dela.
                Color dim = Color.Lerp(Color.black, noteColor, 0.45f);
                sprite.color = Time.time < _flashUntil ? noteColor : dim;
            }

            if (!corrupted || _controller == null) return;

            bool holding = InteractHeld() && _controller.IsGrounded;

            // Apanhou no meio: perde o que tinha raspado.
            if (_scraping && _health != null && Time.time - _health.LastHitTime < 0.1f)
            {
                _progress = 0f;
                holding = false;
            }

            if (holding != _scraping)
            {
                _scraping = holding;
                _controller.ControlLocked = holding;
            }

            if (_scraping)
            {
                _progress += Time.deltaTime;

                // Raspar faz um barulhinho constante: quem esta perto escuta.
                if (Time.frameCount % 20 == 0) NoiseSystem.Emit(transform.position, 4f, 0.3f);
            }

            HoldAction.Show(this, "Segure E para raspar o veio de Óxido", _progress / scrapeTime);

            if (_progress < scrapeTime) return;

            corrupted = false;
            _scraping = false;
            _controller.ControlLocked = false;
            if (vein != null) vein.enabled = false;
            HoldAction.Hide(this);
        }

        private static bool InteractHeld()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.isPressed || keyboard.kKey.isPressed)) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonNorth.isPressed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            _controller = controller;
            _health = controller.GetComponent<ScrapHealth>();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_controller == null || other.GetComponentInParent<ScrapController>() != _controller) return;

            if (_scraping) _controller.ControlLocked = false;

            _scraping = false;
            _controller = null;
            _health = null;
            HoldAction.Hide(this);
        }
    }
}
