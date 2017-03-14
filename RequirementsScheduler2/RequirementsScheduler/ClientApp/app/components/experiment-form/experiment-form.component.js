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
var index_1 = require("../../services/index");
var forms_1 = require("@angular/forms");
var ng2_validation_1 = require("ng2-validation");
var ExperimentFormComponent = (function () {
    function ExperimentFormComponent(experimentService, formBuilder) {
        this.experimentService = experimentService;
        this.formBuilder = formBuilder;
        this.error = "";
        this.success = "";
        this.loading = false;
    }
    ExperimentFormComponent.prototype.onSubmit = function () {
        var _this = this;
        this.loading = true;
        this.experimentService.create(this.f.value)
            .subscribe(function (result) {
            if (result.status === 200) {
                _this.success = "Experiment was submitted successfully";
                _this.error = '';
                _this.f.reset();
            }
            else {
                _this.error = result.response;
            }
            _this.loading = false;
        });
    };
    ExperimentFormComponent.prototype.ngOnInit = function () {
        this.f = this.formBuilder.group({
            testsAmount: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.min(1)])],
            requirementsAmount: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.min(1)])],
            n1: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.range([0, 100])])],
            n2: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.range([0, 100])])],
            n12: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.range([0, 100])])],
            n21: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.range([0, 100])])],
            minBoundaryRange: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.min(0)])],
            maxBoundaryRange: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.min(0)])],
            minPercentageFromA: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.range([5, 50])])],
            maxPercentageFromA: ['', forms_1.Validators.compose([forms_1.Validators.required, ng2_validation_1.CustomValidators.range([5, 50])])],
            borderGenerationType: ['', forms_1.Validators.required],
            pGenerationType: ['', forms_1.Validators.required]
        });
    };
    return ExperimentFormComponent;
}());
ExperimentFormComponent = __decorate([
    core_1.Component({
        selector: "experiment-form",
        styles: ["\n    .ng-valid[required], .ng-valid.required  {\n        border-left: 5px solid #42A948; /* green */\n    }\n    .ng-invalid:not(form)  {\n        border-left: 5px solid #a94442; /* red */\n    }\n    "],
        template: require("./experiment-form.component.html")
    }),
    __metadata("design:paramtypes", [index_1.ExperimentService,
        forms_1.FormBuilder])
], ExperimentFormComponent);
exports.ExperimentFormComponent = ExperimentFormComponent;
//# sourceMappingURL=experiment-form.component.js.map