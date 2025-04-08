FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services.LlmService/Services.LlmService.csproj", "Services.LlmService/"]
COPY ["EventBus/EventBus.csproj", "EventBus/"]
COPY ["EventBus.RabbitMq/EventBus.RabbitMq.csproj", "EventBus.RabbitMq/"]
RUN dotnet restore "Services.LlmService/Services.LlmService.csproj"
COPY . .
WORKDIR "/src/Services.LlmService"
RUN dotnet build "Services.LlmService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Services.LlmService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Services.LlmService.dll"]
