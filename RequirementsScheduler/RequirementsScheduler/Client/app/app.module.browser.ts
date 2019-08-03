import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { APP_BASE_HREF } from '@angular/common';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { ORIGIN_URL, REQUEST } from '@nguniversal/aspnetcore-engine/tokens';
import { AppModuleShared } from './app.module';
import { AppComponent } from './app.component';
import { BrowserTransferStateModule } from '@angular/platform-browser';
import { BrowserPrebootModule } from 'preboot/browser';

import { ApplicationInsightsModule, AppInsightsService } from '@markpieszak/ng-application-insights';

export function getOriginUrl() {
    return window.location.origin;
}

export function getRequest() {
    // the Request object only lives on the server
    return { cookie: document.cookie };
}

@NgModule({
    bootstrap: [AppComponent],
    imports: [
        BrowserPrebootModule.replayEvents(),
        BrowserAnimationsModule,

        // Our Common AppModule
        AppModuleShared,
        ApplicationInsightsModule.forRoot({
            instrumentationKey: '660270e3-0760-44d8-b002-a4725627aeed'
        })
        
    ],
    providers: [
        {
            // We need this for our Http calls since they'll be using APP_BASE_HREF (since the Server requires Absolute URLs)
            provide: ORIGIN_URL,
            useFactory: (getOriginUrl)
        },
        {
            provide: REQUEST,
            useFactory: (getRequest)
        },
        AppInsightsService
    ]
})
export class AppModule {
}
