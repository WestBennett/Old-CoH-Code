<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PowerPicker.ascx.cs" Inherits="PKs_C_Web_Field_Trainer.PowerPicker" %>
 <fieldset>
  <legend><asp:Label id="lbl" runat="server" Text="Level 1:" /></legend>
  
     <asp:DropDownList ID="ddl" runat="server" Height="18px" Width="800px" OnSelectedIndexChanged="DdlIndex_Changed" AutoPostBack = "true">
     </asp:DropDownList>
     <br />
     <asp:CheckBox ID="chkSlot1" runat="server" Text="1" AutoPostBack="true" OnCheckedChanged="CheckedChanged" Enabled="False" />
     <asp:CheckBox ID="chkSlot2" runat="server" Text="2" AutoPostBack="true" OnCheckedChanged="CheckedChanged" Enabled="False" />
     <asp:CheckBox ID="chkSlot3" runat="server" Text="3" AutoPostBack="true" OnCheckedChanged="CheckedChanged" Enabled="False" />
     <asp:CheckBox ID="chkSlot4" runat="server" Text="4" AutoPostBack="true" OnCheckedChanged="CheckedChanged" Enabled="False" />
     <asp:CheckBox ID="chkSlot5" runat="server" Text="5" AutoPostBack="true" OnCheckedChanged="CheckedChanged" Enabled="False" />
     <asp:CheckBox ID="chkSlot6" runat="server" Text="6" AutoPostBack="true" OnCheckedChanged="CheckedChanged" Enabled="False" />
  
 </fieldset>