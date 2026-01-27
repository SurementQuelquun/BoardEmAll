//using System.Collections;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UIElements;

//public class GameOver : MonoBehaviour
//{
//    [Header("Scenes")]
//    [Tooltip("Name of the gameplay scene to reload when replaying.")]
//    [SerializeField] private string _mapSceneName = "Map1";
//    [Tooltip("Name of the main menu / outgame scene.")]
//    [SerializeField] private string _menuSceneName = "OutGame";
//    [Tooltip("Name of the GameOver UI scene that will be loaded additively.")]
//    [SerializeField] private string _gameOverSceneName = "GameOver";

//    private bool _isGameOver = false;

//    private void OnEnable()
//    {
//        HealthManager.OnGameOver += HandleGameOver;
//    }

//    private void OnDisable()
//    {
//        HealthManager.OnGameOver -= HandleGameOver;
//    }

//    private void HandleGameOver()
//    {
//        if (_isGameOver) return;
//        _isGameOver = true;

//        //Debug.Log("GameOver: health reached 0. Stopping game.");

//        // Pause the game and audio immediately
//        Time.timeScale = 0f;
//        AudioListener.pause = true;

//        // Load the GameOver scene additively and set it active when ready.
//        StartCoroutine(LoadGameOverSceneAdditiveAndWire(_gameOverSceneName));
//    }

//    private IEnumerator LoadGameOverSceneAdditiveAndWire(string sceneName)
//    {
//        // If already loaded, set active and wire UI
//        var existing = SceneManager.GetSceneByName(sceneName);
//        if (existing.IsValid() && existing.isLoaded)
//        {
//            SceneManager.SetActiveScene(existing);
//            yield return null; // let one frame pass
//            WireGameOverUI(existing);
//            yield break;
//        }

//        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

//        while (!loadOp.isDone)
//            yield return null;

//        var loaded = SceneManager.GetSceneByName(sceneName);

//        SceneManager.SetActiveScene(loaded);

//        // Wait one frame so Awake/Start of that scene run and UIDocuments build their trees.
//        yield return null;

//        WireGameOverUI(loaded);

//    }

//    // Find UI Toolkit buttons in the loaded GameOver scene and wire their callbacks.
//    private void WireGameOverUI(Scene gameOverScene)
//    {
//        // Look for a UIDocument in the loaded scene
//        foreach (var root in gameOverScene.GetRootGameObjects())
//        {
//            var uidoc = root.GetComponentInChildren<UIDocument>(true);
//            if (uidoc == null) continue;

//            var rootVE = uidoc.rootVisualElement;
//            if (rootVE == null) continue;

//            // Try to find UI Toolkit Buttons by name
//            var menuBtn = rootVE.Q<UnityEngine.UIElements.Button>("MenuButton");
//            if (menuBtn != null)
//            {
//                menuBtn.clicked -= OnMenuButtonPressed;
//                menuBtn.clicked += OnMenuButtonPressed;
//            }

//            var replayBtn = rootVE.Q<UnityEngine.UIElements.Button>("ReplayButton");
//            if (replayBtn == null)
//            {
//                replayBtn = rootVE.Q<UnityEngine.UIElements.Button>("RestartButton");
//            }

//            if (replayBtn != null)
//            {
//                replayBtn.clicked -= OnReplayButtonPressed;
//                replayBtn.clicked += OnReplayButtonPressed;
//            }

//            // We've wired the first UIDocument we find.
//            return;
//        }

//        // Fallback: if no UIDocument found, try legacy GameObject tags (optional)
//        var menuGO = GameObject.FindWithTag("MenuButton");
//        if (menuGO != null)
//        {
//            menuGO.SetActive(true);
//            var uiBtn = menuGO.GetComponent<UnityEngine.UI.Button>();
//        }

//        var replayGO = GameObject.FindWithTag("RestartButton") ?? GameObject.FindWithTag("ReplayButton");
//        if (replayGO != null)
//        {
//            replayGO.SetActive(true);
//            var uiBtn = replayGO.GetComponent<UnityEngine.UI.Button>();
//        }
//    }

