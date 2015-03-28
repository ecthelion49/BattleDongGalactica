using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is actually Outside of the Utilds class
public enum BoundsTest {
	center,		// Is the center of the game object on screen?
	onScreen,	// Are the bounds entirely on screen?
	offScreen	// Are the bounds entirely off screen?
}


public class Utils : MonoBehaviour {
//=============================================Bounds function=============================================================

	// Creates bounds that encapsulate the two Bounds passed in
	public static Bounds BoundsUnion( Bounds b0, Bounds b1) {
		// If the size of one of the bounds is Vector.zero, ignore that one
		if (b0.size == Vector3.zero && b1.size != Vector3.zero) {					// 1
			return (b1);
		} else if (b0.size != Vector3.zero && b1.size == Vector3.zero) {
			return (b0);
		} else if (b0.size == Vector3.zero && b1.size == Vector3.zero) {
			return (b0);
		}

		// Stretch b0 to include the b1.min and ba.max
		b0.Encapsulate(b1.min);
		b0.Encapsulate(b1.max);
		return (b0);
	}

	public static Bounds CombineBoundsOfChildren(GameObject go) {
		Bounds b = new Bounds (Vector3.zero, Vector3.zero);

		// If this GameObject has a Rendere component..
		if (go.renderer != null) {
			// Expand b to contain the Renderer's bounds
			b = BoundsUnion (b, go.renderer.bounds);
		}

		// If this GameObject has a Collider Component
		if (go.collider != null) {
			// Expand b to contain the Collider's bounds
			b = BoundsUnion (b, go.collider.bounds);
		}

		// Recursively iterate through each child of this gameObject.transgfom
		foreach (Transform t in go.transform) {										// 1
			// Expand b to contain their Bounds as well
			b = BoundsUnion (b, CombineBoundsOfChildren (t.gameObject));			// 2
		}

		return (b);

	}

	// Make a static read-only public property camBounds
	static public Bounds camBounds {
		get {
			// if _camBounds hasn't been set yet
			if (_camBounds.size == Vector3.zero) {
				// SetCameraBounds using the default camera
				SetCameraBounds ();
			}

			return (_camBounds);
		}
	}

	// This is the private static field that camBounds uses
	static private Bounds _camBounds;												// 2

	// This function is used by camBounds to set _camBounds and can also be 
	// called directly
	public static void SetCameraBounds(Camera cam = null) {							// 3
		// If no Camera was passed in, use the main Camera
		if (cam == null) {
			cam = Camera.main;
		}

		// This makes a couple of important assumptions about the camera!:
		//	1. The camera is Orthographic
		//	2. The camera is at a rotation of R:[0, 0, 0]

		// Make vector3s at the topLeft and bottomRight of the Screen co-ords
		Vector3 topLeft = new Vector3 (0, 0, 0);
		Vector3 bottomRight = new Vector3 (Screen.width, Screen.height, 0);

		// Convert these to world co-ordinates
		Vector3 boundTLN = cam.ScreenToWorldPoint (topLeft);
		Vector3 boundBRF = cam.ScreenToWorldPoint (bottomRight);

		// Adjust their zs to be at the neat and far camera clipping planes
		boundTLN.z += cam.nearClipPlane;
		boundBRF.z += cam.farClipPlane;

		// Find the center of the Bounds
		Vector3 center = (boundTLN + boundBRF) / 2f;
		_camBounds = new Bounds (center, Vector3.zero);

		// Expand _camBounds to encapsulate the extents
		_camBounds.Encapsulate (boundTLN);
		_camBounds.Encapsulate (boundBRF);
	}

	// Checks to see whether the Bounds bnd are within the camBounds
	public static Vector3 ScreenBoundsCheck (Bounds bnd, BoundsTest test = BoundsTest.center) {
		return (BoundsInBoundsCheck (camBounds, bnd, test));
	}


