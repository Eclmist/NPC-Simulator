﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public abstract class ActorFSM : MonoBehaviour
{

	public enum FSMState //changed to fsm state
	{
		IDLE,
		PETROL,
		COMBAT,
		EVENTHANDLING,
		SHOPPING,
		DEATH
	}

	protected bool isHandlingAction = false;

	protected Vector3 actualVelocity;
	[SerializeField]
	protected Transform eye;

	[SerializeField]
	protected float detectionDistance = 10, attackRange = 0;
	protected List<Node> path;
	protected bool requestedPath, pathFound;
	[SerializeField]
	protected FSMState currentState;
	protected IDamagable target;
	protected Animator animator;
	protected Rigidbody rigidBody;
	protected FSMState nextState;
	protected List<Vector3> pathNodeOffset = new List<Vector3>();
	protected List<NodeEvent> handledEvents = new List<NodeEvent>();

	[SerializeField]
	[Range(0.5f, 5)]
	protected float movementSpeed = 1;

	[SerializeField]
	protected LayerMask aggroLayer;
	#region Avoidance
	[Header("Avoidance")]
	[SerializeField]
	protected float minimumDistToAvoid;
	[SerializeField]
	protected LayerMask avoidanceIgnoreMask;
	#endregion

	public abstract AI CurrentAI
	{
		get;
	}
	public virtual void ChangeState(FSMState state)
	{

		isHandlingAction = false;
		StopAllCoroutines();
		pathFound = false;
		currentState = state;
		animator.speed = 1;
		animator.SetFloat("Speed", 0);
		animator.SetBool("Attacking", false);

		if (state != FSMState.EVENTHANDLING)
		{
			foreach (NodeEvent e in handledEvents)
			{
				e.CurrentNode.Occupied = false;
			}
			handledEvents.Clear();
		}

		switch (state)
		{
			case FSMState.DEATH:
				animator.SetBool("Death", true);
				return;
			case FSMState.PETROL:
				petrolWaitThreshold = 5;
				animator.SetFloat("Speed", movementSpeed);
				SetNodeOffset();
				return;

			default:
				return;
		}

	}
	public virtual void NewSpawn()
	{
		nextState = FSMState.IDLE;
		ChangeState(FSMState.IDLE);

	}

	protected virtual void Awake()
	{
		animator = GetComponent<Animator>();
		rigidBody = GetComponent<Rigidbody>();
	}
	public IDamagable Target
	{
		get
		{
			return target;
		}
		set
		{
			target = value;
		}
	}

	public void SetNodeOffset()
	{
		pathNodeOffset.Clear();
		foreach (Node n in path)
		{
			Vector2 newVec2 = Random.insideUnitCircle * .25f;
			Vector3 newVec3 = new Vector3(newVec2.x, 0, newVec2.y);
			pathNodeOffset.Add(newVec3);
		}
	}
	// Use this for initialization

	public FSMState State
	{
		get
		{
			return currentState;
		}
		set
		{
			currentState = value;
		}
	}

	protected virtual void FixedUpdate()
	{
		if (!animator.GetBool("Stun"))
			rigidBody.velocity = new Vector3(actualVelocity.x, rigidBody.velocity.y, actualVelocity.z);
		rigidBody.angularVelocity = Vector3.zero;
		if (CanMove())
			rigidBody.isKinematic = false;
		else
			rigidBody.isKinematic = true;

	}
	protected virtual void Update()
	{
		actualVelocity = Vector3.zero;

		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
		{
			UpdateFSMState();
		}
		else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Death") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
		{
			CurrentAI.Despawn();
		}
	}

	protected virtual void UpdateFSMState()
	{

		UpdateAnyState();
		switch (currentState)
		{
			case FSMState.EVENTHANDLING:
				UpdateEventHandling();
				return;
			case FSMState.PETROL:
				UpdatePetrolState();
				return;
			case FSMState.COMBAT:
				UpdateCombatState();
				return;
			case FSMState.IDLE:
				UpdateIdleState();
				return;

		}
	}

	protected virtual void UpdateAnyState()
	{
		if (target != null && target.Equals(null))
		{
			target = null;
		}

		if (currentState != FSMState.COMBAT)
		{
			if (CheckForTargets())
			{
				ChangeState(FSMState.COMBAT);
			}
		}

	}

	protected virtual bool CheckForTargets()
	{
		Vector3 subVec = transform.position;
		subVec.y = eye.position.y;
		Collider[] cols = Physics.OverlapCapsule(subVec, transform.position,
						CurrentAI.Collider.bounds.extents.x + minimumDistToAvoid,
						aggroLayer);
		foreach (Collider col in cols)
		{
			IDamagable currentTarget = col.GetComponent<IDamagable>();
			if (currentTarget != null && currentTarget.CanBeAttacked)
			{
				target = currentTarget;
				return true;
			}
		}
		return false;
	}


	public virtual void SetStun(int i)
	{

		switch (i)
		{
			case 1:
				animator.SetBool("Stun", true);
				break;
			case 0:
				animator.SetBool("Stun", false);
				break;
		}
	}

	public virtual void SetVelocity(Vector3 vel)
	{
		rigidBody.isKinematic = false;
		rigidBody.AddForce(vel, ForceMode.Impulse);
	}
	//if (backing)
	//{
	//    animator.SetFloat("Speed", movementSpeed);
	//    targetPoint = transform.position - dir;
	//    if (distance > tempAttackRange / 2.0)
	//        backing = false;
	//}
	//else if (!backing && distance < tempAttackRange / 8.0)
	//{
	//    animator.SetBool("Attacking", false);
	//    animator.SetFloat("Speed", movementSpeed);
	//    targetPoint = transform.position - dir;
	//    backing = true;
	//}

	protected abstract void UpdateIdleState();



	protected virtual void UpdateCombatState()
	{
		if (target != null && !target.Equals(null) && target.CanBeAttacked)
		{

			float tempAttackRange = attackRange;

		
			Vector3 useVector = transform.position;
			useVector.y = eye.position.y;
			Vector3 dir = (target.transform.position - useVector).normalized;

			Ray ray = new Ray(useVector, dir);
			RaycastHit hit;

			//Debug.DrawRay(head.transform.position - head.right, _direction);
			if (target.Collider.Raycast(ray, out hit, detectionDistance))
			{
				dir.Normalize();
				dir.y = 0;

				float angle = Vector3.Angle(transform.forward, dir);
				Vector3 tempPoint = hit.point;
				tempPoint.y = transform.position.y;
				float distance = Vector3.Distance(transform.position, tempPoint);

				if (distance > tempAttackRange || Mathf.Abs(angle) > 30)
				{

					animator.SetBool("Attacking", false);
					animator.SetFloat("Speed", movementSpeed);
					if (CanMove())
					{
						LookAtPlayer(target.transform.position);
						actualVelocity = transform.forward * animator.speed * movementSpeed;
					}
				}
				else
				{
					animator.SetFloat("Speed", 0);
					HandleCombatAction();

				}
				return;
			}

		}
		if (!CheckForTargets())
			ChangeState(FSMState.IDLE);


	}

	protected virtual void HandleCombatAction()
	{

	}

	protected virtual void UpdateEventHandling()
	{
		if (!isHandlingAction)
		{
			if (handledEvents.Count <= 0)
			{
				path[0].Occupied = false;
				path.RemoveAt(0);
				pathNodeOffset.RemoveAt(0);
				ChangeState(FSMState.PETROL);
				return;
			}
			else
			{
				NodeEvent firstEvent = handledEvents[0];
				handledEvents.RemoveAt(0);
				if (firstEvent.Activated && (!firstEvent.Final || (firstEvent.Final && path.Count == 1)))
					firstEvent.HandleEvent(CurrentAI);
			}
		}
	}

	protected float petrolWaitThreshold = 5f;

	protected virtual void UpdatePetrolState()
	{
		if (path.Count == 0)
		{
			ChangeState(nextState);
		}
		else
		{
			Vector3 targetPoint = path[0].Position + pathNodeOffset[0];
			targetPoint.y = transform.position.y;

			if (path.Count == 1 && path[0].Occupied)
			{
				animator.SetFloat("Speed", 0);
				petrolWaitThreshold -= Time.deltaTime;
				if (petrolWaitThreshold <= 0f)
					ChangeState(nextState);


			}
			else
			{
				petrolWaitThreshold = 5;

				float minDist = path.Count > 1 ? 1f : .5f;

				animator.SetFloat("Speed", movementSpeed);
				if (CanMove())
				{
					LookAtPlayer(targetPoint);
					actualVelocity = transform.forward * animator.speed * movementSpeed;
				}


				if (Vector3.Distance(transform.position, targetPoint) < minDist)
				{

					handledEvents.AddRange(path[0].Events);
					if (handledEvents.Count > 0)
					{
						path[0].Occupied = true;
						ChangeState(FSMState.EVENTHANDLING);
					}
					else
					{
						path.RemoveAt(0);
						pathNodeOffset.RemoveAt(0);
					}
				}
			}
		}
	}

	protected virtual bool CanMove()
	{
		return (animator.GetCurrentAnimatorStateInfo(0).IsName("Petrol") ||
			animator.GetAnimatorTransitionInfo(0).IsName("Idle -> Petrol")
			|| animator.GetBool("Stun")) && currentState != FSMState.DEATH;
	}
	public void SetNode(Node n)
	{
		int index = -1;
		for (int i = 0; i < path.Count; i++)
		{
			if (path[i] == n)
				index = i;
		}
		if (index != 1)
			path.RemoveRange(0, index);
	}

	[SerializeField]
	protected EquipSlot.EquipmentSlotType animUseSlot;

	protected void ChangePath(List<Node> _path)
	{
		if (_path.Count > 0)
		{
			pathFound = true;
			path = _path;
			requestedPath = false;
		}
		else
		{
            Debug.Log("cantfind");
			pathFound = false;
			requestedPath = false;
		}
	}

	protected void ChangePath(Node _path)
	{
		List<Node> newPath = new List<Node>();
		newPath.Add(_path);
		ChangePath(newPath);
	}

	protected virtual void CheckHit()
	{
		float tempAttackRange = attackRange;
		if (target is Player)
			tempAttackRange += 1;
		Vector3 dir = (target.transform.position - transform.position).normalized;
		dir.y = 0;
		float angle = Vector3.Angle(transform.forward, dir);

		if (target != null)
		{
			GenericItem currentUseWeapon = CurrentAI.returnSlotItem(animUseSlot);
            Vector3 lookAtDir = (target.transform.position - eye.position).normalized;
            Ray ray = new Ray(eye.position, lookAtDir);
            RaycastHit hit;
			if (Mathf.Abs(angle) < 30 && target.Collider.Raycast(ray, out hit, attackRange))
			{
				CurrentAI.Attack(currentUseWeapon, target);
			}
			else if (target is Actor)
			{
				CurrentAI.DealDamage(0, target, JobType.COMBAT);
			}

		}
	}


	public virtual void SetAnimUseSlot(int i)
	{
		switch (i)
		{
			case 0:
				animUseSlot = EquipSlot.EquipmentSlotType.LEFTHAND;
				break;
			case 1:
				animUseSlot = EquipSlot.EquipmentSlotType.RIGHTHAND;
				break;

		}
	}


	protected Vector3 AvoidObstacles(Vector3 endPoint)
	{
		RaycastHit Hit;
		//Check that the vehicle hit with the obstacles within it's minimum distance to avoid
		Vector3 useVec = eye.position;
		useVec.y = (transform.position.y + eye.position.y) / 2;
		Vector3 right45 = (transform.forward + transform.right).normalized;
		Vector3 left45 = (transform.forward - transform.right).normalized;

		if (Physics.Raycast(useVec, right45, out Hit,
			minimumDistToAvoid, ~avoidanceIgnoreMask))
		{
			float distanceExp = Vector3.Distance(Hit.point, useVec) / minimumDistToAvoid;
			// 5 if near, 0 if far
			//distanceExp = 5 - distanceExp * 5;

			return -transform.right * (1 - distanceExp) * 5;
		}
		else if (Physics.Raycast(useVec, left45, out Hit,
			minimumDistToAvoid, ~avoidanceIgnoreMask))
		{
			float distanceExp = Vector3.Distance(Hit.point, useVec) / minimumDistToAvoid;
			// 5 if near, 0 if far
			//distanceExp = 5 - distanceExp * 5;

			return transform.right * (1 - distanceExp) * 5;
		}

		else
		{

			Vector3 dir = (endPoint - transform.position).normalized;
			dir.y = rigidBody.velocity.y;
			return dir;
		}
	}

	public virtual void DamageTaken(Actor attacker)
	{
		if (!(target is Actor) && currentState != FSMState.DEATH)
		{
			ChangeState(FSMState.COMBAT);
			target = attacker;
		}
	}


	protected void LookAtPlayer(Vector3 endPoint)
	{
		//float distance = (endPoint - transform.position).magnitude / 60;
		//float angle = Vector3.Angle(endPoint - transform.position, transform.forward) / 180;
		//float rotationSpeed = turnRateOverAngle.Evaluate(angle);

		Vector3 lookAtTarget = AvoidObstacles(endPoint);
		lookAtTarget.y = 0; //Force no y change;
		float lerpSpeed = lookAtTarget.magnitude * 5;
		if (lookAtTarget != Vector3.zero)
			transform.rotation = Quaternion.Slerp(transform.rotation,
				Quaternion.LookRotation(lookAtTarget, Vector3.up),
				lerpSpeed * Time.deltaTime);//hardcorded 10
	}

	//protected Vector3 GetReversePoint()
	//{
	//    RaycastHit Hit;
	//    //Check that the vehicle hit with the obstacles within it's minimum distance to avoid
	//    Vector3 dir = -transform.forward;

	//    if (Physics.Raycast(transform.position, dir, out Hit,
	//        minimumDistToAvoid, ~avoidanceIgnoreMask))
	//    {

	//        return Hit.point;
	//    }
	//    else
	//        return (-transform.forward * minimumDistToAvoid) + transform.position;
	//}
	public void StartLookAtRoutine(Transform targetTransform, float seconds, float angle)
	{
		StartCoroutine(LookAtTransform(targetTransform, seconds, angle));
	}
	public IEnumerator LookAtTransform(Transform targetTransform, float seconds, float angle)
	{
		while (seconds >= 0)
		{

			isHandlingAction = true;

			Vector3 dir = targetTransform.position - transform.position;
			dir.y = 0;
			transform.rotation = Quaternion.Slerp(transform.rotation,
				Quaternion.LookRotation(dir, Vector3.up),
				5 * Time.deltaTime);//hardcorded 10
			yield return new WaitForEndOfFrame();
			seconds -= Time.deltaTime;

		}
		isHandlingAction = false;

	}



}
