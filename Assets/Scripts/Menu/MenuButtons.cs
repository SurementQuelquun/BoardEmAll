#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuButtons : MonoBehaviour
{
    private Button QuitButton;
    private Button startButton;


    [System.Obsolete]
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        QuitButton = uiDocument.rootVisualElement.Q("QuitButton") as Button;
        QuitButton.RegisterCallback<ClickEvent>(QuitCallback);

        startButton = uiDocument.rootVisualElement.Q("PlayButton") as Button;
        startButton.RegisterCallback<ClickEvent>(PlayCallback);

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

    [System.Obsolete]
    private void PlayCallback(ClickEvent evt)
    {
        // Start coroutine that loads Map1 and sets it active once loading finishes
        StartCoroutine(LoadMapAndSetActive("Map1", "OutGame"));
    }

    private IEnumerator LoadMapAndSetActive(string sceneToLoad, string sceneToUnload = null)
    {
        // Start loading the scene additively
        var loadOp = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        // Wait until the load finishes
        while (!loadOp.isDone)
            yield return null;

        // Ensure scene is available, then set it active
        var newScene = SceneManager.GetSceneByName(sceneToLoad);
        SceneManager.SetActiveScene(newScene);



        // Optionally unload the previous scene (if provided)
        if (!string.IsNullOrEmpty(sceneToUnload))
        {
            var unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
            if (unloadOp != null)
            {
                while (!unloadOp.isDone)
                    yield return null;
            }
        }
    }
}