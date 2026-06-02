using UnityEngine;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 움직임의 속도
    public Transform cameraTransform; // 카메라의 트랜스폼

    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터

    private void Start()
    {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }

    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate()
    {
        // 물리 갱신 주기마다 움직임, 회전, 애니메이션 처리 실행

        // 움직임 실행
        Move();

        // 입력값에 따라 애니메이터의 Move 파라미터 값 변경
        playerAnimator.SetFloat("Move", playerInput.move);
    }

    // 입력값에 따라 캐릭터를 움직임
    private void Move()
    {
        // w와 s 키 입력에 따라 캐릭터를 앞뒤로 움직임
        float verticalInput = Input.GetKey("w") ? 1f : (Input.GetKey("s") ? -1f : 0);
        // a와 d 키 입력에 따라 캐릭터를 좌우로 움직임
        float horizontalInput = Input.GetKey("d") ? 1f : (Input.GetKey("a") ? -1f : 0);

        // 카메라의 앞, 뒤, 왼쪽, 오른쪽 방향을 기준으로 이동 방향 벡터 계산
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // y 축 방향 성분 제거
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // 이동 방향 벡터 계산
        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput).normalized;
        // 상대적으로 이동할 거리 계산
        Vector3 moveDistance = moveDirection * moveSpeed * Time.deltaTime;
        // 리지드바디를 이용해 게임 오브젝트 위치 변경
        playerRigidbody.MovePosition(playerRigidbody.position + moveDistance);
    }
}
