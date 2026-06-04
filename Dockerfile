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
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*
RUN wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/logo.png && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/logo1.png && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/logo3.png && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/hero.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/cta-bg.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/proj-1.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/proj-2.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/proj-3.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/proj-4.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/proj-5.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/proj-6.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/service-bedroom.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/service-kitchen.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/service-living.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/service-wardrobe.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/why-main.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/why-overlay.jpg && \
    wget -q -P wwwroot/images/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/wwwroot/images/signature.png && \
    mkdir -p Data/Quotes && \
    wget -q -P Data/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/Data/quotes.xlsx && \
    wget -q -P Data/Quotes/ https://raw.githubusercontent.com/rainishit324-tech/inyartha/master/Inyartha/Data/Quotes/items.xlsx
ENV ASPNETCORE_URLS=http://0.0.0.0:7860
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 7860
CMD ["dotnet", "Inyartha.dll"]
