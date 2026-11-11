
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Vuforia;
using Image = UnityEngine.UI.Image;

public class ForgyLocationGuide : MonoBehaviour
{
    [Header("Your Existing TMP Font")]
    public TMP_FontAsset projectFont;

    [Header("Area Target References")]
    public ObserverBehaviour informaticsTarget;
    public ObserverBehaviour sofaTarget;
    public ObserverBehaviour tableChairTarget;

    [Header("Guide Settings")]
    public float reopenDelay = 4f;

    private ObserverBehaviour[] targets;

    private readonly string[] names =
    {
        "Informatics Writing",
        "Lounge Sofa",
        "Table & Chair"
    };

    private readonly string[] descriptions =
    {
        "Near the lift",
        "Blue sofa in the lounge",
        "Main lounge seating area"
    };

    private readonly string[] clues =
    {
        "Near the lift, look for the Informatics mural.",
        "Find the blue sofa and scan around it.",
        "Scan the table-and-chair seating area."
    };

    private readonly string[] animations =
    {
        "WAVE", "PEEK", "CHEER"
    };

    private readonly bool[] discovered = new bool[3];

    private RectTransform safeRoot;
    private Rect previousSafeArea;

    private GameObject guidePanel;
    private TMP_Text statusText;
    private TMP_Text counterText;
    private TMP_Text hintText;

    private readonly Image[] cards = new Image[3];
    private readonly TMP_Text[] cardTags = new TMP_Text[3];

    private int currentTarget = -1;
    private int selectedLocation = -1;

    private bool guideOpen = true;
    private bool autoCollapsed;
    private float trackingLostTime;

    // Original holographic color palette
    private static readonly Color PanelColor =
        new Color32(7, 25, 42, 235);

    private static readonly Color CardColor =
        new Color32(20, 48, 67, 240);

    private static readonly Color SelectedColor =
        new Color32(30, 85, 103, 250);

    private static readonly Color Accent =
        new Color32(119, 229, 242, 255);

    private static readonly Color White =
        new Color32(237, 249, 255, 255);

    private static readonly Color Muted =
        new Color32(170, 203, 216, 255);

    private void Awake()
    {
        targets = new ObserverBehaviour[]
        {
            informaticsTarget,
            sofaTarget,
            tableChairTarget
        };

        BuildUI();
    }

