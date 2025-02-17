﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FriendlyAIFSM : ActorFSM
{

    protected List<Shop> visitedShop = new List<Shop>();

    protected Shop targetShop;

    protected FriendlyAI currentFriendlyAI;

    public override void ChangeState(FSMState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case FSMState.SHOPPING:
                visitedShop.Add(targetShop);
                break;
        }
    }
    public override AI CurrentAI
    {
        get
        {
            return currentFriendlyAI;
        }
    }

    protected override void Awake()
    {
        currentFriendlyAI = GetComponent<FriendlyAI>();
        base.Awake();
    }
    protected virtual void UpdateShoppingState()
    {
        
        if (targetShop.Owner == null)
        {
            ChangeState(FSMState.IDLE);
            return;
        }
        float val = Random.value;
        Node shopPoint = targetShop.GetRandomPoint(transform.position);
        if ((val > .5 || targetShop.InteractionNode.Occupied) && shopPoint != null)
        {

            ChangePath(shopPoint);
            nextState = FSMState.SHOPPING;
            ChangeState(FSMState.PETROL);

        }
        else
        {
            
            if ((!(targetShop.Owner is Player) || ((targetShop.Owner is Player) && currentFriendlyAI.IsInteractionAvailable())) && !targetShop.InteractionNode.Occupied)
            {
                ChangePath(targetShop.InteractionNode);
                ChangeState(FSMState.PETROL);
                nextState = FSMState.IDLE;
            }
            else
            {
                ChangeState(FSMState.IDLE);
            }
        }
    }

    protected override void UpdateIdleState()
    {
       
        if (pathFound)
        {
            ChangeState(FSMState.PETROL);
        }
        else if (!requestedPath)
        {
            Shop _targetShop = TownManager.Instance.GetRandomShop(visitedShop);
            
            if (_targetShop != null)//And check for anymore shop
            {
                targetShop = _targetShop;
                AStarManager.Instance.RequestPath(transform.position, _targetShop.Location.Position, ChangePath);
                
                requestedPath = true;
                nextState = FSMState.SHOPPING;

            }
            else
            {
                Node endPoint = TownManager.Instance.GetRandomSpawnPoint();
                AStarManager.Instance.RequestPath(transform.position, endPoint.Position, ChangePath);
                requestedPath = true;
            }
        }
    }

    public void StartInteractRoutine(Actor actor)
    {
        StartCoroutine(Interact(actor));
    }

    public override void NewSpawn()
    {
        base.NewSpawn();
        visitedShop.Clear();
    }

    protected override void UpdateFSMState()
    {
        base.UpdateFSMState();

        switch (currentState)
        {
            case FSMState.SHOPPING:
                UpdateShoppingState();
                return;
        }

    }

   
    public IEnumerator Interact(Actor actor)
    {
        target = actor;
        isHandlingAction = true;
        float waitTimer = 5;

        while (isHandlingAction)
        {

            Vector3 endPos = target.transform.position;
            endPos.y = transform.position.y;
            Vector3 dir = (target.transform.position - transform.position).normalized;
            dir.y = 0;

            float angle = Mathf.Abs(Vector3.Angle(transform.forward, dir));

            if (angle < 30)
            {

                Player player = target as Player;


                if (currentFriendlyAI.IsInteractionAvailable() || currentFriendlyAI.Interacting)
                {
                    if (player.CheckConversingWith(currentFriendlyAI))
                    {
                        waitTimer = 5;

                        if (!currentFriendlyAI.Interacting)
                        {
                            currentFriendlyAI.StartInteraction();
                        }
                    }
                    else
                    {
                        waitTimer -= Time.deltaTime;
                        if (waitTimer <= 0)
                        {
                            currentFriendlyAI.StopAllInteractions();
                            break;
                        }

                    }
                }
                else
                {
                    currentFriendlyAI.StopAllInteractions();
                    break;
                }
            }
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), Time.deltaTime * 5);


            yield return new WaitForEndOfFrame();

        }

        isHandlingAction = false;
    }
}
