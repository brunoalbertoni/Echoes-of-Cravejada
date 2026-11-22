using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase04Layout
    {
        // ------------------------------------------------------------------
        // 1. O Saguao
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde a cabine do District 3 para. Um saguao de recepcao: o balcao,
        /// o cabide de jalecos, o portao de prata arrombado por dentro.
        ///
        /// A cabine chega de baixo: o poco desce do chao do saguao ate bem
        /// fundo, com os trilhos e uns lampioes de servico, e a camera sobe
        /// com o Scrap o caminho inteiro (ElevatorArrival).
        /// </summary>
        private static void BuildLobby()
        {
            const string room = "01_Lobby";
            Room(room, 3, 40, 30, 49);

            Carve(3, 40, 30, 49);

            // O poco. O chao da cabine, parada no andar, tampa a boca dele.
            (int shaftX, int bottom, int stop, int _) = ElevatorShaft;
            Carve(shaftX, bottom, shaftX + 2, stop - 2);
            for (int x = shaftX; x <= shaftX + 2; x++) ObjectCells.Add((x, stop - 1));

            for (int y = bottom + 5; y < stop - 2; y += 8) Lamp(shaftX + 1.5f, y, 3.5f);

            Tell("sign", room, "Lore_Portal", "a placa do portão", 20, 40,
                "O portão era de prata maciça. Alguém arrombou ele por dentro.",
                "Na placa, em letra de ourives: \"ATELIÊS ALQUÍMICOS DE SUA MAJESTADE. ENTRADA PROIBIDA A METAIS COMUNS.\"");

            Put("lab_coats", 14.5f, 40);
            Put("lab_lectern", 26f, 40);
            Put("lab_shelf", 29f, 40, 4);
            Hang("lab_chandelier", 17f, 50);

            Windows.Add((0, 23, 44));

            Lamp(13f, 45f, 8f);
            Lamp(24f, 45f, 8f);
        }

        // ------------------------------------------------------------------
        // 2. A Galeria dos Mestres
        // ------------------------------------------------------------------

        private static void BuildGallery()
        {
            const string room = "02_MastersGallery";
            Room(room, 31, 42, 63, 50);

            Carve(31, 42, 58, 50);

            // A porta do atelie: baixa, com o batente de marmore.
            Carve(59, 42, 63, 44);

            Tell("diary", room, "Lore_Diary_1", "diário de laboratório, folha do dia 12", 55, 42,
                "\"Dia 12. Descoberta: o mercúrio retarda a corrosão do Óxido. Mergulhei uma placa de prata no mercúrio e deixei do lado de outra, seca, no mesmo vapor verde.\"",
                "\"A seca começou a manchar no primeiro dia. A mergulhada aguentou três dias a mais. É pouco, mas é a primeira coisa que funciona.\"",
                "\"Sua Majestade foi informada. Há esperança. Vamos curar a cidade inteira.\"",
                "A letra é caprichada, de quem tá feliz.");

            Enemy(FoeKind.Slime, "Slime_Gallery", 31, 63, 42);

            Put("lab_bust", 32.5f, 42, 4);
            Put("lab_rack", 58f, 42, 4);
            Hang("lab_chandelier", 42.5f, 51);

            Lamp(37.5f, 47f, 7f);
            Lamp(47.5f, 47f, 7f);
            Lamp(56f, 46f, 5f);
        }

        // ------------------------------------------------------------------
        // 3. O Atelie de Crisol (o centro)
        // ------------------------------------------------------------------

        /// <summary>
        /// O centro: a bancada, Crisol e o Grande Alambique. Daqui saem as tres
        /// alas — a chamine da Optica no canto oeste, o poco da Destilaria no
        /// chao e a porta da Incubacao a leste — e, pelo mezanino, a grade do
        /// alambique, que leva ao chefe.
        /// </summary>
        private static void BuildAtelier()
        {
            const string room = "03_Atelier";
            Room(room, 64, 40, 100, 54);

            Carve(64, 40, 100, 54);

            // A chamine da Optica, no canto oeste: a parede de leste desce do teto
            // e deixa o pe livre pra passar.
            Solid(68, 44, 68, 54);
            Carve(64, 55, 67, 60);

            Bench(room, "Workbench_Atelier", 70, 40);
            Mark(room, "Crisol", CrisolTile.X, CrisolTile.Y);

            // O poco de servico pra Destilaria, no chao: cai no Salao dos Alambiques.
            Carve(80, 31, 83, 39);

            // O mezanino do alambique: um degrau e o patamar ate a grade.
            Ledge(86, 88, 43);
            Ledge(91, 100, 47);

            Mark(room, "Alembic", AlembicTile.X, AlembicTile.Y);
            Put("lab_great_alembic", 95.5f, 40, 3);

            // A porta da Incubacao, a leste, um degrau acima.
            Carve(101, 41, 104, 44);

            Put("lab_lens_bench", 79f, 40, 4);
            Put("lab_retort", 85.5f, 40, 5);
            Put("lab_shelf", 99f, 40, 4);
            Hang("lab_chandelier", 82f, 55);

            Lamp(71f, 44f, 8f);
            Lamp(78f, 45f, 8f);
            Lamp(94f, 46f, 7f, true);
        }

        // ------------------------------------------------------------------
        // 4. A Destilaria (embaixo)
        // ------------------------------------------------------------------

        /// <summary>
        /// Tres andares de laboratorio embaixo do atelie, feitos pra dar a volta:
        ///
        ///   a o Salao dos Alambiques, onde o poco de servico cai. Dali so se
        ///     sai pela porta oeste (e de volta pelo poco, quicando nas paredes).
        ///   b a Serpentina, a oeste: desce em zigue-zague pelas prateleiras,
        ///     com goteiras de mercurio e cacos no chao.
        ///   c os Condensadores, no fundo: corredor baixo de canos, de volta pro
        ///     leste, com tres fossos (o ultimo com um pilar no meio).
        ///   d a Torre do Grande Destilador, a leste: sobe-se por patamares ate
        ///     o Mercurio Destilado, no alto.
        ///
        /// Do alto da torre, uma passarela volta ao salao por cima, num
        /// patamar que de baixo e alto demais pra alcancar: o atalho de volta.
        /// </summary>
        private static void BuildDistillery()
        {
            const string hall = "04a_StillHall";
            const string serpentine = "04b_Serpentine";
            const string condensers = "04c_Condensers";
            const string tower = "04d_GreatStill";
            Room(hall, 48, 18, 100, 30);
            Room(serpentine, 16, 6, 47, 22);
            Room(condensers, 47, 1, 111, 12);
            Room(tower, 101, 6, 132, 31);

            // a. O Salao dos Alambiques.
            Carve(48, 18, 100, 30);

            // As paredes do poco descem dentro do salao e param a quatro tiles do
            // chao: da pra passar por baixo, e de baixo pular pra dentro do poco.
            Solid(79, 22, 79, 30);
            Solid(84, 22, 84, 30);

            Ledge(60, 63, 21);
            Enemy(FoeKind.FlaskThrower, "Thrower_Hall", 61, 61, 22);
            Enemy(FoeKind.FailedExperiment, "Failed_Hall", 38, 100, 18, 72);

            // O patamar do atalho, a leste: so se chega por cima, pela passarela.
            Ledge(92, 100, 25);

            // A porta oeste, pra Serpentina.
            Carve(47, 18, 47, 21);

            Put("lab_still", 52f, 18, 3);
            Put("lab_alembic", 56.5f, 18);
            Put("lab_burner", 66f, 18, 4);
            Put("lab_still", 71f, 18, 3);
            Put("lab_retort", 76.5f, 18, 4);
            Put("lab_rack", 88f, 18, 4);
            Put("lab_alembic", 92.5f, 18);
            Put("lab_still", 97f, 18, 3);
            Hang("lab_pipes", 58f, 31);
            Hang("lab_pipes", 70f, 31);
            Hang("lab_pipes", 95f, 31);

            Lamp(54f, 23f, 7f);
            Lamp(66f, 24f, 7f, true);
            Lamp(81.5f, 23f, 6f);
            Lamp(94f, 22f, 7f, true);
            Lamp(97f, 28f, 5f);

            // b. A Serpentina: entra-se numa prateleira alta e desce-se em
            // zigue-zague. No chao, uma faixa de cacos de vidro.
            Carve(16, 6, 46, 22);
            Solid(38, 14, 46, 17);
            Ledge(29, 34, 14);
            Ledge(19, 24, 10);
            Spikes(28, 34, 6);

            Drips.Add((22, 22, 11));
            Drips.Add((31, 22, 7));
            Drips.Add((41, 22, 3));

            Tell("chalk", serpentine, "Lore_Chalk_Mercury", "um quadro-negro", 43, 18,
                "Um quadro-negro cheio de contas. No meio, sublinhado três vezes: \"PRATA + MERCÚRIO = PRATA QUE NÃO APODRECE?\"");

            Enemy(FoeKind.FlaskThrower, "Thrower_Serpentine", 32, 32, 15);
            Enemy(FoeKind.Slime, "Slime_Serpentine", 16, 27, 6, 20);

            Put("lab_shelf", 39.5f, 18, 3);
            Put("lab_jar_1", 21.5f, 11, 4);
            Put("lab_alembic", 18.5f, 6);
            Put("lab_still", 24f, 6, 3);
            Hang("lab_pipes", 26f, 23);
            Hang("lab_chandelier", 36f, 23);

            Lamp(42f, 21f, 6f);
            Lamp(31f, 18f, 6f, true);
            Lamp(21f, 14f, 6f);
            Lamp(31f, 8f, 6f, true);

            // c. Os Condensadores: corredor baixo, de volta pro leste.
            Carve(47, 6, 111, 12);

            Carve(56, 3, 60, 5);
            Spikes(56, 60, 3);
            Carve(70, 3, 74, 5);
            Spikes(70, 74, 3);

            // O fosso de descarte, com um pilar no meio: atravessa-se pulando.
            Carve(88, 1, 99, 5);
            Spikes(88, 99, 1);
            Solid(93, 1, 94, 5);

            Ledge(77, 80, 9);

            Drips.Add((64, 12, 6));
            Drips.Add((83, 12, 6));
            Drips.Add((97, 12, 6));
            Drips.Add((106, 12, 6));

            Tell("diary", condensers, "Lore_Diary_Mercury", "diário de laboratório, folha do dia 40", 66, 6,
                "\"Dia 40. Sete destilações seguidas. O mercúrio sai tão puro que reflete o rosto de quem olha, e no reflexo não aparece mancha nenhuma de Óxido.\"",
                "\"Efeito colateral: o vapor do mercúrio é venenoso. Os assistentes de cobre desmaiam perto dos alambiques. O Mestre respira fundo e diz que é perfume.\"",
                "Tem uma marca de dedo prateado no canto da folha.");

            Enemy(FoeKind.FailedExperiment, "Failed_Condensers", 35, 55, 6, 50);
            Enemy(FoeKind.Slime, "Slime_Condensers", 61, 69, 6);
            Enemy(FoeKind.FlaskThrower, "Thrower_Condensers", 78, 78, 10);
            Enemy(FoeKind.Slime, "Slime_Condensers_2", 75, 87, 6, 84);

            Put("lab_burner", 50f, 6, 4);
            Put("lab_jar_0", 64f, 6, 4);
            Put("lab_rack", 82f, 6, 4);
            Put("lab_jar_2", 104f, 6, 4);
            Hang("lab_pipes", 52f, 13);
            Hang("lab_pipes", 66f, 13);
            Hang("lab_pipes", 85f, 13);
            Hang("lab_pipes", 103f, 13);

            Lamp(52f, 10f, 6f);
            Lamp(65f, 10f, 6f, true);
            Lamp(80f, 10f, 6f);
            Lamp(93.5f, 5f, 5f, true);
            Lamp(105f, 10f, 6f);

            // d. A Torre do Grande Destilador: patamares alternados ate o alto.
            Carve(112, 6, 132, 31);
            Ledge(126, 130, 9);
            Ledge(118, 122, 13);
            Ledge(126, 130, 17);
            Ledge(118, 122, 21);

            // O alto: o bloco do Grande Destilador, com o reagente.
            Solid(124, 22, 132, 25);
            AddReagent(tower, "Mercury", "MERCÚRIO DESTILADO", 0, 130, 26);

            // O atalho: a passarela que volta ao salao, por cima.
            Ledge(112, 117, 25);
            Carve(101, 26, 111, 29);

            Enemy(FoeKind.FailedExperiment, "Failed_Tower", 100, 132, 6, 122);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tower", 128, 128, 18);
            Enemy(FoeKind.Sentinel, "Sentinel_GreatStill", 124, 132, 26);

            Put("lab_still", 115f, 6, 3);
            Put("lab_great_tank", 122f, 6, 2);
            Put("lab_alembic", 126.5f, 26);
            Put("lab_shelf", 108f, 26, 3);
            Hang("lab_pipes", 120f, 32);
            Hang("lab_chandelier", 127f, 32);

            Lamp(116f, 10f, 6f);
            Lamp(126f, 13f, 6f, true);
            Lamp(120f, 20f, 6f);
            Lamp(128f, 29f, 7f, true);
            Lamp(106f, 28f, 5f);
        }

        // ------------------------------------------------------------------
        // 5. A Optica (em cima)
        // ------------------------------------------------------------------

        /// <summary>
        /// Em cima da galeria, pela chamine do atelie. Aqui a luz alquimica e
        /// dobrada em espelhos de prata que giram (E): a luz-guia, fria, so
        /// acende fechaduras de cristal; os raios-perigo, quentes, piscam em
        /// ritmo e queimam.
        ///
        ///   a a Sala das Lentes: dois raios-perigo na entrada e o primeiro
        ///     enigma, de um espelho so. A luz cai do teto num espelho em cima
        ///     de um pedestal; girado, ele a manda pra fechadura em cima da
        ///     grade da Camara dos Espelhos. O quadro-negro ensina.
        ///   b a Camara dos Espelhos, alta: cinco espelhos em dois lados de uma
        ///     escalada. A luz entra rente ao chao e tem que subir em
        ///     zigue-zague ate a fechadura do nicho da Luz Engarrafada, no
        ///     alto. Um desvio no penultimo espelho leva a luz pra uma
        ///     fechadura escondida na parede leste: o cofre do chassi.
        /// </summary>
        private static void BuildOptics()
        {
            const string lenses = "05a_Lenses";
            const string mirrors = "05b_MirrorChamber";
            Room(lenses, 30, 61, 70, 69);
            Room(mirrors, 3, 61, 29, 80);

            // a. A Sala das Lentes. A chamine do atelie chega pelo chao, a leste.
            Carve(30, 61, 70, 69);

            HazardBeam("Beam_Lenses_1", 50, 69, 0, -1, 1.6f, 1.4f, 0f);
            HazardBeam("Beam_Lenses_2", 54, 69, 0, -1, 1.6f, 1.4f, 1.5f);

            // O primeiro enigma: a luz cai do teto no espelho do pedestal. Em
            // "\" vai pro leste, contra a parede; em "/", pro oeste, ate a
            // fechadura em cima da grade.
            Solid(39, 61, 41, 63);
            GuideLight("Guide_Lenses", 40, 68, 0, -1);
            Mirror(lenses, "Mirror_Lenses", 40, 66, false, 40, 64);
            CrystalLock("Lock_Lenses", 30, 66, LensesGate);

            // A grade pra Camara dos Espelhos, um degrau acima do chao.
            Carve(29, 62, 29, 64);
            Gate(LensesGate, 29, 62, 1, 3);

            Tell("chalk", lenses, "Lore_Chalk_Light", "um quadro-negro", 35, 61,
                "Um quadro-negro com um desenho: um raio saindo de uma lâmpada, batendo num espelho inclinado e dobrando a esquina até um cristal.",
                "Do lado, em letra de aula: \"O espelho não cria a luz. Só escolhe pra onde ela vai.\"",
                "Embaixo, menor, com uma seta em volta do espelho: \"Gire. Siga o raio. O cristal abre.\"");

            Tell("diary", lenses, "Lore_Diary_Light", "diário de laboratório, folha do dia 150", 62, 61,
                "\"Dia 150. A luz alquímica atravessa o Óxido e mostra o que já apodreceu por dentro do metal. Testamos em placas de prata nobre: por dentro, estavam podres.\"",
                "\"Ou seja: a prata também tá apodrecendo, a cidade inteira tá. Só demora mais pra aparecer. Sua Majestade proibiu que a luz fosse apontada pra Corte.\"",
                "\"Aviso pra quem trabalha aqui: as lâmpadas quentes queimam, andem entre um clarão e outro. A fria, a dos espelhos, só aponta o caminho.\"");

            Enemy(FoeKind.Sentinel, "Sentinel_Lenses", 30, 38, 61, 34);
            Ledge(56, 59, 64);
            Enemy(FoeKind.FlaskThrower, "Thrower_Lenses", 57, 57, 65);

            Put("lab_lens_bench", 45f, 61, 4);
            Put("lab_prism", 59f, 61, 4);
            Hang("lab_chandelier", 46f, 70);

            Lamp(34f, 66f, 7f);
            Lamp(45f, 66f, 6f);
            Lamp(60f, 66f, 7f);

            // b. A Camara dos Espelhos.
            Carve(9, 61, 28, 80);

            // A escalada: patamares nos dois lados, deixando livres as colunas
            // e as linhas por onde a luz passa (coluna 12, coluna 24, linhas
            // 62, 70 e 78).
            Ledge(16, 20, 64);
            Ledge(13, 15, 68);
            Ledge(25, 27, 68);
            Ledge(16, 20, 72);
            Ledge(13, 15, 76);
            Ledge(9, 11, 76);
            Ledge(25, 28, 76);

            // A luz entra rente ao chao, pelo leste.
            GuideLight("Guide_Mirrors", 27, 62, -1, 0);

            //   A (12,62)  "/" manda pro chao;      "\" sobe a coluna 12.
            //   B (12,70)  "\" manda pra parede;    "/" vai pro leste.
            //   C (24,70)  "\" manda pro chao;      "/" sobe a coluna 24.
            //   D (24,78)  "\" vai pro oeste;       "/" vai pro leste: o segredo.
            //   E (12,78)  "/" desce de volta;      "\" sobe: a fechadura do nicho.
            Mirror(mirrors, "Mirror_A", 12, 62, true, 13, 61);
            Mirror(mirrors, "Mirror_B", 12, 70, false, 13, 69);
            Mirror(mirrors, "Mirror_C", 24, 70, false, 25, 69);
            Mirror(mirrors, "Mirror_D", 24, 78, false, 25, 77);
            Mirror(mirrors, "Mirror_E", 12, 78, true, 13, 77);

            // O nicho da Luz Engarrafada, gradeado; a fechadura em cima da entrada.
            Carve(3, 77, 8, 79);
            Gate(MirrorsGate, 8, 77, 1, 3);
            CrystalLock("Lock_Mirrors", 12, 80, MirrorsGate);
            AddReagent(mirrors, "Light", "LUZ ENGARRAFADA", 1, 5, 77);

            // O cofre: um bloco falso na parede leste e, na frente dele, uma
            // fechadura apagada, quase da cor do marmore.
            Carve(30, 77, 36, 80);
            Carve(29, 77, 29, 79);
            Gate(VaultGate, 29, 77, 1, 3, 0, 1, true);
            CrystalLock("Lock_Vault", 28, 78, VaultGate, true);
            Mark(mirrors, "Secret_Chassis", SecretChassisTile.X, SecretChassisTile.Y);

            Enemy(FoeKind.Sentinel, "Sentinel_Mirror", 9, 28, 61, 20);
            Enemy(FoeKind.Sentinel, "Sentinel_Mirror_Ledge", 16, 20, 65);
            Enemy(FoeKind.FlaskThrower, "Thrower_Mirror", 26, 26, 69);

            Put("lab_prism", 22f, 61, 4);
            Put("lab_lens_bench", 16.5f, 61, 4);
            Put("lab_mirror", 26f, 61, 3);
            Put("lab_lens_bench", 18f, 73, 4);
            Put("lab_shelf", 33f, 77, 3);
            Hang("lab_chandelier", 18f, 81);

            Lamp(18f, 64f, 6f);
            Lamp(19f, 71f, 6f);
            Lamp(18f, 78f, 6f);
            Lamp(5f, 79f, 4f);
            Lamp(33f, 79f, 3f);
        }

        // ------------------------------------------------------------------
        // 6. A Incubacao (a leste)
        // ------------------------------------------------------------------

        /// <summary>
        /// Os tanques de cultura, cada um com uma cobaia de cobre dentro (dois
        /// rachados, com os cacos no fosso); o bioterio, mais fundo, com uma
        /// vala de cacos e as gaiolas por cima; e no fim a Oficina de
        /// Prototipos: seis mesas com seis tentativas desmontadas, e a setima,
        /// vazia. A Assistente se escondeu no armario de reagentes.
        /// </summary>
        private static void BuildIncubation()
        {
            const string tanks = "06a_Tanks";
            const string vivarium = "06b_Vivarium";
            const string workshop = "06c_PrototypeWorkshop";
            Room(tanks, 101, 38, 150, 46);
            Room(vivarium, 151, 34, 180, 46);
            Room(workshop, 181, 36, 215, 44);

            Carve(105, 41, 150, 46);

            // Os tanques rachados: o vidro escorreu pro fosso embaixo deles.
            Carve(116, 38, 119, 40);
            Spikes(116, 119, 38);
            Carve(130, 38, 133, 40);
            Spikes(130, 133, 38);

            // Os tanques ficam no chao firme: o da beira do fosso leste chega
            // meio tile pro lado, senao a base dele fica no ar.
            for (int i = 0; i < 7; i++) Put("lab_tank_" + (i % 3), 108.5f + i * 6.5f + (i == 4 ? 0.5f : 0f), 41, 3);

            Ledge(118, 121, 44);
            Ledge(138, 141, 44);

            Enemy(FoeKind.FailedExperiment, "Failed_Tanks_1", 120, 129, 41, 125);
            Enemy(FoeKind.FailedExperiment, "Failed_Tanks_2", 134, 150, 41, 142);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tanks_1", 119, 119, 45);
            Enemy(FoeKind.FlaskThrower, "Thrower_Tanks_2", 139, 139, 45);

            Tell("diary", tanks, "Lore_Diary_2", "diário de laboratório, folha do dia 90", 110, 41,
                "\"Dia 90. Conclusão ruim: o mercúrio só atrasa o Óxido, não cura. Em placa, a gente ganha dias. Precisamos saber o que acontece num corpo inteiro.\"",
                "\"Sua Majestade autorizou testes em cobaias de cobre. Disse que tem muitos, e que ninguém vai sentir falta.\"");

            Drips.Add((124, 46, 5));
            Drips.Add((145, 46, 5));

            Lamp(111f, 44f, 6f, true);
            Lamp(117.5f, 40f, 4f, true);
            Lamp(124f, 44f, 6f, true);
            Lamp(131.5f, 40f, 4f, true);
            Lamp(137f, 44f, 6f, true);
            Lamp(147f, 44f, 6f);

            // O bioterio, mais fundo, com a vala de cacos no meio e uma gaiola
            // grande por cima dela.
            Carve(151, 34, 180, 46);
            Ledge(151, 154, 37);
            Spikes(160, 169, 34);
            Ledge(163, 166, 37);

            Enemy(FoeKind.Slime, "Slime_Vivarium_1", 151, 159, 34, 155);
            Enemy(FoeKind.Slime, "Slime_Vivarium_2", 170, 180, 34, 175);

            Tell("diary", vivarium, "Lore_Diary_3", "diário de laboratório, folha do dia 210", 177, 34,
                "\"Dia 210. Teste em corpo inteiro. O tanque 4 não acordou. O tanque 7 acordou errado. Juntamos o que sobrou do 7 com o que sobrou do 9.\"",
                "\"O mercúrio entra no corpo e não sai mais. O Mestre diz que estamos quase lá. Ele não dorme. E a prata dele tá ficando mole, brilhante demais.\"",
                "A letra piorou muito.");

            Put("lab_cage", 156f, 34, 4);
            Put("lab_cage_broken", 164.5f, 38, 4);
            Put("lab_cage", 173f, 34, 4);
            Put("lab_table", 178f, 34, 4);
            Hang("lab_pipes", 165f, 47);

            Lamp(157f, 40f, 6f, true);
            Lamp(164.5f, 41f, 7f);
            Lamp(176f, 39f, 6f);

            // A Oficina de Prototipos.
            Carve(181, 36, 215, 44);

            for (int n = 0; n < 6; n++) Put("lab_proto_table_" + n, 190f + n * 3.5f, 36, 4);
            Put("lab_proto_table_6", 211.5f, 36, 4);

            Tell("assistant", workshop, "Npc_Assistant", "Assistente", 184, 36,
                "N-não abre! ...Ah. Você não é um deles. Você é... de bronze? De pedra?",
                "Eu era a aprendiz do Mestre. Do Alquimista. Tô trancada aqui desde que ele começou a escorrer.",
                "Ele não era cruel, tá? Você precisa saber disso. Ele chorou no primeiro tanque. Queria curar todo mundo, até a gente, de cobre.",
                "Mas o Rei mandava, e ele obedecia. Cada ordem um pouco pior que a outra. No fim, ele obedecia antes de o Rei pedir.",
                "Ele tá no Grande Laboratório, depois do alambique. Enquanto tá líquido, nada acerta ele. Mas os círculos do chão... eles prendem ele em prata.");

            Tell("board", workshop, "Lore_PrototypeBoard", "o quadro das fichas", 189, 36,
                "Um quadro de cortiça com seis fichas presas por alfinete, uma pra cada mesa.",
                "N.º 1: \"Ferro fundido. Pesado demais pra andar.\" N.º 2: \"Latão. O Óxido pegou em três dias.\"",
                "N.º 3: \"Cobre. Cobaia forçada.\"",
                "N.º 4: \"Vidro. Quebrou ao se ver no espelho.\" N.º 5: \"Prata nobre. Apodreceu antes de acordar.\"",
                "N.º 6: \"Quartzo lapidado. Perfeito. Não aguentou o primeiro passo.\"",
                "Tem um sétimo alfinete, sem ficha nenhuma.");

            Tell("diary", workshop, "Lore_Diary_5", "diário de laboratório, folha do dia 300", 200, 36,
                "\"Dia 300. Descoberta principal: os protótipos brutos são imunes. Quartzo sem lapidar, bronze sem polimento. O Óxido não tem onde se agarrar neles.\"",
                "\"Isso é a cura. Um corpo bruto pra cada um da cidade, e ninguém mais apodrece.\"",
                "\"Levei o n.º 7 até o trono, andando. Imune. Sua Majestade recusou: disse que a prata não vai ser salva por sucata. Ordem de descarte assinada.\"");

            Tell("sheet", workshop, "Lore_PrototypeSheet", "ficha de protótipo", 208, 36,
                "Uma prancheta. Um desenho técnico, feito com capricho: cabeça de quartzo bruto, dois olhos, uma mola.",
                "É você. É o seu corpo, cotado em milímetros.",
                "\"PROTÓTIPO BRUTO N.º 7. IMUNE AO ÓXIDO. FUNCIONAL.\"",
                "E, por cima de tudo, carimbado em vermelho: \"REJEITADO — IMPERFEITO\".",
                "No campo de destino, impresso: \"POÇO DE SUCATA\".");

            AddReagent(workshop, "Salt", "SAL DE QUARTZO", 2, 213, 36);

            Enemy(FoeKind.Sentinel, "Sentinel_Workshop", 181, 215, 36, 196);

            Put("lab_shelf", 182.5f, 36, 3);
            Hang("lab_chandelier", 200f, 45);

            Lamp(186f, 40f, 6f);
            Lamp(198f, 41f, 8f);
            Lamp(210f, 40f, 7f);
        }

        // ------------------------------------------------------------------
        // 7. A subida pro chefe
        // ------------------------------------------------------------------

        /// <summary>
        /// Do mezanino do atelie, a grade do alambique; atras dela um corredor
        /// curto e uma chamine ate a antessala do Grande Laboratorio, com a
        /// bancada.
        /// </summary>
        private static void BuildBossRoute()
        {
            const string room = "07_PreBoss";
            Room(room, 100, 48, 128, 68);

            Carve(101, 48, 112, 51);
            Gate(AlembicGate, 101, 48, 1, 4);

            Carve(109, 52, 112, 61);

            // Dois raios-perigo atravessam a chamine: sobe-se quicando entre um clarao e outro.
            HazardBeam("Beam_Chimney_Low", 109, 55, 1, 0, 1.2f, 1.6f, 0f);
            HazardBeam("Beam_Chimney_High", 109, 59, 1, 0, 1.2f, 1.6f, 1.4f);

            Carve(100, 62, 128, 68);
            Bench(room, "Workbench_PreBoss", 116, 62);

            Tell("diary", room, "Lore_Diary_4", "diário de laboratório, folha do dia 255", 104, 62,
                "\"Dia 255. O Mestre entrou no tanque grande, sozinho. A ideia dele: se o mercúrio atrasa o Óxido, então quem for feito de mercúrio não apodrece nunca.\"",
                "\"Ele saiu. Fala como antes, educado. Mas escorre. E quando ri, o riso continua depois que a boca fecha.\"",
                "\"Os guardas de prata perfeita passaram a vigiar o laboratório. Não sei se tão protegendo a gente ou ele.\"");

            Put("lab_shelf", 122f, 62, 4);
            Put("lab_jar_1", 125.5f, 62, 4);

            Lamp(106f, 65f, 6f);
            Lamp(118f, 66f, 7f);
        }

        // ------------------------------------------------------------------
        // 8. O Grande Laboratorio
        // ------------------------------------------------------------------

        /// <summary>
        /// A arena do Alquimista: alta, com tanques nas paredes e tres estrados
        /// de marmore, cada um com um circulo de transmutacao. No meio, o ralo.
        /// Na segunda metade da luta o chao entre os estrados vira mercurio.
        /// </summary>
        private static void BuildLaboratory()
        {
            const string room = "08_GrandLaboratory";
            Cells arena = ArenaRoom;
            Room(room, arena.X0, arena.Y0, arena.X1, arena.Y1);

            Carve(arena.X0, arena.Y0, arena.X1, arena.Y1);

            // Cantos de cima chanfrados.
            Solid(129, 77, 132, 80);
            Solid(167, 77, 170, 80);

            int[] daises = { 132, 147, 162 };
            for (int i = 0; i < daises.Length; i++)
            {
                int x = daises[i];
                // A runa fica no alto, numa prateleira em cima do estrado:
                // nunca na altura dele. Sobe-se pelo estrado.
                Solid(x, 62, x + 4, 63);
                Ledge(x, x + 4, 67);
                ArenaCircles.Add(("ArenaCircle_" + (i + 1), x + 2, 68));
                Mark(room, "ArenaCircle_" + (i + 1), x + 2, 68);
            }

            ArenaFloodPools.Add(new Cells(129, 61, 131, 61));
            ArenaFloodPools.Add(new Cells(137, 61, 146, 61));
            ArenaFloodPools.Add(new Cells(152, 61, 161, 61));
            ArenaFloodPools.Add(new Cells(167, 61, 170, 61));

            Mark(room, "BossSpawn_MercuryAlchemist", BossTile.X, BossTile.Y);

            Put("lab_tank_2", 140f, 62, 2);
            Put("lab_tank_0", 156f, 62, 2);
            Put("lab_great_tank", 149.5f, 64, 1);
            Put("lab_drain", DrainTile.X + 0.5f, DrainTile.Y, 6);
            Hang("lab_chandelier", 149.5f, 81);
            Hang("lab_pipes", 138f, 81);
            Hang("lab_pipes", 161f, 81);

            Windows.Add((1, 141, 70));
            Windows.Add((0, 157, 70));

            Lamp(135f, 70f, 8f, true);
            Lamp(149.5f, 74f, 11f);
            Lamp(164f, 70f, 8f, true);

            // A saida, gradeada ate o Alquimista cair.
            Carve(171, 62, 171, 66);
            Gate(BossExitGate, 171, 62, 1, 5);
        }

        // ------------------------------------------------------------------
        // 9. O corredor de saida
        // ------------------------------------------------------------------

        /// <summary>
        /// Da saida da arena ao anfiteatro: um corredor com dois fossos e, no
        /// fim, uma chamine.
        /// </summary>
        private static void BuildGuardCorridor()
        {
            const string room = "09_GuardCorridor";
            Room(room, 172, 59, 226, 75);

            Carve(172, 62, 225, 67);

            Carve(182, 59, 186, 61);
            Spikes(182, 186, 59);
            Carve(196, 59, 201, 61);
            Spikes(196, 201, 59);

            // A chamine pro anfiteatro: a parede de oeste desce ate a altura da cabeca.
            Solid(221, 66, 221, 67);
            Carve(222, 68, 225, 75);

            Put("lab_tank_2", 177f, 62, 3);
            Put("lab_cage_broken", 191f, 62, 4);
            Put("lab_shelf", 207f, 62, 4);
            Put("lab_rack", 215f, 62, 4);

            Lamp(178f, 65f, 6f);
            Lamp(191f, 65f, 6f);
            Lamp(206f, 65f, 6f);
            Lamp(223.5f, 71f, 5f);
        }

        // ------------------------------------------------------------------
        // 10. O Anfiteatro de Demonstracoes
        // ------------------------------------------------------------------

        /// <summary>
        /// Onde a nobreza assistia aos experimentos, e onde o Rei recusou o
        /// prototipo. A arquibancada a oeste, o palco no meio com o grande
        /// circulo (o coracao do distrito), o pulpito, e a leste a passagem pra
        /// Grande Escadaria.
        /// </summary>
        private static void BuildAmphitheater()
        {
            const string room = "10_Amphitheater";
            Room(room, 190, 76, 236, 88);

            Carve(190, 76, 236, 88);

            // A arquibancada, em degraus.
            Solid(190, 76, 195, 79);
            Solid(196, 76, 201, 77);

            Mark(room, "StageCircle", StageCircle.X, StageCircle.Y);

            Tell("plaque", room, "Lore_Amphitheater", "a placa do palco", 205, 76,
                "Uma placa de prata na beira do palco: \"TRIBUNAL DE INOVAÇÕES. AQUI A CIÊNCIA SE APRESENTA A SUA MAJESTADE.\"",
                "Nos degraus, almofadas de veludo, ainda arrumadas. Ninguém se levantou pra ir embora; só pararam de vir.");

            Tell("decree", room, "Lore_DiscardOrder", "a ordem no púlpito", 218, 76,
                "Um papel timbrado com o selo de prata do Rei, preso no púlpito do palco.",
                "\"O protótipo bruto foi apresentado a este Tribunal. É imune ao Óxido. Isso não muda nada.\"",
                "\"Cravejada está apodrecendo, distrito por distrito. Que apodreça. Prefiro uma cidade podre a uma cidade salva por sucata.\"",
                "\"Destino de todo o lote: Poço de Sucata. Que não se fale mais nisso.\"",
                "Assinado, embaixo, numa letra que não treme.");

            // A passagem pra Grande Escadaria, na parede leste.
            Carve(237, 76, 239, 81);

            Put("lab_seats", 192.5f, 80, 3);
            Put("lab_seats", 198.5f, 78, 3);
            Put("lab_stage", StageCircle.X + 0.5f, StageCircle.Y, 2);
            Put("lab_lectern", 218f, 76, 4);
            Hang("lab_chandelier", 212.5f, 89);

            Windows.Add((1, 205, 81));
            Windows.Add((0, 222, 81));

            Lamp(193f, 84f, 7f);
            Lamp(212.5f, 84f, 9f);
            Lamp(229f, 82f, 7f);

            // Depois da purificacao: a Assistente sai do armario e senta na
            // arquibancada; Crisol vem assistir.
            Purified.Add(new LoreSpot
            {
                Kind = "standing", Name = "Npc_Assistant_Purified", Title = "Assistente", X = 193, Y = 80,
                Lines = new[]
                {
                    "Saí do armário. As pernas ainda não acreditam.",
                    "O Mestre... eu vi pelo vidro. Ele não gritou. Pediu desculpa, eu acho. Pro mercúrio, pra ele mesmo, sei lá.",
                    "Você era o n.º 7, né? Eu limpei a sua mesa no dia em que te levaram. Guardei a mola que sobrou.",
                    "Mentira, não guardei. Joguei fora, como mandaram. Mas eu lembro dela. Isso conta alguma coisa?",
                },
            });

            Purified.Add(new LoreSpot
            {
                Kind = "crisol", Name = "Npc_Crisol_Purified", Title = "Crisol", X = 199, Y = 78,
                Lines = CrisolPurifiedLines,
            });
        }

        // ------------------------------------------------------------------
        // 11. A Grande Escadaria
        // ------------------------------------------------------------------

        /// <summary>
        /// Um salao alto de marmore, a leste do anfiteatro: dois lances largos
        /// de degraus baixos, com a passadeira vermelha e a balaustrada
        /// dourada, um patamar no meio e, no alto, a porta do palacio. Os
        /// degraus sao de meio tile: o Scrap sobe andando.
        /// </summary>
        private static void BuildGrandStair()
        {
            const string room = "11_GrandStair";
            (int x0, int y0, int x1, int y1) = StairHall;
            Room(room, x0, y0, x1, y1);

            Carve(x0 + 3, y0, x1, y1);

            Flight(243, 18, y0);
            Landing(261, 264, 85);
            Flight(265, 18, 85);
            Landing(283, 286, 94);

            Mark(room, "Exit_District5", StairTop.X, StairTop.Y);

            Hang("lab_chandelier", 252f, 98);
            Hang("lab_chandelier", 272f, 98);
            Windows.Add((1, 246, 84));
            Windows.Add((0, 256, 90));
            Windows.Add((1, 268, 92));

            Lamp(244f, 80f, 6f);
            Lamp(256f, 88f, 7f);
            Lamp(270f, 92f, 7f);
            Lamp(284f, 96f, 6f);
        }

        /// <summary>
        /// Um lance subindo pra direita: cada coluna meio tile acima da
        /// anterior. Por baixo e marmore macico (tile); a metade de cima de
        /// cada degrau impar e um meio-degrau (objeto).
        /// </summary>
        private static void Flight(int x0, int steps, int floor)
        {
            Flights.Add((x0, steps, floor));

            for (int i = 0; i < steps; i++)
            {
                int x = x0 + i;
                int halves = i + 1;
                int full = halves / 2;

                // Marmore macico do chao do salao ate o degrau.
                int solidTop = floor + full - 1;
                if (solidTop >= StairHall.Y0) Solid(x, StairHall.Y0, x, solidTop);

                if (halves % 2 == 1)
                {
                    int cell = floor + full;
                    Solid(x, cell, x, cell);
                    ObjectCells.Add((x, cell));
                    HalfSteps.Add((x, cell));
                }
            }
        }

        private static void Landing(int x0, int x1, int y)
        {
            Landings.Add((x0, x1, y));
            Solid(x0, StairHall.Y0, x1, y - 1);
        }
    }
}
