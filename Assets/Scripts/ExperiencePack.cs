using UnityEngine;

// 플레이어 경험치를 증가시키는 아이템
public class ExperiencePack : MonoBehaviour, IItem {
    public int experience = 50; // 증가할 경험치

    public void Use(GameObject target) {
        // 전달받은 플레이어 오브젝트에서 경험치 컴포넌트 가져오기 시도
        PlayerExperience playerExperience = target.GetComponent<PlayerExperience>();

        if (playerExperience == null)
        {
            playerExperience = target.AddComponent<PlayerExperience>();
        }

        if (playerExperience != null)
        {
            // 설정된 수치만큼 경험치 추가
            playerExperience.AddExperience(experience);
        }

        // 사용되었으므로, 자신을 파괴
        Destroy(gameObject);
    }
}
