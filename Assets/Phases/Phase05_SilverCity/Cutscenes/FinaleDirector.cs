using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O fim do jogo: O Grande Polimento. Quando o Scrap entrega o nucleo no
    /// Altar, a camera fecha na Coluna, a luz branca desce por ela, a
    /// cutscene desenhada do final passa e por cima de tudo entra o cartao de
    /// FIM.
    ///
    /// Depois disso o jogo fica no cartao: o controle nao volta.
    /// </summary>
    public class FinaleDirector : MonoBehaviour
    {
        [SerializeField] private PurificationCartoon polishCartoon;
        [SerializeField] private SpriteRenderer conduit;
        [SerializeField] private Sprite conduitClean;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Vector2 focus;

        private bool _started;
        private bool _showCard;

        /// <summary>Se o final ja comecou (a trilha sonora troca pra musica do fim).</summary>
        public static bool Running { get; private set; }

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Running = false;
        }
        private float _cardSince;
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _small;
        private GUIStyle _credit;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(PurificationCartoon polish, SpriteRenderer newConduit, Sprite newConduitClean, Light2D global, Vector2 newFocus)
        {
            polishCartoon = polish;
            conduit = newConduit;
            conduitClean = newConduitClean;
            globalLight = global;
            focus = newFocus;
        }

        public void Begin()
        {
            if (_started) return;
            _started = true;
            Running = true;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) controller.ControlLocked = true;
            if (!Cinematic.Active) Cinematic.Begin();

            ScrapCameraFollow.Direct(focus + Vector2.up * 3f, 0.9f, 0.8f);

            // O nucleo acende; a luz branca toma a Coluna de cima pra baixo.
            for (float t = 0f; t < 2.2f; t += Time.deltaTime)
            {
                if (globalLight != null) globalLight.intensity = Mathf.Lerp(globalLight.intensity, 1.1f, Time.deltaTime);
                yield return null;
            }

            if (conduit != null && conduitClean != null) conduit.sprite = conduitClean;

            if (polishCartoon != null && polishCartoon.IsReady) yield return polishCartoon.Play(null);

            _showCard = true;
            _cardSince = Time.time;
        }

        private void OnGUI()
        {
            if (!_showCard) return;

            if (_title == null)
            {
                _title = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 42, fontStyle = FontStyle.Bold, wordWrap = true };
                _body = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 20, wordWrap = true };
                _small = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, wordWrap = true };
                _credit = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 13, wordWrap = true };
            }

            float fade = Mathf.Clamp01((Time.time - _cardSince) / 2f);
            Color previous = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, fade);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            float text = Mathf.Clamp01((Time.time - _cardSince - 1f) / 2f);
            var tint = new Color(1f, 0.95f, 0.8f);

            GUI.color = new Color(tint.r, tint.g, tint.b, text);
            float w = Screen.width;
            float h = Screen.height;

            GUI.Label(new Rect(w * 0.1f, h * 0.18f, w * 0.8f, 60f), "O GRANDE POLIMENTO", _title);

            const string story =
                "O Óxido deixou o Coração de Quartzo sem que ele se partisse. A fumaça verde subiu pelos poços e se foi. " +
                "Nos telhados da Cidade de Prata, os Tarnished acenderam as primeiras lâmpadas. A cidade vai ser reconstruída — " +
                "por mãos de cobre, de latão e de pedra. E por um protótipo que nunca foi polido.";

            GUI.Label(new Rect(w * 0.15f, h * 0.32f, w * 0.7f, h * 0.3f), story, _body);

            GUI.color = new Color(tint.r, tint.g, tint.b, Mathf.Clamp01((Time.time - _cardSince - 3f) / 2f));
            GUI.Label(new Rect(w * 0.1f, h * 0.68f, w * 0.8f, 40f), "FIM", _title);
            GUI.Label(new Rect(w * 0.1f, h * 0.78f, w * 0.8f, 30f), "Echoes of Cravejada", _small);
            GUI.Label(new Rect(w * 0.1f, h * 0.84f, w * 0.8f, 30f), "Obrigado por jogar.", _small);

            // Credito da trilha (a licenca CC BY 4.0 pede o nome do autor).
            GUI.color = new Color(tint.r, tint.g, tint.b, 0.75f * Mathf.Clamp01((Time.time - _cardSince - 5f) / 2f));
            GUI.Label(new Rect(w * 0.1f, h * 0.92f, w * 0.8f, 26f),
                "Música: Scott Buckley — scottbuckley.com.au (CC BY 4.0)", _credit);

            GUI.color = previous;
        }
    }
}
