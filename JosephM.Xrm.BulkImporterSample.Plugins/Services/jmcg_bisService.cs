using JosephM.Xrm.BulkImporterSample.Plugins.Xrm;

namespace JosephM.Xrm.BulkImporterSample.Plugins.Services
{
    /// <summary>
    /// A service class for performing logic
    /// </summary>
    public class jmcg_bisService
    {
        private XrmService XrmService { get; set; }
        private jmcg_bisSettings jmcg_bisSettings { get; set; }

        public jmcg_bisService(XrmService xrmService, jmcg_bisSettings settings)
        {
            XrmService = xrmService;
            jmcg_bisSettings = settings;
        }
    }
}
