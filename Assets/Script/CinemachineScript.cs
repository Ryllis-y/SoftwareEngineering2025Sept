using UnityEngine;
/// <summary>
/// 制造移动视差效果
/// </summary>
public class CinemachineScript : MonoBehaviour
{
    public Transform background;
    public Vector3 lastPosition;
    public Vector3 offsetSpeed;


    public Transform backgroundTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastPosition = this.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 deltaMovement = this.transform.position - lastPosition;
        backgroundTransform.position += new Vector3(deltaMovement.x * offsetSpeed.x, deltaMovement.y * offsetSpeed.y, 0);
        lastPosition = this.transform.position;
    }
}
