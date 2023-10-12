#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 7168
EXPOSE 5183

RUN apt-get update && apt-get install -y curl

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LingoAITutor.Host/LingoAITutor.Host.csproj", "LingoAITutor.Host/"]
RUN dotnet restore "LingoAITutor.Host/LingoAITutor.Host.csproj"
COPY . .
WORKDIR "/src/LingoAITutor.Host"
RUN dotnet build "LingoAITutor.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LingoAITutor.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LingoAITutor.Host.dll"]