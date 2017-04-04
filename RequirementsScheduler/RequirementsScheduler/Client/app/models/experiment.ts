import { Serializable } from './serializable';

export enum ExperimentStatus {
    New,
    InProgress,
    Completed
}

export class Experiment implements Serializable<Experiment> {
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

    deserialize(input): Experiment {
        //console.log('Response from server: ');
        //console.log(input);
        this.id = input.id;
        this.testsAmount = input.testsAmount;
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
        this.experimentStatus = input.ExperimentStatus;
        this.created = input.created;

        //console.log('Result of deserialization: ');
        //console.log(this);

        return this;
    }
}