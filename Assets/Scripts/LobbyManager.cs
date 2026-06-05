using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 로비 화면의 제목, 시작 버튼, 종료 버튼을 생성하고 버튼 동작을 관리
public class LobbyManager : MonoBehaviour
{
    public string gameSceneName = "DemoScene"; // 게임 시작 버튼으로 이동할 실제 플레이 씬 이름
    public string backgroundResourceName = "LobbyBackground"; // Resources 폴더에서 불러올 로비 배경 이미지 이름
    public Font uiFont; // 기존 HUD 버튼과 같은 폰트를 사용하기 위한 참조

    private Font defaultFont;

    private void Start()
    {
        Time.timeScale = 1f;
        defaultFont = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        CreateEventSystemIfNeeded();
        CreateCameraIfNeeded();
        CreateLobbyUI();
    }

    // 버튼 클릭 입력을 처리할 EventSystem이 씬에 없으면 자동으로 생성
    private void CreateEventSystemIfNeeded()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    // 로비는 UI만 있어도 동작하지만, 카메라가 없으면 Unity가 경고를 띄우므로 기본 카메라를 준비
    private void CreateCameraIfNeeded()
    {
        if (Camera.main != null)
        {
            return;
        }

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.GetComponent<Camera>().backgroundColor = new Color(0.07f, 0.08f, 0.09f, 1f);
    }

    private void CreateLobbyUI()
    {
        GameObject canvasObject = new GameObject("Lobby Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        Image background = canvasObject.AddComponent<Image>();
        ApplyBackgroundImage(background);
        CreateDarkOverlay(canvasObject.transform);

        Text titleText = CreateText("Title Text", canvasObject.transform, "Monster Defense", 92, TextAnchor.MiddleCenter);
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 190f);
        titleRect.sizeDelta = new Vector2(1000f, 120f);

        int highScore = PlayerPrefs.GetInt(GameManager.HighScoreKey, 0);
        Text highScoreText = CreateText("High Score Text", canvasObject.transform, "High Score : " + highScore, 48, TextAnchor.MiddleCenter);
        RectTransform highScoreRect = highScoreText.GetComponent<RectTransform>();
        highScoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        highScoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        highScoreRect.pivot = new Vector2(0.5f, 0.5f);
        highScoreRect.anchoredPosition = new Vector2(0f, 95f);
        highScoreRect.sizeDelta = new Vector2(700f, 80f);

        Button startButton = CreateButton("Start Button", canvasObject.transform, "Start Game", new Vector2(0f, -35f));
        startButton.onClick.AddListener(StartGame);

        Button quitButton = CreateButton("Quit Button", canvasObject.transform, "Quit Game", new Vector2(0f, -175f));
        quitButton.onClick.AddListener(QuitGame);
    }

    private void ApplyBackgroundImage(Image background)
    {
        Sprite backgroundSprite = Resources.Load<Sprite>(backgroundResourceName);

        if (backgroundSprite == null)
        {
            background.color = new Color(0.07f, 0.08f, 0.09f, 1f);
            return;
        }

        background.sprite = backgroundSprite;
        background.color = Color.white;
        background.preserveAspect = true;
        background.raycastTarget = false;
    }

    private void CreateDarkOverlay(Transform parent)
    {
        GameObject overlayObject = new GameObject("Dark Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayObject.transform.SetParent(parent, false);

        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.28f);
        overlayImage.raycastTarget = false;
    }

    private Button CreateButton(string objectName, Transform parent, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(500f, 80f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = Color.black;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        Text buttonText = CreateText("Text", buttonObject.transform, label, 14, TextAnchor.MiddleCenter);
        buttonText.resizeTextForBestFit = true;
        buttonText.resizeTextMinSize = 10;
        buttonText.resizeTextMaxSize = 40;

        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private Text CreateText(string objectName, Transform parent, string text, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        Text uiText = textObject.GetComponent<Text>();
        uiText.text = text;
        uiText.font = defaultFont;
        uiText.fontSize = fontSize;
        uiText.alignment = alignment;
        uiText.color = Color.white;
        uiText.raycastTarget = false;

        return uiText;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
