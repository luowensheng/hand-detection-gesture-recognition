using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public static class TextureExtentions
{
    public static Texture2D ToTexture2D(this Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        //Texture2D texture2D = new Texture2D(texture.width, texture.height, GraphicsFormat.R8G8B8_UNorm, new TextureCreationFlags());

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return texture2D;

    }

    public static Texture2D Crop(this Texture2D texture, int x, int y, int width, int height, bool mipChain = true)
    {

        Texture2D croppedTexture = new Texture2D(width, height, TextureFormat.RGB24, mipChain);

        croppedTexture.SetPixels(texture.GetPixels(x, y, width, height));

        croppedTexture.Apply();

        return croppedTexture;
    }




    public static Texture2D Resized(this Texture texture, (int width, int height) shape)
    {
        return texture.ToTexture2D().Resized(shape);
    }

    public static Texture2D Resized(this Texture2D texture, (int width, int height) shape)
    {
        int x_step = texture.width / shape.width;
        int y_step = texture.height / shape.height;

        Texture2D croppedTexture = new Texture2D(shape.width, shape.height, TextureFormat.RGB24, false);

        for (int i = 0; i < shape.width; i++)
        {
            for (int j = 0; j < shape.height; j++)
            {
                var colors = new List<Color>();
                var inds = new List<(int x, int y)>();
                for (int x = x_step * i; x < x_step * (i + 1); x++)
                {
                    for (int y = y_step * j; y < y_step * (j + 1); y++)
                    {
                        inds.Add((x, y));
                        colors.Add(texture.GetPixel(x, y));
                    }
                }
                Debug.Log(inds);
                int counter = 0;
                var arr = colors.Aggregate(new float[4] { 0, 0, 0, 0 }, (acc, cur) =>
                {
                    counter++;

                    acc[0] += cur.r;
                    acc[1] += cur.g;
                    acc[2] += cur.b;
                    acc[3] += cur.a;
                    return acc;
                });

                var color = new Color(arr[0] / counter, arr[1] / counter, arr[2] / counter, arr[3] / counter);
                croppedTexture.SetPixel(i, j, color);
            }
        }

        return croppedTexture;

    }

    public static Texture2D DrawLine(this Texture2D tex, Vector2 p1, Vector2 p2, Color col)
    {
        Vector2 t = p1;
        float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
        float ctr = 0;

        while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
        {
            t = Vector2.Lerp(p1, p2, ctr);
            ctr += frac;
            tex.SetPixel((int)t.x, (int)t.y, col);
        }
        tex.Apply();

        return tex;
    }

    public static void SaveTexture(this Texture texture, string filename = "Image")
    {
        SaveTexture(texture.ToTexture2D(), filename);
    }

    public static void SaveTexture(this Texture2D output, string filename = "Image")
    {
        byte[] bytes = output.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + filename + ".png", bytes);
    }

    public static Texture2D LoadImageRessource(string filename)
    {
        var tex = Resources.Load<Texture2D>(filename);
        return tex;
    }

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(200, 200);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }


}
