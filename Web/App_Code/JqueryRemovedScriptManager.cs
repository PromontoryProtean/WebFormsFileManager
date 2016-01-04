using System;
using System.Linq;
using System.Web.UI;

public class JqueryRemovedScriptManager : ScriptManager
{
    protected override void OnInit(EventArgs e)
    {
        Page.PreRenderComplete += Page_PreRenderComplete;
        base.OnInit(e);
    }

    private void Page_PreRenderComplete(object sender, EventArgs e)
    {
        var jqueryReferences = Scripts.Where(s => s.Name.Equals("jquery", StringComparison.OrdinalIgnoreCase)).ToList();
        if (jqueryReferences.Count > 0)
        {
            // Remove the jquery references as we're rendering it manually in the master page <head>
            foreach (var reference in jqueryReferences)
            {
                Scripts.Remove(reference);
            }
        }
    }
}