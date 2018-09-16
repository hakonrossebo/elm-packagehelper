# PackagesCache

Use this utility to download a cache of all latest Elm packages. Created in F#, but runs on .Net Core and runs on all platforms.

The intention is to generate offline docs for use in the ElmDocs package. Elm repl currently prevents any results from tasks that could be used to download packages on demand.

## Usage

1. To avoid downloading too many packages, extract the .zip file located in the cache folder to cache.
2. Run from the terminal using >dotnet run

## Todo

* Automate creation of packages to ElmDocs package daily
* Refactoring and improvements