
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class ForgyWelcomeScreen : MonoBehaviour
{
    [Header("Use your existing TextMeshPro font")]
    [SerializeField] private TMP_FontAsset projectFont;

    [Header("Optional transparent Forgy PNG (Sprite)")]
    [SerializeField] private Sprite forgyPortrait;

    [Header("Scene opened by the button")]
    [SerializeField] private string destinationScene = "MainScene_AreaTarget";

    [Header("Subtle animation")]
    [SerializeField, Range(0f, 2f)]
    private float animationSpeed = 1f;

    private Canvas generatedCanvas;
    private RectTransform safeRoot;
    private Rect previousSafeArea;

    private RectTransform mascotGroup;
    private RectTransform haloRing;

    private Image portraitImage;
    private Image actionButtonGlow;

    private Texture2D backgroundTexture;
    private Texture2D glowTexture;
    private Texture2D ringTexture;
    private Sprite ringSprite;

    // Minimal color palette
    private static readonly Color Background =
        new Color32(6, 19, 33, 255);

    private static readonly Color Accent =
        new Color32(129, 236, 248, 255);

    private static readonly Color White =
        new Color32(237, 247, 250, 255);

    private static readonly Color Secondary =
        new Color32(146, 166, 181, 255);

    private void Awake()
    {
        if (projectFont == null)
        {
            Debug.LogWarning(
                "ForgyWelcomeScreen: Please assign your TMP Font Asset."
            );
        }

        BuildScreen();
    }

    private void Update()
    {
        if (generatedCanvas == null)
            return;

        if (Screen.safeArea != previousSafeArea)
        {
            ApplySafeArea();
        }

        float time = Time.unscaledTime * animationSpeed;

        // Gentle floating effect
        if (mascotGroup != null)
        {
            Vector2 position = mascotGroup.anchoredPosition;

            position.y = Mathf.Sin(time * 1.15f) * 11f;

            mascotGroup.anchoredPosition = position;
        }

        // Slowly rotating holographic ring
        if (haloRing != null)
        {
            haloRing.localRotation =
                Quaternion.Euler(0f, 0f, time * 3f);
        }

        // Very subtle portrait pulse
        if (portraitImage != null)
        {
            Color color = portraitImage.color;

            color.a =
                0.94f + 0.06f * Mathf.Sin(time * 1.4f);

            portraitImage.color = color;
        }

        // Button glow animation
        if (actionButtonGlow != null)
        {
            Color color = actionButtonGlow.color;

            color.a =
                0.045f +
                0.025f * (1f + Mathf.Sin(time * 1.4f));

            actionButtonGlow.color = color;
        }
    }

    private void BuildScreen()
    {
        // Create Canvas
        GameObject canvasObject = new GameObject(
            "Generated_Minimal_Welcome_UI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(transform, false);

        generatedCanvas = canvasObject.GetComponent<Canvas>();

        generatedCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        generatedCanvas.sortingOrder = 200;

        // Responsive canvas for portrait iPhone
        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight = 0.6f;

        // An EventSystem is needed for UI buttons
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            Debug.LogWarning(
                "ForgyWelcomeScreen: An EventSystem is required for the button."
            );
        }

        // Background
        backgroundTexture = MakeBackground(4, 256);

        RawImage background = Raw(
            "Midnight Background",
            canvasObject.transform,
            backgroundTexture,
            new Vector2(0.5f, 0.5f),
            Vector2.zero
        );

        background.rectTransform.anchorMin =
            Vector2.zero;

        background.rectTransform.anchorMax =
            Vector2.one;

        background.rectTransform.offsetMin =
            Vector2.zero;

        background.rectTransform.offsetMax =
            Vector2.zero;

        background.color = Color.white;

        // Safe area for iPhone notch / Dynamic Island
        safeRoot = Rect(
            "iPhone Safe Area",
            canvasObject.transform,
            new Vector2(0.5f, 0.5f),
            Vector2.zero
        );

        ApplySafeArea();

        // -------------------------
        // TOP HEADER
        // -------------------------

        Text(
            "Brand",
            "IMT UCM",
            safeRoot,
            new Vector2(0.20f, 0.945f),
            new Vector2(350f, 58f),
            32f,
            White,
            TextAlignmentOptions.MidlineLeft,
            true,
            2.8f
        );

        Text(
            "Header Tag",
            "AR EXPERIENCE",
            safeRoot,
            new Vector2(0.79f, 0.945f),
            new Vector2(390f, 58f),
            24f,
            Secondary,
            TextAlignmentOptions.MidlineRight,
            false,
            2f
        );

        Solid(
            "Header Hairline",
            safeRoot,
            new Vector2(0.5f, 0.907f),
            new Vector2(930f, 2f),
            Alpha(Accent, 0.14f)
        );

        // -------------------------
        // FORGY HOLOGRAM
        // -------------------------

        mascotGroup = Rect(
            "Floating Forgy",
            safeRoot,
            new Vector2(0.5f, 0.635f),
            new Vector2(650f, 650f)
        );

        // Soft circular glow
        glowTexture = MakeGlow(256);

        RawImage softGlow = Raw(
            "Soft Cyan Glow",
            mascotGroup,
            glowTexture,
            new Vector2(0.5f, 0.5f),
            new Vector2(620f, 620f)
        );

        softGlow.color =
            Alpha(Accent, 0.25f);

        // Thin holographic ring
        ringTexture = MakeRing(512);

        ringSprite = Sprite.Create(
            ringTexture,
            new Rect(0, 0, 512, 512),
            new Vector2(0.5f, 0.5f)
        );

        haloRing = Rect(
            "Single Holographic Ring",
            mascotGroup,
            new Vector2(0.5f, 0.5f),
            new Vector2(520f, 520f)
        );

        Image ringImage =
            haloRing.gameObject.AddComponent<Image>();

        ringImage.sprite = ringSprite;
        ringImage.color = Alpha(Accent, 0.45f);
        ringImage.raycastTarget = false;

        // Forgy image or fallback letter
        if (forgyPortrait != null)
        {
            RectTransform portrait = Rect(
                "Forgy Portrait",
                mascotGroup,
                new Vector2(0.5f, 0.5f),
                new Vector2(455f, 455f)
            );

            portraitImage =
                portrait.gameObject.AddComponent<Image>();

            portraitImage.sprite = forgyPortrait;
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
        }
        else
        {
            Text(
                "Forgy Monogram",
                "F",
                mascotGroup,
                new Vector2(0.5f, 0.5f),
                new Vector2(380f, 400f),
                240f,
                Accent,
                TextAlignmentOptions.Center,
                true
            );
        }

        // -------------------------
        // MAIN TITLE
        // -------------------------

        Text(
            "Eyebrow",
            "YOUR CAMPUS AR COMPANION",
            safeRoot,
            new Vector2(0.5f, 0.433f),
            new Vector2(890f, 54f),
            27f,
            Accent,
            TextAlignmentOptions.Center,
            false,
            3.1f
        );

        Text(
            "Main Title",
            "HEY, FORGY!",
            safeRoot,
            new Vector2(0.5f, 0.357f),
            new Vector2(1000f, 150f),
            124f,
            White,
            TextAlignmentOptions.Center,
            true,
            -0.5f
        );

        Text(
            "Tagline",
            "A little magic around campus.",
            safeRoot,
            new Vector2(0.5f, 0.300f),
            new Vector2(950f, 65f),
            34f,
            Secondary,
            TextAlignmentOptions.Center
        );

        // -------------------------
        // ENTER AR BUTTON
        // -------------------------

        actionButtonGlow = Solid(
            "Soft Button Glow",
            safeRoot,
            new Vector2(0.5f, 0.157f),
            new Vector2(850f, 142f),
            Alpha(Accent, 0.08f)
        );

        Image buttonImage = Solid(
            "Enter AR",
            safeRoot,
            new Vector2(0.5f, 0.157f),
            new Vector2(815f, 116f),
            Accent,
            true
        );

        Button button =
            buttonImage.gameObject.AddComponent<Button>();

        button.targetGraphic = buttonImage;

        button.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;

        colors.normalColor = Color.white;

        colors.highlightedColor =
            new Color32(220, 249, 252, 255);

        colors.pressedColor =
            new Color32(177, 216, 222, 255);

        colors.selectedColor = Color.white;

        button.colors = colors;

        // Scene transition
        button.onClick.AddListener(EnterAR);

        Text(
            "Button Text",
            "ENTER AR",
            buttonImage.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(700f, 100f),
            42f,
            Background,
            TextAlignmentOptions.Center,
            true,
            1.7f
        );

        // -------------------------
        // FOOTER
        // -------------------------

        Text(
            "Footer",
            "EXPLORE 3 CAMPUS LOCATIONS",
            safeRoot,
            new Vector2(0.5f, 0.069f),
            new Vector2(900f, 46f),
            23f,
            Secondary,
            TextAlignmentOptions.Center,
            false,
            2.1f
        );
    }

    // -------------------------
    // SCENE TRANSITION
    // -------------------------

    private void EnterAR()
    {
        if (!Application.CanStreamedLevelBeLoaded(
            destinationScene))
        {
            Debug.LogError(
                "ForgyWelcomeScreen: Scene '" +
                destinationScene +
                "' is missing from Build Profiles > Scene List."
            );

            return;
        }

        SceneManager.LoadScene(destinationScene);
    }

    // -------------------------
    // IPHONE SAFE AREA
    // -------------------------

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

    // -------------------------
    // UI HELPERS
    // -------------------------

    private static RectTransform Rect(
        string name,
        Transform parent,
        Vector2 anchor,
        Vector2 size)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform)
        );

        go.transform.SetParent(parent, false);

        RectTransform result =
            go.GetComponent<RectTransform>();

        result.anchorMin = anchor;
        result.anchorMax = anchor;

        result.pivot =
            new Vector2(0.5f, 0.5f);

        result.anchoredPosition =
            Vector2.zero;

        result.sizeDelta = size;

        return result;
    }

    private static Image Solid(
        string name,
        Transform parent,
        Vector2 anchor,
        Vector2 size,
        Color color,
        bool raycast = false)
    {
        RectTransform rect =
            Rect(name, parent, anchor, size);

        Image graphic =
            rect.gameObject.AddComponent<Image>();

        graphic.color = color;
        graphic.raycastTarget = raycast;

        return graphic;
    }

    private static RawImage Raw(
        string name,
        Transform parent,
        Texture texture,
        Vector2 anchor,
        Vector2 size)
    {
        RectTransform rect =
            Rect(name, parent, anchor, size);

        RawImage graphic =
            rect.gameObject.AddComponent<RawImage>();

        graphic.texture = texture;
        graphic.raycastTarget = false;

        return graphic;
    }

    private TextMeshProUGUI Text(
        string name,
        string value,
        Transform parent,
        Vector2 anchor,
        Vector2 size,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment,
        bool bold = false,
        float spacing = 0f)
    {
        RectTransform rect =
            Rect(name, parent, anchor, size);

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<TextMeshProUGUI>();

        text.text = value;

        // Preserve your original font
        if (projectFont != null)
        {
            text.font = projectFont;
        }

        text.fontSize = fontSize;

        text.enableAutoSizing = true;

        text.fontSizeMin =
            fontSize * 0.67f;

        text.fontSizeMax = fontSize;

        text.fontStyle =
            bold ? FontStyles.Bold : FontStyles.Normal;

        text.color = color;
        text.alignment = alignment;
        text.characterSpacing = spacing;

        // Unity 6-compatible TMP property
        text.textWrappingMode =
            TextWrappingModes.NoWrap;

        text.overflowMode =
            TextOverflowModes.Ellipsis;

        text.raycastTarget = false;

        return text;
    }

    // -------------------------
    // GENERATED BACKGROUND
    // -------------------------

    private static Texture2D MakeBackground(
        int width,
        int height)
    {
        Texture2D texture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.filterMode =
            FilterMode.Bilinear;

        Color[] pixels =
            new Color[width * height];

        Color top =
            new Color32(11, 29, 43, 255);

        Color bottom =
            new Color32(4, 10, 19, 255);

        for (int y = 0; y < height; y++)
        {
            float t =
                (float)y / (height - 1);

            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] =
                    Color.Lerp(bottom, top, t);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);

        return texture;
    }

    // -------------------------
    // SOFT CYAN GLOW
    // -------------------------

    private static Texture2D MakeGlow(int size)
    {
        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        Color[] pixels =
            new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx =
                    (x + 0.5f) / size - 0.5f;

                float dy =
                    (y + 0.5f) / size - 0.5f;

                float radius =
                    Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Pow(
                    Mathf.Clamp01(
                        1f - radius / 0.5f
                    ),
                    2.5f
                );

                pixels[y * size + x] =
                    new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);

        return texture;
    }

    // -------------------------
    // HOLOGRAPHIC RING
    // -------------------------

    private static Texture2D MakeRing(int size)
    {
        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );

        Color[] pixels =
            new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx =
                    (x + 0.5f) / size - 0.5f;

                float dy =
                    (y + 0.5f) / size - 0.5f;

                float r =
                    Mathf.Sqrt(dx * dx + dy * dy);

                float ring = 1f - Mathf.SmoothStep(
                    0.0045f,
                    0.011f,
                    Mathf.Abs(r - 0.43f)
                );

                float angle =
                    Mathf.Atan2(dy, dx);

                float gap =
                    Mathf.Abs(Mathf.Sin(angle * 2f));

                float opacity =
                    Mathf.Clamp01((gap - 0.08f) * 12f);

                pixels[y * size + x] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        Mathf.Clamp01(ring * opacity)
                    );
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);

        return texture;
    }

    private static Color Alpha(
        Color color,
        float alpha)
    {
        color.a = alpha;
        return color;
    }

    private void OnDestroy()
    {
        if (ringSprite != null)
            Destroy(ringSprite);

        if (backgroundTexture != null)
            Destroy(backgroundTexture);

        if (glowTexture != null)
            Destroy(glowTexture);

        if (ringTexture != null)
            Destroy(ringTexture);
    }
}
