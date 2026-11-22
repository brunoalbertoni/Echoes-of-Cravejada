using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase02Layout
    {
        // ------------------------------------------------------------------
        // 1. A Galeria de Transicao
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde o Scrap chega do District 1, por um poco gradeado. A metade
        /// oeste ainda e a rocha cor de ferrugem do Scrap Pit; a leste o latao
        /// comeca, com o tablado onde ele ganha o primeiro polimento. Um bloater
        /// sozinho na saida ensina o primeiro corrompido do distrito.
        /// </summary>
        private static void BuildGallery()
        {
            const string room = "01_Gallery";
            Room(room, 3, 6, 37, 13);

            MaterialZones.Add((Area.Between(0, 0, 18, 20), Material.Rock));

            Carve(3, 6, 36, 13);
            Solid(3, 11, 9, 13);
            Solid(16, 13, 21, 13);

            // O poco por onde se chega, ja com a grade fechada por cima.
            Carve(7, 1, 9, 5);
            Grate(7, 9, 5);
            Shine(8.5f, 3f, 6f, 0.8f, Glow.Oxide);
            Leaks.Add(new Leak(8.5f, 5f, 0f, 1f));
            Hazes.Add(new Haze("Oxide_Gallery", 12f, 7f, 16f, 2.5f, 0.14f, 1.1f, false, Glow.Oxide));

            // O tablado do polimento, com dois lampioes: a primeira luz limpa.
            Solid(22, 6, 27, 6);
            Mark(room, "Tarnished_1", 23, 7);
            Mark(room, "Tarnished_2", 26, 7);
            Lamp(23, 13);
            Lamp(26, 13);
            Decor(FixtureKind.Sign, 23, 10);

            // A saida pros Primeiros Canos.
            Carve(37, 6, 37, 9);

            Enemy(EnemyKind.AcidBloater, "Bloater_Gallery", 28, 46, 6, start: 32);

            // Lado da sucata.
            Put(GreyRock(3), 4, 6);
            Put(Debris(1), 5, 6);
            Put(Crate(1), 11, 6);
            Put(Crate(0), 14, 6);
            Put(Crate(2), 14, 7);
            Put(Debris(0), 15, 6);
            Put(Fence(0), 17, 6);
            Put(Fence(1), 18, 6);
            Put(DarkRock(1), 12, 6, PropLayer.Front);
            Put(DarkWeed(2), 19, 6, PropLayer.Front);
            Ore(Phase01Layout.RustBand, 15, 9, PropLayer.Back);
            Ore(Phase01Layout.GreyBand, 1, 8, PropLayer.Middle);

            // Lado do latao.
            Furnace("Furnace_Gallery", 35, 6);
            Fix(FixtureKind.GaugeBig, "Gauge_Gallery", 30, 10);
            Decor(FixtureKind.Barrel, 20, 6);
            Decor(FixtureKind.Barrel, 21, 6);
            HangChain(31, 13, 3);

            RunPipe(PipeLayer.Back, false, (19, 12), (37, 12));
            RunPipe(PipeLayer.Back, true, (28, 12), (28, 7));
            RunPipe(PipeLayer.Mid, false, (33, 13), (33, 10), (37, 10));
            Steam(room, Area.Between(19, 6, 36, 13), 0.07f);
            DripsIn(Area.Between(19, 6, 36, 13), 6, 11, false);
        }

        // ------------------------------------------------------------------
        // 2. Os Primeiros Canos
        // ------------------------------------------------------------------

        /// <summary>
        /// Um poco de tres colunas de vapor, alternando de lado: ensina a subir
        /// no vapor, sem perigo. No alto, uma prateleira leva pra Valvula
        /// Emperrada. A coluna que continua subindo (ate o Sotao) so liga com a
        /// valvula de la: e um atalho de volta, nao um caminho.
        /// </summary>
        private static void BuildFirstPipes()
        {
            const string room = "02_FirstPipes";
            Room(room, 38, 6, 46, 39);

            Carve(38, 6, 46, 39);

            Lift("Lift_FirstPipes_A", 38, 40, 6, 16);
            PipeFloor(43, 46, 14);
            Rail(43, 46, 15);

            Lift("Lift_FirstPipes_B", 44, 46, 15, 26);
            PipeFloor(38, 41, 23);
            Rail(38, 41, 24);

            Lift("Lift_FirstPipes_C", 38, 40, 24, 39);

            // A prateleira de cima, na altura da porta da Valvula Emperrada.
            Solid(42, 33, 46, 35);


            Fix(FixtureKind.GaugeSmall, "Gauge_FirstPipes_A", 45, 9);
            Fix(FixtureKind.GaugeSmall, "Gauge_FirstPipes_B", 39, 19);
            Fix(FixtureKind.GaugeSmall, "Gauge_FirstPipes_C", 45, 28);
            Lamp(44, 39, 0.6f);

            RunPipe(PipeLayer.Mid, false, (36, 8), (38, 8), (38, 5));
            RunPipe(PipeLayer.Mid, false, (48, 17), (46, 17), (46, 14));
            RunPipe(PipeLayer.Mid, false, (36, 25), (38, 25), (38, 23));
            RunPipe(PipeLayer.Back, true, (42, 6), (42, 13));
            RunPipe(PipeLayer.Back, false, (43, 16), (43, 32));
            Steam(room, Area.Between(38, 30, 46, 39), 0.12f);
        }

        // ------------------------------------------------------------------
        // 3. A Valvula Emperrada
        // ------------------------------------------------------------------

        /// <summary>
        /// Uma sala de bombas cortada por uma poca de acido larga demais pro
        /// dash. A valvula fica antes dela, a vista: girar seca a poca. Do
        /// outro lado, uma sentinela atira por cima do buraco, uma sanguessuga
        /// espera no teto, e numa prateleira a primeira pagina do livro de
        /// turnos — o encanador, na Cisterna, vai querer de volta.
        /// </summary>
        private static void BuildStuckValve()
        {
            const string room = "03_StuckValve";
            Room(room, 47, 36, 80, 44);

            Carve(47, 36, 80, 44);
            Solid(47, 43, 52, 44);
            Solid(74, 44, 80, 44);

            // A poca, com um degrau de cada lado pra descer depois de seca.
            Carve(55, 32, 66, 35);
            Solid(55, 32, 56, 33);
            Solid(65, 32, 66, 33);
            Acid("Acid_StuckValve", 57, 32, 64, 33);

            Valve(room, "Valve_StuckValve", 48, 38, 49, 36, "pool:Acid_StuckValve");
            RunPipe(PipeLayer.Mid, true, (49, 38), (49, 37), (58, 37), (58, 35));

            Enemy(EnemyKind.ValveSentry, "Sentry_StuckValve", 69, 69, 36, -1, 9);
            Leech("Leech_StuckValve", 72, 44, 36, 71, 75);

            // A prateleira da pagina, e a porta pra Praca.
            Solid(76, 36, 80, 37);
            Logbook(room, 1, "página do livro de turnos", 79, 38,
                "LIVRO DE TURNOS DA CALDEIRA — turno da noite.",
                "A pressão subiu de novo. Abri a válvula de alívio duas vezes, e ela voltou a subir.",
                "O vapor das caldeiras de baixo tá saindo esverdeado. Avisei lá em cima. Disseram que é impressão minha.");
            Carve(81, 38, 81, 41);

            Fix(FixtureKind.GaugeBig, "Gauge_StuckValve", 52, 40);
            Decor(FixtureKind.Sign, 59, 41);
            Furnace("Furnace_StuckValve", 77, 38);
            Decor(FixtureKind.Barrel, 53, 36);
            Decor(FixtureKind.Barrel, 67, 36);
            Lamp(50, 42);
            Lamp(70, 44);
            Lamp(78, 43, 0.6f);

            Fumes("StuckValve", Area.Between(57, 32, 64, 34));
            RunPipe(PipeLayer.Back, false, (47, 42), (81, 42));
            RunPipe(PipeLayer.Back, true, (61, 42), (61, 37));
            RunPipe(PipeLayer.Back, false, (68, 42), (68, 38), (72, 38));
            RunPipe(PipeLayer.Mid, true, (60, 46), (60, 43), (64, 43), (64, 46));
            Steam(room, Area.Between(47, 36, 80, 44));
            DripsIn(Area.Between(55, 36, 66, 44), 3, 31, true);
        }

        // ------------------------------------------------------------------
        // 4. A Cisterna de Crisol
        // ------------------------------------------------------------------

        /// <summary>
        /// A sala segura, embaixo da Praca: desce-se por uma escada de canos
        /// no chao dela. A bancada, Crisol e o estoque dele, o alcapao da Ala
        /// Baixa, e o cofre do velho encanador atras de uma parede rachada —
        /// a caldeira corrompida do lado ensina que elas derrubam parede.
        /// </summary>
        private static void BuildCistern()
        {
            const string room = "04_Cistern";
            Room(room, 84, 25, 112, 31);

            Carve(84, 25, 112, 31);
            Solid(84, 30, 86, 31);
            Solid(110, 30, 112, 31);

            // A escada de canos que desce da Praca.
            Carve(94, 25, 99, 33);
            PipeFloor(98, 99, 27);
            PipeFloor(94, 95, 29);
            PipeFloor(98, 99, 31);
            Fix(FixtureKind.Hatch, "Hatch_Cistern", 92, 25);

            Bench(room, "Workbench_Cistern", 85, 25);
            Mark(room, "Crisol", CrisolTile.X, CrisolTile.Y);

            // O estoque de Crisol, no canto dele.
            Decor(FixtureKind.Shelf, 87, 25);
            Put(Crate(0), 90, 25);
            Put(Crate(2), 90, 26);

            // O alcapao da Ala Baixa: so desce.
            Carve(102, 15, 103, 24);
            Decor(FixtureKind.Barrel, 100, 25);

            // A caldeira corrompida, a parede rachada e o cofre.
            PressureBoiler(room, "Boiler_PlumberVault", 105, 25, 104, 25, "Wall_PlumberVault");
            Cracked("Wall_PlumberVault", 113, 25, 113, 27);
            Carve(114, 25, 121, 29);
            Put(Crate(3), 120, 25);
            Put(Crate(0), 121, 25);
            Put(Crate(2), 121, 26);
            Put(Debris(1), 115, 25);
            Lamp(117, 29, 0.6f);

            OldPlumber(room, 118, 25);

            Lamp(89, 31, 1f);
            Lamp(108, 31, 0.8f);
            Fix(FixtureKind.GaugeSmall, "Gauge_Cistern", 101, 28);

            RunPipe(PipeLayer.Back, false, (83, 30), (113, 30));
            RunPipe(PipeLayer.Back, true, (101, 30), (101, 27));
            RunPipe(PipeLayer.Back, true, (116, 28), (122, 28));
        }

        /// <summary>
        /// O velho encanador: conta por que sobrou operario aqui embaixo e, na
        /// segunda conversa, entrega uma das duas gemas do distrito. As folhas
        /// do livro de turnos dele estao espalhadas pelo mapa, so pra ler.
        /// </summary>
        private static void OldPlumber(string room, int x, int y)
        {
            Tell(LoreKind.Tarnished, room, "Lore_OldPlumber", "o velho encanador", x, y,
                "...Não bate! Eu ouvi a caldeira estourar. Achei que era o fim... mas era só você abrindo o cofre.",
                "Eu sou encanador. Era. Tô trancado aqui desde que o verde subiu pelos canos.",
                "O Óxido veio de baixo, misturado no vapor das caldeiras. Quem respirava aquele vapor ia ficando verde por dentro.",
                "Sabe por que ainda sobrou operário aqui? Por causa das máscaras de solda. Elas filtram o ar, e o Óxido não passa por elas.",
                "Ninguém deu máscara pra gente por bondade. Era só equipamento de serviço. E foi isso que salvou a gente.",
                "Os de prata, lá em cima, nunca precisaram de máscara... até precisarem.",
                "Você me tirou daqui. Não tenho prata pra te dar, mas tenho isso: uma gema que o chefe de manutenção me deu no dia em que se trancou com as caldeiras.",
                "Ele disse pra guardar até alguém merecer. Acho que é você.",
                "Se achar folhas do meu livro de turnos pelos canos, lê. Alguém tem que saber o que aconteceu aqui embaixo.")
                .Quest = new QuestSpec
            {
                // Nada a buscar: a gema vem na primeira conversa, por ter sido libertado.
                ItemPrefix = LogbookPrefix,
                ItemCount = 0,
                PurifiedGreeting = "O verde foi embora dos canos. Pela primeira vez em anos, abri o respiro da máscara.",
                Progress = new string[0][],
                Complete = new string[0],
                Done = new[]
                {
                    "Achou alguma folha do meu livro? Faz tempo que não vejo a minha própria letra.",
                },
            };
        }

        // ------------------------------------------------------------------
        // 5. A Praca dos Operarios
        // ------------------------------------------------------------------

        /// <summary>
        /// O centro do distrito: o salao onde os operarios trocavam de turno.
        /// Armarios, o cabide das mascaras, as pichacoes. Chega-se por um
        /// balcao a oeste, vindo da Valvula Emperrada.
        ///
        /// Daqui sai tudo: a coluna do balcao sobe pra Casa de Maquinas (Ala
        /// Alta); a escada no chao desce pra Cisterna e, dela, pro Esgoto (Ala
        /// Baixa); a porta leste da na Cadeia de Vapor (Ala Leste). E, a vista
        /// desde o balcao, a Escadaria das Tres Travas sobe pro boss.
        /// </summary>
        private static void BuildWelderHall()
        {
            const string room = "05_WelderHall";
            Room(room, 82, 34, 130, 46);

            Carve(82, 34, 130, 46);

            // O balcao de chegada, e o degrau que desce pro salao.
            Solid(82, 34, 89, 37);
            Solid(90, 34, 91, 35);
            Rail(86, 89, 38);

            // Ala Alta: a coluna do balcao, que atravessa o teto. Ocupa so
            // metade do buraco: pela outra metade se desce de volta.
            Carve(83, 47, 86, 51);
            Lift("Lift_ToMachineHall", 83, 84, 38, 53);

            // Ala Baixa: o buraco no chao por onde a coluna do Esgoto devolve
            // quem sobe de la. (A entrada da ala e o alcapao da Cisterna.)

            // Ala Leste: a porta.
            Carve(131, 34, 131, 38);
            Decor(FixtureKind.Sign, 127, 39);

            // O caminho por cima dos canos, ate a pichacao do alto.
            PipeFloor(100, 104, 36);
            PipeFloor(106, 110, 38);
            PipeFloor(112, 115, 40);
            Rail(112, 115, 41);

            // --- A Escadaria das Tres Travas ---
            //
            // Um poco que sobe do teto da Praca ate a sala antes da arena, com
            // uma coluna sempre ligada do lado oeste e tres grades atravessadas.
            // Cada grade abre com a valvula do fim de uma ala: foram os operarios
            // que trancaram a porta por fora, como o chefe pediu. O lado leste do
            // poco fica livre pra descer de volta.
            Carve(118, 47, 122, 58);
            Lift("Lift_LockStair", 118, 119, 34, 60);
            Gate("Trava_Alta", 118, 49, 122, 49, GateOpener.Valve, 1, 0);
            Gate("Trava_Baixa", 118, 53, 122, 53, GateOpener.Valve, 1, 0);
            Gate("Trava_Leste", 118, 57, 122, 57, GateOpener.Valve, 1, 0);

            // Os escombros da escadaria la de cima, caidos no pe do poco.
            Put(Debris(0), 116, 34);
            Put(Debris(1), 117, 34);
            Put(Crate(3), 121, 34);
            Put(Debris(0), 122, 34);
            Put(Debris(1), 123, 35, PropLayer.Front);
            Fixtures.Add(new Fixture(FixtureKind.Railing, DecorName("RailFallen"), 120, 34, 2));

            // O painel: um manometro por trava, do lado do poco.
            Fix(FixtureKind.GaugeBig, "Gauge_Lock_Alta", 123, 44);
            Fix(FixtureKind.GaugeBig, "Gauge_Lock_Baixa", 125, 44);
            Fix(FixtureKind.GaugeBig, "Gauge_Lock_Leste", 127, 44);
            Shine(120.5f, 47f, 5f, 0.8f, Glow.Furnace, true);
            Shine(120.5f, 55f, 5f, 0.8f, Glow.Furnace, true);

            // O vestiario: armarios, o cabide das mascaras com uma so, e as
            // pichacoes de quem ficou.
            Lockers(106, 34, 6);
            Decor(FixtureKind.MaskRack, 86, 40);
            Decor(FixtureKind.Poster, 123, 39);
            Decor(FixtureKind.Sign, 104, 43);

            Tell(LoreKind.Graffiti, room, "Lore_Graffiti_1", "a pichação", 92, 34,
                "Riscado na chapa, em letra de quem tinha pressa:",
                "\"A PRATA BEBE, O LATÃO SUA.\"");

            Tell(LoreKind.GraffitiMask, room, "Lore_Graffiti_2", "o desenho", 88, 38,
                "Um desenho feito a giz, do lado do cabide: uma máscara de soldador.",
                "Embaixo, alguém escreveu: \"QUEM TEM MÁSCARA RESPIRA.\"");

            Tell(LoreKind.Graffiti, room, "Lore_Graffiti_3", "a pichação", 114, 41,
                "Lá no alto, onde só chega quem anda pelos canos:",
                "\"ELES NUNCA DESCERAM ATÉ AQUI. NEM PARA VER.\"",
                "E, mais abaixo, outra mão, de outro dia: \"DAQUI EU VIA AS LUZES DA AVENIDA DAS FACETAS PELO RESPIRO DO TETO. APAGARAM-SE UMA A UMA.\"")
                .PurifiedLines = new[]
            {
                "Lá no alto, onde só chega quem anda pelos canos:",
                "\"ELES NUNCA DESCERAM ATÉ AQUI. NEM PARA VER.\"",
                "Embaixo, em giz novo, numa letra que ainda treme: \"Um desceu. Era de sucata.\"",
            };

            // Depois da purificacao, uma soldadora tira a mascara aqui, na
            // frente dos armarios: e quem conta quem era o Mestre das Caldeiras.
            LoreSpec welder = Tell(LoreKind.Tarnished, room, "Npc_Solda", "Solda", 106, 34,
                "Pode chegar. Tirei a máscara. É a primeira vez que respiro sem ela desde que vim soldar aqui embaixo.",
                "Você derrubou o Mestre das Caldeiras? ...Então precisa saber quem ele era. Era o nosso chefe de manutenção.",
                "Quando a Coroa mandou recolher nossas máscaras pros de prata, foi ele que escondeu as caixas e jurou que não tinha nenhuma. Sem elas, o Óxido tinha pegado todo mundo.",
                "Depois ele se trancou com as caldeiras pra manter a pressão. Sem vapor, a gente congelava aqui embaixo antes de o Óxido chegar.",
                "Ele salvou a gente duas vezes. Na segunda, já nem sabia quem era. Obrigada por deixar ele descansar.");
            welder.AfterPurification = true;

            Enemy(EnemyKind.BrassWelder, "Welder_Hall_1", 100, 125, 34, start: 106);
            Enemy(EnemyKind.BrassWelder, "Welder_Hall_2", 100, 125, 34, start: 115);
            Leech("Leech_Hall", 124, 46, 34, 100, 125);

            Boiler("Boiler_Hall_West", 100, 34);
            Furnace("Furnace_Hall", 122, 34);
            Lamp(90, 46);
            Lamp(100, 46);
            Lamp(110, 46);
            Lamp(126, 46);
            HangChain(96, 46, 4);
            HangChain(116, 46, 3);

            RunPipe(PipeLayer.Back, false, (82, 44), (117, 44));
            RunPipe(PipeLayer.Back, true, (97, 44), (97, 36));
            RunPipe(PipeLayer.Back, false, (123, 42), (131, 42));
            RunPipe(PipeLayer.Mid, false, (111, 47), (111, 44), (117, 44));
            Leaks.Add(new Leak(97.5f, 42f, 1f, 0f));
            Steam(room, Area.Between(82, 34, 130, 46), 0.10f);
            DripsIn(Area.Between(90, 34, 117, 46), 8, 51, false);
        }

        // ------------------------------------------------------------------
        // 6. A Cadeia de Vapor (Ala Leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Um lago de acido que se cruza no vapor: coluna, ilha, jato, ilha,
        /// coluna que pulsa e o patamar do outro lado, com a bancada da ala e a
        /// porta do Labirinto. A volta e por baixo, por canos baixos ate a
        /// primeira coluna.
        ///
        /// O segredo da ala — e uma das duas gemas do distrito — fica num
        /// nicho no alto da parede oeste, em cima da porta: so se chega
        /// saindo do topo da coluna que pulsa e emendando dash em duas nuvens.
        /// </summary>
        private static void BuildSteamChain()
        {
            const string room = "06_SteamChain";
            Room(room, 132, 24, 162, 48);

            Carve(132, 24, 162, 48);
            Voids.Add(Area.Between(132, 24, 162, 48));

            Solid(132, 24, 135, 33);
            Solid(159, 24, 162, 43);
            Acid("Acid_SteamChain", 136, 24, 158, 26);

            // A travessia.
            Lift("Lift_Chain_1", 136, 138, 27, 40);
            PipeFloor(140, 143, 36);
            Rail(140, 143, 37);
            Jet("Jet_Chain", 144, 150, 37, 40, 1);
            PipeFloor(150, 154, 33);
            Rail(150, 154, 34);

            Vent pulsing = Lift("Lift_Chain_2", 155, 157, 27, 47);
            pulsing.OnTime = 2.6f;
            pulsing.OffTime = 1.3f;

            // A volta, por baixo: do patamar se cai na ilha do jato, e dali num
            // cano baixo ate a primeira coluna.
            PipeFloor(144, 147, 30);

            // Rente ao acido, uma ilha de respiro pra quem cai — e pra quem desce
            // de proposito.
            PipeFloor(146, 148, 27);
            Tell(LoreKind.GraffitiMask, room, "Lore_LakeMask", "uma máscara de solda", 147, 28,
                "Uma máscara de solda, rachada ao meio, presa entre o cano e a grade.",
                "Por dentro, gravado a prego, em letra pequena: \"PARA A PEQUENA, QUANDO CRESCER.\"",
                "A máscara é do tamanho de uma cabeça de criança.");

            // O patamar, a bancada da ala e a porta do Labirinto.
            Bench(room, "Workbench_Chain", 159, 44);
            Carve(163, 44, 163, 47);

            // O segredo: duas nuvens no alto e o nicho em cima da porta oeste.
            Cloud("Cloud_Chain_1", 149, 44, 151, 47);
            Cloud("Cloud_Chain_2", 141, 44, 143, 47);
            PipeFloor(132, 135, 44);
            Reward(room, RewardKind.Gem, "D2_Chain_Gem", 133, 45);
            Tell(LoreKind.Page, room, "Lore_Letter", "uma carta dobrada", 135, 45,
                "Uma carta dobrada em quatro, presa atrás de um cano com um rebite.",
                "\"Pequena, o vapor está mais pesado esta semana. Não venha me visitar no turno. Fique no distrito de cobre, perto das janelas.\"",
                "\"Se mandarem todo mundo descer pela calha, não tenha medo. Eu desço os canos atrás de você. Prometo.\"",
                "A carta nunca foi enviada.");

            Enemy(EnemyKind.ValveSentry, "Sentry_Chain_2", 143, 143, 37, -1, 5);
            Enemy(EnemyKind.AcidBloater, "Bloater_Chain", 150, 154, 34, start: 153);

            Decor(FixtureKind.Sign, 133, 40);
            Lamp(160, 48);
            HangChain(139, 48, 5);
            HangChain(153, 48, 4);

            RunPipe(PipeLayer.Mid, true, (137, 22), (137, 26));
            RunPipe(PipeLayer.Mid, true, (156, 22), (156, 26));
            RunPipe(PipeLayer.Mid, false, (143, 49), (143, 41), (144, 41));
            RunPipe(PipeLayer.Far, false, (66, 18), (81, 18));
            RunPipe(PipeLayer.Far, true, (70, 11), (70, 24));
            RunPipe(PipeLayer.Far, true, (78, 11), (78, 24));

            Fumes("SteamChain", Area.Between(136, 24, 158, 26), 0.18f);
            Hazes.Add(new Haze("Steam_SteamChain", 147f, 40f, 28f, 10f, 0.06f, 2.4f, false, Glow.Steam));
            Skylines.Add(new Skyline("Skyline_Chain_Far", 25f, false, 3f, 0.25f));
            Skylines.Add(new Skyline("Skyline_Chain_Near", 23f, true, 2f, 0.5f));
            DripsIn(Area.Between(136, 27, 158, 48), 5, 61, true);
        }

        // ------------------------------------------------------------------
        // 7. O Labirinto de Valvulas (Ala Leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Tres valvulas em sequencia, cada uma abrindo a proxima, todas a
        /// vista. Entra-se pelo alto de um poco e cai-se no corredor de baixo:
        ///   A, no pe do poco, abre o portao do corredor;
        ///   B, no fim do corredor, liga a coluna pro andar do meio;
        ///   C, no andar do meio, e a da trava.
        /// Um buraco no andar do meio devolve ao corredor, e a coluna do poco
        /// (sempre ligada) leva de volta a porta: da pra sair a qualquer hora.
        /// O corredor de baixo e onde os colegas do livro de turnos sumiram.
        /// </summary>
        private static void BuildValveMaze()
        {
            const string room = "07_ValveMaze";
            Room(room, 164, 27, 185, 47);

            // O poco de entrada: a porta da no alto dele, sobre um degrau. Uma
            // coluna sempre ligada sobe o poco: e por ela que se sai, a
            // qualquer hora.
            Carve(164, 27, 170, 47);
            Solid(164, 43, 165, 43);
            Lift("Lift_Maze_Elevator", 168, 170, 27, 46);

            // Corredor de baixo, o portao no meio, e o beco da valvula B.
            Carve(171, 27, 185, 31);
            Gate("Gate_Maze", 176, 27, 176, 31, GateOpener.Valve);
            Valve(room, "Valve_Maze_A", 166, 29, 167, 27, "gate:Gate_Maze");
            Valve(room, "Valve_Maze_B", 184, 29, 185, 27, "vent:Lift_Maze_Up");
            Carve(181, 32, 183, 35);
            Lift("Lift_Maze_Up", 181, 183, 27, 38).StartsOff = true;

            Logbook(room, 3, "página do livro de turnos", 172, 27,
                "LIVRO DE TURNOS DA CALDEIRA — a letra tá tremida.",
                "Três colegas não voltaram do turno. Fui atrás deles no corredor de baixo, onde o vapor é mais grosso.",
                "Achei só as ferramentas. Estavam cobertas de Óxido.");

            // Andar do meio: a valvula da trava, e um buraco no chao que devolve
            // ao corredor de baixo (o portao ja esta aberto quando se chega aqui).
            Carve(172, 36, 185, 41);
            Carve(175, 32, 176, 35);
            Valve(room, "Valve_Maze_C", 172, 38, 173, 36, "gate:Trava_Leste").Lock = true;

            Enemy(EnemyKind.AcidBloater, "Bloater_Maze", 177, 185, 27, start: 180);
            Leech("Leech_Maze", 180, 31, 27, 177, 185);
            Enemy(EnemyKind.BrassWelder, "Welder_Maze", 177, 180, 36);
            Enemy(EnemyKind.ValveSentry, "Sentry_Maze", 185, 185, 36, -1, 4);

            Decor(FixtureKind.Sign, 166, 45);
            Fix(FixtureKind.GaugeBig, "Gauge_Maze_1", 168, 34);
            Fix(FixtureKind.GaugeBig, "Gauge_Maze_2", 182, 39);
            Decor(FixtureKind.Barrel, 174, 27);
            Lamp(167, 47, 0.7f);
            Lamp(180, 31, 0.5f);
            Lamp(183, 41, 0.6f);
            HangChain(169, 47, 6);

            Plumb(Area.Between(171, 27, 185, 31), 701, PipeLayer.Back, false, 0);
            Plumb(Area.Between(172, 36, 185, 41), 702, PipeLayer.Back, true, 0);
            RunPipe(PipeLayer.Mid, true, (170, 33), (164, 33));
            Leaks.Add(new Leak(170f, 33.5f, -1f, 0f));
            Steam(room, Area.Between(172, 36, 185, 41), 0.12f);
            DripsIn(Area.Between(171, 27, 185, 31), 5, 71, true);
        }

        // ------------------------------------------------------------------
        // 8. A sala antes da arena
        // ------------------------------------------------------------------

        /// <summary>
        /// No alto da Escadaria das Travas: a bancada de antes da luta e a
        /// porta da sala das caldeiras, trancada por fora. O poco da Escadaria
        /// abre no chao, e por ele se desce de volta pra Praca. Depois da
        /// purificacao, o atalho da Galeria Alta cai aqui.
        /// </summary>
        private static void BuildWorkbenchRoom()
        {
            const string room = "08_PreBoss";
            Room(room, 110, 59, 128, 65);

            Carve(110, 59, 128, 65);

            // Passagem pra arena, a leste. O selo fecha a coluna do lado dela.
            Carve(129, 59, 131, 63);

            Bench(room, "Workbench_PreBoss", 111, 59);
            Fix(FixtureKind.Hatch, "Hatch_PreBoss", 128, 59);

            // O poco da Escadaria nao acaba aqui: continua subindo ate a
            // estacao do elevador, la no alto. Mas a escadaria desabou — os
            // degraus que sobraram ficam pendurados, longe demais um do outro
            // pra subir, e os pedacos cairam aqui e na Praca.
            BrokenStairway();
            Fix(FixtureKind.GaugeBig, "Gauge_PreBoss", 124, 62);
            Decor(FixtureKind.Poster, 114, 62);

            Tell(LoreKind.Graffiti, room, "Lore_BoilerDoor", "a porta da sala das caldeiras", 126, 59,
                "A porta da sala das caldeiras tem uma tranca soldada pelo lado de fora.",
                "Riscado no latão, em letras de mãos diferentes: \"NÃO ABRAM.\" \"OUÇAM O QUE OUVIREM.\" \"ELE PEDIU.\"",
                "E, mais embaixo, mais fundo, como quem riscou muitas vezes no mesmo lugar: \"Desculpe, chefe.\"");

            // Os escombros da escada, no chao da antessala.
            Put(Debris(0), 114, 59);
            Put(Debris(1), 115, 59);
            Put(Crate(2), 116, 59);
            Put(Debris(0), 124, 59);
            Put(Crate(0), 125, 59);
            Put(Crate(1), 125, 60);
            Fixtures.Add(new Fixture(FixtureKind.Railing, DecorName("RailFallen"), 113, 59, 2));
            Lamp(111, 65);
            Lamp(127, 65, 0.6f);

            RunPipe(PipeLayer.Back, false, (108, 64), (131, 64));
            RunPipe(PipeLayer.Back, true, (121, 64), (121, 61));
        }

        /// <summary>
        /// A escadaria quebrada: o poco da Escadaria das Travas continua acima
        /// da antessala ate o piso da estacao do elevador. Sobraram quatro
        /// lances, alternando de lado, cada um cinco tiles acima do outro —
        /// nem com o Salto de Pressao se sobe por eles. Descer, da: e o atalho
        /// de volta de quem chega na estacao.
        /// </summary>
        private static void BrokenStairway()
        {
            Carve(114, 66, 126, 98);

            int[,] flights = { { 121, 125, 77 }, { 115, 119, 83 }, { 121, 125, 89 }, { 115, 119, 95 } };
            for (int i = 0; i < flights.GetLength(0); i++)
            {
                int x0 = flights[i, 0];
                int x1 = flights[i, 1];
                int y = flights[i, 2];

                PipeFloor(x0, x1, y);

                // O guarda-corpo, partido no meio de cada lance.
                Rail(x0, x0 + 1, y + 1);
                Put(Debris(i % 2), x1, y + 1);
            }

            // Os pedacos dos lances que faltam, pendurados nas paredes.
            HangChain(121, 98, 6);
            HangChain(124, 98, 9);
            HangChain(122, 88, 4);
            Lamp(126, 98, 0.5f);

            Fix(FixtureKind.GaugeBig, "Gauge_Stairway_1", 114, 79);
            Fix(FixtureKind.GaugeBig, "Gauge_Stairway_2", 115, 92);
            RunPipe(PipeLayer.Back, true, (113, 66), (113, 98));
            RunPipe(PipeLayer.Back, false, (127, 66), (127, 98));
            Hazes.Add(new Haze("Steam_Stairway", 120f, 94f, 12f, 5f, 0.07f, 1.5f, false, Glow.Steam));
            DripsIn(Area.Between(114, 66, 126, 98), 4, 141, false);
        }

        // ------------------------------------------------------------------
        // 9. A Sala de Caldeiras
        // ------------------------------------------------------------------

        private static void BuildBoilerRoom()
        {
            const string room = "09_BoilerRoom";
            Room(room, 132, 59, 178, 74);

            // Entra-se a oeste, vindo do pre-boss (o selo fecha x 131); sai-se a
            // leste, pro poco de fuga (a grade fica em x 180).
            Carve(132, 59, 178, 74);
            Voids.Add(Area.Between(132, 59, 178, 74));

            Solid(175, 73, 178, 74);
            Solid(177, 71, 178, 72);
            Solid(132, 73, 135, 74);
            Solid(132, 71, 133, 72);

            Carve(138, 53, 172, 57);
            Acid("Acid_BoilerRoom", 138, 53, 172, 56);
            Grate(138, 172, 58);

            PipeFloor(172, 173, 60);
            PipeFloor(137, 138, 60);
            PipeFloor(165, 170, 62);
            PipeFloor(140, 145, 62);
            PipeFloor(158, 162, 64);
            PipeFloor(148, 152, 64);
            PipeFloor(154, 156, 66);

            Carve(179, 59, 181, 62);

            Mark(room, "BossSpawn_BoilerMaster", BossTile.X, BossTile.Y);
            Mark(room, "Gate_Entry", 132, 59);

            Gate(BossGate, 180, 59, 180, 62, GateOpener.Boss);

            Boiler("Boiler_Arena_West", 142, 59);
            Boiler("Boiler_Arena_Center", 154, 59);
            Boiler("Boiler_Arena_East", 166, 59);

            Fix(FixtureKind.GaugeBig, "Gauge_Arena_West", 148, 69);
            Fix(FixtureKind.GaugeBig, "Gauge_Arena_East", 162, 69);
            HangChain(140, 74, 4);
            HangChain(170, 74, 4);

            RunPipe(PipeLayer.Mid, true, (168, 65), (168, 76));
            RunPipe(PipeLayer.Mid, false, (155, 65), (155, 68));
            RunPipe(PipeLayer.Mid, true, (143, 65), (143, 76));
            RunPipe(PipeLayer.Mid, false, (180, 72), (173, 72), (173, 69));
            RunPipe(PipeLayer.Mid, false, (130, 72), (135, 72), (135, 69));
            RunPipe(PipeLayer.Far, false, (66, 34), (89, 34));
            RunPipe(PipeLayer.Far, true, (71, 28), (71, 38));
            RunPipe(PipeLayer.Far, true, (84, 28), (84, 38));

            Fumes("BoilerRoom", Area.Between(138, 53, 172, 56), 0.14f);
            Hazes.Add(new Haze("Steam_BoilerRoom", 156f, 69f, 44f, 6f, 0.09f, 2.2f, false, Glow.Steam));
            Skylines.Add(new Skyline("Skyline_Arena_Far", 56f, false, 3f, 0.25f));
            Skylines.Add(new Skyline("Skyline_Arena_Near", 55.5f, true, 2f, 0.5f));
            DripsIn(Area.Between(138, 59, 172, 74), 7, 91, false);
        }

        // ------------------------------------------------------------------
        // O tunel da caldeira
        // ------------------------------------------------------------------

        /// <summary>
        /// Atras da grade leste da arena, que abre quando o Boiler Master cai
        /// (e o distrito se purifica): um tunel curto e, no fim dele, a coluna
        /// que sobe pra Galeria Alta. Dali em diante e so subir.
        /// </summary>
        private static void BuildExitTunnel()
        {
            Carve(182, 59, 188, 62);

            // A coluna ocupa so metade do poco: pela outra metade se desce.
            Carve(183, 63, 186, 77);
            Lift("Lift_ToHighGallery", 183, 184, 59, 80);

            Decor(FixtureKind.Sign, 183, 66);
            Decor(FixtureKind.Barrel, 187, 59);
            Lamp(187, 62, 0.6f);
            Shine(184f, 70f, 5f, 0.6f, Glow.Steam);
            RunPipe(PipeLayer.Back, false, (181, 62), (188, 62));
            RunPipe(PipeLayer.Back, true, (187, 63), (187, 77));
            Fix(FixtureKind.GaugeSmall, "Gauge_ExitTunnel", 182, 61);
        }
    }
}
