using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma area que purifica o distrito quando o Scrap chega nela. No
    /// District 2 fica no topo do poco de fuga: a purificacao so vem depois de
    /// derrotar o Boiler Master E escapar do acido ascendente.
    ///
    /// E uma mini cena: o Scrap para, as faixas de cinema entram e o distrito
    /// muda em volta dele; quando a luz assenta, o controle volta.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PurifyTrigger : MonoBehaviour
    {
        [SerializeField] private AreaPurifier purifier;

        private bool _running;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(AreaPurifier newPurifier)
        {
            purifier = newPurifier;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_running || purifier == null || !BoilerMaster.Defeated || AreaPurifier.IsPurified) return;

            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            StartCoroutine(Purify(controller));
        }

        private IEnumerator Purify(ScrapController controller)
        {
            _running = true;
            Cinematic.Begin();

            var body = controller.GetComponent<Rigidbody2D>();
            if (body != null) body.linearVelocity = new Vector2(0f, Mathf.Min(0f, body.linearVelocity.y));

            purifier.Purify();
            ScrapCameraFollow.Shake(0.15f, 0.6f);

            for (float t = 0f; t < purifier.Duration + 0.3f; t += Time.deltaTime)
            {
                controller.ControlLocked = true;
                yield return null;
            }

            controller.ControlLocked = false;
            Cinematic.End();
            _running = false;
        }
    }
}
