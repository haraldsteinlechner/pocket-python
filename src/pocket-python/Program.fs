open System
open System.Net
open System.IO
open System.IO.Compression

open System
open System.Diagnostics

let runProc filename args startDir env = 
    let timer = Stopwatch.StartNew()
    let procStartInfo = 
        ProcessStartInfo(
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            FileName = filename,
            Arguments = args
        )
    match startDir with | Some d -> procStartInfo.WorkingDirectory <- d | _ -> ()
    let paths = procStartInfo.EnvironmentVariables.["Path"]
    let f = String.concat ";" (List.map Path.GetFullPath env)
    procStartInfo.EnvironmentVariables.["Path"] <- f + ";" + paths
    procStartInfo.UseShellExecute <- false

    let outputs = System.Collections.Generic.List<string>()
    let errors = System.Collections.Generic.List<string>()
    let outputHandler f (_sender:obj) (args:DataReceivedEventArgs) = Console.WriteLine(args.Data); f args.Data
    let p = new Process(StartInfo = procStartInfo)
    p.OutputDataReceived.AddHandler(DataReceivedEventHandler (outputHandler outputs.Add))
    p.ErrorDataReceived.AddHandler(DataReceivedEventHandler (outputHandler errors.Add))
    let started = 
        try
            p.Start()
        with | ex ->
            ex.Data.Add("filename", filename)
            reraise()
    if not started then
        failwithf "Failed to start process %s" filename
    printfn "Started %s with pid %i" p.ProcessName p.Id
    p.BeginOutputReadLine()
    p.BeginErrorReadLine()
    p.WaitForExit()
    timer.Stop()
    printfn "Finished %s after %A milliseconds" filename timer.ElapsedMilliseconds
    let cleanOut l = l |> Seq.filter (fun o -> String.IsNullOrEmpty o |> not)
    if p.ExitCode = 0 then Choice1Of2 (cleanOut outputs,cleanOut errors)
    else Choice2Of2 (cleanOut outputs,cleanOut errors)

let url = "https://www.python.org/ftp/python/3.8.1/python-3.8.1-embed-win32.zip"

[<EntryPoint;STAThread>]
let main argv = 

    let wd = System.Environment.CurrentDirectory
    let target = Path.Combine(wd,"python")
    use wc = new WebClient()
    let scripts = [Path.Combine(target,"Scripts")]
    if not (Directory.Exists target) then

        let download = Path.Combine(wd, "python.zip")
        let d = wc.DownloadFile(url, "python.zip")
        Directory.CreateDirectory target |> ignore
        ZipFile.ExtractToDirectory(download, target)

        let pthPath = Path.Combine(target,"python38._pth")
        let pth = File.ReadAllText(pthPath)
        File.WriteAllText(pthPath, pth.Replace("#import site","import site"))

        wc.DownloadFile("https://bootstrap.pypa.io/get-pip.py", Path.Combine(target,"get-pip.py"))
        runProc (Path.Combine(target,"python.exe")) "get-pip.py" (Some target) [] |> printfn "%A"


    runProc (Path.Combine(target,"Scripts","pip.exe")) "install exif opencv-python numpy lensfunpy" (Some target) [] |> printfn "%A"

    0