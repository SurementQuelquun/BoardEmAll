#if UNITY_EDITOR
using UnityEditor;
#endif
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
        SceneManager.UnloadScene("OutGame");
        SceneManager.LoadScene("Map1", LoadSceneMode.Additive);

    }
}