using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraIn : MonoBehaviour
{
    public GameObject trainsHead;
    public Vector3 deviation; // ƫ����
    // Start is called before the first frame update
    void Start()
    {
        deviation = transform.position - trainsHead.transform.position; // ��ʼ�����������ƫ����=�����λ�� - �ƶ������ƫ����
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        transform.position = trainsHead.transform.position + deviation;
    }
}
