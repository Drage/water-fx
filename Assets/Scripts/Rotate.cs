using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour 
{
    public Vector3 axis = new Vector3(0, 0, 1);
    public float speed = 10;

    private float angle;
    private new Transform transform;

	private void Start()
    {
        transform = GetComponent<Transform>();
    }

	private void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(speed * Time.deltaTime, axis);
    }
}
