using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A Assistente do Alquimista, de cobre, de pe (quem aparece na plateia
    /// depois da purificacao). Os corrompidos do District 4 estao no
    /// District4EnemyArt, no detalhe das outras fases.
    ///
    /// Olha pra direita, a 16 px por tile, com o pivo no meio dos pes.
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class LabFoeArt
    {
        public const int IdleFrames = 2;

        private static readonly Rgba[] Mercury = LabArt.Mercury;
        private static readonly Rgba[] Copper = ArtKit.Copper;
        private static readonly Rgba[] Silver = LabArt.Silver;
        private static readonly Rgba[] Brass = ArtKit.Brass;
        private static readonly Rgba Ink = ArtKit.Outline;
        private static readonly Rgba Green = LabArt.OxideGreen;
        private static readonly Rgba GreenDark = LabArt.OxideDark;

        private static readonly Rgba[] Coat =
            { new Rgba(126, 130, 122), new Rgba(176, 180, 168), new Rgba(214, 216, 204), new Rgba(242, 242, 232) };

        private static readonly Rgba[] Glass =
            { new Rgba(84, 110, 140), new Rgba(140, 176, 204), new Rgba(196, 224, 240), new Rgba(244, 252, 255) };

        private static readonly Rgba[] Thread =
            { new Rgba(60, 40, 30), new Rgba(110, 80, 50), new Rgba(170, 130, 80), new Rgba(220, 190, 130) };

        // ------------------------------------------------------------------
        // Corpo de gente (o arremessador e a assistente)
        // ------------------------------------------------------------------

        /// <summary>Um corpo de pe, magro, de jaleco: pes em ground, cabeca em cima.</summary>
        private static void Person(PixelCanvas c, float x, float ground, float bob, float stride, int frame, bool moving,
            Rgba[] skin, Rgba[] coat, float armAngle, bool stained)
        {
            float hipY = ground - 9f + bob;
            float shoulderY = hipY - 8f;

            // Pernas.
            double phase = frame * Math.PI / 2;
            for (int leg = 0; leg < 2; leg++)
            {
                double a = phase + leg * Math.PI;
                float footX = x + (moving ? (float)Math.Cos(a) * stride : (leg == 0 ? -1.5f : 1.5f));
                float lift = moving ? Math.Max(0f, (float)Math.Sin(a)) * 2f : 0f;
                ArtKit.Limb(c, x, hipY, footX, ground - 1f - lift, 1.3f, skin, leg == 0 ? 0f : -0.4f);
                c.Fill((int)footX - 1, (int)(ground - 1f - lift), 3, 1, skin[0]);
            }

            // O jaleco, aberto embaixo.
            ArtKit.Polygon(c, new[] { (x - 3.5f, shoulderY), (x + 3.5f, shoulderY), (x + 4.5f, hipY + 3f), (x - 4.5f, hipY + 3f) },
                (px, py) => ArtKit.Tone(coat, 0.45f - (px - x + 4f) / 9f));
            ArtKit.Line(c, (int)x, (int)shoulderY + 1, (int)x, (int)hipY + 2, coat[0]);

            if (stained)
            {
                c.Set((int)x + 2, (int)hipY - 1, Green);
                c.Set((int)x + 2, (int)hipY, GreenDark);
                c.Set((int)x - 2, (int)hipY + 2, Green);
            }

            // O braco.
            (float hx, float hy) = ArtKit.Along(x + 1f, shoulderY + 1f, armAngle, 7f);
            ArtKit.Limb(c, x + 1f, shoulderY + 1f, hx, hy, 1.2f, coat, 0.1f);
            ArtKit.Ball(c, hx, hy, 1.3f, 1.3f, ArtKit.Plain(skin), 0.2f);
        }

        // ------------------------------------------------------------------
        // A Assistente
        // ------------------------------------------------------------------

        /// <summary>A Assistente de pe, 18x30: de cobre, jaleco limpo, oculos na testa, uma prancheta.</summary>
        public static PixelCanvas Assistant(int frame)
        {
            var c = new PixelCanvas(18, 30);
            const float x = 9f;
            const float ground = 30f;
            float bob = frame % 2 == 0 ? 0f : 0.5f;

            Person(c, x, ground, bob, 0f, 0, false, Copper, Coat, 80f, false);

            // A prancheta, abraçada.
            c.Fill(9, 15, 4, 5, new Rgba(214, 200, 160));
            c.Fill(9, 14, 4, 1, ArtKit.Leather[2]);

            // A cabeca, com o cabelo de fio de cobre preso e os oculos na testa.
            float hy = ground - 22f + bob;
            ArtKit.Ball(c, x, hy, 3.4f, 3.6f, ArtKit.Plain(Copper), 0.15f);
            ArtKit.Ball(c, x - 2.5f, hy - 1f, 2.2f, 2.8f, ArtKit.Plain(new[] { new Rgba(70, 36, 26), new Rgba(110, 56, 36), new Rgba(150, 82, 52), new Rgba(190, 112, 72) }), 0f);
            c.Fill((int)x - 2, (int)hy - 3, 6, 2, ArtKit.Leather[1]);
            c.Fill((int)x, (int)hy - 3, 2, 1, new Rgba(170, 220, 240));
            c.Set((int)x + 2, (int)hy, Ink);
            c.Set((int)x + 1, (int)hy + 2, Copper[0]);

            ArtKit.AddOutline(c);
            return c;
        }
    }
}
