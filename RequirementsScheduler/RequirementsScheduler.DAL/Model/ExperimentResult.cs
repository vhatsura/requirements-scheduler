﻿using System;
using LinqToDB.Mapping;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.DAL.Model
{
    public class ExperimentResult : IRepositoryModel<int>
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        public int OfflineResolvedConflictAmount { get; set; }
        public int OnlineResolvedConflictAmount { get; set; }
        public int OnlineUnResolvedConflictAmount { get; set; }

        public int Stop1Percentage { get; set; }
        public int Stop2Percentage { get; set; }
        public int Stop3Percentage { get; set; }
        public int Stop4Percentage { get; set; }

        public float DeltaCmaxMax { get; set; }
        public float DeltaCmaxAverage { get; set; }

        public TimeSpan OnlineExecutionTime { get; set; }

        public Guid ExperimentId { get; set; }
        [Association(ThisKey = nameof(ExperimentId), OtherKey = nameof(Model.Experiment.Id))]
        public Experiment Experiment { get; set; }
    }
}
