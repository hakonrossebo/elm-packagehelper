open System.Net;
open FSharp.Data;
open FileHandling


type SearchJsonPackages = JsonProvider<"data/search.json">

let packages = SearchJsonPackages.GetSamples()

let packagesAndLastVersion = packages
                            |> Seq.map (fun x -> (x.Name, Array.last x.Versions.Strings))
                            |> Seq.take 5

// Plan: Get a list of package docs to download.

let downloadFile (wc: WebClient) localBasePath (packageName, packageVersion) =
    let downloadPath = sprintf "https://package.elm-lang.org/packages/%s/%s/docs.json" packageName packageVersion
    let localPath = sprintf @"%s\%s\%s\docs.json" localBasePath packageName packageVersion
    // wc.DownloadFile(downloadPath, @"c:\temp\remotedatadocs.json")
    printfn "Download from %s - save to %s" downloadPath localPath
    |> ignore


[<EntryPoint>]
let main argv =
    let wc = new WebClient()
    let downloadFilePrep = downloadFile wc @"c:\temp\elm-packageinfo"
    // packagesAndLastVersion
    // |> Seq.iter downloadFilePrep
    // getDirectoryNames @"c:\"
    // |> Seq.iter (fun x -> printfn "%s" x)
    getDownloadedPackagesWithVersion @"c:\temp\elm-packageinfo"
    |> Seq.iter (fun (x,y,z) -> printfn "%s / %s / %s" x y z )
    0