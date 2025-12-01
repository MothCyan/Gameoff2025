using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡管理器 - 管理所有关卡的切换和激活
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("关卡设置")]
    public List<Level> levels = new List<Level>(); // 所有关卡列表
    public int currentLevelIndex = 0; // 当前关卡索引
    
    [Header("地图设置")]
    [Tooltip("对应每个关卡的地图GameObject，索引需要与关卡列表一致")]
    public List<GameObject> levelMaps = new List<GameObject>(); // 关卡地图列表
    
    [Header("通关奖励")]
    [Tooltip("所有关卡通过后激活的GameObject（例如：胜利界面、下一章节入口等）")]
    public GameObject allLevelsCompleteObject; // 全关卡通过后激活的对象
    
    [Header("玩家设置")]
    public Transform player; // 玩家对象
    
    [Header("关卡切换设置")]
    public float transitionDelay = 2f; // 切换延迟时间
    
    private Level currentLevel;
    private GameObject currentMap; // 当前激活的地图

    void Start()
    {
        // 自动查找玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // 初始化关卡
        InitializeLevels();
        
        // 激活第一个关卡
        if (levels.Count > 0)
        {
            ActivateLevel(currentLevelIndex);
        }
        else
        {
            Debug.LogError("没有找到任何关卡！请在Inspector中添加关卡。");
        }
    }

    /// <summary>
    /// 初始化所有关卡
    /// </summary>
    void InitializeLevels()
    {
        // 如果列表为空，自动查找场景中的所有关卡
        if (levels.Count == 0)
        {
            Level[] foundLevels = FindObjectsOfType<Level>();
            levels.AddRange(foundLevels);
            Debug.Log($"自动找到 {foundLevels.Length} 个关卡");
        }
        
        // 检查地图列表与关卡列表数量是否一致
        if (levelMaps.Count > 0 && levelMaps.Count != levels.Count)
        {
            Debug.LogWarning($"⚠️ 地图数量({levelMaps.Count})与关卡数量({levels.Count})不一致！");
        }
        
        // 先禁用所有关卡
        foreach (Level level in levels)
        {
            if (level != null)
            {
                level.gameObject.SetActive(false);
            }
        }
        
        // 先禁用所有地图
        foreach (GameObject map in levelMaps)
        {
            if (map != null)
            {
                map.SetActive(false);
            }
        }
        
        // 初始时禁用通关奖励对象
        if (allLevelsCompleteObject != null)
        {
            allLevelsCompleteObject.SetActive(false);
            Debug.Log($"通关奖励对象已初始化为禁用状态: {allLevelsCompleteObject.name}");
        }
    }

    /// <summary>
    /// 激活指定索引的关卡
    /// </summary>
    public void ActivateLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError($"关卡索引 {levelIndex} 超出范围！");
            return;
        }
        
        // 禁用当前关卡
        if (currentLevel != null)
        {
            currentLevel.gameObject.SetActive(false);
            currentLevel.OnLevelDeactivated();
        }
        
        // 禁用当前地图（如果有）
        if (currentMap != null)
        {
            currentMap.SetActive(false);
            Debug.Log($"禁用地图: {currentMap.name}");
        }
        
        // 激活新关卡
        currentLevelIndex = levelIndex;
        currentLevel = levels[currentLevelIndex];
        currentLevel.gameObject.SetActive(true);
        
        // 激活对应的地图（如果有）
        if (levelIndex < levelMaps.Count && levelMaps[levelIndex] != null)
        {
            currentMap = levelMaps[levelIndex];
            currentMap.SetActive(true);
            Debug.Log($"激活地图: {currentMap.name}");
        }
        else
        {
            currentMap = null;
            if (levelMaps.Count > 0)
            {
                Debug.LogWarning($"⚠️ 关卡 {levelIndex} 没有对应的地图！");
            }
        }
        
        // 设置关卡管理器引用
        currentLevel.SetLevelManager(this);
        
        // 移动玩家到出生点
        MovePlayerToSpawnPoint();
        
        // 初始化关卡
        currentLevel.OnLevelActivated();
        
        Debug.Log($"✅ 激活关卡 {currentLevelIndex}: {currentLevel.gameObject.name}");
    }

    /// <summary>
    /// 移动玩家到当前关卡的出生点
    /// </summary>
    void MovePlayerToSpawnPoint()
    {
        if (player != null && currentLevel != null && currentLevel.spawnPoint != null)
        {
            player.position = currentLevel.spawnPoint.position;
            Debug.Log($"玩家移动到出生点: {currentLevel.spawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("无法移动玩家到出生点！检查玩家或出生点是否设置。");
        }
    }

    /// <summary>
    /// 进入下一个关卡
    /// </summary>
    public void GoToNextLevel()
    {
        if (currentLevelIndex < levels.Count - 1)
        {
            StartCoroutine(TransitionToNextLevel());
        }
        else
        {
            Debug.Log("已经是最后一个关卡！游戏完成！");
            OnAllLevelsCompleted();
        }
    }

    /// <summary>
    /// 关卡切换协程
    /// </summary>
    IEnumerator TransitionToNextLevel()
    {
        Debug.Log($"⏳ 准备进入下一关卡，{transitionDelay}秒后切换...");
        
        // 记录当前通过的关卡索引
        int completedLevelIndex = currentLevelIndex;
        
        yield return new WaitForSeconds(transitionDelay);
        
        // 先激活下一关卡（包括下一个地图）
        ActivateLevel(currentLevelIndex + 1);
        
        // 然后失活上一关卡的地图
        if (completedLevelIndex < levelMaps.Count && levelMaps[completedLevelIndex] != null)
        {
            levelMaps[completedLevelIndex].SetActive(false);
            Debug.Log($"✅ 关卡 {completedLevelIndex} 通过！地图已失活: {levelMaps[completedLevelIndex].name}");
        }
    }

    /// <summary>
    /// 所有关卡完成
    /// </summary>
    void OnAllLevelsCompleted()
    {
        Debug.Log("🎉 恭喜！所有关卡已完成！");
        
        // 激活通关奖励对象
        if (allLevelsCompleteObject != null)
        {
            allLevelsCompleteObject.SetActive(true);
            Debug.Log($"✅ 通关奖励已激活: {allLevelsCompleteObject.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ 未设置通关奖励对象！");
        }
        
        // 可以在这里添加其他游戏胜利的逻辑
        // 例如：播放胜利音乐、保存游戏进度等
    }

    /// <summary>
    /// 重新开始当前关卡
    /// </summary>
    public void RestartCurrentLevel()
    {
        ActivateLevel(currentLevelIndex);
    }

    /// <summary>
    /// 跳转到指定关卡
    /// </summary>
    public void GoToLevel(int levelIndex)
    {
        ActivateLevel(levelIndex);
    }

    /// <summary>
    /// 获取当前关卡
    /// </summary>
    public Level GetCurrentLevel()
    {
        return currentLevel;
    }

    /// <summary>
    /// 获取总关卡数
    /// </summary>
    public int GetTotalLevels()
    {
        return levels.Count;
    }
    
    /// <summary>
    /// 获取当前地图
    /// </summary>
    public GameObject GetCurrentMap()
    {
        return currentMap;
    }
    
    /// <summary>
    /// 手动失活指定索引的地图
    /// </summary>
    public void DeactivateMap(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex < levelMaps.Count && levelMaps[mapIndex] != null)
        {
            levelMaps[mapIndex].SetActive(false);
            Debug.Log($"手动失活地图 {mapIndex}: {levelMaps[mapIndex].name}");
        }
    }
    
    /// <summary>
    /// 检查关卡和地图配置（用于调试）
    /// </summary>
    [ContextMenu("检查关卡地图配置")]
    public void DebugLevelMapConfiguration()
    {
        Debug.Log($"========== 关卡地图配置检查 ==========");
        Debug.Log($"关卡数量: {levels.Count}");
        Debug.Log($"地图数量: {levelMaps.Count}");
        Debug.Log($"通关奖励对象: {(allLevelsCompleteObject != null ? allLevelsCompleteObject.name : "未设置")}");
        Debug.Log($"");
        
        int maxCount = Mathf.Max(levels.Count, levelMaps.Count);
        
        for (int i = 0; i < maxCount; i++)
        {
            string levelName = i < levels.Count && levels[i] != null ? levels[i].gameObject.name : "NULL";
            string mapName = i < levelMaps.Count && levelMaps[i] != null ? levelMaps[i].name : "NULL";
            
            string status = "✅";
            if (i >= levels.Count || levels[i] == null)
            {
                status = "❌ 缺少关卡";
            }
            else if (i >= levelMaps.Count || levelMaps[i] == null)
            {
                status = "⚠️ 缺少地图";
            }
            
            Debug.Log($"[{i}] {status}");
            Debug.Log($"    关卡: {levelName}");
            Debug.Log($"    地图: {mapName}");
        }
        
        Debug.Log($"========== 检查完成 ==========");
    }
    
    /// <summary>
    /// 手动触发全关卡通过（用于测试）
    /// </summary>
    [ContextMenu("测试-触发全关卡通过")]
    public void TestAllLevelsComplete()
    {
        Debug.Log("🧪 [测试] 手动触发全关卡通过...");
        OnAllLevelsCompleted();
    }
}
