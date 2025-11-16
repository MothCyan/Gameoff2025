using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹管理器 - 管理所有发射的子弹
/// </summary>
public class zidanManager : BaseSingleton<zidanManager>
{
    private List<GameObject> bullets = new List<GameObject>();
    
    /// <summary>
    /// 添加子弹到管理器
    /// </summary>
    public void AddZidan(GameObject bulletObj)
    {
        if (!bullets.Contains(bulletObj))
        {
            bullets.Add(bulletObj);
            Debug.Log($"[zidanManager] 添加子弹，当前子弹数: {bullets.Count}");
        }
    }

    /// <summary>
    /// 移除子弹
    /// </summary>
    public void RemoveZidan(GameObject bulletObj)
    {
        if (bullets.Remove(bulletObj))
        {
            Debug.Log($"[zidanManager] 移除子弹，当前子弹数: {bullets.Count}");
        }
    }

    /// <summary>
    /// 销毁所有子弹
    /// </summary>
    public void Pu()
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            if (bullets[i] != null)
            {
                Destroy(bullets[i]);
            }
        }
        bullets.Clear();
        Debug.Log("[zidanManager] 销毁所有子弹");
    }

    /// <summary>
    /// 获取当前子弹数量
    /// </summary>
    public int GetBulletCount()
    {
        // 清理空引用
        bullets.RemoveAll(b => b == null);
        return bullets.Count;
    }

    /// <summary>
    /// 清理空引用的子弹
    /// </summary>
    public void CleanupDeadBullets()
    {
        int removed = bullets.RemoveAll(b => b == null);
        if (removed > 0)
        {
            Debug.Log($"[zidanManager] 清理了 {removed} 个死亡子弹");
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)) Pu();
        
        // 定期清理死亡的子弹（每1秒一次）
        if (Time.frameCount % 60 == 0)
        {
            CleanupDeadBullets();
        }
    }
}