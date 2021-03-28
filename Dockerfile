#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Blog.Core/Blog.Core.csproj", "Blog.Core/"]
COPY ["Blog.Core.Model/Blog.Core.Model.csproj", "Blog.Core.Model/"]
COPY ["Blog.Core.Common/Blog.Core.Common.csproj", "Blog.Core.Common/"]
COPY ["Blog.Core.IRepository/Blog.Core.IRepository.csproj", "Blog.Core.IRepository/"]
COPY ["Blog.Core.IServices/Blog.Core.IServices.csproj", "Blog.Core.IServices/"]
COPY ["Blog.Core.Extensions/Blog.Core.Extensions.csproj", "Blog.Core.Extensions/"]
RUN dotnet restore "Blog.Core/Blog.Core.csproj"
COPY . .
WORKDIR "/src/Blog.Core"
RUN dotnet build "Blog.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blog.Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blog.Core.dll"]