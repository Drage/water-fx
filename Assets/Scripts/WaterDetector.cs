using UnityEngine;
using System.Collections.Generic;

public class WaterDetector : MonoBehaviour 
{
    private Water water;

	private void Start()
    {
        water = transform.parent.GetComponent<Water>();
    }

	private void OnTriggerEnter2D(Collider2D hit)
    {
		Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
			float splashForce = rb.velocity.y * rb.mass;
			water.Splash(hit.gameObject, transform.position.x, splashForce);
        }
    }
}
