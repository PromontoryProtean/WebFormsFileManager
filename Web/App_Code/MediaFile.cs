using ImageProcessor;
using ImageProcessor.Web.Helpers;
using System;
using System.Web.Hosting;

public class MediaFile
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileExtension { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime LastWriteUtc { get; set; }

    public long SizeKb { get; set; }

    private int? _width;
    public int Width
    {
        get
        {
            if (!_width.HasValue)
                GetDimensions();
            return _width.Value;
        }
    }

    private int? _height;
    public int Height
    {
        get
        {
            if (!_height.HasValue)
                GetDimensions();
            return _height.Value;
        }
    }

    private void GetDimensions()
    {
        if (IsImage)
        {
            using (var imageFactory = new ImageFactory())
            {
                var filePath = FilePath;
                if (filePath.StartsWith("/secure-files"))
                    filePath = filePath.Replace("/secure-files", "/App_Data/secure-files");
                imageFactory.Load(HostingEnvironment.MapPath("~" + filePath));
                var size = imageFactory.Image.Size;
                _width = size.Width;
                _height = size.Height;
            }
        }
        else
        {
            _width = 0;
            _height = 0;
        }
    }

    private string _dimensions;
    public string Dimensions
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_dimensions))
                _dimensions = Width.ToString() + "x" + Height.ToString();
            return _dimensions;
        }
    }

    private bool? _isImage;
    public bool IsImage
    {
        get
        {
            _isImage = _isImage == null ? ImageHelpers.IsValidImageExtension(FileName) : _isImage;
            return _isImage.Value;
        }
    }
}