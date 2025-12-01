using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 房间状态枚举
/// </summary>
public enum RoomState
{
    NotCleared,  // 未通过
    Cleared      // 已通过
}

/// <summary>
/// 房间管理器 - 管理单个房间的战斗状态和传送点
/// </summary>
public class Room : MonoBehaviour
{
    [Header("房间状态")]
    [SerializeField] private RoomState roomState = RoomState.NotCleared;
    
    [Header("敌人配置")]
    [Tooltip("房间内的所有敌人")]
    public List<GameObject> enemies = new List<GameObject>();
    
    [Header("激活距离设置")]
    [Tooltip("玩家进入这个距离内激活敌人")]
    public float activationDistance = 15f;
    [Tooltip("是否启用距离激活")]
    public bool useDistanceActivation = true;
    
    [Header("传送点配置")]
    [Tooltip("房间内的所有传送点")]
    public List<TeleportPoint> teleportPoints = new List<TeleportPoint>();
    
    [Header("事件")]
    public UnityEvent onRoomEnterBattle;  // 进入战斗时触发
    public UnityEvent onRoomCleared;      // 房间通过时触发
    
    private bool isInBattle = false;      // 是否正在战斗
    private bool hasEnteredBattle = false; // 是否已经进入过战斗（防止重复触发）
    private Transform player;              // 玩家Transform
    private bool isMonitoringDistance = false; // 是否正在监控距离

    void Start()
    {
        Debug.Log($"[Room] ========== {gameObject.name} Start 开始 ==========");
        
        // 查找玩家
        FindPlayer();
        
        // 自动查找房间内的传送点
        FindTeleportPointsInChildren();
        
        // 自动查找房间内的敌人
        FindEnemiesInChildren();
        
        // 初始化时停止所有敌人
        DeactivateAllEnemies();
        
        // 根据初始状态设置传送点
        UpdateTeleportPointsState();
        
        // 如果启用距离激活,开始监控
        if (useDistanceActivation && roomState == RoomState.NotCleared)
        {
            StartCoroutine(MonitorPlayerDistance());
        }
        
        Debug.Log($"[Room] ========== {gameObject.name} Start 完成 ==========");
    }
    
    void OnEnable()
    {
        Debug.Log($"[Room] OnEnable 被调用: {gameObject.name}");
        
        // 重新查找并停止敌人（防止Level激活时敌人被意外激活）
        if (enemies.Count > 0)
        {
            Debug.Log($"[Room] OnEnable中停止敌人（防止意外激活）");
            DeactivateAllEnemies();
        }
    }
    
