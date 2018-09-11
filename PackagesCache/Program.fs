open System.Net;
open FSharp.Data;
open System.IO;
open System.IO


let getDirectories path =
    let dirInfo = DirectoryInfo(path)
    dirInfo.EnumerateDirectories()
        // |> Seq.map (fun x -> x.Name)

// Create a list of packageCreator/Name/Version based on a root folder

let getVendorPackageVersion rootPath vendorName packageName packageVersion : seq<string * string * string> =
    let vendorPath = Path.Combine(rootPath, vendorName)
    let packagePath = Path.Combine(vendorPath, packageName)
    let packageVersionPath = Path.Combine(packagePath, packageVersion)
    let file = Path.Combine(packageVersionPath, "docs.json")
    let f = FileInfo(file)
    match f.Exists with
    | true ->
        [("","","")] |> List.toSeq
    | false ->
        [("","","")] |> List.toSeq

let getVendorPackages rootPath vendorName : seq<string * string * string> =

    [("","","")] |> List.toSeq

let getDownloadedPackagesWithVersion path : seq<string * string * string> =
    let packageVendors = getDirectories path
    packageVendors
    |> Seq.map (fun vendor -> vendor)

    [("","","")] |> List.toSeq


// let getDirectories2 path =
//     Directory.EnumerateDirectories(path)

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
    0