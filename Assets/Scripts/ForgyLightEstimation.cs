
using UnityEngine;
using Vuforia;

public class ForgyLightEstimation : MonoBehaviour
{
    public Light forgyLight;

    public float minIntensity = 0.2f;
    public float maxIntensity = 2f;
    public float smoothing = 0.5f;

    private float targetIntensity = 1f;

    void Start()
    {
        if (forgyLight != null)
            targetIntensity = forgyLight.intensity;
    }

    void Update()
    {
        if (VuforiaBehaviour.Instance != null &&
            VuforiaBehaviour.Instance.World != null)
        {
            float? ambient = VuforiaBehaviour.Instance
                .World.IlluminationData.AmbientIntensity;

            if (ambient.HasValue && ambient.Value >= 0f)
            {
            
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log("Forgy Light Estimation: " +
                    (ambient.HasValue
                        ? ambient.Value.ToString("F0")
                        : "UNAVAILABLE"));
            }

                // Normalize the estimated ambient brightness.
                float brightness = Mathf.InverseLerp(
                    200f, 1500f, ambient.Value);

                targetIntensity = Mathf.Lerp(
                    minIntensity, maxIntensity, brightness);
            }
        }

        if (forgyLight != null)
        {
            forgyLight.intensity = Mathf.Lerp(
                forgyLight.intensity,
                targetIntensity,
                Time.deltaTime * smoothing);
        }
    }
}
