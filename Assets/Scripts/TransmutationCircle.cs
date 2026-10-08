using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um circulo de transmutacao gravado em prata no chao: a purificacao local
    /// do District 4. Segurar o botao em cima dele faz o Scrap raspar e polir a
    /// runa do centro; quando termina, o circulo acende e transmuta um
    /// material da sala em outro:
    ///
    ///   vidro em prata       a parede some, a ponte aparece
    ///   mercurio em vapor    a poca some, a coluna de vapor aparece
    ///   marmore em areia     o bloco se desfaz
    ///
    /// Pro circulo isso e sempre a mesma coisa: uma lista de objetos que somem
    /// e outra dos que aparecem.
    ///
    /// Enquanto raspa o Scrap nao anda nem pula: fica vulneravel. Apanhar no
    /// meio joga fora o que ja tinha raspado.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TransmutationCircle : MonoBehaviour
    {
        [Tooltip("Segundos segurando ate a runa acender.")]
        [SerializeField] private float scrapeTime = 2.4f;

        [Tooltip("O que o aviso diz que ele transmuta. Ex.: \"vidro em prata\".")]
        [SerializeField] private string effect = "a materia";

        [Header("O que ele muda")]
        [SerializeField] private GameObject[] vanish;
        [SerializeField] private GameObject[] appear;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer rune;
        [SerializeField] private Color litColor = new Color(0.75f, 0.95f, 1f);

        private ScrapController _controller;
        private ScrapHealth _health;
        private float _progress;
        private bool _scraping;
        private bool _lit;

        public bool IsLit => _lit;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(string newEffect, GameObject[] newVanish, GameObject[] newAppear, SpriteRenderer newRune)
        {
            effect = newEffect;
            vanish = newVanish;
            appear = newAppear;
            rune = newRune;
        }

        private void Start()
        {
            // O que ele cria ainda nao existe.
            Set(appear, false);
        }

        private static void Set(GameObject[] objects, bool active)
        {
            if (objects == null) return;

            foreach (GameObject target in objects)
            {
                if (target != null) target.SetActive(active);
            }
        }

        private void Update()
        {
            if (_lit || _controller == null) return;

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

            if (_scraping) _progress += Time.deltaTime;

            HoldAction.Show(this, "Segure E para polir a runa: " + effect, _progress / scrapeTime);

            if (_progress < scrapeTime) return;

            _lit = true;
            _scraping = false;
            _controller.ControlLocked = false;
            HoldAction.Hide(this);

            if (rune != null) rune.color = litColor;

            Set(vanish, false);
            Set(appear, true);

            // A transmutacao estala: quem escuta, escuta.
            NoiseSystem.Emit(transform.position, 10f, 0.5f);
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
