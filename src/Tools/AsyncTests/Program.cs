using System.Collections.Concurrent;
using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace AsyncTests;

[Event("misc")] public record MiscEvent : EventRecord;
[Event("text")] public record TextEvent(
        [property:EventPart("text")] int Text
    ) : EventRecord, IAsyncEvent;

[Event("yesno")]
public record PromptYesNoEvent(
    [property: EventPart("text")] int Text
) : EventRecord, IBranchingEvent;

[Event("numeric")] public record PromptNumericEvent(
        [property:EventPart("val")] ushort value
    ) : EventRecord, IBranchingEvent;

class InputHandler : Component
{
    static readonly string[] Strings =
    {
        "", // 0 unused
        "Intro text", // 1
        "Choose yes or no", // 2
        "Yes was chosen", // 3
        "No was chosen", // 4
        "Enter the code", // 5
        "The code was correct", // 6
        "The code was not correct" // 7
    };

    readonly BlockingCollection<Action> _queue = new();

    public InputHandler()
    {
        OnAsync<TextEvent>((e, c) =>
        {
            Console.Write($" \"{Strings[e.Text]}\" ");
            _queue.Add(() =>
            {
                Console.ReadKey();
                c();
            });

            return true;
        });

        OnAsync<PromptYesNoEvent, bool>((e, c) =>
        {
            Console.Write($" \"{Strings[e.Text]}\" Y/N? ");

            _queue.Add(() =>
            {
                for (;;)
                {
                    var key = Console.ReadKey();
                    var keyChar = char.ToLowerInvariant(key.KeyChar);
                    if (keyChar == 'y')
                    {
                        c(true);
                        break;
                    }

                    if (keyChar == 'n')
                    {
                        c(false);
                        break;
                    }
                }
            });

            return true;
        });

        OnAsync<PromptNumericEvent, bool>((e, c) =>
        {
            _queue.Add(() =>
            {
                for (;;)
                {
                    Console.Write(" Enter number: ");
                    var line = Console.ReadLine();
                    if (int.TryParse(line, out var num))
                    {
                        c(num == e.value);
                        break;
                    }
                }
            });

            return true;
        });
    }

    public void HandleInputs(Task task)
    {
        foreach(var item in _queue.GetConsumingEnumerable())
            item();
    }

    public void Complete() => _queue.CompleteAdding();
}

public static class Program
{
    const string TestScript = @"
{
    Chain0:
    misc
    text 1
    misc
    if (yesno 2) {
        text 3
    } else {
        text 4
    }
    misc
    text 5
    if (numeric 1042) {
        text 6
    } else {
        text 7
    }
}
";

    static readonly DeterministicTaskScheduler Scheduler = new();
    static readonly TaskFactory Factory = new(Scheduler);

    public static void Main()
    {
        Event.AddEventsFromAssembly(typeof(MiscEvent).Assembly);

        EventExchange exchange = new();
        var input = new InputHandler();
        exchange.Attach(input);
        var testLayout = AlbionCompiler.Compile(TestScript);
        var testSet = new EventSet(EventSetId.None, testLayout.Events, testLayout.Chains);
        Console.WriteLine("Starting");
        bool done = false;
        var task = Run(exchange, testSet, 0).ContinueWith(_ =>
        {
            done = true;
        });

        Console.WriteLine("Started");

        var thread = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Game Loop Thread";
            while (!done)
            {
                Scheduler.RunPendingTasks();
                Thread.Sleep(100);
            }

            input.Complete();
        });
        thread.Start();

        input.HandleInputs(task);
        thread.Join();

        Console.WriteLine("Done");
        Console.ReadLine();
    }

    static Task Run(EventExchange exchange, IEventSet set, int chainId)
    {
        var entryPoint = set.Events[set.Chains[chainId]];
        return Factory.StartNew(() => RunAsync(exchange, entryPoint)).Unwrap();
    }

    static async Task RunAsync(EventExchange exchange, IEventNode? node)
    {
        while (node != null)
        {
            Console.Write(node.ToString());

            if (node is IBranchNode branch && node.Event is IAsyncEvent<bool> boolEvent)
            {
                var result = await RaiseAsyncBool(exchange, boolEvent);
                node = result ? branch.Next : branch.NextIfFalse;
                Console.WriteLine($" => {result}");
            }
            else if (node.Event is IAsyncEvent asyncEvent)
            {
                await RaiseAsync(exchange, asyncEvent);
                node = node.Next;
                Console.WriteLine();
                Console.WriteLine(" => Done");
            }
            else
            {
                exchange.Raise(node.Event, null);
                node = node.Next;
                Console.WriteLine();
            }
        }
    }

    static Task RaiseAsync(EventExchange exchange, IAsyncEvent e)
    {
        var source = new TaskCompletionSource();
        int waiters = exchange.RaiseAsync(e, null, source.SetResult);
        return waiters == 0 ? Task.CompletedTask : source.Task;
    }

    static Task<bool> RaiseAsyncBool(EventExchange exchange, IAsyncEvent<bool> e)
    {
        var source = new TaskCompletionSource<bool>();
        int waiters = exchange.RaiseAsync(e, null, source.SetResult);
        return waiters == 0 ? Task.FromResult(false) : source.Task;
    }
}
