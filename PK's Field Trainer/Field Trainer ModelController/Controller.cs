using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Philotic_Knight.Model;
using static Philotic_Knight.DefMethods;

namespace Philotic_Knight
{
    /// <summary>
    /// Controller for both versions of the Field Trainer
    /// </summary>
    public static class Controller
    {
        /// <summary>
        /// Resets the AllPowers Static DataTable to the default setting for loading
        /// </summary>
        /// <returns></returns>
        public static DataTable PrepAllPowers()
        {
            DataTable retVal = new DataTable(nameof(AllPowers));
            retVal.Columns.AddRange(new DataColumn[]
            {
                new DataColumn(nameof(AllPowersFields.AT_Name)),
                new DataColumn(nameof(AllPowersFields.AT_DisplayName)),
                new DataColumn(nameof(AllPowersFields.AT_CategoryType)),
                new DataColumn(nameof(AllPowersFields.AT_SetType), typeof(int)), //Used for later sorting
                new DataColumn(nameof(AllPowersFields.PowerSet_Name)),
                new DataColumn(nameof(AllPowersFields.PowerSet_DisplayName)),
                new DataColumn(nameof(AllPowersFields.Power_Name)),
                new DataColumn(nameof(AllPowersFields.Power_DisplayName)),
                new DataColumn(nameof(AllPowersFields.LevelAvailable), typeof(int))
            }); ;
            return retVal;
        }

        public static DataTable AddPowerToAllPowers(DataTable InputDataTable, string ATName, string AT_DisplayName, string AT_CategoryType,
            int AllPowersSetType, string PowerSetName, string PowerSetDisplayName, string PowerName, string PowerDisplayName, int LevelAvailable)
        {
            DataTable retVal = InputDataTable;
            DataRow newRow = InputDataTable.NewRow();
            newRow[nameof(AllPowersFields.AT_Name)] = ATName;
            newRow[nameof(AllPowersFields.AT_DisplayName)] = AT_DisplayName;
            newRow[nameof(AllPowersFields.AT_CategoryType)] = AT_CategoryType;
            newRow[nameof(AllPowersFields.AT_SetType)] = AllPowersSetType;
            newRow[nameof(AllPowersFields.PowerSet_Name)] = PowerSetName;
            newRow[nameof(AllPowersFields.PowerSet_DisplayName)] = PowerSetDisplayName;
            newRow[nameof(AllPowersFields.Power_Name)] = PowerName;
            newRow[nameof(AllPowersFields.Power_DisplayName)] = PowerDisplayName;
            newRow[nameof(AllPowersFields.LevelAvailable)] = LevelAvailable;
            retVal.Rows.Add(newRow);
            return retVal;
        }

        public static DataTable PrepCharacter()
        {
            DataTable retVal = new DataTable(nameof(CharacterData));
            retVal.Columns.AddRange(new DataColumn[]
            {
                new DataColumn(nameof(CharacterDataFields.CharacterName)),
                new DataColumn(nameof(CharacterDataFields.AT_Name)),
                new DataColumn(nameof(CharacterDataFields.PrimaryPowerSet_Name)),
                new DataColumn(nameof(CharacterDataFields.SecondaryPowerSet_Name)),
                new DataColumn(nameof(CharacterDataFields.CurrentLevel), typeof(int)),
                new DataColumn(nameof(CharacterDataFields.ProgramVersion))
            });
            return retVal;
        }

        /// <summary>
        /// Preps the datatable of power picks
        /// </summary>
        /// <returns></returns>
        public static DataTable PrepCharacterPowerPicks()
        {
            DataTable retVal = new DataTable(nameof(CharacterPowerPicks));
            retVal.Columns.AddRange(new DataColumn[]
            {
                new DataColumn(nameof(CharacterPowerPicksFields.AT_Name)),
                new DataColumn(nameof(CharacterPowerPicksFields.AT_SetType), typeof(int)),
                new DataColumn(nameof(CharacterPowerPicksFields.PowerSet_Name)),
                new DataColumn(nameof(CharacterPowerPicksFields.Power_Name)),
                new DataColumn(nameof(CharacterPowerPicksFields.LevelPicked), typeof(int))
            });
            return retVal;
        }

        public static DataTable PrepCharacterSlotPicks()
        {
            DataTable retVal = new DataTable(nameof(CharacterEnhancementSlots));
            retVal.Columns.AddRange(new DataColumn[]
            {
                new DataColumn(nameof(CharacterEnhancementSlotsFields.AT_Name)),
                new DataColumn(nameof(CharacterEnhancementSlotsFields.PowerSet_Name)),
                new DataColumn(nameof(CharacterEnhancementSlotsFields.Power_Name)),
                new DataColumn(nameof(CharacterEnhancementSlotsFields.SlotNumber), typeof(int)),
                new DataColumn(nameof(CharacterEnhancementSlotsFields.LevelPicked), typeof(int)),
                new DataColumn(nameof(CharacterEnhancementSlotsFields.EnhancementName)),
                new DataColumn(nameof(CharacterEnhancementSlotsFields.EnhancementLevel), typeof(int)),
            });
            return retVal;
        }

