using UnityEngine;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ParticlePostProcess : MonoBehaviour
{
    [Range(0, 1)]
    public float bloomThreshold = 0.5f;
    [Range(0, 10)]
    public float bloomIntensity = 1.5f;
    [Range(0, 10)]
    public float blurSize = 3f;
    [Range(0.1f, 5f)]
    public float softness = 1f;

    public Shader postProcessShader;
    private Material postProcessMaterial;

    void OnEnable()
    {
        if (postProcessShader == null)
        {
            postProcessShader = Shader.Find("Hidden/ParticlePostProcess");
            Debug.Log(postProcessShader == null ? "Failed to find shader" : "Found shader");
        }
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

        // Create temporary RenderTextures
        RenderTexture brightPass = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        RenderTexture blur1 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, source.format);
        RenderTexture blur2 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, source.format);

        postProcessMaterial.SetFloat("_BloomThreshold", bloomThreshold);
        postProcessMaterial.SetFloat("_BloomIntensity", bloomIntensity);
        postProcessMaterial.SetFloat("_BlurSize", blurSize);
        postProcessMaterial.SetFloat("_Softness", softness);

        // Extract bright areas
        Graphics.Blit(source, brightPass, postProcessMaterial, 0);

        // Blur the bright areas
        Graphics.Blit(brightPass, blur1, postProcessMaterial, 1);
        Graphics.Blit(blur1, blur2, postProcessMaterial, 2);

        // Set up the final combine
        postProcessMaterial.SetTexture("_BloomTex", blur2);
        Graphics.Blit(source, destination, postProcessMaterial, 3);

        // Clean up
        RenderTexture.ReleaseTemporary(brightPass);
        RenderTexture.ReleaseTemporary(blur1);
        RenderTexture.ReleaseTemporary(blur2);
    }
}