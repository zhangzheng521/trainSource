using System.Collections.Generic;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainBox : MonoBehaviour
{
    public bool isHeadTrainBox = false;// �Ƿ�ͷ
    public bool isTailTrainBox = false;// �Ƿ�β

    public float trainBoxPosition = 0f;

    // ����ʼ����·
    private static List<CurvySpline> curvySplineObjList;
    private static float speed = 10f;
    private static Trains trains;
    private static CurvySpline curvySpline;
    // �����ƶ�����
    private static MovementDirection moveDirection = MovementDirection.Forward;
    //private static CurvySpline curvySplineNext;
    // private static float headTrainDeltaVect = 0;// ��ͷλ��
    //private static bool isOver = false;

    private SplineController splineController;
    private bool isStart = false;

    // ��ʼ��Trains����
    private bool isInitTrains = false;
    private void EveryInitTrains()
    {
        if(isInitTrains)
        {
            return;
        }
        trains = transform.parent.GetComponent<Trains>();
        if(trains != null )
        {
            isInitTrains = true;
        }
        curvySplineObjList = trains.GetCurvySplineObjList();
    }
    // ��ʼ��Trains���Զ���
    private bool isInitTrainsTest = false;
    private void EveryInitTrainsTest()
    {
        if (isInitTrainsTest)
        {
            return;
        }
        trains = transform.parent.GetComponent<Trains>();
        if (trains != null)
        {
            isInitTrainsTest = true;
        }
        //curvySplineObjList = trains.GetCurvySplineObjList();
    }
    private bool isInited = false;
    private void EveryInitCurvySpline()
    {
        if (curvySplineObjList == null)
        {
            return;
        }
        if(curvySplineObjList.Count < 1)
        {
            return;
        }
        if (isInited)
        {
            return;
        }
        // ��ʼ��·
        curvySpline = curvySplineObjList[0];

        if(curvySpline==null)
        {
            return;
        }
        isInited = true;

        InitCurvySpline(curvySpline, speed, CurvyClamping.Clamp);
        // ����·�����¼�
        splineController.OnControlPointReached.AddListenerOnce(OnControlPointReached);
        // ��ͷ�����յ�ʱ����
        if (isHeadTrainBox)
        {
            splineController.OnEndReached.AddListenerOnce(OnTrainHeadEndReached);
        }
        // ��β�����յ�ʱ����
        if (isTailTrainBox)
        {
            splineController.OnEndReached.AddListenerOnce(OntrainTailEndReached);
        }
    }
    // ���³�ʼ����·
    public void ReInitCurvySpline(int index=0)
    {
        if (isHeadTrainBox == false)
        {
            Debug.Log("�ǳ�ͷ.���³�ʼ��ʧ��!");
            return;
        }
        if (curvySplineObjList == null)
        {
            Debug.Log("���³�ʼ��ʧ��!");
            return;
        }
        if (curvySplineObjList.Count < 1)
        {
            Debug.Log("���³�ʼ��ʧ��!");
            return;
        }
        // ��ʼ��·
        curvySpline = curvySplineObjList[index];
        if (curvySpline == null)
        {
            Debug.Log("���³�ʼ����·ʧ��!");
            return;
        }
        InitCurvySpline(curvySpline, speed, CurvyClamping.Clamp);
    }

    private bool isInitedTest = false;
    private void EveryInitTest()
    {
        // ��������ƫ����
        float trainTotalPosition = 0;
        if (trains != null)
        {
            trainTotalPosition = trainBoxPosition + trains.trainPosition;
        }
        if (splineController != null)
        {
            splineController.Position = trainTotalPosition < 0f ? trainTotalPosition = 0f : trainTotalPosition;
        }
        // ʼ�ջ�ȡ
        trains = transform.parent.GetComponent<Trains>();
        curvySplineObjList = trains.GetCurvySplineObjList();
        if(curvySplineObjList == null) { return; }
        if (isInitedTest)
        {
            return;
        }
        try
        {
            // ��ʼ��·
            curvySpline = curvySplineObjList[0];
        }
        catch
        {
            Debug.Log("curvySplineObjList ��ʱ��ʼ��!");
        }
        
        if (curvySpline == null)
        {
            return;
        }
        
        // ��ʼ�����߿�����
        InitCurvySpline(curvySpline, speed, CurvyClamping.Clamp);
        
        isInitedTest = true;
        Debug.Log("���Գ�ʼ�����!");
    }
    // ����
    private void OnDrawGizmos()
    {
        if (isStart)
        {
            return;
        }
        EveryInitTrainsTest();
        EveryInitTest();
    }
    // �����ٶ�
    public void SetSpeed(float Speed) 
    {
        if (IsTrainHeadEnd||IsTrainTailEnd)
        {
            speed = 0;
            return;
        }
        if (isHeadTrainBox)
        {
            speed = Speed<0? Speed=0: Speed;
        }
        else
        {
            Debug.Log("�����ٶ���Ч����������һ����ͷ!");
        }
    }
    // ���û���ʻ����
    public void SetHeadTrainDirection(MovementDirection movementDirection)
    {
        if (isHeadTrainBox)
        {
            moveDirection = movementDirection;
        }
        else
        {
            Debug.Log("���û���ʻ������Ч����������һ����ͷ!");
        }
    }
    // ��ȡ��ͷλ��
    public Vector3 GetTrainHeadPosition()
    {
        if (isHeadTrainBox)
        {
            return transform.position;
        }
        return Vector3.zero;
    }
    // ��ȡ��βλ��
    public Vector3 GetTrainTailPosition() 
    { 
        if (isTailTrainBox) 
        {
            return transform.position;
        }
        return Vector3.zero;
    }

    void Start()
    {
        isStart = true;
    }

    // Update is called once per frame
    void Update()
    {
        EveryInitTrains();
        EveryInitCurvySpline();
        // ÿִ֡����·������
        if (splineController != null)
        {
            splineController.Speed = speed;
            splineController.MovementDirection = moveDirection;
        }
    }

    private TrainMessage trainMsg = new();

    private void OnControlPointReached(CurvySplineMoveEventArgs arg0)
    {
        //Debug.Log($"{gameObject.name}��ʼ����·��:" + arg0.ControlPoint.ToString());
        if (trains!=null)
        {

            
            trainMsg.type = "wayPoint";
            trainMsg.data = arg0.ControlPoint.ToString();
            trainMsg.message = $"{gameObject.name}��ʼ����·��.";

            trainMsg.isTrainHead = false;
            trainMsg.isTrainTail = false;
            if (isHeadTrainBox)
            {
                trainMsg.isTrainHead = true;
            }
            if (isTailTrainBox)
            {
                trainMsg.isTrainTail = true;
            }

            trains.OnTrainMessage(trainMsg);
            Dictionary<string, int>  wayDic =trains.GetCurvyWay();
            foreach(var itm in wayDic)
            {
                string wayName = itm.Key;
                int index = itm.Value;
                NextCurvySpline(wayName, curvySplineObjList[index], arg0.ControlPoint);
            }
        }
        
    }
    // �Ƿ�ͷ���յ�
    private static bool IsTrainHeadEnd = false;
    // �Ƿ�β���յ�
    private static bool IsTrainTailEnd = false;
    private void OnTrainHeadEndReached(CurvySplineMoveEventArgs arg0)
    {
        IsTrainHeadEnd = true;
        //Debug.Log("��ͷ�����յ���!");
        trainMsg.type = "trainHeadEnd";
        trainMsg.isTrainHead = true;
        trainMsg.isTrainTail = false;
        trainMsg.message = $"��ͷ{gameObject.name}�����յ���.";
        trains.OnTrainMessage(trainMsg);
    }
    private void OntrainTailEndReached(CurvySplineMoveEventArgs arg0)
    {
        IsTrainTailEnd = true;
        //Debug.Log("��β�����յ���!");
        trainMsg.type = "trainTailEnd";
        trainMsg.isTrainHead = false;
        trainMsg.isTrainTail = true;
        trainMsg.message = $"��β{gameObject.name}�����յ���.";
        trains.OnTrainMessage(trainMsg);
    }

    

    

    // ��ʼ��ǰ��·
    private void InitCurvySpline(CurvySpline spline, float speedCurrent, CurvyClamping curvyClamping = CurvyClamping.Clamp)
    {
        if (GetComponent<SplineController>() == null)
        {
            splineController = gameObject.AddComponent<SplineController>();
        }
        else
        {
            splineController = GetComponent<SplineController>();
        }
        splineController.Speed = speedCurrent;
        splineController.Spline = spline;
        splineController.Clamping = curvyClamping;
    }

    // ѡ����һ��·��
    private void NextCurvySpline(string ControlPointContrast,CurvySpline splineNext, CurvySplineSegment ControlPoint)
    {
        if (ControlPoint.ToString() == ControlPointContrast)
        {
            splineController.SwitchTo(splineNext, 0f, 1f);
        }
    }
}
