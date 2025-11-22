using UnityEngine;

public class OrbitingObject : MonoBehaviour
{
    public float orbitRadius = 5;
    public bool canDestroyWaves = false;
    public Transform player; // 玩家对象引用
    public float rotationSpeed = 360f; // 旋转速度（度/滚轮单位）
    
    [Header("盾牌模式")]
    public bool isDestroyMode = true; // true=消除模式，false=反弹模式
    public float bounceForce = 10f; // 反弹力度
    
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

        // 盾牌初始化为失活状态
        canDestroyWaves = false;

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
        // 盾牌位置由滚轮控制
        UpdateShieldPosition();
    }

    public void UpdateShieldPosition()
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
            if (isDestroyMode)
            {
                // 消除模式：直接销毁Wave
                Debug.Log("盾牌消除了Wave");
                Destroy(other.gameObject);
            }
            else
            {
                // 反弹模式：给Wave一个反向力
                Debug.Log("盾牌反弹了Wave");
                Rigidbody2D waveRb = other.GetComponent<Rigidbody2D>();
                if (waveRb != null)
                {
                    // 计算反弹方向
                    Vector2 bounceDirection = (other.transform.position - transform.position).normalized;
                    waveRb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
                }
            }
        }
        else
        {
            // 调试信息：盾牌未激活或不是Wave
            if (other.CompareTag("Wave") && !canDestroyWaves)
            {
                Debug.Log("盾牌未激活，Wave穿过了盾牌");
            }
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
        // 旋转180度以匹配精灵图方向
        transform.rotation = Quaternion.AngleAxis(angle + 180f, Vector3.forward);
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
        Debug.Log($"盾牌能力设置: canDestroyWaves = {canDestroyWaves}, isDestroyMode = {isDestroyMode}");
    }

    public void SetShieldMode(bool destroyMode)
    {
        isDestroyMode = destroyMode;
        Debug.Log($"盾牌模式设置为: {(isDestroyMode ? "消除模式" : "反弹模式")}");
    }
}