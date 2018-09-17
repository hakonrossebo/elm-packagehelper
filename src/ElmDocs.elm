module ElmDocs exposing (help, getPackageInfo, getPackageModuleValues, search, searchType, getAllPackageModules)

{-| A helper package to be used in elm REPL. By importing this package you can search all available
Elm packages directly from the REPL.


# Usage

@docs help, getPackageInfo, getPackageModuleValues, search, searchType, getAllPackageModules

-}

import Elm.Docs
import Elm.Type
import Generated.AllElmDocsDecoders exposing (..)
import Json.Decode exposing (..)


type FindPackageResult
    = Found String String (List String)
    | FoundEmptyList
    | FoundWithDecoderError String
    | MultipleFound String (List String)
    | NotFound String


type SearchType
    = TextSearch String
    | TypeSearch Elm.Type.Type


type Location
    = Location String ( String, String )
    | TypeLocation Elm.Type.Type String ( String, String )


type ItemInfo
    = ModuleValue Location
    | ModuleAlias Location
    | ModuleUnion Location
    | ModuleBinop Location


{-| Get available functions to run when using REPL

    import ElmDocs exposing (..)
    help

-}
help : List String
help =
    [ "Commands/Functions to run:", "search searchFor", "searchType type", "getAllPackageModules", "getPackageModuleValues packageName moduleName", "getPackageInfo packageName" ]


{-| Search all package module values/unions/aliases

    import ElmDocs exposing (..)
    search "sqrt"

-}
search : String -> ( String, List Location )
search searchFor =
    let
        parsedSearchFor =
            parseSearchFor searchFor

        searchResults =
            indexAllModulesFields
                |> List.filter (filterLocation parsedSearchFor)

        searchResultLength =
            List.length searchResults
    in
    if searchResultLength == 0 then
        ( "Not found", [] )

    else if searchResultLength <= 50 then
        ( "Found", searchResults )

    else
        ( "More than 50 results. Please narrow search.", searchResults |> List.take 50 )


{-| Search all package module values/unions/aliases for a type signature

    import ElmDocs exposing (..)
    searchType "a - a"
    searchType "Basics.Int -> Basics.Float"

-}
searchType : String -> ( String, List Location )
searchType searchFor =
    String.concat [ "\"", searchFor, "\"" ]
        |> search


{-| Get a list of all package modules - Generates a long list in REPL

    import ElmDocs exposing (..)
    getAllPackageModules

-}
getAllPackageModules : List FindPackageResult
getAllPackageModules =
    decoderList
        |> List.map runDecoder


{-| Get a list of all values for a specific module in a package
run getPackageInfo "packageVendor/packageName" first to get a list of available modules

    import ElmDocs exposing (..)
    getPackageModuleValues "elm/core" "List"

-}
getPackageModuleValues : String -> String -> FindPackageResult
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

    import ElmDocs exposing (..)
    getPackageInfo "elm/core"

-}
getPackageInfo : String -> FindPackageResult
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


filterLocation : SearchType -> Location -> Bool
filterLocation searchForValue location =
    case location of
        Location loc _ ->
            case searchForValue of
                TextSearch textSearchForValue ->
                    String.contains textSearchForValue loc

                _ ->
                    False

        TypeLocation loc _ _ ->
            case searchForValue of
                TypeSearch typeSearchForValue ->
                    loc == typeSearchForValue

                _ ->
                    False


getSearchableFieldsForUnionType : String -> String -> String -> ( String, List Elm.Type.Type ) -> List Location
getSearchableFieldsForUnionType packageName moduleName unionName ( unionTypeName, unionTypeTypes ) =
    unionTypeTypes
        |> List.concatMap (\unionTypeType -> [ TypeLocation unionTypeType (unionName ++ " - " ++ unionTypeName) ( packageName, moduleName ) ])


getSearchableFieldsForUnion : String -> String -> Elm.Docs.Union -> List Location
getSearchableFieldsForUnion packageName moduleName union =
    let
        name =
            [ Location union.name ( packageName, moduleName ) ]

        args =
            union.args
                |> List.concatMap (\arg -> [ Location (union.name ++ " - " ++ arg) ( packageName, moduleName ) ])

        unionTypes =
            union.tags
                |> List.concatMap (getSearchableFieldsForUnionType packageName moduleName union.name)
    in
    name ++ args ++ unionTypes


getSearchableFieldsInPackageModule : String -> Elm.Docs.Module -> List Location
getSearchableFieldsInPackageModule packageName packageModule =
    let
        moduleName =
            packageModule.name

        aliasFields =
            packageModule.aliases
                |> List.concatMap (\moduleAlias -> [ Location moduleAlias.name ( packageName, moduleName ), TypeLocation moduleAlias.tipe moduleAlias.name ( packageName, moduleName ) ])

        valueFields =
            packageModule.values
                |> List.concatMap (\moduleValue -> [ Location moduleValue.name ( packageName, moduleName ), TypeLocation moduleValue.tipe moduleValue.name ( packageName, moduleName ) ])

        binopFields =
            packageModule.values
                |> List.concatMap (\moduleBinop -> [ Location moduleBinop.name ( packageName, moduleName ), TypeLocation moduleBinop.tipe moduleBinop.name ( packageName, moduleName ) ])

        unionFields =
            packageModule.unions
                |> List.concatMap (getSearchableFieldsForUnion packageName moduleName)
    in
    aliasFields ++ valueFields ++ binopFields ++ unionFields


getSearchableFieldsInPackage : ( String, Result Error (List Elm.Docs.Module) ) -> List Location
getSearchableFieldsInPackage ( packageName, packageDecoder ) =
    packageDecoder
        |> Result.map (List.concatMap (getSearchableFieldsInPackageModule packageName))
        |> Result.withDefault []


indexAllModulesFields =
    decoderList
        |> List.concatMap getSearchableFieldsInPackage


decodeAllModules =
    decoderList
        |> List.map (\( packageName, packageDecoder ) -> ( packageName, packageDecoder ))
        |> List.length


parseSearchFor : String -> SearchType
parseSearchFor searchFor =
    case Json.Decode.decodeString Elm.Type.decoder searchFor of
        Ok searchForType ->
            TypeSearch searchForType

        _ ->
            TextSearch searchFor
