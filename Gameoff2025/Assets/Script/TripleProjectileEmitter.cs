using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleProjectileEmitter : MonoBehaviour
{
    [Header("发射设置")]
    public GameObject projectilePrefab; // 要发射的预制体
    public Transform target; // 通用目标对象
    public float fireInterval = 0.5f; // 发射间隔
    public float projectileSpeed = 10f; // 发射速度
    public float scaleIncreaseRate = 0.1f; // 缩放增加速率

    [Header("发射位置")]
    public Transform firePoint; // 发射点，如果为空则使用自身位置

    [Header("不同波的目标设置")]
    public Transform piercingWaveTarget; // 穿透波的目标
    public Transform earthquakeWaveTarget; // 地震波的目标
    public Transform normalProjectileTarget; // 普通子弹的目标

    private bool isFiring = false;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) FireTripleProjectiles();
        // 如果目标存在，让发射器朝向目标（只改变Z轴）
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                // 计算朝向目标的Z轴角度
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
    void Start()
    {
        FireTripleProjectiles();
    }

    /// <summary>
    /// 开始发射三次预制体
    /// </summary>
    public void FireTripleProjectiles()
    {
        if (isFiring || projectilePrefab == null) return;

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
        // 确定发射位置
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        
        // 实例化预制体
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
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
    }

    /// <summary>
    /// 根据预制体脚本类型设置对应的目标
    /// </summary>
    private void SetProjectileTarget(GameObject projectile, Vector3 spawnPosition)
    {
        // 检查预制体上挂载的脚本类型
        PiercingProjectile piercingProjectile = projectile.GetComponent<PiercingProjectile>();
        if (piercingProjectile != null && piercingWaveTarget != null)
        {
            piercingProjectile.SetTarget(piercingWaveTarget);
            Debug.Log("穿透波目标已设置");
            return;
        }

        EarthquakeProjectile earthquakeProjectile = projectile.GetComponent<EarthquakeProjectile>();
        if (earthquakeProjectile != null && earthquakeWaveTarget != null)
        {
            earthquakeProjectile.SetTarget(earthquakeWaveTarget);
            Debug.Log("地震波目标已设置");
            return;
        }

        ProjectileTargetFollower normalProjectile = projectile.GetComponent<ProjectileTargetFollower>();
        if (normalProjectile != null && normalProjectileTarget != null)
        {
            normalProjectile.SetTarget(normalProjectileTarget);
            Debug.Log("普通子弹目标已设置");
            return;
        }

        // 如果没有特定目标，使用通用目标
        if (target != null)
        {
            if (piercingProjectile != null)
                piercingProjectile.SetTarget(target);
            else if (earthquakeProjectile != null)
                earthquakeProjectile.SetTarget(target);
            else if (normalProjectile != null)
                normalProjectile.SetTarget(target);
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