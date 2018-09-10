open System.Net;
open FSharp.Data;

let allPackages = JsonValue.Load(__SOURCE_DIRECTORY__ + "/data/all-packages.json")

let allPackages' = allPackages.Properties()


let countReleases v = 
    match v with
    | JsonValue.Array arr ->
        Array.length arr
    | _ -> 0

let getLastRelease v = 
    match v with
    | JsonValue.Array arr ->
        let last = 
            arr |> Array.last
        match last with 
        | JsonValue.String str ->
            str 
        | _ -> "Verson not a string"
    | _ -> "No version"




[<EntryPoint>]
let main argv =
    // let wc = new WebClient()
    // wc.DownloadFile("https://package.elm-lang.org/packages/krisajenkins/remotedata/latest/docs.json", @"c:\temp\remotedatadocs.json")
    // Seq.iter (fun (k, v)  -> printfn "%s - %s" k (getLastRelease v)) allPackages'
    allPackages'
    |> Seq.map (fun (k, v)  -> (k, (getLastRelease v)))
    |> Seq.iter (fun (name, release) -> printfn "%s - %s" name release)
    0