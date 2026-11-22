using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A cenografia do District 1, desenhada por codigo: o que conta, so de
    /// olhar, que isto e o Poco de Sucata da Coroa.
    ///
    ///   Na parede — vigas rebitadas, canos com flange, correntes com gancho,
    ///   engrenagens gigantes presas na rocha (que giram), a Calha de Descarte
    ///   N.º 1 saindo do teto, cartazes da Coroa, a esteira e os cestos da
    ///   Triagem, cabos caidos, respiros com o verde do Oxide atras.
    ///
    ///   No chao — montes de sucata feitos de pecas de autômato (cabecas,
    ///   bracos, engrenagens, canos, latas), carcacas de descartados que nao
    ///   levantaram mais, carrinhos, rodas, barris, caldeiras furadas, os
    ///   postes de lampiao de cristal da Avenida das Facetas (que acendem com a
    ///   purificacao) e pocas de Oxide (que somem com ela).
    ///
    /// Tudo em 16 pixels por tile, como o tileset da caverna, pra conversar
    /// com o terreno. Os nomes e os pivos de cada peca estao em Specs.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static partial class DecorArt
    {
        public const int PixelsPerUnit = 16;

        private static readonly Rgba Ink = new Rgba(24, 18, 26);

        /// <summary>Como uma peca e importada: o pivo (fracao do tamanho) e se repete em modo Tiled.</summary>
        public readonly struct Spec
        {
            public readonly float PivotX;
            public readonly float PivotY;
            public readonly bool Tiled;

            public Spec(float pivotX, float pivotY, bool tiled = false)
            {
                PivotX = pivotX;
                PivotY = pivotY;
                Tiled = tiled;
            }
        }

        // ------------------------------------------------------------------
        // Os nomes
        // ------------------------------------------------------------------

        public const string Chute = "d1_chute";
        public const string GirderV = "d1_girder_v";
        public const string GirderH = "d1_girder_h";
        public const string PipeV = "d1_pipe_v";
        public const string PipeH = "d1_pipe_h";
        public const string PipeFlange = "d1_pipe_flange";

        // As curvas do cano, pelos dois lados por onde ele entra e sai.
        public const string PipeElbowDownRight = "d1_pipe_elbow_down_right";
        public const string PipeElbowDownLeft = "d1_pipe_elbow_down_left";
        public const string PipeElbowUpLeft = "d1_pipe_elbow_up_left";
        public const string PipeElbowUpRight = "d1_pipe_elbow_up_right";

        public const string GearBig = "d1_gear_big";
        public const string GearMid = "d1_gear_mid";
        public const string Chain = "d1_chain";
        public const string Hook = "d1_hook";
        public const string Cage = "d1_cage";
        public const string PosterCrown = "d1_poster_crown";
        public const string PosterDecree = "d1_poster_decree";
        public const string Hazard = "d1_hazard";
        public const string ArrowUp = "d1_arrow_up";
        public const string Vent = "d1_vent";
        public const string Conveyor = "d1_conveyor";
        public const string ConveyorMotor = "d1_conveyor_motor";
        public const string SortingBins = "d1_bins";
        public const string Scaffold = "d1_scaffold";
        public const string ClawMarks = "d1_claw_marks";
        public const string Tally = "d1_tally";
        public const string Cable = "d1_cable";
        public const string WallLamp = "d1_wall_lamp";

        public static string Heap(int size, int variant) => $"d1_heap_{size}_{variant}";
        public const int HeapSizes = 3;
        public const int HeapVariants = 3;

        public static string Carcass(int variant) => "d1_carcass_" + variant;
        public const int CarcassVariants = 3;

        public const string Barrel = "d1_barrel";
        public const string BarrelTipped = "d1_barrel_tipped";
        public const string Cart = "d1_cart";
        public const string Wheel = "d1_wheel";
        public const string LampPost = "d1_lamp_post";
        public const string LampPostGlow = "d1_lamp_post_glow";
        public const string Puddle = "d1_puddle";
        public const string PuddleSmall = "d1_puddle_small";
        public const string Bolts = "d1_bolts";
        public const string BoilerWreck = "d1_boiler_wreck";
        public const string PipePile = "d1_pipe_pile";
        public const string Crate = "d1_crate";
        public const string CrateOpen = "d1_crate_open";
        public const string ToolRack = "d1_tool_rack";

        public const int LoosePartVariants = 3;

        /// <summary>Uma peca de descartado solta no chao (0 cabeca, 1 braco, 2 perna).</summary>
        public static string LoosePart(int variant) => "d1_part_" + variant;

        /// <summary>Todas as pecas, desenhadas, pelo nome.</summary>
        public static Dictionary<string, PixelCanvas> All()
        {
            var all = new Dictionary<string, PixelCanvas>
            {
                [Chute] = DrawChute(),
                [GirderV] = DrawGirder(true),
                [GirderH] = DrawGirder(false),
                [PipeV] = DrawPipe(true),
                [PipeH] = DrawPipe(false),
                [PipeFlange] = DrawFlange(),
                [PipeElbowDownRight] = DrawElbow(),
                // Cada quarto de volta no sentido horario leva o lado de baixo
                // pra esquerda e o da direita pra baixo.
                [PipeElbowDownLeft] = DrawElbow().RotatedClockwise(),
                [PipeElbowUpLeft] = DrawElbow().RotatedClockwise().RotatedClockwise(),
                [PipeElbowUpRight] = DrawElbow().RotatedClockwise().RotatedClockwise().RotatedClockwise(),
                [GearBig] = DrawGear(64, 14, 31f, 26f, 3),
                [GearMid] = DrawGear(40, 10, 19f, 15.5f, 7),
                [Chain] = DrawChain(),
                [Hook] = DrawHook(),
                [Cage] = DrawCage(),
                [PosterCrown] = DrawPoster(false),
                [PosterDecree] = DrawPoster(true),
                [Hazard] = DrawHazard(),
                [ArrowUp] = DrawArrow(),
                [Vent] = DrawVent(),
                [Conveyor] = DrawConveyor(),
                [ConveyorMotor] = DrawConveyorMotor(),
                [SortingBins] = DrawBins(),
                [Scaffold] = DrawScaffold(),
                [ClawMarks] = DrawClawMarks(),
                [Tally] = DrawTally(),
                [Cable] = DrawCable(),
                [WallLamp] = DrawWallLamp(),
                [Barrel] = DrawBarrel(),
                [BarrelTipped] = DrawBarrelTipped(),
                [Cart] = DrawCart(),
                [Wheel] = DrawWheel(),
                [LampPost] = DrawLampPost(false),
                [LampPostGlow] = DrawLampPost(true),
                [Puddle] = DrawPuddle(32, 8, 1),
                [PuddleSmall] = DrawPuddle(18, 6, 2),
                [Bolts] = DrawBolts(),
                [BoilerWreck] = DrawBoilerWreck(),
                [PipePile] = DrawPipePile(),
                [Crate] = DrawCrate(false),
                [CrateOpen] = DrawCrate(true),
                [ToolRack] = DrawToolRack(),
            };

            int[] heapW = { 32, 52, 84 };
            int[] heapH = { 18, 28, 42 };
            for (int size = 0; size < HeapSizes; size++)
            {
                for (int variant = 0; variant < HeapVariants; variant++)
                {
                    all[Heap(size, variant)] = DrawHeap(heapW[size], heapH[size], 100 + size * 10 + variant);
                }
            }

            for (int variant = 0; variant < CarcassVariants; variant++) all[Carcass(variant)] = DrawCarcass(variant);
            for (int variant = 0; variant < LoosePartVariants; variant++) all[LoosePart(variant)] = DrawLooseParts(variant);

            return all;
        }

        /// <summary>O pivo e o modo de cada peca. O que nao esta aqui tem o pivo no canto de baixo, a esquerda.</summary>
        public static Spec SpecOf(string name)
        {
            switch (name)
            {
                case GirderV:
                case GirderH:
                case PipeV:
                case PipeH:
                case Chain:
                case Conveyor:
                    return new Spec(0f, 0f, true);

                // As engrenagens giram em volta do eixo.
                case GearBig:
                case GearMid:
                    return new Spec(0.5f, 0.5f);

                // O que pendura balanca em volta do ponto de cima.
                case Hook:
                case Cage:
                    return new Spec(0.5f, 1f);

                default:
                    return new Spec(0f, 0f);
            }
        }

        private static readonly Dictionary<char, string[]> Font = new Dictionary<char, string[]>
        {
            ['9'] = new[] { "###", "#.#", "###", "..#", "###" },
            ['2'] = new[] { "###", "..#", "###", "#..", "###" },
            ['5'] = new[] { "###", "#..", "###", "..#", "###" },
            ['1'] = new[] { ".#.", "##.", ".#.", ".#.", "###" },
        };
        private static readonly Rgba[] Wood =
            { new Rgba(58, 38, 28), new Rgba(96, 64, 40), new Rgba(138, 96, 58), new Rgba(176, 132, 84) };
    }
}
