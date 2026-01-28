#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Serialization;

public class MenuButtons : MonoBehaviour
{
    private Button QuitButton;
    private Button startButton1;
    private Button startButton2;
    private Button startButton3;

    [Header("Map Textures (assign in Inspector)")]
    [Tooltip("Texture for map_01 (placed in Assets/Map Images or Resources). Assign here.")]
    [SerializeField] private Texture2D map_01;
    [SerializeField] private Texture2D map_02;
    [SerializeField] private Texture2D map_03;

    [FormerlySerializedAs("_mapSceneName")]
    /*[SerializeField]*/
    private string _mapSceneName = "Map1";
    /*[SerializeField]*/
    private string _outSceneName = "OutGame";

    [System.Obsolete]
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();

        QuitButton = uiDocument.rootVisualElement.Q("QuitButton") as Button;
        QuitButton.RegisterCallback<ClickEvent>(QuitCallback);

        startButton1 = uiDocument.rootVisualElement.Q("PlayButton1") as Button;
        startButton1.RegisterCallback<ClickEvent>((evt) => PlayMapCallback(map_01));

        startButton2 = uiDocument.rootVisualElement.Q("PlayButton2") as Button;
        startButton2.RegisterCallback<ClickEvent>((evt) => PlayMapCallback(map_02));

        startButton3 = uiDocument.rootVisualElement.Q("PlayButton3") as Button;
        startButton3.RegisterCallback<ClickEvent>((evt) => PlayMapCallback(map_03));
    }

    private void QuitCallback(ClickEvent evt)
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        // Quit the built application
        Application.Quit();
#endif
    }

    // Centralized handler: loads Map1 scene (if needed), then finds GridManager in that scene
    // and calls SetMapAndGenerate with the selected texture.
    private void PlayMapCallback(Texture2D selectedMap)
    {
        if (selectedMap == null)
        {
            Debug.LogError("MenuButtons: selected map texture is null. Assign the map textures in the Inspector.");
            return;
        }

        StartCoroutine(LoadMapSceneAndApplyTexture(selectedMap));
    }

    private IEnumerator LoadMapSceneAndApplyTexture(Texture2D mapTexture)
    {
        // Load Map1 additively if not already loaded
        var scene = SceneManager.GetSceneByName(_mapSceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            var loadOp = SceneManager.LoadSceneAsync(_mapSceneName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                Debug.LogError($"MenuButtons: failed to start loading scene '{_mapSceneName}'.");
                yield break;
            }
            while (!loadOp.isDone)
                yield return null;

            scene = SceneManager.GetSceneByName(_mapSceneName);
        }

        // Make sure Awake/Start in the scene had a chance to run
        yield return null;

        // Set active scene (so newly generated GameObjects are placed into it)
        SceneManager.SetActiveScene(scene);

        // Find GridManager(s) inside the loaded scene and call SetMapAndGenerate
        foreach (var root in scene.GetRootGameObjects())
        {
            var gridManagers = root.GetComponentsInChildren<GridManager>(true);
            foreach (var gm in gridManagers)
            {
                gm.SetMapAndGenerate(mapTexture);
            }
        }

        // Optionally unload the menu/out scene
        //if (!string.IsNullOrEmpty(_outSceneName))
        //{
        var outScene = SceneManager.GetSceneByName(_outSceneName);
        if (outScene.IsValid() && outScene.isLoaded)
        {
            var unloadOp = SceneManager.UnloadSceneAsync(outScene);
            // wait for unload to finish
            if (unloadOp != null)
            {
                while (!unloadOp.isDone)
                    yield return null;
            }
        }
        //}
    }
}