module ElmDocs exposing (help, getPackageInfo, getPackageModuleValues, search, getAllPackageModules)

{-| A helper package to be used in elm REPL


# Usage

@docs help, getPackageInfo, getPackageModuleValues, search, getAllPackageModules

-}

import Elm.Docs
import Generated.AllElmDocsDecoders exposing (..)
import Json.Decode exposing (..)


type FindPackageResult
    = Found String String (List String)
    | FoundEmptyList
    | FoundWithDecoderError String
    | MultipleFound String (List String)
    | NotFound String


type Location
    = Location String ( String, String )


type ItemInfo
    = ModuleValue Location
    | ModuleAlias Location
    | ModuleUnion Location
    | ModuleBinop Location


{-| Get available functions to run when using REPL

> import ElmDocs exposing (..)
> help

-}
help =
    [ "Commands/Functions to run:", "search searchFor", "getAllPackageModules", "getPackageModuleValues packageName moduleName", "getPackageInfo packageName" ]


{-| Search all package module values/unions/aliases

> import ElmDocs exposing (..)
> search "sqrt"

-}
search : String -> List Location
search searchFor =
    let
        searchResults =
            indexAllModules
                |> List.filter (filterLocation searchFor)

        searchResultLength =
            List.length searchResults
    in
    if searchResultLength == 0 then
        Debug.log "No results" searchResults

    else if searchResultLength <= 20 then
        Debug.log "Results:" searchResults

    else
        Debug.log "More than 20 results. Please narrow search." (searchResults |> List.take 20)


{-| Get a list of all package modules - Generates a long list in REPL

> import ElmDocs exposing (..)
> getAllPackageModules

-}
getAllPackageModules =
    decoderList
        |> List.map runDecoder


{-| Get a list of all values for a specific module in a package
run getPackageInfo "packageVendor/packageName" first to get a list of available modules

> import ElmDocs exposing (..)
> getPackageModuleValues "elm/core" "List"

-}
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


{-| Get a list of all modues in a package

> import ElmDocs exposing (..)
> getPackageInfo "elm/core"

-}
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


filterLocation : String -> Location -> Bool
filterLocation searchForValue location =
    case location of
        Location loc _ ->
            String.contains searchForValue loc


getSearchableFieldsInPackageModule : String -> Elm.Docs.Module -> List Location
getSearchableFieldsInPackageModule packageName packageModule =
    let
        -- TODO: Need to parse tipe to some searchable value
        moduleName =
            packageModule.name

        aliasFields =
            packageModule.aliases
                |> List.concatMap (\moduleAlias -> [ Location moduleAlias.name ( packageName, moduleName ) ])

        valueFields =
            packageModule.values
                |> List.concatMap (\moduleValue -> [ Location moduleValue.name ( packageName, moduleName ) ])
    in
    aliasFields ++ valueFields


getSearchableFieldsInPackage : ( String, Result Error (List Elm.Docs.Module) ) -> List Location
getSearchableFieldsInPackage ( packageName, packageDecoder ) =
    packageDecoder
        |> Result.map (List.concatMap (getSearchableFieldsInPackageModule packageName))
        |> Result.withDefault []


indexAllModules =
    decoderList
        |> List.concatMap getSearchableFieldsInPackage


decodeAllModules =
    decoderList
        |> List.map (\( packageName, packageDecoder ) -> ( packageName, packageDecoder ))
        |> List.length
