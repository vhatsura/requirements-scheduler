import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
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

import { CustomFormsModule } from 'ng2-validation'

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
        TabsComponent
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FormsModule,
        CustomFormsModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent, canActivate: [AuthGuard] },
            //{ path: 'experiment', component: CounterComponent },
            { path: 'users', component: UsersComponent, canActivate: [AuthGuard]},
            { path: 'reports', component: FetchDataComponent },
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent, canActivate: [AuthGuard] },
            { path: '**', redirectTo: 'home' }
        ])
    ],
    providers: [
        AuthGuard,
        AlertService,
        AuthenticationService,
        UserService,
        ExperimentService
    ]
})
export class AppModule {
}
