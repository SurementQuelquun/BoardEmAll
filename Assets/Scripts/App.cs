using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class App : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static App Instance { get; private set; }

    private InputAction quitAction;


    public InputActionAsset InputSystem_Actions;

    private void Awake()
    {
        // Ensure a single persistent App instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        this.quitAction = InputSystem.actions.FindAction("Quit");
        this.quitAction.Enable();
        yield return StartCoroutine(WaitAndPrint("OutGame"));
    }
    IEnumerator WaitAndPrint(string SceneName)
    {
        var loadOp = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError($"Failed to start loading scene '{SceneName}'. Make sure it's added to Build Settings.");
            yield break;
        }

        int frameCount = 0;
        // Wait until the async operation reports done, counting frames
        while (!loadOp.isDone)
        {
            frameCount++;
            yield return null;
        }

        //Debug.Log($"{SceneName} Scene Loaded in {frameCount} frames");
        //yield break;
    }



    // Update is called once per frame
    void Update()
    {

    }
}
