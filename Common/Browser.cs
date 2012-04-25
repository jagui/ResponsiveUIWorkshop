using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Browser
    {
        public static  void SuppressScriptErrors(System.Windows.Controls.WebBrowser wb, bool Hide)
        {
            FieldInfo fi = typeof(System.Windows.Controls.WebBrowser).GetField(
                "_axIWebBrowser2",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (fi != null)
            {
                object browser = fi.GetValue(wb);

                if (browser != null)
                {
                    browser.GetType().InvokeMember("Silent", BindingFlags.SetProperty,
                        null, browser, new object[] { Hide });

                }
            }
        }
    }
}
