using UnityEngine;

namespace RyanBH.Progression.Stats
{
    /// <summary> A stat that can be used as a resource, akin to health and mana in traditional RPGs. </summary>
    public class ResourceStat : Stat
    {
        /// <summary> Amount of resource Stat left unused. </summary>
        private int leftValue;

        /// <summary> Sets the name and base amount of the resource Stat. </summary>
        /// <param name="resourceName"> Replace with the name of the resource stat. </param>
        /// <param name="resourceBaseAmount"> Replace with the base amount of the resource Stat.</param>
        public ResourceStat(string resourceName, int resourceBaseAmount) : base(resourceName, resourceBaseAmount)
        {
            // No need to put statName = name since it's already in the base(). Same goes for baseValue.
        }

        ///<summary> Gets the amount left of a resource Stat as an int. </summary>
        ///<returns> The amount of resource stat left. </returns>
        public int GetLeftValue()
        {
            return leftValue;
        }

        #region Resource Restoration

        /// <summary> Restores the leftValue of the resource Stat by a certain amount. </summary>
        /// <param name="amountRestored"> The amount of restored resource Stat.</param>
        public void RestoreResource(int amountRestored)
        {
            leftValue += amountRestored;
            if (leftValue > currentValue)
            {
                leftValue = currentValue;
            }
        }

        /// <summary> Fully restores the leftValue of the resource Stat to its currentValue. </summary>
        public void FullyRestoreResource()
        {
            leftValue = currentValue;
        }
        
        #endregion

        #region Resource Usage and Check
        
        /// <summary> Decreases the resource Stat by a certain amount. </summary>
        /// <param name="amountUsed"> The amount of resource Stat used.</param>
        public bool UseResource(int amountUsed)
        {
            if (ResourceCheck(amountUsed) != false)
            {
                leftValue -= amountUsed;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary> Use this to check if you can use the resource stat or not. </summary>
        /// <returns> False if (leftValue - amount) is less than zero. </returns>
        /// <param name="amount"> Amount to be subtracted by leftValue. </param>
        private bool ResourceCheck(int amount)
        {
            if (leftValue - amount < 0)
            {
                leftValue = 0;
                Debug.LogWarning("Not enough " + statName + " to use this!");
                return false;
            }
            else
            {
                return true;
            }
        }
        
        #endregion
    }
}