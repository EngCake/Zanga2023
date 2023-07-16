using System;
using UnityEngine;

namespace CakeEngineering
{
    [CreateAssetMenu(fileName = "Attribute", menuName = "Attribute")]
    public class EntityAttribute : ScriptableObject
    {
        public string Name;

        public bool Locked;

        public EntityAttribute Opposite;

        public override bool Equals(object obj)
        {
            return obj is EntityAttribute attribute &&
                   base.Equals(obj) &&
                   Name == attribute.Name &&
                   Locked == attribute.Locked;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Name, Locked, Opposite);
        }
    }
}