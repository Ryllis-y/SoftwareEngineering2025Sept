using UnityEngine;
// 添加到Player_0对象上
public class AnimationEventForwarder : MonoBehaviour
{
    public GameObject targetObject;
    void Start()
    {
        targetObject = GameObject.FindWithTag("PlayerTag");
    }

    // 动画事件调用的函数
    public void OnAnimationEvent()
    {


        if (targetObject != null)
        {
            // 调用PlayerObject上的函数
            targetObject.SendMessage("PlayerMove", SendMessageOptions.DontRequireReceiver);
            targetObject.SendMessage("PlayerMoveFixed", SendMessageOptions.DontRequireReceiver);
            targetObject.SendMessage("PlayerJump", SendMessageOptions.DontRequireReceiver);
            targetObject.SendMessage("PlayerAttack", SendMessageOptions.DontRequireReceiver);
            targetObject.SendMessage("StartDash", SendMessageOptions.DontRequireReceiver);
            targetObject.SendMessage("EndDash", SendMessageOptions.DontRequireReceiver);
            targetObject.SendMessage("CheckGround", SendMessageOptions.DontRequireReceiver);
            //targetObject.SendMessage("PlayerMove", SendMessageOptions.DontRequireReceiver);
            //targetObject.SendMessage("PlayerMove", SendMessageOptions.DontRequireReceiver);

        }
    }
}