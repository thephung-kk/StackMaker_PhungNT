using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform Target;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float speed = 8;


    private void FixedUpdate()
    {
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, Target.position + offset, speed * Time.deltaTime);
    }
}
