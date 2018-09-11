open System.Net;
open FSharp.Data;
open FileHandling


type SearchJsonPackages = JsonProvider<"data/search.json">

let availablePackages = SearchJsonPackages.GetSamples()


let getPackageInfo (name:string) (versions:string array) =
    let nameInfo = name.Split("/")
    (nameInfo.[0], nameInfo.[1], Array.last versions)


let packagesAndLastVersion (packages: SearchJsonPackages.Root []) = 
                            packages
                            |> Seq.map (fun x -> (getPackageInfo x.Name x.Versions.Strings))
                            |> Seq.take 30

// Plan: Get a list of package docs to download.

let downloadFile (wc: WebClient) localBasePath (packageName, packageVersion) =
    let downloadPath = sprintf "https://package.elm-lang.org/packages/%s/%s/docs.json" packageName packageVersion
    let localPath = sprintf @"%s\%s\%s\docs.json" localBasePath packageName packageVersion
    printfn "Download from %s - save to %s" downloadPath localPath
    |> ignore
    


[<EntryPoint>]
let main argv =
    let wc = new WebClient()
    let downloadFilePrep = downloadFile wc @"c:\temp\elm-packageinfo"
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
    |> Seq.iter (fun (x,y,z) -> printfn "%s / %s / %s" x y z )
    0