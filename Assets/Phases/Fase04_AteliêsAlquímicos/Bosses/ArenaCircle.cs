using System;
using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um circulo de transmutacao no estrado do Grande Laboratorio. A runa do
    /// centro acende com golpes de cinzel: cada golpe a enche um pouco de luz
    /// (dois bastam), sem aviso nenhum na tela. Se o Alquimista estiver perto
    /// quando ela acender, ele vira prata.
    ///
    /// Depois de usado, o Alquimista o afoga em mercurio (Smother): a runa
    /// apaga e fica inutil por um tempo, e o Scrap tem que ir a outro.
    /// </summary>
    public class ArenaCircle : MonoBehaviour, IDamageable
    {
        [UnityEngine.Serialization.FormerlySerializedAs("hitsToLight")] [Tooltip("Quantos golpes ate a runa acender.")]
        [SerializeField] private int golpesParaLuz = 2;

        [UnityEngine.Serialization.FormerlySerializedAs("hitMemory")] [Tooltip("Quanto tempo um golpe fica valendo; parado demais, a luz da runa se apaga.")]
        [SerializeField] private float memoriaGolpe = 2.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("rune")] [SerializeField] private SpriteRenderer runa;
        [UnityEngine.Serialization.FormerlySerializedAs("idleColor")] [SerializeField] private Color corParado = new Color(0.62f, 0.68f, 0.82f);
        [UnityEngine.Serialization.FormerlySerializedAs("litColor")] [SerializeField] private Color corAceso = new Color(0.80f, 1f, 1f);
        [UnityEngine.Serialization.FormerlySerializedAs("smotheredColor")] [SerializeField] private Color corAbafado = new Color(0.30f, 0.32f, 0.40f);

        private int _golpes;
        private float _ultimoGolpe;
        private float _abafadoAte;
        private float _claraoAte;
        private float _solavanco;

        /// <summary>Avisa quando a runa acende.</summary>
        public event Action<ArenaCircle> Polido;

        /// <summary>So funciona com a luta acontecendo.</summary>
        public bool Armado { get; set; }

        public bool Usavel => Armado && Time.time >= _abafadoAte;

        /// <summary>Usado pelos geradores de fase. O segundo valor sobrou de quando se segurava o botao.</summary>
        public void Configurar(SpriteRenderer novoRuna, float sem)
        {
            runa = novoRuna;
        }

        /// <summary>Afogado em mercurio: inutil por tantos segundos.</summary>
        public void Abafar(float segundos)
        {
            _abafadoAte = Time.time + segundos;
            _golpes = 0;
        }

        /// <summary>Volta como no comeco da luta.</summary>
        public void ReiniciarCirculo()
        {
            _abafadoAte = 0f;
            _claraoAte = 0f;
            _golpes = 0;
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (!Usavel) return;

            if (Time.time - _ultimoGolpe > memoriaGolpe) _golpes = 0;
            _ultimoGolpe = Time.time;
            _golpes++;
            _solavanco = 1f;

            if (_golpes < golpesParaLuz) return;

            _golpes = 0;
            _claraoAte = Time.time + 0.5f;
            NoiseSystem.Emitir(transform.position, 10f, 0.5f);
            Sfx.PolidoRuna.Tocar(transform.position);
            StartCoroutine(Estourar());
            Polido?.Invoke(this);
        }

        // ------------------------------------------------------------------
        // O estouro
        // ------------------------------------------------------------------

        private static Sprite _pixel;

        private static Sprite PixelBranco()
        {
            if (_pixel != null) return _pixel;
            var textura = new Texture2D(1, 1) { filterMode = FilterMode.Point };
            textura.SetPixel(0, 0, Color.white);
            textura.Apply();
            _pixel = Sprite.Create(textura, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
            return _pixel;
        }

        private SpriteRenderer Peca(string nome, Sprite sprite, int ordem)
        {
            var peca = new GameObject(nome);
            peca.transform.position = transform.position;
            var renderizador = peca.AddComponent<SpriteRenderer>();
            renderizador.sprite = sprite;
            if (runa != null)
            {
                renderizador.sortingLayerID = runa.sortingLayerID;
                renderizador.sharedMaterial = runa.sharedMaterial;
            }
            renderizador.sortingOrder = ordem;
            return renderizador;
        }

        /// <summary>A runa estoura: um anel de luz abre e some.</summary>
        private IEnumerator Estourar()
        {
            if (runa == null || runa.sprite == null) yield break;

            SpriteRenderer anel = Peca("RuneBurst", runa.sprite, runa.sortingOrder + 1);
            Vector3 escalaBase = transform.lossyScale;
            for (float t = 0f; t < 0.45f; t += Time.deltaTime)
            {
                float k = t / 0.45f;
                anel.transform.localScale = escalaBase * Mathf.Lerp(1f, 2.6f, 1f - (1f - k) * (1f - k));
                anel.color = new Color(corAceso.r, corAceso.g, corAceso.b, 1f - k);
                yield return null;
            }

            Destroy(anel.gameObject);
        }

        /// <summary>
        /// O raio da runa ate o Alquimista: corre ate ele, pega, engrossa,
        /// pisca e some, com um clarao no ponto onde acerta.
        /// </summary>
        public void Golpear(Vector3 alvo)
        {
            StartCoroutine(RotinaDoGolpe(alvo));
        }

        private IEnumerator RotinaDoGolpe(Vector3 alvo)
        {
            Vector3 de = transform.position + Vector3.up * 0.3f;
            Vector3 delta = alvo - de;
            float comprimento = delta.magnitude;
            float angulo = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            SpriteRenderer projetil = Peca("RuneBolt", PixelBranco(), 30);
            projetil.transform.position = de;
            projetil.transform.rotation = Quaternion.Euler(0f, 0f, angulo);
            projetil.color = corAceso;

            for (float t = 0f; t < 0.12f; t += Time.deltaTime)
            {
                projetil.transform.localScale = new Vector3(comprimento * (t / 0.12f), 0.18f, 1f);
                yield return null;
            }

            SpriteRenderer clarao = runa != null && runa.sprite != null ? Peca("RuneHit", runa.sprite, 31) : null;
            if (clarao != null) clarao.transform.position = alvo;

            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                float k = t / 0.35f;
                float tremeluzir = Mathf.Repeat(t * 30f, 1f) < 0.5f ? 1f : 0.6f;
                projetil.transform.localScale = new Vector3(comprimento, Mathf.Lerp(0.5f, 0.05f, k) * tremeluzir, 1f);
                Color c = Color.Lerp(Color.white, corAceso, k);
                c.a = 1f - k * 0.6f;
                projetil.color = c;
                if (clarao != null)
                {
                    clarao.transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.8f, k);
                    clarao.color = new Color(1f, 1f, 1f, 1f - k);
                }
                yield return null;
            }

            Destroy(projetil.gameObject);
            if (clarao != null) Destroy(clarao.gameObject);
        }

        private void Update()
        {
            if (_golpes > 0 && Time.time - _ultimoGolpe > memoriaGolpe) _golpes = 0;
            _solavanco = Mathf.MoveTowards(_solavanco, 0f, Time.deltaTime * 4f);

            if (runa == null) return;

            // A runa enche de luz a cada golpe, e pulsa no golpe.
            Color alvo = Time.time < _claraoAte ? corAceso : Usavel ? corParado : corAbafado;
            if (Usavel && _golpes > 0) alvo = Color.Lerp(corParado, corAceso, _golpes / (float)golpesParaLuz);
            alvo = Color.Lerp(alvo, Color.white, _solavanco * 0.5f);
            runa.color = Color.Lerp(runa.color, alvo, 12f * Time.deltaTime);
        }
    }
}
