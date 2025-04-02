using UnityEngine;
using DG.Tweening;
using System.Data;
using Unity.PlasticSCM.Editor.WebApi;

public class CastLevelFireFlicker : MonoBehaviour
{
    [Header("Texture Reference")]
    public SpriteRenderer texture;

    [Header("Flicker Settings")]
    public float flickerSpeed = 0.2F;
    public float maxBrightness = 0.3F;
    public float minBrightness = 0;
    public float maxDelayBetweenFlicker = 0.1F; 
    private float alphaTarget;
    private float delayCounter = 0;

    private Tweener currentTween;

	void FixedUpdate()
	{
        if(delayCounter > 0){
            delayCounter -= Time.deltaTime;
            return;
        }
		if (flickerSpeed > 0 && texture != null && currentTween == null)
        {
            alphaTarget = Random.Range(minBrightness, maxBrightness);
            Color targetColor = texture.color;
            targetColor.a = alphaTarget;
            currentTween = texture.DOColor(targetColor, flickerSpeed)
                .SetEase(Ease.OutQuad) // Optional: choose an easing function
                .OnComplete(() => {
                    delayCounter = Random.Range(0, maxDelayBetweenFlicker);
                    currentTween = null;
                });
        }
	}
}