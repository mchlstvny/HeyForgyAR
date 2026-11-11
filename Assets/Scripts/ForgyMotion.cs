
using UnityEngine;

public class ForgyMotion : MonoBehaviour
{
    [Header("Bone References")]
    public Transform leftArm;
    public Transform rightArm;
    public Transform headBone;
    public Transform bodyBone;

    [Header("Natural Idle Movement")]
    public float armIdleAngle = 4f;
    public float armIdleSpeed = 1.2f;
    public float headIdleAngle = 2f;
    public float bodySwayAngle = 1.5f;

    [Header("Wave")]
    public float waveRaiseAngle = -93f;
    public float waveAmplitude = 12f;
    public float waveCycles = 2.5f;
    public float waveRaiseDuration = 0.5f;
    public float waveHoldDuration = 1.6f;
    public float waveLowerDuration = 0.65f;

    [Header("Cheer")]
    public float cheerLeftAngle = -93f;
    public float cheerRightAngle = 93f;
    public float cheerRaiseDuration = 0.5f;
    public float cheerHoldDuration = 0.7f;
    public float cheerLowerDuration = 0.8f;

    [Header("Relax")]
    public float relaxHeadAngle = 15f;
    public float relaxTiltDuration = 0.7f;
    public float relaxHoldDuration = 2f;
    public float relaxReturnDuration = 0.8f;

    [Header("Smoothing")]
    public float followSpeed = 14f;

    private Quaternion leftRest;
    private Quaternion rightRest;
    private Quaternion headRest;
    private Quaternion bodyRest;

    private enum Motion
    {
        Idle,
        Wave,
        Cheer,
        Relax
    }

    private Motion currentMotion = Motion.Idle;
    private float motionTime;

    void Start()
    {
        // Save the current relaxed pose from the scene.
        if (leftArm != null)
            leftRest = leftArm.localRotation;

        if (rightArm != null)
            rightRest = rightArm.localRotation;

        if (headBone != null)
            headRest = headBone.localRotation;

        if (bodyBone != null)
            bodyRest = bodyBone.localRotation;
    }

    void LateUpdate()
    {
        if (currentMotion != Motion.Idle)
        {
            motionTime += Time.deltaTime;

            if (motionTime >= GetMotionDuration())
            {
                currentMotion = Motion.Idle;
                motionTime = 0f;
            }
        }

        // Gentle movement while floating.
        float leftAngle =
            Mathf.Sin(Time.time * armIdleSpeed)
            * armIdleAngle;

        float rightAngle =
            Mathf.Sin(Time.time * armIdleSpeed + 1.4f)
            * armIdleAngle;

        float headAngle =
            Mathf.Sin(Time.time * 0.85f)
            * headIdleAngle;

        float headNod =
            Mathf.Sin(Time.time * 0.65f) * 0.8f;

        float bodyAngle =
            Mathf.Sin(Time.time * 0.9f)
            * bodySwayAngle;

        if (currentMotion == Motion.Wave)
        {
            float influence = GetInfluence(
                waveRaiseDuration,
                waveHoldDuration,
                waveLowerDuration
            );

            // Raise left arm from resting pose.
            leftAngle =
                leftAngle * (1f - influence)
                + waveRaiseAngle * influence;

            // Small repeated waving movement.
            if (motionTime >= waveRaiseDuration &&
                motionTime < waveRaiseDuration + waveHoldDuration)
            {
                float t = Mathf.Clamp01(
                    (motionTime - waveRaiseDuration)
                    / Mathf.Max(0.01f, waveHoldDuration)
                );

                float envelope = Mathf.Sin(t * Mathf.PI);

                leftAngle +=
                    Mathf.Sin(t * waveCycles * Mathf.PI * 2f)
                    * waveAmplitude * envelope;
            }

            headAngle =
                headAngle * (1f - influence)
                + 7f * influence;

            bodyAngle += 2f * influence;
        }
        else if (currentMotion == Motion.Cheer)
        {
            float influence = GetInfluence(
                cheerRaiseDuration,
                cheerHoldDuration,
                cheerLowerDuration
            );

            leftAngle =
                leftAngle * (1f - influence)
                + cheerLeftAngle * influence;

            rightAngle =
                rightAngle * (1f - influence)
                + cheerRightAngle * influence;

            bodyAngle +=
                Mathf.Sin(motionTime * 5f)
                * 2f * influence;
        }
        else if (currentMotion == Motion.Relax)
        {
            float influence = GetInfluence(
                relaxTiltDuration,
                relaxHoldDuration,
                relaxReturnDuration
            );

            headAngle =
                headAngle * (1f - influence)
                + relaxHeadAngle * influence;

            headNod *= 1f - influence;
        }

        // Smooth transitions between poses.
        float smoothing =
            1f - Mathf.Exp(-followSpeed * Time.deltaTime);

        if (leftArm != null)
        {
            Quaternion target =
                leftRest * Quaternion.Euler(0f, 0f, leftAngle);

            leftArm.localRotation = Quaternion.Slerp(
                leftArm.localRotation, target, smoothing
            );
        }

        if (rightArm != null)
        {
            Quaternion target =
                rightRest * Quaternion.Euler(0f, 0f, rightAngle);

            rightArm.localRotation = Quaternion.Slerp(
                rightArm.localRotation, target, smoothing
            );
        }

        if (headBone != null)
        {
            Quaternion target =
                headRest * Quaternion.Euler(
                    headNod, 0f, headAngle
                );

            headBone.localRotation = Quaternion.Slerp(
                headBone.localRotation, target, smoothing
            );
        }

        if (bodyBone != null)
        {
            Quaternion target =
                bodyRest * Quaternion.Euler(0f, 0f, bodyAngle);

            bodyBone.localRotation = Quaternion.Slerp(
                bodyBone.localRotation, target, smoothing
            );
        }
    }

    private float GetInfluence(
        float raiseDuration,
        float holdDuration,
        float lowerDuration)
    {
        raiseDuration = Mathf.Max(0.01f, raiseDuration);
        lowerDuration = Mathf.Max(0.01f, lowerDuration);

        float holdEnd = raiseDuration + holdDuration;

        if (motionTime < raiseDuration)
        {
            return Mathf.SmoothStep(
                0f, 1f, motionTime / raiseDuration
            );
        }

        if (motionTime < holdEnd)
            return 1f;

        return Mathf.SmoothStep(
            1f, 0f,
            Mathf.Clamp01(
                (motionTime - holdEnd) / lowerDuration
            )
        );
    }

    private float GetMotionDuration()
    {
        switch (currentMotion)
        {
            case Motion.Wave:
                return waveRaiseDuration
                    + waveHoldDuration
                    + waveLowerDuration;

            case Motion.Cheer:
                return cheerRaiseDuration
                    + cheerHoldDuration
                    + cheerLowerDuration;

            case Motion.Relax:
                return relaxTiltDuration
                    + relaxHoldDuration
                    + relaxReturnDuration;

            default:
                return 0f;
        }
    }

    public void Wave()
    {
        currentMotion = Motion.Wave;
        motionTime = 0f;
    }

    public void Cheer()
    {
        currentMotion = Motion.Cheer;
        motionTime = 0f;
    }

    public void Relax()
    {
        currentMotion = Motion.Relax;
        motionTime = 0f;
    }
}
