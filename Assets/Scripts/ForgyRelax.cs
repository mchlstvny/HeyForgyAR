using System.Collections;
using UnityEngine;

public class ForgyRelax : MonoBehaviour
{
    [Header("Bone References")]
    public Transform headBone;
    public Transform leftArm;
    public Transform rightArm;

    [Header("Peeking Movement")]
    public float duckDepth = 0.06f;
    public float peekHeight = 0.16f;

    [Header("Animation Timing")]
    public float prepareDuration = 0.30f;
    public float peekDuration = 0.45f;
    public float holdDuration = 1.10f;
    public float returnDuration = 0.50f;

    [Header("Head Pose")]
    public Vector3 headPeekEuler = new Vector3(-10f, 0f, 14f);

    [Header("Arms Holding Sofa")]
    public Vector3 leftArmPeekEuler = new Vector3(-18f, 0f, -28f);
    public Vector3 rightArmPeekEuler = new Vector3(-18f, 0f, 28f);

    private Vector3 startLocalPosition;

    private Quaternion headRestRotation;
    private Quaternion leftArmRestRotation;
    private Quaternion rightArmRestRotation;

    private Coroutine relaxCoroutine;

    // Lets ForgyIdle know when Relax is playing
    public bool IsRelaxing => relaxCoroutine != null;

    void Start()
    {
        startLocalPosition = transform.localPosition;

        if (headBone != null)
            headRestRotation = headBone.localRotation;

        if (leftArm != null)
            leftArmRestRotation = leftArm.localRotation;

        if (rightArm != null)
            rightArmRestRotation = rightArm.localRotation;
    }

    public void Relax()
    {
        if (headBone == null)
        {
            Debug.LogWarning("ForgyRelax: Head bone not assigned!");
            return;
        }

        if (relaxCoroutine != null)
            return;

        relaxCoroutine = StartCoroutine(PlayRelax());
    }

    private IEnumerator PlayRelax()
    {
        Vector3 hiddenPosition =
            startLocalPosition + Vector3.down * duckDepth;

        Vector3 peekPosition =
            startLocalPosition + Vector3.up * peekHeight;

        Quaternion headPeekRotation =
            headRestRotation * Quaternion.Euler(headPeekEuler);

        Quaternion leftArmPeekRotation = leftArm != null
            ? leftArmRestRotation * Quaternion.Euler(leftArmPeekEuler)
            : Quaternion.identity;

        Quaternion rightArmPeekRotation = rightArm != null
            ? rightArmRestRotation * Quaternion.Euler(rightArmPeekEuler)
            : Quaternion.identity;

        // Step 1: go slightly down first
        yield return AnimatePose(
            startLocalPosition,
            hiddenPosition,
            headRestRotation,
            headRestRotation,
            leftArmRestRotation,
            leftArmRestRotation,
            rightArmRestRotation,
            rightArmRestRotation,
            prepareDuration
        );

        // Step 2: peek up once with hands forward and head tilted
        yield return AnimatePose(
            hiddenPosition,
            peekPosition,
            headRestRotation,
            headPeekRotation,
            leftArmRestRotation,
            leftArmPeekRotation,
            rightArmRestRotation,
            rightArmPeekRotation,
            peekDuration
        );

        // Step 3: hold the peek pose
        ApplyPose(
            peekPosition,
            headPeekRotation,
            leftArmPeekRotation,
            rightArmPeekRotation
        );

        yield return new WaitForSeconds(holdDuration);

        // Step 4: go back down and return to normal
        yield return AnimatePose(
            peekPosition,
            startLocalPosition,
            headPeekRotation,
            headRestRotation,
            leftArmPeekRotation,
            leftArmRestRotation,
            rightArmPeekRotation,
            rightArmRestRotation,
            returnDuration
        );

        ApplyPose(
            startLocalPosition,
            headRestRotation,
            leftArmRestRotation,
            rightArmRestRotation
        );

        relaxCoroutine = null;
    }

    private IEnumerator AnimatePose(
        Vector3 fromPosition,
        Vector3 toPosition,
        Quaternion fromHead,
        Quaternion toHead,
        Quaternion fromLeftArm,
        Quaternion toLeftArm,
        Quaternion fromRightArm,
        Quaternion toRightArm,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.01f))
            );

            Vector3 currentPosition =
                Vector3.Lerp(fromPosition, toPosition, t);

            Quaternion currentHead =
                Quaternion.Slerp(fromHead, toHead, t);

            Quaternion currentLeftArm =
                Quaternion.Slerp(fromLeftArm, toLeftArm, t);

            Quaternion currentRightArm =
                Quaternion.Slerp(fromRightArm, toRightArm, t);

            ApplyPose(
                currentPosition,
                currentHead,
                currentLeftArm,
                currentRightArm
            );

            yield return null;
        }

        ApplyPose(
            toPosition,
            toHead,
            toLeftArm,
            toRightArm
        );
    }

    private void ApplyPose(
        Vector3 localPosition,
        Quaternion headRotation,
        Quaternion leftArmRotation,
        Quaternion rightArmRotation)
    {
        transform.localPosition = localPosition;

        if (headBone != null)
            headBone.localRotation = headRotation;

        if (leftArm != null)
            leftArm.localRotation = leftArmRotation;

        if (rightArm != null)
            rightArm.localRotation = rightArmRotation;
    }
}