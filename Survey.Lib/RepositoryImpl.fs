namespace Survey.Lib
open System.IO
open SQLite
open Newtonsoft.Json
open System
open Chessie.ErrorHandling
open Survey.Shared

module SQLiteHelper = 
    [<CLIMutable>]
    type SurveyWrapper = 
        {   [<PrimaryKey; AutoIncrement>]
            ID : int
            [<Indexed>]
            EntityId : string
            Name : string
            Status : int
            Serialized : string }
        static member create (e : SurveyDefinition ) = 
            {   SurveyWrapper.ID = 0
                EntityId = e.Id |> string
                Name = e.Name
                Status = e.Status |> function | Edit -> 1 | Live -> 2 | Archived -> 3 
                Serialized = e |> JsonConvert.SerializeObject  }
        member x.Deserialize() = 
            x.Serialized  |> JsonConvert.DeserializeObject<SurveyDefinition>

    [<CLIMutable>]
    type SurveyResponseWrapper = 
        {
            [<PrimaryKey; AutoIncrement>]
            ID : int
            [<Indexed>]
            EntityId : string
            SurveyDefinitionId : string
            Status : int
            Serialized : string 
        }
        static member create (e : SurveyResponse) = 
            { SurveyResponseWrapper.ID = 0
              EntityId = e.Id |> string 
              SurveyDefinitionId = e.SurveyDefinitionId |> string
              Status = e.Status |> function | Started -> 1 | Completed -> 2 | Cancelled -> 3 | Submitted -> 4
              Serialized = e |> JsonConvert.SerializeObject 
              }
        member x.Deserialize() =
            x.Serialized |> JsonConvert.DeserializeObject<SurveyResponse>

    let private createTables (db : SQLiteAsyncConnection) = 
        async {
                let! r1 = db.CreateTableAsync<SurveyWrapper>() |> Async.AwaitTask
                let! _ = db.CreateTableAsync<SurveyResponseWrapper>() |> Async.AwaitTask
                return r1}

    let getConnection(dbFN : string) = 
            File.Exists(dbFN) |> function 
        | true -> async {   let c =  new SQLiteAsyncConnection(dbFN, true)
                            let _ = createTables c
                            return c }
        | false -> failwith (sprintf "create a db file @ %s-  can use sqllite3" dbFN )

    let testingOnlyDropAndRecreateDatabases (dbFN : string) = 
        async {
            let! db = getConnection dbFN
            let! r = createTables db
            let! r2 = db.DropTableAsync<SurveyWrapper>()|> Async.AwaitTask
            let! _ = db.DropTableAsync<SurveyResponseWrapper>()|> Async.AwaitTask
            let! r = createTables db
            do! db.CloseAsync() |> Async.AwaitTask 
            return r
                }

module SurveyDefinitionRepositoryImpl = 
        
    open SQLiteHelper

    let private getW (dbFN : string) =
        fun (id : Guid) -> 
            let sql = sprintf "select * from SurveyWrapper where entityid='%s'" (id |> string)
            async {
                let! conn = SQLiteHelper.getConnection(dbFN)
                let! r = conn.QueryAsync<SurveyWrapper>(sql) |> Async.AwaitTask
                let r1 = r |> Seq.length
                            |> function
                            | 1 -> r.[0] |> Some
                            | _ -> None
                return r1}

    let private get(dbFN : string) =
        fun (id : Guid) -> 
            async {
                let! r1 = getW dbFN id
                return (r1 |> Option.map (fun x -> x.Deserialize()))
            }
    let private getAll (dbFN : string) = 
        fun () -> 
            let sql = sprintf "select * from SurveyWrapper"
            async {
                let! conn = SQLiteHelper.getConnection(dbFN)
                let! r = conn.QueryAsync<SurveyWrapper>(sql) |> Async.AwaitTask
                return (r |> Seq.map (fun x-> x.Deserialize()) |> Seq.toList )
            }
    let private addReplace (dbFN:string) = 
        fun(survey) ->
            let wrapper = SurveyWrapper.create survey
            async {
                try 
                    let! conn = SQLiteHelper.getConnection(dbFN)
                    let! existing = getW dbFN survey.Id
                    let! r = existing |> function
                                | None -> async { let! r1 = conn.InsertAsync wrapper |> Async.AwaitTask
                                                return r1 }
                                | Some w -> async { let w1 = {wrapper with ID = w.ID }
                                                    let! r1 = conn.UpdateAsync w1 |> Async.AwaitTask
                                                    return r1 } 
                    return (survey |> ok)  
                with 
                | _ -> return "Failed to save survey"  |> fail                                                                                       
            }
    let getRepository(dbFN:string) = 
        {   SurveyDefinitionRepository.get = get(dbFN)
            getAll = getAll(dbFN)
            addUpdate = addReplace(dbFN) }

module SurveyResponseRepositoryImpl = 
        
    open SQLiteHelper

    let private getW (dbFN : string) =
        fun (id : Guid) -> 
            let sql = sprintf "select * from SurveyResponseWrapper where entityid='%s'" (id |> string)
            async {
                let! conn = SQLiteHelper.getConnection(dbFN)
                let! r = conn.QueryAsync<SurveyResponseWrapper>(sql) |> Async.AwaitTask
                let r1 = r |> Seq.length
                            |> function
                            | 1 -> r.[0] |> Some
                            | _ -> None
                return r1}

    let private get(dbFN : string) =
        fun (id : Guid) -> 
            async {
                let! r1 = getW dbFN id
                return (r1 |> Option.map (fun x -> x.Deserialize()))
            }
    let private getBySurveyDefinition (dbFN : string) = 
        fun (surveyDefId : Guid) -> 
            let sql = sprintf "select * from SurveyResponseWrapper where surveydefinitionid='%s'" (surveyDefId |> string)
            async {
                let! conn = SQLiteHelper.getConnection(dbFN)
                let! r = conn.QueryAsync<SurveyResponseWrapper>(sql) |> Async.AwaitTask
                return (r |> Seq.map (fun x-> x.Deserialize()) |> Seq.toList )
            }

    let private addReplace (dbFN:string) = 
        fun(surveyResponse) ->
            let wrapper = SurveyResponseWrapper.create surveyResponse
            async {
                try 
                    let! conn = SQLiteHelper.getConnection(dbFN)
                    let! existing = getW dbFN surveyResponse.Id
                    let! r = existing |> function
                                | None -> async { let! r1 = conn.InsertAsync wrapper |> Async.AwaitTask
                                                  return r1 }
                                | Some w -> async { let w1 = {wrapper with ID = w.ID }
                                                    let! r1 = conn.UpdateAsync w1 |> Async.AwaitTask
                                                    return r1 } 
                    return (surveyResponse |> ok)  
                with 
                | _ -> return "Failed to save survey response"  |> fail                                                                                       
            }
    let getRepository(dbFN:string) = 
        {   SurveyResponseRepository.get = get(dbFN)
            getBySurveyDefinition = getBySurveyDefinition(dbFN)
            addUpdate = addReplace(dbFN) }

module RepositoryBuilder = 
    let getRepositories dbFN  = 
        { Repositories.surveyRepository = SurveyDefinitionRepositoryImpl.getRepository dbFN
          Repositories.surveyResponseRespository = SurveyResponseRepositoryImpl.getRepository dbFN }

