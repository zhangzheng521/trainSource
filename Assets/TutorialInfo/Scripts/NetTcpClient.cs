using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

public class NetTcpClient : MonoBehaviour
{
    

    public int inputPort = 8091;
    public string inputIp = "127.0.0.1";
    // 重连间隔，单位毫秒
    public int reconnectionInterval = 5000;



    private Socket socketSend;
    private string recMes;

    private TypeClass typeClass=new ();
    public static StationDriverJs stationDriverJs=new ();
    public static TrainCJs trainCJs=new ();


    // Start is called before the first frame update
    void Start()
    {
        //ClickConnect();
        ClientConnect();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 站场信号驱动
    public StationDriverJs GetStationDriverJs()
    {
        return stationDriverJs;
    }
    // 机车控制信息
    public TrainCJs GetTrainCJs()
    {
        return trainCJs;
    }
    // 是否正在连接
    private bool isConnecting = true;

   
    // 实现客户端socket连接、断线重连
    private void ClientConnect()
    {
        int _port = Convert.ToInt32(inputPort);             //获取端口号
        string _ip = inputIp;                               //获取ip地址
        //创建客户端Socket，获得远程ip和端口号
        socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ip = IPAddress.Parse(_ip);
        IPEndPoint point = new(ip, _port);
        Thread conn_thread = new (() =>
        {
            while (isConnecting)
            {
                try
                {
                    socketSend.Connect(point);
                    Debug.Log("连接成功 , " + " ip = " + ip + " port = " + _port);
                    isConnecting = false;
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"连接失败，正在重试连接.  原因:{e.Message}");
                    CloseSocket();
                    socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Thread.Sleep(reconnectionInterval);//等待5s再去重连
                }
            }
            //开启新的线程，不停的接收服务器发来的消息
            isReceiveMessage = true;
            Thread receiveMessageTread = new (Received);
            receiveMessageTread.IsBackground = true;
            receiveMessageTread.Start();
            
            Debug.Log("开始接收服务器消息!");

            /*Thread s_thread = new Thread(SendMessage);          //开启新的线程，不停的给服务器发送消息
            s_thread.IsBackground = true;
            s_thread.Start();*/
        });
        conn_thread.IsBackground = true;
        conn_thread.Start();
    }

    private bool isReceiveMessage = true;
    public void Received()
    {
        while (isReceiveMessage)
        {
            try
            {
                byte[] buffer = new byte[1024 * 6];
                //实际接收到的有效字节数
                int len = socketSend.Receive(buffer);
                if (len == 0)
                {
                    break;
                }

                recMes = Encoding.UTF8.GetString(buffer, 0, len);

                Debug.Log("客户端接收到的数据 ： " + recMes);

                try
                {
                    typeClass = JsonUtility.FromJson<TypeClass>(recMes);
                    string Type = typeClass.Type;
                    switch (Type)
                    {
                        case "StationDriverJs":
                            stationDriverJs = JsonUtility.FromJson<StationDriverJs>(recMes);
                            break;
                        case "TrainCJs":
                            trainCJs = JsonUtility.FromJson<TrainCJs>(recMes);
                            break;
                    }
                    //Debug.Log("Type: "+ Type);
                }
                catch (Exception eJson)
                {
                    Debug.Log($"json解析错误: {eJson.Message}" );
                }
                //call(recMes);
            }
            catch (Exception ex)
            {
                isReceiveMessage = false;
                CloseSocket();
                Debug.LogWarning($"消息接收异常: {ex.Message}");
                Thread.Sleep(reconnectionInterval);
                isConnecting = true;
                ClientConnect();

            }
        }
    }

    private void SendJCJson()
    {
        TrainRJs trainRJs = new TrainRJs()
        {
            Type = " TrainRJs",
            Train = "T001",
            Speed = 100,
            PositionCard = "1234", //机车当前最新读取到的应当器编号
            Zone = "T1003",//机车当前所在区段
            PositionCardB = "5678", //机车当前最新读取到的应当器编号
            ZoneB = "T1003",//机车当前所在区段
            TrianRunningStateEnum = "Forward"//运行状态，Forward -前进 Back_off -后退 Stop-停止（手动模式下有效）
        };
        string json = JsonUtility.ToJson(trainRJs);
        SendMsg(json);

    }
    private void SendMsg(string msg)
    {
        try
        {
            Debug.Log("msg:"+ msg);
            byte[] buffer = new byte[1024 * 6];
            buffer = Encoding.UTF8.GetBytes(msg);
            socketSend.Send(buffer);
        }
        catch 
        {
            Debug.Log("发生消息失败!");
        }
    }


    private void OnDisable()
    {
        Debug.Log("begin OnDisable()");
        CloseSocket();
        Debug.Log("end OnDisable()");
    }

    private void CloseSocket()
    {
        if (socketSend!=null && socketSend.Connected)
        {
            try
            {
                socketSend.Shutdown(SocketShutdown.Both);    //禁用Socket的发送和接收功能
                socketSend.Close();                          //关闭Socket连接并释放所有相关资源
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }


    [Serializable]
    public class TypeClass
    {
        public string Type;
    }
    // 接收信号灯数据
    [Serializable]
    public class StationDriverJs
    {
        public string Type;
        public string StationName;
        public List<string> XHJ;//信号机驱动  信号机名称/状态 B-蓝 W-白 R-红 G-绿 Y-黄 RY-红黄 GY-绿黄 GZ-故障灭灯
        public List<string> DC;//道岔驱动 道岔名称/状态 D-定位 F-反位
    }
    // 发送给机车数据
    [Serializable]
    public class TrainRJs
    {
        public string Type;
        public string Train;
        public int Speed;
        public string PositionCard;//机车当前最新读取到的应当器编号
        public string Zone;//机车当前所在区段
        public string PositionCardB;//机车当前最新读取到的应当器编号
        public string ZoneB;//机车当前所在区段
        public string TrianRunningStateEnum;//运行状态，Forward -前进 Back_off -后退 Stop-停止（手动模式下有效）
    }
    // 接收的机车数据
    [Serializable]
    public class TrainCJs
    {
        public string Type;
        public string Train;//车辆编号 T001-T006
        public string RunMode;//控制模式：AM-自动 BY-手动 AR-折返
        public int Speed;//机车速度
        public string MovingCard;//移动目标点，（自动/折返模式下有效，空值时MovingZone有效）
        public string MovingZone;//移动目标区段
        public string TrainDriveEnum;//驾驶室 I-1端 II-2端
        public string TrainRunningStateEnum;//运行状态，Forward -前进 Back_off -后退 Stop-停止（手动模式下有效）
    }

   







}
