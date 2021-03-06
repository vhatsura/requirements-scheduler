﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.BLL.Model
{
    public sealed class Experiment
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "The test amount is required")]
        [Range(1, 1000000, ErrorMessage = "The tests amount must be in [1, 1000000] range")]
        public int TestsAmount { get; set; }

        [Required(ErrorMessage = "The requirement amount is required")]
        [Range(1, 1000000, ErrorMessage = "The requirements amount must be in [1, 1000000] range")]
        public int RequirementsAmount { get; set; }

        [Required(ErrorMessage = "Percent of detail 1 is required")]
        [Range(0, 100, ErrorMessage = "Percent of detail 1 must be in [0,100] range")]
        public int N1 { get; set; }

        [Required(ErrorMessage = "Percent of detail 2 is required")]
        [Range(0, 100, ErrorMessage = "Percent of detail 2 must be in [0,100] range")]
        public int N2 { get; set; }

        [Required(ErrorMessage = "Percent of detail 12 is required")]
        [Range(0, 100, ErrorMessage = "Percent of detail 12 must be in [0,100] range")]
        public int N12 { get; set; }

        [Required(ErrorMessage = "Percent of detail 21 is required")]
        [Range(0, 100, ErrorMessage = "Percent of detail 21 must be in [0,100] range")]
        public int N21 { get; set; }

        [Required(ErrorMessage = "Min boundary is required")]
        public int MinBoundaryRange { get; set; }

        [Required(ErrorMessage = "Max boundary is required")]
        public int MaxBoundaryRange { get; set; }

        [Required]
        [Range(5, 300)]
        public int MinPercentageFromA { get; set; }

        [Required]
        [Range(5, 300)]
        public int MaxPercentageFromA { get; set; }

        [Required(ErrorMessage = "Border generation type is required")]
        public Distribution BorderGenerationType { get; set; }

        [Required(ErrorMessage = "P generation type is required")]
        public Distribution PGenerationType { get; set; }

        public List<ExperimentInfo> Results { get; } = new List<ExperimentInfo>();

        public ReportInfo Report { get; set; }

        public ExperimentStatus Status { get; set; }

        public DateTime? Created { get; set; }
    }

    public enum ExperimentStatus
    {
        New,
        InProgress,
        Completed
    }
}
