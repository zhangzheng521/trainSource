using System.Net.Sockets;
using UnityEngine;
using static NetTcpClient;


public class Lights : MonoBehaviour
{
    public string lightType;
    public float flashDelay = 0.5f;

    //private static NetTcpClient netTcpObject;
    private Transform[] father;

    private ColorLights[] colorSignalArr;
    private float DataTime=0f;
    private static NetTcpClient.StationDriverJs stationDriverJs;


    // ��ǰ��λ����
    private string lightName;
    // ���տͻ�����Ϣ
    private void ReceiveMsg()
    {
        stationDriverJs = NetTcpClient.stationDriverJs;

        if (stationDriverJs.XHJ!=null)
        {
            foreach(string xhj in stationDriverJs.XHJ)
            {
                string[] strArr = xhj.Split("/");
                string aLightName = strArr[0];
                if (lightName == aLightName)
                {
                    string aColor = strArr[1];
                    SetDeviceLight(aColor);
                    //Debug.Log($"��λ{lightName}�յ���Ϣ:" + JsonUtility.ToJson(stationDriverJs));
                }
            }
            
        }
        

    }
    private void OnEnable()
    {
        father = GetComponentsInChildren<Transform>();
        //netTcpObject = GameObject.Find("NetTcpClient").GetComponent<NetTcpClient>();
        lightName = gameObject.name;
    }
    private void Start()
    {
        // Ĭ�������
        SetLight("red", 1);
    }

    private void Update()
    {
        ReceiveMsg();
        if (colorSignalArr != null)
        {
            lightDirect();
        }
        // ������������
        KeyBord();
    }
    private void SetDeviceLight(string color)
    {
        if (lightType == "two")
        {
            switch (color)
            {
                case "R":
                    color = "red";
                    SetLight(color, 1);
                    break;
                case "G":
                    color = "green";
                    SetLight(color, 2);
                    break;
                case "Y":
                    color = "yellow";
                    SetLight(color, 2);
                    break;
                default:
                    return;
            }
        }
    }
    // ��������
    private void SetLight(string color, int position, bool flashing = false)
    {
        ColorLights colorLights = new ColorLights();
        colorLights.position = position;
        colorLights.color = color;
        colorLights.flashing = flashing;
        colorSignalArr = new ColorLights[1] { colorLights };
    }
    private void KeyBord()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetLight("red", 2);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetLight("yellow", 2);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SetLight("blue", 1);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            SetLight("green", 1);
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SetLight("white", 1);
        }
    }
    

    private void lightDirect()
    {
        // �źŵ�ȫ������
        for (int i = 1; i < father.Length; i++)
        {
            father[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

        for (var i=0; i<colorSignalArr.Length; i++)
        {
            ColorLights colorSignal = colorSignalArr[i];
            int position = colorSignal.position;
            // ��������������������
            if (position < 1 || position >= father.Length)
            {
                return;
            }
            string color = colorSignal.color;
            bool flashing = colorSignal.flashing;

            MeshRenderer render = father[position].gameObject.GetComponent<MeshRenderer>();
            // ��ʾ����
            if(flashing)
            {
                if (flashInterval(flashDelay))
                {
                    render.enabled = true;
                }
                else
                {
                    render.enabled = false;
                }
            }
            else
            {
                render.enabled = true;
            }
            
            render.material.EnableKeyword("_EMISSION");
            switch (color)
            {
                case "blue":
                    render.material.SetColor("_EmissionColor", Color.blue);
                    break;
                case "red":
                    render.material.SetColor("_EmissionColor", Color.red);
                    break;
                case "yellow":
                    render.material.SetColor("_EmissionColor", Color.yellow);
                    break;
                case "green":
                    render.material.SetColor("_EmissionColor", Color.green);
                    break;
                case "white":
                    render.material.SetColor("_EmissionColor", Color.white);
                    break;
                default:
                    render.material.SetColor("_EmissionColor", Color.black);
                    break;
            }

        }
    }
    // ��˸���
    private bool flashInterval(float delay)
    {
        DataTime += Time.deltaTime;
        if (DataTime>0&& DataTime<= delay)
        {
            return true;
        }
        else if(DataTime> delay && DataTime<= delay*2)
        {
            return false;
        }
        else
        {
            DataTime = 0f;
            return false;
        }
    }

    public class ColorLights
    {
        public int position;
        public string color;
        public bool flashing;
    }

}
