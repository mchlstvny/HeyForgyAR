
using System.Collections;
using UnityEngine;

public class ForgyCheer : MonoBehaviour
{
    [Header("Arm Bones")]
    public Transform leftArm;
    public Transform rightArm;

    [Header("Superhero Flying Pose")]
    public Vector3 forwardArmRotation =
        new Vector3(-35f, 0f, -55f);

    public Vector3 otherArmRotation =
        new Vector3(0f, 0f, 15f);

    [Header("Victory Pose")]
    public float victoryLeftAngle = -85f;
    public float victoryRightAngle = 85f;

    [Header("Animation Timing")]
    public float prepareDuration = 0.3f;
    public float takeoffDuration = 0.8f;
    public float flipDuration = 0.95f;
    public float victoryDuration = 0.65f;
    public float landingDuration = 0.9f;
    public float returnDuration = 0.55f;

    [Header("Flying Path")]
    public float forwardDistance = -0.18f;
    public float backwardDistance = 0.10f;
    public float flightHeight = 0.22f;
    public float prepareDip = 0.025f;
    public float flipLift = 0.035f;

    [Header("Somersault")]
    public bool enableSomersault = true;
    public float somersaultDirection = 1f;

    [Header("Playful Details")]
    public float armWiggleAngle = 3f;
    public float landingBounce = 0.035f;

    private Quaternion leftRestRotation;
    private Quaternion rightRestRotation;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private Coroutine cheerCoroutine;

    private ForgyWave wave;
    private ForgyRelax relax;

    // Used by ForgyIdle and ForgyEyes
    public bool IsCheering => cheerCoroutine != null;

    void Awake()
    {
        wave = GetComponent<ForgyWave>();
        relax = GetComponent<ForgyRelax>();
    }

    void Start()
    {
        if (leftArm != null)
            leftRestRotation = leftArm.localRotation;

        if (rightArm != null)
            rightRestRotation = rightArm.localRotation;
    }

    public void Cheer()
    {
        if (leftArm == null || rightArm == null)
        {
            Debug.LogWarning(
                "ForgyCheer: Arm bones are missing!"
            );
            return;
        }

        if (cheerCoroutine != null)
            return;

        if (wave != null && wave.IsWaving)
            return;

        if (relax != null && relax.IsRelaxing)
            return;

        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        cheerCoroutine = StartCoroutine(PlayCheer());
    }

