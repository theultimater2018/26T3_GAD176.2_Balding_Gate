using RyanBH.Progression.Stats;
using System.Linq;
using UnityEngine;

namespace RyanBH.Progression.Manager
{
    /// <summary>
    /// Manages the interaction between the player's stats and the player's level.
    /// </summary>
    public class PlayerProgressionManager : MonoBehaviour
    {
        #region Variables
        
        /// <summary> This is the player's level bar </summary>
        [SerializeField] private LevelBar.PlayerLevelBar levelMeter;
        /// <summary> This array holds all the stats you want to use. </summary>
        [SerializeField] private Stat[] statsList;
        /// <summary> This array holds all the Resource Stats. </summary>
        [SerializeField] private ResourceStat[] resourceStatsList;
        
        #endregion

        /// <summary>
        /// Calculates all stats at the current level.
        /// </summary>
        private void CalculateAllStats()
        {
            // This for loop goes through each stat in the list (including resources) and calculates them at that certain level
            for (int i = 0; i < statsList.Length; i++)
            {
                statsList[i].CalculateCurrentValue(levelMeter.GetLevel());
            }
            // This is the same but for resource stats and it fully restores each resource Stat.
            for (int i = 0; i < resourceStatsList.Length; i++)
            {
                resourceStatsList[i].CalculateCurrentValue(levelMeter.GetLevel());
                resourceStatsList[i].FullyRestoreResource();
            }
        }

        #region Unity Functions

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            levelMeter.ResetLevelUpBar(30);
            CalculateAllStats();
        }
        #endregion
    }
}
