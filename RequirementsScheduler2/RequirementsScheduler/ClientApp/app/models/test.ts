import { Serializable } from './serializable';

export class Test implements Serializable<Test> {
    
    deserialize(input): Test {
        this.testNumber = input.testNumber;
        this.isOptimized = input.isOptimized;

        return this;
    }

    isOptimized: boolean;
    testNumber: number;
    j1: Array<any>;
    j2: Array<any>;
    j12: Array<any>;
    j21: Array<any>;
    j12Chain: Array<any>;
    j21Chain: Array<any>;
}