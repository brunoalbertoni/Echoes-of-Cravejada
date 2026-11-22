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
        [Tooltip("Quantos tiles ele anda ao abrir.")]
        [SerializeField] private float rise = 6f;

        [Tooltip("Pra que lado ele abre. Pra cima e o normal; uma escotilha no chao abre de lado.")]
        [SerializeField] private Vector2 direction = Vector2.up;

        [SerializeField] private float openTime = 1.2f;

        private bool _open;
        private string _key;

        /// <summary>Os portoes ja abertos (cena + posicao). Vai no save.</summary>
        public static readonly HashSet<string> Opened = new HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => Opened.Clear();

        private void Awake()
        {
            Vector3 p = transform.position;
            _key = $"{SceneManager.GetActiveScene().name}/{Mathf.RoundToInt(p.x * 4)}_{Mathf.RoundToInt(p.y * 4)}";
        }

        private void Start()
        {
            // Aberto num save anterior: ja comeca no teto, calado.
            if (_open || !Opened.Contains(_key)) return;
            _open = true;
            transform.position += (Vector3)(direction.normalized * rise);
            GetComponent<Collider2D>().enabled = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newRise)
        {
            rise = newRise;
        }

        /// <summary>Usado pelos geradores de fase, pra portoes que nao abrem pra cima.</summary>
        public void Configure(float newRise, Vector2 newDirection)
        {
            rise = newRise;
            direction = newDirection;
        }

        public void Open()
        {
            if (_open) return;
            _open = true;
            if (_key != null) Opened.Add(_key);

            // Grade de ferro correndo na pedra: quem esta perto escuta.
            NoiseSystem.Emit(transform.position, 8f, 0.6f);

            // Os que ja abrem quando a cena carrega (chefe derrotado, porta ja destrancada) abrem calados.
            if (Time.timeSinceLevelLoad > 0.5f) Sfx.GateOpen.Play(transform.position);

            StartCoroutine(Lift());
        }

        private IEnumerator Lift()
        {
            Vector3 from = transform.position;
            Vector3 to = from + (Vector3)(direction.normalized * rise);

            for (float t = 0f; t < openTime; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(from, to, t / openTime);
                yield return null;
            }

            transform.position = to;

            // Ja esta todo dentro do teto: nao precisa mais barrar nada.
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
