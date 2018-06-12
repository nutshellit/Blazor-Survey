using Microsoft.AspNetCore.Blazor;
using Survey.Client.ComponentCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Survey.Shared.SurveyVM;

namespace Survey.Client.Admin
{
    public class AdminSurveyDefinitionsPage : BaseComponent
    {
        protected SurveyEditDTO[] surveys = null;

        protected override async Task OnInitAsync()
        {
            surveys = await SurveyClient.GetSurveys();
        }
    }
}
