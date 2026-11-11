
using UnityEngine;

public class ForgyEyes : MonoBehaviour
{
    [Header("Original Eye Meshes")]
    public MeshFilter leftEye;
    public MeshFilter rightEye;

    [Header("Wave Animation")]
    public ForgyWave wave;
    public float transitionSpeed = 2.5f;

    [Header("Happy Eye Shape")]
    [Range(0.2f, 1.5f)]
    public float archHeight = 1f;

    [Range(0.05f, 0.5f)]
    public float eyeThickness = 0.18f;

    [Range(0.5f, 2.5f)]
    public float happyWidth = 1.5f;

    [Header("Happy Eye Position")]
    [Range(0f, 3f)]
    public float leftInward = 1.2f;

    [Range(0f, 3f)]
    public float rightInward = 1.2f;

    [Range(-1f, 2f)]
    public float upwardShift = 0.2f;

    [Header("Cheer Animation")]
    public ForgyCheer cheer;

    [Header("Cheer Eye Shape ><")]
    [Range(0.2f, 2f)]
    public float cheerWidth = 1f;

    [Range(0.2f, 1.5f)]
    public float cheerHeight = 0.85f;

    [Range(0.05f, 0.5f)]
    public float cheerThickness = 0.15f;

    // NEW: Sleepy expression for Relax
    [Header("Sleepy / Relax Animation")]
    public ForgyRelax relax;

    // Turn this on ONLY for Sofa Forgy.
    public bool enableSleepyOnRelax = false;

    [Header("Sleepy Eye Shape")]
    [Range(0.2f, 1.5f)]
    public float sleepyArchHeight = 1f;

    [Range(0.05f, 0.5f)]
    public float sleepyThickness = 0.18f;

    [Range(0.5f, 2.5f)]
    public float sleepyWidth = 1.5f;

    [Range(-1f, 2f)]
    public float sleepyDownwardShift = 0.15f;

    private readonly MeshFilter[] eyes =
        new MeshFilter[2];

    private readonly Mesh[] runtimeMeshes =
        new Mesh[2];

    private readonly Mesh[] sourceMeshes =
        new Mesh[2];

    private readonly Vector3[][] originalVertices =
        new Vector3[2][];

    private readonly Vector3[][] happyVertices =
        new Vector3[2][];

    private readonly Vector3[][] cheerVertices =
        new Vector3[2][];

    // NEW: Sleepy vertex positions
    private readonly Vector3[][] sleepyVertices =
        new Vector3[2][];

    private readonly Vector3[][] workingVertices =
        new Vector3[2][];

    private float happyAmount;
    private float cheerAmount;
    private float sleepyAmount;

    void Start()
    {
        if (wave == null)
            wave = GetComponent<ForgyWave>();

        if (cheer == null)
            cheer = GetComponent<ForgyCheer>();

        if (relax == null)
            relax = GetComponent<ForgyRelax>();

        eyes[0] = leftEye;
        eyes[1] = rightEye;

        if (leftEye == null || rightEye == null)
        {
            Debug.LogError(
                "ForgyEyes: Assign both eye meshes."
            );

            enabled = false;
            return;
        }

        for (int i = 0; i < 2; i++)
        {
            sourceMeshes[i] = eyes[i].sharedMesh;

            if (sourceMeshes[i] == null ||
                !sourceMeshes[i].isReadable)
            {
                Debug.LogError(
                    "ForgyEyes: Eye mesh missing " +
                    "or Read/Write disabled."
                );

                enabled = false;
                return;
            }
        }

        // Eye centers in Forgy's coordinate space.
        Vector3 leftCenter = ToForgySpace(
            leftEye.transform,
            sourceMeshes[0].bounds.center
        );

        Vector3 rightCenter = ToForgySpace(
            rightEye.transform,
            sourceMeshes[1].bounds.center
        );

        // Horizontal follows the line between eyes.
        Vector3 horizontal =
            (rightCenter - leftCenter).normalized;

        if (horizontal.sqrMagnitude < 0.001f)
            horizontal = Vector3.right;

        // Keep the original working face directions.
        Vector3 vertical = Vector3.ProjectOnPlane(
            Vector3.up,
            horizontal
        ).normalized;

        if (vertical.sqrMagnitude < 0.001f)
            vertical = Vector3.forward;

        Vector3 middle =
            (leftCenter + rightCenter) * 0.5f;

        SetupEye(
            0,
            leftCenter,
            middle,
            horizontal,
            vertical,
            leftInward
        );

        SetupEye(
            1,
            rightCenter,
            middle,
            horizontal,
            vertical,
            rightInward
        );
    }

