using System.Threading;
using Cysharp.Threading.Tasks;
using TensorFlowLite;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BlazePose form MediaPipe
/// https://github.com/google/mediapipe
/// https://viz.mediapipe.dev/demo/pose_tracking
/// </summary>
[RequireComponent(typeof(WebCamInput))]
public class BlazePoseSample : MonoBehaviour
{
    [SerializeField]
    protected BlazePose.Options options = default;

    [SerializeField]
    protected RectTransform containerView = null;
    [SerializeField]
    protected RawImage debugView = null;
    [SerializeField]
    protected RawImage segmentationView = null;

    [SerializeField]
    protected Canvas canvas = null;
    [SerializeField] protected bool runBackground = true;
    [SerializeField, Range(0f, 1f)]
    protected float visibilityThreshold = 0.5f;


    protected BlazePose pose;
    protected PoseDetect.Result poseResult;
    protected PoseLandmarkDetect.Result landmarkResult;
    protected BlazePoseDrawer drawer;

    protected UniTask<bool> task;
    protected CancellationToken cancellationToken;

    protected void Start()
    {
        pose = new BlazePose(options);

        drawer = new BlazePoseDrawer(Camera.main, gameObject.layer, containerView);

        cancellationToken = this.GetCancellationTokenOnDestroy();

        GetComponent<WebCamInput>().OnTextureUpdate.AddListener(OnTextureUpdate);
    }

    protected void OnDestroy()
    {
        GetComponent<WebCamInput>().OnTextureUpdate.RemoveListener(OnTextureUpdate);
        pose?.Dispose();
        drawer?.Dispose();
    }

    protected void OnTextureUpdate(Texture texture)
    {
        if (runBackground)
        {
            if (task.Status.IsCompleted())
            {
                task = InvokeAsync(texture);
                AfterInvoke(texture);
            }
        }
        else
        {
            Invoke(texture);
            AfterInvoke(texture);
        }
    }

    public virtual void AfterInvoke(Texture texture) { }


    protected void Update()
    {
        drawer.DrawPoseResult(poseResult);

        if (landmarkResult != null && landmarkResult.score > 0.2f)
        {
            drawer.DrawCropMatrix(pose.CropMatrix);
            drawer.DrawLandmarkResult(landmarkResult, visibilityThreshold, canvas.planeDistance);
            if (options.landmark.useWorldLandmarks)
            {
                drawer.DrawWorldLandmarks(landmarkResult, visibilityThreshold);
            }
        }
    }

    protected void Invoke(Texture texture)
    {
        landmarkResult = pose.Invoke(texture);
        poseResult = pose.PoseResult;
        if (pose.LandmarkInputTexture != null)
        {
            debugView.texture = pose.LandmarkInputTexture;
        }
        if (landmarkResult != null && landmarkResult.SegmentationTexture != null)
        {
            segmentationView.texture = landmarkResult.SegmentationTexture;
        }
    }

    protected async UniTask<bool> InvokeAsync(Texture texture)
    {
        landmarkResult = await pose.InvokeAsync(texture, cancellationToken);
        poseResult = pose.PoseResult;
        if (pose.LandmarkInputTexture != null)
        {
            debugView.texture = pose.LandmarkInputTexture;
        }
        if (landmarkResult != null && landmarkResult.SegmentationTexture != null)
        {
            segmentationView.texture = landmarkResult.SegmentationTexture;
        }
        return landmarkResult != null;
    }
}
