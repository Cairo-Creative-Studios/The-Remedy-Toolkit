using Remedy.Framework;
//using SaintsField;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//Enable SaintsEditor and Change Mono to SaintsMonoBehaviour
namespace Remedy.Materials
{
    public class MaterialComunicator : MonoBehaviour
    {
        public Material Material;
        public List<FloatEvent> FloatValues = new();
        public List<Vector2Event> Vector2Values = new();

        private void Awake()
        {
            foreach (var bEvent in FloatValues)
            {
                bEvent.OnEvent.Subscribe(this, (float value) => {
                    if (bEvent.ParameterName == "") return;

                    Material.SetFloat(bEvent.ParameterName, value);
                });
            }
            foreach (var v2Event in Vector2Values)
            {
                v2Event.OnEvent.Subscribe(this, (Vector2 value) =>
                {
                    if (v2Event.XParameter != "") Material.SetFloat(v2Event.XParameter, value.x);
                    if (v2Event.YParameter != "") Material.SetFloat(v2Event.YParameter, value.y);
                    if (v2Event.MagnitudeParameter != "") Material.SetFloat(v2Event.MagnitudeParameter, value.magnitude);
                });
            }
        }

        private void OnValidate()
        {
            if (Material == null)
            {
                Material = GetComponent<Renderer>().sharedMaterial;
            }
        }

        [Serializable]
        public class FloatEvent
        {
            public SignalData OnEvent;
            //[ShaderParam(ShaderPropertyType.Float)]
            public string ParameterName;
        }
        [Serializable]
        public class Vector2Event
        {
            public SignalData OnEvent;
            //[ShaderParam(ShaderPropertyType.Float)]
            [Tooltip("The Parameter that is set to the X axis value of the Vector2 passed from the Event")]
            public string XParameter;
            //[ShaderParam(ShaderPropertyType.Float)]
            [Tooltip("The Parameter that is set to the Y axis value of the Vector2 passed from the Event")]
            public string YParameter;
            //[ShaderParam(ShaderPropertyType.Float)]
            [Tooltip("The Parameter that is set to the Magnitude value of the Vector2 passed from the Event")]
            public string MagnitudeParameter;
            //[ShaderParam(ShaderPropertyType.Vector)]
            [Tooltip("The Parameter that is set to the Magnitude value of the Vector2 passed from the Event")]
            public string Value;
        }
    }
}