	// Checks to see whether Bounds lilB are within Bounds bigB
	public static Vector3 BoundsInBoundsCheck (Bounds bigB, Bounds lilB, BoundsTest test = BoundsTest.onScreen) {
		// The behaviour of this function is deifferent based on the BoundsTest
		// that has been selected

		// Get the center of lilB
		Vector3 pos = lilB.center;

		// Initialise the offset at [0, 0, 0]
		Vector3 off = Vector3.zero;

		switch (test) {
			// The center test determines what off (offset) would have to be applied
			// to lilB to move its center back inside bigB
		case BoundsTest.center:
			if (bigB.Contains (pos) ) {
				return (Vector3.zero);
			}

			if (pos.x > bigB.max.x) {
				off.x = pos.x - bigB.max.x;
			} else if (pos.x < bigB.min.x) {
				off.x = pos.x - bigB.min.x;
			}

			if (pos.y > bigB.max.y) {
				off.y = pos.y - bigB.max.y;
			} else if (pos.y < bigB.min.y) {
				off.y = pos.y - bigB.min.y;
			}

			if (pos.z > bigB.max.z) {
				off.z = pos.z - bigB.max.z;
			} else if (pos.z < bigB.min.z) {
				off.z = pos.z - bigB.min.z;
			}

			return (off);

			// The Onscreen test determines what off would have to be applied to
			// keep all of lilB inside bigB
		case BoundsTest.onScreen:
			if (bigB.Contains (lilB.min) && bigB.Contains (lilB.max)) {
				return (Vector3.zero);
			}

			if (lilB.max.x > bigB.max.x) {
				off.x = lilB.max.x - bigB.max.x;
			} else if (lilB.min.x < bigB.min.x) {
				off.x = lilB.min.x - bigB.min.x;
			}

			if (lilB.max.y > bigB.max.y) {
				off.y = lilB.max.y - bigB.max.y;
			} else if (lilB.min.y < bigB.min.y) {
				off.y = lilB.min.y - bigB.min.y;
			}
			if (lilB.max.z > bigB.max.z) {
				off.z = lilB.max.z - bigB.max.z;
			} else if (lilB.min.z < bigB.min.z) {
				off.z = lilB.min.z - bigB.min.z;
			}

			return (off);

			// The offScreen test determines what off would need to be applied to 
			// any tiny part of lilB inside bigB
		case BoundsTest.offScreen:
			bool cMin = bigB.Contains (lilB.min);
			bool cMax = bigB.Contains (lilB.max);

			if (cMin || cMax) {
				return (Vector3.zero);
			}

			if (lilB.min.x > bigB.max.x) {
				off.x = lilB.min.x - bigB.max.x;
			} else if (lilB.max.x < bigB.min.x) {
				off.x = lilB.max.x - bigB.min.x;
			}

			if (lilB.min.y > bigB.max.y) {
				off.y = lilB.min.y - bigB.max.y;
			} else if (lilB.max.y < bigB.min.y) {
				off.y = lilB.max.y - bigB.min.y;
			}
			
			if (lilB.min.z > bigB.max.z) {
				off.z = lilB.min.z - bigB.max.z;
			} else if (lilB.max.z < bigB.min.z) {
				off.z = lilB.max.z - bigB.min.z;
			}

			return (off);
		}

		return (Vector3.zero);
	}

	//================================== Transform functions ===========================================================

	// This function will iteratively climb up the transform.parent tree 
	// until it either fands a parent with a tag != "Untagged" or no parent
	public static GameObject FindTaggedParent(GameObject go) {				// 1
		if (go.tag != "Untagged") {
			// then return this game object
			return(go);
		}

		// If there is no parent of this Transform
		if (go.transform.parent == null) {
			// We've reached the top of the hierachy with no interesting tage
			// so return null
			return (null);
		}

		// Otherwise, recursively climb up the tree
		return (FindTaggedParent (go.transform.parent.gameObject));
	}

	// This version of the function handles things if a Transform is passed in
	public static GameObject FindTaggedParent (Transform T) {
		return (FindTaggedParent (T.gameObject));
	}


	//=================================== Materials functions ===============================================================

	// Returns a list of all Materials on the GameObject or its children
	static public Material[] GetAllMaterials (GameObject go) {
		List<Material> mats = new List<Material> ();
		if (go.renderer != null) {
			mats.Add (go.renderer.material);
		}

		foreach (Transform t in go.transform) {
			mats.AddRange (GetAllMaterials (t.gameObject));
		}

		return (mats.ToArray ());
	}




}
