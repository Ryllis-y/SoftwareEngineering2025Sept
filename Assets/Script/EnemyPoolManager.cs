// EnemyPoolManager.cs
using UnityEngine;
using System.Collections.Generic;

public class EnemyPoolManager : MonoBehaviour
{
    [Header("敌人设置")]
    public GameObject enemyPrefab; // 敌人预制体
    public int poolSize = 10;      // 对象池大小（只需1个）
    public int maxActiveEnemies = 1; // 最大活跃敌人数

    [Header("生成区域设置")]
    public Vector3 fixedSpawnPosition1 = new Vector3(0, 5, 0); // 固定生成位置
    public Vector3 fixedSpawnPosition2 = new Vector3(-100, 5, 0); // 第二个固定生成位置

    private List<GameObject> enemyPool;
    [Header("调试信息")]
    [Tooltip("当前场上活跃怪物数量")]
    public int activeEnemyCount = 0;

    private float respawnTimer = 0f;
    private bool waitingForRespawn = false;

    // 用于Invoke延迟打印计数
    public void LogActiveEnemyCount()
    {
        Debug.Log($"[延迟2秒] activeEnemyCount(遍历池)={activeEnemyCount}");
    }

    void Start()
    {
        InitializePool();
        TrySpawnEnemyAtFixedPosition();
    }

    void Update()
    {
        //UpdateActiveEnemyCount();
        if (activeEnemyCount < maxActiveEnemies)
        {
            if (!waitingForRespawn)
            {
                waitingForRespawn = true;
                respawnTimer = 0f;
                Debug.Log("怪物死亡，5秒后尝试生成新怪物");
            }
        }
        if (waitingForRespawn)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= 5f && activeEnemyCount < maxActiveEnemies)
            {
                Debug.Log($"5秒计时结束，尝试生成怪物，当前活跃怪物数: {activeEnemyCount}");
                TrySpawnEnemyAtFixedPosition();
                waitingForRespawn = false;
            }
            if (activeEnemyCount >= maxActiveEnemies)
            {
                waitingForRespawn = false;
            }
        }
    }

    // 初始化对象池
    void InitializePool()
    {
        enemyPool = new List<GameObject>();

        for (int i = 0; i < poolSize - 1; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            enemyPool.Add(enemy);

            // 设置敌人名称以便识别
            enemy.name = "Enemy_" + i;
        }

        Debug.Log($"敌人对象池初始化完成，大小: {poolSize}");
    }

    // 在固定位置尝试生成敌人
    public void TrySpawnEnemyAtFixedPosition()
    {
        SpawnEnemyAtPosition(fixedSpawnPosition1);
        //SpawnEnemyAtPosition(fixedSpawnPosition2);
    }

    // 不再需要查找随机生成位置

    // 在指定位置生成敌人
    void SpawnEnemyAtPosition(Vector3 position)
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy)
            {
                // 设置位置和旋转
                enemy.transform.position = position;
                enemy.transform.rotation = Quaternion.identity;

                // 激活敌人
                enemy.SetActive(true);

                // 重置敌人状态
                ResetEnemyState(enemy);

                activeEnemyCount++;
                Debug.Log($"生成敌人于位置: {position}, 活跃敌人: {activeEnemyCount}");
                return;
            }
        }

        Debug.LogWarning("对象池已满，无法生成新敌人");
    }

    // 重置敌人状态
    void ResetEnemyState(GameObject enemy)
    {
        // 获取敌人基础组件并重置
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            // 重置到巡逻状态
            enemyBase.ChangeState(EnemyState.Patrol);

            // 重置生命值
            enemyBase.currentHealth = enemyBase.maxHealth;
        }

        // 重置其他组件状态...
    }

    // 回收敌人到对象池
    public void ReturnEnemyToPool(GameObject enemy)
    {
        // 回收到队列尾部，实现先进先出
        if (enemyPool.Contains(enemy))
        {
            enemy.SetActive(false);
            enemyPool.Remove(enemy);
            enemyPool.Add(enemy); // 加到队尾
            activeEnemyCount--;
            Debug.Log($"敌人回收到对象池队尾，activeEnemyCount={activeEnemyCount}");
        }
    }

    // 更新活跃敌人计数
    // 不再需要遍历池子统计activeEnemyCount

    // 获取活跃敌人列表（用于调试）
    public List<GameObject> GetActiveEnemies()
    {
        List<GameObject> activeEnemies = new List<GameObject>();
        foreach (GameObject enemy in enemyPool)
        {
            if (enemy.activeInHierarchy)
            {
                activeEnemies.Add(enemy);
            }
        }
        return activeEnemies;
    }

    // 可视化生成点（调试用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(fixedSpawnPosition1, 0.5f);
        //Gizmos.DrawSphere(fixedSpawnPosition2, 0.5f);
    }
}
