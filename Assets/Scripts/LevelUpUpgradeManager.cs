using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 레벨업시 3개의 랜덤 업그레이드 카드를 보여주고 선택한 능력을 적용
public class LevelUpUpgradeManager : MonoBehaviour
{
    private const int CardCount = 3;
    private const float SelectionDelay = 1f;

    private readonly List<UpgradeOption> allOptions = new List<UpgradeOption>();
    private readonly Button[] cardButtons = new Button[CardCount];
    private readonly Text[] cardTitleTexts = new Text[CardCount];
    private readonly Text[] cardDescriptionTexts = new Text[CardCount];

    private PlayerExperience playerExperience;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;
    private ItemSpawner itemSpawner;

    private GameObject panelObject;
    private bool isShowing;
    private bool canSelect;
    private bool isLevelUpSequenceActive;
    private int pendingLevelUps;
    private float timeScaleBeforePause = 1f;

    public void Initialize(PlayerExperience experience)
    {
        if (playerExperience != null)
        {
            playerExperience.onLevelUp -= HandleLevelUp;
        }

        playerExperience = experience;

        FindGameplayReferences();
        CreateOptions();
        CreatePanelIfNeeded();

        if (playerExperience != null)
        {
            playerExperience.onLevelUp += HandleLevelUp;
        }
    }

