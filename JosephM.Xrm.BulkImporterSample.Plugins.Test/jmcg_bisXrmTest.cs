using JosephM.Xrm.BulkImporterSample.Plugins.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JosephM.Xrm.BulkImporterSample.Plugins.Test
{
    [TestClass]
    public class jmcg_bisXrmTest : XrmTest
    {
        //USE THIS IF NEED TO VERIFY SCRIPTS FOR A PARTICULAR SECURITY ROLE
        //private XrmService _xrmService;
        //public override XrmService XrmService
        //{
        //    get
        //    {
        //        if (_xrmService == null)
        //        {
        //            var xrmConnection = new XrmConfiguration()
        //            {
        //                AuthenticationProviderType = XrmConfiguration.AuthenticationProviderType,
        //                DiscoveryServiceAddress = XrmConfiguration.DiscoveryServiceAddress,
        //                OrganizationUniqueName = XrmConfiguration.OrganizationUniqueName,
        //                Username = "",
        //                Password = ""
        //            };
        //            _xrmService = new XrmService(xrmConnection);
        //        }
        //        return _xrmService;
        //    }
        //}

        protected override IEnumerable<string> EntitiesToDelete
        {
            get
            {
                return new string[0];
            }
        }

        //class for shared services or settings objects for tests
        //or extending base class logic
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
