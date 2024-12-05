using System;
using UnityEngine;

// - The easiest way to work with the pool is to attach this script to the prefab you want to pool
// -- Then to return it to the pool is turn the object off or disable the component
// -- You can also implement the IPoolable interface on your own scripts, set the generic type to the type of your script
public class PoolObject : MonoBehaviour, IPoolable<PoolObject> {
	private Action<PoolObject> returnToPool;

	private void OnDisable() {
		ReturnToPool();
	}

	public void Initialize(Action<PoolObject> returnAction) {
		this.returnToPool = returnAction;
	}

	public void ReturnToPool() {
		returnToPool?.Invoke(this);
	}
}