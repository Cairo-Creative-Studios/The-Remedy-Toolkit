using Remedy.Framework;
using Remedy.Schematics.Utils;
//using SaintsField;
using System;
using System.Collections.Generic;
using UnityEngine;

//Enable SaintsEditor and Change Mono to SaintsMonoBehaviour
namespace Remedy.Animation
{
    public class AnimationComunicator : MonoBehaviour
    {
        public Animator Animator => gameObject.GetCachedComponent<Animator>();
        public List<TriggerEvent> Triggers = new();
        public List<BooleanEvent> BooleanValues = new();
        public List<FloatEvent> FloatValues = new();
        public List<Vector2Event> Vector2Values = new();

        private void Awake()
        {
            foreach (var tEvent in Triggers)
            {
                tEvent.OnEvent.Subscribe(this, (Union value) => {
                    if (tEvent.ParameterName == 0) return;

                    Animator.ResetTrigger(tEvent.ParameterName);
                    Animator.SetTrigger(tEvent.ParameterName);
                });
            }
            foreach (var bEvent in BooleanValues)
            {
                bEvent.OnEvent.Subscribe(this, (bool value) => {
                    if (bEvent.ParameterName == 0) return;

                    Animator.SetBool(bEvent.ParameterName, value);
                });
            }

            foreach (var bEvent in FloatValues)
            {
                bEvent.OnEvent.Subscribe(this, (float value) => {
                    if (bEvent.ParameterName == 0) return;

                    Animator.SetFloat(bEvent.ParameterName, value);
                });
            }
            foreach (var v2Event in Vector2Values)
            {
                v2Event.OnEvent.Subscribe(this, (Vector2 value) =>
                {
                    if (v2Event.XParameter != 0) Animator.SetFloat(v2Event.XParameter, value.x);
                    if (v2Event.YParameter != 0) Animator.SetFloat(v2Event.YParameter, value.y);
                    if (v2Event.MagnitudeParameter != 0) Animator.SetFloat(v2Event.MagnitudeParameter, value.magnitude);
                });
            }
        }

        [Serializable]
        public class TriggerEvent
        {
            public SignalData OnEvent;
            //[AnimatorParam(AnimatorControllerParameterType.Trigger)]
            public int ParameterName;
        }
        [Serializable]
        public class BooleanEvent
        {
            public SignalData OnEvent;
            //[AnimatorParam(AnimatorControllerParameterType.Bool)]
            public int ParameterName;
        }
        [Serializable]
        public class FloatEvent
        {
            public SignalData OnEvent;
            //[AnimatorParam(AnimatorControllerParameterType.Float)]
            public int ParameterName;
        }
        [Serializable]
        public class Vector2Event
        {
            public SignalData OnEvent;
            //[AnimatorParam(AnimatorControllerParameterType.Float)]
            [Tooltip("The Parameter that is set to the X axis value of the Vector2 passed from the Event")]
            public int XParameter;
            //[AnimatorParam(AnimatorControllerParameterType.Float)]
            [Tooltip("The Parameter that is set to the Y axis value of the Vector2 passed from the Event")]
            public int YParameter;
            //[AnimatorParam(AnimatorControllerParameterType.Float)]
            [Tooltip("The Parameter that is set to the Magnitude value of the Vector2 passed from the Event")]
            public int MagnitudeParameter;
        }
    }
}
