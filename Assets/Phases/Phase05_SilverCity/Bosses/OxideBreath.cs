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
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float floorY;
        [SerializeField] private float height = 2.6f;
        [SerializeField] private float speed = 13f;
        [SerializeField] private Sprite puff;
        [SerializeField] private Transform[] covers;

        private ScrapHealth _health;
        private Transform _player;
        private readonly List<SpriteRenderer> _clouds = new List<SpriteRenderer>();

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, float newFloorY, Sprite newPuff, Transform[] newCovers)
        {
            minX = newMinX;
            maxX = newMaxX;
            floorY = newFloorY;
            puff = newPuff;
            covers = newCovers;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null)
            {
                _player = controller.transform;
                _health = controller.GetComponent<ScrapHealth>();
            }

            ScrapHealth.Died += Clear;
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= Clear;
        }

        public void Clear()
        {
            StopAllCoroutines();
            foreach (SpriteRenderer cloud in _clouds) if (cloud != null) Destroy(cloud.gameObject);
            _clouds.Clear();
        }

        public IEnumerator Blow(float fromX)
        {
            Clear();

            float left = fromX;
            float right = fromX;
            bool hit = false;
            float spawn = 0f;

            while (left > minX || right < maxX)
            {
                left = Mathf.Max(minX, left - speed * Time.deltaTime);
                right = Mathf.Min(maxX, right + speed * Time.deltaTime);

                // As nuvens que vao ficando pra tras, rentes ao chao.
                spawn -= Time.deltaTime;
                if (spawn <= 0f)
                {
                    spawn = 0.05f;
                    Cloud(left);
                    Cloud(right);
                }

                if (!hit && _player != null && _health != null)
                {
                    float px = _player.position.x;
                    bool reached = px >= left && px <= right;
                    bool low = _player.position.y < floorY + height - 0.4f;
                    if (reached && low && !Covered(fromX, px))
                    {
                        _health.TakeDamage(1, new Vector2(fromX, floorY + 1f));
                        hit = true;
                    }
                }

                yield return null;
            }

            // A fumaca assenta e some.
            for (float t = 0f; t < 1f; t += Time.deltaTime)
            {
                foreach (SpriteRenderer cloud in _clouds) if (cloud != null) cloud.color = new Color(1f, 1f, 1f, 0.8f * (1f - t));
                yield return null;
            }

            Clear();
        }

        private bool Covered(float originX, float playerX)
        {
            if (covers == null) return false;

            foreach (Transform cover in covers)
            {
                if (cover == null || !cover.gameObject.activeInHierarchy) continue;

                float cx = cover.position.x;
                bool between = (cx - originX) * (playerX - originX) > 0f && Mathf.Abs(cx - originX) < Mathf.Abs(playerX - originX);
                if (between && Mathf.Abs(cx - playerX) < 1.8f) return true;
            }

            return false;
        }

        private void Cloud(float x)
        {
            if (puff == null) return;

            var cloudObject = new GameObject("BreathCloud");
            cloudObject.transform.SetParent(transform, false);
            cloudObject.transform.position = new Vector3(x, floorY + Random.Range(0.2f, height - 0.6f), 0f);
            cloudObject.transform.localScale = Vector3.one * Random.Range(1f, 1.6f);

            var renderer = cloudObject.AddComponent<SpriteRenderer>();
            renderer.sprite = puff;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 9;
            renderer.color = new Color(1f, 1f, 1f, 0.8f);
            _clouds.Add(renderer);
        }
    }
}
