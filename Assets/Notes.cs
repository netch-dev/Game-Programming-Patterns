using System.Collections.Generic;
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
	}
}
