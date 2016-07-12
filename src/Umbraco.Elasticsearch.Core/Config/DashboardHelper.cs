using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Umbraco.Elasticsearch.Core.Config
{
    /// <summary>
    /// Class DashboardHelper.
    /// </summary>
    public static class DashboardHelper
    {
        /// <summary>
        /// Ensures the section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="path">The path.</param>
        internal static void EnsureSection(string section, string caption, string path)
        {
            var configPath = HostingEnvironment.MapPath("~/config/dashboard.config");
            if (configPath == null)
                return;

            var configXml = XDocument.Load(configPath);
            var dashboardXml = configXml.XPathSelectElement("//dashBoard");
            if (dashboardXml != null)
            {
                var sectionXml = configXml.XPathSelectElement($"//section [@alias='{section}']");
                if (sectionXml == null)
                {
                    sectionXml =
                        XElement.Parse(
                            $"<section alias=\"{section}\"><areas><area>developer</area></areas><tab caption=\"{caption}\"><access><grant>admin</grant></access><control>{path}</control></tab></section>");
                    dashboardXml.Add(sectionXml);
                    configXml.Save(configPath);
                }
            }
        }

        /// <summary>
        /// Removes a section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="caption">The caption.</param>
        internal static void RemoveSection(string section, string caption)
        {
            var configPath = HostingEnvironment.MapPath("~/config/dashboard.config");
            if (configPath == null)
                return;

            var configXml = XDocument.Load(configPath);
            var sectionXml = configXml.XPathSelectElement($"//section [@alias='{section}']");
            sectionXml.Remove();
            configXml.Save(configPath);
        }
    }
}
