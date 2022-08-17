using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Essentials
{
    public enum BodyParts
    {
        NOSE = 0,
        LEFT_EYE = 1,
        RIGHT_EYE = 2,
        LEFT_EAR = 3,
        RIGHT_EAR = 4,
        LEFT_SHOULDER = 5,
        RIGHT_SHOULDER = 6,
        LEFT_ELBOW = 7,
        RIGHT_ELBOW = 8,
        LEFT_WRIST = 9,
        RIGHT_WRIST = 10,
        LEFT_HIP = 11,
        RIGHT_HIP = 12,
        LEFT_KNEE = 13,
        RIGHT_KNEE = 14,
        LEFT_ANKLE = 15,
        RIGHT_ANKLE = 16
    }

    public struct Hands
    {
        public int leftHand;
        public int rightHand;
    }

    public readonly struct Keypoint
    {
        public readonly float x;
        public readonly float y;
        public readonly int index;
        public readonly float confidence;

        public Keypoint(float x, float y, int index, float confidence)
        {
            this.x = x;
            this.y = y;
            this.index = index;
            this.confidence = confidence;
        }

        public override string ToString()
        {
            return $"x={x}, y={y}, confidence={confidence}, index={index}";
        }
    }

    public struct BoundingBox
    {
        public float ymin, xmin, ymax, xmax, score;

        public BoundingBox(float ymin, float xmin, float ymax, float xmax, float score)
        {
            this.ymin = ymin;
            this.xmin = xmin;
            this.ymax = ymax;
            this.xmax = xmax;
            this.score = score;
        }

        public override string ToString()
        {
            return $"ymin={ymin}, xmin={xmin}, ymax={ymax}, xmax={xmax}, score={score}";
        }
    }


    public class Person
    {
        public BoundingBox boundingBox; // { get; set; }
        public Keypoint[] keypoints; // { get; set; }

        public Person(BoundingBox boundingBox, Keypoint[] keypoints)
        {
            this.keypoints = keypoints;
            this.boundingBox = boundingBox;
        }

        public Person(int nKepoints)
        {
            this.keypoints = new Keypoint[nKepoints];

            for (int i = 0; i < nKepoints; i++)
            {
                this.keypoints[i] = new Keypoint(0, 0, 0, 0);

            }
            this.boundingBox = new BoundingBox(0, 0, 0, 0, 0);

        }

        public override string ToString()
        {
            string output = $"box:{boundingBox}\nkeypoints:";

            foreach (var kp in keypoints)
            {
                output += $"\n{kp}";
            }

            return output;
        }
        public Keypoint this[int key]
        {
            get => this.keypoints[key - 1];
        }

    }

    public class DetectionEventArgs : EventArgs
    {
        public Person[] people { get; set; }
        public Texture texture { get; set; }
    }

    public interface IDetector
    {
        Person[] GetDetections();
        public event EventHandler<DetectionEventArgs> OnPredictionEnd;
        public Hands GetHandsKeypointsIndices();
        public int[] GetFaceKeypointsIndices();
        public int GetNKeypoints();
        public int GetNoseIndex();

    }


}
