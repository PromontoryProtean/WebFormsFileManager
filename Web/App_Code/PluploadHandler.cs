using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Web.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Web;

public class PluploadHandler : IHttpHandler
{
    public bool IsReusable
    {
        get { return false; }
    }

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        // Plupload will look at the HTTP status code to determine what happened with the request, but it also utilizes its own error codes which get carried in the json response
        // See: http://www.plupload.com/punbb/viewtopic.php?id=394 "The existing error event is for HTTP status responses and other I/O related errors that can be detected by the runtimes."
        // HTTP status code 500 indicates a generic server side error
        // I am sending the json for completeness, though I do not *think* Plupload is using it

        if (context.Request.Files.Count > 0)
        {
            var uploadDirectory = context.Request.QueryString["dir"];
            if (!string.IsNullOrWhiteSpace(uploadDirectory) && context.Server.UrlDecode(uploadDirectory).StartsWith("/secure-files"))
            {
                uploadDirectory = "/App_Data" + context.Server.UrlDecode(uploadDirectory);
            }
            else if (!string.IsNullOrWhiteSpace(uploadDirectory))
            {
                uploadDirectory = context.Server.UrlDecode(uploadDirectory);
                if (!uploadDirectory.StartsWith(FileManagerSettings.UserUploadDirectory))
                {
                    context.Response.StatusCode = 500;
                    context.Response.Write("{\"jsonrpc\" : \"2.0\", \"error\" : {\"code\": 400, \"message\": \"Access is denied on the upload directory.\"}, \"id\" : \"id\"}");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"jsonrpc\" : \"2.0\", \"error\" : {\"code\": 400, \"message\": \"An upload directory was not specified.\"}, \"id\" : \"id\"}");
            }

            var hasPermission = Utilities.VerifyWritePermission(uploadDirectory);
            if (!hasPermission)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"jsonrpc\" : \"2.0\", \"error\" : {\"code\": 400, \"message\": \"Permissions have not been set correctly on the currently selected folder.\"}, \"id\" : \"id\"}");
            }
            else
            {
                var fileName = context.Request["name"] != null ? context.Request["name"] : string.Empty;
                fileName = Path.GetFileNameWithoutExtension(fileName).Slugify() + Path.GetExtension(fileName).ToLower();

                var chunks = context.Request["chunks"] != null ? int.Parse(context.Request["chunks"]) : 0;
                var chunk = context.Request["chunk"] != null ? int.Parse(context.Request["chunk"]) : 0;
                var isLastChunk = (chunk >= (chunks - 1)) ? true : false;

                var postedFile = context.Request.Files[0];
                var savePath = context.Server.MapPath("~" + uploadDirectory + "/");
                using (var fs = new FileStream(Path.Combine(savePath, fileName), chunk == 0 ? FileMode.Create : FileMode.Append))
                {
                    var buffer = new byte[postedFile.InputStream.Length];
                    postedFile.InputStream.Read(buffer, 0, buffer.Length);
                    fs.Write(buffer, 0, buffer.Length);
                }

                if (isLastChunk && ImageHelpers.IsValidImageExtension(fileName))
                {
                    // check the querystring for image properties and validate them if they exist
                    string widthQs = context.Request.QueryString["width"];
                    string heightQs = context.Request.QueryString["height"];
                    int width = 0;
                    int.TryParse(widthQs, out width);
                    if (width > 4000)
                        width = 0;
                    int height = 0;
                    int.TryParse(heightQs, out height);
                    if (height > 4000)
                        height = 0;

                    if ((width > 0 || height > 0))
                    {
                        var sourceFilePath = Path.Combine(savePath, fileName);
                        var newFilePath = Path.Combine(savePath, Guid.NewGuid().ToString());

                        var resizeLayer = new ResizeLayer(new Size(width, height), ResizeMode.Max, AnchorPosition.Center, false);
                        using (var imageFactory = new ImageFactory())
                        {
                            imageFactory.Load(sourceFilePath)
                                        .Resize(resizeLayer)
                                        .Save(newFilePath);
                        }

                        File.Delete(sourceFilePath);
                        File.Move(newFilePath, sourceFilePath);
                    }
                }

                context.Response.Write("{\"jsonrpc\" : \"2.0\", \"result\" : null, \"id\" : \"id\"}");
            }
        }
    }
}