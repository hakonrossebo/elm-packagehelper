open System.Net;
open FSharp.Data;
open System.IO;
open System.IO


let getDirectories path =
    let dirInfo = DirectoryInfo(path)
    dirInfo.EnumerateDirectories()
        // |> Seq.map (fun x -> x.Name)

// Create a list of packageCreator/Name/Version based on a root folder
let createVendorPath vendorName rootPath  : string =
    Path.Combine(rootPath, vendorName)

let createVendorPackagePath  packageName vendorPath: string =
    Path.Combine(vendorPath, packageName)

let createVendorPackageVersionPath  packageVersion vendorPackagePath: string =
    Path.Combine(vendorPackagePath, packageVersion)

let createDocsFilePath vendorPackageVersionPath : string =
    Path.Combine(vendorPackageVersionPath, "docs.json")

let getVendorPackageVersionWhenDocsExists rootPath vendorName packageName packageVersion : seq<string * string * string> =
    let file = rootPath 
            |> createVendorPath vendorName
            |> createVendorPackagePath packageName
            |> createVendorPackageVersionPath packageVersion
            |> createDocsFilePath 
    let f = FileInfo(file)
    match f.Exists with
    | true ->
        [(vendorName, packageName, packageVersion)] |> List.toSeq
    | false ->
        [] |> List.toSeq

let getVendorPackageVersions rootPath vendorName vendorPackagePath (vendorPackageDirectoryInfo:DirectoryInfo) : seq<string * string * string> =
    let vendorPackagePath = createVendorPackagePath vendorPackageDirectoryInfo.Name vendorPackagePath
    let vendorPackageVersions = getDirectories vendorPackagePath
    let packages = 
        vendorPackageVersions
        |> Seq.collect (fun vendorPackageVersion -> getVendorPackageVersionWhenDocsExists rootPath vendorName vendorPackageDirectoryInfo.Name vendorPackageVersion.Name)
    packages

let getVendorPackages rootPath (vendor:DirectoryInfo) : seq<string * string * string> =
    let vendorPath = createVendorPath vendor.Name rootPath
    let vendorPackages = getDirectories vendorPath
    let packages = 
        vendorPackages
        |> Seq.collect (fun vendorPackage -> getVendorPackageVersions rootPath vendor.Name vendorPath vendorPackage)
    packages


let getDownloadedPackagesWithVersion rootPath : seq<string * string * string> =
    let packageVendors = getDirectories rootPath
    let packages = 
        packageVendors
        |> Seq.collect (fun vendor -> getVendorPackages rootPath vendor)
    packages



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