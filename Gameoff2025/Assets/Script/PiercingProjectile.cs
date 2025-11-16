using System.Collections;
using UnityEngine;

/// <summary>
/// 穿透波 - 可以穿透镜子（盾牌），其他逻辑与 ProjectileTargetFollower 相同
/// </summary>
public class PiercingProjectile : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target;
    public float speed = 10f; // 初速度较快
    public float lifeTime = 8f;
    
    [Header("碰撞反弹设置")]
    public bool enableBounce = true; // 是否启用反弹
    public float bounceFactor = 0.8f; // 反弹系数 (0-1)
    public int maxBounceCount = 5; // 最大反弹次数
    
    [Header("穿透设置")]
    public float pierceSpeedThreshold = 5f; // 穿透速度阈值

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
    /// 主要区别：可以穿透镜子（如果速度足够高）
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        return;
        if (other.tag == "Wave")
            return;

        if (other.gameObject.tag == "Dun")
        {
           
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
