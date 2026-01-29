using UnityEngine;

public class TowerSFX : MonoBehaviour
{
    public AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayShoot()
    {
        if (!audioSource.isPlaying)
            audioSource.Play();
    }
}
