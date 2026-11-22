using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O laboratorio do District 4, peca por peca: alambiques, retortas, bicos
    /// de gas, o grande destilador, vidros de especime, estantes de reagente,
    /// tanques de cultura com as cobaias de cobre, gaiolas, a mesa de
    /// dissecacao, as mesas dos prototipos, o Grande Alambique e o que vai no
    /// anfiteatro. Tudo a 16 px por tile, desenhado aqui (y cresce pra baixo),
    /// sem UnityEngine.
    ///
    /// <see cref="Sprites"/> lista os nomes e o desenho de cada um, pro gerador
    /// gravar tudo num laco.
    /// </summary>
    public static partial class LabArt
    {
        // ------------------------------------------------------------------
        // Cores
        // ------------------------------------------------------------------

        public static readonly Rgba[] Silver =
            { new Rgba(70, 76, 96), new Rgba(120, 128, 150), new Rgba(178, 186, 206), new Rgba(236, 240, 250) };

        public static readonly Rgba[] Marble =
            { new Rgba(92, 98, 120), new Rgba(150, 156, 176), new Rgba(198, 204, 220), new Rgba(238, 240, 248) };

        public static readonly Rgba[] Walnut =
            { new Rgba(40, 26, 24), new Rgba(70, 44, 34), new Rgba(104, 68, 46), new Rgba(142, 98, 66) };

        public static readonly Rgba[] Velvet =
            { new Rgba(56, 14, 26), new Rgba(98, 26, 42), new Rgba(146, 44, 62), new Rgba(194, 82, 96) };

        public static readonly Rgba[] Copper = ArtKit.Copper;
        public static readonly Rgba[] Brass = ArtKit.Brass;
        public static readonly Rgba[] Iron = ArtKit.Iron;

        public static readonly Rgba[] Mercury =
            { new Rgba(96, 104, 124), new Rgba(160, 170, 192), new Rgba(214, 222, 236), new Rgba(255, 255, 255) };

        public static readonly Rgba OxideGreen = new Rgba(92, 242, 153);
        public static readonly Rgba OxideDark = new Rgba(40, 150, 96);
        public static readonly Rgba Amber = new Rgba(240, 168, 60);
        public static readonly Rgba Azure = new Rgba(96, 156, 255);
        public static readonly Rgba Rose = new Rgba(224, 96, 196);
        public static readonly Rgba Flame = new Rgba(120, 180, 255);
        public static readonly Rgba FlameCore = new Rgba(230, 244, 255);

        private static readonly Rgba GlassRim = new Rgba(200, 228, 240, 230);
        private static readonly Rgba GlassFill = new Rgba(150, 196, 214, 70);
        private static readonly Rgba GlassShine = new Rgba(250, 254, 255, 235);
        private static readonly Rgba Ink = ArtKit.Outline;

        // ------------------------------------------------------------------
        // A lista
        // ------------------------------------------------------------------

        /// <summary>Os sprites de cenario: nome, desenho e se o pivo e no alto (pendurado).</summary>
        public static IEnumerable<(string Name, Func<PixelCanvas> Draw, bool Hanging)> Sprites()
        {
            yield return ("lab_alembic", Alembic, false);
            yield return ("lab_retort", Retort, false);
            yield return ("lab_burner", Burner, false);
            yield return ("lab_still", Still, false);
            for (int i = 0; i < 3; i++) { int k = i; yield return ("lab_jar_" + i, () => SpecimenJar(k), false); }
            yield return ("lab_rack", TubeRack, false);
            yield return ("lab_table", DissectionTable, false);
            yield return ("lab_shelf", ReagentShelf, false);
            yield return ("lab_cage", () => Cage(false), false);
            yield return ("lab_cage_broken", () => Cage(true), false);
            for (int i = 0; i < 3; i++) { int k = i; yield return ("lab_tank_" + i, () => CultureTank(k), false); }
            yield return ("lab_great_tank", GreatTank, false);
            yield return ("lab_chandelier", Chandelier, true);
            yield return ("lab_pipes", Pipes, true);
            yield return ("lab_coats", CoatRack, false);
            yield return ("lab_lectern", Lectern, false);
            yield return ("lab_bust", Bust, false);
            yield return ("lab_lens_bench", LensBench, false);
            yield return ("lab_prism", Prism, false);
            yield return ("lab_mirror", StandingMirror, false);
            yield return ("lab_seats", Seats, false);
            yield return ("lab_stage", Stage, false);
            yield return ("lab_drain", Drain, false);
            for (int i = 0; i < 7; i++) { int k = i; yield return ("lab_proto_table_" + i, () => PrototypeTable(k), false); }
        }

        /// <summary>O Grande Alambique com tantos encaixes acesos (0 a 3).</summary>
        public static string GreatAlembicName(int lit) => "lab_great_alembic_" + lit;

        public static string ReagentName(int index) => "lab_reagent_" + index;
    }
}
