open System
open System.IO
open System.Net.Http

let repo = "EHotwagner/Constitutions"
let branch = "main"

let buildRawUrl (constitutionName: string) =
    $"https://raw.githubusercontent.com/{repo}/{branch}/{constitutionName}/constitution.md"

let findConstitutions (startDir: string) =
    let rec search (dir: string) (acc: string list) =
        let files =
            try Directory.GetFiles(dir, "constitution.md") |> Array.toList
            with _ -> []
        let subdirs =
            try Directory.GetDirectories(dir) |> Array.toList
            with _ -> []
        let acc = acc @ files
        subdirs |> List.fold (fun a d -> search d a) acc
    search startDir [] |> Array.ofList

let download (url: string) =
    use client = new HttpClient()
    client.Timeout <- TimeSpan.FromSeconds(30.0)
    try
        let response = client.GetAsync(url) |> Async.AwaitTask |> Async.RunSynchronously
        if response.IsSuccessStatusCode then
            let content = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            Some content
        else
            eprintfn $"Failed to download: HTTP {int response.StatusCode}"
            None
    with ex ->
        eprintfn $"Download error: {ex.Message}"
        None

[<EntryPoint>]
let main argv =
    if argv.Length < 1 then
        eprintfn "Usage: upcons <constitution-name>"
        eprintfn "Example: upcons fsGeneral"
        eprintfn ""
        eprintfn "Downloads the named constitution from GitHub and replaces"
        eprintfn "any constitution.md found in the current directory tree."
        1
    else
        let name = argv.[0].TrimStart('-')
        let url = buildRawUrl name
        let searchDir = Directory.GetCurrentDirectory()

        printfn $"Downloading constitution '{name}' from GitHub..."
        match download url with
        | None ->
            eprintfn $"Could not download constitution '{name}'."
            eprintfn $"URL: {url}"
            2
        | Some content ->
            let targets = findConstitutions searchDir
            if targets.Length = 0 then
                eprintfn "No constitution.md found in current directory tree."
                eprintfn "Create a constitution.md file first, then run upcons to replace it."
                3
            else
                for path in targets do
                    File.WriteAllText(path, content)
                    printfn $"Updated: {path}"
                printfn $"Replaced {targets.Length} constitution file(s) with '{name}'."
                0
