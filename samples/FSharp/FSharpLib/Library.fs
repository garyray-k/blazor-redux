namespace FSharpLib

open System
open BlazorRedux

type WeatherForecast() =
    member val Date = DateTime.MinValue with get, set
    member val TemperatureC = 0 with get, set
    member val TemperatureF = 0 with get, set
    member val Summary = "" with get, set

type MyModel =
    {
        Count: int;
        Forecasts: WeatherForecast[] option;
    }

type MyMsg =
    | IncrementByOne
    | IncrementByValue of n : int
    | LoadWeather
    | ReceiveWeather of r : WeatherForecast[]

type MyAppComponent() =
    inherit ReduxComponent<MyModel, MyMsg>()

module MyFuncs =
    let MyReducer state action =
        match action with
            | IncrementByOne -> { state with Count = state.Count + 1 }
            | IncrementByValue n -> { state with Count = state.Count + n }
            | LoadWeather -> { state with Forecasts = None }
            | ReceiveWeather r -> { state with Forecasts = Some r }

    let InitStore = new Store<MyModel, MyMsg>(Reducer<MyModel, MyMsg>MyReducer, { Count = 0; Forecasts = None })

module ActionCreators =
    open System.Net.Http
    open System.Threading.Tasks
    open FSharp.Control.Tasks
    open Microsoft.AspNetCore.Blazor

    let LoadWeather (http: HttpClient) =
        let t = fun (dispatch: Dispatcher<MyMsg>) state -> 
            task {
                dispatch.Invoke(MyMsg.LoadWeather) |> ignore
                let! forecasts = http.GetJsonAsync<WeatherForecast[]>("/sample-data/weather.json") |> Async.AwaitTask
                dispatch.Invoke(MyMsg.ReceiveWeather forecasts) |> ignore
            } :> Task

        AsyncActionsCreator<MyModel, MyMsg>t
