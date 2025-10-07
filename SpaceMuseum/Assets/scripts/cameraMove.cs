using UnityEngine;

public class cameraMove : MonoBehaviour
{
    // 鼠标控制视角的灵敏度
    public float mouseXSpeed = 1f;
    public float mouseYSpeed = 1f;

    private void Update()
    {
        // 获取鼠标的水平和垂直移动量
        float mouseX = Input.GetAxis("Mouse X") * mouseXSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * mouseYSpeed;

        // 绕Y轴（垂直轴）旋转，实现水平视角转动
        transform.Rotate(Vector3.up, mouseX);
        // 绕X轴（水平轴）旋转，实现垂直视角转动
        transform.Rotate(Vector3.right, -mouseY);
    }
}