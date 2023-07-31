using System.Collections.Generic;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainBox : MonoBehaviour
{
    public bool isHeadTrainBox = false;// 是否车头
    public bool isTailTrainBox = false;// 是否车尾

    public float trainBoxPosition = 0f;

    // 待初始化线路
    private static List<CurvySpline> curvySplineObjList;
    private static float speed = 10f;
    private static Trains trains;
    private static CurvySpline curvySpline;
    // 车厢移动方向
    private static MovementDirection moveDirection = MovementDirection.Forward;
    //private static CurvySpline curvySplineNext;
    // private static float headTrainDeltaVect = 0;// 火车头位移
    //private static bool isOver = false;

    private SplineController splineController;
    private bool isStart = false;

    // 初始化Trains对象
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
    // 初始化Trains测试对象
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
        // 初始线路
        curvySpline = curvySplineObjList[0];

        if(curvySpline==null)
        {
            return;
        }
        isInited = true;

        InitCurvySpline(curvySpline, speed, CurvyClamping.Clamp);
        // 监听路径点事件
        splineController.OnControlPointReached.AddListenerOnce(OnControlPointReached);
        // 车头到达终点时触发
        if (isHeadTrainBox)
        {
            splineController.OnEndReached.AddListenerOnce(OnTrainHeadEndReached);
        }
        // 车尾到达终点时触发
        if (isTailTrainBox)
        {
            splineController.OnEndReached.AddListenerOnce(OntrainTailEndReached);
        }
    }
    // 重新初始化线路
    public void ReInitCurvySpline(int index=0)
    {
        if (isHeadTrainBox == false)
        {
            Debug.Log("非车头.重新初始化失败!");
            return;
        }
        if (curvySplineObjList == null)
        {
            Debug.Log("重新初始化失败!");
            return;
        }
        if (curvySplineObjList.Count < 1)
        {
            Debug.Log("重新初始化失败!");
            return;
        }
        // 初始线路
        curvySpline = curvySplineObjList[index];
        if (curvySpline == null)
        {
            Debug.Log("重新初始化线路失败!");
            return;
        }
        InitCurvySpline(curvySpline, speed, CurvyClamping.Clamp);
    }

    private bool isInitedTest = false;
    private void EveryInitTest()
    {
        // 火车箱子总偏移量
        float trainTotalPosition = 0;
        if (trains != null)
        {
            trainTotalPosition = trainBoxPosition + trains.trainPosition;
        }
        if (splineController != null)
        {
            splineController.Position = trainTotalPosition < 0f ? trainTotalPosition = 0f : trainTotalPosition;
        }
        // 始终获取
        trains = transform.parent.GetComponent<Trains>();
        curvySplineObjList = trains.GetCurvySplineObjList();
        if(curvySplineObjList == null) { return; }
        if (isInitedTest)
        {
            return;
        }
        try
        {
            // 初始线路
            curvySpline = curvySplineObjList[0];
        }
        catch
        {
            Debug.Log("curvySplineObjList 暂时初始化!");
        }
        
        if (curvySpline == null)
        {
            return;
        }
        
        // 初始化曲线控制器
        InitCurvySpline(curvySpline, speed, CurvyClamping.Clamp);
        
        isInitedTest = true;
        Debug.Log("测试初始化完成!");
    }
    // 测试
    private void OnDrawGizmos()
    {
        if (isStart)
        {
            return;
        }
        EveryInitTrainsTest();
        EveryInitTest();
    }
    // 设置速度
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
            Debug.Log("设置速度无效，请先设置一个火车头!");
        }
    }
    // 设置火车行驶方向
    public void SetHeadTrainDirection(MovementDirection movementDirection)
    {
        if (isHeadTrainBox)
        {
            moveDirection = movementDirection;
        }
        else
        {
            Debug.Log("设置火车行驶方向无效，请先设置一个火车头!");
        }
    }
    // 获取车头位置
    public Vector3 GetTrainHeadPosition()
    {
        if (isHeadTrainBox)
        {
            return transform.position;
        }
        return Vector3.zero;
    }
    // 获取车尾位置
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
        // 每帧执行线路控制器
        if (splineController != null)
        {
            splineController.Speed = speed;
            splineController.MovementDirection = moveDirection;
        }
    }

    private TrainMessage trainMsg = new();

    private void OnControlPointReached(CurvySplineMoveEventArgs arg0)
    {
        //Debug.Log($"{gameObject.name}开始进入路点:" + arg0.ControlPoint.ToString());
        if (trains!=null)
        {

            
            trainMsg.type = "wayPoint";
            trainMsg.data = arg0.ControlPoint.ToString();
            trainMsg.message = $"{gameObject.name}开始进入路点.";

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
    // 是否车头到终点
    private static bool IsTrainHeadEnd = false;
    // 是否车尾到终点
    private static bool IsTrainTailEnd = false;
    private void OnTrainHeadEndReached(CurvySplineMoveEventArgs arg0)
    {
        IsTrainHeadEnd = true;
        //Debug.Log("车头到达终点了!");
        trainMsg.type = "trainHeadEnd";
        trainMsg.isTrainHead = true;
        trainMsg.isTrainTail = false;
        trainMsg.message = $"车头{gameObject.name}到达终点了.";
        trains.OnTrainMessage(trainMsg);
    }
    private void OntrainTailEndReached(CurvySplineMoveEventArgs arg0)
    {
        IsTrainTailEnd = true;
        //Debug.Log("车尾到达终点了!");
        trainMsg.type = "trainTailEnd";
        trainMsg.isTrainHead = false;
        trainMsg.isTrainTail = true;
        trainMsg.message = $"车尾{gameObject.name}到达终点了.";
        trains.OnTrainMessage(trainMsg);
    }

    

    

    // 初始当前线路
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

    // 选择下一个路线
    private void NextCurvySpline(string ControlPointContrast,CurvySpline splineNext, CurvySplineSegment ControlPoint)
    {
        if (ControlPoint.ToString() == ControlPointContrast)
        {
            splineController.SwitchTo(splineNext, 0f, 1f);
        }
    }
}
