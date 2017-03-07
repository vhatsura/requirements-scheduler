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
var angular2_jwt_1 = require("angular2-jwt");
var index_1 = require("../models/index");
var Observable_1 = require("rxjs/Observable");
var index_2 = require("../models/index");
var ExperimentService = (function () {
    function ExperimentService(authHttp) {
        this.authHttp = authHttp;
    }
    ExperimentService.prototype.create = function (experiment) {
        return this.authHttp.post('/api/experiments', experiment)
            .map(function (response) {
            var httpResponse = new index_1.HttpResponse();
            httpResponse.status = response.status;
            httpResponse.response = response.json();
            return httpResponse;
        });
    };
    ExperimentService.prototype.getByStatus = function (status) {
        var _this = this;
        return Observable_1.Observable.create(function (observer) {
            _this.authHttp.get('/api/experiments/GetByStatus/' + status)
                .map(function (response) { return response.json(); })
                .subscribe(function (result) {
                var experiments = new Array();
                for (var r in result) {
                    if (result.hasOwnProperty(r)) {
                        experiments.push(new index_2.Experiment().deserialize(r));
                    }
                }
                observer.next(experiments);
                observer.complete();
            });
        });
    };
    return ExperimentService;
}());
ExperimentService = __decorate([
    core_1.Injectable(),
    __metadata("design:paramtypes", [angular2_jwt_1.AuthHttp])
], ExperimentService);
exports.ExperimentService = ExperimentService;
//# sourceMappingURL=experiment.service.js.map