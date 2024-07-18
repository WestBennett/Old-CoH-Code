<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="PowerPicker.ascx.vb" Inherits="Philotic_Knights_Web_Field_Trainer.PowerPicker" %>
 <fieldset>
  <legend><asp:Label id="lblName" runat="server" Text="Level 1:" /></legend>
  
     <asp:DropDownList ID="ddlPowerPicker" runat="server" Height="16px" Width="181px">
     </asp:DropDownList>
     <br />
     <asp:CheckBox ID="chkSlot1" runat="server" EnableTheming="True" Text="1" />
     <asp:CheckBox ID="chkSlot2" runat="server" Text="2" />
     <asp:CheckBox ID="chkSlot3" runat="server" Text="3" />
     <asp:CheckBox ID="chkSlot4" runat="server" Text="4" />
     <asp:CheckBox ID="chkSlot5" runat="server" Text="5" />
     <asp:CheckBox ID="chkSlot6" runat="server" Text="6" />
  
 </fieldset>