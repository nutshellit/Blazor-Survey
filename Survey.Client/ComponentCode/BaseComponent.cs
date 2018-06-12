using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Services;
using Survey.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Survey.Shared.SurveyVM;

namespace Survey.Client.ComponentCode
{
    public abstract class BaseComponent : BlazorComponent {
        [Inject]
        protected HttpClient Http { get; set; }

        [Inject]
        protected IUriHelper UriHelper { get; set; }

        [Inject]
        protected ISurveyEditClient SurveyClient { get; set; }
    }


    public abstract class BaseResponseComponent : BlazorComponent {
        [Inject]
        protected HttpClient Http { get; set; }

        [Inject]
        protected IUriHelper UriHelper { get; set; }

        [Inject]
        protected ISurveyResponseClient SurveyResponseClient { get; set; }

        [Inject]
        protected ISurveyEditClient SurveyClient { get; set; }
    }

    

    
}
