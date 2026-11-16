using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A carcaca que um chefe deixa ao cair: um objeto da cena, irmao do
    /// chefe (nao filho, que o chefe pode se destruir), desligado ate a queda,
    /// com a folha em loop de fumaca e brasa (SpriteLoop). Quem cai chama
    /// Show no lugar onde caiu; quem volta a uma arena ja vencida chama Show
    /// sem lugar, e ela aparece onde o gerador a deixou.
    /// </summary>
    public static class BossWreck
    {
        public static void Show(GameObject wreck, Vector3? place = null, bool flipX = false)
        {
            if (wreck == null) return;

            if (place.HasValue) wreck.transform.position = place.Value;

            var renderer = wreck.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.flipX = flipX;

            wreck.SetActive(true);
        }
    }
}
