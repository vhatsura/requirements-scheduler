import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UniversalModule } from 'angular2-universal';

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

import { CustomFormsModule } from 'ng2-validation'

import { AUTH_PROVIDERS } from 'angular2-jwt';

@NgModule({
    bootstrap: [ AppComponent ],
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
        ExperimentFormComponent
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FormsModule,
        CustomFormsModule,
        ReactiveFormsModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent, canActivate: [AuthGuard] },
            //{ path: 'experiment', component: CounterComponent },
            { path: 'users', component: UsersComponent, canActivate: [AuthGuard]},
            { path: 'reports', component: FetchDataComponent },
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent, canActivate: [AuthGuard] },
            { path: '**', redirectTo: '' }
        ])  
    ],
    providers: [
        AUTH_PROVIDERS,
        AuthGuard,
        AlertService,
        AuthenticationService,
        UserService,
        ExperimentService
    ]
})
export class AppModule {
}
