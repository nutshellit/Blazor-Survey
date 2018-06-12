namespace Survey.Shared
open System
open System.Threading.Tasks

type CommandSubmitExecutionResult = 
    | OK = 1
    | Fail = 2

type QuestionTypeEnum = 
    | Text = 1
    | Rating = 2
    | SingleChoice = 3
    | MultiChoice = 4

type SurveyDefinitionStatusEnum = 
        | Edit = 1
        | Live = 2
        | Archived = 3

type CommandSubmitResult<'a>() = 
        member val Id = Unchecked.defaultof<Guid> with get,set
        member val Messages = Array.empty<string> with get,set
        member val EntityQry = Unchecked.defaultof<'a> with get,set 
        member val Result  = CommandSubmitExecutionResult.Fail with get,set 
        member val UpdatedId = Unchecked.defaultof<Guid> with get,set
        static member create id msg qry r updatedId = 
            CommandSubmitResult<_>(
                Id = id,
                Messages = msg,
                EntityQry = qry,
                Result = r,
                UpdatedId = updatedId
                )

module rec SurveyVM =
    let uc<'a> = Unchecked.defaultof<'a>
    //example take out
    type WeatherForecast () =
        member val Date = uc<DateTime> with get, set
        member val TemperatureC = Unchecked.defaultof<int> with get, set
        member val TemperatureF = Unchecked.defaultof<int> with get, set
        member val Summary = Unchecked.defaultof<string> with get, set

    type SurveyEditDTO() =
        member val SurveyId = uc<Guid> with get,set
        member val Name = uc<string> with get,set
        member val Intro = uc<string> with get,set
        member val Status = uc<SurveyDefinitionStatusEnum> with get,set
        member val Sections = Array.empty<SurveySectionEditDTO> with get,set
    
    type SurveySectionEditDTO() = 
           member val SectionId = uc<Guid> with get,set
           member val SectionName = uc<string> with get,set
           member val SectionIntro = uc<string> with get,set
           member val Order = uc<int> with get,set
           member val Questions = Array.empty<SurveyQuestionEditDTO> with get,set
           

    type SurveyQuestionEditDTO() = 
          member val QuestionId = uc<Guid> with get,set
          member val Text = uc<string> with get,set
          member val Help = uc<string> with get,set
          member val Order = uc<int> with get,set
          member val QuestionType = uc<QuestionTypeEnum> with get,set
          member val Options = Array.empty<SurveyQuestionOptionEditDTO> with get,set
          member val RatingMin = uc<int> with get,set
          member val RatingMax = uc<int> with get,set
         
    type SurveyQuestionOptionEditDTO() = 
          member val OptionId = uc<Guid> with get,set
          member val Text = uc<string> with get,set
          member val Value = uc<string> with get,set
          member val Order = uc<int> with get,set
          static member Map(opt : QuestionOption) = 
            SurveyQuestionOptionEditDTO(OptionId = opt.Id, Text = opt.Text, Value = opt.Value, Order = opt.Order)

    

    //-- DTO MAPPERS methods
    type SurveyEditDTO with
        static member Map(sd : SurveyDefinition) = 
            SurveyEditDTO(SurveyId = sd.Id, 
                          Name = sd.Name, 
                          Intro = sd.Intro,
                          Status = (sd.Status |> function   
                                      | Edit -> SurveyDefinitionStatusEnum.Edit 
                                      | Live -> SurveyDefinitionStatusEnum.Live
                                      | Archived -> SurveyDefinitionStatusEnum.Archived
                                    ),
                          Sections = (sd.Sections()
                                         |> Seq.map (fun sect -> SurveySectionEditDTO.Map sect sd)
                                         |> Seq.sortBy (fun (sect : SurveySectionEditDTO) -> sect.Order)
                                         |> Seq.toArray))
        member x.TotalNoQuestions() = 
            x.Sections |> Seq.map (fun x -> x.Questions)
                       |> Seq.concat
                       |> Seq.length
        

    type SurveySectionEditDTO with
           static member Map (ss : SurveySection) (sd : SurveyDefinition)  = 
                SurveySectionEditDTO(SectionId = ss.SurveyItemId
                                     ,SectionName = ss.Name
                                     ,SectionIntro = ss.SectionIntro
                                     ,Order = ss.Order
                                     ,Questions = (sd.QuestionsBySection ss.SurveyItemId
                                                  |> Seq.map (fun q -> q.Question |> function 
                                                                        | SingleChoice sc -> SurveyQuestionEditDTO.mapSingleChoiceQ sc q.SurveyItemId q.Order
                                                                        | MultiChoice mc -> SurveyQuestionEditDTO.mapMultiChoiceQ mc q.SurveyItemId q.Order
                                                                        | SimpleText t -> SurveyQuestionEditDTO.mapTextQ t q.SurveyItemId q.Order
                                                                        | Rating r -> SurveyQuestionEditDTO.mapRatingQ r q.SurveyItemId q.Order)
                                                  |> Seq.sortBy (fun (q :SurveyQuestionEditDTO) -> q.Order)
                                                  |> Seq.toArray))

    type SurveyQuestionEditDTO with
          static  member private MapCommon id txt help order qType = 
                 SurveyQuestionEditDTO(
                              QuestionId = id,
                              Text = txt,
                              Help = help,
                              Order = order ,
                              QuestionType = qType,
                              Options = Array.empty,
                              RatingMin = -1,
                              RatingMax = -1 ) 
          static member mapTextQ (t: SingleTextQuestion) id order = 
              SurveyQuestionEditDTO.MapCommon id t.Common.Text t.Common.Help order QuestionTypeEnum.Text
              
          static member mapRatingQ (r: RatingQuestion) id order = 
              SurveyQuestionEditDTO.MapCommon id r.Common.Text r.Common.Help order QuestionTypeEnum.Rating
              |> fun dto -> dto.RatingMax <- r.Max
                            dto.RatingMin <- r.Min
                            dto
                      
          static member mapSingleChoiceQ (sc : SingleOptionQuestion) id order = 
            SurveyQuestionEditDTO.MapCommon id sc.Common.Text sc.Common.Help order QuestionTypeEnum.SingleChoice
            |> fun dto -> dto.Options <- sc.Options |> Seq.sortBy (fun x -> x.Order ) 
                                         |> Seq.map SurveyQuestionOptionEditDTO.Map 
                                         |> Seq.toArray
                          dto
          static member mapMultiChoiceQ (sc : MultiOptionQuestion) id order = 
            SurveyQuestionEditDTO.MapCommon id sc.Common.Text sc.Common.Help order QuestionTypeEnum.MultiChoice
            |> fun dto -> dto.Options <- sc.Options |> Seq.sortBy (fun x -> x.Order ) 
                                         |> Seq.map SurveyQuestionOptionEditDTO.Map 
                                         |> Seq.toArray
                          dto

