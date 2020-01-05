using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPlacer : MonoBehaviour
{

    public GameObject lightPrefab;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            Instantiate(lightPrefab,transform.position,Quaternion.identity);
    }
}
