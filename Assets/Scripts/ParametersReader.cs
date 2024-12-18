using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ParametersReader : MonoBehaviour
{

    [SerializeField] private TextAsset file;

    private List<List<string>> parameters = new List<List<string>>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<List<string>> ReadParameters()
    {
        parameters.Clear();
        StreamReader reader = new StreamReader(new MemoryStream(file.bytes));
        string line;
        int counter = 0;
        while ((line = reader.ReadLine()) != null)
        {
            if(counter > 0)
            {
                List<string> lineParameters = new List<string>();
                string[] values = line.Replace(" ", "").Split(',');
                foreach (string value in values)
                {
                    lineParameters.Add(value);
                }
                parameters.Add(lineParameters);
            }
            counter++;
        }
        reader.Close();
        return parameters;
    }

    public string GetFirstLine()
    {
        StreamReader reader = new StreamReader(new MemoryStream(file.bytes));
        string line = reader.ReadLine();
        reader.Close();
        return line;
    }




}