    private void Start()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
            {
                Debug.LogWarning(
                    "ForgyLocationGuide: Missing target " +
                    names[i]
                );
            }
        }

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            Debug.LogWarning(
                "ForgyLocationGuide: Scene needs an EventSystem."
            );
        }
    }

    private void Update()
    {
        if (Screen.safeArea != previousSafeArea)
            ApplySafeArea();

        int trackedIndex = FindTrackedLocation();

        if (trackedIndex != currentTarget)
        {
            currentTarget = trackedIndex;

            if (trackedIndex >= 0)
            {
                discovered[trackedIndex] = true;

                statusText.text =
                    "FORGY FOUND  /  " +
                    names[trackedIndex].ToUpper();

                guideOpen = false;
                autoCollapsed = true;
                guidePanel.SetActive(false);
            }
            else
            {
                statusText.text =
                    "LOOK AROUND TO FIND FORGY";

                trackingLostTime = Time.unscaledTime;
            }

            UpdateCounter();
            UpdateCards();
        }

        if (currentTarget == -1 &&
            autoCollapsed &&
            !guideOpen &&
            Time.unscaledTime - trackingLostTime >= reopenDelay)
        {
            guideOpen = true;
            autoCollapsed = false;
            guidePanel.SetActive(true);
        }
    }

    private int FindTrackedLocation()
    {
        if (currentTarget >= 0 && IsTracked(currentTarget))
            return currentTarget;

        for (int i = 0; i < targets.Length; i++)
        {
            if (IsTracked(i))
                return i;
        }

        return -1;
    }

    private bool IsTracked(int index)
    {
        ObserverBehaviour target = targets[index];

        if (target == null || !target.isActiveAndEnabled)
            return false;

        Status status = target.TargetStatus.Status;

        return status == Status.TRACKED ||
               status == Status.EXTENDED_TRACKED;
    }

    private void ToggleGuide()
    {
        guideOpen = !guideOpen;
        autoCollapsed = false;
        guidePanel.SetActive(guideOpen);
    }

    private void SelectLocation(int index)
    {
        selectedLocation = index;

        // Short instructions are easier to read
        // without covering too much camera space.
        hintText.text = clues[index];

        UpdateCards();
    }

    private void UpdateCounter()
    {
        int count = 0;

        for (int i = 0; i < discovered.Length; i++)
        {
            if (discovered[i])
                count++;
        }

        counterText.text =
            "LOCATIONS  " + count + "/3";
    }

    private void UpdateCards()
    {
        for (int i = 0; i < 3; i++)
        {
            if (cards[i] != null)
            {
                cards[i].color =
                    selectedLocation == i
                    ? SelectedColor
                    : CardColor;
            }

            if (cardTags[i] != null)
            {
                cardTags[i].text =
                    discovered[i]
                    ? "FOUND"
                    : animations[i];
            }
        }
    }

    private void BuildUI()
    {
        GameObject canvasObject = new GameObject(
            "ForgyFinder_HUD",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas =
            canvasObject.GetComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 10;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight = 0.5f;

        safeRoot = MakeRect(
            "Safe Area",
            canvasObject.transform,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero,
            new Vector2(0.5f, 0.5f)
        );

        ApplySafeArea();

        // ==================================
        // TOP HEADER
        // ==================================

        MakeText(
            "Brand",
            safeRoot,
            "FORGY FINDER",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(42f, -60f),
            new Vector2(550f, 72f),
            48f,
            White,
            TextAlignmentOptions.Left
        );

        // Top-right Locations button
        Image buttonImage = MakeImage(
            "Locations Button",
            safeRoot,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-40f, -47f),
            new Vector2(340f, 92f),
            new Color32(20, 58, 76, 238),
            new Vector2(1f, 1f)
        );

        Button button =
            buttonImage.gameObject.AddComponent<Button>();

        button.targetGraphic = buttonImage;
        button.onClick.AddListener(ToggleGuide);

        counterText = MakeText(
            "Location Count",
            buttonImage.transform,
            "LOCATIONS  0/3",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(325f, 76f),
            34f,
            Accent,
            TextAlignmentOptions.Center
        );

        // Tracking status
        statusText = MakeText(
            "Tracking Status",
            safeRoot,
            "LOOK AROUND TO FIND FORGY",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -183f),
            new Vector2(960f, 62f),
            34f,
            Accent,
            TextAlignmentOptions.Center
        );

        // ==================================
        // BOTTOM DISCOVERY PANEL
        // ==================================

        Image panel = MakeImage(
            "Discovery Guide",
            safeRoot,
            new Vector2(0.055f, 0f),
            new Vector2(0.945f, 0f),
            new Vector2(0f, 45f),
            new Vector2(0f, 760f),
            PanelColor,
            new Vector2(0.5f, 0f)
        );

        guidePanel = panel.gameObject;

        Outline border =
            panel.gameObject.AddComponent<Outline>();

        border.effectColor =
            new Color(0.35f, 0.8f, 0.9f, 0.25f);

        border.effectDistance =
            new Vector2(1.5f, 1.5f);

        // Panel title
        MakeText(
            "Panel Title",
            panel.transform,
            "HEY, FORGY!",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(34f, -53f),
            new Vector2(800f, 78f),
            60f,
            White,
            TextAlignmentOptions.Left
        );

        // Panel subtitle
        MakeText(
            "Panel Subtitle",
            panel.transform,
            "Explore three spots on the fifth floor.",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(34f, -125f),
            new Vector2(850f, 55f),
            38f,
            Muted,
            TextAlignmentOptions.Left
        );

        // ==================================
        // THREE LOCATION CARDS
        // ==================================

        for (int i = 0; i < 3; i++)
        {
            int locationIndex = i;

            Image card = MakeImage(
                "Location " + (i + 1),
                panel.transform,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, -177f - i * 154f),
                new Vector2(-64f, 136f),
                CardColor,
                new Vector2(0.5f, 1f)
            );

            cards[i] = card;

            Button cardButton =
                card.gameObject.AddComponent<Button>();

            cardButton.targetGraphic = card;

            cardButton.onClick.AddListener(
                () => SelectLocation(locationIndex)
            );

            // Location number: separate column
            MakeText(
                "Location Number",
                card.transform,
                (i + 1).ToString("00"),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(25f, -47f),
                new Vector2(67f, 60f),
                36f,
                Accent,
                TextAlignmentOptions.Left
            );

            // Location name
            MakeText(
                "Location Name",
                card.transform,
                names[i],
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(108f, -46f),
                new Vector2(535f, 65f),
                48f,
                White,
                TextAlignmentOptions.Left
            );

            // Description aligned under the name
            MakeText(
                "Location Description",
                card.transform,
                descriptions[i],
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(108f, -105f),
                new Vector2(630f, 46f),
                37f,
                Muted,
                TextAlignmentOptions.Left
            );

            // Status / animation on the right
            cardTags[i] = MakeText(
                "Animation Tag",
                card.transform,
                animations[i],
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-25f, -47f),
                new Vector2(165f, 60f),
                32f,
                Accent,
                TextAlignmentOptions.Right
            );
        }

        // ==================================
        // BOTTOM LOCATION HINT
        // ==================================

        hintText = MakeText(
            "Selected Location Hint",
            panel.transform,
            "Tap a location for a clue.",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 67f),
            new Vector2(845f, 105f),
            34f,
            Muted,
            TextAlignmentOptions.Center,
            true
        );

        UpdateCounter();
        UpdateCards();
    }

    // ==================================
    // IPHONE SAFE AREA
    // ==================================

    private void ApplySafeArea()
    {
        if (safeRoot == null ||
            Screen.width <= 0 ||
            Screen.height <= 0)
        {
            return;
        }

        previousSafeArea = Screen.safeArea;

        safeRoot.anchorMin = new Vector2(
            previousSafeArea.xMin / Screen.width,
            previousSafeArea.yMin / Screen.height
        );

        safeRoot.anchorMax = new Vector2(
            previousSafeArea.xMax / Screen.width,
            previousSafeArea.yMax / Screen.height
        );

        safeRoot.offsetMin = Vector2.zero;
        safeRoot.offsetMax = Vector2.zero;
    }

    // ==================================
    // UI HELPERS
    // ==================================

    private RectTransform MakeRect(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 position,
        Vector2 size,
        Vector2 pivot)
    {
        GameObject go = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        go.transform.SetParent(parent, false);

        RectTransform rect =
            go.GetComponent<RectTransform>();

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return rect;
    }

    private Image MakeImage(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 position,
        Vector2 size,
        Color color,
        Vector2 pivot)
    {
        RectTransform rect = MakeRect(
            objectName,
            parent,
            anchorMin,
            anchorMax,
            position,
            size,
            pivot
        );

        Image image =
            rect.gameObject.AddComponent<Image>();

        image.color = color;
        image.raycastTarget = true;

        return image;
    }

    private TMP_Text MakeText(
        string objectName,
        Transform parent,
        string value,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment,
        bool allowWrapping = false)
    {
        // Align the RectTransform pivot to the
        // actual text alignment.
        float pivotX = 0.5f;

        if (alignment == TextAlignmentOptions.Left)
            pivotX = 0f;
        else if (alignment == TextAlignmentOptions.Right)
            pivotX = 1f;

        RectTransform rect = MakeRect(
            objectName,
            parent,
            anchorMin,
            anchorMax,
            position,
            size,
            new Vector2(pivotX, 0.5f)
        );

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<TextMeshProUGUI>();

        // Keep your existing font.
        if (projectFont != null)
            text.font = projectFont;

        text.text = value;
        text.fontSize = fontSize;

        // IMPORTANT:
        // Do not automatically shrink our fonts.
        text.enableAutoSizing = false;

        text.color = color;
        text.alignment = alignment;

        text.textWrappingMode = allowWrapping
            ? TextWrappingModes.Normal
            : TextWrappingModes.NoWrap;

        text.overflowMode =
            TextOverflowModes.Ellipsis;

        text.raycastTarget = false;

        return text;
    }
}