    /// <summary>
    /// 查找玩家
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"[Room] 找到玩家: {player.name}");
        }
        else
        {
            Debug.LogWarning($"[Room] 未找到玩家! 请确保玩家Tag为'Player'");
        }
    }
    
    /// <summary>
    /// 监控玩家距离
    /// </summary>
    private IEnumerator MonitorPlayerDistance()
    {
        isMonitoringDistance = true;
        Debug.Log($"[Room] 开始监控玩家距离，激活距离: {activationDistance}");
        
        while (!hasEnteredBattle && roomState == RoomState.NotCleared)
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                
                // 每秒输出一次距离信息
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[Room] 玩家距离房间中心: {distance:F2}，激活距离: {activationDistance}");
                }
                
                if (distance <= activationDistance)
                {
                    Debug.Log($"[Room] ✅ 玩家进入激活范围! 距离: {distance:F2}");
                    EnterBattle();
                    yield break;
                }
            }
            else
            {
                // 如果玩家丢失,重新查找
                FindPlayer();
            }
            
            yield return new WaitForSeconds(0.1f); // 每0.1秒检查一次
        }
        
        isMonitoringDistance = false;
    }

    /// <summary>
    /// 自动查找子物体中的传送点
    /// </summary>
    private void FindTeleportPointsInChildren()
    {
        TeleportPoint[] foundTeleportPoints = GetComponentsInChildren<TeleportPoint>();
        foreach (var tp in foundTeleportPoints)
        {
            if (!teleportPoints.Contains(tp))
            {
                teleportPoints.Add(tp);
                tp.SetRoom(this); // 设置传送点所属的房间
            }
        }
        Debug.Log($"[Room] {gameObject.name} 找到 {teleportPoints.Count} 个传送点");
    }

    /// <summary>
    /// 自动查找子物体中的敌人
    /// </summary>
    private void FindEnemiesInChildren()
    {
        Debug.Log($"[Room] 开始查找敌人...");
        enemies.Clear();
        
        // 通过Tag查找
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        Debug.Log($"[Room] GetComponentsInChildren找到 {allChildren.Length} 个Transform");
        
        int enemyCount = 0;
        foreach (Transform child in allChildren)
        {
            try
            {
                if (child.CompareTag("enemy"))
                {
                    Debug.Log($"[Room] 找到敌人: {child.gameObject.name}, 激活状态: {child.gameObject.activeInHierarchy}");
                    if (!enemies.Contains(child.gameObject))
                    {
                        enemies.Add(child.gameObject);
                        enemyCount++;
                    }
                }
            }
            catch (UnityException)
            {
                // Tag不存在,跳过
            }
        }
        
        Debug.Log($"[Room] {gameObject.name} 找到 {enemyCount} 个敌人，列表总数: {enemies.Count}");
    }

    /// <summary>
    /// 手动触发战斗（由其他系统调用）
    /// </summary>
    public void TriggerBattle()
    {
        if (roomState == RoomState.NotCleared && !hasEnteredBattle)
        {
            Debug.Log($"[Room] 手动触发战斗");
            EnterBattle();
        }
    }

    /// <summary>
    /// 进入战斗状态
    /// </summary>
    private void EnterBattle()
    {
        Debug.Log($"[Room] 房间 {gameObject.name} 进入战斗!");
        isInBattle = true;
        hasEnteredBattle = true;
        
        // 激活所有敌人
        ActivateAllEnemies();
        
        // 禁用所有传送点
        SetTeleportPointsEnabled(false);
        
        // 触发战斗事件
        onRoomEnterBattle?.Invoke();
        
        // 开始监控敌人
        StartCoroutine(MonitorEnemies());
    }

    /// <summary>
    /// 监控敌人状态
    /// </summary>
    private IEnumerator MonitorEnemies()
    {
        while (isInBattle)
        {
            // 移除已被销毁的敌人
            enemies.RemoveAll(enemy => enemy == null);
            
            // 检查是否所有敌人都被消灭
            if (enemies.Count == 0)
            {
                ClearRoom();
                yield break;
            }
            
            yield return new WaitForSeconds(0.5f); // 每0.5秒检查一次
        }
    }

    /// <summary>
    /// 房间通过
    /// </summary>
    private void ClearRoom()
    {
        Debug.Log($"[Room] 房间 {gameObject.name} 通过!");
        isInBattle = false;
        roomState = RoomState.Cleared;
        
        // 停止所有敌人（虽然应该都被消灭了，但以防万一）
        DeactivateAllEnemies();
        
        // 启用所有传送点
        SetTeleportPointsEnabled(true);
        
        // 触发通过事件
        onRoomCleared?.Invoke();
    }

    /// <summary>
    /// 设置所有传送点的启用状态
    /// </summary>
    private void SetTeleportPointsEnabled(bool enabled)
    {
        foreach (var tp in teleportPoints)
        {
            if (tp != null)
            {
                tp.SetEnabled(enabled);
            }
        }
        Debug.Log($"[Room] 传送点状态设置为: {enabled}");
    }

    /// <summary>
    /// 更新传送点状态（根据房间状态）
    /// </summary>
    private void UpdateTeleportPointsState()
    {
        // 未通过状态：如果还没进入战斗，传送点启用；如果已进入战斗，传送点禁用
        // 已通过状态：传送点启用
        bool shouldEnable = (roomState == RoomState.Cleared) || (roomState == RoomState.NotCleared && !hasEnteredBattle);
        SetTeleportPointsEnabled(shouldEnable);
    }

    /// <summary>
    /// 获取房间状态
    /// </summary>
    public RoomState GetRoomState()
    {
        return roomState;
    }

    /// <summary>
    /// 是否正在战斗
    /// </summary>
    public bool IsInBattle()
    {
        return isInBattle;
    }

    /// <summary>
    /// 手动设置房间为通过状态（用于测试）
    /// </summary>
    public void SetCleared()
    {
        if (roomState != RoomState.Cleared)
        {
            enemies.Clear();
            ClearRoom();
        }
    }

    /// <summary>
    /// 重置房间状态（用于测试）
    /// </summary>
    public void ResetRoom()
    {
        roomState = RoomState.NotCleared;
        isInBattle = false;
        hasEnteredBattle = false;
        
        // 停止所有敌人
        DeactivateAllEnemies();
        
        UpdateTeleportPointsState();
        
        // 重新开始监控距离
        if (useDistanceActivation && !isMonitoringDistance)
        {
            StartCoroutine(MonitorPlayerDistance());
        }
    }
    
    /// <summary>
    /// 在Scene视图中绘制激活范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (useDistanceActivation)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, activationDistance);
            
            // 绘制标签
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * activationDistance, 
                $"激活距离: {activationDistance}m");
            #endif
        }
    }
    
    /// <summary>
    /// 激活所有敌人
    /// </summary>
    private void ActivateAllEnemies()
    {
        Debug.Log($"[Room] ========== 开始激活所有敌人 ==========");
        Debug.Log($"[Room] 敌人列表数量: {enemies.Count}");
        
        int activatedCount = 0;
        int failedCount = 0;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Debug.Log($"[Room] 检查敌人: {enemy.name}");
                
                // 尝试获取TripleProjectileEmitter组件
                TripleProjectileEmitter emitter = enemy.GetComponent<TripleProjectileEmitter>();
                if (emitter != null)
                {
                    Debug.Log($"[Room] 找到TripleProjectileEmitter组件，isEnemyAI={emitter.isEnemyAI}");
                    emitter.ActivateAI();
                    activatedCount++;
                    Debug.Log($"[Room] ✅ 成功激活敌人: {enemy.name}");
                }
                else
                {
                    failedCount++;
                    Debug.LogWarning($"[Room] ❌ 敌人 {enemy.name} 没有TripleProjectileEmitter组件!");
                }
                
                // 可以在这里添加其他敌人类型的激活逻辑
            }
            else
            {
                Debug.LogWarning($"[Room] ⚠️ 敌人对象为null");
            }
        }
        
        Debug.Log($"[Room] 激活完成: 成功={activatedCount}, 失败={failedCount}, 总数={enemies.Count}");
    }
    
    /// <summary>
    /// 停止所有敌人
    /// </summary>
    private void DeactivateAllEnemies()
    {
        Debug.Log($"[Room] ========== 开始停止所有敌人 ==========");
        Debug.Log($"[Room] 敌人列表数量: {enemies.Count}");
        
        int deactivatedCount = 0;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                // 尝试获取TripleProjectileEmitter组件
                TripleProjectileEmitter emitter = enemy.GetComponent<TripleProjectileEmitter>();
                if (emitter != null)
                {
                    emitter.DeactivateAI();
                    deactivatedCount++;
                    Debug.Log($"[Room] ✅ 停止敌人: {enemy.name}");
                }
                
                // 可以在这里添加其他敌人类型的停止逻辑
            }
        }
        
        Debug.Log($"[Room] 停止完成: {deactivatedCount}/{enemies.Count}");
    }
}
