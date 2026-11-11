
using UnityEngine;

public class ForgyWave : MonoBehaviour
{
    [Header("Bone References")]
    public Transform waveArm;
    public Transform bodyBone;
    public Transform headBone;

    [Header("Wave Settings")]
    public float raiseAngle = -65f;
    public float raiseDuration = 0.5f;
    public float waveDuration = 2f;
    public float lowerDuration = 0.65f;

    [Header("Arm Wave Movement")]
    public float waveAmplitude = 12f;
    public float waveSpeed = 3f;

    [Header("Natural Body Movement")]
    public float bodySwayAngle = 2f;
    public float bodySwaySpeed = 1.2f;
    public float waveBodyLean = 3f;

    [Header("Head Movement")]
    public float headTiltAngle = 8f;
    public float headNodAngle = 3f;

    private Quaternion armRestRotation;
    private Quaternion bodyRestRotation;
    private Quaternion headRestRotation;

    private float waveTimer;
    private bool isWaving;

    // Lets ForgyIdle check whether Wave is playing
    public bool IsWaving => isWaving;

    void Start()
    {
        if (waveArm != null)
            armRestRotation = waveArm.localRotation;

        if (bodyBone != null)
            bodyRestRotation = bodyBone.localRotation;

        if (headBone != null)
            headRestRotation = headBone.localRotation;
    }

    void LateUpdate()
    {
        if (isWaving)
        {
            waveTimer += Time.deltaTime;

            float totalDuration =
                raiseDuration + waveDuration + lowerDuration;

            if (waveTimer >= totalDuration)
            {
                isWaving = false;
                waveTimer = 0f;

                if (waveArm != null)
                    waveArm.localRotation = armRestRotation;

                if (headBone != null)
                    headBone.localRotation = headRestRotation;
            }
        }

        AnimateBody();

        if (isWaving)
        {
            AnimateArm();
            AnimateHead();
        }
    }

    void AnimateBody()
    {
        if (bodyBone == null)
            return;

        // Continuous gentle body sway
        float idleSway =
            Mathf.Sin(Time.time * bodySwaySpeed)
            * bodySwayAngle;

        float idleForward =
            Mathf.Sin(Time.time * bodySwaySpeed * 0.7f)
            * 0.7f;

        // Extra lean only during Wave
        float extraLean = 0f;

        if (isWaving)
        {
            float influence = GetWaveInfluence();
            extraLean = influence * waveBodyLean;
        }

        bodyBone.localRotation =
            bodyRestRotation *
            Quaternion.Euler(
                idleForward,
                0f,
                idleSway + extraLean
            );
    }

    void AnimateArm()
    {
        if (waveArm == null)
            return;

        float angle = 0f;

        float waveStart = raiseDuration;
        float waveEnd = raiseDuration + waveDuration;
        float totalDuration = waveEnd + lowerDuration;

        if (waveTimer < waveStart)
        {
            // Raise arm smoothly
            float t = SmoothEase(
                waveTimer / Mathf.Max(0.01f, raiseDuration)
            );

            angle = Mathf.Lerp(0f, raiseAngle, t);
        }
        else if (waveTimer < waveEnd)
        {
            // Gentle repeated waving
            float t =
                (waveTimer - waveStart) /
                Mathf.Max(0.01f, waveDuration);

            float envelope = Mathf.Sin(t * Mathf.PI);

            float waveMotion =
                Mathf.Sin(
                    t * waveSpeed * Mathf.PI * 2f
                ) * waveAmplitude * envelope;

            angle = raiseAngle + waveMotion;
        }
        else if (waveTimer < totalDuration)
        {
            // Lower arm smoothly
            float t = SmoothEase(
                (waveTimer - waveEnd) /
                Mathf.Max(0.01f, lowerDuration)
            );

            angle = Mathf.Lerp(raiseAngle, 0f, t);
        }

        waveArm.localRotation =
            armRestRotation *
            Quaternion.Euler(0f, 0f, angle);
    }

    void AnimateHead()
    {
        if (headBone == null)
            return;

        float influence = GetWaveInfluence();

        // Tilt head gently while waving
        float tilt = headTiltAngle * influence;

        // Small, playful head nod
        float nod =
            Mathf.Sin(waveTimer * 4f) *
            headNodAngle *
            influence;

        headBone.localRotation =
            headRestRotation *
            Quaternion.Euler(nod, 0f, tilt);
    }

    float GetWaveInfluence()
    {
        float totalDuration =
            raiseDuration + waveDuration + lowerDuration;

        float progress = Mathf.Clamp01(
            waveTimer / Mathf.Max(0.01f, totalDuration)
        );

        return Mathf.Sin(progress * Mathf.PI);
    }

    float SmoothEase(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    // Called by the Wave UI button
    public void Wave()
    {
        if (waveArm == null)
        {
            Debug.LogWarning("Forgy's wave arm is missing!");
            return;
        }

        waveTimer = 0f;
        isWaving = true;
    }
}
