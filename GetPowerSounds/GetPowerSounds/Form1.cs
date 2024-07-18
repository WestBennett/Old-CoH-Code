using Philotic_Knight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetPowerSounds
{
    public partial class Form1 : Form
    {

        static SortedDictionary<string, EntityType> EntityTypes = null;
        static string lineToUse = "";

        public Form1()
        {
            InitializeComponent();
        }

        public static void UpdateStatus(string TextValue)
        {
            Debug.WriteLine(TextValue);
            Application.DoEvents();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Show();

            string Folder = "";
            using (FolderBrowserDialog ofd = new FolderBrowserDialog())
            {
                ofd.Description = "Choose the directory to scan";
                if (ofd.ShowDialog() == DialogResult.Cancel || ofd.SelectedPath.ToString().Trim() == "" || !Directory.Exists(ofd.SelectedPath)) return;
                Folder = ofd.SelectedPath;
            }

            GetTextures(Folder.ToUpper() + @"\");

            //GetAllSounds(txtStatus, Folder.ToUpper() + @"\", 6);
            MessageBox.Show("Process Complete!");
        }

        private void GetTextures(string dataPath)
        {
            try
            {
                bool GetPowerTextures = true;
                DataSet ds = new DataSet();
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                Debug.WriteLine("Processing took " + String.Format("{0:00}:{1:00}:{2:00}",
                    stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds));

                //Get the list of all texture files and where they can be found at
                string textureCSV = @"c:\temp\TextureFiles.csv";
                string[] textureFiles;
                SortedDictionary<string, string> TextureFiles = new SortedDictionary<string, string>();
                //See if the Texture File list exists. If it does, then pull it, otherwise, generate it
                if (File.Exists(textureCSV))
                {
                    textureFiles = File.ReadAllLines(textureCSV);
                    foreach (string texture in textureFiles)
                    {
                        TextureFiles.Add(texture.Split(',')[0], texture.Split(',')[1]);
                    }
                }
                else
                {
                    textureFiles = Directory.GetFiles(dataPath + @"texture_library\", "*.texture", SearchOption.AllDirectories);

                    foreach (string soundFile in textureFiles)
                    {
                        if (Path.GetFileNameWithoutExtension(soundFile).Length < 3) continue; //Skip short named files to rule out those that are just a number
                        if (!TextureFiles.ContainsKey(Path.GetFileNameWithoutExtension(soundFile).ToUpper()))
                            TextureFiles.Add(Path.GetFileNameWithoutExtension(soundFile).ToUpper(),
                                Path.GetDirectoryName(soundFile).ToUpper().Replace(@"C:\SCORE\", "")
                                + @"\" + Path.GetFileNameWithoutExtension(soundFile).ToUpper());
                    }

                    //Generate the TextureFiles csv file so that we don't have to do this part again
                    using (StreamWriter sw = new StreamWriter(textureCSV))
                    {
                        foreach (KeyValuePair<string, string> kvp in TextureFiles)
                        {
                            sw.WriteLine(kvp.Key + "," + kvp.Value);
                        }
                    }
                }

                //Add the TextureFiles found to the DataSet as a DataTable
                DataTable dtIndex = new DataTable("Index");
                dtIndex.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Texture Name"),
                    new DataColumn("Texture File")
                });

                foreach (KeyValuePair<string, string> kvp in TextureFiles)
                {
                    dtIndex.Rows.Add(kvp.Key, kvp.Value);
                }
                ds.Tables.AddRange(new DataTable[] { dtIndex });

                //Add powers related textures
                if (GetPowerTextures)
                {
                    //Setup the default powers table to clone
                    DataTable dtPowers = new DataTable("Powers");
                    dtPowers.Columns.AddRange(new DataColumn[]
                    {
                        new DataColumn("Texture Name"),
                        new DataColumn("Texture File"),
                        new DataColumn("PowerSetName"),
                        new DataColumn("PowerName"),
                        new DataColumn("Type"), //FX or Icon
                        new DataColumn("Particle File"),
                        new DataColumn("FX File")
                    });

                    dtPowers.PrimaryKey = new DataColumn[] {
                        dtPowers.Columns["Texture Name"],
                        dtPowers.Columns["Texture File"],
                        dtPowers.Columns["PowerSetName"],
                        dtPowers.Columns["PowerName"],
                        dtPowers.Columns["Type"],
                        dtPowers.Columns["Particle File"],
                        dtPowers.Columns["FX File"],
                    };

                    DataTable dtIcons = dtPowers.Clone();
                    dtIcons.TableName = "PowerIcons";

                    DataTable dtPrimary = dtPowers.Clone();
                    dtPrimary.TableName = "Player Powers";

                    DataTable dtPool = dtPowers.Clone();
                    dtPool.TableName = "Pool Powers";

                    DataTable dtEpic = dtPowers.Clone();
                    dtEpic.TableName = "Epic";

                    DataTable dtIncarnate = dtPowers.Clone();
                    dtIncarnate.TableName = "Incarnate";

                    DataTable dtTemporary = dtPowers.Clone();
                    dtTemporary.TableName = "Temporary Powers";

                    DataTable dtEnhancements = dtPowers.Clone();
                    dtEnhancements.TableName = "Enhancements";

                    DataTable dtMisc = dtPowers.Clone();
                    dtMisc.TableName = "Misc";

                    Dictionary<string, string> foundMissingFiles = new Dictionary<string, string>();

                    // Check primary location (powers)
                    string[] categoryList = Directory.GetFiles(dataPath + @"Defs\powers\", "*.categories");
                    int categoryNumber = 0;
                    foreach (string categoryFile in categoryList)
                    {
                        categoryNumber++;
                        List<DefObject> categories = DefMethods.GetDefObjectsFromDefFile(categoryFile);

                        // Enumerate through all of the found categories
                        foreach (DefObject category in categories)
                        {
                            foreach (DefProperty dp in category.BaseProperties)
                            {
                                if (dp.PropertyName != "PowerSets") continue;

                                string[] PowerSetNames = dp.PropertyValue.ToUpper().Split(',');
                                string LastPowerSetFileName = "";
                                string fileName = "";
                                foreach (string PowerSetName in PowerSetNames)
                                {
                                    string PowerSetFileName = PowerSetName.Split('.')[0].Trim();
                                    //Skip enhancements 
                                    if (PowerSetFileName == "Boosts" || PowerSetFileName == "Items_Of_Power" || PowerSetFileName == "Base_Traps") continue;
                                    if (LastPowerSetFileName != PowerSetFileName)
                                    {
                                        LastPowerSetFileName = PowerSetFileName;
                                        fileName = dataPath + @"Defs\powers\" + PowerSetFileName + ".powersets";
                                    }
                                    if (!File.Exists(fileName)) throw new Exception("Cannot find file for PowerSet '" + PowerSetFileName + "'");
                                    List<DefObject> powerSets = DefMethods.GetDefObjectsFromDefFile(fileName);
                                    foreach (DefObject powerSet in powerSets)
                                    {
                                        //Get the PowerSet name info
                                        if (PowerSetName != powerSet.ObjectName.ToUpper()) continue;
                                        UpdateStatus("Adding info for powerset '" + powerSet.ObjectName + "'");

                                        foreach (DefSubObject power in powerSet.Attributes)
                                        {
                                            //Find the correct Power file
                                            fileName = dataPath + @"Defs\powers\" + powerSet.ObjectName.Replace('.', '_') + ".powers";
                                            if (!File.Exists(fileName))
                                            {
                                                fileName = fileName.Replace("_Aux", "");
                                                if (!File.Exists(fileName))
                                                {
                                                    continue;
                                                }
                                            }
                                            List<DefObject> PowersObjects = DefMethods.GetDefObjectsFromDefFile(fileName);

                                            foreach (DefObject powerObject in PowersObjects)
                                            {
                                                string PowerName = "";

                                                foreach (DefProperty dpPO in powerObject.BaseProperties)
                                                {
                                                    if (dpPO.PropertyName == "Name")
                                                    {
                                                        PowerName = DefMethods.IxQuotes(dpPO.PropertyValue).ToUpper();
                                                        continue;
                                                    }
                                                    else if (dpPO.PropertyName == "IconName")
                                                    {
                                                        //Get the name of the icon, minus the extension, and check against our list
                                                        string IconName = Path.GetFileNameWithoutExtension(dpPO.PropertyValue.Split('"')[1].ToUpper());
                                                        if (!TextureFiles.ContainsKey(IconName))
                                                        {
                                                            string MissingRefFile = @"c:\temp\MissingReferences.txt";
                                                            string message = "Invalid icon reference '" + IconName +
                                                                "' found in powers file '" + fileName + "'.";
                                                            string MissingReferences = File.Exists(MissingRefFile) ? File.ReadAllText(MissingRefFile) : "";

                                                            if (!MissingReferences.Contains(message))
                                                            {
                                                                using (StreamWriter sw = new StreamWriter(MissingRefFile, true))
                                                                {
                                                                    sw.WriteLine(message);
                                                                    Debug.WriteLine("Failed to find texture '" + IconName + "'");
                                                                    continue;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            UpdateStatus("Adding Power Icon Texture '" + IconName +
                                                                "' for Power " + PowerSetName + "." + PowerName);

                                                            //Add the icon for the power to our datatable
                                                            AddPowersRow(categoryNumber, categoryList.Length, dtIcons, IconName, TextureFiles[IconName], PowerSetName, PowerName, "Icon", "", "");
                                                        }
                                                    }
                                                    else if (dpPO.PropertyName == "VisualFX")
                                                    {
                                                        //Get the Visual FX file and work from there

                                                        //Get the PFX File
                                                        string PFXFile = Controller.GetFileNameInPath(dataPath,
                                                            (dataPath + DefMethods.IxQuotes(dpPO.PropertyValue)));
                                                        foreach (string line in File.ReadAllLines(PFXFile))
                                                        {
                                                            string actualLine = line.Trim().ToUpper();
                                                            if (actualLine == "" || actualLine.StartsWith("//") || !actualLine.Contains(".FX")) continue;

                                                            string[] elements = actualLine.Split('\t');
                                                            string FXType = "";
                                                            string FXFileName = "";

                                                            foreach (string element in elements)
                                                            {
                                                                if (element.Trim() == "") continue;
                                                                if (FXType == "")
                                                                {
                                                                    FXType = element.Trim().ToUpper();
                                                                    continue;
                                                                }
                                                                else if (FXFileName == "")
                                                                {
                                                                    FXFileName = DefMethods.IxQuotes(element.Trim());
                                                                    break;
                                                                }
                                                            }

                                                            FXFileName = FXFileName.ToUpper();
                                                            //Open up the FX file and get the sound info, if there is any
                                                            if (!File.Exists(dataPath + @"FX\" + FXFileName)) continue;

                                                            foreach (string fxLine in File.ReadAllLines(dataPath + @"FX\" + FXFileName))
                                                            {
                                                                string fxActualLine = fxLine.Trim().ToUpper();
                                                                //Only process the Particle lines
                                                                if (!fxActualLine.StartsWith("PART")) continue;

                                                                string[] partElements = fxActualLine.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                                                                string particleFilePath = dataPath + @"FX\" +
                                                                    partElements[1].Replace("/", @"\").Replace(":", "");

                                                                //Find the particle file, if we can't throw an error
                                                                if (!File.Exists(particleFilePath))
                                                                {
                                                                    //Try adding in the path of the FX file to the beginning of the reference
                                                                    particleFilePath = Path.GetDirectoryName(dataPath + @"FX\" + FXFileName) +
                                                                        @"\" + partElements[1].Replace("/", @"\").Replace(":", "").Replace(
                                                                            @"FX\", "");

                                                                    if (!File.Exists(particleFilePath))
                                                                    {
                                                                        particleFilePath = Path.GetFileName(dataPath + @"FX\" +
                                                                                partElements[1].Replace("/", @"\").Replace(":", ""));

                                                                        if (foundMissingFiles.ContainsKey(particleFilePath))
                                                                        {
                                                                            particleFilePath = foundMissingFiles[particleFilePath];
                                                                        }
                                                                        else
                                                                        {
                                                                            UpdateStatus("Searching for missing part file '" + particleFilePath + "'");
                                                                            //Final try, see if we can search for it in the data directory
                                                                            string[] foundFiles = Directory.GetFiles(
                                                                                dataPath, Path.GetFileName(particleFilePath), SearchOption.AllDirectories);
                                                                            if (foundFiles.Length == 0)
                                                                            {
                                                                                string MissingRefFile = @"c:\temp\MissingReferences.txt";
                                                                                string message = "Invalid particle file reference '" + particleFilePath +
                                                                                    "' found in FX file '" + dataPath + @"FX\" + FXFileName + "'.";
                                                                                string MissingReferences = File.Exists(MissingRefFile) ? File.ReadAllText(MissingRefFile) : "";

                                                                                if (!MissingReferences.Contains(message))
                                                                                {
                                                                                    using (StreamWriter sw = new StreamWriter(MissingRefFile, true))
                                                                                    {
                                                                                        sw.WriteLine(message);
                                                                                        Debug.WriteLine("Failed to find texture '" + particleFilePath + "'");
                                                                                        continue;
                                                                                    }
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                //Just take the first one found
                                                                                particleFilePath = foundFiles[0];
                                                                                foundMissingFiles.Add(Path.GetFileName(foundFiles[0].ToUpper()), foundFiles[0].ToUpper());
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                if (!File.Exists(particleFilePath))
                                                                {
                                                                    string MissingRefFile = @"c:\temp\MissingReferences.txt";
                                                                    string message = "Invalid particle file reference '" + particleFilePath +
                                                                        "' found in FX file '" + dataPath + @"FX\" + FXFileName + "'.";
                                                                    string MissingReferences = File.Exists(MissingRefFile) ? File.ReadAllText(MissingRefFile) : "";

                                                                    if (!MissingReferences.Contains(message))
                                                                    {
                                                                        using (StreamWriter sw = new StreamWriter(MissingRefFile, true))
                                                                        {
                                                                            sw.WriteLine(message);
                                                                            Debug.WriteLine("Failed to find texture '" + particleFilePath + "'");
                                                                            continue;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                    //Finally, open the particle file, to get the texture reference
                                                                    foreach (string particleLine in File.ReadAllLines(particleFilePath))
                                                                    {
                                                                        string pLine = particleLine.Trim().ToUpper();
                                                                        if (!pLine.StartsWith("TEXTURENAME")) continue;

                                                                        string textureName = Path.GetFileNameWithoutExtension(
                                                                            particleLine.Split((char[])null,
                                                                            StringSplitOptions.RemoveEmptyEntries)[1]).ToUpper();

                                                                        if (textureName.Contains("."))
                                                                        {
                                                                            string MissingRefFile = @"c:\temp\MissingReferences.txt";
                                                                            string message = "Invalid file reference '" + particleLine.Split((char[])null,
                                                                            StringSplitOptions.RemoveEmptyEntries)[1].ToUpper() + "' found in particle file '" + particleFilePath + "'.";
                                                                            string MissingReferences = File.Exists(MissingRefFile) ? File.ReadAllText(MissingRefFile) : "";

                                                                            if (!MissingReferences.Contains(message))
                                                                            {
                                                                                using (StreamWriter sw = new StreamWriter(MissingRefFile, true))
                                                                                {
                                                                                    sw.WriteLine(message);
                                                                                    Debug.WriteLine("Failed to find texture '" + textureName + "'");
                                                                                    continue;
                                                                                }
                                                                            }
                                                                        }

                                                                        //Get the name of the texture, and check against our list
                                                                        if (!TextureFiles.ContainsKey(textureName))
                                                                        {
                                                                            if (textureName != "0")
                                                                            {
                                                                                string MissingRefFile = @"c:\temp\MissingReferences.txt";
                                                                                string message = "Failed to find '" + textureName + "' for particle file '" + particleFilePath + "'.";
                                                                                string MissingReferences = File.Exists(MissingRefFile) ? File.ReadAllText(MissingRefFile) : "";

                                                                                if (!MissingReferences.Contains(message))
                                                                                {
                                                                                    using (StreamWriter sw = new StreamWriter(MissingRefFile, true))
                                                                                    {
                                                                                        sw.WriteLine(message);
                                                                                        Debug.WriteLine("Failed to find texture '" + textureName + "'");
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            UpdateStatus("Adding Power FX Texture '" + textureName +
                                                                                "' for Power " + PowerSetName + "." + PowerName);

                                                                            if (IsPlayerPower(PowerSetName))
                                                                            {
                                                                                //Add the icon for the power to our datatable
                                                                                AddPowersRow(categoryNumber, categoryList.Length, dtPrimary, textureName, TextureFiles[textureName],
                                                                                    PowerSetName, PowerName, "FX", particleFilePath.Replace(dataPath, ""),
                                                                                    FXFileName.Replace(dataPath, ""));
                                                                            }
                                                                            else if (PowerSetName.ToUpper().StartsWith("POOL."))
                                                                            {
                                                                                AddPowersRow(categoryNumber, categoryList.Length, dtPool, textureName, TextureFiles[textureName],
                                                                                    PowerSetName, PowerName, "FX", particleFilePath.Replace(dataPath, ""),
                                                                                    FXFileName.Replace(dataPath, ""));
                                                                            }
                                                                            else if (PowerSetName.ToUpper().StartsWith("EPIC."))
                                                                            {
                                                                                AddPowersRow(categoryNumber, categoryList.Length, dtEpic, textureName, TextureFiles[textureName],
                                                                                    PowerSetName, PowerName, "FX", particleFilePath.Replace(dataPath, ""),
                                                                                    FXFileName.Replace(dataPath, ""));
                                                                            }
                                                                            else if (PowerSetName.ToUpper().StartsWith("INCARNATE."))
                                                                            {
                                                                                AddPowersRow(categoryNumber, categoryList.Length, dtIncarnate, textureName, TextureFiles[textureName],
                                                                                    PowerSetName, PowerName, "FX", particleFilePath.Replace(dataPath, ""),
                                                                                    FXFileName.Replace(dataPath, ""));
                                                                            }
                                                                            else if (PowerSetName.ToUpper().StartsWith("TEMPORARY_POWERS."))
                                                                            {
                                                                                AddPowersRow(categoryNumber, categoryList.Length, dtTemporary, textureName, TextureFiles[textureName],
                                                                                    PowerSetName, PowerName, "FX", particleFilePath.Replace(dataPath, ""),
                                                                                    FXFileName.Replace(dataPath, ""));
                                                                            }
                                                                            else if (PowerSetName.ToUpper().StartsWith("BOOSTS."))
                                                                            {
                                                                                AddPowersRow(categoryNumber, categoryList.Length, dtEnhancements, textureName, TextureFiles[textureName],
                                                                                    PowerSetName, PowerName, "FX", particleFilePath.Replace(dataPath, ""),
                                                                                    FXFileName.Replace(dataPath, ""));
                                                                            }
                                                                            else
                                                                            {
                                                                                //Add the icon for the power to our datatable
                                                                                AddPowersRow(categoryNumber, categoryList.Length, dtMisc, textureName, TextureFiles[textureName],
                                                                                    PowerSetName, PowerName, "FX", particleFilePath.Replace(dataPath, ""),
                                                                                    FXFileName.Replace(dataPath, ""));
                                                                            }
                                                                        }
                                                                    }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    ds.Tables.AddRange(new DataTable[] {
                        dtIcons, dtPrimary, dtPool, dtEpic, dtIncarnate, dtTemporary, dtEnhancements, dtMisc
                    });
                }

                //Add player costume/avatar related textures


                //Add NPC related textures


                //Add world FX related textures
                stopWatch.Stop();
                Debug.WriteLine("Processing took " + String.Format("{0:00}:{1:00}:{2:00}",
                        stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds));
                DefMethods.WriteDataSetToExcelFile(ds, @"c:\temp\VisualRosettaStone_v" + 2 + ".xlsx");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool IsPlayerPower(string powerSetName)
        {
            try
            {
                List<string> PlayerPowersets = new List<string>()
                {
                    "Blaster_Ranged".ToUpper(),
                    "Blaster_Support".ToUpper(),
                    "Brute_Defense".ToUpper(),
                    "Brute_Melee".ToUpper(),
                    "Controller_Buff".ToUpper(),
                    "Controller_Control".ToUpper(),
                    "Corruptor_Buff".ToUpper(),
                    "Corruptor_Ranged".ToUpper(),
                    "Defender_Buff".ToUpper(),
                    "Defender_Ranged".ToUpper(),
                    "Dominator_Assault".ToUpper(),
                    "Dominator_Control".ToUpper(),
                    "Mastermind_Buff".ToUpper(),
                    "Mastermind_Summon".ToUpper(),
                    "Scrapper_Defense".ToUpper(),
                    "Scrapper_Melee".ToUpper(),
                    "Sentinel_Defense".ToUpper(),
                    "Sentinel_Ranged".ToUpper(),
                    "Stalker_Defense".ToUpper(),
                    "Stalker_Melee".ToUpper(),
                    "Tanker_Defense".ToUpper(),
                    "Tanker_Melee".ToUpper(),
                    "Feral_Might".ToUpper(),
                    "Primalist_Misc".ToUpper(),
                    "Peacebringer_Offensive".ToUpper(),
                    "Peacebringer_Defensive".ToUpper(),
                    "Warshade_Offensive".ToUpper(),
                    "Warshade_Defensive".ToUpper(),
                    "Widow_Training".ToUpper(),
                    "Teamwork".ToUpper(),
                    "Arachnos_Soldiers".ToUpper(),
                    "Training_Gadgets".ToUpper()
                };
                if (powerSetName.Contains(".")) powerSetName = powerSetName.Split('.')[0];
                if (PlayerPowersets.Contains(powerSetName.ToUpper()))
                {
                    return true;
                }
                return false;
            }
            catch   (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        private void AddPowersRow(int CurrentNumber, int TotalCount, DataTable dt, string textureName, string textureFile, string powerSetName, string powerName,
            string Type, string particleFile, string FXFile)
        {
            //            new DataColumn("Texture Name"),
            //            new DataColumn("Texture File"),
            //            new DataColumn("PowerSetName"),
            //            new DataColumn("PowerName"),
            //            new DataColumn("Type"), //FX or Icon
            //            new DataColumn("Particle File"),
            //            new DataColumn("FX File")
            try
            {
                Debug.WriteLine("Category " + CurrentNumber + " of " + TotalCount);
                DataRow found = dt.Rows.Find(new object[] { textureName, textureFile, powerSetName, powerName, Type, particleFile, FXFile });

                if (found != null) return;
                dt.Rows.Add(textureName, textureFile, powerSetName, powerName, Type, particleFile, FXFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        public static bool GetAllSounds(TextBox txtStatus, string dataPath, int version_number)
        {
            try
            {
                bool GetPowerSounds = false;
                bool GetEntitySequences = false;
                bool GetMusic = false;
                bool GetMiscFX = true;

                Dictionary<string, string> SoundFiles = new Dictionary<string, string>();
                string[] soundFiles = Directory.GetFiles(dataPath + @"sound\", "*.ogg", SearchOption.AllDirectories);
                foreach (string soundFile in soundFiles)
                {
                    if (!SoundFiles.ContainsKey(Path.GetFileNameWithoutExtension(soundFile).ToUpper()))
                        SoundFiles.Add(Path.GetFileNameWithoutExtension(soundFile).ToUpper(), Path.GetDirectoryName(soundFile).ToUpper().Replace(@"E:\SCORE\", "")
                            + @"\" + Path.GetFileNameWithoutExtension(soundFile).ToUpper());
                }

                DataTable dt = new DataTable("Powers-Based Sounds");
                dt.Columns.AddRange(new DataColumn[] {
                    new DataColumn("PowerSetName"),
                    new DataColumn("PowerName"),
                    new DataColumn("FX_Type"),
                    new DataColumn("FX_FileName"),
                    new DataColumn("SoundFileName")
                });

                if (GetPowerSounds)
                {
                    // Check primary location (powers)
                    foreach (string categoryFile in Directory.GetFiles(dataPath + @"Defs\powers\", "*.categories"))
                    {
                        List<DefObject> categories = DefMethods.GetDefObjectsFromDefFile(categoryFile);

                        // Enumerate through all of the found categories
                        foreach (DefObject category in categories)
                        {
                            foreach (DefProperty dp in category.BaseProperties)
                            {
                                if (dp.PropertyName != "PowerSets") continue;

                                string[] PowerSetNames = dp.PropertyValue.ToUpper().Split(',');
                                string LastPowerSetFileName = "";
                                string fileName = "";
                                foreach (string PowerSetName in PowerSetNames)
                                {
                                    string PowerSetFileName = PowerSetName.Split('.')[0].Trim();
                                    //Skip enhancements 
                                    if (PowerSetFileName == "Boosts" || PowerSetFileName == "Items_Of_Power" || PowerSetFileName == "Base_Traps") continue;
                                    if (LastPowerSetFileName != PowerSetFileName)
                                    {
                                        LastPowerSetFileName = PowerSetFileName;
                                        fileName = dataPath + @"Defs\powers\" + PowerSetFileName + ".powersets";
                                    }
                                    if (!File.Exists(fileName)) throw new Exception("Cannot find file for PowerSet '" + PowerSetFileName + "'");
                                    List<DefObject> powerSets = DefMethods.GetDefObjectsFromDefFile(fileName);
                                    foreach (DefObject powerSet in powerSets)
                                    {
                                        //Get the PowerSet name info
                                        if (PowerSetName != powerSet.ObjectName.ToUpper()) continue;
                                        UpdateStatus("Adding info for powerset '" + powerSet.ObjectName + "'");

                                        foreach (DefSubObject power in powerSet.Attributes)
                                        {
                                            //Find the correct Power file
                                            fileName = dataPath + @"Defs\powers\" + powerSet.ObjectName.Replace('.', '_') + ".powers";
                                            if (!File.Exists(fileName))
                                            {
                                                fileName = fileName.Replace("_Aux", "");
                                                if (!File.Exists(fileName))
                                                {
                                                    continue;
                                                }
                                            }
                                            List<DefObject> PowersObjects = DefMethods.GetDefObjectsFromDefFile(fileName);

                                            foreach (DefObject powerObject in PowersObjects)
                                            {
                                                string PowerName = "";

                                                foreach (DefProperty dpPO in powerObject.BaseProperties)
                                                {
                                                    if (dpPO.PropertyName == "Name")
                                                    {
                                                        PowerName = DefMethods.IxQuotes(dpPO.PropertyValue).ToUpper();
                                                        continue;
                                                    }
                                                    else if (dpPO.PropertyName == "VisualFX")
                                                    {
                                                        //Get the Visual FX file and work from there

                                                        //Get the PFX File
                                                        string PFXFile = Controller.GetFileNameInPath(dataPath,
                                                            (dataPath + DefMethods.IxQuotes(dpPO.PropertyValue)));
                                                        foreach (string line in File.ReadAllLines(PFXFile))
                                                        {
                                                            string actualLine = line.Trim().ToUpper();
                                                            if (actualLine == "" || actualLine.StartsWith("//") || !actualLine.Contains(".FX")) continue;

                                                            string[] elements = actualLine.Split('\t');
                                                            string FXType = "";
                                                            string FXFileName = "";

                                                            foreach (string element in elements)
                                                            {
                                                                if (element.Trim() == "") continue;
                                                                if (FXType == "")
                                                                {
                                                                    FXType = element.Trim().ToUpper();
                                                                    continue;
                                                                }
                                                                else if (FXFileName == "")
                                                                {
                                                                    FXFileName = DefMethods.IxQuotes(element.Trim());
                                                                    break;
                                                                }
                                                            }

                                                            FXFileName = FXFileName.ToUpper();
                                                            //Open up the FX file and get the sound info, if there is any
                                                            if (!File.Exists(dataPath + @"FX\" + FXFileName)) continue;

                                                            //Check if this item exists already, if it doesn't, add it
                                                            DataRow[] drs = dt.Select("PowerSetName = '" + PowerSetName + "' AND " +
                                                                "PowerName = '" + PowerName + "' AND " +
                                                                "FX_Type = '" + FXType + "' AND " +
                                                                "FX_FileName = '" + FXFileName + "'"
                                                                );
                                                            if (drs.Length > 0) continue; //Already added this effect to this power, skip it

                                                            foreach (string fxLine in File.ReadAllLines(dataPath + @"FX\" + FXFileName))
                                                            {
                                                                string fxActualLine = fxLine.Trim().ToUpper();
                                                                //Ignore common false positives
                                                                if (!fxActualLine.StartsWith("SOUND") ||
                                                                    fxActualLine.Contains("BHVR") ||
                                                                    fxActualLine.StartsWith("SOUNDNOREPEAT")) continue;

                                                                string[] soundelements = fxActualLine.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                                                                if (soundelements[0] != "SOUND")
                                                                {
                                                                    UpdateStatus("Failed to parse line '" + fxActualLine + "' in file '" +
                                                                        FXFileName + "'");
                                                                    continue;
                                                                }

                                                                string soundFileNameAndPath = soundelements[1];
                                                                if (SoundFiles.ContainsKey(soundFileNameAndPath))
                                                                {
                                                                    soundFileNameAndPath = SoundFiles[soundFileNameAndPath].Replace(dataPath, "");
                                                                }

                                                                //Check if this item exists already, if it doesn't, add it
                                                                DataRow[] foundSounds = dt.Select("PowerSetName = '" + PowerSetName + "' AND " +
                                                                    "PowerName = '" + PowerName + "' AND " +
                                                                    "FX_Type = '" + FXType + "' AND " +
                                                                    "SoundFileName = '" + soundFileNameAndPath + "'"
                                                                    );
                                                                if (foundSounds.Length > 0) continue; //Already added this effect to this power, skip it

                                                                UpdateStatus("Adding Power Sound '" + soundFileNameAndPath + "' for Power " + PowerSetName + "." + PowerName);
                                                                dt.Rows.Add(PowerSetName, PowerName, FXType, FXFileName, soundFileNameAndPath);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                DataTable dt2 = dt.Clone();
                dt2.Columns["PowerSetName"].ColumnName = "EntityType";
                dt2.Columns["PowerName"].ColumnName = "FXName";
                if (dt2.Columns.Contains("FX_Type")) dt2.Columns.Remove(dt2.Columns["FX_Type"]);
                dt2.TableName = "Entity Sequence Based Sounds";
                //Add all of the FX from the AI sequencers - start at the entity type files

                //Get a list of all SequencFile.ReadAllLines(sequenceFile)es and their related Entity Types
                if (GetEntitySequences)
                {
                    Dictionary<string, string[]> SequenceLines = new Dictionary<string, string[]>();
                    string[] files = Directory.GetFiles(dataPath + @"sequencers\", "*.*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        string fileName = file.ToUpper();
                        SequenceLines.Add(fileName, File.ReadAllLines(fileName));
                    }

                    Dictionary<Tuple<string, string>, string> TypeSequences = GetTypeSequences(dataPath, SequenceLines);

                    //Go through all of the sequence files and parse them
                    int fileNum = 0;
                    foreach (KeyValuePair<string, string[]> kvp in SequenceLines)
                    {
                        string sequenceFileToUse = kvp.Key.ToString().ToUpper();
                        fileNum++;
                        ParseSequenceFile(fileNum, SequenceLines.Count, txtStatus, sequenceFileToUse, dt2, TypeSequences, dataPath, SoundFiles);
                    }
                }

                DataTable dt3 = new DataTable("Music, Scripts, and Loops");
                dt3.Columns.AddRange(new DataColumn[] {
                    new DataColumn("Scene"),
                    new DataColumn("AudioGroup"),
                    new DataColumn("MapFile"),
                    new DataColumn("SoundFileName")
                });

                if (GetMusic)
                {
                    //Find all of the files with a Scenefile reference, ignore the rest
                    Dictionary<string, string[]> MapLines = new Dictionary<string, string[]>();
                    string[] files = Directory.GetFiles(dataPath + @"maps\", "*.*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        string fileName = file.ToUpper();
                        UpdateStatus("Checking file '" + fileName + "' for Scene");
                        string[] fileLines = File.ReadAllLines(fileName);
                        bool FoundScenefile = false;
                        foreach (string line in fileLines)
                        {
                            string lineToCheck = line.ToUpper().Trim();
                            if (lineToCheck.StartsWith("SCENEFILE"))
                            {
                                FoundScenefile = true;
                                break;
                            }
                        }
                        if (!FoundScenefile) continue;

                        MapLines.Add(fileName, fileLines);
                    }


                    //Find all of the Import files used by that file
                    foreach (KeyValuePair<string, string[]> MapFile in MapLines)
                    {
                        string Scene = "";

                        //Read the file, and add all imports
                        foreach (string line in MapFile.Value)
                        {
                            string lineToCheck = line.ToUpper().Trim();
                            if (lineToCheck.StartsWith("SCENEFILE"))
                            {
                                Scene = Path.GetFileNameWithoutExtension(lineToCheck.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1]);
                                UpdateStatus("File '" + MapFile.Key + "': set Scene '" + Scene + "'");
                                continue;
                            }
                            else if (lineToCheck.StartsWith("IMPORT"))
                            {
                                string FullMapFile = dataPath + lineToCheck.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1].Replace("/", @"\").
                                    Replace(dataPath, "").Replace(@"C:\GAME\DATA\", "").Replace(@"C:\GAMEFIX\DATA\", "");
                                UpdateStatus("Checking Import '" + FullMapFile + "'");
                                string MapFileName = FullMapFile.Replace(dataPath, "");
                                if (!File.Exists(FullMapFile)) continue;

                                string GroupName = "";
                                string SoundFile = "";

                                foreach (string ImportLine in File.ReadAllLines(FullMapFile))
                                {
                                    string ImportLineToCheck = ImportLine.ToUpper().Trim();
                                    if (ImportLineToCheck.StartsWith("DEF"))
                                    {
                                        if (GroupName != "" && SoundFile != "")
                                        {
                                            //Check if this item exists already, if it doesn't, add it
                                            DataRow[] foundSounds = dt3.Select("Scene = '" + Scene + "' AND " +
                                                "AudioGroup = '" + GroupName + "' AND " +
                                                "MapFile = '" + MapFileName + "' AND " +
                                                "SoundFileName = '" + SoundFile + "'"
                                                );
                                            if (foundSounds.Length > 0) continue; //Already added this effect to this power, skip it

                                            UpdateStatus(Path.GetFileName(MapFile.Key) + ":Adding Sound - " + Scene + " " + GroupName + " " + MapFileName + " " + SoundFile);
                                            dt3.Rows.Add(Scene, GroupName, MapFileName, SoundFile);
                                            GroupName = "";
                                            SoundFile = "";
                                        }

                                        GroupName = ImportLineToCheck.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1].Replace("'", "");
                                    }
                                    else if (ImportLineToCheck.StartsWith("SOUND"))
                                    {
                                        if (ImportLineToCheck.StartsWith("SOUNDSCRIPT"))
                                        {
                                            //Process the sound script and add every sound
                                            string[] ScriptFiles = Directory.GetFiles(dataPath + @"sound\Scripts\",
                                                ImportLineToCheck.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1]);
                                            if (ScriptFiles.Length == 0) continue;
                                            foreach (string scriptFile in ScriptFiles)
                                            {
                                                string currentGroupName = "";
                                                foreach (string scriptFileLine in File.ReadAllLines(scriptFile))
                                                {
                                                    string scriptFileLineToUse = scriptFileLine.ToUpper().Trim();
                                                    if (scriptFileLineToUse.StartsWith("SOUND"))
                                                    {
                                                        currentGroupName = GroupName + "." + scriptFileLineToUse.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1];
                                                    }
                                                    else if (scriptFileLineToUse.StartsWith("NAME"))
                                                    {
                                                        SoundFile = scriptFileLineToUse.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1];
                                                        if (SoundFiles.ContainsKey(SoundFile)) SoundFile = SoundFiles[SoundFile].Replace(dataPath, "");

                                                        //Immediately add this, as another file could be listed next in the script sequence
                                                        //Check if this item exists already, if it doesn't, add it
                                                        DataRow[] foundSounds = dt3.Select("Scene = '" + Scene + "' AND " +
                                                            "AudioGroup = '" + currentGroupName + "' AND " +
                                                            "MapFile = '" + MapFileName + "' AND " +
                                                            "SoundFileName = '" + SoundFile + "'"
                                                            );
                                                        if (foundSounds.Length > 0) continue; //Already added this effect to this power, skip it

                                                        UpdateStatus(Path.GetFileName(MapFile.Key) + ":Adding Sound - " + Scene + " " + currentGroupName + " " + MapFileName + " " + SoundFile);
                                                        dt3.Rows.Add(Scene, currentGroupName, MapFileName, SoundFile);
                                                        SoundFile = "";

                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            SoundFile = ImportLineToCheck.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1];
                                            if (SoundFiles.ContainsKey(SoundFile)) SoundFile = SoundFiles[SoundFile].Replace(dataPath, "");
                                        }
                                    }
                                    else if (ImportLineToCheck.StartsWith("END") && GroupName != "" && SoundFile != "")
                                    {
                                        //Check if this item exists already, if it doesn't, add it
                                        DataRow[] foundSounds = dt3.Select("Scene = '" + Scene + "' AND " +
                                            "AudioGroup = '" + GroupName + "' AND " +
                                            "MapFile = '" + MapFileName + "' AND " +
                                            "SoundFileName = '" + SoundFile + "'"
                                            );
                                        if (foundSounds.Length > 0) continue; //Already added this effect to this power, skip it

                                        UpdateStatus(Path.GetFileName(MapFile.Key) + ":Adding Sound - " + Scene + " " + GroupName + " " + MapFileName + " " + SoundFile);
                                        dt3.Rows.Add(Scene, GroupName, MapFileName, SoundFile);
                                        GroupName = "";
                                        SoundFile = "";
                                    }
                                }
                            }
                        }
                    }
                }

                DataTable dt4 = new DataTable("FX Sound Index");
                dt4.Columns.AddRange(new DataColumn[] {
                    new DataColumn("FX_FileName"),
                    new DataColumn("SoundFileName"),
                });

                if (GetMiscFX)
                {
                    Dictionary<string, string[]> FXLines = new Dictionary<string, string[]>();
                    string[] files = Directory.GetFiles(dataPath + @"fx\", "*.fx", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        string fileName = file.ToUpper();
                        FXLines.Add(fileName, File.ReadAllLines(fileName));
                    }

                    foreach (KeyValuePair<string, string[]> kvp in FXLines)
                    {
                        UpdateStatus("Processing Misc FX File '" + kvp.Key + "'");
                        foreach (string FXLine in kvp.Value)
                        {
                            string line = FXLine.ToUpper().Trim();
                            if (!line.StartsWith("SOUND")) continue;
                            string[] elements = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                            if (elements[0] != "SOUND") continue;

                            string soundFileNameAndPath = elements[1];
                            if (SoundFiles.ContainsKey(soundFileNameAndPath))
                            {
                                soundFileNameAndPath = SoundFiles[soundFileNameAndPath].Replace(dataPath, "");
                            }

                            string FX_FileName = kvp.Key.ToUpper().Replace(dataPath, "");
                            //Check if this item exists already, if it doesn't, add it
                            DataRow[] foundSounds = dt4.Select("FX_FileName = '" + FX_FileName + "' AND " +
                                "SoundFileName = '" + soundFileNameAndPath + "'"
                                );
                            if (foundSounds.Length > 0) continue; //Already added this effect to this power, skip it

                            UpdateStatus(Path.GetFileName(kvp.Key) + ":Adding Sound - " + FX_FileName + " " + soundFileNameAndPath);
                            dt4.Rows.Add(FX_FileName, soundFileNameAndPath);
                        }
                    }
                }

                DataSet ds = new DataSet();
                if (GetPowerSounds) ds.Tables.Add(dt);
                if (GetEntitySequences) ds.Tables.Add(dt2);
                if (GetMusic) ds.Tables.Add(dt3);
                if (GetMiscFX) ds.Tables.Add(dt4);
                DefMethods.WriteDataSetToExcelFile(ds, @"c:\temp\RosettaStone_v" + version_number + ".xlsx");

            }
            catch (Exception ex)
            {
                UpdateStatus(ex.ToString());
            }
            return true;
        }

        private static void ParseSequenceFile(int FileNum, int TotFiles, TextBox txtStatus, string sequenceFile, DataTable dt2, Dictionary<Tuple<string, string>, string> TypeSequences,
            string dataPath, Dictionary<string, string> soundFiles, string MoveName = "")
        {
            try
            {
                if (!File.Exists(sequenceFile)) return;
                string TypeName = "";
                int LineNum = 0;
                string[] lines = File.ReadAllLines(sequenceFile);
                foreach (string sequenceLine in lines)
                {
                    LineNum++;
                    UpdateStatus("Parsing Sequence File #" + sequenceFile);
                    string sequenceLineToUse = sequenceLine.Trim().ToUpper();
                    if (sequenceLineToUse == "" || sequenceLineToUse.StartsWith("#") || sequenceLineToUse.StartsWith("//")) continue;
                    string[] sequencerFileElements = sequenceLineToUse.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (sequencerFileElements.Length == 0) continue;
                    //If the line is an Include line, then process it recursively
                    if (sequencerFileElements[0] == "INCLUDE")
                    {
                        ParseSequenceFile(FileNum, TotFiles, txtStatus, dataPath + @"\" + sequencerFileElements[1], dt2, TypeSequences, dataPath, soundFiles, MoveName);
                    }
                    else if (sequencerFileElements[0] == "MOVE")
                    {
                        //If the line is a Move line, then store the Move name
                        MoveName = sequencerFileElements[1];
                        continue;
                    }
                    else if (sequencerFileElements[0] == "TYPE")
                    {
                        TypeName = sequencerFileElements[1];
                        continue;
                    }
                    else if (sequencerFileElements[0].ToUpper().EndsWith("FX"))
                    {

                        //Skip and report if we have an unknonwn type
                        if (!TypeSequences.ContainsKey(new Tuple<string, string>(TypeName, sequenceFile)))
                        {
                            UpdateStatus("Invalid Type '" + TypeName + "' for file '" + sequenceFile + "'");
                            continue;
                        }

                        //Data fixing for bad data
                        if (sequencerFileElements[1].Contains(":")) sequencerFileElements[1] = sequencerFileElements[1].Split(':')[0];
                        if (sequencerFileElements[1].Contains(@"""")) sequencerFileElements[1] = sequencerFileElements[1].Replace(@"""", "");
                        string fxFile = dataPath + @"FX\" + sequencerFileElements[1];
                        if (!File.Exists(fxFile)) continue;

                        //Within the FX file, find the sound file reference
                        foreach (string FXLine in File.ReadAllLines(fxFile))
                        {
                            string fxLineToUse = FXLine.Trim().ToUpper();
                            if (fxLineToUse == "" || fxLineToUse.StartsWith("#") || fxLineToUse.StartsWith("//")) continue;

                            //Get the latest "Type" if there is one
                            string[] soundFileElements = fxLineToUse.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                            if (soundFileElements.Length < 2) continue;

                            if (soundFileElements[0] != "SOUND") continue;

                            string EntityType = TypeName;
                            string FXName = MoveName;
                            string soundFileNameAndPath = soundFileElements[1];
                            if (soundFiles.ContainsKey(soundFileNameAndPath))
                            {
                                soundFileNameAndPath = soundFiles[soundFileNameAndPath];
                            }

                            //Check if this item exists already, if it doesn't, add it
                            DataRow[] foundSounds = dt2.Select("EntityType = '" + EntityType + "' AND " +
                                "FXName = '" + FXName + "' AND " +
                                "SoundFileName = '" + soundFileNameAndPath + "'"
                                );
                            if (foundSounds.Length > 0) continue; //Already added this effect to this power, skip it

                            UpdateStatus(Path.GetFileName(sequenceFile) + ":Adding Sound - " + EntityType + " " + FXName + " " + soundFileNameAndPath);
                            dt2.Rows.Add(EntityType, FXName, fxFile, soundFileNameAndPath);
                        }
                    }
                    else continue;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus(ex.ToString());
            }
        }

        /// <summary>
        /// Return a dictionary of Types and related sequence files to search
        /// </summary>
        /// <param name="dataPath"></param>
        /// <returns></returns>
        private static Dictionary<Tuple<string, string>, string> GetTypeSequences(string dataPath, Dictionary<string, string[]> SequenceLines)
        {
            try
            {
                Dictionary<Tuple<string, string>, string> retVal = new Dictionary<Tuple<string, string>, string>();
                EntityTypes = new SortedDictionary<string, EntityType>();

                string fileName = "";

                Dictionary<string, string[]> EntityLines = new Dictionary<string, string[]>();
                foreach (string file in Directory.GetFiles(dataPath + @"\ent_types\", "*.*", SearchOption.AllDirectories))
                {
                    fileName = file.ToUpper();
                    EntityLines.Add(fileName, File.ReadAllLines(fileName));
                }

                //Get all of the Entity Types
                //First, get all entity types listed in the ent_type directory.
                List<Task> AllTasks = new List<Task>();
                foreach (KeyValuePair<string, string[]> kvp in EntityLines)
                {
                    Task task = Task.Factory.StartNew(() => ProcessEntity(kvp));
                    AllTasks.Add(task);
                }
                Task.WaitAll(AllTasks.ToArray());

                //Now go through all sequence files, and find matching entities
                AllTasks = new List<Task>();
                foreach (KeyValuePair<string, string[]> kvp2 in SequenceLines)
                {
                    Task task = Task.Factory.StartNew(() => ProcessSequenceLine(kvp2, dataPath, retVal));
                    AllTasks.Add(task);
                }
                Task.WaitAll(AllTasks.ToArray());

                return retVal;
            }
            catch (Exception ex)
            {
                UpdateStatus(ex.ToString());
                return null;
            }
        }

        private static void ProcessSequenceLine(KeyValuePair<string, string[]> kvp2, string dataPath, Dictionary<Tuple<string, string>, string> retVal)
        {
            try
            {
                string currentInclude = "";
                UpdateStatus("Pass 2 of 2, Match Entity to Sequence - Checking file " + kvp2.Key);
                foreach (string sequenceLine in kvp2.Value)
                {
                    string seqLineToUse = sequenceLine.ToUpper().Trim();

                    string[] lineElements = seqLineToUse.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (lineElements.Length == 0 || (lineElements[0] != "TYPEDEF" && lineElements[0] != "INCLUDE" && lineElements[0] != "TYPE")) continue;

                    if (lineElements[0] == "INCLUDE")
                    {
                        currentInclude = dataPath + @"\" + lineElements[1];
                    }
                    else if (lineElements[0].StartsWith("TYPE") && !EntityTypes.ContainsKey(lineElements[1]))
                    {
                        UpdateStatus("Skipping adding entity '" + lineElements[1] + "' to final list, since it's not a known entity.");
                    }
                    else if (lineElements[0].StartsWith("TYPE") && !retVal.ContainsKey(new Tuple<string, string>(lineElements[1], kvp2.Key)))
                    {
                        UpdateStatus("Adding new Sequence reference - " + lineElements[1] + ", " + kvp2.Key);
                        try
                        {
                            retVal.Add(new Tuple<string, string>(lineElements[1], kvp2.Key), kvp2.Key);
                        }
                        catch
                        {
                            //Don't Care
                        }

                        if (currentInclude != "" && !retVal.ContainsKey(new Tuple<string, string>(lineElements[1], currentInclude)))
                        {
                            UpdateStatus("Adding new Sequence reference - " + lineElements[1] + ", " + currentInclude);
                            try
                            {
                                retVal.Add(new Tuple<string, string>(lineElements[1], currentInclude), currentInclude);
                            }
                            catch
                            {
                                //Don't Care
                            }

                            currentInclude = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private static void ProcessEntity(KeyValuePair<string, string[]> kvp)
        {
            try
            {
                //If the filename is the same as a known entity, then set that as the entity name
                EntityType e = null;
                if (EntityTypes.ContainsKey(Path.GetFileNameWithoutExtension(kvp.Key))) e = EntityTypes[Path.GetFileNameWithoutExtension(kvp.Key)];

                //Read the file to get the Entity Type
                if (e == null)
                {
                    int SeqLineNum = 0;
                    foreach (string entityLine in kvp.Value)
                    {
                        GC.Collect(2, GCCollectionMode.Forced, true, true);
                        lineToUse = entityLine.ToUpper().Trim();
                        SeqLineNum++;
                        UpdateStatus("Pass 1 of 2, Entity Type Creation - Checking file  " + kvp.Key);

                        string[] lineElements = lineToUse.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                        if (lineElements.Length < 2 || (lineElements[0].ToUpper() != "SEQUENCERTYPE" && lineElements[0].ToUpper() != "TYPE")) continue;
                        if (EntityTypes.ContainsKey(lineElements[1].ToUpper()))
                        {
                            //If this name doesn't match the name of the file, then add it as a sub-Entity
                            if (lineElements[1].ToUpper() == Path.GetFileNameWithoutExtension(kvp.Key))
                            {
                                e = EntityTypes[lineElements[1].ToUpper()];
                            }
                            else
                            {
                                e = new EntityType(Path.GetFileNameWithoutExtension(kvp.Key), EntityTypes[lineElements[1].ToUpper()]);
                            }
                        }
                        else
                        {
                            //Create the new EntityType
                            e = new EntityType(lineElements[1].ToUpper(), null);
                        }

                        if (!EntityTypes.ContainsKey(e.Name))
                        {
                            try
                            {
                                EntityTypes.Add(e.Name, e);
                            }
                            catch
                            {
                                //Don't care, already there
                            }
                        }

                        //If we got to the sequencertype, there's nothing else in this file we care about, skip the rest of it to save time
                        if (lineElements[0] == "SEQUENCERTYPE") break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private class EntityType
        {
            public EntityType(string name, EntityType parent)
            {
                Name = name;
                Parent = parent;
            }

            public string Name { get; set; }
            public EntityType Parent { get; set; }

            /// <summary>
            /// Returns whether or not an entity has this type name anywhere in it's own name or its parent structure
            /// </summary>
            /// <param name="Entity"></param>
            /// <param name="TypeName"></param>
            /// <returns></returns>
            public static bool HasTypeNameInChain(EntityType Entity, string TypeName)
            {
                if (Entity.Name == TypeName) return true;
                if (Entity.Parent == null) return false;
                return HasTypeNameInChain(Entity.Parent, TypeName);
            }
        }

    }
}
