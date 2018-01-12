using JosephM.Xrm.BulkImporterSample.Plugins.Workflows;
using JosephM.Xrm.BulkImporterSample.Plugins.Xrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace JosephM.Xrm.BulkImporterSample.Plugins.Test
{
    [TestClass]
    public class BulkImporterCreateRecordTest : jmcg_bisXrmTest
    {
        [TestMethod]
        public void BulkImporterCreateRecordTestTest()
        {
            var topic = "TESTIMPORTTOPIC";
            var email = "TESTIMPORTEMAIL@EXAMPLE.COM";
            var company = "TESTIMPORTCOMPANY";

            DeleteMatchingData(topic, email, company);

            var jSon = ParseDictionaryToObjectJson(new Dictionary<string, string>
            {
                { "First Name", "TESTIMPORTFIRSTNAME" },
                { "Last Name", "TESTIMPORTLASTNAME" },
                { "Email", email },
                { "Mobile Phone", "04 1234 5678" },
                { "Business Phone", "04 9876 5432" },
                { "Job Title", "Programmer" },
                { "Company", company },
                { "Topic", topic },
            });

            //invoke the workflow activity directly - this could be used to step through and/or debug the workflow activity code
            var activity = CreateWorkflowInstance<BulkImporterCreateRecordInstance>();
            var recordCreated = activity.GetOrCreate(jSon);

            //call the action, rather than invoke the workflow activity directly
            var req = new OrganizationRequest(Actions.jmcg_bis_BulkImporterCreateRecord.Name);
            req[Actions.jmcg_bis_BulkImporterCreateRecord.In.RowJson] = jSon;
            var response = XrmService.Execute(req);
            var outtedId = new Guid(response[Actions.jmcg_bis_BulkImporterCreateRecord.Out.RecordId].ToString());

            //verify the same record id is returned
            //the second call had identical details so should have matched the initial opportunity
            Assert.AreEqual(recordCreated.Id, outtedId);

            //lets create a second opportunity to verify the matching of contact with email requirement
            var jSon2 = ParseDictionaryToObjectJson(new Dictionary<string, string>
            {
                { "First Name", "email already exists in system" },
                { "Last Name", "so will use existing contact" },
                { "Email", email },
                { "Mobile Phone", "04 1234 5678" },
                { "Business Phone", "04 9876 5432" },
                { "Job Title", "Programmer" },
                { "Company", company },
                { "Topic", topic + topic },
            });

            var activity2 = CreateWorkflowInstance<BulkImporterCreateRecordInstance>();
            var recordCreated2 = activity2.GetOrCreate(jSon2);
            //it should have created a new opporutnity
            //but used the same contact as the initial opporutnity as he had the same email
            Assert.AreNotEqual(recordCreated.Id, recordCreated2.Id);
            Assert.AreEqual(recordCreated.GetLookupGuid(Fields.opportunity_.parentcontactid), recordCreated2.GetLookupGuid(Fields.opportunity_.parentcontactid));
            Assert.AreEqual(recordCreated.GetLookupGuid(Fields.opportunity_.customerid), recordCreated2.GetLookupGuid(Fields.opportunity_.customerid));
        }

        private void DeleteMatchingData(string topic, string email, string company)
        {
            var opportunties = XrmService.RetrieveAllAndClauses(Entities.opportunity, new[]
            {
                new ConditionExpression(Fields.opportunity_.name, ConditionOperator.In, new object[] { topic, topic + topic})
            });
            DeleteMultiple(opportunties);
            var contacts = XrmService.RetrieveAllAndClauses(Entities.contact, new[]
            {
                new ConditionExpression(Fields.contact_.emailaddress1, ConditionOperator.In, new object[] { email})
            });
            DeleteMultiple(contacts);
            var accounts = XrmService.RetrieveAllAndClauses(Entities.account, new[]
            {
                new ConditionExpression(Fields.account_.name, ConditionOperator.In, new object[] { company})
            });
            DeleteMultiple(accounts);
        }

        private static string ParseDictionaryToObjectJson(Dictionary<string, string> csvFields)
        {
            var serialiser = new DataContractJsonSerializer(typeof(Dictionary<string, string>), new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
            using (var stream = new MemoryStream())
            {
                serialiser.WriteObject(stream, csvFields);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }
    }
}