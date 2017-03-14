"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var core_1 = require("@angular/core");
var router_1 = require("@angular/router");
var forms_1 = require("@angular/forms");
var angular2_universal_1 = require("angular2-universal");
var app_component_1 = require("./components/app/app.component");
var navmenu_component_1 = require("./components/navmenu/navmenu.component");
var home_component_1 = require("./components/home/home.component");
var users_component_1 = require("./components/users/users.component");
var alert_component_1 = require("./components/alert/alert.component");
var fetchdata_component_1 = require("./components/fetchdata/fetchdata.component");
var login_component_1 = require("./components/login/login.component");
var register_component_1 = require("./components/register/register.component");
var auth_guard_1 = require("./guards/auth.guard");
var index_1 = require("./services/index");
var tabs_component_1 = require("./components/tabs/tabs.component");
var tab_component_1 = require("./components/tab/tab.component");
var experiment_form_component_1 = require("./components/experiment-form/experiment-form.component");
var experiments_component_1 = require("./components/experiments/experiments.component");
var experiment_detail_component_1 = require("./components/experiment-detail/experiment-detail.component");
var ng2_validation_1 = require("ng2-validation");
var angular2_jwt_1 = require("angular2-jwt");
var angular2_busy_1 = require("angular2-busy");
var angular2_generic_table_1 = require("angular2-generic-table");
var imports = [
    angular2_universal_1.UniversalModule,
    // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
    forms_1.FormsModule,
    ng2_validation_1.CustomFormsModule,
    forms_1.ReactiveFormsModule,
    angular2_busy_1.BusyModule,
    angular2_generic_table_1.GenericTableModule,
    router_1.RouterModule.forRoot([
        { path: '', component: home_component_1.HomeComponent, canActivate: [auth_guard_1.AuthGuard] },
        //{ path: 'experiment', component: CounterComponent },
        { path: 'users', component: users_component_1.UsersComponent, canActivate: [auth_guard_1.AuthGuard] },
        { path: 'reports', component: fetchdata_component_1.FetchDataComponent },
        { path: 'login', component: login_component_1.LoginComponent },
        { path: 'register', component: register_component_1.RegisterComponent, canActivate: [auth_guard_1.AuthGuard] },
        { path: '**', redirectTo: '' }
    ])
];
if (angular2_universal_1.isBrowser) {
}
var AppModule = (function () {
    function AppModule() {
    }
    return AppModule;
}());
AppModule = __decorate([
    core_1.NgModule({
        bootstrap: [app_component_1.AppComponent],
        declarations: [
            app_component_1.AppComponent,
            alert_component_1.AlertComponent,
            navmenu_component_1.NavMenuComponent,
            //CounterComponent,
            home_component_1.HomeComponent,
            users_component_1.UsersComponent,
            fetchdata_component_1.FetchDataComponent,
            login_component_1.LoginComponent,
            register_component_1.RegisterComponent,
            tab_component_1.TabComponent,
            tabs_component_1.TabsComponent,
            experiment_form_component_1.ExperimentFormComponent,
            experiments_component_1.ExperimentsComponent,
            experiment_detail_component_1.ExperimentDetailComponent
        ],
        imports: imports,
        providers: [
            angular2_jwt_1.AUTH_PROVIDERS,
            auth_guard_1.AuthGuard,
            index_1.AlertService,
            index_1.AuthenticationService,
            index_1.UserService,
            index_1.ExperimentService
        ],
        entryComponents: [experiment_detail_component_1.ExperimentDetailComponent]
    })
], AppModule);
exports.AppModule = AppModule;
//# sourceMappingURL=app.module.js.map