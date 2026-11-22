using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O estouro do Acid Bloater: uma copia dele que incha e pisca verde por
    /// um instante, e so entao arrebenta e deixa a poca. E o aviso que da
    /// tempo de sair de perto.
    /// </summary>
    public class BloaterBurst : MonoBehaviour
    {
        private const float TempoInchar = 0.55f;

        private SpriteRenderer _sprite;
        private Vector3 _solo;
        private float _largura;
        private float _tempo;
        private Sprite _spritePoca;
        private Sprite[] _quadrosInchar;

        /// <param name="quadrosInchar">Os quadros de inchar (o vidro rachando). Sem eles, so a copia do sprite incha.</param>
        public static void Iniciar(SpriteRenderer fonte, Vector3 centro, Vector3 solo, float largura,
            float tempo, Sprite spritePoca, Sprite[] quadrosInchar = null)
        {
            var objetoRajada = Prefabs.Criar("BloaterBurst");
            objetoRajada.transform.position = centro;

            var sprite = Prefabs.Componente<SpriteRenderer>(objetoRajada);
            if (fonte != null)
            {
                sprite.sprite = fonte.sprite;
                sprite.flipX = fonte.flipX;
                sprite.sortingLayerID = fonte.sortingLayerID;
                sprite.sortingOrder = fonte.sortingOrder;
                sprite.sharedMaterial = fonte.sharedMaterial;
            }

            var rajada = objetoRajada.AddComponent<BloaterBurst>();
            rajada._sprite = sprite;
            rajada._solo = solo;
            rajada._largura = largura;
            rajada._tempo = tempo;
            rajada._spritePoca = spritePoca;
            rajada._quadrosInchar = quadrosInchar;
        }

        private void Awake()
        {
            // Se o Scrap morrer no meio, o bloater volta inteiro: o estouro some.
            ScrapHealth.Morreu += Cancelar;
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= Cancelar;
        }

        private void Cancelar()
        {
            Destroy(gameObject);
        }

        private IEnumerator Start()
        {
            var acido = new Color(0.7f, 1f, 0.35f);

            for (float t = 0f; t < TempoInchar; t += Time.deltaTime)
            {
                float k = t / TempoInchar;

                // Com os quadros desenhados, o vidro vai rachando; a escala so ajuda.
                bool desenhado = _quadrosInchar != null && _quadrosInchar.Length > 0;
                if (desenhado && _sprite != null)
                {
                    Sprite quadro = _quadrosInchar[Mathf.Min(_quadrosInchar.Length - 1, (int)(k * _quadrosInchar.Length))];
                    if (quadro != null) _sprite.sprite = quadro;
                }

                // Incha cada vez mais rapido, tremendo, e pisca entre branco e verde.
                float inchar = 1f + (desenhado ? 0.12f : 0.35f) * k * k + Mathf.Sin(t * 60f) * 0.03f * k;
                transform.localScale = new Vector3(inchar, inchar * (1f + 0.08f * k), 1f);

                bool piscar = Mathf.Repeat(t * (6f + 14f * k), 1f) < 0.5f;
                if (_sprite != null) _sprite.color = piscar ? acido : Color.white;

                yield return null;
            }

            Sfx.RajadaInflado.Tocar(transform.position);
            AcidPuddle.Derramar(_solo, _largura, _tempo, _spritePoca);
            Destroy(gameObject);
        }
    }
}
