//using System.Collections;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.SceneManagement;


//public class App : MonoBehaviour
//{
//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    public static App Instance { get; private set; }

//    private InputAction quitAction;

//    public InputActionAsset InputSystem_Actions;

//    private void Awake()
//    {
//        // Ensure a single persistent App instance
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        //DontDestroyOnLoad(gameObject);
//    }

//    private void OnEnable()
//    {
//        SceneManager.sceneLoaded += OnSceneLoaded;
//    }

//    private void OnDisable()
//    {
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }

//    IEnumerator Start()
//    {
//        // Load the minimal application scene (OutGame) or keep current scene as the UI host.
//        // Do not trigger any grid generation here; generation will happen when Map1 is loaded.
//        yield return StartCoroutine(LoadSceneAdditiveIfNeeded("OutGame"));
//    }

//    // Public wrapper so other scripts (MenuButtons) can request map load + generation:
//    public void StartLoadMapAndGenerate(string sceneName)
//    {
//        StartCoroutine(LoadMapAndGenerate(sceneName));
//    }

//    // This callback will run whenever any scene is loaded. We start grid generation
//    // when Map1 becomes available so the App scene stays minimal.
//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        if (scene.name == "Map1")
//        {
//            // Kick off generation coroutine so we don't block the sceneLoaded event.
//            StartCoroutine(GenerateGridInScene(scene));
//        }
//    }

//    private IEnumerator LoadSceneAdditiveIfNeeded(string sceneName)
//    {
//        // If scene already loaded, return.
//        var existing = SceneManager.GetSceneByName(sceneName);
//        if (!existing.IsValid())
//        {
//            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
//            if (loadOp == null)
//            {
//                Debug.LogError($"Failed to start loading scene '{sceneName}'. Make sure it's added to Build Settings.");
//                yield break;
//            }

//            while (!loadOp.isDone)
//                yield return null;
//        }

//        yield break;
//    }

//    // Find GridManager-like components in the loaded scene and invoke a generation method.
//    // Uses reflection to avoid hard compile-time dependencies.
//    private IEnumerator GenerateGridInScene(Scene scene)
//    {
//        // Wait one frame to ensure all Awake/Start have run in the just-loaded scene.
//        yield return null;

//        var rootObjects = scene.GetRootGameObjects();
//        foreach (var root in rootObjects)
//        {
//            var all = root.GetComponentsInChildren<MonoBehaviour>(true);
//            foreach (var mb in all)
//            {
//                if (mb == null) continue;

//                var type = mb.GetType();
//                // Look for components named GridManager (or a helper) and try common method names.
//                if (type.Name == "GridManager" || type.Name == "TileGridBootstrapHelper")
//                {
//                    var initMethod = type.GetMethod("Generate") ?? type.GetMethod("InitializeGrid") ?? type.GetMethod("Init");
//                    if (initMethod != null)
//                    {
//                        initMethod.Invoke(mb, null);
//                        Debug.Log($"Invoked {initMethod.Name} on {type.Name} in scene {scene.name}");
//                    }
//                }
//            }
//        }
//    }

//    private IEnumerator LoadMapAndGenerate(string sceneName)
//    {
//        // Load additively and wait
//        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
//        if (loadOp == null)
//        {
//            Debug.LogError($"Failed to start loading scene '{sceneName}'.");
//            yield break;
//        }

//        while (!loadOp.isDone)
//            yield return null;

//        // Retrieve the loaded scene
//        var scene = SceneManager.GetSceneByName(sceneName);
//        if (!scene.IsValid())
//        {
//            Debug.LogError($"Loaded scene '{sceneName}' is not valid.");
//            yield break;
//        }

//        // Wait one frame so Awake/Start have executed in that scene
//        yield return null;

//        // Optionally make the new scene active
//        SceneManager.SetActiveScene(scene);

//        // Find all GridManager components inside the loaded scene and call Generate()
//        foreach (var root in scene.GetRootGameObjects())
//        {
//            var gridManagers = root.GetComponentsInChildren<GridManager>(true);
//            foreach (var gm in gridManagers)
//            {
//                gm.Generate();
//            }
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}


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
        DontDestroyOnLoad(gameObject);
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
            if (loadOp == null)
            {
                Debug.LogError($"Failed to start loading scene '{sceneName}'.");
                yield break;
            }
            while (!loadOp.isDone) yield return null;
        }
    }

    private IEnumerator LoadMapAndGenerate(string sceneName)
    {
        // 1. Load the scene additively
        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError($"Failed to start loading scene '{sceneName}'.");
            yield break;
        }

        // Wait until fully loaded
        while (!loadOp.isDone)
            yield return null;

        // 2. Get the scene reference
        var scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            Debug.LogError($"Loaded scene '{sceneName}' is not valid.");
            yield break;
        }

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