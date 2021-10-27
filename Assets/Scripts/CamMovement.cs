using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


public class CamMovement : MonoBehaviour
{
    public Transform target;

    [SerializeField] float smoothSpeed;
    [SerializeField] private Vector3 _Offset;
    // private Vector3 offset = new Vector3(0, 10, 0);


    void LateUpdate()
    {
        Vector3 desiredPos = target.position + _Offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        transform.position = new Vector3(transform.position.x, transform.position.y, smoothedPos.z);

    }
}