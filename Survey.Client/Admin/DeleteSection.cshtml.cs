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
    public class DeleteSectionPage : BaseComponent
    {
        [Parameter]
        protected string Id { get; private set; } = "";
        Guid _sectionId = Guid.Empty;
        protected SurveyEditDTO survey = null;
        protected SurveySectionEditDTO section = null;
        public string ValidationMessage = "";

        protected override void OnParametersSet()
        {
            if (Id != null)
            {
                _sectionId = Guid.Parse(Id);
                survey = SurveyClient.CachedSurvey;
                section = survey.Sections.FirstOrDefault(n => n.SectionId == _sectionId);
            }
        }

        public async Task OnDeleteClick()
        {
            var cmd = new SurveyDefinitionRemoveSurveyItemCmd(survey.SurveyId, section.SectionId);
            var result = await SurveyClient.DeleteSurveyItem(cmd);
            if (result.Result == CommandSubmitExecutionResult.OK)
                UriHelper.NavigateTo($"editsurvey/");
            else
            {
                ValidationMessage = result.Messages.FirstOrDefault();
            }
        }

    }
}
