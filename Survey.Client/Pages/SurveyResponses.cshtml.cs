using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.FSharp.Core;
using Survey.Client.ComponentCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Survey.Shared.SurveyResponseVM;

namespace Survey.Client.Pages
{
    public class SurveyResponsesPage : BaseResponseComponent
    {
        [Parameter]
        protected string Id { get; private set; } = "";

        Guid _surveyId = Guid.Empty;
        public string SurveyName { get; set; }
        public string SurveyIntro { get; set; }
        public List<SurveyResponseDTO> Responses { get; set; } = new List<SurveyResponseDTO>();

        protected override async Task OnParametersSetAsync()
        {
            if (Id != null) {
                _surveyId = Guid.Parse(Id);
                var survey = await SurveyClient.GetSurvey(_surveyId);
                SurveyName = survey.Name;
                SurveyIntro = survey.Intro;
                var responses = await SurveyResponseClient.GetSurveyResponses(_surveyId);
                Responses = responses.ToList();
            }
        }
    }
}
