using UnityEngine;
using UnityEngine.UI;
public enum EnemyState
{
    Idle,
    Patrol,
    Pursuit,
    Attack,
    GetHit,
    Die
}

public class EnemyBase : MonoBehaviour
{
    private EnemyPoolManager poolManager;
    public SpriteRenderer spriteRenderer;
    public float maxHealth = 100f;
    public float currentHealth;
    public Animator animator;
    [Header("Enemy Move")]
    public Transform leftBoundry;
    public Transform rightBoundry;
    public Rigidbody2D rb;
    public bool isRight = false;
    public float moveSpeed = 2f;
    public bool ableToMove = true;
    public EnemyState currentState = EnemyState.Patrol;
    [Header("Enemy Idle")]
    public float idleDuration = 2f;
    [Header("Enemy Pursuit")]
    GameObject player;
    [Header("Enemy Attack")]
    public float attackCooldown = 2f;
    public float attackDistance = 1f;
    public float attackPreparationTime = 0.2f; // 攻击前摇时间
    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;
    public float attackDuration = 1.5f; // 攻击动作持续时间
    [Header("Enemy Attack Damage")]
    public float enemyDamage = 5f; // 怪物伤害
    public float attackDetectionRadius = 1.5f; // 攻击检测半径
    public LayerMask playerLayer; // 玩家层级

    [Header("Enemy GetHit")]
    public float getHitDuration = 0.3f; // 受击动画时长（可根据动画调整）
    public float hitStiffnessDuration = 0.5f; // 受击后僵直时长（核心：设置为0.5s）
    public bool isGetHit = false;
    //记录受击前的状态（用于僵直后恢复原状态）
    private EnemyState preHitState;
    public Slider healthBar; // 血条UI
    public Text hpText; // 血量文本
    public RectTransform damageNumberPoint; // 伤害数字生成点
    public GameObject damageNumberPrefab; // 伤害数字预制体

    public virtual void Start()
    {
        currentHealth = maxHealth;
        currentHealth = maxHealth;
        hpText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
        playerLayer = LayerMask.GetMask("Player");
        if (playerLayer == 0)
        {
            Debug.LogError("未找到Player层级");
        }
        UpdateSpriteDirection();
        ChangeState(EnemyState.Patrol);
        UpdateSpriteDirection();
        ChangeState(EnemyState.Patrol);
        poolManager = FindFirstObjectByType<EnemyPoolManager>();
    }

