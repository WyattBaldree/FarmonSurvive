using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.States
{

    public abstract class StateMachineState
    {
        public StateMachine _stateMachine;

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void Tick() { }
    }
}