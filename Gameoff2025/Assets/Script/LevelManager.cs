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
    
    [Header("玩家设置")]
    public Transform player; // 玩家对象
    
    [Header("关卡切换设置")]
    public float transitionDelay = 2f; // 切换延迟时间
    
    private Level currentLevel;

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
        
        // 先禁用所有关卡
        foreach (Level level in levels)
        {
            if (level != null)
            {
                level.gameObject.SetActive(false);
            }
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
        
        // 激活新关卡
        currentLevelIndex = levelIndex;
        currentLevel = levels[currentLevelIndex];
        currentLevel.gameObject.SetActive(true);
        
        // 设置关卡管理器引用
        currentLevel.SetLevelManager(this);
        
        // 移动玩家到出生点
        MovePlayerToSpawnPoint();
        
        // 初始化关卡
        currentLevel.OnLevelActivated();
        
        Debug.Log($"激活关卡 {currentLevelIndex}: {currentLevel.gameObject.name}");
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
        Debug.Log($"准备进入下一关卡，{transitionDelay}秒后切换...");
        
        yield return new WaitForSeconds(transitionDelay);
        
        ActivateLevel(currentLevelIndex + 1);
    }

    /// <summary>
    /// 所有关卡完成
    /// </summary>
    void OnAllLevelsCompleted()
    {
        Debug.Log("恭喜！所有关卡已完成！");
        // 可以在这里添加游戏胜利的逻辑
        // 例如：显示胜利界面、播放胜利音乐等
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
}
