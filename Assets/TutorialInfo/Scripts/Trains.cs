using System.Collections.Generic;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Controllers;
using UnityEngine;

public class Trains : MonoBehaviour
{
    // ������
    public float limitSpeed = 10f;
    // �𳵼��ٶ�
    public float acceleration = 4f;
    // ��С�����ٶ�
    public float minSlowSpeed = 1f;
    // ��С���پ���
    public float nearSlowDistance = 50f;
    // ����ʼλ��
    public float trainPosition = 0f;
    // ��ͣվ��
    public string initTrainStopStation;
    // ѡ�����ʻ����
    public MovementDirection movementDirection = MovementDirection.Forward;

    // ����ʼ����·����
    public List<GameObject> splineObjList;



    // �𳵳���
    public float speed = 0f;


    // ����ʼ����·
    private List<CurvySpline> curvySplineObjList = new();
    // ��ѡ�����·
    private Dictionary<string, string> selectCurvySplines = new();
    // ��ͣվ��
    private Vector3 trainStopStationPosition;
    // �𳵾���վ�����
    private float trainStopToStationDistance;
    // ���Ƿ���ü���״̬
    private bool isAccelerationState = false;


    private bool isStart = false;

    private void Awake()
    {
        // ������·
        SetCurvySplines("CurvySpline1.CP0005", "CurvySpline3");
        SetCurvySplines("CurvySpline3.CP0002", "CurvySpline4");
        SetCurvySplines("CurvySpline4.CP0005", "CurvySpline3");
        //��ʼ��ͣ����
        SetTrainStopStation();
    }
    private void Start()
    {
        isStart = true;
    }
    // �༭��ģʽ��ÿִ֡��
    private void OnDrawGizmos()
    {
        if (isStart)
        {
            return;
        }
        EveyInitCurvySplines();
    }

    private void Update()
    {
        EveyInitCurvySplines();
        if (speed < 0)
        {
            speed = 0;
        }
        EverySetSpeed(speed);
        EverySetTrainDirection();
        // ʵʱ����𳵵�Ŀ������
        EveryGetTrainToTargetDistance();
        // �𳵿�ʼ��ʻ
        EveryRunTrain();
    }

    private TrainBox trainHeadBox;// ��ͷ
    private TrainBox trainTailBox;// ��β
    private TrainBox[] myTrainBoxs;
    private void GetTrainHead()
    {
        myTrainBoxs = GetComponentsInChildren<TrainBox>();
        for (int i = 0; i < myTrainBoxs.Length; i++)
        {
            if (myTrainBoxs[i].isHeadTrainBox == true)
            {
                trainHeadBox = myTrainBoxs[i];
            }
            if (myTrainBoxs[i].isTailTrainBox == true)
            {
                trainTailBox = myTrainBoxs[i];
            }
        }
    }
    private void EverySetSpeed(float speed)
    {
        if (trainHeadBox != null)
        {
            trainHeadBox.SetSpeed(speed);
        }
    }
    private void EverySetTrainDirection()
    {
        if (trainHeadBox != null)
        {
            trainHeadBox.SetHeadTrainDirection(movementDirection);
        }
    }

    private bool isInitCurySplines = false;
    public void EveyInitCurvySplines()
    {
        if (isInitCurySplines)
        {
            return;
        }
        for (int i = 0; i < splineObjList.Count; i++)
        {
            GameObject splineObj = splineObjList[i];
            CurvySpline curvySpline = splineObj.GetComponent<CurvySpline>();
            if (curvySpline == null)
            {
                return;
            }
            curvySplineObjList.Add(curvySpline);
        }
        isInitCurySplines = true;
        // ��ȡtrainBox
        GetTrainHead();
    }

    public List<CurvySpline> GetCurvySplineObjList()
    {
        return curvySplineObjList;
    }

    private int GetObjName(string name)
    {
        for (int i = 0; i < splineObjList.Count; i++)
        {
            if (splineObjList[i].gameObject.name == name)
            {
                return i;
            }
        }
        return -1;
    }

