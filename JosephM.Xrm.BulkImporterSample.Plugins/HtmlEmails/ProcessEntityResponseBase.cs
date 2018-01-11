using JosephM.Xrm.BulkImporterSample.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Xrm.BulkImporterSample.Plugins.HtmlEmails
{
    public class ProcessEntityResponseBase
    {
        public Entity Entity { get; set; }
        public Exception Exception { get; set; }

        public bool HasError
        {
            get { return Exception != null; }
        }

        public string ErrorMessage
        {
            get { return Exception == null ? null : Exception.Message; }
        }

        public string ErrorDetail
        {
            get { return Exception.XrmDisplayString(); }
        }
    }
}