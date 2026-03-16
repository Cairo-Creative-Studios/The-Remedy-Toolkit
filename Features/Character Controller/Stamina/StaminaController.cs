/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;

namespace Remedy.CharacterControllers.Stamina
{
    [SchematicComponent("Characters/Movement/Stamina Controller")]
    //[Searchable]
    public class StaminaController : MonoBehaviour
    {
        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Input")]*/
        [Tooltip("Called from other scripts to use Stamina")]
        public SignalData UseStamina;
        [Tooltip("Boolean Events that determine whether Stamina can regerate or not.")]
        public SignalData RegenerateStamina;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Output")]*/
        [Tooltip("Passes the amount that was consumed")]
        public SignalData OnStaminaConsumed;
        [Tooltip("Passes the current amount of stamina")]
        public SignalData OnStaminaChanged;
        [Tooltip("Triggered when the Controller runs out of Stamina")]
        public SignalData OnStaminaDrained;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Properties")]
        [Dropdown("GetProperties")]*/
        [SchematicProperties]
        public StaminaProperties Properties;

        public bool Regenerating = false;
/*
        public DropdownList<StaminaProperties> GetProperties()
        {
            var list = new DropdownList<StaminaProperties>();
            var scriptableObjects = Resources.LoadAll<StaminaProperties>("");

            foreach (var so in scriptableObjects)
            {
                list.Add(so.name, so);
            }

            return list;
        }*/

        public float Stamina = 100f;

        void OnEnable()
        {
            UseStamina?.Subscribe(this, (float value) => ConsumeStamina(value));
            RegenerateStamina?.Subscribe(this, (bool value) => Regenerating = value);

            Stamina = Properties.MaxStamina;
        }

        private void OnDisable()
        {
            UseStamina?.Unsubscribe(this);
            RegenerateStamina?.Unsubscribe(this);
        }

        private void Update()
        {
            if (Regenerating)
            {
                if (Properties.MaxStamina > Stamina)
                    Stamina += Properties.StaminaReginAmount * Time.deltaTime;
                else
                    Stamina = Properties.MaxStamina;
            }
            OnStaminaChanged?.Invoke(Stamina);
        }

        /// <summary>
        /// Attempts to consume some Stamina, and returns whether it was successful or not.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool ConsumeStamina(float amount)
        {
            if (Stamina < amount) return false;
            Stamina -= amount;
            OnStaminaConsumed?.Invoke(amount);
            return true;
        }
    }
}