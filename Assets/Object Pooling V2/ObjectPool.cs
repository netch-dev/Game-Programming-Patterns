using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : IPool<T> where T : MonoBehaviour, IPoolable<T> {
	public ObjectPool(GameObject pooledObject, int numToSpawn = 0) {
		this.prefab = pooledObject;
		Spawn(numToSpawn);
	}

	public ObjectPool(GameObject pooledObject, Action<T> pullObject, Action<T> pushObject, int numToSpawn = 0) {
		this.prefab = pooledObject;
		this.pullObject = pullObject;
		this.pushObject = pushObject;
		Spawn(numToSpawn);
	}

	private System.Action<T> pullObject;
	private System.Action<T> pushObject;
	private Stack<T> pooledObjects = new Stack<T>();
	private GameObject prefab;
	public int pooledCount => pooledObjects.Count;

	public T Pull() {
		T t;
		if (pooledCount > 0)
			t = pooledObjects.Pop();
		else
			t = GameObject.Instantiate(prefab).GetComponent<T>();

		t.gameObject.SetActive(true); // Ensure the object is on, this keeps our initialize functions clean
		t.Initialize(Push);

		pullObject?.Invoke(t);

		return t;
	}

	public T Pull(Vector3 position) {
		T t = Pull();
		t.transform.position = position;
		return t;
	}

	public T Pull(Vector3 position, Quaternion rotation) {
		T t = Pull();
		t.transform.position = position;
		t.transform.rotation = rotation;
		return t;
	}

	public GameObject PullGameObject() {
		return Pull().gameObject;
	}

	public GameObject PullGameObject(Vector3 position) {
		GameObject go = Pull().gameObject;
		go.transform.position = position;
		return go;
	}

	public GameObject PullGameObject(Vector3 position, Quaternion rotation) {
		GameObject go = Pull().gameObject;
		go.transform.position = position;
		go.transform.rotation = rotation;
		return go;
	}

	public void Push(T t) {
		pooledObjects.Push(t);

		pushObject?.Invoke(t);

		t.gameObject.SetActive(false);
	}

	private void Spawn(int number) {
		T t;

		for (int i = 0; i < number; i++) {
			t = GameObject.Instantiate(prefab).GetComponent<T>();
			pooledObjects.Push(t);
			t.gameObject.SetActive(false);
		}
	}
}

// - Using a generic interface with a generic paramater that is the type of object we are pooling
// - Now our push and pull functions know what types they are working with
public interface IPool<T> {
	T Pull();
	void Push(T t);
}

// - This interface is used to define objects that can be in the object pool
// - The Initialize function that takes in an action, set in the object pool, and is intended to be the function that returns the object to the pool
// -- This action is intended to be invoked in the ReturnToPool function
public interface IPoolable<T> {
	void Initialize(System.Action<T> returnAction);
	void ReturnToPool();
}