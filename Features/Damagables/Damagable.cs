using System.Collections;
using UnityEngine;
using UnityEngine.Events;
//using SaintsField;
//using SaintsField.Playa;
using Cysharp.Threading.Tasks;
using Remedy.Common;

namespace Remedy.Damagables
{
    [SchematicComponent("Damageables/Damageable")]
    //[Searchable]
    public class Damageable : MonoBehaviour
    {

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Output")]*/
        public SignalData OnDamageBegin;
        public SignalData OnHealBegin;
        public SignalData OnHealInstigation;
        public SignalData OnDamageContinued;
        public SignalData OnHealContinued;
        public SignalData OnDamageFinish;
        public SignalData OnHealFinish;
        [Tooltip("Not handled by the Damageable. This functionality must be uniquely implemented for each object that uses this Component. The given value is the direction from the instigation position to this object.")]
        public UnityEvent<Vector3> OnKnockback;
        public SignalData OnDeath;
        public SignalData OnFullHealth;
        public SignalData OnRevive;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Properties")]
        [Dropdown("GetDamageLayer")]*/
        public int DamageLayer;
        //public DropdownList<int> GetDamageLayer => DamageData.GetDamageLayers();

        [SerializeField]
        private float _health = 0;
        public float Health => _health;

        [SerializeField]
        private float _maxHealth;
        public float MaxHealth => _maxHealth;

        [SerializeField]
        private bool _isAlive = true;
        public bool IsAlive => _isAlive;

        /// <summary>
        /// Damage this object
        /// </summary>
        /// <param name="instigator"></param>
        public virtual void Damage(DamageInstigation instigation)
        {
            OnDamageBegin?.FinalInvoke(default);
            OnKnockback?.Invoke(transform.position - instigation.Position);

            if (!DamageInstigationData.Lookup.TryGetValue(instigation.DataID, out var data))
            {
                Debug.LogWarning($"DamageInstigationData with ID {instigation.DataID} not found!");
                return;
            }

            if (data.Duration > 0)
            {
                ApplyDamageOverTime(data.Duration, data.Amount).Forget();
            }
            else
            {
                _health -= data.Amount;
                OnDamageFinish?.FinalInvoke(default);

                if (_health <= 0f && _isAlive)
                {
                    _isAlive = false;
                    OnDeath?.FinalInvoke(default);
                }
            }
        }

        /// <summary>
        /// Heal this object
        /// </summary>
        /// <param name="instigator"></param>
        public virtual void Heal(DamageInstigation instigation)
        {
            OnHealBegin?.FinalInvoke(default);

            if (!DamageInstigationData.Lookup.TryGetValue(instigation.DataID, out var data))
            {
                Debug.LogWarning($"DamageInstigationData with ID {instigation.DataID} not found!");
                return;
            }

            if (data.Duration > 0)
            {
                ApplyHealingOverTime(data.Duration, data.Amount).Forget();
            }
            else
            {
                _health = Mathf.Min(_health + data.Amount, _maxHealth);
                OnHealInstigation?.FinalInvoke(default);

                if (_health == MaxHealth)
                    OnFullHealth?.FinalInvoke(default);
            }
        }

        /// <summary>
        /// A coroutine that applies damage over time
        /// </summary>
        /// <param name="instigator"></param>
        /// <returns></returns>
        private async UniTaskVoid ApplyDamageOverTime(float time, float amount)
        {
            int steps = Mathf.CeilToInt(time / Time.fixedDeltaTime);
            float damagePerStep = amount / steps;

            for (int i = 0; i < steps; i++)
            {
                _health -= damagePerStep;
                OnDamageContinued?.FinalInvoke(default);

                if (_health <= 0f && _isAlive)
                {
                    _health = 0f;
                    _isAlive = false;
                    OnDamageFinish?.FinalInvoke(default);
                    OnDeath?.FinalInvoke(default);
                    break;
                }

                await UniTask.WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// A coroutine that applies damage over time
        /// </summary>
        /// <param name="instigator"></param>
        /// <returns></returns>
        private async UniTaskVoid ApplyHealingOverTime(float time, float amount)
        {
            int steps = Mathf.CeilToInt(time / Time.fixedDeltaTime);
            float damagePerStep = amount / steps;

            for (int i = 0; i < steps; i++)
            {
                _health += damagePerStep;

                if (_health == _maxHealth)
                {
                    OnHealFinish?.FinalInvoke(default);
                    OnFullHealth?.FinalInvoke(default);
                }

                OnHealContinued?.FinalInvoke(default);
                await UniTask.WaitForFixedUpdate();
            }

            OnHealFinish?.FinalInvoke(default);
        }

        /// <summary>
        /// Instantly Kills the Damagable
        /// </summary>
        /// <param name="instigator"></param>
        public void Kill(DamageInstigation instigation)
        {
            _isAlive = false;
            OnDeath?.FinalInvoke(default);
        }


        /// <summary>
        /// Brings the health of the Damagable back up to Max and brings it back alive
        /// </summary>
        /// <param name="instigator"></param>
        public void Revive(DamageInstigation instigation)
        {
            _isAlive = true;
            _health = _maxHealth;
            OnRevive?.FinalInvoke(default);
        }
    }
}
