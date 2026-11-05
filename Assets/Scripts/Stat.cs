using UnityEngine;

namespace RyanBH.Progression.Stats
{
    /// <summary> Contains all variables and functions relating to a stat. </summary>
    public class Stat
    {
        protected string statName;
        protected int baseValue;
        protected int currentValue;

        /// <summary> Sets the name and base amount of a stat. </summary>
        /// <param name="name">Replace with the name of the stat.</param>
        /// <param name="baseAmount">Replace with the base amount of the stat.</param>
        public Stat(string name, int baseAmount)
        {
            statName = name;
            baseValue = baseAmount;
        }

        #region Get the variables

        /// <summary> Returns the current value of the stat. </summary>
        public int GetCurrentValue()
        {
            return currentValue;
        }
        
        /// <summary> Returns the name of the stat. </summary>
        public string GetStatName()
        {
            return statName;
        }

        /// <summary> Reutrns the base value of the stat. </summary>
        public int GetBaseValue()
        {
            return baseValue;
        }
        
        #endregion

        /// <summary> Calculate the current value of the stat. </summary>
        /// <param name="characterLevel"> Replace with the level bar's GetLevel()</param>
        public void CalculateCurrentValue(int characterLevel)
        {
            currentValue = (((2 * baseValue) * characterLevel) / 100) + 1;
            Debug.Log(statName + ": " + currentValue);
        }
    }
}
