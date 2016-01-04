<%@ Page Language="C#" AutoEventWireup="false" CodeFile="default.aspx.cs" Inherits="_default" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Web Forms File Manager</title>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css" integrity="sha384-1q8mTJOASx8j1Au+a5WDVnPi2lkFfwwEAa8hDDdjZlpLegxhjVME1fgjWPGmkzs7" crossorigin="anonymous">
    <link href="/css/common.css" rel="stylesheet" />
    <script src="//code.jquery.com/jquery-1.11.3.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/js/bootstrap.min.js" integrity="sha384-0mSbJDEHialfmuBBQP6A4Qrprq5OVfW37PRR3j5ELqxss1yVqOtnepnHVP9aJ7xS" crossorigin="anonymous"></script>
    <script src="/Scripts/jquery.cookie/js.cookie-2.1.0.min.js"></script>
    <script src="/Scripts/plupload/plupload.full.min.js"></script>
    <script src="/Scripts/plupload/jquery.plupload.queue/jquery.plupload.queue.js"></script>
    <script src="/Scripts/file-manager.js"></script>
    <script type="text/javascript">
        var currentDir = "<%= Server.UrlEncode(_currentDirectory) %>";
    </script>
</head>
<body>
    <form id="WebForm" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server">
            <Scripts>
                <asp:ScriptReference Name="MsAjaxBundle" />
                <asp:ScriptReference Name="WebForms.js" Assembly="System.Web" Path="~/Scripts/WebForms/WebForms.js" />
                <asp:ScriptReference Name="WebUIValidation.js" Assembly="System.Web" Path="~/Scripts/WebForms/WebUIValidation.js" />
                <asp:ScriptReference Name="MenuStandards.js" Assembly="System.Web" Path="~/Scripts/WebForms/MenuStandards.js" />
                <asp:ScriptReference Name="GridView.js" Assembly="System.Web" Path="~/Scripts/WebForms/GridView.js" />
                <asp:ScriptReference Name="DetailsView.js" Assembly="System.Web" Path="~/Scripts/WebForms/DetailsView.js" />
                <asp:ScriptReference Name="TreeView.js" Assembly="System.Web" Path="~/Scripts/WebForms/TreeView.js" />
                <asp:ScriptReference Name="WebParts.js" Assembly="System.Web" Path="~/Scripts/WebForms/WebParts.js" />
                <asp:ScriptReference Name="Focus.js" Assembly="System.Web" Path="~/Scripts/WebForms/Focus.js" />
                <asp:ScriptReference Name="WebFormsBundle" />
            </Scripts>
        </asp:ScriptManager>
        <div class="container-fluid">
            <h1>Web Forms File Manager</h1>
            <div id="message" runat="server" class="alert" EnableViewState="false" Visible="false"></div>
            <div class="table-toolbar">
                <div id="file-view-mode-btn-group" class="btn-group" data-toggle="buttons">
                    <label id="DetailsViewButtonLabel" runat="server" class="btn btn-primary active">
                        <asp:RadioButton ID="DetailsViewButton" runat="server" ClientIDMode="Static" GroupName="FileManagerViewMode" value="Details" />
                        <i class="fa fa-list"></i>
                    </label>
                    <label id="TileViewButtonLabel" runat="server" class="btn btn-primary">
                        <asp:RadioButton ID="TileViewButton" runat="server" ClientIDMode="Static" GroupName="FileManagerViewMode" value="Tiles" />
                        <i class="fa fa-th"></i>
                    </label>
                </div>
                <div id="start-directory-btn-group" class="btn-group" data-toggle="buttons">
                    <label id="PublicFilesButtonLabel" runat="server" class="btn btn-primary active">
                        <asp:RadioButton ID="PublicFilesButton" runat="server" ClientIDMode="Static" GroupName="StartDirectory" />
                        Public Files
                    </label>
                    <label id="SecureFilesButtonLabel" runat="server" class="btn btn-primary">
                        <asp:RadioButton ID="SecureFilesButton" runat="server" ClientIDMode="Static" GroupName="StartDirectory" />
                        Secure Files
                    </label>
                </div>
                <button id="create-directory-button" type="button" class="btn btn-primary" data-toggle="modal" data-target="#create-directory-dialog">Create a Directory</button>
                <button id="upload-files-button" type="button" class="btn btn-primary" data-toggle="modal" data-target="#upload-files-dialog">Upload Files</button>
                <div id="BulkActionsButtons" runat="server" class="btn-group">
                    <button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown">
                        Bulk Actions <span class="caret"></span>
                    </button>
                    <ul class="dropdown-menu" role="menu">
                        <li>
                            <asp:LinkButton ID="DeleteAllButton" runat="server" OnClick="DeleteAllButton_Click" OnClientClick="return confirm('Deleting directories and files cannot be undone. Are you sure you want to delete the selected items?');">Delete Files</asp:LinkButton>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="breadcrumbs-wrap">
                <span>Current Directory: </span>
                <asp:Literal ID="Breadcrumbs" runat="server" />
            </div>
            <asp:ListView ID="DetailsView" runat="server" Visible="true" ItemType="System.IO.FileSystemInfo" SelectMethod="DetailsView_GetData" OnItemDataBound="DetailsView_ItemDataBound" OnItemCommand="DetailsView_ItemCommand">
                <EmptyDataTemplate>
                    <table class="table table-striped">
                        <thead>
                            <tr>
                                <th>&nbsp;</th>
                                <th>Size</th>
                                <th>Last Modified</th>
                                <th>&nbsp;</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td colspan="4">This directory is empty.</td>
                            </tr>
                        </tbody>
                    </table>
                </EmptyDataTemplate>
                <LayoutTemplate>
                    <table class="table table-striped file-manager">
                        <thead>
                            <tr>
                                <th class="checkbox-column">
                                    <input type="checkbox" id="SelectAllCheckbox" onchange="ToggleCheckboxes(this)" />
                                </th>
                                <th>Directory/File Name</th>
                                <th>Last Modified</th>
                                <th>Size</th>
                                <th>&nbsp;</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr id="itemPlaceholder" runat="server">
                            </tr>
                        </tbody>
                    </table>
                </LayoutTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="checkbox-column">
                            <asp:CheckBox ID="DetailsViewItemCheckbox" runat="server" />
                        </td>
                        <td>
                            <i id="Icon" runat="server"></i>&nbsp;
                            <asp:HyperLink ID="BrowseLink" runat="server"><%# Item.Name %></asp:HyperLink>
                            <asp:Literal ID="FileName" runat="server" Text='<%#: Item.Name %>' />
                        </td>
                        <td>
                            <%# Item.LastWriteTime.ToString("M/d/yyyy h:mm tt") %>
                        </td>
                        <td>
                            <asp:Literal ID="Size" runat="server" />
                        </td>
                        <td class="text-right command-col">
                            <asp:HyperLink ID="DownloadLink" runat="server">Download</asp:HyperLink>
                            <asp:LinkButton ID="DeleteButton" runat="server" CommandArgument='<%# Item.Name %>'>Delete</asp:LinkButton>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>
            <asp:ListView ID="TileView" runat="server" Visible="false" ItemType="System.IO.FileSystemInfo" SelectMethod="TileView_GetData" OnItemDataBound="TileView_ItemDataBound">
                <EmptyDataTemplate>
                    <p>
                        This directory is empty.
                    </p>
                </EmptyDataTemplate>
                <LayoutTemplate>
                    <div class="tile-list-wrap">
                        <ul class="tile-list">
                            <li id="itemPlaceholder" runat="server"></li>
                        </ul>
                        <div class="tile-details">
                            <h2>File Details</h2>
                            Click a file to see file details.
                        </div>
                    </div>
                </LayoutTemplate>
                <ItemTemplate>
                    <li>
                        <asp:Panel ID="IconWrap" runat="server" CssClass="tile-wrap">
                            <asp:HyperLink ID="BrowseLink" runat="server">
                                <img alt="Directory" src="/images/icons/125x125/folder.png" />
                            </asp:HyperLink>
                        </asp:Panel>
                        <asp:Panel ID="ThumbnailWrap" runat="server" CssClass="tile-wrap">
                            <asp:Image ID="Thumbnail" runat="server" />
                        </asp:Panel>
                        <div class="tile-name">
                            <label>
                                <asp:CheckBox ID="TileViewItemCheckbox" runat="server" />
                                <asp:Literal ID="FileName" runat="server" Text='<%#: Item.Name %>' />
                            </label>
                        </div>
                    </li>
                </ItemTemplate>
            </asp:ListView>
            <div class="modal fade" id="create-directory-dialog" tabindex="-1" role="dialog" aria-labelledby="create-directory-label" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                            <h4 class="modal-title" id="create-directory-label">Create a Directory</h4>
                        </div>
                        <div class="modal-body">
                            <asp:ValidationSummary ID="CreateDirectoryValSummary" runat="server" ValidationGroup="CreateDirectoryGroup" HeaderText="The following problems occured when submitting the form:" CssClass="alert alert-danger" />
                            <div class="form-group">
                                <asp:Label ID="CreateDirectoryLabel" runat="server" AssociatedControlID="CreateDirectoryField">Directory Name: </asp:Label>
                                <asp:TextBox ID="CreateDirectoryField" runat="server" CssClass="form-control" />
                                <asp:RequiredFieldValidator ID="reqCreateDirectoryField" runat="server" ValidationGroup="CreateDirectoryGroup" ControlToValidate="CreateDirectoryField" Display="None" ErrorMessage="Directory name is required." />
                                <asp:RegularExpressionValidator ID="rexCreateDirectoryField" runat="server" ValidationGroup="CreateDirectoryGroup" ControlToValidate="CreateDirectoryField" Display="None"
                                    ValidationExpression="^.{1,50}$" ErrorMessage="Invalid directory name." />
                            </div>
                            <asp:Button ID="CreateDirectoryButton" runat="server" OnClick="CreateDirectoryButton_Click" Text="Create Directory" ValidationGroup="CreateDirectoryGroup" CssClass="btn btn-primary" />
                        </div>
                    </div>
                </div>
            </div>
            <div class="modal fade" id="upload-files-dialog" tabindex="-1" role="dialog" aria-labelledby="upload-files-label" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                            <h4 class="modal-title" id="upload-files-label">Upload Files</h4>
                        </div>
                        <div class="modal-body">
                            <p>
                                <strong>Image Settings</strong><br />
                                <asp:Label ID="DoResizeImagesLabel" runat="server" AssociatedControlID="DoResizeImages">Resize Images</asp:Label>
                                <asp:CheckBox ID="DoResizeImages" runat="server" ClientIDMode="Static" Checked="true" />
                                &nbsp;&nbsp;<asp:Label ID="WidthLabel" runat="server" AssociatedControlID="WidthField">Width</asp:Label>
                                <asp:TextBox ID="WidthField" runat="server" CssClass="textbox-small" Text="1200" ClientIDMode="Static" />
                                &nbsp;&nbsp;<asp:Label ID="HeightLabel" runat="server" AssociatedControlID="HeightField">Height</asp:Label>
                                <asp:TextBox ID="HeightField" runat="server" CssClass="textbox-small" Text="1200" ClientIDMode="Static" />
                            </p>
                            <div id="uploader">
                                <p>You browser does not support HTML5.</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>