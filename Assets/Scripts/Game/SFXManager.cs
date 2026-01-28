using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Assign AudioClips here")]
    public AudioClip selectTowerClip;
    public AudioClip placeTowerClip;
    public AudioClip projectileShootClip;
    public AudioClip seaUrchinAttackClip;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton simple
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // Fonction pour jouer un son
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }
}