    public virtual void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleUpdate();
                break;
            case EnemyState.Patrol:
                PatrolUpdate();
                break;
            case EnemyState.Pursuit:
                PursuitUpdate();
                break;
            case EnemyState.Attack:
                AttackUpdate();
                break;
            case EnemyState.GetHit:
                GetHitUpdate();
                break;
            case EnemyState.Die:
                DieUpdate();
                break;
        }
    }

    public virtual void FixedUpdate()
    {
        if (ableToMove && !isAttacking)
        {
            rb.linearVelocity = new Vector2((isRight ? 1 : -1) * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void UpdateSpriteDirection()
    {
        spriteRenderer.flipX = isRight;
    }

    #region State Methods

    public virtual void IdleEnter()
    {
        ableToMove = false; // Idle状态下禁用移动（实现僵直）
        animator.SetBool("isRunning", false);
        // 仅在“正常巡逻的Idle”时设置默认时长，受击后的僵直Idle由单独逻辑控制
        if (preHitState != EnemyState.GetHit)
        {
            Invoke("ReturnToPatrol", idleDuration);
        }
    }

    public virtual void IdleUpdate() { }

    public virtual void IdleExit()
    {
        CancelInvoke("ReturnToPatrol");
        // 退出Idle时恢复移动能力（僵直结束）
        if (currentState != EnemyState.GetHit && currentState != EnemyState.Die)
        {
            ableToMove = true;
        }
    }

    public virtual void PatrolEnter()
    {
        ableToMove = true;
        animator.SetBool("isRunning", true);
    }

    public virtual void PatrolUpdate()
    {
        if (isRight && transform.position.x >= rightBoundry.position.x)
        {
            ChangeState(EnemyState.Idle);
        }
        else if (!isRight && transform.position.x <= leftBoundry.position.x)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    public virtual void PatrolExit() { }

    private void ReturnToPatrol()
    {
        isRight = !isRight;
        UpdateSpriteDirection();
        ChangeState(EnemyState.Patrol);
    }

    // 新增：僵直结束后恢复原状态（追击/巡逻）
    private void ReturnToPreHitState()
    {
        if (player != null && Vector2.Distance(transform.position, player.transform.position) <= attackDistance * 2f)
        {
            // 玩家仍在范围内，恢复到追击状态
            ChangeState(EnemyState.Pursuit);
        }
        else
        {
            // 玩家不在范围内，恢复到巡逻状态
            ChangeState(EnemyState.Patrol);
        }
    }

    public virtual void PursuitEnter()
    {
        ableToMove = true;
        animator.SetBool("isRunning", true);
    }

    public virtual void PursuitUpdate()
    {
        if (player != null)
        {
            bool playerIsRight = player.transform.position.x > transform.position.x;

            if (isRight != playerIsRight)
            {
                isRight = playerIsRight;
                UpdateSpriteDirection();
            }

            // 检查攻击条件：距离足够且在冷却时间外
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            bool canAttack = distanceToPlayer <= attackDistance &&
                           Time.time - lastAttackTime >= attackCooldown;

            if (canAttack)
            {
                ChangeState(EnemyState.Attack);
            }
        }
    }

    public virtual void PursuitExit() { }

    public virtual void AttackEnter()
    {
        ableToMove = false;
        isAttacking = true;
        animator.SetBool("isRunning", false);
        rb.linearVelocity = Vector2.zero;
        // 触发攻击动画
        animator.SetTrigger("Attack1");

        // 记录攻击时间
        lastAttackTime = Time.time;

        // 攻击前摇（可选）
        Invoke("OnAttackHit", attackPreparationTime);

        // 攻击结束后返回追击状态
        Invoke("FinishAttack", attackCooldown + attackDuration);

    }

    public virtual void AttackUpdate()
    {
        // 攻击过程中可以保持面向玩家
        rb.linearVelocity = Vector2.zero;
        if (player != null)
        {
            bool playerIsRight = player.transform.position.x > transform.position.x;
            if (isRight != playerIsRight)
            {
                isRight = playerIsRight;
                UpdateSpriteDirection();
            }
        }
    }

    public virtual void AttackExit()
    {
        isAttacking = false;
        CancelInvoke("OnAttackHit");
        CancelInvoke("FinishAttack");
        animator.ResetTrigger("Attack1");
    }

    // 攻击命中的时机（由动画事件或计时器调用）
    protected virtual void OnAttackHit()
    {
        Debug.Log($"攻击检测: 当前状态={currentState}, 玩家={(player != null ? player.name : "null")}");

        // 检查玩家是否在攻击范围内
        if (player != null && Vector2.Distance(transform.position, player.transform.position) <= attackDistance * 1.2f)
        {
            // 执行攻击检测
            ExecuteEnemyAttack();
        }
    }
    private void ExecuteEnemyAttack()
    {
        Debug.Log("怪物执行攻击检测");

        Vector3 attackPosition = transform.position;
        if (spriteRenderer != null)
        {
            // 根据朝向调整攻击位置
            float attackDirection = isRight ? 1f : -1f;
            attackPosition += new Vector3(attackDirection * 0.8f, 0.5f, 0);
        }

        // 使用物理检测范围内的玩家
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
            attackPosition,
            attackDetectionRadius,
            playerLayer
        );

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerScriptTest playerScript = playerCollider.GetComponent<PlayerScriptTest>();
            if (playerScript != null)
            {
                playerScript.GetHit(enemyDamage);
                Debug.Log("玩家被击中！伤害: " + enemyDamage);
            }
        }
    }


    // 攻击结束
    protected virtual void FinishAttack()
    {
        if (player != null && Vector2.Distance(transform.position, player.transform.position) <= attackDistance * 1.5f)
        {
            // 玩家还在攻击范围内，继续攻击
            ChangeState(EnemyState.Attack);
        }
        else if (player != null)
        {
            // 玩家离开攻击范围，返回追击
            ChangeState(EnemyState.Pursuit);
        }
        else
        {
            // 玩家消失，返回巡逻
            ChangeState(EnemyState.Patrol);
        }
    }

    public virtual void GetHitEnter()
    {
        // 记录受击前的状态（用于后续恢复）
        preHitState = currentState;
        ableToMove = false;
        animator.SetBool("isRunning", false);
        // 受击时中断攻击
        CancelInvoke("OnAttackHit");
        CancelInvoke("FinishAttack");
        isAttacking = false;
        // 触发受击动画
        animator.SetBool("getHit", true);
        isGetHit = true;
    }

    public virtual void GetHitUpdate()
    {
        // 受击动画播放完成后（按getHitDuration时长），进入Idle僵直
        if (isGetHit)
        {
            Invoke(nameof(EnterHitStiffness), getHitDuration);
            isGetHit = false;
        }
    }

    public virtual void GetHitExit()
    {
        animator.SetBool("getHit", false);
        isGetHit = false;
        CancelInvoke(nameof(EnterHitStiffness));
    }

    // 新增：进入受击后的Idle僵直状态
    private void EnterHitStiffness()
    {
        if (currentHealth > 0)
        {
            // 切换到Idle状态，保持hitStiffnessDuration（0.5s）僵直
            ChangeState(EnemyState.Idle);
            // 僵直结束后，恢复到受击前的状态（追击/巡逻）
            Invoke(nameof(ReturnToPreHitState), hitStiffnessDuration);
        }
    }

    public virtual void DieEnter()
    {
        // 死亡时中断所有攻击相关操作
        CancelInvoke("OnAttackHit");
        CancelInvoke("FinishAttack");
        CancelInvoke(nameof(EnterHitStiffness));
        CancelInvoke(nameof(ReturnToPreHitState));
        isAttacking = false;
        ableToMove = false;
        Debug.Log($"[DieEnter] 怪物死亡准备回收，当前activeEnemyCount={poolManager?.activeEnemyCount}");
        if (poolManager != null)
        {
            poolManager.ReturnEnemyToPool(gameObject.transform.parent.gameObject);
            Debug.Log($"[DieEnter] 怪物已回收到对象池，activeEnemyCount={poolManager.activeEnemyCount}");
            // 延迟2秒后更新计数
            poolManager.Invoke("UpdateActiveEnemyCount", 2f);
            poolManager.Invoke("LogActiveEnemyCount", 2f);
        }
    }

    public virtual void DieUpdate() { }

    public virtual void DieExit() { }

    #endregion

    public void ChangeState(EnemyState newState)
    {
        // 退出当前状态
        switch (currentState)
        {
            case EnemyState.Idle: IdleExit(); break;
            case EnemyState.Patrol: PatrolExit(); break;
            case EnemyState.Pursuit: PursuitExit(); break;
            case EnemyState.Attack: AttackExit(); break;
            case EnemyState.GetHit: GetHitExit(); break;
            case EnemyState.Die: DieExit(); break;
        }

        // 更新当前状态
        currentState = newState;

        // 进入新状态
        switch (newState)
        {
            case EnemyState.Idle: IdleEnter(); break;
            case EnemyState.Patrol: PatrolEnter(); break;
            case EnemyState.Pursuit: PursuitEnter(); break;
            case EnemyState.Attack: AttackEnter(); break;
            case EnemyState.GetHit: GetHitEnter(); break;
            case EnemyState.Die: DieEnter(); break;
        }
    }

    public virtual void FindPlayer(GameObject player)
    {
        this.player = player;
        ChangeState(EnemyState.Pursuit);
    }

    public virtual void LosePlayer()
    {
        player = null;
        ChangeState(EnemyState.Patrol);
    }

    // 可视化攻击范围（调试用）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    public virtual void GetHit(float damage)
    {
        Debug.Log($"[GetHit] 受击调用: 当前状态={currentState}, 伤害={damage}, 当前血量={currentHealth}");
        healthBar.value = currentHealth / maxHealth;
        hpText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
        GameObject dmgNum = Instantiate(damageNumberPrefab, damageNumberPoint.position, Quaternion.identity, healthBar.transform.parent);
        dmgNum.transform.localScale = damageNumberPoint.localScale;
        dmgNum.GetComponent<Text>().text = damage.ToString();
        if (currentState != EnemyState.Die)
        {
            // 清除之前的所有延迟调用（避免状态冲突）
            CancelInvoke(nameof(ReturnToPatrol));
            CancelInvoke(nameof(ReturnToPreHitState));
            // 切换到受击状态
            ChangeState(EnemyState.GetHit);
            // 扣除生命值
            currentHealth -= damage;
            Debug.Log($"[GetHit] 敌人受击！剩余生命值：{currentHealth}");
            // 生命值归零时切换到死亡状态
            if (currentHealth <= 0)
            {
                Debug.Log($"[GetHit] 生命值<=0，切换到Die，activeEnemyCount={poolManager?.activeEnemyCount}");
                ChangeState(EnemyState.Die);
                Debug.Log("[GetHit] 敌人死亡！");
                animator.SetTrigger("Dead");
                //Destroy(healthBar.gameObject);
                //Destroy(hpText.gameObject);
                Invoke("ResetHealth", 5f);
            }
        }
    }
    private void ReturnToPool()
    {
        if (poolManager != null)
        {
            poolManager.ReturnEnemyToPool(gameObject);
        }
    }
    private void ResetHealth()
    {
        currentHealth = maxHealth;
        healthBar.value = currentHealth / maxHealth;
        hpText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
    }
}