using UnityEngine;
using System.Collections;

// This is an enum of the various possible weapon trpes
// It also includes a shield type to allow a shield power-up
// Items marked  {NI] below are not implemented in this book

public enum WeaponType {
	none,					// The default / no weapon
	blaster,				// A simple blaster
	spread,					// Two shots simultaneously
	phaser,					// Shots that move in waves [NI]
	missile, 				// Homing missiles {NI]
	laser,					// Damage over time {NI]
	shield,					// Raise shieldLevel
}

// The WeaponDefinition class allows you to set the properties
// of a specific weapon in the inspector. Main has an array of 
// WeaponDefinitions that makes this possible.
// [Systemt.Seializable] tells Unity to try to view weaponDefinition
// in the inspector pane. It doesn't work for everything, but it 
// will work for simple classes like this:

[System.Serializable]
public class WeaponDefinition {
	public WeaponType		type = WeaponType.none;
	//public WeaponType		type = WeaponType.blaster;
	public string			letter;							// The letter to show on the power-up
	public Color			color = Color.white;			// Color of Collar and power-up
	public GameObject		projectilePrefab;				// preFab for projectiles
	public Color 			projectileColor = Color.white;	
	public float			damageOnHit = 0;				// Amount of damage caused
	public float			continuousDamage = 0;			// Damage per second (Laser)
	public float			delayBetweenShots = 0;			
	public float			velocity = 20;					// Speed of projectiles
}
// Note: Weapon prefabs, colors and son on are set in the class Main

public class Weapon : MonoBehaviour {
	static public Transform		PROJECTILE_ANCHOR;

	public bool 				____________________________________;

	[SerializeField]
	private WeaponType			_type = WeaponType.none;
	//private WeaponType			_type = WeaponType.blaster;
	public WeaponDefinition		def;
	public GameObject			collar;
	public float				lastShot;					// Time last shot was fired

	void Awake() {
		collar = transform.Find ("Collar").gameObject;
	}

	void Start() {
		// Call SetType() properly for the default _type
		SetType (_type);

		if (PROJECTILE_ANCHOR == null) {
			GameObject go = new GameObject ("_Projectile_Anchor");
			PROJECTILE_ANCHOR = go.transform;
		}

		// Find the fireDelegate of the parent
		GameObject parentGO = transform.parent.gameObject;
		if (parentGO.tag == "Hero") {
			Hero.S.fireDelegate += Fire;
		}
	}

	public WeaponType type {
		get {
			return (_type);
		}
		set {
			SetType (value);
		}
	}

	public void SetType (WeaponType wt) {
		_type = wt;

		if (type == WeaponType.none) {
			this.gameObject.SetActive (false);
			return;
		} else {
			this.gameObject.SetActive (true);
		}

		def = Main.GetWeaponDefinition (_type);
		collar.renderer.material.color = def.color;
		lastShot = 0;			// You can always fire immediately after _type is met
	}

	public void Fire() {
		// If this.GameObject is inactive, return
		if (!gameObject.activeInHierarchy) {
			return;
		}

		// If it hasn't been enough time between shots, return
		if (Time.time - lastShot < def.delayBetweenShots) {
			return;
		}

		Projectile p;
		switch (type) {
		case WeaponType.blaster:
			p = MakeProjectile();
			p.rigidbody.velocity = Vector3.up*def.velocity;
			break;
		case WeaponType.spread:
			p = MakeProjectile();
			p.rigidbody.velocity = Vector3.up*def.velocity;
			p = MakeProjectile ();
			p.rigidbody.velocity = new Vector3( -0.2f, 0.9f, 0)*def.velocity;
			p = MakeProjectile();
			p.rigidbody.velocity = new Vector3( 0.2f, 0.9f, 0)*def.velocity;
			break;
		}
	}

	public Projectile MakeProjectile() {
		GameObject go = Instantiate (def.projectilePrefab) as GameObject;
		if (transform.parent.gameObject.tag == "Hero") {
			go.tag = "ProjectileHero";
			go.layer = LayerMask.NameToLayer("ProjectileHero");
		} else {
			go.tag = "ProjectileEnemy";
			go.layer = LayerMask.NameToLayer("ProjectileEnemy");
		}

		go.transform.position = collar.transform.position;
		go.transform.parent = PROJECTILE_ANCHOR;
		Projectile p = go.GetComponent<Projectile>();
		p.type = type;
		lastShot = Time.time;
		return(p);
	}
	


}
