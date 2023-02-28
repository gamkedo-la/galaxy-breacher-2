using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Practice commit - will remove
public class Asteroid : MonoBehaviour
{
  public Vector3 velocity = Vector3.zero;
  public Vector3 rotationSpeed = Vector3.zero;

  void FixedUpdate()
  {
    transform.position += velocity * Time.deltaTime;
    transform.Rotate(rotationSpeed.x * Time.deltaTime, rotationSpeed.y * Time.deltaTime, rotationSpeed.z * Time.deltaTime);
  }
}
