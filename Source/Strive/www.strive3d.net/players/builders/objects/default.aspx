<%@ Page language="c#" Codebehind="default.aspx.cs" AutoEventWireup="false" Inherits="www.strive3d.net.players.builders.objects._default" %>
<%@ Register TagPrefix="Controls" TagName="Header" Src="~/players/Controls/Header.ascx" %>
<%@ Register TagPrefix="Controls" TagName="Footer" Src="~/players/Controls/Footer.ascx" %>
<%@ Import Namespace="System.Data" %>
<CONTROLS:HEADER id="Header1" title="Objects - List View" runat="Server"></CONTROLS:HEADER>
<table valign="top" style="vertical-align:top">
	<tr>
		<th>
			Mobiles</th>
		<td><A href="./TemplateMobile.aspx">[Add]</A></td>
		<th>
			Equipables</th>
		<td><A href="./TemplateItemEquipable.aspx?TemplateName=Mobile<%=www.strive3d.net.Utils.TabHref%>">[Add]</A></td>
		<th>
			Junk</th>
		<td><A href="./TemplateItemJunk.aspx?TemplateName=ItemJunk<%=www.strive3d.net.Utils.TabHref%>">[Add]</A></td>
		<th>
			Quaffables</th>
		<td><A href="./TemplateItemQuaffable.aspx?TemplateName=ItemQuaffable<%=www.strive3d.net.Utils.TabHref%>">[Add]</A></td>
		<th>
			Readables</th>
		<td><A href="./TemplateItemReadable.aspx?TemplateName=ItemReadable<%=www.strive3d.net.Utils.TabHref%>">[Add]</A></td>
		<th>
			Wieldables</th>
		<td><A href="./TemplateItemWieldable.aspx?TemplateName=ItemWieldable<%=www.strive3d.net.Utils.TabHref%>">[Add]</A></td>
		<th>
			Terrain</th>
		<td><A href="./TemplateTerrain.aspx?TemplateName=Terrain<%=www.strive3d.net.Utils.TabHref%>">[Add]</A></td>
	</tr>
	<tr>
		<td colSpan="2" valign="top" style="vertical-align:top"><asp:repeater id="TemplateMobileList" runat="server"><ItemTemplate><a href='./TemplateMobile.aspx?TemplateObjectID=<%#DataBinder.Eval(Container.DataItem, "TemplateObjectID")%>&amp;TemplateName=Mobile<%=www.strive3d.net.Utils.TabHref%>'><%#DataBinder.Eval(Container.DataItem, "TemplateObjectName")%></a><br />
				</ItemTemplate>
			</asp:repeater></td>
		<td colSpan="2" valign="top" style="vertical-align:top"><asp:Repeater ID="TemplateItemEquipableList" runat="server"><ItemTemplate><a href='./TemplateItemEquipable.aspx?TemplateObjectID=<%#DataBinder.Eval(Container.DataItem, "TemplateObjectID")%>&amp;TemplateName=ItemEquipable<%=www.strive3d.net.Utils.TabHref%>'><%#DataBinder.Eval(Container.DataItem, "TemplateObjectName")%></a><br />
				</ItemTemplate>
			</asp:Repeater></td>
		<td colSpan="2" valign="top" style="vertical-align:top"><asp:Repeater ID="TemplateItemJunkList" runat="server"><ItemTemplate><a href='./TemplateItemJunk.aspx?TemplateObjectID=<%#DataBinder.Eval(Container.DataItem, "TemplateObjectID")%>&amp;TemplateName=ItemJunk<%=www.strive3d.net.Utils.TabHref%>'><%#DataBinder.Eval(Container.DataItem, "TemplateObjectName")%></a><br />
				</ItemTemplate>
			</asp:Repeater></td>
		<td colSpan="2" valign="top" style="vertical-align:top"><asp:Repeater ID="TemplateItemQuaffableList" runat="server"><ItemTemplate><a href='./TemplateItemQuaffable.aspx?TemplateObjectID=<%#DataBinder.Eval(Container.DataItem, "TemplateObjectID")%>&amp;TemplateName=ItemQuaffable<%=www.strive3d.net.Utils.TabHref%>'><%#DataBinder.Eval(Container.DataItem, "TemplateObjectName")%></a><br />
				</ItemTemplate>
			</asp:Repeater></td>
		<td colSpan="2" valign="top" style="vertical-align:top"><asp:Repeater ID="TemplateItemReadableList" runat="server"><ItemTemplate><a href='./TemplateItemReadable.aspx?TemplateObjectID=<%#DataBinder.Eval(Container.DataItem, "TemplateObjectID")%>&amp;TemplateName=ItemReadable<%=www.strive3d.net.Utils.TabHref%>'><%#DataBinder.Eval(Container.DataItem, "TemplateObjectName")%></a><br />
				</ItemTemplate>
			</asp:Repeater></td>
		<td colSpan="2" valign="top" style="vertical-align:top"><asp:Repeater ID="TemplateItemWieldableList" runat="server"><ItemTemplate><a href='./TemplateItemWieldable.aspx?TemplateObjectID=<%#DataBinder.Eval(Container.DataItem, "TemplateObjectID")%>&amp;TemplateName=ItemWieldable<%=www.strive3d.net.Utils.TabHref%>'><%#DataBinder.Eval(Container.DataItem, "TemplateObjectName")%></a><br />
				</ItemTemplate>
			</asp:Repeater></td>
		<td colSpan="2" valign="top" style="vertical-align:top"><asp:Repeater ID="TemplateTerrainList" runat="server"><ItemTemplate><a href='./TemplateTerrain.aspx?TemplateObjectID=<%#DataBinder.Eval(Container.DataItem, "TemplateObjectID")%>&amp;TemplateName=Terrain<%=www.strive3d.net.Utils.TabHref%>'><%#DataBinder.Eval(Container.DataItem, "TemplateObjectName")%></a><br />
				</ItemTemplate>
			</asp:Repeater></td>
	</tr>
</table>
<Controls:Footer runat="server" ID="Footer1" NAME="Footer1" />