        public static DataRow AddPowerToPicks(DataTable InputData, string ATName, int ATSetType, string powerSetName, string powerName, int levelPicked)
        {
            //Validate that the datatable doesn't already have the power
            if (PowerPicksHavePower(InputData, ATName, powerSetName, powerName)) throw new Exception("Power '" +
                ATName + "." + powerSetName + "." + powerName + "' is already assigned to this character! Please report this issue to the developer(s).");
            DataRow dr = InputData.NewRow();
            dr[nameof(CharacterPowerPicksFields.AT_Name)] = ATName;
            dr[nameof(CharacterPowerPicksFields.AT_SetType)] = ATSetType;
            dr[nameof(CharacterPowerPicksFields.PowerSet_Name)] = powerSetName;
            dr[nameof(CharacterPowerPicksFields.Power_Name)] = powerName;
            dr[nameof(CharacterPowerPicksFields.LevelPicked)] = levelPicked;
            InputData.Rows.Add(dr);

            //Always add the first slot to the enhancements list
            AddSlot(CharacterEnhancementSlots, ATName, powerSetName, powerName, 1, levelPicked, "", 0);

            return dr;
        }

        /// <summary>
        /// Determine whether the input power already exists inside the passed in DataTable
        /// </summary>
        /// <param name="InputData"></param>
        /// <param name="ATName"></param>
        /// <param name="powerSetName"></param>
        /// <param name="powerName"></param>
        /// <returns></returns>
        public static bool PowerPicksHavePower(DataTable InputData, string ATName, string powerSetName, string powerName)
        {
            DataRow[] drFound = InputData.Select(nameof(CharacterPowerPicksFields.AT_Name) + " = '" + ATName + "' AND " +
                nameof(CharacterPowerPicksFields.PowerSet_Name) + " = '" + powerSetName + "' AND " +
                nameof(CharacterPowerPicksFields.Power_Name) + " = '" + powerName + "'");
            return drFound.Length > 0 ? true : false;
        }

        /// <summary>
        /// Gets the appropriate Action Selection
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Model.Action GetAction(int level)
        {
            switch (level)
            {
                case -3:
                    return Model.Action.Pick_Archetype;
                case -2:
                    return Model.Action.Pick_Primary;
                case -1:
                    return Model.Action.Pick_Secondary;
                case 0:
                    return Model.Action.Pick_First_Power;
                case 1:
                    return Model.Action.Pick_Second_Power;
                case 2: case 4: case 6: case 8: case 10:
                case 12: case 14: case 16: case 18: case 20: case 22:
                case 24: case 26: case 28: case 30: case 32: case 35:
                case 38: case 41: case 44: case 47: case 49:
                    return Model.Action.Pick1Power;
                case 3: case 5: case 7: case 9: case 11: case 13: case 15:
                case 17: case 19: case 21: case 23: case 25: case 27: case 29:
                    return Model.Action.Pick2EnhancementSlots;
                case 31: case 33: case 34: case 36: case 37: case 39:
                case 40: case 42: case 43: case 45: case 46: case 48: case 50:
                    return Model.Action.Pick3EnhancementSlots;
                case 51:
                    return Model.Action.None;
                default: throw new Exception("Invalid Level '" + level + "'");
            }
        }

        public static string GetActionDescription(Model.Action action)
        {
            return Model.ActionDescriptions[action];
        }

        public static DataSet GetDataSetFromXML(string fileName)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(fileName);
            return ds;
        }

        public static DataTable GetArchetypes(DataTable InputData)
        {
            DataTable retVal = InputData.DefaultView.ToTable(true, new string[] { 
                nameof(AllPowersFields.AT_Name), nameof(AllPowersFields.AT_DisplayName) });
            retVal.Rows.Add("", "");
            retVal.DefaultView.Sort = nameof(AllPowersFields.AT_Name);
            retVal = retVal.DefaultView.ToTable();
            return retVal;
        }

        public static DataTable GetPrimaries(DataTable InputData, string Archetype)
        {
            DataTable retVal = InputData.Select(nameof(AllPowersFields.AT_Name) + " = '" + Archetype + "' AND " +
                nameof(AllPowersFields.AT_CategoryType) + " = '" + nameof(AllPowersSetTypes.PRIMARY) + "'").
                CopyToDataTable().DefaultView.ToTable(true, new string[] { 
                    nameof(AllPowersFields.PowerSet_Name), nameof(AllPowersFields.PowerSet_DisplayName)});
            retVal.Rows.Add("", "");
            retVal.DefaultView.Sort = nameof(AllPowersFields.PowerSet_Name);
            retVal = retVal.DefaultView.ToTable();
            return retVal;
        }

