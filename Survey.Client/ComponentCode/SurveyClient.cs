using Survey.Shared;
using System;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Survey.Shared.SurveyDefinitionEditCommands;
using static Survey.Shared.SurveyVM;
using System.Net.Http;

namespace Survey.Client.ComponentCode
{
    public class SurveyEditClient : ISurveyEditClient
    {
        readonly string _baseUrl = "/api/surveyedit/";

        SurveyEditDTO _cachedSurvey;

        HttpClient _client;

        public SurveyEditDTO CachedSurvey => _cachedSurvey;

        public SurveyEditClient(HttpClient client)
        {
            _client = client;
        }
        public async Task<SurveyEditDTO> GetSurvey(Guid surveyId)
        {
            string url = $"{_baseUrl}get/{surveyId}";
            _cachedSurvey =  await _client.GetJsonAsync<SurveyEditDTO>(url);
            return _cachedSurvey;
        }

        public async Task<SurveyEditDTO[]> GetSurveys()
        {
            _cachedSurvey = null;
            string url = $"{_baseUrl}getall";
            return await _client.GetJsonAsync<SurveyEditDTO[]>(url);
        }

        async Task<CommandSubmitResult<SurveyEditDTO>> PostCmd<T>(T cmd, string url)
        {
            var result = await _client.PostJsonAsync<CommandSubmitResult<SurveyEditDTO>>(url, cmd);
            if (result.Result == CommandSubmitExecutionResult.OK)
            {
                _cachedSurvey = result.EntityQry;
            }
            return result;
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> AddSurvey(SurveyDefinitionCreateCmd cmd)
        {
            string url = $"{_baseUrl}addsurvey";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> AddSection(SurveyDefinitionAddSectionCmd cmd)
        {
            string url = $"{_baseUrl}addsection";
            return await PostCmd(cmd, url);  
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> DeleteSurveyItem(SurveyDefinitionRemoveSurveyItemCmd cmd)
        {
            string url = $"{_baseUrl}deletesection";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> AddTextQuestion(SurveyDefinitionTextQuestionAddToSectionCmd cmd)
        {
            string url = $"{_baseUrl}addtextquestion";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> AddRatingQuestion(SurveyDefinitionAddRatingQuestionToSectionCmd cmd)
        {
            string url = $"{_baseUrl}addratingquestion";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> AddSingleOptionQuestion(SurveyDefinitionSingleOptionQuestionAddToSectionCmd cmd)
        {
            string url = $"{_baseUrl}addsingleoptionquestion";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> AddMultiOptionQuestion(SurveyDefinitionMultiOptionQuestionAddToSectionCmd cmd)
        {
            string url = $"{_baseUrl}addmultioptionquestion";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> AddOptionToMultiOptionQuestion(AddOptionToMultiOptionQuestionCmd cmd)
        {
            string url = $"{_baseUrl}addoptiontomultioptionquestion";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> AddOptionToSingleOptionQuestionCmd(AddOptionToSingleOptionQuestionCmd cmd)
        {
            string url = $"{_baseUrl}addoptiontosingleoptionquestion";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> MoveItemUp(SurveyDefinitionMoveSurveyItemUpCmd cmd)
        {
            string url = $"{_baseUrl}moveitemup";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> MoveItemDown(SurveyDefinitionMoveSurveyItemDownCmd cmd)
        {
            string url = $"{_baseUrl}moveitemdown";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyEditDTO>> MakeLive(SurveyDefinitionMakeLiveCmd cmd)
        {
            string url = $"{_baseUrl}makelive";
            return await PostCmd(cmd, url);
        }
    }
}
