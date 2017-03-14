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
var angular2_universal_1 = require("angular2-universal");
var angular2_generic_table_1 = require("angular2-generic-table");
var experiment_detail_component_1 = require("../experiment-detail/experiment-detail.component");
var ExperimentsComponent = (function () {
    function ExperimentsComponent(experimentService) {
        this.experimentService = experimentService;
        this.tableInfo = {};
        this.data = new core_1.EventEmitter();
        this.expandedRow = experiment_detail_component_1.ExperimentDetailComponent;
        this.showColumnControls = false;
        this.ExperimentStatus = index_1.ExperimentStatus;
        this.configObject = {
            settings: [
                {
                    objectKey: 'id',
                    visible: true,
                    sort: 'desc',
                    columnOrder: 0
                },
                {
                    objectKey: 'testsAmount',
                    visible: true,
                    sort: 'enable',
                    columnOrder: 1
                }
            ],
            fields: [
                {
                    name: 'Id',
                    objectKey: 'id',
                    classNames: 'clickable sort-string',
                    expand: true
                },
                {
                    name: 'Amount of tests',
                    objectKey: 'testsAmount',
                    classNames: 'sort-numeric',
                    value: function (row) { return row.testsAmount; }
                }
            ],
            data: []
        };
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
    ExperimentsComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.busy = this.experimentService.getByStatus(this.experimentStatus)
            .subscribe(function (experiments) { return _this.configObject.data = experiments; });
    };
    ExperimentsComponent.prototype.updateExperiments = function () {
        var _this = this;
        this.busy = this.experimentService.getByStatus(this.experimentStatus)
            .subscribe(function (experiments) {
            console.log(experiments);
            _this.configObject.data = [];
            _this.configObject.data.push(experiments);
            return _this.configObject.data = experiments;
        });
    };
    ExperimentsComponent.prototype.isBrowser = function () {
        return angular2_universal_1.isBrowser;
    };
    return ExperimentsComponent;
}());
__decorate([
    core_1.Output(),
    __metadata("design:type", Object)
], ExperimentsComponent.prototype, "data", void 0);
__decorate([
    core_1.ViewChild(angular2_generic_table_1.GenericTableComponent),
    __metadata("design:type", angular2_generic_table_1.GenericTableComponent)
], ExperimentsComponent.prototype, "myTable", void 0);
__decorate([
    core_1.Input('experimentStatus'),
    __metadata("design:type", Number),
    __metadata("design:paramtypes", [Number])
], ExperimentsComponent.prototype, "status", null);
ExperimentsComponent = __decorate([
    core_1.Component({
        selector: "experiments",
        styles: ["\n  "],
        template: require("./experiments.component.html")
    }),
    __metadata("design:paramtypes", [index_2.ExperimentService])
], ExperimentsComponent);
exports.ExperimentsComponent = ExperimentsComponent;
//# sourceMappingURL=experiments.component.js.map