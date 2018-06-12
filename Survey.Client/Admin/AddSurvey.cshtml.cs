using Survey.Client.ComponentCode;
using Survey.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Survey.Shared.SurveyDefinitionEditCommands;

namespace Survey.Client.Admin
{
    public class AddSurveyPage : BaseComponent
    {
        public string SurveyName;
        public string SurveyIntro;
        public string ValidationMessage;

        public async Task OnSaveClick()
        {
            if (String.IsNullOrWhiteSpace(SurveyName))
            {
                ValidationMessage = "Survey Name is a required field";
                return;
            }

            var cmd = new SurveyDefinitionCreateCmd(SurveyName, SurveyIntro);
            var result = await SurveyClient.AddSurvey(cmd);
            if(result.Result == CommandSubmitExecutionResult.OK)
            {
                UriHelper.NavigateTo("adminsurveydefinitions");
            }
        }

    }
}
