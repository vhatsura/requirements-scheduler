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
var http_1 = require("@angular/http");
var Observable_1 = require("rxjs/Observable");
var BehaviorSubject_1 = require("rxjs/BehaviorSubject");
var angular2_universal_1 = require("angular2-universal");
require("rxjs/add/operator/map");
require("rxjs/add/operator/catch");
require("rxjs/add/observable/of");
var alert_service_1 = require("./alert.service");
var angular2_jwt_1 = require("angular2-jwt");
var AuthenticationService = (function () {
    function AuthenticationService(http, alertService) {
        this.http = http;
        this.alertService = alertService;
        this.jwtHelper = new angular2_jwt_1.JwtHelper();
        // Observable navItem source
        this._user = new BehaviorSubject_1.BehaviorSubject(null);
        this.user = this._user.asObservable();
    }
    AuthenticationService.prototype.loggedIn = function () {
        if (angular2_universal_1.isBrowser) {
            return angular2_jwt_1.tokenNotExpired();
        }
        else {
            return false;
        }
    };
    AuthenticationService.prototype.userRole = function () {
        if (angular2_universal_1.isBrowser) {
            var token = localStorage.getItem('id_token');
            var decodedToken = this.jwtHelper.decodeToken(token);
            var role = decodedToken.role;
            if (role)
                return role.toLowerCase();
            else
                return "";
        }
        return "";
    };
    AuthenticationService.prototype.login = function (username, password) {
        var _this = this;
        var headers = new http_1.Headers({ 'Content-Type': 'application/x-www-form-urlencoded' });
        var options = new http_1.RequestOptions({ headers: headers });
        var body = "username=" + username + "&password=" + password;
        return this.http.post('/api/token', body, options)
            .map(function (response) {
            // login successful if there's a jwt token in the response
            var user = response.json();
            if (user && user.access_token) {
                if (angular2_universal_1.isBrowser) {
                    // store user details and jwt token in local storage to keep user logged in between page refreshes
                    localStorage.setItem('id_token', user.access_token);
                    _this._user.next(user);
                    return true;
                }
            }
            return false;
        })
            .catch(function (error) {
            // In a real world app, we might use a remote logging infrastructure
            var errMsg;
            if (error instanceof http_1.Response) {
                var body_1 = error.json() || '';
                var err = body_1.error || JSON.stringify(body_1);
                errMsg = error.status + " - " + (error.statusText || '') + " " + err;
            }
            else {
                errMsg = error.message ? error.message : error.toString();
            }
            _this.alertService.error(errMsg, true);
            return Observable_1.Observable.of(false);
        });
    };
    AuthenticationService.prototype.logout = function () {
        if (angular2_universal_1.isBrowser) {
            // remove user from local storage to log user out
            localStorage.removeItem('id_token');
            this._user.next(null);
        }
    };
    return AuthenticationService;
}());
AuthenticationService = __decorate([
    core_1.Injectable(),
    __metadata("design:paramtypes", [http_1.Http,
        alert_service_1.AlertService])
], AuthenticationService);
exports.AuthenticationService = AuthenticationService;
//# sourceMappingURL=authentication.service.js.map