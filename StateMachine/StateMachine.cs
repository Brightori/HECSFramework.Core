using System;
using System.Collections.Generic;
using HECSFramework.Core.Helpers;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, Doc.FSM, "Simple realisation for fsm pattern")]
    public partial class StateMachine : IUpdatable, IDisposable
    {
        private Dictionary<int, BaseFSMState> states = new Dictionary<int, BaseFSMState>(8);
        private Dictionary<int, HECSList<ITransition>> transitions = new(2);

        private Queue<int> changeState = new Queue<int>(1);

        private Entity owner;

        private int currentState;
        private int previousState;

        public StateMachine(Entity owner)
        {
            this.owner = owner;
            this.owner.World.RegisterUpdatable(this, true);
        }

        public int CurrentState { get => currentState; }
        public int PreviousState { get => previousState; }

        public void UpdateLocal()
        {
            if (changeState.TryDequeue(out var state))
            {
                ProcessChangeState(state);
            }

            if (CurrentState != 0)
            {
                states[CurrentState].Update(owner);
            }
        }

        public void ChangeState(int toState)
        {
            if (toState == 0)
            {
                previousState = currentState;
                currentState = 0;
            }
            changeState.Enqueue(toState);
        }

        private void ProcessChangeState(int toState)
        {
            if (currentState != 0)
            {
                states[currentState].Exit(owner);
            }

            if (TryGetState(toState, out var state))
            {
                previousState = currentState;
                currentState = state.StateID;
                state.Enter(owner);
            }
        }

        public void NextState()
        {
            if (currentState == 0)
                return;

            //we look into transitions, if we have special one we try to proceed by transition
            if (this.transitions.TryGetValue(currentState, out var transitionsOfState))
            {
                foreach (var t in transitionsOfState)
                {
                    if (t.IsReady())
                    {
                        ChangeState(t.ToState);
                        return;
                    }
                }
            }
         
            //if we dont have valid transition we proceed by default scenario of current state
            ChangeState(states[currentState].NextStateID);
        }

        public void AddState(BaseFSMState state)
        {
            states.AddOrReplace(state.StateID, state);
        }

        public bool RemoveState(BaseFSMState state)
        {
            return states.Remove(state.StateID);
        }

        public bool TryGetState(int id, out BaseFSMState state)
        {
            return states.TryGetValue(id, out state);
        }

        public void Dispose()
        {
            owner.World.GlobalUpdateSystem.Register(this, false);
        }
    }

    [Documentation(Doc.FSM, Doc.HECS, "its parent class for hecs fsm realisation")]
    public abstract class BaseFSMState : IDisposable
    {
        public abstract int StateID { get; }
        public abstract int NextStateID { get; }

        protected StateMachine stateMachine;

        public abstract void Enter(Entity entity);
        public abstract void Exit(Entity entity);
        public abstract void Update(Entity entity);
        
        protected void EndState()
        {
            stateMachine.NextState();
        }

        protected bool IsCurrentState()
        {
            return stateMachine.CurrentState == StateID;
        }

        public virtual void Dispose()
        {
        }

        public BaseFSMState(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
    }

    public interface ITransition
    {
        int ToState { get; }
        bool IsReady();
    }
}
