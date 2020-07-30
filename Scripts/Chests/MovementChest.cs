using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementChest : Chest
{
    protected override void OnStart()
    {
        _chestAnimator.Play("SpawnMovementChest");
    }

    protected override void OnSelect()
    {
        _chestAnimator.Play("SelectMovementChest");
    }
    
    protected override void OnUnselect()
    {
        _chestAnimator.Play("UnselectMovementChest");
    }

    protected override void OnOpen()
    {
        _chestAnimator.Play("OpenMovementChest");
    }
}
