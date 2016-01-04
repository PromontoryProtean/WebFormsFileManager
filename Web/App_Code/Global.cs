using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.UI;

public class Global : HttpApplication
{
    // ========================================================================
    // Application Events
    // ========================================================================

    public virtual void Application_Start(object sender, EventArgs e)
    {
        // WebApi routes
        RouteTable.Routes.MapHttpRoute(name: "Default WebApi Route", routeTemplate: "api/{controller}/{action}", defaults: new { id = RouteParameter.Optional });

        // route, bundle and script registration
        BundleTable.Bundles.Add(new ScriptBundle("~/bundles/WebFormsJs").Include(
              "~/Scripts/WebForms/WebForms.js",
              "~/Scripts/WebForms/WebUIValidation.js",
              "~/Scripts/WebForms/MenuStandards.js",
              "~/Scripts/WebForms/Focus.js",
              "~/Scripts/WebForms/GridView.js",
              "~/Scripts/WebForms/DetailsView.js",
              "~/Scripts/WebForms/TreeView.js",
              "~/Scripts/WebForms/WebParts.js"));

        BundleTable.Bundles.Add(new ScriptBundle("~/bundles/MsAjaxJs").Include(
            "~/Scripts/WebForms/MsAjax/MicrosoftAjax.js",
            "~/Scripts/WebForms/MsAjax/MicrosoftAjaxApplicationServices.js",
            "~/Scripts/WebForms/MsAjax/MicrosoftAjaxTimer.js",
            "~/Scripts/WebForms/MsAjax/MicrosoftAjaxWebForms.js"));

        ScriptManager.ScriptResourceMapping.AddDefinition("jquery", new ScriptResourceDefinition
        {
            Path = "~/Scripts/jquery.min.js"
        });
    }

    // ========================================================================
    // Session Events
    // ========================================================================

    //protected virtual void Session_Start(object sender, EventArgs e)
    //{
    //}

    // ========================================================================
    // Request Events
    // ========================================================================

    public void Application_Error(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EnableErrorLogging"]) && ConfigurationManager.AppSettings["EnableErrorLogging"].ToLower() == "true")
            HandleError();
    }

    //protected virtual void Application_BeginRequest(object sender, EventArgs e)
    //{
    //}

    // ========================================================================
    // private methods
    // ========================================================================

    private void HandleError()
    {
        var exception = Server.GetLastError();
        if (exception is HttpUnhandledException)
        {
            if (exception.InnerException != null)
                exception = exception.InnerException;
        }

        var dtNow = DateTime.UtcNow;
        var fileName = Server.MapPath("~/App_Data/Logs/" + dtNow.Date.Month.ToString() + "-" + dtNow.Date.Day.ToString() + "-" + dtNow.Date.Year.ToString() + ".txt");
        using (var sw = System.IO.File.AppendText(fileName))
        {
            sw.WriteLine("Request Object: " + Request.Url.ToString());
            sw.WriteLine(exception.ToString());
            sw.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------");
        }
    }
}