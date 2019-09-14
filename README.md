# Requirements Scheduler

## How to run locally

* Run mssql server:

MS-SQL Server can be run in docker container. For this, you need to execute the following command:

```powershell
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P4ssw0rd' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest
``` 