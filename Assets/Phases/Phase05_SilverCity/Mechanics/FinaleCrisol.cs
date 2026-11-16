using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Crisol do fim: quando o Rei termina de cair, ele vem correndo pelo
    /// corredor ate a Sala do Trono, para do lado do Scrap e explica as duas
    /// escolhas sem escolher por ele. So depois disso a Coluna e a Coroa
    /// passam a aceitar o golpe (FinaleChoice.Ready).
    ///
    /// O Crisol da loja, no corredor, some quando este entra.
    /// </summary>
    public class FinaleCrisol : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private GameObject corridorCrisol;
        [SerializeField] private float fromX = 92f;
        [SerializeField] private float toX = 117f;
        [SerializeField] private float floorY = 2f;
        [SerializeField] private float speed = 7.5f;
        [SerializeField, TextArea] private string[] lines;

        private bool _started;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(SpriteRenderer newBody, GameObject newCorridorCrisol, float newFromX, float newToX, float newFloorY, string[] newLines)
        {
            body = newBody;
            corridorCrisol = newCorridorCrisol;
            fromX = newFromX;
            toX = newToX;
            floorY = newFloorY;
            lines = newLines;
        }

        private void Awake()
        {
            FinaleChoice.ResetAll();
        }

        private void Start()
        {
            // Voltou pra cena com o Rei ja caido: o Crisol ja esta la e ja falou.
            if (SilverKing.Defeated)
            {
                _started = true;
                Place(toX);
                if (corridorCrisol != null) corridorCrisol.SetActive(false);
                FinaleChoice.Ready = true;
                return;
            }

            if (body != null) body.enabled = false;
        }

        private void Update()
        {
            if (_started || !SilverKing.Defeated) return;

            // Espera a queda do Rei terminar (a cena dele tem a sua cinematica).
            if (Cinematic.Active || ScriptedDialogue.Active) return;

            _started = true;
            StartCoroutine(Arrive());
        }

        private void Place(float x)
        {
            transform.position = new Vector3(x, floorY, 0f);
            if (body != null)
            {
                body.enabled = true;
                body.flipX = false;
            }
        }

        private IEnumerator Arrive()
        {
            yield return new WaitForSeconds(0.8f);

            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) controller.ControlLocked = true;
            Cinematic.Begin();

            if (corridorCrisol != null) corridorCrisol.SetActive(false);
            Place(fromX);

            // Vem correndo, aos pulinhos, com a camera indo buscar ele.
            float x = fromX;
            while (x < toX)
            {
                x = Mathf.MoveTowards(x, toX, speed * Time.deltaTime);
                float hop = Mathf.Abs(Mathf.Sin(Time.time * 14f)) * 0.18f;
                transform.position = new Vector3(x, floorY + hop, 0f);
                ScrapCameraFollow.Direct(new Vector2(x + 3f, floorY + 3f), 0.85f, 0.35f);
                yield return null;
            }

            transform.position = new Vector3(toX, floorY, 0f);
            Sfx.Land.Play(transform.position);
            yield return new WaitForSeconds(0.4f);

            yield return ScriptedDialogue.PlayAs("crisol", "Crisol", body, lines);

            FinaleChoice.Ready = true;
            ScrapCameraFollow.Release();
            Cinematic.End();
            if (controller != null) controller.ControlLocked = false;
        }
    }
}
