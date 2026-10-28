using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um elevador de estacao: a saida pro distrito de cima. O Scrap entra na
    /// cabine e segura o botao de interagir; a cabine sobe o poco inteiro com
    /// ele dentro, a tela escurece no alto e a proxima cena carrega.
    ///
    /// Se o poco estiver entupido (o objeto em Blocker ainda ativo, como os
    /// escombros do District 3), a cabine nao sai: so aparece o aviso.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ElevatorRide : MonoBehaviour
    {
        [Tooltip("Altura (y de mundo) em que a cabine chega no alto e a cena troca.")]
        [SerializeField] private float topY;

        [SerializeField] private string sceneName;
        [SerializeField] private string spawnId;

        [Tooltip("Tiles por segundo, na velocidade de cruzeiro.")]
        [SerializeField] private float speed = 6f;

        [Tooltip("Segundos segurando o botao ate a cabine partir.")]
        [SerializeField] private float holdTime = 0.5f;

        [Tooltip("O que entope o poco. Enquanto estiver ativo, a cabine nao sobe.")]
        [SerializeField] private GameObject blocker;

        [Tooltip("O aviso de quando o poco esta entupido.")]
        [SerializeField] private string blockedNotice = "O poço está bloqueado.";

        private ScrapController _controller;
        private float _held;
        private bool _riding;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newTopY, string newSceneName, string newSpawnId)
        {
            topY = newTopY;
            sceneName = newSceneName;
            spawnId = newSpawnId;
        }

        /// <summary>Usado pelos geradores de fase: o que entope o poco, e o aviso.</summary>
        public void ConfigureBlocker(GameObject newBlocker, string newNotice)
        {
            blocker = newBlocker;
            blockedNotice = newNotice;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (_riding || _controller == null) return;

            if (blocker != null && blocker.activeInHierarchy)
            {
                HudNotice.Show(blockedNotice, 0.2f);
                _held = 0f;
                return;
            }

            HudNotice.Show("Segure E para subir no elevador", 0.2f);

            if (InteractHeld() && _controller.IsGrounded) _held += Time.deltaTime;
            else _held = 0f;

            if (_held >= holdTime) StartCoroutine(Ride());
        }

        private IEnumerator Ride()
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("[ElevatorRide] A cena " + sceneName + " nao esta no Build Settings. " +
                               "Gere a fase pelo menu Echoes, que ela e adicionada sozinha.", this);
                yield break;
            }

            _riding = true;
            Cinematic.Begin();

            Transform player = _controller.transform;
            var body = player.GetComponent<Rigidbody2D>();

            _controller.ControlLocked = true;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            // O Scrap vai no meio da cabine.
            Vector3 offset = new Vector3(transform.position.x + 1.5f - player.position.x, 0f, 0f);
            player.position += offset;
            Vector3 riderOffset = player.position - transform.position;

            // O tranco da partida.
            ScrapCameraFollow.Shake(0.08f, 0.35f);
            yield return new WaitForSeconds(0.6f);

            float startY = transform.position.y;
            float distance = Mathf.Max(1f, topY - startY);
            float current = 0f;

            while (transform.position.y < topY)
            {
                // Arranca devagar e pega velocidade.
                current = Mathf.MoveTowards(current, speed, speed * 0.8f * Time.deltaTime);

                Vector3 position = transform.position;
                position.y = Mathf.Min(topY, position.y + current * Time.deltaTime);
                transform.position = position;
                player.position = position + riderOffset;

                // Os ultimos tiles escurecem a tela.
                float left = topY - position.y;
                Cinematic.Fade = Mathf.Clamp01(1f - left / Mathf.Min(6f, distance));

                yield return null;
            }

            Cinematic.Fade = 1f;
            yield return new WaitForSeconds(0.3f);

            // O estado de cena e estatico: tem que acabar aqui, senao a
            // proxima fase comeca com as faixas pretas e o Scrap imune.
            Cinematic.End();
            SceneTravel.PendingSpawnId = spawnId;
            SceneManager.LoadScene(sceneName);
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
            if (controller != null) _controller = controller;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_riding || _controller == null) return;
            if (other.GetComponentInParent<ScrapController>() != _controller) return;

            _controller = null;
            _held = 0f;
        }
    }
}
