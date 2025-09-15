FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Measurement/Measurement.csproj", "Measurement/"]
COPY ["Cyclone.Common/Cyclone.Common.csproj", "Cyclone.Common/"]
COPY ["Batch/Batch.csproj", "Batch/"]
RUN dotnet restore "Measurement/Measurement.csproj"
COPY . .
WORKDIR "/src/Measurement"
RUN dotnet build "./Measurement.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Measurement.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Measurement.dll"]
