export enum ExperimentStatus {
    New,
    InProgress,
    Completed
}

export class Experiment {
    id : string;
    testsAmount: number;
    requirementsAmount: number;
    n1: number;
    n2: number;
    n12: number;
    n21: number;
    minBoundaryRange: number;
    maxBoundaryRange: number;
    minPercentageFromA: number;
    maxPercentageFromA: number;
    borderGenerationType: string;
    pGenerationType: string;
    experimentStatus: ExperimentStatus;
    created: Date;
}