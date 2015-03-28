using UnityEngine;
using System.Collections;

// Pasrt is another serializable data storage class just like WeaponDefinition
[System.Serializable]
public class Part {
	// These three fields need to e defined in the inspector pane
	public string		name;			// The name of the part
	public float		health;			// The amount of health this part has
	public string[]		protectedBy;	// The other pasrt that protect this

	// These two fields are set automatically in Start()
	// Cachine like this makes it faster and easier to find these later
	public GameObject	go;				// The GameObject of this part
	public Material		mat;			// The material to show damage
}

public class Enemy_4 : Enemy {
	// Enemy 4 will start offscreen and the pick a random point on screen
	// to move to. Once it has arrivevd, it will pick another random point and 
	// continue until the player has shot it down

	public Vector3[]			points;			// Stores the p0 and p1 for interpolation
	public float				timeStart;		// Birth time for this enemy_4
	public float				duration = 4;	// Duration of movement

	public Part[]				parts;			// The array of ship parts

	// Use this for initialization
	void Start () {
		points = new Vector3[2];

		// There is already an initial position chosen by Main.SpawnEnemy()
		// so add it to points as the initial po and o1
		points [0] = pos;
		points [1] = pos;

		InitMovement ();

		// Cache GameObject and material of each part in parts
		Transform t;
		foreach (Part prt in parts) {
			t = transform.Find(prt.name);

			if (t != null) {
				prt.go = t.gameObject;
				prt.mat = prt.go.renderer.material;
			}
		}
	}
	

	void InitMovement() {
		// Pick a new point to move to this is on screen
		Vector3 p1 = Vector3.zero;
		float esp = Main.S.enemySpawnPadding;
		Bounds cBounds = Utils.camBounds;

		p1.x = Random.Range (cBounds.min.x + esp, cBounds.max.x - esp);
		p1.y = Random.Range (cBounds.min.y + esp, cBounds.max.y - esp);

		points [0] = points [1];		// Shift points[1] to points[0]
		points [1] = p1;


		// Reset the time
		timeStart = Time.time;
	}

	public override void Move () {
		// This completely ovverides Enemy.Move() with a linear interpolation

		float u = (Time.time - timeStart) / duration;
		if (u >= 1) {	// If u >= 1 ..
			InitMovement ();		// Then initialise the movement to a new point
			u = 0;
		}

		u = 1 - Mathf.Pow (1 - u, 2);		// Apples ease out easing to u

		pos = (1 - u) * points [0] + u * points [1];	// Simple linear interpolation
	}

	// THis will override the OnCollisionEnter that is poart of Enemy.cs
	// Because of the way that MonoBehavious declares common Unity functions 
	// Like OnCollissionEnter(), the override keyword is not necessary
	void OnCollisionEnter (Collision coll) {
		GameObject other = coll.gameObject;

		switch (other.tag) {
		case "ProjectileHero":
			Projectile p = other.GetComponent<Projectile> ();

			// Enemies font take damage unless they are on screen
			// This stops the player from shooting them before they are vidible
			bounds.center = transform.position + boundsCenterOffset;

			if (bounds.extents == Vector3.zero || Utils.ScreenBoundsCheck (bounds, BoundsTest.offScreen) != Vector3.zero) {
				Destroy (other);
				break;
			}

			// Hurt this enemy
			// Find the GameObject that was hit
			// The collission coll has contacts[], an array of contract points
			// Because there was a collision, we are guarenteed that there is at least
			// a contacts[0], and ContactPoints have a reference to
			// thisCollider, whcih will be the collider for the part of the
			// Enemy_4 that was hit
			GameObject goHit = coll.contacts [0].thisCollider.gameObject;
			Part prtHit = FindPart (goHit);

			if (prtHit == null) {
				// If prtHit was not found
				// ... then it is usually because, very rearely, thisCollider on
				// contacts[0] will be the ProjectileHere instead of the ship part
				// If so, just look for otherCollider instead
				goHit = coll.contacts [0].otherCollider.gameObject;
				prtHit = FindPart (goHit);
			}

			// Check whether this part is still prtected
			if (prtHit.protectedBy != null) {
				foreach (string s in prtHit.protectedBy) {
					// If one of the protecting parts has not been destroyed
					if (!Destroyed (s)) {
						// ... then do not damage this part yet
						Destroy (other);		// Destroy the projectileHero
						return;
					}
				}
			}

			// It's not protected, so make it take damage
			// Get the damage amount from the Projectile.type and Main.W_defs
			prtHit.health -= Main.W_DEFS [p.type].damageOnHit;

			// Shoe damage on the part
			ShowLocalizedDamage (prtHit.mat);

			if (prtHit.health <= 0) {
				// Instead of Destroyinh this enemy, disable the damaged part
				prtHit.go.SetActive (false);
			}

			// Check to see if the whole ship is destroyed
			bool allDestroyed = true;		// Assume it is destroyed
			foreach (Part prt in parts) {
				if (!Destroyed (prt)) {
					// If the part still exists
					allDestroyed = false;	// ... change allDestroyed to false
					break;
				}
			}

			if (allDestroyed) {	// If it IS completely destroyed
				// Tell the Main singleon that this ship has been destroyed
				Main.S.ShipDestroyed (this);

				// Destroy this enemy
				Destroy (this.gameObject);
			}

			Destroy (other);		// Destroy the projectileHero
			break;
		}
	}

	// These two functions find a Part in this.parts by name or GameObject
	Part FindPart (string n) {
		foreach (Part prt in parts) {
			if (prt.name == n) {
				return (prt);
			}
		}

		return (null);
	}

	Part FindPart (GameObject go) {
		foreach (Part prt in parts) {
			if (prt.go == go) {
				return (prt);
			}
		}
		
		return (null);
	}

	// These functions return true if the Part has been destroyed
	bool Destroyed (GameObject go) {
		return (Destroyed (FindPart (go)));
	}

	bool Destroyed (string n) {
		return (Destroyed (FindPart (n)));
	}

	bool Destroyed(Part prt) {
		if (prt == null) {		// If no real Part was passed in
			return (true);		// Return true (meaining yes, it was destroyed)
		}

		// Returns the results of the comparison: prt.health <= 0
		// If prt.health is 0 or less, teturns true
		return (prt.health <= 0);
	}

	// This changes the color of just one Part to red instead of the whole ship
	void ShowLocalizedDamage (Material m) {
		m.color = Color.red;
		remainingDamageFrames = showDamageForFrames;
	}

}
