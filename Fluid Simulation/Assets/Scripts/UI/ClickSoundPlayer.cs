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
    [SerializeField] private bool enableLeftClickHoldSound = false; // Toggle for continuous left click sound

    [Header("Right Click Sounds")]
    [SerializeField] private List<AudioClip> rightClickSounds = new List<AudioClip>();
    [SerializeField] private float rightClickMinPitch = 0.95f;
    [SerializeField] private float rightClickMaxPitch = 1.05f;
    [SerializeField] private float rightClickVolume = 1f;
    [SerializeField] private bool enableRightClickHoldSound = false; // Toggle for continuous right click sound

    private AudioClip currentLeftClickSound;
    private AudioClip currentRightClickSound;

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
        // Handle left click
        if (Input.GetMouseButtonDown(0))
        {
            if (enableLeftClickHoldSound)
            {
                // Start continuous play
                currentLeftClickSound = GetRandomSound(leftClickSounds);
                if (currentLeftClickSound != null)
                {
                    audioSource.loop = true;
                    audioSource.clip = currentLeftClickSound;
                    audioSource.pitch = Random.Range(leftClickMinPitch, leftClickMaxPitch);
                    audioSource.volume = leftClickVolume;
                    audioSource.Play();
                }
            }
            else
            {
                // Play single sound
                PlayRandomSound(leftClickSounds, leftClickVolume, leftClickMinPitch, leftClickMaxPitch);
            }
        }
        else if (Input.GetMouseButtonUp(0) && enableLeftClickHoldSound && currentLeftClickSound != null)
        {
            if (audioSource.clip == currentLeftClickSound)
            {
                audioSource.Stop();
                audioSource.loop = false;
                currentLeftClickSound = null;
            }
        }

        // Handle right click
        if (Input.GetMouseButtonDown(1))
        {
            if (enableRightClickHoldSound)
            {
                // Start continuous play
                currentRightClickSound = GetRandomSound(rightClickSounds);
                if (currentRightClickSound != null)
                {
                    audioSource.loop = true;
                    audioSource.clip = currentRightClickSound;
                    audioSource.pitch = Random.Range(rightClickMinPitch, rightClickMaxPitch);
                    audioSource.volume = rightClickVolume;
                    audioSource.Play();
                }
            }
            else
            {
                // Play single sound
                PlayRandomSound(rightClickSounds, rightClickVolume, rightClickMinPitch, rightClickMaxPitch);
            }
        }
        else if (Input.GetMouseButtonUp(1) && enableRightClickHoldSound && currentRightClickSound != null)
        {
            if (audioSource.clip == currentRightClickSound)
            {
                audioSource.Stop();
                audioSource.loop = false;
                currentRightClickSound = null;
            }
        }
    }

    private AudioClip GetRandomSound(List<AudioClip> soundList)
    {
        if (soundList == null || soundList.Count == 0)
        {
            Debug.LogWarning("No sound clips assigned to the list!");
            return null;
        }

        int randomIndex = Random.Range(0, soundList.Count);
        AudioClip randomClip = soundList[randomIndex];

        if (randomClip == null)
        {
            Debug.LogWarning("Null audio clip found in the list!");
        }

        return randomClip;
    }

    private void PlayRandomSound(List<AudioClip> soundList, float volume, float minPitch, float maxPitch)
    {
        AudioClip randomClip = GetRandomSound(soundList);
        if (randomClip != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.volume = volume;
            audioSource.PlayOneShot(randomClip);
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