    Vector3 ToForgySpace(
        Transform eye,
        Vector3 point)
    {
        return transform.InverseTransformPoint(
            eye.TransformPoint(point)
        );
    }

    Vector3 ToEyeSpace(
        Transform eye,
        Vector3 point)
    {
        return eye.InverseTransformPoint(
            transform.TransformPoint(point)
        );
    }

    void SetupEye(
        int index,
        Vector3 center,
        Vector3 middle,
        Vector3 horizontal,
        Vector3 vertical,
        float inwardAmount)
    {
        MeshFilter eye = eyes[index];

        Mesh copy = Instantiate(sourceMeshes[index]);

        copy.name =
            sourceMeshes[index].name +
            "_AnimatedRuntime";

        copy.MarkDynamic();

        eye.sharedMesh = copy;
        runtimeMeshes[index] = copy;

        Vector3[] original = copy.vertices;

        originalVertices[index] = original;

        happyVertices[index] =
            new Vector3[original.Length];

        cheerVertices[index] =
            new Vector3[original.Length];

        sleepyVertices[index] =
            new Vector3[original.Length];

        workingVertices[index] =
            new Vector3[original.Length];

        Vector3[] points =
            new Vector3[original.Length];

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        // Measure eye in Forgy's face coordinates.
        for (int i = 0; i < original.Length; i++)
        {
            points[i] = ToForgySpace(
                eye.transform,
                original[i]
            );

            Vector3 relative =
                points[i] - center;

            float x = Vector3.Dot(
                relative,
                horizontal
            );

            float y = Vector3.Dot(
                relative,
                vertical
            );

            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);

            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }

        float centerX =
            (minX + maxX) * 0.5f;

        float centerY =
            (minY + maxY) * 0.5f;

        float halfWidth = Mathf.Max(
            0.00001f,
            (maxX - minX) * 0.5f
        );

        float halfHeight = Mathf.Max(
            0.00001f,
            (maxY - minY) * 0.5f
        );

        float inwardSign = Mathf.Sign(
            Vector3.Dot(
                middle - center,
                horizontal
            )
        );

