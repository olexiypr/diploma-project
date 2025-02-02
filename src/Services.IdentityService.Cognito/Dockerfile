FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Diploma1.IdentityService/Diploma1.IdentityService.csproj", "Diploma1.IdentityService/"]
RUN dotnet restore "Diploma1.IdentityService/Diploma1.IdentityService.csproj"
COPY . .
WORKDIR "/src/Diploma1.IdentityService"
RUN dotnet build "Diploma1.IdentityService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Diploma1.IdentityService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Diploma1.IdentityService.dll"]
