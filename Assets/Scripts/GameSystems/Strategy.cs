using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Strategy {

    public Stack<Tactic> plan;

    public float cost, reward;
    public float determination
    {
        get
        {
            return Mathf.Pow(reward - cost, 2f);
        }
    }

    public Strategy(Stack<Tactic> _plan)
    {
        plan = _plan;

        cost = 0f;
        reward = 0f;

        foreach (Tactic tc in plan)
        {
            cost += tc.cost;
            reward += tc.reward;
        }
        //cost = _cost;
        //reward = _reward;
    }

}
