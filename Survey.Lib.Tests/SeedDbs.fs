module SeedDbs

open Expecto 
open Survey.Lib
open Chessie.ErrorHandling
open System
open Survey.Shared

let private getSurveyService() = 
    let dbLoc = DbConfig.dbConnLocation()
    let surveyRepo = RepositoryBuilder.getRepositories dbLoc
    SurveyDefinitionServices.SurveyDefinitionEditServiceImpl.getService surveyRepo

let private getSurveyResponseService() = 
    let dbLoc = DbConfig.dbConnLocation()
    let surveyRepo = RepositoryBuilder.getRepositories dbLoc
    SurveyResponseServices.SurveyResponseEditServiceImpl.getService surveyRepo

let private mapResult (result : Async<Result<CmdExecutionResult<_>,string>>) =
    result |> Async.RunSynchronously 
    |> function
    | Ok (r,_) -> r |> Some
    | Bad m ->  Console.WriteLine ( m |> Seq.head)
                None

let private createSurvey (service : SurveyDefinitionEditService) name intro =
        let cmd = {SurveyDefinitionCreateCmd.Name=name; Intro = intro }
        service.create cmd |> mapResult

let private addSection (service : SurveyDefinitionEditService) surveyId name intro = 
        let cmd = {SurveyDefinitionAddSectionCmd.SurveyDefinitionId=surveyId
                   Name = name
                   SectionIntro = intro }
        getSurveyService().addSection cmd |> mapResult

let private addTxtQuestion (service : SurveyDefinitionEditService) surveyId sectionId text help = 
    let cmd = { SurveyDefinitionTextQuestionAddToSectionCmd.SurveyDefinitionId = surveyId
                SectionId = sectionId
                Text = text
                Help = help }
    getSurveyService().addTextQuestion cmd |> mapResult
    
let private addRatingQuestion (service : SurveyDefinitionEditService) surveyId sectionId text help min max = 
    let cmd = { SurveyDefinitionAddRatingQuestionToSectionCmd.Text = text 
                SurveyDefinitionId = surveyId
                SectionId = sectionId
                Help = help
                MinValue = min
                MaxValue = max }
    getSurveyService().addRatingQuestion cmd |> mapResult

let private addSingleOptionQuestion (service : SurveyDefinitionEditService) surveyId sectionId text help = 
    let cmd = { SurveyDefinitionSingleOptionQuestionAddToSectionCmd.SurveyDefinitionId = surveyId
                SectionId = sectionId
                Text = text 
                Help = help }
    getSurveyService().addSingleOptionQuestion cmd |> mapResult

let private addOptionToSingleOptionQuestion (service : SurveyDefinitionEditService) surveyId questionId text value = 
    let cmd = { AddOptionToSingleOptionQuestionCmd.SurveyDefinitionId = surveyId
                QuestionId = questionId
                Text = text
                Value = value  }
    getSurveyService().addOptionToSingleOptionQuestion cmd |> mapResult

let private addMultiOptionQuestion (service : SurveyDefinitionEditService) surveyId sectionId text help = 
    let cmd = { SurveyDefinitionMultiOptionQuestionAddToSectionCmd.SurveyDefinitionId = surveyId
                SectionId = sectionId
                Text = text 
                Help = help }
    getSurveyService().addMultiOptionQuestion cmd |> mapResult

let private addOptionToMultiOptionQuestion (service : SurveyDefinitionEditService) surveyId questionId text value = 
    let cmd = { AddOptionToMultiOptionQuestionCmd.SurveyDefinitionId = surveyId
                QuestionId = questionId
                Text = text
                Value = value  }
    getSurveyService().addOptionToMultiOptionQuestion cmd |> mapResult

let private addSampleSurvey name = 
    let service = getSurveyService()
    //add a survey
    Console.WriteLine "Adding Survey"
    let result = createSurvey service name "This is a sample customer satisfaction template"
    let survey = result.Value.Result
    Console.WriteLine "Adding section"
    let result = addSection service survey.Id "Part 1" "Start here please"
    let sectionId = result.Value.ItemIdUpdated
    Console.WriteLine "Adding quesion 1"
    let r_ = addRatingQuestion service survey.Id sectionId "1. Please rate this company" "" 0 10
    Console.WriteLine "Adding quesion 2"
    let so1 = addSingleOptionQuestion service survey.Id sectionId "2. Overall, how satisfied or dissatisfied are you with our company" ""
    let so1id = so1.Value.ItemIdUpdated
    let _ = addOptionToSingleOptionQuestion service survey.Id so1id "Very Satisfied" "1"
    let _ = addOptionToSingleOptionQuestion service survey.Id so1id "Somewhat Satisfied" "2"
    let _ = addOptionToSingleOptionQuestion service survey.Id so1id "Neither satisfied nor dissatisfied" "3"
    let _ = addOptionToSingleOptionQuestion service survey.Id so1id "Somewhat dissatisfied" "4"
    let _ = addOptionToSingleOptionQuestion service survey.Id so1id "Very dissatisfied" "5"
    Console.WriteLine "Adding quesion 3"
    let mo1 = addMultiOptionQuestion service survey.Id sectionId "3. Which of the following words would you use to describe our products? Select all that apply" ""
    let mo1id = mo1.Value.ItemIdUpdated
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "Reliable" "1"
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "High Quality" "2"
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "Useful" "3"
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "Unique" "4"
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "Good value for money" "5"
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "Overpriced" "6"
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "Impractical" "7"
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "Ineffective" "8"
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "Poor quality" "9"
    let _ = addOptionToMultiOptionQuestion service survey.Id mo1id "Unreliable" "10"
    Console.WriteLine "Adding second section"
    let result2 = addSection service survey.Id "Part 2" "Some more questions...."
    let sectionId2 = result2.Value.ItemIdUpdated
    let so2 = addSingleOptionQuestion service survey.Id sectionId2 "4. How well do our products meet your needs" ""
    let so2id = so2.Value.ItemIdUpdated
    let _ = addOptionToSingleOptionQuestion service survey.Id so2id "Extremely well" "1"
    let _ = addOptionToSingleOptionQuestion service survey.Id so2id "Very well" "2"
    let _ = addOptionToSingleOptionQuestion service survey.Id so2id "Somewhat well" "3"
    let _ = addOptionToSingleOptionQuestion service survey.Id so2id "Not so well" "4"
    let _ = addOptionToSingleOptionQuestion service survey.Id so2id "Not at all well" "5"
    let t1 = addTxtQuestion service survey.Id sectionId2 "5. Do you have any other comments, questions, or concerns?" ""
    t1.Value.Result

let private addResponseToSurvey(surveyId : Guid) = 
    let cmd = { SurveyResponseCreateCmd.SurveyDefinitionId = surveyId } 
    getSurveyResponseService().create cmd |> mapResult


let seedDb() = 
    let survey1 = addSampleSurvey "Customer Satisfaction Survey #1"
    let survey2 = addSampleSurvey "Customer Satisfaction Survey #2"
    Console.WriteLine "Make live #1"
    let cmd = { SurveyDefinitionMakeLiveCmd.SurveyDefinitionId = survey1.Id }
    let service = getSurveyService()
    let _ = service.makeLive cmd |> mapResult
    let _ = addResponseToSurvey survey1.Id
    let _ = addResponseToSurvey survey1.Id
    let _ = addResponseToSurvey survey1.Id
    let _ = addResponseToSurvey survey1.Id

    ()


    



