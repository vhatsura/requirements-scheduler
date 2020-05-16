# Requirements Scheduler

## How to run locally

```powershell
docker-compose up --build --force-recreate --renew-anon-volumes
```

## Useful SQL scripts

### Generate reports

```tsql
SELECT 
    e.Id,
    TestsAmount, RequirementsAmount,
    CASE 
        WHEN e.PGenerationType = 0 THEN 'Uniform'
        WHEN e.PGenerationType = 1 THEN 'Gamma'
        ELSE 'Unknown'
    END as 'P Generation type',
    N1, N2, N12, N21,
    MinBoundaryRange, MaxBoundaryRange,
    MinPercentageFromA, MaxPercentageFromA,
    convert(decimal(10, 3), Stop1Percentage) as Stop1, convert(decimal(10, 3), Stop2Percentage) as Stop2, convert(decimal(10, 3), Stop3Percentage) as Stop3, convert(decimal(10, 3), Stop4Percentage) as Stop4,
    OfflineExecutionTime, OnlineExecutionTime as 'OnlineExecutionTime in conflict resolution',
    OfflineResolvedConflictAmount, OnlineResolvedConflictAmount, OnlineUnResolvedConflictAmount, DowntimeAmount, DeltaCmaxMax, DeltaCmaxAverage
FROM [model].[dbo].[Experiment] e
JOIN [model].[dbo].[ExperimentResult] er ON e.Id = er.ExperimentId
 ORDER BY N1, N2, N12, N21, MinPercentageFromA, RequirementsAmount
```
### Cleanup database

```tsql
DELETE FROM [model].[dbo].[ExperimentResult] 
DELETE FROM [model].[dbo].[ExperimentsFailures]
DELETE FROM [model].[dbo].[Experiment] 
```
