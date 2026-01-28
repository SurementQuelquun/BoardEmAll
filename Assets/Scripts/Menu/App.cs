using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class App : MonoBehaviour
{
    public static App Instance { get; private set; }

    public InputActionAsset InputSystem_Actions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(LoadSceneAdditiveIfNeeded("OutGame"));
    }

    public void StartLoadMapAndGenerate(string sceneName)
    {
        StartCoroutine(LoadMapAndGenerate(sceneName));
    }

    private IEnumerator LoadSceneAdditiveIfNeeded(string sceneName)
    {
        var existing = SceneManager.GetSceneByName(sceneName);

        if (!existing.IsValid())
        {
            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            //Debug.LogError($"Failed to start loading scene '{sceneName}'.");
            yield break;
        }
    }

    private IEnumerator LoadMapAndGenerate(string sceneName)
    {
        // 1. Load the scene additively
        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);


        // Wait until fully loaded
        while (!loadOp.isDone)
            yield return null;

        // 2. Get the scene reference
        var scene = SceneManager.GetSceneByName(sceneName);


        // 3. IMPORTANT: Set the new scene as Active BEFORE generating anything
        SceneManager.SetActiveScene(scene);

        // Optional: Wait one frame to ensure Awake/OnEnable have fired in the new scene
        yield return null;

        // 4. Find GridManagers strictly within the loaded scene and Generate
        foreach (var root in scene.GetRootGameObjects())
        {
            var gridManagers = root.GetComponentsInChildren<GridManager>(true);
            foreach (var gm in gridManagers)
            {
                gm.Generate();
            }
        }
    }
}