using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data {

    public enum DataState
    {
        Used, //데이터 사용
        Clear, //초기화 완료
        Stay    // 대기 
    }
    public DataState CurrentDataState = DataState.Clear;

    public List<byte[]>[] ImageByteDataList;
}
