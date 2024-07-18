<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PKs_C_Web_Field_Trainer.DefaultForm" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Philotic Knight&#39;s Web Field Trainer</h1>
        <asp:Label ID="StatusLabel" runat="server" Text=""></asp:Label>
        <div style="vertical-align: central">
            <asp:Button ID="btnStartNewCharacter" runat="server" Text="Start New Character" Width="159px" />
            &nbsp;<asp:Button ID="btnLoadExistingCharacter" runat="server" Text="Load Existing Character" Width="168px" />
        </div>
        <p>
            <asp:Table runat="server" ID="tblHeader">
                <asp:TableHeaderRow>
                    <asp:TableCell>
                        <asp:Label ID="lblCharacterName" runat="server" Text="Character Name:"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtCharacterName" runat="server"></asp:TextBox><asp:Button ID="btnStartCharacter" runat="server" Text="Begin" OnClick="StartCharacter_Click" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:Label ID="lblCharacterLevel" runat="server" Text="Character Level:"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtCharacterLevel" runat="server" Enabled="False" TabIndex="-1" ReadOnly="True" Width="36px" Text ="-3"></asp:TextBox>
                    </asp:TableCell>

                </asp:TableHeaderRow>

            </asp:Table>
        </p>
        <p>
            <asp:Table runat="server" ID="tblPrimary">
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="right">
                        <asp:Label ID="lblArchetype" runat="server" Text="Archetype:"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="ddlArchetype" runat="server" Width="479px" Enabled="False" AutoPostBack="true" OnSelectedIndexChanged="DdlSelectedIndex_Changed">
                        </asp:DropDownList>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="right">
                        <asp:Label ID="lblPrimary" runat="server" Text="Primary:"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="ddlPrimary" runat="server" Width="479px" Enabled="False" AutoPostBack="true" OnSelectedIndexChanged="DdlSelectedIndex_Changed">
                        </asp:DropDownList>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="right">
                        <asp:Label ID="lblSecondary" runat="server" Text="Secondary:"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="ddlSecondary" runat="server" Width="479px" Enabled="False" AutoPostBack="true" OnSelectedIndexChanged="DdlSelectedIndex_Changed">
                        </asp:DropDownList>
                    </asp:TableCell>
                </asp:TableRow>

            </asp:Table>
        </p>
        <asp:Panel ID="PowerPanel" runat="server" Height="1000px">
        </asp:Panel>
    </div>

    <div class="row">
        <div class="col-md-4">
        </div>
    </div>

</asp:Content>
