using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMorpher : MonoBehaviour
{
    [Range(0,1)]
    public float power; //Change in terrain per second

    [Range(1,1000)]
    public float range;
    [Range (1,5)]
    public float radius;
    private ProceduralTerrain terrainManager;

    // Start is called before the first frame update
    void Start()
    {
        terrainManager = ProceduralTerrain.Instance;
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
                var offsets = GetOffsetsWithinSphere(hit.point, radius);

                foreach (var offset in offsets)
                {
                    terrainManager.ModifyTerrainAtPoint(hit.point + offset, (radius - offset.magnitude) * power * Time.deltaTime);
                }
            }
        }

        if (Input.GetMouseButton(1))
        {
            Ray ray = new Ray(transform.position, Camera.main.transform.forward);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range))
            {
                var offsets = GetOffsetsWithinSphere(hit.point, radius);

                foreach (var offset in offsets)
                {
                    terrainManager.ModifyTerrainAtPoint(hit.point + offset, -(radius - offset.magnitude) * power * Time.deltaTime);
                }
            }
        }
    }

    List<Vector3> GetOffsetsWithinSphere(Vector3 _spherePos, float _radius)
    {
        List<Vector3> returnList = new List<Vector3>();
        float sqrRadius = Mathf.Pow(_radius, 2);

        for (float x = -_radius; x < _radius; x++)
        {
            for (float y = -_radius; y < _radius; y++)
            {
                for (float z = -_radius; z < _radius; z++)
                {
                    Vector3 offset = new Vector3(x, y, z);

                    if (offset.sqrMagnitude > sqrRadius)
                        continue;

                    returnList.Add(offset);
                }
            }
        }
        return returnList;
    }

}
