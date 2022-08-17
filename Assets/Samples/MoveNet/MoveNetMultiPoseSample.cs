using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TensorFlowLite;
using TensorFlowLite.MoveNet;

[RequireComponent(typeof(WebCamInput))]
public class MoveNetMultiPoseSample : MonoBehaviour
{
    [SerializeField]
    MoveNetMultiPose.Options options = default;

    [SerializeField]
    protected RectTransform cameraView = null;

    [SerializeField]
    protected bool runBackground = true;

    [SerializeField, Range(0, 1)]
    protected float threshold = 0.3f;

    protected MoveNetMultiPose moveNet;
    protected MoveNetPoseWithBoundingBox[] poses;
    protected MoveNetDrawer drawer;

    protected UniTask<bool> task;
    protected CancellationToken cancellationToken;

    protected void Start()
    {
        moveNet = new MoveNetMultiPose(options);
        drawer = new MoveNetDrawer(Camera.main, cameraView);

        cancellationToken = this.GetCancellationTokenOnDestroy();

        var webCamInput = GetComponent<WebCamInput>();
        webCamInput.OnTextureUpdate.AddListener(OnTextureUpdate);
    }

    protected void OnDestroy()
    {
        var webCamInput = GetComponent<WebCamInput>();
        webCamInput.OnTextureUpdate.RemoveListener(OnTextureUpdate);
        moveNet?.Dispose();
        drawer?.Dispose();
    }

    protected void Update()
    {
        if (poses != null)
        {
            foreach (var pose in poses)
            {
                drawer.DrawPose(pose, threshold);
            }
        }
    }
    public virtual void AfterInvoke(Texture texture) { }

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

    protected void Invoke(Texture texture)
    {
        moveNet.Invoke(texture);
        poses = moveNet.GetResults();
    }

    protected async UniTask<bool> InvokeAsync(Texture texture)
    {
        await moveNet.InvokeAsync(texture, cancellationToken);
        poses = moveNet.GetResults();
        return true;
    }
}
