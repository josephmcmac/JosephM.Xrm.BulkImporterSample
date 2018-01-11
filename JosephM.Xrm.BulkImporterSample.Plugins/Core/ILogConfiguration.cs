namespace JosephM.Xrm.BulkImporterSample.Plugins.Core
{
    public interface ILogConfiguration
    {
        bool Log { get; }
        string LogFilePath { get; }
        bool LogDetail { get; }
        string LogFilePrefix { get; }
    }
}