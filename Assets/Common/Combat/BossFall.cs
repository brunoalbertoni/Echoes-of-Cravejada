using System;
using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A explosao em Oxide com que todo chefe cai: ele trava, treme e pisca
    /// entre o branco e o verde, solta jatos de Oxide cada vez mais perto um
    /// do outro (a tela tremendo junto) e estoura numa nuvem grande. No fim,
    /// ele ja nao aparece nem encosta em ninguem.
    ///
    /// Quem chama roda isto dentro da propria cena de queda (Cinematic ja
    /// comecada) e decide o que vem depois: purificacao, recompensa, saida.
    /// </summary>
    public static class BossFall
    {
        private static readonly Color OxideGreen = new Color(0.45f, 1f, 0.65f);

        /// <param name="boss">O chefe. Treme em volta de home.</param>
        /// <param name="sprite">O desenho dele, que pisca e some no estouro.</param>
        /// <param name="cloud">A nuvem de Oxide dele (um filho). Pode faltar.</param>
        /// <param name="home">Onde ele esta, no chao.</param>
        /// <param name="hold">Chamado todo quadro: e onde quem chama mantem o Scrap travado.</param>
        /// <param name="progress">Chamado todo quadro com quanto da tremedeira ja passou (0 a 1): e onde quem tem animacao de queda troca o quadro.</param>
        public static IEnumerator Explode(Transform boss, SpriteRenderer sprite, ParticleSystem cloud,
            Vector3 home, Action hold, float buildUp = 2.2f, Action<float> progress = null)
        {
            float nextPuff = 0f;
            int puff = 0;

            for (float t = 0f; t < buildUp; t += Time.deltaTime)
            {
                hold?.Invoke();

                float k = t / buildUp;
                progress?.Invoke(k);
                float jitter = Mathf.Lerp(0.04f, 0.16f, k);
                boss.position = home + new Vector3(UnityEngine.Random.Range(-jitter, jitter),
                    UnityEngine.Random.Range(0f, jitter * 0.5f), 0f);

                if (sprite != null)
                {
                    bool lit = Mathf.FloorToInt(t * Mathf.Lerp(8f, 20f, k)) % 2 == 0;
                    sprite.color = lit ? OxideGreen : Color.white;
                }

                if (t >= nextPuff)
                {
                    if (cloud != null) cloud.Emit(8 + puff * 3);
                    ScrapCameraFollow.Shake(Mathf.Lerp(0.05f, 0.18f, k), 0.3f);
                    nextPuff = t + Mathf.Lerp(0.45f, 0.12f, k);
                    puff++;
                }

                yield return null;
            }

            // O estouro.
            boss.position = home;
            if (cloud != null) cloud.Emit(110);
            ScrapCameraFollow.Shake(0.45f, 0.9f);
            NoiseSystem.Emit(home, 14f, 1f);

            if (sprite != null) sprite.enabled = false;
            foreach (Collider2D part in boss.GetComponents<Collider2D>()) part.enabled = false;
        }

        /// <summary>Espera, mantendo o Scrap travado.</summary>
        public static IEnumerator Hold(float seconds, Action hold)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                hold?.Invoke();
                yield return null;
            }
        }
    }
}
