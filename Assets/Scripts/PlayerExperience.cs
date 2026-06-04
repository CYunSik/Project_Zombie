using System;
using UnityEngine;

// 플레이어의 레벨과 경험치 진행도를 관리
public class PlayerExperience : MonoBehaviour
{
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int firstLevelExperience = 100;
    [SerializeField] private int experienceIncreasePerLevel = 20;

    public int level { get; private set; }
    public int currentExperience { get; private set; }
    public int experienceToNextLevel { get; private set; }

    public event Action<int, int, int> onExperienceChanged;

    private void Awake()
    {
        ResetExperience();
    }

    // 경험치를 추가하고, 필요한 경험치를 넘으면 여러 번이라도 레벨업 처리
    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentExperience += amount;

        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            level++;
            experienceToNextLevel += experienceIncreasePerLevel;
        }

        NotifyExperienceChanged();
    }

    private void ResetExperience()
    {
        level = Mathf.Max(1, startingLevel);
        currentExperience = 0;
        experienceToNextLevel = Mathf.Max(1, firstLevelExperience);

        NotifyExperienceChanged();
    }

    private void NotifyExperienceChanged()
    {
        onExperienceChanged?.Invoke(level, currentExperience, experienceToNextLevel);
    }
}
