using System.Numerics;
using UnityEngine;

public class BackGroundLoop : MonoBehaviour
{
    public GameObject mainCamera;
    public float mapWidth;
    public float mapNumber = 2;
    public float totalWidth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (this.GetComponent<SpriteRenderer>() != null)
            mapWidth = this.GetComponent<SpriteRenderer>().bounds.size.x;
        // mapNumber = Mathf.Ceil((mainCamera.GetComponent<Camera>().orthographicSize * 2 * Screen.width / Screen.height) / mapWidth) + 1;
        totalWidth = mapWidth * mapNumber;
    }

    // Update is called once per frame
    void Update()
    {
        UnityEngine.Vector3 tmpPosition = this.transform.position;
        if (mainCamera.transform.position.x > this.transform.position.x + totalWidth / 2)
        {
            tmpPosition.x += totalWidth;
            this.transform.position = tmpPosition;
        }
        else if (mainCamera.transform.position.x < this.transform.position.x - totalWidth / 2)
        {
            tmpPosition.x -= totalWidth;
            this.transform.position = tmpPosition;
        }

    }
}
