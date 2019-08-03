import { NgModule, Inject } from '@angular/core';
import { RouterModule, PreloadAllModules } from '@angular/router';
import { CommonModule, APP_BASE_HREF  } from '@angular/common';
import { HttpModule, Http, RequestOptions, BrowserXhr } from '@angular/http';
import { HttpClientModule, HttpClient, HTTP_INTERCEPTORS  } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule, BrowserTransferStateModule } from '@angular/platform-browser';
import { TransferHttpCacheModule } from '@nguniversal/common';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { UsersComponent } from './components/users/users.component';
import { AlertComponent } from './components/alert/alert.component';
import { ReportsComponent } from './components/reports/reports.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { AuthGuard } from './guards/auth.guard';
import { AlertService, AuthenticationService, UserService, ExperimentService } from './services/index';
import { ExperimentFormComponent } from './components/experiment-form/experiment-form.component';
import { ExperimentsComponent } from './components/experiments/experiments.component';
import { ExperimentDetailComponent } from './components/experiment-detail/experiment-detail.component';
import { TestListComponent } from './components/test-list/test-list.component';
import { TestDetailComponent } from './components/test-detail/test-detail.component';
import { NameComponent, RoleComponent, EmailComponent } from './components/users/users.component';
import { UserDetailComponent } from './components/user-detail/user-detail.component';
import { NotFoundComponent } from './components/not-found/not-found.component';


import { CustomFormsModule } from 'ng2-validation';

import { JwtModule } from '@auth0/angular-jwt';

import { BusyModule } from 'angular2-busy';
import { GenericTableModule } from 'angular-generic-table';

import { LinkService } from './shared/link.service';
import { ORIGIN_URL } from '@nguniversal/aspnetcore-engine/tokens';

import { Ng2GoogleChartsModule } from 'ng2-google-charts';

// import { ChartMockComponent } from './components/chartMock/chartMock.component';

import { NgProgressModule, NgProgressInterceptor } from 'ngx-progressbar';

import { TabsModule  } from 'ngx-bootstrap';

import { JwtHttpInterceptor } from './services/jwt.http.interceptor';

export function jwtTokenGetter() {
    let token = localStorage.getItem('token');
    return token;
}

export const jwtConf = {
      config: {
        tokenGetter: jwtTokenGetter,
        whitelistedDomains: new Array(new RegExp('^null$')),
        throwNoTokenError: true
      }
    };

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
        ExperimentFormComponent,
        ExperimentsComponent,
        ExperimentDetailComponent,
        TestListComponent,
        TestDetailComponent,
        UserDetailComponent,
        NameComponent,
        RoleComponent,
        EmailComponent,
        NotFoundComponent
    ],
    imports: [
        CommonModule,
        BrowserModule.withServerTransition({
            appId: 'my-app-id' // make sure this matches with your Server NgModule
        }),

        HttpModule,
        HttpClientModule,
        JwtModule.forRoot(jwtConf),
        TransferHttpCacheModule,
        BrowserTransferStateModule,

        FormsModule,
        CustomFormsModule,
        ReactiveFormsModule,

        Ng2GoogleChartsModule,

        TabsModule.forRoot(),

        GenericTableModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent, canActivate: [AuthGuard] },
            { path: 'users', component: UsersComponent, canActivate: [AuthGuard] },
            { path: 'reports', component: ReportsComponent },
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent, canActivate: [AuthGuard] },
            { path: '**', redirectTo: '' },
            {
                 path: '**', component: NotFoundComponent,
                 data: {
                     title: '404 - Not found',
                     meta: [{ name: 'description', content: '404 - Error' }],
                     links: [
                         { rel: 'canonical', href: 'http://blogs.example.com/bootstrap/something' },
                         { rel: 'alternate', hreflang: 'es', href: 'http://es.example.com/bootstrap-demo' }
                     ]
                 }
             }
        ], {
            // Router options
            useHash: false,
            preloadingStrategy: PreloadAllModules,
            initialNavigation: 'enabled'
        }),
        NgProgressModule
    ],
    providers: [
        AuthGuard,
        AlertService,
        AuthenticationService,
        UserService,
        ExperimentService,
        LinkService,
        { provide: HTTP_INTERCEPTORS, useClass: JwtHttpInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: NgProgressInterceptor, multi: true }
    ],
    entryComponents: [
        ExperimentDetailComponent,
        UserDetailComponent,
        NameComponent,
        RoleComponent,
        EmailComponent
    ]
})
export class AppModuleShared {
}
