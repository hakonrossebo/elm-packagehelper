open System.Net;
open FSharp.Data;


type SearchJsonPackages = JsonProvider<"data/search.json">

let packages = SearchJsonPackages.GetSamples()

let packagesAndLastVersion = packages
                            |> Seq.map (fun x -> (x.Name, Array.last x.Versions.Strings))
                            |> Seq.take 5


[<EntryPoint>]
let main argv =
    // let wc = new WebClient()
    // wc.DownloadFile("https://package.elm-lang.org/packages/krisajenkins/remotedata/latest/docs.json", @"c:\temp\remotedatadocs.json")
    packagesAndLastVersion
    |> Seq.iter (fun (name, release) -> printfn "%s - %s" name release)
    0