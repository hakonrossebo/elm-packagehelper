module FileHandling

open System.IO;
let getFiles path =
    let files = DirectoryInfo(path)
    files.EnumerateFiles()
    |> Seq.filter (fun file -> file.Extension = ".json")


let parseFileInfoToPackageMetadata (fileInfo:FileInfo) : (string * string * string) option=
    try
        let fileNameParts = fileInfo.Name.Split "___"
        let parts = Some (fileNameParts.[0], fileNameParts.[1], fileNameParts.[2])
        parts
    with
    | Failure _ -> None

let getDownloadedPackagesWithVersion rootPath : seq<string * string * string> =
    getFiles rootPath
    |> Seq.choose parseFileInfoToPackageMetadata