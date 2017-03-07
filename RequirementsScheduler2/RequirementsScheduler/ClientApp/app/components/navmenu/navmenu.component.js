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
var angular2_jwt_1 = require("angular2-jwt");
var NavMenuComponent = (function () {
    function NavMenuComponent(authService) {
        this.authService = authService;
        this.jwtHelper = new angular2_jwt_1.JwtHelper();
    }
    NavMenuComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.subscription = this.authService.user
            .subscribe(function (item) {
            if (_this.authService.loggedIn()) {
                _this.isLogged = true;
                _this.isAdmin = _this.authService.userRole() === "admin";
            }
            else {
                _this.isLogged = false;
            }
        });
        if (this.authService.loggedIn()) {
            this.isLogged = true;
            this.isAdmin = this.authService.userRole() === "admin";
        }
        else {
            this.isLogged = false;
        }
    };
    NavMenuComponent.prototype.ngOnDestroy = function () {
        // prevent memory leak when component is destroyed
        this.subscription.unsubscribe();
    };
    return NavMenuComponent;
}());
NavMenuComponent = __decorate([
    core_1.Component({
        selector: 'nav-menu',
        template: require('./navmenu.component.html'),
        styles: [require('./navmenu.component.css')]
    }),
    __metadata("design:paramtypes", [index_1.AuthenticationService])
], NavMenuComponent);
exports.NavMenuComponent = NavMenuComponent;
//# sourceMappingURL=navmenu.component.js.map