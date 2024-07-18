using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Philotic_Knight.DefMethods;

namespace Philotic_Knight
{
    public partial class MainForm : Form
    {
        readonly string AllPowersFile = AppDomain.CurrentDomain.BaseDirectory + "AllPowers.xml";
        private PowerPicker lastPP = null;

        public MainForm()
        {
            InitializeComponent();

            //Get all Powers from default file
            string fileName = AllPowersFile;
            if (File.Exists(fileName))
            {
                Model.AllPowers = Controller.GetDataSetFromXML(fileName).Tables[nameof(Model.AllPowers)];
            }
            else
            {
                if (MessageBox.Show("Cannot find config file '" + fileName + "', which is required for program operation. Do you have a file you wish to load?",
                    "Missing AllPowersXML file", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    ImportDefData();
                }
                else
                {
                    if (MessageBox.Show("Do you have a data directory that you wish to scan to generate the necessary file?",
                        "Scan Direcotry?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                        ScanDefData();
                    }
                    else
                    {
                        MessageBox.Show("With no powers data, this program cannot run. It will now exit.",
                            "No AllPowersXML File Provided", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                }
            }

            //Fix the int column types in case they're not correct from the import
            if (Model.AllPowers.Columns[nameof(Model.AllPowersFields.LevelAvailable)].DataType != typeof(int))
            {
                DataTable NewPowers = Model.AllPowers.Clone();
                NewPowers.Columns[nameof(Model.AllPowersFields.LevelAvailable)].DataType = typeof(int);
                NewPowers.Columns[nameof(Model.AllPowersFields.AT_SetType)].DataType = typeof(int);
                foreach (DataRow dr in Model.AllPowers.Rows)
                {
                    NewPowers.ImportRow(dr);
                }
                Model.AllPowers = NewPowers;
            }
        }

        private void ImportDefData()
        {
            DataSet ds = GetDataSetFromXMLFile();
            if (ds == null) return;
            Model.AllPowers = ds.Tables[nameof(Model.AllPowers)];
            ds = new DataSet(nameof(Model.AllData));
            ds.Tables.Add(Model.AllPowers.Copy());
            ds.WriteXml(AllPowersFile);
            MessageBox.Show("All Powers Data imported successfully into the application.", "Import Successful",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ScanDefData()
        {
            Model.AllPowers = GetDataSetFromDataDirectory();

            //Write that to the default location for later usage
            DataSet ds = new DataSet(nameof(Model.AllData));
            ds.Tables.Add(Model.AllPowers);
            ds.WriteXml(AllPowersFile);
            MessageBox.Show("All Powers Data successfully scanned from directory.", "Scan Successful",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ScanDefDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScanDefData();
        }

        private void ImportDefDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportDefData();
        }

        private void ExportDefDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Choose the XML file to save to.";
                sfd.DefaultExt = ".xml";
                sfd.Filter = "XML Files|*.xml";
                if (sfd.ShowDialog() == DialogResult.Cancel || sfd.FileName.ToString().Trim() == "") return;
                if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                DataSet ds = new DataSet();
                ds.Tables.Add(Model.AllPowers.Copy());
                ds.WriteXml(sfd.FileName);
                MessageBox.Show("File exported successfully to '" + sfd.FileName + "'.",
                    "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ImportCharacterFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Please choose the character file to import.";
                ofd.Filter = "XML Files|*.xml";
                if (ofd.ShowDialog() == DialogResult.Cancel || !File.Exists(ofd.FileName)) return;

                //Verify that we have a good file
                DataSet ds = new DataSet();
                ds.ReadXml(ofd.FileName);
                if (!ds.Tables.Contains(nameof(Model.CharacterData)) || !ds.Tables.Contains(nameof(Model.CharacterPowerPicks)) ||
                    !ds.Tables.Contains(nameof(Model.CharacterEnhancementSlots)))
                {
                    MessageBox.Show("Invalid character file, please try again with another file.", "Invalid File",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                WipeOutData();

                //Let the UI recreate the character from the data
                DataTable TempCharacterData = ds.Tables[nameof(Model.CharacterData)].Copy();
                DataTable TempPowerPicks = ds.Tables[nameof(Model.CharacterPowerPicks)].Copy();
                DataTable TempEnhancementPicks = ds.Tables[nameof(Model.CharacterEnhancementSlots)].Copy();

                //Load the character basic data
                txtCharacterName.Text = TempCharacterData.Rows[0][nameof(Model.CharacterDataFields.CharacterName)].ToString();
                TxtCharacterName_Validated(txtCharacterName, new EventArgs());
                Application.DoEvents();
                cboArchetype.SelectedValue = TempCharacterData.Rows[0][nameof(Model.CharacterDataFields.AT_Name)].ToString();
                Application.DoEvents();
                cboPrimary.SelectedValue = TempCharacterData.Rows[0][nameof(Model.CharacterDataFields.PrimaryPowerSet_Name)].ToString();
                Application.DoEvents();
                cboSecondary.SelectedValue = TempCharacterData.Rows[0][nameof(Model.CharacterDataFields.SecondaryPowerSet_Name)].ToString();
                Application.DoEvents();

                //Go through every level and repick the actions one by one
                int RecordedLevels = 0;
                for (int level = 1; level <= 50; level++)
                {
                    //Check what action happened at that level
                    Model.Action action = Controller.GetAction(level);

                    switch (action)
                    {
                        case Model.Action.Pick_Second_Power:
                            // Fake out the actions necessary to pick the first three powers. From there, we can proceed as normal
                            lastPP.cboPowerPicker.SelectedValue = TempPowerPicks.Rows[0][nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString();
                            Application.DoEvents();
                            lastPP.cboPowerPicker.SelectedValue = TempPowerPicks.Rows[1][nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString();
                            Application.DoEvents();
                            lastPP.cboPowerPicker.SelectedValue = TempPowerPicks.Rows[2][nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString();
                            Application.DoEvents();
                            level++; //Skip up to level 3
                            RecordedLevels = 3;
                            break;
                        case Model.Action.Pick3EnhancementSlots:
                        case Model.Action.Pick2EnhancementSlots:
                        case Model.Action.Pick1EnhancementSlot:
                            //Go through all Enhancement slot picks for this level, and commit them one by one
                            DataRow[] drs = TempEnhancementPicks.Select(nameof(Model.CharacterEnhancementSlotsFields.LevelPicked) + " = '" + level + "'");
                            foreach (DataRow dr in drs)
                            {
                                //Skip the first one, since the first one is ALWAYS clicked
                                if (dr[nameof(Model.CharacterEnhancementSlotsFields.SlotNumber)].ToString() == "1") continue;

                                //Get the name of the power, and then find that power picker
                                bool clicked = false;
                                foreach(PowerPicker pp in PowerPanel.Controls)
                                {
                                    if (pp.cboPowerPicker.SelectedValue.ToString() != dr[nameof(Model.CharacterEnhancementSlotsFields.Power_Name)].ToString()) continue;
                                    foreach (Control c in pp.grpName.Controls)
                                    {
                                        if (c.GetType() != typeof(CheckBox)) continue;
                                        if (((CheckBox)c).Checked) continue;
                                        //Click the first available checkbox to click, and then bail out
                                        clicked = true;
                                        CheckBox chk = ((CheckBox)c);
                                        chk.Checked = true;
                                        ActionHappened(chk, new EventArgs());
                                        Application.DoEvents();
                                        break;
                                    }
                                    if (clicked) break;
                                }
                            }
                            RecordedLevels++;
                            break;
                        case Model.Action.Pick1Power:
                            foreach (DataRow drPower in TempPowerPicks.Select(nameof(Model.CharacterPowerPicksFields.LevelPicked) + " = '" + level + "'"))
                            {
                                lastPP.cboPowerPicker.SelectedValue = drPower[nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString();
                                Application.DoEvents();
                                Debug.WriteLine("");
                                RecordedLevels++;
                            }
                            break;
                        default:
                            throw new Exception("Unhandled action of '" + action.ToString() + "'");
                    }
                }

                if (RecordedLevels == 51) RecordedLevels = 50;
                txtCharacterLevel.Text = RecordedLevels.ToString();

                Model.CharacterData.Rows[0][nameof(Model.CharacterDataFields.CurrentLevel)] = RecordedLevels;

                ////Force the level 51 code
                //if (txtCharacterLevel.Text == "51")
                //{
                //    ActionHappened(null, new EventArgs());
                //    Application.DoEvents();
                //}
            }
        }

        private void ExportCharacterFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Model.CharacterData == null || Model.CharacterData.Rows.Count == 0 ||
                Model.CharacterPowerPicks == null || Model.CharacterPowerPicks.Rows.Count == 0 ||
                Model.CharacterEnhancementSlots == null || Model.CharacterEnhancementSlots.Rows.Count == 0)
            {
                MessageBox.Show("No character data found. Please create a new character or import an existing character file.",
                    "No Character Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "Export File",
                Filter = "XML Files|*.xml",
                FileName = txtCharacterName.Text + ".xml"
            };
            if (sfd.ShowDialog() == DialogResult.Cancel || sfd.FileName.Trim() == "") return;
            if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
            DataSet ds = new DataSet(Model.CharacterData.Rows[0][nameof(Model.CharacterDataFields.CharacterName)].ToString());

            //Make sure that the current character's level is up to date from the screen
            Model.CharacterData.Rows[0][nameof(Model.CharacterDataFields.CurrentLevel)] = txtCharacterLevel.Text;

            ds.Tables.Add(Model.CharacterData);
            ds.Tables.Add(Model.CharacterPowerPicks);
            ds.Tables.Add(Model.CharacterEnhancementSlots);
            ds.WriteXml(sfd.FileName);
            MessageBox.Show("Character data has been written to file '" + sfd.FileName + "'", "File Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private DataSet GetDataSetFromXMLFile()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Choose the XML file to load.";
                ofd.DefaultExt = ".xml";
                ofd.Filter = "XML Files|*.xml";
                if (ofd.ShowDialog() == DialogResult.Cancel || ofd.FileName.ToString().Trim() == "" || !File.Exists(ofd.FileName)) return null;
                return Controller.GetDataSetFromXML(ofd.FileName);
            }
        }

        private DataTable GetDataSetFromDataDirectory()
        {
            string Folder = "";
            using (FolderBrowserDialog ofd = new FolderBrowserDialog())
            {
                ofd.Description = "Choose the directory to scan";
                if (ofd.ShowDialog() == DialogResult.Cancel || ofd.SelectedPath.ToString().Trim() == "" || !Directory.Exists(ofd.SelectedPath)) return null;
                Folder = ofd.SelectedPath;
            }

            return Controller.ScanPowersDataFromPath(Folder, StatusLabel);
        }

        private void WipeOutData()
        {
            //Wipe out all child controls
            do
            {
                foreach (Control c in PowerPanel.Controls)
                {
                    c.Parent = null;
                }
            } while (PowerPanel.Controls.Count > 0) ;

            cboSecondary.DataSource = null;
            cboSecondary.SelectedValue = "";
            cboPrimary.DataSource = null;
            cboPrimary.SelectedValue = "";
            cboArchetype.DataSource = null;
            cboArchetype.SelectedValue = null;
            txtCharacterLevel.Text = "-3";
            txtCharacterName.Text = null;
            Model.CharacterData = null;
            Model.CharacterEnhancementSlots = null;
            Model.CharacterPowerPicks = null;
            lastPP = null;
        }

        private void BtnNewCharacter_Click(object sender, EventArgs e)
        {

        }

        private void StartNewCharacter()
        {
            StatusLabel.Text = Controller.GetActionDescription(Model.Action.Enter_Character_Name);
            txtCharacterName.Enabled = true;
            txtCharacterName.Focus();
        }

        private void ActionHappened(object sender, EventArgs e)
        {
            //All done!
            if (txtCharacterLevel.Text == "51")
            {
                //Disable all checkboxes in all PowerPickers
                foreach (PowerPicker ppDisable in PowerPanel.Controls)
                {
                    foreach (Control c in ppDisable.grpName.Controls)
                    {
                        if (c.GetType() != typeof(CheckBox)) continue;
                        ((CheckBox)c).Enabled = false;
                    }
                }

                //Set the level artifically back to 50
                txtCharacterLevel.Text = "50";
                return;
            }

            ComboBox cbo = null;
            CheckBox chk = null;
            DataRow dr = null;
            PowerPicker pp = null;
            Model.Action newAction;
            Control ctl = (Control)sender;

            if (ctl.GetType() == typeof(ComboBox))
            {
                cbo = (ComboBox)sender;
                if (IsNullOrEmpty(cbo.SelectedValue)) return;
            }
            else
            {
                chk = (CheckBox)sender;
            }

            //Get the action, either from the control or from the level if the control doesn't have one
            Model.Action action = ctl.Tag == null ? Controller.GetAction(int.Parse(txtCharacterLevel.Text)) : (Model.Action)ctl.Tag;
            StatusLabel.Text = Controller.GetActionDescription(action);

            //Determine what to do, based on the return action for that level
            switch (action)
            {
                case Model.Action.Pick_Archetype:
                    cboPrimary.ValueMember = nameof(Model.AllPowersFields.PowerSet_Name);
                    cboPrimary.DataSource = Controller.GetPrimaries(Model.AllPowers, cboArchetype.SelectedValue.ToString());
                    cboPrimary.DisplayMember = nameof(Model.AllPowersFields.PowerSet_DisplayName);
                    cboPrimary.DropDownWidth = Controller.GetMaxDropDownWidth(cboPrimary);
                    cboPrimary.Enabled = true;
                    cboArchetype.Enabled = false;
                    cboPrimary.Focus();

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));
                    break;
                case Model.Action.Pick_Primary:
                    cboSecondary.ValueMember = nameof(Model.AllPowersFields.PowerSet_Name);
                    cboSecondary.DataSource = Controller.GetSecondaries(Model.AllPowers, cboArchetype.SelectedValue.ToString());
                    cboSecondary.DisplayMember = nameof(Model.AllPowersFields.PowerSet_DisplayName);
                    cboSecondary.DropDownWidth = Controller.GetMaxDropDownWidth(cboSecondary);
                    cboSecondary.Enabled = true;
                    cboPrimary.Enabled = false;
                    cboSecondary.Focus();

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));
                    break;
                case Model.Action.Pick_Secondary:
                    //Start prepping the character with our data so far
                    Model.CharacterData = Controller.PrepCharacter();

                    DataRow charData = Model.CharacterData.NewRow();
                    charData[nameof(Model.CharacterDataFields.CharacterName)] = txtCharacterName.Text;
                    charData[nameof(Model.CharacterDataFields.AT_Name)] = cboArchetype.SelectedValue.ToString();
                    charData[nameof(Model.CharacterDataFields.PrimaryPowerSet_Name)] = cboPrimary.SelectedValue.ToString();
                    charData[nameof(Model.CharacterDataFields.SecondaryPowerSet_Name)] = cboSecondary.SelectedValue.ToString();
                    charData[nameof(Model.CharacterDataFields.CurrentLevel)] = int.Parse(txtCharacterLevel.Text);
                    charData[nameof(Model.CharacterDataFields.ProgramVersion)] = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    Model.CharacterData.Rows.Add(charData);

                    AddPowerPicker(nameof(Model.AllPowersFields.Power_Name),
                        Controller.GetFirstPower(Model.AllPowers, cboArchetype.SelectedValue.ToString(), cboPrimary.SelectedValue.ToString()),
                        nameof(Model.AllPowersFields.Power_DisplayName),
                        null, "Level 1:");

                    cboSecondary.Enabled = false;

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));
                    break;
                case Model.Action.Pick_First_Power:
                    //Setup the Power Picks table, since this is the first power picked
                    Model.CharacterPowerPicks = Controller.PrepCharacterPowerPicks();

                    //Add Power pick to the list of power picks
                    dr = Controller.AddPowerToPicks(Model.CharacterPowerPicks, cboArchetype.SelectedValue.ToString(),
                        (int)Model.AllPowersSetTypes.PRIMARY,
                        cboPrimary.SelectedValue.ToString(),
                        lastPP.cboPowerPicker.SelectedValue.ToString(), 1);

                    //Setup the next comobox - special case, as the first two powers are the only two picked next to each other in level
                    AddPowerPicker(nameof(Model.AllPowersFields.Power_Name),
                        Controller.GetSecondPower(Model.AllPowers, cboArchetype.SelectedValue.ToString(), cboSecondary.SelectedValue.ToString()),
                        nameof(Model.AllPowersFields.Power_DisplayName), dr, "Level 1:");

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));
                    break;
                case Model.Action.Pick_Second_Power:

                    //Add Power pick to the list of power picks
                    dr = Controller.AddPowerToPicks(Model.CharacterPowerPicks, cboArchetype.SelectedValue.ToString(),
                        (int)Model.AllPowersSetTypes.SECONDARY,
                        cboSecondary.SelectedValue.ToString(),
                        lastPP.cboPowerPicker.SelectedValue.ToString(), 1);

                    //Setup the next combobox - special case, as the first two powers are the only two picked next to each other in level
                    AddPowerPicker(nameof(Model.AllPowersFields.Power_Name),
                        Controller.GetAvailablePowers(Model.AllPowers, Model.CharacterPowerPicks, cboArchetype.SelectedValue.ToString(),
                        cboPrimary.SelectedValue.ToString(), cboSecondary.SelectedValue.ToString(), 2),
                        nameof(Model.AllPowersFields.Power_DisplayName), dr, "Level 2:");

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));
                    break;
                case Model.Action.Pick1Power:
                    //Find the row of data based on the power name chosen, should only be one per combobox, unless something unforseen happens
                    DataRow[] drs = ((DataTable)cbo.DataSource).Select(nameof(Model.AllPowersFields.Power_Name) + " = '" + cbo.SelectedValue.ToString() + "'");
                    if (drs.Length != 1) throw new Exception("Either didn't find any datarows, or found multiple datarows for power '" + cbo.SelectedValue + "'");

                    //Add Power pick to the list of power picks
                    dr = Controller.AddPowerToPicks(Model.CharacterPowerPicks, cboArchetype.SelectedValue.ToString(),
                        int.Parse(drs[0][nameof(Model.AllPowersFields.AT_SetType)].ToString()),
                        drs[0][nameof(Model.AllPowersFields.PowerSet_Name)].ToString(),
                        cbo.SelectedValue.ToString(), int.Parse(txtCharacterLevel.Text)); ;

                    // Disable this box, after setting the PowerPicker as the last one, and the datarow itself to the Tag for future use
                    lastPP = (PowerPicker)cbo.Parent.Parent;
                    lastPP.Tag = dr;
                    cbo.Enabled = false;

                    //Setup the next action - should always be to choose some enhancement slots, so activate all of the "next ones".
                    foreach (PowerPicker currentPP in GetPowerPickers(PowerPanel))
                    {
                        foreach (CheckBox chkCurrent in GetCheckBoxes(currentPP))
                        {
                            //Skip the ones that are already checked
                            if (chkCurrent.Checked) continue;

                            //Always just enable the first combobox, then quit
                            chkCurrent.Enabled = true;
                            chkCurrent.Click -= new EventHandler(ActionHappened); //Unsubscribe to prevent duplicates
                            chkCurrent.Click += new EventHandler(ActionHappened);
                            break;
                        }
                    }

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));

