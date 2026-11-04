
using UnityEngine;
using Vuforia;

public class ForgyLightEstimation_New : MonoBehaviour
{
    public Light forgyLight;

    [Header("Directional Light")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 0.8f;
    public float smoothing = 1.5f;

    [Header("Dark Room Detection")]
    public float darkThreshold = 350f;
    public float lightThreshold = 700f;

    private float targetIntensity;

    void Start()
    {
        targetIntensity = forgyLight != null
            ? forgyLight.intensity
            : 0.5f;
    }

    void Update()
    {
        if (VuforiaBehaviour.Instance != null &&
            VuforiaBehaviour.Instance.World != null)
        {
            float? ambient = VuforiaBehaviour.Instance.World
                .IlluminationData.AmbientIntensity;

            if (ambient.HasValue && ambient.Value >= 0f)
            {
                float brightness = Mathf.InverseLerp(
                    200f, 1500f, ambient.Value);

                // Keep our previous lighting behavior
                float normalIntensity = Mathf.Lerp(
                    minIntensity,
                    maxIntensity,
                    brightness);

                // 0 = dark, 1 = sufficiently bright
                float lightFactor = Mathf.InverseLerp(
                    darkThreshold,
                    lightThreshold,
                    ambient.Value);

                // Smooth transition between dark and bright
                lightFactor = Mathf.SmoothStep(
                    0f, 1f, lightFactor);

                // No directional lighting in very dark areas
                targetIntensity = normalIntensity * lightFactor;
            }
        }

        if (forgyLight != null)
        {
            float t = 1f - Mathf.Exp(
                -smoothing * Time.deltaTime);

            forgyLight.intensity = Mathf.Lerp(
                forgyLight.intensity,
                targetIntensity,
                t);
        }
    }
}
