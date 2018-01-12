using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using JosephM.Xrm.BulkImporterSample.Plugins.Xrm;
using System.Collections.Generic;
using System.Linq;
using Schema;
using System;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Microsoft.Xrm.Sdk.Query;

namespace JosephM.Xrm.BulkImporterSample.Plugins.Workflows
{
    /// <summary>
    /// This class is for the static type required for registration of the custom workflow activity in CRM
    /// </summary>
    public class BulkImporterCreateRecord : XrmWorkflowActivityRegistration
    {
        [Input("Row Json")]
        public InArgument<string> RowJson { get; set; }

        [Output("Record Id")]
        public OutArgument<string> RecordId { get; set; }

        [Output("Record Type")]
        public OutArgument<string> RecordType { get; set; }

        protected override XrmWorkflowActivityInstanceBase CreateInstance()
        {
            return new BulkImporterCreateRecordInstance();
        }
    }

    /// <summary>
    /// This class is instantiated per execution
    /// </summary>
    public class BulkImporterCreateRecordInstance
        : jmcg_bisWorkflowActivity<BulkImporterCreateRecord>
    {
        protected override void Execute()
        {
            var jSon = ActivityThisType.RowJson.Get(ExecutionContext);
            var record = GetOrCreate(jSon);
            ActivityThisType.RecordType.Set(ExecutionContext, record.LogicalName);
            ActivityThisType.RecordId.Set(ExecutionContext, record.Id.ToString());
        }

        public Entity GetOrCreate(string rowJson)
        {
            var theDictionary = ParseJsonToDictionary(rowJson);
            var targetRecord = GetOrCreate(theDictionary, OpportunityImportMapping);
            return targetRecord;
        }

        private Entity GetOrCreate(Dictionary<string, object> theDictionary, IRecurseFieldCreation mappings)
        {
            var targetRecord = new Entity(mappings.TargetType);
            if (mappings.ExplicitValues != null)
            {
                foreach (var field in mappings.ExplicitValues)
                {
                    targetRecord.SetField(field.Name, XrmService.ParseField(field.Name, targetRecord.LogicalName, field.Value));
                }
            }
            foreach (var field in mappings.FieldMappings)
            {
                var type = field.TargetType;
                if (!string.IsNullOrWhiteSpace(type) && field.FieldMappings != null)
                {
                    var got = GetOrCreate(theDictionary, field);
                    targetRecord.SetLookupField(field.Name, got.Id, got.LogicalName);
                }
                else
                {
                    var value = theDictionary.ContainsKey(field.CsvName)
                        ? theDictionary[field.CsvName]
                        : null;
                    targetRecord.SetField(field.Name, XrmService.ParseField(field.Name, targetRecord.LogicalName, value));
                }
            }

            var matchExistingFields = mappings.KeyMappings.ToDictionary(m => m.Name, m => XrmService.ConvertToQueryValue(m.Name, targetRecord.LogicalName, targetRecord.GetField(m.Name)))
                .ToDictionary(kv => kv.Key, kv => (object)kv.Value);

            var emptyValues = matchExistingFields.Where(kv => kv.Value == null);
            if (emptyValues.Any())
            {
                throw new NullReferenceException(string.Format("The field {0} on type {1} is required", XrmService.GetFieldLabel(emptyValues.First().Key, targetRecord.LogicalName), XrmService.GetEntityLabel(targetRecord.LogicalName)));
            }

            matchExistingFields.Add("statecode", XrmPicklists.State.Active);
            var matches = GetMatchingEntities(targetRecord.LogicalName, matchExistingFields);
            if (matches.Any())
            {
                targetRecord.Id = matches.First().Id;
            }
            else
            {
                targetRecord.Id = XrmService.Create(targetRecord);
            }

            return targetRecord;
        }

        private TypeMapping OpportunityImportMapping
        {
            get
            {
                return new TypeMapping()
                {
                    TargetType = Entities.opportunity,
                    ExplicitValues = new ExplicitValue[0],
                    FieldMappings = new[]
                    {
                        new FieldMapping()
                        {
                            IsKey = true,
                            Name = Fields.opportunity_.name,
                            CsvName = "Topic"
                        },
                        new FieldMapping()
                        {
                            IsKey = true,
                            Name = Fields.opportunity_.customerid,
                            TargetType = Entities.account,
                            FieldMappings = new []
                            {
                                new FieldMapping() { Name = Fields.account_.name, CsvName = "Company", IsKey=true}
                            },
                            ExplicitValues = new ExplicitValue[]
                            {
                                new ExplicitValue(Fields.account_.customertypecode, new OptionSetValue(OptionSets.Account.RelationshipType.Prospect))
                            },
                        },
                        new FieldMapping()
                        {
                            IsKey = true,
                            Name = Fields.opportunity_.parentcontactid,
                            TargetType = Entities.contact,
                            FieldMappings = new []
                            {
                                new FieldMapping() { Name = Fields.contact_.firstname, CsvName = "First Name" },
                                new FieldMapping() { Name = Fields.contact_.lastname, CsvName = "Last Name" },
                                new FieldMapping() { Name = Fields.contact_.emailaddress1, CsvName = "Email", IsKey = true },
                                new FieldMapping() { Name = Fields.contact_.mobilephone, CsvName = "Mobile Phone" },
                                new FieldMapping() { Name = Fields.contact_.telephone1, CsvName = "Business Phone" },
                                new FieldMapping() { Name = Fields.contact_.jobtitle, CsvName = "Job Title" },
                                new FieldMapping() { Name = Fields.contact_.parentcustomerid, CsvName = "Company", TargetType = Entities.account },
                            },
                            ExplicitValues = new ExplicitValue[0],
                        }
                    }
                };
            }
        }

        public interface IRecurseFieldCreation
        {
            IEnumerable<FieldMapping> FieldMappings { get; }
            IEnumerable<FieldMapping> KeyMappings { get; }
            IEnumerable<ExplicitValue> ExplicitValues { get; }
            string TargetType { get; }
        }

        public class TypeMapping : IRecurseFieldCreation
        {
            public IEnumerable<FieldMapping> FieldMappings { get; set; }
            public IEnumerable<FieldMapping> KeyMappings { get { return FieldMappings.Where(f => f.IsKey); } }
            public IEnumerable<ExplicitValue> ExplicitValues { get; set; }
            public string TargetType { get; set; }
        }

        public class FieldMapping : IRecurseFieldCreation
        {
            public string CsvName { get; set; }
            public string Name { get; set; }
            public string TargetType { get; set; }
            public bool IsKey { get; set; }
            public IEnumerable<FieldMapping> FieldMappings { get; set; }
            public IEnumerable<FieldMapping> KeyMappings { get { return FieldMappings.Where(f => f.IsKey); } }
            public IEnumerable<ExplicitValue> ExplicitValues { get; set; }
        }

        public class ExplicitValue
        {
            public ExplicitValue(string name, object value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; set; }
            public object Value { get; set; }
            public bool SetOnMatch { get; set; }
        }

        private static Dictionary<string, object> ParseJsonToDictionary(string rowJson)
        {
            Dictionary<string, object> theDictionary;
            var serialiser = new DataContractJsonSerializer(typeof(Dictionary<string, object>), new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(rowJson)))
            {
                theDictionary = (Dictionary<string, object>)serialiser.ReadObject(stream);
            }

            return theDictionary;
        }

        private IEnumerable<Entity> GetMatchingEntities(string type, string field, object value)
        {
            return GetMatchingEntities(type, new Dictionary<string, object>()
            {
                { field, value }
            });
        }

        private IEnumerable<Entity> GetMatchingEntities(string type, Dictionary<string, object> matchFields)
        {
            var conditions = new List<ConditionExpression>();
            conditions.AddRange(matchFields.Select(kv => new ConditionExpression(kv.Key, ConditionOperator.Equal, kv.Value)));
            if (type == "workflow")
                conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, XrmPicklists.WorkflowType.Definition));
            if (type == "knowledgearticle")
                conditions.Add(new ConditionExpression("islatestversion", ConditionOperator.Equal, true));
            if (type == "account" || type == "contact")
                conditions.Add(new ConditionExpression("merged", ConditionOperator.NotEqual, true));

            return XrmService.RetrieveAllAndClauses(type, conditions, new string[0]);
        }
    }
}
