using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Vai numa valvula do District 2 cujo efeito acontece longe da vista de
    /// quem gira: as tres do fim das alas, que abrem as travas da Escadaria da
    /// Praca. Quando o volante gira, uma cena curta e muda: o Scrap para, a
    /// camera vai ate a trava, ela se abre, e a camera volta. Nada na tela.
    /// </summary>
    [RequireComponent(typeof(Valve))]
    public class LockValveNotice : MonoBehaviour
    {
        [Tooltip("Se abre uma das travas da Escadaria (e mostra a cena).")]
        [SerializeField] private bool countsAsLock = true;

        [Tooltip("Sobra de quando havia aviso escrito. Nao vai mais pra tela.")]
        [SerializeField] private string extra;

        private Valve _valve;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(bool newCountsAsLock, string newExtra)
        {
            countsAsLock = newCountsAsLock;
            extra = newExtra;
        }

        private void Awake()
        {
            _valve = GetComponent<Valve>();
            if (countsAsLock) _valve.DeferGates = true;
        }

        private void OnEnable()
        {
            _valve.Turned += OnTurned;
        }

        private void OnDisable()
        {
            _valve.Turned -= OnTurned;
        }

        private void OnTurned(Valve valve)
        {
            if (countsAsLock) StartCoroutine(ShowLock());
        }

        private IEnumerator ShowLock()
        {
            Gate[] gates = _valve.Gates;
            if (gates == null || gates.Length == 0) yield break;

            Vector2 focus = Vector2.zero;
            int count = 0;
            foreach (Gate gate in gates)
            {
                if (gate == null) continue;
                focus += (Vector2)gate.transform.position;
                count++;
            }

            if (count == 0) yield break;
            focus /= count;

            ScrapController scrap = FindAnyObjectByType<ScrapController>();
            if (scrap != null) scrap.ControlLocked = true;

            // Deixa o volante terminar de girar antes de sair.
            yield return new WaitForSeconds(0.5f);

            ScrapCameraFollow.Direct(focus, 0.85f, 0.55f);
            yield return new WaitForSeconds(1.3f);

            foreach (Gate gate in gates)
            {
                if (gate != null) gate.Open();
            }

            ScrapCameraFollow.Shake(0.08f, 0.5f);
            yield return new WaitForSeconds(1.6f);

            ScrapCameraFollow.Release();
            yield return new WaitForSeconds(0.9f);

            if (scrap != null) scrap.ControlLocked = false;
        }
    }
}
