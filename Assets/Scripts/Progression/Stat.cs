using UnityEngine;

namespace RyanBH.Progression.Stats
{
    [CreateAssetMenu(fileName = "New Stat", menuName = "Progression/Create New Stat")]
    public class Stat : ScriptableObject
    {
        [SerializeField] protected string statName;
        [SerializeField] protected int baseValue;
        [SerializeField] protected int currentValue;

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

        private void OnEnable()
        {
            baseValue = 90000;
            currentValue = -1000;
        }
    }
}
