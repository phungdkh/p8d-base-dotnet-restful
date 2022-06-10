#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["P8D.Api/P8D.Api.csproj", "P8D.Api/"]
RUN dotnet restore "P8D.Api/P8D.Api.csproj"
COPY . .
WORKDIR "/src/P8D.Api"
RUN dotnet build "P8D.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "P8D.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "P8D.Api.dll"]