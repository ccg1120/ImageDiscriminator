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
    /// <param name="color1"></param>
    /// <param name="color2"></param>
    /// <returns></returns>
    public static bool ImageDistinctionFunction(Color[] color1, Color[] color2)
    {
        if(color1.Length != color2.Length)
        {
            return false;
        }
        for (int index = 0; index < color1.Length; index++)
        {
            if (color1[index] != color2[index])
            {
                Debug.Log("Check Color1 :" + color1[index] + ", Color2  :"+ color2[index]);
                return false;
            }
        }
        return true;
    }

}
