using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5;
    public float boostedSpeed = 15f; // 加速后的速度
    public float boostDuration = 0.3f; // 加速持续时间
    public float rotationSpeed = 360f; // 旋转速度（度/秒）
    
    [Header("盾牌设置")]
    public float shieldRadius1 = 2;
    public float shieldRadius2 = 3;
    public OrbitingObject shieldPrimary;
    public OrbitingObject shieldSecondary;
    
    [Header("盾牌模式")]
    public bool isDestroyMode = true; // true=消除模式，false=反弹模式
    
    [Header("血条能量条设置")]
    public int maxHealth = 100;
    public int maxEnergy = 100;
    public float energyRegenRate = 10f; // 每秒恢复的能量
    public float energyConsumptionRate = 20f; // 激活盾牌每秒消耗的能量
    public Image healthBar;
    public Image energyBar;
    
    [Header("盾牌能量消耗")]
    public float shieldEnergyCost = 0.1f; // 盾牌每帧消耗的能量

    private Rigidbody2D rb;
    private Camera mainCamera;
    private float currentSpeed;
    private bool isBoosting = false;
    private int currentHealth;
    private float currentEnergy;
    private bool isShieldActive = false;
    private Vector2 lastInputDirection; // 记录最后的输入方向

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        currentSpeed = moveSpeed;
        
        // 设置Rigidbody2D属性以减少外力影响
        if (rb != null)
        {
            rb.drag = 10f; // 增加阻力，快速停止外力影响
            rb.angularDrag = 10f; // 增加角阻力，防止旋转
            rb.gravityScale = 0f; // 确保重力为0
        }
        
        // 初始化血条和能量条
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        
        // 设置角色初始朝向为向下（精灵图初始方向）
        transform.rotation = Quaternion.AngleAxis(-90f, Vector3.forward);
        
        // 初始化盾牌为失活状态
        DeactivateShields();
        
        UpdateHealthBar();
        UpdateEnergyBar();
    }

    void Update()
    {
        HandleMovement();
        HandleBoost();
        HandleShieldInput();
        HandleEnergyRegen();
        UpdateShields();
        UpdateUI();
    }

    void HandleShieldInput()
    {
        // 按下左键切换盾牌状态（激活/失活）
        if (Input.GetMouseButtonDown(0))
        {
            if (!isShieldActive && currentEnergy > 0)
            {
                // 激活盾牌
                isShieldActive = true;
                ActivateShields();
                Debug.Log("盾牌激活");
            }
            else if (isShieldActive)
            {
                // 失活盾牌
                isShieldActive = false;
                DeactivateShields();
                Debug.Log("盾牌失活");
            }
        }
        
        // 盾牌激活时持续消耗能量
        if (isShieldActive)
        {
            currentEnergy -= shieldEnergyCost;
            if (currentEnergy <= 0)
            {
                currentEnergy = 0;
                isShieldActive = false;
                DeactivateShields();
                Debug.Log("能量耗尽，盾牌失活");
            }
        }
        
        // 右键切换是否消除波的能力
        if (Input.GetMouseButtonDown(1))
        {
            if (isShieldActive)
            {
                // 如果盾牌激活，切换消除波的能力
                bool currentCanDestroy = shieldPrimary != null ? shieldPrimary.CanDestroyWaves() : false;
                bool newCanDestroy = !currentCanDestroy;
                
                if (shieldPrimary != null)
                    shieldPrimary.SetCanDestroyWaves(newCanDestroy);
                if (shieldSecondary != null)
                    shieldSecondary.SetCanDestroyWaves(newCanDestroy);
                    
                Debug.Log($"盾牌消除波能力切换为: {(newCanDestroy ? "开启" : "关闭")}");
            }
            else
            {
                Debug.Log("盾牌未激活，无法切换消除波能力");
            }
        }
    }

    void HandleEnergyRegen()
    {
        // 能量自动恢复（不激活盾牌时）
        if (!isShieldActive && currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        }
    }

    void ActivateShields()
    {
        // 激活盾牌GameObject（不自动开启消除波能力）
        if (shieldPrimary != null)
        {
            shieldPrimary.gameObject.SetActive(true);
            shieldPrimary.SetCanDestroyWaves(false);
        }
        if (shieldSecondary != null)
        {
            shieldSecondary.gameObject.SetActive(true);
            shieldSecondary.SetCanDestroyWaves(false);
        }
        UpdateShieldMode();
    }

    void DeactivateShields()
    {
        // 失活盾牌GameObject
        if (shieldPrimary != null)
        {
            shieldPrimary.gameObject.SetActive(false);
            shieldPrimary.SetCanDestroyWaves(false);
        }
        if (shieldSecondary != null)
        {
            shieldSecondary.gameObject.SetActive(false);
            shieldSecondary.SetCanDestroyWaves(false);
        }
    }

    void UpdateShieldMode()
    {
        if (shieldPrimary != null)
            shieldPrimary.SetShieldMode(isDestroyMode);
        if (shieldSecondary != null)
            shieldSecondary.SetShieldMode(isDestroyMode);
    }

    void UpdateUI()
    {
        UpdateHealthBar();
        UpdateEnergyBar();
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    void UpdateEnergyBar()
    {
        if (energyBar != null)
        {
            energyBar.fillAmount = currentEnergy / maxEnergy;
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 inputDirection = new Vector2(moveX, moveY).normalized;
        
        // 应用移动速度或强制停止移动
        if (inputDirection.magnitude > 0.1f)
        {
            rb.velocity = inputDirection * currentSpeed;
            lastInputDirection = inputDirection; // 记录玩家的输入方向
        }
        else
        {
            // 没有输入时强制停止移动，抵消所有外力
            rb.velocity = Vector2.zero;
        }
    }
    
    void LateUpdate()
    {
        // 在LateUpdate中再次检查并修正外力影响
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 inputDirection = new Vector2(moveX, moveY).normalized;
        
        // 如果没有输入但角色仍在移动，强制停止
        if (inputDirection.magnitude <= 0.1f && rb.velocity.magnitude > 0.1f)
        {
            rb.velocity = Vector2.zero;
        }
        
        // 角色朝向处理
        if (lastInputDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(lastInputDirection.y, lastInputDirection.x) * Mathf.Rad2Deg + 90f;
            Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
            
            // 使用平滑旋转
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleBoost()
    {
        // 按下空格键触发加速
        if (Input.GetKeyDown(KeyCode.Space) && !isBoosting)
        {
            StartCoroutine(BoostCoroutine());
        }
    }

    /// <summary>
    /// 加速协程
    /// </summary>
    private IEnumerator BoostCoroutine()
    {
        isBoosting = true;
        currentSpeed = boostedSpeed; // 速度加快
        Debug.Log($"加速启动！速度: {currentSpeed}");
        
        yield return new WaitForSeconds(boostDuration);
        
        currentSpeed = moveSpeed; // 速度恢复
        isBoosting = false;
        Debug.Log($"加速结束！速度: {currentSpeed}");
    }

    void UpdateShields()
    {
        // 盾牌位置现在由OrbitingObject脚本自行处理滚轮控制
        // 这里只需要确保盾牌状态同步即可
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wave"))
        {
            TakeDamage(10); // 受到伤害
            zidanManager.Instance.Pu();
        }
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        
        Debug.Log($"受到伤害: {damage}，当前血量: {currentHealth}/{maxHealth}");
        
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
        Debug.Log("玩家死亡!");
        // 可以在这里添加死亡动画、重启游戏等逻辑
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 恢复血量
    /// </summary>
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log($"恢复血量: {healAmount}，当前血量: {currentHealth}/{maxHealth}");
    }
    
    /// <summary>
    /// 恢复能量
    /// </summary>
    public void RestoreEnergy(float energyAmount)
    {
        currentEnergy += energyAmount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        Debug.Log($"恢复能量: {energyAmount}，当前能量: {currentEnergy}/{maxEnergy}");
    }
}