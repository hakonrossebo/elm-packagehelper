# Updating the generated packages and publishing a new version

Clone the project
Navigate to ./PackagesCache
Ensure dotnet core is installed with dotnet --version - needs 2.1.403

Ensure that the cache directory is populated with content of zip. See Cache readme and bash scripts.

Run: dotnet run
When all updated packages are downloaded, Elm files are generated
Verify that the ElmDocs files have been generated
Run the project/tests with Elm test and repl to verify everything is working
If the update was large, consider generating new zip of cache and add to git

>elm bump (updates elm.json with new version - use this version in tag and commit message)
In project root: git add generated files and the updated elm.json
>git commit -m"Update to new version x.y.z"
>git tag -a x.y.z -m"Annotation comment"
>git push --tags origin master
>elm publish


in case of tagging error/updates after tagging - use -f

Resources for publishing packages:
https://becoming-functional.com/publishing-your-first-elm-package-13d984a1200a
https://korban.net/posts/elm/2018-10-02-basic-steps-publish-package-elm-19/




