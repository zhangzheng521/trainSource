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
    // �����������λ����
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

    // վ���ź�����
    public StationDriverJs GetStationDriverJs()
    {
        return stationDriverJs;
    }
    // ����������Ϣ
    public TrainCJs GetTrainCJs()
    {
        return trainCJs;
    }
    // �Ƿ���������
    private bool isConnecting = true;

   
    // ʵ�ֿͻ���socket���ӡ���������
    private void ClientConnect()
    {
        int _port = Convert.ToInt32(inputPort);             //��ȡ�˿ں�
        string _ip = inputIp;                               //��ȡip��ַ
        //�����ͻ���Socket�����Զ��ip�Ͷ˿ں�
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
                    Debug.Log("���ӳɹ� , " + " ip = " + ip + " port = " + _port);
                    isConnecting = false;
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"����ʧ�ܣ�������������.  ԭ��:{e.Message}");
                    CloseSocket();
                    socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Thread.Sleep(reconnectionInterval);//�ȴ�5s��ȥ����
                }
            }
            //�����µ��̣߳���ͣ�Ľ��շ�������������Ϣ
            isReceiveMessage = true;
            Thread receiveMessageTread = new (Received);
            receiveMessageTread.IsBackground = true;
            receiveMessageTread.Start();
            
            Debug.Log("��ʼ���շ�������Ϣ!");

            /*Thread s_thread = new Thread(SendMessage);          //�����µ��̣߳���ͣ�ĸ�������������Ϣ
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
                //ʵ�ʽ��յ�����Ч�ֽ���
                int len = socketSend.Receive(buffer);
                if (len == 0)
                {
                    break;
                }

                recMes = Encoding.UTF8.GetString(buffer, 0, len);

                Debug.Log("�ͻ��˽��յ������� �� " + recMes);

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
                    Debug.Log($"json��������: {eJson.Message}" );
                }
                //call(recMes);
            }
            catch (Exception ex)
            {
                isReceiveMessage = false;
                CloseSocket();
                Debug.LogWarning($"��Ϣ�����쳣: {ex.Message}");
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
            PositionCard = "1234", //������ǰ���¶�ȡ����Ӧ�������
            Zone = "T1003",//������ǰ��������
            PositionCardB = "5678", //������ǰ���¶�ȡ����Ӧ�������
            ZoneB = "T1003",//������ǰ��������
            TrianRunningStateEnum = "Forward"//����״̬��Forward -ǰ�� Back_off -���� Stop-ֹͣ���ֶ�ģʽ����Ч��
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
            Debug.Log("������Ϣʧ��!");
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
                socketSend.Shutdown(SocketShutdown.Both);    //����Socket�ķ��ͺͽ��չ���
                socketSend.Close();                          //�ر�Socket���Ӳ��ͷ����������Դ
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
    // �����źŵ�����
    [Serializable]
    public class StationDriverJs
    {
        public string Type;
        public string StationName;
        public List<string> XHJ;//�źŻ�����  �źŻ�����/״̬ B-�� W-�� R-�� G-�� Y-�� RY-��� GY-�̻� GZ-�������
        public List<string> DC;//�������� ��������/״̬ D-��λ F-��λ
    }
    // ���͸���������
    [Serializable]
    public class TrainRJs
    {
        public string Type;
        public string Train;
        public int Speed;
        public string PositionCard;//������ǰ���¶�ȡ����Ӧ�������
        public string Zone;//������ǰ��������
        public string PositionCardB;//������ǰ���¶�ȡ����Ӧ�������
        public string ZoneB;//������ǰ��������
        public string TrianRunningStateEnum;//����״̬��Forward -ǰ�� Back_off -���� Stop-ֹͣ���ֶ�ģʽ����Ч��
    }
    // ���յĻ�������
    [Serializable]
    public class TrainCJs
    {
        public string Type;
        public string Train;//������� T001-T006
        public string RunMode;//����ģʽ��AM-�Զ� BY-�ֶ� AR-�۷�
        public int Speed;//�����ٶ�
        public string MovingCard;//�ƶ�Ŀ��㣬���Զ�/�۷�ģʽ����Ч����ֵʱMovingZone��Ч��
        public string MovingZone;//�ƶ�Ŀ������
        public string TrainDriveEnum;//��ʻ�� I-1�� II-2��
        public string TrainRunningStateEnum;//����״̬��Forward -ǰ�� Back_off -���� Stop-ֹͣ���ֶ�ģʽ����Ч��
    }

   







}
