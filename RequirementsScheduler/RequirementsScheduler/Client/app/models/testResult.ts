export class TestResult {
    key: number;
    value: ResultInfo;
}

export class ResultInfo {
    deltaCmax: number;
    isResolvedOnCheck3InOnline: boolean;
    isStop3OnOnline: boolean;
    offlineResolvedConflictAmount: number;
    onlineExecutionTime: string;
    onlineResolvedConflictAmount: number;
    onlineUnResolvedConflictAmount: number;
    type: string;
}