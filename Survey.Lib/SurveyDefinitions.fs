namespace Survey.Lib

open System
open Chessie.ErrorHandling
open System.Net.Http
open Survey.Shared
open Survey.Shared.SurveyDefinitionCore
            

[<AutoOpen>]
module SurveyDefinitionServices = 
     

    type CmdExecutionResult<'a> = 
        { 
            Result : 'a
            //need to feedback new ids 
            ItemIdUpdated : Guid
        }
        static member create id result = 
            { Result = result
              ItemIdUpdated = id}

    type SurveyDefinitionEditService = 
        { create : SurveyDefinitionCreateCmd -> Result<CmdExecutionResult<SurveyDefinition>, string> Async
          addSection : SurveyDefinitionAddSectionCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async
          addTextQuestion : SurveyDefinitionTextQuestionAddToSectionCmd ->  Result<CmdExecutionResult<SurveyDefinition>,string> Async
          addSingleOptionQuestion : SurveyDefinitionSingleOptionQuestionAddToSectionCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async 
          addMultiOptionQuestion : SurveyDefinitionMultiOptionQuestionAddToSectionCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async
          addRatingQuestion : SurveyDefinitionAddRatingQuestionToSectionCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async
          addOptionToSingleOptionQuestion : AddOptionToSingleOptionQuestionCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async
          addOptionToMultiOptionQuestion : AddOptionToMultiOptionQuestionCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async
          removeSurveyItem : SurveyDefinitionRemoveSurveyItemCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async
          moveSurveyItemUp : SurveyDefinitionMoveSurveyItemUpCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async
          moveSurveyItemDown : SurveyDefinitionMoveSurveyItemDownCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async
          makeLive : SurveyDefinitionMakeLiveCmd -> Result<CmdExecutionResult<SurveyDefinition>,string> Async
          } 

    module SurveyDefinitionEditServiceImpl = 
        let private save (repo: SurveyDefinitionRepository ) sd =  
            repo.addUpdate sd |> AsyncResult.AR
        let private get (repo: SurveyDefinitionRepository ) id =
            repo.get id     
        let private validateStr (str : string) (mess : string) = 
            str |> String.IsNullOrWhiteSpace |> function 
            | true -> fail mess
            | false -> str |> ok   
        let private validateIsEditable surveyId (repo: SurveyDefinitionRepository )  = 
            asyncTrial {
                let! rOpt = repo.get surveyId
                let! r = rOpt |> failIfNone "Survey Definition not found"
                let! r1 = r.Status |> function 
                         | Edit -> r |> ok
                         | Live -> "A live survey cannot be edited" |> fail
                         | Archived -> "An archived survey cannot be edited" |> fail
                return r1                  
            }   

        let private validateSection (survey :SurveyDefinition) (sectionId : Guid) = 
            trial {
                let! section = survey.Sections() 
                               |> Seq.tryFind (fun x -> x.SurveyItemId = sectionId) 
                               |> failIfNone "Failed to locate section"
                return section                   
            }

        let private validateIsSurveyItem (survey : SurveyDefinition) (surveyItemId : Guid) = 
            trial {
                let! item = survey.Items
                            |> Seq.tryFind(fun x -> x.Id() = surveyItemId)
                            |> failIfNone "Failed to locate survey item"
                return item
            }

        let private create repo  = 
            fun (cmd : SurveyDefinitionCreateCmd)  -> 
                asyncTrial {
                    let id = Guid.NewGuid()
                    let! name = validateStr cmd.Name "Name is required"
                    let sd = 
                        { SurveyDefinition.Id = id
                          Name = name
                          Intro = cmd.Intro
                          Status = Edit 
                          Items = List.empty }
                    let! r = save repo sd 
                    return (r |> CmdExecutionResult<_>.create id)
                } |> Async.ofAsyncResult
        let private addSection repo = 
            fun (cmd : SurveyDefinitionAddSectionCmd) -> 
                asyncTrial {
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! sectionName = validateStr cmd.Name "Section name is required"
                    let sectionId = Guid.NewGuid()
                    let section = {SurveySection.SurveyItemId = sectionId
                                   Name = sectionName
                                   SectionIntro = cmd.SectionIntro
                                   Order = (survey.Sections() |> Seq.length) + 1    }
                                  |> SurveySectionItem
                    let survey = { survey with Items = survey.Items @ [section] }  
                    let! r = save repo survey 
                    return (survey |> CmdExecutionResult<_>.create sectionId)                               
                } |> Async.ofAsyncResult   

        let private addTextQuestion repo = 
            fun (cmd :SurveyDefinitionTextQuestionAddToSectionCmd) -> 
                asyncTrial {
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! section = validateSection survey cmd.SectionId
                    let qid = Guid.NewGuid()
                    let question = {QuestionItem.SurveyItemId = qid
                                    ParentSectionId = section.SurveyItemId
                                    Question = { SingleTextQuestion.Common = { Text = cmd.Text; Help = cmd.Help }  } 
                                               |> SimpleText
                                    Order = (survey.QuestionsBySection section.SurveyItemId |> Seq.length) + 1
                                     } |> SurveyQuestionItem
                    let survey = { survey with Items = survey.Items @ [question] }   
                    let! r = save repo survey 
                    return (survey |> CmdExecutionResult<_>.create qid)
                } |> Async.ofAsyncResult    

        let private addSingleOptionQuestion repo = 
            fun (cmd : SurveyDefinitionSingleOptionQuestionAddToSectionCmd) -> 
                asyncTrial {
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! section = validateSection survey cmd.SectionId
                    let qid = Guid.NewGuid()
                    let question = {QuestionItem.SurveyItemId = qid
                                    ParentSectionId = section.SurveyItemId
                                    Question = { SingleOptionQuestion.Common = { Text = cmd.Text; Help = cmd.Help }
                                                 Options = List.empty  } 
                                                |> SingleChoice
                                    Order = (survey.QuestionsBySection section.SurveyItemId |> Seq.length) + 1                                                 
                                    }|> SurveyQuestionItem
                    let survey = { survey with Items = survey.Items @ [question] }   
                    let! r = save repo survey 
                    return (survey |> CmdExecutionResult<_>.create qid)
                } |> Async.ofAsyncResult   

        let private validateSingleOptionQuestion (questionId : Guid) (survey:SurveyDefinition) = 
            trial {
                let! q = survey.Questions() 
                        |> Seq.tryFind (fun x -> x.SurveyItemId = questionId)
                        |> Option.bind (fun x -> x.Question |> function | SingleChoice sc -> (sc, x) |> Some | _ -> None  )
                        |> failIfNone "Failed to lookup single option question"
                return q    
            }
        let private addOptionToSingleOptQuestion repo = 
            fun (cmd : AddOptionToSingleOptionQuestionCmd) -> 
                asyncTrial {
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! (soq,q) = validateSingleOptionQuestion cmd.QuestionId survey
                    let optId = Guid.NewGuid()
                    let opt = {Id = optId; QuestionOption.Text = cmd.Text; Value = cmd.Value; Order = (soq.Options |> Seq.length) + 1   }
                    let sq = {soq with Options = soq.Options @ [opt] }
                              |> SingleChoice 
                              |> fun sc -> { q with Question = sc }
                              |> SurveyQuestionItem
                    let survey = survey.Items 
                                |> List.filter (fun x -> x.Id() <> sq.Id() )
                                |> fun items -> { survey with Items = items @ [sq] }
                    let! r = save repo survey                         
                    return (survey |> CmdExecutionResult<_>.create optId)
                }  |> Async.ofAsyncResult    

        let private addMultiOptionQuestion repo = 
            fun (cmd : SurveyDefinitionMultiOptionQuestionAddToSectionCmd) -> 
                asyncTrial {
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! section = validateSection survey cmd.SectionId
                    let qid = Guid.NewGuid()
                    let question = {QuestionItem.SurveyItemId = qid
                                    ParentSectionId = section.SurveyItemId
                                    Question = { MultiOptionQuestion.Common = { Text = cmd.Text; Help = cmd.Help }
                                                 Options = List.empty  } 
                                                |> MultiChoice
                                    Order = (survey.QuestionsBySection section.SurveyItemId |> Seq.length) + 1                                                 
                                    }|> SurveyQuestionItem
                    let survey = { survey with Items = survey.Items @ [question] }   
                    let! r = save repo survey 
                    return (survey |> CmdExecutionResult<_>.create qid)
                } |> Async.ofAsyncResult                                     
        let private validateMultiOptionQuestion (questionId : Guid) (survey:SurveyDefinition) = 
            trial {
                let! q = survey.Questions() 
                        |> Seq.tryFind (fun x -> x.SurveyItemId = questionId)
                        |> Option.bind (fun x -> x.Question |> function | MultiChoice sc -> (sc, x) |> Some | _ -> None  )
                        |> failIfNone "Failed to lookup multi option question"
                return q    
            }
        let private addOptionToMultiOptQuestion repo = 
            fun (cmd : AddOptionToMultiOptionQuestionCmd) -> 
                asyncTrial {
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! (soq,q) = validateMultiOptionQuestion cmd.QuestionId survey
                    let optId = Guid.NewGuid()
                    let opt = {Id = optId; QuestionOption.Text = cmd.Text; Value = cmd.Value; Order = (soq.Options |> Seq.length) + 1   }
                    let sq = {soq with Options = soq.Options @ [opt] }
                              |> MultiChoice 
                              |> fun sc -> { q with Question = sc }
                              |> SurveyQuestionItem
                    let survey = survey.Items 
                                |> List.filter (fun x -> x.Id() <> sq.Id() )
                                |> fun items -> { survey with Items = items @ [sq] }
                    let! r = save repo survey                         
                    return (survey |> CmdExecutionResult<_>.create optId)
                }  |> Async.ofAsyncResult  

        let private addRatingQuestion repo = 
            fun (cmd : SurveyDefinitionAddRatingQuestionToSectionCmd) ->
                asyncTrial {
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! section = validateSection survey cmd.SectionId
                    let qid = Guid.NewGuid()
                    let question = { QuestionItem.SurveyItemId = qid
                                     ParentSectionId = section.SurveyItemId
                                     Question = { RatingQuestion.Common = { Text = cmd.Text; Help = cmd.Help }
                                                  Min = cmd.MinValue
                                                  Max = cmd.MaxValue  }
                                                |> Rating
                                     Order = (survey.QuestionsBySection section.SurveyItemId |> Seq.length) + 1                                                 
                                    }|> SurveyQuestionItem
                    let survey = { survey with Items = survey.Items @ [question] }   
                    let! r = save repo survey 
                    return (survey |> CmdExecutionResult<_>.create qid)
                    } |> Async.ofAsyncResult    

        let private makeLive repo =
            fun (cmd : SurveyDefinitionMakeLiveCmd) -> 
                asyncTrial {
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! basicCheck = survey.Questions() |> Seq.length > 0 |> function | true -> survey |> ok | false -> fail "Must have at least one question to make live"
                    let survey = { basicCheck with Status = Live } 
                    let! r = save repo survey 
                    return (survey |> CmdExecutionResult<_>.create survey.Id)
                    } |> Async.ofAsyncResult   

        let private removeSurveyItem repo = 
            
            fun (cmd : SurveyDefinitionRemoveSurveyItemCmd) -> 
                asyncTrial {
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo 
                    let! surveyItem = validateIsSurveyItem survey cmd.ItemId
                    let! isValid = //check that section survey item has no questions
                        surveyItem |> function
                        | SurveySectionItem _ -> survey.QuestionsBySection cmd.ItemId 
                                                   |> Seq.length > 0 |> function
                                                   | true -> fail "This section has existing questions"
                                                   | false -> surveyItem |> ok
                        | SurveyQuestionItem _ -> surveyItem |> ok
                    let survey = { survey with Items = survey.Items |> List.filter (fun x -> x.Id() <> cmd.ItemId ) }
                    let! r = save repo survey 
                    return (survey |> CmdExecutionResult<_>.create Guid.Empty)
                    } |> Async.ofAsyncResult   
                    
        let private itemsToSort (survey : SurveyDefinition) (item : SurveyItem) : SurveyItem seq = 
            item |> function
                        | SurveySectionItem _ -> survey.Sections() |> Seq.map  SurveySectionItem
                        | SurveyQuestionItem q -> survey.QuestionsBySection q.ParentSectionId
                                                  |> Seq.map  SurveyQuestionItem
        let private orderItems (items : SurveyItem seq) = 
            items 
            |> Seq.mapi (fun i item -> item |> function 
                                            | SurveySectionItem item -> { item with Order = i } |> SurveySectionItem
                                            | SurveyQuestionItem item -> { item with Order = i} |> SurveyQuestionItem)
        
        let private replaceSurveyItems (survey : SurveyDefinition) (newItems : SurveyItem seq) = 
                        { survey with Items = survey.Items 
                                                       |> List.filter (fun item -> not (newItems 
                                                                                |> Seq.exists( fun ci -> ci.Id() =  item.Id())))
                                                       |> fun items -> items @ (newItems |> Seq.toList)
                                                       }
       

        let private moveSurveyItemUp repo =
            fun (cmd : SurveyDefinitionMoveSurveyItemUpCmd) -> 
                asyncTrial{
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! surveyItem = validateIsSurveyItem survey cmd.ItemId
                    let currentItems = itemsToSort survey surveyItem
                    let ci = currentItems |> Seq.findIndex (fun x -> x.Id() = cmd.ItemId)
                    let ni = ci - 1
                    let sortedItems = ni = -1
                                        |> function 
                                        | true -> currentItems
                                        | false -> seq {  yield! currentItems |> Seq.take (ni)
                                                          yield! currentItems |> Seq.skip (ni + 1) |> Seq.take 1
                                                          yield! currentItems |> Seq.skip (ni) |> Seq.take 1
                                                          yield! currentItems |> Seq.skip (ci + 1) 
                                                          }
                                        |> orderItems
                    let survey = replaceSurveyItems survey sortedItems
                    let! r = save repo survey 
                    return (survey |> CmdExecutionResult<_>.create Guid.Empty)
                } |> Async.ofAsyncResult

        let private moveSurveyItemDown repo =
            fun (cmd : SurveyDefinitionMoveSurveyItemDownCmd) -> 
                asyncTrial{
                    let! survey = validateIsEditable cmd.SurveyDefinitionId repo
                    let! surveyItem = validateIsSurveyItem survey cmd.ItemId
                    let currentItems = itemsToSort survey surveyItem
                    let ci = currentItems |> Seq.findIndex (fun x -> x.Id() = cmd.ItemId)
                    let ni = ci + 1
                    let sortedItems = ni = (currentItems |> Seq.length)
                                        |> function 
                                        | true -> currentItems
                                        | false -> seq {  yield! currentItems |> Seq.take (ci)
                                                          yield! currentItems |> Seq.skip (ci + 1) |> Seq.take 1
                                                          yield! currentItems |> Seq.skip (ci) |> Seq.take 1
                                                          yield! currentItems |> Seq.skip (ci + 2) 
                                                          }
                                        |> orderItems
                    
                    let survey = replaceSurveyItems survey sortedItems
                    let! r = save repo survey 
                    return (survey |> CmdExecutionResult<_>.create Guid.Empty)
                } |> Async.ofAsyncResult

        let getService (repo: Repositories ) = 
            { SurveyDefinitionEditService.create = create repo.surveyRepository
              addSection = addSection repo.surveyRepository
              addTextQuestion = addTextQuestion repo.surveyRepository
              addSingleOptionQuestion = addSingleOptionQuestion repo.surveyRepository 
              addMultiOptionQuestion = addMultiOptionQuestion repo.surveyRepository
              addRatingQuestion = addRatingQuestion repo.surveyRepository
              addOptionToSingleOptionQuestion = addOptionToSingleOptQuestion repo.surveyRepository
              addOptionToMultiOptionQuestion = addOptionToMultiOptQuestion repo.surveyRepository
              removeSurveyItem = removeSurveyItem repo.surveyRepository
              moveSurveyItemUp = moveSurveyItemUp repo.surveyRepository 
              moveSurveyItemDown = moveSurveyItemDown repo.surveyRepository 
              makeLive = makeLive repo.surveyRepository }               
