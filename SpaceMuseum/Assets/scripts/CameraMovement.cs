using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("移动参数")]
    [Tooltip("移动速度")]
    public float moveSpeed = 5f;
    [Tooltip("是否忽略Y轴移动（保持高度不变）")]
    public bool ignoreYAxis = true;

    private void Update()
    {
        // 获取输入（上下左右或WSAD）
        float horizontal = Input.GetAxis("Horizontal"); // 左右（A/D或左/右方向键）
        float vertical = Input.GetAxis("Vertical");     // 前后（W/S或上/下方向键）

        // 标准化输入向量（避免斜向移动速度过快）
        Vector3 inputDir = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDir.magnitude > 0.1f) // 有有效输入时才移动
        {
            // 获取视角的前方和右方方向（忽略Y轴旋转，避免上下看影响前后移动）
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            // 忽略Y轴，确保移动在水平面上（如果需要保持高度）
            if (ignoreYAxis)
            {
                cameraForward.y = 0;
                cameraRight.y = 0;
                // 重新标准化，避免斜向移动时方向向量长度变化
                cameraForward.Normalize();
                cameraRight.Normalize();
            }

            // 计算最终移动方向（基于视角的前/右方向）
            Vector3 moveDir = cameraForward * vertical + cameraRight * horizontal;

            // 移动对象
            transform.Translate(moveDir * moveSpeed * Time.deltaTime);
        }
    }
}