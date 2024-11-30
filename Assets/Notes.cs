using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace Netch.GameProgrammingPatterns {
	public class Notes {
		#region State Pattern
		// "Allow an object to alter its behavior when its internal state changes. The object will appear to change its class."

		// - Useful when dealing with objects that have different modes/behaviours
		// - Makes it easier to add new modes/behaviours

		// - Finite state machine is a common implementation of the state pattern

		// - Example: Player character with different states (Idle, Walking, Running, Jumping, etc.)
		// - Example: Traffic light with different states (Red, Yellow, Green)

		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - Without the state pattern, you would have to use a lot of if-else statements to check the current state
		// - This code works perfectly fine but it's not very clean and hard to add new states
		private void NPCUpdate_WithoutStateMachine() {
			/*			if (CanSeeEnemy()) {
							Attack();
						} else if (IsHurt()) {
							RunAway();
						} else {
							Wander();
						}
			*/
		}

		// - Some downsides of state machines:
		// -- Can become complicated and hard to maintain when there are many states and transitions. A behaviour tree might be a better choice in that case
		// -- States not fully decoupled. By keeping all of the variables on the base class, it does make the state classes cleaner, but the cost is making the variables public
		// --- This can be partially resolved with a data container class, which would result in fewer public variables on the base class. Using a data container class would make it easier to re-use states for other entities


		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - Using an enum to represent the different states
		// - A limitation of this approach is that we need a way to transition between states
		// -- In each case conditionals are used to determine if the state should change
		// -- An advantage of this is we only need to decide if we need to change states

		// - The end result can still be a complicated mess since all of the state transitions are in one place
		public enum NPCState {
			Attack,
			RunAway,
			Wander,
		}

		private NPCState currentState = NPCState.Wander;
		private void NPCUpdate_WithStateMachine() {
			/*			switch (currentState) {
							case NPCState.Attack:
								if (CanSeeEnemy()) {
									Attack();
								} else {
									currentState = NPCState.Wander;
								}
								break;
							case NPCState.RunAway:
								if (IsHurt()) {
									RunAway();
								} else {
									currentState = NPCState.Wander;
								}
								break;
							case NPCState.Wander:
								Wander();
								if (CanSeeEnemy()) {
									currentState = NPCState.Attack;
								} else if (IsHurt()) {
									currentState = NPCState.RunAway;
								}
								break;
						}*/
		}

		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - Using class based states
		// - By abstracting the states into their own classes, we can encapsulate the logic for each state
		// - Each state will determine when it should transition, and which state to transition to

		public class NPC : MonoBehaviour {
			private INPCState currentState;
			private string currentStateName;

			public AttackState attackState = new AttackState();
			public RunAwayState runAwayState = new RunAwayState();
			public WanderState wanderState = new WanderState();

			public NavMeshAgent navMeshAgent;

			public Vector3 nextLocation;
			public GameObject enemyTarget;

			public static List<GameObject> enemies = new List<GameObject>();

			private void OnEnable() {
				currentState = wanderState;
			}

			private void Update() {
				currentState = currentState.DoState(this);
				currentStateName = currentState.ToString();
			}
		}

		public interface INPCState {
			INPCState DoState(NPC npc);
		}

		public class AttackState : INPCState {
			public INPCState DoState(NPC npc) {
				if (CanSeeEnemy()) {
					Attack();
				} else {
					return npc.wanderState;
				}

				return this;
			}

			private void Attack() {
				// Attack logic
			}
			private bool CanSeeEnemy() {
				return true;
			}
		}

		public class RunAwayState : INPCState {
			public INPCState DoState(NPC npc) {
				if (IsHurt()) {
					RunAway();
				} else {
					return npc.wanderState;
				}

				return this;
			}

			private void RunAway() {
				// Run away logic
			}
			private bool IsHurt() {
				return true;
			}
		}

		public class WanderState : INPCState {
			public INPCState DoState(NPC npc) {
				Wander();

				if (CanSeeEnemy()) {
					return npc.attackState;
				} else if (IsHurt()) {
					return npc.runAwayState;
				}

				return this;
			}

			private void Wander() {
				// Wander logic
			}

			private bool IsHurt() {
				return true;
			}

			private bool CanSeeEnemy() {
				return true;
			}
		}
		#endregion

		#region Object Pooling
		// - Object pooling is a design pattern that allows you to reuse objects instead of creating and destroying them
		// - Saving memory and CPU cycles
		// - Useful for objects that are frequently created and destroyed

		// - Reusing objects is faster than creating new ones
		// - This is mostly done by turning the object off and moving it to a new location


		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - Simple pool for a single object:
		// - The problem with this approach is that it only works for a single object type, and when the project gets bigger there will be more object types. Each requiring its own pool

		// Create a gameobject and attach this script to it
		public class NPCPool_Simple : MonoBehaviour {
			[SerializeField] private GameObject enemyPrefab;
			[SerializeField] private Queue<GameObject> enemyPool = new Queue<GameObject>();
			[SerializeField] private int poolStartSize = 5;

			private void Start() {
				for (int i = 0; i < poolStartSize; i++) {
					GameObject enemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
					enemy.SetActive(false);
					enemyPool.Enqueue(enemy);
				}
			}

			public GameObject GetEnemy() {
				if (enemyPool.Count > 0) {
					GameObject enemy = enemyPool.Dequeue();
					enemy.SetActive(true);
					return enemy;
				}

				GameObject newEnemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
				return newEnemy;
			}

			public void ReturnEnemy(GameObject enemy) {
				enemy.SetActive(false);
				enemyPool.Enqueue(enemy);
			}
		}

		// Create a gameobject and attach this script to it
		public class NPCSpawner : MonoBehaviour {
			[SerializeField] private float spawnInterval = 5f;
			private float timeSinceSpawn;
			public NPCPool_Simple objectPool;

			private void Start() {
				objectPool = FindObjectOfType<NPCPool_Simple>();
			}

			private void Update() {
				timeSinceSpawn += Time.deltaTime;
				if (timeSinceSpawn >= spawnInterval) {
					GameObject enemy = objectPool.GetEnemy();
					// Run initialization function if needed
					enemy.transform.position = transform.position;
					timeSinceSpawn = 0f;
				}
			}
		}

		// Add this script to the enemy prefab
		public class SimpleEnemyReturn : MonoBehaviour {
			public NPCPool_Simple objectPool;

			private void Start() {
				objectPool = FindObjectOfType<NPCPool_Simple>();
			}

			private void OnDisable() {
				if (objectPool) objectPool.ReturnEnemy(gameObject);
			}
		}

		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - An advanced pool that can handle multiple object types
		// - Handles multiple object types, and can grow dynamically if needed
		// - Essentially create a pool of pools. There may be a performance hit for this approach but won't be a problem for most games

		// Create a gameobject and attach this script to it
		public class ObjectPool_Advanced : MonoBehaviour {
			private Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>(); // Prefab name, Queue of objects

			public GameObject GetObject(GameObject prefab) {
				if (objectPool.TryGetValue(prefab.name, out Queue<GameObject> pool)) {
					if (pool.Count == 0) {
						return CreateNewObject(prefab);
					} else {
						GameObject gameObject = pool.Dequeue();
						gameObject.SetActive(true);
						return gameObject;
					}
				} else {
					return CreateNewObject(prefab);
				}
			}

			private GameObject CreateNewObject(GameObject prefab) {
				GameObject gameObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
				gameObject.name = prefab.name; // It's important to rename the object the same as the prefab. Crucial step since we're using the name as the dictionary key
				return gameObject;
			}

			public void ReturnGameObject(GameObject gameObject) {
				if (objectPool.TryGetValue(gameObject.name, out Queue<GameObject> pool)) {
					gameObject.SetActive(false);
					pool.Enqueue(gameObject);
				} else {
					Queue<GameObject> newPool = new Queue<GameObject>();
					newPool.Enqueue(gameObject);
					objectPool.Add(gameObject.name, newPool);
				}
			}
		}

		// Create a gameobject and attach this script to it
		public class Spawner_Advanced : MonoBehaviour {
			[SerializeField] private GameObject prefab;
			[SerializeField] private float spawnInterval = 5f;

			private float timeSinceSpawn;
			private ObjectPool_Advanced objectPool;

			private void Start() {
				objectPool = FindObjectOfType<ObjectPool_Advanced>();
			}

			private void Update() {
				timeSinceSpawn += Time.deltaTime;
				if (timeSinceSpawn >= spawnInterval) {
					GameObject gameObject = objectPool.GetObject(prefab);
					// Run initialization function if needed
					gameObject.transform.position = transform.position;
					timeSinceSpawn = 0f;
				}
			}
		}

		// Add this script to the prefab
		// This is just an example any script can be used to return the object to the pool of course
		public class ReturnToPool_Advanced : MonoBehaviour {
			private ObjectPool_Advanced objectPool;

			private void Start() {
				objectPool = FindObjectOfType<ObjectPool_Advanced>();
			}

			private void OnDisable() {
				if (objectPool) objectPool.ReturnGameObject(gameObject);
			}
		}
		#endregion

		#region Observer Pattern
		// - Communicating between objects while decoupling them
		// - Makes it easier to implemenet multiple systems that need to update based on events
		// -- For example a kill UI counter, achievement system, audio system

		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - Delegates:
		// -- Can be thought of as a variable that can be assigned a function as a value
		// -- exampleDelegate = MyFunction; // subscribes function to the delegate
		// -- exampleDelegate?.Invoke(); // check if there is a subscriber, then calls the function

		// - Multicast capable, meaning multiple functions can be subscribed to the same delegate
		// - They can be made public or even static, which allows others classes to subscribe and unsubscribe

		// - Example usage:
		public delegate void ExampleDelegate(string optionalParam, int optionalParam2); //defines the delegate
		public ExampleDelegate exampleDelegate; //creates an instance of the delegate. this will be subscribed to and invoked
		private void OnEnable() {
			// Adding a single function to the delegate
			// Using the = operator will overwrite any existing subscribers, so using += is best practice for subscribing
			//exampleDelegate = MyFunction; //subscribes MyFunction to the delegate

			// Adding multiple functions to the delegate
			exampleDelegate += MyFunction;
			exampleDelegate += MyOtherFunction;
		}

		private void OnDisable() {
			exampleDelegate -= MyFunction; //unsubscribes MyFunction from the delegate
			exampleDelegate -= MyOtherFunction;
		}

		private void Update() {
			exampleDelegate?.Invoke("Hello", 5); //invokes the delegate
		}

		private void MyFunction(string arg1, int arg2) {
			Debug.Log("MyFunction was called with " + arg1 + " and " + arg2);
		}

		private void MyOtherFunction(string arg1, int arg2) {
			// Another function that can be subscribed to the delegate
		}

		// - Example static delegate
		// - Extremely useful for global events
		public class StaticDelegateClass : MonoBehaviour {
			public delegate void StaticDelegate();
			public static StaticDelegate staticDelegate;

			private void Update() {
				staticDelegate?.Invoke();
			}
		}

		public class StaticDelegateSubscriber : MonoBehaviour {
			private void OnEnable() {
				StaticDelegateClass.staticDelegate += MyFunction;
			}

			private void OnDisable() {
				StaticDelegateClass.staticDelegate -= MyFunction;
			}

			private void MyFunction() {
				Debug.Log("Static delegate was called");
			}
		}

		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - Events:
		// - Events are delegates with some extra features
		// - Using the keyword event prevents other classes from overriding subscribed functions, and public invoking
		// -- All that can be done publicly is subscribing and unsubscribing
		// -- Events are a more secure way of implementing the observer pattern
		// -- When implementing the observer pattern, events are the preferred way
		// -- Implementation of events is similar to delegates

		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - Actions and funcs:
		// - Inherit from delegate, making them shortcuts for creating delegates
		// -- Actions are delegates that have input parameters but do not have a return value
		// -- Funcs are delegates that have input parameters and a return value, but funcs handle return values as an out value that is always the last input parameter

		// - Actions and funcs can reduce the number of lines needed to create a delegate
		public class StaticEventClassWithAction : MonoBehaviour {
			// Single line of code, the use of an Action has already defined the delegate for us
			// Using actions this way is just a shorthand for delegates
			public static event Action myStaticEvent; // static instance
			public static event Action<int> myStaticEventWithInt; // static instance with int parameter

			private void Update() {
				myStaticEvent?.Invoke();
				myStaticEventWithInt?.Invoke(5);
			}
		}

		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - Example usage:
		// - Attach this script to the enemy prefab
		public class EnemyDisable : MonoBehaviour {
			public static event Action<GameObject> OnEnemyDisable; // static instance

			private void OnDisable() {
				OnEnemyDisable?.Invoke(gameObject);
			}
		}

		public class GameManager : MonoBehaviour {
			private void OnEnable() {
				EnemyDisable.OnEnemyDisable += OnEnemyDisable;
			}

			private void OnDisable() {
				EnemyDisable.OnEnemyDisable -= OnEnemyDisable;
			}

			private void OnEnemyDisable(GameObject enemy) {
				// Do something, add points, spawn more enemies, etc.
			}
		}

		public class UIManager : MonoBehaviour {
			[SerializeField] private TextMeshProUGUI killCounter;

			private int killCount;

			private void OnEnable() {
				EnemyDisable.OnEnemyDisable += OnEnemyDisable;
			}

			private void OnDisable() {
				EnemyDisable.OnEnemyDisable -= OnEnemyDisable;
			}

			private void OnEnemyDisable(GameObject enemy) {
				// Update the UI, show a kill counter, etc.
				killCount++;
				killCounter.SetText("Kills: " + killCount.ToString("N0"));
			}
		}

		public class AchievementSystem : MonoBehaviour {
			private void OnEnable() {
				EnemyDisable.OnEnemyDisable += OnEnemyDisable;
			}

			private void OnDisable() {
				EnemyDisable.OnEnemyDisable -= OnEnemyDisable;
			}

			private void OnEnemyDisable(GameObject enemy) {
				// Check if the player has killed a certain amount of enemies, and give an achievement
			}
		}
		#endregion
	}
}