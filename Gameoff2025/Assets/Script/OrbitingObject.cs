using UnityEngine;

public class OrbitingObject : MonoBehaviour
{
    public float orbitRadius = 5;
    public bool canDestroyWaves = false;
    public Transform player; // 玩家对象引用
    public float rotationSpeed = 360f; // 旋转速度（度/滚轮单位）
    private Rigidbody2D rb;
    private float currentAngle = 0f; // 当前旋转角度

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.isKinematic = true;
        rb.gravityScale = 0;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = true;

        // 如果没有设置玩家引用，自动查找
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        // 如果有玩家引用，更新盾牌位置和旋转
        if (player != null)
        {
            UpdateShieldPosition();
        }

        // 按下左键切换销毁 Wave 的能力
        if (Input.GetMouseButtonDown(0))
        {
            ToggleDestroyWaves();
            Debug.Log($"盾牌销毁 Wave 能力已切换: {canDestroyWaves}");
        }
    }

    void UpdateShieldPosition()
    {
        // 使用鼠标滚轮控制旋转角度
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentAngle += scroll * rotationSpeed; // 使用可配置的旋转速度

        // 根据当前角度计算盾牌的轨道位置
        Vector2 direction = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
        Vector3 shieldPosition = player.position + (Vector3)direction * orbitRadius;

        // 更新盾牌位置和旋转
        UpdateOrbit(shieldPosition, currentAngle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wave") && canDestroyWaves)
        {
            Destroy(other.gameObject);
        }
    }

    public void UpdateOrbit(Vector3 position, float angle)
    {
        if (rb != null)
        {
            rb.MovePosition((Vector2)position);
        }
        else
        {
            transform.position = position;
        }
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void ToggleDestroyWaves()
    {
        canDestroyWaves = !canDestroyWaves;
    }

    public bool CanDestroyWaves()
    {
        return canDestroyWaves;
    }

    public void SetCanDestroyWaves(bool state)
    {
        canDestroyWaves = state;
    }
}