    // ����𳵵�Ŀ������
    private void EveryGetTrainToTargetDistance()
    {
        if (trainStopStationPosition == Vector3.zero)
        {
            return;
        }
        if (movementDirection == MovementDirection.Forward)
        {
            GetTrainHeadToTargetDistance();
        }
        if (movementDirection == MovementDirection.Backward)
        {
            GetTrainTailToTargetDistance();
        }
        SlowDownSpeed();
        //Debug.Log("trainStopToStationDistance:" + trainStopToStationDistance);
    }
    private float recordSpeed;// ��¼��ʼ����ʱ�����һ���ٶ�
    private float recordDeltaSpeed;// ��¼��ʼ����ʱ�����һ��ÿ���پ��뵥λ���ٶ�
    private float recordDistance = 0f;// ��¼ÿ���ٵ�λ����
    // ���㵽վ���ٵ�0
    private void SlowDownSpeed()
    {
        if (trainStopToStationDistance > nearSlowDistance) 
        {
            // ��¼����ǰ���ٶ�ֵ
            recordSpeed = speed;
            // ��¼ÿ�������ٵ��ٶ�ֵ
            recordDeltaSpeed = recordSpeed / nearSlowDistance;
            return;
        }
        else
        {
            // ������ٷ�Χʱ�����ü���
            isAccelerationState = true;
        }
        if(speed> minSlowSpeed)
        {
            if (recordDistance < 1f)
            {
                recordDistance += Time.deltaTime * speed;
            }
            else
            {
                recordDistance = 0f;
                speed -= recordDeltaSpeed;
            }
        }
    }

    // ���㳵ͷ��Ŀ������
    private void GetTrainHeadToTargetDistance()
    {
        if (trainStopStationPosition == Vector3.zero)
        {
            return;
        }
        if (trainHeadBox == null)
        {
            Debug.LogError("�����û�ͷ!");
            return;
        }
        Vector3 trainHeadPosition = trainHeadBox.GetTrainHeadPosition();
        if (trainHeadPosition == Vector3.zero)
        {
            return;
        }
        trainStopToStationDistance = Vector3.Distance(trainStopStationPosition, trainHeadPosition);
    }
    // ���㳵β��Ŀ������
    private void GetTrainTailToTargetDistance()
    {
        if (trainStopStationPosition == Vector3.zero)
        {
            return;
        }
        if (trainTailBox == null)
        {
            Debug.LogError("�����û�β!");
            return;
        }
        Vector3 trainTailPosition = trainTailBox.GetTrainTailPosition();
        if (trainTailPosition == Vector3.zero)
        {
            return;
        }
        trainStopToStationDistance = Vector3.Distance(trainStopStationPosition, trainTailPosition);
    }

    public Dictionary<string, int> GetCurvyWay()
    {
        var dic = new Dictionary<string, int>();
        foreach (var item in selectCurvySplines)
        {
            string from = item.Key;
            string to = item.Value;
            dic[from] = GetObjName(to);
        }
        return dic;
    }
    // ������·
    public void SetCurvySplines(string from, string to)
    {
        selectCurvySplines[from] = to;
    }

    
    // �𳵿�ʼ��ʻ
    public void EveryRunTrain()
    {
        if (!isAccelerationState && speed < limitSpeed)
        {
            float currentFrameTime = Time.time;
            float v = acceleration * currentFrameTime/1000;
            speed += v;
        }
    }


    //���û�ͣվ�� ���ð���: trainStopStation2=CurvySpline1.CP0004
    public void SetTrainStopStation(string trainStopStation2 = "")
    {
        if (string.IsNullOrEmpty(initTrainStopStation))
        {
            // ����ǿ�ƻ�ֹͣ
            speed = 0;
            // ���ü���
            isAccelerationState = true;
            Debug.LogError("�����ó�ʼ��ͣ��վ��!");
            return;
        }
        if (!string.IsNullOrEmpty(trainStopStation2))
        {
            initTrainStopStation = trainStopStation2;
        }
        string src = initTrainStopStation.Replace(".", "/");
        GameObject gm = GameObject.Find(src);
        trainStopStationPosition = gm.transform.position;
    }
    // ���³�ʼ����·
    public void ReInitCurvySpline(string cuvrySpineName)
    {
        int index = GetObjName(cuvrySpineName);
        // TODO ������splineObjList˳��
        trainHeadBox.ReInitCurvySpline(index);
    }

    public void OnTrainMessage(TrainMessage trainMsg)
    {
        //Debug.Log($"�յ���Ϣ:{JsonUtility.ToJson(trainMsg)}");
        // ƥ��ͣվ��Ϣͣ��
        if (trainMsg.type == "wayPoint" && trainMsg.data == initTrainStopStation)
        {
            // ��ǰ��������Գ�ͷΪ׼
            if ((movementDirection == MovementDirection.Forward && trainMsg.isTrainHead) || (movementDirection == MovementDirection.Backward && trainMsg.isTrainTail))
            {
                speed = 0;// ͣ��
                // ���ü���
                isAccelerationState = true;
                // ���㳵ͷ��Ŀ������
                Debug.Log("initTrainStopStation:" + JsonUtility.ToJson(trainMsg));
            }

        }
    }
    

}


