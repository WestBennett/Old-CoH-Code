using Philotic_Knight;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKs_Def_Tools;
using System.Reflection;

namespace PKs_C_Web_Field_Trainer
{
    public partial class DefaultForm : Page
    {
        readonly string LastPP = "LastPP";
        readonly string StoredPowerPickers = "StoredPowerPickers";
        readonly string AllPowersFile = AppDomain.CurrentDomain.BaseDirectory + "AllPowers.xml";
        private List<PowerPicker> AllPowerPickers = new List<PowerPicker>();

        protected void Page_Load(object sender, EventArgs e)
        {
            StatusLabel.Text = AllPowersFile;
            Model.AllPowers = Controller.GetDataSetFromXML(AllPowersFile).Tables[nameof(Model.AllPowers)];
        }

        protected void Page_PreRender()
        {
            System.Diagnostics.Debug.WriteLine("");

        }

        protected void Page_Init()
        {
            if (!DefMethods.IsNullOrEmpty(Session[StoredPowerPickers])) AllPowerPickers = (List<PowerPicker>)Session[StoredPowerPickers];
            if (AllPowerPickers.Count > 0)
            {
                List<PowerPicker> PickersToAdd = new List<PowerPicker>(AllPowerPickers);

                if (Model.CharacterPowerPicks != null) System.Diagnostics.Debug.WriteLine(Model.CharacterPowerPicks.ToString()); //REMOVE, for checking only

                foreach (PowerPicker pp in AllPowerPickers)
                {
                    //See if the control exists on the page, if not, re-add it
                    bool ControlExists = false;
                    foreach (Control c in PowerPanel.Controls)
                    {
                        if (c.ID == pp.ID) ControlExists = true;
                    }
                    if (ControlExists && PickersToAdd.Contains(pp))
                    {
                        PickersToAdd.Remove(pp);
                    }
                }

                foreach (PowerPicker pp in PickersToAdd)
                {
                    //Re-add the missing control to the Power Panel
                    AddPowerPicker(pp.ValueMember, pp.DataSource, pp.DisplayMember, pp.DdlPowerPicker.SelectedValue, pp.LblName.Text);
                }

                //Re-enable the PowerPickers' Checkboxes, if appropriate
                string Level = ((TextBox)tblHeader.FindControl(nameof(txtCharacterLevel))).Text;
                Model.Action a = Controller.GetAction(int.Parse((Level)));
                if (a == Model.Action.Pick3EnhancementSlots || a == Model.Action.Pick2EnhancementSlots || a == Model.Action.Pick1EnhancementSlot)
                {
                    foreach (Control c in PowerPanel.Controls)
                    {
                        if (c.GetType() == typeof(PowerPicker))
                        {
                            System.Diagnostics.Debug.WriteLine("");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("");
                        }
                    }
                }
            }
        }

