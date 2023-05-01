namespace CancellationTokenDemo
{
    /*
     * This Program reads a huge file with multiple records and performs 2 operations for each record
     * 1. Create a corresponding record in a new file (takes 1 second to write the record to the file)
     * 2. Update database for the record (takes 2 seconds to update the record in the database)
     * 
     * The file processing needs to be done within 10 seconds, after which all resources should be released preventing long running operations.
     */
    class Program
    {
        static void Main(string[] args)
        {
            // Define the cancellation token.
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            source.CancelAfter(TimeSpan.FromSeconds(10)); // Stop processing after 10 seconds

            ProcessFile(20, token).Wait(); //Process a file with 20 records
            
            Console.ReadKey(true);
        }

        private static async Task ProcessFile(int records, CancellationToken cancellationToken)
        {
            Console.WriteLine("Processing a huge file");

            for(int i=0; i<records; i++)
            {
                var fileTask = AddRecordToNewFile(i, cancellationToken);
                var dbTask = UpdateDatabase(i, cancellationToken);

                await Task.WhenAll(fileTask, dbTask);

                if (cancellationToken.IsCancellationRequested) // When cancellation is requested, stop processing and release resources
                {
                    Console.WriteLine("Task was cancelled while processing file record : {0}", i);
                    break;
                }
            }
        }

        private static async Task AddRecordToNewFile(int recordNumber, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                Console.WriteLine("Record added to new file for record : {0}", recordNumber);
            }
            catch (TaskCanceledException) // Task is cancelled automatically when token is cancelled from source
            {
                Console.WriteLine("Task was cancelled while adding record to new file for record : {0}", recordNumber);
            }
        }

        private static async Task UpdateDatabase(int recordNumber, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                Console.WriteLine("Record updated in database for record : {0}", recordNumber);
            }
            catch (TaskCanceledException) // Task is cancelled automatically when token is cancelled from source
            {
                Console.WriteLine("Task was cancelled while updating database for record : {0}", recordNumber);
            }
        }
    }
}