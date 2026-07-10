# ---- build 階段:用 SDK 映像檔編譯 ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 先複製 csproj 並還原套件(利用 Docker 快取,套件沒變就不用重裝)
COPY *.csproj ./
RUN dotnet restore

# 再複製其餘原始碼並發佈
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---- runtime 階段:用較小的 aspnet 映像檔跑 ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "DigitalProject.dll"]
