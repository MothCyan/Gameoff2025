using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 敌人血量管理 - 处理敌人的生命值和死亡
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("血量设置")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("伤害设置")]
    public int bouncedWaveDamage = 100; // 被反弹的波造成的伤害（一击必杀）
    public int normalWaveDamage = 30; // 普通波造成的伤害
    
    [Header("死亡设置")]
    public float destroyDelay = 0.5f; // 死亡后延迟销毁时间
    public GameObject deathEffect; // 死亡特效预制体
    
    [Header("无敌设置")]
    public bool isInvincible = false; // 是否无敌
    public float invincibilityDuration = 0.1f; // 受击后无敌时间
    
    [Header("事件")]
    public UnityEvent onDeath; // 死亡事件
    public UnityEvent onTakeDamage; // 受伤事件
    
    private bool isDead = false;
    private bool isInvincibleNow = false;

    void Start()
    {
        // 初始化血量
        currentHealth = maxHealth;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[EnemyHealth] {gameObject.name} 检测到碰撞: {other.gameObject.name}, Tag: '{other.tag}'");
        
        // 检查是否是波 - 先尝试通过组件判断
        bool isWave = other.CompareTag("Wave") || 
                      other.GetComponent<ProjectileTargetFollower>() != null ||
                      other.GetComponent<PiercingProjectile>() != null ||
                      other.GetComponent<EarthquakeProjectile>() != null;
        
        if (isWave)
        {
            Debug.Log($"[EnemyHealth] {gameObject.name} 确认是波，开始处理伤害");
            HandleWaveHit(other.gameObject);
        }
        else
        {
            Debug.Log($"[EnemyHealth] {gameObject.name} 不是波 (Tag: '{other.tag}')");
        }
    }

    /// <summary>
    /// 处理波的攻击
    /// </summary>
    void HandleWaveHit(GameObject wave)
    {
        if (isDead || isInvincible || isInvincibleNow) return;

        // 检查波是否被反弹过
        bool isBounced = CheckIfWaveBounced(wave);
        
        int damage = isBounced ? bouncedWaveDamage : normalWaveDamage;
        
        Debug.Log($"{gameObject.name} 被 {(isBounced ? "反弹" : "普通")} 波击中，造成 {damage} 点伤害");
        
        TakeDamage(damage);
        
        // 销毁波
        Destroy(wave);
        
        // 如果设置了无敌时间，启动无敌
        if (invincibilityDuration > 0 && !isBounced) // 反弹波不触发无敌
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    /// <summary>
    /// 检查波是否被反弹过
    /// </summary>
    bool CheckIfWaveBounced(GameObject wave)
    {
        // 检查 ProjectileTargetFollower 组件
        ProjectileTargetFollower follower = wave.GetComponent<ProjectileTargetFollower>();
        if (follower != null)
        {
            // 使用反射获取 currentBounceCount（因为是私有变量）
            var field = typeof(ProjectileTargetFollower).GetField("currentBounceCount", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                int bounceCount = (int)field.GetValue(follower);
                return bounceCount > 0; // 如果反弹次数大于0，说明被反弹过
            }
        }
        
        // 检查 PiercingProjectile 组件
        PiercingProjectile piercing = wave.GetComponent<PiercingProjectile>();
        if (piercing != null)
        {
            var field = typeof(PiercingProjectile).GetField("currentBounceCount", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                int bounceCount = (int)field.GetValue(piercing);
                return bounceCount > 0;
            }
        }
        
        return false;
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"{gameObject.name} 受到 {damage} 点伤害，剩余血量: {currentHealth}/{maxHealth}");

        // 触发受伤事件
        onTakeDamage?.Invoke();

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡
    /// </summary>
    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} 死亡！");

        // 触发死亡事件
        onDeath?.Invoke();

        // 播放死亡特效
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 禁用敌人AI（如果有）
        TripleProjectileEmitter emitter = GetComponent<TripleProjectileEmitter>();
        if (emitter != null)
        {
            emitter.enabled = false;
        }

        // 延迟销毁
        Destroy(gameObject, destroyDelay);
    }

    /// <summary>
    /// 无敌协程
    /// </summary>
    IEnumerator InvincibilityCoroutine()
    {
        isInvincibleNow = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincibleNow = false;
    }

    /// <summary>
    /// 恢复血量
    /// </summary>
    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        Debug.Log($"{gameObject.name} 恢复 {healAmount} 血量，当前血量: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 设置无敌状态
    /// </summary>
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    /// <summary>
    /// 获取当前血量
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 获取最大血量
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// 是否已死亡
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
}
