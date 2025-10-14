FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskTamer/TaskTamer_API.csproj", "TaskTamer/"]
COPY ["TaskTamer_Application/TaskTamer_Application.csproj", "TaskTamer_Application/"]
COPY ["TaskTamer_Logic/TaskTamer_Logic.csproj", "TaskTamer_Logic/"]
COPY ["TaskTamer_Persistence/TaskTamer_Persistence.csproj", "TaskTamer_Persistence/"]
RUN dotnet restore "TaskTamer/TaskTamer_API.csproj"
COPY . .
WORKDIR "/src/TaskTamer"
RUN dotnet build "./TaskTamer_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskTamer_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskTamer_API.dll"]
