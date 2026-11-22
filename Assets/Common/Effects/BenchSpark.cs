using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma fagulha solta: pula, cai com a gravidade, esfria do amarelo pro
    /// laranja e some. A Bancada de Ourives solta um punhado a cada martelada.
    /// </summary>
    public class BenchSpark : MonoBehaviour
    {
        private static Sprite _ponto;

        private SpriteRenderer _renderizador;
        private Vector2 _velocidadeVetor;
        private float _duracaoVida;
        private float _idade;
        private Color _quente;
        private Color _frio;

        /// <summary>Solta <paramref name="quantidade"/> fagulhas de <paramref name="de"/>.</summary>
        public static void Estourar(Vector2 de, int quantidade, Color quente, Color frio, float velocidade = 4f, float espalhamento = 70f)
        {
            for (int i = 0; i < quantidade; i++)
            {
                float angulo = (90f + Random.Range(-espalhamento, espalhamento)) * Mathf.Deg2Rad;
                var velocidadeVetorial = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * velocidade * Random.Range(0.5f, 1.2f);
                Criar(de, velocidadeVetorial, Random.Range(0.35f, 0.7f), quente, frio);
            }
        }

        private static void Criar(Vector2 de, Vector2 velocidadeVetorial, float duracaoVida, Color quente, Color frio)
        {
            var faisca = Prefabs.Criar("BenchSpark");
            faisca.transform.position = de;

            var renderizador = Prefabs.Componente<SpriteRenderer>(faisca);
            renderizador.sprite = Ponto();
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 30;
            renderizador.color = quente;

            var roteiro = faisca.AddComponent<BenchSpark>();
            roteiro._renderizador = renderizador;
            roteiro._velocidadeVetor = velocidadeVetorial;
            roteiro._duracaoVida = duracaoVida;
            roteiro._quente = quente;
            roteiro._frio = frio;
        }

        /// <summary>Um pixel branco de 2x2 (a 32 px por tile), feito uma vez so.</summary>
        private static Sprite Ponto()
        {
            if (_ponto != null) return _ponto;

            var textura = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            textura.SetPixels32(new[] { new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255) });
            textura.Apply();
            _ponto = Sprite.Create(textura, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 32f);
            return _ponto;
        }

        private void Update()
        {
            _idade += Time.deltaTime;
            if (_idade >= _duracaoVida)
            {
                Destroy(gameObject);
                return;
            }

            _velocidadeVetor.y -= 14f * Time.deltaTime;
            transform.position += (Vector3)(_velocidadeVetor * Time.deltaTime);

            float k = _idade / _duracaoVida;
            Color cor = Color.Lerp(_quente, _frio, k);
            cor.a = 1f - k * k;
            _renderizador.color = cor;
        }
    }
}
