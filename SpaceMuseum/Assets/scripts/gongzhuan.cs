using UnityEngine;

public class gongzhuan : MonoBehaviour
{
    [Tooltip("公转速度（度/秒）")]
    public float orbitSpeed = 10f;

    [Tooltip("轨道半径")]
    public float orbitRadius = 8f;

    [Tooltip("拖拽地球模型到这里")]
    public Transform earthTransform;

    [Header("轨道倾斜设置")]
    [Tooltip("轨道倾斜角度（度），0为垂直轨道，90为水平轨道")]
    [Range(0, 90)] public float tiltAngle = 0f;

    [Tooltip("轨道在水平面上的旋转方向（0-360度）")]
    [Range(0, 360)] public float orbitRotation = 0f;

    private Vector3 orbitAxis; // 实际用于旋转的轴（根据倾斜角计算）

    void Start()
    {
        // 校验地球引用
        if (earthTransform == null)
        {
            Debug.LogError("请给" + gameObject.name + "绑定地球模型！");
            return;
        }

        // 计算倾斜的轨道轴
        CalculateOrbitAxis();

        // 初始化位置到倾斜轨道上
        SetInitialPosition();
    }

    // 计算倾斜的轨道轴
    void CalculateOrbitAxis()
    {
        // 先将角度转换为弧度
        float tiltRadians = tiltAngle * Mathf.Deg2Rad;
        float rotationRadians = orbitRotation * Mathf.Deg2Rad;

        // 根据倾斜角和水平旋转角度计算轨道轴
        // 核心公式：通过球面坐标转换生成倾斜轴
        orbitAxis.x = Mathf.Sin(tiltRadians) * Mathf.Sin(rotationRadians);
        orbitAxis.y = Mathf.Cos(tiltRadians); // Y分量控制垂直倾斜
        orbitAxis.z = Mathf.Sin(tiltRadians) * Mathf.Cos(rotationRadians);

        // 标准化向量（确保旋转速度一致）
        orbitAxis = orbitAxis.normalized;
    }

    // 设置初始位置到倾斜轨道上
    void SetInitialPosition()
    {
        // 计算轨道上的初始点（垂直于轨道轴的方向）
        Vector3 initialDirection = Vector3.Cross(orbitAxis, Vector3.up).normalized;
        if (initialDirection.sqrMagnitude < 0.01f)
        {
            // 特殊情况：当轨道轴接近Y轴时，用X轴作为初始方向
            initialDirection = Vector3.Cross(orbitAxis, Vector3.right).normalized;
        }

        // 设置初始位置
        transform.position = earthTransform.position + initialDirection * orbitRadius;
    }

    void Update()
    {
        if (earthTransform == null) return;

        // 绕倾斜轴公转
        transform.RotateAround(
            earthTransform.position,
            orbitAxis, // 使用计算好的倾斜轴
            orbitSpeed * Time.deltaTime
        );
    }
}

