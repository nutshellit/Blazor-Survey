using Microsoft.AspNetCore.Blazor.Components;
using Survey.Client.ComponentCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Survey.Shared.SurveyResponseVM;

namespace Survey.Client.Pages
{
    public class SurveyResponseOverviewPage : BaseResponseComponent
    {
        [Parameter]
        protected string SurveyResponseId { get; private set; } = "";
        Guid _surveyResponseId = Guid.Empty;
        public SurveyResponseDTO SurveyResponse { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var responseId = Guid.Parse(SurveyResponseId);
            SurveyResponse = await SurveyResponseClient.GetResponse(responseId);
        }

    }
}
