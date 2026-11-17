using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

// Minimal, mobile-first welcome screen for Hey, Forgy!
// Matches ForgyLocationGuide's palette and works with an existing UI Sprite.
public class ForgyWelcomeScreenFinal : MonoBehaviour
{
    [Header("Existing TextMeshPro Font")]
    [SerializeField] private TMP_FontAsset projectFont;

    [Header("ForgyWelcome2 (Sprite 2D and UI)")]
    [SerializeField] private Sprite forgyPortrait;

    [Header("Scene Opened by Enter AR")]
    [SerializeField] private string destinationScene = "MainScene_AreaTarget";

    [Header("Subtle Animation")]
    [SerializeField, Range(0f, 2f)] private float animationSpeed = 1f;

    private RectTransform safeRoot;
    private RectTransform mascotGroup;
    private RectTransform haloRing;
    private RectTransform glowRect;
    private RectTransform avatarFrame;
    private RectTransform avatarMask;
    private RectTransform portraitRect;

    private Image buttonGlow;
    private TMP_Text errorText;
    private Rect previousSafeArea;
    private Vector2 previousRootSize;
    private bool isEnteringAR;

    private Texture2D backgroundTexture;
    private Texture2D glowTexture;
    private Texture2D ringTexture;
    private Texture2D circleTexture;
    private Texture2D roundedTexture;
    private Sprite ringSprite;
    private Sprite circleSprite;
    private Sprite roundedSprite;

    // SAME palette as ForgyLocationGuide.
    private static readonly Color PanelColor = new Color32(7, 25, 42, 235);
    private static readonly Color CardColor = new Color32(20, 48, 67, 240);
    private static readonly Color SelectedColor = new Color32(30, 85, 103, 250);
    private static readonly Color Accent = new Color32(119, 229, 242, 255);
    private static readonly Color White = new Color32(237, 249, 255, 255);
    private static readonly Color Muted = new Color32(170, 203, 216, 255);

    private void Awake()
    {
        BuildScreen();
    }

    private void Start()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
            Debug.LogWarning("ForgyWelcomeScreen: The scene needs an EventSystem for ENTER AR.");

        if (projectFont == null)
            Debug.LogWarning("ForgyWelcomeScreen: Assign your Jupiter TMP font in the Inspector.");

