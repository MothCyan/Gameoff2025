using UnityEngine;

/// <summary>
/// 敌人设置检查器 - 帮助诊断敌人配置问题
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemySetupChecker : MonoBehaviour
{
    void Start()
    {
        CheckSetup();
    }

    void CheckSetup()
    {
        Debug.Log($"===== 检查敌人 {gameObject.name} 的设置 =====");
        
        // 检查Collider2D
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Debug.Log($"✓ Collider2D 存在: {collider.GetType().Name}");
            Debug.Log($"  - Is Trigger: {collider.isTrigger}");
            if (!collider.isTrigger)
            {
                Debug.LogWarning($"⚠ Collider2D 的 isTrigger 应该设置为 true！");
            }
        }
        else
        {
            Debug.LogError($"✗ 缺少 Collider2D 组件！");
        }
        
        // 检查Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.Log($"✓ Rigidbody2D 存在");
            Debug.Log($"  - Body Type: {rb.bodyType}");
            Debug.Log($"  - Gravity Scale: {rb.gravityScale}");
        }
        else
        {
            Debug.LogWarning($"⚠ 建议添加 Rigidbody2D 组件（设置为 Kinematic）");
        }
        
        // 检查EnemyHealth
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            Debug.Log($"✓ EnemyHealth 存在");
            Debug.Log($"  - Max Health: {health.maxHealth}");
            Debug.Log($"  - Bounced Wave Damage: {health.bouncedWaveDamage}");
            Debug.Log($"  - Normal Wave Damage: {health.normalWaveDamage}");
        }
        else
        {
            Debug.LogWarning($"⚠ 缺少 EnemyHealth 组件！");
        }
        
        // 检查Tag
        Debug.Log($"  - GameObject Tag: {gameObject.tag}");
        
        Debug.Log($"===== 检查完成 =====");
    }
}
