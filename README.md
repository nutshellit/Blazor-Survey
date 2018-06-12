# Blazor-Survey
## A hybrid F#/C# blazor sample app to kick the tyres of blazor.
I wanted to see how I could mix up F# and C# in a sample Blazor app, F# on the server, C# on the client but be able to make calls into shared F# code on the client. 
* The biggest issue that I encountered was the deserialisation of FSharp records on blazor. As I understand Blazor comes with a library called SimpleJson.
  * This can be solved by using f# classes rather than records.
  * I was able to serialise simple f# command records on blazor and send to api.
* Client side there is no problem calling into and consuming F# types from Blazor C# so one can pack as much F# magic into a Blazor app as required.
## Survey functionality
You can setup a Survey definition with sections and 4 types of questions within those sections. Once you make the survey definition live you can add complete one or more survey responses based on survey definition. Clearly in a real world app you would do survey admin in one place and survey responses in another.  Please forgive brutal css design

## Build and Run
From the root directory you can run buildseedrun.bat and it should build, seed the app and run. You can also run from visual studio.

## ??
