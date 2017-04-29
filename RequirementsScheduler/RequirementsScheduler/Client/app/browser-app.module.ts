import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { ApplicationInsightsModule, AppInsightsService } from '@markpieszak/ng-application-insights';

import { ORIGIN_URL } from './shared/constants/baseurl.constants';
import { AppModule } from './app.module';
import { AppComponent } from './components/app/app.component';

import { REQUEST } from './shared/constants/request';
import { BrowserTransferStateModule } from '../modules/transfer-state/browser-transfer-state.module';

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
        BrowserModule.withServerTransition({
            appId: 'my-app-id' // make sure this matches with your Server NgModule
        }),
        BrowserAnimationsModule,

        BrowserTransferStateModule,
        // Our Common AppModule
        AppModule,
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
export class BrowserAppModule {
}
