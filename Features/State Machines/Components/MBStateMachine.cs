//using SaintsField;
using System;
using UnityEngine;
//using SaintsField.Playa;
using System.Collections.Generic;

namespace Remedy.StateMachines
{
    [SchematicComponent("Logical/State Machine")]
    //[Searchable]
    public class MBStateMachine : MonoBehaviour
    {
        [Tooltip("An optional 'Pause' Input that disables all states while True. If any of the Boolean Events in this list are true, all State Machine managed Behaviours are disabled.")]
        public SignalData Pause;

        [IdentityListRenderer(identifierType: ListIdentifierType.Name, identifierField: "Name", depth: 0, foldoutTitle: "States", itemLabel: "State")]
        [SerializeField]
        public MBState[] States = new MBState[0];
        [SerializeField]
        private string _currentState = "";
        public string CurrentState => _currentState;
//        private DropdownList<string> _cachedStates = new();
        private List<string> _cachedMonoBehaviours;
        /// <summary>
        /// A public readonly accessor to the _cachedMonoBehaviours Dropdown List
        /// </summary>
        public List<string> CachedMonoBehaviours
        {
            get
            {
                if(_cachedMonoBehaviours == null || _cachedMonoBehaviours.Count == 0)
                {
                    _cachedMonoBehaviours = new();
                    _cachedMonoBehaviours.Add("None");

                    foreach (var MB in gameObject.GetComponents<MonoBehaviour>())
                    {
                        _cachedMonoBehaviours.Add(MB.GetType().Name/*, MB.GetType().AssemblyQualifiedName*/);
                    }
                }
                return _cachedMonoBehaviours;
            }
        }
        private int _currentDelay = 0;
        private MonoBehaviour[] _managedMonobehaviours;
        private bool _isPaused = false;


        public void OnEnable()
        {
            // References Updates
            List<MonoBehaviour> mbs = new();

            foreach (var state in States)
            {
                state.StateMachine = this;

                foreach (var handle in state.Enabled)
                {
                    if (!mbs.Contains(handle.CachedComponent))
                    {
                        mbs.Add(handle.CachedComponent);
                    }

                    handle.StateMachine = this;
                }
                foreach (var handle in state.Disabled)
                {
                    handle.StateMachine = this;
                }
            }

            _managedMonobehaviours = mbs.ToArray();

            // State Subscriptions
            foreach(var state in States)
            {
                state.Transition?.Subscribe(this, (object val) => {
                    SetState(state.Name);
                });
            }

            // Subscribe all to Pause Input
            Pause.Subscribe(this, (bool val) =>
            {
                _isPaused = false;

                if (val) _isPaused = true;
                else
                {
                    if (Pause.Parameters[0].GetValue<bool>())
                    {
                        _isPaused = true;
                    }
                }

                if(_isPaused)
                {
                    foreach (var mb in _managedMonobehaviours)
                    {
                        mb.enabled = false;
                    }
                }
            });

            // Cache them monos, don't be slow
            _cachedMonoBehaviours = new();
            _cachedMonoBehaviours.Add("None");

            foreach (var MB in gameObject.GetComponents<MonoBehaviour>())
            {
                _cachedMonoBehaviours.Add(MB.GetType().Name);
            }

            foreach(var state in States)
            {
                if(state.Name == CurrentState)
                {
                    SetState(CurrentState);
                    break;
                }
            }
        }

        private void OnDisable()
        {
            foreach (var state in States)
            {
                state.Transition.Unsubscribe(this);
            }
        } 
          
        private void OnValidate()
        {
            foreach (var state in States)
            {
                state.StateMachine = this;

                foreach (var handle in state.Enabled)
                {
                    handle.StateMachine = this;
                }
                foreach (var handle in state.Disabled)
                {
                    handle.StateMachine = this;
                }
            }

            _cachedMonoBehaviours = new()
            {
                "None"
            };

            foreach (var MB in gameObject.GetComponents<MonoBehaviour>())
            {
                if (MB == null || _cachedMonoBehaviours == null) continue;
                _cachedMonoBehaviours.Add(MB.GetType().Name);
            }
        }

        private void Update()
        {
            _currentDelay -= _currentDelay > 0 ? 1 : 0;

            // Sometimes state transitions fail into no states, where all MBs are disabled. This corrects that.
            bool isInState = false;
            foreach(var mb in _managedMonobehaviours)
            {
                if (mb == null) 
                    continue;

                if (mb.enabled)
                    isInState = true;
            }
            if (!isInState && !_isPaused)
                SetState(CurrentState);
        }

        public void SetState(string stateName)
        {
            foreach(var state in States)
            {
                if(state.Name == stateName)
                {
                    foreach (var currentHandle in state.Enabled)
                    {
                        if (currentHandle.CachedComponent != null)
                            currentHandle.CachedComponent.enabled = true;

                        state.ActiveState?.Invoke(true);
                    }
                    foreach (var currentHandle in state.Disabled)
                    {
                        if (currentHandle.CachedComponent != null)
                            currentHandle.CachedComponent.enabled = false;
                    }

                    _currentState = stateName;
                }
                else
                    state.ActiveState?.Invoke(false);
            }
        }

        public List<string> AvailableStates
        {
            get
            {
                List<string> list = new();

                list.Add("Any");

                if (States.Length > 0)
                {
                    foreach (var state in States)
                    {
                        list.Add(state.Name);
                    }
                }
                else
                {
                    list.Add("No States");
                }

                return list;
            }
        }

        [Serializable]
        public class MBState
        {
            public string Name;
            public MBStateMachine StateMachine;
            public SignalData Transition;
            public SignalData ActiveState;

            [IdentityListRenderer(identifierType: ListIdentifierType.Dropdown,
                                  identifierField: "MonoBehaviour",
                                  depth: 1,
                                  options: "./" + nameof(CachedMonoBehaviours),
                                  foldoutTitle: "MonoBehaviours to Enable",
                                  itemLabel: "MonoBehaviour")]
            public MBStateHandle[] Enabled = new MBStateHandle[0];

            [IdentityListRenderer(identifierType: ListIdentifierType.Dropdown,
                                  identifierField: "MonoBehaviour",
                                  depth: 1,
                                  options: "./" + nameof(CachedMonoBehaviours),
                                  foldoutTitle: "MonoBehaviours to Disable",
                                  itemLabel: "MonoBehaviour")]
            public MBStateHandle[] Disabled = new MBStateHandle[0];
        }

        [Serializable]
        public class MBStateHandle
        {
            [NonSerialized]
            public MBStateMachine StateMachine;
            //[Dropdown("GetMonobehaviours")]
            public string MonoBehaviour = "MonoBehaviour";

            private MonoBehaviour _cachedComponent;
            public MonoBehaviour CachedComponent
            {
                get
                {
                    return _cachedComponent ??= (MonoBehaviour)StateMachine.GetComponent(Type.GetType(MonoBehaviour));
                }
            }

            public MBStateHandle() { }

            public List<string> AvailableMonoBehaviours
            {
                get
                {
                    return StateMachine.CachedMonoBehaviours;
                }
            }
        }
    }
}
