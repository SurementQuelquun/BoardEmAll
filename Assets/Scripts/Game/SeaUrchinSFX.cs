using UnityEngine;

public class SeaUrchinSFX : MonoBehaviour
{
    public AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayShockWave()
    {
        audioSource.Play();
    }
}

