module FileHandling

open System.IO;


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
