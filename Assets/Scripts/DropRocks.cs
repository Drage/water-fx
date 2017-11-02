using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropRocks : MonoBehaviour {

	public GameObject rock;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 position = Input.mousePosition;
			position = Camera.main.ScreenToWorldPoint(position);
			position.z = 0;

			float angle = Random.Range(0.0f, 360.0f);
			Vector3 axis = new Vector3(0, 0, 1);
			Quaternion rotation = Quaternion.AngleAxis(angle, axis);

			GameObject newRock = GameObject.Instantiate(rock, position, rotation); 
			float scale = Random.Range(0.1f, 0.5f);
			newRock.transform.localScale = new Vector3(scale, scale, 0);
			newRock.GetComponent<Rigidbody2D>().mass = scale * 10;
		}
	}
}
