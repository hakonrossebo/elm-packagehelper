open FileHandling
open ElmFileGenerator;

[<EntryPoint>]
let main argv =
    downloadPackages ()
    deleteOldPackageVersions ()
    generateElmFilesFromCachedPackages FileHandling.rootPath
    0