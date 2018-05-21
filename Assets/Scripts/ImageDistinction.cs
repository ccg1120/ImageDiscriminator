using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageDistinction : MonoBehaviour {


		
    /// <summary>
    /// 이미지 byte[]를 이용한 판별 
    /// </summary>
    /// <param name="imagebytes1"></param>
    /// <param name="imagebytes2"></param>
    /// <returns></returns>
    public static bool ImageDistinctionFunction(byte[] imagebytes1,byte[] imagebytes2)
    {
        for (int index = 0; index < imagebytes1.Length; index++)
        {
            if(imagebytes1[index] != imagebytes2[index])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 이미지 Color[]를 이용한 판별
    /// </summary>
    /// <param name="imagebytes1"></param>
    /// <param name="imagebytes2"></param>
    /// <returns></returns>
    public static bool ImageDistinctionFunction(Color[] imagebytes1, Color[] imagebytes2)
    {
        for (int index = 0; index < imagebytes1.Length; index++)
        {
            if (imagebytes1[index] != imagebytes2[index])
            {
                return false;
            }
        }
        return true;
    }

}
