using UnityEngine;

public class InitData : MonoBehaviour
{
    private TestJson testJson;
    // Start is called before the first frame update
    private void Awake()
    {
        testJson = LoadJsonFile.LoadJsonFromFile<TestJson>("JsonData/test.json");
    }

    private void Start()
    {
        Debug.Log("json:" + JsonUtility.ToJson(testJson));
    }
}
