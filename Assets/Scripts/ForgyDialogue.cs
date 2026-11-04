
using UnityEngine;
using TMPro;

public class ForgyDialogue : MonoBehaviour
{
    [Header("References")]
    public Transform head;
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

    private float hideTime;
    private bool showing;
    private Canvas canvas;

    void Start()
    {
        if (dialoguePanel != null)
        {
            canvas = dialoguePanel.GetComponentInParent<Canvas>();
            dialoguePanel.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (!showing ||
            head == null ||
            arCamera == null ||
            dialoguePanel == null ||
            canvas == null)
        {
            return;
        }

        // Hide dialogue after the display duration
        if (Time.time >= hideTime)
        {
            showing = false;
            dialoguePanel.gameObject.SetActive(false);
            return;
        }

        // Position the dialogue above Forgy's head
        Vector3 worldPosition =
            head.position + Vector3.up * heightOffset;

        Vector3 screenPosition =
            arCamera.WorldToScreenPoint(worldPosition);

        // Hide when Forgy is behind the camera
        if (screenPosition.z <= 0f)
        {
            dialoguePanel.gameObject.SetActive(false);
            return;
        }

        // Convert screen position to Canvas position
        Camera uiCamera =
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

        RectTransform canvasRect =
            canvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            uiCamera,
            out Vector2 localPosition))
        {
            dialoguePanel.anchoredPosition = localPosition;

            if (!dialoguePanel.gameObject.activeSelf)
            {
                dialoguePanel.gameObject.SetActive(true);
            }
        }
    }

    // WAVE
    public void ShowWaveDialogue()
    {
        ShowDialogue(
            "Hellooo! I'm Forgy!",
            waveVoice
        );
    }

    // CHEER
    public void ShowCheerDialogue()
    {
        ShowDialogue(
            "You got this! Keep going!",
            cheerVoice
        );
    }

    // RELAX / BREAK
    public void ShowRelaxDialogue()
    {
        ShowDialogue(
            "Time for a little break!",
            breakVoice
        );
    }

    // Shared dialogue and voice logic
    private void ShowDialogue(string message, AudioClip voice)
    {
        if (dialoguePanel == null || dialogueText == null)
            return;

        dialogueText.text = message;
        showing = true;

        // Keep the bubble visible while Forgy speaks
        float duration = voice != null
            ? Mathf.Max(displayDuration, voice.length)
            : displayDuration;

        hideTime = Time.time + duration;

        dialoguePanel.gameObject.SetActive(true);

        // Play the matching voice
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
