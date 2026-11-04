
using System.Collections;
using UnityEngine;

public class ForgyCheer : MonoBehaviour
{
    [Header("Arm Bones")]
    public Transform leftArm;
    public Transform rightArm;

    [Header("Cheer Settings")]
    public float leftRaiseAngle = -65f;
    public float rightRaiseAngle = 65f;

    public float raiseDuration = 0.5f;
    public float holdDuration = 0.6f;
    public float lowerDuration = 0.8f;

    private Quaternion leftRestRotation;
    private Quaternion rightRestRotation;
    private Coroutine cheerCoroutine;

    void Start()
    {
        if (leftArm != null && rightArm != null)
        {
            leftRestRotation = leftArm.localRotation;
            rightRestRotation = rightArm.localRotation;
        }
    }

    public void Cheer()
    {
        if (leftArm == null || rightArm == null)
        {
            Debug.LogWarning("ForgyCheer: Arm bones are not assigned!");
            return;
        }

        if (cheerCoroutine != null)
            return;

        cheerCoroutine = StartCoroutine(PlayCheer());
    }

    private IEnumerator PlayCheer()
    {
        Quaternion leftRaised = leftRestRotation *
            Quaternion.Euler(0f, 0f, leftRaiseAngle);

        Quaternion rightRaised = rightRestRotation *
            Quaternion.Euler(0f, 0f, rightRaiseAngle);

        // Raise both arms
        float elapsed = 0f;

        while (elapsed < raiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(
                0f, 1f,
                Mathf.Clamp01(elapsed / Mathf.Max(raiseDuration, 0.01f))
            );

            leftArm.localRotation =
                Quaternion.Slerp(leftRestRotation, leftRaised, t);

            rightArm.localRotation =
                Quaternion.Slerp(rightRestRotation, rightRaised, t);

            yield return null;
        }

        leftArm.localRotation = leftRaised;
        rightArm.localRotation = rightRaised;

        // Hold the cheering pose
        yield return new WaitForSeconds(holdDuration);

        // Lower both arms
        elapsed = 0f;

        while (elapsed < lowerDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(
                0f, 1f,
                Mathf.Clamp01(elapsed / Mathf.Max(lowerDuration, 0.01f))
            );

            leftArm.localRotation =
                Quaternion.Slerp(leftRaised, leftRestRotation, t);

            rightArm.localRotation =
                Quaternion.Slerp(rightRaised, rightRestRotation, t);

            yield return null;
        }

        leftArm.localRotation = leftRestRotation;
        rightArm.localRotation = rightRestRotation;

        cheerCoroutine = null;
    }
}
