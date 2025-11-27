FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["USR/UserService/UserService.csproj", "USR/UserService/"]
RUN dotnet restore "USR/UserService/UserService.csproj"

COPY USR/UserService/ USR/UserService/
WORKDIR "/src/USR/UserService"
RUN dotnet build "UserService.csproj" -c Release -o /app/build

#---

FROM build AS publish
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish /p:UseAppHost=false

#---

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

USER $APP_UID

ENTRYPOINT ["dotnet", "UserService.dll"]
