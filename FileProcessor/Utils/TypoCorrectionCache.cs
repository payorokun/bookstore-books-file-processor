using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace FileProcessor.Utils;

public class TypoCorrectionCache
{
    private readonly CloudTable _table;
    private readonly ConcurrentDictionary<string, string> _localCache = new();

    public TypoCorrectionCache(string storageConnectionString, string tableName)
    {
        var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        var tableClient = storageAccount.CreateCloudTableClient();
        _table = tableClient.GetTableReference(tableName);
    }
    
    public async Task<string> GetCorrectedKeyAsync(string key)
    {
        if (_localCache.TryGetValue(key, out var correctedKey))
        {
            return correctedKey;
        }

        var retrieveOperation = TableOperation.Retrieve<TypoCorrectionEntity>("TypoCorrections", key);
        var result = await _table.ExecuteAsync(retrieveOperation);
        if (result.Result is not TypoCorrectionEntity entity) return key;
        _localCache[key] = entity.Correction;
        return entity.Correction;
    }

    public async Task AddCorrectionAsync(string key, string correctedKey)
    {
        var entity = new TypoCorrectionEntity(key, correctedKey);
        var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);
        await _table.ExecuteAsync(insertOrReplaceOperation);
        _localCache[key] = correctedKey;
    }
}