using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraIn : MonoBehaviour
{
    public GameObject trainsHead;
    public Vector3 deviation; // 偏移量
    // Start is called before the first frame update
    void Start()
    {
        deviation = transform.position - trainsHead.transform.position; // 初始物体与相机的偏移量=相机的位置 - 移动物体的偏移量
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
