using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Lembra o ultimo chao seguro em que o Scrap pisou e o devolve pra la
    /// quando ele cai no acido ou nos espinhos.
    ///
    /// "Seguro" e ter ficado no chao por um instante: assim a beirada de onde
    /// ele acabou de pular vale, e um toque de raspao no meio de uma queda nao.
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapRespawn : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("settleTime")] [Tooltip("Quanto tempo no chao ate o lugar contar como seguro.")]
        [SerializeField] private float tempoAssentar = 0.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("immunityTime")] [Tooltip("Depois de voltar, por quanto tempo ele nao pode ser jogado de novo.")]
        [SerializeField] private float tempoImunidade = 0.6f;

        private ScrapController _controlador;
        private Rigidbody2D _corpo;
        private SpriteRenderer _sprite;

        private Vector2 _posicaoSeguro;
        private float _noChaoPor;
        private float _imuneAte;

        /// <summary>O ultimo chao seguro. E tambem onde a old scrap fica quando ele morre.</summary>
        public Vector2 PosicaoSeguro => _posicaoSeguro;

        private void Awake()
        {
            _controlador = GetComponent<ScrapController>();
            _corpo = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _posicaoSeguro = transform.position;
        }

        private void Update()
        {
            _noChaoPor = _controlador.EstaNoChao ? _noChaoPor + Time.deltaTime : 0f;

            if (_noChaoPor >= tempoAssentar && Time.time >= _imuneAte)
            {
                _posicaoSeguro = _corpo.position;
            }
        }

        /// <summary>Chamado por quem move o Scrap de uma vez, pra o lugar novo virar o seguro.</summary>
        public void DefinirPosicaoSegura(Vector2 posicao)
        {
            _posicaoSeguro = posicao;
            _noChaoPor = 0f;
        }

        /// <summary>
        /// Joga o Scrap de volta pro ultimo chao seguro. Com
        /// <paramref name="silencioso"/> ele nao pisca: quem chamou ja cuida disso.
        /// </summary>
        public void Renascer(bool silencioso = false)
        {
            if (Time.time < _imuneAte) return;
            _imuneAte = Time.time + tempoImunidade;

            // Um tiquinho acima, pra ele assentar em vez de nascer cravado no chao.
            Vector2 alvo = _posicaoSeguro + Vector2.up * 0.05f;

            _corpo.position = alvo;
            transform.position = alvo;
            _corpo.linearVelocity = Vector2.zero;
            _noChaoPor = 0f;

            if (!silencioso && _sprite != null)
            {
                StopAllCoroutines();
                StartCoroutine(Piscar());
            }
        }

        private IEnumerator Piscar()
        {
            float fim = Time.time + tempoImunidade;

            while (Time.time < fim)
            {
                _sprite.enabled = !_sprite.enabled;
                yield return new WaitForSeconds(0.07f);
            }

            _sprite.enabled = true;
        }
    }
}
