// SpawnAreaManager.cs
using UnityEngine;

public class SpawnAreaManager : MonoBehaviour
{
    public void OnDrawGizmos()
    {
        // 在场景中可视化生成区域
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(20f, 0.1f, 15f));

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(20f, 0.1f, 15f));
    }
}