using ImageProcessor.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

public partial class _default : System.Web.UI.Page
{
    protected string _rootDirectory;
    protected string _currentDirectory;

    protected override void OnInit(EventArgs e)
    {
        // get the current directory
        _currentDirectory = Request.QueryString["dir"];
        if (!string.IsNullOrWhiteSpace(_currentDirectory) && Server.UrlDecode(_currentDirectory).StartsWith("/secure-files"))
        {
            _rootDirectory = "/App_Data";
            _currentDirectory = Server.UrlDecode(_currentDirectory);
        }
        else if (!string.IsNullOrWhiteSpace(_currentDirectory))
        {
            _rootDirectory = "";
            _currentDirectory = Server.UrlDecode(_currentDirectory);
            if (!_currentDirectory.StartsWith(FileManagerSettings.UserUploadDirectory))
                throw new HttpException("Access is denied on the user upload directory.");
        }
        else
        {
            _rootDirectory = "";
            _currentDirectory = FileManagerSettings.UserUploadDirectory;
        }

        // get breadcrumbs
        var crumbs = new StringBuilder();
        var url = new StringBuilder();
        int i = 0;
        var dirCount = _currentDirectory.Length - _currentDirectory.Replace("/", "").Length;

        var qs = HttpUtility.ParseQueryString(Request.QueryString.ToString());
        foreach (string crumb in _currentDirectory.Remove(0, 1).Split('/'))
        {
            url.Append("/" + crumb);
            i += 1;
            if (i == dirCount)
                crumbs.Append("<li>" + crumb + "</li>" + Environment.NewLine);
            else
            {
                qs.Set("dir", Server.UrlEncode(url.ToString()));
                crumbs.Append("<li><a href=\"" + Request.Url.LocalPath + "?" + qs + "\">" + crumb + "</a></li>" + Environment.NewLine);
            }
        }
        crumbs.Insert(0, "<ol class=\"breadcrumb\">" + Environment.NewLine);
        crumbs.Append(Environment.NewLine + "</ol>");
        Breadcrumbs.Text = crumbs.ToString();

        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            // view mode
            var fileManagerViewModeCookie = Request.Cookies["FileManagerViewMode"];
            if (fileManagerViewModeCookie != null)
            {
                if (fileManagerViewModeCookie.Value == "Tiles")
                {
                    TileViewButton.Checked = true;
                    DetailsViewButtonLabel.Attributes.Add("class", "btn btn-primary");
                    TileViewButtonLabel.Attributes.Add("class", "btn btn-primary active");
                    DetailsView.Visible = false;
                    TileView.Visible = true;
                }
            }

            // start directory
            var qs = HttpUtility.ParseQueryString(Request.QueryString.ToString());
            qs.Remove("dir");
            if (qs.Count > 0)
                PublicFilesButton.Attributes.Add("value", Request.Url.LocalPath + "?" + qs);
            else
                PublicFilesButton.Attributes.Add("value", Request.Url.LocalPath);
            qs.Set("dir", Server.UrlEncode("/secure-files"));
            SecureFilesButton.Attributes.Add("value", Request.Url.LocalPath + "?" + qs);
            if (!string.IsNullOrWhiteSpace(_currentDirectory) && Server.UrlDecode(_currentDirectory).StartsWith("/secure-files"))
            {
                SecureFilesButton.Checked = true;
                PublicFilesButtonLabel.Attributes.Add("class", "btn btn-primary");
                SecureFilesButtonLabel.Attributes.Add("class", "btn btn-primary active");
            }

            // image upload settings
            if (Request.Cookies["ImageUploadSettings"] != null)
            {
                var doResizeImages = false;
                bool.TryParse(Request.Cookies["ImageUploadSettings"]["doResizeImages"], out doResizeImages);
                DoResizeImages.Checked = doResizeImages;

                var width = 0;
                int.TryParse(Request.Cookies["ImageUploadSettings"]["width"], out width);
                WidthField.Text = width > 0 ? width.ToString() : "";

                var height = 0;
                int.TryParse(Request.Cookies["ImageUploadSettings"]["height"], out height);
                HeightField.Text = height > 0 ? height.ToString() : "";
            }
        }

