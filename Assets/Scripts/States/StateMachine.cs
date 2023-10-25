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

            //Set the _currentState to the new state before calling exit on it in case the exit method
            //changes the state again which would lead to the _currentState value being the previous state.
            StateMachineState oldState = _currentState;
            _currentState = nextState;
            oldState.Exit();

            nextState._stateMachine = this;
            nextState.Enter();
        }
        public void Tick()
        {
            CurrentState.Tick();
        }
    }
}