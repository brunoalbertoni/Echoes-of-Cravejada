using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A carcaca do Geode Drill, parada no chao da arena: a cabine arriada em
    /// cima da esteira da esquerda, a esteira da direita arrebentada e
    /// torta, o tanque de Oxide caido pra tras e apagado, e a broca espiral
    /// arrancada, enfiada de ponta no chao. Cristais quebrados em volta,
    /// brasa nas frestas e fumaca saindo do motor.
    ///
    /// Nasce do proprio desenho da broca (GeodeArt.Drill), que o usuario
    /// aprovou, cortado e remontado pelo WreckKit, no mesmo 16 px por tile.
    /// Mais larga que a broca, com o mesmo pivo (meio, no pe).
    /// </summary>
    public static class DrillWreckArt
    {
        public const int Width = 128;
        public const int Height = 64;
        public const int FrameCount = 8;

        private const int Shift = (Width - 80) / 2;
        private const int Ground = Height - 1;

        private static readonly Rgba[] Crystal =
            { new Rgba(60, 46, 110), new Rgba(110, 86, 180), new Rgba(170, 150, 236), new Rgba(234, 226, 255) };

        public static PixelCanvas[] Frames()
        {
            PixelCanvas source = GeodeArt.Drill(0);

            bool Bit(int x, int y) => x >= 57 && y < 38;
            bool Tank(int x, int y) => x <= 23 && y < 31;
            bool Tread(int x, int y) => y >= 37;

            PixelCanvas bit = WreckKit.Cut(source, Bit);
            PixelCanvas tank = WreckKit.Cut(source, Tank);
            PixelCanvas body = WreckKit.Cut(source, (x, y) => !Bit(x, y) && !Tank(x, y) && !Tread(x, y));
            PixelCanvas treadLeft = WreckKit.Cut(source, (x, y) => Tread(x, y) && x < 38);
            PixelCanvas treadRight = WreckKit.Cut(source, (x, y) => Tread(x, y) && x >= 38);

            var c = new PixelCanvas(Width, Height);
            Rgba[][] ramps = { ArtKit.Brass, ArtKit.Iron, ArtKit.Verdigris, ArtKit.Iron };

            WreckKit.Rubble(c, Shift + 4, Shift + 70, Ground, 5, ramps, 707);

            WreckKit.Place(c, tank, -24f, 12f, 30f, Shift + 9f, Ground - 1f);
            WreckKit.Place(c, treadRight, 14f, 58f, 47f, Shift + 66f, Ground + 1f);
            WreckKit.Place(c, treadLeft, 0f, 19f, 47f, Shift + 19f, Ground);
            WreckKit.Place(c, body, -7f, 40f, 37f, Shift + 40f, Ground - 8f);
            WreckKit.Place(c, bit, 40f, 68f, 26f, Shift + 84f, Ground - 9f);

            WreckKit.Scorch(c, 37, 0.3f, 0.15f);

            var embers = new List<(int X, int Y)>();
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 30f, Ground - 26f, 25f, 8, 41));
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 46f, Ground - 22f, 100f, 6, 42));
            WreckKit.Hole(c, Shift + 38f, Ground - 30f, 2.2f, embers);
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 8f, Ground - 14f, 60f, 5, 43));

            // Cristais partidos e lascas em volta.
            Shard(c, Shift - 4f, Ground, 5f, -20f);
            Shard(c, Shift + 74f, Ground, 4f, 15f);
            Shard(c, Shift + 100f, Ground, 6f, 25f);
            Shard(c, Shift + 56f, Ground, 3f, -10f);
            WreckKit.Gear(c, Shift + 56f, Ground - 3f, 2.5f, ArtKit.Iron, 6, 0.2f);
            WreckKit.Scatter(c, 2, Width - 2, Ground, ramps, 808, 9);

            ArtKit.AddOutline(c);

            var vents = new[]
            {
                new WreckKit.Vent(Shift + 38f, Ground - 32f, 28f, 3f),
                new WreckKit.Vent(Shift + 10f, Ground - 16f, 18f, 2f),
            };

            return WreckKit.Animate(c, vents, embers, FrameCount, 47, 5);
        }

        /// <summary>Um cristal da geodo partido, fincado: ponta clara, base escura.</summary>
        private static void Shard(PixelCanvas c, float x, float ground, float height, float lean)
        {
            (float tx, float ty) = ArtKit.Along(x, ground, -90f + lean, height);
            ArtKit.Polygon(c, new[] { (x - 1.8f, ground), (tx, ty), (x + 1.8f, ground) }, (px, py) =>
                px < (x + tx) * 0.5f ? Crystal[2] : Crystal[1]);
            c.Set((int)tx, (int)ty + 1, Crystal[3]);
        }
    }
}
