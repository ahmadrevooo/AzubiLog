namespace AzubiLog.Services.ReportEntries;

public sealed record ReportEntryOption(int Id, string Name, string Email = "", string Department = "");
