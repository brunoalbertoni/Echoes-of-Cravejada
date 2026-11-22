using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase02Layout
    {
        // ------------------------------------------------------------------
        // A. O Esgoto (Ala Baixa)
        // ------------------------------------------------------------------

        /// <summary>
        /// O comeco da Ala Baixa, embaixo da Cisterna: cai-se pelo alcapao numa
        /// ilha e cruza-se o canal de ilha em ilha, com o dash, ate a coluna
        /// que (religada no fim da ala) devolve o Scrap a Praca. Dali um tunel
        /// curto segue pro Deposito. A oeste, um patamar seco com as marcas de
        /// quem ficou preso aqui.
        /// </summary>
        private static void BuildSewer()
        {
            const string room = "A_Sewer";
            Room(room, 86, 6, 129, 14);

            Carve(86, 6, 129, 14);

            // O canal: ilhas largas e vaos de tres, que se pulam sem dash. Cada
            // vao e uma poca so dele, encaixada entre as ilhas — o acido nunca
            // passa por baixo de onde se pisa.
            int[,] gaps = { { 89, 91 }, { 96, 98 }, { 107, 109 }, { 114, 116 }, { 121, 123 } };
            for (int i = 0; i < gaps.GetLength(0); i++)
            {
                Carve(gaps[i, 0], 3, gaps[i, 1], 5);
                Acid("Acid_Sewer_" + (i + 1), gaps[i, 0], 3, gaps[i, 1], 5);
            }

            // Uma ilha mais alta no meio: pisa-se em 7.
            Solid(110, 6, 113, 6);

            Tell(LoreKind.Graffiti, room, "Lore_SewerTally", "as marcas de giz", 86, 6,
                "Riscos de giz na parede, em grupos de cinco. Dezenas de grupos.",
                "Em cima deles, numa letra cansada: \"DIAS SEM SUBIR.\"",
                "O último grupo tem só três riscos.");

            // A coluna de volta pra Praca: sempre ligada, e o jeito de sair do
            // Esgoto a qualquer hora.
            Carve(126, 15, 128, 33);
            Lift("Lift_Sewer_Up", 126, 128, 6, 36);

            Tell(LoreKind.Plaque, room, "Lore_SewerPlaque", "a placa do coletor", 124, 6,
                "Uma placa de latão, meio afundada na lama do esgoto:",
                "\"COLETOR DE CONDENSADO N.º 4. NÃO ENTRAR SEM MÁSCARA.\"",
                "Embaixo, alguém riscou nomes, um por linha. Doze. O último está pela metade.");

            // O tunel pro Deposito.
            Carve(130, 6, 140, 9);
            Tell(LoreKind.Graffiti, room, "Lore_TunnelArrow", "a seta pintada", 131, 6,
                "Uma seta pintada às pressas na parede, apontando para o leste: \"DEPÓSITO — MÁSCARAS\".",
                "Alguém riscou \"MÁSCARAS\" com tinta fresca e escreveu por cima: \"NADA\".",
                "A tinta escorreu antes de secar.");

            // So sanguessugas aqui embaixo, cada uma no meio de uma ilha larga:
            // da pra ve-las do vao anterior e esperar ela cair antes de pular.
            Leech("Leech_Sewer_1", 94, 14, 6, 92, 95);
            Leech("Leech_Sewer_2", 112, 14, 7, 110, 113);
            Leech("Leech_Sewer_3", 119, 14, 6, 117, 120);
            Leech("Leech_Tunnel", 137, 9, 6, 124, 152);

            Decor(FixtureKind.Barrel, 87, 6);
            Decor(FixtureKind.Sign, 100, 11);
            HangChain(105, 14, 4);
            HangChain(122, 14, 3);
            Lamp(87, 14, 0.5f);
            Lamp(107, 14, 0.5f);

            Fumes("Sewer", Area.Between(89, 3, 123, 5), 0.20f);
            RunPipe(PipeLayer.Mid, true, (84, 12), (130, 12));
            RunPipe(PipeLayer.Back, true, (90, 13), (90, 7));
            RunPipe(PipeLayer.Back, false, (115, 13), (115, 8));
            RunPipe(PipeLayer.Back, true, (130, 9), (141, 9));
            RunPipe(PipeLayer.Far, true, (44, 4), (64, 4));
            DripsIn(Area.Between(86, 6, 129, 14), 5, 81, true);
            DripsIn(Area.Between(130, 6, 140, 9), 4, 82, true);
        }

        // ------------------------------------------------------------------
        // B. A Casa de Maquinas (Ala Alta)
        // ------------------------------------------------------------------

        /// <summary>
        /// A Ala Alta: um salao de quatro andares de passarela por cima da
        /// Praca, com as caldeiras grandes no fundo. Chega-se pela coluna do
        /// balcao da Praca, que abre no chao, a leste.
        ///
        /// O caminho: chao → coluna sempre ligada → andar 1, cuja valvula (no
        /// fim leste, a vista) liga a coluna pro andar 2 → no andar 2, um vao
        /// cruzado por uma nuvem e a coluna que pulsa → andar 3, com a bancada
        /// da ala e a valvula da trava, que abre tambem a escotilha do chao,
        /// de volta pro balcao da Praca.
        ///
        /// Fora do caminho: a oeste do andar 1, a porta do Sotao; no fim do
        /// andar 2, o cartaz da Coroa; e, depois de outra nuvem no andar 3, um
        /// cofre.
        /// </summary>
        private static void BuildMachineHall()
        {
            const string room = "B_MachineHall";
            Room(room, 44, 52, 92, 84);

            Carve(44, 52, 92, 84);

            // Chao: o buraco da coluna que sobe da Praca. Pela metade leste
            // dele se cai de volta no balcao, a qualquer hora.
            Lift("Lift_Hall_1", 70, 72, 52, 64);

            Logbook(room, 2, "página do livro de turnos", 91, 52,
                "LIVRO DE TURNOS DA CALDEIRA — turno da manhã.",
                "A água tá com gosto de metal podre. Troquei o cano e não adiantou: o problema vem de baixo, do fundo da rocha.",
                "Quem pega turno perto das caldeiras sai tossindo verde.");

            // Andar 1.
            Grate(44, 68, 61);
            Grate(74, 92, 61);
            Rail(74, 92, 62);
            Carve(43, 62, 43, 65);
            Valve(room, "Valve_Hall_1", 89, 64, 90, 62, "vent:Lift_Hall_2");
            Lift("Lift_Hall_2", 80, 82, 62, 74).StartsOff = true;

            // Andar 2: o vao do meio pede a nuvem.
            Grate(47, 57, 71);
            Grate(66, 78, 71);
            Grate(84, 92, 71);
            Rail(66, 78, 72);
            Cloud("Cloud_Hall", 60, 72, 63, 75);
            Tell(LoreKind.Plaque, room, "Lore_CrownPoster", "um cartaz da Coroa", 54, 72,
                "Um cartaz desbotado, com o selo dos 925 Nobres:",
                "\"O VAPOR É O SANGUE DA CIDADE. CADA OPERÁRIO, UMA GOTA.\"");

            Decor(FixtureKind.Poster, 52, 74);

            Vent pulsing = Lift("Lift_Hall_3", 44, 46, 72, 83);
            pulsing.OnTime = 2.4f;
            pulsing.OffTime = 1.2f;

            // Andar 3: a bancada da ala, a valvula da trava, e, depois de uma
            // nuvem, o cofre.
            Grate(47, 70, 80);
            Rail(47, 70, 81);
            Bench(room, "Workbench_MachineHall", 49, 81);

            Valve(room, "Valve_Hall_2", 65, 83, 66, 81, "gate:Trava_Alta").Lock = true;

            Cloud("Cloud_Hall_Cache", 74, 81, 76, 84);
            Grate(81, 92, 80);
            Reward(room, RewardKind.ShardCache, "D2_Hall_Cache", 90, 81, 20);
            Mark(room, "Secret_SilverFragment_Hall", 90, 81);

            Enemy(EnemyKind.BrassWelder, "Welder_Machine_1", 44, 82, 52, start: 56);
            Leech("Leech_Machine_1", 76, 60, 52, 44, 82);
            Enemy(EnemyKind.AcidBloater, "Bloater_Machine", 74, 92, 62, start: 81);
            Enemy(EnemyKind.ValveSentry, "Sentry_Machine_1", 45, 45, 62, 1, 8);
            Enemy(EnemyKind.ValveSentry, "Sentry_Machine_2", 92, 92, 72, -1, 6);
            Leech("Leech_Machine_2", 70, 79, 72, 66, 78);
            Enemy(EnemyKind.BrassWelder, "Welder_Machine_2", 53, 70, 81, start: 58);

            // As caldeiras grandes no fundo, as fornalhas e o resto.
            Boiler("Boiler_Machine_1", 52, 52);
            Boiler("Boiler_Machine_2", 61, 52);
            Boiler("Boiler_Machine_3", 76, 52);
            Furnace("Furnace_Machine_1", 66, 52);
            Furnace("Furnace_Machine_2", 84, 62);
            Decor(FixtureKind.Sign, 80, 57);
            Decor(FixtureKind.Poster, 86, 74);
            Fix(FixtureKind.GaugeBig, "Gauge_Machine_1", 57, 64);
            Fix(FixtureKind.GaugeBig, "Gauge_Machine_2", 88, 66);
            Fix(FixtureKind.GaugeBig, "Gauge_Machine_3", 72, 76);
            Decor(FixtureKind.Barrel, 69, 81);
            Lamp(58, 60);
            Lamp(86, 60);
            Lamp(52, 70);
            Lamp(72, 70);
            Lamp(56, 84);
            Lamp(86, 84);
            HangChain(64, 84, 6);
            HangChain(78, 84, 4);

            RunPipe(PipeLayer.Mid, false, (57, 51), (57, 86));
            RunPipe(PipeLayer.Mid, true, (82, 51), (82, 61));
            RunPipe(PipeLayer.Back, false, (44, 69), (92, 69));
            RunPipe(PipeLayer.Back, true, (44, 78), (92, 78));
            RunPipe(PipeLayer.Back, false, (68, 52), (68, 60));
            RunPipe(PipeLayer.Far, false, (22, 35), (46, 35));
            RunPipe(PipeLayer.Far, true, (30, 26), (30, 42));
            Leaks.Add(new Leak(57.5f, 66f, 1f, 0f));
            Leaks.Add(new Leak(82.5f, 58f, -1f, 0f));

            Hazes.Add(new Haze("Steam_MachineHall", 68f, 80f, 48f, 7f, 0.08f, 3.2f, false, Glow.Steam));
            DripsIn(Area.Between(44, 52, 92, 84), 9, 91, false);
        }

        // ------------------------------------------------------------------
        // C. O Sotao dos Manometros (Ala Alta)
        // ------------------------------------------------------------------

        /// <summary>
        /// A oeste do andar 1 da Casa de Maquinas: uma sala baixa e comprida,
        /// forrada de manometros. E um desvio — os corrompidos guardam a
        /// entrada; no fundo, em paz, o relatorio do Rei de Prata, um cofre e a
        /// valvula que liga a coluna dos Primeiros Canos (o atalho de volta da
        /// entrada ate aqui).
        /// </summary>
        private static void BuildGaugeAttic()
        {
            const string room = "C_GaugeAttic";
            Room(room, 4, 62, 42, 72);

            Carve(4, 62, 42, 72);
            Solid(4, 70, 10, 72);
            Solid(16, 62, 18, 63);

            Reward(room, RewardKind.ShardCache, "D2_Attic_Cache", 11, 62, 10);
            Mark(room, "Secret_SilverFragment_Attic", 11, 62);
            Tell(LoreKind.Page, room, "Lore_InspectionReport", "um relatório de inspeção", 8, 62,
                "Um relatório com o selo dos 925, esquecido embaixo de um manômetro.",
                "\"INSPEÇÃO DAS CALDEIRAS DO DISTRITO DE LATÃO. Pressão acima do limite em todas as linhas. Recomendo desligar metade das caldeiras.\"",
                "Carimbado por cima, em vermelho: \"NEGADO. A CIDADE NÃO PODE ESCURECER. — O REI DE PRATA.\"");

            Enemy(EnemyKind.BrassWelder, "Welder_Attic", 19, 34, 62, start: 28);
            Enemy(EnemyKind.ValveSentry, "Sentry_Attic", 36, 36, 62, -1, 7);
            Leech("Leech_Attic_1", 28, 72, 62, 19, 34);
            Leech("Leech_Attic_2", 20, 72, 62, 19, 34);

            // A parede de manometros.
            for (int x = 12; x <= 40; x += 5)
            {
                Fix(FixtureKind.GaugeBig, $"Gauge_Attic_{x}", x, 66 + (x / 5) % 2 * 2);
            }

            Decor(FixtureKind.Shelf, 13, 62);
            Decor(FixtureKind.Poster, 34, 66);
            Furnace("Furnace_Attic", 30, 62);
            Lamp(9, 69, 0.8f);
            Lamp(26, 72, 0.6f);
            Lamp(38, 72, 0.6f);

            Plumb(Area.Between(11, 62, 42, 72), 1001, PipeLayer.Back, false, 0);
            RunPipe(PipeLayer.Mid, true, (2, 66), (14, 66), (14, 63));
            Steam(room, Area.Between(4, 62, 42, 72), 0.09f);
            DripsIn(Area.Between(11, 62, 42, 72), 8, 101, false);
        }

        // ------------------------------------------------------------------
        // D. O Deposito (Ala Baixa)
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde as mascaras ficavam. Entra-se por baixo de um patamar (o tunel
        /// do Esgoto); a coluna do lado sobe ate ele, com a ficha da Coroa e um
        /// cofre. Uma caldeira corrompida derruba a parede rachada pra metade
        /// leste: prateleiras vazias, a ultima pagina do livro, a bancada da
        /// ala e a porta da Torre.
        /// </summary>
        private static void BuildDepot()
        {
            const string room = "D_Depot";
            Room(room, 141, 6, 182, 19);

            Carve(141, 6, 182, 19);

            Solid(141, 10, 146, 14);
            Lift("Lift_Depot_West", 147, 149, 6, 17);
            Reward(room, RewardKind.ShardCache, "D2_Depot_Cache", 142, 15, 10);
            Mark(room, "Secret_SilverFragment_Depot", 142, 15);
            Tell(LoreKind.Page, room, "Lore_DepotSheet", "a ficha do depósito", 145, 15,
                "Uma ficha de inventário, pregada na porta do depósito.",
                "\"ORDEM DA COROA: recolher todas as máscaras de solda e enviá-las ao alto de Cravejada, a Cidade de Prata. Prioridade: Nobres.\"",
                "Logo abaixo, à mão: \"Recolhidas: nenhuma. Não há máscaras neste depósito. — O chefe de manutenção.\"",
                "Nas prateleiras atrás da ficha, só restam as marcas de caixas retiradas às pressas.");

            Solid(160, 10, 161, 19);
            Cracked("Wall_Depot", 160, 6, 161, 9);
            PressureBoiler(room, "Boiler_Depot", 153, 6, 151, 6, "Wall_Depot");

            Logbook(room, 4, "última página do livro de turnos", 164, 6,
                "LIVRO DE TURNOS DA CALDEIRA — a última página.",
                "Quem trabalha de máscara de solda tá bem. Nenhuma mancha. O vapor verde não passa pelo filtro.",
                "As máscaras protegem do Óxido. Vou contar pro chefe.");

            Bench(room, "Workbench_Depot", 179, 6);
            Carve(183, 6, 193, 9);

            Enemy(EnemyKind.AcidBloater, "Bloater_Depot", 162, 177, 6, start: 173);
            Enemy(EnemyKind.BrassWelder, "Welder_Depot", 162, 177, 6, start: 166);
            Leech("Leech_Depot", 172, 19, 6, 162, 177);

            // As prateleiras vazias, de onde as caixas sairam as pressas.
            foreach (int x in new[] { 166, 169, 177 }) Decor(FixtureKind.Shelf, x, 6);
            Decor(FixtureKind.Shelf, 141, 15);
            Decor(FixtureKind.Barrel, 150, 6);
            Decor(FixtureKind.Barrel, 157, 6);
            Decor(FixtureKind.Barrel, 158, 6);
            Put(Debris(1), 171, 6);
            Put(Debris(0), 168, 6);
            Decor(FixtureKind.Sign, 152, 15);
            Lamp(144, 19, 0.7f);
            Lamp(156, 19);
            Lamp(170, 19);
            Lamp(180, 19, 0.9f);

            Furnace("Furnace_Depot", 174, 6);
            Fix(FixtureKind.GaugeBig, "Gauge_Depot", 166, 16);
            RunPipe(PipeLayer.Back, true, (141, 18), (182, 18));
            RunPipe(PipeLayer.Mid, false, (162, 17), (184, 17));
            DripsIn(Area.Between(147, 6, 182, 19), 7, 111, false);
        }

        // ------------------------------------------------------------------
        // E. A Torre das Valvulas (Ala Baixa)
        // ------------------------------------------------------------------

        /// <summary>
        /// O fim da Ala Baixa: um poco de quatro andares, a leste do Deposito.
        /// Na base, a valvula da primeira coluna; no andar 1, a da segunda; no
        /// andar 2, a placa da linha de vapor e a coluna que pulsa; no alto, o
        /// diario do chefe de manutencao e a valvula da trava.
        /// </summary>
        private static void BuildValveTower()
        {
            const string room = "E_ValveTower";
            Room(room, 194, 6, 213, 74);

            Carve(194, 6, 213, 74);

            // Base.
            Valve(room, "Valve_Tower_1", 210, 8, 211, 6, "vent:Lift_Tower_1");
            Lift("Lift_Tower_1", 195, 197, 6, 27).StartsOff = true;
            Enemy(EnemyKind.AcidBloater, "Bloater_Tower", 183, 213, 6, start: 198);
            Leech("Leech_Tower_1", 204, 24, 6, 183, 213);

            // Andar 1.
            Grate(200, 213, 25);
            Rail(203, 213, 26);
            Valve(room, "Valve_Tower_2", 211, 28, 212, 26, "vent:Lift_Tower_2");
            Lift("Lift_Tower_2", 200, 202, 26, 47).StartsOff = true;

            // Andar 2.
            Grate(194, 198, 45);
            Grate(206, 213, 45);
            Tell(LoreKind.Plaque, room, "Lore_TowerLine", "a placa da linha", 195, 46,
                "Cada andar da torre tem uma placa com o bairro que a sua linha de vapor aquecia. Esta diz:",
                "\"LINHA 3 — AVENIDA DAS FACETAS. PRIORIDADE: NOBRES.\"",
                "Embaixo, a giz: \"Sempre eles.\"");
            Vent pulsing = Lift("Lift_Tower_3", 207, 209, 46, 66);
            pulsing.OnTime = 2.5f;
            pulsing.OffTime = 1.2f;
            Enemy(EnemyKind.ValveSentry, "Sentry_Tower_1", 213, 213, 46, -1, 6);

            // O alto.
            Grate(194, 205, 65);
            Grate(211, 213, 65);
            Rail(194, 205, 66);

            Valve(room, "Valve_Tower_3", 194, 68, 195, 66, "gate:Trava_Baixa").Lock = true;

            Enemy(EnemyKind.BrassWelder, "Welder_Tower", 194, 205, 66, start: 201);
            Leech("Leech_Tower_3", 201, 74, 66, 194, 205);

            Tell(LoreKind.Page, room, "Lore_ChiefDiary", "o diário do chefe de manutenção", 211, 66,
                "Um caderno de capa de couro, deixado no alto da torre, virado pra sala das caldeiras.",
                "\"Se as caldeiras pararem, o vapor para. Sem vapor, o pessoal aqui de baixo congela antes mesmo de o Óxido chegar.\"",
                "\"Vou ficar com elas e manter a pressão. Tranquem a porta por fora e não abram, escutem o que escutarem.\"",
                "Embaixo da marca de uma mão, queimada no papel: \"Esse é o meu sacrifício. — O chefe de manutenção.\"");

            Boiler("Boiler_Tower_Base", 205, 6);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_1", 199, 14);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_2", 204, 36);
            Fix(FixtureKind.GaugeBig, "Gauge_Tower_3", 198, 56);
            Decor(FixtureKind.Sign, 203, 30);
            Lamp(208, 24, 0.7f);
            Lamp(196, 44, 0.7f);
            Lamp(212, 64, 0.7f);
            Lamp(212, 74, 0.8f);
            HangChain(196, 80, 7);
            HangChain(212, 44, 5);

            for (int x = 199; x <= 211; x += 6)
            {
                RunPipe(PipeLayer.Back, x % 12 == 3, (x, 4), (x, 76));
            }

            RunPipe(PipeLayer.Back, false, (192, 40), (215, 40));
            RunPipe(PipeLayer.Back, true, (192, 60), (215, 60));
            Hazes.Add(new Haze("Steam_Tower", 204f, 70f, 20f, 6f, 0.09f, 2f, false, Glow.Steam));
            DripsIn(Area.Between(194, 6, 213, 24), 6, 121, false);
        }

        // ------------------------------------------------------------------
        // F. A Galeria Alta
        // ------------------------------------------------------------------

        /// <summary>
        /// Por cima da Sala de Caldeiras: chega-se pela coluna do tunel, a
        /// leste. Na chegada, a placa do cano-mestre. A oeste, o desafio do
        /// Salto de Pressao: canos longe e alto demais um do outro ate o
        /// mirante, de onde se ve a cidade la em cima, e um cofre. A leste, a
        /// porta da subida pro elevador.
        /// </summary>
        private static void BuildHighGallery()
        {
            const string room = "F_HighGallery";
            Room(room, 130, 78, 188, 92);

            Carve(130, 78, 188, 92);
            Voids.Add(Area.Between(130, 78, 188, 92));

            Solid(130, 78, 140, 89);
            PipeFloor(160, 163, 79);
            PipeFloor(152, 155, 83);
            PipeFloor(144, 147, 87);

            // A porta da subida, a leste.
            Carve(189, 78, 189, 81);

            Tell(LoreKind.Plaque, room, "Lore_MasterPipe", "a inscrição do cano-mestre", 178, 78,
                "Um cano da grossura de uma casa, subindo até sumir na rocha. Está seco e corroído por dentro.",
                "Na placa, em letra de fundição:",
                "\"PARA A CIDADE DE PRATA.\"");
            RunPipe(PipeLayer.Far, true, (88, 39), (88, 47));
            RunPipe(PipeLayer.Far, true, (89, 39), (89, 47));

            Reward(room, RewardKind.ShardCache, "D2_Gallery_Cache", 133, 90, 20);
            Mark(room, "Secret_SilverFragment_Gallery", 133, 90);
            Tell(LoreKind.Page, room, "Lore_GalleryView", "um bilhete amassado", 137, 90,
                "Daqui, por uma fresta na rocha, dá pra ver Cravejada lá em cima pela primeira vez: os geodos enormes e apagados.",
                "Preso na grade, um bilhete amassado, na mesma letra redonda da carta:",
                "\"Me desculpa, minha Pequena.\"");

            Boiler("Boiler_Gallery", 166, 78);
            Fix(FixtureKind.GaugeBig, "Gauge_Gallery_1", 172, 84);
            Fix(FixtureKind.GaugeBig, "Gauge_Gallery_2", 148, 88);
            Furnace("Furnace_Gallery_High", 174, 78);
            Decor(FixtureKind.Sign, 134, 91);
            Decor(FixtureKind.Poster, 156, 86);
            Lamp(150, 92);
            Lamp(170, 92);
            Lamp(184, 92, 0.6f);
            Rail(130, 140, 90);

            RunPipe(PipeLayer.Mid, false, (128, 91), (190, 91));
            RunPipe(PipeLayer.Mid, true, (164, 91), (164, 84), (160, 84));

            Skylines.Add(new Skyline("Skyline_Gallery_Far", 75f, false, 3f, 0.25f));
            Skylines.Add(new Skyline("Skyline_Gallery_Near", 74.5f, true, 2f, 0.5f));

            Hazes.Add(new Haze("Steam_HighGallery", 159f, 88f, 58f, 5f, 0.08f, 3f, false, Glow.Steam));
            DripsIn(Area.Between(142, 78, 188, 92), 9, 131, false);
        }

        // ------------------------------------------------------------------
        // G. A Subida
        // ------------------------------------------------------------------

        /// <summary>
        /// A direita da Galeria Alta: o acesso de servico ao elevador, ja que
        /// a escadaria desabou. Um poco de passarelas, cada uma quatro tiles
        /// acima da outra, alternando de lado — feito pro Salto de Pressao. E
        /// a continuacao do poco da Torre das Valvulas, aberto embaixo: da
        /// torre se ve a subida, mas nao se alcanca. No
        /// alto, a passarela dos inspetores volta pra oeste, por cima da
        /// galeria, ate a estacao.
        /// </summary>
        private static void BuildElevatorClimb()
        {
            const string room = "G_ElevatorClimb";
            Room(room, 190, 78, 207, 103);

            Carve(190, 78, 207, 103);

            // O fundo e aberto pra Torre das Valvulas, logo embaixo: do alto
            // dela se ve a subida, mas e alto demais pra chegar (ate com o
            // Salto de Pressao). So sobra um patamar, na porta da galeria. De
            // cima, da pra se deixar cair ate a torre.
            Carve(194, 75, 207, 77);
            Rail(190, 193, 78);

            // A primeira passarela fica so ao lado do patamar da porta (x 190-193),
            // nunca em cima dele: debaixo dela o Scrap batia a cabeca na grade e
            // nao tinha de onde pular. Do patamar se pula (com o Salto de
            // Pressao) e cai-se em cima dela.
            int[,] decks = { { 195, 197, 81 }, { 199, 207, 85 }, { 190, 197, 89 }, { 199, 207, 93 }, { 190, 197, 97 } };
            for (int i = 0; i < decks.GetLength(0); i++)
            {
                Grate(decks[i, 0], decks[i, 1], decks[i, 2]);
                Rail(decks[i, 0], decks[i, 1], decks[i, 2] + 1);
            }

            Tell(LoreKind.Graffiti, room, "Lore_ClimbArrow", "a seta de giz", 193, 78,
                "Uma seta a giz, subindo pela parede, e uma palavra só: \"ELEVADOR\".",
                "Embaixo, em outra letra: \"A escada caiu. Subam por aqui.\"");

            Decor(FixtureKind.Sign, 200, 79);
            Put(Crate(1), 190, 78);
            Lamp(194, 88, 0.7f);
            Lamp(203, 84, 0.7f);
            Lamp(203, 92, 0.7f);
            Lamp(198, 103, 0.8f);
            HangChain(198, 103, 8);
            HangChain(207, 103, 5);
            Fix(FixtureKind.GaugeBig, "Gauge_Climb_1", 205, 88);
            Fix(FixtureKind.GaugeBig, "Gauge_Climb_2", 191, 94);

            RunPipe(PipeLayer.Back, false, (198, 76), (198, 105));
            RunPipe(PipeLayer.Back, true, (204, 76), (204, 105));
            RunPipe(PipeLayer.Mid, false, (188, 101), (209, 101));
            Leaks.Add(new Leak(198.5f, 91f, 1f, 0f));
            Steam(room, Area.Between(190, 96, 207, 103), 0.10f);
            DripsIn(Area.Between(190, 78, 207, 103), 6, 151, false);
        }

        // ------------------------------------------------------------------
        // H. A Estacao do Elevador
        // ------------------------------------------------------------------

        /// <summary>
        /// O topo do distrito: a passarela dos inspetores, por cima da Galeria
        /// Alta, e a estacao, em cima do poco da Escadaria das Travas. A escada
        /// que subia da antessala chega aqui quebrada — um buraco no piso, com
        /// o guarda-corpo partido. Por ele se desce de volta. A cabine do
        /// elevador fica bem em cima do poco: sobe ate o alto, onde o latao vira
        /// cristal, e leva pro District 3.
        /// </summary>
        private static void BuildElevatorStation()
        {
            const string room = "H_ElevatorStation";
            Room(room, 104, 100, 189, 110);

            // A passarela dos inspetores.
            Carve(133, 100, 189, 103);
            Rail(133, 189, 100);
            Decor(FixtureKind.Poster, 146, 102);
            Decor(FixtureKind.Poster, 170, 102);
            Lamp(140, 103, 0.6f);
            Lamp(158, 103, 0.6f);
            Lamp(176, 103, 0.6f);
            RunPipe(PipeLayer.Back, false, (131, 102), (191, 102));

            // A estacao.
            Carve(104, 100, 132, 110);
            Solid(104, 108, 108, 110);
            Solid(128, 108, 132, 110);

            // O buraco da escada quebrada, a oeste da cabine.
            Carve(114, 99, 118, 99);
            Rail(112, 113, 100);
            Rail(119, 119, 100);
            Put(Debris(0), 113, 100);

            // A cabine e o poco dela, que sobe ate o alto do mapa.
            Carve(120, 111, 122, ElevatorTopY + 4 < Height - 1 ? (int)ElevatorTopY + 4 : Height - 2);
            Mark(room, "Exit_District3", District3Tile.X, District3Tile.Y);
            Fixtures.Add(new Fixture(FixtureKind.ElevatorRail, "Elevator_Rails", 120, 100, (int)ElevatorTopY + 4 - 100));

            // No alto do poco o latao acaba: e rocha azulada e cristal.
            MaterialZones.Add((Area.Between(114, 120, 128, Height - 1), Material.Crystal));
            Ore(Phase01Layout.BlueBand, 123, 124, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 118, 128, PropLayer.Middle);
            Ore(Phase01Layout.BlueBand, 123, 132, PropLayer.Middle);
            Shine(121.5f, 126f, 6f, 0.8f, Glow.Crystal);
            Shine(121.5f, 134f, 6f, 1.0f, Glow.Crystal);

            Tell(LoreKind.Plaque, room, "Lore_ElevatorSign", "a placa do elevador", 126, 100,
                "Uma placa de latão ao lado da cabine: \"ELEVADOR DE SERVIÇO — DO DISTRITO DE LATÃO AO LABORATÓRIO.\"");

            Decor(FixtureKind.Sign, 119, 107);
            Fix(FixtureKind.GaugeBig, "Gauge_Station_1", 124, 104);
            Fix(FixtureKind.GaugeBig, "Gauge_Station_2", 110, 104);
            Decor(FixtureKind.Barrel, 128, 100);
            Decor(FixtureKind.Barrel, 129, 100);
            Put(Crate(0), 106, 100);
            Put(Crate(2), 107, 100);
            Put(Crate(1), 106, 101);
            Lamp(112, 110);
            Lamp(126, 110);
            HangChain(110, 110, 3);
            Furnace("Furnace_Station", 130, 100);

            RunPipe(PipeLayer.Back, true, (102, 106), (134, 106));
            RunPipe(PipeLayer.Back, false, (119, 99), (119, 112));
            Steam(room, Area.Between(104, 100, 132, 110), 0.08f);
        }
    }
}
