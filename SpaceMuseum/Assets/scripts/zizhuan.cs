using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zizhuan : MonoBehaviour
{
    [Tooltip("������ת�ٶȣ���/�룩")]
    public float rotationSpeed = 15.0f; // Ĭ���ٶ�15��/��

    void Update()
    {
        // ������Y�ᣨ����ָ������ת
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}