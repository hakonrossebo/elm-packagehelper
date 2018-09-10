open System.Net;
open FSharp.Data;

type AllPackages = JsonProvider<"data/all-packages.json">
let packages = AllPackages.GetSample()

let x = packages.``1602ElmFeather``.Strings



[<EntryPoint>]
let main argv =
    // let wc = new WebClient()
    // wc.DownloadFile("https://package.elm-lang.org/packages/krisajenkins/remotedata/latest/docs.json", @"c:\temp\remotedatadocs.json")
    x.Length |> printfn "%d"
    0