"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var core_1 = require("@angular/core");
var index_1 = require("../../models/index");
var index_2 = require("../../services/index");
var ExperimentsComponent = (function () {
    function ExperimentsComponent(experimentService) {
        this.experimentService = experimentService;
        this.ExperimentStatus = index_1.ExperimentStatus;
    }
    Object.defineProperty(ExperimentsComponent.prototype, "status", {
        set: function (value) {
            var status = this.ExperimentStatus[value.toString()];
            switch (status) {
                case index_1.ExperimentStatus.New:
                    this.experimentStatus = index_1.ExperimentStatus.New;
                    break;
                case index_1.ExperimentStatus.InProgress:
                    this.experimentStatus = index_1.ExperimentStatus.InProgress;
                    break;
                case index_1.ExperimentStatus.Completed:
                    this.experimentStatus = index_1.ExperimentStatus.Completed;
                    break;
                default:
                    this.experimentStatus = value;
                    break;
            }
        },
        enumerable: true,
        configurable: true
    });
    ExperimentsComponent.prototype.ngAfterContentInit = function () {
        var _this = this;
        this.experimentService.getByStatus(this.experimentStatus)
            .subscribe(function (experiments) { return _this.experiments = experiments; });
    };
    return ExperimentsComponent;
}());
__decorate([
    core_1.Input('experimentStatus'),
    __metadata("design:type", Number),
    __metadata("design:paramtypes", [Number])
], ExperimentsComponent.prototype, "status", null);
ExperimentsComponent = __decorate([
    core_1.Component({
        selector: 'experiments',
        styles: ["\n  "],
        template: require('./experiments.component.html'),
    }),
    __metadata("design:paramtypes", [index_2.ExperimentService])
], ExperimentsComponent);
exports.ExperimentsComponent = ExperimentsComponent;
//# sourceMappingURL=experiments.component.js.map