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
        private const float Step = 0.25f;
        private const float Reach = 10f;
        private static readonly Collider2D[] Hits = new Collider2D[16];

        public static Vector3 Free(Vector3 position, float radius = 0.6f)
        {
            if (IsFree(position, radius)) return position;

            for (float d = Step; d <= Reach; d += Step)
            {
                Vector3 left = position + Vector3.left * d;
                if (IsFree(left, radius)) return left;
                Vector3 right = position + Vector3.right * d;
                if (IsFree(right, radius)) return right;
                Vector3 up = position + Vector3.up * d;
                if (IsFree(up, radius)) return up;
            }

            return position;
        }

        private static bool IsFree(Vector3 position, float radius)
        {
            int count = Physics2D.OverlapCircleNonAlloc(position, radius, Hits);
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = Hits[i];
                if (hit == null || hit.isTrigger) continue;
                // Quem se mexe (o Scrap, inimigos, o chefe) nao e parede.
                Rigidbody2D body = hit.attachedRigidbody;
                if (body != null && body.bodyType == RigidbodyType2D.Dynamic) continue;
                if (hit.GetComponentInParent<ScrapController>() != null) continue;
                return false;
            }

            return true;
        }
    }
}
