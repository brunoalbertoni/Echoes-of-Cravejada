using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A onda do Resonance Strike: dois arcos que se abrem a partir do Scrap e
    /// somem. E so o desenho — quem fere e quebra e o ScrapChisel.
    /// </summary>
    public class ResonanceRing : MonoBehaviour
    {
        private const float Life = 0.35f;

        private SpriteRenderer[] _arcs;
        private float _age;
        private float _reach;

        public static void Spawn(Vector3 position, Sprite arc, float reach, Material material)
        {
            var ring = new GameObject("ResonanceRing");
            ring.transform.position = position;

            var arcs = new SpriteRenderer[2];

            for (int i = 0; i < arcs.Length; i++)
            {
                var side = new GameObject("Arc");
                side.transform.SetParent(ring.transform, false);

                arcs[i] = side.AddComponent<SpriteRenderer>();
                arcs[i].sprite = arc;
                arcs[i].flipX = i == 1;
                arcs[i].color = new Color(0.80f, 0.90f, 1f);
                arcs[i].sortingLayerName = "Actors";
                arcs[i].sortingOrder = 5;
                if (material != null) arcs[i].sharedMaterial = material;
            }

            var component = ring.AddComponent<ResonanceRing>();
            component._arcs = arcs;
            component._reach = reach;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            float k = Mathf.Clamp01(_age / Life);

            for (int i = 0; i < _arcs.Length; i++)
            {
                float side = i == 0 ? 1f : -1f;

                _arcs[i].transform.localPosition = new Vector3(side * _reach * k, 0f, 0f);
                _arcs[i].transform.localScale = Vector3.one * Mathf.Lerp(1f, 3f, k);

                Color color = _arcs[i].color;
                color.a = 1f - k;
                _arcs[i].color = color;
            }

            if (_age >= Life) Destroy(gameObject);
        }
    }
}
