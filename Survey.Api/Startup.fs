namespace Survey.Api

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy;
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System.Net.Mime;
open Newtonsoft.Json.Serialization
open Microsoft.AspNetCore.Blazor.Server
open Microsoft.AspNetCore.ResponseCompression

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                         .AddJsonOptions(fun o -> o.SerializerSettings.ContractResolver <- new DefaultContractResolver() 
                                                  )|> ignore
        services.AddResponseCompression(fun o -> let mimeTypes = [|MediaTypeNames.Application.Octet; WasmMediaTypeNames.Application.Wasm|]
                                                 o.MimeTypes <- ResponseCompressionDefaults.MimeTypes.Concat(mimeTypes) 
                                                 () ) |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseHsts() |> ignore

        app.UseHttpsRedirection() |> ignore
        app.UseMvc(fun routes -> routes.MapRoute(name = "default", template = "{controller}/{action}/{id?}") |> ignore
         ) |> ignore
        app.UseBlazor<Survey.Client.Program>()


    member val Configuration : IConfiguration = null with get, set