#r "paket: groupref FakeBuild //"
#load ".fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "Facades/netstandard"
#r "netstandard"
#endif

open System
open System.IO
open Fake

open System
open System.IO
open System.Text.RegularExpressions
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.Tools.Git
open Fake.JavaScript


// Filesets
let projects  =
      !! "src/**.fsproj"

let versionFromGlobalJson : DotNet.CliInstallOptions -> DotNet.CliInstallOptions = (fun o ->
        { o with Version = DotNet.Version (DotNet.getSDKVersionFromGlobalJson()) }
    )

let dotnetSdk = lazy DotNet.install versionFromGlobalJson
let inline dtntWorkDir wd =
    DotNet.Options.lift dotnetSdk.Value
    >> DotNet.Options.withWorkingDirectory wd

Target.create "Install" (fun _ ->
    projects
    |> Seq.iter (fun s ->
        let dir = IO.Path.GetDirectoryName s
        DotNet.restore (dtntWorkDir dir) ""
    )
)

Target.create "Build" (fun _ ->
    projects
    |> Seq.iter (fun s ->
        let dir = IO.Path.GetDirectoryName s
        DotNet.build (dtntWorkDir dir) ""
    )
)

let release = ReleaseNotes.load "RELEASE_NOTES.md"

Target.create "Meta" (fun _ ->
    [ "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">"
      "<PropertyGroup>"
      "<Description>Hot Module Replacement for Elmish apps</Description>"
      "<PackageProjectUrl>https://github.com/elmish/hmr</PackageProjectUrl>"
      "<PackageLicenseUrl>https://raw.githubusercontent.com/elmish/hmr/master/LICENSE.md</PackageLicenseUrl>"
      "<PackageIconUrl>https://raw.githubusercontent.com/elmish/hmr/master/docs/files/img/logo.png</PackageIconUrl>"
      "<RepositoryUrl>https://github.com/elmish/hmr.git</RepositoryUrl>"
      "<PackageTags>fable;elmish;fsharp;hmr</PackageTags>"
      "<Authors>Maxime Mangel</Authors>"
      sprintf "<Version>%s</Version>" (string release.SemVer)
      "</PropertyGroup>"
      "</Project>"]
    |> File.write false "Directory.Build.props"
)

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target.create "Package" (fun _ ->
    DotNet.pack (dtntWorkDir "src") ""
)

Target.create "PublishNuget" (fun _ ->
    let args = sprintf "push Fable.Elmish.HMR.%s.nupkg -s nuget.org -k %s" (string release.SemVer) (Environment.environVar "nugetkey")

    let result = DotNet.exec (dtntWorkDir "src/bin/Release") "nuget" args
    if not result.OK then failwithf "Build of tests project failed."

)


// --------------------------------------------------------------------------------------
// Generate the documentation

let website = "https://elmish.github.io/hmr"
let gitName = "hmr"
let gitOwner = "elmish"

module Doc =
    // Paths with template/source/output locations
    let bin        = __SOURCE_DIRECTORY__ @@ "bin"
    let content    = __SOURCE_DIRECTORY__ @@ "docsrc/content"
    let output     = __SOURCE_DIRECTORY__ @@ "docs"
    let files      = __SOURCE_DIRECTORY__ @@ "docsrc/files"
    let templates  = __SOURCE_DIRECTORY__ @@ "docsrc/tools/templates"
    let formatting = __SOURCE_DIRECTORY__ @@ "packages/formatting/FSharp.Formatting"
    let docTemplate = "docpage.cshtml"

let github_release_user = Environment.environVarOrDefault "github_release_user" gitOwner
let githubLink = sprintf "https://github.com/%s/%s" github_release_user gitName
let gitHome = sprintf "https://github.com/%s" gitOwner

// Specify more information about your project
let info =
  [ "project-name", "elmish-hmr"
    "project-author", "Maxime Mangel"
    "project-summary", "Hot Module Replacement integration for Elmish apps"
    "project-github", githubLink
    "project-nuget", "http://nuget.org/packages/Fable.Elmish.HMR" ]

let root = website

let referenceBinaries = []

let layoutRootsAll = new System.Collections.Generic.Dictionary<string, string list>()
layoutRootsAll.Add("en", [ Doc.templates;
                           Doc.formatting @@ "templates"
                           Doc.formatting @@ "templates/reference" ])

