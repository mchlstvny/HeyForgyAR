
using System.Collections;
using UnityEngine;

public class ForgyRelax : MonoBehaviour
{
    [Header("Head Bone")]
    public Transform headBone;

    [Header("Relax Settings")]
    public float tiltAngle = 15f;
    public float tiltDuration = 0.7f;
    public float holdDuration = 2f;
    public float returnDuration = 0.8f;

    private Coroutine relaxCoroutine;

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
        Quaternion startRotation = headBone.localRotation;
        Quaternion tiltedRotation = startRotation *
            Quaternion.Euler(0f, 0f, tiltAngle);

        // Tilt head smoothly
        yield return RotateHead(
            startRotation, tiltedRotation, tiltDuration);

        // Hold relaxed pose
        yield return new WaitForSeconds(holdDuration);

        // Return to normal
        yield return RotateHead(
            tiltedRotation, startRotation, returnDuration);

        headBone.localRotation = startRotation;
        relaxCoroutine = null;
    }

    private IEnumerator RotateHead(
        Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f, 1f,
                Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.01f))
            );

            headBone.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }

        headBone.localRotation = to;
    }
}
