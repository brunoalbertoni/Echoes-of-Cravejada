using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A campanula de vidro que o Alquimista derruba do teto: ele quer uma
    /// amostra do prototipo.
    ///
    /// Primeiro a sombra aparece no chao, embaixo de onde o Scrap estava, e o
    /// vidro range no teto. Depois ela despenca. Se pegar o Scrap, ele leva um
    /// golpe e fica preso: as paredes de vidro nao deixam sair, e so o cinzel
    /// (tres golpes) quebra a redoma. Enquanto esta preso, o ar acaba e ele
    /// apanha de novo de tempos em tempos. Se nao pegar ninguem, ela fica no
    /// chao um instante e estilhaca sozinha.
    ///
    /// Quem a usa chama Drop; ela volta pro teto, escondida, quando acaba.
    /// </summary>
    public class BellJar : MonoBehaviour, IDamageable
    {
        [UnityEngine.Serialization.FormerlySerializedAs("ceilingY")] [SerializeField] private float tetoY = 79f;
        [UnityEngine.Serialization.FormerlySerializedAs("floorY")] [SerializeField] private float chaoY = 62f;
        [UnityEngine.Serialization.FormerlySerializedAs("warnTime")] [SerializeField] private float tempoAlerta = 0.9f;
        [UnityEngine.Serialization.FormerlySerializedAs("suffocateEvery")] [SerializeField] private float cadaSufocar = 2.6f;
        [UnityEngine.Serialization.FormerlySerializedAs("hitsToBreak")] [SerializeField] private int golpesParaQuebra = 3;

        [UnityEngine.Serialization.FormerlySerializedAs("glass")] [Header("Visual")]
        [SerializeField] private SpriteRenderer vidro;
        [UnityEngine.Serialization.FormerlySerializedAs("crackStages")] [SerializeField] private Sprite[] estagiosRachadura;
        [UnityEngine.Serialization.FormerlySerializedAs("shadow")] [SerializeField] private SpriteRenderer sombraChao;

        [UnityEngine.Serialization.FormerlySerializedAs("walls")] [Header("Paredes de vidro")]
        [SerializeField] private Collider2D[] paredes;

        private ScrapHealth _preso;
        private int _golpes;
        private bool _ocupado;

        public bool Ocupado => _ocupado;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoTetoY, float novoChaoY, SpriteRenderer novoVidro, Sprite[] novoRachaduras,
            SpriteRenderer novoSombra, Collider2D[] novoParedes)
        {
            tetoY = novoTetoY;
            chaoY = novoChaoY;
            vidro = novoVidro;
            estagiosRachadura = novoRachaduras;
            sombraChao = novoSombra;
            paredes = novoParedes;
        }

        private void Awake()
        {
            Esconder();
            ScrapHealth.Morreu += Esconder;
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= Esconder;
        }

        /// <summary>Some com tudo: sem vidro, sem sombra, sem paredes, sem ninguem preso.</summary>
        public void Esconder()
        {
            StopAllCoroutines();
            _ocupado = false;
            _preso = null;
            _golpes = 0;

            if (vidro != null) vidro.enabled = false;
            if (sombraChao != null) sombraChao.enabled = false;
            DefinirParedes(false);

            var area = GetComponent<Collider2D>();
            if (area != null) area.enabled = false;
        }

        public void Soltar(float x)
        {
            if (_ocupado) return;
            StartCoroutine(Cair(x));
        }

        /// <summary>O chao embaixo de x: o piso da arena ou o alto de um estrado.</summary>
        private float ChaoAbaixo(float x)
        {
            int mascara = LayerMask.GetMask("Ground");
            RaycastHit2D[] golpes = Physics2D.RaycastAll(new Vector2(x, tetoY - 3.2f), Vector2.down, tetoY - chaoY + 2f, mascara);
            float melhor = chaoY;
            foreach (RaycastHit2D golpe in golpes)
            {
                if (golpe.collider == null || golpe.collider.isTrigger || golpe.collider.transform.IsChildOf(transform)) continue;
                melhor = Mathf.Max(chaoY, golpe.point.y);
                break;
            }

            return melhor;
        }

        private IEnumerator Cair(float x)
        {
            _ocupado = true;
            _golpes = 0;
            float pousar = ChaoAbaixo(x);

            transform.position = new Vector3(x, tetoY - 2.75f, 0f);
            if (vidro != null)
            {
                vidro.enabled = true;
                vidro.sprite = Estagio(0);
                vidro.color = new Color(1f, 1f, 1f, 0.9f);
            }

            if (sombraChao != null)
            {
                sombraChao.enabled = true;
                sombraChao.transform.position = new Vector3(x, pousar, 0f);
            }

            // O aviso: a sombra cresce no chao e o vidro treme no teto.
            Vector3 topo = transform.position;
            Sfx.RangidoRedoma.Tocar(topo, tom2: 0.9f / Mathf.Max(0.3f, tempoAlerta));
            for (float t = 0f; t < tempoAlerta; t += Time.deltaTime)
            {
                float k = t / tempoAlerta;
                if (sombraChao != null) sombraChao.transform.localScale = new Vector3(0.4f + k * 0.6f, 1f, 1f);
                transform.position = topo + Vector3.right * (Mathf.Sin(t * 60f) * 0.05f);
                yield return null;
            }

            transform.position = topo;

            // A queda.
            float velocidade = 0f;
            while (transform.position.y > pousar)
            {
                velocidade += 60f * Time.deltaTime;
                transform.position += Vector3.down * (velocidade * Time.deltaTime);
                yield return null;
            }

            transform.position = new Vector3(x, pousar, 0f);
            if (sombraChao != null) sombraChao.enabled = false;
            NoiseSystem.Emitir(transform.position, 10f, 0.7f);
            Sfx.QuedaRedoma.Tocar(transform.position);

            var area = GetComponent<Collider2D>();
            if (area != null) area.enabled = true;

            // Pegou o Scrap?
            var controlador = ScrapRef.Controlador;
            if (controlador != null)
            {
                Vector2 deslocamento = controlador.transform.position - transform.position;
                if (Mathf.Abs(deslocamento.x) < 1.15f && deslocamento.y > -0.4f && deslocamento.y < 2.2f)
                {
                    _preso = controlador.GetComponent<ScrapHealth>();
                    var corpo = controlador.GetComponent<Rigidbody2D>();
                    if (corpo != null)
                    {
                        corpo.position = new Vector2(x, pousar + 0.05f);
                        corpo.linearVelocity = Vector2.zero;
                    }

                    if (_preso != null) _preso.LevarDano(1, transform.position + Vector3.up);

                    // O golpe matou: a redoma ja sumiu junto com a luta.
                    if (!_ocupado) yield break;
                }
            }

            if (_preso != null)
            {
                DefinirParedes(true);

                // Preso: o ar vai acabando.
                float proximo = Time.time + cadaSufocar;
                while (_preso != null && _golpes < golpesParaQuebra)
                {
                    if (Time.time >= proximo)
                    {
                        // O ar acabando dentro do vidro.
                        Sfx.SufocarRedoma.Tocar(transform.position);
                        _preso.LevarDano(1, transform.position + Vector3.up);
                        proximo = Time.time + cadaSufocar;
                    }

                    yield return null;
                }

                if (!_ocupado) yield break;
            }
            else
            {
                // Ninguem dentro: fica um instante e racha sozinha.
                yield return new WaitForSeconds(1.1f);
            }

            yield return Estilhacar();
        }

        private IEnumerator Estilhacar()
        {
            DefinirParedes(false);
            NoiseSystem.Emitir(transform.position, 9f, 0.6f);
            Sfx.EstilhacoRedoma.Tocar(transform.position);

            if (vidro != null)
            {
                vidro.sprite = Estagio(estagiosRachadura != null ? estagiosRachadura.Length - 1 : 0);
                for (float t = 0f; t < 0.3f; t += Time.deltaTime)
                {
                    vidro.color = new Color(1f, 1f, 1f, 0.9f * (1f - t / 0.3f));
                    transform.localScale = Vector3.one * (1f + t * 0.4f);
                    yield return null;
                }
            }

            transform.localScale = Vector3.one;
            Esconder();
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (!_ocupado || _preso == null) return;

            _golpes += Mathf.Max(1, quantia);
            if (vidro != null) vidro.sprite = Estagio(Mathf.Min(_golpes, golpesParaQuebra - 1));
            NoiseSystem.Emitir(transform.position, 6f, 0.4f);

            // Cada rachadura mais aguda: da pra ouvir quanto falta.
            if (_golpes < golpesParaQuebra) Sfx.GolpeRedoma.Tocar(transform.position, tom2: 1f + 0.15f * (_golpes - 1));

            if (_golpes >= golpesParaQuebra) _preso = null;
        }

        private Sprite Estagio(int indice)
        {
            if (estagiosRachadura == null || estagiosRachadura.Length == 0) return vidro != null ? vidro.sprite : null;
            return estagiosRachadura[Mathf.Clamp(indice, 0, estagiosRachadura.Length - 1)];
        }

        private void DefinirParedes(bool on)
        {
            if (paredes == null) return;
            foreach (Collider2D parede in paredes)
            {
                if (parede != null) parede.enabled = on;
            }
        }
    }
}
