using UnityEngine;

public class MirrorReflection : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;     // XR Camera / Main Camera
    public Transform mirrorSurface;    // 镜子平面
    public Camera mirrorCamera;

    [Header("Options")]
    public bool updateRotation = true;
    public bool updatePosition = true;

    private void LateUpdate()
    {
        if (playerCamera == null || mirrorSurface == null || mirrorCamera == null)
            return;

        Vector3 mirrorPos = mirrorSurface.position;
        Vector3 mirrorNormal = mirrorSurface.forward;

        // 玩家相机相对镜面的向量
        Vector3 toPlayer = playerCamera.position - mirrorPos;

        // 镜像位置：沿镜面法线反射
        Vector3 reflectedPosition = Vector3.Reflect(toPlayer, mirrorNormal) + mirrorPos;

        if (updatePosition)
            mirrorCamera.transform.position = reflectedPosition;

        if (updateRotation)
        {
            Vector3 reflectedForward = Vector3.Reflect(playerCamera.forward, mirrorNormal);
            Vector3 reflectedUp = Vector3.Reflect(playerCamera.up, mirrorNormal);

            mirrorCamera.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);
        }
    }
}