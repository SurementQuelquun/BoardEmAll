using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("UI")]
    public AudioSource uiClickSource;

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

    public void PlayUIClick()
    {
        if (!uiClickSource.isPlaying)
            uiClickSource.Play();
    }
}

