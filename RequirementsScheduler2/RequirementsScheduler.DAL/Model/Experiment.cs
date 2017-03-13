﻿using System;
using System.Collections.Generic;
using LinqToDB.Mapping;
using RequirementsScheduler.DAL.Repository;

namespace RequirementsScheduler.DAL.Model
{
    public class Experiment : IRepositoryModel<Guid>
    {
        [PrimaryKey, Identity]
        public Guid Id { get; set; }

        public int TestsAmount { get; set; }
        public int RequirementsAmount { get; set; }
        public int N1 { get; set; }
        public int N2 { get; set;}
        public int N12 { get; set; }
        public int N21 { get; set; }
        public int MinBoundaryRange { get; set; }
        public int MaxBoundaryRange { get; set; }  
        public int MinPercentageFromA { get; set; }
        public int MaxPercentageFromA { get; set; }

        public int Status { get; set; }

        public int UserId { get; set; }
        [Association(ThisKey = nameof(UserId), OtherKey = nameof(Model.User.Id))]
        public User User { get; set; }

        public IEnumerable<ExperimentResult> Results { get; set; }
    }
}