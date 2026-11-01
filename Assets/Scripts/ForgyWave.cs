
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ForgyWave : MonoBehaviour
{
    public Transform armBone;

    public float raiseAngle = -65f;
    public float raiseDuration = 0.5f;
    public float holdDuration = 0.35f;
    public float lowerDuration = 0.8f;

    private Quaternion restingRotation;
    private bool isWaving;

    void Start()
    {
        if (armBone != null)
        {
            restingRotation = armBone.localRotation;
        }
    }

    // Spacebar test in Unity Editor
    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Wave();
        }
    }

    // Called by the Wave button
    public void Wave()
    {
        if (armBone != null && !isWaving)
        {
            StartCoroutine(WaveRoutine());
        }
    }

    IEnumerator WaveRoutine()
    {
        isWaving = true;

        Quaternion raisedRotation =
            restingRotation * Quaternion.Euler(0f, 0f, raiseAngle);

        // Raise arm smoothly
        yield return RotateArm(
            restingRotation,
            raisedRotation,
            raiseDuration
        );

        // Hold arm briefly
        yield return new WaitForSeconds(holdDuration);

        // Lower arm smoothly
        yield return RotateArm(
            raisedRotation,
            restingRotation,
            lowerDuration
        );

        isWaving = false;
    }

    IEnumerator RotateArm(
        Quaternion from,
        Quaternion to,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            armBone.localRotation =
                Quaternion.Slerp(from, to, t);

            yield return null;
        }

        armBone.localRotation = to;
    }
}
