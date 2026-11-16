using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que se le no District 4, o dos laboratorios. Mesmo esquema das outras
    /// fases: o desenho pequeno do mapa e o desenho grande da leitura, com o
    /// texto escrito nele.
    ///
    /// Toda folha do diario de laboratorio e o mesmo papel pautado (LabPage),
    /// com o dia no alto; todo retrato e a mesma moldura de prata; os dois
    /// quadros-negros sao o mesmo quadro; as placas de prata sao a mesma placa.
    /// A ficha do n.º 7 tem o desenho tecnico do proprio Scrap.
    /// </summary>
    public static partial class LoreArt
    {
        private static Dictionary<string, Piece> _phase04;

        /// <summary>As do District 4, pelo nome do LoreSpot.</summary>
        public static IReadOnlyDictionary<string, Piece> Phase04 => _phase04 ?? (_phase04 = BuildPhase04());

        private static readonly Rgba LabInk = new Rgba(40, 46, 78);
        private static readonly Rgba LabPaper = new Rgba(226, 222, 204);
        private static readonly Rgba LabPaperDark = new Rgba(180, 172, 146);
        private static readonly Rgba RuleBlue = new Rgba(150, 174, 210);
        private static readonly Rgba Blackboard = new Rgba(34, 46, 40);

        private static Dictionary<string, Piece> BuildPhase04()
        {
            var pieces = new List<Piece>
            {
                Make("Lore_Portal", "silver_sign", SilverSignMap, 2.2f, true, SilverPlaqueDoc(
                    new[] { "ATELIÊS ALQUÍMICOS", "DE SUA MAJESTADE", "", "ENTRADA PROIBIDA", "A METAIS COMUNS" },
                    null, 4401)),

                Make("Lore_Diary_1", "lab_page", LabPageMap, 0f, false, LabPageDoc(12, new[]
                {
                    "Descoberta: o mercúrio retarda a corrosão do Óxido. Mergulhei uma placa de prata no mercúrio e deixei do lado de outra, seca, no mesmo vapor verde.",
                    "A seca começou a manchar no primeiro dia. A mergulhada aguentou três dias a mais. É pouco, mas é a primeira coisa que funciona.",
                    "Sua Majestade foi informada. Há esperança. Vamos curar a cidade inteira.",
                }, 4421, 0)),
                Make("Lore_Diary_Mercury", "lab_page", LabPageMap, 0f, false, LabPageDoc(40, new[]
                {
                    "Sete destilações seguidas. O mercúrio sai tão puro que reflete o rosto de quem olha, e no reflexo não aparece mancha nenhuma de Óxido.",
                    "Efeito colateral: o vapor do mercúrio é venenoso. Os assistentes de cobre desmaiam perto dos alambiques. O Mestre respira fundo e diz que é perfume.",
                }, 4422, 1)),
                Make("Lore_Diary_2", "lab_page", LabPageMap, 0f, false, LabPageDoc(90, new[]
                {
                    "Conclusão ruim: o mercúrio só atrasa o Óxido, não cura. Em placa, a gente ganha dias. Precisamos saber o que acontece num corpo inteiro.",
                    "Sua Majestade autorizou testes em cobaias de cobre. Disse que tem muitos, e que ninguém vai sentir falta.",
                }, 4423, 1)),
                Make("Lore_Diary_Light", "lab_page", LabPageMap, 0f, false, LabPageDoc(150, new[]
                {
                    "A luz alquímica atravessa o Óxido e mostra o que já apodreceu por dentro do metal. Testamos em placas de prata nobre: por dentro, estavam podres.",
                    "Ou seja: a prata também tá apodrecendo, a cidade inteira tá. Só demora mais pra aparecer. Sua Majestade proibiu que a luz fosse apontada pra Corte.",
                    "Aviso: as lâmpadas quentes queimam, andem entre um clarão e outro. A fria, a dos espelhos, só aponta o caminho.",
                }, 4424, 1)),
                Make("Lore_Diary_3", "lab_page", LabPageMap, 0f, false, LabPageDoc(210, new[]
                {
                    "Teste em corpo inteiro. O tanque 4 não acordou. O tanque 7 acordou errado. Juntamos o que sobrou do 7 com o que sobrou do 9.",
                    "O mercúrio entra no corpo e não sai mais. O Mestre diz que estamos quase lá. Ele não dorme. E a prata dele tá ficando mole, brilhante demais.",
                }, 4425, 2)),
                Make("Lore_Diary_4", "lab_page", LabPageMap, 0f, false, LabPageDoc(255, new[]
                {
                    "O Mestre entrou no tanque grande, sozinho. A ideia dele: se o mercúrio atrasa o Óxido, então quem for feito de mercúrio não apodrece nunca.",
                    "Ele saiu. Fala como antes, educado. Mas escorre.",
                    "Os guardas de prata perfeita passaram a vigiar o laboratório. Não sei se tão protegendo a gente ou ele.",
                }, 4426, 3)),
                Make("Lore_Diary_5", "lab_page", LabPageMap, 0f, false, LabPageDoc(300, new[]
                {
                    "Descoberta principal: os protótipos brutos são imunes. Quartzo sem lapidar, bronze sem polimento. O Óxido não tem onde se agarrar neles.",
                    "Isso é a cura. Um corpo bruto pra cada um da cidade, e ninguém mais apodrece.",
                    "Levei o n.º 7 até o trono, andando. Imune. Sua Majestade recusou. Ordem de descarte assinada.",
                }, 4427, 3)),

                Make("Lore_Chalk_Mercury", "chalkboard", ChalkboardMap, 1.2f, true, ChalkMercuryDoc()),
                Make("Lore_Chalk_Light", "chalkboard", ChalkboardMap, 1.2f, true, ChalkLightDoc()),

                Make("Lore_PrototypeBoard", "cork_board", CorkBoardMap, 1.4f, true, PrototypeBoardDoc()),
                Make("Lore_PrototypeSheet", "proto_sheet", () => MarbleArt.PrototypeSheet(), 1.6f, true, PrototypeSheetDoc()),

                Make("Lore_Amphitheater", "silver_sign", SilverSignMap, 1.8f, true, SilverPlaqueDoc(
                    new[] { "TRIBUNAL DE", "INOVAÇÕES", "", "AQUI A CIÊNCIA SE", "APRESENTA A", "SUA MAJESTADE" },
                    null, 4441)),
                Make("Lore_DiscardOrder", "noble_paper_pinned", () => NoblePaperMap(true), 1.6f, true, DiscardOrderDoc()),
            };

            return Index(pieces);
        }

        // ------------------------------------------------------------------
        // A folha do diario de laboratorio
        // ------------------------------------------------------------------


        /// <summary>A folha no chao: o caderno aberto, pautado.</summary>
        private static PixelCanvas LabPageMap()
        {
            var c = new PixelCanvas(16, 8);
            c.Fill(1, 2, 14, 5, LabPaper);
            c.Fill(8, 2, 1, 5, LabPaperDark);
            for (int x = 2; x < 14; x++)
            {
                if (x == 8) continue;
                if (x % 3 != 0) c.Set(x, 4, LabInk);
                if (x % 4 != 1) c.Set(x, 5, RuleBlue);
            }

            c.Set(11, 3, LabArt.Mercury[2]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // Os retratos
        // ------------------------------------------------------------------


        // ------------------------------------------------------------------
        // O quadro-negro
        // ------------------------------------------------------------------


        /// <summary>O quadro-negro no mapa: num cavalete, com riscos de giz.</summary>
        private static PixelCanvas ChalkboardMap()
        {
            var c = new PixelCanvas(26, 20);
            c.Fill(1, 1, 24, 15, LabArt.Walnut[2]);
            c.Fill(2, 2, 22, 13, Blackboard);
            for (int row = 0; row < 4; row++)
            {
                for (int x = 4; x < 21; x++) if ((x * 3 + row) % 5 != 0) c.Set(x, 4 + row * 3, Chalk.WithAlpha(200));
            }

            c.Fill(4, 16, 2, 4, LabArt.Walnut[1]);
            c.Fill(20, 16, 2, 4, LabArt.Walnut[1]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A placa de prata
        // ------------------------------------------------------------------


        /// <summary>A placa de prata no mapa, na parede.</summary>
        private static PixelCanvas SilverSignMap()
        {
            var c = new PixelCanvas(24, 12);
            c.Fill(1, 1, 22, 10, LabArt.Silver[2]);
            c.Fill(1, 1, 22, 1, LabArt.Silver[3]);
            c.Fill(1, 10, 22, 1, LabArt.Silver[0]);
            for (int row = 0; row < 3; row++)
            {
                for (int x = 4; x < 20; x++) if ((x + row) % 4 != 0) c.Set(x, 3 + row * 2, LabArt.Silver[0]);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // O quadro das fichas dos prototipos
        // ------------------------------------------------------------------


        private static PixelCanvas CorkBoardMap()
        {
            var c = new PixelCanvas(26, 20);
            c.Fill(1, 1, 24, 18, LabArt.Walnut[2]);
            c.Fill(2, 2, 22, 16, new Rgba(176, 130, 86));
            for (int i = 0; i < 6; i++) c.Fill(4 + (i % 3) * 7, 4 + (i / 3) * 7, 5, 5, new Rgba(232, 226, 206));
            c.Set(12, 16, new Rgba(200, 40, 40));
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A ficha do n.º 7
        // ------------------------------------------------------------------

    }
}
