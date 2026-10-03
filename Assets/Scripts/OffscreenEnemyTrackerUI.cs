using UnityEngine;
using UnityEngine.UI;

public class OffscreenEnemyTrackerUI : MonoBehaviour
{
    [SerializeField] private RectTransform pointerUI;
    [SerializeField] private float edgeMargin = 40f; // Padding from screen boundary

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (pointerUI == null) pointerUI = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        bool anyOnScreen = false;
        Enemy closestOffscreenEnemy = null;
        float minDistanceSq = float.MaxValue;

        Vector3 cameraPos = mainCamera.transform.position;

        foreach (Enemy enemy in enemies)
        {
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(enemy.transform.position);

            // Enemy is on screen if viewport X and Y are within [0, 1] and in front of camera
            bool isOnScreen = viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

            if (isOnScreen)
            {
                anyOnScreen = true;
                break; // Screen is active; hide indicator immediately
            }

            float distSq = (enemy.transform.position - cameraPos).sqrMagnitude;
            if (distSq < minDistanceSq)
            {
                minDistanceSq = distSq;
                closestOffscreenEnemy = enemy;
            }
        }

        // Hide pointer if an enemy is visible or no offscreen enemy exists
        if (anyOnScreen || closestOffscreenEnemy == null)
        {
            if (pointerUI.gameObject.activeSelf)
                pointerUI.gameObject.SetActive(false);
            return;
        }

        if (!pointerUI.gameObject.activeSelf)
            pointerUI.gameObject.SetActive(true);

        // Clamp pointer along screen border
        Vector3 screenPos = mainCamera.WorldToScreenPoint(closestOffscreenEnemy.transform.position);

        // Reverse if target is behind the camera plane
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

        Vector2 clampedPos = screenCenter + fromCenter * scale;
        pointerUI.position = clampedPos;

        // Rotate the tracker icon toward the target
        float angle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg;
        pointerUI.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}