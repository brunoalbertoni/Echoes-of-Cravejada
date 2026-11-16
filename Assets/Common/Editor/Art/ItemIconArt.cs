using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os icones do inventario do HUD (16x16, desenhados em dobro): o que o
    /// Scrap carrega pra usar ou entregar. O nome de cada um bate com
    /// <c>InventoryIcons</c> em runtime (Resources/HudIcons/item_NOME).
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class ItemIconArt
    {
        public static readonly string[] Names =
        {
            "graver", "loupe", "tweezers", "motor", "key", "mercury", "light", "salt", "ruby",
        };

        private static readonly Rgba Outline = new Rgba(20, 14, 14);
        private static readonly Rgba Glint = new Rgba(255, 252, 240);

        private static readonly Rgba[] Brass =
            { new Rgba(92, 60, 22), new Rgba(150, 104, 40), new Rgba(206, 156, 66), new Rgba(248, 214, 130) };

        private static readonly Rgba[] Steel =
            { new Rgba(52, 54, 64), new Rgba(96, 100, 112), new Rgba(150, 156, 168), new Rgba(214, 220, 230) };

        private static readonly Rgba[] Wood =
            { new Rgba(64, 34, 18), new Rgba(104, 60, 30), new Rgba(150, 94, 50), new Rgba(196, 136, 80) };

        private static readonly Rgba[] Glass =
            { new Rgba(40, 70, 80), new Rgba(80, 130, 140), new Rgba(150, 200, 206), new Rgba(220, 245, 248) };

        private static readonly Rgba[] Paper =
            { new Rgba(120, 104, 74), new Rgba(170, 152, 114), new Rgba(214, 200, 164), new Rgba(240, 232, 206) };

        public static PixelCanvas Draw(string name)
        {
            var c = new PixelCanvas(16, 16);
            switch (name)
            {
                case "graver":
                    // O buril: cabo de madeira em pera e a haste de aco em bisel.
                    ArtKit.Ball(c, 4.5f, 11.5f, 3.2f, 3.2f, Wood, 0.2f);
                    ArtKit.Taper(c, 6f, 10f, 13.5f, 2.5f, 1.2f, 0.6f, ArtKit.Plain(Steel), 0.3f);
                    c.Set(13, 2, Glint);
                    c.Set(6, 9, Brass[3]);
                    c.Set(7, 9, Brass[2]);
                    break;
                case "loupe":
                    // A lupa: aro de latao, a lente com reflexo e o cabo.
                    ArtKit.Limb(c, 10f, 10f, 14f, 14f, 1.4f, Wood, 0.2f);
                    ArtKit.Ball(c, 6.5f, 6.5f, 5.2f, 5.2f, Brass, 0.3f);
                    ArtKit.Ball(c, 6.5f, 6.5f, 3.6f, 3.6f, Glass, 0.6f);
                    c.Set(5, 4, Glint);
                    c.Set(4, 5, Glint);
                    break;
                case "tweezers":
                    // A pinca: duas laminas de aco que se abrem.
                    ArtKit.Taper(c, 4f, 2f, 7f, 14f, 0.9f, 0.5f, ArtKit.Plain(Steel), 0.3f);
                    ArtKit.Taper(c, 4.5f, 2f, 11f, 13f, 0.9f, 0.5f, ArtKit.Plain(Steel), 0.3f);
                    ArtKit.Ball(c, 4.5f, 2.5f, 1.6f, 1.6f, Brass, 0.4f);
                    c.Set(11, 12, Glint);
                    break;
                case "motor":
                    // O motor de vapor do Coletor: cilindro de cobre, pistao e a valvula.
                    ArtKit.Drum(c, 7.5f, 9f, 5.5f, 4.5f, 3f, ArtKit.Plain(ArtKit.Copper), 0.2f);
                    ArtKit.Limb(c, 12f, 7f, 14.5f, 4f, 1f, Steel, 0.3f);
                    ArtKit.Ball(c, 5f, 3.5f, 1.8f, 1.8f, Brass, 0.3f);
                    ArtKit.Limb(c, 5f, 4.5f, 5f, 6f, 0.7f, Brass, 0.2f);
                    for (int x = 3; x < 12; x += 3) c.Set(x, 9, ArtKit.Copper[0]);
                    c.Set(4, 7, Glint);
                    break;
                case "key":
                    // A chave antiga: anel com cristal roxo, haste e dentes.
                    ArtKit.Ball(c, 4.5f, 4.5f, 3.6f, 3.6f, Steel, 0.3f);
                    ArtKit.Ball(c, 4.5f, 4.5f, 1.6f, 1.6f, new[] { new Rgba(60, 30, 110), new Rgba(110, 60, 180), new Rgba(170, 120, 236), new Rgba(230, 210, 255) }, 0.5f);
                    ArtKit.Limb(c, 7f, 7f, 13.5f, 13.5f, 1f, Steel, 0.2f);
                    ArtKit.Limb(c, 11f, 13f, 9.5f, 14.5f, 0.8f, Steel, 0.2f);
                    ArtKit.Limb(c, 13f, 11f, 14.5f, 9.5f, 0.8f, Steel, 0.2f);
                    break;
                case "mercury":
                    Flask(c, new[] { new Rgba(80, 86, 100), new Rgba(130, 138, 152), new Rgba(190, 198, 212), new Rgba(245, 248, 255) }, false);
                    break;
                case "light":
                    Flask(c, new[] { new Rgba(190, 130, 30), new Rgba(240, 190, 60), new Rgba(255, 230, 130), new Rgba(255, 252, 220) }, true);
                    break;
                case "salt":
                    // O sal de quartzo: um potinho com cristais brancos saindo.
                    ArtKit.Drum(c, 8f, 11f, 5f, 3.5f, 4f, ArtKit.Plain(Glass), 0.3f);
                    for (int i = 0; i < 4; i++)
                        ArtKit.Taper(c, 5.5f + i * 1.8f, 9f, 5f + i * 2.2f, 3.5f + (i % 2) * 1.5f, 1.1f, 0.2f, ArtKit.Plain(new[] { new Rgba(150, 160, 180), new Rgba(200, 208, 224), new Rgba(236, 240, 250), new Rgba(255, 255, 255) }), 0.4f);
                    c.Fill(4, 13, 8, 1, Wood[1]);
                    break;
                case "ruby":
                    RoughGem(c, new[] { new Rgba(70, 6, 20), new Rgba(130, 14, 34), new Rgba(196, 28, 48), new Rgba(244, 84, 92) });
                    break;
            }

            ArtKit.AddOutline(c, Outline);
            return c;
        }

        /// <summary>Um frasco de alambique: bojo redondo, gargalo, rolha; aceso brilha em volta.</summary>
        private static void Flask(PixelCanvas c, Rgba[] liquid, bool glow)
        {
            if (glow) ArtKit.Glow(c, 8f, 10f, 7f, new Rgba(255, 220, 120), 0.5f, false);
            ArtKit.Ball(c, 8f, 10.5f, 4.6f, 4.2f, Glass, 0.4f);
            ArtKit.Ball(c, 8f, 11.2f, 3.6f, 3f, liquid, 0.3f);
            c.Fill(7, 3, 3, 4, Glass[2]);
            c.Fill(7, 2, 3, 2, Wood[2]);
            c.Set(6, 9, Glint);
            c.Set(6, 10, Glint);
        }

        /// <summary>Uma gema bruta: pedra irregular, sem lapidacao, com um brilho so.</summary>
        private static void RoughGem(PixelCanvas c, Rgba[] ramp)
        {
            ArtKit.Polygon(c, new[] { (3f, 9f), (6f, 3f), (11f, 2.5f), (14f, 7f), (12f, 13f), (6f, 14f) },
                (x, y) => ArtKit.Tone(ramp, (x < 8f ? 0.4f : -0.2f) + (y < 7f ? 0.3f : -0.2f)));
            c.Set(7, 5, Glint);
            c.Set(8, 5, ramp[3]);
            c.Set(10, 10, ramp[0]);
        }
    }
}
