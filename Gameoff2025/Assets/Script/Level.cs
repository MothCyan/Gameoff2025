using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 关卡脚本 - 管理单个关卡的敌人和出生点
/// </summary>
public class Level : MonoBehaviour
{
    [Header("关卡信息")]
    public string levelName = "关卡1";
    public int levelNumber = 1;
    
    [Header("出生点")]
    public Transform spawnPoint; // 玩家出生点
    
    [Header("敌人管理")]
    public List<GameObject> enemies = new List<GameObject>(); // 敌人列表
    public bool autoFindEnemies = true; // 是否自动查找子物体中的敌人
    public string enemyTag = "Enemy"; // 敌人标签
    
    [Header("关卡完成设置")]
    public bool requireAllEnemiesDead = true; // 是否需要杀死所有敌人才能进入下一关
    public UnityEvent onLevelComplete; // 关卡完成事件
    public UnityEvent onLevelStart; // 关卡开始事件
    
    private LevelManager levelManager;
    private int aliveEnemiesCount = 0;
    private bool levelCompleted = false;

    void Awake()
    {
        // 自动查找出生点
        if (spawnPoint == null)
        {
            Transform spawnTransform = transform.Find("SpawnPoint");
            if (spawnTransform != null)
            {
                spawnPoint = spawnTransform;
            }
            else
            {
                // 如果没有找到，使用关卡自身的位置
                spawnPoint = transform;
                Debug.LogWarning($"关卡 {levelName} 没有设置出生点，将使用关卡中心位置。");
            }
        }
    }

    /// <summary>
    /// 关卡激活时调用
    /// </summary>
    public void OnLevelActivated()
    {
        levelCompleted = false;
        
        // 自动查找敌人
        if (autoFindEnemies)
        {
            FindEnemiesInChildren();
        }
        
        // 统计活着的敌人数量
        CountAliveEnemies();
        
        // 为每个敌人注册死亡监听
        RegisterEnemyDeathListeners();
        
        Debug.Log($"关卡 {levelName} 激活！共有 {aliveEnemiesCount} 个敌人。");
        
        // 触发关卡开始事件
        onLevelStart?.Invoke();
    }

    /// <summary>
    /// 关卡失活时调用
    /// </summary>
    public void OnLevelDeactivated()
    {
        Debug.Log($"关卡 {levelName} 失活。");
    }

    /// <summary>
    /// 自动查找子物体中的所有敌人
    /// </summary>
    void FindEnemiesInChildren()
    {
        enemies.Clear();
        
        // 查找带有特定标签的敌人
        GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag(enemyTag);
        foreach (GameObject enemy in foundEnemies)
        {
            // 只添加当前关卡下的敌人
            if (enemy.transform.IsChildOf(transform))
            {
                enemies.Add(enemy);
            }
        }
        
        // 如果没有通过标签找到，尝试查找带有TripleProjectileEmitter组件的对象
        if (enemies.Count == 0)
        {
            TripleProjectileEmitter[] emitters = GetComponentsInChildren<TripleProjectileEmitter>(true);
            foreach (var emitter in emitters)
            {
                if (!enemies.Contains(emitter.gameObject))
                {
                    enemies.Add(emitter.gameObject);
                }
            }
        }
        
        Debug.Log($"在关卡 {levelName} 中自动找到 {enemies.Count} 个敌人。");
    }

    /// <summary>
    /// 统计活着的敌人数量
    /// </summary>
    void CountAliveEnemies()
    {
        aliveEnemiesCount = 0;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] != null)
            {
                aliveEnemiesCount++;
            }
            else
            {
                // 移除已经不存在的敌人
                enemies.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 为敌人注册死亡监听
    /// </summary>
    void RegisterEnemyDeathListeners()
    {
        // 启动协程持续监测敌人状态
        StartCoroutine(MonitorEnemies());
    }

    /// <summary>
    /// 监测敌人状态的协程
    /// </summary>
    IEnumerator MonitorEnemies()
    {
        while (!levelCompleted)
        {
            yield return new WaitForSeconds(0.5f); // 每0.5秒检查一次
            
            CheckEnemyStatus();
        }
    }

    /// <summary>
    /// 检查敌人状态
    /// </summary>
    void CheckEnemyStatus()
    {
        int currentAliveCount = 0;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                currentAliveCount++;
            }
        }
        
        // 如果敌人数量发生变化
        if (currentAliveCount != aliveEnemiesCount)
        {
            aliveEnemiesCount = currentAliveCount;
            Debug.Log($"关卡 {levelName} 剩余敌人: {aliveEnemiesCount}");
            
            // 如果所有敌人都死了
            if (aliveEnemiesCount <= 0 && requireAllEnemiesDead)
            {
                OnLevelCompleted();
            }
        }
    }

    /// <summary>
    /// 关卡完成
    /// </summary>
    void OnLevelCompleted()
    {
        if (levelCompleted) return;
        
        levelCompleted = true;
        Debug.Log($"关卡 {levelName} 完成！所有敌人已被消灭！");
        
        // 触发关卡完成事件
        onLevelComplete?.Invoke();
        
        // 通知关卡管理器进入下一关
        if (levelManager != null)
        {
            levelManager.GoToNextLevel();
        }
    }

    /// <summary>
    /// 设置关卡管理器引用
    /// </summary>
    public void SetLevelManager(LevelManager manager)
    {
        levelManager = manager;
    }

    /// <summary>
    /// 手动完成关卡（用于特殊情况）
    /// </summary>
    public void CompleteLevel()
    {
        OnLevelCompleted();
    }

    /// <summary>
    /// 添加敌人到列表
    /// </summary>
    public void AddEnemy(GameObject enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
            aliveEnemiesCount++;
            Debug.Log($"添加敌人到关卡 {levelName}，当前敌人数: {aliveEnemiesCount}");
        }
    }

    /// <summary>
    /// 移除敌人从列表
    /// </summary>
    public void RemoveEnemy(GameObject enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            CheckEnemyStatus();
        }
    }

    /// <summary>
    /// 获取剩余敌人数量
    /// </summary>
    public int GetAliveEnemiesCount()
    {
        return aliveEnemiesCount;
    }
}
