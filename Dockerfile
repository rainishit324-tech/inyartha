FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Inyartha/Inyartha.csproj Inyartha/
RUN dotnet restore Inyartha/Inyartha.csproj
COPY . .
WORKDIR /src/Inyartha
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:7860
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 7860
CMD ["dotnet", "Inyartha.dll"]
