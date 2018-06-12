using Survey.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Survey.Shared.SurveyResponseVM;
using static Survey.Shared.SurveyResponseCommands;
using Microsoft.AspNetCore.Blazor;

namespace Survey.Client.ComponentCode
{
    public class SurveyResponseClient : ISurveyResponseClient
    {
        readonly string _baseUrl = "/api/surveyresponse/";
        HttpClient _client;
        public SurveyResponseClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<SurveyResponseDTO> GetResponse(Guid responseId)
        {
            string url = $"{_baseUrl}get/{responseId}";
            return await _client.GetJsonAsync<SurveyResponseDTO>(url);
        }

        public async Task<SurveyResponseDTO[]> GetSurveyResponses(Guid surveyId)
        {
            string url = $"{_baseUrl}getbysurvey/{surveyId}";
            return await _client.GetJsonAsync<SurveyResponseDTO[]>(url);
        }

        async Task<CommandSubmitResult<SurveyResponseDTO>> PostCmd<T>(T cmd, string url)
        {
            var result = await _client.PostJsonAsync<CommandSubmitResult<SurveyResponseDTO>>(url, cmd);
            if (result.Result == CommandSubmitExecutionResult.OK)
            {
                //_cachedSurvey = result.EntityQry;
            }
            return result;
        }
        public async Task<CommandSubmitResult<SurveyResponseDTO>> AddResponse(SurveyResponseCreateCmd cmd)
        {
            string url = $"{_baseUrl}addsurveyresponse";

            return await PostCmd(cmd, url);

        }

        public async Task<CommandSubmitResult<SurveyResponseDTO>> CancelResponse(SurveyResponseSetCancelledCmd cmd)
        {
            string url = $"{_baseUrl}cancelresponse";
            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyResponseDTO>> CompleteResponse(SurveyResponseSetSubmittedCmd cmd)
        {
            string url = $"{_baseUrl}submitresponse";
            return await PostCmd(cmd, url);
        }



        public async Task<CommandSubmitResult<SurveyResponseDTO>> MultiOptionResponse(SurveyResponseMultiOptionResponseCmd cmd)
        {
            string url = $"{_baseUrl}addmultioptionresponse";

            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyResponseDTO>> RatingResponse(SurveyResponseRatingResponseCmd cmd)
        {
            string url = $"{_baseUrl}addratingresponse";

            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyResponseDTO>> SingleOptionResponse(SurveyResponseSingleOptionResponseCmd cmd)
        {
            string url = $"{_baseUrl}addsingleoptionresponse";

            return await PostCmd(cmd, url);
        }

        public async Task<CommandSubmitResult<SurveyResponseDTO>> TextResponse(SurveyResponseTextResponseCmd cmd)
        {
            string url = $"{_baseUrl}addtextresponse";

            return await PostCmd(cmd, url);
        }
    }
}
