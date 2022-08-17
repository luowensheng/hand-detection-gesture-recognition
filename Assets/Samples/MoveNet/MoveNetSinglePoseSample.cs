using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TensorFlowLite;
using TensorFlowLite.MoveNet;

[RequireComponent(typeof(WebCamInput))]
public class MoveNetSinglePoseSample : MonoBehaviour
{
    [SerializeField]
    protected MoveNetSinglePose.Options options = default;

    [SerializeField]
    protected RectTransform cameraView = null;

    [SerializeField]
    protected bool runBackground = false;

    [SerializeField, Range(0, 1)]
    protected float threshold = 0.3f;

    protected MoveNetSinglePose moveNet;
    protected MoveNetPose pose;
    protected MoveNetDrawer drawer;

    protected UniTask<bool> task;
    protected CancellationToken cancellationToken;

    protected void Start()
    {
        moveNet = new MoveNetSinglePose(options);
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
        if (pose != null)
        {
            drawer.DrawPose(pose, threshold);
        }
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
    
    protected void Invoke(Texture texture)
    {
        moveNet.Invoke(texture);
        pose = moveNet.GetResult();
    }

    protected async UniTask<bool> InvokeAsync(Texture texture)
    {
        await moveNet.InvokeAsync(texture, cancellationToken);
        pose = moveNet.GetResult();
        return true;
    }
}
