using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References (Legacy Text)")]
    [SerializeField] private Text scrapText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text hpText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel; // Assign a UI Panel with a Button inside it

    [Header("Death Scene Settings")]
    [SerializeField] private float slowMotionScale = 0.3f;
    [SerializeField] private float waitBeforeGameOver = 2.0f;
    [SerializeField] private string titleSceneName = "TitleScreen";

    private int scrapCollected = 0;
    private float gameTimer = 0f;
    private bool isTimerRunning = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        UpdateScrapUI();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            gameTimer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void AddScrap(int amount)
    {
        scrapCollected += amount;
        UpdateScrapUI();
    }

    public void UpdateHPUI(float currentHP, float maxHP)
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {currentHP} / {maxHP}";
        }
    }

    private void UpdateScrapUI()
    {
        if (scrapText != null)
        {
            scrapText.text = $"Scraps: {scrapCollected}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60F);
            int seconds = Mathf.FloorToInt(gameTimer - minutes * 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void TriggerDeathScene(Player player)
    {
        StartCoroutine(DeathSequence(player));
    }

    private IEnumerator DeathSequence(Player player)
    {
        isTimerRunning = false;

        // 1. Freeze the player in place
        player.rb.linearVelocity = Vector2.zero;
        player.rb.angularVelocity = 0f;
        player.rb.simulated = false; // Completely halts physics interactions on the player

        // 2. Play explosion particles
        if (player.explosionParticles != null)
        {
            player.explosionParticles.Play();
        }

        // 3. Hide sprite (Do NOT Destroy(gameObject) here, or particles and camera follow will instantly break)
        if (player.playerSprite != null)
        {
            player.playerSprite.enabled = false;
        }

        // 4. Slow-motion everything else
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Smooths out physics step for slow-mo

        // 5. Wait X seconds (use Realtime because Time.timeScale is altered)
        yield return new WaitForSecondsRealtime(waitBeforeGameOver);

        // 6. Complete freeze and show Game Over UI
        Time.timeScale = 0f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    /// 
    /// Link this method to your UI Button's "On Click()" event in the Inspector.
    /// 
    public void ReturnToTitle()
    {
        Time.timeScale = 1f; // Critical: Reset time scale before loading a new scene
        Time.fixedDeltaTime = 0.02f;
        SceneManager.LoadScene(titleSceneName);
    }
}