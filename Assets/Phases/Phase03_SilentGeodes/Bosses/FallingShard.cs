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
        private const float FallSpeed = 15f;

        private SpriteRenderer _sprite;
        private Transform _player;
        private ScrapHealth _health;
        private float _floorY;

        /// <summary>Solta um estilhaco do ponto top, depois de tremer por warning segundos.</summary>
        public static void Drop(Vector2 top, float floorY, Sprite sprite, float warning)
        {
            var shard = new GameObject("FallingShard");
            shard.transform.position = top;

            // A arte do estilhaco aponta pra direita: girado, aponta pro chao.
            shard.transform.rotation = Quaternion.Euler(0f, 0f, -90f);

            var renderer = shard.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 5;

            var falling = shard.AddComponent<FallingShard>();
            falling._sprite = renderer;
            falling._floorY = floorY;
            falling.StartCoroutine(falling.Fall(warning));
        }

        private IEnumerator Fall(float warning)
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null)
            {
                _player = controller.transform;
                _health = controller.GetComponent<ScrapHealth>();
            }

            Vector3 origin = transform.position;

            // O aviso: treme preso no teto, e solta po.
            for (float t = 0f; t < warning; t += Time.deltaTime)
            {
                transform.position = origin + new Vector3(Mathf.Sin(t * 60f) * 0.06f, 0f, 0f);
                yield return null;
            }

            transform.position = origin;

            while (transform.position.y > _floorY)
            {
                transform.position += Vector3.down * (FallSpeed * Time.deltaTime);

                if (_player != null && _health != null)
                {
                    Vector2 offset = _player.position - transform.position;

                    // O Scrap tem uns 1,5 de altura, com os pes no transform.
                    if (Mathf.Abs(offset.x) < 0.55f && offset.y < 0.3f && offset.y > -1.6f)
                    {
                        _health.TakeDamage(1, transform.position);
                        break;
                    }
                }

                yield return null;
            }

            // O estalo no chao: baixo demais pra chamar alguem.
            NoiseSystem.Emit(transform.position, 3f, 0.2f);
            Destroy(gameObject);
        }
    }
}
