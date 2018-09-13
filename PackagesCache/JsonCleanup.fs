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


let cleanupAllFiles rootPath newRootPath =
    getFiles rootPath
    |> Seq.iter (fun file -> cleanAndWriteFile file.Name file.FullName newRootPath)
