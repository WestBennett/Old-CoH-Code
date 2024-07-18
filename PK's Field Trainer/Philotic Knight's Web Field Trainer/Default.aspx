<%@ Page Title="Home Page" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.vb" Inherits="Philotic_Knights_Web_Field_Trainer.Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Philotic Knight&#39;s Web Field Trainer</h1>

        <div style="vertical-align: central">
            <asp:Button ID="btnStartNewCharacter" runat="server" Text="Start New Character" Width="159px" />
            &nbsp;<asp:Button ID="btnLoadExistingCharacter" runat="server" Text="Load Existing Character" Width="168px" />
            &nbsp;<asp:Button ID="btnNextAction" runat="server" Text="Next Action" />
        </div>
        <p>
        <asp:Table runat="server">
            <asp:TableHeaderRow>
                <asp:TableCell>
                    <asp:Label ID="lblCharacterName" runat="server" Text="Character Name:"></asp:Label>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:TextBox ID="txtCharacterName" runat="server"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Label ID="lblCharacterLevel" runat="server" Text="Character Level:"></asp:Label>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:TextBox ID="txtCharacterLevel" runat="server" Enabled ="False" TabIndex="-1" ReadOnly="True" Width="36px"></asp:TextBox>
                </asp:TableCell>

            </asp:TableHeaderRow>

        </asp:Table>
            </p>
        <p>
            <asp:Table runat="server">
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="right">
                        <asp:Label ID="lblArchetype" runat="server" Text="Archetype:"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="ddlArchetype" runat="server" Width="479px" Enabled ="False">
                        </asp:DropDownList>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="right">
                        <asp:Label ID="lblPrimary" runat="server" Text="Primary:"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="ddlPrimary" runat="server" Width="479px" Enabled ="False">
                        </asp:DropDownList>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="right">
                        <asp:Label ID="lblSecondary" runat="server" Text="Secondary:"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="ddlSecondary" runat="server" Width="479px" Enabled ="False">
                        </asp:DropDownList>
                    </asp:TableCell>
                </asp:TableRow>

            </asp:Table>
        </p>
    </div>

    <div class="row">
        <div class="col-md-4">
        </div>
    </div>

</asp:Content>
