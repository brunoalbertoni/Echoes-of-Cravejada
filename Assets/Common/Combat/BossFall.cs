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
        private static readonly Color VerdeOxido = new Color(0.45f, 1f, 0.65f);

        /// <param name="chefe">O chefe. Treme em volta de home.</param>
        /// <param name="sprite">O desenho dele, que pisca e some no estouro.</param>
        /// <param name="nuvem">A nuvem de Oxide dele (um filho). Pode faltar.</param>
        /// <param name="origem2">Onde ele esta, no chao.</param>
        /// <param name="segurar">Chamado todo quadro: e onde quem chama mantem o Scrap travado.</param>
        /// <param name="progresso">Chamado todo quadro com quanto da tremedeira ja passou (0 a 1): e onde quem tem animacao de queda troca o quadro.</param>
        public static IEnumerator Explodir(Transform chefe, SpriteRenderer sprite, ParticleSystem nuvem,
            Vector3 origem2, Action segurar, float cimaPreparo = 2.2f, Action<float> progresso = null)
        {
            float proximoBaforada = 0f;
            int baforada = 0;

            for (float t = 0f; t < cimaPreparo; t += Time.deltaTime)
            {
                segurar?.Invoke();

                float k = t / cimaPreparo;
                progresso?.Invoke(k);
                float variacao = Mathf.Lerp(0.04f, 0.16f, k);
                chefe.position = origem2 + new Vector3(UnityEngine.Random.Range(-variacao, variacao),
                    UnityEngine.Random.Range(0f, variacao * 0.5f), 0f);

                if (sprite != null)
                {
                    bool aceso = Mathf.FloorToInt(t * Mathf.Lerp(8f, 20f, k)) % 2 == 0;
                    sprite.color = aceso ? VerdeOxido : Color.white;
                }

                if (t >= proximoBaforada)
                {
                    if (nuvem != null) nuvem.Emit(8 + baforada * 3);
                    Sfx.JatoQuedaChefe.Tocar(origem2, volume: Mathf.Lerp(0.6f, 1f, k), tom2: Mathf.Lerp(0.85f, 1.25f, k));
                    ScrapCameraFollow.Tremer(Mathf.Lerp(0.05f, 0.18f, k), 0.3f);
                    proximoBaforada = t + Mathf.Lerp(0.45f, 0.12f, k);
                    baforada++;
                }

                yield return null;
            }

            // O estouro.
            chefe.position = origem2;
            if (nuvem != null) nuvem.Emit(110);
            Sfx.RajadaQuedaChefe.Tocar(origem2);
            ScrapCameraFollow.Tremer(0.45f, 0.9f);
            NoiseSystem.Emitir(origem2, 14f, 1f);

            if (sprite != null) sprite.enabled = false;
            foreach (Collider2D parte in chefe.GetComponents<Collider2D>()) parte.enabled = false;
        }

        /// <summary>Espera, mantendo o Scrap travado.</summary>
        public static IEnumerator Segurar(float segundos, Action segurar)
        {
            for (float t = 0f; t < segundos; t += Time.deltaTime)
            {
                segurar?.Invoke();
                yield return null;
            }
        }
    }
}