open SurveyVM
type SurveyEditSubmitResult = CommandSubmitResult<SurveyEditDTO> Task
//contract to be implemented by client to edit surveys
type ISurveyEditClient = 
    abstract member CachedSurvey : SurveyEditDTO with get
    abstract member GetSurvey : Guid -> Task<SurveyEditDTO>
    abstract member GetSurveys : unit -> Task<SurveyEditDTO[]>
    abstract member AddSurvey : SurveyDefinitionCreateCmd -> SurveyEditSubmitResult
    abstract member AddSection : SurveyDefinitionAddSectionCmd -> SurveyEditSubmitResult 
    abstract member DeleteSurveyItem : SurveyDefinitionRemoveSurveyItemCmd -> SurveyEditSubmitResult
    abstract member AddTextQuestion : SurveyDefinitionTextQuestionAddToSectionCmd -> SurveyEditSubmitResult
    abstract member AddRatingQuestion : SurveyDefinitionAddRatingQuestionToSectionCmd -> SurveyEditSubmitResult
    abstract member AddSingleOptionQuestion : SurveyDefinitionSingleOptionQuestionAddToSectionCmd -> SurveyEditSubmitResult 
    abstract member AddMultiOptionQuestion : SurveyDefinitionMultiOptionQuestionAddToSectionCmd -> SurveyEditSubmitResult 
    abstract member AddOptionToMultiOptionQuestion : AddOptionToMultiOptionQuestionCmd -> SurveyEditSubmitResult 
    abstract member AddOptionToSingleOptionQuestionCmd : AddOptionToSingleOptionQuestionCmd -> SurveyEditSubmitResult 
    abstract member MoveItemUp : SurveyDefinitionMoveSurveyItemUpCmd -> SurveyEditSubmitResult
    abstract member MoveItemDown : SurveyDefinitionMoveSurveyItemDownCmd -> SurveyEditSubmitResult
    abstract member MakeLive : SurveyDefinitionMakeLiveCmd -> SurveyEditSubmitResult


