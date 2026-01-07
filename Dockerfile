# ====================================================   FAZA 1   ================+========================================
# cilj fze prebacit nas kod u .dll (razumljivo pc-u)

#odredimo sdk koji ce se koristit kasnije za buildanje
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#napravi mapu app i udje u nju
WORKDIR /app

#kopiramo csprojeve projekata jer oni sadrze potrebne pakete
COPY ["Backend/UrlShortener.API/UrlShortener.API.csproj", "Backend/UrlShortener.API/"]
COPY ["Backend/UrlShortener.Application/UrlShortener.Application.csproj", "Backend/UrlShortener.Application/"]
COPY ["Backend/UrlShortener.Domain/UrlShortener.Domain.csproj", "Backend/UrlShortener.Domain/"]
COPY ["Backend/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj", "Backend/UrlShortener.Infrastructure/"]
COPY ["Tests/UrlShortener.Application.Tests/UrlShortener.Application.Tests.csproj", "Tests/UrlShortener.Application.Tests/"]
COPY ["Tests/UrlShortener.IntegrationTests/UrlShortener.IntegrationTests.csproj", "Tests/UrlShortener.IntegrationTests/"]

#instaliramo potrebne pakete
RUN dotnet restore "Backend/UrlShortener.API/UrlShortener.API.csproj"


# Kopiramo sav ostali kôd da bi nam kasnije bilo lakse updateat kod
# da se ne bi moramo sve ponvoo skidat
COPY . .

#ulazi u nas api projekt di se nalazi program.cs
WORKDIR "Backend/UrlShortener.API"
RUN dotnet publish "UrlShortener.API.csproj" -c Release -o /app/publish

# da testiramo
# docker build --target build -t test-build .
# docker run -it test-build sh



# ====================================================   FAZA 2   ================+========================================


# 1. Uzimamo Runtime image (puno manji i brži od SDK-a)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# 2. MAGIJA: Kopiramo gotov proizvod iz prve faze (koju smo nazvali 'build')
COPY --from=build /app/publish .

# 3. Postavljamo port na kojem će tvoj API slušati unutar kontejnera
ENV ASPNETCORE_URLS=http://+:8080

# 4. Finalna naredba koja pali tvoj program
ENTRYPOINT ["dotnet", "UrlShortener.API.dll"]

# pokretanje
# docker build -t url-shortener-api .

#znaci ovde umjesto sdka koristimo runtime, kreiramo app... i kopiramo samo ono sto smo u prvoj fazi napravili 
#i nazvali build... i spremili poutput u app.publish, postavljamo port 8080 koji ce api slusati unutar containera... i
#na kraju palimo nas program