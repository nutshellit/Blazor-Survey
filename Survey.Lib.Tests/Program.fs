
open System
open Expecto
open System.IO

module Utils = 
    let RunTests argv = 
        //reset the db 
        let connstr = DbConfig.dbConnLocation()
        let r = DbConfig.dbSetup connstr 

        let df = { defaultConfig with ``parallel`` = false 
                                      verbosity = Logging.Verbose }
        Tests.runTestsInAssembly df argv |> ignore

[<EntryPoint>]
let main argv =
    let arg1 = argv |> Seq.tryHead
    arg1 |> function
    | None -> //default to running expecto tests
        Utils.RunTests argv
    | Some arg -> 
        arg = "seed" |> function 
        | true -> 
            let fn = DbConfig.dbConnLocation()
            let r = DbConfig.dbSetup fn 
            SeedDbs.seedDb()
            Console.WriteLine(fn)
        | false ->  Console.WriteLine "seed"
                    ()
    0
