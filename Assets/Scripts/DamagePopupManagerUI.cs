using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopUpManagerUI : MonoBehaviour
{
    [Header("UI Prefab Settings")]
    [SerializeField] private GameObject healthTextPrefab; // Prefab containing a Text or TextMeshProUGUI component
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, 0f);

    private Camera mainCamera;
    private RectTransform overlayRectTransform;

    private void Awake()
    {
        overlayRectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
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

        // Instantiate pop-up child element on this RectTransform
        GameObject popUp = Instantiate(healthTextPrefab, transform);
        Text textComponent = popUp.GetComponentInChildren<Text>();

        float healthPercentage = Mathf.Clamp01(destructible.CurrentHealth / destructible.MaxHealth) * 100f;

        if (textComponent != null)
        {
            textComponent.text = $"{healthPercentage:F0}%";
        }

        MonoBehaviour targetMono = destructible as MonoBehaviour;
        Transform targetTransform = targetMono != null ? targetMono.transform : null;

        StartCoroutine(AnimateAndDestroyPopUp(popUp, targetTransform));
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
                // Follow the target's world position on screen
                Vector3 screenPos = mainCamera.WorldToScreenPoint(targetTransform.position + worldOffset);
                popUpRect.position = screenPos;
            }

            yield return null;
        }

        Destroy(popUp);
    }
}