﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetLightningSpell : Spell {

    [SerializeField]
    protected float attackRadius;

    protected float nearestDistance = 0;

    protected GameObject monster;

    protected GameObject nearestMob;

    public override void InitializeSpell(SpellHandler handler)
    {
        base.InitializeSpell(handler);

        if(Player.Instance.StatContainer.GetStat(Stats.StatsType.MANA).Current >= manaCost)
        {
            nearestMob = CheckForMonsterDistance();

            if (nearestMob != null)
            {
                transform.position = new Vector3(nearestMob.transform.position.x, nearestMob.transform.position.y + 2, nearestMob.transform.position.z);
                transform.parent = nearestMob.transform;

                this.gameObject.SetActive(true);

                handler.Castor.StatContainer.ReduceMana(manaCost);
			}
			else
			{
				handler.DecastSpell();
			}

            handler.DecastSpell();
        }
        else
        {
            handler.DecastSpell();
            Destroy(this.gameObject, 1);
        }
        
    }

    protected GameObject CheckForMonsterDistance()
    {
		
		Vector3 target = Player.Instance.transform.position + Player.Instance.transform.forward * 10;

		Collider[] hitColliders = Physics.OverlapSphere(Player.Instance.transform.position, 10);

        foreach (Collider c in hitColliders)
        {
            Monster mob = c.GetComponent<Monster>();

			if (mob)
			{
				float distance = Vector3.Distance(Player.Instance.transform.position, mob.gameObject.transform.position);

				if (distance <= nearestDistance || nearestDistance == 0)
				{
					monster = mob.gameObject;
					nearestDistance = distance;
				}
			}
        }

        return monster;
    }

}
