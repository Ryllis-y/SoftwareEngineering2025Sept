using UnityEngine;

public class EnemyGetHurtNum : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, 0.7f); // 0.7秒后销毁该游戏对象
    }

}
