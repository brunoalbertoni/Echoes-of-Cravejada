using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O menu Echoes: um comando por cena. Gerar uma fase valida o mapa, monta
    /// a cena e refaz os Animators, nessa ordem.
    /// </summary>
    public static class EchoesMenu
    {
        private const string MenuScene = "Assets/Common/Scenes/MainMenu.unity";

        [MenuItem("Echoes/Gerar Menu Principal", false, 0)]
        private static void GenerateMenu() => Build("Menu Principal", MenuScene, null, MainMenuGenerator.Generate);

        [MenuItem("Echoes/Gerar Fase 1 - Scrap Yard", false, 10)]
        private static void GeneratePhase1() => Build("Fase 1", PhaseScene("Fase01_Poço", "Phase01_ScrapYard"), Phase01Layout.Validate, Phase01Generator.Generate);

        [MenuItem("Echoes/Gerar Fase 2 - Brass Pipelines", false, 11)]
        private static void GeneratePhase2() => Build("Fase 2", PhaseScene("Fase02_Encanamentos", "Phase02_BrassPipelines"), Phase02Layout.Validate, Phase02Generator.Generate);

        [MenuItem("Echoes/Gerar Fase 3 - Silent Geodes", false, 12)]
        private static void GeneratePhase3() => Build("Fase 3", PhaseScene("Fase03_GeodosSilenciosos", "Phase03_SilentGeodes"), Phase03Layout.Validate, Phase03Generator.Generate);

        [MenuItem("Echoes/Gerar Fase 4 - Alchemical Ateliers", false, 13)]
        private static void GeneratePhase4() => Build("Fase 4", PhaseScene("Fase04_AteliêsAlquímicos", "Phase04_AlchemicalAteliers"), Phase04Layout.Validate, Phase04Generator.Generate);

        [MenuItem("Echoes/Gerar Fase 5 - Silver Palace", false, 14)]
        private static void GeneratePhase5() => Build("Fase 5", PhaseScene("Fase05_CidadeDePrata", "Phase05_SilverCity"), Phase05Layout.Validate, Phase05Generator.Generate);

        [MenuItem("Echoes/Redesenhar Interface", false, 50)]
        private static void RedrawInterface()
        {
            CheckpointArtAssets.Redraw();
            HudIconAssets.Redraw();
            HudIconAssets.RedrawDeathScreen();
            HudIconAssets.RedrawMenu();
            HudIconAssets.RedrawPause();
            HudIconAssets.RedrawPortraits();
        }

        private static string PhaseScene(string pasta, string cena) => $"Assets/Phases/{pasta}/Scene/{cena}.unity";

        private static void Build(string label, string scenePath, Func<List<string>> validate, Action generate)
        {
            if (validate != null)
            {
                List<string> problems = validate();
                if (problems != null && problems.Count > 0)
                {
                    foreach (string problem in problems) Debug.LogError($"[{label}] {problem}");
                    EditorUtility.DisplayDialog("Echoes", $"{label}: o mapa tem {problems.Count} problema(s). Veja o Console. Nada foi gerado.", "OK");
                    return;
                }
            }

            DateTime before = File.Exists(scenePath) ? File.GetLastWriteTimeUtc(scenePath) : DateTime.MinValue;
            generate();

            // Cancelado no aviso de salvar: a cena nao mudou, entao nao ha o que refazer.
            if (!File.Exists(scenePath) || File.GetLastWriteTimeUtc(scenePath) == before) return;

            AnimatorBaker.BakeAll();
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }
}
