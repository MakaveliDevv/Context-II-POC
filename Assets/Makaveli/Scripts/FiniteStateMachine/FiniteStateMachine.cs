using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Makaveli.Scripts.FiniteStateMachine
{
    /// <summary>
    /// Generic state interface that all concrete states must implement
    /// </summary>
    /// <typeparam name="T">The entity type this state operates on</typeparam>
    public interface IState<T>
    {
        void Enter(T entity);
        void Execute(T entity);
        void Exit(T entity);
        bool CanTransitionTo(Type stateType);
    }

    /// <summary>
    /// Abstract base state that implements common functionality
    /// </summary>
    /// <typeparam name="T">The entity type this state operates on</typeparam>
    public abstract class BaseState<T> : IState<T>
    {
        protected List<Type> allowedTransitions = new ();
        
        public virtual void Enter(T entity) { }
        public abstract void Execute(T entity);
        public virtual void Exit(T entity) { }
        
        public bool CanTransitionTo(Type stateType)
        {
            return allowedTransitions.Contains(stateType);
        }
        
        protected void AddAllowedTransition<TState>() where TState : IState<T>
        {
            allowedTransitions.Add(typeof(TState));
        }
    }

    /// <summary>
    /// The state machine that manages states and transitions
    /// </summary>
    /// <typeparam name="T">The entity type the state machine operates on</typeparam>
    public class StateMachine<T>
    {
        private readonly T owner;
        private readonly Dictionary<Type, IState<T>> states = new();
        private IState<T> currentState;
        private IState<T> previousState;
        private IState<T> globalState;

        public StateMachine(T owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Register a state with the state machine
        /// </summary>
        public void RegisterState(IState<T> state)
        {
            states[state.GetType()] = state;
        }

        /// <summary>
        /// Set the initial state of the state machine
        /// </summary>
        public void SetInitialState<TState>() where TState : IState<T>
        {
            Type stateType = typeof(TState);
            if (states.TryGetValue(stateType, out IState<T> state))
            {
                currentState = state;
                currentState.Enter(owner);
            }
            else
            {
                Debug.LogError($"State {stateType.Name} not registered with the state machine");
            }
        }

        /// <summary>
        /// Set a global state that executes every update regardless of the current state
        /// </summary>
        public void SetGlobalState<TState>() where TState : IState<T>
        {
            Type stateType = typeof(TState);
            if (states.TryGetValue(stateType, out IState<T> state))
            {
                globalState = state;
                globalState.Enter(owner);
            }
            else
            {
                Debug.LogError($"Global state {stateType.Name} not registered with the state machine");
            }
        }

        /// <summary>
        /// Transition to a new state if the transition is valid
        /// </summary>
        public bool ChangeState<TState>() where TState : IState<T>
        {
            Type newStateType = typeof(TState);
            
            // Check if the state exists
            if (!states.TryGetValue(newStateType, out IState<T> newState))
            {
                Debug.LogError($"Cannot transition to state {newStateType.Name} - state not registered");
                return false;
            }
            
            // Check if the current state allows this transition
            if (currentState != null && !currentState.CanTransitionTo(newStateType))
            {
                Debug.LogWarning($"State transition from {currentState.GetType().Name} to {newStateType.Name} not allowed");
                return false;
            }
            
            // Valid transition, perform the change
            if (currentState != null)
            {
                currentState.Exit(owner);
            }
            
            previousState = currentState;
            currentState = newState;
            currentState.Enter(owner);
            
            return true;
        }

        /// <summary>
        /// Revert to the previous state
        /// </summary>
        public bool RevertToPreviousState()
        {
            if (previousState != null)
            {
                return ChangeState(previousState.GetType());
            }
            return false;
        }

        /// <summary>
        /// Generic change state method that takes a Type parameter
        /// </summary>
        public bool ChangeState(Type newStateType)
        {
            if (!typeof(IState<T>).IsAssignableFrom(newStateType))
            {
                Debug.LogError($"Type {newStateType.Name} does not implement IState<{typeof(T).Name}>");
                return false;
            }
            
            // Check if the state exists
            if (!states.TryGetValue(newStateType, out IState<T> newState))
            {
                Debug.LogError($"Cannot transition to state {newStateType.Name} - state not registered");
                return false;
            }
            
            // Check if the current state allows this transition
            if (currentState != null && !currentState.CanTransitionTo(newStateType))
            {
                Debug.LogWarning($"State transition from {currentState.GetType().Name} to {newStateType.Name} not allowed");
                return false;
            }
            
            // Valid transition, perform the change
            if (currentState != null)
            {
                currentState.Exit(owner);
            }
            
            previousState = currentState;
            currentState = newState;
            currentState.Enter(owner);
            
            return true;
        }

        /// <summary>
        /// Update the state machine - call this from your MonoBehaviour's Update method
        /// </summary>
        public void Update()
        {
            // Execute global state if present
            globalState?.Execute(owner);
            
            // Execute current state if present
            currentState?.Execute(owner);
        }

        /// <summary>
        /// Check if the state machine is in a specific state
        /// </summary>
        public bool IsInState<TState>() where TState : IState<T>
        {
            return currentState?.GetType() == typeof(TState);
        }

        /// <summary>
        /// Get the current state type
        /// </summary>
        public Type CurrentStateType => currentState?.GetType();

        /// <summary>
        /// Get the current state name
        /// </summary>
        public string CurrentStateName => currentState?.GetType().Name;
    }

    /// <summary>
    /// State Machine MonoBehaviour that can be attached to GameObjects
    /// </summary>
    public abstract class StateMachineBehaviour : MonoBehaviour
    {
        protected StateMachine<StateMachineBehaviour> stateMachine;

        protected virtual void Awake()
        {
            stateMachine = new StateMachine<StateMachineBehaviour>(this);
            ConfigureStateMachine();
        }

        protected virtual void Update()
        {
            stateMachine.Update();
        }

        /// <summary>
        /// Override this method to register states and configure initial state
        /// </summary>
        protected abstract void ConfigureStateMachine();
    }
}