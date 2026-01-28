using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameOver : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string _mapSceneName = "Map1";
    private string _menuSceneName = "OutGame";
    private string _gameOverSceneName = "GameOver";

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