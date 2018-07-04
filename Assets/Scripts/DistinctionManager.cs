using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;
//using TimerChecker;

public class DistinctionManager : MonoBehaviour {


    public static Data[] DataArray = new Data[2];

    private static int currentSwitchingNum = 0;

    public Texture2D[] MP4textureArray;
    public Texture2D[] TStextureArray;

    public static List<Color[]> MP4ColorsList = new List<Color[]>();
    public static List<Color[]> TSColorsList = new List<Color[]>();
    
    public static string[] MP4ImageFileNameArray;
    public static string[] TSImageFileNameArray;

    public static  string MP4Path = @"E:\Study\ImageDiscriminatorTestSample\MP4\";
    public static string TSPath = @"E:\Study\ImageDiscriminatorTestSample\TS\";


    private static int MP4ChunkCount = 0;
    private static int TSChunkCount = 0;
            
    private static int minFileCount = 0;
    private static int minChunkCount = 0;

    private static int currentChunkCount = 0;
    private static int currentJudgeCount = 0;
    private static int currentJudgeTaskCount = 0;

    public enum MovieType
    {
        MP4,
        TS
    }

    private static readonly int MaxLoadImageCount = 50;
    private static readonly int HalfLoadImageCount = 25;
    private static readonly string[] ImageType = new string[] { ".jpg",".png"};

    private static object lock_LoadPath = new object();
    private static object lock_Judge = new object();

    private static Task[] T_JudgeArray = null;
    

    private static Task[] T_LoadPath2ImageByte = null;

    private static Task testTask;

    private static bool judgeMainCheck = false;
    // Use this for initialization
    void Start () {
        testTask = new Task(()=>TestThread());
        GetALLFileName();
        TempTexture2DCreate();

        //버퍼2개
        DataArray = new Data[2];
        for (int index = 0; index < DataArray.Length; index++)
        {
            DataArray[index] = new Data();
            //비교 영상 갯수 ,MP4, ts 2종류
            DataArray[index].ImageByteDataList = new List<byte[]>[2];
            for (int index2 = 0; index2 < DataArray[index].ImageByteDataList.Length; index2++)
            {
                DataArray[index].ImageByteDataList[index2] = new List<byte[]>();
            }
        }
        //테스크 할당 
        T_JudgeArray = new Task[2];
        for (int index = 0; index < T_JudgeArray.Length; index++)
        {
            int num = index;
            T_JudgeArray[index] = new Task(() => TaskImage(num * HalfLoadImageCount));
        }

        T_LoadPath2ImageByte = new Task[2];
        for (int index = 0; index < T_LoadPath2ImageByte.Length; index++)
        {
            T_LoadPath2ImageByte[index] = new Task(() => LoadPath2ImageByte());
            //T_LoadPath2ImageByte[index].Start();
        }

        //Task.WaitAll(T_LoadPath2ImageByte);

        

        Debug.Log("Task Load Done!!");

        //TaskJudge();

        
    }

