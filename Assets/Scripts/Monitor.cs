using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Essentials;
using TensorFlowLite;

public class Monitor : MonoBehaviour
{
    // Start is called before the first frame update

    float fps = 0;
    float m_frameCounter = 0;
    float cam_framCounter = 0;

    public float m_refreshTime = 0.5f;
    private Dictionary<string, string> items;
    GUIStyle style;

    public int CalculteFps(float count) => (int)(count / Time.realtimeSinceStartup);


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

        var cam = GetComponent<WebCamInput>();
        var detector = GetComponent<IDetector>();

        if (detector != null)
        {
            detector.OnPredictionEnd += (_, _) =>
            {
                m_frameCounter++;
            };
        }

        if (cam != null)
            //detector.OnPredictionEnd += GetFps;
            cam.OnTextureUpdate.AddListener((a)=>
            {
                cam_framCounter++;

            });
    }

    private void OnDestroy()
    {
        var detector = GetComponent<IDetector>();

        //if (detector != null)
        //    detector.OnPredictionEnd -= GetFps;
    }

    private void Update()
    {
    }


    void UpdateFps()
    {
        m_frameCounter++;
        SetItem("Number of Predictions", m_frameCounter.ToString());
        SetItem("Time", ((int)Time.realtimeSinceStartup).ToString() + " s");
        SetItem("Cam Fps", (CalculteFps(cam_framCounter)).ToString());
        SetItem("Model Fps", (CalculteFps(m_frameCounter)).ToString());

    }

    void OnGUI()
    {
        UpdateFps();

        var y = 300;

        // GUI.Label(new Rect(100, y, 1000, 200), $"Current fps: [{CalculteFps()}]", style);

        foreach (var item in items)
        {
            y += 40;
            GUI.Label(new Rect(100, y, 1000, 200), $"{item.Key}: [{item.Value}]", style);
        }

    }

}
