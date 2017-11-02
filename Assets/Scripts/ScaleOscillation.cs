using UnityEngine;
using System.Collections;

public class ScaleOscillation : MonoBehaviour 
{
    public float magnitude = 0.25f;
    public float speed = 1.0f;
    private float current = 0;
    private Vector3 scale;
    private new Transform transform;

	private void Start()
    {
        transform = GetComponent<Transform>();
        scale = transform.localScale;
    }
	
	private void Update() 
    {
        current += Time.deltaTime * speed;
        float scaleMod = magnitude * Mathf.Sin(current);
        Vector3 newScale = scale + new Vector3(scaleMod, scaleMod, 0);
        transform.localScale = newScale;
	}
}
