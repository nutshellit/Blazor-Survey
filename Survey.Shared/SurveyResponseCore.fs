namespace Survey.Shared

open System
[<AutoOpen>]
module rec SurveyResponseCore = 
    type SurveyResponse = 
        { Id : Guid
          SurveyDefinitionId : Guid
          DateStarted : DateTime
          Status : SurveyResponseStatus
          Responses : QuestionResponse list
          }
    type SurveyResponseStatus = 
        | Started 
        | Completed
        | Cancelled
        | Submitted 

    type QuestionResponse =
        | TextResponse of TextResponseItem
        | RatingResponse of RatingResponseItem
        | SingleChoiceResponse of SingleChoiceResponseItem
        | MultiChoiceResponse of MultiChoiceResponseItem
        member x.SurveyItemId = 
            x |> function
            | TextResponse t -> t.SurveyItemId
            | RatingResponse r -> r.SurveyItemId
            | SingleChoiceResponse s -> s.SurveyItemId
            | MultiChoiceResponse m -> m.SurveyItemId

    type TextResponseItem = 
        { SurveyItemId : Guid
          Text : string}

    type RatingResponseItem =
        { SurveyItemId : Guid 
          Selection : int }

    type SingleChoiceResponseItem =
        { SurveyItemId : Guid 
          QuestionOptionId : Guid }

    type MultiChoiceResponseItem = 
        { SurveyItemId : Guid 
          QuestionOptionIds : Guid list }

[<AutoOpen>]
module SurveyResponseCommands = 
    type SurveyResponseCreateCmd = 
        { SurveyDefinitionId : Guid }

    type SurveyResponseTextResponseCmd = 
        { SurveyResponseId : Guid 
          SurveyItemId : Guid 
          Text : string }

    type SurveyResponseRatingResponseCmd = 
        { SurveyResponseId : Guid 
          SurveyItemId : Guid 
          Selection : int }

    type SurveyResponseSingleOptionResponseCmd = 
        { SurveyResponseId : Guid
          SurveyItemId : Guid 
          QuestionOptionId : Guid
          }

    type SurveyResponseMultiOptionResponseCmd = 
        { SurveyResponseId : Guid 
          SurveyItemId : Guid 
          QuestionOptionIds : Guid [] }

    type SurveyResponseSetCancelledCmd = 
        { SurveyResponseId : Guid }

    type SurveyResponseSetSubmittedCmd = 
        { SurveyResponseId : Guid }




