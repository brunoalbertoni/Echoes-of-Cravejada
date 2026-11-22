using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um portao de grade. Fechado, e parede; aberto, sobe pra dentro do teto e
    /// deixa de barrar. Quem manda abrir e a alavanca.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Gate : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("rise")] [Tooltip("Quantos tiles ele anda ao abrir.")]
        [SerializeField] private float subida = 6f;

        [UnityEngine.Serialization.FormerlySerializedAs("direction")] [Tooltip("Pra que lado ele abre. Pra cima e o normal; uma escotilha no chao abre de lado.")]
        [SerializeField] private Vector2 direcao = Vector2.up;

        [UnityEngine.Serialization.FormerlySerializedAs("openTime")] [SerializeField] private float tempoAberto = 1.2f;

        private bool _aberto;
        private string _chave;

        /// <summary>Os portoes ja abertos (cena + posicao). Vai no save.</summary>
        [SaveProgress("Opened")]
        public static readonly HashSet<string> Abertos = new HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar() => Abertos.Clear();

        private void Awake()
        {
            Vector3 p = transform.position;
            _chave = $"{SceneManager.GetActiveScene().name}/{Mathf.RoundToInt(p.x * 4)}_{Mathf.RoundToInt(p.y * 4)}";
        }

        private void Start()
        {
            // Aberto num save anterior: ja comeca no teto, calado.
            if (_aberto || !Abertos.Contains(_chave)) return;
            _aberto = true;
            transform.position += (Vector3)(direcao.normalized * subida);
            GetComponent<Collider2D>().enabled = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoSubida)
        {
            subida = novoSubida;
        }

        /// <summary>Usado pelos geradores de fase, pra portoes que nao abrem pra cima.</summary>
        public void Configurar(float novoSubida, Vector2 novoDirecao)
        {
            subida = novoSubida;
            direcao = novoDirecao;
        }

        public void Abrir()
        {
            if (_aberto) return;
            _aberto = true;
            if (_chave != null) Abertos.Add(_chave);

            // Grade de ferro correndo na pedra: quem esta perto escuta.
            NoiseSystem.Emitir(transform.position, 8f, 0.6f);

            // Os que ja abrem quando a cena carrega (chefe derrotado, porta ja destrancada) abrem calados.
            if (Time.timeSinceLevelLoad > 0.5f) Sfx.AbertoPortao.Tocar(transform.position);

            StartCoroutine(Erguer());
        }

        private IEnumerator Erguer()
        {
            Vector3 de = transform.position;
            Vector3 to = de + (Vector3)(direcao.normalized * subida);

            for (float t = 0f; t < tempoAberto; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(de, to, t / tempoAberto);
                yield return null;
            }

            transform.position = to;

            // Ja esta todo dentro do teto: nao precisa mais barrar nada.
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