        public static DataTable GetSecondaries(DataTable InputData, string Archetype)
        {
            DataTable retVal = InputData.Select(nameof(AllPowersFields.AT_Name) + " = '" + Archetype + "' AND " +
                nameof(AllPowersFields.AT_CategoryType) + " = '" + nameof(AllPowersSetTypes.SECONDARY) + "'").
                CopyToDataTable().DefaultView.ToTable(true, new string[] { 
                    nameof(AllPowersFields.PowerSet_Name), nameof(AllPowersFields.PowerSet_DisplayName) });
            retVal.Rows.Add("", "");
            retVal.DefaultView.Sort = nameof(AllPowersFields.PowerSet_Name);
            retVal = retVal.DefaultView.ToTable();
            return retVal;
        }

        public static DataTable GetFirstPower(DataTable InputData, string Archetype, string Primary)
        {
            DataTable retVal;

            retVal = InputData.Select(nameof(AllPowersFields.AT_Name) + " = '" + Archetype + "' AND " +
                nameof(AllPowersFields.AT_CategoryType) + " = '" + AllPowersSetTypes.PRIMARY + "' AND " +
                nameof(AllPowersFields.PowerSet_Name) + " = '" + Primary + "' AND " +
                nameof(AllPowersFields.LevelAvailable) + " <= 1").CopyToDataTable();
            retVal.Rows.Add("", "");
            retVal.DefaultView.Sort = nameof(AllPowersFields.Power_Name);
            retVal = retVal.DefaultView.ToTable();

            //Finally, take out any exceptions due to missing requirements (special cases)

            return retVal;
        }

        public static DataTable GetSecondPower(DataTable InputData, string Archetype, string Secondary)
        {
            DataTable retVal;

            retVal = InputData.Select(nameof(AllPowersFields.AT_Name) + " = '" + Archetype + "' AND " +
                nameof(AllPowersFields.AT_CategoryType) + " = '" + AllPowersSetTypes.SECONDARY + "' AND " +
                nameof(AllPowersFields.PowerSet_Name) + " = '" + Secondary + "' AND " +
                nameof(AllPowersFields.LevelAvailable) + " <= 1").CopyToDataTable();
            retVal.Rows.Add("", "");
            retVal.DefaultView.Sort = nameof(AllPowersFields.Power_Name);
            retVal = retVal.DefaultView.ToTable();

            //Finally, take out any exceptions due to missing requirements (special cases)

            return retVal;
        }

        /// <summary>
        /// Adds an Enhancement slot to the Enhancement slots table
        /// </summary>
        /// <param name="InputData"></param>
        /// <param name="ATName"></param>
        /// <param name="PowerSetName"></param>
        /// <param name="PowerName"></param>
        /// <param name="SlotNumber"></param>
        /// <param name="LevelPicked"></param>
        /// <param name="EnhancementName"></param>
        /// <param name="EnhancementLevel"></param>
        public static void AddSlot(DataTable InputData, string ATName, string PowerSetName, string PowerName, int SlotNumber, int LevelPicked,
            string EnhancementName, int EnhancementLevel)
        {
            if (InputData == null || InputData.Columns.Count == 0)
            {
                InputData = PrepCharacterSlotPicks();
                CharacterEnhancementSlots = InputData;
            }
            InputData.Rows.Add(ATName, PowerSetName, PowerName, SlotNumber, LevelPicked, EnhancementName, EnhancementLevel);
        }

