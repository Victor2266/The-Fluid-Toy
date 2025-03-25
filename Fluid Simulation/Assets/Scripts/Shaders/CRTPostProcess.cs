using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CRTPostProcess : MonoBehaviour
{
    [Header("Bloom Settings")]
    [Range(0, 1)] public float bloomThreshold = 0.5f;
    [Range(0, 10)] public float bloomIntensity = 1.5f;
    [Range(0, 10)] public float blurSize = 3f;
    [Range(0.1f, 5f)] public float softness = 1f;

    [Header("CRT Effects")]
    [Range(-1, 1)] public float paniniDistance = 0.2f;
    [Range(0.1f, 5f)] public float paniniCrop = 1.0f;
    [Range(0, 1)] public float vignetteIntensity = 0.5f;
    [Range(0, 2)] public float vignetteRadius = 0.5f;
    [Range(0, 1)] public float vignetteSmoothness = 0.2f;
    [Range(0, 0.1f)] public float chromaticAberration = 0.01f;
    public Vector2 chromaticDirection = new Vector2(1, 0);

    public Shader postProcessShader;
    private Material postProcessMaterial;

    void OnEnable()
    {
        if (postProcessShader == null)
            postProcessShader = Shader.Find("Hidden/CRTPostProcess");
    }

    private void OnDisable()
    {
        if (postProcessMaterial != null)
            DestroyImmediate(postProcessMaterial);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (postProcessShader == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (postProcessMaterial == null)
            postProcessMaterial = new Material(postProcessShader);

        // Set CRT effect parameters
        postProcessMaterial.SetFloat("_PaniniDistance", paniniDistance);
        postProcessMaterial.SetFloat("_PaniniCrop", paniniCrop);
        postProcessMaterial.SetFloat("_VignetteIntensity", vignetteIntensity);
        postProcessMaterial.SetFloat("_VignetteRadius", vignetteRadius);
        postProcessMaterial.SetFloat("_VignetteSmoothness", vignetteSmoothness);
        postProcessMaterial.SetFloat("_ChromaticAberration", chromaticAberration);
        postProcessMaterial.SetVector("_ChromaticDirection", chromaticDirection);

        // Bloom parameters
        postProcessMaterial.SetFloat("_BloomThreshold", bloomThreshold);
        postProcessMaterial.SetFloat("_BloomIntensity", bloomIntensity);
        postProcessMaterial.SetFloat("_BlurSize", blurSize);
        postProcessMaterial.SetFloat("_Softness", softness);

        RenderTexture brightPass = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        RenderTexture blur1 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, source.format);
        RenderTexture blur2 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, source.format);

        // Bloom extraction
        Graphics.Blit(source, brightPass, postProcessMaterial, 0);
        Graphics.Blit(brightPass, blur1, postProcessMaterial, 1);
        Graphics.Blit(blur1, blur2, postProcessMaterial, 2);

        postProcessMaterial.SetTexture("_BloomTex", blur2);
        Graphics.Blit(source, destination, postProcessMaterial, 3);

        RenderTexture.ReleaseTemporary(brightPass);
        RenderTexture.ReleaseTemporary(blur1);
        RenderTexture.ReleaseTemporary(blur2);
    }
}