using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.States
{
    public class StateMachine
    {
        StateMachineState _currentState;
        StateMachineState _prevState;

        public Dictionary<string, StateMachineState> stateDictionary = new Dictionary<string, StateMachineState>();

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
            nextState.Enter();
        }
        public void Tick()
        {
            CurrentState.Tick();
        }

        public void AddState( string stateName, StateMachineState newState )
        {
            newState._stateMachine = this;

            stateDictionary.Add(stateName, newState);
        }
    }
}