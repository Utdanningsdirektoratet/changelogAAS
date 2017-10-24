module Changelog.Suave

open System
open System.IO
open Suave
open Suave.Successful
open Suave.Writers
open Suave.Operators
open Suave.Filters
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Changelog
open HumanReadable



    let environmentParameters = {
        projectName = "UDIR.PAS2"
        fromEnvironmentName = "PAS2-QA"
        toEnvironmentName = "PAS2-PROD"
        octoUrl = "https://oslaz-pas2-od.udir.no"
        octoApiKey = System.Environment.GetEnvironmentVariable("OCTO_API_KEY")
        teamcityUrl = "https://oslaz-pas2-int.udir.no"
        tcUsername = System.Environment.GetEnvironmentVariable("TEAMCITY_USERNAME")
        tcPassword = System.Environment.GetEnvironmentVariable("TEAMCITY_PASSWORD")
        jiraUrl = "https://jira.udir.no"
        jiraUsername = System.Environment.GetEnvironmentVariable("JIRA_USERNAME")
        jiraPassword = System.Environment.GetEnvironmentVariable("JIRA_PASSWORD")
        projectMappings = [
            {
                githubUrl = "https://github.com/Utdanningsdirektoratet/PAS2-hoved"
                jiraKey = "PASX"
                octoDeployName = "UDIR.PAS2"
                teamcityName = "Pas2_ReleasePas2HovedPsake"
            }
        ]
    }

    let versionParameters = {
        projectName = "UDIR.PAS2"
        fromVersion = "1.0.4460"
        toVersion = "1.0.4470"
        teamcityUrl = "https://oslaz-pas2-int.udir.no"
        tcUsername = System.Environment.GetEnvironmentVariable("TEAMCITY_USERNAME")
        tcPassword = System.Environment.GetEnvironmentVariable("TEAMCITY_PASSWORD")
        jiraUrl = "https://jira.udir.no"
        jiraUsername = System.Environment.GetEnvironmentVariable("JIRA_USERNAME")
        jiraPassword = System.Environment.GetEnvironmentVariable("JIRA_PASSWORD")
        projectMappings = [
            {
               githubUrl = "https://github.com/Utdanningsdirektoratet/PAS2-hoved"
               jiraKey = "PASX"
               octoDeployName = "UDIR.PAS2"
               teamcityName = "Pas2_ReleasePas2HovedPsake"
            }
        ]
    }

    let getEnvironmentChanges() : WebPart =
        fun (ctx : HttpContext) ->
            async {
                let changes = Changelog.getChangesBetweenEnvironments environmentParameters
                let jsonSerializerSettings = JsonSerializerSettings()
                jsonSerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()
                let json = JsonConvert.SerializeObject(changes, jsonSerializerSettings) 
                return! OK json ctx
            }

    let getEnvironmentChangesAsMarkdown() : WebPart =
        fun (ctx : HttpContext) ->
            async {
                let changes = Changelog.getChangesBetweenEnvironments environmentParameters
                let html = HumanReadable.changelogToHtml changes
                return! OK html ctx
            }

    let getVersionChanges() : WebPart =
        fun (ctx : HttpContext) ->
            async {
                let changes = Changelog.getChangesBetweenVersions versionParameters
                let jsonSerializerSettings = JsonSerializerSettings()
                jsonSerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()
                let json = JsonConvert.SerializeObject(changes, jsonSerializerSettings) 
                return! OK json ctx
            }

    let getVersionChangesAsMarkdown() : WebPart =
        fun (ctx : HttpContext) ->
            async {
                let changes = Changelog.getChangesBetweenVersions versionParameters
                let html = HumanReadable.changelogToHtml changes
                return! OK html ctx
            }

[<EntryPoint>]
    let main argv =
    let app = 
        choose [ 
            GET >=> path "/api" >=> getEnvironmentChanges() >=> setMimeType "application/json; charset=utf-8"
            GET >=> path "/api/version" >=> getVersionChanges() >=> setMimeType "application/json; charset=utf-8"
            GET >=> path "/" >=> getEnvironmentChangesAsMarkdown()
            GET >=> path "/version" >=> getVersionChangesAsMarkdown()
            GET >=> Files.browseHome
            RequestErrors.NOT_FOUND "Page not found." 
        ]
 
    let config =
        { defaultConfig with homeFolder = Some (Path.GetFullPath "./wwwroot") }
    
    startWebServer config app
    0 // return an integer exit code
