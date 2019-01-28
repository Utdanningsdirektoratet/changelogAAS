#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators
open Fake.DotNet.NuGet

let baseDir = __SOURCE_DIRECTORY__
let buildDir = baseDir @@ @"\build"
let artifactsDir = baseDir @@ @"\artifacts"
let udirNugetFeed = Environment.environVar "udir_nuget_feed"

let version =
    match BuildServer.buildServer with
    | TeamCity  -> BuildServer.buildVersion
    | LocalBuild -> "1.0.0-local"
    | _ -> Environment.environVarOrDefault "version" "1.0.0"

Target.create "Clean" (fun _ ->
    Shell.cleanDirs [buildDir; artifactsDir]
)

Target.create "Build" (fun _ ->
    "src/ChangelogProvider.Generator/ChangelogProvider.Generator.fsproj"
    |> DotNet.build (fun options -> 
        { options with
            OutputPath = Some buildDir 
        })
)

Target.create "Pack" (fun _ -> 
    "src/ChangelogProvider.Generator/ChangelogProvider.Generator.fsproj"
    |> DotNet.pack (fun opt -> 
        { opt with 
            OutputPath = Some artifactsDir
            MSBuildParams = {
            opt.MSBuildParams with
                Properties =
                    ["PackageVersion", version]
            }
        }
    )   
)

Target.create "Publish" (fun _ -> 
    NuGet.NuGetPublish (fun p ->
      { p with
          PublishUrl = udirNugetFeed
          WorkingDir = artifactsDir
          OutputPath = artifactsDir
          Project = "UDIR.PAS2.ChangelogAAS.Provider"
          Version = version })
  
)

"Clean"
  ==> "Build"

"Clean"
    ==> "Pack"
    ==> "Publish"

Target.runOrDefault "Build"
