using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philotic_Knight
{
    /// <summary>
    /// The Model for the Field Trainer System
    /// </summary>
    public class Model
    {
        /// <summary>
        /// DataSet to contain all of the data the application uses
        /// </summary>
        public static DataSet AllData = null;

        /// <summary>
        /// DataTable that contains all powers within it
        /// </summary>
        public static DataTable AllPowers = null;

        /// <summary>
        /// Fields in the All Powers DataTable
        /// </summary>
        public enum AllPowersFields
        {
            AT_Name,
            AT_DisplayName,
            AT_SetType,
            AT_CategoryType,
            PowerSet_Name,
            PowerSet_DisplayName,
            Power_Name,
            Power_DisplayName,
            LevelAvailable
        }

        public enum AllPowersSetTypes
        {
            PRIMARY,
            SECONDARY,
            EPIC,
            POOL,
            INHERENT
        }

        /// <summary>
        /// DataTable containing Character basic data
        /// </summary>
        public static DataTable CharacterData = null;

        /// <summary>
        /// Fields in the CharacterData DataTable
        /// </summary>
        public enum CharacterDataFields
        {
            CharacterName,
            AT_Name,
            PrimaryPowerSet_Name,
            SecondaryPowerSet_Name,
            CurrentLevel,
            ProgramVersion
        }

        /// <summary>
        /// DataTable containing a character's power picks
        /// </summary>
        public static DataTable CharacterPowerPicks = null;

        public enum CharacterPowerPicksFields
        {
            AT_Name,
            AT_SetType,
            PowerSet_Name,
            Power_Name,
            LevelPicked
        }

        /// <summary>
        /// DataTable containing a character's enhancement choices
        /// </summary>
        public static DataTable CharacterEnhancementSlots = null;

        public enum CharacterEnhancementSlotsFields
        {
            AT_Name,
            PowerSet_Name,
            Power_Name,
            SlotNumber,
            LevelPicked,
            EnhancementName,
            EnhancementLevel
        }

        /// <summary>
        /// Container to hold all message data inside of
        /// </summary>
        public static Dictionary<string, string> AllMessages = new Dictionary<string, string>();

        /// <summary>
        /// Action Selections for the Controller
        /// </summary>
        public enum Action
        {
            Enter_Character_Name,
            Pick_Archetype,
            Pick_Primary,
            Pick_Secondary,
            Pick_First_Power,
            Pick_Second_Power,
            Pick1Power,
            Pick1EnhancementSlot,
            Pick2EnhancementSlots,
            Pick3EnhancementSlots,
            None
        }

        public static Dictionary<Action, string> ActionDescriptions = new Dictionary<Action, string>()
        {
            {Action.Enter_Character_Name, "Enter your Character Name" },
            {Action.Pick_Archetype, "Pick an Archetype" },
            {Action.Pick_Primary, "Pick your Primary PowerSet" },
            {Action.Pick_Secondary, "Pick your Secondary PowerSet" },
            {Action.Pick_First_Power, "Pick First Primary Power" },
            {Action.Pick_Second_Power, "Pick First Secondary Power" },
            {Action.Pick1Power, "Pick 1 Power" },
            {Action.Pick1EnhancementSlot, "Pick 1 Enhancement Slot" },
            {Action.Pick2EnhancementSlots, "Pick 2 Enhancement Slots" },
            {Action.Pick3EnhancementSlots, "Pick 3 Enhancement Slots" },
            {Action.None, "All Done!" },
        };
    }
}
