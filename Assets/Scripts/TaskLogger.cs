using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TaskLogger : MonoBehaviour
{
    [SerializeField] private string fileName = "tasklogs";
    private StreamWriter writer;

    // Start is called before the first frame update
    void Start()
    {
        // create a folder
        Directory.CreateDirectory(Application.persistentDataPath + "/stroopTask/");
        Debug.Log("Path = " + Application.persistentDataPath + "/stroopTask/");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void WriteToFile(string text)
    {
        writer.WriteLine(text);
    }

    public void StartTaskLogging()
    {
        writer = new StreamWriter(Application.persistentDataPath + "/stroopTask/" + fileName + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv", true);
        writer.AutoFlush = true;
    }

    public void CloseFile()
    {
        if (writer != null) { 
            writer.Close();
        }
    }

    private void OnApplicationQuit()
    {
        CloseFile();
    }
}
