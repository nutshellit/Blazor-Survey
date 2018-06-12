using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.FSharp.Core;
using Survey.Client.ComponentCode;
using Survey.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Survey.Shared.SurveyResponseCommands;
using static Survey.Shared.SurveyResponseVM;
using Microsoft.AspNetCore.Blazor;

namespace Survey.Client.Pages
{
    public class EditSurveyResponsePage : BaseResponseComponent
    {
        [Parameter]
        protected string SurveyId { get; private set; } = "";
        [Parameter]
        protected string SurveyResponseId { get; private set; } = "";
        [Parameter]
        protected string QuestionId { get; private set; } = "";


        Guid _surveyResponseId = Guid.Empty;
        public string SurveyName { get; set; }
        public string SurveyIntro { get; set; }
        public SurveyResponseDTO SurveyResponse { get; set; }
        public SurveyQuestionResponseDTO Response { get; set; }

        //bindings
        public string TxtAnswer { get; set; }
        public int RatingAnswer { get; set; }
        public Guid SingleOptionAnswer { get; set; } = Guid.Empty;
        public List<CheckBoxBinding> MultiOptionAnswers = new List<CheckBoxBinding>();
        public string ValidationMessage = "";


        protected override async Task OnParametersSetAsync()
        {
            if (IsAdd())
            {
                var surveyId = Guid.Parse(SurveyId);
                var ok = await LoadForAddResponse(surveyId);

            }
            else if (IsEdit())
            {
                var responseId = Guid.Parse(SurveyResponseId);
                var ok = await LoadForEditResponse(responseId);
            }
            else {
                Console.WriteLine("#Question ....");
                var responseId = Guid.Parse(SurveyResponseId);
                var questionId = Guid.Parse(QuestionId);
                var ok = await LoadQuestion(responseId, questionId);
            }

        }

        async Task<bool> LoadForAddResponse(Guid surveyId)
        {
            var cmd = new SurveyResponseCreateCmd(surveyId);
            var response = await SurveyResponseClient.AddResponse(cmd);
            if (response.Result == CommandSubmitExecutionResult.OK)
            {
                SurveyResponse = response.EntityQry;
                Load();
            }
            Console.WriteLine($" Result - {response.Result}");
            return true;
        }

        async Task<bool> LoadForEditResponse(Guid responseId)
        {
            SurveyResponse = await SurveyResponseClient.GetResponse(responseId);
            CheckIsNotAlreadySubmitted();
            Load();
            return true;
        }

        void CheckIsNotAlreadySubmitted()
        {
            if (SurveyResponse.Status == SurveyResponseStatusEnum.Submitted)
            {
                UriHelper.NavigateTo($"/surveyresponseoverview/{SurveyResponse.SurveyResponseId}");
            }
        }

        async Task<bool> LoadQuestion(Guid responseId, Guid questionId)
        {
            SurveyResponse = await SurveyResponseClient.GetResponse(responseId);
            CheckIsNotAlreadySubmitted();
            Load();
            Response = SurveyResponse.ByQuestion(questionId);
            DoBindings();
            return true;
        }

        void Load()
        {
            SurveyName = SurveyResponse.SurveyDefinition.Name;
            SurveyIntro = SurveyResponse.SurveyDefinition.Intro;
            Response = SurveyResponse.NextUnansweredQuestion();
            DoBindings();
        }
        void DoBindings()
        {
            QuestionResponseDTO existingResponse = null;
            if (Response.QuestionResponse.IsSome<QuestionResponseDTO>())
                existingResponse = Response.QuestionResponse.Value;
            switch (Response.Question.QuestionType)
            {
                case QuestionTypeEnum.Text:
                    TxtAnswer = Response.IsAnswered() ? existingResponse.TextResponse : "";
                    break;
                case QuestionTypeEnum.Rating:
                    RatingAnswer = Response.IsAnswered() ? existingResponse.RatingResponse : Response.Question.RatingMin;
                    break;
                case QuestionTypeEnum.SingleChoice:
                    SingleOptionAnswer = Response.IsAnswered() ? existingResponse.SingleOptionResponseOptionId : Guid.Empty;
                    break;
                case QuestionTypeEnum.MultiChoice:
                    MultiOptionAnswers = Response.Question.Options
                                            .Select(n => new CheckBoxBinding
                                            {
                                                ChoiceId = n.OptionId,
                                                ChoiceTxt = n.Text,
                                                Selected = Response.IsAnswered() 
                                                                ? existingResponse.MultiOptionResponseOptionIds.Contains(n.OptionId)
                                                                : false
                                            })
                                            .ToList();
                    break;
            }
        }

        bool IsAdd()
        {
            return this.UriHelper.GetAbsoluteUri().Contains("addsurveyresponse");
        }
        bool IsEdit()
        {
            return this.UriHelper.GetAbsoluteUri().Contains("editsurveyresponse");

        }

