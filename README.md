# TravelAgent

TravelAgent is a Swiss travel assistant CLI tool built with .NET, Semantic Kernel, and Ollama.

## Features
- **Interactive Chat (`chat`)**: Interactive AI chat assistant for Swiss public transport and weather information.
- **Ask Single Question (`ask`)**: Ask a single travel-related question from the command line.
- **Direct Weather Forecast (`weather`)**: Query Open-Meteo weather forecasts directly for Swiss locations.
- **Direct Transport Connections (`connections`)**: Query real-time Swiss public transport connections directly from transport.opendata.ch.

## Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- **Ollama** running locally with a compatible model (e.g., `qwen2.5:3b` or `qwen3:4b`)

### Ollama Setup on macOS (Native - Recommended for Apple Silicon GPU speed)
```sh
# 1. Install Ollama via Homebrew or download from https://ollama.com/download
brew install ollama

# 2. Start Ollama service
ollama serve

# 3. Pull a model (in another terminal)
ollama pull qwen2.5:3b
```

### Alternative: Ollama Setup via Docker
```sh
cd ollama
docker-compose up -d
docker exec -it ollama ollama pull qwen2.5:3b
```

## Usage

### Interactive Chat Mode (Ollama included)
```sh
dotnet run --project src/TravelAgent -- chat
```

### Ask a Single Question (Ollama included)
```sh
dotnet run --project src/TravelAgent -- ask "Today at 08:00 from Zürich to St. Gallen by train. And how will the weather be?"
dotnet run --project src/TravelAgent -- ask "Today at 08:00 from Zürich to St. Gallen by train. I'm not interested in the weather."
```

### Direct Weather Forecast (Ollama not included)
```sh
dotnet run --project src/TravelAgent -- weather 'St. Gallen' --days 4
```

### Direct Public Transport Connections (Ollama not included)
```sh
dotnet run --project src/TravelAgent -- connections "Zürich HB" "St. Gallen" --time "08:00"
```

### Help
```sh
dotnet run --project src/TravelAgent -- --help
```

## Global Tool Installation

To build and install `TravelAgent` as a global .NET tool:

```sh
chmod +x install.sh
./install.sh 1.0.0
```

After installation, you can run `travel-agent` from anywhere:

```sh
travel-agent chat
```
