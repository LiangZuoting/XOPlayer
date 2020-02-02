using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    private float rotateSpeed = 2.0f;

    public void RotateAround()
    {
        transform.RotateAround(transform.position, Vector3.down, rotateSpeed * Input.GetAxis("Mouse X"));
        transform.RotateAround(transform.position, transform.right, rotateSpeed * Input.GetAxis("Mouse Y"));
    }
}
