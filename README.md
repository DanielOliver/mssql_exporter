# mssql_exporter

MSSQL Exporter for Prometheus

## QuickStart

```powershell
> git clone https://github.com/DanielOliver/mssql_exporter.git
> cd mssql_exporter
> cd src
> cd server
> dotnet run serve -ConfigFile "..\..\test.json" -DataSource "Server=tcp:{ YOUR DATABASE HERE },1433;Initial Catalog={ YOUR INITIAL CATALOG HERE };Persist Security Info=False;User ID={ USER ID HERE };Password={ PASSWORD HERE };MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

Open http://localhost/metrics

