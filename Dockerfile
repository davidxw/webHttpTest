FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["webHttpTest.csproj", ""]
RUN dotnet restore "./webHttpTest.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "webHttpTest.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "webHttpTest.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "webHttpTest.dll"]