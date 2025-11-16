# 🎮 游戏物理系统 - 快速参考卡片

## 物体类型和配置

### Player（玩家）
```csharp
Rigidbody2D.isKinematic = true
Collider2D.isTrigger = true
能推动Box？ ❌ NO
```

### Shield（盾牌）
```csharp
Rigidbody2D.isKinematic = true
Collider2D.isTrigger = true
能推动Box？ ❌ NO
```

### Wave（波浪）
```csharp
Rigidbody2D.isKinematic = false
Collider2D.isTrigger = false
能推动Box？ ✅ YES
```

### MoveBox（可推动盒子）
```csharp
Rigidbody2D.isKinematic = true
Collider2D.isTrigger = true
能被推动？仅Wave可推动
```

### DestoryBox（破坏盒子）
```csharp
Rigidbody2D.isKinematic = true
Collider2D.isTrigger = true
能被推动？❌ NO
只能被Wave销毁
```

---

## 碰撞规则速查

| 碰撞关系 | 结果 | 说明 |
|---------|------|------|
| Player → Wave | 伤害 | 扣1血 |
| Player → Box | 无反应 | isKinematic阻止 |
| Shield → Wave | 销毁Wave | 可切换状态 |
| Shield → Box | 无反应 | isKinematic阻止 |
| Wave → Box | 推动 | 距离=5单位 |
| Wave → DestoryBox | 销毁 | 盒子消失 |

---

## 关键代码片段

### 确保不能推动Box
```csharp
// Player或Shield在Start中
rb = GetComponent<Rigidbody2D>();
rb.isKinematic = true;  // ← 关键！
```

### 检查Wave能否推动
```csharp
void OnTriggerEnter2D(Collider2D other) {
    // 1. 检查Tag
    if (!other.CompareTag("Wave")) return;
    
    // 2. 检查Rb
    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
    if (rb == null) return;
    
    // 3. 检查速度
    if (rb.velocity.magnitude < 0.1f) return;
    
    // 执行推动
    StartCoroutine(MoveInDirection(rb.velocity));
}
```

### 使用Helper配置
```csharp
PhysicsHelper.SetupPlayer(gameObject);
PhysicsHelper.SetupWave(gameObject);
// 一行搞定！
```

---

## Debug技巧

### 查看是否是运动学刚体
```csharp
if (rb.isKinematic) {
    Debug.Log("运动学刚体 - 不能被物理推动");
} else {
    Debug.Log("动态刚体 - 可以被推动");
}
```

### 查看碰撞类型
```csharp
bool isKinematic = GetComponent<Rigidbody2D>().isKinematic;
bool isTrigger = GetComponent<Collider2D>().isTrigger;
Debug.Log($"运动学:{isKinematic}, 触发器:{isTrigger}");
```

### 检查碰撞权限
```csharp
if (CollisionManager.Instance.CanPushBox(other.gameObject)) {
    Debug.Log("允许推动");
} else {
    Debug.Log("禁止推动");
}
```

---

## 常见问题解决

### 问题：Box还是被推动了
**检查清单：**
- [ ] Box.rb.isKinematic = true?
- [ ] Box.collider.isTrigger = true?
- [ ] Player/Shield.rb.isKinematic = true?
- [ ] OnTriggerEnter中有检查吗？

### 问题：Wave推不动Box
**检查清单：**
- [ ] Wave.rb.isKinematic = false?
- [ ] Wave有速度吗？
- [ ] 碰撞检查返回了吗？
- [ ] velocity.magnitude > 0.1f?

### 问题：Player不能移动
**检查清单：**
- [ ] Player.rb存在吗？
- [ ] FixedUpdate中有MovePosition吗？
- [ ] 游戏是否在运行？

---

## 修改规则方法

### 改变推动距离
```csharp
// MoveBox.cs中
[SerializeField] private float movementDistance = 5f;  // ← 改这个
```

### 改变推动速度
```csharp
// MoveBox.cs中
[SerializeField] private float moveSpeed = 1f;  // ← 改这个
```

### 改变允许推动的Tag
```csharp
// MoveBox.cs中
[SerializeField] private string validPusherTag = "Wave";  // ← 改这个
```

### 改变速度门槛
```csharp
// MoveBox.cs中的OnTriggerEnter2D
if (otherRb.velocity.magnitude > 0.1f)  // ← 改成你想要的值
```

---

## 文件导航

📍 **脚本文件位置**
```
Assets/Script/
├── Player/Player.cs ← 玩家脚本
├── OrbitingObject.cs ← 盾牌脚本
├── MoveBox.cs ← 可推动盒子
├── DestoryBox.cs ← 破坏盒子
├── PhysicsHelper.cs ← 新增：物理助手
└── CollisionManager.cs ← 新增：碰撞管理
```

📍 **文档位置**
```
Gameoff2025/
├── CLEANUP_REPORT.md ← 查漏补缺报告
├── OPTIMIZATION_LOG.md ← 详细优化日志
├── OPTIMIZATION_SUMMARY.md ← 优化总结
└── OPTIMIZATION_COMPLETE.md ← 完成报告
```

---

## 性能小贴士

### 减少碰撞检查
```csharp
// ❌ 每帧都检查
if (Input.GetKeyDown(KeyCode.Space)) { Check(); }

// ✅ 有需要时才检查
if (needsCheck) Check();
```

### 缓存引用
```csharp
// ❌ 每次都GetComponent
Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

// ✅ Start时缓存
Rigidbody2D rb = GetComponent<Rigidbody2D>();
```

### 使用对象池
```csharp
// Wave多了？考虑对象池
WavePool.GetWave();
WavePool.ReturnWave(wave);
```

---

## 命令速查

### 查看所有运动学刚体
```csharp
FindObjectsOfType<Rigidbody2D>()
    .Where(rb => rb.isKinematic)
```

### 查看所有触发器
```csharp
FindObjectsOfType<Collider2D>()
    .Where(c => c.isTrigger)
```

### 禁用所有物理
```csharp
Physics2D.gravity = Vector2.zero;
Physics2D.autoSimulation = false;
```

---

## 紧急修复

### Player意外能推动Box？
```csharp
// 立即修复
Player.GetComponent<Rigidbody2D>().isKinematic = true;
```

### Wave推不动Box？
```csharp
// 检查Wave
Wave.GetComponent<Rigidbody2D>().isKinematic = false;
Wave.GetComponent<Collider2D>().isTrigger = false;
```

### Box消失了？
```csharp
// 检查DestoryBox脚本是否有Bug
Debug.Log(Box.GetComponent<Collider2D>().isTrigger);
```

---

## 下次改进

- [ ] 添加物理层管理
- [ ] 实现对象池
- [ ] 添加碰撞可视化
- [ ] 性能监控
- [ ] 更多音效反馈

---

**快速参考卡 v1.0**  
**最后更新: 2025年11月12日**  
**状态: ✅ 可用**
