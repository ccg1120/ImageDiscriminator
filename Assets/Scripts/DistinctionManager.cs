using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;

using TimerChecker;

public class DistinctionManager : MonoBehaviour {

    public List<byte[]> MP4ImageByteList = new List<byte[]>(); 
    public List<byte[]> TSImageByteList = new List<byte[]>();

    public Texture2D[] MP4textureArray;
    public Texture2D[] TStextureArray;

    public List<Color[]> MP4ColorsList = new List<Color[]>();
    public List<Color[]> TSColorsList = new List<Color[]>();

    public string[] MP4ImageFileNameArray;
    public string[] TSImageFileNameArray;

    public string MP4Path = @"E:\Study\ImageDiscriminatorTestSample\MP4\";
    public string TSPath = @"E:\Study\ImageDiscriminatorTestSample\TS\";

    private int MP4ChunkCount = 0;
    private int TSChunkCount = 0;

    private int MinChunkCount = 0;

    private bool getFileState = false;
    private bool Judge = true;


    public enum MovieType
    {
        MP4,
        TS
    }

    private readonly int MaxLoadImageCount = 100;
    private readonly string[] ImageType = new string[] { ".jpg",".png"};

    // Use this for initialization
    void Start () {
        
        GetALLFileName();
        TempTexture2DCreate();

        LoadPath2ImageByte(0);

        //Task TestTask = new Task(()=> Texture2DLoadImage(0,0));
        //TestTask.Start();
        Texture2DLoadImage(0, MaxLoadImageCount);
        Debug.Log(MP4textureArray[0].GetPixels().Length);
        Debug.Log(TStextureArray[0].GetPixels().Length);

        
        TimeChecker.StartTimer(1, "Start");
        
        TaskJudge();


        TimeChecker.EndTimer(1);

        Debug.Log("result : " +Judge);

    }

    private void TaskJudge()
    {
        int half =(int)( MaxLoadImageCount * 0.5f);
        for (int index = 0; index <  MinChunkCount; index+=MaxLoadImageCount)
        {
            Texture2DLoadImage(index, MaxLoadImageCount);
            Task judge1 = new Task(() => TaskImage(half));
            Task judge2 = new Task(() => TaskImage(MaxLoadImageCount));
            judge1.Start();
            judge2.Start();
            Task.WaitAll(judge1,judge2);
        }
    }

    private void TaskImage(int num)
    {
        for (int index = 0; index < num; index++)
        {
            if(!ImageDistinction.ImageDistinctionFunction(MP4ColorsList[index], TSColorsList[index]))
            {
                Debug.LogError("Image X");
                return;
            }
        }
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
    }
    private void Texture2DLoadImage(int num, int lenght)
    {
        Debug.Log("check1 " + MP4ImageByteList.Count);
        Debug.Log("check2 " + TSImageByteList.Count);
        MP4ColorsList.Clear();
        TSColorsList.Clear();
        for (int index = 0; index < lenght; index++)
        {
            MP4textureArray[index].LoadImage(MP4ImageByteList[index]);
            MP4ColorsList.Add(MP4textureArray[index].GetPixels());
            TStextureArray[index].LoadImage(TSImageByteList[index]);
            TSColorsList.Add(TStextureArray[index].GetPixels());
        }
        
    }

    private void LoadPath2ImageByte(int num)
    {
        TimeChecker.StartTimer(0,"Load Files");
        for (int index = 0; index < MaxLoadImageCount; index++)
        {
            MP4ImageByteList.Add(LoadImage(MP4Path + MP4ImageFileNameArray[index]));
            TSImageByteList.Add(LoadImage(TSPath + TSImageFileNameArray[index]));
        }
        TimeChecker.EndTimer(0);
    }
    private byte[] LoadImage(string path)
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
    private void GetALLFileName()
    {
        Task<bool> T_GetMP4FileName = new Task<bool>(GetMP4FileName);
        Task<bool> T_GetTSFileName = new Task<bool>(GetTSFileName);
        T_GetMP4FileName.Start();
        T_GetTSFileName.Start();
        
        Task.WaitAll(T_GetMP4FileName, T_GetTSFileName);

        if(MP4ChunkCount > TSChunkCount)
        {
            MinChunkCount = TSChunkCount;
        }
        else
        {
            MinChunkCount = MP4ChunkCount;
        }
        Debug.Log("Minchungk : " + MinChunkCount);
    }
    private void GetALLFileName(MovieType type, string path)
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
    private bool GetMP4FileName()
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
    private bool GetTSFileName()
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