    private void Update()
    {
        if (currentJudgeTaskCount >= minChunkCount)
        {
            Debug.Log("=====Done!!!!!!!!!!!!");
            return;
        }

        Debug.Log("DataArray[0] : " + DataArray[0].ImageByteDataList[0].Count);
        Debug.Log("DataArray[0] : " + DataArray[0].ImageByteDataList[1].Count);
        //Debug.Log("DataArray[1] : " + DataArray[1].ImageByteDataList[0].Count);
        //Debug.Log("Thread state0 : " + T_LoadPath2ImageByte[0].Status);
        //Debug.Log("Thread state1 : " + T_LoadPath2ImageByte[1].Status);

        for (int index = 0; index < DataArray.Length; index++)
        {
            Debug.Log(index + " , state : " + DataArray[index].CurrentDataState);

            if (DataArray[index].CurrentDataState == Data.DataState.Clear)
            {
                for (int index2 = 0; index2 < T_LoadPath2ImageByte.Length; index2++)
                {
                    Debug.Log(index2 + " , Thread state : " + T_LoadPath2ImageByte[index2].Status + ", switch num : " + currentSwitchingNum);
                    if (T_LoadPath2ImageByte[index2].Status == TaskStatus.Created)
                    {
                        Debug.Log(index + " Thread Start " + index2 + ", Status : " + T_LoadPath2ImageByte[index2].Status);
                        currentSwitchingNum = index;
                        DataArray[index].CurrentDataState = Data.DataState.Run;
                        
                        T_LoadPath2ImageByte[index2].Start();
                        break;
                    }
                    else if(T_LoadPath2ImageByte[index2].Status == TaskStatus.RanToCompletion)
                    {
                        T_LoadPath2ImageByte[index2] = null;
                        T_LoadPath2ImageByte[index2] = new Task(()=>LoadPath2ImageByte());
                    }
                }
            }
            //if (DataArray[index].CurrentDataState == Data.DataState.Stay)
            //{
            //    Debug.Log("judgeMainCheck : " + judgeMainCheck);
            //    if (!judgeMainCheck)
            //    {
            //        TaskJudge();
            //    }
            //}
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (DataArray[currentJudgeTaskCount].CurrentDataState == Data.DataState.Stay)
            {
                Debug.Log("judgeMainCheck : " + judgeMainCheck);
                if (!judgeMainCheck)
                {
                    TaskJudge(currentJudgeTaskCount);
                }
            }
            currentJudgeTaskCount++;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            if(testTask.Status == TaskStatus.Created)
            {
                testTask.Start();
            }
            else if(testTask.Status == TaskStatus.RanToCompletion)
            {
                testTask = null;
                testTask = new Task(()=> TestThread());
            }
        }
    }


    private void TestThread()
    {
        Debug.Log("TestThread");
    }
    private void ColorClear()
    {
        Debug.Log("Count 1:" + MP4ColorsList.Count);
        Debug.Log("Count 2:" + TSColorsList.Count);
        //MP4ColorsList
        for (int index = MP4ColorsList.Count - 1; index >= 0; index--)
        {
            MP4ColorsList[index] = null;
            MP4ColorsList.RemoveAt(index);
        }
        MP4ColorsList.Clear();

        for (int index = TSColorsList.Count - 1; index >= 0; index--)
        {
            TSColorsList[index] = null;
            TSColorsList.RemoveAt(index);
        }
        TSColorsList.Clear();

        Debug.Log("MP4ColorsList.Count : " + MP4ColorsList.Count);
        Debug.Log("TSColorsList.Count : " + TSColorsList.Count);
    }

    private void TaskJudge(int num)
    {
        Debug.Log("TaskJudge Start");
        judgeMainCheck = true;

        Texture2DLoadImage(num);

        TimerChecker.TimeChecker.StartTimer(0," START  Image");


        for (int index = 0; index < T_JudgeArray.Length; index++)
        {
            T_JudgeArray[index].Start();
        }
        Task.WaitAll(T_JudgeArray);

        
        //꼭 이렇게 초기화를 해야되는가? 재사용을 할수 없는건가?
        for (int index = 0; index < T_JudgeArray.Length; index++)
        {
            int count = index;
            T_JudgeArray[index] = new Task(() => TaskImage(count * HalfLoadImageCount));
        }

        TimerChecker.TimeChecker.EndTimer(0);
        

        ColorClear();

        DataArray[num].CurrentDataState = Data.DataState.Clear;
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        currentJudgeCount++;
        judgeMainCheck = false;
    }
    
    private void DestroyTextureAll()
    {
        for (int index = MaxLoadImageCount -1 ; index >= 0; index--)
        {
            Destroy(MP4textureArray[index]);
            Destroy(TStextureArray[index]);
        }
    }

