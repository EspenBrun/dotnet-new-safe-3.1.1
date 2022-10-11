module Program

open EntityFrameworkCore.FSharp
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Design
open Microsoft.Extensions.DependencyInjection
open Saturn
open BloggingModel

open Shared

module Storage =
    let todos = ResizeArray()

    let addTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

    do
        addTodo (Todo.create "Create new SAFE project")
        |> ignore

        addTodo (Todo.create "Write your app") |> ignore
        addTodo (Todo.create "Ship it !!!") |> ignore

let todosApi =
    { getTodos = fun () -> async { return Storage.todos |> List.ofSeq }
      addTodo =
        fun todo ->
            async {
                return
                    match Storage.addTodo todo with
                    | Ok () -> todo
                    | Error e -> failwith e
            } }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let configureServices (services : IServiceCollection) =
    services.AddDbContext<BloggingContext>(fun options ->
        let connectionString = "Server=localhost; Port=3306; Database=fstest2; Uid=newuser; Pwd=newuser"
        let serverVersion = ServerVersion.AutoDetect(connectionString)
        options.UseMySql(connectionString, serverVersion) |> ignore)

let configureApp (app : IApplicationBuilder) =
    use serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()
    use dbContext = serviceScope.ServiceProvider.GetService<BloggingContext>()
    dbContext.Database.Migrate()
    app

type DesignTimeServices() =
    interface IDesignTimeServices with
        member __.ConfigureDesignTimeServices(serviceCollection: IServiceCollection) =
            let fSharpServices = EFCoreFSharpServices() :> IDesignTimeServices
            fSharpServices.ConfigureDesignTimeServices serviceCollection
            ()

let app =
    application {
        app_config configureApp
        service_config configureServices
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

[<EntryPoint>]
let main _ =
    run app
    0