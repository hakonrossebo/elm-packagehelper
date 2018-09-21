module Example exposing (suite)

import ElmDocs exposing (..)
import Expect exposing (Expectation)
import Fuzz exposing (Fuzzer, int, list, string)
import Test exposing (..)


suite : Test
suite =
    describe "Integration tests"
        [ describe "search tests"
            [ test "search for toString" <|
                \_ ->
                    let
                        ( msg, results ) =
                            -- Debug.log "Search toString" <| ElmDocs.search "toString"
                            Debug.log "Search toString" <| ElmDocs.search "toString"
                    in
                    Expect.greaterThan 5 (List.length results)
            ]
        ]