    private static void TaskImage(int num)
    {
        int innum;
        
        lock (lock_Judge)
        {
            Debug.Log(lock_Judge);
            innum = num;
            Debug.Log("TaskImage + " + innum);
        }
        
            
        for (int index = 0; index < HalfLoadImageCount; index++)
        {
            Debug.Log(innum + index);
            if (!ImageDistinction.ImageDistinctionFunction(MP4ColorsList[innum + index], TSColorsList[index]))
            {

                Debug.LogError("Image X" + (innum + index));
                continue;
            }
        }
        Debug.Log("Task Done!!!! "+ Thread.CurrentThread.ManagedThreadId);
    }
    

    private void TempTexture2DCreate()
    {
        MP4textureArray = new Texture2D[MaxLoadImageCount];
        TStextureArray = new Texture2D[MaxLoadImageCount];

        for (int index = 0; index < MaxLoadImageCount; index++)
        {
            MP4textureArray[index] = new Texture2D(1, 1);
            TStextureArray[index] = new Texture2D(1, 1);
        }

        //MP4ColorsList = new Color[MaxLoadImageCount][];
        //TSColorsList = new Color[MaxLoadImageCount][];
    }

    private void Texture2DLoadImage(int type)
    {
        DataArray[type].CurrentDataState = Data.DataState.Used;
        for (int index = 0; index < MaxLoadImageCount; index++)
        {
            MP4textureArray[index].LoadImage(DataArray[type].ImageByteDataList[0][index]);
            TStextureArray[index].LoadImage(DataArray[type].ImageByteDataList[1][index]);

            Color[] tempmp4 = MP4textureArray[index].GetPixels();
            MP4ColorsList.Add(tempmp4);
            Color[] tempts = TStextureArray[index].GetPixels();
            TSColorsList.Add(tempts);

            MP4textureArray[index] = null;
            TStextureArray[index] = null;
        }

        for (int index = 0; index < MaxLoadImageCount; index++)
        {
            Destroy(MP4textureArray[index]);
            Destroy(TStextureArray[index]);
        }

    }
    
    private static void ClearByteList(int num)
    {
        Debug.Log("ClearByteList : "+ DataArray[num].CurrentDataState);
        //if(DataArray[num].CurrentDataState == Data.DataState.Used)
        //{
            for (int index = 0; index < DataArray[num].ImageByteDataList.Length; index++)
            {
                for (int index2 = 0; index2 < DataArray[num].ImageByteDataList[index].Count; index2++)
                {
                    DataArray[num].ImageByteDataList[index][index2] = null;
                }
                DataArray[num].ImageByteDataList[index].Clear();
                DataArray[num].CurrentDataState = Data.DataState.Clear;
            }
        //}
    }

    private static void LoadPath2ImageByte()
    {
        int intype = -1;
        int num;
        
        lock (lock_LoadPath)
        {
            intype = currentSwitchingNum;
            Debug.Log("LoadPath2ImageByte : type :: " + intype + ", switch num : " + currentSwitchingNum );
            ClearByteList(intype);
            num = currentChunkCount * MaxLoadImageCount;
            
            currentChunkCount++;
            
            Debug.Log("Thread ID :" + Thread.CurrentThread.ManagedThreadId + ", type : " + intype);
        }

        Debug.Log("LoadPath2ImageByte : intype :: " + intype);


        for (int index = 0; index < MaxLoadImageCount; index++)
        {
            DataArray[intype].ImageByteDataList[0].Add(LoadImage(MP4Path + MP4ImageFileNameArray[num + index]));
            DataArray[intype].ImageByteDataList[1].Add(LoadImage(TSPath + TSImageFileNameArray[num + index]));
        }

        DataArray[intype].CurrentDataState = Data.DataState.Stay;
        return;
    }

    private static byte[] LoadImage(string path)
    {
        return File.ReadAllBytes(path);
    }

