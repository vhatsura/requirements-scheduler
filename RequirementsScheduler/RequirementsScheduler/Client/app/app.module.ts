import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Http, RequestOptions, HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { UsersComponent } from './components/users/users.component';
import { AlertComponent } from './components/alert/alert.component';
import { FetchDataComponent } from './components/fetchdata/fetchdata.component';
import { CounterComponent } from './components/counter/counter.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { AuthGuard } from './guards/auth.guard';
import { AlertService, AuthenticationService, UserService, ExperimentService } from "./services/index";
import { TabsComponent } from './components/tabs/tabs.component';
import { TabComponent } from './components/tab/tab.component';
import { ExperimentFormComponent } from './components/experiment-form/experiment-form.component';
import { ExperimentsComponent } from './components/experiments/experiments.component';
import { ExperimentDetailComponent } from './components/experiment-detail/experiment-detail.component';
import { TestListComponent } from './components/test-list/test-list.component';
import { TestDetailComponent } from './components/test-detail/test-detail.component';

import { CustomFormsModule } from 'ng2-validation'

import { AuthHttp, AuthConfig } from 'angular2-jwt';

import { BusyModule } from 'angular2-busy';
import { GenericTableModule } from 'angular-generic-table';

let imports = [
    // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
    FormsModule,
    HttpModule,
    CommonModule,
    CustomFormsModule,
    ReactiveFormsModule,

    GenericTableModule,
    RouterModule.forRoot([
        { path: '', component: HomeComponent, canActivate: [AuthGuard] },
        //{ path: 'experiment', component: CounterComponent },
        { path: 'users', component: UsersComponent, canActivate: [AuthGuard] },
        { path: 'reports', component: FetchDataComponent },
        { path: 'login', component: LoginComponent },
        { path: 'register', component: RegisterComponent, canActivate: [AuthGuard] },
        { path: '**', redirectTo: '' }
    ])
];

export function authHttpServiceFactory(http: Http, options: RequestOptions) {
    return new AuthHttp(new AuthConfig({
        tokenName: 'id_token',
        tokenGetter: (() => localStorage.getItem('id_token')),
        globalHeaders: [{ 'Content-Type': 'application/json' }],
    }), http, options);
}

@NgModule({
    declarations: [
        AppComponent,
        AlertComponent,
        NavMenuComponent,
        //CounterComponent,
        HomeComponent,
        UsersComponent,
        FetchDataComponent,
        LoginComponent,
        RegisterComponent,
        TabComponent,
        TabsComponent,
        ExperimentFormComponent,
        ExperimentsComponent,
        ExperimentDetailComponent,
        TestListComponent,
        TestDetailComponent
    ],
    imports: imports,
    providers: [
        {
            provide: AuthHttp,
            useFactory: authHttpServiceFactory,
            deps: [Http, RequestOptions]
        },
        AuthGuard,
        AlertService,
        AuthenticationService,
        UserService,
        ExperimentService
    ],
    entryComponents: [ExperimentDetailComponent]
})
export class AppModule {
}
