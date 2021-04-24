using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnWaterParticles : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public int depth = 10;
    public float particleSize = 1;

    public bool trigger = false;

    private void Start()
    {
        CreateWater();
    }

    // Update is called once per frame
    void Update()
    {
        if (trigger)
        {
            CreateWater();
            trigger = false;
        }
    }

    void CreateWater()
    {
        for (int x=0; x< width; x++)
        {
            for (int y=0; y < depth; y++)
            {
                for (int z=0; z < height; z++)
                {
                    GameObject ptc = Instantiate((GameObject)Resources.Load("Prefabs/Water Particle", typeof(GameObject)));
                    ptc.transform.localScale = new Vector3(particleSize, particleSize, particleSize);
                    ptc.transform.position = transform.position + new Vector3(x * particleSize, y* particleSize, z* particleSize);
                    ptc.transform.parent = transform;
                }
            }
        }
    }
}
