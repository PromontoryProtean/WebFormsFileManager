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

        var m = new MediaFile();
        var fi = new FileInfo(HttpContext.Current.Server.MapPath("~" + filePath));
        m.FilePath = path;
        m.FileName = fi.Name;
        m.FileExtension = fi.Extension;
        m.CreatedUtc = fi.CreationTimeUtc;
        m.LastWriteUtc = fi.LastWriteTimeUtc;
        m.SizeKb = fi.Length / 1000;

        return m;
    }
}