        //Method to determine which powers are available in a given comboBox
        public static DataTable GetAvailablePowers(DataTable InputData, DataTable CurrentCharacterPowers, string Archetype, string Primary, string Secondary, int level)
        {
            try
            {
                DataTable retVal = null;

                // First, from the list of all powers, take away any that aren't related to the current character's class,
                // or are higher than the character's current level using LINQ-to-DataTable
                EnumerableRowCollection<DataRow> query = from row in InputData.AsEnumerable()
                                                         let at = row.Field<string>(nameof(AllPowersFields.AT_Name))
                                                         let type = row.Field<string>(nameof(AllPowersFields.AT_SetType))
                                                         let lvl = row.Field<string>(nameof(AllPowersFields.LevelAvailable))
                                                         let cat = row.Field<string>(nameof(AllPowersFields.AT_CategoryType))
                                                         let pwrSet = row.Field<string>(nameof(AllPowersFields.PowerSet_Name))
                                                         where at == Archetype && int.Parse(lvl) <= level &&
                                                         (
                                                            (cat == nameof(AllPowersSetTypes.PRIMARY) && pwrSet == Primary) ||
                                                            (cat == nameof(AllPowersSetTypes.SECONDARY) && pwrSet == Secondary) ||
                                                            cat == nameof(AllPowersSetTypes.POOL) || cat == nameof(AllPowersSetTypes.EPIC)
                                                            )
                                                         select row;
                retVal = query.CopyToDataTable();

                // Second, take away all that the player already has
                foreach (DataRow dr in CurrentCharacterPowers.Rows)
                {
                    DataRow[] foundRows = retVal.Select(nameof(CharacterPowerPicksFields.AT_Name) + " = '" +
                        dr[nameof(AllPowersFields.AT_Name)] + "' AND " +
                        nameof(CharacterPowerPicksFields.PowerSet_Name) + " = '" +
                        dr[nameof(AllPowersFields.PowerSet_Name)] + "' AND " +
                        nameof(CharacterPowerPicksFields.Power_Name) + " = '" +
                        dr[nameof(AllPowersFields.Power_Name)] + "'");
                    if (foundRows.Length > 0) retVal.Rows.Remove(foundRows[0]);
                }

                //If the player has already picked four power pools, take away all other powers that aren't in those four
                //Get the list of distinct pool power powerset names
                DataRow[] poolRowsSelected = CurrentCharacterPowers.Select(nameof(Model.CharacterPowerPicksFields.AT_SetType) + " = " + (int)AllPowersSetTypes.POOL);
                if (poolRowsSelected.Length > 0)
                {
                    DataTable poolsSelected = new DataView(poolRowsSelected.CopyToDataTable()).ToTable(true, nameof(AllPowersFields.PowerSet_Name));
                    if (poolsSelected.Rows.Count == 4)
                    {
                        List<DataRow> rowsToRemove = new List<DataRow>();
                        foreach (DataRow dr in retVal.Rows)
                        {
                            if ((int)dr[nameof(AllPowersFields.AT_SetType)] != (int)AllPowersSetTypes.POOL) continue;
                            //Search for this row's PowerSet in the list of chosen powersets
                            DataRow[] drs = poolsSelected.Select(nameof(AllPowersFields.PowerSet_Name) + " = '"
                                + dr[nameof(AllPowersFields.PowerSet_Name)].ToString() + "'");
                            if (drs.Length == 0) rowsToRemove.Add(dr);
                        }

                        foreach (DataRow dr in rowsToRemove)
                        {
                            retVal.Rows.Remove(dr);
                        }
                    }
                }

                //If the player has selected any epic powers, remove all epic powers that aren't a part of that set
                DataRow[] epicRowsSelected = CurrentCharacterPowers.Select(nameof(Model.CharacterPowerPicksFields.AT_SetType) + " = " + (int)AllPowersSetTypes.EPIC);
                if (epicRowsSelected.Length > 0)
                {
                    DataTable epicSelected = new DataView(epicRowsSelected.CopyToDataTable()).ToTable(true, nameof(AllPowersFields.PowerSet_Name));
                    if (epicSelected.Rows.Count > 0)
                    {
                        List<DataRow> rowsToRemove = new List<DataRow>();
                        foreach (DataRow dr in retVal.Rows)
                        {
                            if ((int)dr[nameof(AllPowersFields.AT_SetType)] != (int)AllPowersSetTypes.EPIC) continue;
                            //Search for this row's PowerSet in the list of chosen powersets
                            DataRow[] drs = epicSelected.Select(nameof(AllPowersFields.PowerSet_Name) + " = '"
                                + dr[nameof(AllPowersFields.PowerSet_Name)].ToString() + "'");
                            if (drs.Length == 0) rowsToRemove.Add(dr);
                        }

                        foreach (DataRow dr in rowsToRemove)
                        {
                            retVal.Rows.Remove(dr);
                        }
                    }
                }

                //Finally, take out any exceptions due to missing requirements (special cases)

                //Resort by the category
                retVal.DefaultView.Sort = nameof(Model.AllPowersFields.AT_SetType);
                retVal = retVal.DefaultView.ToTable();

                //Don't forget to add in the blank row!
                retVal.Rows.InsertAt(retVal.NewRow(), 0);
                return retVal;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Get Available Powers.", ex);
            }
        }

        public static bool LogMessage(string Message, object LogDevice)
        {
            try
            {
                if (LogDevice.GetType() == typeof(TextBox))
                {
                    ((TextBox)LogDevice).Text = Message;
                }
                else if (LogDevice.GetType() == typeof(ToolStripStatusLabel))
                {
                    ((ToolStripStatusLabel)LogDevice).Text = Message;
                }
                Application.DoEvents();
                return true;
            }
            catch(Exception ex)
            {
                throw new Exception("Failed to log message '" + Message + "' to device '" + LogDevice.ToString(), ex);
            }
        }