    private bool CheckImageLenght()
    {
        if(MP4ImageFileNameArray.Length != TSImageFileNameArray.Length)
        {
            return false;
        }
        return true;
    }

    #region GetFilename
    private static void GetALLFileName()
    {
        Task<bool> T_GetMP4FileName = new Task<bool>(GetMP4FileName);
        Task<bool> T_GetTSFileName = new Task<bool>(GetTSFileName);
        T_GetMP4FileName.Start();
        T_GetTSFileName.Start();
        
        Task.WaitAll(T_GetMP4FileName, T_GetTSFileName);

        if(MP4ChunkCount > TSChunkCount)
        {
            minFileCount = TSChunkCount;
        }
        else
        {
            minFileCount = MP4ChunkCount;
        }
        Debug.Log("Minchungk : " + minFileCount);

        minChunkCount = (int)(minFileCount / MaxLoadImageCount);
    }
    private static  void GetALLFileName(MovieType type, string path)
    {
        DirectoryInfo info = new DirectoryInfo(path);

        if(!info.Exists)
        {
            info.Create();
        }
        int filecount = info.GetFiles().Length;

        switch (type)
        {
            case MovieType.MP4:
                MP4ImageFileNameArray = new string[filecount];
                break;
            case MovieType.TS:
                TSImageFileNameArray = new string[filecount];
                break;
        }

        for (int index = 0; index < info.GetFiles().Length; index++)
        {
            for (int index2 = 0; index2 < ImageType.Length; index2++)
            {
                if(info.GetFiles()[index].Name.Contains(ImageType[index2]))
                {
                    Debug.Log("Image type Error :" + info.GetFiles()[index].FullName);
                    return;
                }
            }
            switch (type)
            {
                case MovieType.MP4:
                    MP4ImageFileNameArray[index] = info.GetFiles()[index].Name;
                    break;
                case MovieType.TS:
                    TSImageFileNameArray[index] = info.GetFiles()[index].Name;
                    break;
            }
        }
    }
    private static bool GetMP4FileName()
    {
        DirectoryInfo info = new DirectoryInfo(MP4Path);
        if (!info.Exists)
        {
            Debug.Log("GetMP4FileName Error :" + MP4Path);
            return false;
        }

        int filecount = info.GetFiles().Length;
        MP4ImageFileNameArray = new string[filecount];
        FileInfo[] fileInfos = info.GetFiles();

        for (int index = 0; index < filecount; index++)
        {
            if (fileInfos[index].Name.Contains(ImageType[0]) ||
                fileInfos[index].Name.Contains(ImageType[0].ToUpper())||
                fileInfos[index].Name.Contains(ImageType[1]) ||
                fileInfos[index].Name.Contains(ImageType[1].ToUpper())
                )
            {
                MP4ImageFileNameArray[index] = fileInfos[index].Name;
            }
        }

        MP4ChunkCount = fileInfos.Length / MaxLoadImageCount;

        Debug.Log("MP4 File Count : " + filecount);
        return true;
    }
    private static bool GetTSFileName()
    {
        DirectoryInfo info = new DirectoryInfo(TSPath);
        if (!info.Exists)
        {
            Debug.Log("GetTSFileName Error :" + TSPath);
            return false;
        }

        int filecount = info.GetFiles().Length;
        FileInfo[] fileInfos = info.GetFiles();

        TSImageFileNameArray = new string[filecount];

        for (int index = 0; index < filecount; index++)
        {
            if (fileInfos[index].Name.Contains(ImageType[0]) ||
                fileInfos[index].Name.Contains(ImageType[0].ToUpper()) ||
                fileInfos[index].Name.Contains(ImageType[1]) ||
                fileInfos[index].Name.Contains(ImageType[1].ToUpper())
                )
            {
                TSImageFileNameArray[index] = fileInfos[index].Name;
            }
        }

        TSChunkCount = fileInfos.Length / MaxLoadImageCount;

        return true;
    }
    #endregion

}
