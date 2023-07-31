using System.IO;
using UnityEngine;

public class LoadJsonFile : MonoBehaviour
{
    public static T LoadJsonFromFile<T>(string dir) where T : class
    {
        if (!File.Exists(Application.dataPath + "/"+ dir))
        {
            Debug.LogError("Don't Find");
            return null;
        }

        StreamReader sr = new StreamReader(Application.dataPath + "/"+ dir);
        if (sr == null)
        {
            return null;
        }
        string json = sr.ReadToEnd();

        if (json.Length > 0)
        {
            return JsonUtility.FromJson<T>(json);
        }
        return null;
    }
}
