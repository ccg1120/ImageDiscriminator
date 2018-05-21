using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakeTest : MonoBehaviour {

    private void Awake()
    {
        Debug.Log("AWAKE !! " + this.gameObject.name);
    }
}
