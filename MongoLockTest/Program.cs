namespace MongoLockTest
{
    public class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("RunWithoutSemaphore");
            await RunWithoutSemaphore();
            Console.WriteLine();
            Console.WriteLine("RunWithSemaphore");
            await RunWithSemaphore();

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private static async Task RunWithoutSemaphore()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(RunTaskWithoutSemaphore(i));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task RunTaskWithoutSemaphore(int number)
        {
            await Task.Run(async () =>
            {
                await DoSth(number);
            });
        }

        private static async Task RunWithSemaphore()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(RunTaskWithSemaphore(i));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task RunTaskWithSemaphore(int number)
        {
            await Task.Run(async () =>
            {
                var mongoSemaphoreFactory = new MongoSemaphoreFactory();
                await using (var semaphore = mongoSemaphoreFactory.CreateSemaphore(MongoSemaphoresIds.SampleLockId))
                {
                    await semaphore.WaitAsync();
                    await DoSth(number);
                }
            });
        }

        private static async Task DoSth(int i)
        {
            Console.WriteLine($"Start executing for {i}");
            await Task.Delay(500);
            Console.WriteLine($"Finish executing for {i}");
        }
    }
}