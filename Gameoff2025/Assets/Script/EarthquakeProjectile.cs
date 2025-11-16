using System.Collections;
using UnityEngine;

/// <summary>
/// 地震波 - 无法反弹，只能翻滚过去，但会打到队友（敌人的队友就是敌人）
/// </summary>
public class EarthquakeProjectile : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target;
    public float speed = 3f; // 速度较慢
    public float lifeTime = 10f;
    
    [Header("翻滚设置")]
    public float rollDuration = 0.5f; // 翻滚过镜子的持续时间
    public AnimationCurve rollEase = AnimationCurve.EaseInOut(0, 0, 1, 1); // 翻滚缓动曲线
    
    [Header("碰撞设置")]
    public bool canHitTeammate = true; // 是否能打到队友

    private Rigidbody2D rb;
    private Vector3 direction;
    private float currentSpeed;
    private bool isRolling = false; // 是否正在翻滚
    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = speed;
        originalScale = transform.localScale;
        
        StartCoroutine(DestroyAfterTime());

        // 计算目标方向
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
        else
        {
            direction = Vector3.right;
        }

        AddZidan(gameObject);
    }

    void Update()
    {
        // 更新朝向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // 移动
        if (rb != null)
        {
            rb.velocity = direction * currentSpeed;
        }
        else
        {
            transform.position += direction * currentSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Wave")
            return;

        // 如果碰撞到镜子（盾牌），进行翻滚
        if (other.gameObject.tag == "Dun" && !isRolling)
        {
            Debug.Log("地震波：翻滚过镜子！");
            StartCoroutine(RollThroughShield());
        }
        // 如果可以打到队友
        else if (canHitTeammate && other.gameObject.tag == "TeamMate")
        {
            Debug.Log("地震波：打到队友！");
            // 可在这里添加伤害逻辑
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 翻滚过镜子的动画协程
    /// </summary>
    private IEnumerator RollThroughShield()
    {
        isRolling = true;
        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / rollDuration;
            
            // 使用缓动曲线控制翻滚
            float easeValue = rollEase.Evaluate(progress);
            
            // 翻滚旋转效果（沿着运动方向旋转）
            float rotationAmount = easeValue * 360f;
            transform.Rotate(0, 0, rotationAmount);

            // 可选：翻滚时改变缩放（压扁效果）
            float scaleY = Mathf.Lerp(1f, 0.7f, Mathf.Sin(progress * Mathf.PI));
            transform.localScale = new Vector3(originalScale.x, originalScale.y * scaleY, originalScale.z);

            yield return null;
        }

        // 恢复原始缩放
        transform.localScale = originalScale;
        isRolling = false;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        currentSpeed = newSpeed;
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void AddZidan(GameObject zidanObj)
    {
        zidanManager zidanMgr = FindObjectOfType<zidanManager>();
        if (zidanMgr != null)
        {
            zidanMgr.AddZidan(zidanObj);
        }
    }
}
