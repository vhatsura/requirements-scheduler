import { Serializable } from './serializable';

export enum ExperimentStatus {
    New,
    InProgress,
    Completed
}

export class Experiment implements Serializable<Experiment> {
    id : string;
    testAmount: number;
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
    ExperimentStatus: ExperimentStatus;

    deserialize(input): Experiment {
        //this.id = input.id;
        this.testAmount = input.testAmount;
        this.requirementsAmount = input.requirementsAmount;
        this.n1 = input.n1;
        this.n2 = input.n2;
        this.n12 = input.n12;
        this.n21 = input.n21;
        this.minBoundaryRange = input.minBoundaryRange;
        this.maxBoundaryRange = input.maxBoundaryRange;
        this.minPercentageFromA = input.minPercentageFromA;
        this.maxPercentageFromA = input.maxPercentageFromA;
        this.borderGenerationType = input.borderGenerationType;
        this.pGenerationType = input.pGenerationType;
        this.ExperimentStatus = input.ExperimentStatus;

        return this;
    }
}