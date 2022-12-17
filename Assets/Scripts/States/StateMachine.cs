using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.States
{
    public class StateMachine
    {
        StateMachineState _currentState;
        StateMachineState _prevState;

        public StateMachineState CurrentState
        {
            get => _currentState;
        }

        public StateMachineState PrevState
        {
            get => _prevState;
        }

        public void InitializeStateMachine(StateMachineState startingState)
        {
            _prevState = startingState;
            _currentState = startingState;
            startingState.Enter();
        }

        public void ChangeState(StateMachineState nextState)
        {
            _prevState = _currentState;

            _currentState.Exit();
            _currentState = nextState;

            nextState._stateMachine = this;
            nextState.Enter();
        }
        public void Tick()
        {
            CurrentState.Tick();
        }
    }
}