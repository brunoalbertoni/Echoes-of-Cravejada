using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A onda do Resonance Strike: dois arcos que se abrem a partir do Scrap e
    /// somem. E so o desenho — quem fere e quebra e o ScrapChisel.
    /// </summary>
    public class ResonanceRing : MonoBehaviour
    {
        private const float DuracaoVida = 0.35f;

        private SpriteRenderer[] _arcos;
        private float _idade;
        private float _alcance;

        public static void Criar(Vector3 posicao, Sprite arco, float alcanceGolpe, Material material)
        {
            var anel = new GameObject("ResonanceRing");
            anel.transform.position = posicao;

            var arcos = new SpriteRenderer[2];

            for (int i = 0; i < arcos.Length; i++)
            {
                var lado = new GameObject("Arc");
                lado.transform.SetParent(anel.transform, false);

                arcos[i] = lado.AddComponent<SpriteRenderer>();
                arcos[i].sprite = arco;
                arcos[i].flipX = i == 1;
                arcos[i].color = new Color(0.80f, 0.90f, 1f);
                arcos[i].sortingLayerName = "Actors";
                arcos[i].sortingOrder = 5;
                if (material != null) arcos[i].sharedMaterial = material;
            }

            var componente = anel.AddComponent<ResonanceRing>();
            componente._arcos = arcos;
            componente._alcance = alcanceGolpe;
        }

        private void Update()
        {
            _idade += Time.deltaTime;
            float k = Mathf.Clamp01(_idade / DuracaoVida);

            for (int i = 0; i < _arcos.Length; i++)
            {
                float lado = i == 0 ? 1f : -1f;

                _arcos[i].transform.localPosition = new Vector3(lado * _alcance * k, 0f, 0f);
                _arcos[i].transform.localScale = Vector3.one * Mathf.Lerp(1f, 3f, k);

                Color cor = _arcos[i].color;
                cor.a = 1f - k;
                _arcos[i].color = cor;
            }

            if (_idade >= DuracaoVida) Destroy(gameObject);
        }
    }
}
