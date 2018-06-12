namespace Survey.Shared

open System
open SurveyVM

type SurveyResponseStatusEnum = 
        | Started = 1
        | Completed = 2
        | Cancelled = 3
        | Submitted = 4
        

module rec SurveyResponseVM = 
    let uc<'a> = Unchecked.defaultof<'a>
    type SurveyResponseDTO() = 
        member val SurveyResponseId = uc<Guid> with get,set
        member val SurveyDefinition = uc<SurveyEditDTO> with get,set
        member val DateStarted = uc<DateTime> with get,set
        member val Status = uc<SurveyResponseStatusEnum> with get,set
        member val Responses = Array.empty<QuestionResponseDTO> with get,set

    type QuestionResponseDTO() = 
        member val SurveyItemId = uc<Guid> with get,set
        member val TextResponse = uc<string> with get,set
        member val RatingResponse = uc<int> with get,set 
        member val SingleOptionResponseOptionId = uc<Guid> with get,set 
        member val MultiOptionResponseOptionIds = uc<Guid[]> with get,set 

    
    

    type QuestionResponseDTO with 
        static member Map = 
            function 
            | TextResponse tr ->    
                    QuestionResponseDTO(SurveyItemId = tr.SurveyItemId,
                                        TextResponse = tr.Text)
            | RatingResponse rr -> 
                    QuestionResponseDTO(SurveyItemId = rr.SurveyItemId,
                                        RatingResponse = rr.Selection)
            | SingleChoiceResponse sc -> 
                    QuestionResponseDTO(SurveyItemId = sc.SurveyItemId,
                                        SingleOptionResponseOptionId = sc.QuestionOptionId)
            | MultiChoiceResponse mc -> 
                    QuestionResponseDTO(SurveyItemId = mc.SurveyItemId,
                                        MultiOptionResponseOptionIds = (mc.QuestionOptionIds |> Seq.toArray))

    //used client side only to aid viewing/editing
    type SurveyQuestionResponseDTO = 
        {
            SectionId : Guid
            SectionName : string
            SectionOrder : int
            ResponseStatus : SurveyResponseStatusEnum
            Question : SurveyQuestionEditDTO
            QuestionResponse : QuestionResponseDTO option
        }
        member x.IsAnswered() = 
            x.QuestionResponse.IsSome
    type SurveyResponseDTO with 
        static member Map(sd : SurveyEditDTO) (sr : SurveyResponse) = 
            SurveyResponseDTO(SurveyResponseId = sr.Id,
                              SurveyDefinition = sd, 
                              DateStarted = sr.DateStarted,
                              Status = (sr.Status |> function 
                                       | Started -> SurveyResponseStatusEnum.Started
                                       | Cancelled -> SurveyResponseStatusEnum.Cancelled
                                       | Completed -> SurveyResponseStatusEnum.Completed
                                       | Submitted -> SurveyResponseStatusEnum.Submitted),
                              Responses =
                                        (sr.Responses 
                                          |> Seq.map (fun x -> QuestionResponseDTO.Map x)
                                          |> Seq.toArray))
        //--  Client side calls 
        member x.TotalNoQuestions() = 
            x.SurveyDefinition.TotalNoQuestions()
        member x.NoQuestionsAnswered() = 
            x.Responses |> Seq.length
        member x.NoUnAnsweredQuestions() = 
            x.TotalNoQuestions() - x.NoQuestionsAnswered()
        member x.EditScreens() = 
            x.SurveyDefinition.Sections 
            |> Seq.map (fun s -> s.Questions 
                                 |> Seq.map (fun q -> {SurveyQuestionResponseDTO.SectionId = s.SectionId
                                                       SectionName = s.SectionName
                                                       SectionOrder = s.Order
                                                       Question = q
                                                       ResponseStatus = x.Status 
                                                       QuestionResponse = x.Responses 
                                                                          |> Seq.tryFind (fun r -> r.SurveyItemId = q.QuestionId )
                                                       }  ) )
            |> Seq.concat
            |> fun items -> query { for dto in items do    
                                        sortBy dto.SectionOrder
                                        thenBy dto.Question.Order}
            |> Seq.toArray

        member x.NextScreen(currentItemId : Guid) = 
            let screens = x.EditScreens() 
            screens |> Seq.tryFindIndex (fun s -> s.Question.QuestionId = currentItemId)
            |> Option.bind (fun idx -> screens |> Seq.tryItem (idx + 1) )

        member x.PreviousScreen (currentItemId : Guid) = 
            let screens = x.EditScreens()
            screens |> Seq.tryFindIndex(fun s -> s.Question.QuestionId = currentItemId)
            |> Option.bind (fun idx -> idx = 0 |> function 
                                       | true -> None 
                                       | false -> screens |> Seq.tryItem (idx - 1))
        
        member x.FirstScreen() = 
            x.EditScreens() |> Seq.head

        member x.LastScreen() = 
            x.EditScreens() |> Seq.last

        //get the next unanswered question
        // - if all answered then go to last
        member x.NextUnansweredQuestion() = 
            x.EditScreens()
            |> Seq.filter (fun s -> s.QuestionResponse.IsNone)
            |> Seq.tryHead
            |> function 
            | Some s -> s 
            | None -> x.LastScreen() //otherwise show last screen

        member x.ByQuestion questionId = 
            x.EditScreens()
                |> Seq.filter (fun x -> x.Question.QuestionId = questionId)
                |> Seq.tryHead
                |> function 
                | Some s -> s 
                | None -> x.LastScreen() //otherwise show last screen


open SurveyResponseVM
open System.Threading.Tasks
type SurveyResponseSubmitResult = CommandSubmitResult<SurveyResponseDTO> Task

type ISurveyResponseClient = 
    abstract member GetResponse : Guid -> Task<SurveyResponseDTO>
    abstract member GetSurveyResponses : Guid -> Task<SurveyResponseDTO []>
    abstract member AddResponse : SurveyResponseCreateCmd -> SurveyResponseSubmitResult 
    abstract member TextResponse : SurveyResponseTextResponseCmd -> SurveyResponseSubmitResult
    abstract member RatingResponse : SurveyResponseRatingResponseCmd -> SurveyResponseSubmitResult
    abstract member SingleOptionResponse : SurveyResponseSingleOptionResponseCmd -> SurveyResponseSubmitResult
    abstract member MultiOptionResponse : SurveyResponseMultiOptionResponseCmd -> SurveyResponseSubmitResult
    abstract member CancelResponse : SurveyResponseSetCancelledCmd -> SurveyResponseSubmitResult
    abstract member CompleteResponse : SurveyResponseSetSubmittedCmd -> SurveyResponseSubmitResult
    




                             

    

