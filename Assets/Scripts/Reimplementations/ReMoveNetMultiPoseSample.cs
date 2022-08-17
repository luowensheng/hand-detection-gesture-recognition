using UnityEngine;
using Essentials;
using System;

public class ReMoveNetMultiPoseSample : MoveNetMultiPoseSample, IDetector
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
        runBackground = true;
        output = new Person[6];
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = new Person(GetNKeypoints());
        }
    }



    public override void AfterInvoke(Texture texture)
    {
        if (poses == null) return;

        PoseToPerson();
        OnPredictionEnd?.Invoke(this, new DetectionEventArgs() { people = output, texture = texture });
    }

    private void PoseToPerson()
    {
        for (int i = 0; i < output.Length; i++)
        {
            float x, y;
            float score = 0;

            for (int j = 0; j < poses[i].Length; j++)
            {
                x = poses[i].joints[j].x;
                y = poses[i].joints[j].y;
                score += poses[i].joints[j].score;
                output[i].keypoints[j] = new Keypoint(x: x, y: y, index: j, confidence: score);
            }


            output[i].boundingBox = new BoundingBox(xmax: poses[i].boundingBox.xMax, 
                                                    xmin: poses[i].boundingBox.xMin, 
                                                    ymax: poses[i].boundingBox.yMax,
                                                    ymin: poses[i].boundingBox.yMin,
                                                    score: poses[i].score
                                                    );

        }
    }



    public Hands GetHandsKeypointsIndices()
    {
        return new Hands { leftHand = 9, rightHand = 10 };
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
