using UnityEngine;
using System.Collections;

// Enemy 1 extendy the enemy class
public class Enemy_1 : Enemy {
	// Because Enemy_1 extends the Enemy, the ____ bool won't work
	// The same way in the Inspector pane

	// no of seconds for a full sine wave
	public float		waveFrequency = 2;

	// sine wave width in meters
	public float		waveWidth = 4;
	public float		waveRotY = 45;

	private float		x0 = -12345;	// The initial value of the x position
	private float		birthTime;

	// Use this for initialization
	void Start () {
		// Set x0 to the initial x position of Enemy_1
		// Thisworks fine because the position will have already been set by Main.SpawnEnemy
		// before Start() runs (though Awake() would have been too early!).
		// This is also goo because there is no Start method on Enemy
		x0 = pos.x;
		birthTime = Time.time;
	}
	
	// Override the Move function on Emeny
	public override void Move() {
		// Because pos is a property, you can't directly set pos.x
		// so get the pos as an editiably vector3
		Vector3 tempPos = pos;

		// Theta adjusts based on time
		float age = Time.time - birthTime;
		float theta = Mathf.PI * 2 * age / waveFrequency;
		float sin = Mathf.Sin (theta);
		tempPos.x = x0 + waveWidth * sin;
		pos = tempPos;

		// rotate a bit about y
		Vector3 rot = new Vector3 (0, sin * waveRotY, 0);
		this.transform.rotation = Quaternion.Euler (rot);

		// base.Move() still handles the movement down in y
		base.Move ();
	}
}
