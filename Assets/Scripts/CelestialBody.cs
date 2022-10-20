using UnityEngine;

public class CelestialBody : MonoBehaviour
{
  [SerializeField] GameObject player;

  void Update()
  {
    Quaternion lookRotation = Quaternion.LookRotation(transform.position - player.transform.position);
    transform.rotation = lookRotation;
  }
}
