## BeavBus Server

![Build Status](https://github.com/OSU-App-Club/BeavBus-Server/actions/workflows/test.yml/badge.svg)

The backend that powers the BeavBus apps' Corvallis Transit System data.
- [BeavBus](https://github.com/OSU-App-Club/beavbus)

## Prerequisites for running

The [.NET SDK](https://dotnet.microsoft.com/download) must be installed. The precise version that you should install can be found in [global.json](global.json). Then you can run the following commands in the repo root directory:
```sh
# Running tests
$ dotnet test CorvallisBus.Test

# Running Web App
$ cd CorvallisBus.Web
$ dotnet run --project CorvallisBus.Web

# Run the data init job locally by sending a POST request
$ curl -d {} localhost:57855/api/job/init
```

## Purpose

To have a more convenient way to get real-time information about the free buses in Corvallis.  Data from CTS is merged with data from Corvallis' GTFS data, with some convenient projections applied, and mapped into some easily-digestable JSON for different use cases.

## Disclaimer

We assume no liability for any missed buses.  Buses may be erratic in their arrival behavior, and we cannot control that.

## API Documentation

Visit https://corvallisb.us/api for documentation.
