using System.Collections.Concurrent;
using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace AsyncTests;

[Event("misc")] public record MiscEvent : EventRecord;
[Event("text")] public record TextEvent(
        [property:EventPart("text")] int Text
    ) : EventRecord;

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
        OnAsync<TextEvent>(e =>
        {
            Console.Write($" \"{Strings[e.Text]}\" ");
            var source = new AlbionTaskSource();
            _queue.Add(() =>
            {
                Console.ReadKey();
                source.Complete();
            });

            return source.Task;
        });

        OnQueryAsync<PromptYesNoEvent, bool>(e =>
        {
            Console.Write($" \"{Strings[e.Text]}\" Y/N? ");

            var source = new AlbionTaskSource<bool>();
            _queue.Add(() =>
            {
                for (;;)
                {
                    var key = Console.ReadKey();
                    var keyChar = char.ToLowerInvariant(key.KeyChar);
                    if (keyChar == 'y')
                    {
                        source.Complete(true);
                        break;
                    }

                    if (keyChar == 'n')
                    {
                        source.Complete(false);
                        break;
                    }
                }
            });

            return source.Task;
        });

        OnQueryAsync<PromptNumericEvent, bool>(e =>
        {
            var source = new AlbionTaskSource<bool>();
            _queue.Add(() =>
            {
                for (;;)
                {
                    Console.Write(" Enter number: ");
                    var line = Console.ReadLine();
                    if (int.TryParse(line, out var num))
                    {
                        source.Complete(num == e.value);
                        break;
                    }
                }
            });

            return source.Task;
        });
    }

    public void HandleInputs()
    {
        foreach (var item in _queue.GetConsumingEnumerable())
            item();
    }

    public void Complete() => _queue.CompleteAdding();
}

public static class Program
{
    const string testScript = @"
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

    public static void Main()
    {
        Event.AddEventsFromAssembly(typeof(MiscEvent).Assembly);

        EventExchange exchange = new();
        var input = new InputHandler();
        exchange.Attach(input);
        var testLayout = AlbionCompiler.Compile(testScript);
        var testSet = new EventSet(EventSetId.None, testLayout.Events, testLayout.Chains);
        Console.WriteLine("Starting");
        // bool done = false;

        var entryPoint = testSet.Events[testSet.Chains[0]];
        _ = RunAsync(exchange, entryPoint);
        // task.OnCompleted(() => { done = true; });

        Console.WriteLine("Started");
/*
        var thread = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Game Loop Thread";
            while (!done)
            {
                AlbionTaskScheduler.Default.RunPendingTasks();
                Thread.Sleep(100);
            }

            input.Complete();
        });
        thread.Start();
*/

        input.HandleInputs();
        // thread.Join();

        Console.WriteLine("Done");
        Console.ReadLine();
    }

    static async AlbionTask RunAsync(EventExchange exchange, IEventNode? node)
    {
        while (node != null)
        {
            Console.Write(node.ToString());

            if (node is IBranchNode branch && node.Event is IQueryEvent<bool> boolEvent)
            {
                var result = await exchange.RaiseQueryA(boolEvent, null);
                node = result ? branch.Next : branch.NextIfFalse;
                Console.WriteLine($" => {result}");
            }
            else
            {
                await exchange.RaiseA(node.Event, null);
                node = node.Next;
                Console.WriteLine(" => Done");
            }
        }

        Console.WriteLine("Finished script");
    }
}

