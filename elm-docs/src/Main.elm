module Main exposing (..)

import Elm.Docs
import Generated.AllElmDocsDecoders exposing (..)
import Json.Decode exposing (..)


-- searchModules searchArg =
--     decoderList
--         |> List.filter (\( packageName, _ ) -> String.contains searchArg packageName)
--         |> List.map (\( packageName, _ ) -> packageName)


type
    FindPackageResult
    -- = Found (List Elm.Docs.Module)
    = Found String String (List String)
    | FoundEmptyList
    | FoundWithDecoderError String
    | MultipleFound String (List String)
    | NotFound String


help =
    [ "Commands/Functions to run:", "getAllPackageModules", "getPackageModuleValues packageName moduleName", "getPackageInfo packageName" ]


decodeAllModules =
    decoderList
        |> List.map (\( packageName, packageDecoder ) -> ( packageName, packageDecoder ))
        |> List.length


getAllPackageModules =
    decoderList
        |> List.map runDecoder


getPackageModuleValues findPackageName findModule =
    let
        matchingPackages =
            decoderList
                |> List.filter (\( packageName, _ ) -> String.contains findPackageName packageName)

        package =
            case List.length matchingPackages of
                0 ->
                    NotFound "Could not find any matching packages"

                1 ->
                    matchingPackages
                        |> List.head
                        |> decodeIfFound (runDecoderAndFindModule findModule)

                _ ->
                    matchingPackages
                        |> List.map (\( packageName, _ ) -> packageName)
                        |> MultipleFound "Found multiple matching packages"
    in
    package


getPackageInfo findPackageName =
    let
        matchingPackages =
            decoderList
                |> List.filter (\( packageName, _ ) -> String.contains findPackageName packageName)

        package =
            case List.length matchingPackages of
                0 ->
                    NotFound "Could not find any matching packages"

                1 ->
                    matchingPackages
                        |> List.head
                        |> decodeIfFound runDecoder

                _ ->
                    matchingPackages
                        |> List.map (\( packageName, _ ) -> packageName)
                        |> MultipleFound "Found multiple matching packages"
    in
    package


decodeIfFound runner maybeDecoder =
    case maybeDecoder of
        Just packageInfo ->
            runner packageInfo

        Nothing ->
            FoundEmptyList


runDecoder ( packageName, decoder ) =
    case decoder of
        Ok modules ->
            modules
                |> List.map (\packageModule -> packageModule.name)
                |> Found packageName "Modules:"

        Err err ->
            FoundWithDecoderError (Json.Decode.errorToString err)


runDecoderAndFindModule findModule ( packageName, decoder ) =
    case decoder of
        Ok modules ->
            modules
                |> List.filter (\packageModule -> packageModule.name == findModule)
                |> List.concatMap (\packageModule -> packageModule.values)
                |> List.map (\packageValue -> packageValue.name)
                |> Found (packageName ++ findModule) "Modules:"

        Err err ->
            FoundWithDecoderError (Json.Decode.errorToString err)
