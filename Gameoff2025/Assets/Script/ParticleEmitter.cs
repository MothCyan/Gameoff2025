using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    private ParticleSystem particleSystem;
    private bool isPlaying = false;
    
    void Start()
    {
        // 获取粒子系统组件
        particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem == null)
        {
            Debug.LogWarning("ParticleSystem组件未找到！请确保此脚本挂载在包含ParticleSystem的GameObject上。");
        }
    }
    
    /// <summary>
    /// 播放粒子效果
    /// </summary>
    public void Play()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
            isPlaying = true;
            Debug.Log("粒子效果开始播放");
        }
    }
    
    /// <summary>
    /// 停止粒子效果
    /// </summary>
    public void Stop()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
            isPlaying = false;
            Debug.Log("粒子效果已停止");
        }
    }
    
    /// <summary>
    /// 暂停粒子效果
    /// </summary>
    public void Pause()
    {
        if (particleSystem != null)
        {
            particleSystem.Pause();
            isPlaying = false;
            Debug.Log("粒子效果已暂停");
        }
    }
    
    /// <summary>
    /// 清除所有粒子
    /// </summary>
    public void Clear()
    {
        if (particleSystem != null)
        {
            particleSystem.Clear();
            Debug.Log("粒子已清除");
        }
    }
    
    /// <summary>
    /// 切换粒子播放状态
    /// </summary>
    public void Toggle()
    {
        if (isPlaying)
        {
            Stop();
        }
        else
        {
            Play();
        }
    }
    
    /// <summary>
    /// 检查粒子是否正在播放
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }
    
    /// <summary>
    /// 设置粒子发射速率
    /// </summary>
    /// <param name="rate">发射速率</param>
    public void SetEmissionRate(float rate)
    {
        if (particleSystem != null)
        {
            var emission = particleSystem.emission;
            emission.rateOverTime = rate;
        }
    }
    
    /// <summary>
    /// 设置粒子颜色
    /// </summary>
    /// <param name="color">颜色</param>
    public void SetColor(Color color)
    {
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startColor = color;
        }
    }
    
    /// <summary>
    /// 设置粒子生命周期
    /// </summary>
    /// <param name="lifetime">生命周期</param>
    public void SetLifetime(float lifetime)
    {
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startLifetime = lifetime;
        }
    }
    
    /// <summary>
    /// 添加子弹到管理器
    /// </summary>
    public void AddZidan(GameObject zidanObj)
    {
        zidanManager zidanMgr = FindObjectOfType<zidanManager>();
        if (zidanMgr != null)
        {
            zidanMgr.AddZidan(zidanObj);
        }
    }
}
