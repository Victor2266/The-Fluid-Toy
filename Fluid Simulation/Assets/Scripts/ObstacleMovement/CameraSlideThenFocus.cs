using UnityEngine;
using DG.Tweening;

public class CameraSlideThenFocus : MonoBehaviour
{
    [Header("Camera Movement")]
    [Tooltip("The starting position of the Camera.")]
    public Vector3 startPosition = new Vector3(0f, 0f, -10f);
    [Tooltip("The final position of the Camera.")]
    public Vector3 endPosition = new Vector3(5f, 5f, -10f);
    [Tooltip("Duration for the camera move tween (in seconds).")]
    public float moveDuration = 3f;

    [Header("Camera Focusing (Orthographic Size)")]
    [Tooltip("The starting orthographic size of the Camera.")]
    public float startSize = 5f;
    [Tooltip("The intermediate orthographic size value.")]
    public float midSize = 7f;
    [Tooltip("The final orthographic size value.")]
    public float finalSize = 4f;
    [Tooltip("Duration for first focus tween (from startSize to midSize).")]
    public float focusDuration1 = 1.5f;
    [Tooltip("Duration for second focus tween (from midSize to finalSize).")]
    public float focusDuration2 = 1.5f;

    public AudioClip moveSound;
    public AudioSource audioSource;

    private void Start()
    {
        // Ensure we have a Camera component.
        Camera cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("No Camera component found on this GameObject!");
            return;
        }

        // Set initial values for position and orthographic size.
        transform.position = startPosition;
        cam.orthographicSize = startSize;

        // Optional: if you want the focus effect to exactly coincide with the camera move,
        // you can ensure moveDuration equals (focusDuration1 + focusDuration2).

        // Create a sequence to synchronize both the movement and the focusing effect.
        Sequence sequence = DOTween.Sequence();

        // Append the camera movement tween.
        sequence.Join(transform.DOMove(endPosition, moveDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            // Play the audio clip from the audio source.
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }));
        // Append an on complete callback to play the audio clip after the sequence completes.
        


        // Create a nested sequence for the camera orthographic size focusing effect.
        // This tween goes from startSize -> midSize and then from midSize -> finalSize.
        Sequence focusSequence = DOTween.Sequence();
        focusSequence.Append(
            DOTween.To(
                () => cam.orthographicSize,
                x => cam.orthographicSize = x,
                midSize,
                focusDuration1
            ).SetEase(Ease.OutBack)
        );
        focusSequence.Append(
            DOTween.To(
                () => cam.orthographicSize,
                x => cam.orthographicSize = x,
                finalSize,
                focusDuration2
            ).SetEase(Ease.InOutBack)
        );
        focusSequence.InsertCallback(focusDuration2, () =>
        {
            if (audioSource != null && moveSound != null)
            {
                audioSource.PlayOneShot(moveSound);
            }
        });
        // Join the focusing sequence to run concurrently with the camera move.
        sequence.Append(focusSequence);

        // (Optional) If you prefer everything to run exactly in parallel without sequencing,
        // you could start the tweens separately:
        //
        // transform.DOMove(endPosition, moveDuration).SetEase(Ease.InOutQuad);
        // DOTween.Sequence()
        //    .Append(DOTween.To(() => cam.orthographicSize, x => cam.orthographicSize = x, midSize, focusDuration1).SetEase(Ease.OutQuad))
        //    .Append(DOTween.To(() => cam.orthographicSize, x => cam.orthographicSize = x, finalSize, focusDuration2).SetEase(Ease.InQuad));

        // Play the sequence.
        sequence.Play();
    }
}
