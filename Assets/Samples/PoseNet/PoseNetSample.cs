﻿using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(WebCamInput))]
public class PoseNetSample : MonoBehaviour
{
    [SerializeField, FilePopup("*.tflite")]
    protected string fileName = "posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite";

    [SerializeField]
    protected RectTransform containerView = null;

    [SerializeField, Range(0f, 1f)]
    protected float threshold = 0.5f;

    [SerializeField, Range(0f, 1f)]
    protected float lineThickness = 0.5f;

    [SerializeField]
    protected bool runBackground;

    protected PoseNet poseNet;
    protected readonly Vector3[] rtCorners = new Vector3[4];
    protected PrimitiveDraw draw;
    protected UniTask<bool> task;
    protected PoseNet.Result[] results;
    protected CancellationToken cancellationToken;

    protected void Start()
    {
        poseNet = new PoseNet(fileName);

        draw = new PrimitiveDraw(Camera.main, gameObject.layer)
        {
            color = Color.green,
        };

        cancellationToken = this.GetCancellationTokenOnDestroy();

        var webCamInput = GetComponent<WebCamInput>();
        webCamInput.OnTextureUpdate.AddListener(OnTextureUpdate);
    }

    protected void OnDestroy()
    {
        var webCamInput = GetComponent<WebCamInput>();
        webCamInput.OnTextureUpdate.RemoveListener(OnTextureUpdate);

        poseNet?.Dispose();
        draw?.Dispose();
    }

    protected void Update()
    {
        DrawResult(results);
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

    protected void DrawResult(PoseNet.Result[] results)
    {
        if (results == null || results.Length == 0)
        {
            return;
        }

        var rect = containerView;
        rect.GetWorldCorners(rtCorners);
        Vector3 min = rtCorners[0];
        Vector3 max = rtCorners[2];

        var connections = PoseNet.Connections;
        int len = connections.GetLength(0);
        for (int i = 0; i < len; i++)
        {
            var a = results[(int)connections[i, 0]];
            var b = results[(int)connections[i, 1]];
            if (a.confidence >= threshold && b.confidence >= threshold)
            {
                draw.Line3D(
                    MathTF.Lerp(min, max, new Vector3(a.x, 1f - a.y, 0)),
                    MathTF.Lerp(min, max, new Vector3(b.x, 1f - b.y, 0)),
                    lineThickness
                );
            }
        }

        draw.Apply();
    }

    protected void Invoke(Texture texture)
    {
        poseNet.Invoke(texture);
        results = poseNet.GetResults();
     //   cameraView.material = poseNet.transformMat;
    }

    protected async UniTask<bool> InvokeAsync(Texture texture)
    {
        results = await poseNet.InvokeAsync(texture, cancellationToken);
       // cameraView.material = poseNet.transformMat;
        return true;
    }
}
