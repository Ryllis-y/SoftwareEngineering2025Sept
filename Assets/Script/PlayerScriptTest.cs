using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using System;
[System.Serializable]
public class PlayerScriptTest : MonoBehaviour
{
    public GameObject player;
    [Header("Player Move")]
    public RuntimeAnimatorController animatorController;
    public Vector2 playerDirection;
    public Rigidbody2D rb;

    private float xInput;
    public float moveSpeed = 5f;
    public Animator playerAnimator;

    // 动画平滑过渡参数
    [Header("Player Better Move")]
    public float animationSmoothTime = 0.1f; // 动画参数平滑过渡时间
    private float currentSpeed; // 当前速度（用于平滑过渡）
    private float velocity; // 平滑阻尼计算用的速度变量

    // 方向跟踪变量
    private float lastDirection = 1f; // 初始赋值为向右 (1表示右，-1表示左)

    [Header("Player Jump")]
    public float jumpForce = 300f;
    public LayerMask groundLayer;
    public bool isGrounded = true;
    public bool isJumping = false;
    public bool alreadyFalled = false;

    [Header("Player Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    public bool isDashing = false; // 改为bool类型
    private float lastDashTime = -Mathf.Infinity;
    private float dashTimer = 0f; // 冲刺计时器

    [Header("Player Attack")]
    public int combo = 0;  // 当前连招数
    public float attackComboTimeout = 30000f; // 连招时间间隔
    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    public Transform attackPoint; // 攻击点位置
    public GameObject attackBox; // 攻击碰撞体
    public float attackRadius = 10f; // 攻击范围半径
    public LayerMask enemyLayer; // 敌人层级
    public float[] comboDamages = { 10f, 15f, 20f }; // 连招伤害数组
    [Header("Player Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvulnerable = false; // 无敌状态
    public float invulnerabilityDuration = 0.5f; // 无敌时间
    public Slider healthBar;
    public Text hpText;
    public RectTransform damageNumberPoint;
    public GameObject damageNumberPrefab;
    public Text gameOverText;
    [Header("工具设置")]
    public Tool currentTool; // 当前手持工具（在Inspector中拖入配置好的工具）
    public List<Tool> availableTools; // 玩家拥有的工具列表
    [Header("建造/破坏设置")]
    public float interactRange = 5f; // 交互最大距离
    public LayerMask blockLayer; // 方块所在的层级（在Unity中创建“Block”层）
    [Header("方块选择UI")]
    public KeyCode[] blockSelectionKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };
    public BlockType[] availableBlockTypes = { BlockType.Dirt, BlockType.Stone, BlockType.Wood, BlockType.Sand, BlockType.Glass };
    // 添加缺失的建造相关变量
    [Header("方块预览")]
    public GameObject blockPreviewPrefab; // 方块预览预制体
    private GameObject currentPreview; // 当前预览对象
    private BlockType selectedBlockType = BlockType.Dirt; // 当前选中的方块类型



    void Start()
    {
        gameOverText.enabled = false;
        // 查找Player对象
        player = GameObject.FindWithTag("PlayerTag");
        if (player == null)
        {
            Debug.LogError("找不到带有PlayerTag标签的玩家对象！");
            return;
        }
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.value = 1f;
        if (hpText != null)
            hpText.text = $"{currentHealth} / {maxHealth}";

        //currentTool = availableTools[0]; // 初始时默认选择第一个工具
        // 自动获取或添加Rigidbody2D组件
        rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // 自动获取或添加Animator组件并设置控制器
        playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null)
        {
            playerAnimator = player.AddComponent<Animator>();
        }

        enemyLayer = LayerMask.GetMask("Enemy");
        if (enemyLayer == 0)
        {
            Debug.Log("未找到Enemy层，请确保已创建并分配给敌人对象");
        }
        else
        {
            Debug.Log("成功找到Enemy层");
        }
        groundLayer = LayerMask.GetMask("Ground", "Block");
        if (groundLayer == 0)
        {
            Debug.Log("未找到Ground或Block层，请确保已创建并分配");
        }
        else
        {
            Debug.Log("成功合并Ground和Block层用于接地检测");
        }

        // 设置动画控制器
        if (animatorController == null)
        {
            animatorController = Resources.Load<RuntimeAnimatorController>("PlayerAnimationController");
            if (animatorController != null)
            {
                playerAnimator.runtimeAnimatorController = animatorController;
                Debug.Log("成功加载并设置PlayerAnimationController");
            }
            else
            {
                Debug.LogError("未找到PlayerAnimationController，请检查资源路径或在Inspector中指定");
            }
        }
        else
        {
            playerAnimator.runtimeAnimatorController = animatorController;
        }

        playerDirection = new Vector2(5f, 0);

        // 初始化动画参数
        if (playerAnimator != null)
        {
            //playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.SetBool("isRunning", false);
            playerAnimator.SetBool("isDashing", false); // 初始化isDashing为false
        }
    }

    void Update()
    {
        // 检测冲刺输入
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time - lastDashTime > dashCooldown && !isDashing && !isAttacking)// 防止在冲刺中再次触发 不加isGrounded约束增强手感
        {
            StartDash();
        }

        // 死亡判定：玩家y坐标低于-30自动死亡
        if (player != null && player.transform.position.y < -30f && enabled)
        {
            Debug.Log("玩家坠落出界，判定为死亡");
            Die();
        }

        PlayerMove();

        CheckGround();
        PlayerJump();
        PlayerAttack();
        if (Input.GetMouseButtonDown(1))
        {
            TryPlaceBlock();
        }
        if (Input.GetMouseButtonDown(0))
        {
            TryBreakBlock();
        }
        if (Input.GetMouseButton(1)) // 按住右键时显示调试信息
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Debug.DrawRay(mouseWorldPos, Vector3.up * 0.5f, Color.red, 0.1f);
            Debug.DrawRay(mouseWorldPos, Vector3.down * 0.5f, Color.red, 0.1f);
            Debug.DrawRay(mouseWorldPos, Vector3.left * 0.5f, Color.red, 0.1f);
            Debug.DrawRay(mouseWorldPos, Vector3.right * 0.5f, Color.red, 0.1f);
        }

        // 实时更新预览位置
        //UpdateBlockPreview();

        // 处理方块选择输入
        HandleBlockSelection();

        // 冲刺计时
        if (isDashing)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashDuration)
            {
                EndDash();
            }
        }

        // // 连招超时检测
        // if (isAttacking)
        // {
        //     attackTimer += Time.deltaTime;
        //     // 如果攻击动画播放时间过长或需要重置，可以在这里处理
        // }    可扩展，暂时不需要

        // 连招超时重置
        // if (combo > 1 && Time.time - lastAttackTime > attackComboTimeout)
        // {
        //     ResetCombo();
        // }
    }

    void FixedUpdate()
    {
        PlayerMoveFixed();
    }

    public void PlayerMove()
    {
        if (playerAnimator == null) return;

        xInput = Input.GetAxis("Horizontal");

        // 更新最后方向：只有当有水平输入时才更新
        if (Mathf.Abs(xInput) > 0.1f)
        {
            lastDirection = Mathf.Sign(xInput);
        }

        // 使用平滑阻尼让速度变化更自然
        float targetSpeed = Mathf.Abs(xInput);
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref velocity, animationSmoothTime);

        // 优化：使用float参数控制动画过渡，比bool更平滑
        //playerAnimator.SetFloat("Speed", currentSpeed);

        // 保持bool参数用于兼容，但主要使用float参数控制动画
        playerAnimator.SetBool("isRunning", currentSpeed > 0.1f);

        // 角色翻转优化：只有当输入有明显变化时才翻转
        if (Mathf.Abs(xInput) > 0.1f)
        {
            // 平滑翻转：使用Lerp让翻转更柔和
            Vector3 newScale = player.transform.localScale;
            newScale.x = Mathf.Lerp(newScale.x, Mathf.Sign(xInput), Time.deltaTime * 20f);
            player.transform.localScale = newScale;
        }
    }

    public void PlayerMoveFixed()
    {
        if (rb == null) return;

        if (isDashing)
        {
            // 冲刺时，使用最后记录的方向而不是当前输入
            rb.linearVelocity = new Vector2(lastDirection * dashSpeed, rb.linearVelocity.y);
        }
        else if (!isAttacking) // 攻击时不能移动
        {
            // 移动添加平滑过渡，避免突然的速度变化
            Vector2 targetVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 10f);
        }
        else
        {
            // 攻击时减速或停止移动
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);
        }
    }

    public void PlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isAttacking)
        {
            rb.AddForce(new Vector2(rb.linearVelocity.x, jumpForce));
            playerAnimator.SetTrigger("isJumping");
            isJumping = true;
        }
    }

    public void PlayerAttack()
    {
        if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
        {
            combo++;
            if (combo > 3) combo = 1;

            // 触发对应的攻击动画
            switch (combo)
            {
                case 1:
                    playerAnimator.ResetTrigger("Attack3");
                    playerAnimator.SetTrigger("Attack1");
                    break;
                case 2:
                    playerAnimator.ResetTrigger("Attack1");
                    playerAnimator.SetTrigger("Attack2");
                    break;
                case 3:
                    playerAnimator.ResetTrigger("Attack2");
                    playerAnimator.SetTrigger("Attack3");
                    break;
            }

            // 立即执行攻击检测
            ExecuteAttack();

            isAttacking = true;
            lastAttackTime = Time.time;
            attackTimer = 0f;

            Invoke("ResetAttackState", 0.3f);
        }
    }
    // 执行攻击检测（替代碰撞体）
    // 修改 ExecuteAttack 方法
    private void ExecuteAttack()
    {
        Debug.Log("执行攻击检测");
        if (attackPoint == null)
        {
            Debug.LogError("攻击点未设置！");
        }
        Vector3 attackPosition;

        // 如果攻击点未设置，使用玩家位置加上偏移
        if (attackPoint == null)
        {
            float attackDirection = Mathf.Sign(transform.localScale.x); // 获取玩家朝向
            attackPosition = transform.position + new Vector3(attackDirection * 0.5f, 0, 0);
        }
        else
        {
            attackPosition = attackPoint.position;
        }

        // 使用物理检测范围内的敌人
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPosition,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase == null)
            {
                Debug.LogError("敌人对象缺少EnemyBase组件！");
                continue;
            }
            if (enemyBase != null)
            {
                enemyBase.GetHit(comboDamages[combo - 1]);
                Debug.Log("Enemy hit! Damage: " + comboDamages[combo - 1]);
            }
        }
    }

    // 重置攻击状态
    private void ResetAttackState()
    {
        isAttacking = false;
    }

    // 重置连招
    private void ResetCombo()
    {
        combo = 1;
    }

    // 开始冲刺
    public void StartDash()
    {
        isDashing = true;
        dashTimer = 0f;
        lastDashTime = Time.time;
        playerAnimator.SetBool("isDashing", true); // 设置isDashing为true，触发冲刺动画
    }

    // 结束冲刺
    public void EndDash()
    {
        isDashing = false;
        playerAnimator.SetBool("isDashing", false); // 设置isDashing为false，退出冲刺动画
    }

    public void CheckGround()
    {
        Vector2 endPos = player.transform.position + Vector3.down;
        Vector2 startPos = player.transform.position;
        RaycastHit2D hit = Physics2D.Linecast(startPos, endPos, groundLayer);

        // 1. 检测是否在地面或方块上
        bool isOnGroundOrBlock = hit.collider != null;

        // 2. 检测垂直速度：velocity.y < 0 表示"正在下降"
        bool isFalling = rb.linearVelocity.y < 0;

        // 3. 状态1：在地面或方块上
        if (isOnGroundOrBlock)
        {
            isGrounded = true;
            playerAnimator.SetTrigger("isGround");

            // 落地时，重置所有跳跃/下降状态
            if (isJumping || isFalling)
            {
                isJumping = false;
                isFalling = false;
                alreadyFalled = false;
                playerAnimator.ResetTrigger("isJumping");
                playerAnimator.ResetTrigger("isLanding");
                playerAnimator.SetTrigger("isGround");
            }
        }
        // 4. 状态2：在空中且正在下降（已过最高点）
        else if (isFalling)
        {
            playerAnimator.ResetTrigger("isGround");
            isGrounded = false;
            if (!alreadyFalled)
            {
                alreadyFalled = true;
                playerAnimator.SetTrigger("isLanding");
            }
            playerAnimator.ResetTrigger("isJumping");
        }
        // 5. 状态3：在空中且正在上升（未到最高点）
        else if (rb.linearVelocity.y > 0 && isJumping)
        {
            playerAnimator.ResetTrigger("isGround");
            isGrounded = false;
        }
    }
    public void CreateAttackBox()
    {
        Debug.Log("尝试创建攻击碰撞体");
        if (attackPoint == null)
        {
            Debug.LogError("攻击点未设置！");
            return;
        }
        GameObject tmpObject = Instantiate(attackBox, attackPoint.position, attackPoint.rotation);
        tmpObject.transform.localScale = attackPoint.localScale;
        if (tmpObject == null)
        {
            Debug.LogError("攻击碰撞体实例化失败！");
        }
        else
        {
            Debug.Log(tmpObject.name + " 攻击碰撞体已创建");
        }
    }
    public void GetHit(float damage)
    {
        if (isInvulnerable) return; // 无敌状态下不受伤害

        currentHealth -= damage;
        Debug.Log($"玩家受击！伤害: {damage}, 剩余生命: {currentHealth}");

        if (healthBar != null)
            healthBar.value = currentHealth / maxHealth;
        if (hpText != null)
            hpText.text = $"{currentHealth} / {maxHealth}";
        // 伤害飘字
        if (damageNumberPrefab != null && damageNumberPoint != null)
        {
            GameObject dmgNum = Instantiate(damageNumberPrefab, damageNumberPoint.position, Quaternion.identity, healthBar.transform.parent);
            dmgNum.transform.localScale = damageNumberPoint.localScale;
            dmgNum.GetComponent<Text>().text = damage.ToString();
        }

        // 可选：添加无敌时间
        StartCoroutine(InvulnerabilityCoroutine());

        // 检查死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 无敌时间协程（可选）
    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    // 死亡方法
    private void Die()
    {
        Debug.Log("玩家死亡！");
        playerAnimator.ResetTrigger("GetHit");
        playerAnimator.SetBool("isRunning", false);
        playerAnimator.SetBool("isDashing", false);
        playerAnimator.SetTrigger("isGround");
        playerAnimator.ResetTrigger("isJumping");
        playerAnimator.ResetTrigger("isLanding");
        playerAnimator.ResetTrigger("Attack1");
        playerAnimator.ResetTrigger("Attack2");
        playerAnimator.ResetTrigger("Attack3");
        playerAnimator.SetTrigger("Dead");
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Die");
        }

        // 禁用玩家控制
        enabled = false;
        rb.linearVelocity = Vector2.zero;

        // 游戏结束处理
        Invoke("GameOver", 2f);
    }

    private void GameOver()
    {
        Debug.Log("游戏结束");
        // 显示“game over”文本
        if (gameOverText != null)
        {
            gameOverText.enabled = true;
        }

        // 3秒后返回菜单场景
        Invoke("LoadMenuScene", 3f);
    }

    // 可视化玩家受击范围（调试用）
    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }

    private void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu"); // 切换到名为"Menu"的场景
    }

    // 处理方块选择输入
    private void HandleBlockSelection()
    {
        for (int i = 0; i < blockSelectionKeys.Length && i < availableBlockTypes.Length; i++)
        {
            if (Input.GetKeyDown(blockSelectionKeys[i]))
            {
                selectedBlockType = availableBlockTypes[i];
                Debug.Log($"选择方块类型: {selectedBlockType}");
            }
        }
    }

    // 更新方块预览（显示在鼠标指向的位置）
    // private void UpdateBlockPreview()
    // {
    //     // 获取鼠标在世界中的位置
    //     Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     mouseWorldPos.z = 0; // 确保z坐标为0（2D游戏）

    //     // 射线检测鼠标指向的方块表面
    //     Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
    //     RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero, 0f, blockLayer);

    //     if (hit.collider != null)
    //     {
    //         // 计算放置位置（在点击的方块旁边）
    //         Vector2Int placePos = WorldData.WorldToGrid(hit.point + hit.normal * 0.5f);
    //         WorldData worldData = FindObjectOfType<WorldData>();

    //         // 检查位置是否可放置
    //         if (worldData.IsPositionInWorld(placePos)) //&& !worldData.HasBlockAt(placePos)
    //         {
    //             Vector3 worldPos = WorldData.GridToWorld(placePos);

    //             // 检查背包是否有足够资源
    //             ItemType requiredItem = selectedBlockType switch
    //             {
    //                 BlockType.Dirt => ItemType.DirtClump,
    //                 BlockType.Stone => ItemType.StoneChunk,
    //                 BlockType.Wood => ItemType.WoodLog,
    //                 BlockType.Sand => ItemType.SandGrain,
    //                 BlockType.Glass => ItemType.GlassPane,
    //                 BlockType.Torch => ItemType.TorchItem,
    //                 BlockType.Painting => ItemType.PaintingItem,
    //                 _ => ItemType.DirtClump
    //             };

    //             Inventory inventory = GetComponent<Inventory>();
    //             bool canPlace = inventory.GetItemAmount(requiredItem) >= 1;

    //             // 创建或移动预览模型
    //             if (currentPreview == null)
    //             {
    //                 currentPreview = Instantiate(blockPreviewPrefab, worldPos, Quaternion.identity);
    //             }
    //             else
    //             {
    //                 currentPreview.transform.position = worldPos;
    //             }

    //             // 根据是否可以放置改变预览颜色
    //             SpriteRenderer previewRenderer = currentPreview.GetComponent<SpriteRenderer>();
    //             if (previewRenderer != null)
    //             {
    //                 previewRenderer.color = canPlace ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 0.3f, 0.3f, 0.5f);
    //             }
    //         }
    //         else
    //         {
    //             // 位置不可放置时，隐藏预览
    //             if (currentPreview != null) Destroy(currentPreview);
    //         }
    //     }
    //     else
    //     {
    //         // 鼠标未指向方块时，隐藏预览
    //         if (currentPreview != null) Destroy(currentPreview);
    //     }
    // }

    // 尝试放置方块
    // 获取方块类型对应的物品类型
    private ItemType GetRequiredItemForBlock(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Dirt => ItemType.DirtClump,
            BlockType.Stone => ItemType.StoneChunk,
            BlockType.Wood => ItemType.WoodLog,
            BlockType.Sand => ItemType.SandGrain,
            BlockType.Glass => ItemType.GlassPane,
            BlockType.Torch => ItemType.TorchItem,
            BlockType.Painting => ItemType.PaintingItem,
            _ => ItemType.DirtClump
        };
    }

    // 尝试放置方块
    private void TryPlaceBlock()
    {
        // 获取鼠标在世界中的位置
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Debug.Log($"鼠标世界坐标: {mouseWorldPos}");

        // 检查交互距离
        float distance = Vector3.Distance(player.transform.position, mouseWorldPos);
        if (distance > interactRange)
        {
            Debug.Log($"距离太远，无法放置方块。距离: {distance}, 最大距离: {interactRange}");
            return;
        }

        // 射线检测鼠标指向的方块表面
        Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // 调试信息：显示射线参数
        Debug.Log($"射线起点: {rayOrigin}");
        Debug.Log($"Block Layer Mask: {blockLayer}");
        Debug.Log($"Block Layer Value: {blockLayer.value}");

        // 使用更大的检测范围，而不是单点检测
        RaycastHit2D hit = Physics2D.CircleCast(rayOrigin, 0.1f, Vector2.zero, 0f, blockLayer);

        // 如果CircleCast失败，尝试OverlapPoint
        if (hit.collider == null)
        {
            Collider2D overlapHit = Physics2D.OverlapPoint(rayOrigin, blockLayer);
            if (overlapHit != null)
            {
                // 创建一个虚拟的RaycastHit2D
                hit = new RaycastHit2D();
                // 注意：这里需要手动设置hit的属性，但RaycastHit2D是只读的
                // 所以我们需要用不同的方法处理
                Debug.Log($"OverlapPoint检测到方块: {overlapHit.name}");
                HandleBlockPlacement(overlapHit, rayOrigin);
                return;
            }
        }

        if (hit.collider != null)
        {
            Debug.Log($"射线检测成功，击中方块: {hit.collider.name}");
            Debug.Log($"碰撞点: {hit.point}");
            Debug.Log($"法线方向: {hit.normal}");

            // 计算放置位置（在碰撞点的法线方向偏移0.5单位）
            Vector2Int placePos = WorldData.WorldToGrid(hit.point + hit.normal * 0.5f);
            Debug.Log($"计算的放置位置: {placePos}");

            WorldData worldData = FindObjectOfType<WorldData>();

            if (worldData == null)
            {
                Debug.LogError("未找到WorldData组件！");
                return;
            }

            // 检查位置是否可放置
            if (worldData.IsPositionInWorld(placePos) && !worldData.HasBlockAt(placePos))
            {
                // 检查背包是否有足够资源
                ItemType requiredItem = GetRequiredItemForBlock(selectedBlockType);
                Inventory inventory = GetComponent<Inventory>();

                if (inventory == null)
                {
                    Debug.LogError("未找到Inventory组件！");
                    return;
                }

                if (inventory.GetItemAmount(requiredItem) >= 1)
                {
                    // 消耗资源并放置方块
                    inventory.RemoveItem(requiredItem, 1);
                    worldData.AddBlock(selectedBlockType, placePos);
                    Debug.Log($"成功放置 {selectedBlockType} 在位置 {placePos}");
                }
                else
                {
                    Debug.Log($"缺少 {requiredItem}，无法放置 {selectedBlockType}");
                }
            }
            else
            {
                Debug.Log($"该位置无法放置方块 - 位置: {placePos}");
                Debug.Log($"在世界范围内: {worldData.IsPositionInWorld(placePos)}");
                Debug.Log($"位置已有方块: {worldData.HasBlockAt(placePos)}");
            }
        }
        else
        {
            Debug.Log("射线检测失败 - 可能的原因:");
            Debug.Log("1. Block Layer 设置错误");
            Debug.Log("2. 方块没有Collider2D组件");
            Debug.Log("3. 方块的Layer设置错误");
            Debug.Log("4. 摄像机设置问题");

            // 尝试检测所有层的对象
            RaycastHit2D allLayersHit = Physics2D.Raycast(rayOrigin, Vector2.zero, 0f);
            if (allLayersHit.collider != null)
            {
                Debug.Log($"在其他层检测到对象: {allLayersHit.collider.name}, Layer: {LayerMask.LayerToName(allLayersHit.collider.gameObject.layer)}");
            }
            else
            {
                Debug.Log("在所有层都没有检测到任何对象");
            }
        }
    }

    // 处理方块放置的辅助方法
    private void HandleBlockPlacement(Collider2D targetCollider, Vector2 mousePos)
    {
        // 计算放置位置（简化版，直接在鼠标位置附近放置）
        Vector2Int placePos = WorldData.WorldToGrid(mousePos);

        // 尝试在周围找一个空位置
        Vector2Int[] offsets = {
        new Vector2Int(0, 1),   // 上
        new Vector2Int(0, -1),  // 下
        new Vector2Int(1, 0),   // 右
        new Vector2Int(-1, 0),  // 左
    };

        WorldData worldData = FindObjectOfType<WorldData>();
        if (worldData == null) return;

        foreach (Vector2Int offset in offsets)
        {
            Vector2Int testPos = placePos + offset;
            if (worldData.IsPositionInWorld(testPos) && !worldData.HasBlockAt(testPos))
            {
                // 找到可放置的位置
                ItemType requiredItem = GetRequiredItemForBlock(selectedBlockType);
                Inventory inventory = GetComponent<Inventory>();

                if (inventory != null && inventory.GetItemAmount(requiredItem) >= 1)
                {
                    inventory.RemoveItem(requiredItem, 1);
                    worldData.AddBlock(selectedBlockType, testPos);
                    Debug.Log($"成功放置 {selectedBlockType} 在位置 {testPos}");
                    return;
                }
            }
        }

        Debug.Log("周围没有可放置的位置");
    }

    // 尝试破坏方块
    private void TryBreakBlock()
    {
        // 获取鼠标在世界中的位置
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 检查交互距离
        float distance = Vector3.Distance(player.transform.position, mouseWorldPos);
        if (distance > interactRange)
        {
            Debug.Log("距离太远，无法破坏方块");
            return;
        }

        // 射线检测鼠标指向的方块
        Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        RaycastHit2D hit = Physics2D.CircleCast(rayOrigin, 0.1f, Vector2.zero, 0f, blockLayer);

        if (hit.collider != null)
        {
            // 从方块GameObject的名称解析坐标（格式：BlockType_x_y）
            string[] nameParts = hit.collider.gameObject.name.Split('_');
            if (nameParts.Length >= 3 &&
                int.TryParse(nameParts[1], out int x) &&
                int.TryParse(nameParts[2], out int y))
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                WorldData worldData = FindObjectOfType<WorldData>();

                if (worldData == null)
                {
                    Debug.LogError("未找到WorldData组件！");
                    return;
                }

                if (currentTool == null)
                {
                    Debug.LogError("未设置当前工具！");
                    return;
                }

                // 调用世界数据的方块伤害方法
                worldData.DamageBlock(gridPos, currentTool);
            }
            else
            {
                Debug.LogError($"无法解析方块坐标，GameObject名称: {hit.collider.gameObject.name}");
            }
        }
        else
        {
            Debug.Log("未检测到可破坏的方块");
        }
    }

    // 清理预览对象（在组件被销毁时调用）
    private void OnDestroy()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
    }
    public void RestorePlayerData(GameData data)
    {
        // 恢复位置
        if (player != null)
        {
            player.transform.position = new Vector3(data.playerX, data.playerY, 0);
        }

        // 恢复生命值
        currentHealth = data.playerHealth;
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
        if (hpText != null)
        {
            hpText.text = $"{currentHealth} / {maxHealth}";
        }
    }

}
