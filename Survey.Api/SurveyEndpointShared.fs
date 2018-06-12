module SurveyEndpointShared

open Microsoft.Extensions.Configuration
open System.IO
open Survey.Lib

let rec walkupDirs dir fn =
    let f = sprintf @"%s\%s" dir fn
    printfn "Trying %s" f
    File.Exists(f) |> function 
    | true -> f
    | false -> walkupDirs (Directory.GetParent(dir).FullName) fn

let dbConnLocation() = 
    let config = ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build()
    let fn = config.["dblocation"]
    let d = Directory.GetCurrentDirectory()
    let f = walkupDirs d fn
    f

let dbSetup(connStr: string ) = 
    SQLiteHelper.testingOnlyDropAndRecreateDatabases connStr |> Async.RunSynchronously   


