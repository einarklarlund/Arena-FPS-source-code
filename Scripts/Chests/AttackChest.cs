using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackChest : Chest
{
    protected override void OnStart()
    {
        _chestAnimator.Play("SpawnAttackChest");
    }

    protected override void OnSelect()
    {
        _chestAnimator.Play("SelectAttackChest");
    }
    
    protected override void OnUnselect()
    {
        _chestAnimator.Play("UnselectAttackChest");
    }

    protected override void OnOpen()
    {
        _chestAnimator.Play("OpenAttackChest");
    }
}