                    break;
                case Model.Action.Pick1EnhancementSlot:
                    //Add the current enhancement slot to the enhancement slots list
                    pp = (PowerPicker)chk.Parent.Parent;
                    dr = (DataRow)pp.Tag;

                    chk.Enabled = false;

                    //If the tag is null, then try to find it again via the name of the power from the character's picks
                    if (dr == null) dr = Model.CharacterPowerPicks.Select(nameof(Model.CharacterPowerPicksFields.Power_Name)
                        + " = '" + pp.cboPowerPicker.SelectedValue.ToString() + "'")[0];

                    Controller.AddSlot(Model.CharacterEnhancementSlots, cboArchetype.SelectedValue.ToString(),
                        dr[nameof(Model.CharacterPowerPicksFields.PowerSet_Name)].ToString(),
                        dr[nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString(), Controller.GetOnlyNumbers(chk.Name),
                        int.Parse(txtCharacterLevel.Text), "", 0);

                    // Increment the level for the next action and display the next action
                    int level = int.Parse(txtCharacterLevel.Text) + 1;
                    txtCharacterLevel.Text = level.ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(level));

                    //Finally, enable the next power picker, if applicable
                    newAction = Controller.GetAction(level);
                    if (newAction == Model.Action.Pick1Power)
                    {
                        AddPowerPicker(nameof(Model.AllPowersFields.Power_Name),
                            Controller.GetAvailablePowers(Model.AllPowers, Model.CharacterPowerPicks, cboArchetype.SelectedValue.ToString(),
                            cboPrimary.SelectedValue.ToString(), cboSecondary.SelectedValue.ToString(), level),
                            nameof(Model.AllPowersFields.Power_DisplayName),
                            null, "Level " + level + ":");

                        // Disable all of the checkboxes, and take away their tags, because the next action will be a power pick
                        foreach (PowerPicker currentPP in GetPowerPickers(PowerPanel))
                        {
                            foreach (CheckBox chkCurrent in GetCheckBoxes(currentPP))
                            {
                                //Always just enable the first combobox, assign the action to it, then quit
                                chkCurrent.Enabled = false;
                                chkCurrent.Tag = null;
                            }
                        }
                    }
                    else if (newAction == Model.Action.Pick3EnhancementSlots)
                    {
                        //Or enable the appropriate checkboxes, if applicable
                        foreach (PowerPicker currentPP in GetPowerPickers(PowerPanel))
                        {
                            foreach (CheckBox chkCurrent in GetCheckBoxes(currentPP))
                            {
                                //Skip the ones that are already checked
                                if (chkCurrent.Checked) continue;

                                //Always just enable the first combobox, then quit
                                chkCurrent.Enabled = true;
                                chkCurrent.Click -= new EventHandler(ActionHappened); //Unsubscribe to prevent duplicates
                                chkCurrent.Click += new EventHandler(ActionHappened);
                                break;
                            }
                        }
                    }

