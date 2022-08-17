using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Essentials;


public class Monitor : MonoBehaviour
{
    // Start is called before the first frame update
    float fps = 0;
    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float m_lastFramerate = 0.0f;
    public float m_refreshTime = 0.5f;
    private Dictionary<string, string> items;
    GUIStyle style;

    public void SetItem(string itemName, string itemValue)
    {
        if (items.ContainsKey(itemName))
        {
            items[itemName] = itemValue;
        }
        else
        {
            items.Add(itemName, itemValue);
        }
    }

    void Start()
    {
        style = new GUIStyle
        {
            fontSize = 20
        };

        style.normal.textColor = Color.red;

        items = new Dictionary<string, string>();

        var detector = GetComponent<IDetector>();
        
        if (detector != null)
            detector.OnPredictionEnd += GetFps;
    }

    private void OnDestroy()
    {
        var detector = GetComponent<IDetector>();

        if (detector != null)
            detector.OnPredictionEnd -= GetFps;
    }

    private void Update()
    {
    }

    void GetFps(object o, DetectionEventArgs a)
    {
        if (m_timeCounter < m_refreshTime)
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            m_lastFramerate = (float)m_frameCounter / m_timeCounter;
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
        }
        print($"Current fps: [{m_lastFramerate.ToString()}]");
    }

    void OnGUI()
    {
        var y = 400;

        GUI.Label(new Rect(100, y, 1000, 200), $"Current fps: [{(int)m_lastFramerate}]", style);

        foreach (var item in items)
        {
            y += 40;
            GUI.Label(new Rect(100, y, 1000, 200), $"{item.Key}: [{item.Value}]", style);
        }

    }

}
