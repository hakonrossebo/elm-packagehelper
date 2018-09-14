module JsonCleanup
open FSharp.Data;
open System.IO;
open FileHandling;

let rec cleanupComments json =
    match json with
    | JsonValue.String s -> json
    | JsonValue.Number d -> json
    | JsonValue.Float f -> json
    | JsonValue.Boolean _  | JsonValue.Null -> json
    | JsonValue.Record props ->
      props
      |> Array.map (fun (key, value) -> key,
                                          if key.Equals("comment") then JsonValue.String ""
                                          else cleanupComments value)
                                          |> JsonValue.Record
    | JsonValue.Array array ->
      array
      |> Array.map cleanupComments
      |> JsonValue.Array

let cleanAndWriteFile name (fullPath:string) newRootPath =
    let jsonFile = JsonValue.Load(fullPath)
    let cleanJson = cleanupComments jsonFile
    let newFilePath = Path.Combine(newRootPath, name)
    use textWriter = new StreamWriter(newFilePath)
    cleanJson.WriteTo (textWriter, JsonSaveOptions.DisableFormatting)

let cleanAndWriteToStream (textWriter:StreamWriter) (decoderFile:StreamWriter) name (fullPath:string) =
    match parseFileNameToPackageMetadata name with
    | Some (vendorName, packageName, _) ->
        let jsonFile = JsonValue.Load(fullPath)
        let cleanJson = cleanupComments jsonFile
        let sourcePackageName = (vendorName.ToLower()) + "_" + packageName.Replace("-", "_")
        let decoderSourcePackageName = "decode_" + (vendorName.ToLower()) + "_" + packageName.Replace("-", "_")
        let packageNameLine = sprintf @"%s = """""" " sourcePackageName
        textWriter.WriteLine packageNameLine
        cleanJson.WriteTo (textWriter, JsonSaveOptions.DisableFormatting)
        textWriter.WriteLine @""""""""
        textWriter.WriteLine ""

        decoderFile.WriteLine (sprintf @"%s = Json.Decode.decodeString (Json.Decode.list decoder) %s" decoderSourcePackageName sourcePackageName)
        decoderFile.WriteLine ""

    | None ->
        ()

let cleanupAllFiles rootPath newRootPath =
    getFiles rootPath
    |> Seq.iter (fun file -> cleanAndWriteFile file.Name file.FullName newRootPath)

let cleanupAllFilesToOneSource rootPath (newFile:string) (decoderFile:string) =
    use textWriter = new StreamWriter(newFile)
    textWriter.WriteLine "module AllElmDocs exposing (..)"
    textWriter.WriteLine ""
    use decoderFile = new StreamWriter(decoderFile)
    decoderFile.WriteLine "module AllElmDocsDecoders exposing (..)"
    decoderFile.WriteLine ""
    decoderFile.WriteLine "import AllElmDocs exposing (..)"
    decoderFile.WriteLine ""
    getFiles rootPath
    |> Seq.iter (fun file -> cleanAndWriteToStream textWriter decoderFile file.Name file.FullName)
