# aspnet-core-todo-minimal-web-api

ASP.NET Core Todo Minimal Web API

![](/assets/image1.png)

## Build Project

```shell
dotnet build
```

## Run Project

```shell
dotnet run
```

> Update port number in curl requests after running project. You can find it in Debug Console

## Fetches all todos

```shell
curl https://localhost:7235/todos
```

## Fetches completed todos

```shell
curl https://localhost:7235/todos/completed
```

## Fetches incomplete todos

```shell
curl https://localhost:7235/todos/incomplete
```

## Get todo by id

```shell
curl https://localhost:7235/todos/33333333-3333-3333-3333-333333333333
```

## Create Todo

```shell
curl -X POST -H "Content-Type: application/json" -d '{"name":"walk dog","isComplete":true}' https://localhost:7235/todos
```

## Update Todo

```shell
curl -X PUT -H "Content-Type: application/json" -d '{"name":"walk dog", "isComplete":false}' https://localhost:7235/todos/33333333-3333-3333-3333-333333333333
```

## Swagger UI Endpoints

```
http://localhost:5164/swagger
```

```
https://localhost:7235/swagger
```

![](/assets/image1.png)
