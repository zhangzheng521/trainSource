using Unity.VisualScripting;
using UnityEngine;
using static NetTcpClient;

public class Turnout : MonoBehaviour
{

    private string turnoutName;// 道岔名
    //private static NetTcpClient netTcpObject;
    private static NetTcpClient.StationDriverJs stationDriverJs;

    private void ReceiveMsg()
    {
        stationDriverJs = NetTcpClient.stationDriverJs;
        if (stationDriverJs.DC != null)
        {
            foreach (string dc in stationDriverJs.DC)
            {
                string[] strArr = dc.Split('/');
                string aDcName = strArr[0];
                //Debug.Log($"aDcName:{aDcName}");
                if (aDcName == turnoutName)
                {
                    string aDcState = strArr[1];
                    
                    SetStateWay(aDcState);
                }
            }
        }
    }
    private Renderer renderer0;
    private Renderer renderer1;
    private void OnEnable()
    {
        turnoutName = gameObject.name;
        //netTcpObject = GameObject.Find("NetTcpClient").GetComponent<NetTcpClient>();
        renderer0 = transform.GetChild(0).GetComponent<Renderer>();
        renderer1 = transform.GetChild(1).GetComponent<Renderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        // 默认道岔总定
        SetStateWay("D");
    }

    // Update is called once per frame
    void Update()
    {
        ReceiveMsg();
        // 道岔测试
        KeyBord();
    }
    private void KeyBord()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SetStateWay("D");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            SetStateWay("F");
        }
    }
    // 0是道岔总定 1是道岔总反
    private void SetStateWay(string state)
    {
        int stateInt;
        switch (state)
        {
            case "D":
                stateInt = 0;
                break;
            case "F":
                stateInt = 1;
                break;
            default:
                stateInt = 0;
                break;
        }
        renderer0.enabled = false;
        renderer1.enabled = false;
        switch (stateInt)
        {
            case 0:
                renderer0.enabled = true;
                break;
            case 1:
                renderer1.enabled = true;
                break;
        }
    }
}
