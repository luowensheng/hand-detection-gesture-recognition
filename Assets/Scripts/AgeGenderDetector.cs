using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using Essentials;
using System;

//[RequireComponent(typeof(Tracker))]
[RequireComponent(typeof(Tracker), typeof(IDetector))]
public class AgeGenderDetector : MonoBehaviour
{

    [SerializeField] Canvas canvas = default;
    AgeGenderDetection model;

    (int xmin, int xmax, int ymin, int ymax) faceBbox;
    int[] faceKeypoints;
    readonly string itemPath = "Text";
    int n_times_called = 0;

    //[SerializeField]
    Text text;

    delegate Dictionary<int, Vector2> action();

    action GetkeypointsOfInterest;
    int noseIndex;
    private int counter = 1;
    GameObject LoadItem() => Resources.Load<GameObject>(itemPath);
    // Watcher("ScreenMessage", (object message)=>{ text.text = val; });

    void Start()
    {
        var detector = GetComponent<IDetector>();

        faceKeypoints = detector.GetFaceKeypointsIndices();
        noseIndex = detector.GetNoseIndex();

        var tracker = GetComponent<Tracker>();
        model = new AgeGenderDetection();

        if (canvas == null) return;
        var item = Instantiate(LoadItem(), canvas.transform);

        item.SetActive(true);
        text = item.GetComponent<Text>();

        setText($"model loaded {model != null}");


        GetkeypointsOfInterest = () => tracker.GetkeypointsOfInterest(faceKeypoints);

        tracker.onTargetFound += OnTextureUpdate;
       // Watcher.PutItem("Screen", (object data) => setText(data.ToString()));
    }

    protected void OnDestroy()
    {
        GetComponent<Tracker>().onTargetFound -= OnTextureUpdate;
        model?.Dispose();
    }

    private void setText(string val)
    {
        if (text != null)
        {
            if (counter % 20 == 0)
            {
                text.text = "";
            }
            text.text += $"{counter++}. {val}\n";
            Debug.Log(val);
        }
        else
        {
            Debug.Log("text is null");
        }
    }

    private void OnTextureUpdate(object source, Tracker.TargetEventArgs ev)
    {
        n_times_called++;

        if (n_times_called % 5 != 0) return;

        try
        {
            var texture = ev.texture;
            var keypoints = GetkeypointsOfInterest();

            UpdateBoundingBoxFromKeypoints(keypoints, texture.width, texture.height);

            if (BoundingBoxIsNotValid()) return;

            var cropped = texture.ToTexture2D().Crop(x: faceBbox.xmin,
                                       y: faceBbox.ymin,
                                       width: faceBbox.xmax - faceBbox.xmin,
                                       height: faceBbox.ymax - faceBbox.ymin)
                                       .Resized(model.resizedShape)
                                       ;
           // cropped.SaveTexture("new");

            model.Predict(cropped);


            var res = model.results;

            setText($"{res}");

            GetComponent<Monitor>()?.SetItem("Age Gender", $"{res}");


        }
        catch (System.Exception e)
        {
            setText(e.StackTrace);
            throw e;
        }
    }

    private bool BoundingBoxIsNotValid() => ((faceBbox.xmax - faceBbox.xmin) == 0) || ((faceBbox.ymax - faceBbox.ymin) == 0);

    private void UpdateBoundingBoxFromKeypoints(Dictionary<int, Vector2> keypoints, int width, int height)
    {

        float maxX = 0;
        float minX = 10000;

        Vector2 nosePosition = keypoints[noseIndex];

        foreach (var item in keypoints.Values)
        {
            if (maxX < item.x)
            {
                maxX = item.x;
            }
            if (minX > item.x)
            {
                minX = item.x;
            }
        }

        faceBbox.xmin = ClipValues(minX, width);
        faceBbox.xmax = ClipValues(maxX, width);
        faceBbox.ymin = ClipValues(nosePosition.y - minX * 0.8f, height);
        faceBbox.ymax = ClipValues(nosePosition.y + minX * 1.2f, height);

    }

    int ClipValues(float val, int max) => (int)(val > 0 ? (val <= max ? val : max) : 0);

}