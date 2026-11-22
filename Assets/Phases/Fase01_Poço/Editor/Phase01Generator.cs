using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Monta a cena da Phase 1 do zero, a partir do desenho em Phase01Layout.
    ///
    /// Rodar de novo refaz a cena inteira e descarta o que voce tiver ajustado
    /// nela na mao — entao ajuste o layout no codigo enquanto estiver iterando,
    /// e so comece a mexer na cena quando o formato estiver bom.
    ///
    /// Menu:      Echoes > Gerar Phase 1 (Scrap Yard)
    ///            Echoes > Validar layout da Phase 1
    /// Batchmode: -executeMethod EchoesOfCravejada.EditorTools.Phase01Generator.GenerateFromCommandLine
    /// </summary>
    public static partial class Phase01Generator
    {
        public const string ScenePath = "Assets/Phases/Fase01_Poço/Scene/Phase01_ScrapYard.unity";
        private const string GroundLayerName = "Ground";

        // As camadas de paisagem bg_0X_*.png do pack continuam de fora de
        // proposito: sao cenas de exterior completas (ceu com lua, horizonte,
        // props espalhados) e, esticadas pra cobrir uma sala fechada, vazam
        // pelas paredes e se confundem com as plataformas.
        //
        // O fundo e a parede de caverna ATRAS da sala, pintada no TM_Background
        // e terminando onde a sala termina. O que ha de profundidade aparece
        // so pelas janelas dessa parede: la atras ficam as silhuetas dos geodos,
        // em parallax, contra o clarao verde do Oxide.

        /// <summary>
        /// A cor do que se ve pelas janelas: o clarao do Oxide ao longe. E tambem
        /// o fundo da camera, entao se alguma camada falhar a tela fica verde
        /// escura, e nao preta.
        /// </summary>
        private static readonly Color OxideHaze = new Color(0.06f, 0.15f, 0.12f);

        /// <summary>Verde-cobre do Oxide, usado nas luzes e na nevoa.</summary>
        private static readonly Color OxideGreen = new Color(0.36f, 0.95f, 0.60f);

        /// <summary>
        /// Escurece a parede do fundo pra ela recuar em relacao ao terreno. E o
        /// que separa "onde eu piso" de "o que esta atras", sem contorno nenhum.
        /// </summary>
        private static readonly Color BackWallTint = new Color(0.45f, 0.55f, 0.52f);

        /// <summary>
        /// A regra de todas as fases: o que o Scrap nao toca (nao usa, nao bate,
        /// nao pisa) fica em segundo plano — mais escuro e apagado que o chao,
        /// os inimigos e o que se usa, mesmo quando esta na frente do terreno.
        /// So a luz propria (fogo, vidro aceso, Oxide) escapa dessa tinta.
        /// </summary>
        internal static readonly Color DecorTint = new Color(0.60f, 0.66f, 0.66f);

        /// <summary>
        /// Se a queda do Coletor termina na cutscene desenhada da troca de
        /// visual. Desligada por enquanto: as cutscenes entram quando o jogo
        /// estiver completo. Sem ela, o Scrap brilha e troca de visual na hora.
        /// </summary>
        private static readonly bool UsePurificationCartoon = false;

        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[Phase 1] Geracao cancelada.");
                return;
            }

            Build();
        }

        /// <summary>
        /// Confere que o mapa e navegavel sem precisar entrar no jogo.
        /// Vale rodar toda vez que voce mexer nas plataformas do Phase01Layout.
        /// </summary>
        public static void ValidateLayout()
        {
            List<string> problems = Phase01Layout.Validate();
            problems.AddRange(Phase01Decor.Validate());

            if (problems.Count == 0)
            {
                Debug.Log("[Phase 1] Layout OK: da pra andar pela sala inteira a partir de onde " +
                          "o Scrap nasce e chegar embaixo da boca do poco.");
                return;
            }

            foreach (string problem in problems)
            {
                Debug.LogWarning($"[Phase 1] {problem}");
            }

            Debug.LogError($"[Phase 1] {problems.Count} problema(s) no layout. Veja os avisos acima.");
        }

        /// <summary>Ponto de entrada pro modo batch, sem nenhum dialogo.</summary>
        public static void GenerateFromCommandLine()
        {
            Build();
        }

        private static void Build()
        {
            Phase01Assets.EnsureFolders();
            int groundLayer = Phase01Assets.EnsureLayer(GroundLayerName);

            // Precisa vir antes de montar a cena: atribuir sortingLayerName a um
            // nome que nao existe faz a Unity cair calada de volta pra Default.
            Phase01Assets.EnsureSortingLayers();
            Phase01Assets.VerifySortingLayers();

            // Um mapa quebrado vira uma cena que nao da pra jogar, entao o
            // aviso vem antes de gastar tempo gerando.
            List<string> layoutProblems = Phase01Layout.Validate();
            layoutProblems.AddRange(Phase01Decor.Validate());
            foreach (string problem in layoutProblems)
            {
                Debug.LogWarning($"[Phase 1] {problem}");
            }

            if (!Phase01Assets.SliceTileset())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui fatiar o tileset.");
                return;
            }

            if (!Phase01Assets.SliceBackgroundTileset())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui fatiar a parede de fundo.");
                return;
            }

            if (!Phase01Assets.SliceObjects())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui fatiar as folhas de objetos.");
                return;
            }

            if (!Phase01Assets.SliceBackdropPieces())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui recortar as janelas e as silhuetas do fundo.");
                return;
            }

            Dictionary<string, TileBase> tiles = Phase01Assets.BuildTiles();
            if (tiles.Count == 0)
            {
                Debug.LogError("[Phase 1] Abortado: nenhum tile foi criado.");
                return;
            }

            if (!Phase02Assets.EnsureMechanicsArt())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui preparar a arte das mecanicas.");
                return;
            }

            // Os descartados, o rastejante e o Coletor, no detalhe do Scrap.
            Phase02Assets.EnsureDistrict1Art();

            // A cenografia: vigas, canos, calha, montes de sucata, carcacas, postes...
            Phase01DecorBuilder.EnsureArt();

            // Os marcadores que flutuam em cima do que da pra usar.
            EnsureCueArt();

            // O que se le: o desenho do mapa e o grande, da leitura, de cada um.
            WriteLoreArt();

            // Os quadros da cutscene desenhada da purificacao (fim do boss).
            if (UsePurificationCartoon) CartoonAssets.EnsureArt();

            Sprite scrapSprite = Phase01Assets.CreateScrapPlaceholder();
            Sprite polishedSprite = Phase02Assets.LoadSprite(Phase02Assets.ScrapPolished);
            Material fogMaterial = Phase01Assets.GetOrCreateFogMaterial();
            Material unlitMaterial = Phase01Assets.GetOrCreateUnlitMaterial();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildTilemaps(tiles, groundLayer);
            BuildBackdrop(unlitMaterial);
            Transform scrap = BuildScrap(scrapSprite, polishedSprite, groundLayer, Phase01Layout.SpawnTile);

            // A primeira cena do jogo: ele despencando pela Calha N.º 1 e acordando no monte de sucata.
            IntroFallAssets.Attach(scrap.gameObject);
            BuildCamera(scrap, Phase01Layout.Width, Phase01Layout.Height, OxideHaze);
            BuildLighting();
            BuildFog(fogMaterial);
            BuildDust(fogMaterial);
            BuildMarkers();
            BuildTravel();
            BuildMechanics(groundLayer);

            // Depois da luz (as luzes dela vao pra raiz Lighting) e antes do
            // boss (a purificacao procura as pecas dela pelo nome).
            Phase01DecorBuilder.Build();
            BuildBossAndPurification(groundLayer, fogMaterial);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            Debug.Log($"[Phase 1] Cena gerada: {ScenePath}. " +
                      $"Mapa {Phase01Layout.Width}x{Phase01Layout.Height} (poco + arena), " +
                      $"{Phase01Layout.Props.Length} objetos de cenario, " +
                      $"{Phase01Layout.Windows.Length} janelas.");
        }

        // A poeira cai com uma velocidade inicial e vai acelerando.
        private const float DustGravityModifier = 0.02f;
        private const float DustSlowestFall = 0.2f;
        private const float DustFastestFall = 0.5f;
    }
}