        bool CanEdit()
        {
            return SurveyResponse.Status == SurveyResponseStatusEnum.Started || SurveyResponse.Status == SurveyResponseStatusEnum.Completed;
        }
        public async Task OnSaveClick()
        {
            ValidationMessage = "";
            (bool, SurveyResponseDTO) attempt = (false, null);
            switch (Response.Question.QuestionType)
            {
                case QuestionTypeEnum.Text:
                    attempt = await SaveTextResponse();
                    break;
                case QuestionTypeEnum.Rating:
                    attempt = await SaveRatingResponse();
                    break;
                case QuestionTypeEnum.SingleChoice:
                    attempt = await SaveSingleChoiceResponse();
                    break;
                case QuestionTypeEnum.MultiChoice:
                    attempt = await SaveMultiChoiceResponse();
                    break;

            }
            if (attempt.Item1)
            {
                SurveyResponse = attempt.Item2;
                if (SurveyResponse.Status == SurveyResponseStatusEnum.Completed)
                {
                    UriHelper.NavigateTo($"/surveyresponseoverview/{SurveyResponse.SurveyResponseId}");
                }
                else
                {
                    Response = SurveyResponse.NextUnansweredQuestion();
                    DoBindings();
                }
            }
        }

        public void PreviousQuestion()
        {
            var r = SurveyResponse.PreviousScreen(Response.Question.QuestionId);
            if (r.IsSome<SurveyQuestionResponseDTO>())
            {
                Response = r.Value;
            }
            DoBindings();
        }

        async Task<(bool, SurveyResponseDTO)> SaveTextResponse()
        {
            if (string.IsNullOrWhiteSpace(TxtAnswer))
            {
                ValidationMessage = "Please answer text question";
                return (false, null);
            }
            var cmd = new SurveyResponseTextResponseCmd(SurveyResponse.SurveyResponseId, Response.Question.QuestionId, TxtAnswer);
            var response = await SurveyResponseClient.TextResponse(cmd);
            if (response.Result == CommandSubmitExecutionResult.Fail)
            {
                ValidationMessage = response.Messages.FirstOrDefault();
                return (false, null);
            }
            return (true, response.EntityQry);
        }

        async Task<(bool, SurveyResponseDTO)> SaveRatingResponse()
        {
            //validation necessary - will always be in range of values?
            var cmd = new SurveyResponseRatingResponseCmd(SurveyResponse.SurveyResponseId,
                Response.Question.QuestionId,
                RatingAnswer);
            var response = await SurveyResponseClient.RatingResponse(cmd);
            if (response.Result == CommandSubmitExecutionResult.Fail)
            {
                ValidationMessage = response.Messages.FirstOrDefault();
                return (false, null);
            }
            return (true, response.EntityQry);
        }

        async Task<(bool, SurveyResponseDTO)> SaveSingleChoiceResponse()
        {
            if (SingleOptionAnswer == Guid.Empty)
            {
                ValidationMessage = "Please select a response";
                return (false, null);
            }
            var cmd = new SurveyResponseSingleOptionResponseCmd(SurveyResponse.SurveyResponseId,
                Response.Question.QuestionId,
                SingleOptionAnswer);
            var response = await SurveyResponseClient.SingleOptionResponse(cmd);
            if (response.Result == CommandSubmitExecutionResult.Fail)
            {
                ValidationMessage = response.Messages.FirstOrDefault();
                return (false, null);
            }
            return (true, response.EntityQry);
        }

        async Task<(bool,SurveyResponseDTO)> SaveMultiChoiceResponse()
        {
            if (!MultiOptionAnswers.Any(n => n.Selected))
            {
                ValidationMessage = "Please select at least one response";
                return (false, null);
            }
            var cmd = new SurveyResponseMultiOptionResponseCmd(SurveyResponse.SurveyResponseId,
                Response.Question.QuestionId,
                MultiOptionAnswers.Where(n => n.Selected).Select(n => n.ChoiceId).ToArray());
            var response = await SurveyResponseClient.MultiOptionResponse(cmd);
            if (response.Result == CommandSubmitExecutionResult.Fail)
            {
                ValidationMessage = response.Messages.FirstOrDefault();
                return (false, null);
            }
            return (true, response.EntityQry);
        }

        protected void OnRadioChange(UIChangeEventArgs args)
        {
            SingleOptionAnswer = Guid.Parse(args.Value.ToString());
        }

        protected async Task OnSubmit()
        {
            var cmd = new SurveyResponseSetSubmittedCmd(SurveyResponse.SurveyResponseId);
            var response = await SurveyResponseClient.CompleteResponse(cmd);
            UriHelper.NavigateTo($"/surveyresponseoverview/{SurveyResponse.SurveyResponseId}");
        }
        protected async Task OnCancel()
        {
            var cmd = new SurveyResponseSetCancelledCmd(SurveyResponse.SurveyResponseId);
            var response = await SurveyResponseClient.CancelResponse(cmd);
            UriHelper.NavigateTo($"/surveyresponseoverview/{SurveyResponse.SurveyResponseId}");
        }

        public class CheckBoxBinding
        {
            public Guid ChoiceId { get; set; }
            public string ChoiceTxt { get; set; }
            public bool Selected
            {
                get; set;
            }
        }

    }
    
}
