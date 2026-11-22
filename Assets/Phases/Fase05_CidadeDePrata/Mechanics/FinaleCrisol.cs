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
        [UnityEngine.Serialization.FormerlySerializedAs("body")] [SerializeField] private SpriteRenderer corpo;
        [UnityEngine.Serialization.FormerlySerializedAs("corridorCrisol")] [SerializeField] private GameObject crisolCorredor;
        [UnityEngine.Serialization.FormerlySerializedAs("fromX")] [SerializeField] private float deX = 92f;
        [UnityEngine.Serialization.FormerlySerializedAs("toX")] [SerializeField] private float paraX = 117f;
        [UnityEngine.Serialization.FormerlySerializedAs("floorY")] [SerializeField] private float chaoY = 2f;
        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 7.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("lines")] [SerializeField, TextArea] private string[] linhas2;

        private bool _iniciado;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(SpriteRenderer novoCorpo, GameObject novoCrisolCorredor, float novoDeX, float novoParaX, float novoChaoY, string[] novoLinhas)
        {
            corpo = novoCorpo;
            crisolCorredor = novoCrisolCorredor;
            deX = novoDeX;
            paraX = novoParaX;
            chaoY = novoChaoY;
            linhas2 = novoLinhas;
        }

        private void Awake()
        {
            FinaleChoice.ReiniciarTudo();
        }

        private void Start()
        {
            // Voltou pra cena com o Rei ja caido: o Crisol ja esta la e ja falou.
            if (SilverKing.Derrotado)
            {
                _iniciado = true;
                Posicionar(paraX);
                if (crisolCorredor != null) crisolCorredor.SetActive(false);
                FinaleChoice.Pronto = true;
                return;
            }

            if (corpo != null) corpo.enabled = false;
        }

        private void Update()
        {
            if (_iniciado || !SilverKing.Derrotado) return;

            // Espera a queda do Rei terminar (a cena dele tem a sua cinematica).
            if (Cinematic.Ativo || ScriptedDialogue.Ativo) return;

            _iniciado = true;
            StartCoroutine(Chegar());
        }

        private void Posicionar(float x)
        {
            transform.position = new Vector3(x, chaoY, 0f);
            if (corpo != null)
            {
                corpo.enabled = true;
                corpo.flipX = false;
            }
        }

        private IEnumerator Chegar()
        {
            yield return new WaitForSeconds(0.8f);

            var controlador = ScrapRef.Controlador;
            if (controlador != null) controlador.ControleTravado = true;
            Cinematic.Iniciar();

            if (crisolCorredor != null) crisolCorredor.SetActive(false);
            Posicionar(deX);

            // Vem correndo, aos pulinhos, com a camera indo buscar ele.
            float x = deX;
            while (x < paraX)
            {
                x = Mathf.MoveTowards(x, paraX, velocidade * Time.deltaTime);
                float salto = Mathf.Abs(Mathf.Sin(Time.time * 14f)) * 0.18f;
                transform.position = new Vector3(x, chaoY + salto, 0f);
                ScrapCameraFollow.Direcionar(new Vector2(x + 3f, chaoY + 3f), 0.85f, 0.35f);
                yield return null;
            }

            transform.position = new Vector3(paraX, chaoY, 0f);
            Sfx.Pousar.Tocar(transform.position);
            yield return new WaitForSeconds(0.4f);

            yield return ScriptedDialogue.TocarComo("crisol", "Crisol", corpo, linhas2);

            FinaleChoice.Pronto = true;
            ScrapCameraFollow.Liberar();
            Cinematic.Encerrar();
            if (controlador != null) controlador.ControleTravado = false;
        }
    }
}