    private void FindGameplayReferences()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerShooter = FindFirstObjectByType<PlayerShooter>();
        itemSpawner = FindFirstObjectByType<ItemSpawner>();
    }

    private void CreateOptions()
    {
        allOptions.Clear();
        allOptions.Add(new UpgradeOption("Max HP +10", "Maximum health increases by 10.", ApplyMaxHealth));
        allOptions.Add(new UpgradeOption("Move Speed +1", "Player movement speed increases by 1.", ApplyMoveSpeed));
        allOptions.Add(new UpgradeOption("Ammo +100", "Reserve ammo increases by 100.", ApplyAmmo));
        allOptions.Add(new UpgradeOption("Magazine +5", "Magazine capacity increases by 5.", ApplyMagazineCapacity));
        allOptions.Add(new UpgradeOption("Gun Damage +5", "Gun damage increases by 5.", ApplyGunDamage));
        allOptions.Add(new UpgradeOption("Item Burst", "Spawn 5 items around the player.", ApplyItemBurst));
        allOptions.Add(new UpgradeOption("Score +1000", "Score increases by 1000.", ApplyScore));
        allOptions.Add(new UpgradeOption("Heal +50", "Current health recovers by 50.", ApplyHeal));
    }

    private void HandleLevelUp(int newLevel)
    {
        pendingLevelUps++;

        if (!isShowing)
        {
            ShowNextUpgrade();
        }
    }

    private void ShowNextUpgrade()
    {
        if (pendingLevelUps <= 0)
        {
            return;
        }

        pendingLevelUps--;
        isShowing = true;
        canSelect = false;

        if (!isLevelUpSequenceActive)
        {
            timeScaleBeforePause = Time.timeScale;
            isLevelUpSequenceActive = true;
        }

        Time.timeScale = 0f;

        panelObject.SetActive(true);
        List<UpgradeOption> selectedOptions = PickRandomOptions();

        for (int i = 0; i < cardButtons.Length; i++)
        {
            UpgradeOption option = selectedOptions[i];
            cardTitleTexts[i].text = option.title;
            cardDescriptionTexts[i].text = option.description;
            cardButtons[i].interactable = false;

            cardButtons[i].onClick.RemoveAllListeners();
            cardButtons[i].onClick.AddListener(() => SelectOption(option));
        }

        StartCoroutine(EnableSelectionAfterDelay());
    }

    private IEnumerator EnableSelectionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(SelectionDelay);

        canSelect = true;

        for (int i = 0; i < cardButtons.Length; i++)
        {
            cardButtons[i].interactable = true;
        }
    }

    private List<UpgradeOption> PickRandomOptions()
    {
        List<UpgradeOption> optionPool = new List<UpgradeOption>(allOptions);
        List<UpgradeOption> selectedOptions = new List<UpgradeOption>();

        for (int i = 0; i < CardCount && optionPool.Count > 0; i++)
        {
            int selectedIndex = Random.Range(0, optionPool.Count);
            selectedOptions.Add(optionPool[selectedIndex]);
            optionPool.RemoveAt(selectedIndex);
        }

        return selectedOptions;
    }

    private void SelectOption(UpgradeOption option)
    {
        if (!canSelect)
        {
            return;
        }

        canSelect = false;
        option.Apply();
        panelObject.SetActive(false);
        isShowing = false;

        if (pendingLevelUps > 0)
        {
            ShowNextUpgrade();
            return;
        }

        Time.timeScale = timeScaleBeforePause;
        isLevelUpSequenceActive = false;
    }

    private void ApplyMaxHealth()
    {
        if (playerHealth != null)
        {
            playerHealth.IncreaseMaxHealth(10f);
        }
    }

    private void ApplyMoveSpeed()
    {
        if (playerMovement != null)
        {
            playerMovement.moveSpeed += 1f;
        }
    }

    private void ApplyAmmo()
    {
        if (playerShooter != null && playerShooter.gun != null)
        {
            playerShooter.gun.ammoRemain += 100;
        }
    }

    private void ApplyGunDamage()
    {
        if (playerShooter != null && playerShooter.gun != null)
        {
            playerShooter.gun.IncreaseDamage(5f);
        }
    }

    private void ApplyMagazineCapacity()
    {
        if (playerShooter != null && playerShooter.gun != null)
        {
            playerShooter.gun.IncreaseMagCapacity(5);
        }
    }

    private void ApplyItemBurst()
    {
        if (itemSpawner != null)
        {
            itemSpawner.SpawnMultiple(5);
        }
    }

    private void ApplyScore()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(1000);
        }
    }

    private void ApplyHeal()
    {
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth(50f);
        }
    }

    private void CreatePanelIfNeeded()
    {
        if (panelObject != null)
        {
            return;
        }

        UIManager uiManager = GetComponent<UIManager>();
        Font hudFont = uiManager != null && uiManager.scoreText != null
            ? uiManager.scoreText.font
            : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        panelObject = new GameObject("Level Up Upgrade UI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.7f);

        for (int i = 0; i < CardCount; i++)
        {
            CreateCard(i, hudFont);
        }

        CreateText("Level Up Upgrade", panelObject.transform, new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(1000f, 100f), 72, TextAnchor.MiddleCenter, hudFont);

        panelObject.SetActive(false);
    }

    private void CreateCard(int index, Font font)
    {
        GameObject cardObject = new GameObject("Upgrade Card " + (index + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        cardObject.transform.SetParent(panelObject.transform, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = new Vector2((index - 1) * 540f, 0f);
        cardRect.sizeDelta = new Vector2(480f, 570f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);

        Button button = cardObject.GetComponent<Button>();
        button.targetGraphic = cardImage;
        cardButtons[index] = button;

        cardTitleTexts[index] = CreateText("Title", cardObject.transform, new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(420f, 150f), 51, TextAnchor.MiddleCenter, font);
        cardDescriptionTexts[index] = CreateText("Description", cardObject.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -65f), new Vector2(405f, 255f), 39, TextAnchor.MiddleCenter, font);
    }

    private Text CreateText(string textName, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Font font)
    {
        GameObject textObject = new GameObject(textName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = anchor;
        textRect.anchorMax = anchor;
        textRect.anchoredPosition = position;
        textRect.sizeDelta = size;
        textRect.pivot = new Vector2(0.5f, 0.5f);

        Text text = textObject.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;

        return text;
    }

    private void OnDestroy()
    {
        if (playerExperience != null)
        {
            playerExperience.onLevelUp -= HandleLevelUp;
        }

        if (isLevelUpSequenceActive)
        {
            Time.timeScale = timeScaleBeforePause;
        }
    }

    private class UpgradeOption
    {
        public readonly string title;
        public readonly string description;
        private readonly System.Action apply;

        public UpgradeOption(string title, string description, System.Action apply)
        {
            this.title = title;
            this.description = description;
            this.apply = apply;
        }

        public void Apply()
        {
            apply?.Invoke();
        }
    }
}
