using AI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderBehaviorTree
{
    public BehaviorTree tree;

    public WanderBehaviorTree() 
    {
        tree = new BehaviorTree("Wander", priority: 0);
    }
}
