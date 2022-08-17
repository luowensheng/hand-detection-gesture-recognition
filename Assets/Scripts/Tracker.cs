using System;
using System.Collections.Generic;
using TensorFlowLite;
using UnityEngine;
using Essentials;


[RequireComponent(typeof(IDetector))]
public class Tracker : MonoBehaviour
{
    public class TargetEventArgs : EventArgs
    {
        public Person target { get; set; }
        public Texture texture { get; set; }
    }
    public event EventHandler<TargetEventArgs> onTargetFound;

   // [SerializeField, Range(0, 1)]
    private readonly float distanceCoefficient = 1f;


   // [SerializeField, Range(0, 1)]
     private readonly float predictionCoefficient = 0.4f;

    //[SerializeField, Range(0, 1)]
     private readonly float sizeCoefficient = 0.6f;


    private Person target;
    private readonly float[] scores = new float[6];

    float previousScore = 0f;

    [SerializeField, Range(0, 1)]
    readonly float changeTargetThreshold = 0.85f;

    [SerializeField]
    readonly int changeTargetWait = 3;

    [SerializeField]
    readonly bool drawOthers = true;

    int counter = 0;

    int width;
    int height;
    int targetIdx;

    bool foundTarget = false;

    Hands handIndices;

    void Start()
    {
        var detector = gameObject.GetComponent<IDetector>();
        handIndices = detector.GetHandsKeypointsIndices();
        target = new Person(detector.GetNKeypoints());

        var webCamInput = GetComponent<WebCamInput>();

        detector.OnPredictionEnd += OnTrackRequest;
    }

    void OnTrackRequest(object source, DetectionEventArgs ev)
    {
        (width, height) = GetComponent<WebCamInput>().GetTextureShape();

        if (width * height == 0) return;
        
        ComputeTargetLocation(source, ev);
    }

    public void OnDestroy()
    {
        GetComponent<IDetector>().OnPredictionEnd -= OnTrackRequest;
    }

    public int GetTargetBoxSize()
    {
        var box = GetTargetBox();
        int size = (box.xmax - box.xmin) * (box.ymax - box.ymin);
        return size;
    }


    public (int xmin, int xmax, int ymin, int ymax) GetTargetBox()
    {
        var result = target.boundingBox;

        (int xmin, int xmax, int ymin, int ymax) box = ((int)(result.xmin * width),
                                                        (int)(result.xmax * width),
                                                        (int)(result.ymin * height),
                                                        (int)(result.ymax * height)
                                                        );
        return box;
    }

    public ((int x, int y) leftHand, (int x, int y) rightHand) GetTargetHands()
    {
        var keypoints = GetkeypointsOfInterest(new int[2] { handIndices.leftHand, handIndices.rightHand });

        return (((int)keypoints[0].x , (int)keypoints[0].y), ((int)keypoints[1].x , (int)keypoints[1].y));
    }

    public Dictionary<int, Vector2> GetkeypointsOfInterest(int[] keypoints)
    {
        var results = new Dictionary<int, Vector2>();

        foreach (int index in keypoints)
        {
            int px = (int)(target.keypoints[index].x * width);
            int py = (int)(target.keypoints[index].y * height);
            var item = new Vector2(px, py);
            results.Add(index, item);
        }
        return results;
    }


    void Update()
    {

    }

    private void ComputeTargetLocation(object source, DetectionEventArgs e)
    {

        var people = e.people;
        float best_score = 0;
        int best_index = 0;

        for (int index = 0; index < people.Length; index++)
        {
            if (ActionRecognition.IsRaisingHands(people[index], handIndices))
            {
                foundTarget = true;
                print("Raised Hand");
                GetComponent<Monitor>()?.SetItem("Hands raised", $"{true}");
            }
            else
            {
                GetComponent<Monitor>()?.SetItem("Hands raised", $"{false}");
            }

            if (foundTarget)
            {
                var score = GetScore(people[index]);
                scores[index] = score;

                if (score > best_score)
                {
                    best_index = index;
                    best_score = score;
                }
            }

        }


        if ((foundTarget) && ((counter > changeTargetWait) || (previousScore * changeTargetThreshold < best_score)))
        {
            previousScore = best_score;
            target = people[best_index];
            counter = 0;
            targetIdx = best_index;

            onTargetFound?.Invoke(this, new TargetEventArgs() { target = target, texture = e.texture });
        }
        else
        {
            counter++;
        }
        //onTargetFound?.Invoke(this, new TargetEventArgs() { target = target, texture = e.texture });


        Debug.Log("computed target location");
    }

    private void ShowScores()
    {
        var res = "";
        for (int i = 0; i < scores.Length; i++)
        {
            res += $"({i} : {scores[i]}) \n";
        }
        print(res);
    }

    private float GetScore(Person person)
    {
        var bbox_score = BboxScore(person.boundingBox);
        var keypoint_score = GetKeypointScore(person.keypoints);

        return bbox_score * keypoint_score;

    }

    private float GetKeypointScore(Keypoint[] keypoints)
    {
        float score = 0f;
        float dist_score = 0f;

        for (int i = 0; i < keypoints.Length; i++)
        {
            var dist_x = 1 - Mathf.Abs(keypoints[i].x - target.keypoints[i].x);
            var dist_y = 1 - Mathf.Abs(keypoints[i].y - target.keypoints[i].y);
            dist_score += (dist_x + dist_y) / 2;
            score += keypoints[i].confidence;
        }

        return (score * predictionCoefficient + dist_score * distanceCoefficient) / keypoints.Length;
    }

    private float BboxScore(BoundingBox bbox)
    {
        var size_score = (bbox.xmax - bbox.xmin) * (bbox.ymax - bbox.ymin);
        var dist_score = GetDistanceScore(bbox);
        return sizeCoefficient * size_score + dist_score * distanceCoefficient + bbox.score * predictionCoefficient;
    }

    private float GetDistanceScore(BoundingBox bbox)
    {
        var dist_xm = CompareDistance(bbox.xmax, target.boundingBox.xmax);
        var dist_xn = CompareDistance(bbox.xmin, target.boundingBox.xmin);
        var dist_ym = CompareDistance(bbox.ymax, target.boundingBox.ymax);
        var dist_yn = CompareDistance(bbox.ymin, target.boundingBox.ymin);

        return (dist_xm + dist_xn + dist_ym + dist_yn) / 4;

    }

    private float CompareDistance(float x, float y)
    {
        return 1 - Mathf.Abs(x - y);
    }

}
