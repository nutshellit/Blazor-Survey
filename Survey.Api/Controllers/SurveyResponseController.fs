namespace Survey.Api.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Survey.Shared.SurveyVM
open Survey.Shared
open Chessie.ErrorHandling
open Survey.Lib
open Survey.Lib.SurveyResponseServices
open Survey.Shared.SurveyResponseVM



[<Route("api/[controller]")>]
[<ApiController>]
type SurveyResponseController () =
    inherit ControllerBase()
    let dbLoc = SurveyEndpointShared.dbConnLocation()

    let getRepo(dbLoc:string) = 
        RepositoryBuilder.getRepositories dbLoc

    let getSurveyDTO (getSurvey : Guid -> SurveyDefinition option Async) (surveyId : Guid) = 
        async {
            let! r = getSurvey surveyId 
            return (SurveyEditDTO.Map(r.Value))}

    let mapResult (getSurvey : Guid -> SurveyDefinition option Async) (result : Result<CmdExecutionResult<SurveyResponse>,string>) = 
        result |> function 
        | Ok (sd,_) ->
                       async {
                        let! surveyDTO = getSurveyDTO getSurvey sd.Result.SurveyDefinitionId
                        let vm = SurveyResponseDTO.Map surveyDTO sd.Result
                        let r = CommandSubmitResult<_>.create sd.Result.Id Array.empty vm  CommandSubmitExecutionResult.OK sd.ItemIdUpdated
                        return r
                        }
        | Bad m -> CommandSubmitResult<_>.create Guid.Empty (m |> Seq.toArray) (SurveyResponseDTO())CommandSubmitExecutionResult.Fail Guid.Empty
                   |> async.Return

    let getService (repo : Repositories) = 
        let surveyResponseSrv = repo |> SurveyResponseEditServiceImpl.getService
        surveyResponseSrv

    [<HttpGet("getbysurvey/{surveyId}")>]
    member this.GetBySurvey(surveyId : Guid) =
        let repo = getRepo dbLoc
        let responseSrv = getService repo
        async {
            let! surveyDTO = getSurveyDTO repo.surveyRepository.get surveyId
            let! result = repo.surveyResponseRespository.getBySurveyDefinition surveyId
            let r = result |> Seq.map (fun sr -> SurveyResponseDTO.Map surveyDTO sr )
            return (r |> Seq.toArray)
        }

    [<HttpGet("get/{id}")>]
    member this.Get (id : Guid) = 
        let repo = getRepo dbLoc
        async {
            let! sr = repo.surveyResponseRespository.get id
            let! dto = sr |> function 
                          | None -> failwith "Invalid response"
                          | Some sr' -> async { let! surveyDTO = getSurveyDTO repo.surveyRepository.get sr'.SurveyDefinitionId
                                                let mapped = SurveyResponseDTO.Map surveyDTO sr'
                                                return mapped}
            return dto
        }

    [<HttpPost>]
    [<Route("addsurveyresponse")>]
    member this.AddSurveyResponse(cmd : SurveyResponseCreateCmd) = 
        let repo = getRepo dbLoc
        let service = getService repo
        async {
            let! r = service.create cmd
            let! mapped = mapResult repo.surveyRepository.get r
            return mapped
        }    

    [<HttpPost>]
    [<Route("addtextresponse")>]
    member this.AddTextResponse(cmd : SurveyResponseTextResponseCmd) = 
        let repo = getRepo dbLoc
        let service = getService repo
        async {
            let! r = service.updateTextResponse cmd
            let! mapped = mapResult repo.surveyRepository.get r
            return mapped
        }  

    [<HttpPost>]
    [<Route("addratingresponse")>]
    member this.AddRatingResponse(cmd : SurveyResponseRatingResponseCmd) = 
        let repo = getRepo dbLoc
        let service = getService repo
        async {
            let! r = service.updateRatingResponse cmd
            let! mapped = mapResult repo.surveyRepository.get r
            return mapped
        }  

    [<HttpPost>]
    [<Route("addsingleoptionresponse")>]
    member this.AddSingleOptionResponse(cmd : SurveyResponseSingleOptionResponseCmd) = 
        let repo = getRepo dbLoc
        let service = getService repo
        async {
            let! r = service.updateSingleOptionResponse cmd
            let! mapped = mapResult repo.surveyRepository.get r
            return mapped
        }  

    [<HttpPost>]
    [<Route("addmultioptionresponse")>]
    member this.AddMultiOptionResponse(cmd : SurveyResponseMultiOptionResponseCmd) = 
        let repo = getRepo dbLoc
        let service = getService repo
        async {
            let! r = service.updateMultiOptionResponse cmd
            let! mapped = mapResult repo.surveyRepository.get r
            return mapped
        }  

    [<HttpPost>]
    [<Route("cancelresponse")>]
    member this.CancelResponse(cmd : SurveyResponseSetCancelledCmd) = 
        let repo = getRepo dbLoc
        let service = getService repo
        async {
            let! r = service.setCancelled cmd
            let! mapped = mapResult repo.surveyRepository.get r
            return mapped
        }  

    [<HttpPost>]
    [<Route("submitresponse")>]
    member this.CompleteResponse(cmd : SurveyResponseSetSubmittedCmd) = 
        let repo = getRepo dbLoc
        let service = getService repo
        async {
            let! r = service.setSubmitted cmd
            let! mapped = mapResult repo.surveyRepository.get r
            return mapped
        }  



    



   
