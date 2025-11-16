using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 被破坏的盒子 - 被Wave碰撞时销毁，无法被玩家或盾牌推动
/// </summary>
public class DestoryBox : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D boxCollider;

    void Start()
    {
        // 初始化Box物理设置 - 确保不能被推动
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<Collider2D>();
        
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.isKinematic = true;  // 运动学刚体，不受物理推动
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;  // 作为触发器，防止物理碰撞
        }
        
        Debug.Log("[DestoryBox] 破坏盒子初始化完成");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 只响应Wave的碰撞
        if (other.CompareTag("Wave"))
        {
            Debug.Log("[DestoryBox] 盒子被Wave销毁");
            Destroy(gameObject);
        }
        else
        {
            // 阻止其他对象推动
            Debug.Log($"[DestoryBox] 被非Wave对象碰撞: {other.gameObject.name}，已忽略");
        }
    }
}

