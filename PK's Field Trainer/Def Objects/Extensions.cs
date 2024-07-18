namespace PKs_Def_Tools
{
    using System.Collections.Generic;
    using System.Web;
    using System.Web.UI;

    public static class Extensions
    {
        public static void SetTag(this Control ctl, object tagValue)
        {
            if (ctl.SessionTagDictionary().ContainsKey(TagName(ctl)))
                ctl.SessionTagDictionary()[TagName(ctl)] = tagValue;
            else
                ctl.SessionTagDictionary().Add(TagName(ctl), tagValue);
        }

        public static object GetTag(this Control ctl)
        {
            if (ctl.SessionTagDictionary().ContainsKey(TagName(ctl)))
                return ctl.SessionTagDictionary()[TagName(ctl)];
            else
                return string.Empty;
        }

        private static string TagName(Control ctl)
        {
            return ctl.Page.ClientID + "." + ctl.ClientID;
        }

        private static Dictionary<string, object> SessionTagDictionary(this Control ctl)
        {
            Dictionary<string, object> SessionTagDictionary;
            if (HttpContext.Current.Session["TagDictionary"] == null)
            {
                SessionTagDictionary = new Dictionary<string, object>();
                HttpContext.Current.Session["TagDictionary"] = SessionTagDictionary;
            }
            else
                SessionTagDictionary = (Dictionary<string, object>)HttpContext.Current.Session["TagDictionary"];
            return SessionTagDictionary;
        }
    }

}
