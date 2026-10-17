# Multi-stage build: compile with the SDK image, run with the much smaller ASP.NET runtime image.
# Build context must be the repo root (needs all four src/ projects + tests/ for restore to resolve).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first so Docker layer caching skips `dotnet restore` when only source changes.
COPY InterviewPrep.slnx .
COPY src/InterviewPrep.Domain/InterviewPrep.Domain.csproj src/InterviewPrep.Domain/
COPY src/InterviewPrep.Application/InterviewPrep.Application.csproj src/InterviewPrep.Application/
COPY src/InterviewPrep.Infrastructure/InterviewPrep.Infrastructure.csproj src/InterviewPrep.Infrastructure/
COPY src/InterviewPrep.Api/InterviewPrep.Api.csproj src/InterviewPrep.Api/
RUN dotnet restore src/InterviewPrep.Api/InterviewPrep.Api.csproj

COPY src/ src/
RUN dotnet publish src/InterviewPrep.Api/InterviewPrep.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Container convention: listen on 8080 (non-privileged), not the 5000/5041 used for local dev.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Connection string targets the `sql` service in docker-compose.yml, not LocalDB (Windows-only,
# not reachable from inside a Linux container) — overridden there via ConnectionStrings__DefaultConnection.
ENTRYPOINT ["dotnet", "InterviewPrep.Api.dll"]