    private IEnumerator PlayCheer()
    {
        Quaternion leftStart = leftArm.localRotation;
        Quaternion rightStart = rightArm.localRotation;

        Quaternion leftFlying =
            leftRestRotation *
            Quaternion.Euler(forwardArmRotation);

        Quaternion rightFlying =
            rightRestRotation *
            Quaternion.Euler(otherArmRotation);

        Quaternion leftVictory =
            leftRestRotation *
            Quaternion.Euler(
                0f, 0f, victoryLeftAngle
            );

        Quaternion rightVictory =
            rightRestRotation *
            Quaternion.Euler(
                0f, 0f, victoryRightAngle
            );

        // ===================================
        // PHASE 1: ANTICIPATION
        // ===================================

        float elapsed = 0f;
        float duration = Mathf.Max(
            0.01f, prepareDuration
        );

        while (elapsed < duration)
        {
            float t = Ease(elapsed / duration);

            ApplyPose(
                Vector3.down * prepareDip * t,
                -10f * t,
                Quaternion.Slerp(
                    leftStart, leftFlying, t
                ),
                Quaternion.Slerp(
                    rightStart, rightFlying, t
                )
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ===================================
        // PHASE 2: CONTINUOUS FLIGHT
        // ===================================

        // Flight points in Forgy's local space.
        // The full path is interpolated smoothly.
        Vector3[] points =
        {
            new Vector3(0f, -prepareDip, 0f),

            // Forward and up
            new Vector3(
                0f,
                flightHeight * 0.8f,
                forwardDistance
            ),

            // Highest point during the flip
            new Vector3(
                0f,
                flightHeight + flipLift,
                backwardDistance * 0.5f
            ),

            // Victory while airborne
            new Vector3(
                0f,
                flightHeight * 0.8f,
                backwardDistance
            ),

            // Return to original hovering point
            Vector3.zero
        };

        float[] segmentDurations =
        {
            Mathf.Max(0.01f, takeoffDuration),
            Mathf.Max(0.01f, flipDuration),
            Mathf.Max(0.01f, victoryDuration),
            Mathf.Max(0.01f, landingDuration)
        };

        float totalFlightDuration =
            segmentDurations[0] +
            segmentDurations[1] +
            segmentDurations[2] +
            segmentDurations[3];

        // Start flipping late in takeoff.
        float flipStart =
            segmentDurations[0] * 0.65f;

        // Finish the flip before the victory pose.
        float flipEnd =
            segmentDurations[0] +
            segmentDurations[1] +
            segmentDurations[2] * 0.4f;

        // Gradually change to both-arms-up pose.
        float victoryStart =
            segmentDurations[0] +
            segmentDurations[1] * 0.55f;

        float victoryEnd =
            segmentDurations[0] +
            segmentDurations[1] +
            segmentDurations[2] * 0.8f;

        float fullRotation =
            enableSomersault
            ? (somersaultDirection < 0f ? -360f : 360f)
            : 0f;

        elapsed = 0f;

        while (elapsed < totalFlightDuration)
        {
            float progress = Mathf.Clamp01(
                elapsed / totalFlightDuration
            );

            // One continuous smooth flight path.
            Vector3 position = EvaluateFlight(
                points,
                segmentDurations,
                elapsed
            );

            // Ease only the somersault,
            // not every section of the flight.
            float flipT = Ease(
                Mathf.InverseLerp(
                    flipStart,
                    flipEnd,
                    elapsed
                )
            );

            float spin = fullRotation * flipT;

            // Gradually straighten the body.
            float naturalLean =
                Mathf.Lerp(
                    -10f,
                    0f,
                    Ease(progress)
                );

            float pitch = naturalLean + spin;

            float victoryT = Ease(
                Mathf.InverseLerp(
                    victoryStart,
                    victoryEnd,
                    elapsed
                )
            );

            Quaternion leftPose = Quaternion.Slerp(
                leftFlying,
                leftVictory,
                victoryT
            );

            Quaternion rightPose = Quaternion.Slerp(
                rightFlying,
                rightVictory,
                victoryT
            );

            // Subtle arm movement that fades
            // in and out naturally.
            float wiggle =
                Mathf.Sin(elapsed * 10f) *
                Mathf.Sin(progress * Mathf.PI) *
                armWiggleAngle *
                (0.25f + 0.75f * victoryT);

            leftPose *= Quaternion.Euler(
                0f, 0f, wiggle
            );

            rightPose *= Quaternion.Euler(
                0f, 0f, -wiggle
            );

            ApplyPose(
                position,
                pitch,
                leftPose,
                rightPose
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Finish at the starting position,
        // upright, with both arms celebrating.
        ApplyPose(
            Vector3.zero,
            fullRotation,
            leftVictory,
            rightVictory
        );

        // ===================================
        // PHASE 3: SOFT LANDING AND RECOVERY
        // ===================================

        elapsed = 0f;
        duration = Mathf.Max(
            0.01f, returnDuration
        );

        while (elapsed < duration)
        {
            float u = Mathf.Clamp01(
                elapsed / duration
            );

            float t = Ease(u);

            // Soft dip, followed by a rebound.
            // Both ends have gentle transitions.
            float bounce =
                -Mathf.Sin(t * Mathf.PI * 2f) *
                Mathf.Pow(
                    Mathf.Sin(t * Mathf.PI),
                    2f
                ) *
                landingBounce;

            float landingTilt =
                Mathf.Sin(t * Mathf.PI) * 4f;

            Quaternion leftPose = Quaternion.Slerp(
                leftVictory,
                leftRestRotation,
                t
            );

            Quaternion rightPose = Quaternion.Slerp(
                rightVictory,
                rightRestRotation,
                t
            );

            ApplyPose(
                Vector3.up * bounce,
                fullRotation + landingTilt,
                leftPose,
                rightPose
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        RestoreRestingPose();
        cheerCoroutine = null;
    }

    // Smooth flight path using cubic Hermite curves.
    // This maintains velocity continuity between
    // takeoff, flip, victory, and descent.
    private Vector3 EvaluateFlight(
        Vector3[] points,
        float[] durations,
        float elapsed)
    {
        int segment = 0;
        float remaining = elapsed;

        while (
            segment < durations.Length - 1 &&
            remaining > durations[segment]
        )
        {
            remaining -= durations[segment];
            segment++;
        }

        float segmentDuration = durations[segment];

        float t = Mathf.Clamp01(
            remaining / segmentDuration
        );

        // Velocities at the start and end
        // of the current path segment.
        Vector3 startVelocity = Vector3.zero;
        Vector3 endVelocity = Vector3.zero;

        if (segment > 0)
        {
            startVelocity =
                (points[segment + 1] -
                 points[segment - 1]) /
                (durations[segment - 1] +
                 durations[segment]);
        }

        if (segment < durations.Length - 1)
        {
            endVelocity =
                (points[segment + 2] -
                 points[segment]) /
                (durations[segment] +
                 durations[segment + 1]);
        }

        float t2 = t * t;
        float t3 = t2 * t;

        float h00 = 2f * t3 - 3f * t2 + 1f;
        float h10 = t3 - 2f * t2 + t;
        float h01 = -2f * t3 + 3f * t2;
        float h11 = t3 - t2;

        return
            h00 * points[segment] +
            h10 * startVelocity * segmentDuration +
            h01 * points[segment + 1] +
            h11 * endVelocity * segmentDuration;
    }

    private void ApplyPose(
        Vector3 offset,
        float pitch,
        Quaternion leftPose,
        Quaternion rightPose)
    {
        // Move only the forgy child.
        transform.localPosition =
            startPosition + offset;

        transform.localRotation =
            startRotation *
            Quaternion.Euler(pitch, 0f, 0f);

        leftArm.localRotation = leftPose;
        rightArm.localRotation = rightPose;
    }

    private float Ease(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    private void RestoreRestingPose()
    {
        transform.localPosition = startPosition;
        transform.localRotation = startRotation;

        if (leftArm != null)
            leftArm.localRotation = leftRestRotation;

        if (rightArm != null)
            rightArm.localRotation = rightRestRotation;
    }

    void OnDisable()
    {
        if (cheerCoroutine == null)
            return;

        StopCoroutine(cheerCoroutine);
        cheerCoroutine = null;

        RestoreRestingPose();
    }
}
