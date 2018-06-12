namespace Survey.Shared
open System

[<AutoOpen>]
module rec SurveyDefinitionCore = 
    type SurveyDefinition = 
        { Id : Guid
          Name : string 
          Intro : string
          Status : SurveyDefinitionStatus
          Items : SurveyItem list
            }
        
        member x.Sections() = 
            x.Items |> Seq.choose (fun x -> x |> function | SurveySectionItem s-> s |> Some | _ -> None )
        member x.Questions() = 
            x.Items |> Seq.choose (fun x -> x |> function | SurveyQuestionItem s-> s |> Some | _ -> None )  
        member x.QuestionsBySection sectionId = 
            x.Items |> Seq.choose (fun x -> x |> function | SurveyQuestionItem s-> s |> Some | _ -> None )
                    |> Seq.filter (fun x -> x.ParentSectionId = sectionId)  

    type SurveyDefinitionStatus = 
        | Edit
        | Live
        | Archived
        override x.ToString() = 
            x |> function
            | Edit -> "Edit"
            | Live -> "Live"
            | Archived -> "Archived"

    type SurveySection = 
        { SurveyItemId : Guid
          Name : string 
          SectionIntro : string
          Order : int
            }

    type QuestionItem = 
        { SurveyItemId : Guid 
          ParentSectionId : Guid
          Question : SurveyQuestion
          Order : int }        

    type SurveyQuestion =
        | SingleChoice of SingleOptionQuestion
        | MultiChoice of MultiOptionQuestion
        | SimpleText of SingleTextQuestion
        | Rating of RatingQuestion

    type SurveyQuestionCommon = 
        { Text : string
          Help : string  }

    type QuestionOption = 
        { Id : Guid
          Text : string
          Order : int
          Value : string }  

    type SingleOptionQuestion = 
        { Common : SurveyQuestionCommon
          Options : QuestionOption list }

    type MultiOptionQuestion = 
        { Common : SurveyQuestionCommon
          Options : QuestionOption list }

    type SingleTextQuestion = 
        { Common : SurveyQuestionCommon }         

    type RatingQuestion = 
        { Common : SurveyQuestionCommon
          Min : int
          Max : int }

    type SurveyItem = 
        | SurveySectionItem of SurveySection
        | SurveyQuestionItem of QuestionItem
        member x.Id() = 
            x |> function | SurveySectionItem i -> i.SurveyItemId | SurveyQuestionItem i -> i.SurveyItemId

 

[<AutoOpen>]
module SurveyDefinitionEditCommands = 
    type SurveyDefinitionCreateCmd = 
        { Name : string
          Intro : string }
    [<CLIMutable>]
    type SurveyDefinitionAddSectionCmd = 
        { SurveyDefinitionId : Guid
          Name : string
          SectionIntro : string }   
    type SurveyDefinitionTextQuestionAddToSectionCmd = 
        { SurveyDefinitionId : Guid
          SectionId : Guid
          Text : string
          Help : string
           }   
    type SurveyDefinitionSingleOptionQuestionAddToSectionCmd = 
        { SurveyDefinitionId : Guid
          SectionId : Guid
          Text : string
          Help : string}
    type SurveyDefinitionMultiOptionQuestionAddToSectionCmd = 
        { SurveyDefinitionId : Guid
          SectionId : Guid
          Text : string
          Help : string} 
    type AddOptionToSingleOptionQuestionCmd = 
        { SurveyDefinitionId : Guid
          QuestionId : Guid
          Text : string
          Value : string}
    type AddOptionToMultiOptionQuestionCmd = 
        { SurveyDefinitionId : Guid
          QuestionId : Guid
          Text : string
          Value : string}          
    type SurveyDefinitionAddRatingQuestionToSectionCmd = 
        { SurveyDefinitionId : Guid
          SectionId : Guid
          MinValue : int
          MaxValue : int
          Text : string
          Help : string}    
    [<CLIMutable>]          
    type SurveyDefinitionRemoveSurveyItemCmd = 
        { SurveyDefinitionId : Guid
          ItemId : Guid }
    type SurveyDefinitionMoveSurveyItemUpCmd =               
        { SurveyDefinitionId : Guid
          ItemId : Guid }
    type SurveyDefinitionMoveSurveyItemDownCmd = 
        { SurveyDefinitionId : Guid
          ItemId : Guid }   

    type SurveyDefinitionMakeLiveCmd = 
        { SurveyDefinitionId : Guid }     