//    // Called when Menu button is pressed in GameOver UI
//    private void OnMenuButtonPressed()
//    {
//        // Resume time and audio before switching scenes
//        AudioListener.pause = false;
//        Time.timeScale = 1f;

//        // Load the menu scene (single mode will unload other scenes but App should be DontDestroyOnLoad)
//        SceneManager.LoadScene("App");
//        SceneManager.LoadScene(_menuSceneName, LoadSceneMode.Additive);
//    }

//    // Called when Replay button is pressed in GameOver UI
//    private void OnReplayButtonPressed()
//    {
//        // Resume audio/time before reload
//        AudioListener.pause = false;
//        Time.timeScale = 1f;

//        SceneManager.LoadScene("App");
//        SceneManager.LoadScene(_mapSceneName, LoadSceneMode.Additive);

//        //StartCoroutine(ReloadMapCoroutine(_mapSceneName));
//        //SceneManager.UnloadScene("GameOver");

//    }
//    //Public helper so other GameOver instances can be reset after reload.
//    public void ResetGameOverState()
//    {
//        _isGameOver = false;
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameOver : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string _mapSceneName = "Map1";
    [SerializeField] private string _menuSceneName = "OutGame";
    [SerializeField] private string _gameOverSceneName = "GameOver";

    private bool _isGameOver = false;

    private void OnEnable()
    {
        HealthManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        HealthManager.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;

        StartCoroutine(LoadGameOverScene());
    }

    private IEnumerator LoadGameOverScene()
    {
        // 1. Load the scene if it's not already there
        if (!SceneManager.GetSceneByName(_gameOverSceneName).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(_gameOverSceneName, LoadSceneMode.Additive);
        }

        // 2. Find the UIDocument specifically in the GameOver scene
        // (This prevents grabbing the HUD or Main Menu by mistake)
        var loadedScene = SceneManager.GetSceneByName(_gameOverSceneName);
        UIDocument uiDoc = null;

        foreach (var root in loadedScene.GetRootGameObjects())
        {
            uiDoc = root.GetComponentInChildren<UIDocument>();
            if (uiDoc != null) break;
        }

        if (uiDoc != null)
        {
            var rootVE = uiDoc.rootVisualElement;

            // Wire up Menu Button
            var menuBtn = rootVE.Q<Button>("MenuButton");
            menuBtn?.RegisterCallback<ClickEvent>(OnMenuCallback);

            // Wire up Replay Button
            var replayBtn = rootVE.Q<Button>("RestartButton");
            replayBtn?.RegisterCallback<ClickEvent>(OnReplayCallback);
        }
        else
        {
            Debug.LogError("GameOver Scene loaded, but NO UIDocument found inside it!");
        }
    }

    private void OnMenuCallback(ClickEvent evt)
    {
        ResetGameSystem();
        // Load App (resetting singletons) and the Menu
        SceneManager.LoadScene("App");
        SceneManager.LoadScene(_menuSceneName, LoadSceneMode.Additive);
    }

    private void OnReplayCallback(ClickEvent evt)
    {
        ResetGameSystem();

        // FIX: Start the sequence to reload just the Map
        StartCoroutine(ReloadMapSequence());
    }

    private void ResetGameSystem()
    {
        _isGameOver = false;
        AudioListener.pause = false;
        Time.timeScale = 1f;
    }

    private IEnumerator ReloadMapSequence()
    {
        // 1. Unload the GameOver UI
        yield return SceneManager.UnloadSceneAsync(_gameOverSceneName);

        // 2. Unload the current Map (if loaded)
        var currentMap = SceneManager.GetSceneByName(_mapSceneName);
        if (currentMap.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(_mapSceneName);
        }

        // 3. Load the Map fresh (App scene stays alive in background)
        yield return SceneManager.LoadSceneAsync(_mapSceneName, LoadSceneMode.Additive);

        // 4. Set active so lighting/navmesh work
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_mapSceneName));
    }
}