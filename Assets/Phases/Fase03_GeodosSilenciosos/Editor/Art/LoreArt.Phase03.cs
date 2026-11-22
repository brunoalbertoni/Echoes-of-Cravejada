using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que se le no District 3, o das geodes. Mesmo esquema das outras
    /// fases: o desenho pequeno do mapa (o mesmo do cenario, do GeodeArt e do
    /// StatueArt) e o desenho grande da leitura, com o texto escrito nele.
    ///
    /// Os bilhetes de mineiro sao todos o mesmo papel (MinerNoteView), preso
    /// por uma lasca de cristal; as gravuras das Ruinas aparecem ampliadas;
    /// a tabua do turno tem os doze nomes; a placa da Coroa e a de prata tem
    /// so o que esta escrito nelas.
    /// </summary>
    public static partial class LoreArt
    {
        private static Dictionary<string, Piece> _phase03;

        /// <summary>As do District 3, pelo nome do LoreSpot (e a placa do chassi da broca).</summary>
        public static IReadOnlyDictionary<string, Piece> Phase03 => _phase03 ?? (_phase03 = BuildPhase03());

        /// <summary>A placa no chassi da broca, que nao e um LoreSpot: o gerador liga ela na mao.</summary>
        public const string DrillPlaque = "Lore_DrillPlaque";

        private static readonly Rgba MinerInk = new Rgba(54, 48, 72);

        private static Dictionary<string, Piece> BuildPhase03()
        {
            var pieces = new List<Piece>
            {
                Make("Lore_Helmet", "helmet", GeodeArt.Helmet, 0f, false, HelmetDoc()),
                Make("Lore_RiftSign", "crown_sign", GeodeArt.CrownSign, 0f, false, CrownSignDoc()),
                Make("Lore_HunterRoster", "miner_note", GeodeArt.MinerNote, 0.8f, true, RosterDoc()),
                Make("Lore_WordPlaque", "silver_words", StatueArt.SilverPlaque, 1.45f, true, WordPlaqueDoc()),

                Make("Lore_VaultPage", "miner_note", GeodeArt.MinerNote, 0.8f, true, VaultPageDoc()),
                Make("Lore_LoopNote", "miner_note", GeodeArt.MinerNote, 0.8f, true, LoopNoteDoc()),
                Make("Lore_MinerNote", "miner_note", GeodeArt.MinerNote, 0.8f, true, MinerNoteDoc()),

                Make(DrillPlaque, "drill_plaque", DrillPlaqueMap, 0f, false, DrillPlaqueDoc()),
            };

            for (int scene = 0; scene < CrystalArt.EngravingCount; scene++)
            {
                int s = scene;
                pieces.Add(Make("Lore_Engraving_" + (scene + 1), "engraving_" + scene, () => CrystalArt.Engraving(s), 1.75f, true,
                    EngravingDoc(scene)));
            }

            return Index(pieces);
        }

        // ------------------------------------------------------------------
        // O capacete
        // ------------------------------------------------------------------


        // ------------------------------------------------------------------
        // A placa da Coroa, na beira da Fenda
        // ------------------------------------------------------------------


        // ------------------------------------------------------------------
        // A tabua do turno
        // ------------------------------------------------------------------

        private static readonly string[] Shift =
        {
            "BENTO", "ISAURA", "JOAQUIM", "TEREZA", "OSVALDO", "MIRANDA",
            "LÚCIO", "DAMIÃO", "ZÉ CORDA", "NEVES", "ARLINDO",
        };


        // ------------------------------------------------------------------
        // A placa de prata e o bloco de notas
        // ------------------------------------------------------------------


        // ------------------------------------------------------------------
        // Os bilhetes de mineiro
        // ------------------------------------------------------------------


        // ------------------------------------------------------------------
        // As gravuras das Ruinas
        // ------------------------------------------------------------------


        // ------------------------------------------------------------------
        // A placa do chassi da broca
        // ------------------------------------------------------------------


        private static PixelCanvas DrillPlaqueMap()
        {
            var c = new PixelCanvas(12, 7);
            c.Fill(1, 1, 10, 5, Silver[2]);
            c.Fill(1, 1, 10, 1, Silver[3]);
            for (int x = 3; x < 9; x++) if (x % 2 == 0) c.Set(x, 3, Silver[0]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
