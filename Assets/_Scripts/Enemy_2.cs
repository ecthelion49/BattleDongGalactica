using UnityEngine;
using System.Collections;

public class Enemy_2 : Enemy {
	// Enemy_2 uses a sin wave to modigy a 2 point linear interpolation
	public Vector3[] 		points;
	public float 			birthTime;
	public float			lifeTime = 10;

	// Determine hom much the sine wave will affect movement
	public float sinEccentricity = 0.6f;

	// Use this for initialization
	void Start () {
		// Initiate the points
		points = new Vector3[2];

		// Find the Utils.camBounds
		Vector3 cbMin = Utils.camBounds.min;
		Vector3 cbMax = Utils.camBounds.max;

		Vector3 v = Vector3.zero;

		// Pick any point on the left side of the screen
		v.x = cbMin.x - Main.S.enemySpawnPadding;
		v.y = Random.Range (cbMin.y, cbMax.y);
		points [0] = v;

		v = Vector3.zero;
		// Picka any point on the right side of the screen
		v.x = cbMax.x + Main.S.enemySpawnPadding;
		v.y = Random.Range (cbMin.y, cbMax.y);
		points [1] = v;

		// Possibly swap sides
		if (Random.value < 0.5f) {
			// Setting the .x of each point to its negative will move it 
			// to the other side of the screen
			points[0].x *= -1;
			points[1].x *= -1;
		}

		// Set the birthTime
		birthTime = Time.time;
	}
	
	public override void Move() {
		// Bezier vurves work based on a u value between 0 and 1
		float u = (Time.time - birthTime) / lifeTime;

		// If u > 1 then it has been longet than lifeTime since birthTime
		if (u > 1) {
			// This Enemy_2 has finished its life
			Destroy (this.gameObject);
			return;
		}

		// Adjust u by adding an easing curve based on a sine wave
		u = u + sinEccentricity * (Mathf.Sin (u * Mathf.PI * 2));

		// Interpolate the two linear interpolation points
		pos = (1 - u) * points [0] + u * points [1];
	}

}
