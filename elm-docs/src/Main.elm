module Main exposing (..)

import Elm.Docs exposing (..)
import Json.Decode exposing (..)
import SmallDocs exposing (..)


listModules =
    case decodeElmCoreModules of
        Ok modules ->
            List.map (\x -> x.name) modules

        Err err ->
            [ Json.Decode.errorToString err ]


decodeElmCoreModules =
    Json.Decode.decodeString (Json.Decode.list decoder) coreDocs
