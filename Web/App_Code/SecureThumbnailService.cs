using ImageProcessor.Web.Helpers;
using ImageProcessor.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

public class SecureThumbnailService : IImageService
{
    /// <summary>
    /// The prefix for the given implementation.
    /// I am using ashx instead of axd because IIS7 does not enable forms authentication by default on axd files
    /// </summary>
    private string _prefix = "secure-image.ashx";

    /// <summary>
    /// Gets or sets the prefix for the given implementation.
    /// <remarks>
    /// This value is used as a prefix for any image requests that should use this service.
    /// </remarks>
    /// </summary>
    public string Prefix
    {
        get
        {
            return _prefix;
        }

        set
        {
            _prefix = value;
            //var path = HttpContext.Current.Request.ApplicationPath;
            //throw new Exception(path);
            //_prefix = (path.TrimEnd('/') + "/" + value).TrimStart('/');
            //throw new Exception(_prefix);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the image service requests files from the locally based file system.
    /// </summary>
    public bool IsFileLocalService
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// Gets or sets any additional settings required by the service.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; }

    /// <summary>
    /// Gets or sets the white list of <see cref="System.Uri"/>.
    /// </summary>
    public Uri[] WhiteList { get; set; }

    /// <summary>
    /// Gets a value indicating whether the current request passes sanitizing rules.
    /// </summary>
    /// <param name="path">
    /// The image path.
    /// </param>
    /// <returns>
    /// <c>True</c> if the request is valid; otherwise, <c>False</c>.
    /// </returns>
    public bool IsValidRequest(string path)
    {
        return ImageHelpers.IsValidImageExtension(path);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureThumbnailService"/> class.
    /// </summary>
    public SecureThumbnailService()
    {
        Settings = new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets the image using the given identifier.
    /// </summary>
    /// <param name="id">
    /// The value identifying the image to fetch.
    /// </param>
    /// <returns>
    /// The <see cref="System.Byte"/> array containing the image data.
    /// </returns>
    public async Task<byte[]> GetImage(object id)
    {
        var context = HttpContext.Current;
        if (string.IsNullOrWhiteSpace(context.Request.QueryString["dir"]))
            throw new HttpException(404, "Not Found");
        string path = HostingEnvironment.MapPath("~/App_Data" + context.Request.QueryString["dir"] + "/" + id.ToString());

        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
            throw new HttpException(404, "Not Found");

        byte[] buffer;
        using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        {
            buffer = new byte[file.Length];
            await file.ReadAsync(buffer, 0, (int)file.Length);
        }

        return buffer;
    }
}