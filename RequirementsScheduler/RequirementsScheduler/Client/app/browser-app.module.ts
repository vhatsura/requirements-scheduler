import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { APP_BASE_HREF } from '@angular/common';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { ApplicationInsightsModule, AppInsightsService } from '@markpieszak/ng-application-insights';

import { AppModule } from './app.module';
import { AppComponent } from './components/app/app.component';

@NgModule({
    bootstrap: [AppComponent],
    imports: [
        BrowserModule.withServerTransition({
            appId: 'my-app-id' // make sure this matches with your Server NgModule
        }),
        BrowserAnimationsModule,
        // Our Common AppModule
        AppModule,
        ApplicationInsightsModule.forRoot({
            instrumentationKey: '660270e3-0760-44d8-b002-a4725627aeed'
        })
        
    ],
    providers: [
        {
            // We need this for our 
            provide: APP_BASE_HREF,
            useValue: window.location.origin
        },
        AppInsightsService
    ]
})
export class BrowserAppModule {
}
