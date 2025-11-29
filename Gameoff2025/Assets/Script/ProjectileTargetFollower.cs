using System.Collections;
using UnityEngine;

public class ProjectileTargetFollower : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target;
    public float speed = 5f;
    public float lifeTime = 5f; // 预制体存活时间
    [Header("碰撞反弹设置")]
    public bool enableBounce = true; // 是否启用反弹
    public float bounceFactor = 0.8f; // 反弹系数 (0-1)
    public int maxBounceCount = 3; // 最大反弹次数

    private Rigidbody2D rb;
    private Vector3 direction;
    private int currentBounceCount = 0; // 当前反弹次数

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 开始生命周期计时
        StartCoroutine(DestroyAfterTime());

        // 计算目标方向（只计算一次）
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
        else
        {
            direction = Vector3.right;
        }

        // 添加子弹到管理器
        AddZidan(gameObject);
    }

    void Update()
    {
        // 使用固定的方向移动，不持续追踪
            
        // 更新朝向 - 在原有的基础上旋转Z轴180度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
            
        // 移动
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        else
        {
            // 如果没有Rigidbody，使用Transform移动
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 设置目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 设置速度
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    /// <summary>
    /// 触发器检测（用于isTrigger碰撞器）
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Wave")
            return;
            
        // 检测敌人碰撞
        if (other.CompareTag("enemy") && other.isTrigger&&currentBounceCount > 0)
        {
            Debug.Log($"波击中敌人: {other.gameObject.name}");
            // 删除敌人
            Destroy(other.gameObject);
            // 删除波自己
            Destroy(gameObject);
            return;
        }
            
        //Debug.Log("触发碰撞: " + other.gameObject.name);
        if (enableBounce && currentBounceCount < maxBounceCount && other.gameObject.tag == "Dun")
        {
            // 计算碰撞点与物体中心的向量作为近似法线
            Vector2 collisionPoint = other.ClosestPoint(transform.position);
            Vector2 normal = (transform.position - (Vector3)collisionPoint).normalized;
            
            // 计算反射方向
            direction = Vector3.Reflect(direction, normal).normalized;
            
            // 应用反弹系数减少速度
            speed *= bounceFactor;
            
            // 增加反弹计数
            currentBounceCount++;
            
            Debug.Log($"反弹! 剩余反弹次数: {maxBounceCount - currentBounceCount}, 当前速度: {speed}");
        }
        else
        {
            // 如果达到最大反弹次数或未启用反弹，销毁物体
            Debug.Log("达到最大反弹次数或未启用反弹，销毁物体");
            
        }
    }

    /// <summary>
    /// 在指定时间后销毁
    /// </summary>
    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 添加子弹到管理器
    /// </summary>
    private void AddZidan(GameObject zidanObj)
    {
        zidanManager zidanMgr = FindObjectOfType<zidanManager>();
        if (zidanMgr != null)
        {
            zidanMgr.AddZidan(zidanObj);
        }
    }
}