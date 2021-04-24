using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    Vector3 force;
    public float speed = 100;
    public float diameter = 3;
    public float lifetime = 5;

    private void Start()
    {
        force = transform.forward * speed;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.position = transform.position;
            obj.transform.localScale = new Vector3(diameter, diameter, diameter);
            obj.AddComponent<Rigidbody>().AddForce(force);
            Destroy(obj, lifetime);
        }
    }
}
