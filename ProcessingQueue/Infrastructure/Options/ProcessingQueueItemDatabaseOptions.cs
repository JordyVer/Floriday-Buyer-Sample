namespace ProcessingQueue.Infrastructure.Options
{
    public class ProcessingQueueItemDatabaseOptions
    {
        public ProcessingQueueItemDatabaseOptions()
        {
            Schema = "processing";
            TableName = "ProcessingQueueItem";
            MaxItemsToPreprocess = 10;
            MaxItemsToProcess = 10;
            RetryAttempts = 5;
        }

        public string Schema { get; set; }
        public string TableName { get; set; }
        public int MaxItemsToPreprocess { get; set; }
        public int MaxItemsToProcess { get; set; }
        public int RetryAttempts { get; set; }
    }
}