                    break;
                case Model.Action.Pick2EnhancementSlots:
                case Model.Action.Pick3EnhancementSlots:
                    //Add the current enhancement slot to the enhancement slots list

                    //Get the PowerSet name and Power name from the parent's Tag
                    pp = chk == null? (PowerPicker)cbo.Parent.Parent : (PowerPicker)chk.Parent.Parent;
                    dr = (DataRow)pp.Tag;

                    //If the tag is null, then try to find it again via the name of the power from the character's picks
                    if (dr == null) dr = Model.CharacterPowerPicks.Select(nameof(Model.CharacterPowerPicksFields.Power_Name) 
                        + " = '" + pp.cboPowerPicker.SelectedValue.ToString() + "'")[0];
                    Controller.AddSlot(Model.CharacterEnhancementSlots, cboArchetype.SelectedValue.ToString(),
                        dr[nameof(Model.CharacterPowerPicksFields.PowerSet_Name)].ToString(),
                        dr[nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString(), Controller.GetOnlyNumbers(chk.Name),
                        int.Parse(txtCharacterLevel.Text), "", 0);

                    //Then disable this checkbox
                    chk.Enabled = false;

                    newAction = action == Model.Action.Pick3EnhancementSlots ? Model.Action.Pick2EnhancementSlots : Model.Action.Pick1EnhancementSlot;

                    //Setup the next action - should always be to choose some enhancement slots, so activate all of the "next ones".
                    foreach (PowerPicker currentPP in GetPowerPickers(PowerPanel))
                    {
                        foreach (CheckBox chkCurrent in GetCheckBoxes(currentPP))
                        {
                            //Skip the ones that are already checked
                            if (chkCurrent.Checked) continue;

                            //Always just enable the first combobox, assign the action to it, then quit
                            chkCurrent.Enabled = true;
                            chkCurrent.Tag = newAction;
                            chkCurrent.Click -= new EventHandler(ActionHappened); //Unsubscribe to prevent duplicates
                            chkCurrent.Click += new EventHandler(ActionHappened);
                            break;
                        }
                    }

                    // Display the next action
                    StatusLabel.Text = Controller.GetActionDescription(newAction);
                    break;
                default: throw new Exception("Unhandled action type '" + action.ToString() + "'");
            }
        }

