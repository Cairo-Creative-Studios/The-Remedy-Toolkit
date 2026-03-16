using Remedy.Framework;
//using SaintsField;
using UnityEngine;
using Remedy.Common;

namespace Remedy.LevelDesign
{
    /// <summary>
    /// Trigger Volumes are a base for any Volumes that would invoke Events related to the player's interactions with it. 
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class TriggerVolume : MonoBehaviour
    {
        public SignalData OnTrigger;
        public bool showCollider = true;

        //[Dropdown("GetTriggerVolumes")]
        public int TriggerType;
        public LevelDesignData.TriggerInfo TriggerInfo;

        private BoxCollider _boxCollider;
/*
        public DropdownList<int> GetTriggerVolumes()
        {
            var list = new DropdownList<int>();

            for(int i = 0; i < LevelDesignData.Instance.Triggers.TriggerTypes.Length; i++)
            {
                list.Add(LevelDesignData.Instance.Triggers.TriggerTypes[i].Name, i);
            }

            return list;
        }*/

        private void OnValidate()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(GlobalSubsystem.PlayerTag))
            {
                OnTrigger?.Invoke();
            }
        }


        void OnDrawGizmos()
        {
            TriggerInfo = LevelDesignData.Instance.Triggers;
            if (showCollider)
            {
                if(_boxCollider == null)
                    _boxCollider = GetComponent<BoxCollider>();

                if (LevelDesignData.Instance.Triggers.TriggerTypes.Length == 0) return;

                Gizmos.color = LevelDesignData.Instance.Triggers.TriggerTypes[TriggerType].Color;

                if (LevelDesignData.Instance.Triggers.TriggerTypes[TriggerType].Filled)
                {
                    Gizmos.DrawCube(transform.position + _boxCollider.center, _boxCollider.size);
                    Gizmos.DrawWireCube(transform.position + _boxCollider.center, _boxCollider.size);
                }
                else
                    Gizmos.DrawWireCube(transform.position + _boxCollider.center, _boxCollider.size);
            }
        }
    }
}
