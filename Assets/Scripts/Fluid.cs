using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class Fluid : MonoBehaviour
{
    //FluidCube fluid3D;
    //FluidSquare fluid2D;
    FluidSim simulation;

    public int fluidSize = 16;
    public float diffusion = 0;
    public float viscocity = 0;
    public float scale = 1;
    public float clickIntensity = 20;

    public Material material;
    public bool update = false;

    [SerializeField]
    GameObject water;

    Mesh mesh;
    Color[] colors;
    
    [Range(0, 10)]
    public int accuracy = 1;

    //GameObject waterObj;

    private void Start()
    {
        if (water)
        {
            DestroyImmediate(water);
        }

        //fluid3D = new FluidCube(fluidSize, diffusion, viscocity, Time.deltaTime);
        //fluid2D = new FluidSquare(fluidSize, diffusion, viscocity, Time.deltaTime, 4);
        simulation = new FluidSim(fluidSize, diffusion, viscocity, accuracy);
        CreateMesh2D();
    }

    private void FixedUpdate()
    {
        simulation.Update();
    }

    private void Update()
    {
        if (!mesh)
        {
            return;
        }

        MouseInteractions();

        UpdateMesh();
    }

    private void MouseInteractions()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                Vector2 coords = LocalCoords(hit.point);

                float velX = Input.GetAxis("Mouse X");
                float velY = Input.GetAxis("Mouse Y");

                int x = (int)coords.x;
                int y = (int)coords.y;

                float density = clickIntensity;

                if (Mathf.Abs(velY) <= 1 && Mathf.Abs(velX) <= 1)
                {
                    density = 2;
                }

                //fluid2D.AddDensity(x, y, density);
                //fluid2D.AddVelocity(x, y, velX, velY);
                simulation.ApplyForceAt(x, y, velY, velY, density);
            }
        }
    }

    void CreateMesh2D()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
        }

        //if (waterObj)
        //{
        //    return;
        //}

        int size = simulation.Size();

        Vector3[] verts = new Vector3[size * size];
        Vector2[] uv = new Vector2[size * size];
        colors = new Color[verts.Length];
        int[] tris = new int[(size - 1) * (size - 1) * 6];
        int triIndex = 0;

        int i = 0;
        for (int x=0; x< size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector3 pos = new Vector3(x, 0, y) * scale;
                verts[i] = pos;
                Vector2 uvcoord = new Vector2(x, y) / size;
                uv[i] = uvcoord;

                if (x != size - 1 && y != size - 1)
                {
                    tris[triIndex] = i;
                    tris[triIndex + 1] = i + size + 1;
                    tris[triIndex + 2] = i + size;

                    tris[triIndex + 3] = i;
                    tris[triIndex + 4] = i + 1;
                    tris[triIndex + 5] = i + size + 1;

                    triIndex += 6;
                }

                i++;
            }
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = tris;
        mesh.name = "Fluid";
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateUVDistributionMetrics();

        water = new GameObject();
        var renderer = water.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        water.AddComponent<MeshFilter>().sharedMesh = mesh;
        water.name = "Fluid";

        //var collider = water.AddComponent<MeshCollider>();
        var collider = water.AddComponent<BoxCollider>();
        //collider.sharedMesh = mesh;
        //collider.convex = true;
        collider.isTrigger = true;

        water.layer = LayerMask.NameToLayer("Water");
        //Rigidbody rb = water.AddComponent<Rigidbody>();
        //rb.isKinematic = true;
        //rb.useGravity = false;
        water.transform.parent = transform;
        water.transform.localPosition = Vector3.zero;
    }

    private void UpdateMesh()
    {
        int size = simulation.Size();

        int i = 0;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                //r, g, b is the velocity at that point
                //a is the density
                float dens = simulation.Density(x, y)+ 0.01f;
                float xVel = simulation.VelocityX(x, y) * 5;
                float yVel = simulation.VelocityY(x, y) * 5;
                Color col = new Color(xVel, yVel, 0, dens);

                colors[i] = col;
                i++;
            }
        }

        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    private void OnTriggerEnter(Collider other)
    {
        var rb = other.gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 vel = rb.velocity;
            float mass = rb.mass;

            Debug.Log(vel.magnitude);

            AddVelocity(other.gameObject, 10);
            CircleSplash(other.transform.position, vel.magnitude);
            //ObjectSplash(other.gameObject, 1, vel.magnitude);    
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<Rigidbody>() != null)
        {
            //ObjectSplash(other.gameObject, -1);
        }
    }

    private Vector2 LocalCoords(Vector3 worldCoords)
    {
        Vector3 origin = mesh.vertices[0] + water.transform.position;
        Vector3 coords = worldCoords - origin;

        return new Vector2(coords.x, coords.z);
    }

    private void AddVelocity(GameObject obj, float speed)
    {
        Vector3 pos = LocalCoords(obj.transform.position);
        int x = (int)pos.x;
        int y = (int)pos.z;

        var rb = obj.GetComponent<Rigidbody>();

        float xVel = rb.velocity.x;
        float yVel = rb.velocity.z;
        Debug.Log(new Vector2(x, y));
        //fluid2D.AddVelocity(x, y, xVel * speed, yVel * speed);
    }

    public void CircleSplash(Vector3 worldCoords, float power)
    {
        Vector2 center = LocalCoords(worldCoords);
        int centerX = (int)center.x;
        int centerY = (int)center.y;


        for (int x = -1; x<=1; x++)
        {
            for (int y=-1; y<=1; y++)
            {
                Vector2 force = new Vector2(x, y);
                force.Normalize();
                force *= (power/8);

                //fluid2D.AddDensity(centerX + x, centerY + y, power/2);
                //fluid2D.AddVelocity(centerX + x, centerY + y, force.x, force.y);
            }
        }
    }
}
