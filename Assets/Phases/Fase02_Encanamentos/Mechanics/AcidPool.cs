using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma poca de acido com nivel. Encostar tira vida e joga o Scrap de volta
    /// pro ultimo chao seguro; o Scrap e imune ao Oxide, nao ao acido.
    ///
    /// O nivel muda: uma valvula ou a purificacao secam a poca, e no poco do
    /// acido ascendente ela sobe. O desenho e o gatilho acompanham o nivel.
    ///
    /// O objeto fica no canto de baixo, a esquerda, do tanque.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class AcidPool : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("width")] [SerializeField] private float largura = 4f;

        [UnityEngine.Serialization.FormerlySerializedAs("height")] [Tooltip("Altura do acido quando a cena comeca.")]
        [SerializeField] private float altura = 2f;

        [UnityEngine.Serialization.FormerlySerializedAs("drainSpeed")] [Tooltip("Tiles por segundo ao secar.")]
        [SerializeField] private float velocidadeDreno = 1.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("damage")] [SerializeField] private int dano = 1;

        [UnityEngine.Serialization.FormerlySerializedAs("body")] [Header("Visual")]
        [SerializeField] private SpriteRenderer corpo;
        [UnityEngine.Serialization.FormerlySerializedAs("surface")] [SerializeField] private SpriteRenderer superficie;
        [UnityEngine.Serialization.FormerlySerializedAs("bubbles")] [SerializeField] private Transform bolhas;

        private BoxCollider2D _gatilho;
        private float _nivel;
        private float _nivelInicio;
        private Coroutine _movendo;

        /// <summary>A altura atual do acido, a partir do fundo do tanque.</summary>
        public float Nivel => _nivel;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoLargura, float novoAltura, SpriteRenderer novoCorpo,
            SpriteRenderer novoSuperficie, Transform novoBolhas)
        {
            largura = novoLargura;
            altura = novoAltura;
            corpo = novoCorpo;
            superficie = novoSuperficie;
            bolhas = novoBolhas;
        }

        private void Awake()
        {
            _gatilho = GetComponent<BoxCollider2D>();
            _gatilho.isTrigger = true;

            _nivel = altura;
            _nivelInicio = altura;
            Aplicar();

            // Quando o Scrap morre, o acido que estava subindo volta pro lugar.
            ScrapHealth.Morreu += ReiniciarNivel;
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= ReiniciarNivel;
        }

        private void ReiniciarNivel()
        {
            // Poca que ja secou continua seca: so desfaz subida.
            if (_nivel <= _nivelInicio) return;

            Parar();
            _nivel = _nivelInicio;
            Aplicar();
        }

        /// <summary>Seca a poca ate o fundo.</summary>
        public void Drenar()
        {
            if (_nivel > 0.05f && isActiveAndEnabled && Time.timeSinceLevelLoad > 0.5f) Sfx.DrenoAcido.Tocar(transform.position + new Vector3(largura * 0.5f, 0f, 0f));
            MoverPara(0f, velocidadeDreno);
            _nivelInicio = 0f;
        }

        /// <summary>Faz o acido subir ate uma altura, numa velocidade.</summary>
        public void SubirAte(float alturaAlvo, float velocidade)
        {
            if (alturaAlvo <= _nivel) return;
            MoverPara(alturaAlvo, velocidade);
        }

        private void MoverPara(float alvo, float velocidade)
        {
            if (!isActiveAndEnabled)
            {
                _nivel = alvo;
                Aplicar();
                return;
            }

            Parar();
            _movendo = StartCoroutine(Mover(alvo, velocidade));
        }

        private void Parar()
        {
            if (_movendo != null) StopCoroutine(_movendo);
            _movendo = null;
        }

        private IEnumerator Mover(float alvo, float velocidade)
        {
            while (!Mathf.Approximately(_nivel, alvo))
            {
                _nivel = Mathf.MoveTowards(_nivel, alvo, velocidade * Time.deltaTime);
                Aplicar();
                yield return null;
            }

            _movendo = null;
        }

        /// <summary>Poe o desenho e o gatilho na altura do nivel atual.</summary>
        private void Aplicar()
        {
            bool algum = _nivel > 0.05f;

            // A superficie fica uns 5 px abaixo do topo, entao o gatilho tambem:
            // so queima quando o pe afunda de verdade.
            float ferido = Mathf.Max(0f, _nivel - 0.35f);

            _gatilho.enabled = algum && ferido > 0.01f;
            _gatilho.size = new Vector2(Mathf.Max(0.1f, largura - 0.2f), Mathf.Max(0.01f, ferido));
            _gatilho.offset = new Vector2(largura * 0.5f, ferido * 0.5f);

            if (corpo != null)
            {
                corpo.enabled = algum;
                corpo.size = new Vector2(largura, Mathf.Max(0.01f, _nivel - 0.3f));
            }

            if (superficie != null)
            {
                superficie.enabled = algum;
                superficie.transform.localPosition = new Vector3(0f, Mathf.Max(0f, _nivel - 0.3f), 0f);
            }

            if (bolhas != null)
            {
                bolhas.gameObject.SetActive(algum);
                bolhas.localPosition = new Vector3(largura * 0.5f, Mathf.Max(0f, _nivel - 0.5f), 0f);
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Queimar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Queimar(outro);
        }

        private void Queimar(Collider2D outro)
        {
            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida == null) return;

            Sfx.ChiarAcido.Tocar(vida.transform.position);
            vida.LevarDano(dano, transform.position, true);
        }
    }
}
