using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleProjectileEmitter : MonoBehaviour
{
    [Header("发射设置")]
    public GameObject projectilePrefab; // 要发射的预制体（兼容性保留）
    public Transform target; // 通用目标对象
    public float fireInterval = 0.5f; // 发射间隔
    public float projectileSpeed = 10f; // 发射速度
    public float scaleIncreaseRate = 0.1f; // 缩放增加速率
    public float fireRate = 1f; // 发射频率（每秒发射次数）
    
    [Header("多种波预制体")]
    public GameObject piercingWavePrefab; // 穿透波预制体
    public GameObject earthquakeWavePrefab; // 地震波预制体
    public bool randomWaveType = true; // 是否随机选择波类型

    [Header("发射位置")]
    public Transform firePoint; // 发射点，如果为空则使用自身位置

    [Header("不同波的目标设置")]
    public Transform piercingWaveTarget; // 穿透波的目标
    public Transform earthquakeWaveTarget; // 地震波的目标
    public Transform normalProjectileTarget; // 普通子弹的目标
    
    [Header("敌人AI设置")]
    public bool isEnemyAI = false; // 是否启用敌人AI
    public float moveSpeed = 5f; // AI移动速度
    public float moveRadius = 10f; // 移动半径
    public float moveChangeInterval = 3f; // 改变移动方向的间隔
    public float shootInterval = 2f; // 发射间隔

    private bool isFiring = false;
    
    // AI相关变量
    private Vector3 aiTargetPosition; // AI的目标位置
    private Vector3 startPosition; // 起始位置
    private float lastMoveChangeTime;
    private float lastShootTime;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 记录起始位置（用于AI随机移动的中心点）
        startPosition = transform.position;
        
        if (isEnemyAI)
        {
            // 敌人AI模式初始化
            SetRandomTargetPosition();
            lastMoveChangeTime = Time.time;
            lastShootTime = Time.time;
        }
        else
        {
            // 原有模式
            FireTripleProjectiles();
        }
    }

    void Update()
    {
        if (isEnemyAI)
        {
            // 敌人AI模式
            HandleEnemyAI();
        }
        else
        {
            // 原有模式 - 移除自动旋转，保持Z轴0度
            if(Input.GetKeyDown(KeyCode.Space)) FireTripleProjectiles();
            
            // 保持Z轴为0度，不进行旋转
            transform.rotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// 处理敌人AI逻辑
    /// </summary>
    void HandleEnemyAI()
    {
        // 保持Z轴为0度
        transform.rotation = Quaternion.identity;
        
        // 计算基于发射频率的发射间隔
        float calculatedShootInterval = fireRate > 0 ? 1f / fireRate : shootInterval;
        
        // 检查是否需要发射波
        if (Time.time - lastShootTime > calculatedShootInterval)
        {
            // 攻击时停止移动
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
            
            AIShootTripleWaves();
            lastShootTime = Time.time;
        }
        else
        {
            // 不在攻击时才移动
            // 检查是否需要改变移动方向
            if (Time.time - lastMoveChangeTime > moveChangeInterval)
            {
                SetRandomTargetPosition();
                lastMoveChangeTime = Time.time;
            }
            
            // 移动到目标位置
            Vector3 moveDirection = (aiTargetPosition - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, aiTargetPosition);
            
            if (distanceToTarget > 0.5f) // 如果距离目标还有一定距离
            {
                if (rb != null)
                {
                    rb.velocity = moveDirection * moveSpeed;
                }
                else
                {
                    transform.position += moveDirection * moveSpeed * Time.deltaTime;
                }
            }
            else
            {
                // 到达目标位置，设置新的随机目标
                SetRandomTargetPosition();
            }
        }
    }

    /// <summary>
    /// 设置随机目标位置
    /// </summary>
    void SetRandomTargetPosition()
    {
        // 在起始位置周围的圆形区域内随机选择一个点
        Vector2 randomOffset = Random.insideUnitCircle * moveRadius;
        aiTargetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
    }

    /// <summary>
    /// AI发射三道波
    /// </summary>
    void AIShootTripleWaves()
    {
        if (projectilePrefab != null)
        {
            // 寻找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                StartCoroutine(AITripleWaveSequence(player.transform));
            }
        }
    }

    /// <summary>
    /// AI三道波发射序列协程
    /// </summary>
    private IEnumerator AITripleWaveSequence(Transform playerTarget)
    {
        for (int i = 0; i < 3; i++)
        {
            AIShootSingleWave(playerTarget);
            yield return new WaitForSeconds(fireInterval);
        }
    }

    /// <summary>
    /// AI发射单道波
    /// </summary>
    void AIShootSingleWave(Transform playerTarget)
    {
        if (playerTarget == null) return;
        
        // 选择要发射的波类型
        GameObject selectedWavePrefab = GetSelectedWavePrefab();
        if (selectedWavePrefab == null) return;
        
        // 计算朝玩家方向的向量
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Vector3 shootDirection = (playerTarget.position - spawnPosition).normalized;
        
        // 计算波的角度（与ProjectileTargetFollower保持一致，精灵图初始向下，需要加90度偏移）
        float waveAngle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg + 90f;
        Quaternion waveRotation = Quaternion.Euler(0, 0, waveAngle);
        
        // 生成波并设置角度
        GameObject wave = Instantiate(selectedWavePrefab, spawnPosition, waveRotation);
        
        // 设置波的目标和属性
        SetWaveProperties(wave, playerTarget, shootDirection);
        
        Debug.Log($"敌人AI发射{GetWaveTypeName(selectedWavePrefab)}！方向: {shootDirection}, 角度: {waveAngle}度");
    }
    
    /// <summary>
    /// 获取选定的波预制体
    /// </summary>
    GameObject GetSelectedWavePrefab()
    {
        if (randomWaveType)
        {
            // 随机选择波类型
            var availableWaves = new System.Collections.Generic.List<GameObject>();
            if (piercingWavePrefab != null) availableWaves.Add(piercingWavePrefab);
            if (earthquakeWavePrefab != null) availableWaves.Add(earthquakeWavePrefab);
            if (projectilePrefab != null) availableWaves.Add(projectilePrefab);
            
            if (availableWaves.Count > 0)
            {
                return availableWaves[Random.Range(0, availableWaves.Count)];
            }
        }
        else
        {
            // 按顺序选择（穿透波 -> 地震波 -> 普通波）
            if (piercingWavePrefab != null) return piercingWavePrefab;
            if (earthquakeWavePrefab != null) return earthquakeWavePrefab;
            if (projectilePrefab != null) return projectilePrefab;
        }
        
        return null;
    }
    
    /// <summary>
    /// 设置波的属性
    /// </summary>
    void SetWaveProperties(GameObject wave, Transform playerTarget, Vector3 shootDirection)
    {
        // 检查波的类型并设置相应属性
        PiercingProjectile piercingComp = wave.GetComponent<PiercingProjectile>();
        if (piercingComp != null)
        {
            piercingComp.SetTarget(playerTarget);
            piercingComp.SetSpeed(projectileSpeed);
            return;
        }
        
        EarthquakeProjectile earthquakeComp = wave.GetComponent<EarthquakeProjectile>();
        if (earthquakeComp != null)
        {
            earthquakeComp.SetTarget(playerTarget);
            earthquakeComp.SetSpeed(projectileSpeed * 0.6f); // 地震波速度稍慢
            return;
        }
        
        // 普通波的处理
        ProjectileTargetFollower normalComp = wave.GetComponent<ProjectileTargetFollower>();
        if (normalComp != null)
        {
            normalComp.SetTarget(playerTarget);
        }
        
        // 设置物理速度
        Rigidbody2D waveRb = wave.GetComponent<Rigidbody2D>();
        if (waveRb != null)
        {
            waveRb.velocity = shootDirection * projectileSpeed;
        }
    }
    
    /// <summary>
    /// 获取波类型名称（用于调试）
    /// </summary>
    string GetWaveTypeName(GameObject wavePrefab)
    {
        if (wavePrefab == piercingWavePrefab) return "穿透波";
        if (wavePrefab == earthquakeWavePrefab) return "地震波";
        return "普通波";
    }

    /// <summary>
    /// 开始发射三次预制体
    /// </summary>
    public void FireTripleProjectiles()
    {
        if (isFiring) return;

        // 检查是否有可用的波预制体
        if (piercingWavePrefab == null && earthquakeWavePrefab == null && projectilePrefab == null)
        {
            Debug.LogWarning("没有设置任何波预制体！");
            return;
        }

        StartCoroutine(FireSequence());
    }

    /// <summary>
    /// 发射序列协程
    /// </summary>
    private IEnumerator FireSequence()
    {
        isFiring = true;

        // 计算统一的发射方向
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Vector3 unifiedFireDirection = target != null ?
            (target.position - spawnPosition).normalized :
            transform.forward;

        for (int i = 0; i < 3; i++)
        {
            FireSingleProjectile(unifiedFireDirection);
            yield return new WaitForSeconds(fireInterval);
        }

        isFiring = false;
    }

    /// <summary>
    /// 发射单个预制体
    /// </summary>
    private void FireSingleProjectile(Vector3 fireDirection)
    {
        // 选择要发射的波类型
        GameObject selectedWavePrefab = GetSelectedWavePrefab();
        if (selectedWavePrefab == null) return;
        
        // 确定发射位置
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        
        // 实例化预制体
        GameObject projectile = Instantiate(selectedWavePrefab, spawnPosition, Quaternion.identity);
        
        // 添加子弹到管理器
        zidanManager.Instance.AddZidan(projectile);
        
        // 根据预制体脚本类型设置目标
        SetProjectileTarget(projectile, spawnPosition);

        // 添加速度
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = fireDirection * projectileSpeed;
        }
        else
        {
            // 如果没有Rigidbody，使用Transform移动
            projectile.transform.position += fireDirection * projectileSpeed * Time.deltaTime;
        }

        // 开始缩放协程
        StartCoroutine(ScaleProjectileOverTime(projectile.transform));
        
        Debug.Log($"发射了{GetWaveTypeName(selectedWavePrefab)}");
    }

    /// <summary>
    /// 根据预制体脚本类型设置对应的目标
    /// </summary>
    private void SetProjectileTarget(GameObject projectile, Vector3 spawnPosition)
    {
        Transform targetToUse = target; // 默认使用通用目标
        
        // 检查预制体上挂载的脚本类型
        PiercingProjectile piercingProjectile = projectile.GetComponent<PiercingProjectile>();
        if (piercingProjectile != null)
        {
            // 优先使用特定目标，否则使用通用目标
            Transform specificTarget = piercingWaveTarget != null ? piercingWaveTarget : targetToUse;
            if (specificTarget != null)
            {
                piercingProjectile.SetTarget(specificTarget);
                piercingProjectile.SetSpeed(projectileSpeed);
                Debug.Log("穿透波目标已设置");
            }
            return;
        }

        EarthquakeProjectile earthquakeProjectile = projectile.GetComponent<EarthquakeProjectile>();
        if (earthquakeProjectile != null)
        {
            // 优先使用特定目标，否则使用通用目标
            Transform specificTarget = earthquakeWaveTarget != null ? earthquakeWaveTarget : targetToUse;
            if (specificTarget != null)
            {
                earthquakeProjectile.SetTarget(specificTarget);
                earthquakeProjectile.SetSpeed(projectileSpeed * 0.6f); // 地震波速度稍慢
                Debug.Log("地震波目标已设置");
            }
            return;
        }

        ProjectileTargetFollower normalProjectile = projectile.GetComponent<ProjectileTargetFollower>();
        if (normalProjectile != null)
        {
            // 优先使用特定目标，否则使用通用目标
            Transform specificTarget = normalProjectileTarget != null ? normalProjectileTarget : targetToUse;
            if (specificTarget != null)
            {
                normalProjectile.SetTarget(specificTarget);
                Debug.Log("普通子弹目标已设置");
            }
            return;
        }
    }

    /// <summary>
    /// 随时间缩放预制体的协程
    /// </summary>
    private IEnumerator ScaleProjectileOverTime(Transform projectileTransform)
    {
        Vector3 initialScale = projectileTransform.localScale;
        
        while (projectileTransform != null)
        {
            // 逐渐增加缩放
            projectileTransform.localScale += Vector3.one * scaleIncreaseRate * Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 设置通用目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 设置穿透波的目标
    /// </summary>
    public void SetPiercingWaveTarget(Transform newTarget)
    {
        piercingWaveTarget = newTarget;
    }

    /// <summary>
    /// 设置地震波的目标
    /// </summary>
    public void SetEarthquakeWaveTarget(Transform newTarget)
    {
        earthquakeWaveTarget = newTarget;
    }

    /// <summary>
    /// 设置普通子弹的目标
    /// </summary>
    public void SetNormalProjectileTarget(Transform newTarget)
    {
        normalProjectileTarget = newTarget;
    }

    /// <summary>
    /// 设置发射预制体
    /// </summary>
    public void SetProjectilePrefab(GameObject newPrefab)
    {
        projectilePrefab = newPrefab;
    }

    /// <summary>
    /// 设置发射频率（每秒发射次数）
    /// </summary>
    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Max(0.1f, newFireRate); // 最小值为0.1以避免过于频繁
        Debug.Log($"发射频率设置为: {fireRate} 次/秒");
    }
    
    /// <summary>
    /// 获取当前发射频率
    /// </summary>
    public float GetFireRate()
    {
        return fireRate;
    }
    
    /// <summary>
    /// 增加发射频率
    /// </summary>
    public void IncreaseFireRate(float increment)
    {
        SetFireRate(fireRate + increment);
    }
    
    /// <summary>
    /// 减少发射频率
    /// </summary>
    public void DecreaseFireRate(float decrement)
    {
        SetFireRate(fireRate - decrement);
    }

    /// <summary>
    /// 检查是否正在发射
    /// </summary>
    public bool IsFiring()
    {
        return isFiring;
    }

    /// <summary>
    /// 停止发射
    /// </summary>
    public void StopFiring()
    {
        StopAllCoroutines();
        isFiring = false;
    }
}