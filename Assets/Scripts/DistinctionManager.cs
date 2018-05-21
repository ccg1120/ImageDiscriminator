using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;

using TimerChecker;

public class DistinctionManager : MonoBehaviour {

    public int MaxLoadImageCount = 10;

    public List<byte[]> Image1ByteList = new List<byte[]>(); 
    public List<byte[]> Image2ByteList = new List<byte[]>();
    public Texture2D[] texture2d1Array;
    public Texture2D[] texture2d2Array;

    public string SampelPath = @"E:\Study\ImageDiscriminatorTestSample\test.jpg";
    public string SampelPath2 = @"E:\Study\ImageDiscriminatorTestSample\test2.png";

    public string MP4Path = @"E:\Study\ImageDiscriminatorTestSample\MP4\";
    public string TSPath = @"E:\Study\ImageDiscriminatorTestSample\TS\";

    // Use this for initialization
    void Start () {
        
        //TempTexture2DCreate();

        byte[] test = LoadImage(SampelPath);

        Debug.Log(test.Length);
        Debug.Log(test);

        Texture2D tt = new Texture2D(1,1);

        tt.LoadImage(test);
        

        Color[] colors = tt.GetPixels();
        Debug.Log(colors.Length);

        //Task task = new Task(TestTask);
        //task.Start();

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Reset();
        sw.Start();

        TimeChecker.StartTimer(0,"Total main");

        Task<byte[]>[] TaskArray;

        Task<byte[]>[] TaskArray2;
        TaskArray = new Task<byte[]>[10];
        TaskArray2 = new Task<byte[]>[10];
        Debug.Log("Task Start ");
        TimeChecker.StartTimer(1, "TaskArray");



        for (int index = 0; index < TaskArray.Length; index++)
        {
            TaskArray[index] = new Task<byte[]>(()=> LoadImage(SampelPath));
            TaskArray[index].Start();
        }
        Task.WhenAll(TaskArray);
        for (int index = 0; index < TaskArray.Length; index++)
        {
            Image1ByteList.Add(TaskArray[index].Result);
        }
        TimeChecker.EndTimer(1);
        TimeChecker.StartTimer(2, "TaskArray2");
        for (int index = 0; index < TaskArray2.Length; index++)
        {
            TaskArray2[index] = new Task<byte[]>(() => LoadImage(SampelPath2));
            TaskArray2[index].Start();
        }
        Task.WhenAll(TaskArray2);
        for (int index = 0; index < TaskArray2.Length; index++)
        {
            Image2ByteList.Add(TaskArray2[index].Result);
        }
        TimeChecker.EndTimer(2);
        Debug.Log("Task End ");
        sw.Stop();
         
        TimeChecker.EndTimer(0);
        Debug.Log("File Load Done!!");


        TempTexture2DCreate();
    

    }

    private void TempTexture2DCreate()
    {
        Debug.Log("List Count : Image1ByteList: " + Image1ByteList.Count);
        Debug.Log("List Count : Image2ByteList: " + Image2ByteList.Count);

        texture2d1Array = new Texture2D[MaxLoadImageCount];
        texture2d2Array = new Texture2D[MaxLoadImageCount];
        for (int index = 0; index < MaxLoadImageCount; index++)
        {
            texture2d1Array[index] = new Texture2D(1, 1);
            texture2d1Array[index].LoadImage(Image1ByteList[index]);

            texture2d2Array[index] = new Texture2D(1, 1);
            texture2d2Array[index].LoadImage(Image2ByteList[index]);
        }
    }

    private byte[] LoadImage(string path)
    {
        return File.ReadAllBytes(path);
    }

   
}