        public void ActionHappened(object sender, EventArgs e)
        {
            //All done!
            if (txtCharacterLevel.Text == "51")
            {
                //Disable all checkboxes in all PowerPickers
                foreach (PowerPicker ppDisable in PowerPanel.Controls)
                {
                    foreach (Control c in ppDisable.Controls)
                    {
                        if (c.GetType() != typeof(CheckBox)) continue;
                        ((CheckBox)c).Enabled = false;
                    }
                }

                //Set the level artifically back to 50
                txtCharacterLevel.Text = "50";
                return;
            }

            DropDownList ddl = null;
            CheckBox chk = null;
            DataRow dr = null;
            PowerPicker pp = null;
            Model.Action newAction;
            Control ctl = (Control)sender;

            if (ctl.GetType() == typeof(Button))
            {
                if (txtCharacterName.Text.Trim() == "")
                {
                    StatusLabel.Text = "Must have a non-empty character name to continue";
                    return;
                }

                //Start new character, note that the user has to choose to start a new character or load an existing one
                //ddlArchetype.AppendDataBoundItems = true;
                ddlArchetype.DataValueField = nameof(Model.AllPowersFields.AT_Name);
                ddlArchetype.DataSource = Controller.GetArchetypes(Model.AllPowers);
                ddlArchetype.DataTextField = nameof(Model.AllPowersFields.AT_DisplayName);
                ddlArchetype.DataBind();
                ddlArchetype.Enabled = true;
                btnStartCharacter.Enabled = false;
                StatusLabel.Text = Controller.GetActionDescription(Model.Action.Pick_Archetype);
                ddlArchetype.Focus();
                return;
            }
            else if (ctl.GetType() == typeof(DropDownList))
            {
                ddl = (DropDownList)sender;
                if (DefMethods.IsNullOrEmpty(ddl.SelectedValue)) return;
            }
            else
            {
                chk = (CheckBox)sender;
            }

            //Get the action, either from the control or from the level if the control doesn't have one
            Model.Action action = ctl.GetTag().ToString() == "" ? Controller.GetAction(int.Parse(txtCharacterLevel.Text)) : (Model.Action)ctl.GetTag();
            StatusLabel.Text = Controller.GetActionDescription(action);

            //Determine what to do, based on the return action for that level
            switch (action)
            {
                case Model.Action.Pick_Archetype:
                    ddlPrimary.DataValueField = nameof(Model.AllPowersFields.PowerSet_Name);
                    ddlPrimary.DataSource = Controller.GetPrimaries(Model.AllPowers, ddlArchetype.SelectedValue.ToString());
                    ddlPrimary.DataTextField = nameof(Model.AllPowersFields.PowerSet_DisplayName);
                    ddlPrimary.Enabled = true;
                    ddlPrimary.DataBind();
                    ddlArchetype.Enabled = false;
                    ddlPrimary.Focus();

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));
                    break;
                case Model.Action.Pick_Primary:
                    ddlSecondary.DataValueField = nameof(Model.AllPowersFields.PowerSet_Name);
                    ddlSecondary.DataSource = Controller.GetSecondaries(Model.AllPowers, ddlArchetype.SelectedValue.ToString());
                    ddlSecondary.DataTextField = nameof(Model.AllPowersFields.PowerSet_DisplayName);
                    ddlSecondary.Enabled = true;
                    ddlSecondary.DataBind();
                    ddlPrimary.Enabled = false;
                    ddlSecondary.Focus();

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));
                    break;
                case Model.Action.Pick_Secondary:
                    //Start prepping the character with our data so far
                    Model.CharacterData = Controller.PrepCharacter();

                    DataRow charData = Model.CharacterData.NewRow();
                    charData[nameof(Model.CharacterDataFields.CharacterName)] = txtCharacterName.Text;
                    charData[nameof(Model.CharacterDataFields.AT_Name)] = ddlArchetype.SelectedValue.ToString();
                    charData[nameof(Model.CharacterDataFields.PrimaryPowerSet_Name)] = ddlPrimary.SelectedValue.ToString();
                    charData[nameof(Model.CharacterDataFields.SecondaryPowerSet_Name)] = ddlSecondary.SelectedValue.ToString();
                    charData[nameof(Model.CharacterDataFields.CurrentLevel)] = int.Parse(txtCharacterLevel.Text);
                    charData[nameof(Model.CharacterDataFields.ProgramVersion)] = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    Model.CharacterData.Rows.Add(charData);

                    AddPowerPicker(nameof(Model.AllPowersFields.Power_Name),
                        Controller.GetFirstPower(Model.AllPowers, ddlArchetype.SelectedValue.ToString(), ddlPrimary.SelectedValue.ToString()),
                        nameof(Model.AllPowersFields.Power_DisplayName),
                        null, "Level 1-1:");

                    ddlSecondary.Enabled = false;

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));

                    break;
                case Model.Action.Pick_First_Power:
                    //Setup the Power Picks table, since this is the first power picked
                    Model.CharacterPowerPicks = Controller.PrepCharacterPowerPicks();

                    //Add Power pick to the list of power picks
                    dr = Controller.AddPowerToPicks(Model.CharacterPowerPicks, ddlArchetype.SelectedValue.ToString(),
                        (int)Model.AllPowersSetTypes.PRIMARY,
                        ddlPrimary.SelectedValue.ToString(),
                        ((PowerPicker)Session[LastPP]).DdlPowerPicker.SelectedValue.ToString(), 1);

                    //Setup the next comobox - special case, as the first two powers are the only two picked next to each other in level
                    AddPowerPicker(nameof(Model.AllPowersFields.Power_Name),
                        Controller.GetSecondPower(Model.AllPowers, ddlArchetype.SelectedValue.ToString(), ddlSecondary.SelectedValue.ToString()),
                        nameof(Model.AllPowersFields.Power_DisplayName), dr, "Level 1-2:");

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));
                    break;
                case Model.Action.Pick_Second_Power:

                    //Add Power pick to the list of power picks
                    dr = Controller.AddPowerToPicks(Model.CharacterPowerPicks, ddlArchetype.SelectedValue.ToString(),
                        (int)Model.AllPowersSetTypes.SECONDARY,
                        ddlSecondary.SelectedValue.ToString(),
                        ((PowerPicker)Session[LastPP]).DdlPowerPicker.SelectedValue.ToString(), 1);

                    //Setup the next combobox - special case, as the first two powers are the only two picked next to each other in level
                    AddPowerPicker(nameof(Model.AllPowersFields.Power_Name),
                        Controller.GetAvailablePowers(Model.AllPowers, Model.CharacterPowerPicks, ddlArchetype.SelectedValue.ToString(),
                        ddlPrimary.SelectedValue.ToString(), ddlSecondary.SelectedValue.ToString(), 2),
                        nameof(Model.AllPowersFields.Power_DisplayName), dr, "Level 2:");

                    // Finally, increment the level for the next action and display the next action
                    txtCharacterLevel.Text = (int.Parse(txtCharacterLevel.Text) + 1).ToString();
                    StatusLabel.Text = Controller.GetActionDescription(Controller.GetAction(int.Parse(txtCharacterLevel.Text)));
                    break;
                case Model.Action.Pick1Power:
                    //Find the row of data based on the power name chosen, should only be one per combobox, unless something unforseen happens
                    DataRow[] drs = ((DataTable)ddl.DataSource).Select(nameof(Model.AllPowersFields.Power_Name) + " = '" + ddl.SelectedValue.ToString() + "'");
                    if (drs.Length != 1) throw new Exception("Either didn't find any datarows, or found multiple datarows for power '" + ddl.SelectedValue + "'");

                    //Add Power pick to the list of power picks
                    dr = Controller.AddPowerToPicks(Model.CharacterPowerPicks, ddlArchetype.SelectedValue.ToString(),
                        int.Parse(drs[0][nameof(Model.AllPowersFields.AT_SetType)].ToString()),
                        drs[0][nameof(Model.AllPowersFields.PowerSet_Name)].ToString(),
                        ddl.SelectedValue.ToString(), int.Parse(txtCharacterLevel.Text)); ;

                    // Disable this box, after setting the PowerPicker as the last one, and the datarow itself to the Tag for future use
                    Session[LastPP] = (PowerPicker)ddl.Parent;
                    ((PowerPicker)Session[LastPP]).SetTag(dr);
                    ddl.Enabled = false;

                    //Setup the next action - should always be to choose some enhancement slots, so activate all of the "next ones".
                    foreach (PowerPicker currentPP in GetPowerPickers(PowerPanel))
                    {
                        foreach (CheckBox chkCurrent in GetCheckBoxes(currentPP))
                        {
                            //Skip the ones that are already checked
                            if (chkCurrent.Checked) continue;

                            //Always just enable the first combobox, then quit
                            chkCurrent.Enabled = true;
                            chkCurrent.CheckedChanged -= new EventHandler(ActionHappened); //Unsubscribe to prevent duplicates
                            chkCurrent.CheckedChanged += new EventHandler(ActionHappened);
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
                    dr = (DataRow)pp.GetTag();

                    chk.Enabled = false;

                    //If the tag is null, then try to find it again via the name of the power from the character's picks
                    if (dr == null) dr = Model.CharacterPowerPicks.Select(nameof(Model.CharacterPowerPicksFields.Power_Name)
                        + " = '" + pp.DdlPowerPicker.SelectedValue.ToString() + "'")[0];

                    Controller.AddSlot(Model.CharacterEnhancementSlots, ddlArchetype.SelectedValue.ToString(),
                        dr[nameof(Model.CharacterPowerPicksFields.PowerSet_Name)].ToString(),
                        dr[nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString(), Controller.GetOnlyNumbers(chk.ID),
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
                            Controller.GetAvailablePowers(Model.AllPowers, Model.CharacterPowerPicks, ddlArchetype.SelectedValue.ToString(),
                            ddlPrimary.SelectedValue.ToString(), ddlSecondary.SelectedValue.ToString(), level),
                            nameof(Model.AllPowersFields.Power_DisplayName),
                            null, "Level " + level + ":");

                        // Disable all of the checkboxes, and take away their tags, because the next action will be a power pick
                        foreach (PowerPicker currentPP in GetPowerPickers(PowerPanel))
                        {
                            foreach (CheckBox chkCurrent in GetCheckBoxes(currentPP))
                            {
                                //Always just enable the first combobox, assign the action to it, then quit
                                chkCurrent.Enabled = false;
                                chkCurrent.SetTag(null);
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
                                chkCurrent.CheckedChanged -= new EventHandler(ActionHappened); //Unsubscribe to prevent duplicates
                                chkCurrent.CheckedChanged += new EventHandler(ActionHappened);
                                break;
                            }
                        }
                    }

                    break;
                case Model.Action.Pick2EnhancementSlots:
                case Model.Action.Pick3EnhancementSlots:
                    //Add the current enhancement slot to the enhancement slots list

                    //Get the PowerSet name and Power name from the parent's Tag
                    pp = chk == null ? (PowerPicker)ddl.Parent.Parent : (PowerPicker)chk.Parent.Parent;
                    dr = (DataRow)pp.GetTag();

                    //If the tag is null, then try to find it again via the name of the power from the character's picks
                    if (dr == null) dr = Model.CharacterPowerPicks.Select(nameof(Model.CharacterPowerPicksFields.Power_Name)
                        + " = '" + pp.DdlPowerPicker.SelectedValue.ToString() + "'")[0];
                    Controller.AddSlot(Model.CharacterEnhancementSlots, ddlArchetype.SelectedValue.ToString(),
                        dr[nameof(Model.CharacterPowerPicksFields.PowerSet_Name)].ToString(),
                        dr[nameof(Model.CharacterPowerPicksFields.Power_Name)].ToString(), Controller.GetOnlyNumbers(chk.ID),
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
                            chkCurrent.SetTag(newAction);
                            chkCurrent.CheckedChanged -= new EventHandler(ActionHappened); //Unsubscribe to prevent duplicates
                            chkCurrent.CheckedChanged += new EventHandler(ActionHappened);
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
            //Load the control   
            PowerPicker pp = (PowerPicker)Page.LoadControl("PowerPicker.ascx");
            pp.ParentForm = this;
            pp.ID = "pp" + LabelValue.Split(' ')[1].Replace(":", "");

            // Add the control to the panel  
            PowerPanel.Controls.Add(pp);
            pp.DdlPowerPicker.DataValueField = ValueMember;
            pp.DataSource = DataSource;
            pp.ValueMember = ValueMember;
            pp.DisplayMember = DisplayMember;
            pp.DdlPowerPicker.DataSource = DataSource;
            pp.DdlPowerPicker.DataTextField = DisplayMember;
            pp.DdlPowerPicker.DataBind();
            pp.DdlPowerPicker.SelectedIndexChanged -= pp.DdlIndex_Changed; //Have to unsubscribe before subscribing to prevent duplicate triggers
            pp.DdlPowerPicker.SelectedIndexChanged += new EventHandler(pp.DdlIndex_Changed);
            pp.SetTag(TagValue); //Note that we MUST add a control TO the page, before we can set the Tag, because otherwise the Control's "Page" value is null!
            pp.LblName.Text = LabelValue;
            pp.DdlPowerPicker.Enabled = true;
            if (Session[LastPP] != null) ((PowerPicker)Session[LastPP]).DdlPowerPicker.Enabled = false;
            if (Session[LastPP] == null)
            {
                pp.Attributes.Add("Top", "0");
                pp.Attributes.Add("Left", "0");
            }
            else
            {
                pp.Attributes.Add("Top", (((PowerPicker)Session[LastPP]).Attributes["Top"] + 100).ToString());
                pp.Attributes.Add("Left", (((PowerPicker)Session[LastPP]).Attributes["Left"] + 100).ToString());
            }
            pp.Focus();
            Session[LastPP] = pp;
            
            if (!SessionHasPowerPicker(pp)) ((List<PowerPicker>)Session[StoredPowerPickers]).Add(pp);
        }

        /// <summary>
        /// Verified whether or not the existing PowerPicker already exists in the Session
        /// </summary>
        /// <param name="pp"></param>
        /// <returns></returns>
        private bool SessionHasPowerPicker(PowerPicker pp)
        {
            if (DefMethods.IsNullOrEmpty(Session[StoredPowerPickers])) Session[StoredPowerPickers] = new List<PowerPicker>();
            foreach (PowerPicker ppTemp in (List<PowerPicker>)Session[StoredPowerPickers]) {
                if (ppTemp.ID == pp.ID) return true;
            }
            return false;
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

        protected void StartCharacter_Click(object sender, EventArgs e)
        {
            ActionHappened(sender, e);
        }

        protected void DdlSelectedIndex_Changed(object sender, EventArgs e)
        {
            ActionHappened(sender, e);
        }
    }
}