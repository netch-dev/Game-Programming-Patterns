using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Netch.GameProgrammingPatterns {
	public class Notes : MonoBehaviour {
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

		#region Strategy Pattern
		// - All about minimizing the duplication of code and decoupling classes
		// - Can allow algos to be changed at runtime without any messy switch statements/long chains of if statements

		// - For example it's useful for creating weapons that have different damage types
		// - Use an interface
		public interface IDODamage {
			void DoDamage(int amount);
		}

		// - Don't implement this interface into all of the weapons. That would create a lot of duplicate code
		// -- Instead we can create an instance variable of the interface in the BaseWeapon class
		public class BaseWeapon {
			public int damage = 0;
			public IDODamage damageType;

			public void TryDoAttack() {
				damageType?.DoDamage(damage);
			}

			// The strategy pattern allows us to change the damage type at runtime
			public void SetDamageType(IDODamage damageType) {
				this.damageType = damageType;
			}
		}

		// -- This way we can create classes that implement the IDODamage interface, each with their own damage behaviour
		// -- Encapulating the IDODamage behaviour makes it easier to change at runtime with a simple var assignment
		public class FireDamage : IDODamage {
			public void DoDamage(int damage) {
				// PlayerStats.target.health -= damage;
				// Do fire specific logic
			}
		}

		// -- Then we create a new class for each weapon that inherits from BaseWeapon, and sets the type of damage in the constructor
		public class Fire_Dagger : BaseWeapon {
			public Fire_Dagger() {
				damage = 40;
				damageType = new FireDamage();
			}
		}
		public class Fire_Sword : BaseWeapon {
			public Fire_Sword() {
				damage = 60;
				damageType = new FireDamage();
			}
		}

		// - Easy to add new damage types, and weapons
		public class IceDamage : IDODamage {
			public void DoDamage(int damage) {
				// PlayerStats.target.health -= damage;
				// Do ice specific logic
			}
		}

		public class Ice_Sword : BaseWeapon {
			public Ice_Sword() {
				damage = 50;
				damageType = new IceDamage();
			}
		}

		// - One downside of the strategy pattern is that it can create a lot of classes, but we have very little duplicated code
		// -- If we have to change the FireDamage behaviour, we only have to change it in one place

		// - You can go even further and create generic weapons that have their damage and damage type set by a constructor
		// -- This allows for generic classes for each weapon type, with all of the data injected when the instance is created
		public class Sword : BaseWeapon {
			public Sword(int damage, IDODamage damageType) {
				this.damage = damage;
				this.damageType = damageType;
			}
		}

		// - If you want to have a weapon with multiple damage types, you can create a class that holds multiple damage types
		public class MultiDamage : IDODamage {
			private List<IDODamage> damageTypes = new List<IDODamage>();

			public void AddDamageType(IDODamage damageType) {
				damageTypes.Add(damageType);
			}

			public void RemoveDamageType(IDODamage damageType) {
				damageTypes.Remove(damageType);
			}

			public void DoDamage(int damage) {
				foreach (IDODamage damageType in damageTypes) {
					damageType.DoDamage(damage);
				}
			}
		}
		#endregion

		#region Command Pattern
		// - Sending encapsulated commands to objects. All information that is needed to execute the command is wrapped up in the command class
		// -- This is great because the object executing this command doesn't need any external references. All it has to do is tell the command to execute

		// - Useful for creating a queue of commands, undo/redo systems, that can be run right away or any time later
		// -- For example in a turn based strategy game. All of the commands are queued up and ran when it's the player's turn

		// - It may be worth it to use a pooling system for the ICommand objects, to avoid creating and destroying them all the time

		public interface ICommand {
			void Execute();

			void Undo();
		}

		[System.Serializable]
		public class MoveCommand : ICommand {
			[SerializeField] private Vector3 direction = Vector3.zero;
			private float distance;
			private Transform objectToMove;

			public MoveCommand(Transform objectToMove, Vector3 direction, float distance) {
				this.objectToMove = objectToMove;
				this.direction = direction;
				this.distance = distance;
			}

			public void Execute() {
				objectToMove.position += direction * distance;
			}

			public void Undo() {
				objectToMove.position -= direction * distance;
			}
		}

		// - Then we can use an input manager to create commands
		public class InputManager : MonoBehaviour {
			[SerializeField] private Button up;
			[SerializeField] private Button down;
			[SerializeField] private Button left;
			[SerializeField] private Button right;

			[SerializeField] private Button doTurn;

			[SerializeField] private Button undo;

			[SerializeField] private TurnBasedCharacter character;

			[SerializeField] private UICommandList uiCommandList;

			private void OnEnable() {
				up.onClick.AddListener(() => SendMoveCommand(character.transform, Vector3.forward, 1));
				down.onClick.AddListener(() => SendMoveCommand(character.transform, Vector3.back, 1));
				left.onClick.AddListener(() => SendMoveCommand(character.transform, Vector3.left, 1));
				right.onClick.AddListener(() => SendMoveCommand(character.transform, Vector3.right, 1));

				doTurn.onClick.AddListener(() => character.ExecuteCommands());

				undo.onClick.AddListener(() => character.UndoCommand());
			}

			private void SendMoveCommand(Transform objectToMove, Vector3 direction, float distance) {
				MoveCommand moveCommand = new MoveCommand(objectToMove, direction, distance);
				// Would be better to use events (Observer pattern) to send the command but this is focused on the command pattern
				character?.AddCommand(moveCommand);
				uiCommandList?.AddCommand(moveCommand);
			}
		}

		public class TurnBasedCharacter : MonoBehaviour {
			[SerializeField] private List<ICommand> commandList = new List<ICommand>();

			public void AddCommand(ICommand command) {
				commandList.Add(command);
			}

			public void UndoCommand() {
				if (commandList.Count == 0) return;

				commandList[commandList.Count - 1].Undo();
				commandList.RemoveAt(commandList.Count - 1);
			}

			public void ExecuteCommands() {
				StartCoroutine(ExecuteCommandsCoroutine());
			}

			private IEnumerator ExecuteCommandsCoroutine() {
				foreach (ICommand command in commandList) {
					command.Execute();
					yield return new WaitForSeconds(1f);
				}
			}
		}

		public class UICommandList {
			// Display the commands in the queue
			public void AddCommand(ICommand command) {
				// Add command to the UI
			}
		}

		// -------------------------------
		// -------------------------------
		// -------------------------------

		// - The code above works, but the command handling should not be in the player class

		// - Extracting it to a separate class makes it easier for other objects that might need to run commands
		public class CommandHandler {
			private List<ICommand> commandList = new List<ICommand>();
			private int index; // Track the index of the last command executed for the redo function

			public void AddCommand(ICommand command) {
				if (index < commandList.Count) {
					commandList.RemoveRange(index, commandList.Count - index);
				}

				commandList.Add(command);
				command.Execute();
				index++;
			}

			public void UndoCommand() {
				if (index == 0) return;

				if (index > 0) {
					commandList[index - 1].Undo();
					index--;
				}
			}

			public void RedoCommand() {
				if (commandList.Count == 0) return;

				if (index < commandList.Count) {
					index++;
					commandList[index - 1].Execute();
				}
			}
		}

		// Then use this class in the InputManager as the character, and re-route the buttons to the command handler
		public class CharacterMoveClean : MonoBehaviour {
			public CommandHandler commandHandler = new CommandHandler();
		}
		#endregion

		#region Generics in C#
		// - Generics are used to write less code and make it more reusable

		// - Many generic functions can operate as static functions	 so to maximize their usefulness put them in a static class

		// - Instead of having classes for each type of object, you can create a generic class that can be used with any type
		// --	 of each type having its own class:
		public class Shape : MonoBehaviour {

		}

		public class Cube : Shape {

		}

		public class Sphere : Shape {

		}

		public class Capsule : Shape {

		}

		// - If we wanted to find all objects of each specific type we'd need 3 different functions
		// -- This is not very efficient and can be hard to maintain

		private List<Shape> allShapesInScene = new List<Shape>();

		private List<Cube> GetAllCubes() {
			List<Cube> list = new List<Cube>();
			foreach (Shape shape in allShapesInScene) {
				if (shape is Cube) {
					list.Add((Cube)shape);
				}
			}
			return list;
		}

		private List<Sphere> GetAllSpheres() {
			List<Sphere> list = new List<Sphere>();
			foreach (Shape shape in allShapesInScene) {
				if (shape is Sphere) {
					list.Add((Sphere)shape);
				}
			}
			return list;
		}

		// Etc

		// - Now we have 3 functions that do almost the exact same thing
		// -- Even worse if we want to add a fourth shape we'd need another function 

		// - Since the only difference between functions is the type, a better way would be to use generics
		// -- Use a single function that works with any type

		private List<T> FindAnyTypeOfShape<T>() where T : Shape {
			List<T> values = new List<T>();
			foreach (Shape shape in allShapesInScene) {
				if (shape is T) {
					values.Add((T)shape);
				}
			}
			return values;
		}

		// - The keyword where acts as a constraint, limiting the types that can be used with the function
		// -- Above it limits the types to only those that inherit from Shape
		// --- Without a constraint the compiler assume the type is an object, which has limits on what can be done with it

		// - For example if we want to destroy objects of a given type within the scene
		// -- Since we constrained the type to Component we can access the gameObject
		private void DestroyObjectsOfType<T>() where T : Component {
			T[] objectsInScene = FindObjectsOfType<T>();
			foreach (T obj in objectsInScene) {
				if (Application.isPlaying) {
					Destroy(obj.gameObject);
				} else {
					DestroyImmediate(obj.gameObject);
				}
			}
		}

		// - Another use case for generics is to check for hovering over a specific object
		// -- Rather than check for a specific component we can check for a generic type
		private bool IsPlayerHoveringOverObject<T>() where T : Component {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit)) {
				return hit.collider.GetComponent<T>() != null;
			}
			return false;
		}

		private void Update_HoverTest() {
			bool isMouseHoveringOverCube = IsPlayerHoveringOverObject<Cube>();
		}

		// - We can go a step further and create a generic function that can be used with any type
		// -- By introducing a second type parameter we can specify the type of the list. TClass is the parent class, TSubClass is the subclass
		private List<TSubClass> FindTypesInList<TClass, TSubClass>(List<TClass> list) where TSubClass : TClass {
			List<TSubClass> subclassList = new List<TSubClass>();

			foreach (TClass item in list) {
				if (item is TSubClass) {
					subclassList.Add((TSubClass)item);
				}
			}

			return subclassList;
		}

		// - A step even further into the function above, we can make it only useable with a list of MonoBehaviour
		// -- To do that we need to add a second constraint
		// where TClass : MonoBehaviour
		private List<TSubClass> FindMonoBehaviourTypesInList<TClass, TSubClass>(List<TClass> list) where TSubClass : TClass where TClass : MonoBehaviour {
			List<TSubClass> subclassList = new List<TSubClass>();
			foreach (TClass item in list) {
				if (item is TSubClass) {
					subclassList.Add((TSubClass)item);
				}
			}
			return subclassList;
		}
		#endregion
	}
}