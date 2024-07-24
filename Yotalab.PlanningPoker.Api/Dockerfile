#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Yotalab.PlanningPoker.Api/Yotalab.PlanningPoker.Api.csproj", "Yotalab.PlanningPoker.Api/"]
COPY ["Yotalab.PlanningPoker.Grains.Interfaces/Yotalab.PlanningPoker.Grains.Interfaces.csproj", "Yotalab.PlanningPoker.Grains.Interfaces/"]
COPY ["Yotalab.PlanningPoker.Grains/Yotalab.PlanningPoker.Grains.csproj", "Yotalab.PlanningPoker.Grains/"]
RUN dotnet restore "Yotalab.PlanningPoker.Api/Yotalab.PlanningPoker.Api.csproj"
COPY . .
WORKDIR "/src/Yotalab.PlanningPoker.Api"
RUN dotnet build "Yotalab.PlanningPoker.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Yotalab.PlanningPoker.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
EXPOSE 30000 11111 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yotalab.PlanningPoker.Api.dll"]