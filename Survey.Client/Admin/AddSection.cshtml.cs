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
    public class AddSectionPage : BaseComponent
    {
        [Parameter]
        protected string Id { get; private set; } = "";

        Guid _surveyId = Guid.Empty;
        public string SectionName;
        public string SectionIntro;
        public string ValidationMessage = "";
        protected SurveyEditDTO survey = null;

        protected override void OnParametersSet()
        {
            if (Id != null)
            {
                _surveyId = Guid.Parse(Id);
                survey = SurveyClient.CachedSurvey;
            }
        }

       

        public async Task OnSaveClick()
        {
            var cmd = new SurveyDefinitionAddSectionCmd(_surveyId, SectionName, SectionIntro);
            var result = await SurveyClient.AddSection(cmd);
            if (result.Result == CommandSubmitExecutionResult.OK)
                UriHelper.NavigateTo($"editsurvey/");
            else {
                ValidationMessage = result.Messages.FirstOrDefault();
                Console.WriteLine(ValidationMessage);
            }
        }
    }
}
