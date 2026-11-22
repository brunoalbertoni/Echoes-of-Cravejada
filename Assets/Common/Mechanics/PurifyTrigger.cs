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
        [UnityEngine.Serialization.FormerlySerializedAs("purifier")] [SerializeField] private AreaPurifier purificador;

        private bool _correndo;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(AreaPurifier novoPurificador)
        {
            purificador = novoPurificador;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (_correndo || purificador == null || !BoilerMaster.Derrotado || AreaPurifier.EstaPurificado) return;

            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador == null) return;

            StartCoroutine(Purificar(controlador));
        }

        private IEnumerator Purificar(ScrapController controlador)
        {
            _correndo = true;
            Cinematic.Iniciar();

            var corpo = controlador.GetComponent<Rigidbody2D>();
            if (corpo != null) corpo.linearVelocity = new Vector2(0f, Mathf.Min(0f, corpo.linearVelocity.y));

            purificador.Purificar();
            ScrapCameraFollow.Tremer(0.15f, 0.6f);

            for (float t = 0f; t < purificador.Duracao + 0.3f; t += Time.deltaTime)
            {
                controlador.ControleTravado = true;
                yield return null;
            }

            controlador.ControleTravado = false;
            Cinematic.Encerrar();
            _correndo = false;
        }
    }
}