        for (int i = 0; i < original.Length; i++)
        {
            Vector3 relative =
                points[i] - center;

            float x = Vector3.Dot(
                relative,
                horizontal
            );

            float y = Vector3.Dot(
                relative,
                vertical
            );

            float u = Mathf.Clamp(
                (x - centerX) / halfWidth,
                -1f,
                1f
            );

            // ====================================
            // WAVE EXPRESSION: ^^
            // ORIGINAL FORMULA - UNCHANGED
            // ====================================

            float curve =
                (0.6f - u * u)
                * halfHeight * archHeight;

            float targetX =
                centerX +
                u * halfWidth * happyWidth +
                inwardSign *
                halfWidth * inwardAmount;

            float targetY =
                centerY +
                upwardShift * halfHeight +
                curve +
                (y - centerY) * eyeThickness;

            Vector3 targetPoint =
                points[i] +
                horizontal * (targetX - x) +
                vertical * (targetY - y);

            happyVertices[index][i] = ToEyeSpace(
                eye.transform,
                targetPoint
            );

            // ====================================
            // CHEER EXPRESSION: ><
            // ORIGINAL FORMULA - UNCHANGED
            // ====================================

            float v = Mathf.Clamp(
                (y - centerY) / halfHeight,
                -1f,
                1f
            );

            float chevron =
                (0.5f - Mathf.Abs(v))
                * halfWidth * cheerWidth;

            float cheerX =
                centerX +
                inwardSign * chevron +
                (x - centerX) * cheerThickness;

            float cheerY =
                centerY +
                (y - centerY) * cheerHeight;

            Vector3 cheerPoint =
                points[i] +
                horizontal * (cheerX - x) +
                vertical * (cheerY - y);

            cheerVertices[index][i] = ToEyeSpace(
                eye.transform,
                cheerPoint
            );

            // ====================================
            // SLEEPY EXPRESSION
            // Inverted Wave curve: downward arch
            // ====================================

            float sleepyCurve =
                -(0.6f - u * u)
                * halfHeight * sleepyArchHeight;

            float sleepyX =
                centerX +
                u * halfWidth * sleepyWidth;

            float sleepyY =
                centerY -
                sleepyDownwardShift * halfHeight +
                sleepyCurve +
                (y - centerY) * sleepyThickness;

            Vector3 sleepyPoint =
                points[i] +
                horizontal * (sleepyX - x) +
                vertical * (sleepyY - y);

            sleepyVertices[index][i] = ToEyeSpace(
                eye.transform,
                sleepyPoint
            );
        }
    }

    void LateUpdate()
    {
        bool isWaving =
            wave != null && wave.IsWaving;

        bool isCheering =
            cheer != null && cheer.IsCheering;

        // Sleepy is enabled only on selected Forgy.
        bool isRelaxing =
            enableSleepyOnRelax &&
            relax != null &&
            relax.IsRelaxing;

        // Priorities:
        // Relax > Cheer > Wave.
        float happyTarget =
            isWaving && !isCheering && !isRelaxing
                ? 1f
                : 0f;

        float cheerTarget =
            isCheering && !isRelaxing
                ? 1f
                : 0f;

        float sleepyTarget =
            isRelaxing ? 1f : 0f;

        float step =
            Time.deltaTime * transitionSpeed;

        float nextHappy = Mathf.MoveTowards(
            happyAmount,
            happyTarget,
            step
        );

        float nextCheer = Mathf.MoveTowards(
            cheerAmount,
            cheerTarget,
            step
        );

        float nextSleepy = Mathf.MoveTowards(
            sleepyAmount,
            sleepyTarget,
            step
        );

        if (
            Mathf.Approximately(nextHappy, happyAmount) &&
            Mathf.Approximately(nextCheer, cheerAmount) &&
            Mathf.Approximately(nextSleepy, sleepyAmount)
        )
        {
            return;
        }

        happyAmount = nextHappy;
        cheerAmount = nextCheer;
        sleepyAmount = nextSleepy;

        float happyBlend = Mathf.SmoothStep(
            0f,
            1f,
            happyAmount
        );

        float cheerBlend = Mathf.SmoothStep(
            0f,
            1f,
            cheerAmount
        );

        float sleepyBlend = Mathf.SmoothStep(
            0f,
            1f,
            sleepyAmount
        );

        for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++)
        {
            Mesh mesh = runtimeMeshes[eyeIndex];

            if (mesh == null)
                continue;

            Vector3[] original =
                originalVertices[eyeIndex];

            Vector3[] happy =
                happyVertices[eyeIndex];

            Vector3[] excited =
                cheerVertices[eyeIndex];

            Vector3[] sleepy =
                sleepyVertices[eyeIndex];

            Vector3[] working =
                workingVertices[eyeIndex];

            for (int i = 0; i < original.Length; i++)
            {
                // Original -> Wave
                Vector3 wavePosition = Vector3.Lerp(
                    original[i],
                    happy[i],
                    happyBlend
                );

                // Wave -> Cheer
                Vector3 cheerPosition = Vector3.Lerp(
                    wavePosition,
                    excited[i],
                    cheerBlend
                );

                // Cheer / Normal -> Sleepy
                working[i] = Vector3.Lerp(
                    cheerPosition,
                    sleepy[i],
                    sleepyBlend
                );
            }

            mesh.vertices = working;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }

    void OnDestroy()
    {
        foreach (MeshFilter eye in eyes)
        {
            if (eye == null)
                continue;

            for (int i = 0; i < 2; i++)
            {
                if (
                    eyes[i] == eye &&
                    sourceMeshes[i] != null
                )
                {
                    eye.sharedMesh = sourceMeshes[i];
                    break;
                }
            }
        }

        foreach (Mesh mesh in runtimeMeshes)
        {
            if (mesh != null)
                Destroy(mesh);
        }
    }
}
