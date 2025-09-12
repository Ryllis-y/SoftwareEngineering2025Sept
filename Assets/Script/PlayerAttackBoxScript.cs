using UnityEngine;

public class PlayerAttackBox : MonoBehaviour
{
    public float damage = 10f;
    public float destroyTime = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyTag"))
        {
            EnemyBase enemy = collision.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                //enemy.TakeDamage(damage);
                enemy.GetHit(damage);
                Debug.Log("Enemy hit! Damage: " + damage);
            }
            else
            {
                Debug.LogError("碰撞的对象没有EnemyBase组件！");
            }
        }
    }
}