        private void AddPowerPicker(string ValueMember, DataTable DataSource, string DisplayMember, object TagValue, string LabelValue)
        {
            PowerPicker pp = new PowerPicker();
            pp.cboPowerPicker.ValueMember = ValueMember;
            pp.cboPowerPicker.DataSource = DataSource;
            pp.cboPowerPicker.DisplayMember = DisplayMember;
            pp.cboPowerPicker.DropDownWidth = Controller.GetMaxDropDownWidth(pp.cboPowerPicker);
            pp.Tag = TagValue;
            pp.grpName.Text = LabelValue;
            pp.cboPowerPicker.SelectedValueChanged += new EventHandler(ActionHappened);
            pp.ComboBoxEnabled = true;
            if (lastPP != null) lastPP.ComboBoxEnabled = false;
            PowerPanel.Controls.Add(pp);
            if (lastPP == null)
            {
                pp.Top = 0;
                pp.Left = 0;
            }
            else
            {
                pp.Top = lastPP.Bottom + 5;
                pp.Left = lastPP.Left;
            }
            pp.Width = PowerPanel.Width - 5;
            pp.Focus();
            lastPP = pp;

            pp.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);

            ScrollToBottom();
        }

        private IEnumerable<CheckBox> GetCheckBoxes(PowerPicker currentPP)
        {
            List<CheckBox> chks = new List<CheckBox>();
            //The primary control is a groupBox, so you have to go inside of its controls
            foreach (Control ctl in currentPP.Controls[0].Controls)
            {
                if (ctl.GetType() == typeof(CheckBox)) chks.Add((CheckBox)ctl);
            }
            return chks;
        }

        private List<PowerPicker> GetPowerPickers(Control parentControl)
        {
            List<PowerPicker> pps = new List<PowerPicker>();
            foreach (Control ctl in parentControl.Controls)
            {
                if (ctl.GetType() == typeof(PowerPicker)) pps.Add((PowerPicker)ctl);
            }
            return pps;
        }

        private void TxtCharacterName_Validated(object sender, EventArgs e)
        {
            //Start new character, note that the user has to choose to start a new character or load an existing one
            cboArchetype.ValueMember = nameof(Model.AllPowersFields.AT_Name);
            cboArchetype.DataSource = Controller.GetArchetypes(Model.AllPowers);
            cboArchetype.DisplayMember = nameof(Model.AllPowersFields.AT_DisplayName);
            cboArchetype.Enabled = true;
            StatusLabel.Text = Controller.GetActionDescription(Model.Action.Pick_Archetype);
            cboArchetype.Focus();
        }

        private void TxtCharacterName_Validating(object sender, CancelEventArgs e)
        {
            if (txtCharacterName.Text.Trim() == "")
            {
                MessageBox.Show("Must have a non-empty character name to continue", "Name Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        public void ScrollToBottom()
        {
            using (Control c = new Control() { Parent = PowerPanel, Dock = DockStyle.Bottom })
            {
                PowerPanel.ScrollControlIntoView(c);
                c.Parent = null;
            }
        }

        private void StartNewCharacterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WipeOutData();
            StartNewCharacter();
        }

        private void TxtCharacterName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                CancelEventArgs ce = new CancelEventArgs();
                TxtCharacterName_Validating(sender, ce);
                if (!ce.Cancel) TxtCharacterName_Validated(sender, new EventArgs());
            }
        }

        private void TxtCharacterName_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Tab) e.IsInputKey = true;
        }

        private void ExportForumHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Model.CharacterData == null || Model.CharacterData.Rows.Count == 0 ||
                Model.CharacterPowerPicks == null || Model.CharacterPowerPicks.Rows.Count == 0 ||
                Model.CharacterEnhancementSlots == null || Model.CharacterEnhancementSlots.Rows.Count == 0)
            {
                MessageBox.Show("Insufficient character data to export to HTML Forum format. Please either create a new character, or " +
                    "load an existing one to continue.", "Insufficient Character Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Go through the character's default information
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><head><title>The Philotic Knight's Desktop Field Trainer</title></head><body><p><table border = 1>" +
                "<tr><th colspan=2>The Philotic Knight's Desktop Field Trainer</th></tr>");
            foreach (DataRow CharacterDataRow in Model.CharacterData.Rows)
            {
                foreach (DataColumn dc in Model.CharacterData.Columns)
                {
                    sb.AppendLine("<tr><th align='right'>" + dc.ColumnName + ":</th><td>" + CharacterDataRow[dc.ColumnName].ToString() + "</td></tr>");
                }
            }
            sb.AppendLine("</table></p><p><table border = 1><tr><th>Level</th><th>PowerSet Name</th><th>Power Name</th>" +
                "<th>Slot1</th><th>Slot2</th><th>Slot3</th><th>Slot4</th><th>Slot5</th><th>Slot6</th></tr>");

            //Now, go through all of the powers and put them in a table, along with their enhancement slots
            foreach (DataRow PowerPickRow in Model.CharacterPowerPicks.Rows)
            {
                sb.AppendLine("<tr><td>" + PowerPickRow[nameof(Model.CharacterPowerPicksFields.LevelPicked)].ToString() + "</td>");
                sb.AppendLine("<td>" + PowerPickRow[nameof(Model.CharacterPowerPicksFields.PowerSet_Name)].ToString() + "</td>");
                sb.AppendLine("<td>" + PowerPickRow[nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString() + "</td>");

                DataRow[] drs = Model.CharacterEnhancementSlots.Select(nameof(Model.CharacterEnhancementSlotsFields.Power_Name) +
                    " = '" + PowerPickRow[nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString() + "'");
                if (drs.Length > 0)
                {
                    foreach (DataRow dr in drs)
                    {
                        string EnhancementType = IsNullOrEmpty(dr[nameof(Model.CharacterEnhancementSlotsFields.EnhancementName)]) ?
                            "Empty" : dr[nameof(Model.CharacterEnhancementSlotsFields.EnhancementName)].ToString();
                        sb.AppendLine("<td>(" + dr[nameof(Model.CharacterEnhancementSlotsFields.LevelPicked)].ToString() + ")" + EnhancementType + "</td>");
                    }
                }
            }

            sb.AppendLine("</table></p></body></html>");

            Clipboard.SetText(sb.ToString());

            //Write to a temp file
            string fileName = Path.GetTempFileName().Replace(".tmp", ".html");
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.Write(sb.ToString());
            }

            Process.Start(fileName);

            try
            {
                System.Threading.Thread.Sleep(1000);
                File.Delete(fileName);
            }
            catch
            {
                //Don't care, oh well
            }
            
            MessageBox.Show("Forum HTML has been opened up in a new web browser. To copy and paste, just go to the web browser and type 'CTRL-A' and 'CTRL-C' " +
                "and the data will be copied. You may then paste it into the forum, and it should paste the tables.", "HTML Output",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
