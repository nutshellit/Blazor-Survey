namespace Survey.Api.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Survey.Lib
open Survey.Shared.SurveyVM
open System.Diagnostics
open Survey.Shared
open Chessie.ErrorHandling


[<Route("api/[controller]")>]
[<ApiController>]
type SurveyEditController () =
    inherit ControllerBase()
    let dbLoc = SurveyEndpointShared.dbConnLocation()
    
    let getRepo(dbLoc:string) = 
        RepositoryBuilder.getRepositories dbLoc
    
    let getEditService (dbLoc : string) = 
        getRepo dbLoc |> SurveyDefinitionEditServiceImpl.getService
    
    let mapResult (result : Result<CmdExecutionResult<SurveyDefinition>,string>) = 
        result |> function 
        | Ok (sd,_) -> let vm = SurveyEditDTO.Map(sd.Result)
                       CommandSubmitResult<_>.create sd.Result.Id Array.empty vm  CommandSubmitExecutionResult.OK sd.ItemIdUpdated
        | Bad m -> CommandSubmitResult<_>.create Guid.Empty (m |> Seq.toArray) (SurveyEditDTO())CommandSubmitExecutionResult.Fail Guid.Empty


    [<HttpGet>]
    [<Route("getall")>]
    member this.GetAll() =
        let surveyRepo = getRepo dbLoc
        async 
            { let! r=  surveyRepo.surveyRepository.getAll()
              let r1 = r |> Seq.map (fun sd -> SurveyEditDTO.Map(sd) ) 
                         |> Seq.toArray
              Trace.WriteLine("blazor hit #1")
              //return ActionResult<_>(r1) 
              return r1
              }    
        
    [<HttpGet>]
    [<Route("get/{id}")>]
    member this.Get(id) = 
        let surveyRepo = getRepo dbLoc
        async 
            { let! r = surveyRepo.surveyRepository.get id
              return (SurveyEditDTO.Map(r.Value))}

    [<HttpPost>]
    [<Route("addsurvey")>]
    member this.AddSurvey(cmd : SurveyDefinitionCreateCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.create cmd
            return (r |> mapResult)
        }

    [<HttpPost>]
    [<Route("addsection")>]
    member this.AddSection(cmd : SurveyDefinitionAddSectionCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.addSection cmd
            return (r |> mapResult)
        }

    [<HttpPost>]
    [<Route("deletesection")>]
    member this.DeleteSection(cmd : SurveyDefinitionRemoveSurveyItemCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.removeSurveyItem cmd
            return (r |> mapResult)
        }

    [<HttpPost>]
    [<Route("addtextquestion")>]
    member this.AddTextQuestion(cmd : SurveyDefinitionTextQuestionAddToSectionCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.addTextQuestion cmd
            return (r |> mapResult)
        }

    [<HttpPost>]
    [<Route("addratingquestion")>]
    member this.AddRatingQuestion(cmd : SurveyDefinitionAddRatingQuestionToSectionCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.addRatingQuestion cmd
            return (r |> mapResult)
        }

    [<HttpPost>]
    [<Route("addsingleoptionquestion")>]
    member this.AddSingleOptionQuestion(cmd : SurveyDefinitionSingleOptionQuestionAddToSectionCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.addSingleOptionQuestion cmd
            return (r |> mapResult)
        }
    [<HttpPost>]
    [<Route("addmultioptionquestion")>]
    member this.AddMultiOptionQuestion(cmd : SurveyDefinitionMultiOptionQuestionAddToSectionCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.addMultiOptionQuestion cmd
            let r1 = r |> mapResult
            return (r |> mapResult)
        }
    [<HttpPost>]
    [<Route("addoptiontomultioptionquestion")>]
    member this.AddOptionToMultiOptionQuestion(cmd : AddOptionToMultiOptionQuestionCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.addOptionToMultiOptionQuestion cmd
            return (r |> mapResult)
        }
    [<HttpPost>]
    [<Route("addoptiontosingleoptionquestion")>]
    member this.AddOptionToSingleOptionQuestion(cmd : AddOptionToSingleOptionQuestionCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.addOptionToSingleOptionQuestion cmd
            return (r |> mapResult)
        }

    [<HttpPost>]
    [<Route("moveitemup")>]
    member this.MoveItemUp(cmd : SurveyDefinitionMoveSurveyItemUpCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.moveSurveyItemUp cmd
            return (r |> mapResult)
        }

    [<HttpPost>]
    [<Route("moveitemdown")>]
    member this.MoveItemDown(cmd : SurveyDefinitionMoveSurveyItemDownCmd) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.moveSurveyItemDown cmd
            return (r |> mapResult)
        }

    [<HttpPost>]
    [<Route("makelive")>]
    member this.MakeLive(cmd : SurveyDefinitionMakeLiveCmd ) = 
        let editService = getEditService dbLoc
        async {
            let! r = editService.makeLive cmd
            return (r |> mapResult)
        }
    

        
        


    
