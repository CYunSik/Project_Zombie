using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리자 관련 코드
using UnityEngine.UI; // UI 관련 코드

// 필요한 UI에 즉시 접근하고 변경할 수 있도록 허용하는 UI 매니저
public class UIManager : MonoBehaviour {
    // 싱글톤 접근용 프로퍼티
    public static UIManager instance
    {
        get
        {
            if (m_instance == null)
            {
                // FindFirstObjectByType으로 씬의 UIManager 오브젝트를 찾아 할당
                m_instance = FindFirstObjectByType<UIManager>();
            }

            return m_instance;
        }
    }

    private static UIManager m_instance; // 싱글톤이 할당될 변수

    public Text ammoText; // 탄약 표시용 텍스트
    public Text scoreText; // 점수 표시용 텍스트
    public Text waveText; // 적 웨이브 표시용 텍스트
    public Text levelText; // 플레이어 레벨 표시용 텍스트
    public Text experienceText; // 플레이어 경험치 표시용 텍스트
    public Text timerText; // 진행 시간 표시용 텍스트
    public GameObject gameoverUI; // 게임 오버시 활성화할 UI 

    private PlayerExperience playerExperience; // HUD에 표시할 플레이어 경험치 정보
    private LevelUpUpgradeManager levelUpUpgradeManager; // 레벨업 카드 선택 UI 관리
    private float playTime; // 게임 진행 시간

    private void Start() {
        CreateTimerTextIfNeeded();
        SetupExperienceUI();
        UpdateTimerText();
    }

    private void Update() {
        if (GameManager.instance != null && GameManager.instance.isGameover)
        {
            return;
        }

        // Time.deltaTime은 Time.timeScale이 0이면 증가하지 않으므로 레벨업 카드 선택 중에는 타이머도 멈춘다
        playTime += Time.deltaTime;
        UpdateTimerText();
    }

    // 탄약 텍스트 갱신
    public void UpdateAmmoText(int magAmmo, int remainAmmo) {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    // 점수 텍스트 갱신
    public void UpdateScoreText(int newScore) {
        scoreText.text = "Score : " + newScore;
    }

    // 적 웨이브 텍스트 갱신
    public void UpdateWaveText(int waves, int count) {
        waveText.text = "Wave : " + waves + "\nEnemy Left : " + count;
    }

    // 플레이어 경험치 텍스트 갱신
    public void UpdateExperienceText(int level, int currentExperience, int experienceToNextLevel) {
        levelText.text = "Lv. " + level;
        experienceText.text = "EXP : " + currentExperience + " / " + experienceToNextLevel;
    }

    // 진행 시간 텍스트 갱신
    public void UpdateTimerText() {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.FloorToInt(playTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = "Time : " + minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    // 게임 오버 UI 활성화
    public void SetActiveGameoverUI(bool active) {
        gameoverUI.SetActive(active);
    }

    // 게임 재시작
    public void GameRestart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SetupExperienceUI() {
        CreateExperienceTextsIfNeeded();

        playerExperience = FindFirstObjectByType<PlayerExperience>();

        if (playerExperience == null)
        {
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();

            if (playerHealth != null)
            {
                playerExperience = playerHealth.GetComponent<PlayerExperience>();

                if (playerExperience == null)
                {
                    playerExperience = playerHealth.gameObject.AddComponent<PlayerExperience>();
                }
            }
        }

        if (playerExperience != null)
        {
            playerExperience.onExperienceChanged += UpdateExperienceText;
            UpdateExperienceText(playerExperience.level, playerExperience.currentExperience, playerExperience.experienceToNextLevel);
            SetupLevelUpUpgradeManager();
        }
    }

    private void SetupLevelUpUpgradeManager() {
        levelUpUpgradeManager = GetComponent<LevelUpUpgradeManager>();

        if (levelUpUpgradeManager == null)
        {
            levelUpUpgradeManager = gameObject.AddComponent<LevelUpUpgradeManager>();
        }

        levelUpUpgradeManager.Initialize(playerExperience);
    }

    private void CreateExperienceTextsIfNeeded() {
        if (levelText == null)
        {
            levelText = CreateHudText("Level Text", new Vector2(20f, -20f), new Vector2(240f, 50f), 35);
        }

        if (experienceText == null)
        {
            experienceText = CreateHudText("Experience Text", new Vector2(20f, -70f), new Vector2(360f, 45f), 26);
        }
    }

    private void CreateTimerTextIfNeeded() {
        if (timerText != null)
        {
            return;
        }

        GameObject textObject = new GameObject("Timer Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(transform, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-20f, -20f);
        rectTransform.sizeDelta = new Vector2(360f, 60f);
        rectTransform.pivot = new Vector2(1f, 1f);

        timerText = textObject.GetComponent<Text>();
        timerText.font = scoreText != null ? scoreText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.fontSize = 35;
        timerText.alignment = TextAnchor.MiddleRight;
        timerText.color = Color.white;
        timerText.raycastTarget = false;
    }

    private Text CreateHudText(string objectName, Vector2 anchoredPosition, Vector2 size, int fontSize) {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(transform, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        rectTransform.pivot = new Vector2(0f, 1f);

        Text text = textObject.GetComponent<Text>();
        text.font = scoreText != null ? scoreText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        text.raycastTarget = false;

        return text;
    }

    private void OnDestroy() {
        if (playerExperience != null)
        {
            playerExperience.onExperienceChanged -= UpdateExperienceText;
        }
    }
}
