
# ElmDocs

The intention with this project is to provide offline docs to be used in Elm Repl.
The project consist of an Elm package to be used in REPL and a F# project to download all latest packages to a cache.

## Usage

1. add the package to your project >elm install hakonrossebo/elmdocs (NB! not released yet)
2. Start Elm repl >elm repl
3. run the help function to get suggestions on usage >help

## Available functions

* help
* getPackageInfo
* getPackageModuleValues
* search
* getAllPackageModules

## Screenshot from REPL

![alt text](ElmDocsScreenshot.png)

## Todo

* Prepare and release as package on Elm Packages
* Automate creation of packages to ElmDocs package daily
* Refactoring and improvements
* Help, contributions and suggestions wanted, this is experimental on this stage. Functions and results can change.
