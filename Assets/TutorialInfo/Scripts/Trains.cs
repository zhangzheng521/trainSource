using System.Collections.Generic;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Controllers;
using UnityEngine;

public class Trains : MonoBehaviour
{
    // 火车限速
    public float limitSpeed = 10f;
    // 火车加速度
    public float acceleration = 4f;
    // 最小减速速度
    public float minSlowSpeed = 1f;
    // 最小减速距离
    public float nearSlowDistance = 50f;
    // 火车起始位置
    public float trainPosition = 0f;
    // 火车停站牌
    public string initTrainStopStation;
    // 选择火车行驶方向
    public MovementDirection movementDirection = MovementDirection.Forward;

    // 待初始化线路物体
    public List<GameObject> splineObjList;



    // 火车车速
    public float speed = 0f;


    // 待初始化线路
    private List<CurvySpline> curvySplineObjList = new();
    // 待选择的线路
    private Dictionary<string, string> selectCurvySplines = new();
    // 火车停站点
    private Vector3 trainStopStationPosition;
    // 火车距天站点距离
    private float trainStopToStationDistance;
    // 火车是否禁用加速状态
    private bool isAccelerationState = false;


    private bool isStart = false;

    private void Awake()
    {
        // 设置线路
        SetCurvySplines("CurvySpline1.CP0005", "CurvySpline3");
        SetCurvySplines("CurvySpline3.CP0002", "CurvySpline4");
        SetCurvySplines("CurvySpline4.CP0005", "CurvySpline3");
        //初始化停车牌
        SetTrainStopStation();
    }
    private void Start()
    {
        isStart = true;
    }
    // 编辑器模式下每帧执行
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
        // 实时计算火车到目标点距离
        EveryGetTrainToTargetDistance();
        // 火车开始行驶
        EveryRunTrain();
    }

    private TrainBox trainHeadBox;// 车头
    private TrainBox trainTailBox;// 车尾
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
        // 获取trainBox
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

    // 计算火车到目标点距离
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
    private float recordSpeed;// 记录开始减速时，最后一次速度
    private float recordDeltaSpeed;// 记录开始减速时，最后一次每减少距离单位的速度
    private float recordDistance = 0f;// 记录每减速单位距离
    // 计算到站减速到0
    private void SlowDownSpeed()
    {
        if (trainStopToStationDistance > nearSlowDistance) 
        {
            // 记录减速前的速度值
            recordSpeed = speed;
            // 记录每距离点减少的速度值
            recordDeltaSpeed = recordSpeed / nearSlowDistance;
            return;
        }
        else
        {
            // 进入减速范围时，禁用加速
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

    // 计算车头到目标点距离
    private void GetTrainHeadToTargetDistance()
    {
        if (trainStopStationPosition == Vector3.zero)
        {
            return;
        }
        if (trainHeadBox == null)
        {
            Debug.LogError("请设置火车头!");
            return;
        }
        Vector3 trainHeadPosition = trainHeadBox.GetTrainHeadPosition();
        if (trainHeadPosition == Vector3.zero)
        {
            return;
        }
        trainStopToStationDistance = Vector3.Distance(trainStopStationPosition, trainHeadPosition);
    }
    // 计算车尾到目标点距离
    private void GetTrainTailToTargetDistance()
    {
        if (trainStopStationPosition == Vector3.zero)
        {
            return;
        }
        if (trainTailBox == null)
        {
            Debug.LogError("请设置火车尾!");
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
    // 设置线路
    public void SetCurvySplines(string from, string to)
    {
        selectCurvySplines[from] = to;
    }

    
    // 火车开始行驶
    public void EveryRunTrain()
    {
        if (!isAccelerationState && speed < limitSpeed)
        {
            float currentFrameTime = Time.time;
            float v = acceleration * currentFrameTime/1000;
            speed += v;
        }
    }


    //设置火车停站点 设置案例: trainStopStation2=CurvySpline1.CP0004
    public void SetTrainStopStation(string trainStopStation2 = "")
    {
        if (string.IsNullOrEmpty(initTrainStopStation))
        {
            // 错误强制火车停止
            speed = 0;
            // 禁用加速
            isAccelerationState = true;
            Debug.LogError("请设置初始火车停车站牌!");
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
    // 重新初始化线路
    public void ReInitCurvySpline(string cuvrySpineName)
    {
        int index = GetObjName(cuvrySpineName);
        // TODO 先设置splineObjList顺序
        trainHeadBox.ReInitCurvySpline(index);
    }

    public void OnTrainMessage(TrainMessage trainMsg)
    {
        //Debug.Log($"收到消息:{JsonUtility.ToJson(trainMsg)}");
        // 匹配停站消息停车
        if (trainMsg.type == "wayPoint" && trainMsg.data == initTrainStopStation)
        {
            // 向前、向后走以车头为准
            if ((movementDirection == MovementDirection.Forward && trainMsg.isTrainHead) || (movementDirection == MovementDirection.Backward && trainMsg.isTrainTail))
            {
                speed = 0;// 停车
                // 禁用加速
                isAccelerationState = true;
                // 计算车头到目标点距离
                Debug.Log("initTrainStopStation:" + JsonUtility.ToJson(trainMsg));
            }

        }
    }
    

}


