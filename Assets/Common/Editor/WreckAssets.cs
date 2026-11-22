using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Monta na cena a carcaca de um chefe (ver BossWreck e os *WreckArt):
    /// o sprite com a folha em loop, atras dos atores, desligado ate a queda.
    /// </summary>
    public static class WreckAssets
    {
        public const float FramesPerSecond = 8f;

        public static GameObject Build(Transform parent, string name, Sprite[] frames, Vector3 position, int order = -3)
        {
            var wreck = new GameObject(name);
            wreck.transform.SetParent(parent, false);
            wreck.transform.position = position;

            var renderer = wreck.AddComponent<SpriteRenderer>();
            renderer.sprite = frames != null && frames.Length > 0 ? frames[0] : null;
            renderer.sortingLayerName = Phase01Assets.SortingActors;
            renderer.sortingOrder = order;

            wreck.AddComponent<SpriteLoop>().Configurar(frames, FramesPerSecond, -1);

            wreck.SetActive(false);
            return wreck;
        }
    }
}
