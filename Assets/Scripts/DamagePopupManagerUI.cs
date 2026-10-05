using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro namespace

public class DamagePopUpManagerUI : MonoBehaviour
{
    [Header("UI Prefab Settings")]
    [Tooltip("Prefab containing a TextMeshProUGUI component.")]
    [SerializeField] private TMP_Text healthTextPrefab;
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, 0f);

    private Camera mainCamera;
    private Canvas parentCanvas;
    private RectTransform overlayRectTransform;

    private void Awake()
    {
        overlayRectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        // Ensure canvas setup matches expectation
        if (parentCanvas != null && parentCanvas.worldCamera != null)
        {
            mainCamera = parentCanvas.worldCamera;
        }
        else
        {
            mainCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        Enemy.OnAnyEnemyDamaged += HandleEnemyDamaged;
    }

    private void OnDisable()
    {
        Enemy.OnAnyEnemyDamaged -= HandleEnemyDamaged;
    }

    private void HandleEnemyDamaged(IDestructible destructible, float damage)
    {
        if (destructible == null || healthTextPrefab == null) return;

        // Instantiate pop-up element as a child of this overlay RectTransform
        TMP_Text popUpInstance = Instantiate(healthTextPrefab, transform);

        float healthPercentage = Mathf.Clamp01(destructible.CurrentHealth / destructible.MaxHealth) * 100f;
        popUpInstance.text = $"{healthPercentage:F0}%";

        MonoBehaviour targetMono = destructible as MonoBehaviour;
        Transform targetTransform = targetMono != null ? targetMono.transform : null;

        StartCoroutine(AnimateAndDestroyPopUp(popUpInstance.gameObject, targetTransform));
    }

    private IEnumerator AnimateAndDestroyPopUp(GameObject popUp, Transform targetTransform)
    {
        float elapsed = 0f;
        RectTransform popUpRect = popUp.GetComponent<RectTransform>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (targetTransform != null)
            {
                // Calculate world position to screen coordinate
                Vector3 worldPos = targetTransform.position + worldOffset;
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPos);

                // Convert screen position to canvas local position for Screen Space - Camera
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    overlayRectTransform,
                    screenPoint,
                    mainCamera,
                    out Vector2 localPoint))
                {
                    popUpRect.anchoredPosition = localPoint;
                }
            }

            yield return null;
        }

        Destroy(popUp);
    }
}