using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um fim do jogo, tocado por uma das escolhas (o trono ou a coroa): a
    /// camera fecha no ponto da escolha, a sala clareia, o filme do final
    /// passa e, no preto, entra a tela de fim (EndScreen) daquele final, com
    /// a historia curta e, depois de uns segundos, a placa de voltar ao menu.
    ///
    /// Depois disso o controle nao volta.
    /// </summary>
    public class FinaleDirector : MonoBehaviour
    {
        [SerializeField] private FilmCutscene polishCartoon;
        [SerializeField] private SpriteRenderer conduit;
        [SerializeField] private Sprite conduitClean;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Vector2 focus;

        [Header("A tela de fim")]
        [Tooltip("O nome do final em Resources/EndScreen ({id}_title, {id}_line0...).")]
        [SerializeField] private string endId = "column";
        [SerializeField] private int lineCount = 5;

        [Tooltip("Esporos verdes subindo na tela de fim (o Oxide ainda esta la).")]
        [SerializeField] private bool spores;

        private bool _started;

        /// <summary>Se o final ja comecou (a trilha sonora troca pra musica do fim).</summary>
        public static bool Running { get; private set; }

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Running = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(FilmCutscene polish, SpriteRenderer newConduit, Sprite newConduitClean, Light2D global, Vector2 newFocus)
        {
            polishCartoon = polish;
            conduit = newConduit;
            conduitClean = newConduitClean;
            globalLight = global;
            focus = newFocus;
        }

        /// <summary>Usado pelos geradores de fase: qual tela de fim, quantas linhas e se tem esporos.</summary>
        public void ConfigureEnding(string newId, int newLines, bool newSpores)
        {
            endId = newId;
            lineCount = newLines;
            spores = newSpores;
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

            // O golpe: a sala clareia antes do filme.
            Sfx.FinaleLight.Play(delay: 0.3f);
            for (float t = 0f; t < 2.2f; t += Time.deltaTime)
            {
                if (globalLight != null) globalLight.intensity = Mathf.Lerp(globalLight.intensity, 1.1f, Time.deltaTime);
                yield return null;
            }

            if (conduit != null && conduitClean != null) conduit.sprite = conduitClean;

            // O filme termina no preto; a tela de fim entra por baixo dele.
            if (polishCartoon != null && polishCartoon.IsReady) yield return polishCartoon.Play(() => EndScreen.Show(endId, lineCount, spores));
            else EndScreen.Show(endId, lineCount, spores);
        }
    }
}
