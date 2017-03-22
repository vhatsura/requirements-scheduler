import { Serializable } from './serializable';

export class Test implements Serializable<Test> {
    
    deserialize(input): Test {
        this.testNumber = input.testNumber;
        this.isOptimized = input.isOptimized;

        this.j1 = input.j1;
        this.j2 = input.j2;

        this.j12 = input.j12Chain;
        this.j21 = input.j21Chain;

        return this;
    }

    isOptimized: boolean;
    testNumber: number;
    j1: Array<any>;
    j2: Array<any>;
    j12: Array<any>;
    j21: Array<any>;
}