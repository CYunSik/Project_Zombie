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
        LookAtMouse();
    }

    void LookAtMouse()
    {
        // 마우스 위치를 가져옴
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 마우스 커서가 지면과 교차하는 위치를 찾음
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 targetPosition = hit.point;

            // 캐릭터의 높이(Y축)를 유지하도록 위치 조정
            targetPosition.y = transform.position.y;

            // 캐릭터가 타겟 위치를 바라보도록 회전
            transform.LookAt(targetPosition);
        }
    }
}
