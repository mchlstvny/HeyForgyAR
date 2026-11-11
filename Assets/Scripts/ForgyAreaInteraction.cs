
using UnityEngine;
using UnityEngine.Events;
using Vuforia;

public class ForgyAreaInteraction : MonoBehaviour
{
    public enum InteractionType
    {
        Wave,
        Break,
        Cheer
    }

    [Header("Location Interaction")]
    public InteractionType interactionType = InteractionType.Wave;

    [Header("References")]
    public GameObject forgyObject;
    public Transform arCamera;

    [Header("Automatic Interaction Settings")]
    public float triggerDistance = 2.5f;
    public float stableTrackingDuration = 1f;
    public float cooldownDuration = 12f;

    [Header("Optional Dialogue and Voice")]
    public UnityEvent onInteraction = new UnityEvent();

    private ObserverBehaviour areaTarget;

    private ForgyWave wave;
    private ForgyCheer cheer;
    private ForgyRelax relax;

    private float trackingTimer;
    private float nextInteractionTime;

    private bool interactionPlaying;

    void Start()
    {
        areaTarget = GetComponent<ObserverBehaviour>();

        if (forgyObject != null)
        {
            wave = forgyObject.GetComponent<ForgyWave>();
            cheer = forgyObject.GetComponent<ForgyCheer>();
            relax = forgyObject.GetComponent<ForgyRelax>();
        }

        if (arCamera == null && Camera.main != null)
            arCamera = Camera.main.transform;

        if (areaTarget == null ||
            forgyObject == null ||
            arCamera == null)
        {
            Debug.LogWarning(
                "ForgyAreaInteraction: Missing references on " +
                gameObject.name
            );

            enabled = false;
            return;
        }
    }

    void Update()
    {
        // Only respond to reliably tracked targets.
        Status status = areaTarget.TargetStatus.Status;

        bool targetTracked =
            status == Status.TRACKED ||
            status == Status.EXTENDED_TRACKED;

        if (!targetTracked)
        {
            trackingTimer = 0f;
            return;
        }

        trackingTimer += Time.deltaTime;

        // If an animation is playing, wait until
        // it finishes before starting the cooldown.
        if (interactionPlaying)
        {
            if (!IsCurrentAnimationPlaying())
            {
                interactionPlaying = false;

                nextInteractionTime =
                    Time.time + cooldownDuration;
            }

            return;
        }

        // Wait for tracking to become stable.
        if (trackingTimer < stableTrackingDuration)
            return;

        // Wait until the cooldown has finished.
        if (Time.time < nextInteractionTime)
            return;

        // Check the distance from iPhone to Forgy.
        float distance = Vector3.Distance(
            arCamera.position,
            forgyObject.transform.position
        );

        if (distance > triggerDistance)
            return;

        TriggerInteraction();
    }

    void TriggerInteraction()
    {
        switch (interactionType)
        {
            case InteractionType.Wave:

                if (wave == null)
                    return;

                wave.Wave();
                break;

            case InteractionType.Break:

                if (relax == null)
                    return;

                relax.Relax();
                break;

            case InteractionType.Cheer:

                if (cheer == null)
                    return;

                cheer.Cheer();
                break;
        }

        // Only trigger dialogue and voice if
        // the animation successfully started.
        if (!IsCurrentAnimationPlaying())
            return;

        interactionPlaying = true;

        onInteraction?.Invoke();

        Debug.Log(
            "Forgy interaction: " + interactionType +
            " at " + gameObject.name
        );
    }

    bool IsCurrentAnimationPlaying()
    {
        switch (interactionType)
        {
            case InteractionType.Wave:
                return wave != null && wave.IsWaving;

            case InteractionType.Break:
                return relax != null && relax.IsRelaxing;

            case InteractionType.Cheer:
                return cheer != null && cheer.IsCheering;
        }

        return false;
    }
}
