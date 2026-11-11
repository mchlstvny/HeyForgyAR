
using UnityEngine;

public class ForgyIdle : MonoBehaviour
{
    [Header("Bone References")]
    public Transform leftArm;
    public Transform rightArm;
    public Transform headBone;

    [Header("Idle Arm Movement")]
    public float armIdleAngle = 5f;
    public float armIdleSpeed = 0.9f;

    [Header("Idle Head Movement")]
    public float headTiltAngle = 3.5f;
    public float headNodAngle = 2f;
    public float headIdleSpeed = 0.7f;
    public float headTurnAngle = 2.5f;

    [Header("Smooth Movement")]
    public float smoothSpeed = 5f;

    private Quaternion leftRestRotation;
    private Quaternion rightRestRotation;
    private Quaternion headRestRotation;

    private ForgyWave wave;
    private ForgyCheer cheer;
    private ForgyRelax relax;

    void Awake()
    {
        wave = GetComponent<ForgyWave>();
        cheer = GetComponent<ForgyCheer>();
        relax = GetComponent<ForgyRelax>();
    }

    void Start()
    {
        if (leftArm != null)
            leftRestRotation = leftArm.localRotation;

        if (rightArm != null)
            rightRestRotation = rightArm.localRotation;

        if (headBone != null)
            headRestRotation = headBone.localRotation;
    }

    void LateUpdate()
    {
        bool isWaving =
            wave != null && wave.IsWaving;

        bool isCheering =
            cheer != null && cheer.IsCheering;

        bool isRelaxing =
            relax != null && relax.IsRelaxing;

        // Do not fight with other animations.
        if (isWaving || isCheering || isRelaxing)
            return;

        float time = Time.time;

        float smooth = 1f - Mathf.Exp(
            -smoothSpeed * Time.deltaTime
        );

        // LEFT ARM
        // Two overlapping waves create a less
        // mechanical, more organic movement.
        if (leftArm != null)
        {
            float movement =
                Mathf.Sin(time * armIdleSpeed) * 0.7f +
                Mathf.Sin(
                    time * armIdleSpeed * 0.43f + 1.1f
                ) * 0.3f;

            float angle = movement * armIdleAngle;

            Quaternion target =
                leftRestRotation *
                Quaternion.Euler(
                    -angle * 0.15f,
                    0f,
                    angle
                );

            leftArm.localRotation = Quaternion.Slerp(
                leftArm.localRotation,
                target,
                smooth
            );
        }

        // RIGHT ARM
        // Different frequency and phase so both
        // arms don't move like a mirror.
        if (rightArm != null)
        {
            float movement =
                Mathf.Sin(
                    time * armIdleSpeed * 0.87f + 1.5f
                ) * 0.65f +
                Mathf.Sin(
                    time * armIdleSpeed * 0.39f + 2.4f
                ) * 0.35f;

            float angle = movement * armIdleAngle;

            Quaternion target =
                rightRestRotation *
                Quaternion.Euler(
                    -angle * 0.15f,
                    0f,
                    -angle
                );

            rightArm.localRotation = Quaternion.Slerp(
                rightArm.localRotation,
                target,
                smooth
            );
        }

        // HEAD
        // Gentle tilting, breathing-like nodding,
        // and very slow side-to-side looking.
        if (headBone != null)
        {
            float tilt =
                (
                    Mathf.Sin(
                        time * headIdleSpeed
                    ) * 0.7f +
                    Mathf.Sin(
                        time * headIdleSpeed * 0.41f + 1.7f
                    ) * 0.3f
                ) * headTiltAngle;

            float nod =
                (
                    Mathf.Sin(
                        time * headIdleSpeed * 0.72f
                    ) * 0.8f +
                    Mathf.Sin(
                        time * headIdleSpeed * 1.3f
                    ) * 0.2f
                ) * headNodAngle;

            float look =
                Mathf.Sin(
                    time * headIdleSpeed * 0.35f + 0.8f
                ) * headTurnAngle;

            Quaternion target =
                headRestRotation *
                Quaternion.Euler(
                    nod,
                    look,
                    tilt
                );

            headBone.localRotation = Quaternion.Slerp(
                headBone.localRotation,
                target,
                smooth
            );
        }
    }
}
