import { NgModule, Inject } from '@angular/core';
import { CommonModule, APP_BASE_HREF  } from '@angular/common';
import { Http, RequestOptions, HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { UsersComponent } from './components/users/users.component';
import { AlertComponent } from './components/alert/alert.component';
import { ReportsComponent } from './components/reports/reports.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { AuthGuard } from './guards/auth.guard';
import { AlertService, AuthenticationService, UserService, ExperimentService } from './services/index';
import { TabsComponent } from './components/tabs/tabs.component';
import { TabComponent } from './components/tab/tab.component';
import { ExperimentFormComponent } from './components/experiment-form/experiment-form.component';
import { ExperimentsComponent } from './components/experiments/experiments.component';
import { ExperimentDetailComponent } from './components/experiment-detail/experiment-detail.component';
import { TestListComponent } from './components/test-list/test-list.component';
import { TestDetailComponent } from './components/test-detail/test-detail.component';

import { CustomFormsModule } from 'ng2-validation';

import { AuthHttp, AuthConfig } from 'angular2-jwt';

import { BusyModule } from 'angular2-busy';
import { GenericTableModule } from 'angular-generic-table';

import { LinkService } from './shared/link.service';
import { ORIGIN_URL } from './shared/constants/baseurl.constants';
import { TransferHttpModule } from '../modules/transfer-http/transfer-http.module';

// import { ChartMockComponent } from './components/chartMock/chartMock.component';

import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';

export function authHttpServiceFactory(http: Http, options: RequestOptions) {
    return new AuthHttp(new AuthConfig({
        tokenName: 'token',
        tokenGetter: (() => localStorage.getItem('token')),
        globalHeaders: [{ 'Content-Type': 'application/json' }]
    }), http, options);
}

@NgModule({
    declarations: [
        AppComponent,
        AlertComponent,
        NavMenuComponent,
        HomeComponent,
        UsersComponent,
        ReportsComponent,
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
    imports: [
        CommonModule,
        // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FormsModule,
        HttpModule,
        CustomFormsModule,
        ReactiveFormsModule,

        GenericTableModule,
        TransferHttpModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent, canActivate: [AuthGuard] },
            { path: 'users', component: UsersComponent, canActivate: [AuthGuard] },
            { path: 'reports', component: ReportsComponent },
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent, canActivate: [AuthGuard] },
            { path: '**', redirectTo: '' }
        ])
    ],
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
        ExperimentService,
        LinkService
    ],
    entryComponents: [ExperimentDetailComponent]
})
export class AppModule {
}
