using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Strategy {

    Stack<Tactic> plan;

    float cost, reward;
    public float determination
    {
        get
        {
            return reward - cost;
        }
    }

    public Strategy(Stack<Tactic> _plan, float _cost, float _reward)
    {
        plan = _plan;
        cost = _cost;
        reward = _reward;
    }

}