        if (forgyPortrait == null)
            Debug.LogWarning("ForgyWelcomeScreen: Assign ForgyWelcome2 as a Sprite in the Inspector.");
    }

    private void Update()
    {
        if (Screen.safeArea != previousSafeArea)
            ApplySafeArea();

        if (safeRoot != null)
        {
            Vector2 currentSize = safeRoot.rect.size;
            if ((currentSize - previousRootSize).sqrMagnitude > 1f)
            {
                previousRootSize = currentSize;
                ResizeArtwork(currentSize);
            }
        }

        float t = Time.unscaledTime * animationSpeed;

        if (mascotGroup != null)
            mascotGroup.anchoredPosition = new Vector2(0f, Mathf.Sin(t * 1.1f) * 9f);

        if (haloRing != null)
            haloRing.localRotation = Quaternion.Euler(0f, 0f, t * 5f);

        if (buttonGlow != null)
        {
            Color color = Accent;
            color.a = 0.10f + 0.04f * (0.5f + 0.5f * Mathf.Sin(t * 1.2f));
            buttonGlow.color = color;
        }
    }

    private void BuildScreen()
    {
        GameObject canvasObject = new GameObject(
            "ForgyWelcome_UI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Generate very small reusable UI graphics once, not every frame.
        backgroundTexture = MakeBackground(4, 256);
        glowTexture = MakeGlow(256);
        ringTexture = MakeRing(512);
        circleTexture = MakeCircle(256);
        roundedTexture = MakeRoundedRectangle(128, 64, 18f);

        ringSprite = Sprite.Create(ringTexture, new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f));
        circleSprite = Sprite.Create(circleTexture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
        roundedSprite = Sprite.Create(
            roundedTexture, new Rect(0, 0, 128, 64), new Vector2(0.5f, 0.5f),
            100f, 0, SpriteMeshType.FullRect, new Vector4(22f, 22f, 22f, 22f));

        // Full-screen background; only the content uses iPhone safe-area bounds.
        RawImage background = MakeRaw(
            "Navy Background", canvasObject.transform, backgroundTexture,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Vector2(0.5f, 0.5f));
        background.color = Color.white;

        safeRoot = MakeRect(
            "iPhone Safe Area", canvasObject.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Vector2(0.5f, 0.5f));
        ApplySafeArea();

        // ----- HERO: Forgy portrait + restrained cyan hologram -----
        mascotGroup = MakeRect(
            "Forgy Hero", safeRoot,
            new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f),
            Vector2.zero, new Vector2(620f, 620f), new Vector2(0.5f, 0.5f));

        RawImage softGlow = MakeRaw(
            "Soft Cyan Light", mascotGroup, glowTexture,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(680f, 680f), new Vector2(0.5f, 0.5f));
        softGlow.color = WithAlpha(Accent, 0.24f);
        glowRect = softGlow.rectTransform;

        haloRing = MakeRect(
            "Slow Hologram Ring", mascotGroup,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(620f, 620f), new Vector2(0.5f, 0.5f));
        Image ring = haloRing.gameObject.AddComponent<Image>();
        ring.sprite = ringSprite;
        ring.color = WithAlpha(Accent, 0.48f);
        ring.raycastTarget = false;

        Image frame = MakeSpriteImage(
            "Forgy Portrait Background", mascotGroup, circleSprite,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(510f, 510f), CardColor);
        avatarFrame = frame.rectTransform;

        avatarMask = MakeRect(
            "Circular Portrait Crop", mascotGroup,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(490f, 490f), new Vector2(0.5f, 0.5f));
        Image maskGraphic = avatarMask.gameObject.AddComponent<Image>();
        maskGraphic.sprite = circleSprite;
        maskGraphic.color = Color.white;
        maskGraphic.raycastTarget = false;
        Mask mask = avatarMask.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        if (forgyPortrait != null)
        {
            Image portrait = MakeSpriteImage(
                "ForgyWelcome2", avatarMask, forgyPortrait,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(490f, 490f), Color.white);
            portrait.preserveAspect = true;
            portraitRect = portrait.rectTransform;
        }
        else
        {
            MakeText("Portrait Placeholder", "F", avatarMask,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400f, 400f), 220f, Accent,
                TextAlignmentOptions.Center, true);
        }

        // ----- CONTENT: only the information needed to begin -----
        MakeText("Title", "HEY, FORGY!", safeRoot,
            new Vector2(0.5f, 0.404f), new Vector2(0.5f, 0.404f),
            Vector2.zero, new Vector2(970f, 155f), 116f, White,
            TextAlignmentOptions.Center, true);

        MakeText("One Line Instruction", "Find Forgy at 3 spots on the fifth floor.", safeRoot,
            new Vector2(0.5f, 0.319f), new Vector2(0.5f, 0.319f),
            Vector2.zero, new Vector2(900f, 118f), 37f, Muted,
            TextAlignmentOptions.Center, false, true);

        // ----- PRIMARY CTA: large accessible touch area -----
        buttonGlow = MakeRoundedImage(
            "Button Glow", safeRoot,
            new Vector2(0.09f, 0.165f), new Vector2(0.91f, 0.165f),
            Vector2.zero, new Vector2(0f, 146f),
            WithAlpha(Accent, 0.12f));
        buttonGlow.raycastTarget = false;

        Image buttonImage = MakeRoundedImage(
            "ENTER AR", safeRoot,
            new Vector2(0.10f, 0.165f), new Vector2(0.90f, 0.165f),
            Vector2.zero, new Vector2(0f, 124f), Accent);
        buttonImage.raycastTarget = true;

        Button button = buttonImage.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock buttonColors = button.colors;
        buttonColors.normalColor = Color.white;
        buttonColors.highlightedColor = White;
        buttonColors.pressedColor = new Color32(172, 220, 230, 255);
        buttonColors.selectedColor = Color.white;
        buttonColors.disabledColor = WithAlpha(White, 0.50f);
        button.colors = buttonColors;
        button.onClick.AddListener(EnterAR);

        MakeText("Button Label", "ENTER AR", buttonImage.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(700f, 95f), 45f,
            PanelColor, TextAlignmentOptions.Center, true);

        // Hidden until something goes wrong, so the screen stays uncluttered.
        errorText = MakeText("Scene Error", "", safeRoot,
            new Vector2(0.5f, 0.068f), new Vector2(0.5f, 0.068f),
            Vector2.zero, new Vector2(920f, 70f), 27f, White,
            TextAlignmentOptions.Center, false, true);
    }

    private void EnterAR()
    {
        if (isEnteringAR)
            return;

        if (!Application.CanStreamedLevelBeLoaded(destinationScene))
        {
            Debug.LogError("ForgyWelcomeScreen: '" + destinationScene +
                           "' is not in Build Profiles > Scene List.");
            if (errorText != null)
                errorText.text = "AR scene is unavailable. Check the Scene List.";
            return;
        }

        isEnteringAR = true;
        SceneManager.LoadScene(destinationScene);
    }

    private void ApplySafeArea()
    {
        if (safeRoot == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        previousSafeArea = Screen.safeArea;
        safeRoot.anchorMin = new Vector2(
            previousSafeArea.xMin / Screen.width,
            previousSafeArea.yMin / Screen.height);
        safeRoot.anchorMax = new Vector2(
            previousSafeArea.xMax / Screen.width,
            previousSafeArea.yMax / Screen.height);
        safeRoot.offsetMin = Vector2.zero;
        safeRoot.offsetMax = Vector2.zero;
    }

    private void ResizeArtwork(Vector2 safeSize)
    {
        if (mascotGroup == null || safeSize.x <= 0f || safeSize.y <= 0f)
            return;

        // Scales the portrait down gracefully on narrower / shorter screens.
        float diameter = Mathf.Min(680f, safeSize.x * 0.76f, safeSize.y * 0.33f);
        mascotGroup.sizeDelta = Vector2.one * diameter;

        if (glowRect != null)
            glowRect.sizeDelta = Vector2.one * (diameter * 1.10f);
        if (haloRing != null)
            haloRing.sizeDelta = Vector2.one * (diameter * 0.97f);
        if (avatarFrame != null)
            avatarFrame.sizeDelta = Vector2.one * (diameter * 0.79f);
        if (avatarMask != null)
            avatarMask.sizeDelta = Vector2.one * (diameter * 0.76f);
        if (portraitRect != null)
            portraitRect.sizeDelta = Vector2.one * (diameter * 0.76f);
    }

    // ----- UI helpers -----

    private static RectTransform MakeRect(
        string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 position, Vector2 size, Vector2 pivot)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static RawImage MakeRaw(
        string name, Transform parent, Texture texture,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position,
        Vector2 size, Vector2 pivot)
    {
        RectTransform rect = MakeRect(
            name, parent, anchorMin, anchorMax, position, size, pivot);
        RawImage image = rect.gameObject.AddComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;
        return image;
    }

    private static Image MakeSpriteImage(
        string name, Transform parent, Sprite sprite,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position,
        Vector2 size, Color color)
    {
        RectTransform rect = MakeRect(
            name, parent, anchorMin, anchorMax, position, size,
            new Vector2(0.5f, 0.5f));
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private Image MakeRoundedImage(
        string name, Transform parent, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
    {
        Image image = MakeSpriteImage(
            name, parent, roundedSprite, anchorMin, anchorMax,
            position, size, color);
        image.type = Image.Type.Sliced;
        return image;
    }

    private TextMeshProUGUI MakeText(
        string name, string value, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position,
        Vector2 size, float fontSize, Color color,
        TextAlignmentOptions alignment, bool bold = false, bool wrap = false)
    {
        RectTransform rect = MakeRect(
            name, parent, anchorMin, anchorMax, position, size,
            new Vector2(0.5f, 0.5f));
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (projectFont != null)
            text.font = projectFont;
        text.text = value;
        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    // ----- Small procedural UI textures -----

    private static Texture2D MakeBackground(int width, int height)
    {
        Texture2D texture = NewTexture(width, height);
        Color[] pixels = new Color[width * height];
        Color bottom = new Color32(7, 25, 42, 255);
        Color top = new Color32(17, 42, 59, 255);

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color color = Color.Lerp(bottom, top, t);
            for (int x = 0; x < width; x++)
                pixels[y * width + x] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D MakeGlow(int size)
    {
        Texture2D texture = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f) / size - 0.5f;
            float dy = (y + 0.5f) / size - 0.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Pow(Mathf.Clamp01(1f - r * 2f), 2.3f);
            pixels[y * size + x] = new Color(1f, 1f, 1f, a);
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D MakeCircle(int size)
    {
        Texture2D texture = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f) / size - 0.5f;
            float dy = (y + 0.5f) / size - 0.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Clamp01((0.5f - r) * size);
            pixels[y * size + x] = new Color(1f, 1f, 1f, a);
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D MakeRing(int size)
    {
        Texture2D texture = NewTexture(size, size);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f) / size - 0.5f;
            float dy = (y + 0.5f) / size - 0.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            float edge = Mathf.Abs(r - 0.47f);
            float line = 1f - Mathf.SmoothStep(0.0015f, 0.006f, edge);
            float angle = Mathf.Atan2(dy, dx);
            float segment = Mathf.Clamp01((Mathf.Abs(Mathf.Sin(angle * 2f)) - 0.08f) * 12f);
            pixels[y * size + x] = new Color(1f, 1f, 1f, line * segment);
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D MakeRoundedRectangle(int width, int height, float radius)
    {
        Texture2D texture = NewTexture(width, height);
        Color[] pixels = new Color[width * height];
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float qx = Mathf.Abs(x + 0.5f - halfW) - (halfW - radius);
            float qy = Mathf.Abs(y + 0.5f - halfH) - (halfH - radius);
            float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
                                       Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
            float signedDistance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
            float alpha = Mathf.Clamp01(0.5f - signedDistance);
            pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D NewTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    private void OnDestroy()
    {
        if (ringSprite != null) Destroy(ringSprite);
        if (circleSprite != null) Destroy(circleSprite);
        if (roundedSprite != null) Destroy(roundedSprite);
        if (backgroundTexture != null) Destroy(backgroundTexture);
        if (glowTexture != null) Destroy(glowTexture);
        if (ringTexture != null) Destroy(ringTexture);
        if (circleTexture != null) Destroy(circleTexture);
        if (roundedTexture != null) Destroy(roundedTexture);
    }
}
