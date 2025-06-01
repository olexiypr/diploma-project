FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services.IdentityService.Cognito.csproj", "Services.IdentityService.Cognito/"]
RUN dotnet restore "Services.IdentityService.Cognito/Services.IdentityService.Cognito.csproj"
COPY . .
WORKDIR "/src/Services.IdentityService.Cognito"
RUN dotnet build "Services.IdentityService.Cognito.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Services.IdentityService.Cognito.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Services.IdentityService.Cognito.dll"]
