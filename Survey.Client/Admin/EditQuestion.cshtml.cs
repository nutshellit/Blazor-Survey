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
    public class EditQuestionPage : BaseComponent
    {
        [Parameter]
        protected string SectionId { get; private set; } = "";

        Guid _sectionId = Guid.Empty;
        public string SectionName;
        public string SectionIntro;
        public string ValidationMessage = "";
        protected SurveyEditDTO survey = null;
        protected SurveySectionEditDTO section = null;

        //question bindings
        public List<QuestionTypeEnum> QuestionTypes = new List<QuestionTypeEnum>();
        public string QuestionText;
        public string QuestionHelp;
        public int QuestionTypeId;
        public int RatingMin;
        public int RatingMax;
        public List<OptionBinding> Options = new List<OptionBinding>();
        public string OptionTxt = "";
        public string OptionVal = "";


        protected override void OnParametersSet()
        {
            Console.WriteLine("OnParametersSet #1");
            if (SectionId != null)
            {
                _sectionId = Guid.Parse(SectionId);
                survey = SurveyClient.CachedSurvey;
                section = survey.Sections.FirstOrDefault(n => n.SectionId == _sectionId);
            }
        }
        protected override Task OnInitAsync()
        {
            Console.WriteLine("OnInitAsync");
            QuestionTypeId = (int)QuestionTypeEnum.Text;
            foreach (var qt in Enum.GetValues(typeof(QuestionTypeEnum)))
            {
                QuestionTypes.Add((QuestionTypeEnum)qt);
            }
            return base.OnInitAsync();
        }


        public void AddOption()
        {
            if (String.IsNullOrWhiteSpace(OptionTxt)) {
                return;
            }
            Options.Add(new OptionBinding {Text = OptionTxt, Value=OptionVal });
            OptionTxt = OptionVal = "";
        }

        public async Task OnSaveClick()
        {
            await AddQuestion();
        }

        async Task AddQuestion()
        {
            CommandSubmitResult<SurveyEditDTO> result = null;
            switch ((QuestionTypeEnum)QuestionTypeId) {
                case QuestionTypeEnum.Text:
                    var cmd = new SurveyDefinitionTextQuestionAddToSectionCmd(survey.SurveyId, section.SectionId, QuestionText, QuestionHelp);
                    result = await SurveyClient.AddTextQuestion(cmd);
                    break;
                case QuestionTypeEnum.Rating:
                    //int min = int.Parse(RatingMin);
                    var cmd1 = new SurveyDefinitionAddRatingQuestionToSectionCmd(
                        survey.SurveyId, 
                        section.SectionId, 
                        RatingMin, 
                        RatingMax, 
                        QuestionText, 
                        QuestionHelp);
                    result = await SurveyClient.AddRatingQuestion(cmd1);
                    break;
                case QuestionTypeEnum.MultiChoice:
                    var cmd2 = new SurveyDefinitionMultiOptionQuestionAddToSectionCmd(survey.SurveyId, section.SectionId, QuestionText, QuestionHelp);
                    result = await SurveyClient.AddMultiOptionQuestion(cmd2);
                    if (result.Result == CommandSubmitExecutionResult.OK)
                    {
                        Guid addedQId = result.UpdatedId;
                        foreach (var item in Options)
                        {
                            var cmd3 = new AddOptionToMultiOptionQuestionCmd(survey.SurveyId, addedQId, item.Text, item.Value);
                            result = await SurveyClient.AddOptionToMultiOptionQuestion(cmd3);
                        }
                    }
                    break;
                case QuestionTypeEnum.SingleChoice:
                    var cmd4 = new SurveyDefinitionSingleOptionQuestionAddToSectionCmd(survey.SurveyId, section.SectionId, QuestionText, QuestionHelp);
                    result = await SurveyClient.AddSingleOptionQuestion(cmd4);
                    if (result.Result == CommandSubmitExecutionResult.OK)
                    {
                        Guid addedQId = result.UpdatedId;
                        foreach (var item in Options)
                        {
                            var cmd5 = new AddOptionToSingleOptionQuestionCmd(survey.SurveyId, addedQId, item.Text, item.Value);
                            result = await SurveyClient.AddOptionToSingleOptionQuestionCmd(cmd5);
                        }
                    }
                    break;
            }
            if (result.Result == CommandSubmitExecutionResult.OK)
                UriHelper.NavigateTo($"editsurvey/");
            else
            {
                ValidationMessage = result.Messages.FirstOrDefault();
                Console.WriteLine(ValidationMessage);
            }
        }

        public class OptionBinding
        {
            public Guid OptionId { get; set; }
            public string Text { get; set; }
            public string Value { get; set; }
        }

    }
}
