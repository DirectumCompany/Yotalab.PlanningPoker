#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Yotalab.PlanningPoker.BlazorServerSide/Yotalab.PlanningPoker.BlazorServerSide.csproj", "Yotalab.PlanningPoker.BlazorServerSide/"]
COPY ["Yotalab.PlanningPoker.Grains.Interfaces/Yotalab.PlanningPoker.Grains.Interfaces.csproj", "Yotalab.PlanningPoker.Grains.Interfaces/"]
COPY ["Yotalab.PlanningPoker.Grains/Yotalab.PlanningPoker.Grains.csproj", "Yotalab.PlanningPoker.Grains/"]
RUN dotnet restore "Yotalab.PlanningPoker.BlazorServerSide/Yotalab.PlanningPoker.BlazorServerSide.csproj"
COPY . .
WORKDIR "/src/Yotalab.PlanningPoker.BlazorServerSide"
RUN dotnet build "Yotalab.PlanningPoker.BlazorServerSide.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Yotalab.PlanningPoker.BlazorServerSide.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yotalab.PlanningPoker.BlazorServerSide.dll"]