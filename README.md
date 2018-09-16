
# ElmDocs

The intention with this project is to provide offline docs to be used in Elm Repl.
The project consist of an Elm package to be used in REPL and a F# project to download all latest packages to a cache.

## Usage

1. add the package to your project >elm install hakonrossebo/elmdocs
2. Start Elm repl >elm repl
3. run the help function to get suggestions on usage >help

## Available functions

* help
* getPackageInfo
* getPackageModuleValues
* search
* getAllPackageModules

## Screenshot from REPL

![Image of REPL usage](https://raw.githubusercontent.com/hakonrossebo/elmdocs/master/ElmDocsScreenshot.png)
9i

## Todo

* Automate creation of packages to ElmDocs package daily
* Refactoring and improvements
* Help, contributions and suggestions wanted, this is experimental on this stage. Functions and results can change.
