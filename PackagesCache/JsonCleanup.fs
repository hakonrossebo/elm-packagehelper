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
        let sourcePackageName = "p_" + (vendorName.ToLower().Replace("-", "_")) + "_" + packageName.Replace("-", "_")
        let decoderSourcePackageName = "decode_" + sourcePackageName
        let packageNameLine = sprintf @"%s = """""" " sourcePackageName
        textWriter.WriteLine packageNameLine
        cleanJson.WriteTo (textWriter, JsonSaveOptions.DisableFormatting)
        textWriter.WriteLine @""""""""
        textWriter.WriteLine ""

        decoderFile.WriteLine (sprintf @"%s = Json.Decode.decodeString (Json.Decode.list decoder) %s" decoderSourcePackageName sourcePackageName)
        decoderFile.WriteLine ""
        sprintf @"(""%s/%s"", %s)" vendorName packageName decoderSourcePackageName

    | None ->
        ""

let cleanupAllFiles rootPath newRootPath =
    getFiles rootPath
    |> Seq.iter (fun file -> cleanAndWriteFile file.Name file.FullName newRootPath)


let cleanupAllFilesToOneSource rootPath (newFile:string) (decoderFile:string) =
    use textWriter = new StreamWriter(newFile)
    textWriter.WriteLine "module Generated.AllElmDocs exposing (..)"
    textWriter.WriteLine ""
    use decoderFile = new StreamWriter(decoderFile)
    decoderFile.WriteLine "module Generated.AllElmDocsDecoders exposing (..)"
    decoderFile.WriteLine ""
    decoderFile.WriteLine "import Json.Decode exposing (..)"
    decoderFile.WriteLine "import Elm.Docs exposing (..)"
    decoderFile.WriteLine "import Generated.AllElmDocs exposing (..)"

    decoderFile.WriteLine ""
    let decoderList = 
        getFiles rootPath
        |> Seq.map (fun file -> cleanAndWriteToStream textWriter decoderFile file.Name file.FullName)
        |> Seq.reduce (fun acc newVal -> acc + ", " + newVal)
        |> sprintf @"decoderList = [%s]"

    decoderFile.WriteLine decoderList
    ()