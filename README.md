# mssql_exporter

MSSQL Exporter for Prometheus


| Appveyor | GitHub |
|---:|:---|
| [![Build Status](https://dev.azure.com/mssqlexporter/mssql_exporter/_apis/build/status/DanielOliver.mssql_exporter?branchName=master)](https://dev.azure.com/mssqlexporter/mssql_exporter/_build/latest?definitionId=1&branchName=master) | [![GitHub release](https://img.shields.io/github/release/DanielOliver/mssql_exporter.svg)](https://github.com/DanielOliver/mssql_exporter/releases/latest) |

## Quickstart docker-compose

```powershell
docker-compose up
```

docker-compose.yml

```yml
version: '3'
services:
  mssql_exporter:
    image: "danieloliver/mssql_exporter:latest"
    ports:
      - "80:80"
    depends_on: 
      - sqlserver.dev
    environment:
      - PROMETHEUS_MSSQL_DataSource=Server=tcp:sqlserver.dev,1433;Initial Catalog=master;Persist Security Info=False;User ID=sa;Password=yourStrong(!)Password;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Connection Timeout=10;
      - PROMETHEUS_MSSQL_ConfigFile=metrics.json
      - PROMETHEUS_MSSQL_ServerPath=metrics
      - PROMETHEUS_MSSQL_ServerPort=80
      - PROMETHEUS_MSSQL_LogLevel=Information
      - PROMETHEUS_MSSQL_AddExporterMetrics=false
      - |
        PROMETHEUS_MSSQL_ConfigText=
        {
            "Queries": [
                {
                    "Name": "mssql_deadlocks",
                    "Query": "SELECT cntr_value FROM sys.dm_os_performance_counters where counter_name = 'Number of Deadlocks/sec' AND instance_name = '_Total'",
                    "Description": "Number of lock requests per second that resulted in a deadlock since last restart",
                    "Columns": [
                        {
                            "Name": "cntr_value",
                            "Label": "mssql_deadlocks",
                            "Usage": "Gauge",
                            "DefaultValue": 0
                        }
                    ]
                }
            ],
            "MillisecondTimeout": 4000
        }
  sqlserver.dev:
    image: "mcr.microsoft.com/mssql/server:2017-latest"
    ports:
      - "1433:1433"
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=yourStrong(!)Password
```

## QuickStart binary

1. Download system of your choice from [latest release](https://github.com/DanielOliver/mssql_exporter/releases/latest).

2. Create a file "metrics.json" and  put this in it:

```json
{
    "Queries": [
        {
            "Name": "mssql_process_status",
            "Query": "SELECT status, COUNT(*) count FROM sys.sysprocesses GROUP BY status",
            "Description": "Counts the number of processes per status",
            "Usage": "GaugesWithLabels",
            "Columns": [
                {
                    "Name": "status",
                    "Label": "status",
                    "Usage": "GaugeLabel",
                    "Order": 0
                },
                {
                    "Name": "count",
                    "Label": "count",
                    "Usage": "Gauge"
                }
            ]
        },
        {
            "Name": "mssql_deadlocks",
            "Query": "SELECT cntr_value FROM sys.dm_os_performance_counters where counter_name = 'Number of Deadlocks/sec' AND instance_name = '_Total'",
            "Description": "Number of lock requests per second that resulted in a deadlock since last restart",
            "Columns": [
                {
                    "Name": "cntr_value",
                    "Label": "mssql_deadlocks",
                    "Usage": "Gauge",
                    "DefaultValue": 0
                }
            ]
        }
    ],
    "MillisecondTimeout": 4000
}
```

3. Run mssql_exporter

```bash
./mssql_exporter serve -ConfigFile "metrics.json" -DataSource "Server=tcp:{ YOUR DATABASE HERE },1433;Initial Catalog={ YOUR INITIAL CATALOG HERE };Persist Security Info=False;User ID={ USER ID HERE };Password={ PASSWORD HERE };MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=8;"
```

or

```powershell
.\mssql_exporter.exe serve -ConfigFile "metrics.json" -DataSource "Server=tcp:{ YOUR DATABASE HERE },1433;Initial Catalog={ YOUR INITIAL CATALOG HERE };Persist Security Info=False;User ID={ USER ID HERE };Password={ PASSWORD HERE };MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=8;"
```

4. Open http://localhost/metrics

Content should look like 
```txt
# HELP mssql_up mssql_up
# TYPE mssql_up gauge
mssql_up 1
# HELP mssql_exceptions Number of queries throwing exceptions.
# TYPE mssql_exceptions gauge
mssql_exceptions 0
# HELP mssql_process_status Counts the number of processes per status
# TYPE mssql_process_status gauge
mssql_process_status{status="runnable"} 1
mssql_process_status{status="suspended"} 1
mssql_process_status{status="background"} 86
mssql_process_status{status="sleeping"} 28
# HELP mssql_timeouts Number of queries timing out.
# TYPE mssql_timeouts gauge
mssql_timeouts 0
# HELP mssql_deadlocks mssql_deadlocks
# TYPE mssql_deadlocks gauge
mssql_deadlocks 0
```

_Note_

* *mssql_up* gauge is "1" if the database is reachable. "0" if connection to the database fails.
* *mssql_exceptions* gauge is "0" if all queries run successfully. Else, this is the number of queries that throw exceptions.
* *mssql_timeouts* is "0" if all queries are running with the configured timeout. Else, this is the number of queries that are not completing within the configured timeout.

5. Add Prometheus scrape target (assuming same machine).

```yml
global:
  scrape_interval:     15s # Set the scrape interval to every 15 seconds. Default is every 1 minute.
  evaluation_interval: 15s # Evaluate rules every 15 seconds. The default is every 1 minute.
  
scrape_configs:
  - job_name: 'netcore-prometheus'
    # metrics_path defaults to '/metrics'
    static_configs:
    - targets: ['localhost']
```

## Command Line Options

```
Commands
   help
   serve
      -DataSource (Connection String)
      -ConfigFile (metrics.json)
      -ServerPath (/metrics)
      -ServerPort (80)
      -AddExporterMetrics (false)
      -LogLevel (Error)
      -ConfigText ()

Or environment variables:
      PROMETHEUS_MSSQL_DataSource
      PROMETHEUS_MSSQL_ConfigFile
      PROMETHEUS_MSSQL_ServerPath
      PROMETHEUS_MSSQL_ServerPort
      PROMETHEUS_MSSQL_AddExporterMetrics
      PROMETHEUS_MSSQL_LogLevel
      PROMETHEUS_MSSQL_ConfigText
```

* DataSource
    * Default: empty
    * SQL Server .NET connection String
* ConfigFile
    * Default: "metrics.json"
    * The path to the configuration file as shown in "metrics.json" above.
* ServerPath
    * Default: "metrics"
    * specifies the path for prometheus to answer requests on
* ServerPort
    * Default: 80
* AddExporterMetrics
    * Default: false
    * Options:
        * true
        * false
* LogLevel
    * Default: Error
    * Options:
        * Information
        * Error
        * Debug
        * Warning
* ConfigText
    * Default: empty
    * Optionally fill in this with the contents of the ConfigFile to ignore and not read from the ConfigFile.

## Debug Run and Docker

1. Run Docker

```powershell
docker run -e 'ACCEPT_EULA=Y' -e "SA_PASSWORD=yourStrong(!)Password" --net=host -p 1433:1433 -d --rm --name sqlserverdev mcr.microsoft.com/mssql/server:2017-latest
```

2. Run exporter from "src/server" directory.

```powershell
dotnet run -- serve -ConfigFile "../../metrics.json" -DataSource "Server=tcp:localhost,1433;Initial Catalog=master;Persist Security Info=False;User ID=sa;Password=yourStrong(!)Password;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Connection Timeout=8;" -LogLevel Debug
```

```bash
dotnet run -- serve -ConfigFile "../../metrics.json" -DataSource 'Server=tcp:localhost,1433;Initial Catalog=master;Persist Security Info=False;User ID=sa;Password=yourStrong(!)Password;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Connection Timeout=8;' -LogLevel Debug
```

OR

3. Docker-compose!

```powershell
docker-compose up
```
