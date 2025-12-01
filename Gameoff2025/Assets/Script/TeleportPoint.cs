using UnityEngine;

/// <summary>
/// 传送点 - 碰到一个传送点会传送到另一个传送点
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TeleportPoint : MonoBehaviour
{
    [Header("传送目标")]
    [Tooltip("传送到的目标传送点")]
    public TeleportPoint targetTeleportPoint;
    
    [Header("传送设置")]
    [Tooltip("传送后的偏移量（避免重复触发）")]
    public Vector2 teleportOffset = Vector2.zero;
    
    [Tooltip("传送冷却时间")]
    public float cooldownTime = 0.5f;
    
    [Header("视觉效果")]
    [Tooltip("是否启用")]
    public bool isEnabled = true;
    
    [Tooltip("禁用时的颜色")]
    public Color disabledColor = Color.gray;
    
    [Tooltip("启用时的颜色")]
    public Color enabledColor = Color.cyan;
    
    private Room parentRoom;              // 所属房间
    private bool canTeleport = true;      // 是否可以传送（冷却控制）
    private SpriteRenderer spriteRenderer; // 视觉反馈

    void Start()
    {
        // 确保碰撞体是触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // 获取SpriteRenderer用于视觉反馈
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 更新视觉状态
        UpdateVisuals();
    }

    /// <summary>
    /// 设置所属房间
    /// </summary>
    public void SetRoom(Room room)
    {
        parentRoom = room;
    }

    /// <summary>
    /// 触发器检测
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只传送玩家
        if (!other.CompareTag("Player"))
            return;
        
        // 检查所属房间是否在战斗中
        if (parentRoom != null && parentRoom.IsInBattle())
        {
            Debug.Log($"[TeleportPoint] {gameObject.name} 房间正在战斗中，传送点禁用");
            return;
        }
        
        // 检查是否启用
        if (!isEnabled)
        {
            Debug.Log($"[TeleportPoint] {gameObject.name} 传送点未启用");
            return;
        }
        
        // 检查冷却
        if (!canTeleport)
        {
            Debug.Log($"[TeleportPoint] {gameObject.name} 传送点冷却中");
            return;
        }
        
        // 检查目标传送点
        if (targetTeleportPoint == null)
        {
            Debug.LogWarning($"[TeleportPoint] {gameObject.name} 没有设置目标传送点!");
            return;
        }
        
        // 检查目标传送点是否启用
        if (!targetTeleportPoint.isEnabled)
        {
            Debug.Log($"[TeleportPoint] 目标传送点 {targetTeleportPoint.gameObject.name} 未启用");
            return;
        }
        
        // 检查目标房间是否在战斗中
        if (targetTeleportPoint.parentRoom != null && targetTeleportPoint.parentRoom.IsInBattle())
        {
            Debug.Log($"[TeleportPoint] 目标房间正在战斗中，传送点禁用");
            return;
        }
        
        // 执行传送
        PerformTeleport(other.gameObject);
    }

    /// <summary>
    /// 执行传送
    /// </summary>
    private void PerformTeleport(GameObject player)
    {
        Debug.Log($"[TeleportPoint] ========== 执行传送 ==========");
        Debug.Log($"[TeleportPoint] 从 {gameObject.name} 传送到 {targetTeleportPoint.gameObject.name}");
        Debug.Log($"[TeleportPoint] 源房间: {(parentRoom != null ? parentRoom.gameObject.name : "null")}");
        Debug.Log($"[TeleportPoint] 目标房间: {(targetTeleportPoint.parentRoom != null ? targetTeleportPoint.parentRoom.gameObject.name : "null")}");
        
        // 禁用双方的传送（避免重复触发）
        canTeleport = false;
        targetTeleportPoint.canTeleport = false;
        
        // 计算传送位置（目标位置 + 偏移）
        Vector3 targetPosition = targetTeleportPoint.transform.position + (Vector3)targetTeleportPoint.teleportOffset;
        
        // 传送玩家
        player.transform.position = targetPosition;
        Debug.Log($"[TeleportPoint] 玩家传送到位置: {targetPosition}");
        
        // 启动冷却计时
        Invoke(nameof(ResetCooldown), cooldownTime);
        targetTeleportPoint.Invoke(nameof(ResetCooldown), cooldownTime);
        
        // 只通知目标房间玩家已传送(触发战斗)
        if (targetTeleportPoint.parentRoom != null)
        {
            Debug.Log($"[TeleportPoint] 通知目标房间 {targetTeleportPoint.parentRoom.gameObject.name} 玩家已传送");
            //targetTeleportPoint.parentRoom.OnPlayerTeleported();
        }
    }

    /// <summary>
    /// 重置冷却
    /// </summary>
    private void ResetCooldown()
    {
        canTeleport = true;
    }

    /// <summary>
    /// 设置启用状态
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        UpdateVisuals();
        Debug.Log($"[TeleportPoint] {gameObject.name} 启用状态: {enabled}");
    }

    /// <summary>
    /// 更新视觉效果
    /// </summary>
    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isEnabled ? enabledColor : disabledColor;
        }
    }

    /// <summary>
    /// 在编辑器中绘制连线（方便查看传送关系）
    /// </summary>
    void OnDrawGizmos()
    {
        if (targetTeleportPoint != null)
        {
            Gizmos.color = isEnabled ? Color.cyan : Color.gray;
            Gizmos.DrawLine(transform.position, targetTeleportPoint.transform.position);
            
            // 绘制箭头
            Vector3 direction = (targetTeleportPoint.transform.position - transform.position).normalized;
            Vector3 arrowPos = targetTeleportPoint.transform.position - direction * 0.5f;
            Gizmos.DrawSphere(arrowPos, 0.1f);
        }
        
        // 绘制传送点本身
        Gizmos.color = isEnabled ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    /// <summary>
    /// 在编辑器中绘制标签
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 显示传送点名称
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, gameObject.name);
        #endif
    }
}
