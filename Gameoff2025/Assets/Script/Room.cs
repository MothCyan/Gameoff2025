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
    
    [Header("传送点配置")]
    [Tooltip("房间内的所有传送点")]
    public List<TeleportPoint> teleportPoints = new List<TeleportPoint>();
    
    [Header("事件")]
    public UnityEvent onRoomEnterBattle;  // 进入战斗时触发
    public UnityEvent onRoomCleared;      // 房间通过时触发
    
    private bool isInBattle = false;      // 是否正在战斗
    private bool hasEnteredBattle = false; // 是否已经进入过战斗（防止重复触发）
    private bool playerHasTeleported = false; // 玩家是否已经传送过

    void Start()
    {
        // 自动查找房间内的传送点
        FindTeleportPointsInChildren();
        
        // 自动查找房间内的敌人
        FindEnemiesInChildren();
        
        // 根据初始状态设置传送点
        UpdateTeleportPointsState();
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
        // 通过Tag查找
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag("enemy") && !enemies.Contains(child.gameObject))
            {
                enemies.Add(child.gameObject);
            }
        }
        
        Debug.Log($"[Room] {gameObject.name} 找到 {enemies.Count} 个敌人");
    }

    /// <summary>
    /// 玩家进入房间
    /// </summary>
    public void OnPlayerEnterRoom()
    {
        Debug.Log($"[Room] 玩家进入房间: {gameObject.name}, 状态: {roomState}");
        
        // 如果房间未通过且未进入过战斗，且玩家已经传送过，则进入战斗
        if (roomState == RoomState.NotCleared && !hasEnteredBattle && playerHasTeleported)
        {
            Debug.Log($"[Room] 玩家已传送，触发战斗!");
            EnterBattle();
        }
        else if (roomState == RoomState.NotCleared && !playerHasTeleported)
        {
            Debug.Log($"[Room] 等待玩家使用传送点...");
        }
    }
    
    /// <summary>
    /// 标记玩家已传送（由传送点调用）
    /// </summary>
    public void OnPlayerTeleported()
    {
        playerHasTeleported = true;
        Debug.Log($"[Room] 玩家已使用传送点");
        
        // 如果房间未通过且未进入过战斗，立即触发战斗
        if (roomState == RoomState.NotCleared && !hasEnteredBattle)
        {
            Debug.Log($"[Room] 传送后触发战斗!");
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
        playerHasTeleported = false;
        UpdateTeleportPointsState();
    }
}
