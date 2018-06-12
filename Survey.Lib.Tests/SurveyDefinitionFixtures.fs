module SurveyDefinitionFixtures 

open Expecto 
open Survey.Lib
open Chessie.ErrorHandling
open System
open Survey.Shared

let getSurveyService() = 
    let dbLoc = DbConfig.dbConnLocation()
    let surveyRepo = RepositoryBuilder.getRepositories dbLoc
    SurveyDefinitionServices.SurveyDefinitionEditServiceImpl.getService surveyRepo

let mapResult (result : Async<Result<CmdExecutionResult<_>,string>>) =
    result |> Async.RunSynchronously 
    |> function
    | Ok (r,_) -> r |> Some
    | Bad m -> None

let createSurvey (name :string ) =
        let cmd = {SurveyDefinitionCreateCmd.Name=name; Intro = "my intro" }
        let service = getSurveyService()
        service.create cmd |> mapResult

let addSection (surveyId : Guid ) = 
        let cmd = {SurveyDefinitionAddSectionCmd.SurveyDefinitionId=surveyId
                   Name = "my section name"
                   SectionIntro = "my section intro"
                   }
        let service = getSurveyService()    
        service.addSection cmd |> mapResult
let canCreate = 
    testCase "Can create a survey definition" <| fun _ ->
        let survey = createSurvey "create123"
        "Should return valid result" |> Expect.isSome survey
        let s = survey.Value
        "Name" |> Expect.equal s.Result.Name "create123"
        "Intro" |> Expect.equal s.Result.Intro "my intro"

let canAddSection = 
    testCase "Can add a survey section" <| fun _ -> 
        let survey = createSurvey "add section"
        let s = survey.Value
        let survey1 = addSection s.Result.Id
        "Should return valid result" |> Expect.isSome survey1
        "Should have a section" |> Expect.isTrue (survey1.Value.Result.Sections() |> Seq.length = 1 )                  

let canAddTextQuestion = 
    testCase "can add text question" <| fun _ -> 
        let survey = createSurvey "add text question"
        let s = survey.Value
        let survey1 = addSection s.Result.Id
        let s = survey1.Value
        let section = s.Result.Items.Head
        let cmd = {SurveyDefinitionTextQuestionAddToSectionCmd.SurveyDefinitionId = s.Result.Id
                   SectionId = section.Id()
                   Text = "my question"
                   Help = "question help" }
        let service = getSurveyService()                   
        let survey2 = service.addTextQuestion cmd |> mapResult
        let s2 = survey2.Value
        let q = s2.Result.Questions() |> Seq.head 
        "Should have parent section id" |> Expect.equal q.ParentSectionId (section.Id())
        let txt = q.Question |> function | SimpleText st -> st | _ -> failwith "should be txt question"
        "Text is invalid" |> Expect.equal txt.Common.Text cmd.Text
        "Help is invalid" |> Expect.equal txt.Common.Help cmd.Help

let canAddSingleOptionQuestion = 
    testCase "can add single option question" <| fun _ -> 
        let survey = createSurvey "add single opt question"
        let s = survey.Value
        let survey1 = addSection s.Result.Id
        let s = survey1.Value
        let section = s.Result.Items.Head
        let cmd = { SurveyDefinitionSingleOptionQuestionAddToSectionCmd.SurveyDefinitionId= s.Result.Id
                    SectionId = section.Id()
                    Text = "my single opt question"
                    Help = "question help" }
        let service = getSurveyService()                   
        let survey2 = service.addSingleOptionQuestion cmd |> mapResult
        let s2 = survey2.Value
        let q = s2.Result.Questions() |> Seq.head 
        "Should have parent section id" |> Expect.equal q.ParentSectionId (section.Id())
        let so = q.Question |> function | SingleChoice sc -> sc | _ -> failwith "should be single option question"
        "Text is invalid" |> Expect.equal so.Common.Text cmd.Text
        "Help is invalid" |> Expect.equal so.Common.Help cmd.Help   

let canAddOptionToSingeOptionQuestion = 
    testCase "can add option to single option question" <| fun _ -> 
        let survey = createSurvey "add option to single opt question"
        let s = survey.Value
        let survey1 = addSection s.Result.Id
        let s = survey1.Value
        let section = s.Result.Items.Head
        let cmd = { SurveyDefinitionSingleOptionQuestionAddToSectionCmd.SurveyDefinitionId= s.Result.Id
                    SectionId = section.Id()
                    Text = "my single opt question"
                    Help = "question help" }
        let service = getSurveyService()                   
        let survey2 = service.addSingleOptionQuestion cmd |> mapResult
        let s2 = survey2.Value
        let q = s2.Result.Questions() |> Seq.head 
        let cmd2 = { AddOptionToSingleOptionQuestionCmd.SurveyDefinitionId = s.Result.Id
                     QuestionId = q.SurveyItemId
                     Text = "my opt 1"
                     Value = "1"  }
        let survey3 = service.addOptionToSingleOptionQuestion cmd2 |> mapResult
        let s3 = survey3.Value
        let scq = s3.Result.Questions() |> Seq.head |> fun x -> x.Question |> function | SingleChoice sc -> sc | _ -> failwith "woooh"
        let opt = scq.Options |> Seq.head
        "Option text" |> Expect.equal opt.Text cmd2.Text
        "Option value" |> Expect.equal opt.Value cmd2.Value


                        

[<Tests>]
let tl1 = 
    testList "SurveyDefinition" [canCreate
                                 canAddSection
                                 canAddTextQuestion
                                 canAddSingleOptionQuestion
                                 canAddOptionToSingeOptionQuestion]        
