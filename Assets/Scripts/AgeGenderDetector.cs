using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using Essentials;
using System;

[RequireComponent(typeof(Tracker), typeof(IDetector))]
public class AgeGenderDetector : MonoBehaviour
{

    [SerializeField] Canvas canvas = default;
    AgeGenderDetection model;

    (int xmin, int xmax, int ymin, int ymax) faceBbox;
    int[] faceKeypoints;
    readonly string itemPath = "Text";
    int n_times_called = 0;

    [SerializeField]
    bool runBackground = true;
    

    delegate Dictionary<int, Vector2> action();

    action GetkeypointsOfInterest;
    int noseIndex;

    protected CancellationToken cancellationToken;
    protected UniTask<bool> task;

    void Start()
    {
        var detector = GetComponent<IDetector>();
        cancellationToken = this.GetCancellationTokenOnDestroy();

        faceKeypoints = detector.GetFaceKeypointsIndices();
        noseIndex = detector.GetNoseIndex();

        var tracker = GetComponent<Tracker>();
        model = new AgeGenderDetection();

        GetkeypointsOfInterest = () => tracker.GetkeypointsOfInterest(faceKeypoints);

        tracker.onTargetFound += OnTextureUpdate;
    }

    protected void OnDestroy()
    {
        GetComponent<Tracker>().onTargetFound -= OnTextureUpdate;
        model?.Dispose();
    }


    protected async UniTask<bool> InvokeAsync(Texture texture)
    {
        await model.InvokeAsync(texture, cancellationToken);
        return true;
    }

    Texture? ExtractFace(Texture texture)
    {
        var keypoints = GetkeypointsOfInterest();

        UpdateBoundingBoxFromKeypoints(keypoints, texture.width, texture.height);

        if (BoundingBoxIsNotValid()) return null;

        var cropped = texture.ToTexture2D().Crop(x: faceBbox.xmin,
                                   y: faceBbox.ymin,
                                   width: faceBbox.xmax - faceBbox.xmin,
                                   height: faceBbox.ymax - faceBbox.ymin)
                                  // .Resized(model.resizedShape)
                                   ;
        return cropped;

    }
    void AfterInvoke()
    {
        var res = model.results;
        GetComponent<Monitor>()?.SetItem("Age Gender", $"{res}");
    }

    protected void OnTextureUpdate(object source, Tracker.TargetEventArgs ev)
    {
        if (n_times_called == 0)
            GetComponent<Monitor>()?.SetItem("Age Gender", $"Nothing");

        GetComponent<Monitor>()?.SetItem("Age Gender CALLED: ", n_times_called.ToString() + " times");
        n_times_called++;

        if (runBackground)
        {
            if (task.Status.IsCompleted())
            {
                var face = ExtractFace(ev.texture);
                if (face == null) return;

                task = InvokeAsync(face);
                AfterInvoke();
            }

        }
        else
        {
            var face = ExtractFace(ev.texture);
            if (face == null) return;

            model.Predict(face);
            AfterInvoke();
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