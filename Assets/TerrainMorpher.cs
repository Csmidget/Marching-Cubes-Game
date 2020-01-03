using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMorpher : MonoBehaviour
{
    [Range(0,1)]
    public float power; //Change in terrain per second

    [Range(1,1000)]
    public float range;
    private TerrainManager terrainManager;


    // Start is called before the first frame update
    void Start()
    {
        terrainManager = TerrainManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(transform.position, Camera.main.transform.forward * range);
        if (Input.GetMouseButton(0))
        {
            Ray ray = new Ray(transform.position, Camera.main.transform.forward);   

            RaycastHit hit;
            if (Physics.Raycast(ray ,out hit, range))
            {
                terrainManager.ModifyTerrainAtPoint(hit.point, power * Time.deltaTime);
            }
        }

        if (Input.GetMouseButton(1))
        {
            Ray ray = new Ray(transform.position, Camera.main.transform.forward);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range))
            {
                terrainManager.ModifyTerrainAtPoint(hit.point, -power * Time.deltaTime);
            }
        }
    }
}
