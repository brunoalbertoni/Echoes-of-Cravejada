using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um estilhaco que despenca do teto quando o Capataz grita ou bate na parede.
    /// Antes de cair ele treme preso no teto (e o aviso); depois desce reto
    /// e, se pegar o Scrap no caminho, machuca. No chao, estoura.
    /// </summary>
    public class FallingShard : MonoBehaviour
    {
        private const float VelocidadeQueda = 15f;

        private SpriteRenderer _sprite;
        private Transform _jogador;
        private ScrapHealth _vida;
        private float _chaoY;

        /// <summary>Solta um estilhaco do ponto top, depois de tremer por warning segundos.</summary>
        public static void Soltar(Vector2 topo, float chaoY, Sprite sprite, float avisando)
        {
            var fragmento = Prefabs.Criar("FallingShard");
            fragmento.transform.position = topo;

            // A arte do estilhaco aponta pra direita: girado, aponta pro chao.
            fragmento.transform.rotation = Quaternion.Euler(0f, 0f, -90f);

            var renderizador = Prefabs.Componente<SpriteRenderer>(fragmento);
            renderizador.sprite = sprite;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 5;

            var caindo = fragmento.AddComponent<FallingShard>();
            caindo._sprite = renderizador;
            caindo._chaoY = chaoY;
            caindo.StartCoroutine(caindo.Cair(avisando));
        }

        private IEnumerator Cair(float avisando)
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null)
            {
                _jogador = controlador.transform;
                _vida = controlador.GetComponent<ScrapHealth>();
            }

            Vector3 pontoOrigem = transform.position;

            // O aviso: treme preso no teto, e solta po.
            Sfx.SoltarFragmento.Tocar(pontoOrigem);
            for (float t = 0f; t < avisando; t += Time.deltaTime)
            {
                transform.position = pontoOrigem + new Vector3(Mathf.Sin(t * 60f) * 0.06f, 0f, 0f);
                yield return null;
            }

            transform.position = pontoOrigem;

            while (transform.position.y > _chaoY)
            {
                transform.position += Vector3.down * (VelocidadeQueda * Time.deltaTime);

                if (_jogador != null && _vida != null)
                {
                    Vector2 deslocamento = _jogador.position - transform.position;

                    // O Scrap tem uns 1,5 de altura, com os pes no transform.
                    if (Mathf.Abs(deslocamento.x) < 0.55f && deslocamento.y < 0.3f && deslocamento.y > -1.6f)
                    {
                        _vida.LevarDano(1, transform.position);
                        break;
                    }
                }

                yield return null;
            }

            // O estalo no chao: baixo demais pra chamar alguem.
            NoiseSystem.Emitir(transform.position, 3f, 0.2f);
            Sfx.PousarFragmento.Tocar(transform.position);
            Destroy(gameObject);
        }
    }
}
