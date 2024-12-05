using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// - Example spawner script that utilizes the V2 object pooling system
public class Spawner : MonoBehaviour {
	public GameObject cubePrefab;
	public GameObject spherePrefab;
	public GameObject capsulePrefab;

	[Range(1f, 15f)] public float range = 5f;

	private static ObjectPool<PoolObject> cubePool;
	private static ObjectPool<PoolObject> spherePool;
	private static ObjectPool<PoolObject> capsulePool;

	public bool canSpawn = true;

	private void OnEnable() {
		cubePool = new ObjectPool<PoolObject>(cubePrefab);
		spherePool = new ObjectPool<PoolObject>(spherePrefab);
		capsulePool = new ObjectPool<PoolObject>(capsulePrefab);

		StartCoroutine(SpawnOverTime());
	}

	private IEnumerator SpawnOverTime() {
		while (canSpawn) {
			Spawn();
			yield return null;
		}
	}

	public void Spawn() {
		int random = Random.Range(0, 3);
		Vector3 position = (Random.insideUnitSphere * range) + this.transform.position;
		GameObject prefab;

		switch (random) {
			case 0:
				prefab = cubePool.PullGameObject(position, Random.rotation);
				break;
			case 1:
				prefab = spherePool.PullGameObject(position, Random.rotation);
				break;
			case 2:
				prefab = capsulePool.PullGameObject(position, Random.rotation);
				break;
			default:
				prefab = cubePool.PullGameObject(position, Random.rotation);
				break;
		}

		// This could/should be done in the initialization function
		prefab.GetComponent<Rigidbody>().velocity = Vector3.zero;
	}
}