# aspnet-core-todo-minimal-web-api
ASP.NET Core Todo Minimal Web API

## Build Project
```shell
dotnet build
```

## Run Project
```shell
dotnet run
```

## All Todos
```shell
curl https://localhost:7225/todoitems
```

## Completed Todos
```shell
curl https://localhost:7225/todoitems/complete
```

## Create Todo
```shell
curl -X POST -H "Content-Type: application/json" -d '{"name":"walk dog","isComplete":true}' https://localhost:7225/todoitems
```

## Update Todo
```shell
curl -X PUT -H "Content-Type: application/json" -d '{"name":"walk dog", "isComplete":false}' https://localhost:7225/todoitems/1
```

## Swagger UI Endpoints
```
http://localhost:5164/swagger
```

```
https://localhost:7235/swagger
```

![](/assets/image1.png)