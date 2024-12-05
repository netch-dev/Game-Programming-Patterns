using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Netch.GameProgrammingPatterns {
	public class ObjectPoolingNotes {
		#region Object Pooling v2
		// - Improvements from the v1 pool in Notes.cs:
		// -- No object pool manager
		// -- Objects that can return themselves without needed to find the object pool
		// -- Store a reference to a component, not just the gameobject. Saves on GetComponent calls
		// -- Interface to make the initialization easy and consistent
		#endregion
	}
}
