using Microsoft.WindowsAzure.Storage.Table;

namespace FileProcessor.Utils;

public class TypoCorrectionEntity : TableEntity
{
    public TypoCorrectionEntity(string typo, string correction)
    {
        PartitionKey = "TypoCorrections";
        RowKey = typo;
        Correction = correction;
    }

    public TypoCorrectionEntity() { }

    public string Correction { get; set; }
}