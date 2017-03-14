"use strict";
var ExperimentStatus;
(function (ExperimentStatus) {
    ExperimentStatus[ExperimentStatus["New"] = 0] = "New";
    ExperimentStatus[ExperimentStatus["InProgress"] = 1] = "InProgress";
    ExperimentStatus[ExperimentStatus["Completed"] = 2] = "Completed";
})(ExperimentStatus = exports.ExperimentStatus || (exports.ExperimentStatus = {}));
var Experiment = (function () {
    function Experiment() {
    }
    Experiment.prototype.deserialize = function (input) {
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
        this.ExperimentStatus = input.ExperimentStatus;
        //console.log('Result of deserialization: ');
        //console.log(this);
        return this;
    };
    return Experiment;
}());
exports.Experiment = Experiment;
//# sourceMappingURL=experiment.js.map