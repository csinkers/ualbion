<Query Kind="FSharpProgram">
  <NuGetReference>Microsoft.Diagnostics.Tracing.TraceEvent</NuGetReference>
  <Namespace>Microsoft.Diagnostics.Symbols</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Analysis</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Analysis.GC</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Analysis.JIT</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Etlx</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.EventPipe</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.ApplicationServer</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.AspNet</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.Clr</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.ClrPrivate</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.IIS_Trace</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.JScript</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.JSDumpHeap</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.Kernel</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.Symbol</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Parsers.Tpl</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Session</Namespace>
  <Namespace>Microsoft.Diagnostics.Tracing.Stacks</Namespace>
</Query>

let D x = x.Dump()

let main() =
    use session = new TraceEventSession("MySession")
    session.EnableProvider("*UAlbion-CoreTrace") |> ignore
    session.EnableProvider("*UAlbion-GameTrace") |> ignore
    use source = new ETWTraceEventSource("MySession", TraceEventSourceType.Session)
    source.Dynamic.add_All <| Action<TraceEvent> (fun e ->
        if (int e.ID = 65534) then () else // Skip manifest declaration events
        [
            [("event", (sprintf "%s %s %s [%s (%d)]" (e.TimeStamp.ToString("o")) e.ProviderName e.EventName e.ProcessName e.ProcessID) :> obj)]
            e.PayloadNames |> Array.map (fun x -> (x, e.PayloadByName(x))) |> List.ofArray
        ] |> List.concat |> D
    )
    source.Process() |> ignore

main()