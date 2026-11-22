using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Onde um item solto pode ficar: se o ponto pedido cai dentro de parede
    /// ou chao, procura o vao livre mais perto (pros lados primeiro, depois
    /// pra cima), pra o item nunca nascer enterrado e impossivel de pegar.
    /// </summary>
    public static class DropSpot
    {
        private const float Passo2 = 0.25f;
        private const float Alcance = 10f;
        private static readonly Collider2D[] Golpes = new Collider2D[16];

        public static Vector3 PontoLivre(Vector3 posicao, float raioArea = 0.6f)
        {
            if (EstaLivre(posicao, raioArea)) return posicao;

            for (float d = Passo2; d <= Alcance; d += Passo2)
            {
                Vector3 esquerda = posicao + Vector3.left * d;
                if (EstaLivre(esquerda, raioArea)) return esquerda;
                Vector3 direita = posicao + Vector3.right * d;
                if (EstaLivre(direita, raioArea)) return direita;
                Vector3 up = posicao + Vector3.up * d;
                if (EstaLivre(up, raioArea)) return up;
            }

            return posicao;
        }

        private static bool EstaLivre(Vector3 posicao, float raioArea)
        {
            int quantidade = Physics2D.OverlapCircleNonAlloc(posicao, raioArea, Golpes);
            for (int i = 0; i < quantidade; i++)
            {
                Collider2D golpe = Golpes[i];
                if (golpe == null || golpe.isTrigger) continue;
                // Quem se mexe (o Scrap, inimigos, o chefe) nao e parede.
                Rigidbody2D corpo = golpe.attachedRigidbody;
                if (corpo != null && corpo.bodyType == RigidbodyType2D.Dynamic) continue;
                if (golpe.GetComponentInParent<ScrapController>() != null) continue;
                return false;
            }

            return true;
        }
    }
}
