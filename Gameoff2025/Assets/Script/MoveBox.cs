using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可推动的盒子 - 只能被Wave推动，不能被玩家或盾牌推动
/// </summary>
public class MoveBox : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float movementDistance = 5f;
    
    [Header("碰撞设置")]
    [SerializeField] private string validPusherTag = "Wave"; // 只接受这个Tag的推动
    
    private Rigidbody2D rb;
    private Collider2D boxCollider;
    private bool isMoving = false;

    void Start()
    {
        // 初始化Box物理设置 - 确保运动学刚体
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 配置Rigidbody2D
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.isKinematic = true; // 使用运动学刚体，防止物理推动
        
        boxCollider = GetComponent<Collider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        boxCollider.isTrigger = true; // 作为触发器
        
        // 尝试使用PhysicsHelper（如果存在）
        try
        {
            // PhysicsHelper.SetupBox(gameObject);
        }
        catch { }
        
        Debug.Log("[MoveBox] Box初始化完成，只能被Tag为 '" + validPusherTag + "' 的对象推动");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 严格检查：必须是Wave标签，且不在移动中
        if (other.CompareTag(validPusherTag) && !isMoving)
        {
            // 再次确认其他物体有Rigidbody2D（Wave应该有）
            Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
            if (otherRb != null && otherRb.velocity.magnitude > 0.1f)
            {
                StartCoroutine(MoveInCollisionDirectionSmoothly(otherRb));
            }
        }
        else if (!other.CompareTag(validPusherTag))
        {
            // 非法推动 - 记录日志
            Debug.Log($"[MoveBox] 被非法对象碰撞: {other.gameObject.name} (Tag: {other.tag})，已忽略");
        }
    }
    
    /// <summary>
    /// 根据碰撞物体的移动方向缓慢移动一段距离
    /// </summary>
    /// <param name="waveRb">碰撞的Wave物体的Rigidbody2D</param>
    private IEnumerator MoveInCollisionDirectionSmoothly(Rigidbody2D waveRb)
    {
        isMoving = true;
        
        // 获取碰撞物体的速度方向
        Vector2 direction = waveRb.velocity.normalized;
        
        // 计算目标位置
        Vector2 targetPosition = (Vector2)transform.position + direction * movementDistance;
        
        // 平滑移动到目标位置
        Vector2 startPosition = rb.position;
        float journey = 0f;
        
        while (journey < 1f)
        {
            journey += Time.deltaTime * moveSpeed;
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, journey);
            rb.MovePosition(newPosition);
            yield return null;
        }
        
        // 确保最终位置准确
        rb.MovePosition(targetPosition);
        
        Debug.Log($"[MoveBox] Box被Wave推动了 {movementDistance} 个单位");
        
        isMoving = false;
    }
}
