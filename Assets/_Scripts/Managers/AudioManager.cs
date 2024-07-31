using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource SFXObject;

    public static AudioManager Instance;

    private void Awake()
    {
        #region singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        #endregion
    }

    public void PlaySound(AudioClip clip, float volume)
    {
        AudioSource audioSource = Instantiate(SFXObject, transform, false);

        audioSource.clip = clip;
        audioSource.volume = volume;

        float clipLength = audioSource.clip.length;

        audioSource.Play();

        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlaySound(AudioClip clip, float volume, float pitch)
    {
        AudioSource audioSource = Instantiate(SFXObject, transform, false);

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        float clipLength = audioSource.clip.length;

        audioSource.Play();

        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlaySound(AudioClip clip, float volume, float basePitch, float pitchRandomVariation)
    {
        AudioSource audioSource = Instantiate(SFXObject, transform, false);

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = basePitch + Random.Range(-pitchRandomVariation, pitchRandomVariation);

        float clipLength = audioSource.clip.length;

        audioSource.Play();

        Destroy(audioSource.gameObject, clipLength + 0.25f);
    }
}
