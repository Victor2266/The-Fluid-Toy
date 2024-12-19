using UnityEngine;
using System.Collections.Generic;

public class ClickSoundPlayer : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Left Click Sounds")]
    [SerializeField] private List<AudioClip> leftClickSounds = new List<AudioClip>();
    [SerializeField] private float leftClickMinPitch = 0.95f;
    [SerializeField] private float leftClickMaxPitch = 1.05f;
    [SerializeField] private float leftClickVolume = 1f;

    [Header("Right Click Sounds")]
    [SerializeField] private List<AudioClip> rightClickSounds = new List<AudioClip>();
    [SerializeField] private float rightClickMinPitch = 0.95f;
    [SerializeField] private float rightClickMaxPitch = 1.05f;
    [SerializeField] private float rightClickVolume = 1f;

    private void Start()
    {
        // If no audio source assigned, try to get or add one
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configure audio source
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        // Check for left click
        if (Input.GetMouseButtonDown(0))
        {
            PlayRandomSound(leftClickSounds, leftClickVolume, leftClickMinPitch, leftClickMaxPitch);
        }

        // Check for right click
        if (Input.GetMouseButtonDown(1))
        {
            PlayRandomSound(rightClickSounds, rightClickVolume, rightClickMinPitch, rightClickMaxPitch);
        }
    }

    private void PlayRandomSound(List<AudioClip> soundList, float volume, float minPitch, float maxPitch)
    {
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("No sound clips assigned to the list!");
            return;
        }

        // Get random sound from the list
        int randomIndex = Random.Range(0, soundList.Count);
        AudioClip randomClip = soundList[randomIndex];

        if (randomClip != null)
        {
            // Randomize pitch slightly for variety
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.volume = volume;
            audioSource.PlayOneShot(randomClip);
        }
        else
        {
            Debug.LogWarning("Null audio clip found in the list!");
        }
    }

    // Public methods to play sounds programmatically if needed
    public void PlayRandomLeftClickSound()
    {
        PlayRandomSound(leftClickSounds, leftClickVolume, leftClickMinPitch, leftClickMaxPitch);
    }

    public void PlayRandomRightClickSound()
    {
        PlayRandomSound(rightClickSounds, rightClickVolume, rightClickMinPitch, rightClickMaxPitch);
    }
}