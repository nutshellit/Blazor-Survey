namespace Survey.Lib
open System
open Survey.Shared
open Chessie.ErrorHandling

type SurveyDefinitionRepository = 
        { get : Guid -> SurveyDefinition option Async
          getAll : unit -> SurveyDefinition list Async
          addUpdate : SurveyDefinition -> Result<SurveyDefinition,string> Async } 

type SurveyResponseRepository = 
        { 
            get : Guid -> SurveyResponse option Async
            getBySurveyDefinition : Guid -> SurveyResponse list Async
            addUpdate : SurveyResponse -> Result<SurveyResponse,string> Async
        }

type Repositories = 
    { surveyRepository : SurveyDefinitionRepository
      surveyResponseRespository : SurveyResponseRepository}

