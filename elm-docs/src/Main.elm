module Main exposing (..)

import Generated.AllElmDocsDecoders exposing (..)
import Json.Decode exposing (..)


searchModules searchArg =
    decoderList
        |> List.filter (\( packageName, _ ) -> String.contains searchArg packageName)
        |> List.map (\( packageName, _ ) -> packageName)



-- listModules =
--     case decode_p_elm_core of
--         Ok modules ->
--             List.map (\x -> x.name) modules
--         Err err ->
--             [ Json.Decode.errorToString err ]
