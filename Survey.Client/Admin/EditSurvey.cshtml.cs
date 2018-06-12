using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Survey.Client.ComponentCode;
using Survey.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Survey.Shared.SurveyDefinitionEditCommands;
using static Survey.Shared.SurveyVM;

namespace Survey.Client.Admin
{
    public class EditSurveyPage : BaseComponent
    {
        [Parameter]
        protected string Id { get; private set; } = "";

        Guid _id = Guid.Empty;
        public bool CanEdit { get; set; }
        protected SurveyEditDTO survey = null;
        protected override async Task OnParametersSetAsync()
        {
            
            Console.WriteLine("Edit Survey Page : OnParametersSetAsync #1 ");
            if (Id != "" )
            {
                Console.WriteLine("Get from server, no cache");
                _id = Guid.Parse(Id);
                survey = await SurveyClient.GetSurvey(_id);
            }
            else
            {
                Console.WriteLine("Use cache");
                survey = SurveyClient.CachedSurvey;
                
            }
            CanEdit = (survey.Status == SurveyDefinitionStatusEnum.Edit);
        }

        public async Task DeleteQuestion(SurveyQuestionEditDTO question)
        {
            var cmd = new SurveyDefinitionRemoveSurveyItemCmd(survey.SurveyId, question.QuestionId);
            var result = await SurveyClient.DeleteSurveyItem(cmd);
            string m = result.Messages.FirstOrDefault();
            Console.WriteLine($"Delete Question : {result.Result} - {m} "  );
            if (result.Result == CommandSubmitExecutionResult.OK)
            {
                survey = result.EntityQry;
                StateHasChanged();
            }
            
        }

        public async Task MoveItemUp(Guid surveyItemId)
        {
            var cmd = new SurveyDefinitionMoveSurveyItemUpCmd(survey.SurveyId, surveyItemId);
            var result = await SurveyClient.MoveItemUp(cmd);
            string m = result.Messages.FirstOrDefault();
            Console.WriteLine($"Move up.... : {result.Result} - {m} ");
            if (result.Result == CommandSubmitExecutionResult.OK)
            {
                survey = result.EntityQry;
                StateHasChanged();
            }
        }
        public async Task MoveItemDown(Guid surveyItemId)
        {
            var cmd = new SurveyDefinitionMoveSurveyItemDownCmd(survey.SurveyId, surveyItemId);
            var result = await SurveyClient.MoveItemDown(cmd);
            string m = result.Messages.FirstOrDefault();
            Console.WriteLine($"Move down.... : {result.Result} - {m} ");
            if (result.Result == CommandSubmitExecutionResult.OK)
            {
                survey = result.EntityQry;
                StateHasChanged();
            }
        }

        public async Task MakeLive()
        {
            var cmd = new SurveyDefinitionMakeLiveCmd(survey.SurveyId);
            var result = await SurveyClient.MakeLive(cmd);
            if (result.Result == CommandSubmitExecutionResult.OK)
            {
                survey = result.EntityQry;
                CanEdit = false;
                StateHasChanged();
            }
        }


    }
}
