﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableSlot : VR_Interactable_UI
{

	[SerializeField] private Image image;
	[SerializeField] private Text stack;
	private int index;
	private VR_Interactable_Object pendingItem;


	// Use this for initialization
	protected override void Start()
	{
		base.Start();
		GetComponent<BoxCollider>().isTrigger = true;

	}

	public int Index
	{
		get { return this.index; }
		set { this.index = value; }
	}

	protected override void Update()
	{
		base.Update();
		DisplayInfo();
	}


	private void DisplayInfo()
	{
		if (GetReferredSlot().StoredItem != null)
		{
			image.color = new Color(1, 1, 1, 1);
			stack.color = new Color(1, 1, 1, 1);

			image.sprite = GetReferredSlot().StoredItem.Icon;
			stack.text = GetReferredSlot().CurrentStack.ToString();


		}
		else
		{
			image.color = new Color(0, 0, 0, 0);
			stack.color = new Color(0, 0, 0, 0);
		}



	}

	// To get the index of the referenced slot,
	// we take the (page number) * (number of slots per page) - (number of slots  - current index) - 1
	private Inventory.InventorySlot GetReferredSlot()
	{
		int referencedIndex = (StoragePanel.Instance.CurrentPageNumber) * (StoragePanel.Instance.NumberOfSlotsPerPage) + index;
		// - (StoragePanel.Instance.NumberOfSlotsPerPage - (index + 1)) - 1;

		return StoragePanel.Instance._Inventory.InventoryItemsArr[referencedIndex];



	}



	private void RemoveFromSlot()
	{

		Inventory.InventorySlot temp = GetReferredSlot();

		if (temp.StoredItem != null)
		{
			temp.CurrentStack--;

			GenericItem instanceInteractable = Instantiate(temp.StoredItem.ObjectReference,transform.position, temp.StoredItem.ObjectReference.transform.rotation).GetComponent<GenericItem>();
			instanceInteractable.OnUpdateInteraction(currentInteractingController);
			instanceInteractable.OnTriggerPress(currentInteractingController);

			if (temp.CurrentStack < 1)
			{
				temp.EmptySlot();
			}
		}


	}


	private void AddToSlot(ItemData item)
	{
		Inventory.InventorySlot temp = GetReferredSlot();

		if (temp.StoredItem == null)
		{

			temp.StoredItem = item;
			temp.CurrentStack++;
			currentInteractingController = null;

		}
		else if (temp.StoredItem.ItemID == item.ItemID)
		{
			if (temp.CurrentStack < temp.StoredItem.MaxStackSize)
			{

				temp.CurrentStack++;
			}
			else
			{
				//show red outline
			}
		}



	}




	protected override void OnTriggerPress()
	{
		if (currentInteractingController.UI == this && currentInteractingController.CurrentItemInHand == null)
		{
			base.OnTriggerPress();
			RemoveFromSlot();
		}
	}

	protected override void OnTriggerRelease()
	{
		if (currentInteractingController.UI == this)
		{
			GenericItem g = null;
			if (currentInteractingController.CurrentItemInHand is GenericItem)
			{
				g = currentInteractingController.CurrentItemInHand as GenericItem;
				Debug.Log("0");
			}
			Debug.Log(g);
			if (g)
			{
				ItemData d = ItemManager.Instance.GetItemData(g.ItemID);

				if (d != null && (GetReferredSlot().StoredItem == null || GetReferredSlot().StoredItem.ItemID == d.ItemID))
				{
					currentInteractingController.SetModelActive(true);
					AddToSlot(d);
					Destroy(g.gameObject);
					Debug.Log("1");
				}
				Debug.Log("2");

			}
			Debug.Log("3");
			base.OnTriggerRelease();

		}
	}

}
