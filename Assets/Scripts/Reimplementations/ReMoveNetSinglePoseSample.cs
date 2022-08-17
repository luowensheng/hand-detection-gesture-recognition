using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TensorFlowLite;
using TensorFlowLite.MoveNet;
using Essentials;
using System;

public class ReMoveNetSinglePoseSample : MoveNetSinglePoseSample, IDetector
{

    private Person[] output;


    public event EventHandler<DetectionEventArgs> OnPredictionEnd;

    public Person[] GetDetections()
    {
        return output;
    }

    private void Start()
    {
        output = new Person[1] { new Person(GetNKeypoints()) };
        base.Start();
    }

    public override void AfterInvoke(Texture texture)
    {
        if (pose == null) return;

        PoseToPerson();
        OnPredictionEnd?.Invoke(this, new DetectionEventArgs() { people = output, texture = texture });
    }

    private void PoseToPerson()
    {
        var bbox = new BoundingBox();
        float x, y;
        float score = 0;
        bbox.score = 0;
        for (int i = 0; i < pose.Length; i++)
        {
            x = pose[i].x;
            y = pose[i].y;
            score += pose[i].score;
            output[0].keypoints[i] = new Keypoint(x: x, y: y, index: i, confidence: score);
            
            bbox.score += score;
            bbox.xmax = (x > bbox.xmax) ? x : bbox.xmax;
            bbox.ymax = (y > bbox.ymax) ? y : bbox.ymax;

            bbox.xmin = (x < bbox.xmin) ? x : bbox.xmin;
            bbox.ymin = (x < bbox.ymin) ? x : bbox.ymin;

        }

        bbox.score /= pose.Length;

        output[0].boundingBox = bbox;
        
    }

    public Hands GetHandsKeypointsIndices()
    {
        return new Hands { leftHand = 9, rightHand = 10 };
    }

    public int[] GetFaceKeypointsIndices()
    {
        return new int[5] { 0, 1, 2, 3, 4 };
    }

    public int GetNKeypoints()
    {
        return 17;
    }

    public int GetNoseIndex()
    {
        return 0;
    }
}
