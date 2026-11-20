using UnityEngine;

namespace RyanBH.Progression.LevelBar
{
    /// <summary> Contains all the variables and functions concerning the player's level bar. </summary>
    public class PlayerLevelBar
    {
        
        /// <summary> The current level. </summary>
        private int playerLevel;
        /// <summary> The current XP amount. </summary>
        private int experienceAmount;
        /// <summary> The threshold to level up to the next level. </summary>
        private int experienceThreshold;

        /// <summary> Create a level bar with a level, xp amount and threshold for it. The 3 parameters are for debugging purposes if you will be using ResetLevelUpBar(). </summary>
        ///<param name="lvLevel"> Replace with the level you want to set it at. </param>
        ///<param name="lvThreshold"> Replace with the threshold to the next level.</param>
        ///<param name="lvXpAmount"> Replace with the current amount of XP. </param>
        public PlayerLevelBar(int lvLevel,int lvXpAmount, int lvThreshold)
        {
            playerLevel = lvLevel;
            experienceAmount = lvXpAmount;
            experienceThreshold = lvThreshold;
        }

        #region Getting the variables

        /// <summary> Returns the current Level. </summary>
        public int GetLevel()
        {
            return playerLevel;
        }
        
        /// <summary> Returns the XP amount. </summary>
        public int GetXpAmount()
        {
            return experienceAmount;
        }

        /// <summary> Returns the XP threshold. </summary>
        public int GetXpThreshold()
        {
            return experienceThreshold;
        }
        
        #endregion

        /// <summary> Call this function anytime you want to gain XP. </summary>
        /// <param name="amount"> The amount of XP gained. </param>
        public void GainExperience(int amount)
        {
            experienceAmount += amount;
        }
        
        /// <summary> Use this to reset your level up bar and set the threshold at level 1. </summary>
        /// <param name="firstThreshold"> The threshold to level up to level 2.</param>
        public void ResetLevelUpBar(int firstThreshold)
        {
            playerLevel = 1;
            experienceAmount = 0;
            experienceThreshold = firstThreshold;
            Debug.Log("Level: " + playerLevel + " | XP Amount: " + experienceAmount);
            Debug.Log("XP to Level 2: " + experienceThreshold);
        }

        /// <summary> Increases level by 1, carries over excess XP and increases XP threshold. Sends a Warning if the threshold hasn't been exceeded. </summary>
        public bool LevelUpCheck()
        {
            if (experienceAmount >= experienceThreshold)
            {
                playerLevel += 1;
                experienceAmount = experienceAmount - experienceThreshold;
                experienceThreshold = experienceThreshold + (experienceThreshold/4);
                Debug.Log("LEVEL UP! You leveled up to Level " + playerLevel);
                Debug.Log("XP: " + experienceAmount + "/" + experienceThreshold);
                return true;
            }
            else
            {
                Debug.LogWarning("Not enough XP to Level Up!");
                Debug.LogWarning("XP: " + experienceAmount + "/" + experienceThreshold);
                return false;
            }
        }
    }
}