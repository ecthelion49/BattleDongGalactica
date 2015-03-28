using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour {
	static public Hero			S;		// Singleton

	public float			gameRestartDelay = 2f;

	// These fields control the movement of the ship
	public float				speed = 30;
	public float				rollMult = -45;
	public float				pitchMult = 30;

	// Ship status information
	[SerializeField]
	public float				_shieldLevel = 1;			// Add the underscore

	// Weapon fields
	public Weapon[]				weapons;

	public bool				___________________________;

	public Bounds				bounds;

	// Declare a new delegate type WeaponFireDelegate
	public delegate void WeaponeFireDelegate();

	// Create a WeaponFire Delegate field name fireDelegate
	public WeaponeFireDelegate fireDelegate;

	void Awake() {
		S = this;		// Set the singleton
		bounds = Utils.CombineBoundsOfChildren (this.gameObject);
	}

	void Start() {
		// Reset the weapons to start _Hero with 1 blaster
		ClearWeapons ();
		weapons [0].SetType (WeaponType.blaster);
	}

	// Update is called once per frame
	void Update () {
		// Pull in information from the Input class
		float xAxis = Input.GetAxis ("Horizontal");												// 1
		float yAxis = Input.GetAxis ("Vertical");												// 1

		// Change transform.position based on the axes
		Vector3 pos = transform.position;
		pos.x += xAxis * speed * Time.deltaTime;
		pos.y += yAxis * speed * Time.deltaTime;
		transform.position = pos;

		bounds.center = transform.position;														// 1

		// Keep the ship constrained to the screen bounds
		Vector3 off = Utils.ScreenBoundsCheck (bounds, BoundsTest.onScreen);					// 2
		if (off != Vector3.zero) {
			pos -= off;
			transform.position = pos;
		}

		// Rotate the ship to make it feel more dynamic											// 2
		transform.rotation = Quaternion.Euler (yAxis * pitchMult, xAxis * rollMult, 0);

		// Use the fireDelegate o fire Weapons
		// First, make sure that Axis("jump") button is pressed
		// Then ensure that fireDelegate isnt null to avoid an error
		if (Input.GetAxis ("Jump") == 1 && fireDelegate != null) {
			fireDelegate ();
		}
	}

	// This Variable holds a reference to the last triggering GameObject
	public GameObject lastTriggerGo = null;

	void OnTriggerEnter (Collider other) {
		// Find the tag of other.gameOnject ot its parent GameObjects
		GameObject go = Utils.FindTaggedParent (other.gameObject);

		// If there is a parent with a tag
		if (go != null) {

			// Make sure it's not the same triggering go as last time
			if (go == lastTriggerGo) {
				return;
			}

			lastTriggerGo = go;

			if (go.tag == "Enemy") {
				// If the shield was triggered by an enemy
				// Decrease the level of shield by 1
				shieldLevel--;

				// Destroy the enemy
				Destroy (go);
			} else if (go.tag == "PowerUp") {
				// If the shield was triggered by a PowerUp
				AbsorbPowerUp(go);
			} else {
				// Announce it
				print ("Triggered: " + go.name);
			}

			// Otherwise announce the original other.gameObject
			print ("Triggered: " + other.gameObject.name);
		}
	}

	public void AbsorbPowerUp (GameObject go) {
		PowerUp pu = go.GetComponent<PowerUp> ();
		switch (pu.type) {
		case WeaponType.shield:			// If its a shield
			shieldLevel++;
			break;
		default:						// If it is any Weapon PowerUp
			// Check the current weapon type
			if (pu.type == weapons [0].type) {
				// Then increase the number of weapons of this type
				Weapon w = GetEmptyWeaponSlot ();		// Find an available weapon
				if (w != null) {
					// Set it to pu.type
					w.SetType (pu.type);
				} 
			} else {
					// If this is a dofferemt wea[pm
					ClearWeapons ();
					weapons [0].SetType (pu.type);
			}
			break;
		}
		pu.AbsorbedBy (this.gameObject);
	}

			                      
	Weapon GetEmptyWeaponSlot() {
		for (int i = 0; i < weapons.Length; i++) {
			if (weapons [i].type == WeaponType.none) {
				return (weapons [i]);
			}
		}
		return (null);
	}
	void ClearWeapons() {
		foreach (Weapon w in weapons) {
			w.SetType (WeaponType.none);
		}
	}

	public float shieldLevel {
		get {
			return (_shieldLevel);
		}
		set {
			_shieldLevel = Mathf.Min (value, 4);

			// If the shield is going to be set to less than zero
			if (value < 0) {
				Destroy (this.gameObject);

				// Tell Main.S to restart the fame after a delay
				Main.S.DelayedRestart (gameRestartDelay);
			}
		}
	}

}
