
namespace AOTConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Hello, World!");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var cancel = cancellationTokenSource.Token;
            Task.Run(async () =>
            {
                var i = 0;
                while (true)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;
                    Console.WriteLine($"当前执行-{i++}");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }, cancel);
            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                {
                    cancellationTokenSource.Cancel();
                    Console.WriteLine($"执行完毕------");
                }
                else
                {
                    Console.WriteLine($"输入------{key.KeyChar.ToString()}");
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}