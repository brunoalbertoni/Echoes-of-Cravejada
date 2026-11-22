using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O sopro do Rei de Prata na fase 2: uma onda de Oxide que sai da boca
    /// dele, desce e corre rente ao chao pros dois lados da sala, ate as
    /// paredes. Pega quem estiver na altura dela — a nao ser que um caco de
    /// espelho esteja entre o Scrap e o Rei (o espelho devolve o sopro), ou
    /// que ele esteja no alto, numa sacada.
    /// </summary>
    public class OxideBreath : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;
        [UnityEngine.Serialization.FormerlySerializedAs("floorY")] [SerializeField] private float chaoY;
        [UnityEngine.Serialization.FormerlySerializedAs("height")] [SerializeField] private float altura = 2.6f;
        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 13f;
        [UnityEngine.Serialization.FormerlySerializedAs("puff")] [SerializeField] private Sprite baforada;
        [UnityEngine.Serialization.FormerlySerializedAs("covers")] [SerializeField] private Transform[] coberturas;

        private ScrapHealth _vida;
        private Transform _jogador;
        private readonly List<SpriteRenderer> _nuvens = new List<SpriteRenderer>();

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, float novoChaoY, Sprite novoBaforada, Transform[] novoCoberturas)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            chaoY = novoChaoY;
            baforada = novoBaforada;
            coberturas = novoCoberturas;
        }

        private void Start()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null)
            {
                _jogador = controlador.transform;
                _vida = controlador.GetComponent<ScrapHealth>();
            }

            ScrapHealth.Morreu += Dissipar;
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= Dissipar;
        }

        public void Dissipar()
        {
            StopAllCoroutines();
            foreach (SpriteRenderer nuvem in _nuvens) if (nuvem != null) Destroy(nuvem.gameObject);
            _nuvens.Clear();
        }

        public IEnumerator Soprar(float deX)
        {
            Dissipar();

            // O sopro saindo: rugido verde que corre pros dois lados.
            Sfx.SoproRei.Tocar(new Vector2(deX, chaoY + 1f));

            float esquerda = deX;
            float direita = deX;
            bool golpe = false;
            float nascimentoPonto = 0f;

            while (esquerda > minX || direita < maxX)
            {
                esquerda = Mathf.Max(minX, esquerda - velocidade * Time.deltaTime);
                direita = Mathf.Min(maxX, direita + velocidade * Time.deltaTime);

                // As nuvens que vao ficando pra tras, rentes ao chao.
                nascimentoPonto -= Time.deltaTime;
                if (nascimentoPonto <= 0f)
                {
                    nascimentoPonto = 0.05f;
                    Nuvem(esquerda);
                    Nuvem(direita);
                }

                if (!golpe && _jogador != null && _vida != null)
                {
                    float px = _jogador.position.x;
                    bool alcancou = px >= esquerda && px <= direita;
                    bool baixo = _jogador.position.y < chaoY + altura - 0.4f;
                    if (alcancou && baixo && !EstaCoberto(deX, px))
                    {
                        _vida.LevarDano(1, new Vector2(deX, chaoY + 1f));
                        golpe = true;
                    }
                }

                yield return null;
            }

            // A fumaca assenta e some.
            for (float t = 0f; t < 1f; t += Time.deltaTime)
            {
                foreach (SpriteRenderer nuvem in _nuvens) if (nuvem != null) nuvem.color = new Color(1f, 1f, 1f, 0.8f * (1f - t));
                yield return null;
            }

            Dissipar();
        }

        private bool EstaCoberto(float pontoX, float jogadorX)
        {
            if (coberturas == null) return false;

            foreach (Transform cobertura in coberturas)
            {
                if (cobertura == null || !cobertura.gameObject.activeInHierarchy) continue;

                float cx = cobertura.position.x;
                bool entre = (cx - pontoX) * (jogadorX - pontoX) > 0f && Mathf.Abs(cx - pontoX) < Mathf.Abs(jogadorX - pontoX);
                if (entre && Mathf.Abs(cx - jogadorX) < 1.8f) return true;
            }

            return false;
        }

        private void Nuvem(float x)
        {
            if (baforada == null) return;

            var objetoNuvem = new GameObject("BreathCloud");
            objetoNuvem.transform.SetParent(transform, false);
            objetoNuvem.transform.position = new Vector3(x, chaoY + Random.Range(0.2f, altura - 0.6f), 0f);
            objetoNuvem.transform.localScale = Vector3.one * Random.Range(1f, 1.6f);

            var renderizador = objetoNuvem.AddComponent<SpriteRenderer>();
            renderizador.sprite = baforada;
            renderizador.sortingLayerName = "Actors";
            renderizador.sortingOrder = 9;
            renderizador.color = new Color(1f, 1f, 1f, 0.8f);
            _nuvens.Add(renderizador);
        }
    }
}