        base.OnLoad(e);
    }

    public IEnumerable<FileSystemInfo> DetailsView_GetData()
    {
        return GetData();
    }

    protected void DetailsView_ItemDataBound(object sender, ListViewItemEventArgs args)
    {
        var dataItem = (ListViewDataItem)args.Item;
        if (dataItem == null)
            return;

        // altering the checkbox in code behind because if I do it inline asp.net will wrap the checkbox in a span tag
        ((CheckBox)dataItem.FindControl("DetailsViewItemCheckbox")).InputAttributes.Add("class", "DetailsViewItemCheckbox");
        var icon = (HtmlGenericControl)dataItem.FindControl("Icon");
        var browseLink = (HyperLink)dataItem.FindControl("BrowseLink");
        var fileName = (Literal)dataItem.FindControl("FileName");
        var downloadLink = (HyperLink)dataItem.FindControl("DownloadLink");
        var deleteButton = (LinkButton)dataItem.FindControl("DeleteButton");

        if (dataItem.DataItem is DirectoryInfo)
        {
            var directoryInfo = (DirectoryInfo)dataItem.DataItem;
            var qs = System.Web.HttpUtility.ParseQueryString(Request.QueryString.ToString());
            qs.Set("dir", Server.UrlEncode(_currentDirectory + "/" + directoryInfo.Name));

            icon.Attributes.Add("class", "fa fa-folder-open fa-fw icon-yellow");
            browseLink.NavigateUrl = Request.Url.LocalPath + "?" + qs; ;
            fileName.Visible = false;
            downloadLink.Visible = false;
            deleteButton.CommandName = "DeleteDirectory";
            deleteButton.OnClientClick = "return confirm('Deleting a directory will delete all subdirectories and files. Are you sure you want to delete this directory?');";
        }
        else if (dataItem.DataItem is FileInfo)
        {
            var fileInfo = (FileInfo)dataItem.DataItem;
            icon.Attributes.Add("class", GetIconClass(fileInfo.Extension));
            browseLink.Visible = false;

            var size = (Literal)dataItem.FindControl("Size");
            var sizeKb = fileInfo.Length / 1000;
            var sizeString = string.Format("{0:N0} KB", sizeKb);
            size.Text = sizeString;

            downloadLink.NavigateUrl = "/download.ashx?file=" + Server.UrlEncode(_currentDirectory + "/" + fileInfo.Name);
            deleteButton.CommandName = "DeleteFile";
            deleteButton.OnClientClick = "return confirm('Are you sure you want to delete this file?');";
        }
    }

    protected void DetailsView_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.CommandName == "DeleteDirectory")
        {
            string fullFolderPath = Server.MapPath("~" + _rootDirectory + _currentDirectory + "/" + Convert.ToString(e.CommandArgument));
            Directory.Delete(fullFolderPath, true);
            Response.Redirect(Request.Url.PathAndQuery);
        }
        else if (e.CommandName == "DeleteFile")
        {
            string fullFilePath = Server.MapPath("~" + _rootDirectory + _currentDirectory + "/" + Convert.ToString(e.CommandArgument));
            File.Delete(fullFilePath);
            Response.Redirect(Request.Url.PathAndQuery);
        }
    }

    public IEnumerable<FileSystemInfo> TileView_GetData()
    {
        return GetData();
    }

    protected void TileView_ItemDataBound(object sender, ListViewItemEventArgs args)
    {
        var dataItem = (ListViewDataItem)args.Item;
        if (dataItem == null)
            return;

        var iconWrap = (Panel)dataItem.FindControl("IconWrap");
        var thumbnailWrap = (Panel)dataItem.FindControl("ThumbnailWrap");

        if (dataItem.DataItem is DirectoryInfo)
        {
            var directoryInfo = (DirectoryInfo)dataItem.DataItem;
            var qs = HttpUtility.ParseQueryString(Request.QueryString.ToString());
            qs.Set("dir", Server.UrlEncode(_currentDirectory + "/" + directoryInfo.Name));
            thumbnailWrap.Visible = false;
            ((HyperLink)dataItem.FindControl("BrowseLink")).NavigateUrl = Request.Url.LocalPath + "?" + qs;
        }
        else if (dataItem.DataItem is FileInfo)
        {
            iconWrap.Visible = false;
            var fileInfo = (FileInfo)dataItem.DataItem;
            var thumbnail = (Image)dataItem.FindControl("Thumbnail");

            if (ImageHelpers.IsValidImageExtension(fileInfo.Name))
            {
                thumbnail.ImageUrl = (!string.IsNullOrWhiteSpace(_currentDirectory) && Server.UrlDecode(_currentDirectory).StartsWith("/secure-files"))
                    ? "/secure-image.ashx/" + fileInfo.Name + "?width=125&height=125&mode=max&dir=" + Server.UrlEncode(_currentDirectory)
                    : _currentDirectory + "/" + fileInfo.Name + "?width=125&height=125&mode=max";
                thumbnail.Attributes.Add("data-path", _currentDirectory + "/" + fileInfo.Name);
                thumbnail.CssClass = "thumb";
            }
            else
            {
                thumbnail.ImageUrl = "/images/icons/125x125/file.png";
            }
        }
    }

    protected void DeleteAllButton_Click(object sender, EventArgs e)
    {
        if (DetailsView.Visible)
        {
            foreach (ListViewDataItem i in DetailsView.Items)
            {
                if (i.ItemType == ListViewItemType.DataItem)
                {
                    var cb = (CheckBox)i.FindControl("DetailsViewItemCheckbox");
                    if (cb.Checked)
                    {
                        var l = (Literal)i.FindControl("FileName");
                        string path = Server.MapPath("~" + _rootDirectory + _currentDirectory + "/" + l.Text);
                        if (((HyperLink)i.FindControl("BrowseLink")).Visible)
                            Directory.Delete(path, true);
                        else
                            File.Delete(path);
                    }
                }
            }
        }
        else
        {
            foreach (ListViewDataItem i in TileView.Items)
            {
                if (i.ItemType == ListViewItemType.DataItem)
                {
                    var cb = (CheckBox)i.FindControl("TileViewItemCheckbox");
                    if (cb.Checked)
                    {
                        var l = (Literal)i.FindControl("FileName");
                        var path = Server.MapPath("~" + _rootDirectory + _currentDirectory + "/" + l.Text);
                        if (((Panel)i.FindControl("IconWrap")).Visible)
                            Directory.Delete(path, true);
                        else
                            File.Delete(path);
                    }
                }
            }

        }

        Response.Redirect(Request.Url.PathAndQuery);
    }

    protected void CreateDirectoryButton_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        if (!Utilities.VerifyWritePermission(_rootDirectory + _currentDirectory))
        {
            message.InnerHtml = "Permissions have not been set correctly on the currently selected directory. Please contact the system admin.";
            message.Attributes.Add("class", "alert-danger");
            message.Visible = true;
            return;
        }

        var newDirectoryName = CreateDirectoryField.Text.Slugify();
        Directory.CreateDirectory(Server.MapPath("~" + _rootDirectory + _currentDirectory + "/" + newDirectoryName));

        var qs = HttpUtility.ParseQueryString(Request.QueryString.ToString());
        qs.Set("dir", Server.UrlEncode(_currentDirectory + "/" + newDirectoryName));
        Response.Redirect(Request.Url.LocalPath + "?" + qs);
    }

    private IEnumerable<FileSystemInfo> GetData()
    {
        var dir = new DirectoryInfo(Server.MapPath("~" + _rootDirectory + _currentDirectory));
        var files = dir.GetFileSystemInfos();

        var l = new List<FileSystemInfo>();
        foreach (var fileSystemInfo in files)
        {
            if (fileSystemInfo is DirectoryInfo)
                l.Add(fileSystemInfo);
        }
        foreach (var fileSystemInfo in files)
        {
            if (fileSystemInfo is FileInfo)
                l.Add(fileSystemInfo);
        }

        return l;
    }

    private string GetIconClass(string fileExtension)
    {
        string iconPath = "";
        switch (fileExtension.ToLower())
        {
            case ".bmp":
            case ".gif":
            case ".jpg":
            case ".jpeg":
            case ".png":
            case ".tif":
                iconPath = "fa fa-file-image-o fa-fw";
                break;
            case ".avi":
            case ".fla":
            case ".mpeg":
            case ".mpg":
            case ".swf":
                iconPath = "fa fa-file-video-o fa-fw";
                break;
            case ".aac":
            case ".aif":
            case ".au":
            case ".flac":
            case ".m4a":
            case ".midi":
            case ".mp3":
            case ".ogg":
            case ".wav":
            case ".wma":
                iconPath = "fa fa-file-audio-o fa-fw";
                break;
            case ".doc":
            case ".docx":
                iconPath = "fa fa-file-word-o fa-fw";
                break;
            case ".pdf":
                iconPath = "fa fa-file-pdf-o fa-fw";
                break;
            case ".ppt":
            case ".pptx":
                iconPath = "fa fa-file-powerpoint-o fa-fw";
                break;
            case ".txt":
                iconPath = "fa fa-file-text-o fa-fw";
                break;
            case ".xls":
            case ".xlsx":
                iconPath = "fa fa-file-excel-o fa-fw";
                break;
            case ".zip":
                iconPath = "fa fa-file-archive-o fa-fw";
                break;
            default:
                iconPath = "fa fa-file-o fa-fw";
                break;
        }
        return iconPath;
    }
}