using System.IO;
using System.Web;

public class DownloadHandler : IHttpHandler
{
    public bool IsReusable
    {
        get { return false; }
    }

    public void ProcessRequest(HttpContext context)
    {
        var file = context.Request.QueryString["file"];
        if (!string.IsNullOrWhiteSpace(file) && context.Server.UrlDecode(file).StartsWith("/secure-files"))
            file = "/App_Data" + context.Server.UrlDecode(file);
        else if (!string.IsNullOrWhiteSpace(file))
        {
            file = context.Server.UrlDecode(file);
            if (!file.StartsWith(FileManagerSettings.UserUploadDirectory))
                throw new HttpException("Permission denied.");
        }

        file = context.Server.MapPath("~" + file);
        if (!File.Exists(file))
            throw new HttpException(404, "Not Found");

        var fileInfo = new FileInfo(file);

        context.Response.ContentType = "application/octet-stream";
        context.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileInfo.Name);
        context.Response.AddHeader("Content-Length", fileInfo.Length.ToString());
        context.Response.WriteFile(fileInfo.FullName);
        context.Response.End();
    }
}