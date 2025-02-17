﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public struct UIPrefabs
{
	public GameObject OP_Yes_No;
	public GameObject OP_Ok;
    public GameObject OP_Yes_No_Cancel;
}

public enum UIType
{
    OP_YES_NO,
    OP_OK,
    OP_YES_NO_CANCEL
}


public class UIManager : MonoBehaviour 
{

	[SerializeField] private UIPrefabs prefabs;

	public static UIManager Instance;

	protected void Awake ()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(this);		
	}


    public OptionPane InstantiateDetailPane(GameObject detailPane,Sprite icon,string s,string s2, Vector3 position, Transform receiver,
       Transform sender = null)
    {

        GameObject newUIobject = null;
        OptionPane options = null;

        newUIobject = Instantiate(detailPane);
        options = newUIobject.GetComponent<OP_Ok>();

        Vector3 toReceiver = receiver.transform.position - position;
        newUIobject.transform.position = position + toReceiver.normalized * 0.2F;

        Vector3 lookAt = receiver.transform.position;
        lookAt.y = position.y;
        newUIobject.transform.LookAt(lookAt);

        options.SetContents(s,s2,null,icon);

        return options;
    }


    public OptionPane InstantiateOptions(Vector3 position, Transform receiver,
       Transform sender = null)
    {
        GameObject newUIobject = null;
        OptionPane options = null;

        newUIobject = Instantiate(prefabs.OP_Yes_No_Cancel);
        options = newUIobject.GetComponent<OP_YesNoCancel>();

        Vector3 toReceiver = receiver.transform.position - position;
        newUIobject.transform.position = position + toReceiver.normalized * 0.2F;

        Vector3 lookAt = receiver.transform.position;
        lookAt.y = position.y;
        newUIobject.transform.LookAt(lookAt);


        return options;
    }





    public OptionPane Instantiate(UIType type, string title, string message, Vector3 position, Transform receiver,
		Transform sender = null)
	{

        if (type == UIType.OP_YES_NO_CANCEL)
        {
            return InstantiateOptions(position,receiver);
        }

		GameObject newUIobject = null;
		OptionPane options = null;

		switch (type)
		{
			case UIType.OP_OK:
				newUIobject = Instantiate(prefabs.OP_Ok);
				options = newUIobject.GetComponent<OP_Ok>();
				break;
			case UIType.OP_YES_NO:
				newUIobject = Instantiate(prefabs.OP_Yes_No);
				options = newUIobject.GetComponent<OP_YesNo>();
				break;
			default:
				return null;
		}

		Vector3 toReceiver = receiver.transform.position - position;
		newUIobject.transform.position = position + toReceiver.normalized * 0.2F;

		Vector3 lookAt = receiver.transform.position;
		lookAt.y = position.y;
		newUIobject.transform.LookAt(lookAt);


		if (sender != null)
		{
			MultiActorUI mauiSettings = newUIobject.GetComponent<MultiActorUI>();
			mauiSettings.disabled = false;
			mauiSettings.Receiver = receiver;
			mauiSettings.Initiator = sender;
			mauiSettings.Initialize();
		}

		options.SetContents(title, message);


		return options;
	}

	/**** Template for calling instantiate ****/
	/*
	 *		// inside AI class
	 *		OP_YES_NO options = UIManager.Instance.Instantiate
	 *		(UIType.OP_YES_NO, "Order", "Assign new order to " + name, transform.position, player.Instance.gameobject, gameobject);
	 *	
	 *		// assign events
	 *		options.SetEvents(ButtonType.Yes, new UnityAction(PlaceOrder()));
	 */

}
