using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    Camera cam;
    public float moveSpeed = 10;
    VolumeProfile volume;
    ColorAdjustments colorAdj;

    private void Start()
    {
        cam = Camera.main;

        volume = FindObjectOfType<Volume>().GetComponent<Volume>().profile;
        volume.TryGet(out colorAdj);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = 50;
        }
        else
        {
            moveSpeed = 10;
        }

        Vector3 pos = cam.transform.position;

        if (Input.GetKey(KeyCode.W))
        {
            pos += cam.transform.forward * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            pos -= cam.transform.forward * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            pos -= cam.transform.right * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            pos += cam.transform.right * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            pos += cam.transform.up * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            pos -= cam.transform.up * moveSpeed * Time.deltaTime;
        }

        if (Input.GetMouseButton(1))
        {
            AimCamera();
        }

        cam.transform.position = pos;
    }

    private void AimCamera()
    {
        cam.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0));
    }

    private void OnTriggerStay(Collider other)
    {
        if (!colorAdj)
        {
            Debug.LogError("NO COLOR FILTER");
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            if (transform.position.y < other.transform.position.y)
            {
                Material waterMat = other.gameObject.GetComponent<Renderer>().material;
                Color waterCol = waterMat.GetColor("_ColorShallow");
                colorAdj.colorFilter.Override(waterCol);
            }
            else
            {
                colorAdj.colorFilter.Override(new Color(1, 1, 1));
            }

        }
    }
}
