using System.Threading;
using Cysharp.Threading.Tasks;
using TensorFlowLite;
using UnityEngine;
using UnityEngine.UI;
using Essentials;
using System;

/// <summary>
/// BlazePose form MediaPipe
/// https://github.com/google/mediapipe
/// https://viz.mediapipe.dev/demo/pose_tracking
/// </summary>
public sealed class ReBlazePoseSample : BlazePoseSample, IDetector
{
    private Person[] output;

    public event EventHandler<DetectionEventArgs> OnPredictionEnd;

    public Person[] GetDetections()
    {
        return output;
    }

    private void Start()
    {
        base.Start();
        output = new Person[1] { new Person(GetNKeypoints()) };
        runBackground = true;

    }

    protected new void Update()
    {
        if (poseResult == null) {
            print("POSERESULT IS NONE");
        };

        drawer.DrawPoseResult(poseResult);
        base.Update();
    }


    public override void AfterInvoke(Texture texture)
    {
        if (poseResult == null) return;

        PoseToPerson();
        OnPredictionEnd?.Invoke(this, new DetectionEventArgs() { people = output, texture = texture });
    }

    private void PoseToPerson()
    {
        float x, y;
        float score = poseResult.score;
        var keypoints = poseResult.keypoints;

        for (int i = 0; i < keypoints.Length; i++)
        {
            x = keypoints[i].x;
            y = keypoints[i].y;

            output[0].keypoints[i] = new Keypoint(x: x, y: y, index: i, confidence: score);
        }

        output[0].boundingBox = new BoundingBox(xmax: poseResult.rect.xMax,
                                                xmin: poseResult.rect.xMin,
                                                ymax: poseResult.rect.yMax,
                                                ymin: poseResult.rect.yMin,
                                                score: score
                                                );
    }

    public Hands GetHandsKeypointsIndices()
    {
        return new Hands { leftHand = 15, rightHand = 16 };
    }

    public int[] GetFaceKeypointsIndices()
    {
        return new int[10] { 1,2,3,4,5,6,7,8,9,10 };
    }

    public int GetNKeypoints()
    {
        return 33;
    }

    public int GetNoseIndex()
    {
        return 0;
    }
}
