using JosephM.Xrm.BulkImporterSample.Plugins.Services;
using JosephM.Xrm.BulkImporterSample.Plugins.Xrm;

namespace JosephM.Xrm.BulkImporterSample.Plugins.Plugins
{
    public class jmcg_bisEntityPluginBase : XrmEntityPlugin
    {
        //class for shared services or settings objects for plugins
        private jmcg_bisSettings _settings;
        public jmcg_bisSettings jmcg_bisSettings
        {
            get
            {
                if (_settings == null)
                    _settings = new jmcg_bisSettings(XrmService);
                return _settings;
            }
        }

        private jmcg_bisService _service;
        public jmcg_bisService jmcg_bisService
        {
            get
            {
                if (_service == null)
                    _service = new jmcg_bisService(XrmService, jmcg_bisSettings);
                return _service;
            }
        }
    }
}