        public static DataTable ScanPowersDataFromPath(string dataPath, object LogDevice = null)
        {
            DataTable retVal = PrepAllPowers();
            string tempFileName;

            //Get all powers message files and put them into the global dictionary
            string[] messages = Directory.GetFiles(dataPath + @"\texts\English\Powers\", "*.ms");
            foreach (string messageFile in messages)
            {
                LogMessage("Processing message file '" + messageFile + "'", LogDevice);
                foreach (KeyValuePair<string, string> kvp in DefMethods.MSFileToDictionary(messageFile))
                {
                    if (!AllMessages.ContainsKey(kvp.Key)) AllMessages.Add(kvp.Key, kvp.Value);
                }
            }

            //Get classes messages
            tempFileName = GetFileNameInPath(dataPath, dataPath + @"texts\English\Classes\classes.def.ms");
            if (tempFileName == "") throw new Exception("Failed to find message file 'classes.def.ms'");
            Dictionary<string, string> classNames = DefMethods.MSFileToDictionary(tempFileName);
            if (classNames == null) throw new Exception("Failed to process msFile 'classes.def.ms'");

            //1. Start by building the Classes DataTables
            List<string> classesFiles = Directory.GetFiles(dataPath, "classes.def", SearchOption.AllDirectories).ToList();

            if (classesFiles.Count != 1) throw new Exception("Found more than one classes.def file. Program was only expecting one! Files:" +
                String.Join(Environment.NewLine, classesFiles));

            if (File.Exists(classesFiles[0].Replace("classes.def", "v_classes.def"))) classesFiles.Add(classesFiles[0].Replace("classes.def", "v_classes.def"));
            //Find the missing villain file
            else classesFiles = classesFiles.Concat(Directory.GetFiles(dataPath, "v_classes.def", SearchOption.AllDirectories).ToList()).ToList();

            if (classesFiles.Count != 2) throw new Exception("Found more than one v_classes.def file. Program was only expecting one! Files:" +
                String.Join(Environment.NewLine, classesFiles));

            //Reset the Data path if it's not the same as where we found the classes.def file. Assume the correct data path is two levels up from the classes.def file
            if (dataPath != Path.GetFullPath(Path.Combine(classesFiles[0], @"..\..\"))) dataPath = Path.GetFullPath(Path.Combine(classesFiles[0], @"..\..\"));

            //From that, get the list of Class Def files and DefObjects
            List<DefObject> Classes = new List<DefObject>();
            foreach (string classesFile in classesFiles)
            {
                foreach (string line in File.ReadAllLines(classesFile))
                {
                    if (line == null || line.Trim() == "" || line.StartsWith("//")) continue;
                    tempFileName = GetFileNameInPath(dataPath, dataPath + @"Defs\classes\" + line.Split('/')[line.Split('/').Length - 1]);
                    if (tempFileName == "") throw new Exception("Cannot find file for ClassFile '" + line.Split('/')[line.Split('/').Length - 1] + "'");
                    Classes = Classes.Concat(DefMethods.GetDefObjectsFromDefFile(tempFileName)).ToList();
                }
            }

            //Get the epic powers categories
            tempFileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\Epic_Powers.categories");
            if (tempFileName == "") throw new Exception("Failed to get Epic Powers categories");
            List<DefObject> epicCategories = DefMethods.GetDefObjectsFromDefFile(tempFileName);

            //Get the epic powers powersets
            tempFileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\Epic.powersets");
            if (tempFileName == "") throw new Exception("Failed to get Epic Powers powersets");
            List<DefObject> epicPowerSets = DefMethods.GetDefObjectsFromDefFile(tempFileName);

            foreach (DefObject epicCategory in epicCategories)
            {
                if (epicCategory.ObjectName != "Epic") continue;

                foreach (DefProperty dp in epicCategory.BaseProperties)
                {
                    if (dp.PropertyName != "PowerSets") continue;
                    //Find the Powersets file for the set

                    foreach (DefObject epicPowerSet in epicPowerSets)
                    {
                        if (epicPowerSet.ObjectName != dp.PropertyValue) continue;

                        //Fine the powers file for the set
                        tempFileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\" + epicPowerSet.ObjectName.Replace('.', '_') + ".powers");
                        if (tempFileName == "") throw new Exception("Failed to get Epic Powers file for set '" + epicPowerSet.ObjectName + "'");
                        List<DefObject> epicPowers = DefMethods.GetDefObjectsFromDefFile(tempFileName);

                        //Get the list of ATs that this powerset is appropriate for
                        List<string> ATList = new List<string>();
                        foreach (DefObject epicPower in epicPowers)
                        {
                            foreach (DefProperty dpPower in epicPower.BaseProperties)
                            {
                                if (dpPower.PropertyName != "BuyRequires") continue;
                                string[] elements = dpPower.PropertyValue.Split(new string[] { "$archetype @" }, StringSplitOptions.None);
                                foreach (string element in elements)
                                {
                                    if (!element.StartsWith("Class")) continue;
                                    if (!ATList.Contains(element.Split(' ')[0])) ATList.Add(element.Split(' ')[0]);
                                }
                            }
                        }

                        // Now, add each power, if it hasn't already been added
                        foreach (DefObject epicPower in epicPowers)
                        {

                            //Skip non-power items
                            if (epicPower.BaseProperties[0].PropertyName != "Name") continue;

                            foreach (string AT in ATList)
                            {
                                string displayName = "";
                                //Match with the actual class to get the class name
                                foreach (DefObject @class in Classes)
                                {
                                    foreach (DefProperty classDP in @class.BaseProperties)
                                    {
                                        if (classDP.PropertyName != "Name") continue;
                                        if (IxQuotes(classDP.PropertyValue) != AT) continue;

                                        //Now, get the DisplayName by adding 1 to the index of the current element
                                        displayName = IxQuotes(@class.BaseProperties[@class.BaseProperties.IndexOf(classDP) + 1].PropertyValue);
                                        break;
                                    }
                                    if (displayName != "") break;
                                }

                                //Get the level available from the PowerSet listing for this power
                                int LevelAvailable = 0;
                                foreach (DefSubObject power in epicPowerSet.Attributes)
                                {
                                    if (power.SubObjectsProperties[0].PropertyValue != IxQuotes(epicPower.BaseProperties[0].PropertyValue)) continue;
                                    LevelAvailable = int.Parse(power.SubObjectsProperties[1].PropertyValue) + 1;
                                    break;
                                }

                                string powerSetDisplayName = IxQuotes(epicPowerSet.BaseProperties[1].PropertyValue);
                                string powerDisplayName = IxQuotes(epicPower.BaseProperties[1].PropertyValue);

                                //Add the epic power for this AT
                                retVal = AddPowerToAllPowers(retVal, AT, classNames.ContainsKey(displayName) ? classNames[displayName] : displayName,
                                    nameof(AllPowersSetTypes.EPIC), (int)AllPowersSetTypes.EPIC, IxQuotes(epicPowerSet.BaseProperties[0].PropertyValue),
                                    AllMessages.ContainsKey(powerSetDisplayName) ? AllMessages[powerSetDisplayName] : powerSetDisplayName,
                                    IxQuotes(epicPower.BaseProperties[0].PropertyValue), AllMessages.ContainsKey(
                                    powerDisplayName) ? AllMessages[powerDisplayName] : powerDisplayName, LevelAvailable);
                            }

                        }
                    }
                }
            }

            foreach (DefObject at in Classes)
            {
                string ATName = "", ATDisplayName = "", ATPrimaryCategory = "", ATSecondaryCategory = "";
                foreach (DefProperty dp in at.BaseProperties)
                {
                    if (dp.PropertyName == "Name") ATName = IxQuotes(dp.PropertyValue);

                    else if (dp.PropertyName == "DisplayName")
                    {
                        ATDisplayName = classNames.ContainsKey(IxQuotes(dp.PropertyValue)) ? 
                            classNames[IxQuotes(dp.PropertyValue)] : IxQuotes(dp.PropertyValue);
                    }
                    else if (dp.PropertyName == "PrimaryCategory") ATPrimaryCategory = dp.PropertyValue;
                    else if (dp.PropertyName == "SecondaryCategory") ATSecondaryCategory = dp.PropertyValue;
                }

                GetPowersFromATCategory(retVal, dataPath, ATName, ATDisplayName, ATPrimaryCategory, true, LogDevice);
                GetPowersFromATCategory(retVal, dataPath, ATName, ATDisplayName, ATSecondaryCategory, false, LogDevice);
            }

            //Finally, get all of the pool powers, and assign them to every AT
            tempFileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\Player_Pool.categories");
            if (tempFileName == "") throw new Exception("Failed to get Pool Power categories");
            List<DefObject> poolCategories = DefMethods.GetDefObjectsFromDefFile(tempFileName);

            //Load the singular Pool Powersets file
            tempFileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\Pool.powersets");
            if (tempFileName == "") throw new Exception("Failed to get Pool Power powersets");
            List<DefObject> poolPowerSets = DefMethods.GetDefObjectsFromDefFile(tempFileName);

            DataView view = new DataView(retVal);
            DataTable AllATs = view.ToTable(true, nameof(AllPowersFields.AT_Name), nameof(AllPowersFields.AT_DisplayName));

            foreach (DefObject poolCategory in poolCategories)
            {
                foreach (DefProperty dp in poolCategory.BaseProperties)
                {
                    if (dp.PropertyName != "PowerSets") continue;

                    //Load the singular Pool Powersets file
                    foreach (DefObject poolPowerSetDefinition in poolPowerSets)
                    {

                        //Get the powerset specific file data
                        tempFileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\Pool_" +
                            IxQuotes(poolPowerSetDefinition.BaseProperties[0].PropertyValue) + ".powers");
                        if (tempFileName == "") throw new Exception("Failed to get Pool Power powersets");
                        List<DefObject> powers = DefMethods.GetDefObjectsFromDefFile(tempFileName);

                        LogMessage("Adding Pool Powers for '" +
                            IxQuotes(poolPowerSetDefinition.BaseProperties[0].PropertyValue) + "'", LogDevice);

                        foreach (DefSubObject poolPowerDefinition in poolPowerSetDefinition.Attributes)
                        {

                            foreach (DefObject power in powers)
                            {
                                //Only accept matches here
                                if (poolPowerDefinition.SubObjectsProperties[0].PropertyValue !=
                                    IxQuotes(power.BaseProperties[0].PropertyValue)) continue;

                                string powerSetName = IxQuotes(poolPowerSetDefinition.BaseProperties[0].PropertyValue);
                                foreach (DataRow dr in AllATs.Rows)
                                {
                                    string powerName = IxQuotes(power.BaseProperties[0].PropertyValue);
                                    //Prevent duplicates
                                    if (AllPowersDataHasPower(retVal, dr[nameof(AllPowersFields.AT_Name)].ToString(),
                                        powerSetName,
                                        powerName)) continue;

                                    LogMessage("Adding power '" + power.BaseProperties[0].PropertyValue + "'", LogDevice);

                                    string powerSetDisplayName = IxQuotes(poolPowerSetDefinition.BaseProperties[1].PropertyValue);
                                    string powerdisplayName = IxQuotes(power.BaseProperties[1].PropertyValue);
                                    int Level = int.Parse(poolPowerDefinition.SubObjectsProperties[1].PropertyValue) + 1;
                                    if (Level < 4) Level = 4; //Force it to level 4 if it's not at least that

                                    //Add the Pool Power for this AT
                                    retVal = AddPowerToAllPowers(retVal, dr[nameof(AllPowersFields.AT_Name)].ToString(),
                                        dr[nameof(AllPowersFields.AT_DisplayName)].ToString(),
                                        nameof(AllPowersSetTypes.POOL), (int)AllPowersSetTypes.POOL, powerSetName,
                                        AllMessages.ContainsKey(powerSetDisplayName) ?
                                        AllMessages[powerSetDisplayName] : powerSetDisplayName, powerName, AllMessages.ContainsKey(
                                        powerdisplayName) ? AllMessages[powerdisplayName] : powerdisplayName, Level);
                                }
                            }

                        }
                    }
                }
            }

            //Go through them all one more time, and rename the DisplayName to include the Category, PowerSet, AND Name
            foreach (DataRow dr in retVal.Rows)
            {
                dr[nameof(AllPowersFields.Power_DisplayName)] = dr[nameof(AllPowersFields.AT_CategoryType)] + " > " +
                    dr[nameof(AllPowersFields.PowerSet_DisplayName)] +" > " + dr[nameof(AllPowersFields.Power_DisplayName)];
            }

            //Sort by AT, then Type, then, PowerSet, then Level
            retVal.DefaultView.Sort = nameof(AllPowersFields.AT_Name) + ", " + nameof(AllPowersFields.AT_CategoryType) + ", " +
                nameof(AllPowersFields.PowerSet_Name) + ", " + nameof(AllPowersFields.LevelAvailable);

            retVal = retVal.DefaultView.ToTable();
            return retVal;
        }

        public static bool AllPowersDataHasPower(DataTable inputData, string ATName, string PowerSetName, string PowerName)
        {
            DataRow[] drs = inputData.Select(AllPowersFields.AT_Name.ToString() + " = '" + ATName
                + "' AND " + AllPowersFields.PowerSet_Name.ToString() + " = '" + PowerSetName +
                "' AND " + AllPowersFields.Power_Name.ToString() + " = '" + PowerName + "'");
            return drs.Length > 0 ? true : false;
        }

        public static bool GetPowersFromATCategory(DataTable inputData, string dataPath, string ATName,
            string ATDisplayName, string ATCategory, bool categoryIsPrimary, object LogDevice)
        {
            // Check primary location
            string fileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\Player_" + IxQuotes(ATCategory) + ".categories");
            if (fileName == "")
            {
                // Try the Epic Powers file
                fileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\Epic_Powers.categories");
                if (fileName == "") throw new Exception("Cannot find file for ATCategory '" + IxQuotes(ATCategory) + "'");
            }
            List<DefObject> categories = DefMethods.GetDefObjectsFromDefFile(fileName);

            // Enumerate through all of the found categories
            foreach (DefObject category in categories)
            {
                //Skip categories whose names don't match
                if (IxQuotes(ATCategory).ToLower() != IxQuotes(category.ObjectName).ToLower()) continue;

                foreach (DefProperty dp in category.BaseProperties)
                {
                    if (dp.PropertyName != "PowerSets") continue;

                    string PowerSetName = dp.PropertyValue.Split('.')[0];
                    fileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\" + PowerSetName + ".powersets");
                    if (fileName == "") throw new Exception("Cannot find file for PowerSet '" + PowerSetName + "'");
                    List<DefObject> powerSets = DefMethods.GetDefObjectsFromDefFile(fileName);
                    foreach (DefObject powerSet in powerSets)
                    {
                        //Get the PowerSet name info
                        if (dp.PropertyValue != powerSet.ObjectName) continue;
                       LogMessage("Adding info for powerset '" + powerSet.ObjectName + "'", LogDevice);

                        foreach (DefSubObject power in powerSet.Attributes)
                        {
                            //Find the correct Power file
                            fileName = GetFileNameInPath(dataPath, dataPath + @"Defs\powers\" + powerSet.ObjectName.Replace('.', '_') + ".powers");
                            if (fileName == "" && powerSet.ObjectName.ToLower().Contains("_aux"))
                            {
                                fileName = GetFileNameInPath(dataPath,
                                    (dataPath + @"Defs\powers\" + powerSet.ObjectName.Replace('.', '_') + ".powers").ToLower().Replace("_aux", ""));
                                if (fileName == "") throw new Exception("Cannot find file for Powers '" + powerSet.ObjectName.Replace('.', '_') + "'");
                            }
                            else if (fileName == "") throw new Exception("Cannot find file for Powers '" + powerSet.ObjectName.Replace('.', '_') + "'");
                            List<DefObject> PowersObjects = DefMethods.GetDefObjectsFromDefFile(fileName);

                            string PowerDisplayName = "";
                            foreach (DefObject powerObject in PowersObjects)
                            {
                                foreach (DefProperty dpPO in powerObject.BaseProperties)
                                {
                                    if (dpPO.PropertyName != "Name") continue;
                                    if (IxQuotes(dpPO.PropertyValue) != IxQuotes(power.SubObjectsProperties[0].PropertyValue)) break;

                                    PowerDisplayName = IxQuotes(powerObject.BaseProperties[
                                        powerObject.BaseProperties.IndexOf(dpPO) + 1].PropertyValue);
                                    break;
                                }
                                if (PowerDisplayName != "") break;
                            }

                            //If we can't find the appropriate powers file, then don't add the power!
                            if (PowerDisplayName == "") continue;

                            //Prevent Duplicates
                            if (AllPowersDataHasPower(inputData, ATName, powerSet.BaseProperties[0].PropertyValue, power.SubObjectsProperties[0].PropertyValue)) continue;

                            string powerSetDisplayName = IxQuotes(powerSet.BaseProperties[1].PropertyValue);

                            inputData = AddPowerToAllPowers(inputData, ATName, ATDisplayName, categoryIsPrimary ? 
                                nameof(AllPowersSetTypes.PRIMARY) : nameof(AllPowersSetTypes.SECONDARY),
                                categoryIsPrimary ? (int)AllPowersSetTypes.PRIMARY : (int)AllPowersSetTypes.SECONDARY,
                                IxQuotes(powerSet.BaseProperties[0].PropertyValue), AllMessages.ContainsKey(
                                powerSetDisplayName) ? AllMessages[powerSetDisplayName] : powerSetDisplayName, power.SubObjectsProperties[0].PropertyValue,
                                AllMessages.ContainsKey(PowerDisplayName) ?
                                AllMessages[PowerDisplayName] : powerSet.BaseProperties[1].PropertyValue,
                                (int.Parse(power.SubObjectsProperties[1].PropertyValue)) + 1);
                        }
                    }
                }
            }
            return true;
        }

        public static string GetFileNameInPath(string pathToSearch, string fileName)
        {
            if (File.Exists(fileName)) return fileName;

            //Find it if it doesn't exist where we expect
            string[] foundFiles = Directory.GetFiles(pathToSearch, Path.GetFileName(fileName), SearchOption.AllDirectories);
            if (foundFiles.Length == 1)
            {
                return foundFiles[0];
            }
            return "";
        }

        public static int GetOnlyNumbers(string input)
        {
            string retVal = "";
            foreach (char c in input)
            {
                if (char.IsDigit(c)) retVal += c;
            }
            return int.Parse(retVal);
        }

        public static int GetMaxDropDownWidth(ComboBox cbo)
        {
            int MaxWidth = 0;

            foreach (var Item in cbo.Items)
            {
                int i = TextRenderer.MeasureText(Item.ToString(), cbo.Font).Width + 200;

                if (i > MaxWidth)
                    MaxWidth = i;
            }

            if (MaxWidth < 100)
                MaxWidth = 100;

            return MaxWidth;
        }

    }
}
