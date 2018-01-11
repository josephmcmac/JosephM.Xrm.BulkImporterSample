using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.BulkImporterSample.Plugins.Test
{
    //this class just for general debug purposes
    [TestClass]
    public class DebugTests : jmcg_bisXrmTest
    {
        [TestMethod]
        public void Debug()
        {
            var me = XrmService.WhoAmI();
        }
    }
}