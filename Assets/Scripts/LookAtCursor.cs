using UnityEngine;

public class LookAtCursor : MonoBehaviour
{
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // 레벨업 카드 선택 등으로 게임이 일시정지된 동안에는 마우스 방향을 따라 회전하지 않음
        if (Time.timeScale <= 0f)
        {
            return;
        }

        LookAtMouse();
    }

    void LookAtMouse()
    {
        // 마우스 위치를 기준으로 카메라에서 Ray를 생성
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 마우스 커서가 가리키는 월드 위치를 찾음
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 targetPosition = hit.point;

            // 캐릭터가 위아래로 기울지 않도록 높이(Y축)를 고정
            targetPosition.y = transform.position.y;

            // 캐릭터가 타겟 위치를 바라보도록 회전
            transform.LookAt(targetPosition);
        }
    }
}
