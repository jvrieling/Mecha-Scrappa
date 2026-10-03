using UnityEngine;
using DG.Tweening;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody2D targetRb;

    [Header("Follow Settings")]
    [Tooltip("Base time to smooth toward target when outside the 20% deadzone.")]
    [SerializeField] private float baseSmoothTime = 0.25f;

    [Header("Shake Settings")]
    [SerializeField] private float defaultShakeDuration = 0.15f;
    [SerializeField] private float defaultShakeStrength = 0.4f;
    [SerializeField] private int defaultShakeVibrato = 20;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void Start()
    {
        if (target != null)
        {
            if (targetRb == null) targetRb = target.GetComponent<Rigidbody2D>();
            targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Convert target position to Normalized Viewport Coordinates (0.0 to 1.0)
        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);

        // Center 20% deadzone means viewport bounds are between 0.40 and 0.60
        bool outsideDeadzone = viewportPos.x < 0.45f || viewportPos.x > 0.55f ||
                               viewportPos.y < 0.45f || viewportPos.y > 0.55f;

        if (outsideDeadzone)
        {
            targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
        }

        // Calculate proximity to screen edge on X and Y (0.0 at center, 1.0 at screen edge)
        float edgeDistX = Mathf.Abs(viewportPos.x - 0.5f) * 2f;
        float edgeDistY = Mathf.Abs(viewportPos.y - 0.5f) * 2f;
        float maxEdgeDist = Mathf.Max(edgeDistX, edgeDistY); // 0.9 = 90% to screen edge

        // Calculate target speed factor (scales up to 1.5x at 90%+ proximity)
        float speedMultiplier = 1.0f;
        if (maxEdgeDist >= 0.9f)
        {
            speedMultiplier = 1.5f;
        }
        else if (maxEdgeDist > 0.2f)
        {
            // Smooth blend from 1.0x at deadzone edge (20%) to 1.5x at 90% edge proximity
            float t = (maxEdgeDist - 0.2f) / (0.9f - 0.2f);
            speedMultiplier = Mathf.Lerp(1.0f, 1.5f, t);
        }

        // Dynamically reduce smoothTime to increase tracking speed
        float effectiveSmoothTime = baseSmoothTime / speedMultiplier;

        // Smoothly glide camera toward target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, effectiveSmoothTime);
    }

    /// <summary>
    /// Triggers screen shake via DOTween.
    /// </summary>
    public static void Shake(float duration = 0.15f, float strength = 0.4f, int vibrato = 20)
    {
        if (Instance == null) return;

        Instance.transform.DOKill(complete: true);
        Instance.transform.DOShakePosition(duration, strength, vibrato);
    }
}