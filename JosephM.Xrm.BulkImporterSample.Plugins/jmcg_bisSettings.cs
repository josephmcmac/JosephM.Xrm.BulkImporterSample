using JosephM.Xrm.BulkImporterSample.Plugins.Services;
using JosephM.Xrm.BulkImporterSample.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Schema;
using System;

namespace JosephM.Xrm.BulkImporterSample.Plugins
{
    /// <summary>
    /// A settings object which loads the first record of a given type for accessing its fields/properties
    /// </summary>
    public class jmcg_bisSettings
    {
        private XrmService XrmService { get; set; }

        public jmcg_bisSettings(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        private const string EntityType = Entities.organization;

        private Entity _settings;

        public Entity Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = XrmService.GetFirst(EntityType);
                    if (_settings == null)
                        throw new NullReferenceException(
                            string.Format(
                                "Error getting the {0} record. It does not exist or you do not have permissions to view it",
                                XrmService.GetEntityLabel(EntityType)));
                }
                return _settings;
            }
            set { _settings = value; }
        }

        public string OrganisationName
        {
            get
            {
                return Settings.GetStringField(Fields.organization_.name);
            }
        }

        public string WebIntegrationUrl { get { throw new NotImplementedException(); } }
        public string WebIntegrationPasscode { get { throw new NotImplementedException(); } }
    }
}