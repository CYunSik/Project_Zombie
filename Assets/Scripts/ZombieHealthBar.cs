using UnityEngine;
using UnityEngine.UI;

// 좀비 머리 위에 표시되는 월드 공간 체력바
public class ZombieHealthBar : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private Vector2 size = new Vector2(1.2f, 0.12f);

    private LivingEntity target;
    private RectTransform barTransform;
    private RectTransform fillRect;
    private Camera targetCamera;

    private void Awake()
    {
        target = GetComponent<LivingEntity>();
        targetCamera = Camera.main;

        CreateHealthBar();
    }

    private void OnEnable()
    {
        RefreshImmediate();
    }

    private void LateUpdate()
    {
        if (target == null || barTransform == null)
        {
            return;
        }

        // 체력바를 좀비의 자식으로 두지 않고 월드 위치만 따라가게 해서 좀비 회전 영향을 받지 않게 한다
        barTransform.position = transform.position + offset;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            // 월드 스페이스 Canvas의 앞면이 항상 카메라를 향하도록 회전
            Vector3 lookDirection = barTransform.position - targetCamera.transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                barTransform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        RefreshImmediate();
    }

    public void RefreshImmediate()
    {
        if (target == null || fillRect == null)
        {
            return;
        }

        float maxHealth = Mathf.Max(target.startingHealth, 1f);
        float healthPercent = Mathf.Clamp01(target.health / maxHealth);
        fillRect.anchorMax = new Vector2(healthPercent, 1f);

        if (barTransform != null)
        {
            barTransform.gameObject.SetActive(!target.dead && healthPercent > 0f);
        }
    }

    public void Hide()
    {
        if (barTransform != null)
        {
            barTransform.gameObject.SetActive(false);
        }
    }

    private void CreateHealthBar()
    {
        GameObject canvasObject = new GameObject("Health Bar", typeof(RectTransform));
        barTransform = canvasObject.GetComponent<RectTransform>();

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        barTransform.sizeDelta = size;
        barTransform.localScale = Vector3.one;

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform));
        backgroundObject.transform.SetParent(canvasObject.transform, false);

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = new Color(0.08f, 0f, 0f, 0.75f);

        RectTransform backgroundRect = backgroundImage.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform));
        fillObject.transform.SetParent(backgroundObject.transform, false);

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = Color.red;

        fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        if (barTransform != null)
        {
            Destroy(barTransform.gameObject);
        }
    }
}
