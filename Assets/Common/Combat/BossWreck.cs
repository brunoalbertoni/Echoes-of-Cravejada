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
        public static void Mostrar(GameObject destrocos, Vector3? lugar = null, bool inverterX = false)
        {
            if (destrocos == null) return;

            if (lugar.HasValue) destrocos.transform.position = lugar.Value;

            var renderizador = destrocos.GetComponent<SpriteRenderer>();
            if (renderizador != null) renderizador.flipX = inverterX;

            destrocos.SetActive(true);
        }
    }
}
