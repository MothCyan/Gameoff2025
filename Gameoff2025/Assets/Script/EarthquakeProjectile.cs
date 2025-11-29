using System.Collections;
using UnityEngine;

/// <summary>
/// 地震波 - 无法被盾牌反弹或消除，但会对玩家造成伤害
/// </summary>
public class EarthquakeProjectile : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target;
    public float speed = 3f; // 速度较慢
    public float lifeTime = 10f;
    
    [Header("碰撞设置")]
    public bool canHitTeammate = true; // 是否能打到队友
    public int damageToPlayer = 15; // 对玩家造成的伤害

    private Rigidbody2D rb;
    private Vector3 direction;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = speed;
        
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
            
        // 检测敌人碰撞 - 地震波会删除敌人但不删除自己（可以穿透）
        if (other.CompareTag("Enemy") && other.isTrigger)
        {
            Debug.Log($"地震波击中敌人: {other.gameObject.name}");
            // 删除敌人
            Destroy(other.gameObject);
            // 地震波不删除自己，继续前进
            return;
        }

        // 地震波无法被盾牌反弹和消除，直接穿透
        if (other.gameObject.tag == "Dun")
        {
            Debug.Log("地震波：穿透盾牌！无法被反弹或消除");
            // 不进行任何碰撞处理，继续移动
            return;
        }
        // 碰到玩家时造成伤害
        else if (other.CompareTag("Player"))
        {
            Debug.Log($"地震波：击中玩家！造成{damageToPlayer}点伤害");
            
            // 获取玩家脚本并造成伤害
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damageToPlayer);
            }
            
            // 击中玩家后销毁地震波
            Destroy(gameObject);
        }
        // 如果可以打到队友
        else if (canHitTeammate && other.gameObject.tag == "TeamMate")
        {
            Debug.Log("地震波：打到队友！");
            // 可在这里添加伤害逻辑
            Destroy(gameObject);
        }
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
