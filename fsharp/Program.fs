open System
open System.Net.Http
open Hopac
open System.Threading
open System.Threading.Tasks
open System.Linq

let http = new HttpClient ()

let googleUri = Uri "https://www.cnn.com"

let file = "/var/log/syslog"

let readFileHopac n =
    Array.create n ((fun () -> System.IO.File.ReadAllTextAsync (file)) |> Job.fromTask)
    |> Job.conCollect

let callGoogleHopac n = 
    Array.create n (
        job {
            let! httpResponse = (fun () -> http.GetAsync(googleUri)) |> Job.fromTask
            let! content = (fun () -> httpResponse.Content.ReadAsStringAsync ()) |> Job.fromTask
            printfn "Page length: %i" content.Length
        }
    ) |> Job.conCollect

let callGoogleTask () =
    http.GetAsync(googleUri).ContinueWith (fun (t:Task<HttpResponseMessage>) -> t.Result.Content.ReadAsStringAsync().ContinueWith(fun (t:Task<string>) -> printfn "Read %i characters." t.Result.Length))

let callGoogleTpl n = 
    let tasks = ResizeArray ()
    for i in 1..n do
        tasks.Add(callGoogleTask().Result)
    Task.WaitAll (tasks.ToArray())

let readTask () =
    System.IO.File.ReadAllTextAsync(file).ContinueWith(fun (result:Task<string>) -> printfn "Contains %i characters." result.Result.Length)

let readFileTpl n =
    let tasks = ResizeArray ()
    for i in 1..n do
        tasks.Add (readTask())
    printfn "Created %i tasks" tasks.Count
    Task.WaitAll (tasks.ToArray())

let callGoogleAsync n = 
    Array.create n (
        async {
            let! httpResponse = http.GetAsync(googleUri) |> Async.AwaitTask
            let! content = httpResponse.Content.ReadAsStringAsync () |> Async.AwaitTask
            printfn "Content length: %i" content.Length
        }
    )|> Async.Parallel

let readFileAsync n =
    Array.create n (System.IO.File.ReadAllTextAsync (file) |> Async.AwaitTask)
    |> Async.Parallel

[<EntryPoint>]
let main argv =
    match argv with
    | [|"hopac"|] ->
        printfn "Running hopac calls"
        run (callGoogleHopac 100) |> Seq.iter ignore
        printfn "Done with hopac calls"
    | [|"async"|] ->
        printfn "Running async calls"
        callGoogleAsync 100 |> Async.RunSynchronously |> ignore
        printfn "Done with async calls"
    | [|"tpl"|] ->
        printfn "Running tpl calls"
        let result = (readFileTpl 25)//.GetAwaiter().GetResult()
        printfn "Done with tpl calls"
    | _ -> Console.Error.WriteLine ("Run with 'async', 'tpl', or 'hopac' parameter.")  
    0 // return an integer exit code
