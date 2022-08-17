using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TensorFlowLite;
using UnityEngine;

[RequireComponent(typeof(WebCamInput))]
public class AgeGenderDetection : BaseImagePredictor<float>
{
    public float[,] outputs0;
    private float[,,] intput0e;
    protected readonly int width;
    protected int height;
    private int nClasses = 116;
    public readonly (int, int) resizedShape = (200, 200);
    private UniTask<bool> task;
    public (int age, string name) results = (-1, "");

    public AgeGenderDetection(string filename = "age_gender_model_tflite.tflite", int width = 200, int height = 200) : base(modelPath: filename, Accelerator.GPU)
    {
        int dim0 = 1;
        int dim1 = 3;
        outputs0 = new float[dim0, dim1];
        intput0e = new float[resizedShape.Item1, resizedShape.Item2, 3];
        this.width = width;
        this.height = height;
    }
    public static IEnumerable<int> Range(int start, int stop, int inc = 1)
    {
        for (int i = start; i < stop; i += inc)
        {
            yield return i;
        }
    }
    public override void Invoke(Texture inputTex)
    {
        //.Resized(resizedShape)
        ToTensor(inputTex, intput0e);

        interpreter.SetInputTensorData(0, this.intput0e);
        interpreter.Invoke();
        interpreter.GetOutputTensorData(0, this.outputs0);
    }

    public async UniTask<bool> InvokeAsync(Texture inputTex, CancellationToken cancellationToken)
    {
        ToTensor(inputTex, inputTensor);
        await UniTask.SwitchToThreadPool();

        interpreter.SetInputTensorData(0, this.intput0e);
        interpreter.Invoke();
        interpreter.GetOutputTensorData(0, this.outputs0);
        await UniTask.SwitchToMainThread(PlayerLoopTiming.Update, cancellationToken);

        return true;
    }



    public void predictAsync(Texture inputTex, CancellationToken cancellationToken)
    {
        if (task.Status.IsCompleted())
        {
            task = InvokeAsync(inputTex, cancellationToken);

            Debug.Log("invoked");
        }

    }

    public void Predict(Texture inputTex)
    {
        Invoke(inputTex);
        results = GetResults();
    }


    public (int, string) GetResults()
    {
        int age = (int)(this.outputs0[0, 0] * nClasses);
        string gender = (this.outputs0[0, 1] > this.outputs0[0, 2]) ? "male" : "female";
        return (age, gender);
    }

    private void OnDestroy()
    {
        base.Dispose();
    }

}
