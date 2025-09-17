using Unity.VisualScripting;
using UnityEngine.Rendering;

namespace Remedy.Framework
{
    public static class GenericExtensions
    {
        /// <summary>
        /// Copies field values of the other class instance to fill the values of this class instance.
        /// </summary>
        /// <param name="this">The class instance to fill the values.</param>
        /// <param name="other">The class instance to copy the values from.</param>
        public static void InheritFieldValues(this object @this, object other)
        {
            foreach (var field in other.GetType().GetFields())
            {
                var thisField = @this.GetType().GetField(field.Name);

                if (thisField?.FieldType.IsConvertibleTo(field.FieldType, true) == true)
                {
                    thisField.SetValue(@this, field.GetValue(other));
                }
            }
        }
    }
}