let copyFiles () =
    Shell.copyRecursive Doc.files Doc.output true
    |> Trace.logItems "Copying file: "
    Directory.ensure (Doc.output @@ "content")
    Shell.copyRecursive (Doc.formatting @@ "styles") (Doc.output @@ "content") true
    |> Trace.logItems "Copying styles and scripts: "

Target.create "CleanDocs" (fun _ ->
    Shell.cleanDirs [ Doc.output ]
)

Target.create "GenerateDocs" (fun _ ->

    DirectoryInfo.getSubDirectories (DirectoryInfo.ofPath Doc.templates)
    |> Seq.iter (fun d ->
                    let name = d.Name
                    if name.Length = 2 || name.Length = 3 then
                        layoutRootsAll.Add(
                                name, [Doc.templates @@ name
                                       Doc.formatting @@ "templates"
                                       Doc.formatting @@ "templates/reference" ]))
    copyFiles ()

    for dir in  [ Doc.content; ] do
        let langSpecificPath(lang, path:string) =
            path.Split([|'/'; '\\'|], System.StringSplitOptions.RemoveEmptyEntries)
            |> Array.exists(fun i -> i = lang)
        let layoutRoots =
            let key = layoutRootsAll.Keys |> Seq.tryFind (fun i -> langSpecificPath(i, dir))
            match key with
            | Some lang -> layoutRootsAll.[lang]
            | None -> layoutRootsAll.["en"] // "en" is the default language

        FSFormatting.createDocs (fun args ->
            { args with
                Source = Doc.content
                OutputDirectory = Doc.output
                LayoutRoots = layoutRoots
                ProjectParameters  = ("root", root)::info
                Template = Doc.docTemplate } )
)


// --------------------------------------------------------------------------------------
// Release Scripts

Target.create "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    Shell.cleanDir tempDocsDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

    Shell.copyRecursive Doc.output tempDocsDir true |> Trace.tracefn "%A"
    Staging.stageAll tempDocsDir
    Commit.exec tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Branches.push tempDocsDir
)

// #load "./paket-files/fakebuild/fsharp/FAKE/modules/Octokit/Octokit.fsx"
// open Octokit

Target.create "Release" (fun _ ->
    // Octokit give some warnings
    // let user =
    //     match Environment.environVarOrDefault "github-user" String.Empty with
    //     | s when not (String.IsNullOrWhiteSpace s) -> s
    //     | _ -> UserInput.getUserInput "Username: "
    // let pw =
    //     match Environment.environVarOrDefault "github-pw" String.Empty with
    //     | s when not (String.IsNullOrWhiteSpace s) -> s
    //     | _ -> UserInput.getUserPassword "Password: "
    // let remote =
    //     CommandHelper.getGitResult "" "remote -v"
    //     |> Seq.filter (fun (s: string) -> s.EndsWith("(push)"))
    //     |> Seq.tryFind (fun (s: string) -> s.Contains(gitOwner + "/" + gitName))
    //     |> function None -> gitHome + "/" + gitName | Some (s: string) -> s.Split().[0]

    // Staging.stageAll ""
    // Commit.exec "" (sprintf "Bump version to %s" release.NugetVersion)
    // Branches.pushBranch "" remote (Information.getBranchName "")

    // Branches.tag "" release.NugetVersion
    // Branches.pushTag "" remote release.NugetVersion

    // // release on github
    // createClient user pw
    // |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes
    // |> releaseDraft
    // |> Async.RunSynchronously

    Staging.stageAll ""
    Commit.exec "" (sprintf "Bump version to %s" release.NugetVersion)
    Branches.push ""

    Branches.tag "" release.NugetVersion
    Branches.pushTag "" "origin" release.NugetVersion
)

Target.create "Publish" ignore

// Build order
"Meta"
  ==> "Install"
  ==> "Build"
  ==> "Package"
  ==> "PublishNuget"

"Build"
    ==> "CleanDocs"
    ==> "GenerateDocs"
    ==> "ReleaseDocs"

"Publish"
  <== [ "Build"
        "Package"
        "PublishNuget"
        "ReleaseDocs" ]

// start build
Target.runOrDefault "Build"
