FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MicroNetflix.Api/MicroNetflix.Api.csproj", "MicroNetflix.Api/"]
COPY ["MicroNetflix.Shared/MicroNetflix.Shared.csproj", "MicroNetflix.Shared/"]
RUN dotnet restore "MicroNetflix.Api/MicroNetflix.Api.csproj"
COPY . .
WORKDIR "/src/MicroNetflix.Api"
RUN dotnet build "MicroNetflix.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroNetflix.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroNetflix.Api.dll"]
