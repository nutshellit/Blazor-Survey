namespace Survey.Api.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Survey.Shared.SurveyVM




[<Route("api/[controller]")>]
[<ApiController>]
type ValuesController () =
    inherit ControllerBase()

    let summaries =
        [|
            "Freezing"; "Bracing"; "Chilly"; "Cool"; "Mild"; "Warm"; "Balmy"; "Hot"; "Sweltering"; "Scorching"
        |]



    [<HttpGet>]
    member this.Get() =
        //let values = [|"value1"; "value2"|]
        //ActionResult<string[]>(values)
        let rng = Random()
        seq {
            for index in 1..5 ->
                WeatherForecast(
                    Date = DateTime.Now.AddDays(float index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = summaries.[rng.Next(summaries.Length)]
                )
        }

    [<HttpGet("{id}")>]
    member this.Get(id:int) =
        let value = "value"
        ActionResult<string>(value)

    [<HttpPost>]
    member this.Post([<FromBody>] value:string) =
        ()

    [<HttpPut("{id}")>]
    member this.Put(id:int, [<FromBody>] value:string ) =
        ()

    [<HttpDelete("{id}")>]
    member this.Delete(id:int) =
        ()
