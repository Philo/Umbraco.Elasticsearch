using System.Linq;
using Umbraco.Core;

namespace Umbraco.Elasticsearch.Core.Admin
{
    public class UmbracoStartup : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            AddSection(applicationContext);
        }

        /// <summary>
        /// Adds the application/custom section to Umbraco
        /// </summary>
        /// <param name="applicationContext"></param>
        public void AddSection(ApplicationContext applicationContext)
        {
            //Get SectionService
            var sectionService = applicationContext.Services.SectionService;

            //Try & find a section with the alias of "analyticsSection"
            var searchSection = sectionService.GetSections().SingleOrDefault(x => x.Alias == "searchSection");

            //If we can't find the section - doesn't exist
            if (searchSection == null)
            {
                //So let's create it the section
                sectionService.MakeNew("searchSection", "searchSection", "icon-search", 15);
            }
        }
    }
}
