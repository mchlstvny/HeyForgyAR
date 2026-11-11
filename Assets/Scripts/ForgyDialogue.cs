
using UnityEngine;
using TMPro;

public class ForgyDialogue : MonoBehaviour
{
    [Header("Original Head Reference")]
    public Transform head;

    [Header("Area Target Head References")]
    public Transform waveHead;
    public Transform breakHead;
    public Transform cheerHead;

    [Header("UI References")]
    public Camera arCamera;
    public RectTransform dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Voice Audio")]
    public AudioSource audioSource;
    public AudioClip waveVoice;
    public AudioClip cheerVoice;
    public AudioClip breakVoice;

    [Header("Dialogue Settings")]
    public float heightOffset = 0.35f;
    public float displayDuration = 3f;

    [Header("Automatic Bubble Spacing")]
    public float bubbleGapPixels = 24f;

    [Header("Additional Gap Per Location")]
    public float waveExtraGapPixels = 15f;
    public float breakExtraGapPixels = 0f;
    public float cheerExtraGapPixels = 0f;

    private Transform activeHead;
    private Renderer[] activeRenderers;

    private float activeExtraGap;
    private float hideTime;
    private bool showing;
    private Canvas canvas;

    private readonly Vector3[] panelCorners =
        new Vector3[4];

    void Start()
    {
        activeHead = head;

        if (dialoguePanel != null)
        {
            canvas = dialoguePanel.GetComponentInParent<Canvas>();
            dialoguePanel.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (!showing || dialoguePanel == null)
            return;

        if (Time.time >= hideTime)
        {
            showing = false;
            dialoguePanel.gameObject.SetActive(false);
            return;
        }

        if (activeHead == null ||
            arCamera == null ||
            canvas == null)
        {
            dialoguePanel.gameObject.SetActive(false);
            return;
        }

        // Use head to keep the bubble centered
        // horizontally around Forgy.
        Vector3 headScreen =
            arCamera.WorldToScreenPoint(
                activeHead.position
            );

        if (headScreen.z <= arCamera.nearClipPlane)
        {
            dialoguePanel.gameObject.SetActive(false);
            return;
        }

        // Find the highest visible part of Forgy.
        // This automatically adapts to model scale
        // and camera distance.
        float topScreenY;

        if (!TryGetModelTop(out topScreenY))
        {
            // Fallback for models without renderers.
            Vector3 fallbackScreen =
                arCamera.WorldToScreenPoint(
                    activeHead.position +
                    Vector3.up * heightOffset
                );

            topScreenY = fallbackScreen.y;
        }

        // Camera used to position the UI.
        Camera uiCamera =
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera != null
                    ? canvas.worldCamera
                    : arCamera;

        // Find the bubble height in screen pixels.
        dialoguePanel.GetWorldCorners(panelCorners);

        float panelBottom =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                panelCorners[0]
            ).y;

        float panelTop =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                panelCorners[1]
            ).y;

        float panelHeightPixels =
            Mathf.Abs(panelTop - panelBottom);

        // The position of the pivot relative
        // to the bottom edge of the bubble.
        float pivotFromBottom =
            panelHeightPixels * dialoguePanel.pivot.y;

        // Gap between the top of Forgy and
        // the bottom edge of the dialogue bubble.
        float totalGap =
            Mathf.Max(
                0f,
                bubbleGapPixels + activeExtraGap
            );

        Vector2 targetScreenPosition =
            new Vector2(
                headScreen.x,
                topScreenY +
                totalGap +
                pivotFromBottom
            );

        // Works for Screen Space Overlay and
        // Screen Space Camera canvases.
        RectTransform parentRect =
            dialoguePanel.parent as RectTransform;

        if (parentRect == null)
            return;

        if (RectTransformUtility
            .ScreenPointToWorldPointInRectangle(
                parentRect,
                targetScreenPosition,
                uiCamera,
                out Vector3 worldPoint
            ))
        {
            dialoguePanel.position = worldPoint;

            if (!dialoguePanel.gameObject.activeSelf)
                dialoguePanel.gameObject.SetActive(true);
        }
    }

    // Find top of the 3D model in screen space.
    private bool TryGetModelTop(out float topY)
    {
        topY = float.NegativeInfinity;

        if (activeRenderers == null ||
            activeRenderers.Length == 0)
        {
            return false;
        }

        bool found = false;

        foreach (Renderer renderer in activeRenderers)
        {
            if (renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            Bounds bounds = renderer.bounds;

            // Check all eight corners of the
            // renderer's world-space bounds.
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new Vector3(
                    (i & 1) == 0
                        ? bounds.min.x
                        : bounds.max.x,

                    (i & 2) == 0
                        ? bounds.min.y
                        : bounds.max.y,

                    (i & 4) == 0
                        ? bounds.min.z
                        : bounds.max.z
                );

                Vector3 screen =
                    arCamera.WorldToScreenPoint(corner);

                if (screen.z <= arCamera.nearClipPlane)
                    continue;

                topY = Mathf.Max(
                    topY,
                    screen.y
                );

                found = true;
            }
        }

        return found;
    }

    // Find the 3D model that owns this head.
    private void CacheModelRenderers(
        Transform targetHead)
    {
        activeRenderers = null;

        if (targetHead == null)
            return;

        // ForgyEyes is on the "forgy" object.
        ForgyEyes forgy =
            targetHead.GetComponentInParent<ForgyEyes>();

        if (forgy != null)
        {
            activeRenderers =
                forgy.GetComponentsInChildren<Renderer>(
                    true
                );
        }
    }

    // INFORMATICS — WAVE
    public void ShowWaveDialogue()
    {
        ShowDialogue(
            "Hellooo! I'm Forgy!",
            waveVoice,
            waveHead != null ? waveHead : head,
            waveExtraGapPixels
        );
    }

    // TABLECHAIR — CHEER
    public void ShowCheerDialogue()
    {
        ShowDialogue(
            "You got this! Keep going!",
            cheerVoice,
            cheerHead != null ? cheerHead : head,
            cheerExtraGapPixels
        );
    }

    // SOFA — BREAK / PEEK
    public void ShowRelaxDialogue()
    {
        ShowDialogue(
            "Time for a little break!",
            breakVoice,
            breakHead != null ? breakHead : head,
            breakExtraGapPixels
        );
    }

    private void ShowDialogue(
        string message,
        AudioClip voice,
        Transform targetHead,
        float extraGap)
    {
        if (dialoguePanel == null ||
            dialogueText == null ||
            targetHead == null)
        {
            Debug.LogWarning(
                "ForgyDialogue: Missing dialogue reference."
            );

            return;
        }

        activeHead = targetHead;
        activeExtraGap = extraGap;

        CacheModelRenderers(activeHead);

        dialogueText.text = message;
        showing = true;

        float duration = voice != null
            ? Mathf.Max(displayDuration, voice.length)
            : displayDuration;

        hideTime = Time.time + duration;

        dialoguePanel.gameObject.SetActive(true);

        // Refresh the dialogue layout before
        // calculating its screen-space size.
        Canvas.ForceUpdateCanvases();

        if (audioSource != null)
        {
            audioSource.Stop();

            if (voice != null)
            {
                audioSource.clip = voice;
                audioSource.Play();
            }
        }
    }
}
