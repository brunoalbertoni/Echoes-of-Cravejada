using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Gera a cena do menu principal (Assets/Common/Scenes/MainMenu.unity): uma
    /// camera e o <see cref="MainMenu"/>, que desenha tudo sozinho. Ela entra
    /// em primeiro na lista do build: e por ela que o jogo abre.
    /// </summary>
    public static class MainMenuGenerator
    {
        private const string ScenePath = "Assets/Common/Scenes/" + MainMenu.SceneName + ".unity";

        [MenuItem("Echoes/Gerar Menu Principal", false, 0)]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            // A musica e os efeitos do menu precisam de alguem pra ouvir.
            cameraObject.AddComponent<AudioListener>();

            new GameObject("MainMenu").AddComponent<MainMenu>();

            EditorSceneManager.SaveScene(scene, ScenePath);

            // O menu e a primeira cena do build.
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(entry => entry.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log("[Menu] Cena do menu gerada em " + ScenePath + " e posta em primeiro no build.");
        }
    }
}
