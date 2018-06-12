namespace Survey.Lib
open Survey.Shared.SurveyResponseCore
open System
open Survey.Shared.SurveyResponseCommands
open Chessie.ErrorHandling

module SurveyResponseServices = 
    

    type SurveyResponseEditService = 
        { create : SurveyResponseCreateCmd -> Result<CmdExecutionResult<SurveyResponse>,string> Async
          updateTextResponse : SurveyResponseTextResponseCmd -> Result<CmdExecutionResult<SurveyResponse>,string> Async
          updateRatingResponse : SurveyResponseRatingResponseCmd -> Result<CmdExecutionResult<SurveyResponse>,string> Async
          updateSingleOptionResponse : SurveyResponseSingleOptionResponseCmd -> Result<CmdExecutionResult<SurveyResponse>,string> Async
          updateMultiOptionResponse : SurveyResponseMultiOptionResponseCmd -> Result<CmdExecutionResult<SurveyResponse>,string> Async
          setCancelled : SurveyResponseSetCancelledCmd -> Result<CmdExecutionResult<SurveyResponse>,string> Async
          setSubmitted : SurveyResponseSetSubmittedCmd -> Result<CmdExecutionResult<SurveyResponse>,string> Async
          }

    module SurveyResponseEditServiceImpl = 
        open Survey.Shared.SurveyDefinitionCore

        let private save (repo : SurveyResponseRepository) sr = 
            repo.addUpdate sr |> AsyncResult.AR
        let private get (repo : SurveyResponseRepository) id = 
            repo.get id

        let private getSurvey (repo : SurveyDefinitionRepository) id =
            repo.get id

        let private create repo =
            fun (cmd : SurveyResponseCreateCmd) -> 
                asyncTrial {
                    let id = Guid.NewGuid()
                    let sr = 
                        { SurveyResponse.Id = id
                          SurveyDefinitionId = cmd.SurveyDefinitionId
                          DateStarted = DateTime.Now
                          Status = Started
                          Responses = List.empty
                         }
                    let! r = save repo sr
                    return (r |> CmdExecutionResult<_>.create id)
                } |> Async.ofAsyncResult
        type internal ExistingResponseInfo = 
            { Survey : SurveyDefinition
              Response : SurveyResponse
              Question : QuestionItem 
              QuestionResponse : QuestionResponse option}
            member x.NoQuestions = 
                x.Survey.Questions() |> Seq.length
                
        let private lookupSurveyResponse_SurveyDef_AndSurveyItem repo (surveyResponseId : Guid) (surveyItemQuestionId : Guid) =
            asyncTrial {
                    let! surveyResponseOpt = get repo.surveyResponseRespository surveyResponseId
                    let! surveyResponse = surveyResponseOpt |> failIfNone "Failed to lookup survey response"
                    let! surveyOpt = getSurvey repo.surveyRepository surveyResponse.SurveyDefinitionId
                    let! survey = surveyOpt |> failIfNone "Failed to lookup survey definition"
                    let! survey = survey.Status |> function 
                                  | Live -> survey |> ok
                                  | _ -> fail "Survey must be live in order to respond"
                    let! question = survey.Questions() 
                                    |> Seq.tryFind (fun x -> x.SurveyItemId = surveyItemQuestionId)
                                    |> failIfNone "Failed to lookup survey item in survey definition"
                    let existingResponse = surveyResponse.Responses |> Seq.tryFind (fun x -> x.SurveyItemId = surveyItemQuestionId)
                    let r = { ExistingResponseInfo.Survey = survey 
                              Response = surveyResponse
                              Question = question
                              QuestionResponse = existingResponse}
                    return r
            }

        let private updateResponse (sri : QuestionResponse)(sr : SurveyResponse)  = 
            { sr with Responses =   sr.Responses     
                                    |> List.filter (fun x -> x.SurveyItemId <> sri.SurveyItemId )
                                    |> fun x -> sri :: x
                                    }

        let private validateResponseStatus (sr : SurveyResponse) = 
            sr.Status |> function
            | Started | Completed -> sr |> ok
            | _ -> fail "Invalid status unable to execute command"

        let private updateToCompletedIfAllQuestionsAnswered (noQuestions : int) (sr : SurveyResponse) = 
            sr.Responses |> Seq.length = noQuestions
            |> function
            | true -> { sr with Status = Completed }
            | false -> sr

        let private updateTextResponse repo = 
            fun (cmd : SurveyResponseTextResponseCmd) -> 
                asyncTrial {
                    let! lookupResponseInfo = lookupSurveyResponse_SurveyDef_AndSurveyItem repo cmd.SurveyResponseId cmd.SurveyItemId
                    let! sr = validateResponseStatus lookupResponseInfo.Response
                    let! validatedTxt = cmd.Text |> String.IsNullOrWhiteSpace
                                            |> function
                                            | true -> fail "Invalid text submitted"
                                            | false -> cmd.Text |> ok
                    let! newAnswer = lookupResponseInfo.Question.Question |> function 
                                     | SimpleText _   -> {TextResponseItem.SurveyItemId = cmd.SurveyItemId
                                                          Text = validatedTxt} 
                                                          |> TextResponse
                                                          |> ok
                                     | _ -> fail "The question is not a text question"
                    let sr = lookupResponseInfo.Response 
                             |> updateResponse newAnswer
                             |> updateToCompletedIfAllQuestionsAnswered lookupResponseInfo.NoQuestions
                   
                    let! r = save repo.surveyResponseRespository sr
                    return (r |> CmdExecutionResult<_>.create cmd.SurveyItemId)
                } |> Async.ofAsyncResult
                
        let private updateRatingResponse repo = 
            fun (cmd : SurveyResponseRatingResponseCmd) -> 
                asyncTrial {
                    let! lookupResponseInfo = lookupSurveyResponse_SurveyDef_AndSurveyItem repo cmd.SurveyResponseId cmd.SurveyItemId
                    let! sr = validateResponseStatus lookupResponseInfo.Response
                    let! ratingQ = lookupResponseInfo.Question.Question |> function     
                                   | Rating r -> r |> ok
                                   | _ -> fail "Is not a rating questions"
                    let! validatedRating = (cmd.Selection >= ratingQ.Min && cmd.Selection <= ratingQ.Max)
                                           |> function
                                           | true -> cmd.Selection |> ok
                                           | false -> fail "Submitted amount is out of range"
                    let newAnswer = {RatingResponseItem.SurveyItemId = cmd.SurveyItemId
                                     Selection = cmd.Selection} |> RatingResponse 
                    let sr = lookupResponseInfo.Response 
                             |> updateResponse newAnswer
                             |> updateToCompletedIfAllQuestionsAnswered lookupResponseInfo.NoQuestions
                    let! r = save repo.surveyResponseRespository sr
                    return (r |> CmdExecutionResult<_>.create cmd.SurveyItemId)
                } |> Async.ofAsyncResult

        let private updateSingleOptionResponse repo = 
            fun (cmd : SurveyResponseSingleOptionResponseCmd) -> 
                asyncTrial{
                    let! lookupResponseInfo = lookupSurveyResponse_SurveyDef_AndSurveyItem repo cmd.SurveyResponseId cmd.SurveyItemId
                    let! sr = validateResponseStatus lookupResponseInfo.Response
                    let! questionOK = lookupResponseInfo.Question.Question |> function        
                                        | SingleChoice sc -> sc.Options |> Seq.tryFind (fun x -> x.Id = cmd.QuestionOptionId)
                                                             |> function 
                                                             | Some _ -> sc |> ok
                                                             | None -> fail "Invalid question option"
                                        | _ -> fail "Is not a single option question"
                    let newAnswer = { SingleChoiceResponseItem.QuestionOptionId = cmd.QuestionOptionId
                                      SurveyItemId = cmd.SurveyItemId} |> SingleChoiceResponse
                    let sr = lookupResponseInfo.Response 
                             |> updateResponse newAnswer
                             |> updateToCompletedIfAllQuestionsAnswered lookupResponseInfo.NoQuestions
                    let! r = save repo.surveyResponseRespository sr
                    return (r |> CmdExecutionResult<_>.create cmd.SurveyItemId)
                } |> Async.ofAsyncResult

        let private updateMultiOptionResponse repo = 
            fun (cmd : SurveyResponseMultiOptionResponseCmd) -> 
                asyncTrial{
                    let! lookupResponseInfo = lookupSurveyResponse_SurveyDef_AndSurveyItem repo cmd.SurveyResponseId cmd.SurveyItemId
                    let! sr = validateResponseStatus lookupResponseInfo.Response
                    let! questionOK = lookupResponseInfo.Question.Question |> function        
                                      | MultiChoice mc -> 
                                                    cmd.QuestionOptionIds 
                                                    |> Seq.map (fun anOptId -> 
                                                                        mc.Options 
                                                                        |> Seq.tryFind (fun x -> x.Id = anOptId )
                                                                        |> failIfNone "Failed to locate option in multi option question"
                                                                        )
                                                    
                                                    |> Trial.collect
                                        | _ -> fail "Is not a single option question"
                    let newAnswer = { MultiChoiceResponseItem.QuestionOptionIds = cmd.QuestionOptionIds |> Seq.toList
                                      SurveyItemId = cmd.SurveyItemId} |> MultiChoiceResponse
                    let sr = lookupResponseInfo.Response 
                             |> updateResponse newAnswer
                             |> updateToCompletedIfAllQuestionsAnswered lookupResponseInfo.NoQuestions
                    let! r = save repo.surveyResponseRespository sr
                    return (r |> CmdExecutionResult<_>.create cmd.SurveyItemId)
                } |> Async.ofAsyncResult

        let private setCancelled repo = 
            fun (cmd : SurveyResponseSetCancelledCmd) ->
                asyncTrial {
                    let! surveyResponseOpt = get repo cmd.SurveyResponseId
                    let! surveyResponse = surveyResponseOpt |> failIfNone "Failed to lookup survey response"
                    let! validatedResponse = surveyResponse.Status |> function
                                             | Started -> surveyResponse |> ok
                                             | _ -> fail "Invalid status on survey response - should be Started status"
                    let sr = { surveyResponse with Status = Cancelled}
                    let! r = save repo sr
                    return (r |> CmdExecutionResult<_>.create cmd.SurveyResponseId)
                } |> Async.ofAsyncResult

        let private setSubmitted repo = 
            fun (cmd : SurveyResponseSetSubmittedCmd) ->
                asyncTrial {
                    let! surveyResponseOpt = get repo cmd.SurveyResponseId
                    let! surveyResponse = surveyResponseOpt |> failIfNone "Failed to lookup survey response"
                    let! validatedResponse = surveyResponse.Status |> function
                                             | Completed -> surveyResponse |> ok
                                             | _ -> fail "Invalid status on survey response - should be completed status"
                    let sr = { surveyResponse with Status = Submitted}
                    let! r = save repo sr
                    return (r |> CmdExecutionResult<_>.create cmd.SurveyResponseId)
                } |> Async.ofAsyncResult
                

        let getService (repo : Repositories) = 
            { SurveyResponseEditService.create = create repo.surveyResponseRespository
              updateTextResponse = updateTextResponse repo
              updateRatingResponse = updateRatingResponse repo
              updateSingleOptionResponse = updateSingleOptionResponse repo
              updateMultiOptionResponse = updateMultiOptionResponse repo
              setCancelled = setCancelled repo.surveyResponseRespository
              setSubmitted = setSubmitted repo.surveyResponseRespository
              }

