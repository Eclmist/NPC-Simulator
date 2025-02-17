﻿using System.Collections;





using UnityEngine;





[RequireComponent(typeof(AudioSource))]


public class GenericItem : VR_Interactable_Object, IDamage
{

	protected bool isColliding = false;

	[SerializeField] protected float directionalMultiplier = 5f, maxLerpForce = 10f;
	protected float collisionVibrationMagnitude = 0.002F;

	protected JobType jobType;
	[SerializeField] protected string _name;


	public JobType JobType
	{
		get { return this.jobType; }
	}

	protected Collider itemCollider;

	//protected Vector3 positionOffset;
	//protected Quaternion rotationOffset;

	#region itemData

	[SerializeField] [ReadOnly] private int itemID = -1;

	public int ItemID


	{
		get { return this.itemID; }
		set { this.itemID = value; }
	}

	#endregion itemData


	#region GenericItem
	[SerializeField]


	protected int damage;


	protected AudioSource audioSource;

	#endregion GenericItem


	#region IInteractable





	private bool isFlying;





	private IDamagable target;





	[SerializeField]


	private int maxVelocityDamageMultiplier = 5;





	#endregion IInteractable





	#region





	[SerializeField]


	protected float maxSwingForce;





	#endregion


	public virtual string Description
	{
		get
		{
			return name;
		}
	}

	public virtual string _Name
	{
		get { return this._name; }
	}




	protected override void Awake()





	{


		itemCollider = GetComponentInChildren<Collider>();





		audioSource = GetComponent<AudioSource>();





		base.Awake();


	}





	public virtual float Damage


	{


		get

		{

			if (currentInteractingController != null)
			{
				float val = currentInteractingController.Velocity.magnitude;
				if (val < 1)
					val = 1;
				return val < maxVelocityDamageMultiplier ? (int)(val * damage) : damage * maxVelocityDamageMultiplier;

			}
			else if (isFlying)





				return rigidBody.velocity.magnitude < maxVelocityDamageMultiplier ? (int)(rigidBody.velocity.magnitude * damage) : damage * maxVelocityDamageMultiplier;


			else





				return damage;


		}


	}









	public override void OnTriggerRelease(VR_Controller_Custom referenceCheck)
	{
		base.OnTriggerRelease(referenceCheck);

		rigidBody.velocity = Vector3.zero;
		rigidBody.angularVelocity = Vector3.zero;
		if (!float.IsNaN(currentReleaseVelocity.magnitude))
			rigidBody.AddForceAtPosition(currentReleaseVelocity, targetPositionPoint.transform.position, ForceMode.Impulse);

		gameObject.layer = LayerMask.NameToLayer("Interactable");
		rigidBody.useGravity = true;
		StartCoroutine(Throw(Player.Instance));
	}





	public virtual IEnumerator Throw(Actor thrower)

	{

		isFlying = true;

		while (rigidBody.velocity.magnitude > 0.1)

		{

			if (target != null)

			{


				thrower.Attack(this, target);

				target = null;

				break;

			}

			yield return new WaitForEndOfFrame();


		}





		target = null;





		isFlying = false;


	}



	public override void OnFixedUpdateInteraction(VR_Controller_Custom referenceCheck)
	{
		base.OnFixedUpdateInteraction();

		Vector3 PositionDelta = targetPositionPoint.transform.position - transform.position;

		if (!isColliding)
		{
			rigidBody.MovePosition(targetPositionPoint.transform.position);
			rigidBody.MoveRotation(targetPositionPoint.transform.rotation);
		}
		else
		{
			referenceCheck.Vibrate(collisionVibrationMagnitude);

			float currentForce = maxLerpForce;

			rigidBody.velocity =
				PositionDelta.magnitude * directionalMultiplier > currentForce ?
				(PositionDelta).normalized * currentForce : PositionDelta * directionalMultiplier;


			// Rotation ----------------------------------------------

			//Quaternion targetRotation = referenceCheck.transform.rotation * rotationOffset;

			Quaternion RotationDelta = targetPositionPoint.transform.rotation * Quaternion.Inverse(this.transform.rotation);
			float angle;
			Vector3 axis;
			RotationDelta.ToAngleAxis(out angle, out axis);

			if (angle > 180)
				angle -= 360;

			float angularVelocityNumber = .2f;

			// -------------------------------------------------------
			rigidBody.angularVelocity = axis * angle * angularVelocityNumber;

		}
	}





	public override void OnTriggerPress(VR_Controller_Custom controller)
	{

		controller.SetModelActive(false);
		rigidBody.useGravity = false;
		rigidBody.isKinematic = false;
		gameObject.layer = LayerMask.NameToLayer("Player");
		base.OnTriggerPress(controller);


	}





	protected virtual void OnCollisionEnter(Collision col)
	{
		if (isFlying)
		{
			target = col.transform.GetComponent<Monster>();
		}

		isColliding = true;

		if (currentInteractingController != null)
		{
			// float value = currentInteractingController.Velocity.magnitude <= collisionVibrationMagnitude ? currentInteractingController.Velocity.magnitude : collisionVibrationMagnitude;
			currentInteractingController.Vibrate(triggerEnterVibration);
		}
	}

	protected virtual void OnCollisionStay(Collision collision)
	{
	}

	protected virtual void OnCollisionExit(Collision collision)
	{
		isColliding = false;
	}



	protected void PlaySound(float volume)





	{


		if (audioSource != null)


		{


			audioSource.volume = volume;


			audioSource.Play();


		}


	}

	public virtual GenericItemSceneData GetSceneData()
	{
		return new GenericItemSceneData(itemID, transform.position, transform.rotation);
	}

	public virtual void SetupObject(GenericItemSceneData data)
	{
		transform.position = data.Position;
		transform.rotation = data.Rotation;
		itemID = data.ID;
	}

}