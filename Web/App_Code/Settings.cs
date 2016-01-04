using System;
using System.Configuration;

public static class FileManagerSettings
{
    private static string _userUploadDirectory;
    public static string UserUploadDirectory
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_userUploadDirectory))
            {
                _userUploadDirectory = ConfigurationManager.AppSettings["UserUploadDirectory"];
                if (string.IsNullOrWhiteSpace(_userUploadDirectory))
                    throw new Exception("Invalid UserUploadDirectory setting. Verify the Web.config file.");
            }
            return _userUploadDirectory;
        }
    }
}