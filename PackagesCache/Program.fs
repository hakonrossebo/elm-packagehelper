open System.Net;
open FSharp.Data;
open FileHandling
open System.IO;
open System.Threading;


// type SearchJsonPackages = JsonProvider<"data/search.json">
type SearchJsonPackages = JsonProvider<"https://package.elm-lang.org/search.json">

let availablePackages = SearchJsonPackages.GetSamples()


let getPackageInfo (name:string) (versions:string array) =
    let nameInfo = name.Split("/")
    (nameInfo.[0], nameInfo.[1], Array.last versions)


let packagesAndLastVersion (packages: SearchJsonPackages.Root []) =
                            packages
                            |> Seq.map (fun x -> (getPackageInfo x.Name x.Versions.Strings))
                            // |> Seq.take 9


let downloadFile (wc: WebClient) rootPath (packageVendor, packageName, packageVersion) =
    let downloadPath = sprintf "https://package.elm-lang.org/packages/%s/%s/%s/docs.json" packageVendor packageName packageVersion
    let localFileName = sprintf @"%s___%s___%s___docs.json" packageVendor packageName packageVersion
    let localFileFullPath = Path.Combine(rootPath, localFileName)
    printfn "Download from %s - save to %s" downloadPath localFileFullPath
    |> ignore
    wc.DownloadFile(downloadPath,localFileFullPath)
    Thread.Sleep(1000) // Don't put too much pressure on the server



[<EntryPoint>]
let main argv =
    let rootPath = @"c:\temp\elm-packageinfo"
    let wc = new WebClient()
    let downloadFile' = downloadFile wc rootPath
    let availablePackagesWithInfo =
        availablePackages
        |> packagesAndLastVersion
        |> set

    let downloadedpackagesWithInfo =
        getDownloadedPackagesWithVersion @"c:\temp\elm-packageinfo"
        |> set

    let packagesToDownload =
        Set.difference availablePackagesWithInfo downloadedpackagesWithInfo
        |> Set.toSeq

    packagesToDownload
    |> Seq.iter downloadFile'
    0