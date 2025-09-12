using UnityEngine;

public class TerritoryScript : MonoBehaviour
{
    public EnemyBase enemy;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "PlayerTag")
        {
            Debug.Log("玩家进入敌人视野");
            //enemy = GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.FindPlayer(collision.gameObject);
                Debug.Log("敌人可定位");
            }
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "PlayerTag")
        {
            Debug.Log("玩家离开敌人视野");
            if (enemy != null)
            {
                enemy.LosePlayer();
                Debug.Log("敌人失去定位");
            }
        }
    }
}
