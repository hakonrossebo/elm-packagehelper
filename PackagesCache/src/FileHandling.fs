module FileHandling

open System.IO;
open FSharp.Data;
open System.Net;
open System.Threading;

type SearchJsonPackages = JsonProvider<"https://package.elm-lang.org/search.json">
let availablePackages = SearchJsonPackages.GetSamples()
let rootPath = __SOURCE_DIRECTORY__ + @"/../cache"

let getFiles path =
    let files = DirectoryInfo(path)
    files.EnumerateFiles()
    |> Seq.filter (fun file -> file.Extension = ".json")

let deletePackage rootPath (packageVendor, packageName, packageVersion) =
    let localFileName = sprintf @"%s___%s___%s___docs.json" packageVendor packageName packageVersion
    let localFileFullPath = Path.Combine(rootPath, localFileName)
    File.Delete localFileFullPath
    printfn "Deleted file: %s" localFileFullPath


let parseFileNameToPackageMetadata (fileInfo:string) : (string * string * string) option=
    try
        let fileNameParts = fileInfo.Split "___"
        let parts = Some (fileNameParts.[0], fileNameParts.[1], fileNameParts.[2])
        parts
    with
    | Failure _ -> None

let getDownloadedPackagesWithVersion rootPath : seq<string * string * string> =
    getFiles rootPath
    |> Seq.map (fun fileInfo -> fileInfo.Name)
    |> Seq.choose parseFileNameToPackageMetadata

let sortAndRemoveLastVersion (versions:(string * string * string) seq) =
    versions
    |> Seq.sortByDescending (fun (_, _, version) -> version)
    |> Seq.tail


let getOldPackageVersions (files:FileInfo seq) =
    files
    |> Seq.map (fun fileInfo -> fileInfo.Name)
    |> Seq.choose parseFileNameToPackageMetadata
    |> Seq.groupBy (fun (vendor, package, _) -> (vendor, package))
    |> Seq.filter (fun (_, versions) -> Seq.length versions > 1)
    |> Seq.map (fun (pKey, pVer) -> (pKey, sortAndRemoveLastVersion pVer))
    |> Seq.collect (fun (_, pVer) -> pVer)



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


let downloadPackages () =
    let wc = new WebClient()
    let downloadFile' = downloadFile wc rootPath

    // Get last version of all available packages
    let availablePackagesWithInfo =
        availablePackages
        |> packagesAndLastVersion
        |> set

    // Get existing packages in cache
    let downloadedpackagesWithInfo =
        getDownloadedPackagesWithVersion rootPath
        |> set

    // Limit to packages that has not been downloaded yet
    let packagesToDownload =
        Set.difference availablePackagesWithInfo downloadedpackagesWithInfo
        |> Set.toSeq

    // Download packages to cache
    packagesToDownload
    |> Seq.iter downloadFile'

    printfn "All packages downloaded successfully."


let deleteOldPackageVersions () =
    getFiles rootPath
    |> getOldPackageVersions
    |> Seq.iter (deletePackage rootPath)
    printfn "All old versions of packages deleted successfully."