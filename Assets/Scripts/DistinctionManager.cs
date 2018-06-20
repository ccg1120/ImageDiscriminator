using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;

//using TimerChecker;

public class DistinctionManager : MonoBehaviour {

    public static  List<byte[]> MP4ImageByteList1 = new List<byte[]>(); 
    public static List<byte[]> TSImageByteList1 = new List<byte[]>();

    public static List<byte[]> MP4ImageByteList2 = new List<byte[]>();
    public static List<byte[]> TSImageByteList2= new List<byte[]>();

    private static int currentType = 0;

    public static Texture2D[] MP4textureArray;
    public static Texture2D[] TStextureArray;

    public List<Color[]> MP4ColorsList = new List<Color[]>();
    public List<Color[]> TSColorsList = new List<Color[]>();

    //public Color[][] MP4ColorsList;
    //public Color[][] TSColorsList;

    public static string[] MP4ImageFileNameArray;
    public static string[] TSImageFileNameArray;

    public static  string MP4Path = @"E:\Study\ImageDiscriminatorTestSample\MP4\";
    public static string TSPath = @"E:\Study\ImageDiscriminatorTestSample\TS\";


    private static int MP4ChunkCount = 0;
    private static int TSChunkCount = 0;
            
    private static int minFileCount = 0;
    private static int minChunkCount = 0;
            
    private static int currentChunkCount = 0;
            
    private static int beforeChunkCount = 0;
            
    private static bool Judge = true;

    
    public enum MovieType
    {
        MP4,
        TS
    }

    private static readonly int MaxLoadImageCount = 50;
    private static readonly int HalfLoadImageCount = 25;
    private static readonly string[] ImageType = new string[] { ".jpg",".png"};

    // Use this for initialization
    void Start () {

        GetALLFileName();
        TempTexture2DCreate();
        int gettype = currentType;
        Task T_LoadPath2ImageByte1 = new Task(()=> LoadPath2ImageByte(minChunkCount * MaxLoadImageCount, gettype));
        T_LoadPath2ImageByte1.Start();

        minChunkCount++;
        currentType = 1;
        int gettype2 = currentType;
        Task T_LoadPath2ImageByte2 = new Task(() => LoadPath2ImageByte(minChunkCount * MaxLoadImageCount, gettype2));
        T_LoadPath2ImageByte2.Start();
        currentType = 0;

        Task.WaitAll(T_LoadPath2ImageByte1, T_LoadPath2ImageByte2);

        Debug.Log("Task Load Done!!");

        //TimeChecker.StartTimer(1, "Start");

        TaskJudge();


        ////TimeChecker.EndTimer(1);

        //Debug.Log("result : " + Judge);

        //MP4ImageByteList1.Clear();
        //MP4ImageByteList2.Clear();
        //TSImageByteList1.Clear();
        //TSImageByteList2.Clear();
        ColorClear();
    }

    private void Update()
    {
        //if(currentChunkCount > beforeChunkCount)
        //{
        //    beforeChunkCount = currentChunkCount;
        //    TaskJudge();
        //}
    }

    private void ColorClear()
    {
        Debug.Log("Count :" + MP4ColorsList.Count);
        //MP4ColorsList
        for (int index = MP4ColorsList.Count - 1; index >= 0; index--)
        {
            Debug.Log("index :" + index);
            MP4ColorsList[index] = null;
            MP4ColorsList.RemoveAt(index);
        }
        MP4ColorsList.Clear();
        Debug.Log("MP4ColorsList capacity :" + MP4ColorsList.Capacity);

        for (int index = TSColorsList.Count - 1; index >= 0; index--)
        {
            TSColorsList[index] = null;
            TSColorsList.RemoveAt(index);
        }
        TSColorsList.Clear();

    }

    private void TaskJudge()
    {
        Texture2DLoadImage(currentType);
        //Task judge1 = new Task(() => TaskImage(0));
        //Task judge2 = new Task(() => TaskImage(HalfLoadImageCount));
        //judge1.Start();
        //judge2.Start();
        //Task.WaitAll(judge1, judge2);

        ColorClear();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        
        //상태 교환
        if (currentType == 0)
        {
            currentType = 1;
        }
        else if (currentType == 1)
        {
            currentType = 0;
        }
        currentChunkCount++;

        //DestroyTextureAll();
    }

