using UnityEngine;
using System.Collections;

public class Tactic{

    public readonly float cost;
    float reward;

    public float determination
    {
        get
        {
            return reward - cost;
        }
    }

    public readonly GameObject targetElement;
    Vector3 targetPosition;
    bool requiresLeader;
    int cantMinions;

    // CONSTRUCTORS
    /*public Tactic(float _cost, float _reward, GameObject _targetElement, Vector3 _targetPosition, bool _requiresLeader, int _cantMinions)
    {
        cost = _cost;
        reward = _reward;

        targetElement = _targetElement;
        targetPosition = _targetPosition;
        requiresLeader = _requiresLeader;
        cantMinions = _cantMinions;
    }*/

    public Tactic(float _cost, float _reward, GameObject _targetElement, bool _requiresLeader, int _cantMinions)
    {
        cost = _cost;
        reward = _reward;

        targetElement = _targetElement;
        targetPosition = _targetElement.transform.position; // discretizar según tipo de targetElement -> no siempre queremos el punto exacto
        requiresLeader = _requiresLeader;
        cantMinions = _cantMinions;
    }
    //-------------------------------------------------------------------------------
}
