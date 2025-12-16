FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MicroNetflix.Worker/MicroNetflix.Worker.csproj", "MicroNetflix.Worker/"]
COPY ["MicroNetflix.Shared/MicroNetflix.Shared.csproj", "MicroNetflix.Shared/"]
RUN dotnet restore "MicroNetflix.Worker/MicroNetflix.Worker.csproj"
COPY . .
WORKDIR "/src/MicroNetflix.Worker"
RUN dotnet build "MicroNetflix.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroNetflix.Worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroNetflix.Worker.dll"]
