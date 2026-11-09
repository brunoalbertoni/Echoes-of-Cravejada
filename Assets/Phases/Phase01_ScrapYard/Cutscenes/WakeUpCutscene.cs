using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A primeira cena do jogo: o Scrap acordando no fundo do poco.
    ///
    /// A tela comeca preta. Ele esta caido de lado no monte de sucata, apagado;
    /// o preto se abre, os olhos piscam ate pegar, ele se levanta com um tranco
    /// e o jogo comeca. E/Espaco/Enter pulam a cena.
    ///
    /// So roda uma vez por sessao, e so quando a cena comeca do jeito normal:
    /// voltando do District 2 (por uma SceneExit) ele ja chega acordado.
    /// </summary>
    // Antes do SceneArrival, que consome o recado de quem veio de outra cena.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(ScrapController))]
    public class WakeUpCutscene : MonoBehaviour
    {
        [SerializeField] private float darkTime = 0.6f;
        [SerializeField] private float fadeTime = 1.6f;
        [SerializeField] private float flickerTime = 1.4f;

        [Tooltip("A cor dele apagado, antes de os olhos pegarem.")]
        [SerializeField] private Color offColor = new Color(0.35f, 0.38f, 0.40f);

        private static bool _played;

        private ScrapController _controller;
        private Rigidbody2D _body;
        private SpriteRenderer _sprite;
        private bool _skip;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _played = false;
        }

        private void Awake()
        {
            _controller = GetComponent<ScrapController>();
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            bool arriving = !string.IsNullOrEmpty(SceneTravel.PendingSpawnId);
            if (_played || arriving) return;

            _played = true;

            // Ja comeca no preto, antes do primeiro quadro aparecer.
            Cinematic.Begin();
            Cinematic.Fade = 1f;

            StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            // Espera o chao existir (o SceneArrival segura o Scrap ate la) e ele assentar.
            for (float t = 0f; t < 3f && !(_body.simulated && _controller.IsGrounded); t += Time.deltaTime)
            {
                _controller.ControlLocked = true;
                yield return null;
            }

            Vector3 standing = transform.position;
            Quaternion upright = transform.rotation;

            // Caido de lado: sem fisica, deitado e erguido o bastante pra nao
            // afundar no monte (o pivo e nos pes).
            _controller.ControlLocked = true;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            transform.position = standing + Vector3.up * Mathf.Max(0f, standing.y - _sprite.bounds.min.y);
            _sprite.color = offColor;

            yield return Wait(darkTime);

            for (float t = 0f; t < fadeTime && !_skip; t += Time.deltaTime)
            {
                Cinematic.Fade = 1f - t / fadeTime;
                Hold();
                yield return null;
            }

            Cinematic.Fade = 0f;

            // Os olhos pegando: acende, apaga, acende de vez.
            float[] blinks = { 0.15f, 0.25f, 0.12f, 0.15f, 0.08f, 0.4f };
            float step = flickerTime / blinks.Length;
            for (int i = 0; i < blinks.Length && !_skip; i++)
            {
                _sprite.color = i % 2 == 0 ? Color.white : offColor;
                if (i == blinks.Length - 1) NoiseSystem.Emit(transform.position, 6f, 0.3f);
                yield return Wait(Mathf.Max(blinks[i], step * 0.5f));
            }

            // Levanta: de volta em pe, com um tranco pra cima.
            const float riseTime = 0.25f;
            Vector3 lying = transform.position;
            Quaternion side = transform.rotation;
            for (float t = 0f; t < riseTime && !_skip; t += Time.deltaTime)
            {
                float k = t / riseTime;
                transform.rotation = Quaternion.Slerp(side, upright, k);
                transform.position = Vector3.Lerp(lying, standing, k);
                Hold();
                yield return null;
            }

            transform.rotation = upright;
            transform.position = standing;
            _sprite.color = Color.white;
            _body.position = standing;
            _body.simulated = true;
            _body.linearVelocity = _skip ? Vector2.zero : new Vector2(0f, 6f);

            ScrapCameraFollow.Shake(0.08f, 0.25f);

            // Deixa ele cair de volta no monte antes de soltar o controle.
            yield return null;
            for (float t = 0f; t < 1f && !_controller.IsGrounded; t += Time.deltaTime)
            {
                _controller.ControlLocked = true;
                yield return null;
            }

            _controller.ControlLocked = false;
            Cinematic.End();
        }

        private IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds && !_skip; t += Time.deltaTime)
            {
                Hold();
                yield return null;
            }
        }

        /// <summary>Mantem o controle travado e escuta o botao de pular a cena.</summary>
        private void Hold()
        {
            _controller.ControlLocked = true;

            // Meio segundo de tolerancia: o botao que comecou o jogo nao pula a cena.
            if (Time.time - Cinematic.StartTime > 0.5f && SkipPressed()) _skip = true;
        }

        private static bool SkipPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame
                || keyboard.enterKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad pad = Gamepad.current;
            return pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame);
        }
    }
}
