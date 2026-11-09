using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um circulo de transmutacao no estrado do Grande Laboratorio. Como os da
    /// fase, polir a runa pede segurar o botao parado em cima dele — e apanhar
    /// no meio joga fora o que ja tinha polido. Aqui e mais rapido, e serve
    /// pra uma coisa so: se o Alquimista estiver perto quando a runa acender,
    /// ele vira prata.
    ///
    /// Depois de usado, o Alquimista o afoga em mercurio (Smother): a runa
    /// apaga e fica inutil por um tempo, e o Scrap tem que ir a outro.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    public class ArenaCircle : MonoBehaviour
    {
        [SerializeField] private float polishTime = 1.1f;
        [SerializeField] private SpriteRenderer rune;
        [SerializeField] private Color idleColor = new Color(0.62f, 0.68f, 0.82f);
        [SerializeField] private Color litColor = new Color(0.80f, 1f, 1f);
        [SerializeField] private Color smotheredColor = new Color(0.30f, 0.32f, 0.40f);

        private ScrapController _controller;
        private ScrapHealth _health;
        private float _progress;
        private bool _scraping;
        private float _smotheredUntil;
        private float _flashUntil;

        /// <summary>Avisa quando a runa termina de ser polida.</summary>
        public event Action<ArenaCircle> Polished;

        /// <summary>So funciona com a luta acontecendo.</summary>
        public bool Armed { get; set; }

        public bool Usable => Armed && Time.time >= _smotheredUntil;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(SpriteRenderer newRune, float newPolishTime)
        {
            rune = newRune;
            polishTime = newPolishTime;
        }

        /// <summary>Afogado em mercurio: inutil por tantos segundos.</summary>
        public void Smother(float seconds)
        {
            _smotheredUntil = Time.time + seconds;
            StopScraping();
        }

        /// <summary>Volta como no comeco da luta.</summary>
        public void ResetCircle()
        {
            _smotheredUntil = 0f;
            _flashUntil = 0f;
            StopScraping();
        }

        private void Update()
        {
            if (rune != null)
            {
                Color target = Time.time < _flashUntil ? litColor : Usable ? idleColor : smotheredColor;
                if (_scraping) target = Color.Lerp(idleColor, litColor, _progress / polishTime);
                rune.color = Color.Lerp(rune.color, target, 10f * Time.deltaTime);
            }

            if (_controller == null || !Usable)
            {
                if (_scraping) StopScraping();
                return;
            }

            bool holding = InteractHeld() && _controller.IsGrounded;

            if (_scraping && _health != null && Time.time - _health.LastHitTime < 0.1f)
            {
                _progress = 0f;
                holding = false;
            }

            if (holding != _scraping)
            {
                _scraping = holding;
                _controller.ControlLocked = holding;
                if (!holding) _progress = 0f;
            }

            if (_scraping) _progress += Time.deltaTime;

            HoldAction.Show(this, "Segure E para polir a runa", _progress / polishTime);

            if (_progress < polishTime) return;

            StopScraping();
            _flashUntil = Time.time + 0.5f;
            NoiseSystem.Emit(transform.position, 10f, 0.5f);
            Polished?.Invoke(this);
        }

        private void StopScraping()
        {
            if (_scraping && _controller != null) _controller.ControlLocked = false;
            _scraping = false;
            _progress = 0f;
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

            StopScraping();
            _controller = null;
            _health = null;
        }
    }
}
