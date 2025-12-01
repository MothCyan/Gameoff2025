using UnityEngine;

/// <summary>
/// 房间系统设置指南
/// 这个脚本仅用于说明如何设置房间系统，不需要添加到GameObject上
/// </summary>
public class RoomSetupGuide : MonoBehaviour
{
    /*
     * ==================== 房间系统设置指南 ====================
     * 
     * 1. 创建房间结构:
     *    - 创建空物体命名为 "Room1"
     *    - 添加 Room 组件
     *    - 在Room下创建子物体作为房间内容
     * 
     * 2. 设置敌人:
     *    - 在Room下创建敌人物体
     *    - 敌人Tag必须设为 "enemy"
     *    - 添加 TripleProjectileEmitter 组件
     *    - 勾选 "Is Enemy AI" 选项
     *    - 设置移动速度、攻击频率等参数
     * 
     * 3. 设置传送点:
     *    - 在Room下创建两个传送点 (Portal_A 和 Portal_B)
     *    - 都添加 TeleportPoint 组件
     *    - 都添加 Collider2D 组件并勾选 Is Trigger
     *    - Portal_A 的 Target Teleport Point 设为 Portal_B
     *    - Portal_B 的 Target Teleport Point 设为 Portal_A
     *    - 可选：添加 SpriteRenderer 显示传送门图标
     * 
     * 4. 工作流程:
     *    a) 游戏开始时：
     *       - 敌人不会移动和攻击（处于停止状态）
     *       - 传送点可用（绿色）
     *    
     *    b) 玩家使用传送点：
     *       - 传送到目标位置
     *       - 立即触发战斗
     *       - 所有敌人开始移动和攻击
     *       - 所有传送点禁用（灰色）
     *    
     *    c) 战斗进行中：
     *       - 敌人执行AI逻辑（移动和攻击）
     *       - 玩家无法使用传送点逃跑
     *    
     *    d) 消灭所有敌人：
     *       - 房间通过
     *       - 敌人停止（虽然已被消灭）
     *       - 传送点重新启用（绿色）
     *       - 可以自由传送到其他房间
     *    
     *    e) 已通过的房间：
     *       - 传送点始终可用
     *       - 不会再次触发战斗
     * 
     * 5. 多个房间设置:
     *    - 重复步骤1-3创建多个房间
     *    - Room1、Room2、Room3等
     *    - 每个房间可以有自己的传送点连接
     *    - 可以创建房间间的传送点（Room1的传送点连接到Room2）
     * 
     * 6. 必需的Tag设置:
     *    - 玩家Tag: "Player"
     *    - 敌人Tag: "enemy"
     *    - 波Tag: "Wave"
     * 
     * 7. 调试提示:
     *    - 查看Console输出了解房间状态
     *    - [Room] 前缀表示房间日志
     *    - [TeleportPoint] 前缀表示传送点日志
     *    - [Enemy] 前缀表示敌人日志
     * 
     * 8. 常见问题:
     *    Q: 敌人一开始就会移动？
     *    A: 检查 TripleProjectileEmitter 的 Is Enemy AI 是否勾选
     *    
     *    Q: 传送后敌人还是不动？
     *    A: 检查敌人的Tag是否设为 "enemy"，Room会自动查找
     *    
     *    Q: 传送点不可用？
     *    A: 检查Collider2D是否设为Is Trigger，且两个传送点互相引用
     *    
     *    Q: 战斗中还能传送？
     *    A: 检查传送点是否作为Room的子物体，Room会自动管理
     * 
     * ==================== 示例层级结构 ====================
     * 
     * Scene
     * ├── Player (Tag: Player)
     * ├── Room1 (Room组件)
     * │   ├── Enemy1 (Tag: enemy, TripleProjectileEmitter)
     * │   ├── Enemy2 (Tag: enemy, TripleProjectileEmitter)
     * │   ├── Portal_A (TeleportPoint, Collider2D)
     * │   └── Portal_B (TeleportPoint, Collider2D)
     * ├── Room2 (Room组件)
     * │   ├── Enemy3 (Tag: enemy, TripleProjectileEmitter)
     * │   ├── Portal_C (TeleportPoint, Collider2D)
     * │   └── Portal_D (TeleportPoint, Collider2D)
     * └── RoomManager (RoomManager组件)
     * 
     * ========================================================
     */
}
