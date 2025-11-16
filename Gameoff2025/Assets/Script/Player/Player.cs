using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5;
    public float shieldRadius1 = 2;
    public float shieldRadius2 = 3;
    public OrbitingObject shieldPrimary;
    public OrbitingObject shieldSecondary;
    
    [Header("加速设置")]
    public float boostedSpeed = 15f; // 加速后的速度
    public float boostDuration = 0.3f; // 加速持续时间

    private Rigidbody2D rb;
    private Camera mainCamera;
    private float currentSpeed;
    private bool isBoosting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        HandleMovement();
        HandleBoost();
        UpdateShields();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        rb.velocity = direction * currentSpeed;
    }

    void HandleBoost()
    {
        // 按下右键触发加速
        if (Input.GetMouseButtonDown(1) && !isBoosting)
        {
            StartCoroutine(BoostCoroutine());
        }
    }

    /// <summary>
    /// 加速协程
    /// </summary>
    private IEnumerator BoostCoroutine()
    {
        isBoosting = true;
        currentSpeed = boostedSpeed; // 速度加快
        Debug.Log($"加速启动！速度: {currentSpeed}");
        
        yield return new WaitForSeconds(boostDuration);
        
        currentSpeed = moveSpeed; // 速度恢复
        isBoosting = false;
        Debug.Log($"加速结束！速度: {currentSpeed}");
    }

    void UpdateShields()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector2 dirToMouse = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;

        if (shieldPrimary != null)
        {
            Vector2 pos1 = (Vector2)transform.position + dirToMouse * shieldRadius1;
            shieldPrimary.UpdateOrbit(pos1, angle);
        }

        if (shieldSecondary != null)
        {
            Vector2 pos2 = (Vector2)transform.position + dirToMouse * shieldRadius2;
            shieldSecondary.UpdateOrbit(pos2, angle);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wave"))
        {
            zidanManager.Instance.Pu();
            Destroy(gameObject);
        }
    }
}