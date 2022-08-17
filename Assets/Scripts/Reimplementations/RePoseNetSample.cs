using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;
using Cysharp.Threading.Tasks;
using Essentials;
using System;

[RequireComponent(typeof(WebCamInput))]
public class RePoseNetSample : PoseNetSample, IDetector
{
    private Person[] output;

    public event EventHandler<DetectionEventArgs> OnPredictionEnd;

    public Person[] GetDetections()
    {
        return output;
    }

    public int GetNKeypoints()
    {
        return 17;
    }

    private void Start()
    {
        base.Start();
        output = new Person[1] { new Person(GetNKeypoints()) };
        runBackground = true;

    }



    public override void AfterInvoke(Texture texture)
    {
        if (results == null) return;

        PoseToPerson();
        OnPredictionEnd?.Invoke(this, new DetectionEventArgs() { people = output, texture = texture });
    }

    private void PoseToPerson()
    {
        float x, y;
        float score = 0;
        var keypoints = results;
        var bbox = new BoundingBox();

        for (int i = 0; i < keypoints.Length; i++)
        {
            x = keypoints[i].x;
            y = keypoints[i].y;
            score += keypoints[i].x;

            output[0].keypoints[i] = new Keypoint(x: x, y: y, index: i, confidence: score);

            bbox.score += score;
            bbox.xmax = (x > bbox.xmax) ? x : bbox.xmax;
            bbox.ymax = (y > bbox.ymax) ? y : bbox.ymax;

            bbox.xmin = (x < bbox.xmin) ? x : bbox.xmin;
            bbox.ymin = (x < bbox.ymin) ? x : bbox.ymin;

        }

        bbox.score /= keypoints.Length;

        output[0].boundingBox = bbox;

    }

    public Hands GetHandsKeypointsIndices()
    {
        return new Hands { leftHand = 9,  rightHand= 10 };
    }

    public int[] GetFaceKeypointsIndices()
    {
        return new int[5] { 0, 1, 2, 3, 4 };
    }

    public int GetNoseIndex()
    {
        return 0;
    }
}
