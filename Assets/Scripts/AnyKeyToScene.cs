using UnityEngine;
using UnityEngine.SceneManagement;

public class AnyKeyToScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The exact name of the scene to load in Build Settings.")]
    [SerializeField] private string sceneName;

    [Tooltip("Small delay in seconds before keypresses are registered (prevents instant skip).")]
    [SerializeField] private float inputDelay = 0.5f;

    private float timer = 0f;
    private bool isLoading = false;

    private void Update()
    {
        if (isLoading) return;

        timer += Time.deltaTime;

        if (timer >= inputDelay && Input.anyKeyDown)
        {
            LoadTargetScene();
        }
    }

    public void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            isLoading = true;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("AnyKeyToScene: Scene Name is empty! Please assign a scene name in the Inspector.", this);
        }
    }
}