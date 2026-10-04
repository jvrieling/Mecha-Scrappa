using UnityEngine;
using UnityEngine.UI;

public class OffscreenEnemyTrackerUI : MonoBehaviour
{
    [SerializeField] private RectTransform pointerUI;
    [SerializeField] private Image pointerGraphic; // The graphic component to enable/disable
    [SerializeField] private float edgeMargin = 40f; // Padding from screen boundary

    [Header("Center Exclusion Settings")]
    [Tooltip("Fraction of the camera view considered 'center area' (0.2 = center 20%). Enemies inside this region hide the tracker.")]
    [Range(0f, 1f)]
    [SerializeField] private float centerThreshold = 0.2f; // 20% by default

    private Camera mainCamera;
    private Canvas parentCanvas;
    private RectTransform canvasRect;

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas != null && parentCanvas.worldCamera != null)
        {
            mainCamera = parentCanvas.worldCamera;
        }
        else
        {
            mainCamera = Camera.main;
        }

        if (parentCanvas != null)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
        }

        if (pointerUI == null)
        {
            pointerUI = GetComponent<RectTransform>();
        }

        // Auto-get Image if not manually assigned
        if (pointerGraphic == null)
        {
            pointerGraphic = pointerUI.GetComponent<Image>();
        }
    }

    private void Update()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        bool anyInCenter = false;
        Enemy closestOutsideEnemy = null;
        float minDistanceSq = float.MaxValue;

        Vector3 cameraPos = mainCamera.transform.position;

        // Calculate normalized center boundaries in Viewport space (0 to 1)
        float halfCenter = centerThreshold * 0.5f;
        float minCenter = 0.5f - halfCenter;
        float maxCenter = 0.5f + halfCenter;

        foreach (Enemy enemy in enemies)
        {
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(enemy.transform.position);

            // Check if enemy is within the center region and in front of the camera
            bool isInCenter = viewportPos.z > 0 &&
                              viewportPos.x >= minCenter && viewportPos.x <= maxCenter &&
                              viewportPos.y >= minCenter && viewportPos.y <= maxCenter;

            if (isInCenter)
            {
                anyInCenter = true;
                break; // An enemy is within the center 20%; hide indicator
            }

            float distSq = (enemy.transform.position - cameraPos).sqrMagnitude;
            if (distSq < minDistanceSq)
            {
                minDistanceSq = distSq;
                closestOutsideEnemy = enemy;
            }
        }

        // Hide pointer graphic if an enemy is in the center region or no outside enemy exists
        if (anyInCenter || closestOutsideEnemy == null)
        {
            SetGraphicVisible(false);
            return;
        }

        SetGraphicVisible(true);

        // Calculate target screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(closestOutsideEnemy.transform.position);

        // Reverse target ray if enemy is behind camera plane
        if (screenPos.z < 0)
        {
            screenPos *= -1f;
        }

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 fromCenter = (Vector2)screenPos - screenCenter;

        float maxX = Screen.width * 0.5f - edgeMargin;
        float maxY = Screen.height * 0.5f - edgeMargin;

        float scale = Mathf.Min(
            Mathf.Abs(maxX / (fromCenter.x != 0 ? fromCenter.x : 0.0001f)),
            Mathf.Abs(maxY / (fromCenter.y != 0 ? fromCenter.y : 0.0001f))
        );

        Vector2 clampedScreenPos = screenCenter + fromCenter * scale;

        // Convert clamped screen position to canvas local space for Screen Space - Camera Canvas
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            clampedScreenPos,
            mainCamera,
            out Vector2 localPoint))
        {
            pointerUI.anchoredPosition = localPoint;
        }

        // Rotate the tracker icon toward the target direction
        float angle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg;
        pointerUI.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void SetGraphicVisible(bool visible)
    {
        if (pointerGraphic != null && pointerGraphic.enabled != visible)
        {
            pointerGraphic.enabled = visible;
        }
    }
}