using UnityEngine;
using System.Collections;
using BehaviourMachine;


public class TestingCustomVariables : MonoBehaviour
{
    [System.Serializable]
    [CustomVariable("transformVariable")]
    public class TransformVariable : Vector3Var
    {
        [BehaviourMachine.Tooltip("The target Transform")]
        public Transform transform;

        public override Vector3 Value
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public TransformVariable() : base() { }

        public TransformVariable(GameObject self) : base()
        {
            this.transform = self.transform;
        }
    }


    [System.Serializable]
    [CustomVariable("myVariable")]
    public class MyVariable : Vector3Var
    {
        [BehaviourMachine.Tooltip("My Variable")]
        public Vector3 thisVariable;

        public override Vector3 Value
        {
            get { return thisVariable; }
            set { thisVariable = value; }
        }

        public MyVariable() : base() { }

        public MyVariable(GameObject self) : base()
        {
            thisVariable = Vector3.up;
        }
    }


    Vector3 patata = Vector3.up;
    MyVariable myVariable;

    void Start()
    {
        myVariable = new MyVariable(gameObject);
        myVariable.Value = patata;
        myVariable.genericValue = patata;
    }
}