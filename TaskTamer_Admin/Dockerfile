FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8082
ENV ASPNETCORE_URLS="http://*:8082"
ENV ASPNETCORE_HTTPS_REDIRECT="false"

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskTamer_Admin/TaskTamer_Admin.csproj", "TaskTamer_Admin/"]
COPY ["TaskTamer_Application/TaskTamer_Application.csproj", "TaskTamer_Application/"]
COPY ["TaskTamer_Logic/TaskTamer_Logic.csproj", "TaskTamer_Logic/"]
COPY ["TaskTamer_Persistence/TaskTamer_Persistence.csproj", "TaskTamer_Persistence/"]
RUN dotnet restore "TaskTamer_Admin/TaskTamer_Admin.csproj"
COPY . .
WORKDIR "/src/TaskTamer_Admin"
RUN dotnet build "./TaskTamer_Admin.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskTamer_Admin.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskTamer_Admin.dll"]