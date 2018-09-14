module FileHandling

open System.IO;
let getFiles path =
    let files = DirectoryInfo(path)
    files.EnumerateFiles()
    |> Seq.filter (fun file -> file.Extension = ".json")

let deletePackage rootPath (packageVendor, packageName, packageVersion) =
    let localFileName = sprintf @"%s___%s___%s___docs.json" packageVendor packageName packageVersion
    let localFileFullPath = Path.Combine(rootPath, localFileName)
    File.Delete localFileFullPath
    printfn "Deleted file: %s" localFileFullPath


let parseFileInfoToPackageMetadata (fileInfo:FileInfo) : (string * string * string) option=
    try
        let fileNameParts = fileInfo.Name.Split "___"
        let parts = Some (fileNameParts.[0], fileNameParts.[1], fileNameParts.[2])
        parts
    with
    | Failure _ -> None

let parseFileNameToPackageMetadata (fileInfo:string) : (string * string * string) option=
    try
        let fileNameParts = fileInfo.Split "___"
        let parts = Some (fileNameParts.[0], fileNameParts.[1], fileNameParts.[2])
        parts
    with
    | Failure _ -> None

let getDownloadedPackagesWithVersion rootPath : seq<string * string * string> =
    getFiles rootPath
    |> Seq.choose parseFileInfoToPackageMetadata

let sortAndRemoveLastVersion (versions:(string * string * string) seq) = 
    versions
    |> Seq.sortByDescending (fun (_, _, version) -> version) 
    |> Seq.tail


let getOldPackageVersions (files:FileInfo seq) =
    files
    |> Seq.choose parseFileInfoToPackageMetadata
    |> Seq.groupBy (fun (vendor, package, _) -> (vendor, package))
    |> Seq.filter (fun (_, versions) -> Seq.length versions > 1)
    |> Seq.map (fun (pKey, pVer) -> (pKey, sortAndRemoveLastVersion pVer))
    |> Seq.collect (fun (_, pVer) -> pVer)

// Funksjon som 1: sorterer versjoner, fjerner siste element, flater ut til en seq