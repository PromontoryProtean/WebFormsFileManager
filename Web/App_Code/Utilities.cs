using System;
using System.IO;
using System.Web;

public class Utilities
{
    public static bool VerifyWritePermission(string siteRelativeDirectory)
    {
        try
        {
            var context = HttpContext.Current;
            var testFileName = context.Server.MapPath("~" + siteRelativeDirectory) + "\\WriteTest.txt";
            var dir = context.Server.MapPath("~" + siteRelativeDirectory);

            if (!Directory.Exists(dir))
                return false;

            using (var sw = File.CreateText(dir + "\\WriteTest.txt"))
            {
                sw.WriteLine("WriteTest");
            }

            if ((File.Exists(testFileName)))
                File.Delete(testFileName);

            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
}