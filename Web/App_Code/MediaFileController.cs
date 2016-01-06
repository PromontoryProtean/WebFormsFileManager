using System.IO;
using System.Web;
using System.Web.Http;

public class MediaFileController : ApiController
{
    //[Authorize(Roles = "Administrator")]
    public MediaFile GetMediaFile(string path)
    {
        var filePath = path;
        if (filePath.StartsWith("/secure-files"))
            filePath = filePath.Replace("/secure-files", "/App_Data/secure-files");

        var mediaFile = new MediaFile();
        var fi = new FileInfo(HttpContext.Current.Server.MapPath("~" + filePath));
        mediaFile.FilePath = path;
        mediaFile.FileName = fi.Name;
        mediaFile.FileExtension = fi.Extension;
        mediaFile.CreatedUtc = fi.CreationTimeUtc;
        mediaFile.LastWriteUtc = fi.LastWriteTimeUtc;
        mediaFile.SizeKb = fi.Length / 1000;

        return mediaFile;
    }
}