    private static void DestroyTextureAll()
    {
        for (int index = MaxLoadImageCount -1 ; index >= 0; index--)
        {
            Destroy(MP4textureArray[index]);
            Destroy(TStextureArray[index]);
        }
    }

    private void TaskImage(int num)
    {
        for (int index = 0; index < HalfLoadImageCount; index++)
        {
            if(!ImageDistinction.ImageDistinctionFunction(MP4ColorsList[num + index], TSColorsList[index]))
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

        //MP4ColorsList = new Color[MaxLoadImageCount][];
        //TSColorsList = new Color[MaxLoadImageCount][];
    }

    private void Texture2DLoadImage(int type)
    {
        Debug.Log("check1 " + MP4ImageByteList1.Count);
        Debug.Log("check2 " + TSImageByteList1.Count);

        Debug.Log("type : "+type);
        ColorClear();
        for (int index = 0; index < MaxLoadImageCount; index++)
        {
            switch(type)
            {
                case 0:
                    MP4textureArray[index].LoadImage(MP4ImageByteList1[index]);
                    TStextureArray[index].LoadImage(TSImageByteList1[index]);
                    break;

                case 1:
                    MP4textureArray[index].LoadImage(MP4ImageByteList2[index]);
                    TStextureArray[index].LoadImage(TSImageByteList2[index]);
                    break;
            }
            Color[] mp4color = MP4textureArray[index].GetPixels();
            
            //MP4ColorsList[index] = new Color[mp4color.Length];
            //MP4ColorsList[index] = mp4color;

            MP4ColorsList.Add(mp4color);
            Color[] tscolor = TStextureArray[index].GetPixels();
            

            //TSColorsList[index] = new Color[mp4color.Length];
            //TSColorsList[index] = tscolor;
            TSColorsList.Add(tscolor);

            Destroy(MP4textureArray[index]);
            Destroy(TStextureArray[index]);

            MP4textureArray[index] = null;
            TStextureArray[index] = null;
        }
        //DestroyTextureAll();

    }

    private static void LoadPath2ImageByte(int num, int type)
    {
        Debug.Log("LoadPath2ImageByte : type :: " + type);
        int intype = type;
        Debug.Log("LoadPath2ImageByte : intype :: " + intype);

        switch (type)
        {
            case 0:
                MP4ImageByteList1.Clear();
                TSImageByteList1.Clear();
                break;
            case 1:
                MP4ImageByteList2.Clear();
                TSImageByteList2.Clear();
                break;
        }


        for (int index = 0; index < MaxLoadImageCount; index++)
        {
            switch (type)
            {
                case 0:
                    MP4ImageByteList1.Add(LoadImage(MP4Path + MP4ImageFileNameArray[num + index]));
                    TSImageByteList1.Add(LoadImage(TSPath + TSImageFileNameArray[num + index]));
                    break;
                case 1:
                    MP4ImageByteList2.Add(LoadImage(MP4Path + MP4ImageFileNameArray[num + index]));
                    TSImageByteList2.Add(LoadImage(TSPath + TSImageFileNameArray[num + index]));
                    break;
            }
        }

        return;
        //if (currentType == 0)
        //{
        //    currentType = 1;
        //}
        //else if(currentType == 1)
        //{
        //    currentType = 0;
        //}
        //minChunkCount++;
    }

    private void LoadPath2ImageByte()
    {
        //TimeChecker.StartTimer(0, "Load Files");

        int num = currentChunkCount * MaxLoadImageCount;

        for (int index = 0; index < MaxLoadImageCount; index++)
        {
            switch (currentType)
            {
                case 0:
                    MP4ImageByteList1.Add(LoadImage(MP4Path + MP4ImageFileNameArray[num + index]));
                    TSImageByteList1.Add(LoadImage(TSPath + TSImageFileNameArray[num + index]));
                    break;
                case 1:
                    MP4ImageByteList2.Add(LoadImage(MP4Path + MP4ImageFileNameArray[num + index]));
                    TSImageByteList2.Add(LoadImage(TSPath + TSImageFileNameArray[num + index]));
                    break;
            }
        }

        currentChunkCount++;

        //TimeChecker.EndTimer(0);
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
